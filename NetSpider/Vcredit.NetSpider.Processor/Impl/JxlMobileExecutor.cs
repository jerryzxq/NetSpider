using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Crawler.Mobile;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.Mobile;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.Processor.Operation;
using Vcredit.NetSpider.Service;
using Vcredit.Common.Ext;
using System.Threading.Tasks;
using Vcredit.Common;

namespace Vcredit.NetSpider.Processor.Impl
{
    public class JxlMobileExecutor : IMobileExecutor
    {
        #region 初始化
        public VerCodeRes MobileInit(MobileReq mobileReq)
        {
            VerCodeRes Res = new VerCodeRes();
            if (string.IsNullOrEmpty(mobileReq.IdentityCard) || string.IsNullOrEmpty(mobileReq.Name) || string.IsNullOrEmpty(mobileReq.Mobile))
            {
                Res.StatusDescription = ServiceConsts.IdentityCardOrNameOrMobileEmpty;
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                return Res;
            }
            IMobileCrawler crawler = CreateMobileCrawlerByWebsite("jxl");
            if (crawler != null)
            {
                mobileReq.Website = GetWebsite(mobileReq.Mobile);
                Res = crawler.MobileInit(mobileReq);
            }
            else
                Res.Website = null;
            return Res;
        }

        public VerCodeRes MobileInit(string mobile)
        {
            if (string.IsNullOrEmpty(mobile))
                return new VerCodeRes() { StatusCode = ServiceConsts.StatusCode_fail, StatusDescription = ServiceConsts.MobileEmpty };
            throw new NotImplementedException();
        }

        #endregion

        #region 登录

