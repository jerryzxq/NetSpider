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
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Service;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.RestService.Models.RC;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.RestService.Models.Credit;

namespace Vcredit.NetSpider.RestService.Services
{
    //Asp.net管道兼容
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class CreditService : ICreditService
    {
        #region 声明变量、接口
        IPbccrcExecutor pbccrcExecutor = ExecutorManager.GetPbccrcExecutor();//社保数据采集接口
        IProvidentFundExecutor profundExecutor = ExecutorManager.GetProvidentFundExecutor();//社保数据采集接口
        ISociaSecurityExecutor socialsecExecutor = ExecutorManager.GetSociaSecurityExecutor();//社保数据采集接口

        IPluginSecurityCode secService = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析接口
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
        bool isbase64 = true;
        string ClientIp = CommonFun.GetClientIP();
        #endregion

        public CreditService()
        {
        }
        #region 初始化
        /// <summary>
        /// 央行互联网征信登录初始化（XML）
        /// </summary>
        /// <returns></returns>
        public VerCodeRes PbccrcReportInitForXml()
        {
            return PbccrcReportInit();
        }
        /// <summary>
        /// 央行互联网征信登录初始化（JSON）
        /// </summary>
        /// <returns></returns>
        public VerCodeRes PbccrcReportInitForJson()
        {
            return PbccrcReportInit();
        }
        /// <summary>
        /// 央行互联网征信登录初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes PbccrcReportInit()
        {
            return pbccrcExecutor.Init();
        }
        #endregion

        #region 登录
        /// <summary>
        /// 央行互联网征信登录（XML）
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public BaseRes PbccrcReportLoginForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            LoginReq Req = reqText.DeserializeXML<LoginReq>();

            return PbccrcReportLogin(Req);
        }
        /// <summary>
        /// 央行互联网征信登录（JSON）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcReportLoginForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            LoginReq Req = jsonService.DeserializeObject<LoginReq>(reqText);

