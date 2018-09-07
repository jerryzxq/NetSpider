using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity
{
    public class ServiceConsts
    {
        #region StatusCode
        public const int StatusCode_success = 0;
        public const int StatusCode_fail = 1;
        public const int StatusCode_needvercode = 5;
        public const int StatusCode_error = 110;
        public const int StatusCode_httpfail = 500;

        #region 征信
        public const int StatusCode_NetCredit_NoQuestion = 2;
        public const int StatusCode_NetCredit_ExistUser = 3;
        #endregion
        #endregion

        #region StatusDescription
        public const string StatusDescription_httpfail = "HTTP请求失败";
        public const string StatusDescription_Timeout = "请求超时";
        #endregion

        #region ProvidentFund
        public const string ProvidentFund_PaymentFlag_Normal = "正常缴费";
        public const string ProvidentFund_PaymentFlag_Adjust = "调整";
        public const string ProvidentFund_PaymentFlag_Draw = "支取";
        public const string ProvidentFund_PaymentFlag_Back = "补缴";
        public const string ProvidentFund_PaymentType_Normal = "正常";
        public const string ProvidentFund_PaymentType_Adjust = "调整";
        public const string ProvidentFund_PaymentType_Draw = "支取";
        public const string ProvidentFund_PaymentType_Back = "补缴";
        #region 提示
        public const string ProvidentFund_QueryFail = "公积金网网络异常，查询失败";
        public const string ProvidentFund_QuerySuccess = "公积金查询成功";
        public const string ProvidentFund_QueryError = "公积金查询异常";

        public const string ProvidentFund_InitFail = "公积金初始化异常";
        public const string ProvidentFund_InitSuccess = "公积金初始化成功";
        public const string ProvidentFund_InitError = "公积金初始化异常";
        #endregion
        #endregion

        #region SocialSecurity
        public const string SocialSecurity_PaymentFlag_Normal = "正常缴费";
        public const string SocialSecurity_PaymentFlag_Adjust = "调整";
        public const string SocialSecurity_PaymentFlag_Back = "补缴";
        public const string SocialSecurity_PaymentType_Normal = "正常";
        public const string SocialSecurity_PaymentType_Adjust = "调整";
        public const string SocialSecurity_PaymentType_Back = "补缴";

        #region 提示
        public const string SocialSecurity_QueryFail = "社保网网络异常，查询失败";
        public const string SocialSecurity_QuerySuccess = "社保查询成功";
        public const string SocialSecurity_QueryError = "社保查询异常";

        public const string SocialSecurity_InitFail = "社保网网络异常，初始化失败";
        public const string SocialSecurity_InitSuccess = "社保初始化成功";
        public const string SocialSecurity_InitError = "社保初始化异常";
        #endregion
        #endregion

        #region BusType
        public const string BusType_KAKADAI = "KAKADAI";
        public const string BusType_LIRENDAIO2O = "LIRENDAIO2O";
        public const string BusType_XXQD = "XXQD";
        public const string BusType_DOUDOUQIAN = "doudouqian";
        public const string BusType_PAZJD = "PAZJD";
        #endregion

        #region SpiderType
        public const string SpiderType_Mobile = "mobile";
        public const string SpiderType_JxlMobile = "jxlmobile";
        public const string SpiderType_Shebao = "shebao";
        public const string SpiderType_HousingFund = "housingfund";
        public const string SpiderType_Chsiquery = "chsiquery";
        public const string SpiderType_JxlEdu = "jxledu";
        public const string SpiderType_JxlMobileReset = "jxlmobilereset";
        #endregion

        #region ApplyStatus
        public const int ApplyStatus_Success = 0;
        public const int ApplyStatus_Fail = 1;
        #endregion

        #region CrawlStatus
        public const int CrawlStatus_Success = 0;
        public const int CrawlStatus_Fail = 4;
        public const int CrawlStatus_Mobile_InProcess = 200;
        public const int CrawlStatus_Mobile_LoginSuccess =100;
        public const int CrawlStatus_Mobile_Success = 201;
        #endregion

        #region WebSite
        public const string WebSite_XueXin = "xuexin";
        #endregion

        #region OperationLog
        /// <summary>
        /// 聚信立
        /// </summary>
        public const string OperationLog_Source_jxl = "jxl";
        /// <summary>
        /// 快批
        /// </summary>
        public const string OperationLog_Source_kp = "kp";
        #endregion

        #region 提示
        public const string RequiredEmpty = "缺必填项";
        public const string Required_IdentitycardEmpty = "身份证号不能为空";
        public const string Required_PhotoEmpty = "照片不能为空";
        public const string UserNameOrPasswordEmpty = "用户名或密码不能为空";
        public const string IdentitycardOrPasswordEmpty = "身份证号或密码不能为空";
        public const string IdentitycardOrNameEmpty = "身份证号或姓名不能为空";
        public const string Mobile_MobileOrPasswordEmpty = "手机号或密码不能为空";
        public const string IdentitycardOrCitizencardEmpty = "身份证号或个人编号不能为空";
        public const string UsernameOrNameEmpty = "用户名或姓名不能为空";
        public const string CitizencardOrPasswordEmpty = "社保卡号或密码不能为空";
        public const string IdentitycardOrPasswordOrPasswordEmpty = "身份证号或个人编号或密码不能为空";
        public const string IdentitycardOrPasswordOrCompanyEmpty = "身份证号或单位名称或密码不能为空";
        public const string IdentityCardOrNameOrMobileEmpty = "身份证号或姓名或手机号不能为空";
        public const string MobileEmpty = "手机号不能为空";
        public const string TokenOrMobileEmpty = "Token或手机号不能为空";
        public const string TokenOrMobileOrSmscodeEmpty = "Token或手机号或动态验证码不能为空";
        public const string TokenOrIdentityCardOrNameOrMobileOrPwdEmpty = "Token或身份证号或姓名或手机号或服务密码不能为空";
        public const string Crawlering = "抓取中";
        public const string Analysising = "解析中";

        #endregion

        #region 社保、公积金城市编码
        public const string CityCode_Shanghai = "shanghai";
        public const string CityCode_Qingdao = "qingdao";
        public const string CityCode_Chengdu = "chengdu";
        public const string CityCode_Beijing = "beijing";
        public const string CityCode_Nanjing = "nanjing";
        public const string CityCode_Guangzhou = "guangzhou";
        public const string CityCode_Chongqing = "chongqing";
        public const string CityCode_Xiamen = "xiamen";
        public const string CityCode_Shenzhen = "shenzhen";
        public const string CityCode_Hangzhou = "hangzhou";
        public const string CityCode_Wuxi = "wuxi";
        public const string CityCode_Ningbo = "ningbo";
        public const string CityCode_Suzhou = "suzhou";
        public const string CityCode_Dalian = "dalian";
        public const string CityCode_Fuzhou = "fuzhou";
        #endregion

        #region 手机抓取步骤
        
        /// <summary>
        /// 初始化
        /// </summary>
        public const string NextProCode_Init = "Init";

        /// <summary>
        /// 验证码
        /// </summary>
        public const string NextProCode_Vercode = "Vercode";

        /// <summary>
        /// 登录
        /// </summary>
        public const string NextProCode_Login = "Login";
        /// <summary>
        /// 发送短信
        /// </summary>
        public const string NextProCode_SendSMS = "SendSMS";

        /// <summary>
        /// 发送服务密码
        /// </summary>
        public const string NextProCode_ServicePassword = "ServicePassword";

        /// <summary>
        /// 发送服务密码和短信
        /// </summary>
        public const string NextProCode_SendSMSAndVercode = "SendSMSAndVercode";

        /// <summary>
        /// /验证短信
        /// </summary>
        public const string NextProCode_CheckSMS = "CheckSMS";

        /// <summary>
        /// 查询账单
        /// </summary>
        public const string NextProCode_Query = "Query";


        #endregion

        #region 手机抓取状态（CrawlerStatusCode）

        /// <summary>
        /// 抓取中
        /// </summary>
        public const int CrawlerStatusCode_Crawlering = 100;
        /// <summary>
        /// 抓取成功
        /// </summary>
        public const int CrawlerStatusCode_CrawlerSuccess = 101;
        /// <summary>
        /// 抓取失败
        /// </summary>
        public const int CrawlerStatusCode_CrawlerFail = 102;
        /// <summary>
        /// 解析中
        /// </summary>
        public const int CrawlerStatusCode_Analysising = 200;
        /// <summary>
        /// 解析成功
        /// </summary>
        public const int CrawlerStatusCode_AnalysisSuccess = 201;
        /// <summary>
        /// 解析失败
        /// </summary>
        public const int CrawlerStatusCode_AnalysisFail = 202;
        /// <summary>
        /// 解析成功但没有通话详单
        /// </summary>
        public const int CrawlerStatusCode_AnalysisFail_NoCalls = 20101;
        /// <summary>
        /// 解析成功但没有短信详单
        /// </summary>
        public const int CrawlerStatusCode_AnalysisFail_NoSmss = 20102;
        /// <summary>
        /// 解析成功但没有流量详单
        /// </summary>
        public const int CrawlerStatusCode_AnalysisFail_NoNets = 20103;
        /// <summary>
        /// 检验成功
        /// </summary>
        public const int CrawlerStatusCode_CheckSuccess = 10001;
        /// <summary>
        /// 字段异常
        /// </summary>
        public const int CrawlerStatusCode_ColmonError = 10002;
        /// <summary>
        /// 密码错误
        /// </summary>
        public const int CrawlerStatusCode_PasswordError = 11001;
        /// <summary>
        /// 验证码错误
        /// </summary>
        public const int CrawlerStatusCode_VercodeError = 11002;
        /// <summary>
        /// 动态密码错误
        /// </summary>
        public const int CrawlerStatusCode_SmsCodeError = 11003;
        /// <summary>
        /// 动态密码失效
        /// </summary>
        public const int CrawlerStatusCode_SmsCodeInvalidation = 11004;
        /// <summary>
        /// 简单密码或初始密码无法登录
        /// </summary>
        public const int CrawlerStatusCode_PasswordEasy = 11005;
        /// <summary>
        /// 账户被锁
        /// </summary>
        public const int CrawlerStatusCode_AccountLock = 11006;
        /// <summary>
        /// 登录保护
        /// </summary>
        public const int CrawlerStatusCode_AccountProtect = 11007;
        /// <summary>
        /// 运行商升级
        /// </summary>
        public const int CrawlerStatusCode_SystemUpdate = 12001;
        /// <summary>
        /// 运行商繁忙
        /// </summary>
        public const int CrawlerStatusCode_SystemBusy = 12002;
        /// <summary>
        /// 重置密码成功
        /// </summary>
        public const int CrawlerStatusCode_ResetPWDSuccess = 13000;
        /// <summary>
        /// 重置密码失败
        /// </summary>
        public const int CrawlerStatusCode_ResetPWDFail = 13001;
        /// <summary>
        /// 不支持重置密码
        /// </summary>
        public const int CrawlerStatusCode_NoReset = 13100;
        /// <summary>
        /// 短信验证码重置
        /// </summary>
        public const int CrawlerStatusCode_SmsCodeReset = 13101;
        /// <summary>
        /// 短信验证码和服务密码重置
        /// </summary>
        public const int CrawlerStatusCode_SmsServiceCodeReset = 13102;
        /// <summary>
        /// 短信验证码和服务密码和3个5天以前一个月以内有通话记录的手机号重置
        /// </summary>
        public const int CrawlerStatusCode_SmsServiceCodeOtherReset = 13103;

        #endregion

        #region 日志

        /// <summary>
        /// 必要信息监控日志
        /// </summary>
        public const string Step_Column_Monitoring = "Column_Monitoring";

        /// <summary>
        /// 初始化
        /// </summary>
        public const string Step_Init = "Init";

        /// <summary>
        /// 验证码
        /// </summary>
        public const string Step_Vercode = "Vercode";

        /// <summary>
        /// 登录
        /// </summary>
        public const string Step_Login = "Login";

        /// <summary>
        /// 发送短信
        /// </summary>
        public const string Step_SendSMS = "SendSMS";

        /// <summary>
        /// 验证短信
        /// </summary>
        public const string Step_CheckSMS = "CheckSMS";

        /// <summary>
        /// 基本信息
        /// </summary>
        public const string Step_Infor_Crawler = "Infor_Crawler";

        /// <summary>
        /// 月账单
        /// </summary>
        public const string Step_Bill_Crawler = "Bill_Crawler";

        /// <summary>
        /// 通话详单
        /// </summary>
        public const string Step_Call_Crawler = "CallDtl_Crawler";

        /// <summary>
        /// 短信账单
        /// </summary>
        public const string Step_Sms_Crawler = "SmsDtl_Crawler";

        /// <summary>
        /// 流量账单
        /// </summary>
        public const string Step_Net_Crawler = "NetDtl_Crawler";

        /// <summary>
        /// 基本信息
        /// </summary>
        public const string Step_Infor_Analysis = "Infor_Analysis";

        /// <summary>
        /// 月账单
        /// </summary>
        public const string Step_Bill_Analysis = "Bill_Analysis";

        /// <summary>
        /// 通话详单
        /// </summary>
        public const string Step_Call_Analysis = "CallDtl_Analysis";

        /// <summary>
        /// 短信账单
        /// </summary>
        public const string Step_Sms_Analysis = "SmsDtl_Analysis";

        /// <summary>
        /// 流量账单
        /// </summary>
        public const string Step_Net_Analysis = "NetDtl_Analysis";

        /// <summary>
        /// 重置密码初始化
        /// </summary>
        public const string Step_ResetInit = "ResetInit";

        /// <summary>
        /// 重置密码发送短信
        /// </summary>
        public const string Step_ResetSendSMS = "ResetSendSMS";

        /// <summary>
        /// 重置密码提交
        /// </summary>
        public const string Step_ResetCheck = "ResetCheck";


        #endregion

        #region Result
        public const string BaseRusult_True = "true";
        public const string BaseRusult_False = "false";
        #endregion
    }
}