        public BaseRes MobileLogin(MobileReq mobileReq)
        {
            BaseRes Res = new BaseRes();
            int crawlerCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
            if (string.IsNullOrEmpty(mobileReq.Token) || string.IsNullOrEmpty(mobileReq.IdentityCard) || string.IsNullOrEmpty(mobileReq.Name) || string.IsNullOrEmpty(mobileReq.Mobile) || string.IsNullOrEmpty(mobileReq.Password))
            {
                Res.StatusDescription = ServiceConsts.TokenOrIdentityCardOrNameOrMobileOrPwdEmpty;
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                return Res;
            }
            //判断是否有最近提交
            //IOperationLog oprService = NetSpiderFactoryManager.GetOperationLogService();
            //OperationLogEntity operationLog = oprService.GetByIdentityNoAndNameAndMobile(mobileReq.IdentityCard, mobileReq.Name, mobileReq.Mobile);
            //if (operationLog != null && operationLog.SendTime.AddDays(5) > DateTime.Now)
            //{
            //    Res.StatusCode = ServiceConsts.StatusCode_success;
            //    Res.nextProCode = ServiceConsts.NextProCode_Query;
            //    Res.Result = "{\"Id\":\"" + "jxl_" + operationLog.Id + "\"}";
            //    return Res;
            //}
            IMobileCrawler crawler = CreateMobileCrawlerByWebsite("jxl");
            Res = crawler.MobileLogin(mobileReq);
            Res.Website = mobileReq.Website;
            crawlerCode = Res.StatusCode;
            Res.StatusCode = Res.StatusCode == ServiceConsts.CrawlerStatusCode_CheckSuccess || Res.StatusCode == ServiceConsts.StatusCode_success ? ServiceConsts.StatusCode_success : ServiceConsts.StatusCode_fail;

            if (Res.StatusCode == ServiceConsts.StatusCode_success)
            {
                if (Res.nextProCode == ServiceConsts.NextProCode_Query)
                {
                    try
                    {
                        Res.Result = "{\"Id\":\"" + "jxl_" + SaveOperationLog(mobileReq).Id + "\"}";
                    }
                    catch (Exception e)
                    {
                        Log4netAdapter.WriteError("新增OperationLog(手机" + mobileReq.Mobile + ") 异常", e);
                        throw new Exception(e.Message);
                    }
                }
                var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    try
                    {
                        SaveAppLog(mobileReq, Res, crawlerCode);
                    }
                    catch (Exception e)
                    {
                        Log4netAdapter.WriteError("保存AppLog(手机" + mobileReq.Mobile+ ") 异常", e);
                    }
                });
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted || task.Status == TaskStatus.Canceled)
                    task.Dispose();
            }

            return Res;
        }

        #endregion

        #region 发送和校验短信验证码

        public VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            throw new NotImplementedException();
        }

        public BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            BaseRes Res = new BaseRes();
            int crawlerCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
            if (string.IsNullOrEmpty(mobileReq.Mobile) || string.IsNullOrEmpty(mobileReq.Token) || string.IsNullOrEmpty(mobileReq.Smscode))
            {
                Res.StatusDescription = ServiceConsts.TokenOrMobileOrSmscodeEmpty;
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                return Res;
            }
            if (string.IsNullOrEmpty(mobileReq.IdentityCard) || string.IsNullOrEmpty(mobileReq.Name))
            {
                ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
                Spd_applyEntity applyEntity = applyService.GetByToken(mobileReq.Token);
                if (applyEntity != null)
                {
                    mobileReq.Name = applyEntity.Name;
                    mobileReq.IdentityCard = applyEntity.Identitycard;
                }
            }
            IMobileCrawler crawler = CreateMobileCrawlerByWebsite("jxl");
            Res = crawler.MobileCheckSms(mobileReq);
            Res.Website = mobileReq.Website;
            crawlerCode = Res.StatusCode;
            Res.StatusCode = Res.StatusCode == ServiceConsts.CrawlerStatusCode_CheckSuccess || Res.StatusCode == ServiceConsts.StatusCode_success ? ServiceConsts.StatusCode_success : ServiceConsts.StatusCode_fail;

            if (Res.StatusCode == ServiceConsts.StatusCode_success)
            {
                if (Res.nextProCode == ServiceConsts.NextProCode_Query)
                {
                    try
                    {
                        Res.Result = "{\"Id\":\"" + "jxl_" + SaveOperationLog(mobileReq).Id + "\"}";
                    }
                    catch (Exception e)
                    {
                        Log4netAdapter.WriteError("新增OperationLog(手机" + mobileReq.Mobile + ") 异常", e);
                        throw new Exception(e.Message);
                    }
                }
            }
            //异步保存登录信息
            var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    SaveAppLog(mobileReq, Res, crawlerCode);
                }
                catch (Exception e)
                {
                    Log4netAdapter.WriteError("更新AppLog (手机" + mobileReq.Mobile + ")异常", e);
                }
            });
            if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted || task.Status == TaskStatus.Canceled)
                task.Dispose();

            return Res;
        }

        #endregion

        #region 解析

        public BaseRes MobileAnalysis(MobileReq mobileReq, DateTime crawlerDate)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region 查询

        public Summary MobileSummaryQuery(MobileReq mobileReq)
        {
            if (string.IsNullOrEmpty(mobileReq.Token) && (string.IsNullOrEmpty(mobileReq.IdentityCard) || string.IsNullOrEmpty(mobileReq.Mobile))) return null;
            Summary sunmmary = null;
            try
            {
                ISummary service = NetSpiderFactoryManager.GetSummaryService();
                var entity = service.GetByIdentityNoAndMobile(mobileReq.IdentityCard, mobileReq.Mobile);
                if (entity != null)
                {
                    IBasic basicService = NetSpiderFactoryManager.GetBasicService();
                    BasicEntity basic = basicService.GetByOperid((int)entity.Oper_id);
                    if (String.IsNullOrWhiteSpace(entity.Regdate) && basic != null)
                        entity.Regdate = basic.Reg_time;
                    entity.OperationLog = null;
                    sunmmary = new Summary();
                    sunmmary.City = entity.City;
                    sunmmary.Regdate = entity.Regdate;
                    if (basic != null)
                        sunmmary.IdentityCard = basic != null ? basic.Idcard : "";
                    sunmmary.Mobile = mobileReq.Mobile;
                    if (basic != null)
                        sunmmary.Name = basic != null ? basic.Real_name : "";
                    sunmmary.OneMonthCallRecordAmount = entity.OneMonthCallRecordAmount.Value;
                    sunmmary.OneMonthCallRecordCount = entity.OneMonthCallRecordCount.Value;
                    sunmmary.ThreeMonthCallRecordAmount = entity.ThreeMonthCallRecordAmount.Value;
                    sunmmary.ThreeMonthCallRecordCount = entity.ThreeMonthCallRecordCount.Value;
                    sunmmary.SixMonthCallRecordAmount = entity.SixMonthCallRecordAmount.Value;
                    sunmmary.SixMonthCallRecordCount = entity.SixMonthCallRecordCount.Value;
                    sunmmary.Token = mobileReq.Token;
                    sunmmary.IsRealNameAuth = (int)entity.IsRealNameAuth;
                    sunmmary.CallCntAvgBJ = (decimal)entity.CALL_CNT_AVG_BJ;
                    sunmmary.CallLenNitRate = (decimal)entity.CALL_LEN_NIT_RATE;
                    sunmmary.CallTimes = (int)entity.CallTimes;
                    sunmmary.MaxPlanAmt = (decimal)entity.MAX_PLAN_AMT;
                    sunmmary.PhoneNbrBjRate = (decimal)entity.PHONE_NBR_BJ_RATE;
                    sunmmary.ZjsjPhnCrgAvg = (decimal)entity.ZJSJ_PHN_CRG_AVG;
                    sunmmary.DAY90_CALLING_TIMES = (int)entity.DAY90_CALLING_TIMES;
                    sunmmary.CALLED_PHONE_CNT = (int)entity.CALLED_PHONE_CNT;
                    sunmmary.LOCAL_CALL_TIME = (decimal)entity.LOCAL_CALL_TIME;
                    sunmmary.DAY180_CALLING_SUBTTL = (decimal)entity.DAY180_CALLING_SUBTTL;
                    sunmmary.DAY90_CALL_TTL_TIME = (decimal)entity.DAY90_CALL_TTL_TIME;
                    sunmmary.DAY90_CALL_TIMES = (int)entity.DAY90_CALL_TIMES;
                    sunmmary.DAY90_CALLING_TTL_TIME = (decimal)entity.DAY90_CALLING_TTL_TIME;
                    sunmmary.NET_LSTM6_ONL_FLOW = (decimal)entity.NET_LSTM6_ONL_FLOW;
                    sunmmary.DAY_CALLING_TTL_TIME = (decimal)entity.DAY_CALLING_TTL_TIME;
                    sunmmary.CALLED_TIMES = (int)entity.CALLED_TIMES;
                    sunmmary.CALLED_TTL_TIME = (decimal)entity.CALLED_TTL_TIME;
                    sunmmary.MRNG_CALLED_TIMES = (int)entity.MRNG_CALLED_TIMES;
                    sunmmary.CALL_TTL_TIME = (decimal)entity.CALL_TTL_TIME;
                    sunmmary.NIGHT_CALLED_TTL_TIME = (decimal)entity.NIGHT_CALLED_TTL_TIME;
                    sunmmary.AFTN_CALL_TTL_TIME = (decimal)entity.AFTN_CALL_TTL_TIME;
                    sunmmary.AFTN_CALLING_TTL_TIME = (decimal)entity.AFTN_CALLING_TTL_TIME;
                    sunmmary.NIGHT_CALL_TTL_TIME = (decimal)entity.NIGHT_CALL_TTL_TIME;
                    sunmmary.NIGHT_CALLING_TTL_TIME = (decimal)entity.NIGHT_CALLING_TTL_TIME;
                    sunmmary.CALLING_TTL_TIME = (decimal)entity.CALLING_TTL_TIME;
                    sunmmary.PH_USE_MONS = (decimal)entity.PH_USE_MONS;
                    sunmmary.CALL_PHONE_CNT = (int)entity.CALL_PHONE_CNT;
                    sunmmary.CTT_DAYS_CNT = (int)entity.CTT_DAYS_CNT;
                    sunmmary.CALLED_CTT_DAYS_CNT = (int)entity.CALLED_CTT_DAYS_CNT;
                    sunmmary.CALLING_CTT_DAYS_CNT = (int)entity.CALLING_CTT_DAYS_CNT;
                    sunmmary.CALLED_TIMES_IN30DAY = (int)entity.CALLED_TIMES_IN30DAY;
                    sunmmary.CALLED_TIMES_IN15DAY = (int)entity.CALLED_TIMES_IN15DAY;

                    sunmmary.CreateTime = entity.CreateTime;
                }
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("聚信立手机验证短信统计资料异常", e);
            }
            return sunmmary;

        }

        public Variable_mobile_summaryEntity MobileVariableSummary(MobileReq mobileReq)
        {
            throw new NotImplementedException();
        }

        public Variable_mobile_summaryEntity MobileVariableSummary(string source)
        {
            throw new NotImplementedException();
        }

        public BaseRes MobileCatName(string mobile)
        {
            throw new NotImplementedException();
        }

        public List<Call> MobileCall(MobileReq mobileReq)
        {
            if (string.IsNullOrEmpty(mobileReq.Token) && (string.IsNullOrEmpty(mobileReq.IdentityCard) || string.IsNullOrEmpty(mobileReq.Mobile))) return null;
            List<Call> calllist = null;
            try
            {
                ICalls callservice = NetSpiderFactoryManager.GetCallsService();
                IBasic basicService = NetSpiderFactoryManager.GetBasicService();
                BasicEntity basic = basicService.GetByIdentityNoAndMobile(mobileReq.IdentityCard, mobileReq.Mobile);
                if (basic != null)
                {
                    List<CallsEntity> Calls = callservice.GetCallListByOprid(basic.Oper_id.Value).ToList();
                    if (Calls.Count > 0)
                    {
                        calllist = new List<Call>();
                    }
                    foreach (var item in Calls)
                    {
                        Call call = new Call();
                        call.CallPlace = item.Place;
                        call.CallType = item.Call_type;
                        call.InitType = item.Init_type;
                        call.StartTime = item.Start_time;
                        if (item.Subtotal.HasValue)
                            call.SubTotal = Convert.ToDecimal(item.Subtotal.Value);
                        call.UseTime = item.Use_time;
                        call.OtherCallPhone = item.Other_cell_phone;
                        calllist.Add(call);

                    }
                }

            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("聚信立手机验证短信统计资料异常", e);
            }
            return calllist;
        }

        public Basic MobileQuery(MobileReq mobileReq)
        {
            if (string.IsNullOrEmpty(mobileReq.Token) && (string.IsNullOrEmpty(mobileReq.IdentityCard) || string.IsNullOrEmpty(mobileReq.Mobile))) return null;
            Basic basic = null;
            try
            {
                IBasic basicService = NetSpiderFactoryManager.GetBasicService();
                var entity = basicService.GetByIdentityNoAndMobile(mobileReq.IdentityCard, mobileReq.Mobile);
                if (entity != null)
                {
                    basic = new Basic()
                    {
                        Token = mobileReq.Token,
                        Mobile = entity.Cell_phone,
                        Name = entity.Real_name,
                        Regdate = entity.Reg_time,
                        Idcard = entity.Idcard
                    };
                }
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("聚信立手机验证短信统计资料异常", e);
            }
            return basic;

        }

        public BaseRes GetCrawlerState(MobileReq mobileReq)
        {
            BaseRes Res = new BaseRes();
            try
            {
                IOperationLog oprService = NetSpiderFactoryManager.GetOperationLogService();
                var oprLog = oprService.GetByIdentityNoAndMobile(mobileReq.IdentityCard, mobileReq.Mobile);
                if (oprLog != null)
                {
                    if (oprLog.ReceiveFilePath != null)
                        Res.Result = "{\"Type\":\"jxlmobile\",\"Code\":\"" + ServiceConsts.CrawlerStatusCode_AnalysisSuccess + "\",\"Description\":\"解析成功\"}";
                    else if (oprLog.ReceiveFailCount >= 10)
                        Res.Result = "{\"Type\":\"jxlmobile\",\"Code\":\"" + ServiceConsts.CrawlerStatusCode_AnalysisFail + "\",\"Description\":\"解析失败\"}";
                    else
                        Res.Result = "{\"Type\":\"jxlmobile\",\"Code\":\"" + ServiceConsts.CrawlerStatusCode_Crawlering + "\",\"Description\":\"" + ServiceConsts.Crawlering + "\"}";
                }
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("聚信立手机接口异常", e);
            }
            return Res;
        }

        public List<Spd_applyEntity> GetCollectRecords(Dictionary<string, string> dic)
        {
            throw new NotImplementedException();
        }

        #region 根据sourceid获取

        public List<Call> MobileCall(string source)
        {
            if (source.IsEmpty()) return null;
            List<Call> calllist = null;
            try
            {
                ICalls callservice = NetSpiderFactoryManager.GetCallsService();
                List<CallsEntity> Calls = callservice.GetCallListByOprid(source.Split('_')[1].ToInt().Value).ToList();
                if (Calls.Count > 0)
                {
                    calllist = new List<Call>();
                }
                foreach (var item in Calls)
                {
                    Call call = new Call();
                    call.CallPlace = item.Place;
                    call.CallType = item.Call_type;
                    call.InitType = item.Init_type;
                    call.StartTime = item.Start_time;
                    if (item.Subtotal.HasValue)
                        call.SubTotal = Convert.ToDecimal(item.Subtotal.Value);
                    call.UseTime = item.Use_time;
                    call.OtherCallPhone = item.Other_cell_phone;
                    calllist.Add(call);
                }
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("聚信立手机验证短信统计资料异常", e);
            }
            return calllist;
        }

        public Basic MobileQuery(string source)
        {
            if (source.IsEmpty()) return null;
            Basic basic = null;
            try
            {
                IBasic basicService = NetSpiderFactoryManager.GetBasicService();
                var entity = basicService.GetByOperid(source.Split('_')[1].ToInt().Value);
                if (entity != null)
                {
                    basic = new Basic()
                    {
                        Mobile = entity.Cell_phone,
                        Name = entity.Real_name,
                        Regdate = entity.Reg_time,
                        Idcard = entity.Idcard
                    };
                }
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("聚信立手机验证短信统计资料异常", e);
            }
            return basic;

        }

        public BaseRes GetCrawlerState(string source)
        {
            BaseRes Res = new BaseRes();
            try
            {
                var sources = source.Split('_');
                IOperationLog oprService = NetSpiderFactoryManager.GetOperationLogService();
                var oprLog = oprService.Get(sources[1].ToInt());
                if (oprLog != null)
                {
                    if (oprLog.ReceiveFilePath != null)
                        Res.Result = "{\"Type\":\"jxlmobile\",\"Code\":\"" + ServiceConsts.CrawlerStatusCode_AnalysisSuccess + "\",\"Description\":\"采集成功\"}";
                    else if (oprLog.ReceiveFailCount >= 10)
                        Res.Result = "{\"Type\":\"jxlmobile\",\"Code\":\"" + ServiceConsts.CrawlerStatusCode_AnalysisFail + "\",\"Description\":\"采集失败\"}";
                    else
                        Res.Result = "{\"Type\":\"jxlmobile\",\"Code\":\"" + ServiceConsts.CrawlerStatusCode_Crawlering + "\",\"Description\":\"采集中\"}";
                }
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("聚信立手机接口异常", e);
            }
            return Res;
        }

        #endregion

        #endregion

        #region 重置密码

        public VerCodeRes ResetInit(MobileReq mobileReq)
        {
            VerCodeRes Res = new VerCodeRes();
            if (string.IsNullOrEmpty(mobileReq.IdentityCard) || string.IsNullOrEmpty(mobileReq.Name) || string.IsNullOrEmpty(mobileReq.Mobile))
            {
                Res.StatusDescription = ServiceConsts.IdentityCardOrNameOrMobileEmpty;
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                return Res;
            }
            IResetMobilePassWord crawler = Crawler.CrawlerManager.GetResetMobilePWD(EnumMobileCompany.JuXinLi, "jxl");
            Res = crawler.ResetInit(mobileReq);

            return Res;
        }

        public VerCodeRes ResetSendSms(MobileReq mobileReq)
        {
            VerCodeRes Res = new VerCodeRes();
            if (string.IsNullOrEmpty(mobileReq.Token) || string.IsNullOrEmpty(mobileReq.IdentityCard) || string.IsNullOrEmpty(mobileReq.Name) || string.IsNullOrEmpty(mobileReq.Mobile) || string.IsNullOrEmpty(mobileReq.Password))
            {
                Res.StatusDescription = ServiceConsts.TokenOrIdentityCardOrNameOrMobileOrPwdEmpty;
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                return Res;
            }
            IResetMobilePassWord crawler = Crawler.CrawlerManager.GetResetMobilePWD(EnumMobileCompany.JuXinLi, "jxl");
            Res = crawler.ResetSendSms(mobileReq);

            return Res;
        }

        public BaseRes ResetPassWord(MobileReq mobileReq)
        {
            BaseRes Res = new BaseRes();
            if (string.IsNullOrEmpty(mobileReq.Token) || string.IsNullOrEmpty(mobileReq.IdentityCard) || string.IsNullOrEmpty(mobileReq.Name) || string.IsNullOrEmpty(mobileReq.Mobile) || string.IsNullOrEmpty(mobileReq.Password))
            {
                Res.StatusDescription = ServiceConsts.TokenOrIdentityCardOrNameOrMobileOrPwdEmpty;
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                return Res;
            }
            IResetMobilePassWord crawler = Crawler.CrawlerManager.GetResetMobilePWD(EnumMobileCompany.JuXinLi, "jxl");
            Res = crawler.ResetPassWord(mobileReq);

            return Res;
        }

        #endregion

        #region 私有方法

        private IMobileCrawler CreateMobileCrawlerByWebsite(string region)
        {

            IMobileCrawler crawler = Crawler.CrawlerManager.GetMobileCrawler(EnumMobileCompany.JuXinLi, region);
            return crawler;
        }

        private OperationLogEntity SaveOperationLog(MobileReq mobileReq)
        {
            IOperationLog oprService = NetSpiderFactoryManager.GetOperationLogService();
            OperationLogEntity operationLog = new OperationLogEntity();

            operationLog.BusId = mobileReq.busId ?? String.Empty;
            operationLog.BusType = mobileReq.BusType ?? String.Empty;
            operationLog.Name = mobileReq.Name;
            operationLog.IdentityNo = mobileReq.IdentityCard;
            operationLog.Mobile = mobileReq.Mobile;
            operationLog.Status = 1;
            operationLog.SendTime = DateTime.Now;
            operationLog.Source = ServiceConsts.OperationLog_Source_jxl;
            operationLog.Id = oprService.Save(operationLog).ToString().ToInt().Value;

            return operationLog;
        }

        private void SaveAppLog(MobileReq mobileReq, BaseRes Res, int crawlerCode)
        {
            ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
            Spd_applyEntity applyEntity = applyService.GetByToken(mobileReq.Token);
            if (applyEntity != null)
            {
                applyEntity.Name = mobileReq.Name;
                applyEntity.Identitycard = mobileReq.IdentityCard;
                applyEntity.Apply_status = Res.StatusCode;
                applyEntity.Crawl_status = crawlerCode;
                applyEntity.Description = Res.StatusDescription;
                applyService.Update(applyEntity);
            }
            else
            {
                applyEntity = new Spd_applyEntity();
                applyEntity.Identitycard = mobileReq.IdentityCard;
                applyEntity.IPAddr = mobileReq.IPAddr;
                applyEntity.Mobile = mobileReq.Mobile;
                applyEntity.Name = mobileReq.Name;
                applyEntity.Website = mobileReq.Website;
                applyEntity.Spider_type = ServiceConsts.SpiderType_JxlMobile;
                applyEntity.AppId = mobileReq.BusType ?? String.Empty;
                applyEntity.Token = mobileReq.Token;
                applyEntity.Spd_applyformList.Add(new Spd_applyformEntity() { Form_name = "Password", Form_value = mobileReq.Password });
                applyEntity.Apply_status = Res.StatusCode;
                applyEntity.Crawl_status = crawlerCode;
                applyEntity.Description = Res.StatusDescription;
                applyService.Save(applyEntity);
            }
        }

        /// <summary>
        /// 获取采集网站
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <returns></returns>
        private string GetWebsite(string mobile)
        {
            HttpHelper httpHelper = new HttpHelper();
            string Url = "http://tcc.taobao.com/cc/json/mobile_tel_segment.htm?tel=";
            string enumCom = string.Empty;
            string catname = string.Empty;
            string region = string.Empty;

            HttpItem httpItem = new HttpItem()
            {
                URL = Url + mobile
            };
            HttpResult httpResult = httpHelper.GetHtml(httpItem);
            catname = CommonFun.GetMidStrByRegex(httpResult.Html, "catName:'", "'");
            switch (catname)
            {
                case "中国联通": region = ""; enumCom = "chinaunicom"; break;
                case "中国移动": region = CommonFun.GetProvinceCode(CommonFun.GetMidStrByRegex(httpResult.Html, "province:'", "'")); enumCom = "chinamobile"; break;
                case "中国电信": region = CommonFun.GetProvinceCode(CommonFun.GetMidStrByRegex(httpResult.Html, "province:'", "'")); enumCom = "chinatelecom"; break;
                default: break;
            }
            return enumCom + region.ToLower();
        }

        #endregion



    }
}
