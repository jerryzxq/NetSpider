using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer.Common;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure.Handlers
{
    public class DoCommitHandler : BaseCreditQueryHandler
    {
        #region Properties

        private CRD_CD_CreditUserInfoBusiness _creditUserInfoBiz = new CRD_CD_CreditUserInfoBusiness();

        private CRD_CD_CreditUserInfoEntity _currentUser = null;
        private List<CRD_CD_CreditUserInfoEntity> _userInfoList = null;
        private static readonly Random random = new Random();
        private string _currentCookie = string.Empty;

        /// <summary>
        /// 配置是否真正提交到担保系统
        /// </summary>
        private bool _isRealCommit;

        /// <summary>
        /// 登陆名
        /// </summary>
        private string _loginName = string.Empty;


        /// <summary>
        /// http参数
        /// </summary>
        private HttpHelper _httpHelper = new HttpHelper();
        private HttpItem _httpItem;
        private HttpResult _httpResult;

        #endregion

        public DoCommitHandler()
        {
            this.GetIsRealCommit();
            this.GetLoginName();
        }

        /// <summary>
        /// 获取登录名
        /// </summary>
        /// <returns></returns>
        private string GetLoginName()
        {
            _loginName = ConfigurationManager.AppSettings["loginName"];
            if (_loginName.IsEmpty())
                throw new ArgumentException("loginName");
            return _loginName;
        }

        /// <summary>
        /// 获取系统配置是否真正提交
        /// </summary>
        /// <returns></returns>
        private bool GetIsRealCommit()
        {
            _isRealCommit = ConfigurationManager.AppSettings["isRealCommit"].ToBool(false);

            var msg = string.Format("提交担保系统配置为：{0}", _isRealCommit);
            Console.WriteLine(msg);
            Log4netAdapter.WriteInfo(msg);

            return _isRealCommit;
        }


        protected override HandleResponse Handle(HandleRequest request)
        {
            this.InitData(request);
            this.Commit();
            return new HandleResponse { Description = "操作成功！", DoNextHandler = true };
        }

        private void Commit()
        {
            for (int i = 0; i < _userInfoList.Count; i++)
            {
                _currentUser = _userInfoList[i];

                var isMultiple = this.GetIsMultiple();
                if (isMultiple)
                    continue;

                //var isAdd = this.GetIsAdd();
                //if (isAdd)
                //    continue;

                this.DoWait(i);
                this.DoRequest();
                this.DoJudgment();
            }
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

        /// <summary>
        /// 提交之前判断当前用户（根据身份证号，姓名，等待查询结果）是否已提交，如果已提交，跳过不提交
        /// </summary>
        /// <returns></returns>
        private bool GetIsAdd()
        {
            var url = string.Format(@"https://msi.pbccrc.org.cn/sfcp/crrecord/personqueryresult/list?_dc={0}&itype=2&sname={1}&scardtype=&scardno={2}&istate=1&iblock=1&page=1&start=0&limit=20"
                , DateTime.Now.Ticks
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

                // TODO 标记当前信息为失败

                return true;
            }
            if (reportObj == null || !reportObj.success || reportObj.root == null || !reportObj.root.Any())
            {
                Console.WriteLine(string.Format("没有查询到证件号为： {0} 的征信报告！", _currentUser.Cert_No));
                Log4netAdapter.WriteInfo(string.Format("没有查询到证件号为： {0} 的征信报告！", _currentUser.Cert_No));
                return false;
            }
            var report = reportObj.root.FirstOrDefault();
            return report != null;
        }

        /// <summary>
        /// 判断是否是多条数据，当前待提交数据有多条，如果有多条，跳过不提交
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private bool GetIsMultiple()
        {
            var count = _creditUserInfoBiz.GetNeedCommitUserCountByLimitDay(_currentUser, ConfigData.upLoadLimitedDays);

            var isMultiple = count > 1;
            if (isMultiple)
            {
                Console.WriteLine(string.Format("证件号： {0} 有多条，不予提交", _currentUser.Cert_No));
                Log4netAdapter.WriteInfo(string.Format("证件号： {0} 有多条，不予提交", _currentUser.Cert_No));
            }
            return isMultiple;
        }

        /// <summary>
        /// 请求延时
        /// </summary>
        /// <param name="i"></param>
        private void DoWait(int i)
        {
            if (i > 0)
            {
                // 由于1分钟最多只能5次查询
                // 这里简单处理 多个提交操作间隔 4 到 6 秒
                var second = random.Next(4, 6);
                Thread.Sleep(1000 * second);
            }
        }

        /// <summary>
        /// 模拟请求结果判断
        /// </summary>
        /// <param name="user"></param>
        /// <param name="html"></param>
        private void DoJudgment()
        {
            var html = _httpResult.Html;
            if (html.Contains("success:true"))
            {
                // 数据更新
                _currentUser.State = (int)RequestState.UpLoadSuccess;
                _currentUser.Error_Reason = "提交成功，等待查询结果...";
                _currentUser.ReportTime = DateTime.Now;
                _currentUser.Ukey = this._loginName;

                Console.WriteLine(string.Format("证件号： {0} 提交成功，等待查询结果...", _currentUser.Cert_No));
            }
            else
            {
                Log4netAdapter.WriteInfo(string.Format("证件号： {0} 提交失败，返回html：{1}", _currentUser.Cert_No, html));
                // 数据更新
                _currentUser.State = (int)RequestState.UpLoadFail;
                _currentUser.Error_Reason = "提交失败";

                Console.WriteLine(string.Format("证件号： {0} 提交失败", _currentUser.Cert_No));
            }

            _currentUser.UpdateTime = DateTime.Now;
            _creditUserInfoBiz.UpdateEntity(_currentUser);

            Console.WriteLine(string.Format("证件号： {0} 结束提交", _currentUser.Cert_No));
            Log4netAdapter.WriteInfo(string.Format("证件号： {0} 结束提交", _currentUser.Cert_No));
        }

        /// <summary>
        /// 模拟提交查询
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private void DoRequest()
        {
            Console.WriteLine(string.Format("证件号： {0} 正在提交...", _currentUser.Cert_No));
            Log4netAdapter.WriteInfo(string.Format("证件号： {0} 正在提交...", _currentUser.Cert_No));

            var url = "https://msi.pbccrc.org.cn/sfcp/crrecord/personquery/save";
            var postData = string.Format("scardtype={0}&scardno={1}&sname={2}&spersonreason={3}",
                                       _currentUser.Cert_Type,
                                       _currentUser.Cert_No,
                                       _currentUser.Name.ToUrlEncode(),
                                       this.GetQueryReason());

            _httpItem = new HttpItem
            {
                URL = url,
                Method = "POST",
                Postdata = postData,
                Cookie = _currentCookie,
            };
            _httpItem.PostEncoding = Encoding.UTF8;
            _httpItem.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            // 注意：除了生产环境其他一律不准真正提交
            if (_isRealCommit)
                _httpResult = _httpHelper.GetHtml(_httpItem);
            else
                _httpResult = new HttpResult { Html = "{success:true}" };

            var html = _httpResult.Html;
            Console.WriteLine(string.Format("证件号： {0} 提交返回结果：{1}", _currentUser.Cert_No, html));
            Log4netAdapter.WriteInfo(string.Format("证件号： {0} 提交返回结果：{1}", _currentUser.Cert_No, html));

        }

        /// <summary>
        /// 历史原因修改查询原因code
        /// </summary>
        /// <param name="_currentUser"></param>
        /// <returns></returns>
        private string GetQueryReason()
        {
            // 查询原因编码修改， 历史原因（网站上枚举值修改过）
            var queryReason = _currentUser.QueryReason;
            switch (_currentUser.QueryReason)
            {
                case "03":
                    queryReason = "08";
                    break;
                default:
                    break;
            }
            return queryReason;
        }


    }
}
