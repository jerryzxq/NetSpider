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
using Vcredit.ExternalCredit.CommonLayer.Extension;
using Vcredit.ExtTrade.ModelLayer.Nolmal;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure
{


    public class AssureDelete : AssureBase
    {

        public AssureDelete() : base()
        {
            _cookieToken = ConfigurationManager.AppSettings["redisCookieToken"];
        }

        /// <summary>
        /// 数据删除
        /// </summary>
        /// <returns></returns>
        public void StartDelete()
        {
            var entities = this.GetEntities();
            if (entities == null || !entities.Any())
                return;

            if (!this.CookiesIsInited())
                return;

            var ramdom = new Random();
            foreach (var item in entities)
            {
                _currentEntity = item;

                Thread.Sleep(ramdom.Next(2, 3) * 100);
                if (!this.GetGoperationid())
                    continue;

                Thread.Sleep(ramdom.Next(2, 3) * 100);
                if (!this.DoDelete())
                    continue;

                this.DoDeleteFinal();
            }
        }

        private bool DoDelete()
        {
            var a = new Action(() =>
            {
                var url = string.Format("https://msi.pbccrc.org.cn/sfcp/entdata/gb/guarantcontract/{0}/delete/0", this._goperationid);

                _httpItem = new HttpItem
                {
                    URL = url,
                    Method = "DELETE",
                    Cookie = _currentCookies,
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    Referer = "https://msi.pbccrc.org.cn/sfcp/entdata/gb/guarantcontract?fid=42",
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
            });

            return this.DoStepAction(a, RequestStep.StepDelete);
        }

        private void DoDeleteFinal()
        {
            var message = string.Format("担保业务编号： {0} ，删除成功", _currentEntity.GuaranteeLetterCode);
            Log4netAdapter.WriteInfo(message);

            _currentEntity.State = (int)CommonLayer.SysEnums.AssureReportState.UpLoadSuccess;
            _currentEntity.StateDescription = "删除成功";
            _currentEntity.UpdateTime = DateTime.Now;
            _currentEntity.Ukey = _loginName;
            dao.Save(_currentEntity);
        }

        private bool GetGoperationid()
        {
            var result = true;
            try
            {
                var warranteename = (_currentEntity.Warranteename as string);
                var guaranteeLetterCode = _currentEntity.GuaranteeLetterCode as string;
                if (string.IsNullOrEmpty(warranteename) || string.IsNullOrEmpty(guaranteeLetterCode))
                    throw new ArgumentException("warranteename|guaranteeLetterCode 为空无法获取维护信息", "warranteename|guaranteeLetterCode");

                var page = 1;
                var totalPage = 1;
                var limit = 20;
                Report report = null;
                while (page <= totalPage)
                {
                    // GuaranteeLetterCode 是模糊搜索，下面数据需要处理过滤
                    var url = string.Format("https://msi.pbccrc.org.cn/sfcp/entdata/gb/guarantcontract/page?_dc={0}&guaranteelettercode={1}&warranteename={2}&warranteetype=&guaranteekind=&guaranteemethod=&page=1&start=0&limit={3}"
                        , Commonfunction.GetTimeStamp(false)
                        , guaranteeLetterCode
                        , warranteename.ToUrlEncode()
                        , limit);

                    _httpItem = new HttpItem
                    {
                        URL = url,
                        Method = "GET",
                        Cookie = _currentCookies,
                        Referer = "https://msi.pbccrc.org.cn/sfcp/entdata/gb/guarantcontract?fid=42"
                    };
                    _httpResult = _httpHelper.GetHtml(_httpItem);
                    var entities = JsonConvert.DeserializeObject<GuarantcontractPageObject>(_httpResult.Html);
                    if (entities.success && entities.root != null && entities.root.Any())
                    {
                        if (page == 1)
                            totalPage = entities.total % limit;

                        report = entities.root.Where(x => x.guaranteelettercode.Equals(guaranteeLetterCode) &&
                                                          x.warranteename.Equals(warranteename)).FirstOrDefault();
                    }

                    if (report != null)
                        break;

                    page++;
                    Thread.Sleep(1000);
                }

                if (report == null || report.goperationid.ToInt(0) <= 0)
                {
                    var message = "操作Id获取失败，请求返回html：" + _httpResult.Html;
                    Log4netAdapter.WriteInfo(message);
                    this.DoUploadFail(message);
                    result = false;
                }
                else
                {
                    if (!IsFeedBackFalse(report.ireportstate) && this.GetFeedBackFalseData)
                    {
                        var message = string.Format("担保业务编号： {0} ，当前数据不是“反馈错误”状态，无法修改", _currentEntity.GuaranteeLetterCode);
                        Log4netAdapter.WriteInfo(message);
                        this.DoUploadFail(message);
                        result = false;
                    }
                    if (IsLock(report.ireportstate, report.recordoprtypeofinfo))
                    {
                        var message = string.Format("担保业务编号： {0} ，数据已锁定，无法修改", _currentEntity.GuaranteeLetterCode);
                        Log4netAdapter.WriteInfo(message);
                        this.DoUploadFail(message);
                        result = false;
                    }
                    else
                        this._goperationid = report.goperationid.ToInt(0);
                }
            }
            catch (Exception ex)
            {
                var message = string.Format("担保业务编号： {0} ，方法名称： GetGoperationid， 请求异常", _currentEntity.GuaranteeLetterCode);
                this.DoNeedReUpload(message);
                Log4netAdapter.WriteError(message, ex);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 判断是否反馈报错  ireportstate == 6
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        protected bool IsFeedBackFalse(int ireportstate)
        {
            return (ireportstate == 6);
        }

        private bool GetFeedBackFalseData
        {
            get { return ConfigurationManager.AppSettings["getFeedBackFalseData"].ToBool(false); }
        }


        private List<CRD_CD_AssurDeleteEntity> GetEntities()
        {
            var list = dao.Select<CRD_CD_AssurDeleteEntity>(x => (x.State == (int)CommonLayer.SysEnums.AssureReportState.Default ||
                                                                    x.State == (int)CommonLayer.SysEnums.AssureReportState.NeedReUpload))
                                        .OrderByDescending(x => x.Id)
                                        .Take(_PreReportCount)
                                        .ToList();

            if (list == null || !list.Any())
            {
                var message = "暂时没有可上报的数据";
                Log4netAdapter.WriteInfo(message);
            }
            else
            {
                foreach (var item in list)
                {
                    item.State = (int)CommonLayer.SysEnums.AssureReportState.UpLoading;
                    item.StateDescription = "删除进程正在排队删除";
                    item.UpdateTime = DateTime.Now;
                }
                dao.SaveAll(list);
            }

            var log = string.Format("准备上报{0}条数据", list.Count);
            Log4netAdapter.WriteInfo(log);

            return list;
        }

        #region GuarantcontractPageObject
        private class GuarantcontractPageObject
        {
            public int total { get; set; }
            public Report[] root { get; set; }
            public bool success { get; set; }
        }
        private class Report
        {
            public string iid { get; set; }
            public string financecode { get; set; }
            public string dgetdate { get; set; }
            public int ireportstate { get; set; }
            public string goperationid { get; set; }
            public string nonguaranteemethod { get; set; }
            public string oproccurdate { get; set; }
            public string warranteecerttype { get; set; }
            public string warranteecertno { get; set; }
            public string warranteetype { get; set; }
            public string warranteename { get; set; }
            public int recordoprtypeofinfo { get; set; }
            public string guaranteecontractcode { get; set; }
            public string guaranteelettercode { get; set; }
            public string validityflag { get; set; }
            public string validityvarydate { get; set; }
            public string guaranteesum { get; set; }
            public string guaranteekind { get; set; }
            public string guaranteemethod { get; set; }
            public string guaranteestartdate { get; set; }
            public string guaranteestopdate { get; set; }
            public string cashdepositrate { get; set; }
            public object appostringguaranteerate { get; set; }
            public string rate { get; set; }
            public object yearrate { get; set; }
            public object action { get; set; }
            public string columnnameoproccurdate { get; set; }
            public string start { get; set; }
            public string limit { get; set; }
        }
        #endregion
    }
}
