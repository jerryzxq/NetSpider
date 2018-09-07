using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading;
using System.Web;
//using System.Web.Providers.Entities;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.WorkFlow;
using Vcredit.NetSpider.Parser;
using Vcredit.NetSpider.Processor;
using Vcredit.NetSpider.RestService.Contracts;
using Vcredit.NetSpider.WorkFlow;
using Vcredit.Common.Ext;
using System.Data;
using Vcredit.Common;
using Vcredit.NetSpider.RestService.Operation;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.Entity.Service.Mobile;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.Common.Helper;
using System.Threading.Tasks;
using Vcredit.Common.Constants;
using Vcredit.NetSpider.Entity.Mongo.Log;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.RestService.Services
{
    //Asp.net管道兼容
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class MobileService : IMobileService
    {
        #region 声明变量、接口
        IPbccrcExecutor pbccrcExecutor = ExecutorManager.GetPbccrcExecutor();
        IProvidentFundExecutor profundExecutor = ExecutorManager.GetProvidentFundExecutor();
        //ISociaSecurityExecutor socialsecExecutor = ExecutorManager.GetSociaSecurityExecutor();

        IMobileExecutor mobileExecutor = ExecutorManager.GetMobileExecutor();
        IMobileExecutor jxlmobileExecutor = ExecutorManager.GetJxlMobileExecutor();

        ITaobaoExecutor taobaoExecutor = ExecutorManager.GetTaobaoExecutor();
        IExecutor Executor = ExecutorManager.GetExecutor();
        IVcreditCertifyExecutor vcertExecutor = ExecutorManager.GetVcreditCertifyExecutor();
        IPluginSecurityCode secService = PluginServiceManager.GetSecurityCodeParserPlugin();
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();

        IRuntimeService runService = ProcessEngine.GetRuntimeService();
        IVerCodeParserService vercodeService = ParserServiceManager.GetVerCodeParserService();
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        #endregion

        #region 手机账单采集
        public VerCodeRes MobilInitForXml(string mobileNo)
        {
            return MobilInit(mobileNo);
        }

        public VerCodeRes MobilInitForJson(string mobileNo)
        {
            return MobilInit(mobileNo);
        }
        public VerCodeRes MobilInit(string mobileNo)
        {
            Log4netAdapter.WriteInfo("接口名：初始化:" + CommonFun.GetClientIP() + ",参数：" + jsonService.SerializeObject(mobileNo, true));
            string website = GetWebsite(mobileNo);
            VerCodeRes res = new VerCodeRes();
            if (!IeExistMobileWebsite(website))
            {
                res.StatusCode = 1;
                res.StatusDescription = "";
            }
            else
            {
                //MobileReq mobilereq = new MobileReq()
                //{
                //    Mobile = mobileNo,
                //    Website = website
                //};
                res = mobileExecutor.MobileInit(mobileNo);
            }

            return res;
        }

        public VerCodeRes MobilInitJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            MobileReq Req = jsonService.DeserializeObject<MobileReq>(reqText);
            return MobilInit(Req);
        }

        public VerCodeRes MobilInit(MobileReq mobileNo)
        {
            Log4netAdapter.WriteInfo("接口名:MobilInit;IP:" + CommonFun.GetClientIP() + ",参数：" + jsonService.SerializeObject(mobileNo, true));

            string website = GetWebsite(mobileNo.Mobile);
            VerCodeRes res = null;
            mobileNo.Website = website;
            res = IeExistMobileWebsite(website) ? mobileExecutor.MobileInit(mobileNo) : jxlmobileExecutor.MobileInit(mobileNo);
            return res;
        }

        public BaseRes MobileLoginForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            MobileReq Req = reqText.DeserializeXML<MobileReq>();

            return MobileLogin(Req);
        }

        public BaseRes MobileLoginForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);

            MobileReq Req = jsonService.DeserializeObject<MobileReq>(reqText);

            return MobileLogin(Req);
        }

        public BaseRes MobileLogin(MobileReq mobileReq)
        {
            Log4netAdapter.WriteInfo("接口名：MobileLogin；IP:" + CommonFun.GetClientIP() + ",参数：" + jsonService.SerializeObject(mobileReq, true));
            BaseRes baseRes = new BaseRes();
            string key = mobileReq.Token + ":" + mobileReq.Password;
            try
            {
                string token = mobileReq.Token;
                mobileReq.IPAddr = CommonFun.GetClientIP();
                if (IsRepeatSubmit(key, "login"))
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "登录中，请稍后再试";
                }
                else
                {
                    baseRes = IeExistMobileWebsite(mobileReq.Website) ? mobileExecutor.MobileLogin(mobileReq) : jxlmobileExecutor.MobileLogin(mobileReq);
                }
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("", e);
                baseRes.StatusDescription = e.Message;
            }
            CacheHelper.RemoveCache(key);
            Log4netAdapter.WriteInfo("身份证号：" + mobileReq.IdentityCard + "，输出结果：" + jsonService.SerializeObject(baseRes, true));
            return baseRes;
        }

        public VerCodeRes MobileSendSmsForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            MobileReq Req = reqText.DeserializeXML<MobileReq>();

            return MobileSendSms(Req);
        }

        public VerCodeRes MobileSendSmsForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);

            MobileReq Req = jsonService.DeserializeObject<MobileReq>(reqText);

            return MobileSendSms(Req);
        }
        public VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            Log4netAdapter.WriteInfo("接口名：MobileSendSms；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(mobileReq, true));
            VerCodeRes baseRes = new VerCodeRes();
            string key = mobileReq.Token + ":sendsms";
            try
            {
                string token = mobileReq.Token;
                if (IsRepeatSubmit(key, "sendsms"))
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "短信发送中，请稍后再试";
                }
                else
                {
                    baseRes = IeExistMobileWebsite(mobileReq.Website) ? mobileExecutor.MobileSendSms(mobileReq) : jxlmobileExecutor.MobileSendSms(mobileReq);
                }
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("发送验证码异常：", e);
                baseRes.StatusDescription = e.Message;
            }
            CacheHelper.RemoveCache(key);
            return baseRes;
        }

        public BaseRes MobileCheckSmsForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            MobileReq Req = reqText.DeserializeXML<MobileReq>();

            return MobileCheckSms(Req);
        }

        public BaseRes MobileCheckSmsForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            MobileReq Req = jsonService.DeserializeObject<MobileReq>(reqText);

            return MobileCheckSms(Req);
        }
        public BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            Log4netAdapter.WriteInfo("接口名：MobileCheckSms；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(mobileReq, true));
            BaseRes baseRes = new BaseRes();
            string key = mobileReq.Token + ":" + mobileReq.Smscode;
            try
            {
                string token = mobileReq.Token;
                mobileReq.IPAddr = CommonFun.GetClientIP();
                if (IsRepeatSubmit(key, "checksms"))
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "短信校验中，请稍后再试";
                }
                else
                {
                    baseRes = IeExistMobileWebsite(mobileReq.Website) ? mobileExecutor.MobileCheckSms(mobileReq) : jxlmobileExecutor.MobileCheckSms(mobileReq);
                }
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("校验验证码异常：", e);
                baseRes.StatusDescription = e.Message;
            }

            CacheHelper.RemoveCache(key);
            return baseRes;
        }

        #endregion

        #region 手机账单信息查询
        public BaseRes MobileSummaryQueryFromTokenForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            MobileReq Req = reqText.DeserializeXML<MobileReq>();

            return MobileSummaryQueryFromToken(Req);
        }
        public BaseRes MobileSummaryQueryFromTokenForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            MobileReq Req = jsonService.DeserializeObject<MobileReq>(reqText);

            return MobileSummaryQueryFromToken(Req);
        }
        public BaseRes MobileSummaryQueryFromToken(MobileReq mobileReq)
        {
            Log4netAdapter.WriteInfo("接口名：MobileSummaryQueryFromToken；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(mobileReq, true));
            BaseRes Res = new BaseRes();
            try
            {
                Res.Token = mobileReq.Token;
                //string reslut = String.Empty;
                //reslut = jsonService.SerializeObject(mobileExecutor.MobileSummaryQuery(mobileReq));
                //Res.Result = reslut;
                var reslut = ChangVariableSummary(mobileExecutor.MobileVariableSummary(mobileReq));
                Res.Result = jsonService.SerializeObject(reslut);
                Res.StatusCode = reslut == null ? ServiceConsts.CrawlerStatusCode_Analysising : ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("MobileSummaryQueryFromToken接口异常：", e);
                Res.StatusDescription = e.Message;
            }
            return Res;
        }

        public BaseRes MobileSummaryQueryForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            MobileReq Req = reqText.DeserializeXML<MobileReq>();

            return MobileSummaryQuery(Req);
        }
        public BaseRes MobileSummaryQueryForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            MobileReq Req = jsonService.DeserializeObject<MobileReq>(reqText);

            return MobileSummaryQuery(Req);
        }
        public BaseRes MobileSummaryQuery(MobileReq mobileReq)
        {
            Log4netAdapter.WriteInfo("接口名：MobileSummaryQuery；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(mobileReq, true));
            BaseRes Res = new BaseRes();
            try
            {
                Res.Token = mobileReq.Token;
                //var reslut = mobileExecutor.MobileSummaryQuery(mobileReq);
                //if (reslut != null && Res.Token.IsEmpty()) Res.Token = reslut.Token;
                var reslut = ChangVariableSummary(mobileExecutor.MobileVariableSummary(mobileReq));
                Res.Result = jsonService.SerializeObject(reslut);
                Res.StatusCode = reslut == null ? ServiceConsts.CrawlerStatusCode_Analysising : ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("MobileSummaryQuery接口异常：", e);
                Res.StatusDescription = e.Message;
            }
            return Res;
        }

        public BaseRes MobileSummaryOldQueryFromTokenForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            MobileReq Req = jsonService.DeserializeObject<MobileReq>(reqText);

            return MobileSummaryOldQueryFromToken(Req);
        }
        public BaseRes MobileSummaryOldQueryFromToken(MobileReq mobileReq)
        {
            Log4netAdapter.WriteInfo("接口名：MobileSummaryQuery；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(mobileReq, true));
            BaseRes Res = new BaseRes();
            try
            {
                Res.Token = mobileReq.Token;
                var reslut = mobileExecutor.MobileSummaryQuery(mobileReq);
                if (reslut != null && Res.Token.IsEmpty()) Res.Token = reslut.Token;
                //var reslut = ChangVariableSummary(mobileExecutor.MobileVariableSummary(mobileReq));
                Res.Result = jsonService.SerializeObject(reslut);
                Res.StatusCode = reslut == null ? ServiceConsts.CrawlerStatusCode_Analysising : ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("MobileSummaryQuery接口异常：", e);
                Res.StatusDescription = e.Message;
            }
            return Res;
        }

        public BaseRes MobileQueryForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            MobileReq Req = reqText.DeserializeXML<MobileReq>();

            return MobileQuery(Req);
        }
        public BaseRes MobileQueryForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            MobileReq Req = jsonService.DeserializeObject<MobileReq>(reqText);

            return MobileQuery(Req);
        }
        public BaseRes MobileQuery(MobileReq mobileReq)
        {
            Log4netAdapter.WriteInfo("接口名：MobileQuery；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数Query：" + jsonService.SerializeObject(mobileReq, true));
            BaseRes Res = new BaseRes();
            try
            {
                Res.Token = mobileReq.Token;
                var reslut = mobileExecutor.MobileQuery(mobileReq);
                if (reslut != null && Res.Token.IsEmpty()) Res.Token = reslut.Token;
                Res.Result = jsonService.SerializeObject(reslut);
                Res.StatusCode = reslut == null ? ServiceConsts.CrawlerStatusCode_Analysising : ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("MobileQuery接口异常：", e);
                Res.StatusDescription = e.Message;
            }
            return Res;
        }

        public BaseRes MobileCallForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            MobileReq Req = jsonService.DeserializeObject<MobileReq>(reqText);
            return MobileCall(Req);
        }
        public BaseRes MobileCall(MobileReq mobileReq)
        {
            Log4netAdapter.WriteInfo("接口名：MobileCall；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数Call：" + jsonService.SerializeObject(mobileReq, true));
            BaseRes Res = new BaseRes();
            try
            {
                Res.Token = mobileReq.Token;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonService.SerializeObject(mobileExecutor.MobileCall(mobileReq));
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("mobileReq接口异常：", e);
                Res.StatusDescription = e.Message;
            }
            return Res;
        }

        public BaseRes MobileCatNameForXml(string mobileNo)
        {
            return MobileCatName(mobileNo);
        }
        public BaseRes MobileCatNameForJson(string mobileNo)
        {
            return MobileCatName(mobileNo);
        }
        public BaseRes MobileCatName(string mobileNo)
        {
            return mobileExecutor.MobileCatName(mobileNo);
        }

        public bool IeExistMobileWebsite(string website)
        {
            // return true;
            if (website == null) return false;
            var mobilePix = website.Split('_');
            if (mobilePix[0] == "ChinaUnicom") website = mobilePix[0];
            ConfigMongo mongo = new ConfigMongo();
            MobileSeting seting = mongo.GetMobileSeting(website);
            if (seting == null) return false;
            return seting.IsUse == 1;
        }


        #region 根据sourceid获取

        public BaseRes MobileVariableSummaryQueryForXml(Stream stream)
        {
            string reqText = stream.AsStringText(true);
            Dictionary<string, object> Req = reqText.DeserializeXML<Dictionary<string, object>>();

            return MobileVariableSummaryQuery(Req);
        }
        public BaseRes MobileVariableSummaryQueryForJson(Stream stream)
        {
            string reqText = stream.AsStringText(true);
            Dictionary<string, object> Req = jsonService.DeserializeObject<Dictionary<string, object>>(reqText);

            return MobileVariableSummaryQuery(Req);
        }
        public BaseRes MobileVariableSummaryQuery(Dictionary<string, object> dic)
        {
            Log4netAdapter.WriteInfo("接口名：MobileVariableSummaryQuery；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + dic);
            BaseRes Res = new BaseRes();
            try
            {
                var reslut = mobileExecutor.MobileVariableSummary(dic["id"].ToString());
                Res.Result = jsonService.SerializeObject(reslut);
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("MobileVariableSummaryQuery接口异常：", e);
                Res.StatusDescription = e.Message;
            }
            return Res;
        }

        public BaseRes MobileCrawlerStateQueryForXml(Stream stream)
        {
            string reqText = stream.AsStringText(true);
            Dictionary<string, object> Req = reqText.DeserializeXML<Dictionary<string, object>>();

            return MobileCrawlerStateQuery(Req);
        }
        public BaseRes MobileCrawlerStateQueryForJson(Stream stream)
        {
            string reqText = stream.AsStringText(true);
            Dictionary<string, object> Req = jsonService.DeserializeObject<Dictionary<string, object>>(reqText);

            return MobileCrawlerStateQuery(Req);
        }
        public BaseRes MobileCrawlerStateQuery(Dictionary<string, object> dic)
        {
            Log4netAdapter.WriteInfo("接口名：MobileCrawlerStateQuery；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + dic);
            BaseRes Res = new BaseRes();
            try
            {
                Res = mobileExecutor.GetCrawlerState(dic["id"].ToString());
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("MobileCrawlerStateQuery接口异常：", e);
                Res.StatusDescription = e.Message;
            }
            return Res;
        }

        public BaseRes MobileQueryByIdForXml(Stream stream)
        {
            string reqText = stream.AsStringText(true);
            Dictionary<string, object> Req = reqText.DeserializeXML<Dictionary<string, object>>();

            return MobileQueryById(Req);
        }
        public BaseRes MobileQueryByIdForJson(Stream stream)
        {
            string reqText = stream.AsStringText(true);
            Dictionary<string, object> Req = jsonService.DeserializeObject<Dictionary<string, object>>(reqText);

            return MobileQueryById(Req);
        }
        public BaseRes MobileQueryById(Dictionary<string, object> dic)
        {
            Log4netAdapter.WriteInfo("接口名：MobileQueryById；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数Query：" + dic);
            BaseRes Res = new BaseRes();
            try
            {
                var reslut = mobileExecutor.MobileQuery(dic["id"].ToString());
                Res.Result = jsonService.SerializeObject(reslut);
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("MobileQueryById接口异常：", e);
                Res.StatusDescription = e.Message;
            }
            return Res;
        }

        public BaseRes MobileCallByIdForXml(Stream stream)
        {
            string reqText = stream.AsStringText(true);
            Dictionary<string, object> Req = reqText.DeserializeXML<Dictionary<string, object>>();

            return MobileCallById(Req);
        }
        public BaseRes MobileCallByIdForJson(Stream stream)
        {
            string reqText = stream.AsStringText(true);
            Dictionary<string, object> Req = jsonService.DeserializeObject<Dictionary<string, object>>(reqText);

            return MobileCallById(Req);
        }
        public BaseRes MobileCallById(Dictionary<string, object> dic)
        {
            Log4netAdapter.WriteInfo("接口名：MobileCallById；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数Call：" + dic);
            BaseRes Res = new BaseRes();
            try
            {
                var reslut = mobileExecutor.MobileCall(dic["id"].ToString());
                Res.Result = jsonService.SerializeObject(reslut);
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("MobileCallById接口异常：", e);
                Res.StatusDescription = e.Message;
            }
            return Res;
        }

        #endregion

        #endregion

        #region 手机信息设置
        public BaseRes MobileSetingSave(Stream stream)
        {
            bool isbase64 = true;
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            Log4netAdapter.WriteInfo("接口：MobileSetingSave；客户端IP:" + CommonFun.GetClientIP());
            BaseRes baseRes = new BaseRes();
            try
            {
                MobileSeting seting = jsonService.DeserializeObject<MobileSeting>(reqText);
                ConfigMongo mongo = new ConfigMongo();
                mongo.SaveMobileSeting(seting);
                SetingLogMongo setingLog = new SetingLogMongo();
                setingLog.SaveSetingLog(new SetingLog()
                {
                    Website = seting.Website,
                    WebsiteName = seting.WebsiteName,
                    IP = CommonFun.GetClientIP(),
                    IsUse = 1,
                    Description = "上线"
                });
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "信息保存完毕";
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = "信息保存出错";
                Log4netAdapter.WriteError("信息保存出错", e);
            }
            baseRes.EndTime = DateTime.Now.ToString();
            return baseRes;
        }
        public BaseRes MobileSetingUpdate(Stream stream)
        {
            bool isbase64 = true;
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            Log4netAdapter.WriteInfo("接口：MobileSetingUpdate；客户端IP:" + CommonFun.GetClientIP() + ";参数：" + reqText);
            BaseRes baseRes = new BaseRes();
            try
            {
                MobileSeting seting = jsonService.DeserializeObject<MobileSeting>(reqText);

                ConfigMongo mongo = new ConfigMongo();
                mongo.UpdateMobileSeting(seting);
                SetingLogMongo setingLog = new SetingLogMongo();
                setingLog.SaveSetingLog(new SetingLog()
                {
                    Website = seting.Website,
                    WebsiteName = seting.WebsiteName,
                    IP = CommonFun.GetClientIP(),
                    IsUse = seting.IsUse,
                    Description = seting.IsUse == 0 ? seting.Description : "上线"
                });
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "信息更新完毕";
                var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    try
                    {
                        StringBuilder body = new StringBuilder();
                        body.Append("<html>");
                        body.Append("<head><meta charset=\"UTF-8\"><style>");
                        body.Append(" .table { width: 100%; margin: 0 auto; }");
                        body.Append(".table td, .table th {border-right: 1px solid #ccc; border-bottom: 1px solid #ccc; text-align: center; }");
                        body.Append(".table th {background-color: #f5f5f5;}");
                        body.Append(".table th:first-child {border-left: 1px solid #ccc;}");
                        body.Append(".table td:first-child {border-left: 1px solid #ccc;}");
                        body.Append(".table tr:first-child th {border-top: 1px solid #ccc;}");
                        body.Append("</style></head>");
                        body.Append("<body>");
                        body.Append("<div style=\"width: 80%;margin: 0 auto;\">");
                        body.Append("<table class=\"table\" cellspacing=\"0\" cellpadding=\"10\">");
                        body.Append("<thead><tr><th colspan=\"2\">数据源状态变更通知</th></tr></thead>");
                        body.Append("<tbody>");
                        body.Append("<tr><th>数据源名称</th><td>");
                        body.Append(seting.WebsiteName);
                        body.Append("</td></tr>");
                        body.Append(" <tr><th>变更状态</th><td>");
                        body.Append(seting.IsUse == 1 ? "上线" : "下线");
                        body.Append("</td></tr>");
                        body.Append("<tr><th>变更时间</th><td>");
                        body.Append(DateTime.Now.ToString(Consts.DateFormatString11));
                        body.Append("</td></tr>");
                        if (seting.IsUse == 0)
                        {
                            body.Append("<tr><th>下线原因</th><td>");
                            body.Append(seting.Description);
                            body.Append("</td></tr>");
                        }
                        body.Append("</tbody>");
                        body.Append("</table></div></body></html>");
                        string mes = CommonFun.SendMail("数据源在线状态提醒", body.ToString());
                        if (mes != "ok")
                            Log4netAdapter.WriteInfo("数据源在线状态提醒邮件失败：" + mes);
                    }
                    catch (Exception e)
                    {
                        Log4netAdapter.WriteError("数据源在线状态提醒邮件异常：", e);
                    }
                });
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted || task.Status == TaskStatus.Canceled)
                    task.Dispose();
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = "信息更新出错";
                Log4netAdapter.WriteError("信息更新出错", e);
            }
            baseRes.EndTime = DateTime.Now.ToString();
            return baseRes;
        }

        public BaseRes GetMobileSeting(Stream stream)
        {
            bool isbase64 = true;
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            Log4netAdapter.WriteInfo("接口：GetMobileSeting；客户端IP:" + CommonFun.GetClientIP() + ";参数：" + reqText);
            BaseRes Res = new BaseRes();
            try
            {
                Dictionary<string, string> seting = new Dictionary<string, string>();
                if (!reqText.IsEmpty())
                    seting = jsonService.DeserializeObject<Dictionary<string, string>>(reqText);
                ConfigMongo mongo = new ConfigMongo();
                Res.Result = jsonService.SerializeObject(mongo.GetMobileSeting(seting));
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "信息获取出错";
                Log4netAdapter.WriteError("信息获取出错", e);
            }
            Res.EndTime = DateTime.Now.ToString();
            return Res;
        }

        public BaseRes GetMobileSetingForWebsite(string website)
        {
            Log4netAdapter.WriteInfo("接口：GetMobileSetingForWebsite；客户端IP:" + CommonFun.GetClientIP() + ";参数：" + website);
            BaseRes Res = new BaseRes();
            try
            {
                ConfigMongo mongo = new ConfigMongo();
                Res.Result = jsonService.SerializeObject(mongo.GetMobileSeting(website));
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "信息获取出错";
                Log4netAdapter.WriteError("信息获取出错", e);
            }
            Res.EndTime = DateTime.Now.ToString();
            return Res;
        }

        #endregion

        #region 抓取步骤原始数据
        public BaseRes OriginalHtmlSave(Stream stream)
        {
            bool isbase64 = true;
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            Log4netAdapter.WriteInfo("接口：OriginalHtmlSave；客户端IP:" + CommonFun.GetClientIP());
            BaseRes baseRes = new BaseRes();
            try
            {
                OriginalHtml html = jsonService.DeserializeObject<OriginalHtml>(reqText);
                OriginalHtmlMongo mongo = new OriginalHtmlMongo();
                mongo.Save(html);
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "信息保存完毕";
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = "信息保存出错";
                Log4netAdapter.WriteError("信息保存出错", e);
            }
            baseRes.EndTime = DateTime.Now.ToString();
            return baseRes;
        }

        public BaseRes OriginalHtmlUpdate(Stream stream)
        {
            bool isbase64 = true;
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            Log4netAdapter.WriteInfo("接口：OriginalHtmlUpdate；客户端IP:" + CommonFun.GetClientIP() + ";参数：" + reqText);
            BaseRes baseRes = new BaseRes();
            try
            {
                OriginalHtml html = jsonService.DeserializeObject<OriginalHtml>(reqText);
                OriginalHtmlMongo mongo = new OriginalHtmlMongo();
                mongo.Update(html);
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "信息更新完毕";
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = "信息更新出错";
                Log4netAdapter.WriteError("信息更新出错", e);
            }
            baseRes.EndTime = DateTime.Now.ToString();
            return baseRes;
        }

        #endregion

        #region 重置密码

        public VerCodeRes ResetInitJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            MobileReq Req = jsonService.DeserializeObject<MobileReq>(reqText);
            return ResetInit(Req);
        }

        public VerCodeRes ResetInit(MobileReq mobileReq)
        {
            Log4netAdapter.WriteInfo("接口名：ResetInitJson；IP:" + CommonFun.GetClientIP() + ",参数：" + jsonService.SerializeObject(mobileReq, true));
            VerCodeRes baseRes = new VerCodeRes();
            try
            {
                mobileReq.IPAddr = CommonFun.GetClientIP();
                baseRes = jxlmobileExecutor.ResetInit(mobileReq);

            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("", e);
                baseRes.StatusDescription = e.Message;
            }
            Log4netAdapter.WriteInfo("身份证号：" + mobileReq.IdentityCard + "，输出结果：" + jsonService.SerializeObject(baseRes, true));
            return baseRes;
        }


        public VerCodeRes ResetSendSmsJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            MobileReq Req = jsonService.DeserializeObject<MobileReq>(reqText);
            return ResetSendSms(Req);
        }

        public VerCodeRes ResetSendSms(MobileReq mobileReq)
        {
            Log4netAdapter.WriteInfo("接口名：ResetSendSmsJson；IP:" + CommonFun.GetClientIP() + ",参数：" + jsonService.SerializeObject(mobileReq, true));
            VerCodeRes baseRes = new VerCodeRes();
            string key = mobileReq.Token + ":ResetSendSms";
            try
            {
                string token = mobileReq.Token;
                mobileReq.IPAddr = CommonFun.GetClientIP();
                if (IsRepeatSubmit(key, "ResetSendSmsJson"))
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "短信发送中，请稍后再试";
                }
                else
                {
                    baseRes = jxlmobileExecutor.ResetSendSms(mobileReq);
                }
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("", e);
                baseRes.StatusDescription = e.Message;
            }
            Log4netAdapter.WriteInfo("身份证号：" + mobileReq.IdentityCard + "，输出结果：" + jsonService.SerializeObject(baseRes, true));
            CacheHelper.RemoveCache(key);
            return baseRes;
        }


        public BaseRes ResetPassWordJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            MobileReq Req = jsonService.DeserializeObject<MobileReq>(reqText);
            return ResetPassWord(Req);
        }

        public BaseRes ResetPassWord(MobileReq mobileReq)
        {
            Log4netAdapter.WriteInfo("接口名：ResetPassWordJson；IP:" + CommonFun.GetClientIP() + ",参数：" + jsonService.SerializeObject(mobileReq, true));
            BaseRes baseRes = new BaseRes();
            string key = mobileReq.Token + ":" + mobileReq.Smscode;
            try
            {
                string token = mobileReq.Token;
                if (IsRepeatSubmit(key, "ResetPassWordJson"))
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "密码重置中，请稍后再试";
                }
                else
                {
                    mobileReq.IPAddr = CommonFun.GetClientIP();
                    baseRes = jxlmobileExecutor.ResetPassWord(mobileReq);
                }
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("", e);
                baseRes.StatusDescription = e.Message;
            }
            Log4netAdapter.WriteInfo("身份证号：" + mobileReq.IdentityCard + "，输出结果：" + jsonService.SerializeObject(baseRes, true));
            CacheHelper.RemoveCache(key);
            return baseRes;
        }


        #endregion

        #region 其他

        /// <summary>
        /// 获取采集网站
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <returns></returns>
        public string GetWebsite(string mobile)
        {
            string Url = "http://tcc.taobao.com/cc/json/mobile_tel_segment.htm?tel=";
            EnumMobileCompany enumCom = new EnumMobileCompany();
            string catname = string.Empty;
            string region = string.Empty;

            var httpItem = new HttpItem()
            {
                URL = Url + mobile
            };
            var httpResult = httpHelper.GetHtml(httpItem);
            catname = CommonFun.GetMidStrByRegex(httpResult.Html, "catName:'", "'");
            switch (catname)
            {
                case "中国联通": region = CommonFun.GetProvinceCode(CommonFun.GetMidStrByRegex(httpResult.Html, "province:'", "'")); enumCom = EnumMobileCompany.ChinaUnicom; break;
                case "中国移动": region = CommonFun.GetProvinceCode(CommonFun.GetMidStrByRegex(httpResult.Html, "province:'", "'")); enumCom = EnumMobileCompany.ChinaMobile; break;
                case "中国电信": region = CommonFun.GetProvinceCode(CommonFun.GetMidStrByRegex(httpResult.Html, "province:'", "'")); enumCom = EnumMobileCompany.ChinaNet; break;
                default: break;
            }
            return enumCom + "_" + region;

        }

        /// <summary>
        /// 获取采集状态
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <returns></returns>
        public BaseRes GetCrawlerState(Stream stream)
        {
            string reqText = stream.AsStringText(true);
            MobileReq Req = jsonService.DeserializeObject<MobileReq>(reqText);
            Log4netAdapter.WriteInfo("接口名：GetCrawlerState；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(Req, true));
            BaseRes Res = new BaseRes();
            try
            {
                Res = mobileExecutor.GetCrawlerState(Req);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("GetCrawlerState接口异常：", e);
                Res.StatusDescription = e.Message;
            }
            return Res;
        }

        /// <summary>
        /// 获取采集记录
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public BaseRes GetCollectRecords(Stream stream)
        {
            Log4netAdapter.WriteInfo("接口名：GetCollectRecords；客户端IP:" + CommonFun.GetClientIP());
            string reqText = stream.AsStringText(true);
            Log4netAdapter.WriteInfo("参数：" + stream);
            BaseRes Res = new BaseRes();
            try
            {
                Dictionary<string, string> dic = null;
                if (!reqText.IsEmpty())
                    dic = jsonService.DeserializeObject<Dictionary<string, string>>(reqText);
                Res.Result = jsonService.SerializeObject(mobileExecutor.GetCollectRecords(dic));
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("GetCollectRecords接口异常：", e);
                Res.StatusDescription = e.Message;
            }
            return Res;
        }

        /// <summary>
        /// 获取日志记录
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public BaseRes GetLogSeting(Stream stream)
        {
            bool isbase64 = true;
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            Log4netAdapter.WriteInfo("接口：GetMobileSeting；客户端IP:" + CommonFun.GetClientIP() + ";参数：" + reqText);
            BaseRes Res = new BaseRes();
            try
            {
                Dictionary<string, string> seting = new Dictionary<string, string>();
                if (!reqText.IsEmpty())
                    seting = jsonService.DeserializeObject<Dictionary<string, string>>(reqText);
                SetingLogMongo mongo = new SetingLogMongo();
                Res.Result = jsonService.SerializeObject(mongo.GetLogSeting(seting));
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "信息获取出错";
                Log4netAdapter.WriteError("信息获取出错", e);
            }
            Res.EndTime = DateTime.Now.ToString();
            return Res;
        }

        private object ChangVariableSummary(Variable_mobile_summaryEntity entity)
        {
            if (entity == null) return null;
            var result = new
            {
                Id = entity.SourceType + "_" + entity.SourceId,
                Mobile = entity.Mobile,
                BusName = entity.BusName,
                BusIdentityCard = entity.BusIdentityCard,
                Name = entity.Name,
                IdentityCard = entity.IdentityCard,
                Regdate = entity.Regdate.ToDateTime() != null ? ((DateTime)entity.Regdate.ToDateTime()).ToString(Consts.DateFormatString11) : entity.Regdate,
                City = entity.City,
                IsRealNameAuth = entity.IsRealNameAuth,
                OneMonthCallRecordCount = entity.One_Month_Call_Record_Count,
                OneMonthCallRecordAmount = entity.One_Month_Call_Record_Amount,
                ThreeMonthCallRecordCount = entity.Three_Month_Call_Record_Count,
                ThreeMonthCallRecordAmount = entity.Three_Month_Call_Record_Amount,
                SixMonthCallRecordCount = entity.Six_Month_Call_Record_Count,
                SixMonthCallRecordAmount = entity.Six_Month_Call_Record_Amount,
                CallCntAvgBJ = entity.CALL_CNT_AVG_BJ,
                CallLenNitRate = entity.CALL_LEN_NIT_RATE,
                MaxPlanAmt = entity.MAX_PLAN_AMT,
                PhoneNbrBjRate = entity.PHONE_NBR_BJ_RATE,
                ZjsjPhnCrgAvg = entity.ZJSJ_PHN_CRG_AVG,
                DAY90_CALLING_TIMES = entity.DAY90_CALLING_TIMES,
                CALLED_PHONE_CNT = entity.CALLED_PHONE_CNT,
                LOCAL_CALL_TIME = entity.LOCAL_CALL_TIME,
                DAY180_CALLING_SUBTTL = entity.DAY180_CALLING_SUBTTL,
                DAY90_CALL_TTL_TIME = entity.DAY90_CALL_TTL_TIME,
                DAY90_CALL_TIMES = entity.DAY90_CALL_TIMES,
                DAY90_CALLING_TTL_TIME = entity.DAY90_CALLING_TTL_TIME,
                NET_LSTM6_ONL_FLOW = entity.NET_LSTM6_ONL_FLOW,
                DAY_CALLING_TTL_TIME = entity.DAY_CALLING_TTL_TIME,
                CALLED_TIMES = entity.CALLED_TIMES,
                CALLED_TTL_TIME = entity.CALLED_TTL_TIME,
                MRNG_CALLED_TIMES = entity.MRNG_CALLED_TIMES,
                CALL_TTL_TIME = entity.CALL_TTL_TIME,
                NIGHT_CALLED_TTL_TIME = entity.NIGHT_CALLED_TTL_TIME,
                AFTN_CALL_TTL_TIME = entity.AFTN_CALL_TTL_TIME,
                AFTN_CALLING_TTL_TIME = entity.AFTN_CALLING_TTL_TIME,
                NIGHT_CALL_TTL_TIME = entity.NIGHT_CALL_TTL_TIME,
                NIGHT_CALLING_TTL_TIME = entity.NIGHT_CALLING_TTL_TIME,
                CALLING_TTL_TIME = entity.CALLING_TTL_TIME,
                PH_USE_MONS = entity.PH_USE_MONS,
                CALL_PHONE_CNT = entity.CALL_PHONE_CNT,
                CTT_DAYS_CNT = entity.CTT_DAYS_CNT,
                CALLED_CTT_DAYS_CNT = entity.CALLED_CTT_DAYS_CNT,
                CALLING_CTT_DAYS_CNT = entity.CALLING_CTT_DAYS_CNT,
                CALLED_TIMES_IN30DAY = entity.CALLED_TIMES_IN30DAY,
                CALLED_TIMES_IN15DAY = entity.CALLED_TIMES_IN15DAY,
                CallTimes = entity.CallTimes,
                CALLED_TIMES_IN30DAY_Gray = entity.CALLED_TIMES_IN30DAY_Gray,
                CALLED_TIMES_IN15DAY_Gray = entity.CALLED_TIMES_IN15DAY_Gray,
                OTHER_SMS_PHONE_CNT = entity.OTHER_SMS_PHONE_CNT,
                SMS_PHONE_CNT = entity.SMS_PHONE_CNT,
                ROAMING_CALL_TIMES = entity.ROAMING_CALL_TIMES,
                ALL_CALL_TIMES = entity.ALL_CALL_TIMES,
                AFTN_CALL_TIMES = entity.AFTN_CALL_TIMES,
                CALLING_PHONE_CNT = entity.CALLING_PHONE_CNT,
                DAY3_CHECK_CALL_TIMES = entity.DAY3_CHECK_CALL_TIMES,
                CALLING_SUBTTL = entity.CALLING_SUBTTL,
                CALL_LSTM3_SMY_CALL_MN = entity.CALL_LSTM3_SMY_CALL_MN,
                CALL_LSTM6_CALL_CNT = entity.CALL_LSTM6_CALL_CNT,
                ANS_DAY_CNT = entity.ANS_DAY_CNT,
                CALL_SMY_CALL_NIGHT_CNT = entity.CALL_SMY_CALL_NIGHT_CNT,
                CALL_D180_CALLED_CNT = entity.CALL_D180_CALLED_CNT,
                CALL_SMY_CTT_NIGHT_CNT = entity.CALL_SMY_CTT_NIGHT_CNT,
                CALL_D180_CALLED_TIME = entity.CALL_D180_CALLED_TIME,
                CALL_D180_CALL_TIME = entity.CALL_D180_CALL_TIME,
                D180_NIGHT_CALLED_TIME = entity.D180_NIGHT_CALLED_TIME,
                CALL_SMY_CTT_CNT_TOTAL = entity.CALL_SMY_CTT_CNT_TOTAL
            };
            return result;
        }

        /// <summary>
        /// 判断是否有重复提交
        /// </summary>
        /// <param name="name"></param>
        /// <param name="identityNo"></param>
        /// <param name="mobile"></param>
        /// <returns></returns>
        private bool IsRepeatSubmit(string key, string type)
        {
            if (CacheHelper.GetCache(key) != null)
            {
                return true;
            }
            CacheHelper.SetCache(key, type);
            return false;
        }


        #endregion
    }
}