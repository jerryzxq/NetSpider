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
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using System.Dynamic;
namespace Vcredit.WeiXin.RestService.Services
{
    //Asp.net管道兼容
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class KKDService : IKKDService
    {
        #region 声明变量、接口
        IPbccrcExecutor pbccrcExecutor = ExecutorManager.GetPbccrcExecutor();//社保数据采集接口
        IProvidentFundExecutor profundExecutor = ExecutorManager.GetProvidentFundExecutor();//社保数据采集接口
        ISociaSecurityExecutor socialsecExecutor = ExecutorManager.GetSociaSecurityExecutor();//社保数据采集接口

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
        public KKDService()
        {
        }
        #endregion

        #region 央行互联网征信查询

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
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：PbccrcReportLogin；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);
            Log4netAdapter.WriteInfo(guid + "参数：" + jsonService.SerializeObject(login, true));
            BaseRes baseRes = new BaseRes();
            try
            {
                string token = login.Token;
                baseRes = pbccrcExecutor.Login(token, login.Username, login.Password, login.Vercode);
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(guid + "登录异常", e);
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
        public BaseRes GetPbccrcReportInfoForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            WechatQueryReq queryReq = reqText.DeserializeXML<WechatQueryReq>();
            return GetPbccrcReportInfo(queryReq);
        }
        /// <summary>
        /// 央行互联网征信报告查询（JSON）
        /// </summary>
        /// <param name="stream">post数据</param>
        /// <returns></returns>
        public BaseRes GetPbccrcReportForJson(Stream stream)
        {

            //获得请求内容
            string reqText = stream.AsStringText(true);
            WechatQueryReq queryReq = jsonService.DeserializeObject<WechatQueryReq>(reqText);
            return GetPbccrcReportInfo(queryReq);
        }
        /// <summary>
        /// 央行互联网征信报告查询
        /// </summary>
        /// <param name="queryReq">PbccrcReportQueryReq实体</param>
        /// <returns></returns>
        public BaseRes GetPbccrcReportInfo(WechatQueryReq queryReq)
        {
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：GetPbccrcReportInfo；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);
            Log4netAdapter.WriteInfo(guid + "参数：" + jsonService.SerializeObject(queryReq, true));

