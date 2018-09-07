using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSoup;
using ServiceStack.OrmLite;
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
using Vcredit.ExternalCredit.Dto;
using Vcredit.ExternalCredit.Dto.Assure;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.CommonLayer.helper;
using Vcredit.ExtTrade.ModelLayer.Nolmal;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure
{
    /// <summary>
    /// 担保征信上报
    /// </summary>
    public class AssureReport : AssureBase
    {
        #region properties 

        /// <summary>
        /// 放款方（金融机构）
        /// </summary>
        private String CREDIT_NAME = "";

        /// <summary>
        /// 放款方金融机构代码
        /// </summary>
        private String FINANCE_CODE = ""; 

        private Dictionary<String, String> creditor_dic = new Dictionary<string, string>();
        #endregion

        public AssureReport() : base()
        {
            _cookieToken = ConfigurationManager.AppSettings["redisCookieToken"];
        }

        #region 登陆
        /// <summary>
        /// 登陆
        /// </summary>
        /// <returns></returns>
        public ApiResultDto<AssureLoginResultDto> Login()
        {
            var result = new ApiResultDto<AssureLoginResultDto> { StatusCode = StatusCode.Fail };

            var isGetSuccess = this.CookiesIsInited();
            if (!isGetSuccess)
            {
                result.StatusDescription = "登陆失败了，暂时无法查询数据，请检查登陆接口是否正常";
                Log4netAdapter.WriteInfo(result.StatusDescription);

                return result;
            }
            result.StatusCode = StatusCode.Success;
            result.StatusDescription = "登陆成功";
            result.Result = new AssureLoginResultDto { Cookies = _currentCookies };

            return result;
        }
        #endregion

        #region 数据保存入库
        public ApiResultDto<string> AddReportInfo(AssureReportedParamDto dto)
        {
            //var entity = new CRD_CD_AssureReportedInfoEntity()
            //{
            //    GuaranteeLetterCode = dto.GuaranteeLetterCode,
            //    GuaranteeContractCode = dto.GuaranteeContractCode,
            //    WarranteeName = dto.WarranteeName,
            //    WarranteeCertNo = dto.WarranteeCertNo,
            //    GuaranteeStartDate = dto.GuaranteeStartDate,
            //    GuaranteeStopDate = dto.GuaranteeStopDate,
            //    GuaranteeSum = dto.GuaranteeSum,
            //    Rate = dto.Rate,
            //    InkeepBalance = dto.InkeepBalance,

            //    State = (int)AssureReportState.Default, // 默认状态值
            //    StateDescription = "等待上传...",
            //};

            //if (dao.Save(entity))
            //    return new ApiResultDto<string> { StatusCode = StatusCode.Success, StatusDescription = "数据保存成功，正在等待查询！" };
            //else
            return new ApiResultDto<string> { StatusCode = StatusCode.Fail, StatusDescription = "数据保存失败！" };
        }

        #endregion

        #region 担保征信上报
        /// <summary>
        /// 担保征信上报
        /// </summary>
        public void StartReported()
        {
            var entities = this.GetReportEntities();
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

                Thread.Sleep(ramdom.Next(1, 2) * 500);
                if (!this.SearchCredit())
                    continue;

                Thread.Sleep(ramdom.Next(2, 3) * 500);
                if (!this.DoStepOne())
                    continue;

                Thread.Sleep(ramdom.Next(2, 3) * 500);
                if (!this.DoStepTwo())
                    continue;

                Thread.Sleep(ramdom.Next(2, 3) * 500);
                if (!this.DoStepThree())
                    continue;

                this.DoFinal();
            }
        }

        private List<CRD_CD_AssureReportedInfoEntity> GetReportEntities()
        {
            var GuaranteeStartDateBegin = ConfigurationManager.AppSettings["GuaranteeStartDateBegin"].ToDateTime();
            var GuaranteeStartDateEnd = ConfigurationManager.AppSettings["GuaranteeStartDateEnd"].ToDateTime();
            if (GuaranteeStartDateBegin == null || GuaranteeStartDateEnd == null)
                throw new ArgumentException("appsetting 请配置 GuaranteeStartDateBegin、GuaranteeStartDateEnd");

            var jobIndex = ConfigurationManager.AppSettings["jobIndex"].ToInt();
            var jobCount = ConfigurationManager.AppSettings["jobCount"].ToInt();
            if (jobIndex == null || jobCount == null)
                throw new ArgumentException("appsettings必须配置参数jobIndex、jobCount");
            if (jobIndex >= jobCount)
                throw new ArgumentException("当前jobIndex必须小于jobCount");

            string sql = string.Format(@" SELECT TOP {0} 
                                                * 
                                            FROM credit.CRD_CD_AssureReportedInfo
                                            WHERE 1=1
                                                AND (State = {1} or State = {2})
                                                AND Id % {3} = {4}
                                                AND GuaranteeStartDate >= '{5}'
                                                AND GuaranteeStartDate <= '{6}'
                                                ORDER BY Id DESC
                                            "
                                    , _PreReportCount
                                    , (int)CommonLayer.SysEnums.AssureReportState.Default, (int)CommonLayer.SysEnums.AssureReportState.NeedReUpload
                                    , jobCount, jobIndex
                                    , GuaranteeStartDateBegin.Value.ToString("yyyy-MM-dd HH:mm:ss")
                                    , GuaranteeStartDateEnd.Value.ToString("yyyy-MM-dd HH:mm:ss"));

            var list = dao.Select<CRD_CD_AssureReportedInfoEntity>(sql);
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
                var url = "https://msi.pbccrc.org.cn/sfcp/entdata/gb/guarantcontract/getnewid?_dc=" + Commonfunction.GetTimeStamp(false);

                _httpItem = new HttpItem
                {
                    URL = url,
                    Method = "GET",
                    Cookie = _currentCookies,
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
                var entity = JsonConvert.DeserializeObject<StepResultEntity>(_httpResult.Html);
                if (entity.success)
                    this._goperationid = entity.total.ToInt(0);

                if (this._goperationid <= 0)
                {
                    var message = "操作Id获取失败，请求返回html：" + _httpResult.Html;
                    Log4netAdapter.WriteInfo(message);
                    result = false;
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
        /// 提交 第三步 
        /// 债权人和主合同信息
        /// </summary>
        /// <returns></returns>
        private bool DoStepThree()
        {
            var a = new Action(() =>
            {
                var url = "https://msi.pbccrc.org.cn/sfcp/entdata/gb/creditormaincontract/add";
                //var postData = "str={\"GbCreditorMainContract\":{\"creditortype\":\"1\",\"creditorname\":\"\u4e2d\u56fd\u5bf9\u5916\u7ecf\u6d4e\u8d38\u6613\u4fe1\u6258\u6709\u9650\u516c\u53f8\",\"creditorcerttype\":\"z\",\"creditorcertno\":\"10005071005\",\"maincontractlettercode\":\"1800065\",\"maincontractcode\":\"E181000015019001217\",\"showdirection\":\"\",\"direction\":\"\",\"goperationid\":297669,\"oproccurdate\":\"2016-09-30\",\"state\":\"1\"}}";

                var param = new StepThreeParam
                {
                    GbCreditorMainContract = new Gbcreditormaincontract
                    {
                        creditortype = "1",
                        creditorname = CREDIT_NAME.ToUnicode(),
                        creditorcerttype = "z",
                        creditorcertno = FINANCE_CODE,
                        showdirection = "",
                        direction = "",
                        state = "1",

                        maincontractlettercode = _currentEntity.MainCreditorCode,       // 主合同编号
                        maincontractcode = _currentEntity.MainContractCode,             // 主合同号码

                        oproccurdate = DateTime.Now.ToString("yyyy-MM-dd"), // "2016-09-30",
                        goperationid = this._goperationid,
                    }
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
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
            });

            return this.DoStepAction(a, RequestStep.StepThree);
        }

        /// <summary>
        /// 获取金融机构代码
        /// </summary>
        /// <returns></returns>
        private bool SearchCredit()
        {
            // %25E6%259D%25AD%25E5%25B7%259E%25E9%2593%25B6%25E8%25A1%258C%25E8%2582%25A1%25E4%25BB%25BD%25E6%259C%2589%25E9%2599%2590%25E5%2585%25AC%25E5%258F%25B8
            CREDIT_NAME = _currentEntity.CreditorName as string;
            if (string.IsNullOrEmpty(CREDIT_NAME))
            {
                var message = string.Format("担保业务编号： {0} ，担保方为空无法上报", _currentEntity.GuaranteeLetterCode);
                this.DoUploadFail(message);
                Log4netAdapter.WriteInfo(message);
                return false;
            }
            CREDIT_NAME = CREDIT_NAME.Trim();
            if (creditor_dic.ContainsKey(CREDIT_NAME))
                this.FINANCE_CODE = creditor_dic[CREDIT_NAME];
            else
            {
                var url = String.Format("https://msi.pbccrc.org.cn/sfcp/entdata/gb/listtoporg?_dc=1492422616590&financecode=&sname={0}&page=1&start=0&limit=20", CREDIT_NAME.ToUrlEncode());

                _httpItem = new HttpItem
                {
                    URL = url,
                    Method = "GET",
                    Cookie = _currentCookies,
                    Referer = "https://msi.pbccrc.org.cn/sfcp/entdata/gb/guarantcontract?fid=42"
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);

                ListTopOrgResultData data = null;
                try
                {
                    data = JsonConvert.DeserializeObject<ListTopOrgResultData>(_httpResult.Html);
                }
                catch (Exception ex)
                {
                    var message = string.Format("担保业务编号： {0} ，获取金融机构代码， 处理异常，返回Html：{1}",
                                                               _currentEntity.GuaranteeLetterCode,
                                                               _httpResult.Html
                                                           );
                    this.DoUploadFail(message);
                    Log4netAdapter.WriteError(message, ex);
                    return false;
                }

                ListTopOrgResultRoot orgResultEntity = null;
                if (data == null ||
                    data.root == null ||
                    !data.root.Any() ||
                    (orgResultEntity = data.root.Where(x => x.sname.Trim().Equals(CREDIT_NAME)).First()) == null)
                {
                    var message = string.Format("担保业务编号： {0} ，获取金融机构代码， 没有数据，返回Html：{1}",
                                                           _currentEntity.GuaranteeLetterCode,
                                                           _httpResult.Html
                                                       );
                    this.DoUploadFail(message);
                    Log4netAdapter.WriteInfo(message);
                    return false;
                }

                this.FINANCE_CODE = orgResultEntity.financecode;
                if (string.IsNullOrEmpty(this.FINANCE_CODE))
                {
                    var message = string.Format("担保业务编号： {0} ，获取金融机构代码， 代码为空", _currentEntity.GuaranteeLetterCode);
                    this.DoUploadFail(message);
                    Log4netAdapter.WriteInfo(message);
                    return false;
                }

                // 添加到字典
                creditor_dic.Add(CREDIT_NAME, FINANCE_CODE);
            }
            return true;
        }

        /// <summary>
        /// 提交 第二步
        /// </summary>
        /// <returns></returns>
        private bool DoStepTwo()
        {
            var a = new Action(() =>
            {
                var url = "https://msi.pbccrc.org.cn/sfcp/entdata/gb/realityinkeep/add";
                //var postData = "str={\"GbRealityInKeep\":{\"inkeepbalance\":\"17000\",\"balancechangedate\":\"2016-07-26\",\"guaranteecontractstatus\":\"1\",\"oproccurdate\":\"2016-09-30\",\"goperationid\":297669},\"GbNonGuarantor\":[]}";

                var param = new StepTwoParam
                {
                    GbNonGuarantor = new object[] { },
                    GbRealityInKeep = new Gbrealityinkeep
                    {
                        inkeepbalance = _currentEntity.InkeepBalance.ToString(),
                        balancechangedate = _currentEntity.GuaranteeStartDate.ToString("yyyy-MM-dd"),

                        guaranteecontractstatus = "1",

                        oproccurdate = DateTime.Now.ToString("yyyy-MM-dd"), // "2016-09-30",
                        goperationid = this._goperationid,
                    }
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
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
            });

            return this.DoStepAction(a, RequestStep.StepTwo);
        }

        /// <summary>
        /// 提交 第一步 
        /// </summary>
        private bool DoStepOne()
        {
            // 身份证号判断
            var warranteeCertNo = _currentEntity.WarranteeCertNo;
            if (!Commonfunction.IdentityCardIsValid(warranteeCertNo))
            {
                var message = string.Format("担保业务编号： {0} ，上报失败，身份证校验错误", _currentEntity.GuaranteeLetterCode);
                Log4netAdapter.WriteInfo(message);
                this.DoUploadFail(message);
                return false;
            }

            var a = new Action(() =>
            {
                var url = "https://msi.pbccrc.org.cn/sfcp/entdata/gb/guarantcontract/add";
                //var postData = "str={\"GbGuarantContract\":{\"guaranteelettercode\":\"1800065\",\"guaranteecontractcode\":\"E181000015019001217\",\"warranteetype\":\"2\",\"warranteename\":\"\u8bb8\u82b3\",\"warranteecerttype\":\"0\",\"warranteecertno\":\"130225199109180024\",\"guaranteekind\":\"01\",\"guaranteemethod\":\"1\",\"guaranteestartdate\":\"2016-07-26\",\"guaranteestopdate\":\"2018-07-26\",\"guaranteesum\":\"17000\",\"guaranteesumup\":\"\u58f9\u4e07\u67d2\u4edf\u5143\u6574\",\"cashdepositrate\":\"0\",\"nonguaranteemethod\":\"x\",\"appointguaranteerate\":\"\",\"rate\":\"0.6\",\"yearrate\":\"\",\"inkeepbalance\":\"17000\",\"inkeepbalanceup\":\"\u58f9\u4e07\u67d2\u4edf\u5143\u6574\",\"oproccurdate\":\"2016-09-30\",\"goperationid\":297669}}";

                var param = new StepOneParam
                {
                    GbGuarantContract = new Gbguarantcontract
                    {
                        guaranteelettercode = _currentEntity.GuaranteeLetterCode,
                        guaranteecontractcode = _currentEntity.GuaranteeContractCode,

                        warranteetype = "2",

                        warranteename = (_currentEntity.WarranteeName as string).ToUnicode(),
                        warranteecertno = _currentEntity.WarranteeCertNo.ToUpper(), // 身份证号最后必须是大写

                        warranteecerttype = "0",
                        guaranteekind = "01",
                        guaranteemethod = "1",

                        guaranteestartdate = _currentEntity.GuaranteeStartDate.ToString("yyyy-MM-dd"),   // 2016-07-26
                        guaranteestopdate = _currentEntity.GuaranteeStopDate.ToString("yyyy-MM-dd"),     // 2018-07-26
                        guaranteesum = _currentEntity.GuaranteeSum.ToString(),
                        guaranteesumup = (DataConvertor.CmycurD(_currentEntity.GuaranteeSum) as string).ToUnicode(),
                        inkeepbalance = _currentEntity.InkeepBalance.ToString(),
                        inkeepbalanceup = (DataConvertor.CmycurD(_currentEntity.InkeepBalance) as string).ToUnicode(),

                        cashdepositrate = "0",
                        nonguaranteemethod = "x",
                        appointguaranteerate = "",

                        rate = _currentEntity.Rate.ToString(),

                        yearrate = "",

                        oproccurdate = DateTime.Now.ToString("yyyy-MM-dd"), // "2016-09-30",
                        goperationid = this._goperationid,
                    }
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
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
            });

            return this.DoStepAction(a, RequestStep.StepOne);
        }

        #endregion


        #region 担保后信息上报

        /// <summary>
        /// 担保后信息上报
        /// </summary>
        public void AssureReportedAfter()
        {
            var entities = this.GetAfterAssureReportEntities();
            if (entities == null || !entities.Any())
                return;

            if (!this.CookiesIsInited())
                return;

            var ramdom = new Random();
            foreach (var item in entities)
            {
                _currentEntity = item;

                Thread.Sleep(ramdom.Next(1, 5) * 300);
                if (!this.GetGoperationid())
                    continue;

                Thread.Sleep(ramdom.Next(1, 2) * 500);
                if (!this.SearchCredit())
                    continue;

                Thread.Sleep(ramdom.Next(1, 5) * 300);
                if (!this.DoStepOne())
                    continue;

                Thread.Sleep(ramdom.Next(1, 5) * 300);
                if (!this.DoStepTwo())
                    continue;

                Thread.Sleep(ramdom.Next(1, 5) * 300);
                if (!this.DoStepThree())
                    continue;

                //担保后上传Step
                Thread.Sleep(ramdom.Next(1, 5) * 300);
                if (!this.DoBalanceTransfer())
                    continue;

                Thread.Sleep(ramdom.Next(1, 5) * 300);
                if (!this.GetRealityinkeepiid())
                    continue;

                Thread.Sleep(ramdom.Next(1, 5) * 300);
                if (!this.DoModifyRealityinkeep())
                    continue;

                Thread.Sleep(ramdom.Next(1, 5) * 300);
                if (!this.GetRealityinkeepiid())
                    continue;

                Thread.Sleep(ramdom.Next(1, 5) * 300);
                if (!this.DoRemoveModifyRealityinkeep())
                    continue;

                this.DoFinal();
            }
        }

        /// <summary>
        /// 获取实际在保责任
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
                    this._realityinkeep = entity.root.FirstOrDefault();
                }

                if (this._realityinkeep == null)
                {
                    var message = "操作 realityinkeep 获取失败，请求返回html：" + _httpResult.Html;
                    Log4netAdapter.WriteInfo(message);
                    result = false;
                }
            }
            catch (Exception ex)
            {
                var message = string.Format("担保业务编号： {0} ，方法名称： realityinkeep， 请求异常", _currentEntity.GuaranteeLetterCode);
                Log4netAdapter.WriteError(message, ex);

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

        /// <summary>
        /// 变更责任金额 -- 修改
        /// </summary>
        /// <returns></returns>
        private bool DoModifyRealityinkeep()
        {
            var a = new Action(() =>
            {
                var url = "https://msi.pbccrc.org.cn/sfcp/entdata/gb/realityinkeep/modify";

                var oproccurdate = DateTime.Now.ToString("yyyy-MM-dd"); // "2016-10-14",
                var balancetransferdate = _currentEntity.BalanceTransferDate.ToString("yyyy-MM-dd"); // "2016-10-14",

                var param = new ModifyGbRealityInKeepParam
                {
                    GbRealityInKeep = new GbrealityinkeepParam
                    {
                        iid = this._realityinkeep.iid,
                        dgetdate = this._realityinkeep.dgetdate,

                        guaranteecontractstatus = "1",
                        inkeepbalance = "0",

                        guaranteerelievedate = null,
                        balancechangedate = balancetransferdate, // "2016-08-22",
                        goperationid = this._goperationid,
                        validityvarydate = oproccurdate,    // "2016-10-14",
                        oproccurdate = oproccurdate,        // "2016-10-14",

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
        /// 代偿金额
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

        private List<CRD_CD_AssureReportedAfterInfoEntity> GetAfterAssureReportEntities()
        {
            var list = dao.Select<CRD_CD_AssureReportedAfterInfoEntity>(x => (x.State == (int)CommonLayer.SysEnums.AssureReportState.Default
                                                                            || x.State == (int)CommonLayer.SysEnums.AssureReportState.NeedReUpload)
                                                                    ).OrderByDescending(x => x.Id)
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

            return list;
        }
        #endregion


        #region helpClass

        public class ListTopOrgResultData
        {
            public int total { get; set; }
            public ListTopOrgResultRoot[] root { get; set; }
            public bool success { get; set; }
        }

        public class ListTopOrgResultRoot
        {
            public string sname { get; set; }
            public string financecode { get; set; }
            public object action { get; set; }
            public string columnnameoproccurdate { get; set; }
            public int start { get; set; }
            public int limit { get; set; }
        }



        #region ModifyGbRealityInKeepParam

        private class ModifyGbRealityInKeepParam
        {
            public GbrealityinkeepParam GbRealityInKeep { get; set; }
        }

        #endregion

        #region StepOneParam
        private class StepOneParam
        {
            public Gbguarantcontract GbGuarantContract { get; set; }

            //private Gbguarantcontract _GbGuarantContract = new Gbguarantcontract();
            //public Gbguarantcontract GbGuarantContract
            //{
            //    get { return _GbGuarantContract; }
            //    set { _GbGuarantContract = value; }
            //}
        }

        private class Gbguarantcontract
        {
            /// <summary>
            /// 担保业务编号
            /// </summary>
            public string guaranteelettercode { get; set; }

            /// <summary>
            /// 担保合同号码
            /// </summary>
            public string guaranteecontractcode { get; set; }

            /// <summary>
            /// 被担保人类型
            /// </summary>
            public string warranteetype { get; set; }

            /// <summary>
            /// 被担保人名称
            /// </summary>
            public string warranteename { get; set; }

            /// <summary>
            /// 证件类型
            /// </summary>
            public string warranteecerttype { get; set; }

            /// <summary>
            /// 证件号码
            /// </summary>
            public string warranteecertno { get; set; }

            /// <summary>
            /// 担保业务种类
            /// </summary>
            public string guaranteekind { get; set; }

            /// <summary>
            /// 担保方式
            /// </summary>
            public string guaranteemethod { get; set; }

            /// <summary>
            /// 担保起始日期
            /// </summary>
            public string guaranteestartdate { get; set; }

            /// <summary>
            /// 担保到期日期
            /// </summary>
            public string guaranteestopdate { get; set; }

            /// <summary>
            /// 担保金额(元)
            /// </summary>
            public string guaranteesum { get; set; }
            public string guaranteesumup { get; set; }

            /// <summary>
            /// 存出保证金比例(%)注1
            /// </summary>
            public string cashdepositrate { get; set; }

            /// <summary>
            /// 反担保方式
            /// </summary>
            public string nonguaranteemethod { get; set; }

            /// <summary>
            /// 约定再担保补偿比例(%)注2
            /// </summary>
            public string appointguaranteerate { get; set; }

            /// <summary>
            /// 费率(%)注3
            /// </summary>
            public string rate { get; set; }

            /// <summary>
            /// 年化费率(%)注4
            /// </summary>
            public string yearrate { get; set; }

            /// <summary>
            /// 在保余额
            /// </summary>
            public string inkeepbalance { get; set; }
            public string inkeepbalanceup { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string oproccurdate { get; set; }

            /// <summary>
            /// 唯一编号
            /// </summary>
            public int goperationid { get; set; }
        }
        #endregion

        #region StepTwoParam
        private class StepTwoParam
        {
            public Gbrealityinkeep GbRealityInKeep { get; set; }

            //private Gbrealityinkeep _GbRealityInKeep = new Gbrealityinkeep();
            //public Gbrealityinkeep GbRealityInKeep
            //{
            //    get { return _GbRealityInKeep; }
            //    set { _GbRealityInKeep = value; }
            //}

            public object[] GbNonGuarantor { get; set; }

            //private object[] _GbNonGuarantor = new object[] { };
            //public object[] GbNonGuarantor
            //{
            //    get { return _GbNonGuarantor; }
            //    set { _GbNonGuarantor = value; }
            //}
        }

        private class Gbrealityinkeep
        {
            public string inkeepbalance { get; set; }
            public string balancechangedate { get; set; }
            public string guaranteecontractstatus { get; set; }
            public string oproccurdate { get; set; }
            public int goperationid { get; set; }
        }
        #endregion

        #region StepThreeParam
        private class StepThreeParam
        {
            public Gbcreditormaincontract GbCreditorMainContract { get; set; }
            //private Gbcreditormaincontract _Gbcreditormaincontract = new Gbcreditormaincontract();

            //public Gbcreditormaincontract Gbcreditormaincontract
            //{
            //    get { return _Gbcreditormaincontract; }
            //    set { _Gbcreditormaincontract = value; }
            //}
        }

        private class Gbcreditormaincontract
        {
            public string creditortype { get; set; }
            public string creditorname { get; set; }
            public string creditorcerttype { get; set; }
            public string creditorcertno { get; set; }
            public string maincontractlettercode { get; set; }
            public string maincontractcode { get; set; }
            public string showdirection { get; set; }
            public string direction { get; set; }
            public int goperationid { get; set; }
            public string oproccurdate { get; set; }
            public string state { get; set; }
        }
        #endregion

        #endregion
    }
}
