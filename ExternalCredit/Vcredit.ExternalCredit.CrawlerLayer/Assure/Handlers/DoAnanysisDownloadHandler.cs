using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CrawlerLayer.Assure.AnalisisHtml;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer.Common;
using Vcredit.ExtTrade.ModelLayer.Nolmal;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure.Handlers
{
    public class DoAnanysisDownloadHandler : BaseCreditQueryHandler
    {
        #region Properties

        private CRD_CD_CreditUserInfoEntity _currentUser = null;
        private List<CRD_CD_CreditUserInfoEntity> _userInfoList = null;

        private CRD_CD_CreditUserInfoBusiness _creditUserInfoBiz = new CRD_CD_CreditUserInfoBusiness();
        private static readonly Random random = new Random();
        private string _currentCookie = string.Empty;

        /// <summary>
        /// http参数
        /// </summary>
        private HttpHelper _httpHelper = new HttpHelper();
        private HttpItem _httpItem;
        private HttpResult _httpResult;

        #endregion

        protected override HandleResponse Handle(HandleRequest request)
        {
            this.InitData(request);
            this.Ananysis();

            return new HandleResponse { Description = "操作成功！", DoNextHandler = true };
        }

        private void Ananysis()
        {
            for (int i = 0; i < _userInfoList.Count; i++)
            {
                _currentUser = _userInfoList[i];

                this.DoGetPersonQueryResult();
                var report = this.GetReportEntity();
                if (report == null)
                    continue;

                if (report.istate == (int)ReportStatus.查询成功)
                    this.DoDownloadAndAnalysisReport(report);
                else if (report.istate == (int)ReportStatus.查询失败)
                    this.DoDownloadFail(report);
                else
                {
                    Console.WriteLine(string.Format("证件号为：{0} 的征信报告尚未返回！", _currentUser.Cert_No));
                    Log4netAdapter.WriteInfo(string.Format("证件号为：{0} 的征信报告尚未返回！", _currentUser.Cert_No));
                }
               
                _creditUserInfoBiz.UpdateEntity(_currentUser);
                // if(_currentUser.State !=(int)ReportStatus.查询成功)
                //{
                //    CreditInfoPush.current.PushCredit(new CRD_HD_REPORTEntity { Cert_No = _currentUser.Cert_No }, Vcredit.ExternalCredit.CommonLayer.SysEnums.SourceType.Assure);
                //}
            }
        }

        private void DoDownloadFail(DataEntitty report)
        {
            // 如果失败原因是“连接失败”，标记状态为查询失败
            if (report.serror.Equals("连接失败") ||
                report.serror.Equals("连接失败，请重新提交查询请求"))
                _currentUser.State = (int)RequestState.ConnectionFailed;
			else if (report.serror.Equals("个人征信系统中没有此人的征信记录!"))
				_currentUser.State = (int)RequestState.HaveNoData;
            else
                _currentUser.State = (int)RequestState.QueryFail;

            _currentUser.Error_Reason = string.Format("征信报告查询失败（担保征信中标记状态为查询失败，原因：{0}）", report.serror);
            _currentUser.UpdateTime = DateTime.Now;

            Console.WriteLine(string.Format("证件号为：{0} 征信报告查询失败（担保征信中标记状态为查询失败），原因：{1}）",
                                            _currentUser.Cert_No,
                                            report.serror));
        }

        /// <summary>
        /// 下载解析
        /// </summary>
        /// <param name="report"></param>
        private void DoDownloadAndAnalysisReport(DataEntitty report)
        {
            var url = string.Format("https://msi.pbccrc.org.cn/sfcp/crrecord/openfile?iid={0}", report.iid);
            _httpItem = new HttpItem
            {
                URL = url,
                Cookie = _currentCookie,
                Method = "GET",
                Referer = "https://msi.pbccrc.org.cn/sfcp/creditreport/personqueryresult?fid=51",
                UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36",
                ResultType = ResultType.Byte,
            };
            // 请求延时
            var second = random.Next(1, 10);
            Thread.Sleep(100 * second);
            _httpResult = _httpHelper.GetHtml(_httpItem);

            this.SaveReportToHtml();
            this.SaveReportToDb(report);
        }

        /// <summary>
        /// 保存入库
        /// </summary>
        /// <param name="report"></param>
        /// <param name="byteArray"></param>
        private void SaveReportToDb(DataEntitty report)
        {
            var byteArray = _httpResult.ResultByte;
            try
            {
                // todo 解析征信报告
                string str = Encoding.Default.GetString(byteArray);
                new Analisis().SaveData(_currentUser.Cert_No, str);

                // 状态更改
                _currentUser.State = (int)RequestState.SuccessCome;
                _currentUser.Error_Reason = "成功获取征信报告";

                //todo 获取报告编号
                _currentUser.Report_sn = CommonFun.GetMidStr(str, "报告编号:", "</font>");

                // 获取报告取得结果时间
                _currentUser.UpdateTime = this.GetFeedbacktime(report);

                Console.WriteLine(string.Format("证件号为：{0} 成功获取征信报告！", _currentUser.Cert_No));
            }
            catch (Exception ex)
            {
                // 状态更改
                _currentUser.State = (int)RequestState.AnalysisFail;
                _currentUser.Error_Reason = "征信报告解析失败";
                _currentUser.UpdateTime = DateTime.Now;

                Log4netAdapter.WriteError(string.Format("证件号为：{0} 的征信报告解析失败！", _currentUser.Cert_No), ex);
                Console.WriteLine(string.Format("证件号为：{0} 征信报告解析失败！", _currentUser.Cert_No));
            }
        }

        /// <summary>
        /// 获得取得结果时间
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        private DateTime? GetFeedbacktime(DataEntitty report)
        {
            try
            {
                var ms = Convert.ToDouble(report.dfeedbacktime);
                var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(ms).AddHours((DateTime.Now - DateTime.UtcNow).TotalHours);
                return dt;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 保存到指定文件夹下
        /// </summary>
        /// <param name="byteArray"></param>
        private void SaveReportToHtml()
        {
            var byteArray = _httpResult.ResultByte;
            var floderPath = Path.Combine(ConfigData.AssureDownloadReportPath, _currentUser.LocalDirectoryName ?? DateTime.Now.ToString("yyyyMMdd"));
            if (!Directory.Exists(floderPath))
                Directory.CreateDirectory(floderPath);

            var filePath = Path.Combine(floderPath, _currentUser.Cert_No + ".html");
            var stream = File.Create(filePath);
            stream.Write(byteArray, 0, byteArray.Length);
            stream.Flush();
            stream.Close();

            Console.WriteLine(string.Format("证件号为：{0} 的征信报告下载完成！", _currentUser.Cert_No));
            Log4netAdapter.WriteInfo(string.Format("证件号为：{0} 的征信报告下载完成！", _currentUser.Cert_No));
        }

        private DataEntitty GetReportEntity()
        {
            var html = _httpResult.Html;
            ReportResult reportObj = null;
            try
            {
                reportObj = JsonConvert.DeserializeObject<ReportResult>(html);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("证件号为：{0} 查询结果序列化失败，返回html：{1}", _currentUser.Cert_No, html));
                Log4netAdapter.WriteError(string.Format("证件号为：{0} 查询结果序列化失败，返回html：{1}", _currentUser.Cert_No, html), ex);
                return null;
            }
            if (reportObj == null || !reportObj.success || reportObj.root == null || !reportObj.root.Any())
            {
                Console.WriteLine(string.Format("没有查询到证件号为： {0} 的征信报告！", _currentUser.Cert_No));
                Log4netAdapter.WriteInfo(string.Format("没有查询到证件号为： {0} 的征信报告！", _currentUser.Cert_No));
                return null;
            }
            var report = reportObj.root.FirstOrDefault();
            return report;
        }

        private void DoGetPersonQueryResult()
        {
            // 分析页面得知报告有效时间是10天，这里查询10内的查询的报告
            var timeMin = DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd");
            var timeMax = DateTime.Now.ToString("yyyy-MM-dd");

            var pageNum = 1;
            var url = string.Format(@"https://msi.pbccrc.org.cn/sfcp/crrecord/personqueryresult/list?_dc={0}&itype=2&istate=&iblock=1&dapplytimemin={1}&dapplytimemax={2}&page={3}&start=0&limit=20&sname={4}&scardtype=&scardno={5}"
                , DateTime.Now.Ticks
                , timeMin
                , timeMax
                , pageNum
                , _currentUser.Name
                , _currentUser.Cert_No);

            _httpItem = new HttpItem
            {
                URL = url,
                Method = "GET",
                Referer = "https://msi.pbccrc.org.cn/sfcp/creditreport/personqueryresult?fid=51",
                UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36",
                Cookie = _currentCookie,
            };
            // 请求延时
            var second = random.Next(1, 10);
            Thread.Sleep(100 * second);
            _httpResult = _httpHelper.GetHtml(_httpItem);
        }

        private void InitData(HandleRequest request)
        {
            _currentCookie = request.CurrentCookies;
            _userInfoList = request.UserInfoList;
        }

        protected override void SetNextRequest(HandleRequest request, HandleResponse res)
        {
            return;
        }
    }
}
