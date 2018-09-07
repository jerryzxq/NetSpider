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
using Vcredit.NetSpider.Processor;
using Vcredit.WeiXin.RestService.Contracts;
using Vcredit.Common.Ext;
using System.Data;
using Vcredit.Common;
using Vcredit.NetSpider.PluginManager;
using System.Xml.Linq;
using Vcredit.WeiXin.RestService.Models;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Service;
using Cn.Vcredit.VBS.Model;
using Vcredit.NetSpider.Entity;
using Cn.Vcredit.VBS.BusinessLogic;
using Cn.Vcredit.VBS.BusinessLogic.Entity;
using Vcredit.Common.Helper;
using Vcredit.Common.Constants;
using Vcredit.WeiXin.RestService.Operation;
using Cn.Vcredit.VBS.Interface;
using Cn.Vcredit.VBS.BLL;
using System.Drawing;
using Cn.Vcredit.VBS.PostLoan.FinanceConfig.Action;
using Cn.Vcredit.VBS.PostLoan.OrderInfo;
using Vcredit.NetSpider.Entity.Service.Chsi;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.WeiXin.RestService.Models.Res;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.NetSpider.Crawler;
using Vcredit.NetSpider.Crawler.Edu;

namespace Vcredit.WeiXin.RestService.Services
{
    //Asp.net管道兼容
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class JxlService : IJxlService
    {
        #region 声明变量、接口
        IPbccrcExecutor pbccrcExecutor = ExecutorManager.GetPbccrcExecutor();//社保数据采集接口
        IProvidentFundExecutor profundExecutor = ExecutorManager.GetProvidentFundExecutor();//社保数据采集接口
        ISociaSecurityExecutor socialsecExecutor = ExecutorManager.GetSociaSecurityExecutor();//社保数据采集接口
        IChsiExecutor chsiExecutor = ExecutorManager.GetChsiExecutor();

        IPluginSecurityCode secService = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析接口
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
        string ftpDirectory = "/jxlreport";
        bool isbase64 = true;
        string ClientIp = CommonFun.GetClientIP();
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public JxlService()
        {
        }
        #endregion

        #region 聚信立手机账单获取登录成功后回调
        /// <summary>
        /// 聚信立手机账单获取登录成功后回调
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public BaseRes JxlSubmitSuccessForForXml(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                JxlSubmitInfo Req = reqText.DeserializeXML<JxlSubmitInfo>();
                baseRes = JxlSubmitSuccess(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        /// <summary>
        /// 聚信立手机账单获取登录成功后回调
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public BaseRes JxlSubmitSuccessForJson(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                JxlSubmitInfo Req = jsonService.DeserializeObject<JxlSubmitInfo>(reqText);
                baseRes = JxlSubmitSuccess(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        /// <summary>
        /// 聚信立手机账单获取登录成功后回调
        /// </summary>
        /// <param name="submitInfo">submitInfo实体</param>
        /// <returns></returns>
        BaseRes JxlSubmitSuccess(JxlSubmitInfo submitInfo)
        {
            Log4netAdapter.WriteInfo(string.Format("接口：JxlSubmitSuccess；客户端IP:{0},参数：{1}", ClientIp, jsonService.SerializeObject(submitInfo, true)));
            BaseRes baseRes = new BaseRes();
            try
            {
                Cn.Vcredit.ThirdParty.Juxinli.DataProcess process = new Cn.Vcredit.ThirdParty.Juxinli.DataProcess();
                //process.SubmitInfo(submitInfo.name, submitInfo.identitycard, submitInfo.mobile);
                baseRes.Result = "{\"Id\":\"" + process.SubmitInfo(submitInfo.busId, submitInfo.busType, submitInfo.name, submitInfo.identitycard, submitInfo.mobile) + "\"}";
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "提交成功";
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("提交异常", e);
                baseRes.StatusCode = ServiceConstants.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            baseRes.EndTime = DateTime.Now.ToString();
            return baseRes;
        }
        #endregion

        #region 校验是否实名认证
        /// <summary>
        /// 校验是否实名认证
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public BaseRes MobileIsAuthForXml(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                JxlSubmitInfo Req = reqText.DeserializeXML<JxlSubmitInfo>();
                baseRes = MobileIsAuth(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        /// <summary>
        /// 校验是否实名认证
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public BaseRes MobileIsAuthForJson(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                JxlSubmitInfo Req = jsonService.DeserializeObject<JxlSubmitInfo>(reqText);
                baseRes = MobileIsAuth(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }

        public BaseRes MobileIsAuth(JxlSubmitInfo Req)
        {
            Log4netAdapter.WriteInfo(string.Format("接口：MobileIsAuth；客户端IP:{0},参数：{1}", ClientIp, jsonService.SerializeObject(Req, true)));
            BaseRes baseRes = new BaseRes();
            try
            {
                IOperationLog operService = NetSpiderFactoryManager.GetOperationLogService();
                OperationLogEntity entity = operService.GetByIdentityNoAndMobile(Req.identitycard, Req.mobile);
                if (entity == null)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "没有提交聚信立查询";
                    baseRes.Result = "none";//获取报告失败
                    Log4netAdapter.WriteInfo("没有提交聚信立查询");

                    return baseRes;
                }
                else if (entity.Status == 1 || entity.Status == 5)
                {
                    if (entity.ReceiveFailCount != null && entity.ReceiveFailCount == 10)
                    {
                        baseRes.Result = "fail";//获取报告失败
                    }
                    else
                    {
                        baseRes.Result = "wait";//等待报告
                    }
                }
                else
                {
                    ISummary service = NetSpiderFactoryManager.GetSummaryService();
                    //var summaryEntity = service.GetByBusiness(Req.orderid.ToString(), ServiceConstants.BusType_Kakadai);
                    var summaryEntity = service.GetByOperId(entity.Id);
                    if (summaryEntity != null && summaryEntity.IsRealNameAuth != null && summaryEntity.IsRealNameAuth == 1)
                    {
                        baseRes.Result = "auth";//实名认证
                    }
                    else
                    {
                        baseRes.Result = "noauth";//非实名认证
                    }
                }
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "认证完成";
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteInfo("参数:" + jsonService.SerializeObject(Req));
                Log4netAdapter.WriteError("认证异常", e);
                baseRes.StatusCode = ServiceConstants.StatusCode_error;
                baseRes.StatusDescription = "认证异常";
            }
            baseRes.EndTime = DateTime.Now.ToString();
            return baseRes;
        }
        #endregion

        #region 获取手机账单统计信息
        public BaseRes GetMobileSummaryForXml(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                JxlSubmitInfo Req = reqText.DeserializeXML<JxlSubmitInfo>();
                baseRes = GetMobileSummary(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }

        public BaseRes GetMobileSummaryForJson(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                JxlSubmitInfo Req = jsonService.DeserializeObject<JxlSubmitInfo>(reqText);
                baseRes = GetMobileSummary(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        public BaseRes GetMobileSummary(JxlSubmitInfo Req)
        {
            Log4netAdapter.WriteInfo(string.Format("接口：GetMobileSummary；客户端IP:{0},参数：{1}", ClientIp, jsonService.SerializeObject(Req, true)));
            BaseRes baseRes = new BaseRes();
            try
            {
                ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
                ISummary service = NetSpiderFactoryManager.GetSummaryService();
                IBasic basicService = NetSpiderFactoryManager.GetBasicService();
                BasicEntity basic = new BasicEntity();
                Summary summary = null;

                Spd_applyEntity applyEntity = applyService.GetByIdentityCardAndSpiderTypeAndMobile(Req.identitycard, "mobile", Req.mobile);
                SummaryEntity entity = service.GetByIdentityNoAndMobile(Req.identitycard, Req.mobile);
                if (applyEntity == null && entity == null)
                {
                    baseRes.Result = "";
                    baseRes.StatusDescription = "无数据";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                }
                else
                {
                    baseRes.StatusDescription = "查询成功";
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;

                    MobileMongo mibleMongo = null;
                    if (applyEntity != null)
                    {
                        mibleMongo = new MobileMongo(applyEntity.CreateTime);
                    }
                    if (applyEntity != null && applyEntity.Crawl_status == ServiceConsts.CrawlStatus_Mobile_Success && entity != null)
                    {
                        if (applyEntity.CreateTime > entity.CreateTime.ToDateTime())
                        {
                            summary = mibleMongo.GetSummaryByToken(applyEntity.Token);
                            var result = new
                            {
                                Id = "vcredit_" + applyEntity.ApplyId,
                                City = summary.City,
                                IsRealNameAuth = summary.IsRealNameAuth,
                                Mobile = summary.Mobile,
                                OneMonthCallRecordCount = summary.OneMonthCallRecordCount,
                                OneMonthCallRecordAmount = summary.OneMonthCallRecordAmount,
                                ThreeMonthCallRecordCount = summary.ThreeMonthCallRecordCount,
                                ThreeMonthCallRecordAmount = summary.ThreeMonthCallRecordAmount,
                                SixMonthCallRecordCount = summary.SixMonthCallRecordCount,
                                SixMonthCallRecordAmount = summary.SixMonthCallRecordAmount,
                                Regdate = summary.Regdate.ToDateTime() != null ? ((DateTime)summary.Regdate.ToDateTime()).ToString(Consts.DateFormatString11) : summary.Regdate,
                                IsSelf = 1,
                                CallCntAvgBJ = summary.CallCntAvgBJ,
                                CallLenNitRate = summary.CallLenNitRate,
                                CallTimes = summary.CallTimes,
                                MaxPlanAmt = summary.MaxPlanAmt,
                                PhoneNbrBjRate = summary.PhoneNbrBjRate,
                                ZjsjPhnCrgAvg = summary.ZjsjPhnCrgAvg,
                                DAY90_CALLING_TIMES = summary.DAY90_CALLING_TIMES,
                                CALLED_PHONE_CNT = summary.CALLED_PHONE_CNT,
                                LOCAL_CALL_TIME = summary.LOCAL_CALL_TIME,
                                DAY180_CALLING_SUBTTL = summary.DAY180_CALLING_SUBTTL,
                                DAY90_CALL_TTL_TIME = summary.DAY90_CALL_TTL_TIME,
                                DAY90_CALL_TIMES = summary.DAY90_CALL_TIMES,
                                DAY90_CALLING_TTL_TIME = summary.DAY90_CALLING_TTL_TIME,
                                NET_LSTM6_ONL_FLOW = summary.NET_LSTM6_ONL_FLOW,
                                DAY_CALLING_TTL_TIME = summary.DAY_CALLING_TTL_TIME,
                                CALLED_TIMES = summary.CALLED_TIMES,
                                CALLED_TTL_TIME = summary.CALLED_TTL_TIME,
                                MRNG_CALLED_TIMES = summary.MRNG_CALLED_TIMES,
                                CALL_TTL_TIME = summary.CALL_TTL_TIME,
                                NIGHT_CALLED_TTL_TIME = summary.NIGHT_CALLED_TTL_TIME,
                                AFTN_CALL_TTL_TIME = summary.AFTN_CALL_TTL_TIME,
                                AFTN_CALLING_TTL_TIME = summary.AFTN_CALLING_TTL_TIME,
                                NIGHT_CALL_TTL_TIME = summary.NIGHT_CALL_TTL_TIME,
                                NIGHT_CALLING_TTL_TIME = summary.NIGHT_CALLING_TTL_TIME,
                                CALLING_TTL_TIME = summary.CALLING_TTL_TIME,
                                PH_USE_MONS = summary.PH_USE_MONS,
                                CALL_PHONE_CNT = summary.CALL_PHONE_CNT,
                                CTT_DAYS_CNT = summary.CTT_DAYS_CNT,
                                CALLED_CTT_DAYS_CNT = summary.CALLED_CTT_DAYS_CNT,
                                CALLING_CTT_DAYS_CNT = summary.CALLING_CTT_DAYS_CNT,
                                CALLED_TIMES_IN30DAY = summary.CALLED_TIMES_IN30DAY,
                                CALLED_TIMES_IN15DAY = summary.CALLED_TIMES_IN15DAY,
                                CALLED_TIMES_IN30DAY_GRAY = summary.CALLED_TIMES_IN30DAY_FOR_GRAY,
                                CALLED_TIMES_IN15DAY_GRAY = summary.CALLED_TIMES_IN15DAY_FOR_GRAY,
                            };
                            baseRes.Result = jsonService.SerializeObject(result);
                        }
                        else
                        {
                            if (entity.Regdate.IsEmpty())
                            {
                                basic = basicService.GetByIdentityNoAndMobile(Req.identitycard, Req.mobile);
                                entity.Regdate = basic.Reg_time;
                            }
                            //entity.OperationLog = null;
                            var result = new
                            {
                                Id = "jxl_" + entity.Oper_id,
                                City = entity.City,
                                IsRealNameAuth = entity.IsRealNameAuth,
                                Mobile = entity.Mobile,
                                OneMonthCallRecordCount = entity.OneMonthCallRecordCount,
                                OneMonthCallRecordAmount = entity.OneMonthCallRecordAmount,
                                ThreeMonthCallRecordCount = entity.ThreeMonthCallRecordCount,
                                ThreeMonthCallRecordAmount = entity.ThreeMonthCallRecordAmount,
                                SixMonthCallRecordCount = entity.SixMonthCallRecordCount,
                                SixMonthCallRecordAmount = entity.SixMonthCallRecordAmount,
                                Regdate = entity.Regdate,
                                IsSelf = 0,
                                CallCntAvgBJ = entity.CALL_CNT_AVG_BJ,
                                CallLenNitRate = entity.CALL_LEN_NIT_RATE,
                                CallTimes = entity.CallTimes,
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
                                CALLED_TIMES_IN30DAY_GRAY = entity.CALLED_TIMES_IN30DAY_Gray,
                                CALLED_TIMES_IN15DAY_GRAY = entity.CALLED_TIMES_IN15DAY_Gray,

                            };
                            baseRes.Result = jsonService.SerializeObject(result);
                        }
                    }
                    else if (applyEntity != null && applyEntity.Crawl_status == ServiceConsts.CrawlStatus_Mobile_Success)
                    {
                        summary = mibleMongo.GetSummaryByToken(applyEntity.Token);
                        var result = new
                        {
                            Id = "vcredit_" + applyEntity.ApplyId,
                            City = summary.City,
                            IsRealNameAuth = summary.IsRealNameAuth,
                            Mobile = summary.Mobile,
                            OneMonthCallRecordCount = summary.OneMonthCallRecordCount,
                            OneMonthCallRecordAmount = summary.OneMonthCallRecordAmount,
                            ThreeMonthCallRecordCount = summary.ThreeMonthCallRecordCount,
                            ThreeMonthCallRecordAmount = summary.ThreeMonthCallRecordAmount,
                            SixMonthCallRecordCount = summary.SixMonthCallRecordCount,
                            SixMonthCallRecordAmount = summary.SixMonthCallRecordAmount,
                            Regdate = summary.Regdate.ToDateTime() != null ? ((DateTime)summary.Regdate.ToDateTime()).ToString(Consts.DateFormatString11) : summary.Regdate,
                            IsSelf = 1,
                            CallCntAvgBJ = summary.CallCntAvgBJ,
                            CallLenNitRate = summary.CallLenNitRate,
                            CallTimes = summary.CallTimes,
                            MaxPlanAmt = summary.MaxPlanAmt,
                            PhoneNbrBjRate = summary.PhoneNbrBjRate,
                            ZjsjPhnCrgAvg = summary.ZjsjPhnCrgAvg,
                            DAY90_CALLING_TIMES = summary.DAY90_CALLING_TIMES,
                            CALLED_PHONE_CNT = summary.CALLED_PHONE_CNT,
                            LOCAL_CALL_TIME = summary.LOCAL_CALL_TIME,
                            DAY180_CALLING_SUBTTL = summary.DAY180_CALLING_SUBTTL,
                            DAY90_CALL_TTL_TIME = summary.DAY90_CALL_TTL_TIME,
                            DAY90_CALL_TIMES = summary.DAY90_CALL_TIMES,
                            DAY90_CALLING_TTL_TIME = summary.DAY90_CALLING_TTL_TIME,
                            NET_LSTM6_ONL_FLOW = summary.NET_LSTM6_ONL_FLOW,
                            DAY_CALLING_TTL_TIME = summary.DAY_CALLING_TTL_TIME,
                            CALLED_TIMES = summary.CALLED_TIMES,
                            CALLED_TTL_TIME = summary.CALLED_TTL_TIME,
                            MRNG_CALLED_TIMES = summary.MRNG_CALLED_TIMES,
                            CALL_TTL_TIME = summary.CALL_TTL_TIME,
                            NIGHT_CALLED_TTL_TIME = summary.NIGHT_CALLED_TTL_TIME,
                            AFTN_CALL_TTL_TIME = summary.AFTN_CALL_TTL_TIME,
                            AFTN_CALLING_TTL_TIME = summary.AFTN_CALLING_TTL_TIME,
                            NIGHT_CALL_TTL_TIME = summary.NIGHT_CALL_TTL_TIME,
                            NIGHT_CALLING_TTL_TIME = summary.NIGHT_CALLING_TTL_TIME,
                            CALLING_TTL_TIME = summary.CALLING_TTL_TIME,
                            PH_USE_MONS = summary.PH_USE_MONS,
                            CALL_PHONE_CNT = summary.CALL_PHONE_CNT,
                            CTT_DAYS_CNT = summary.CTT_DAYS_CNT,
                            CALLED_CTT_DAYS_CNT = summary.CALLED_CTT_DAYS_CNT,
                            CALLING_CTT_DAYS_CNT = summary.CALLING_CTT_DAYS_CNT,
                            CALLED_TIMES_IN30DAY = summary.CALLED_TIMES_IN30DAY,
                            CALLED_TIMES_IN15DAY = summary.CALLED_TIMES_IN15DAY,
                            CALLED_TIMES_IN30DAY_GRAY = summary.CALLED_TIMES_IN30DAY_FOR_GRAY,
                            CALLED_TIMES_IN15DAY_GRAY = summary.CALLED_TIMES_IN15DAY_FOR_GRAY,
                        };
                        baseRes.Result = jsonService.SerializeObject(result);
                    }
                    else if (entity != null)
                    {
                        if (entity.Regdate.IsEmpty())
                        {
                            basic = basicService.GetByIdentityNoAndMobile(Req.identitycard, Req.mobile);
                            entity.Regdate = basic.Reg_time;
                        }
                        var result = new
                        {
                            Id = "jxl_" + entity.Oper_id,
                            City = entity.City,
                            IsRealNameAuth = entity.IsRealNameAuth,
                            Mobile = entity.Mobile,
                            OneMonthCallRecordCount = entity.OneMonthCallRecordCount,
                            OneMonthCallRecordAmount = entity.OneMonthCallRecordAmount,
                            ThreeMonthCallRecordCount = entity.ThreeMonthCallRecordCount,
                            ThreeMonthCallRecordAmount = entity.ThreeMonthCallRecordAmount,
                            SixMonthCallRecordCount = entity.SixMonthCallRecordCount,
                            SixMonthCallRecordAmount = entity.SixMonthCallRecordAmount,
                            Regdate = entity.Regdate,
                            IsSelf = 0,
                            CallCntAvgBJ = entity.CALL_CNT_AVG_BJ,
                            CallLenNitRate = entity.CALL_LEN_NIT_RATE,
                            CallTimes = entity.CallTimes,
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
                            CALLED_TIMES_IN30DAY_GRAY = entity.CALLED_TIMES_IN30DAY_Gray,
                            CALLED_TIMES_IN15DAY_GRAY = entity.CALLED_TIMES_IN15DAY_Gray,
                        };
                        baseRes.Result = jsonService.SerializeObject(result);
                    }
                }
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        #endregion

        #region 获取聚信立手机账单报告
        public BaseRes GetMobileReportForXml(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                JxlSubmitInfo Req = reqText.DeserializeXML<JxlSubmitInfo>();
                if (!string.IsNullOrEmpty(Req.id))
                {
                    baseRes = GetMobileReportById(Req);
                }
                else
                {
                    baseRes = GetMobileReport(Req);
                }
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        public BaseRes GetMobileReportForJson(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                JxlSubmitInfo Req = jsonService.DeserializeObject<JxlSubmitInfo>(reqText);
                if (!string.IsNullOrEmpty(Req.id))
                {
                    baseRes = GetMobileReportById(Req);
                }
                else
                {
                    baseRes = GetMobileReport(Req);
                }
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        public BaseRes GetMobileReportOld(JxlSubmitInfo Req)
        {
            Log4netAdapter.WriteInfo(string.Format("接口：GetMobileReport；客户端IP:{0},参数：{1}", ClientIp, jsonService.SerializeObject(Req, true)));
            BaseRes baseRes = new BaseRes();
            try
            {
                string ftpHost = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpAddress"));
                string ftpUser = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpUserName"));
                string ftpPassword = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpPassword"));
                string filepath = Chk.IsNull(ConfigurationHelper.GetAppSetting("JxlReportPath"));
                string filename = string.Empty;
                IOperationLog service = NetSpiderFactoryManager.GetOperationLogService();
                var entity = service.GetByIdentityNoAndMobile(Req.identitycard, Req.mobile);
                if (entity != null)
                {
                    filename = entity.ReceiveFilePath;
                    filepath += filename;
                }

                if (!File.Exists(filepath))
                {
                    FTPHelper ftp = new FTPHelper(ftpHost + ftpDirectory, ftpUser, ftpPassword);
                    ftp.DownloadFile(filepath, filename);
                }


                //baseRes.Result = jsonService.SerializeObject(entity);
                if (File.Exists(filepath))
                {
                    baseRes.Result = CommonFun.ClearFlag(FileOperateHelper.ReadFile(filepath));
                    baseRes.StatusDescription = "已读取手机账单报告";
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                }
                else
                {
                    baseRes.StatusDescription = "无数据";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                }
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }

        public BaseRes GetMobileReport(JxlSubmitInfo Req)
        {
            Log4netAdapter.WriteInfo(string.Format("接口：GetMobileReport；客户端IP:{0},参数：{1}", ClientIp, jsonService.SerializeObject(Req, true)));
            BaseRes baseRes = new BaseRes();
            try
            {
                string ftpHost = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpAddress"));
                string ftpUser = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpUserName"));
                string ftpPassword = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpPassword"));
                //string filepath = Chk.IsNull(ConfigurationHelper.GetAppSetting("JxlReportPath"));
                string filename = string.Empty;
                IOperationLog service = NetSpiderFactoryManager.GetOperationLogService();
                var entity = service.GetByIdentityNoAndMobile(Req.identitycard, Req.mobile);
                if (entity != null)
                {
                    filename = entity.ReceiveFilePath;
                }
                FTPHelper ftp = new FTPHelper(ftpHost + ftpDirectory, ftpUser, ftpPassword);
                string result = "";
                using(var stream = ftp.DownloadFileToStream(filename))
                {
                    result = stream.AsStringText();
                }

                if (!string.IsNullOrEmpty(result))
                {
                    baseRes.Result = CommonFun.ClearFlag(result);
                    baseRes.StatusDescription = "已读取手机账单报告";
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                }
                else
                {
                    baseRes.StatusDescription = "无数据";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                }
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }

        public BaseRes GetMobileReportById(JxlSubmitInfo Req)
        {
            Log4netAdapter.WriteInfo(string.Format("接口：GetMobileReportById；客户端IP:{0},参数：{1}", ClientIp, jsonService.SerializeObject(Req, true)));
            BaseRes baseRes = new BaseRes();
            try
            {
                string ftpHost = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpAddress"));
                string ftpUser = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpUserName"));
                string ftpPassword = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpPassword"));
                //string filepath = Chk.IsNull(ConfigurationHelper.GetAppSetting("JxlReportPath"));
                string filename = string.Empty;
                IOperationLog service = NetSpiderFactoryManager.GetOperationLogService();
                if (Req.id.Contains("jxl_"))
                {
                    Req.id = Req.id.Replace("jxl_", "");
                }
                else
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_error;
                    baseRes.StatusDescription = "数据不正确";
                    return baseRes;
                }
                var entity = service.Get(int.Parse(Req.id));
                if (entity != null)
                {
                    filename = entity.ReceiveFilePath;
                }
                FTPHelper ftp = new FTPHelper(ftpHost + ftpDirectory, ftpUser, ftpPassword);
                string result = "";
                using (var stream = ftp.DownloadFileToStream(filename))
                {
                    result = stream.AsStringText();
                }

                if (!string.IsNullOrEmpty(result))
                {
                    baseRes.Result = CommonFun.ClearFlag(result);
                    baseRes.StatusDescription = "已读取手机账单报告";
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                }
                else
                {
                    baseRes.StatusDescription = "无数据";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                }
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }



        #endregion

        #region 获取聚信立黑名单结果
        public BaseRes GetBlacklistResultForXml(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                Log4netAdapter.WriteInfo("参数:" + reqText);
                JxlSubmitInfo Req = reqText.DeserializeXML<JxlSubmitInfo>();
                baseRes = GetBlacklistResult(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        public BaseRes GetBlacklistResultForJson(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                Log4netAdapter.WriteInfo("参数:" + reqText);
                JxlSubmitInfo Req = jsonService.DeserializeObject<JxlSubmitInfo>(reqText);
                baseRes = GetBlacklistResult(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }


        /// <summary>
        /// 返回聚信立黑名单
        /// </summary>
        /// <param name="Req"></param>
        /// <returns></returns>
        public BaseRes GetBlacklistResult(JxlSubmitInfo Req)
        {
            Log4netAdapter.WriteInfo("接口：GetMobileReport；客户端IP:" + CommonFun.GetClientIP());
            BaseRes baseRes = new BaseRes();
            try
            {
                string ftpHost = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpAddress"));
                string ftpUser = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpUserName"));
                string ftpPassword = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpPassword"));
                string filepath = Chk.IsNull(ConfigurationHelper.GetAppSetting("JxlReportPath"));
                string filename = string.Empty;
                IOperationLog service = NetSpiderFactoryManager.GetOperationLogService();
                var entity = service.GetByIdentityNoAndMobile(Req.identitycard, Req.mobile);
                if (entity != null)
                {
                    filename = entity.ReceiveFilePath;
                    filepath += filename;
                }

                if (!File.Exists(filepath))
                {
                    FTPHelper ftp = new FTPHelper(ftpHost + ftpDirectory, ftpUser, ftpPassword);
                    ftp.DownloadFile(filepath, filename);
                }


                //baseRes.Result = jsonService.SerializeObject(entity);
                if (File.Exists(filepath))
                {
                    baseRes.Result = CommonFun.ClearFlag(FileOperateHelper.ReadFile(filepath));
                    var list = jsonService.GetArrayFromParse(baseRes.Result, "application_check");
                    foreach (var item in list)
                    {
                        if (jsonService.GetResultFromParser(item, "category") == "网络黑名单")
                        {
                            string result = jsonService.GetResultFromParser(item, "result");
                            if (result == "否")
                            {
                                baseRes.Result = "1"; //返回结果为客户属于聚信立黑名单客户
                                break;
                            }
                            else
                            {
                                baseRes.Result = "0"; //返回结果为客户不属于聚信立黑名单客户
                            }
                        }
                    }
                    baseRes.StatusDescription = "已读取手机账单报告";
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                }
                else
                {
                    baseRes.StatusDescription = "无数据";
                    baseRes.Result = "-1";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                }
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }


        #endregion

        #region 查询是否走公司内部接口
        public BaseRes GetMobileInfoIsSelfForXml(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                JxlSubmitInfo Req = reqText.DeserializeXML<JxlSubmitInfo>();
                baseRes = GetMobileInfoIsSelf(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        public BaseRes GetMobileInfoIsSelfForJson(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                JxlSubmitInfo Req = jsonService.DeserializeObject<JxlSubmitInfo>(reqText);
                baseRes = GetMobileInfoIsSelf(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        public BaseRes GetMobileInfoIsSelf(JxlSubmitInfo Req)
        {
            Log4netAdapter.WriteInfo(string.Format("接口：GetMobileInfoIsSelf；客户端IP:{0},参数：{1}", ClientIp, jsonService.SerializeObject(Req, true)));
            BaseRes baseRes = new BaseRes();
            try
            {
                ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
                Spd_applyEntity applyEntity = applyService.GetByIdentityCardAndSpiderTypeAndMobile(Req.identitycard, "mobile", Req.mobile);
                if (applyEntity != null)
                {
                    baseRes.Result = "1";
                }
                else
                {
                    baseRes.Result = "0";
                }
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        #endregion

        #region 聚信立推送数据
        /// <summary>
        /// 聚信立手机账单推送数据
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public JxlPushDataRes JxlPustData(Stream stream)
        {
            JxlPushDataRes baseRes = new JxlPushDataRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(false);
                reqText = reqText.Substring(5).ToUrlDecode();

                Log4netAdapter.WriteInfo("推送数据开始");
                Cn.Vcredit.ThirdParty.Juxinli.DataProcess process = new Cn.Vcredit.ThirdParty.Juxinli.DataProcess();
                int result = process.JxlPustData(reqText);
                //int result = 1;
                if (result == 1)
                {
                    baseRes.note = "接收完成";
                    baseRes.code = 200;
                }
                else if (result == 0)
                {
                    baseRes.note = "接收失败";
                    baseRes.code = 400;
                }
                else if (result == -1)
                {
                    baseRes.note = "数据格式异常";
                    baseRes.code = 400;
                    //Log4netAdapter.WriteInfo(reqText);
                }

                Log4netAdapter.WriteInfo("推送数据结束");
            }
            catch (Exception e)
            {
                baseRes.code = 400;
                baseRes.note = e.Message;
            }
            return baseRes;
        }
        #endregion

        #region 聚信立推送报告
        /// <summary>
        /// 聚信立手机账单推送报告
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public JxlPushDataRes JxlPustReport(Stream stream)
        {
            JxlPushDataRes baseRes = new JxlPushDataRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(false);
                reqText = reqText.Substring(5).ToUrlDecode();
                Cn.Vcredit.ThirdParty.Juxinli.DataProcess process = new Cn.Vcredit.ThirdParty.Juxinli.DataProcess();
                int result = process.JxlPustReport(reqText);
                //int result = 1;
                if (result == 1)
                {
                    baseRes.note = "接收完成";
                    baseRes.code = 200;
                }
                else if (result == 0)
                {
                    baseRes.note = "接收失败";
                    baseRes.code = 400;
                }
                else if (result == -1)
                {
                    baseRes.note = "数据格式异常";
                    baseRes.code = 400;
                    Log4netAdapter.WriteInfo(reqText);
                }
            }
            catch (Exception e)
            {
                baseRes.code = 400;
                baseRes.note = e.Message;
            }
            return baseRes;
        }
        #endregion

        #region 新聚信立推送数据
        /// <summary>
        /// 新聚信立手机账单推送数据
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public JxlPushDataRes JxlPushData(Stream stream)
        {
            JxlPushDataRes baseRes = new JxlPushDataRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(false);
                reqText = reqText.Substring(5).ToUrlDecode();
                Log4netAdapter.WriteInfo("快批推送数据开始");
                Cn.Vcredit.ThirdParty.Juxinli.DataProcess process = new Cn.Vcredit.ThirdParty.Juxinli.DataProcess();
                int result = process.JxlPushData(reqText);
                //int result = 1;
                if (result == 1)
                {
                    baseRes.note = "接收完成";
                    baseRes.code = 200;
                }
                else if (result == 0)
                {
                    baseRes.note = "接收失败";
                    baseRes.code = 400;
                }
                else if (result == -1)
                {
                    baseRes.note = "数据格式异常";
                    baseRes.code = 400;
                    Log4netAdapter.WriteInfo(reqText);
                }

                Log4netAdapter.WriteInfo("快批推送数据结束");
            }
            catch (Exception e)
            {
                baseRes.code = 400;
                baseRes.note = e.Message;
            }
            return baseRes;
        }
        #endregion

        #region 新聚信立推送报告
        /// <summary>
        /// 新聚信立手机账单推送报告
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public JxlPushDataRes JxlPushReport(Stream stream)
        {
            JxlPushDataRes baseRes = new JxlPushDataRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(false);
                reqText = reqText.Substring(5).ToUrlDecode();
                Cn.Vcredit.ThirdParty.Juxinli.DataProcess process = new Cn.Vcredit.ThirdParty.Juxinli.DataProcess();
                int result = process.JxlPushReport(reqText);
                //int result = 1;
                if (result == 1)
                {
                    baseRes.note = "接收完成";
                    baseRes.code = 200;
                }
                else if (result == -1)
                {
                    baseRes.note = "数据格式异常";
                    baseRes.code = 400;
                    Log4netAdapter.WriteInfo(reqText);
                }
            }
            catch (Exception e)
            {
                baseRes.code = 400;
                baseRes.note = e.Message;
            }
            return baseRes;
        }
        #endregion

        #region 查询数据
        public BaseRes GetMobileCallsOneMonthForXml(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(isbase64);
                JxlSubmitInfo Req = reqText.DeserializeXML<JxlSubmitInfo>();
                baseRes = GetMobileCallsOneMonth(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }

        public BaseRes GetMobileCallsOneMonthForJson(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(isbase64);
                JxlSubmitInfo Req = jsonService.DeserializeObject<JxlSubmitInfo>(reqText);
                baseRes = GetMobileCallsOneMonth(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        public BaseRes GetMobileCallsOneMonth(JxlSubmitInfo Req)
        {
            Log4netAdapter.WriteInfo(string.Format("接口：GetMobileCallsOneMonth；客户端IP:{0},参数：{1}", ClientIp, jsonService.SerializeObject(Req, true)));
            BaseRes baseRes = new BaseRes();
            try
            {
                ICalls service = NetSpiderFactoryManager.GetCallsService();
                var calls = service.GetListOneMonth(Req.identitycard, Req.mobile);
                if (calls != null)
                {
                    foreach (var item in calls)
                    {
                        item.OperationLog = null;
                    }
                    baseRes.Result = jsonService.SerializeObject(calls, true);
                    baseRes.StatusDescription = "查询成功";
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                }
                else
                {
                    baseRes.Result = "";
                    baseRes.StatusDescription = "无数据";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                }
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }

        public BaseRes GetMobileMySelfCallsForXml(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(isbase64);
                JxlSubmitInfo Req = reqText.DeserializeXML<JxlSubmitInfo>();
                baseRes = GetMobileMySelfCalls(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }

        public BaseRes GetMobileMySelfCallsForJson(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(isbase64);
                JxlSubmitInfo Req = jsonService.DeserializeObject<JxlSubmitInfo>(reqText);
                baseRes = GetMobileMySelfCalls(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        public BaseRes GetMobileMySelfCalls(JxlSubmitInfo Req)
        {
            Log4netAdapter.WriteInfo(string.Format("接口：GetMobileMySelfCalls；客户端IP:{0},参数：{1}", ClientIp, jsonService.SerializeObject(Req, true)));
            BaseRes baseRes = new BaseRes();
            try
            {
                ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
                Spd_applyEntity applyEntity = applyService.GetByIdentityCardAndSpiderTypeAndMobile(Req.identitycard, "mobile", Req.mobile);

                if (applyEntity != null && applyEntity.Crawl_status == ServiceConsts.CrawlerStatusCode_AnalysisSuccess)
                {
                    MobileMongo mobilemongoSer = new MobileMongo(applyEntity.CreateTime);
                    Basic basicEntity = mobilemongoSer.GetBasicByToken(applyEntity.Token);
                    baseRes.Result = jsonService.SerializeObject(basicEntity.CallList, true);
                    baseRes.StatusDescription = "查询成功";
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                }
                else
                {
                    baseRes.Result = "";
                    baseRes.StatusDescription = "无数据";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                }
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        #endregion

        #region 手机使用月份
        public BaseRes GetMobileUseMonthForXml(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(isbase64);
                JxlSubmitInfo Req = reqText.DeserializeXML<JxlSubmitInfo>();
                baseRes = GetMobileCallsOneMonth(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }

        public BaseRes GetMobileUseMonthForJson(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(isbase64);
                JxlSubmitInfo Req = jsonService.DeserializeObject<JxlSubmitInfo>(reqText);
                baseRes = GetMobileCallsOneMonth(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }

        public BaseRes GetMobileUseMonth(JxlSubmitInfo Req)
        {
            Log4netAdapter.WriteInfo(string.Format("接口：GetMobileUseMonth；客户端IP:{0},参数：{1}", ClientIp, jsonService.SerializeObject(Req, true)));
            BaseRes baseRes = new BaseRes();
            try
            {
                ISummary service = NetSpiderFactoryManager.GetSummaryService();
                int usemonth = service.GetUseMonths(Req.identitycard, Req.mobile);
                baseRes.Result = usemonth.ToString();
                baseRes.StatusDescription = "查询成功";
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        #endregion

        #region 学历查询
        public BaseRes GetEducationInfo(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            return baseRes;
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                JxlSubmitInfo Req = jsonService.DeserializeObject<JxlSubmitInfo>(reqText);
                Log4netAdapter.WriteInfo("通过聚信立接口查询学历，参数：" + reqText);
                try
                {
                    IChsi_Info chsiService = NetSpiderFactoryManager.GetChsi_InfoService();
                    ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();

                    Chsi_InfoEntity entity = chsiService.GetByIdentityCard(Req.identitycard);
                    if (entity == null)
                    {
                        Cn.Vcredit.ThirdParty.Juxinli.DataProcess process = new Cn.Vcredit.ThirdParty.Juxinli.DataProcess();
                        //baseRes.Result = process.GetEducationInfo(Req.name, Req.identitycard);
                        IChsiCrawler crawler = CrawlerManager.GetChsiCrawler();
                        entity = crawler.Query_GetJxlInfo(baseRes.Result);
                        if (!entity.IdentityCard.IsEmpty())
                        {
                            string token = CommonFun.GetGuidID();

                            Spd_applyEntity applyEntity = new Spd_applyEntity();
                            applyEntity.Identitycard = Req.identitycard;
                            applyEntity.IPAddr = ClientIp;
                            applyEntity.Name = Req.name;
                            applyEntity.Website = ServiceConsts.SpiderType_JxlEdu;
                            applyEntity.Spider_type = ServiceConsts.SpiderType_JxlEdu;
                            applyEntity.Apply_status = ServiceConsts.ApplyStatus_Success;
                            applyEntity.Mobile = Req.mobile;
                            applyEntity.Description = "聚信立学历查询";
                            applyEntity.Token = token;
                            applyService.Save(applyEntity);
                            entity.Token = token;
                            chsiService.Save(entity);
                        }
                        else
                        {
                            baseRes.StatusDescription = "无数据";
                            baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                        }
                    }
                    if (!string.IsNullOrEmpty(entity.University))
                    {
                        baseRes.StatusCode = ServiceConsts.StatusCode_success;
                        baseRes.StatusDescription = "聚信立学历查询成功";
                    }
                    baseRes.Result = jsonService.SerializeObject(entity, true);
                }
                catch (Exception e)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_error;
                    baseRes.StatusDescription = e.Message;
                    Log4netAdapter.WriteError("查询学历出错", e);
                }
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        #endregion

    }
}