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
using Vcredit.ExternalCredit.CommonLayer.Extension;
using Vcredit.ExternalCredit.CommonLayer.helper;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure
{
    /// <summary>
    /// 担保维护
    /// </summary>
    public class AssureMaintain : AssureBase
    {
        public AssureMaintain() : base()
        {
            _cookieToken = ConfigurationManager.AppSettings["redisCookieToken"];
        }

        /// <summary>
        /// 剩余本金维护（正常在保余额）
        /// </summary>
        /// <returns></returns>
        public void StartMaintain()
        {
            var entities = this.GetEntities();
            if (entities == null || !entities.Any())
                return;

            if (!this.CookiesIsInited())
                return;
            //_currentCookies = "JSESSIONID=8Q41ZvXWvz1RbD8GgyxPZG910L35HF4q2HWPLTqhL01mh7nSDCRY!772171844;BIGipServerpool_xwqy=l8enPSQ6ZvV27KNn8V2JIwSNn8oszplty6QzV+hQWR/TiS/PZqLDU1ixUb+seLjhWbEMq/vSpairJZo=";

            var ramdom = new Random();
            foreach (var item in entities)
            {
                _currentEntity = item;

                Thread.Sleep(ramdom.Next(2, 3) * 500);
                if (!this.GetGoperationid())
                    continue;

                Thread.Sleep(ramdom.Next(2, 3) * 500);
                if (!this.GetRealityinkeepiid())
                    continue;

                Thread.Sleep(ramdom.Next(2, 3) * 500);
                if (!this.DoModifyRealityinkeep())
                    continue;

                if (_currentEntity.IsEnd)
                {
                    Thread.Sleep(ramdom.Next(2, 3) * 500);
                    if (!this.GetRealityinkeepiid())
                        continue;

                    Thread.Sleep(ramdom.Next(2, 3) * 500);
                    if (!this.DoRemoveModifyRealityinkeep())
                        continue;
                }

                this.DoFinal();
            }
        }

        /// <summary>
        /// 变更责任金额 -- 添加
        /// </summary>
        /// <returns></returns>
        private bool DoModifyRealityinkeep()
        {
            var a = new Action(() =>
            {
                var url = "https://msi.pbccrc.org.cn/sfcp/entdata/gb/guarantcontract/add";
                //var postData = "str={"GbRealityInKeep":{"inkeepbalance":"5333","inkeepbalanceup":"\u4f0d\u4edf\u53c1\u4f70\u53c1\u62fe\u53c1\u5143\u6574","balancechangedate":"2016-10-31","goperationid":314272,"guaranteecontractstatus":"1","oproccurdate":"2016-11-11"}}";

                var param = new UpdateInkeepBalanceParam
                {
                    GbRealityInKeep = new Gbrealityinkeep
                    {
                        inkeepbalance = _currentEntity.InkeepBalance.ToString(),
                        inkeepbalanceup = (DataConvertor.CmycurD(_currentEntity.InkeepBalance) as string).ToUnicode(),
                        balancechangedate = _currentEntity.Balancechangedate.ToString("yyyy-MM-dd"),
                        guaranteecontractstatus = "1",

                        goperationid = this._goperationid,
                        oproccurdate = DateTime.Now.ToString("yyyy-MM-dd"), // "2016-09-30",
                    },
                };
                var postData = string.Format("str={0}", JsonConvert.SerializeObject(param).Replace(@"\\u", @"\u"));

                _httpItem = new HttpItem
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postData,
                    Cookie = _currentCookies,
                    PostEncoding = Encoding.UTF8,
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    Referer = "https://msi.pbccrc.org.cn/sfcp/entdata/gb/guarantcontract?fid=42"
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
            });

            return this.DoStepAction(a, RequestStep.StepOne);
        }

        /// <summary>
        /// 获取实际在保责任（变更责任金额）
        /// </summary>
        /// <returns></returns>
        private bool GetRealityinkeepiid()
        {
            var result = true;
            try
            {
                var url = string.Format("https://msi.pbccrc.org.cn/sfcp/entdata/gb/realityinkeep/getHistroy?_dc={0}&goperationid={1}&page=1&start=0&limit=20", Commonfunction.GetTimeStamp(false), this._goperationid);

                _httpItem = new HttpItem
                {
                    URL = url,
                    Method = "GET",
                    Cookie = _currentCookies,
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
                var entity = JsonConvert.DeserializeObject<RealityinkeepObject>(_httpResult.Html);
                if (entity.success && entity.root != null && entity.root.Any())
                {
                    // 详见 https://msi.pbccrc.org.cn/js/bjs/gb.js?ver=10
                    this._realityinkeep = entity.root.FirstOrDefault();
                    if (IsLock(_realityinkeep.ireportstate, _realityinkeep.recordoprtypeofinfo))
                    {
                        var message = "数据已锁定，无法修改";
                        Log4netAdapter.WriteInfo(message);
                        this.DoUploadFail(message);
                        result = false;
                    }
                    if (_realityinkeep.ireportstate == 6)
                    {
                        var message = "业务有错误，请删除后重报";
                        Log4netAdapter.WriteInfo(message);
                        this.DoUploadFail(message);
                        result = false;
                    }
                    if (_realityinkeep.guaranteecontractstatus == "2")
                    {
                        var message = "担保合同状态为无效，不能再变更责任金额";
                        Log4netAdapter.WriteInfo(message);
                        this.DoUploadFail(message);
                        result = false;
                    }
                }

                if (this._realityinkeep == null)
                {
                    var message = "操作 realityinkeep 获取失败，请求返回html：" + _httpResult.Html;
                    Log4netAdapter.WriteInfo(message);
                    this.DoUploadFail(message);
                    result = false;
                }
            }
            catch (Exception ex)
            {
                var message = string.Format("担保业务编号： {0} ，方法名称： realityinkeep， 请求异常", _currentEntity.GuaranteeLetterCode);
                Log4netAdapter.WriteError(message, ex);
                this.DoUploadFail(message);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 解除担保责任
        /// </summary>
        /// <returns></returns>
        private bool DoRemoveModifyRealityinkeep()
        {
            var a = new Action(() =>
            {
                var url = "https://msi.pbccrc.org.cn/sfcp/entdata/gb/realityinkeep/modify ";

                var oproccurdate = DateTime.Now.ToString("yyyy-MM-dd"); // "2016-10-14",
                var balancetransferdate = _currentEntity.Balancechangedate.ToString("yyyy-MM-dd"); // "2016-10-14",

                var param = new RemoveGbRealityInKeepParam
                {
                    GbRealityInKeep = new GbrealityinkeepParam
                    {
                        iid = this._realityinkeep.iid,
                        dgetdate = this._realityinkeep.dgetdate,

                        guaranteecontractstatus = "2",
                        inkeepbalance = "0",

                        guaranteerelievedate = balancetransferdate, //"2016-08-22",
                        balancechangedate = balancetransferdate,    //"2016-08-22",
                        goperationid = this._goperationid,
                        validityvarydate = oproccurdate,// "2016-10-14",
                        oproccurdate = oproccurdate,    //"2016-10-14",

                        recordoprtypeofinfo = "1",
                        validityflag = "1",
                        ireportstate = "2",
                        id = null
                    },
                };

                var postData = string.Format("str={0}", JsonConvert.SerializeObject(param).Replace(@"\\u", @"\u"));

                _httpItem = new HttpItem
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postData,
                    Cookie = _currentCookies,
                    PostEncoding = Encoding.UTF8,
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    Referer = "https://msi.pbccrc.org.cn/sfcp/entdata/gb/guarantcontract?fid=42",
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
            });

            return this.DoStepAction(a, RequestStep.StepFive);
        }

        /// <summary>
        /// 获取需要维护的实体
        /// </summary>
        /// <returns></returns>
        private List<CRD_CD_AssureMaintainEntity> GetEntities()
        {
            var BalancechangedateBegin = ConfigurationManager.AppSettings["BalancechangedateBegin"].ToDateTime();
            var BalancechangedateEnd = ConfigurationManager.AppSettings["BalancechangedateEnd"].ToDateTime();
            if (BalancechangedateBegin == null || BalancechangedateEnd == null)
                throw new ArgumentException("appsetting 请配置 BalancechangedateBegin、BalancechangedateEnd");

            var jobIndex = ConfigurationManager.AppSettings["jobIndex"].ToInt();
            var jobCount = ConfigurationManager.AppSettings["jobCount"].ToInt();
            if (jobIndex == null || jobCount == null)
                throw new ArgumentException("appsettings必须配置参数jobIndex、jobCount");
            if (jobIndex >= jobCount)
                throw new ArgumentException("当前jobIndex必须小于jobCount");

            // 这里多UKEY 根据 GuaranteeLetterCode 取余，并按照Id顺序取数据
            string sql = string.Format(@" SELECT TOP {0} 
                                                * 
                                            FROM credit.CRD_CD_AssureMaintainInfo
                                            WHERE 1=1
                                                AND (State = {1} or State = {2})
                                                AND GuaranteeLetterCode % {3} = {4}
                                                AND BalancechangeDate >= '{5}'
                                                AND BalancechangeDate <= '{6}'
                                                ORDER BY Id
                                            "
                                , _PreReportCount
                                , (int)CommonLayer.SysEnums.AssureReportState.Default, (int)CommonLayer.SysEnums.AssureReportState.NeedReUpload
                                , jobCount, jobIndex
                                , BalancechangedateBegin.Value.ToString("yyyy-MM-dd HH:mm:ss")
                                , BalancechangedateEnd.Value.ToString("yyyy-MM-dd HH:mm:ss"));

            var list = dao.Select<CRD_CD_AssureMaintainEntity>(sql);

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
                    item.StateDescription = "上报进程正在排队上报";
                    item.UpdateTime = DateTime.Now;
                }
                dao.SaveAll(list);
            }

            var log = string.Format("准备上报{0}条数据", list.Count);
            Log4netAdapter.WriteInfo(log);

            return list;
        }

        /// <summary>
        /// 获取操作id
        /// </summary>
        /// <returns></returns>
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
                    if (IsLock(report.ireportstate, report.recordoprtypeofinfo))
                    {
                        var message = "数据已锁定，无法修改";
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
                var message = string.Format("担保业务编号： {0} ，方法名称： GetGoperationid， 请求异常"
                                            , _currentEntity.GuaranteeLetterCode);
                this.DoNeedReUpload(message);
                Log4netAdapter.WriteError(message, ex);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 代偿信息
        /// </summary>
        /// <returns></returns>
        public void StartMaintainWithBalanceTransfer()
        {
            var entities = this.GetBalanceTransferEntities();
            if (entities == null || !entities.Any())
                return;

            if (!this.CookiesIsInited())
                return;

            var ramdom = new Random();
            foreach (var item in entities)
            {
                _currentEntity = item;

                Thread.Sleep(ramdom.Next(2, 3) * 500);
                if (!this.GetGoperationid())
                    continue;

                Thread.Sleep(ramdom.Next(2, 3) * 500);
                if (!this.GetRealityinkeepiid())
                    continue;

                Thread.Sleep(ramdom.Next(2, 3) * 500);
                if (!this.DoBalanceTransfer())
                    continue;

                Thread.Sleep(ramdom.Next(2, 3) * 500);
                if (!this.DoModifyRealityinkeep_Zero())
                    continue;

                Thread.Sleep(ramdom.Next(2, 3) * 500);
                if (!this.GetRealityinkeepiid())
                    continue;

                Thread.Sleep(ramdom.Next(2, 3) * 500);
                if (!this.DoRemoveModifyRealityinkeep_WithBalanceTransfer())
                    continue;

                this.DoFinal();
            }
        }

        private List<CRD_CD_AssureMaintainBalanceTransferEntity> GetBalanceTransferEntities()
        {
            var list = dao.Select<CRD_CD_AssureMaintainBalanceTransferEntity>(x =>
                                (x.State == (int)CommonLayer.SysEnums.AssureReportState.Default ||
                                    x.State == (int)CommonLayer.SysEnums.AssureReportState.NeedReUpload)
                                )
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
                    item.StateDescription = "上报进程正在排队上报";
                    item.UpdateTime = DateTime.Now;
                }
                dao.SaveAll(list);
            }

            var log = string.Format("准备上报{0}条数据", list.Count);
            Log4netAdapter.WriteInfo(log);

            return list;
        }

        /// <summary>
        /// 添加代偿金额
        /// </summary>
        /// <returns></returns>
        private bool DoBalanceTransfer()
        {
            var a = new Action(() =>
            {
                var url = "https://msi.pbccrc.org.cn//sfcp/entdata/gb/subrogationdetail/modify";

                var oproccurdate = DateTime.Now.ToString("yyyy-MM-dd"); // "2016-10-14",
                var balancetransferdate = _currentEntity.BalanceTransferDate.ToString("yyyy-MM-dd"); // "2016-10-14",
                var balanceTransferSumInt = Convert.ToInt32(_currentEntity.BalanceTransferSum);
                var balanceTransferSumStr = _currentEntity.BalanceTransferSum.ToString();

                var param = new BalanceTransferParam
                {
                    GbBalanceTransfer = new Gbbalancetransfer
                    {
                        subrogationflag = "1",

                        addupbalancetransfersum = balanceTransferSumInt, // 3112
                        assumebalancetransfersum = balanceTransferSumInt, // 3112
                        balancetransferbalance = balanceTransferSumInt, // 3112
                        assumebalancetransferbalance = balanceTransferSumInt, // 3112,

                        addupsubrogationsum = 0,
                        adduplosssum = 0,

                        balancetransferdate = balancetransferdate, // "2016-08-22",
                        balancetransfersum = balanceTransferSumStr, // "3112",
                        keepaccountsdate = balancetransferdate, // "2016-08-22",
                        recentbalancetransferdate = balancetransferdate, // "2016-08-22",

                        goperationid = this._goperationid,
                        oproccurdate = oproccurdate
                    },
                    GbBalanceTransferDetail = new Gbbalancetransferdetail
                    {
                        balancetransferdate = balancetransferdate, // "2016-08-22",
                        balancetransfersum = balanceTransferSumStr, // "3112",

                        goperationid = this._goperationid,
                        oproccurdate = oproccurdate
                    },
                };

                var postData = string.Format("str={0}", JsonConvert.SerializeObject(param).Replace(@"\\u", @"\u"));

                _httpItem = new HttpItem
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postData,
                    Cookie = _currentCookies,
                    PostEncoding = Encoding.UTF8,
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    Referer = "https://msi.pbccrc.org.cn/sfcp/entdata/gb/guarantcontract?fid=42",
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
            });

            return this.DoStepAction(a, RequestStep.StepFour);
        }

        /// <summary>
        /// 变更责任金额为0 -- 添加
        /// </summary>
        /// <returns></returns>
        private bool DoModifyRealityinkeep_Zero()
        {
            var a = new Action(() =>
            {
                var url = "https://msi.pbccrc.org.cn/sfcp/entdata/gb/realityinkeep/modify";
                //var postData = "str={"GbRealityInKeep":{"inkeepbalance":"5333","inkeepbalanceup":"\u4f0d\u4edf\u53c1\u4f70\u53c1\u62fe\u53c1\u5143\u6574","balancechangedate":"2016-10-31","goperationid":314272,"guaranteecontractstatus":"1","oproccurdate":"2016-11-11"}}";

                var balancetransferdate = _currentEntity.BalanceTransferDate.ToString("yyyy-MM-dd"); // "2016-10-14",
                var param = new UpdateInkeepBalanceParam
                {
                    GbRealityInKeep = new Gbrealityinkeep
                    {
                        inkeepbalance = "0",
                        inkeepbalanceup = ("零元整").ToUnicode(),
                        balancechangedate = balancetransferdate,
                        guaranteecontractstatus = "1",

                        goperationid = this._goperationid,
                        oproccurdate = DateTime.Now.ToString("yyyy-MM-dd"), // "2016-09-30",
                    },
                };
                var postData = string.Format("str={0}", JsonConvert.SerializeObject(param).Replace(@"\\u", @"\u"));

                _httpItem = new HttpItem
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postData,
                    Cookie = _currentCookies,
                    PostEncoding = Encoding.UTF8,
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    Referer = "https://msi.pbccrc.org.cn/sfcp/entdata/gb/guarantcontract?fid=42"
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
            });

            return this.DoStepAction(a, RequestStep.StepOne);
        }

        /// <summary>
        /// 解除担保责任（代偿信息）
        /// </summary>
        /// <returns></returns>
        private bool DoRemoveModifyRealityinkeep_WithBalanceTransfer()
        {
            var a = new Action(() =>
            {
                var url = "https://msi.pbccrc.org.cn/sfcp/entdata/gb/realityinkeep/modify ";

                var oproccurdate = DateTime.Now.ToString("yyyy-MM-dd"); // "2016-10-14",
                var balancetransferdate = _currentEntity.BalanceTransferDate.ToString("yyyy-MM-dd"); // "2016-10-14",

                var param = new RemoveGbRealityInKeepParam
                {
                    GbRealityInKeep = new GbrealityinkeepParam
                    {
                        iid = this._realityinkeep.iid,
                        dgetdate = this._realityinkeep.dgetdate,

                        guaranteecontractstatus = "2",
                        inkeepbalance = "0",

                        guaranteerelievedate = balancetransferdate, //"2016-08-22",
                        balancechangedate = balancetransferdate,    //"2016-08-22",
                        goperationid = this._goperationid,
                        validityvarydate = oproccurdate,// "2016-10-14",
                        oproccurdate = oproccurdate,    //"2016-10-14",

                        recordoprtypeofinfo = "1",
                        validityflag = "1",
                        ireportstate = "2",
                        id = null
                    },
                };

                var postData = string.Format("str={0}", JsonConvert.SerializeObject(param).Replace(@"\\u", @"\u"));

                _httpItem = new HttpItem
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postData,
                    Cookie = _currentCookies,
                    PostEncoding = Encoding.UTF8,
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    Referer = "https://msi.pbccrc.org.cn/sfcp/entdata/gb/guarantcontract?fid=42",
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
            });

            return this.DoStepAction(a, RequestStep.StepFive);
        }


        #region helpClass

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

        #region UpdateInkeepBalanceParam

        private class UpdateInkeepBalanceParam
        {
            public Gbrealityinkeep GbRealityInKeep { get; set; }
        }

        private class Gbrealityinkeep
        {
            public string inkeepbalance { get; set; }
            public string inkeepbalanceup { get; set; }
            public string balancechangedate { get; set; }
            public int goperationid { get; set; }
            public string guaranteecontractstatus { get; set; }
            public string oproccurdate { get; set; }
        }

        #endregion

        #endregion
    }
}