            return PbccrcReportLogin(Req);
        }
        /// <summary>
        /// 央行互联网征信登录
        /// </summary>
        /// <param name="login">login实体</param>
        /// <returns></returns>
        public BaseRes PbccrcReportLogin(LoginReq login)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportLogin；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(login, true));
            BaseRes baseRes = new BaseRes();
            try
            {
                string token = login.Token;
                baseRes = pbccrcExecutor.Login(token, login.Username, login.Password, login.Vercode);
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("", e);
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        #endregion

        #region 征信报告查询
        /// <summary>
        /// 央行互联网征信报告查询（XML）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public CRD_HD_REPORTRes GetPbccrcReportForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            PbccrcReportQueryReq queryReq = reqText.DeserializeXML<PbccrcReportQueryReq>();
            return GetPbccrcReport(queryReq);
        }
        /// <summary>
        /// 央行互联网征信报告查询（JSON）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public CRD_HD_REPORTRes GetPbccrcReportForJson(Stream stream)
        {

            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            PbccrcReportQueryReq queryReq = jsonService.DeserializeObject<PbccrcReportQueryReq>(reqText);
            return GetPbccrcReport(queryReq);
        }
        /// <summary>
        /// 央行互联网征信报告查询
        /// </summary>
        /// <param name="queryReq">PbccrcReportQueryReq实体</param>
        /// <returns></returns>
        public CRD_HD_REPORTRes GetPbccrcReport(PbccrcReportQueryReq queryReq)
        {

            Log4netAdapter.WriteInfo("接口名：GetPbccrcReport；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(queryReq, true));

            string token = queryReq.Token;
            BaseRes baseRes = new BaseRes();
            CRD_HD_REPORTRes queryRes = new CRD_HD_REPORTRes();
            try
            {
                PbccrcReportQueryReq spiderReq = new PbccrcReportQueryReq();
                spiderReq.Token = queryReq.Token;
                spiderReq.BusType = queryReq.BusType;
                spiderReq.BusId = queryReq.BusId;
                spiderReq.querycode = queryReq.querycode;
                spiderReq.IdentityCard = queryReq.IdentityCard;
                queryRes = pbccrcExecutor.GetReport(spiderReq);
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("征信信息采集异常", e);
                queryRes.StatusCode = ServiceConstants.StatusCode_error;
                queryRes.StatusDescription = e.Message;
            }
            queryRes.EndTime = DateTime.Now.ToString();
            return queryRes;
        }

        /// <summary>
        /// 获取征信查看记录
        /// </summary>
        /// <param name="reportid"></param>
        /// <returns></returns>
        public BaseRes PbccrcReportQueryRecorddtlForJson(string reportid)
        {
            return GetRecorddt(reportid);
        }

        public BaseRes PbccrcReportQueryRecorddtlForXml(string reportid)
        {
            return GetRecorddt(reportid);
        }

        public BaseRes GetRecorddt(string reportid)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportQueryRecorddtlForXml；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：reportid：" + reportid);
            BaseRes baseRes = new BaseRes();

            try
            {
                baseRes = pbccrcExecutor.GetListCrdQrrecorddtlByrepid(reportid.ToInt().Value);

            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("获取征信查看记录异常", e);
                baseRes.StatusCode = ServiceConstants.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            baseRes.EndTime = DateTime.Now.ToString();
            return baseRes;
        }


        #endregion

        #region 注册
        /// <summary>
        /// 央行互联网征信注册，第一步（XML）
        /// </summary>
        /// <param name="stream">post数据，例如：{'name':'{0}','certNo':'{1}','certType':'0','VerCode':'{2}'}</param>
        /// <returns></returns>
        public BaseRes PbccrcReportRegisterStep1ForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            PbccrcReportRegisterReq registerReq = reqText.DeserializeXML<PbccrcReportRegisterReq>();
            return PbccrcReportRegisterStep1(registerReq);
        }
        /// <summary>
        /// 央行互联网征信注册，第一步（JSON）
        /// </summary>
        /// <param name="stream">post数据，例如：{'name':'{0}','certNo':'{1}','certType':'0','VerCode':'{2}'}</param>
        /// <returns></returns>
        public BaseRes PbccrcReportRegisterStep1ForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            PbccrcReportRegisterReq registerReq = jsonService.DeserializeObject<PbccrcReportRegisterReq>(reqText);
            return PbccrcReportRegisterStep1(registerReq);
        }
        /// <summary>
        /// 央行互联网征信注册，第一步
        /// </summary>
        /// <param name="registerReq">PbccrcReportRegisterReq实体</param>
        /// <returns></returns>
        public BaseRes PbccrcReportRegisterStep1(PbccrcReportRegisterReq registerReq)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportRegisterStep1；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(registerReq, true));

            string token = registerReq.Token;
            return pbccrcExecutor.Register_Step1(token, registerReq.name, registerReq.certNo, registerReq.certType, registerReq.Vercode);
        }
        /// <summary>
        /// 央行互联网征信注册，第二步（XML）
        /// </summary>
        /// <param name="stream">post数据，例如：{'mobileTel':'13524909205'}</param>
        /// <returns></returns>
        public BaseRes PbccrcReportRegisterStep2ForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            PbccrcReportRegisterReq registerReq = reqText.DeserializeXML<PbccrcReportRegisterReq>();
            return PbccrcReportRegisterStep2(registerReq);
        }
        /// <summary>
        /// 央行互联网征信注册，第二步（JSON）
        /// </summary>
        /// <param name="stream">post数据，例如：{'mobileTel':'13524909205'}</param>
        /// <returns></returns>
        public BaseRes PbccrcReportRegisterStep2ForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            PbccrcReportRegisterReq registerReq = jsonService.DeserializeObject<PbccrcReportRegisterReq>(reqText);
            return PbccrcReportRegisterStep2(registerReq);
        }
        /// <summary>
        /// 央行互联网征信注册，第二步
        /// </summary>
        /// <param name="registerReq">PbccrcReportRegisterReq实体</param>
        /// <returns></returns>
        public BaseRes PbccrcReportRegisterStep2(PbccrcReportRegisterReq registerReq)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportRegisterStep2；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(registerReq, true));
            string token = registerReq.Token;
            return pbccrcExecutor.Register_Step2(token, registerReq.mobileTel);
        }
        /// <summary>
        /// 央行互联网征信注册，第三步（XML）
        /// </summary>
        /// <param name="stream">post数据，例如：{'loginname':'{0}','password':'{1}','confirmpassword':'{2}','mobileTel':'{3}','smscode':'{4}'}</param>
        /// <returns></returns>
        public BaseRes PbccrcReportRegisterStep3ForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            PbccrcReportRegisterReq registerReq = reqText.DeserializeXML<PbccrcReportRegisterReq>();
            return PbccrcReportRegisterStep3(registerReq);
        }
        /// <summary>
        /// 央行互联网征信注册，第三步（JSON）
        /// </summary>
        /// <param name="stream">post数据，例如：{'loginname':'{0}','password':'{1}','confirmpassword':'{2}','mobileTel':'{3}','smscode':'{4}'}</param>
        /// <returns></returns>
        public BaseRes PbccrcReportRegisterStep3ForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            PbccrcReportRegisterReq RegisterReq = jsonService.DeserializeObject<PbccrcReportRegisterReq>(reqText);
            return PbccrcReportRegisterStep3(RegisterReq);
        }
        /// <summary>
        /// 央行互联网征信注册，第三步
        /// </summary>
        /// <param name="registerReq">PbccrcReportRegisterReq请求实体</param>
        /// <returns></returns>
        public BaseRes PbccrcReportRegisterStep3(PbccrcReportRegisterReq registerReq)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportRegisterStep3；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(registerReq, true));
            string token = registerReq.Token;
            return pbccrcExecutor.Register_Step3(token, registerReq.Username, registerReq.Password, registerReq.confirmpassword, registerReq.email, registerReq.mobileTel, registerReq.Smscode);
        }
        #endregion

        #region 查询申请
        #region 问题查询
        /// <summary>
        /// 央行互联网征信查询申请，第一步（XML）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcReportQueryApplicationStep1ForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            PbccrcReportQueryApplyReq Req = reqText.DeserializeXML<PbccrcReportQueryApplyReq>();

            return PbccrcReportQueryApplicationStep1(Req);
        }
        /// <summary>
        /// 央行互联网征信查询申请，第一步（JSON）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcReportQueryApplicationStep1ForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            PbccrcReportQueryApplyReq Req = jsonService.DeserializeObject<PbccrcReportQueryApplyReq>(reqText);

            return PbccrcReportQueryApplicationStep1(Req);
        }
        /// <summary>
        /// 央行互联网征信查询申请，第一步
        /// </summary>
        /// <param name="login">WechatQueryReq请求实体</param>
        /// <returns></returns>
        public BaseRes PbccrcReportQueryApplicationStep1(PbccrcReportQueryApplyReq login)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportQueryApplicationStep1；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(login, true));
            string token = login.Token;
            bool isContinue = false;
            if (login.Type == "unverify")
            {
                isContinue = true;
            }
            return pbccrcExecutor.QueryApplication_Step1(token, isContinue, login.IdentityCard);
        }
        /// <summary>
        /// 央行互联网征信查询申请，第二步（XML）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcReportQueryApplicationStep2ForXml(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                PbccrcReportQueryApplyReq applyReq = reqText.DeserializeXML<PbccrcReportQueryApplyReq>();
                return PbccrcReportQueryApplicationStep2(applyReq);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 央行互联网征信查询申请，第二步（JSON）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcReportQueryApplicationStep2ForJson(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                PbccrcReportQueryApplyReq applyReq = jsonService.DeserializeObject<PbccrcReportQueryApplyReq>(reqText);
                return PbccrcReportQueryApplicationStep2(applyReq);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 央行互联网征信查询申请，第二步
        /// </summary>
        /// <param name="applyReq">PbccrcReportQueryApplyReq请求实体</param>
        /// <returns></returns>
        public BaseRes PbccrcReportQueryApplicationStep2(PbccrcReportQueryApplyReq applyReq)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportQueryApplicationStep2；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(applyReq, true));
            string token = applyReq.Token;
            return pbccrcExecutor.QueryApplication_Step2(token, applyReq.KbaQuestions);
        }
        #endregion


        #region 银行卡查询
        /// <summary>
        /// 央行互联网征信查询申请，第一步（XML）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public VerCodeRes PbccrcReportQueryApplicationCreditStep1ForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            PbccrcReportQueryApplyReq Req = reqText.DeserializeXML<PbccrcReportQueryApplyReq>();

            return PbccrcReportQueryApplicationCreditStep1(Req);
        }
        /// <summary>
        /// 央行互联网征信查询申请，第一步（JSON）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public VerCodeRes PbccrcReportQueryApplicationCreditStep1ForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            PbccrcReportQueryApplyReq Req = jsonService.DeserializeObject<PbccrcReportQueryApplyReq>(reqText);

            return PbccrcReportQueryApplicationCreditStep1(Req);
        }
        /// <summary>
        /// 央行互联网征信查询申请，第一步
        /// </summary>
        /// <param name="login">WechatQueryReq请求实体</param>
        /// <returns></returns>
        public VerCodeRes PbccrcReportQueryApplicationCreditStep1(PbccrcReportQueryApplyReq login)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportQueryApplicationStep1；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(login, true));
            bool isContinue = false;
            if (login.Type == "unverify")
            {
                isContinue = true;
            }
            return pbccrcExecutor.QueryApplication_CreditCard_Step1(login.Token, isContinue);
        }
        /// <summary>
        /// 央行互联网征信查询申请，第二步（XML）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcReportQueryApplicationCreditStep2ForXml(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                PbccrcReportQueryApplyReq applyReq = reqText.DeserializeXML<PbccrcReportQueryApplyReq>();
                return PbccrcReportQueryApplicationCreditStep2(applyReq);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 央行互联网征信查询申请，第二步（JSON）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcReportQueryApplicationCreditStep2ForJson(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                PbccrcReportQueryApplyReq applyReq = jsonService.DeserializeObject<PbccrcReportQueryApplyReq>(reqText);
                return PbccrcReportQueryApplicationCreditStep2(applyReq);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 央行互联网征信查询申请，第二步
        /// </summary>
        /// <param name="applyReq">PbccrcReportQueryApplyReq请求实体</param>
        /// <returns></returns>
        public BaseRes PbccrcReportQueryApplicationCreditStep2(PbccrcReportQueryApplyReq applyReq)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportQueryApplicationStep2；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(applyReq, true));
            return pbccrcExecutor.QueryApplication_CreditCard_Step2(applyReq.Token, applyReq.UnionpayCode, applyReq.VerCode);
        }


        /// <summary>
        /// 央行互联网征信填写征信空白（XML）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcReportQueryApplicationAddBlankRecordForXml(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                PbccrcReportRegisterReq applyReq = reqText.DeserializeXML<PbccrcReportRegisterReq>();
                return PbccrcReportQueryApplicationAddBlankRecord(applyReq);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 央行互联网征信填写征信空白（JSON）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcReportQueryApplicationAddBlankRecordForJson(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                PbccrcReportRegisterReq applyReq = jsonService.DeserializeObject<PbccrcReportRegisterReq>(reqText);
                return PbccrcReportQueryApplicationAddBlankRecord(applyReq);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 央行互联网征信填写征信空白
        /// </summary>
        /// <param name="applyReq">PbccrcReportQueryApplyReq请求实体</param>
        /// <returns></returns>
        public BaseRes PbccrcReportQueryApplicationAddBlankRecord(PbccrcReportRegisterReq applyReq)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportQueryApplicationStep2；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(applyReq, true));
            return pbccrcExecutor.QueryApplication_AddBlankRecord(applyReq.certNo, applyReq.name);
        }
        #endregion

        #region 获取银联卡认证码
        /// <summary>
        /// 银联卡获取认证码，初始化（XML）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcQueryApplicationGetUnionPayCodeInitForXml(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                PbccrcReportQueryApplyReq applyReq = reqText.DeserializeXML<PbccrcReportQueryApplyReq>();
                return PbccrcQueryApplicationGetUnionPayCodeInit(applyReq);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 银联卡获取认证码，初始化（JSON）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcQueryApplicationGetUnionPayCodeInitForJson(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                PbccrcReportQueryApplyReq applyReq = jsonService.DeserializeObject<PbccrcReportQueryApplyReq>(reqText);
                return PbccrcQueryApplicationGetUnionPayCodeInit(applyReq);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 银联卡获取认证码，初始化
        /// </summary>
        /// <param name="applyReq">PbccrcReportQueryApplyReq请求实体</param>
        /// <returns></returns>
        public BaseRes PbccrcQueryApplicationGetUnionPayCodeInit(PbccrcReportQueryApplyReq applyReq)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportQueryApplicationStep2；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(applyReq, true));
            return pbccrcExecutor.QueryApplication_GetUnionPayCode_Init(applyReq.UnionHtml);
        }

        /// <summary>
        /// 银联卡获取认证码，第一步（XML）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcQueryApplicationGetUnionPayCodeStep1ForXml(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                UnionPayReq applyReq = reqText.DeserializeXML<UnionPayReq>();
                return PbccrcQueryApplicationGetUnionPayCodeStep1(applyReq);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 银联卡获取认证码，第一步（JSON）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcQueryApplicationGetUnionPayCodeStep1ForJson(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                UnionPayReq applyReq = jsonService.DeserializeObject<UnionPayReq>(reqText);
                return PbccrcQueryApplicationGetUnionPayCodeStep1(applyReq);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 银联卡获取认证码，第一步
        /// </summary>
        /// <param name="applyReq">PbccrcReportQueryApplyReq请求实体</param>
        /// <returns></returns>
        public BaseRes PbccrcQueryApplicationGetUnionPayCodeStep1(UnionPayReq applyReq)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportQueryApplicationStep2；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(applyReq, true));
            return pbccrcExecutor.QueryApplication_GetUnionPayCode_Step1(applyReq.Token, applyReq.creditcardNo);
        }

        /// <summary>
        /// 银联卡获取认证码，第二步（XML）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcQueryApplicationGetUnionPayCodeStep2ForXml(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                UnionPayReq applyReq = reqText.DeserializeXML<UnionPayReq>();
                return PbccrcQueryApplicationGetUnionPayCodeStep2(applyReq);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 银联卡获取认证码，第二步（JSON）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcQueryApplicationGetUnionPayCodeStep2ForJson(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                UnionPayReq applyReq = jsonService.DeserializeObject<UnionPayReq>(reqText);
                return PbccrcQueryApplicationGetUnionPayCodeStep2(applyReq);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 银联卡获取认证码，第二步
        /// </summary>
        /// <param name="applyReq">PbccrcReportQueryApplyReq请求实体</param>
        /// <returns></returns>
        public BaseRes PbccrcQueryApplicationGetUnionPayCodeStep2(UnionPayReq applyReq)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportQueryApplicationStep2；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(applyReq, true));
            return pbccrcExecutor.QueryApplication_GetUnionPayCode_Step2(applyReq.Token, applyReq.mobile);
        }

        /// <summary>
        /// 银联卡获取认证码，第三步（XML）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcQueryApplicationGetUnionPayCodeStep3ForXml(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                UnionPayReq applyReq = reqText.DeserializeXML<UnionPayReq>();
                return PbccrcQueryApplicationGetUnionPayCodeStep3(applyReq);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 银联卡获取认证码，第三步（JSON）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcQueryApplicationGetUnionPayCodeStep3ForJson(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                UnionPayReq applyReq = jsonService.DeserializeObject<UnionPayReq>(reqText);
                return PbccrcQueryApplicationGetUnionPayCodeStep3(applyReq);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 银联卡获取认证码，第三步
        /// </summary>
        /// <param name="applyReq">PbccrcReportQueryApplyReq请求实体</param>
        /// <returns></returns>
        public BaseRes PbccrcQueryApplicationGetUnionPayCodeStep3(UnionPayReq applyReq)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportQueryApplicationStep2；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(applyReq, true));
            return pbccrcExecutor.QueryApplication_GetUnionPayCode_Step3(applyReq);
        }
        #endregion

        /// <summary>
        /// 央行互联网征信,申请查询码结果查询（XML）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcReportQueryApplicationResultForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            BaseReq Req = reqText.DeserializeXML<BaseReq>();

            return PbccrcReportQueryApplicationResult(Req);
        }
        /// <summary>
        /// 央行互联网征信,申请查询码结果查询（JSON）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes PbccrcReportQueryApplicationResultForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            BaseReq Req = jsonService.DeserializeObject<BaseReq>(reqText);

            return PbccrcReportQueryApplicationResult(Req);
        }
        /// <summary>
        /// 央行互联网征信,申请查询码结果查询
        /// </summary>
        /// <param name="login">WechatQueryReq请求实体</param>
        /// <returns></returns>
        public BaseRes PbccrcReportQueryApplicationResult(BaseReq login)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportQueryApplicationResult；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(login, true));
            return pbccrcExecutor.QueryApplication_Result(login.Token);
        }
        #endregion

        #region VBS查询网络版征信
        /// <summary>
        /// 征信报告数据从数据库查询
        /// </summary>
        /// <param name="bid"></param>
        /// <returns></returns>
        public CRD_HD_REPORTRes GetPbccrcReportFromDBForJson(string bid)
        {
            Log4netAdapter.WriteInfo("接口：ProvidentFundQueryFromDBForJson；客户端IP:" + CommonFun.GetClientIP());
            CRD_HD_REPORTRes queryRes = new CRD_HD_REPORTRes();
            CRD_HD_REPORTEntity entity = new CRD_HD_REPORTEntity();
            try
            {
                ICRD_HD_REPORT service = NetSpiderFactoryManager.GetCRDHDREPORTService();
                entity = service.GetByBid(bid.ToInt(0));
                if (entity != null)
                {
                    queryRes = jsonService.DeserializeObject<CRD_HD_REPORTRes>(jsonService.SerializeObject(entity));
                    queryRes.StatusDescription = "查询成功";
                }
                else
                {
                    queryRes.StatusDescription = "无数据";
                }
                queryRes.StatusCode = ServiceConstants.StatusCode_success;
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("查询征信报告异常", e);
                queryRes.StatusCode = ServiceConstants.StatusCode_error;
                queryRes.StatusDescription = e.Message;
            }
            queryRes.EndTime = DateTime.Now.ToString();
            return queryRes;
        }
        public BaseRes SyncCreditInfoToVBS(string reportid)
        {
            Log4netAdapter.WriteInfo("接口：SyncCreditInfoToVBS；客户端IP:" + CommonFun.GetClientIP());
            BaseRes Res = new BaseRes();
            IList<object[]> objs = null;
            try
            {
                ICRD_HD_REPORT service = NetSpiderFactoryManager.GetCRDHDREPORTService();
                objs = service.GetVbsSycnData(reportid.ToInt(0));
                if (objs != null)
                {
                    Res.Result = jsonService.SerializeObject(objs);
                    Res.StatusDescription = "查询成功";
                }
                else
                {
                    Res.StatusDescription = "无数据";
                }
                Res.StatusCode = ServiceConstants.StatusCode_success;
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("同步征信报告数据到VBS异常", e);
                Res.StatusCode = ServiceConstants.StatusCode_error;
                Res.StatusDescription = e.Message;
            }
            Res.EndTime = DateTime.Now.ToString();
            return Res;
        }
        public BaseRes GetPbccrcCardAgeMonth(Stream stream)
        {
            Log4netAdapter.WriteInfo("接口：GetPbccrcCardAgeMonth；客户端IP:" + CommonFun.GetClientIP());
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(isbase64);
                Log4netAdapter.WriteInfo("参数:" + reqText);
                PbccrcReportQueryReq Req = jsonService.DeserializeObject<PbccrcReportQueryReq>(reqText);
                ICRD_HD_REPORT reportService = NetSpiderFactoryManager.GetCRDHDREPORTService();
                CRD_HD_REPORTEntity entity = reportService.GetByIdentityCard(Req.IdentityCard);

                if (entity != null)
                {
                    baseRes.Result = entity.CRD_STAT_LND.CARDAGEMONTH != null ? entity.CRD_STAT_LND.CARDAGEMONTH.ToString() : "0";
                    baseRes.StatusDescription = "查询完毕";
                }
                else
                {
                    baseRes.StatusDescription = "无数据";
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

        #region 征信评分计算
        public BaseRes CreditScoreCalculateForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            CreditReq queryReq = reqText.DeserializeXML<CreditReq>();
            return CreditScoreCalculate(queryReq);
        }
        public BaseRes CreditScoreCalculateForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            CreditReq queryReq = jsonService.DeserializeObject<CreditReq>(reqText);
            return CreditScoreCalculate(queryReq);
        }
        /// <summary>
        /// 默认模型，计算征信评分
        /// </summary>
        /// <param name="queryReq"></param>
        /// <returns></returns>
        public BaseRes CreditScoreCalculate(CreditReq queryReq)
        {
            Log4netAdapter.WriteInfo(string.Format("接口：CreditScoreCalculate；客户端IP:{0},参数：{1}", ClientIp, jsonService.SerializeObject(queryReq.IdentityCard, true)));

            BaseRes baseRes = new BaseRes();
            DecisionResult decisionResult = new DecisionResult();
            RCWebService.RCWebService rcServcie = new RCWebService.RCWebService();
            List<VBXML> xmllist = new List<VBXML>();
            int score = 0;
            try
            {
                ICRD_HD_REPORT reportService = NetSpiderFactoryManager.GetCRDHDREPORTService();
                CRD_HD_REPORTEntity reportEntity = null;
                if (!queryReq.ReportSn.IsEmpty())
                {
                    reportEntity = reportService.GetByReportSn(queryReq.ReportSn, true);

                    if (reportEntity.BusIdentityCard.IsEmpty() && !queryReq.IdentityCard.IsEmpty())
                    {
                        reportEntity.BusIdentityCard = queryReq.IdentityCard;
                        reportService.Update(reportEntity);
                    }

                }
                else if (queryReq.ReportSn.IsEmpty() && !queryReq.IdentityCard.IsEmpty())
                {
                    reportEntity = reportService.GetByIdentityCardAndBusType(queryReq.IdentityCard, queryReq.BusType);

                }
                else if (queryReq.ReportSn.IsEmpty() && queryReq.IdentityCard.IsEmpty())
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "缺少征信信息查询参数";
                    return baseRes;
                }

                #region 计算征信评分
                if (reportEntity != null)
                {
                    score = reportEntity.Score;
                    //校验人员姓名与身份证与注册信息是否一致
                    if (reportEntity.CertNo.Substring(14) != queryReq.IdentityCard.Substring(14) || reportEntity.Name != queryReq.Name)
                    {
                        Log4netAdapter.WriteInfo("警告：征信姓名、身份证号与注册信息不一致");
                        baseRes.StatusDescription = "注册信息与征信数据不一致";
                        return baseRes;
                    }

                    if (reportEntity.CRD_STAT_LN != null && reportEntity.CRD_STAT_LND != null && reportEntity.CRD_STAT_QR != null)
                    {
                        xmllist.Add(new VBXML { VB_COL = "NC_M3_ALL_CNT_TOTAL", VB_VALUE = reportEntity.CRD_STAT_QR.M3ALLCNTTOTAL != null ? reportEntity.CRD_STAT_QR.M3ALLCNTTOTAL.ToString() : "0" });//[网络版征信]三个月内所有征信查询次数
                        xmllist.Add(new VBXML { VB_COL = "NC_M9_DELIVER_CNT", VB_VALUE = reportEntity.CRD_STAT_LND.M9DELIVERCNT != null ? reportEntity.CRD_STAT_LND.M9DELIVERCNT.ToString() : "0" });//[网络版征信]最近九个月发放贷记卡数量
                        xmllist.Add(new VBXML { VB_COL = "ALL_CREDIT_UNCLOSED_CNT", VB_VALUE = reportEntity.CRD_STAT_LND.ALLCREDITUNCLOSEDCNT != null ? reportEntity.CRD_STAT_LND.ALLCREDITUNCLOSEDCNT.ToString() : "0" });//[网络版征信]信用卡未销户账户数(all)
                        xmllist.Add(new VBXML { VB_COL = "NC_ALL_LOAN_OTHER_CNT", VB_VALUE = reportEntity.CRD_STAT_LN.ALLLOANOTHERCNT != null ? reportEntity.CRD_STAT_LN.ALLLOANOTHERCNT.ToString() : "0" });//[网络版征信]其他贷款数量
                        xmllist.Add(new VBXML { VB_COL = "CARD_AGE_MONTH", VB_VALUE = reportEntity.CRD_STAT_LND.CARDAGEMONTH != null ? reportEntity.CRD_STAT_LND.CARDAGEMONTH.ToString() : "0" });//[网络版征信]最早卡龄
                        xmllist.Add(new VBXML { VB_COL = "CREDIT_LIMIT_AMOUNT_NORM_MAX", VB_VALUE = reportEntity.CRD_STAT_LND.CREDITLIMITAMOUNTNORMMAX.ToString() });//[网络版征信]正常信用卡最大额度
                        xmllist.Add(new VBXML { VB_COL = "CCL_GEN_MAR", VB_VALUE = ChinaIDCard.GetSex(queryReq.IdentityCard) + queryReq.Marriage });//[卡卡贷]性别婚姻状况
                        xmllist.Add(new VBXML { VB_COL = "CCL_HOS_EDU", VB_VALUE = queryReq.IsLocal + queryReq.Education });//[卡卡贷]是否本地籍教育程度
                        xmllist.Add(new VBXML { VB_COL = "FC_IF_PAY_FUND", VB_VALUE = queryReq.IsPayfund });//[公积金社保]是否缴纳金
                        xmllist.Add(new VBXML { VB_COL = "LOAN_AGE_MONTH", VB_VALUE = reportEntity.CRD_STAT_LN.LOANAGEMONTH != null ? reportEntity.CRD_STAT_LN.LOANAGEMONTH.ToString() : "0" });//[网络版征信]最早贷龄
                        xmllist.Add(new VBXML { VB_COL = "NC_NORMAL_CREDIT_BALANCE", VB_VALUE = reportEntity.CRD_STAT_LND.NORMALCREDITBALANCE != null ? reportEntity.CRD_STAT_LND.NORMALCREDITBALANCE.ToString() : "0" });//[网络版征信]正常信用卡未用额度
                        xmllist.Add(new VBXML { VB_COL = "NC_NORMAL_USED_RATE", VB_VALUE = reportEntity.CRD_STAT_LND.NORMALUSEDRATE != null ? reportEntity.CRD_STAT_LND.NORMALUSEDRATE.ToString() : "0" });//[网络版征信]正常信用卡使用率
                        xmllist.Add(new VBXML { VB_COL = "Customer_HOUSEHOLD", VB_VALUE = queryReq.IsLocal });//[卡卡贷]是否本地籍

                        Log4netAdapter.WriteInfo(jsonService.SerializeObject(xmllist));

                        string rc = rcServcie.GetRuleResultByCustomWithIdentityNo(queryReq.IdentityCard, queryReq.Name, "网络版征信评分", SerializationHelper.SerializeToXml(xmllist));
                        decisionResult = rc.DeserializeXML<List<DecisionResult>>().FirstOrDefault();

                        if (decisionResult != null)
                        {
                            if (decisionResult.RuleResultCanShowSets.ContainsKey("*网络版征信评分_CBS"))
                            {
                                score = decisionResult.RuleResultCanShowSets["*网络版征信评分_CBS"].ToInt(0);
                            }
                        }
                        reportEntity.Score = score;
                        reportService.Update(reportEntity);
                    }

                    var reportScore = new
                    {
                        ReportId = reportEntity.Id,
                        ReportSn = reportEntity.ReportSn,
                        ReportTime = reportEntity.ReportCreateTime,
                        Score = score
                    };
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                    baseRes.StatusDescription = "决策计算完毕";
                    baseRes.Result = jsonService.SerializeObject(reportScore);
                }
                else
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "无征信";
                }
                #endregion

            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(queryReq.IdentityCard + ",决策异常；", e);
                baseRes.StatusDescription = e.Message;
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
            }
            baseRes.EndTime = DateTime.Now.ToString();

            Log4netAdapter.WriteInfo(queryReq.IdentityCard + "接口：CreditScoreCalculate,调用结束");
            return baseRes;
        }


        public BaseRes CreditScoreCalculateCBS201512ForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            CreditReq queryReq = reqText.DeserializeXML<CreditReq>();
            return CreditScoreCalculateCBS201512(queryReq);
        }
        public BaseRes CreditScoreCalculateCBS201512ForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            CreditReq queryReq = jsonService.DeserializeObject<CreditReq>(reqText);
            return CreditScoreCalculateCBS201512(queryReq);
        }
        /// <summary>
        /// 通过模型CBS201512，计算征信评分
        /// </summary>
        /// <param name="queryReq"></param>
        /// <returns></returns>
        public BaseRes CreditScoreCalculateCBS201512(CreditReq queryReq)
        {
            Log4netAdapter.WriteInfo(string.Format("接口：CreditScoreCalculateCBS201512；客户端IP:{0},参数：{1}", ClientIp, jsonService.SerializeObject(queryReq.IdentityCard, true)));

            BaseRes baseRes = new BaseRes();
            DecisionResult decisionResult = new DecisionResult();
            RCWebService.RCWebService rcServcie = new RCWebService.RCWebService();
            List<VBXML> xmllist = new List<VBXML>();
            int score = 0;
            try
            {
                ICRD_HD_REPORT reportService = NetSpiderFactoryManager.GetCRDHDREPORTService();
                CRD_HD_REPORTEntity reportEntity = null;
                if (!queryReq.ReportSn.IsEmpty())
                {
                    reportEntity = reportService.GetByReportSn(queryReq.ReportSn, true);

                    if (reportEntity.BusIdentityCard.IsEmpty() && !queryReq.IdentityCard.IsEmpty())
                    {
                        reportEntity.BusIdentityCard = queryReq.IdentityCard;
                        reportService.Update(reportEntity);
                    }

                }
                else if (queryReq.ReportSn.IsEmpty() && !queryReq.IdentityCard.IsEmpty())
                {
                    reportEntity = reportService.GetByIdentityCardAndBusType(queryReq.IdentityCard, queryReq.BusType);

                }
                else if (queryReq.ReportSn.IsEmpty() && queryReq.IdentityCard.IsEmpty())
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "缺少征信信息查询参数";
                    return baseRes;
                }

                #region 计算征信评分
                if (reportEntity != null)
                {
                    score = reportEntity.Score;
                    //校验人员姓名与身份证与注册信息是否一致
                    if (reportEntity.CertNo.Substring(14) != queryReq.IdentityCard.Substring(14) || reportEntity.Name != queryReq.Name)
                    {
                        Log4netAdapter.WriteInfo("警告：征信姓名、身份证号与注册信息不一致");
                        baseRes.StatusDescription = "注册信息与征信数据不一致";
                        return baseRes;
                    }

                    if (reportEntity.CRD_STAT_LN != null && reportEntity.CRD_STAT_LND != null && reportEntity.CRD_STAT_QR != null)
                    {
                        xmllist.Add(new VBXML { VB_COL = "Education", VB_VALUE = queryReq.Education });//[申请人]学历
                        xmllist.Add(new VBXML { VB_COL = "AGE", VB_VALUE = ChinaIDCard.GetAge(queryReq.IdentityCard).ToString() });//[申请人]借款人年龄
                        xmllist.Add(new VBXML { VB_COL = "L3M_ACCT_QRY_NUM", VB_VALUE = reportEntity.CRD_STAT_QR.L3M_ACCT_QRY_NUM != null ? reportEntity.CRD_STAT_QR.L3M_ACCT_QRY_NUM.ToString() : "0" });//[征信]评分_过去3个月信用卡审批查询次数
                        xmllist.Add(new VBXML { VB_COL = "L3M_LN_QRY_NUM", VB_VALUE = reportEntity.CRD_STAT_QR.L3M_LN_QRY_NUM != null ? reportEntity.CRD_STAT_QR.L3M_LN_QRY_NUM.ToString() : "0" });//[征信]评分_过去3个月贷款审批查询次数
                        xmllist.Add(new VBXML { VB_COL = "CRD_AGE_UNCLS_OLDEST", VB_VALUE = reportEntity.CRD_STAT_LND.CRD_AGE_UNCLS_OLDEST != null ? reportEntity.CRD_STAT_LND.CRD_AGE_UNCLS_OLDEST.ToString() : "0" });//[征信]评分_最早未销户卡龄
                        xmllist.Add(new VBXML { VB_COL = "L12M_LOANACT_USED_MAX_R", VB_VALUE = reportEntity.CRD_STAT_LND.L12M_LOANACT_USED_MAX_R != null ? reportEntity.CRD_STAT_LND.L12M_LOANACT_USED_MAX_R.ToString() : "0" });//[征信]评分_过去12个月开户的贷款账户最大单笔贷款余额过去12个月开户平均额度之比
                        xmllist.Add(new VBXML { VB_COL = "L24M_OPE_NORM_ACCT_PCT", VB_VALUE = reportEntity.CRD_STAT_LN.L24M_OPE_NORM_ACCT_PCT != null ? reportEntity.CRD_STAT_LN.L24M_OPE_NORM_ACCT_PCT.ToString() : "0" });//[征信]评分_过去24个月内非共享信用卡账户开户数占当前正常账户比例
                        xmllist.Add(new VBXML { VB_COL = "NORM_CDT_BAL_USED_PCT_AVG", VB_VALUE = reportEntity.CRD_STAT_LND.NORM_CDT_BAL_USED_PCT_AVG != null ? reportEntity.CRD_STAT_LND.NORM_CDT_BAL_USED_PCT_AVG.ToString() : "0" });//[征信]评分_当前正常的信用卡账户最大负债额与透支余额之比的均值

                        Log4netAdapter.WriteInfo(jsonService.SerializeObject(xmllist));

                        string rc = rcServcie.GetRuleResultByCustomWithIdentityNoCallFrom(queryReq.BusType, queryReq.IdentityCard, queryReq.Name, "网络版通用征信评分CBS201512", SerializationHelper.SerializeToXml(xmllist));
                        decisionResult = rc.DeserializeXML<List<DecisionResult>>().FirstOrDefault();

                        if (decisionResult != null)
                        {
                            if (decisionResult.RuleResultCanShowSets.ContainsKey("*网络版通用征信评分-CBS201512"))
                            {
                                score = decisionResult.RuleResultCanShowSets["*网络版通用征信评分-CBS201512"].ToInt(0);
                            }
                        }
                        reportEntity.Score = score;
                        reportService.Update(reportEntity);
                    }

                    var reportScore = new
                    {
                        ReportId = reportEntity.Id,
                        ReportSn = reportEntity.ReportSn,
                        ReportTime = reportEntity.ReportCreateTime,
                        Score = score
                    };
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                    baseRes.StatusDescription = "决策计算完毕";
                    baseRes.Result = jsonService.SerializeObject(reportScore);
                }
                else
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "无征信";
                }
                #endregion

            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(queryReq.IdentityCard + ",决策异常；", e);
                baseRes.StatusDescription = e.Message;
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
            }
            baseRes.EndTime = DateTime.Now.ToString();

            Log4netAdapter.WriteInfo(queryReq.IdentityCard + "接口：CreditScoreCalculate,调用结束");
            return baseRes;
        }

        public BaseRes CreditScoreCalculateCBS201603ForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            CreditReq queryReq = reqText.DeserializeXML<CreditReq>();
            return CreditScoreCalculateCBS201603(queryReq);
        }
        public BaseRes CreditScoreCalculateCBS201603ForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            CreditReq queryReq = jsonService.DeserializeObject<CreditReq>(reqText);
            return CreditScoreCalculateCBS201603(queryReq);
        }
        /// <summary>
        /// 通过模型CBS201603，计算征信评分
        /// </summary>
        /// <param name="queryReq"></param>
        /// <returns></returns>
        public BaseRes CreditScoreCalculateCBS201603(CreditReq queryReq)
        {
            Log4netAdapter.WriteInfo(string.Format("接口：CreditScoreCalculateCBS201603；客户端IP:{0},参数：{1}", ClientIp, jsonService.SerializeObject(queryReq.IdentityCard, true)));

            BaseRes baseRes = new BaseRes();
            DecisionResult decisionResult = new DecisionResult();
            RCWebService.RCWebService rcServcie = new RCWebService.RCWebService();
            List<VBXML> xmllist = new List<VBXML>();
            int score = 0;
            try
            {
                ICRD_HD_REPORT reportService = NetSpiderFactoryManager.GetCRDHDREPORTService();
                CRD_HD_REPORTEntity reportEntity = null;
                if (!queryReq.ReportSn.IsEmpty())
                {
                    reportEntity = reportService.GetByReportSn(queryReq.ReportSn, true);

                    if (reportEntity.BusIdentityCard.IsEmpty() && !queryReq.IdentityCard.IsEmpty())
                    {
                        reportEntity.BusIdentityCard = queryReq.IdentityCard;
                        reportService.Update(reportEntity);
                    }

                }
                else if (queryReq.ReportSn.IsEmpty() && !queryReq.IdentityCard.IsEmpty())
                {
                    reportEntity = reportService.GetByIdentityCardAndBusType(queryReq.IdentityCard, queryReq.BusType);

                }
                else if (queryReq.ReportSn.IsEmpty() && queryReq.IdentityCard.IsEmpty())
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "缺少征信信息查询参数";
                    return baseRes;
                }

                #region 计算征信评分
                if (reportEntity != null)
                {
                    score = reportEntity.Score;
                    //校验人员姓名与身份证与注册信息是否一致
                    if (reportEntity.CertNo.Substring(14) != queryReq.IdentityCard.Substring(14) || reportEntity.Name != queryReq.Name)
                    {
                        Log4netAdapter.WriteInfo("警告：征信姓名、身份证号与注册信息不一致");
                        baseRes.StatusDescription = "注册信息与征信数据不一致";
                        return baseRes;
                    }

                    if (reportEntity.CRD_STAT_LN != null && reportEntity.CRD_STAT_LND != null && reportEntity.CRD_STAT_QR != null)
                    {
                        xmllist = new List<VBXML>();
                        //have _160426
                        xmllist.Add(new VBXML { VB_COL = "NON_BNK_LN_QRY_CNT_160426", VB_VALUE = reportEntity.CRD_STAT_QR.NON_BNK_LN_QRY_CNT.ToString() });//[征信]非银行机构的贷款审批查询次数_160426 
                        xmllist.Add(new VBXML { VB_COL = "LST_LOAN_OPE_CNT_160426", VB_VALUE = reportEntity.CRD_STAT_LN.LST_LOAN_OPE_CNT.ToString() });//[征信]过去贷款开户数_160426
                        xmllist.Add(new VBXML { VB_COL = "LST_LOAN_CLS_CNT_160426", VB_VALUE = reportEntity.CRD_STAT_LN.LST_LOAN_CLS_CNT.ToString() });//[征信]过去贷款结清数_160426
                        xmllist.Add(new VBXML { VB_COL = "DLQ_5YR_CNT_MAX_160426", VB_VALUE = reportEntity.CRD_STAT_LND.DLQ_5YR_CNT_MAX.ToString() });//[征信]五年内最大逾期次数_160426
                        xmllist.Add(new VBXML { VB_COL = "ACCT_NUM_160426", VB_VALUE = reportEntity.CRD_STAT_LN.ACCT_NUM.ToString() });//[征信]银行信贷涉及的账户数_160426

                        xmllist.Add(new VBXML { VB_COL = "MAR_EDU", VB_VALUE = queryReq.Marriage + queryReq.Education });//[卡卡贷]婚姻状况教育程度
                        xmllist.Add(new VBXML { VB_COL = "HOS_GEN", VB_VALUE = queryReq.IsLocal + ChinaIDCard.GetSex(queryReq.IdentityCard) });//[卡卡贷]是否本地籍性别

                        //have _160426
                        xmllist.Add(new VBXML { VB_COL = "UNCLS_OLD_CDT_LMT_160426", VB_VALUE = reportEntity.CRD_STAT_LND.UNCLS_OLD_CDT_LMT.ToString() });//[征信]最早未销户贷记卡额度_160426 
                        xmllist.Add(new VBXML { VB_COL = "UNCLS_RCNT_CDT_LMT_160426", VB_VALUE = reportEntity.CRD_STAT_LND.UNCLS_RCNT_CDT_LMT.ToString() }); //[征信]最近未销户贷记卡额度_160426
                        xmllist.Add(new VBXML { VB_COL = "RCNT_CDT_LMT_160426", VB_VALUE = reportEntity.CRD_STAT_LND.RCNT_CDT_LMT.ToString() });//[征信]最近正常贷记卡授信额度_160426

                        xmllist.Add(new VBXML { VB_COL = "CRD_AGE_UNCLS_OLDEST", VB_VALUE = reportEntity.CRD_STAT_LND.CRD_AGE_UNCLS_OLDEST.ToString() });//[征信]评分_最早未销户卡龄
                        xmllist.Add(new VBXML { VB_COL = "L3M_LN_QRY_NUM", VB_VALUE = reportEntity.CRD_STAT_QR.L3M_LN_QRY_NUM.ToString() });//[征信]评分_过去3个月贷款审批查询次数
                        xmllist.Add(new VBXML { VB_COL = "NORM_CDT_BAL_USED_PCT_AVG", VB_VALUE = reportEntity.CRD_STAT_LND.NORM_CDT_BAL_USED_PCT_AVG.ToString() });//[征信]评分_当前正常的信用卡账户最大负债额与透支余额之比的均值

                        Log4netAdapter.WriteInfo(jsonService.SerializeObject(xmllist));

                        string rc = rcServcie.GetRuleResultByCustomWithIdentityNoCallFrom(queryReq.BusType, queryReq.IdentityCard, queryReq.Name, "卡卡贷网络版征信评分-201603", SerializationHelper.SerializeToXml(xmllist));
                        decisionResult = rc.DeserializeXML<List<DecisionResult>>().FirstOrDefault();

                        if (decisionResult != null)
                        {
                            if (decisionResult.RuleResultCanShowSets.ContainsKey("*[评分_卡卡贷征信评分201603]卡卡贷征信评分201603"))
                            {
                                score = decisionResult.RuleResultCanShowSets["*[评分_卡卡贷征信评分201603]卡卡贷征信评分201603"].ToInt(0);
                            }
                        }
                        reportEntity.Score = score;
                        reportService.Update(reportEntity);
                    }
                    var reportScore = new
                    {
                        ReportId = reportEntity.Id,
                        ReportSn = reportEntity.ReportSn,
                        ReportTime = reportEntity.ReportCreateTime,
                        Score = score
                    };
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                    baseRes.StatusDescription = "决策计算完毕";
                    baseRes.Result = jsonService.SerializeObject(reportScore);
                }
                else
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "无征信";
                }
                #endregion

            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(queryReq.IdentityCard + ",决策异常；", e);
                baseRes.StatusDescription = e.Message;
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
            }
            baseRes.EndTime = DateTime.Now.ToString();

            Log4netAdapter.WriteInfo(queryReq.IdentityCard + "接口：CreditScoreCalculateCBS160426,调用结束");
            return baseRes;
        }

        #endregion

        #region 征信统计信息查询
        public BaseRes GetCreditSummaryForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            CreditReq queryReq = reqText.DeserializeXML<CreditReq>();
            return GetCreditSummary(queryReq);
        }
        public BaseRes GetCreditSummaryForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            CreditReq queryReq = jsonService.DeserializeObject<CreditReq>(reqText);
            return GetCreditSummary(queryReq);
        }
        public BaseRes GetCreditSummary(CreditReq queryReq)
        {
            Log4netAdapter.WriteInfo(string.Format("接口：GetCreditSummary；客户端IP:{0},参数：{1}", ClientIp, jsonService.SerializeObject(queryReq)));

            BaseRes baseRes = new BaseRes();
            try
            {
                ICRD_HD_REPORT reportService = NetSpiderFactoryManager.GetCRDHDREPORTService();
                ICRD_CD_LN lnService = NetSpiderFactoryManager.GetCRDCDLNService();
                CRD_HD_REPORTEntity reportEntity = null;
                if (!queryReq.ReportSn.IsEmpty())
                {
                    reportEntity = reportService.GetByReportSn(queryReq.ReportSn, true);
                }
                else if (queryReq.ReportSn.IsEmpty() && !queryReq.IdentityCard.IsEmpty())
                {
                    reportEntity = reportService.GetByIdentityCardAndBusType(queryReq.IdentityCard, queryReq.BusType);

                }
                else if (queryReq.ReportSn.IsEmpty() && queryReq.IdentityCard.IsEmpty())
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "征信数据查询失败，没有输入参数";
                    return baseRes;
                }

                if (reportEntity != null)
                {
                    if (reportEntity.BusIdentityCard.IsEmpty() && !queryReq.IdentityCard.IsEmpty())
                    {
                        reportEntity.BusIdentityCard = queryReq.IdentityCard;
                        reportService.Update(reportEntity);
                    }

                    reportEntity.Loginname = null;
                    reportEntity.Password = null;
                    reportEntity.CRD_CD_GUARANTEEList = null;
                    reportEntity.CRD_CD_LNDList = null;
                    reportEntity.CRD_CD_LNList = null;
                    reportEntity.CRD_CD_STNCARDList = null;
                    reportEntity.CRD_QR_RECORDDTLList = null;

                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                    baseRes.StatusDescription = "征信数据查询完毕";
                    baseRes.Result = jsonService.SerializeObject(reportEntity, true);
                }
                else
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "无数据";
                }



            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(queryReq.IdentityCard + ",征信数据查询异常；", e);
                baseRes.StatusDescription = e.Message;
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
            }
            baseRes.EndTime = DateTime.Now.ToString();

            Log4netAdapter.WriteInfo(queryReq.IdentityCard + "接口：GetCreditSummary,调用结束");
            return baseRes;
        }



        public BaseRes GetCreditDataAllForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            CreditReq queryReq = jsonService.DeserializeObject<CreditReq>(reqText);
            Log4netAdapter.WriteInfo(string.Format("接口：GetCreditDataAllForJson；客户端IP:{0},参数：{1}", ClientIp, jsonService.SerializeObject(queryReq)));

            BaseRes baseRes = new BaseRes();
            try
            {
                ICRD_HD_REPORT reportService = NetSpiderFactoryManager.GetCRDHDREPORTService();
                ICRD_CD_LN lnService = NetSpiderFactoryManager.GetCRDCDLNService();
                CRD_HD_REPORTEntity reportEntity = null;

                if (!queryReq.IdentityCard.IsEmpty() || !queryReq.ReportSn.IsEmpty())
                {
                    reportEntity = reportService.GetDataAllByIdentityCardAndBusType(queryReq.IdentityCard, queryReq.ReportSn, queryReq.BusType);
                }
                else
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "征信数据查询失败，没有输入参数";
                    return baseRes;
                }

                if (reportEntity != null)
                {

                    reportEntity.Loginname = null;
                    reportEntity.Password = null;
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                    baseRes.StatusDescription = "征信数据查询完毕";
                    baseRes.Result = jsonService.SerializeObject(reportEntity, true);
                }
                else
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "无数据";
                }



            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(queryReq.IdentityCard + ",征信数据查询异常；", e);
                baseRes.StatusDescription = e.Message;
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
            }
            baseRes.EndTime = DateTime.Now.ToString();

            Log4netAdapter.WriteInfo(queryReq.IdentityCard + "接口：GetCreditSummary,调用结束");
            return baseRes;
        }
        #endregion

        #region 根据报告编号
        /// <summary>
        /// 根据报告编号,获取对象
        /// </summary>
        /// <param name="reportsn"></param>
        /// <returns></returns>
        public BaseRes GetCrdhdReportByReportSnForJson(string reportsn)
        {
            return GetReportByReportSn(reportsn);
        }
        /// <summary>
        /// 根据报告编号,获取对象
        /// </summary>
        /// <param name="reportsn"></param>
        /// <returns></returns>
        public BaseRes GetCrdhdReportByReportSnForXml(string reportsn)
        {
            return GetReportByReportSn(reportsn);
        }


        public BaseRes GetReportByReportSn(string reportsn)
        {
            Log4netAdapter.WriteInfo("接口名：GetCrdhdReportByReportSnForJson；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + reportsn);
            BaseRes baseRes = new BaseRes();
            try
            {
                ICRD_HD_REPORT reportService = NetSpiderFactoryManager.GetCRDHDREPORTService();
                var Report = reportService.GetByReportSn(reportsn);
                if (Report == null)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "无数据";
                    return baseRes;
                }
                baseRes.Result = jsonService.SerializeObject(Report, true);
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "征信数据查询完毕";

            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(reportsn + "根据报告编号,获取对象异常", e);
                baseRes.StatusDescription = e.Message;
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
            }
            return baseRes;

        }
        #endregion




    }
}