            string token = queryReq.Token;
            BaseRes baseRes = new BaseRes();
            CRD_HD_REPORTRes queryRes = new CRD_HD_REPORTRes();
            //int score = 0;
            //int IsEqual = 0;
            try
            {
                PbccrcReportQueryReq spiderReq = new PbccrcReportQueryReq();
                spiderReq.Token = queryReq.Token;
                spiderReq.BusType = ServiceConstants.BusType_Kakadai;
                spiderReq.BusId = queryReq.OrderId.ToString();
                spiderReq.querycode = queryReq.Querycode;
                spiderReq.IdentityCard = queryReq.Identitycard;
                queryRes = pbccrcExecutor.GetReport(spiderReq);


                //校验人员姓名与身份证与注册信息是否一致
                var report = new
                {
                    ReportCreateTime = queryRes.ReportCreateTime,
                    ReportSn = queryRes.ReportSn,
                    Name = queryRes.Name,
                    IdCard = queryRes.CertNo
                };
                baseRes.Result = jsonService.SerializeObject(report);
                baseRes.StatusCode = queryRes.StatusCode;
                baseRes.StatusDescription = queryRes.StatusDescription;
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(guid + "征信信息采集异常", e);
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
            Log4netAdapter.WriteInfo("接口名：PbccrcReportRegisterStep2；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(registerReq, true));
            string token = registerReq.Token;
            return pbccrcExecutor.Register_Step3(token, registerReq.Username, registerReq.Password, registerReq.confirmpassword, registerReq.email, registerReq.mobileTel, registerReq.Smscode);
        }
        #endregion

        #region 查询申请
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

        #region 发送查询码
        public BaseRes GetPbccrcReportSendQuerycode(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                PbccrcReportQueryApplyReq applyReq = jsonService.DeserializeObject<PbccrcReportQueryApplyReq>(reqText);

                return pbccrcExecutor.QueryReport_SendQueryCode(applyReq.Token);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        #endregion
        #endregion

        #region 决策系统对接
        #region 第一次授信
        public BaseRes GetRCCreditForXml(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                WechatQueryReq Req = reqText.DeserializeXML<WechatQueryReq>();

                return GetRCCredit(Req);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public BaseRes GetRCCreditForJson(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                WechatQueryReq Req = jsonService.DeserializeObject<WechatQueryReq>(reqText);

                return GetRCCredit(Req);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public BaseRes GetRCCredit(WechatQueryReq Req)
        {
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：GetRCCredit；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);

            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(Req, true));

            BaseRes baseRes = new BaseRes();
            RCResult rcResult = new RCResult();
            DecisionResult decisionResult = new DecisionResult();
            RCWebService.RCWebService rcServcie = new RCWebService.RCWebService();
            List<VBXML> xmllist = new List<VBXML>();
            SummaryEntity summaryEntity = null;
            int useMonths = 0;
            string rc = string.Empty;
            try
            {
                ICRD_HD_REPORT reportService = NetSpiderFactoryManager.GetCRDHDREPORTService();
                ISummary summaryService = NetSpiderFactoryManager.GetSummaryService();
                ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
                NetCreditOpr netCreditOpr = new NetCreditOpr();

                CRD_HD_REPORTEntity reportEntity = netCreditOpr.GetSummaryByIdentityCard(Req.Identitycard);
                summaryEntity = GetMobileSummary(Req.Identitycard, Req.Mobile);
                string ThreeMonthCallRecordAmount = "0";
                string SixMonthCallRecordAmount = "0";
                string ThreeMonthCallRecordCount = "0";
                string SixMonthCallRecordCount = "0";
                string CALLED_TIMES_IN15DAY = "0";
                string CALLED_TIMES_IN30DAY = "0";
                string CALLED_TIMES_IN15DAY_GRAY = "0";
                string CALLED_TIMES_IN30DAY_GRAY = "0";

                if (summaryEntity != null)
                {
                    ThreeMonthCallRecordAmount = summaryEntity.ThreeMonthCallRecordAmount.ToString();
                    SixMonthCallRecordAmount = summaryEntity.SixMonthCallRecordAmount.ToString();
                    ThreeMonthCallRecordCount = summaryEntity.ThreeMonthCallRecordCount.ToString();
                    SixMonthCallRecordCount = summaryEntity.SixMonthCallRecordCount.ToString();
                    CALLED_TIMES_IN15DAY = summaryEntity.CALLED_TIMES_IN15DAY.ToString();
                    CALLED_TIMES_IN30DAY = summaryEntity.CALLED_TIMES_IN30DAY.ToString();
                    CALLED_TIMES_IN15DAY_GRAY = summaryEntity.CALLED_TIMES_IN15DAY_Gray != null ? summaryEntity.CALLED_TIMES_IN15DAY_Gray.ToString() : "0";
                    CALLED_TIMES_IN30DAY_GRAY = summaryEntity.CALLED_TIMES_IN30DAY_Gray != null ? summaryEntity.CALLED_TIMES_IN30DAY_Gray.ToString() : "0";

                    if (!summaryEntity.Regdate.IsEmpty())
                    {
                        useMonths = CommonFun.GetIntervalOf2DateTime(DateTime.Now, DateTime.Parse(summaryEntity.Regdate), "M");
                    }
                }

                #region 数据校验
                if (summaryEntity == null)
                {
                    baseRes.StatusDescription = "无手机账单数据";
                    return baseRes;
                }
                if (reportEntity == null)
                {
                    baseRes.StatusDescription = "无征信数据";
                    return baseRes;
                }
                //校验人员姓名与身份证与注册信息是否一致
                if (reportEntity.CertNo.Substring(14) != Req.Identitycard.Substring(14) || reportEntity.Name != Req.Name)
                {
                    Log4netAdapter.WriteInfo("警告：征信姓名、身份证号与注册信息不一致");
                    baseRes.StatusDescription = "注册信息与征信数据不一致";
                    return baseRes;
                }
                #endregion


                #region 调用决策系统
                xmllist.Clear();
                xmllist.Add(new VBXML { VB_COL = "IF_PAY_SOCIALALLOWANCE", VB_VALUE = Req.PayHousingFund });//是否缴纳公积金
                xmllist.Add(new VBXML { VB_COL = "SOCIALSECURITY_AGE", VB_VALUE = ChinaIDCard.GetAge(Req.Identitycard).ToString() });//[卡卡贷]年龄
                xmllist.Add(new VBXML { VB_COL = "CREDIT_grade", VB_VALUE = Req.CreditScore.ToString() });//[网络版征信]评分卡分数
                xmllist.Add(new VBXML { VB_COL = "KKD_ChannelSource", VB_VALUE = Req.ChannelSource });//渠道
                xmllist.Add(new VBXML { VB_COL = "KKD_Region", VB_VALUE = Req.Region });//地区
                xmllist.Add(new VBXML { VB_COL = "Mobile_Score", VB_VALUE = Req.MobileScore.ToString() });//手机评分
                xmllist.Add(new VBXML { VB_COL = "KKD_Mobile_Score_201609", VB_VALUE = Req.MobileScore2.ToString() });//手机评分
                xmllist.Add(new VBXML { VB_COL = "BaiRong_Score_Online", VB_VALUE = Req.BaiRongScore.ToString() });//百融评分
                xmllist.Add(new VBXML { VB_COL = "SEX", VB_VALUE = ChinaIDCard.GetSex(Req.Identitycard) });//百融评分

                xmllist.Add(new VBXML { VB_COL = "CALLED_TIMES_IN15DAY", VB_VALUE = CALLED_TIMES_IN15DAY });//[手机]15天内危险号码被叫次数
                xmllist.Add(new VBXML { VB_COL = "CALLED_TIMES_IN30DAY", VB_VALUE = CALLED_TIMES_IN30DAY });//[手机]30天内危险号码被叫次数
                xmllist.Add(new VBXML { VB_COL = "CALLED_TIMES_IN15DAY_GRAY", VB_VALUE = CALLED_TIMES_IN15DAY_GRAY });//[手机]15天内危险号码被叫次数
                xmllist.Add(new VBXML { VB_COL = "CALLED_TIMES_IN30DAY_GRAY", VB_VALUE = CALLED_TIMES_IN30DAY_GRAY });//[手机]30天内危险号码被叫次数

                #region 央行征信参数

                if (reportEntity == null)
                {
                    reportEntity = new CRD_HD_REPORTEntity();
                    reportEntity.CRD_STAT_LN = new CRD_STAT_LNEntity();
                    reportEntity.CRD_STAT_LND = new CRD_STAT_LNDEntity();
                    reportEntity.CRD_STAT_QR = new CRD_STAT_QREntity();
                }
                xmllist.Add(new VBXML { VB_COL = "NC_M3_ALL_CNT_TOTAL", VB_VALUE = reportEntity.CRD_STAT_QR.M3ALLCNTTOTAL != null ? reportEntity.CRD_STAT_QR.M3ALLCNTTOTAL.ToString() : "0" });//[网络版征信]三个月内所有征信查询次数
                xmllist.Add(new VBXML { VB_COL = "ALL_LOAN_HOUSE_DELAY90_CNT", VB_VALUE = reportEntity.CRD_STAT_LN.ALLLOANHOUSEDELAY90CNT != null ? reportEntity.CRD_STAT_LN.ALLLOANHOUSEDELAY90CNT.ToString() : "0" });//[网络版征信]住房贷款发生过90天以上逾期的账户数(all)
                xmllist.Add(new VBXML { VB_COL = "ALL_LOAN_OTHER_DELAY90_CNT", VB_VALUE = reportEntity.CRD_STAT_LN.ALLLOANOTHERDELAY90CNT != null ? reportEntity.CRD_STAT_LN.ALLLOANOTHERDELAY90CNT.ToString() : "0" });//[网络版征信]其他贷款发生过90天以上逾期的账户数(all)
                xmllist.Add(new VBXML { VB_COL = "ALL_CREDIT_DELAY90_CNT", VB_VALUE = reportEntity.CRD_STAT_LND.ALLCREDITDELAY90CNT != null ? reportEntity.CRD_STAT_LND.ALLCREDITDELAY90CNT.ToString() : "0" });//[网络版征信]信用卡发生过90天以上逾期的账户数(all)
                xmllist.Add(new VBXML { VB_COL = "credit_dlq_amount", VB_VALUE = reportEntity.CRD_STAT_LND.CREDITDLQAMOUNT != null ? reportEntity.CRD_STAT_LND.CREDITDLQAMOUNT.ToString() : "0" });//[网络版征信]信用卡逾期金额
                xmllist.Add(new VBXML { VB_COL = "loan_dlq_amount", VB_VALUE = reportEntity.CRD_STAT_LN.LOANDLQAMOUNT != null ? reportEntity.CRD_STAT_LN.LOANDLQAMOUNT.ToString() : "0" });//[网络版征信]贷款逾期金额
                xmllist.Add(new VBXML { VB_COL = "ALL_LOAN_HOUSE_UNCLOSEACCOUNT_CNT", VB_VALUE = reportEntity.CRD_STAT_LN.ALLLOANHOUSEUNCLOSEDCNT != null ? reportEntity.CRD_STAT_LN.ALLLOANHOUSEUNCLOSEDCNT.ToString() : "0" });//[网络版征信]住房贷款未销户账户数(all)
                xmllist.Add(new VBXML { VB_COL = "ALL_LOAN_OTHER_UNCLOSED_CNT", VB_VALUE = reportEntity.CRD_STAT_LN.ALLLOANOTHERUNCLOSEDCNT != null ? reportEntity.CRD_STAT_LN.ALLLOANOTHERUNCLOSEDCNT.ToString() : "0" });//[网络版征信]其他贷款未销户账户数(all)
                xmllist.Add(new VBXML { VB_COL = "ALL_CREDIT_UNCLOSED_CNT", VB_VALUE = reportEntity.CRD_STAT_LND.ALLCREDITUNCLOSEDCNT != null ? reportEntity.CRD_STAT_LND.ALLCREDITUNCLOSEDCNT.ToString() : "0" });//[网络版征信]信用卡未销户
                xmllist.Add(new VBXML { VB_COL = "CREDIT_LIMIT_AMOUNT_NORM_MAX", VB_VALUE = reportEntity.CRD_STAT_LND.CREDITLIMITAMOUNTNORMMAX != null ? reportEntity.CRD_STAT_LND.CREDITLIMITAMOUNTNORMMAX.ToString() : "0" });//[网络版征信]正常信用卡最大额度
                xmllist.Add(new VBXML { VB_COL = "NC_NORMAL_USED_RATE", VB_VALUE = reportEntity.CRD_STAT_LND.NORMALUSEDRATE != null ? reportEntity.CRD_STAT_LND.NORMALUSEDRATE.ToString() : "0" });//[网络版征信]正常信用卡使用率
                xmllist.Add(new VBXML { VB_COL = "SUM_USED_CREDIT_LIMIT_AMOUNT", VB_VALUE = reportEntity.CRD_STAT_LND.SUMUSEDCREDITLIMITAMOUNT != null ? reportEntity.CRD_STAT_LND.SUMUSEDCREDITLIMITAMOUNT.ToString() : "0" });//[网络版征信]信用卡已使用总额度
                xmllist.Add(new VBXML { VB_COL = "NORMAL_CARD_AGE_MONTH", VB_VALUE = reportEntity.CRD_STAT_LND.NORMALCARDAGEMONTH != null ? reportEntity.CRD_STAT_LND.NORMALCARDAGEMONTH.ToString() : "0" });//[网络版征信]正常状态最早卡龄
                xmllist.Add(new VBXML { VB_COL = "Normal_Used_Credit_Limit_Amount", VB_VALUE = reportEntity.CRD_STAT_LND.NORMALUSEDMAX != null ? reportEntity.CRD_STAT_LND.NORMALUSEDMAX.ToString() : "0" });//[网络版征信]正常信用卡中的最大已使用额度
                xmllist.Add(new VBXML { VB_COL = "SUM_Loan_Amount", VB_VALUE = reportEntity.CRD_STAT_LN.SUMLOANLIMITAMOUNT != null ? reportEntity.CRD_STAT_LN.SUMLOANLIMITAMOUNT.ToString() : "0" });//[网络版征信]所有贷款的贷款本金总额
                xmllist.Add(new VBXML { VB_COL = "lnd_max_overdue_percent", VB_VALUE = reportEntity.CRD_STAT_LND.lnd_max_overdue_percent != null ? reportEntity.CRD_STAT_LND.lnd_max_overdue_percent.ToString() : "0" });//[网络版征信]所有贷款的贷款本金总额
                xmllist.Add(new VBXML { VB_COL = "COUNT_Nonbank_IN3M", VB_VALUE = reportEntity.CRD_STAT_QR.COUNT_Nonbank_IN3M != null ? reportEntity.CRD_STAT_QR.COUNT_Nonbank_IN3M.ToString() : "0" });//[征信]3个月内非银机构查询次数

                #endregion

                #region 手机账单
                xmllist.Add(new VBXML { VB_COL = "MOBILE_USED_IS3MTH", VB_VALUE = useMonths >= 3 ? "是" : "否" });//[手机]最近3个月消费金额
                xmllist.Add(new VBXML { VB_COL = "MOBILE_USED_IS6MTH", VB_VALUE = useMonths >= 6 ? "是" : "否" });//[手机]最近6个月消费金额
                xmllist.Add(new VBXML { VB_COL = "MOBILE_CONSUM_3MTH", VB_VALUE = ThreeMonthCallRecordAmount });//[手机]最近3个月消费金额
                xmllist.Add(new VBXML { VB_COL = "MOBILE_CONSUM_6MTH", VB_VALUE = SixMonthCallRecordAmount });//[手机]最近6个月消费金额
                xmllist.Add(new VBXML { VB_COL = "MOBILE_CALLCOUNT_3MTH", VB_VALUE = ThreeMonthCallRecordCount });//[手机]最近3个月通话次数
                xmllist.Add(new VBXML { VB_COL = "MOBILE_CALLCOUNT_6MTH", VB_VALUE = SixMonthCallRecordCount });//[手机]最近6个月通话次数
                xmllist.Add(new VBXML { VB_COL = "MOBILE_USED_MONTHS", VB_VALUE = useMonths.ToString() });//[手机]手机使用年限

                //if (summaryEntity != null)
                //{
                //    xmllist.Add(new VBXML { VB_COL = "MOBILE_USED_IS3MTH", VB_VALUE = useMonths >= 3 ? "是" : "否" });//[手机]是否使用三个月
                //    xmllist.Add(new VBXML { VB_COL = "MOBILE_USED_IS6MTH", VB_VALUE = useMonths >= 6 ? "是" : "否" });//[手机]是否使用六个月
                //    xmllist.Add(new VBXML { VB_COL = "MOBILE_CONSUM_3MTH", VB_VALUE = summaryEntity.ThreeMonthCallRecordAmount.ToString() });//[手机]最近3个月消费金额
                //    xmllist.Add(new VBXML { VB_COL = "MOBILE_CONSUM_6MTH", VB_VALUE = summaryEntity.SixMonthCallRecordAmount.ToString() });//[手机]最近6个月消费金额
                //    xmllist.Add(new VBXML { VB_COL = "MOBILE_CALLCOUNT_3MTH", VB_VALUE = summaryEntity.ThreeMonthCallRecordCount.ToString() });//[手机]最近3个月通话次数
                //    xmllist.Add(new VBXML { VB_COL = "MOBILE_CALLCOUNT_6MTH", VB_VALUE = summaryEntity.SixMonthCallRecordCount.ToString() });//[手机]最近6个月通话次数
                //}
                //else
                //{
                //    xmllist.Add(new VBXML { VB_COL = "MOBILE_USED_IS3MTH", VB_VALUE = "否" });//[手机]最近3个月消费金额
                //    xmllist.Add(new VBXML { VB_COL = "MOBILE_USED_IS6MTH", VB_VALUE = "否" });//[手机]最近6个月消费金额
                //    xmllist.Add(new VBXML { VB_COL = "MOBILE_CONSUM_3MTH", VB_VALUE = "0" });//[手机]最近3个月消费金额
                //    xmllist.Add(new VBXML { VB_COL = "MOBILE_CONSUM_6MTH", VB_VALUE = "0" });//[手机]最近6个月消费金额
                //    xmllist.Add(new VBXML { VB_COL = "MOBILE_CALLCOUNT_3MTH", VB_VALUE = "0" });//[手机]最近3个月通话次数
                //    xmllist.Add(new VBXML { VB_COL = "MOBILE_CALLCOUNT_6MTH", VB_VALUE = "0" });//[手机]最近6个月通话次数
                //}

                #endregion

                Log4netAdapter.WriteInfo(jsonService.SerializeObject(xmllist));

                string rc1 = string.Empty;
                rc1 = rcServcie.GetRuleResultByCustomWithIdentityNoCallFrom(ServiceConstants.RC_Model_KKD, Req.Identitycard, Req.Name, ServiceConstants.RC_Model_KKD_All, SerializationHelper.SerializeToXml(xmllist));

                decisionResult = rc1.DeserializeXML<List<DecisionResult>>().FirstOrDefault();

                BaseMongo baseMongo = new BaseMongo();
                RCStorage rcStoreage = new RCStorage();
                rcStoreage.Params = xmllist;
                rcStoreage.Result = decisionResult;
                rcStoreage.BusId = Req.Identitycard;
                rcStoreage.BusType = ServiceConstants.RC_Model_KKD_All;
                baseMongo.Insert<RCStorage>(rcStoreage, "KKD_RCStorage" + DateTime.Now.ToString(Consts.DateFormatString7));
                #endregion

                //获取决策结果
                if (decisionResult != null)
                {
                    if (decisionResult.Result == "通过")
                    {
                        //手续费率
                        if (decisionResult.RuleResultCanShowSets.ContainsKey(ServiceConstants.RC_Result_KKD_FormalitiesRate))
                        {
                            rcResult.FormalitiesRate = decisionResult.RuleResultCanShowSets[ServiceConstants.RC_Result_KKD_FormalitiesRate].ToDecimal(0);
                        }
                        //月利率
                        if (decisionResult.RuleResultCanShowSets.ContainsKey(ServiceConstants.RC_Result_KKD_MonthlyInterestRate))
                        {
                            rcResult.MonthlyInterestRate = decisionResult.RuleResultCanShowSets[ServiceConstants.RC_Result_KKD_MonthlyInterestRate].ToDecimal(0);
                        }
                        //月服务费率
                        if (decisionResult.RuleResultCanShowSets.ContainsKey(ServiceConstants.RC_Result_KKD_MonthlyServiceRate))
                        {
                            rcResult.MonthlyServiceRate = decisionResult.RuleResultCanShowSets[ServiceConstants.RC_Result_KKD_MonthlyServiceRate].ToDecimal(0);
                        }
                        //审批金额
                        if (decisionResult.RuleResultCanShowSets.ContainsKey(ServiceConstants.RC_Result_KKD_ExamineLoanMoney))
                        {
                            rcResult.ExamineLoanMoney = decisionResult.RuleResultCanShowSets[ServiceConstants.RC_Result_KKD_ExamineLoanMoney].ToDecimal(0);
                        }
                        //客户分类结果
                        if (decisionResult.RuleResultCanShowSets.ContainsKey("客户分类结果"))
                        {
                            rcResult.CustomerRating = decisionResult.RuleResultCanShowSets["客户分类结果"].ToString();
                        }
                        //最高期数
                        if (decisionResult.RuleResultCanShowSets.ContainsKey(ServiceConstants.RC_Result_KKD_HighestPeriods))
                        {
                            rcResult.HighestPeriods = decisionResult.RuleResultCanShowSets[ServiceConstants.RC_Result_KKD_HighestPeriods].ToInt(0);
                        }
                    }
                    else
                    {
                        List<RCRule> RejectReasons = new List<RCRule>();
                        var RuleResultSets = decisionResult.RuleResultSets;

                        RCRule RCRuleModel = null;
                        foreach (var item in RuleResultSets)
                        {
                            //查询拒绝组拒绝的原因
                            if (item.Value.Result == "1" && item.Value.Rule.RU_GP_TP == 1)
                            {
                                RCRuleModel = new RCRule();
                                RCRuleModel.RuleName = item.Value.Rule.RU_NM;
                                RCRuleModel.RuleDesc = item.Value.Rule.RU_DESC;
                                RejectReasons.Add(RCRuleModel);
                            }
                        }

                        rcResult.RejectReasons = RejectReasons;
                    }
                }
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = guid + "决策计算完毕";
                baseRes.Result = jsonService.SerializeObject(rcResult);
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(guid + "决策异常；", e);
                baseRes.StatusDescription = e.Message;
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
            }
            baseRes.EndTime = DateTime.Now.ToString();

            Log4netAdapter.WriteInfo(guid + "接口：GetRCResult,调用结束");
            return baseRes;
        }
        #endregion

        #region 卡卡贷智策版
        public BaseRes GetRCZhiCeForJson(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                WechatQueryReq Req = jsonService.DeserializeObject<WechatQueryReq>(reqText);
                return GetRCZhiCe(Req);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public BaseRes GetRCZhiCeForXml(Stream stream)
        {
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                WechatQueryReq Req = reqText.DeserializeXML<WechatQueryReq>();

                return GetRCZhiCe(Req);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public BaseRes GetRCZhiCe(WechatQueryReq Req)
        {
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：GetRCZhiCe；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);

            Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(Req, true));

            BaseRes baseRes = new BaseRes();
            RCResult rcResult = new RCResult();
            DecisionResult decisionResult = new DecisionResult();
            RCWebService.RCWebService rcServcie = new RCWebService.RCWebService();
            List<VBXML> xmllist = new List<VBXML>();
            int score = 0;
            string rc = string.Empty;
            int mobileScore = 0;
            try
            {
                ICRD_HD_REPORT reportService = NetSpiderFactoryManager.GetCRDHDREPORTService();
                ISummary summaryService = NetSpiderFactoryManager.GetSummaryService();
                ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
                NetCreditOpr netCreditOpr = new NetCreditOpr();

                CRD_HD_REPORTEntity reportEntity = netCreditOpr.GetSummaryByIdentityCard(Req.Identitycard);
                Spd_applyEntity applyEntity = applyService.GetByIdentityCardAndSpiderTypeAndMobile(Req.Identitycard, "mobile", Req.Mobile);
                int useMonths = 0;
                SummaryEntity summaryEntity = null;
                string ThreeMonthCallRecordAmount = "0";
                string SixMonthCallRecordAmount = "0";
                string ThreeMonthCallRecordCount = "0";
                string SixMonthCallRecordCount = "0";
                string CALLED_TIMES_IN15DAY = "0";
                string CALLED_TIMES_IN30DAY = "0";
                string CALLED_TIMES_IN15DAY_GRAY = "0";
                string CALLED_TIMES_IN30DAY_GRAY = "0";


                summaryEntity = GetMobileSummary(Req.Identitycard, Req.Mobile);
                if (summaryEntity != null)
                {
                    ThreeMonthCallRecordAmount = summaryEntity.ThreeMonthCallRecordAmount.ToString();
                    SixMonthCallRecordAmount = summaryEntity.SixMonthCallRecordAmount.ToString();
                    ThreeMonthCallRecordCount = summaryEntity.ThreeMonthCallRecordCount.ToString();
                    SixMonthCallRecordCount = summaryEntity.SixMonthCallRecordCount.ToString();
                    CALLED_TIMES_IN15DAY = summaryEntity.CALLED_TIMES_IN15DAY.ToString();
                    CALLED_TIMES_IN30DAY = summaryEntity.CALLED_TIMES_IN30DAY.ToString();
                    CALLED_TIMES_IN15DAY_GRAY = summaryEntity.CALLED_TIMES_IN15DAY_Gray != null ? summaryEntity.CALLED_TIMES_IN15DAY_Gray.ToString() : "0";
                    CALLED_TIMES_IN30DAY_GRAY = summaryEntity.CALLED_TIMES_IN30DAY_Gray != null ? summaryEntity.CALLED_TIMES_IN30DAY_Gray.ToString() : "0";

                    if (!summaryEntity.Regdate.IsEmpty())
                    {
                        useMonths = CommonFun.GetIntervalOf2DateTime(DateTime.Now, DateTime.Parse(summaryEntity.Regdate), "M");
                    }

                    mobileScore = GetMobileScore(Req, summaryEntity);//手机评分
                }



                #region 调用决策系统
                xmllist.Clear();
                xmllist.Add(new VBXML { VB_COL = "IF_PAY_SOCIALALLOWANCE", VB_VALUE = Req.PayHousingFund });//是否缴纳公积金
                xmllist.Add(new VBXML { VB_COL = "SOCIALSECURITY_AGE", VB_VALUE = ChinaIDCard.GetAge(Req.Identitycard).ToString() });//[卡卡贷]年龄
                xmllist.Add(new VBXML { VB_COL = "UNAD_rskScore", VB_VALUE = Req.UNAD_rskScore });//[智策]风险分
                xmllist.Add(new VBXML { VB_COL = "UNAD_lastmonthAmt", VB_VALUE = Req.UNAD_lastmonthAmt });//[智策]近一个月消费金额
                xmllist.Add(new VBXML { VB_COL = "UNAD_firstTradeDate", VB_VALUE = Req.UNAD_firstTradeDate });//[智策]历史最早交易日期
                xmllist.Add(new VBXML { VB_COL = "UNAD_threeYearsMaxAmt", VB_VALUE = Req.UNAD_threeYearsMaxAmt });//[智策]最近年单笔最大消费金额
                xmllist.Add(new VBXML { VB_COL = "Mobile_Score", VB_VALUE = mobileScore.ToString() });//手机评分
                xmllist.Add(new VBXML { VB_COL = "KKD_Mobile_Score_201609", VB_VALUE = Req.MobileScore2.ToString() });//手机评分
                xmllist.Add(new VBXML { VB_COL = "CALLED_TIMES_IN15DAY", VB_VALUE = CALLED_TIMES_IN15DAY });//[手机]15天内危险号码被叫次数
                xmllist.Add(new VBXML { VB_COL = "CALLED_TIMES_IN30DAY", VB_VALUE = CALLED_TIMES_IN30DAY });//[手机]30天内危险号码被叫次数
                xmllist.Add(new VBXML { VB_COL = "CALLED_TIMES_IN15DAY_GRAY", VB_VALUE = CALLED_TIMES_IN15DAY_GRAY });//[手机]15天内危险号码被叫次数
                xmllist.Add(new VBXML { VB_COL = "CALLED_TIMES_IN30DAY_GRAY", VB_VALUE = CALLED_TIMES_IN30DAY_GRAY });//[手机]30天内危险号码被叫次数

                #region 手机账单
                xmllist.Add(new VBXML { VB_COL = "MOBILE_USED_IS6MTH", VB_VALUE = useMonths >= 6 ? "是" : "否" });//[手机]最近6个月消费金额
                xmllist.Add(new VBXML { VB_COL = "MOBILE_CONSUM_3MTH", VB_VALUE = ThreeMonthCallRecordAmount });//[手机]最近3个月消费金额
                xmllist.Add(new VBXML { VB_COL = "MOBILE_CALLCOUNT_3MTH", VB_VALUE = ThreeMonthCallRecordCount });//[手机]最近3个月通话次数
                xmllist.Add(new VBXML { VB_COL = "MOBILE_USED_MONTHS", VB_VALUE = useMonths.ToString() });//[手机]手机使用年限

                #endregion

                Log4netAdapter.WriteInfo(jsonService.SerializeObject(xmllist));

                string rc1 = rcServcie.GetRuleResultByCustomWithIdentityNoCallFrom(ServiceConstants.RC_Model_KKD, Req.Identitycard, Req.Name, ServiceConstants.RC_Model_KKD_ZhiCe, SerializationHelper.SerializeToXml(xmllist));
                decisionResult = rc1.DeserializeXML<List<DecisionResult>>().FirstOrDefault();
                BaseMongo baseMongo = new BaseMongo();
                RCStorage rcStoreage = new RCStorage();
                rcStoreage.Params = xmllist;
                rcStoreage.Result = decisionResult;
                rcStoreage.BusId = Req.Identitycard;
                rcStoreage.BusType = ServiceConstants.RC_Model_KKD_ZhiCe;
                baseMongo.Insert<RCStorage>(rcStoreage, "KKD_RCStorage" + DateTime.Now.ToString(Consts.DateFormatString7));

                #endregion

                //获取决策结果
                if (decisionResult != null)
                {
                    if (decisionResult.Result == "通过")
                    {
                        //手续费率
                        if (decisionResult.RuleResultCanShowSets.ContainsKey(ServiceConstants.RC_Result_KKD_FormalitiesRate))
                        {
                            rcResult.FormalitiesRate = decisionResult.RuleResultCanShowSets[ServiceConstants.RC_Result_KKD_FormalitiesRate].ToDecimal(0);
                        }
                        //月利率
                        if (decisionResult.RuleResultCanShowSets.ContainsKey(ServiceConstants.RC_Result_KKD_MonthlyInterestRate))
                        {
                            rcResult.MonthlyInterestRate = decisionResult.RuleResultCanShowSets[ServiceConstants.RC_Result_KKD_MonthlyInterestRate].ToDecimal(0);
                        }
                        //月服务费率
                        if (decisionResult.RuleResultCanShowSets.ContainsKey(ServiceConstants.RC_Result_KKD_MonthlyServiceRate))
                        {
                            rcResult.MonthlyServiceRate = decisionResult.RuleResultCanShowSets[ServiceConstants.RC_Result_KKD_MonthlyServiceRate].ToDecimal(0);
                        }
                        //审批金额
                        if (decisionResult.RuleResultCanShowSets.ContainsKey(ServiceConstants.RC_Result_KKD_ExamineLoanMoney))
                        {
                            rcResult.ExamineLoanMoney = decisionResult.RuleResultCanShowSets[ServiceConstants.RC_Result_KKD_ExamineLoanMoney].ToDecimal(0);
                        }
                        //最高期数
                        if (decisionResult.RuleResultCanShowSets.ContainsKey(ServiceConstants.RC_Result_KKD_HighestPeriods))
                        {
                            rcResult.HighestPeriods = decisionResult.RuleResultCanShowSets[ServiceConstants.RC_Result_KKD_HighestPeriods].ToInt(0);
                        }
                    }
                    else
                    {
                        List<RCRule> RejectReasons = new List<RCRule>();
                        var RuleResultSets = decisionResult.RuleResultSets;

                        RCRule RCRuleModel = null;
                        foreach (var item in RuleResultSets)
                        {
                            //查询拒绝组拒绝的原因
                            if (item.Value.Result == "1" && item.Value.Rule.RU_GP_TP == 1)
                            {
                                RCRuleModel = new RCRule();
                                RCRuleModel.RuleName = item.Value.Rule.RU_NM;
                                RCRuleModel.RuleDesc = item.Value.Rule.RU_DESC;
                                RejectReasons.Add(RCRuleModel);
                            }
                        }

                        rcResult.RejectReasons = RejectReasons;
                    }
                }

                rcResult.CreditScore = score;
                rcResult.MobileScore = mobileScore;
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = guid + "决策计算完毕";
                baseRes.Result = jsonService.SerializeObject(rcResult);
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(guid + "决策异常；", e);
                baseRes.StatusDescription = e.Message;
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
            }
            baseRes.EndTime = DateTime.Now.ToString();

            Log4netAdapter.WriteInfo(guid + "接口：GetRCResult,调用结束");
            return baseRes;
        }

        #endregion

        #region 社保版
        public BaseRes GetRCWithSocial(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            RCResult rcResult = new RCResult();
            DecisionResult decisionResult = new DecisionResult();
            RCWebService.RCWebService rcServcie = new RCWebService.RCWebService();
            List<VBXML> xmllist = new List<VBXML>();
            string rc = string.Empty;
            string guid = CommonFun.GetGuidID();
            SocialOpr socialOpr = new SocialOpr();

            try
            {
                string reqText = stream.AsStringText(true);
                WechatQueryReq Req = jsonService.DeserializeObject<WechatQueryReq>(reqText);

                Log4netAdapter.WriteInfo("接口：GetRCWithSocial；客户端IP:" + CommonFun.GetClientIP() + "；" + guid + "，参数：" + reqText);

                ISocialSecurity socialService = NetSpiderFactoryManager.GetSocialSecurityService();
                IProvidentFund providentService = NetSpiderFactoryManager.GetProvidentFundService();
                ICRD_HD_REPORT reportService = NetSpiderFactoryManager.GetCRDHDREPORTService();
                ISummary summaryService = NetSpiderFactoryManager.GetSummaryService();
                ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
                NetCreditOpr netCreditOpr = new NetCreditOpr();

                //SocialSecurityEntity socialEntity = socialService.GetByIdentityCard(Req.Identitycard);
                //ProvidentFundEntity providentEntity = providentService.GetByIdentityCard(Req.Identitycard);
                SocialSecurityEntity socialEntity = socialOpr.GetShebaoByIdentityCard(Req.Identitycard);
                ProvidentFundEntity providentEntity = socialOpr.GetGJJByIdentityCard(Req.Identitycard);

                CRD_HD_REPORTEntity reportEntity = netCreditOpr.GetSummaryByIdentityCard(Req.Identitycard);
                Spd_applyEntity applyEntity = applyService.GetByIdentityCardAndSpiderTypeAndMobile(Req.Identitycard, "mobile", Req.Mobile);
                int useMonths = 0;
                SummaryEntity summaryEntity = null;
                string ThreeMonthCallRecordAmount = "0";
                string SixMonthCallRecordAmount = "0";
                string ThreeMonthCallRecordCount = "0";
                string SixMonthCallRecordCount = "0";
                string CALLED_TIMES_IN15DAY = "0";
                string CALLED_TIMES_IN30DAY = "0";
                string CALLED_TIMES_IN15DAY_GRAY = "0";
                string CALLED_TIMES_IN30DAY_GRAY = "0";


                summaryEntity = GetMobileSummary(Req.Identitycard, Req.Mobile);
                if (summaryEntity != null)
                {
                    ThreeMonthCallRecordAmount = summaryEntity.ThreeMonthCallRecordAmount.ToString();
                    SixMonthCallRecordAmount = summaryEntity.SixMonthCallRecordAmount.ToString();
                    ThreeMonthCallRecordCount = summaryEntity.ThreeMonthCallRecordCount.ToString();
                    SixMonthCallRecordCount = summaryEntity.SixMonthCallRecordCount.ToString();
                    CALLED_TIMES_IN15DAY = summaryEntity.CALLED_TIMES_IN15DAY.ToString();
                    CALLED_TIMES_IN30DAY = summaryEntity.CALLED_TIMES_IN30DAY.ToString();
                    CALLED_TIMES_IN15DAY_GRAY = summaryEntity.CALLED_TIMES_IN15DAY_Gray.ToString();
                    CALLED_TIMES_IN30DAY_GRAY = summaryEntity.CALLED_TIMES_IN30DAY_Gray.ToString();

                    if (!summaryEntity.Regdate.IsEmpty())
                    {
                        useMonths = CommonFun.GetIntervalOf2DateTime(DateTime.Now, DateTime.Parse(summaryEntity.Regdate), "M");
                    }
                }


                #region 数据校验
                if (summaryEntity == null)
                {
                    baseRes.StatusDescription = "无手机账单数据";
                    return baseRes;
                }
                if (reportEntity == null)
                {
                    baseRes.StatusDescription = "无征信数据";
                    return baseRes;
                }
                //校验人员姓名与身份证与注册信息是否一致
                if (reportEntity.CertNo.Substring(14) != Req.Identitycard.Substring(14) || reportEntity.Name != Req.Name)
                {
                    Log4netAdapter.WriteInfo("警告：征信姓名、身份证号与注册信息不一致");
                    baseRes.StatusDescription = "注册信息与征信数据不一致";
                    return baseRes;
                }
                #endregion

                #region 调用决策系统
                xmllist.Clear();
                xmllist.Add(new VBXML { VB_COL = "IF_PAY_SOCIALALLOWANCE", VB_VALUE = Req.PayHousingFund });//是否缴纳公积金
                xmllist.Add(new VBXML { VB_COL = "SOCIALSECURITY_AGE", VB_VALUE = ChinaIDCard.GetAge(Req.Identitycard).ToString() });//[卡卡贷]年龄
                xmllist.Add(new VBXML { VB_COL = "CREDIT_grade", VB_VALUE = Req.CreditScore.ToString() });//[网络版征信]评分卡分数
                xmllist.Add(new VBXML { VB_COL = "SOCIAL_SCORE", VB_VALUE = Req.SocialSecurityScore.ToString() });//[公积金社保]社保评分卡分数
                xmllist.Add(new VBXML { VB_COL = "Mobile_Score", VB_VALUE = Req.MobileScore.ToString() });//手机评分
                xmllist.Add(new VBXML { VB_COL = "KKD_Mobile_Score_201609", VB_VALUE = Req.MobileScore2.ToString() });//手机评分
                xmllist.Add(new VBXML { VB_COL = "BaiRong_Score_Online", VB_VALUE = Req.BaiRongScore.ToString() });//百融评分
                xmllist.Add(new VBXML { VB_COL = "SEX", VB_VALUE = ChinaIDCard.GetSex(Req.Identitycard) });//百融评分


                xmllist.Add(new VBXML { VB_COL = "KKD_ChannelSource", VB_VALUE = Req.ChannelSource });//渠道
                xmllist.Add(new VBXML { VB_COL = "KKD_Region", VB_VALUE = Req.Region });//地区
                xmllist.Add(new VBXML { VB_COL = "CALLED_TIMES_IN15DAY", VB_VALUE = CALLED_TIMES_IN15DAY });//[手机]15天内危险号码被叫次数
                xmllist.Add(new VBXML { VB_COL = "CALLED_TIMES_IN30DAY", VB_VALUE = CALLED_TIMES_IN30DAY });//[手机]30天内危险号码被叫次数
                xmllist.Add(new VBXML { VB_COL = "CALLED_TIMES_IN15DAY_GRAY", VB_VALUE = CALLED_TIMES_IN15DAY_GRAY });//[手机]15天内危险号码被叫次数
                xmllist.Add(new VBXML { VB_COL = "CALLED_TIMES_IN30DAY_GRAY", VB_VALUE = CALLED_TIMES_IN30DAY_GRAY });//[手机]30天内危险号码被叫次数

                #region 社保参数

                //社保
                if (socialEntity != null)
                {
                    xmllist.Add(new VBXML { VB_COL = "M24_ContinuePay_SumMonth", VB_VALUE = socialEntity.PaymentMonths_Continuous != null ? socialEntity.PaymentMonths_Continuous.ToString() : "0" });//[公积金社保]24个月内最近连续缴费月数
                    xmllist.Add(new VBXML { VB_COL = "SOCIAL_BASEPAY", VB_VALUE = socialEntity.SocialInsuranceBase != null ? socialEntity.SocialInsuranceBase.ToString() : "0" });//[公积金社保]缴纳基数
                    xmllist.Add(new VBXML { VB_COL = "SOCIALFUND_SOCIAL_COMP", VB_VALUE = socialEntity.CompanyName });//[公积金社保]社保缴纳单位
                    xmllist.Add(new VBXML { VB_COL = "SOCIALFUND_FUND_COMP", VB_VALUE = "" });//[公积金社保]公积金缴纳单位
                }
                else if (providentEntity != null)
                {
                    xmllist.Add(new VBXML { VB_COL = "M24_ContinuePay_SumMonth", VB_VALUE = providentEntity.PaymentMonths_Continuous != null ? providentEntity.PaymentMonths_Continuous.ToString() : "0" });//[公积金社保]24个月内最近连续缴费月数
                    xmllist.Add(new VBXML { VB_COL = "SOCIAL_BASEPAY", VB_VALUE = providentEntity.SalaryBase != null ? providentEntity.SalaryBase.ToString() : "0" });//[公积金社保]缴纳基数
                    xmllist.Add(new VBXML { VB_COL = "SOCIALFUND_SOCIAL_COMP", VB_VALUE = "" });//[公积金社保]社保缴纳单位
                    xmllist.Add(new VBXML { VB_COL = "SOCIALFUND_FUND_COMP", VB_VALUE = providentEntity.CompanyName });//[公积金社保]公积金缴纳单位
                }
                else
                {
                    xmllist.Add(new VBXML { VB_COL = "M24_ContinuePay_SumMonth", VB_VALUE = "0" });//[公积金社保]24个月内最近连续缴费月数
                    xmllist.Add(new VBXML { VB_COL = "SOCIAL_BASEPAY", VB_VALUE = "0" });//[公积金社保]缴纳基数
                    xmllist.Add(new VBXML { VB_COL = "SOCIALFUND_SOCIAL_COMP", VB_VALUE = "" });//[公积金社保]社保缴纳单位
                    xmllist.Add(new VBXML { VB_COL = "SOCIALFUND_FUND_COMP", VB_VALUE = "" });//[公积金社保]公积金缴纳单位
                }

                #endregion

                #region 央行征信参数

                if (reportEntity == null)
                {
                    reportEntity = new CRD_HD_REPORTEntity();
                    reportEntity.CRD_STAT_LN = new CRD_STAT_LNEntity();
                    reportEntity.CRD_STAT_LND = new CRD_STAT_LNDEntity();
                    reportEntity.CRD_STAT_QR = new CRD_STAT_QREntity();
                }
                xmllist.Add(new VBXML { VB_COL = "NC_M3_ALL_CNT_TOTAL", VB_VALUE = reportEntity.CRD_STAT_QR.M3ALLCNTTOTAL != null ? reportEntity.CRD_STAT_QR.M3ALLCNTTOTAL.ToString() : "0" });//[网络版征信]三个月内所有征信查询次数
                xmllist.Add(new VBXML { VB_COL = "ALL_LOAN_HOUSE_DELAY90_CNT", VB_VALUE = reportEntity.CRD_STAT_LN.ALLLOANHOUSEDELAY90CNT != null ? reportEntity.CRD_STAT_LN.ALLLOANHOUSEDELAY90CNT.ToString() : "0" });//[网络版征信]住房贷款发生过90天以上逾期的账户数(all)
                xmllist.Add(new VBXML { VB_COL = "ALL_LOAN_OTHER_DELAY90_CNT", VB_VALUE = reportEntity.CRD_STAT_LN.ALLLOANOTHERDELAY90CNT != null ? reportEntity.CRD_STAT_LN.ALLLOANOTHERDELAY90CNT.ToString() : "0" });//[网络版征信]其他贷款发生过90天以上逾期的账户数(all)
                xmllist.Add(new VBXML { VB_COL = "ALL_CREDIT_DELAY90_CNT", VB_VALUE = reportEntity.CRD_STAT_LND.ALLCREDITDELAY90CNT != null ? reportEntity.CRD_STAT_LND.ALLCREDITDELAY90CNT.ToString() : "0" });//[网络版征信]信用卡发生过90天以上逾期的账户数(all)
                xmllist.Add(new VBXML { VB_COL = "credit_dlq_amount", VB_VALUE = reportEntity.CRD_STAT_LND.CREDITDLQAMOUNT != null ? reportEntity.CRD_STAT_LND.CREDITDLQAMOUNT.ToString() : "0" });//[网络版征信]信用卡逾期金额
                xmllist.Add(new VBXML { VB_COL = "loan_dlq_amount", VB_VALUE = reportEntity.CRD_STAT_LN.LOANDLQAMOUNT != null ? reportEntity.CRD_STAT_LN.LOANDLQAMOUNT.ToString() : "0" });//[网络版征信]贷款逾期金额
                xmllist.Add(new VBXML { VB_COL = "ALL_LOAN_HOUSE_UNCLOSEACCOUNT_CNT", VB_VALUE = reportEntity.CRD_STAT_LN.ALLLOANHOUSEUNCLOSEDCNT != null ? reportEntity.CRD_STAT_LN.ALLLOANHOUSEUNCLOSEDCNT.ToString() : "0" });//[网络版征信]住房贷款未销户账户数(all)
                xmllist.Add(new VBXML { VB_COL = "ALL_LOAN_OTHER_UNCLOSED_CNT", VB_VALUE = reportEntity.CRD_STAT_LN.ALLLOANOTHERUNCLOSEDCNT != null ? reportEntity.CRD_STAT_LN.ALLLOANOTHERUNCLOSEDCNT.ToString() : "0" });//[网络版征信]其他贷款未销户账户数(all)
                xmllist.Add(new VBXML { VB_COL = "ALL_CREDIT_UNCLOSED_CNT", VB_VALUE = reportEntity.CRD_STAT_LND.ALLCREDITUNCLOSEDCNT != null ? reportEntity.CRD_STAT_LND.ALLCREDITUNCLOSEDCNT.ToString() : "0" });//[网络版征信]信用卡未销户
                xmllist.Add(new VBXML { VB_COL = "CREDIT_LIMIT_AMOUNT_NORM_MAX", VB_VALUE = reportEntity.CRD_STAT_LND.CREDITLIMITAMOUNTNORMMAX != null ? reportEntity.CRD_STAT_LND.CREDITLIMITAMOUNTNORMMAX.ToString() : "0" });//[网络版征信]正常信用卡最大额度
                xmllist.Add(new VBXML { VB_COL = "NC_NORMAL_USED_RATE", VB_VALUE = reportEntity.CRD_STAT_LND.NORMALUSEDRATE != null ? reportEntity.CRD_STAT_LND.NORMALUSEDRATE.ToString() : "0" });//[网络版征信]正常信用卡使用率
                xmllist.Add(new VBXML { VB_COL = "SUM_USED_CREDIT_LIMIT_AMOUNT", VB_VALUE = reportEntity.CRD_STAT_LND.SUMUSEDCREDITLIMITAMOUNT != null ? reportEntity.CRD_STAT_LND.SUMUSEDCREDITLIMITAMOUNT.ToString() : "0" });//[网络版征信]信用卡已使用总额度
                xmllist.Add(new VBXML { VB_COL = "NORMAL_CARD_AGE_MONTH", VB_VALUE = reportEntity.CRD_STAT_LND.NORMALCARDAGEMONTH != null ? reportEntity.CRD_STAT_LND.NORMALCARDAGEMONTH.ToString() : "0" });//[网络版征信]正常状态最早卡龄
                xmllist.Add(new VBXML { VB_COL = "Normal_Used_Credit_Limit_Amount", VB_VALUE = reportEntity.CRD_STAT_LND.NORMALUSEDMAX != null ? reportEntity.CRD_STAT_LND.NORMALUSEDMAX.ToString() : "0" });//[网络版征信]正常信用卡中的最大已使用额度
                xmllist.Add(new VBXML { VB_COL = "SUM_Loan_Amount", VB_VALUE = reportEntity.CRD_STAT_LN.SUMLOANLIMITAMOUNT != null ? reportEntity.CRD_STAT_LN.SUMLOANLIMITAMOUNT.ToString() : "0" });//[网络版征信]所有贷款的贷款本金总额
                xmllist.Add(new VBXML { VB_COL = "lnd_max_overdue_percent", VB_VALUE = reportEntity.CRD_STAT_LND.lnd_max_overdue_percent != null ? reportEntity.CRD_STAT_LND.lnd_max_overdue_percent.ToString() : "0" });//[网络版征信]所有贷款的贷款本金总额
                xmllist.Add(new VBXML { VB_COL = "COUNT_Nonbank_IN3M", VB_VALUE = reportEntity.CRD_STAT_QR.COUNT_Nonbank_IN3M != null ? reportEntity.CRD_STAT_QR.COUNT_Nonbank_IN3M.ToString() : "0" });//[征信]3个月内非银机构查询次数

                #endregion

                #region 手机账单
                xmllist.Add(new VBXML { VB_COL = "MOBILE_USED_IS3MTH", VB_VALUE = useMonths >= 3 ? "是" : "否" });//[手机]最近3个月消费金额
                xmllist.Add(new VBXML { VB_COL = "MOBILE_USED_IS6MTH", VB_VALUE = useMonths >= 6 ? "是" : "否" });//[手机]最近6个月消费金额
                xmllist.Add(new VBXML { VB_COL = "MOBILE_CONSUM_3MTH", VB_VALUE = ThreeMonthCallRecordAmount });//[手机]最近3个月消费金额
                xmllist.Add(new VBXML { VB_COL = "MOBILE_CONSUM_6MTH", VB_VALUE = SixMonthCallRecordAmount });//[手机]最近6个月消费金额
                xmllist.Add(new VBXML { VB_COL = "MOBILE_CALLCOUNT_3MTH", VB_VALUE = ThreeMonthCallRecordCount });//[手机]最近3个月通话次数
                xmllist.Add(new VBXML { VB_COL = "MOBILE_CALLCOUNT_6MTH", VB_VALUE = SixMonthCallRecordCount });//[手机]最近6个月通话次数
                xmllist.Add(new VBXML { VB_COL = "MOBILE_USED_MONTHS", VB_VALUE = useMonths.ToString() });//[手机]手机使用年限
                #endregion

                Log4netAdapter.WriteInfo(jsonService.SerializeObject(xmllist));
                string rc1 = string.Empty;
                rc1 = rcServcie.GetRuleResultByCustomWithIdentityNoCallFrom(ServiceConstants.RC_Model_KKD, Req.Identitycard, Req.Name, "卡卡贷社保版", SerializationHelper.SerializeToXml(xmllist));

                decisionResult = rc1.DeserializeXML<List<DecisionResult>>().FirstOrDefault();

                BaseMongo baseMongo = new BaseMongo();
                RCStorage rcStoreage = new RCStorage();
                rcStoreage.Params = xmllist;
                rcStoreage.Result = decisionResult;
                rcStoreage.BusId = Req.Identitycard;
                rcStoreage.BusType = ServiceConstants.RC_Model_KKD;
                baseMongo.Insert<RCStorage>(rcStoreage, "KKD_RCStorage" + DateTime.Now.ToString(Consts.DateFormatString7));
                #endregion

                //获取决策结果
                if (decisionResult != null)
                {

                    if (decisionResult.Result == "通过")
                    {
                        //手续费率
                        if (decisionResult.RuleResultCanShowSets.ContainsKey(ServiceConstants.RC_Result_KKD_FormalitiesRate))
                        {
                            rcResult.FormalitiesRate = decisionResult.RuleResultCanShowSets[ServiceConstants.RC_Result_KKD_FormalitiesRate].ToDecimal(0);
                        }
                        //月利率
                        if (decisionResult.RuleResultCanShowSets.ContainsKey(ServiceConstants.RC_Result_KKD_MonthlyInterestRate))
                        {
                            rcResult.MonthlyInterestRate = decisionResult.RuleResultCanShowSets[ServiceConstants.RC_Result_KKD_MonthlyInterestRate].ToDecimal(0);
                        }
                        //月服务费率
                        if (decisionResult.RuleResultCanShowSets.ContainsKey(ServiceConstants.RC_Result_KKD_MonthlyServiceRate))
                        {
                            rcResult.MonthlyServiceRate = decisionResult.RuleResultCanShowSets[ServiceConstants.RC_Result_KKD_MonthlyServiceRate].ToDecimal(0);
                        }
                        //审批金额
                        if (decisionResult.RuleResultCanShowSets.ContainsKey(ServiceConstants.RC_Result_KKD_ExamineLoanMoney))
                        {
                            rcResult.ExamineLoanMoney = decisionResult.RuleResultCanShowSets[ServiceConstants.RC_Result_KKD_ExamineLoanMoney].ToDecimal(0);
                        }
                        //客户分类结果
                        if (decisionResult.RuleResultCanShowSets.ContainsKey("客户分类结果"))
                        {
                            rcResult.CustomerRating = decisionResult.RuleResultCanShowSets["客户分类结果"].ToString();
                        }
                        //最高期数
                        if (decisionResult.RuleResultCanShowSets.ContainsKey(ServiceConstants.RC_Result_KKD_HighestPeriods))
                        {
                            rcResult.HighestPeriods = decisionResult.RuleResultCanShowSets[ServiceConstants.RC_Result_KKD_HighestPeriods].ToInt(0);
                        }

                    }
                    else
                    {
                        List<RCRule> RejectReasons = new List<RCRule>();
                        var RuleResultSets = decisionResult.RuleResultSets;

                        RCRule RCRuleModel = null;
                        foreach (var item in RuleResultSets)
                        {
                            //查询拒绝组拒绝的原因
                            if (item.Value.Result == "1" && item.Value.Rule.RU_GP_TP == 1)
                            {
                                RCRuleModel = new RCRule();
                                RCRuleModel.RuleName = item.Value.Rule.RU_NM;
                                RCRuleModel.RuleDesc = item.Value.Rule.RU_DESC;
                                RejectReasons.Add(RCRuleModel);
                            }
                        }

                        rcResult.RejectReasons = RejectReasons;
                    }
                }
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = guid + "决策计算完毕";
                baseRes.Result = jsonService.SerializeObject(rcResult);
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(guid + "决策异常；", e);
                baseRes.StatusDescription = e.Message;
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
            }
            baseRes.EndTime = DateTime.Now.ToString();

            Log4netAdapter.WriteInfo(guid + "接口：GetRCResult,调用结束");
            return baseRes;
        }

        #endregion

        #endregion

        #region 聚信立手机账单相关接口
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
            Log4netAdapter.WriteInfo("接口：JxlSubmitSuccess；客户端IP:" + CommonFun.GetClientIP());
            Log4netAdapter.WriteInfo("参数:" + jsonService.SerializeObject(submitInfo));

            BaseRes baseRes = new BaseRes();
            try
            {
                Cn.Vcredit.ThirdParty.Juxinli.DataProcess process = new Cn.Vcredit.ThirdParty.Juxinli.DataProcess();
                baseRes.Result = "{\"Id\":\"" + process.SubmitInfo(submitInfo.orderid.ToString(), ServiceConstants.BusType_Kakadai, submitInfo.name, submitInfo.identitycard, submitInfo.mobile) + "\"}";
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

        public BaseRes JxlIsAuthForJson(Stream stream)
        {
            //Log4netAdapter.WriteInfo("接口：JxlIsAuth；客户端IP:" + CommonFun.GetClientIP());
            BaseRes baseRes = new BaseRes();
            //获得请求内容
            string reqText = stream.AsStringText(true);
            try
            {

                JxlSubmitInfo Req = jsonService.DeserializeObject<JxlSubmitInfo>(reqText);
                IOperationLog operService = NetSpiderFactoryManager.GetOperationLogService();
                ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();

                Spd_applyEntity applyEntity = applyService.GetByIdentityCardAndSpiderTypeAndMobile(Req.identitycard, "mobile", Req.mobile);
                OperationLogEntity entity = operService.GetByIdentityNoAndMobile(Req.identitycard, Req.mobile);

                dynamic Jxlresult = new ExpandoObject();
                Jxlresult.Status = null;
                Jxlresult.IdentityNo = null;
                Jxlresult.Name = null;

                if (applyEntity == null && entity == null)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "没有提交聚信立查询";
                    Jxlresult.Status = "none";//获取报告失败
                    baseRes.Result = jsonService.SerializeObject(new
                    {
                        Status = Jxlresult.Status ?? String.Empty,
                        IdentityNo = Jxlresult.IdentityNo ?? String.Empty,
                        Name = Jxlresult.Name ?? String.Empty
                    });
                    Log4netAdapter.WriteInfo("没有提交聚信立查询");

                    return baseRes;
                }
                else if (applyEntity != null && (entity == null || applyEntity.CreateTime > entity.SendTime) && applyEntity.Crawl_status == ServiceConsts.CrawlerStatusCode_AnalysisSuccess)
                {
                    MobileMongo mibleMongo = new MobileMongo(applyEntity.CreateTime);
                    var summary = mibleMongo.GetSummaryByToken(applyEntity.Token);
                    if (summary.IsRealNameAuth == 1)
                    {
                        Jxlresult.Status = "auth";//实名认证
                    }
                    else
                    {
                        Jxlresult.Status = "noauth";//非实名认证
                    }

                    Jxlresult.IdentityNo = summary.IdentityCard;
                    Jxlresult.Name = summary.Name;
                }
                else
                {
                    if (entity.Status == 1 || entity.Status == 5)
                    {
                        if (entity.ReceiveFailCount != null && entity.ReceiveFailCount == 10)
                        {
                            Jxlresult.Status = "fail";//获取报告失败
                        }
                        else
                        {
                            Jxlresult.Status = "wait";//等待报告
                        }
                    }
                    else
                    {
                        ISummary service = NetSpiderFactoryManager.GetSummaryService();
                        IBasic basicService = NetSpiderFactoryManager.GetBasicService();
                        var summaryEntity = service.GetByOperId(entity.Id);
                        var basicEntity = basicService.GetByOperid(entity.Id);
                        if (summaryEntity != null && summaryEntity.IsRealNameAuth != null && summaryEntity.IsRealNameAuth == 1)
                        {
                            Jxlresult.Status = "auth";//实名认证
                        }
                        else
                        {
                            Jxlresult.Status = "noauth";//非实名认证
                        }
                        if (basicEntity != null)
                        {
                            Jxlresult.IdentityNo = basicEntity.Idcard;
                            Jxlresult.Name = basicEntity.Real_name;
                        }

                    }
                }
                baseRes.Result = jsonService.SerializeObject(new
                {
                    Status = Jxlresult.Status ?? String.Empty,
                    IdentityNo = Jxlresult.IdentityNo ?? String.Empty,
                    Name = Jxlresult.Name ?? String.Empty
                });

                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "认证完成";
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteInfo("参数:" + reqText);
                Log4netAdapter.WriteError("认证异常", e);
                baseRes.StatusCode = ServiceConstants.StatusCode_error;
                baseRes.StatusDescription = "认证异常";
            }
            baseRes.EndTime = DateTime.Now.ToString();
            return baseRes;
        }
        #endregion

        #region 从数据库查询数据给VBS展示
        /// <summary>
        /// 征信报告数据从数据库查询
        /// </summary>
        /// <param name="bid"></param>
        /// <returns></returns>
        public CRD_HD_REPORTRes GetPbccrcReportFromDBForJson(string identitycard)
        {
            Log4netAdapter.WriteInfo("接口：ProvidentFundQueryFromDBForJson；客户端IP:" + CommonFun.GetClientIP());
            CRD_HD_REPORTRes queryRes = new CRD_HD_REPORTRes();
            CRD_HD_REPORTEntity entity = new CRD_HD_REPORTEntity();
            try
            {
                ICRD_HD_REPORT service = NetSpiderFactoryManager.GetCRDHDREPORTService();
                //entity = service.GetByBid(bid.ToInt(0));
                //entity = service.GetByBusiness(bid, ServiceConstants.BusType_Kakadai);
                entity = service.GetByIdentityCardAndBusType(identitycard, ServiceConstants.BusType_Kakadai);
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
        /// <summary>
        /// 社保数据从数据库查询
        /// </summary>
        /// <param name="bid"></param>
        /// <returns></returns>
        public SocialSecurityQueryRes SocialSecurityQueryFromDBForJson(string identitycard)
        {
            Log4netAdapter.WriteInfo("接口：SocialSecurityQueryFromDBForJson；客户端IP:" + CommonFun.GetClientIP());
            SocialSecurityQueryRes queryRes = new SocialSecurityQueryRes();
            SocialSecurityEntity entity = new SocialSecurityEntity();
            try
            {
                ISocialSecurity service = NetSpiderFactoryManager.GetSocialSecurityService();
                //entity = service.GetByBusiness(bid, ServiceConstants.BusType_Kakadai);
                entity = service.GetByIdentityCard(identitycard);
                if (entity != null)
                {
                    queryRes = jsonService.DeserializeObject<SocialSecurityQueryRes>(jsonService.SerializeObject(entity));
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
                Log4netAdapter.WriteError("社保信息采集异常", e);
                queryRes.StatusCode = ServiceConstants.StatusCode_error;
                queryRes.StatusDescription = e.Message;
            }
            queryRes.EndTime = DateTime.Now.ToString();
            return queryRes;
        }
        /// <summary>
        /// 公积金数据从数据库查询
        /// </summary>
        /// <param name="bid"></param>
        /// <returns></returns>
        public ProvidentFundQueryRes ProvidentFundQueryFromDBForJson(string identitycard)
        {
            Log4netAdapter.WriteInfo("接口：ProvidentFundQueryFromDBForJson；客户端IP:" + CommonFun.GetClientIP());
            ProvidentFundQueryRes queryRes = new ProvidentFundQueryRes();
            ProvidentFundEntity entity = new ProvidentFundEntity();
            try
            {
                IProvidentFund service = NetSpiderFactoryManager.GetProvidentFundService();
                //entity = service.GetByBusiness(bid, ServiceConstants.BusType_Kakadai);
                entity = service.GetByIdentityCard(identitycard);
                if (entity != null)
                {
                    queryRes = jsonService.DeserializeObject<ProvidentFundQueryRes>(jsonService.SerializeObject(entity));
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
                Log4netAdapter.WriteError("社保信息采集异常", e);
                queryRes.StatusCode = ServiceConstants.StatusCode_error;
                queryRes.StatusDescription = e.Message;
            }
            queryRes.EndTime = DateTime.Now.ToString();
            return queryRes;
        }
        /// <summary>
        /// 获取手机账单报告目录
        /// </summary>
        /// <param name="bid"></param>
        /// <returns></returns>
        public object GetMobileReceiveFilePathFromDBForJson(string bid)
        {
            Log4netAdapter.WriteInfo("接口：GetMobileReceiveFilePathFromDBForJson；客户端IP:" + CommonFun.GetClientIP());
            BaseRes baseRes = new BaseRes();
            string strRet = string.Empty;
            try
            {
                IOperationLog service = NetSpiderFactoryManager.GetOperationLogService();
                var entity = service.GetByBusiness(bid, ServiceConstants.BusType_Kakadai);
                if (entity != null)
                {
                    strRet = entity.ReceiveFilePath;
                }
            }
            catch (Exception e)
            {
            }
            return strRet;
        }
        /// <summary>
        /// 获取手机账单
        /// </summary>
        /// <param name="bid"></param>
        /// <returns></returns>
        public BaseRes GetMobileSummaryFromDBForJson(string bid)
        {
            Log4netAdapter.WriteInfo("接口：GetMobileSummaryFromDBForJson；客户端IP:" + CommonFun.GetClientIP());
            BaseRes baseRes = new BaseRes();
            try
            {
                ISummary service = NetSpiderFactoryManager.GetSummaryService();
                var entity = service.GetByBusiness(bid, ServiceConstants.BusType_Kakadai);
                if (entity != null)
                {
                    entity.OperationLog = null;
                    baseRes.Result = jsonService.SerializeObject(entity);
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
        public BaseRes GetMobileReportForJson(string bid)
        {
            Log4netAdapter.WriteInfo("接口：GetMobileReportForJson；客户端IP:" + CommonFun.GetClientIP());
            BaseRes baseRes = new BaseRes();
            try
            {
                string ftpHost = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpAddress"));
                string ftpUser = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpUserName"));
                string ftpPassword = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpPassword"));
                //string filepath = HttpContext.Current.Server.MapPath("/files/jxl/");
                string filepath = Chk.IsNull(ConfigurationHelper.GetAppSetting("JxlReportPath")); ;
                string filename = string.Empty;
                IOperationLog service = NetSpiderFactoryManager.GetOperationLogService();
                var entity = service.GetByBusiness(bid, ServiceConstants.BusType_Kakadai);

                if (entity != null)
                {
                    filename = entity.ReceiveFilePath;
                    filepath += filename;
                }
                if (!File.Exists(filepath))
                {
                    FTPHelper ftp = new FTPHelper(ftpHost, ftpUser, ftpPassword);
                    ftp.DownloadFile(filepath, filename);
                }


                baseRes.Result = jsonService.SerializeObject(entity);
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
        public BaseRes GetMobileSummaryByIdentityCardForJson(string identitycard, string mobile)
        {
            Log4netAdapter.WriteInfo("接口：GetMobileSummaryFromDBForJson；客户端IP:" + CommonFun.GetClientIP());
            BaseRes baseRes = new BaseRes();
            try
            {
                IVariable_mobile_summary variableService = NetSpiderFactoryManager.GetVariable_mobile_summaryService();
                Variable_mobile_summaryEntity variableEntity = variableService.GetByBusIdentityNoAndMobile(identitycard, mobile);
                if (variableEntity != null)
                {

                    var result = new
                    {
                        City = variableEntity.City,
                        IsRealNameAuth = variableEntity.IsRealNameAuth,
                        Mobile = variableEntity.Mobile,
                        OneMonthCallRecordCount = variableEntity.One_Month_Call_Record_Count,
                        OneMonthCallRecordAmount = variableEntity.One_Month_Call_Record_Amount,
                        ThreeMonthCallRecordCount = variableEntity.Three_Month_Call_Record_Count,
                        ThreeMonthCallRecordAmount = variableEntity.Three_Month_Call_Record_Amount,
                        SixMonthCallRecordCount = variableEntity.Six_Month_Call_Record_Count,
                        SixMonthCallRecordAmount = variableEntity.Six_Month_Call_Record_Amount,
                        Regdate = variableEntity.Regdate,
                        IsSelf = variableEntity.SourceType == "jxl" ? 0 : 1,
                    };
                    baseRes.Result = jsonService.SerializeObject(result);
                    baseRes.StatusDescription = "查询成功";
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                }
                else
                {
                    baseRes.StatusDescription = "无数据";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                }


                //ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
                //ISummary service = NetSpiderFactoryManager.GetSummaryService();
                //Spd_applyEntity applyEntity = applyService.GetByIdentityCardAndSpiderTypeAndMobile(identitycard, "mobile", mobile);
                //if (applyEntity != null)
                //{

                //    if (applyEntity.Crawl_status == null || applyEntity.Crawl_status != ServiceConsts.CrawlStatus_Mobile_Success)
                //    {
                //        baseRes.StatusDescription = "无数据";
                //        baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                //    }
                //    else
                //    {
                //        MobileMongo mibleMongo = new MobileMongo(applyEntity.CreateTime);
                //        var summary = mibleMongo.GetSummaryByToken(applyEntity.Token);
                //        var result = new
                //        {
                //            City = summary.City,
                //            IsRealNameAuth = summary.IsRealNameAuth,
                //            Mobile = summary.Mobile,
                //            OneMonthCallRecordCount = summary.OneMonthCallRecordCount,
                //            OneMonthCallRecordAmount = summary.OneMonthCallRecordAmount,
                //            ThreeMonthCallRecordCount = summary.ThreeMonthCallRecordCount,
                //            ThreeMonthCallRecordAmount = summary.ThreeMonthCallRecordAmount,
                //            SixMonthCallRecordCount = summary.SixMonthCallRecordCount,
                //            SixMonthCallRecordAmount = summary.SixMonthCallRecordAmount,
                //            Regdate = summary.Regdate,
                //            IsSelf = 1
                //        };
                //        baseRes.Result = jsonService.SerializeObject(result);
                //        baseRes.StatusDescription = "查询成功";
                //        baseRes.StatusCode = ServiceConsts.StatusCode_success;
                //    }
                //}
                //else
                //{
                //    var entity = service.GetByIdentityNoAndMobile(identitycard, mobile, ServiceConstants.BusType_Kakadai);
                //    if (entity != null)
                //    {
                //        BasicEntity basic = new BasicEntity();
                //        if (entity.Regdate.IsEmpty())
                //        {
                //            IBasic basicService = NetSpiderFactoryManager.GetBasicService();
                //            basic = basicService.GetByIdentityNoAndMobile(identitycard, mobile);
                //            entity.Regdate = basic.Reg_time;
                //        }
                //        var result = new
                //        {
                //            City = entity.City,
                //            IsRealNameAuth = entity.IsRealNameAuth,
                //            Mobile = entity.Mobile,
                //            OneMonthCallRecordCount = entity.OneMonthCallRecordCount,
                //            OneMonthCallRecordAmount = entity.OneMonthCallRecordAmount,
                //            ThreeMonthCallRecordCount = entity.ThreeMonthCallRecordCount,
                //            ThreeMonthCallRecordAmount = entity.ThreeMonthCallRecordAmount,
                //            SixMonthCallRecordCount = entity.SixMonthCallRecordCount,
                //            SixMonthCallRecordAmount = entity.SixMonthCallRecordAmount,
                //            Regdate = entity.Regdate,
                //            IsSelf = 0
                //        };
                //        baseRes.Result = jsonService.SerializeObject(result);
                //        baseRes.StatusDescription = "查询成功";
                //        baseRes.StatusCode = ServiceConsts.StatusCode_success;
                //        //entity.OperationLog = null;
                //        //baseRes.Result = jsonService.SerializeObject(entity);
                //        //baseRes.StatusDescription = "查询成功";
                //        //baseRes.StatusCode = ServiceConsts.StatusCode_success;
                //    }
                //    else
                //    {
                //        baseRes.Result = "";
                //        baseRes.StatusDescription = "无数据";
                //        baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                //    }
                //}
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        public BaseRes GetMobileReportByIdentityCardForJson(string identitycard, string mobile)
        {

            Log4netAdapter.WriteInfo("接口：GetMobileReportForJson；客户端IP:" + CommonFun.GetClientIP());
            BaseRes baseRes = new BaseRes();
            try
            {
                string ftpHost = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpAddress"));
                string ftpUser = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpUserName"));
                string ftpPassword = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpPassword"));
                string filepath = Chk.IsNull(ConfigurationHelper.GetAppSetting("JxlReportPath")); ;
                string filename = string.Empty;
                IOperationLog service = NetSpiderFactoryManager.GetOperationLogService();
                var entity = service.GetByIdentityNoAndMobile(identitycard, mobile);

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


                baseRes.Result = jsonService.SerializeObject(entity);
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
        /// <summary>
        /// 获取聚信立手机账单报告
        /// </summary>
        /// <param name="bid"></param>
        /// <returns></returns>
        #endregion

        #region 身份证OCR解析
        /// <summary>
        /// 根据图片二进制流，通过OCR解析身份证图片
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public IdentityCard IdentityCardFromBytes(Stream stream)
        {
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：IdentityCardFromBytes；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);
            Vcredit.WeiXin.RestService.OCRService.IdentityCard idcard = null;
            IdentityCard idcardRes = new IdentityCard();
            try
            {
                var bitmap = Bitmap.FromStream(stream);

                byte[] bytes = FileOperateHelper.ImageToBytes(bitmap);

                OCRService.OCRService ocrService = new OCRService.OCRService();
                idcard = ocrService.RecognizeIdentityCardByBytes(bytes);
                string jsonStr = jsonService.SerializeObject(idcard);
                idcardRes = jsonService.DeserializeObject<IdentityCard>(jsonStr);
                Log4netAdapter.WriteInfo(jsonStr);
            }
            catch (Exception e)
            {
                idcardRes.StatusCode = ServiceConsts.StatusCode_error;
                idcardRes.StatusDescription = e.Message;
                Log4netAdapter.WriteError("身份证OCR解析异常；" + guid, e);
            }
            Log4netAdapter.WriteInfo("解析结束；" + guid);
            return idcardRes;
        }
        /// <summary>
        /// 根据图片链接，通过OCR解析身份证图片
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public IdentityCard IdentityCardFromUrl(Stream stream)
        {
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：IdentityCardFromBytes；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);
            Vcredit.WeiXin.RestService.OCRService.IdentityCard idcard = null;
            IdentityCard idcardRes = new IdentityCard();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                OCRReq Req = jsonService.DeserializeObject<OCRReq>(reqText);

                OCRService.OCRService ocrService = new OCRService.OCRService();
                idcard = ocrService.RecognizeIdentityCardByUrl(Req.imageUrl);
                string jsonStr = jsonService.SerializeObject(idcard);
                idcardRes = jsonService.DeserializeObject<IdentityCard>(jsonStr);

                Log4netAdapter.WriteInfo(idcard.StatusDescription);
            }
            catch (Exception e)
            {
                idcard.StatusCode = ServiceConsts.StatusCode_error;
                idcard.StatusDescription = e.Message;
                Log4netAdapter.WriteError("身份证OCR解析异常；" + guid, e);
            }
            Log4netAdapter.WriteInfo("解析结束；" + guid);

            return idcardRes;
        }
        #endregion

        #region 私有方法
        private SummaryEntity GetMobileSummary(string identitycard, string mobile)
        {
            SummaryEntity retSummary = null;
            try
            {
                IVariable_mobile_summary variableService = NetSpiderFactoryManager.GetVariable_mobile_summaryService();
                Variable_mobile_summaryEntity variableEntity = variableService.GetByBusIdentityNoAndMobile(identitycard, mobile);
                if (variableEntity != null)
                {
                    retSummary = new SummaryEntity();
                    retSummary.OneMonthCallRecordAmount = variableEntity.One_Month_Call_Record_Amount;
                    retSummary.OneMonthCallRecordCount = variableEntity.One_Month_Call_Record_Count;
                    retSummary.ThreeMonthCallRecordAmount = variableEntity.Three_Month_Call_Record_Amount;
                    retSummary.ThreeMonthCallRecordCount = variableEntity.Three_Month_Call_Record_Count;
                    retSummary.SixMonthCallRecordAmount = variableEntity.Six_Month_Call_Record_Amount;
                    retSummary.SixMonthCallRecordCount = variableEntity.Six_Month_Call_Record_Count;

                    retSummary.MAX_PLAN_AMT = variableEntity.MAX_PLAN_AMT;
                    retSummary.DAY90_CALLING_TIMES = variableEntity.DAY90_CALLING_TIMES;
                    retSummary.CALLED_PHONE_CNT = variableEntity.CALLED_PHONE_CNT;
                    retSummary.LOCAL_CALL_TIME = variableEntity.LOCAL_CALL_TIME;
                    retSummary.DAY180_CALLING_SUBTTL = variableEntity.DAY180_CALLING_SUBTTL;
                    retSummary.DAY90_CALL_TTL_TIME = variableEntity.DAY90_CALL_TTL_TIME;
                    retSummary.DAY90_CALL_TIMES = variableEntity.DAY90_CALL_TIMES;
                    retSummary.DAY90_CALLING_TTL_TIME = variableEntity.DAY90_CALLING_TTL_TIME;
                    retSummary.NET_LSTM6_ONL_FLOW = variableEntity.NET_LSTM6_ONL_FLOW;
                    retSummary.DAY_CALLING_TTL_TIME = variableEntity.DAY_CALLING_TTL_TIME;
                    retSummary.CALLED_TIMES = variableEntity.CALLED_TIMES;
                    retSummary.CALLED_TTL_TIME = variableEntity.CALLED_TTL_TIME;
                    retSummary.MRNG_CALLED_TIMES = variableEntity.MRNG_CALLED_TIMES;
                    retSummary.CALL_TTL_TIME = variableEntity.CALL_TTL_TIME;
                    retSummary.NIGHT_CALLED_TTL_TIME = variableEntity.NIGHT_CALLED_TTL_TIME;
                    retSummary.AFTN_CALL_TTL_TIME = variableEntity.AFTN_CALL_TTL_TIME;
                    retSummary.AFTN_CALLING_TTL_TIME = variableEntity.AFTN_CALLING_TTL_TIME;
                    retSummary.NIGHT_CALL_TTL_TIME = variableEntity.NIGHT_CALL_TTL_TIME;
                    retSummary.NIGHT_CALLING_TTL_TIME = variableEntity.NIGHT_CALLING_TTL_TIME;
                    retSummary.CALLING_TTL_TIME = variableEntity.CALLING_TTL_TIME;
                    retSummary.PH_USE_MONS = variableEntity.PH_USE_MONS;
                    retSummary.CALL_PHONE_CNT = variableEntity.CALL_PHONE_CNT;
                    retSummary.CTT_DAYS_CNT = variableEntity.CTT_DAYS_CNT;
                    retSummary.CALLED_CTT_DAYS_CNT = variableEntity.CALLED_CTT_DAYS_CNT;
                    retSummary.CALLING_CTT_DAYS_CNT = variableEntity.CALLING_CTT_DAYS_CNT;
                    retSummary.CALLED_TIMES_IN30DAY = (int)variableEntity.CALLED_TIMES_IN30DAY;
                    retSummary.CALLED_TIMES_IN15DAY = (int)variableEntity.CALLED_TIMES_IN15DAY;
                    retSummary.CALLED_TIMES_IN30DAY_Gray = (int)variableEntity.CALLED_TIMES_IN30DAY_Gray;
                    retSummary.CALLED_TIMES_IN15DAY_Gray = (int)variableEntity.CALLED_TIMES_IN15DAY_Gray;
                    retSummary.Regdate = variableEntity.Regdate;
                }
            }
            catch (Exception e)
            {

            }
            return retSummary;

        }

        private int GetMobileScore(WechatQueryReq Req, SummaryEntity summaryEntity)
        {
            List<VBXML> xmllist = new List<VBXML>();
            DecisionResult decisionResult = new DecisionResult();
            RCWebService.RCWebService rcServcie = new RCWebService.RCWebService();
            int mobileScore = 0;//手机评分
            string rc = string.Empty;

            try
            {
                xmllist.Add(new VBXML { VB_COL = "Education", VB_VALUE = Req.Education });//[申请人]学历
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_90DAY_CALLING_TIMES", VB_VALUE = summaryEntity.DAY90_CALLING_TIMES != null ? ((int)summaryEntity.DAY90_CALLING_TIMES).ToString() : "0" });//[评分_手机]90天内主叫次数
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_CALLED_PHONE_CNT", VB_VALUE = summaryEntity.CALLED_PHONE_CNT != null ? ((int)summaryEntity.CALLED_PHONE_CNT).ToString() : "0" });//[评分_手机]被叫联系人个数
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_LOCAL_CALL_TIME", VB_VALUE = summaryEntity.LOCAL_CALL_TIME != null ? ((decimal)summaryEntity.LOCAL_CALL_TIME).ToString() : "0" });//[评分_手机]本地通话时长汇总
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_180_CALLING_SUBTTL", VB_VALUE = summaryEntity.DAY180_CALLING_SUBTTL != null ? ((decimal)summaryEntity.DAY180_CALLING_SUBTTL).ToString() : "0" });//[评分_手机]近180天内累计主叫套餐外话费
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_90DAY_CALL_TTL_TIME", VB_VALUE = summaryEntity.DAY90_CALL_TTL_TIME != null ? ((decimal)summaryEntity.DAY90_CALL_TTL_TIME).ToString() : "0" });//[评分_手机]近90天内累计通话时长
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_90DAY_CALL_TIMES", VB_VALUE = summaryEntity.DAY90_CALL_TIMES != null ? ((int)summaryEntity.DAY90_CALL_TIMES).ToString() : "0" });//[评分_手机]近90天内通话次数
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_90DAY_CALLING_TTL_TIME", VB_VALUE = summaryEntity.DAY90_CALLING_TTL_TIME != null ? ((decimal)summaryEntity.DAY90_CALLING_TTL_TIME).ToString() : "0" });//[评分_手机]近90天内主叫通话时长
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_NET_LSTM6_ONL_FLOW", VB_VALUE = summaryEntity.NET_LSTM6_ONL_FLOW != null ? ((decimal)summaryEntity.NET_LSTM6_ONL_FLOW).ToString() : "0" });//[评分_手机]近六月内累计上网流量
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_DAY_CALLING_TTL_TIME", VB_VALUE = summaryEntity.DAY_CALLING_TTL_TIME != null ? ((decimal)summaryEntity.DAY_CALLING_TTL_TIME).ToString() : "0" });//[评分_手机]累计白天主叫时长(9:00-18:00)
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_CALLED_TIMES", VB_VALUE = summaryEntity.CALLED_TIMES != null ? ((int)summaryEntity.CALLED_TIMES).ToString() : "0" });//[评分_手机]累计被叫次数
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_CALLED_TTL_TIME", VB_VALUE = summaryEntity.CALLED_TTL_TIME != null ? ((decimal)summaryEntity.CALLED_TTL_TIME).ToString() : "0" });//[评分_手机]累计被叫时长
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_MRNG_CALLED_TIMES", VB_VALUE = summaryEntity.MRNG_CALLED_TIMES != null ? ((int)summaryEntity.MRNG_CALLED_TIMES).ToString() : "0" });//[评分_手机]累计上午被叫次数(9:00-13:00)
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_CALL_TTL_TIME", VB_VALUE = summaryEntity.CALL_TTL_TIME != null ? ((decimal)summaryEntity.CALL_TTL_TIME).ToString() : "0" });//[评分_手机]累计通话时长
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_NIGHT_CALLED_TTL_TIME", VB_VALUE = summaryEntity.NIGHT_CALLED_TTL_TIME != null ? ((decimal)summaryEntity.NIGHT_CALLED_TTL_TIME).ToString() : "0" });//[评分_手机]累计晚间被叫时长(18:00-23:00)
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_AFTN_CALL_TTL_TIME", VB_VALUE = summaryEntity.AFTN_CALL_TTL_TIME != null ? ((decimal)summaryEntity.AFTN_CALL_TTL_TIME).ToString() : "0" });//[评分_手机]累计下午通话时长(13:00-18:00)
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_AFTN_CALLING_TTL_TIME", VB_VALUE = summaryEntity.AFTN_CALLING_TTL_TIME != null ? ((decimal)summaryEntity.AFTN_CALLING_TTL_TIME).ToString() : "0" });//[评分_手机]累计下午主叫时长(13:00-18:00)
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_NIGHT_CALL_TTL_TIME", VB_VALUE = summaryEntity.NIGHT_CALL_TTL_TIME != null ? ((decimal)summaryEntity.NIGHT_CALL_TTL_TIME).ToString() : "0" });//[评分_手机]累计夜晚通话时长(23:00-9:00)
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_NIGHT_CALLING_TTL_TIME", VB_VALUE = summaryEntity.NIGHT_CALLING_TTL_TIME != null ? ((decimal)summaryEntity.NIGHT_CALLING_TTL_TIME).ToString() : "0" });//[评分_手机]累计夜晚主叫时长(23:00-9:00)
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_CALLING_TTL_TIME", VB_VALUE = summaryEntity.CALLING_TTL_TIME != null ? ((decimal)summaryEntity.CALLING_TTL_TIME).ToString() : "0" });//[评分_手机]累计主叫时长
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_PH_USE_MONS", VB_VALUE = summaryEntity.PH_USE_MONS != null ? ((decimal)summaryEntity.PH_USE_MONS).ToString() : "0" });//[评分_手机]手机开卡时长至申请时间月数间隔
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_CALL_PHONE_CNT", VB_VALUE = summaryEntity.CALL_PHONE_CNT != null ? ((int)summaryEntity.CALL_PHONE_CNT).ToString() : "0" });//[评分_手机]所有联系人个数
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_CTT_DAYS_CNT", VB_VALUE = summaryEntity.CTT_DAYS_CNT != null ? ((int)summaryEntity.CTT_DAYS_CNT).ToString() : "0" });//[评分_手机]通话天数
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_CALLED_CTT_DAYS_CNT", VB_VALUE = summaryEntity.CALLED_CTT_DAYS_CNT != null ? ((int)summaryEntity.CALLED_CTT_DAYS_CNT).ToString() : "0" });//[评分_手机]有被叫的天数
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_CALLING_CTT_DAYS_CNT", VB_VALUE = summaryEntity.CALLING_CTT_DAYS_CNT != null ? ((int)summaryEntity.CALLING_CTT_DAYS_CNT).ToString() : "0" });//[评分_手机]有主叫的通话天数
                xmllist.Add(new VBXML { VB_COL = "SCR_CELL_MAX_PLAN_AMT", VB_VALUE = summaryEntity.MAX_PLAN_AMT != null ? ((decimal)summaryEntity.MAX_PLAN_AMT).ToString() : "0" });//[评分_手机]最大套餐金额

                rc = rcServcie.GetRuleResultByCustomWithIdentityNoCallFrom(ServiceConstants.RC_Model_KKD, Req.Identitycard, "", ServiceConstants.RC_Model_KKD_MobileScore, SerializationHelper.SerializeToXml(xmllist));
                decisionResult = rc.DeserializeXML<List<DecisionResult>>().FirstOrDefault();
                if (decisionResult != null)
                {
                    if (decisionResult.RuleResultCanShowSets.ContainsKey(ServiceConstants.RC_Result_KKD_MobileScore))
                    {
                        mobileScore = decisionResult.RuleResultCanShowSets[ServiceConstants.RC_Result_KKD_MobileScore].ToInt(0);
                    }
                }
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("评分计算失败", e);
            }

            return mobileScore;
        }
        #endregion

    }
}