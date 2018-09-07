using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Vcredit.NetSpider.Entity.Service;

namespace Vcredit.NetSpider.RestService.Contracts
{
    [ServiceContract]
    interface ICreditService
    {
        #region 央行互联网征信
        [WebGet(UriTemplate = "/pbccrc/init/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("央行互联网征信初始化")]
        VerCodeRes PbccrcReportInitForXml();

        [WebGet(UriTemplate = "/pbccrc/init/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("央行互联网征信初始化")]
        VerCodeRes PbccrcReportInitForJson();


        [WebInvoke(UriTemplate = "/pbccrc/register/1/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信注册,第一步，输入基本信息")]
        BaseRes PbccrcReportRegisterStep1ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/register/1/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信注册,第一步，输入基本信息")]
        BaseRes PbccrcReportRegisterStep1ForJson(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/register/2/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信注册,第二步，发送手机验证码")]
        BaseRes PbccrcReportRegisterStep2ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/register/2/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信注册,第二步，发送手机验证码")]
        BaseRes PbccrcReportRegisterStep2ForJson(Stream stream);
        [WebInvoke(UriTemplate = "/pbccrc/register/3/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信注册,第三步，补充信息")]
        BaseRes PbccrcReportRegisterStep3ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/register/3/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信注册,第三步，补充信息")]
        BaseRes PbccrcReportRegisterStep3ForJson(Stream stream);


        [WebInvoke(UriTemplate = "/pbccrc/login/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信查询登录")]
        BaseRes PbccrcReportLoginForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/login/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信查询登录")]
        BaseRes PbccrcReportLoginForJson(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/1/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信,申请信用信息查询")]
        BaseRes PbccrcReportQueryApplicationStep1ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/1/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信,申请信用信息查询")]
        BaseRes PbccrcReportQueryApplicationStep1ForJson(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/2/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信,申请信用信息查询")]
        BaseRes PbccrcReportQueryApplicationStep2ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/2/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信,申请信用信息查询")]
        BaseRes PbccrcReportQueryApplicationStep2ForJson(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/bankcard/init/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信,申请信用信息查询")]
        VerCodeRes PbccrcReportQueryApplicationCreditStep1ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/bankcard/init/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信,申请信用信息查询")]
        VerCodeRes PbccrcReportQueryApplicationCreditStep1ForJson(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/bankcard/submit/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信,申请信用信息查询")]
        BaseRes PbccrcReportQueryApplicationCreditStep2ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/bankcard/submit/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信,申请信用信息查询")]
        BaseRes PbccrcReportQueryApplicationCreditStep2ForJson(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/result/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信,申请查询码结果查询")]
        BaseRes PbccrcReportQueryApplicationResultForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/result/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信,申请查询码结果查询")]
        BaseRes PbccrcReportQueryApplicationResultForJson(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/report/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信查询(数据采集)")]
        CRD_HD_REPORTRes GetPbccrcReportForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/report/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信查询(数据采集)")]
        CRD_HD_REPORTRes GetPbccrcReportForJson(Stream stream);

        //[WebInvoke(UriTemplate = "/pbccrc/blank/insert/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        //[Description("央行互联网征信填写征信空白")]
        //BaseRes PbccrcReportQueryApplicationAddBlankRecordForXml(Stream stream);

        //[WebInvoke(UriTemplate = "/pbccrc/blank/insert/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        //[Description("央行互联网征信填写征信空白")]
        //BaseRes PbccrcReportQueryApplicationAddBlankRecordForJson(Stream stream);

        [WebGet(UriTemplate = "/pbccrc/query/recorddtl/{reportid}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("查看征信查看记录")]
        BaseRes PbccrcReportQueryRecorddtlForJson(string reportid);
        [WebGet(UriTemplate = "/pbccrc/query/recorddtl/{reportid}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("查看征信查看记录")]
        BaseRes PbccrcReportQueryRecorddtlForXml(string reportid);
        #endregion

        #region 获取银联卡认证码
        [WebInvoke(UriTemplate = "/pbccrc/query/apply/unionpay/init/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信,获取银联卡认证码")]
        BaseRes PbccrcQueryApplicationGetUnionPayCodeInitForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/unionpay/init/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信,获取银联卡认证码")]
        BaseRes PbccrcQueryApplicationGetUnionPayCodeInitForJson(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/unionpay/check/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信,获取银联卡认证码")]
        BaseRes PbccrcQueryApplicationGetUnionPayCodeStep1ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/unionpay/check/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信,获取银联卡认证码")]
        BaseRes PbccrcQueryApplicationGetUnionPayCodeStep1ForJson(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/unionpay/sendsms/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信,获取银联卡认证码")]
        BaseRes PbccrcQueryApplicationGetUnionPayCodeStep2ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/unionpay/sendsms/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信,获取银联卡认证码")]
        BaseRes PbccrcQueryApplicationGetUnionPayCodeStep2ForJson(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/unionpay/submit/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信,获取银联卡认证码")]
        BaseRes PbccrcQueryApplicationGetUnionPayCodeStep3ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/unionpay/submit/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信,获取银联卡认证码")]
        BaseRes PbccrcQueryApplicationGetUnionPayCodeStep3ForJson(Stream stream);
        #endregion

        #region 网络版征信数据相关

        #region VBS相关接口
        [WebGet(UriTemplate = "/vbs/sync/netcredit/{reportid}", ResponseFormat = WebMessageFormat.Json)]
        [Description("同步数据到VBS")]
        BaseRes SyncCreditInfoToVBS(string reportid);

        [WebInvoke(UriTemplate = "/vbs/getcardage", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取最早卡龄")]
        BaseRes GetPbccrcCardAgeMonth(Stream stream);
        #endregion

        #region 公共数据接口

        #region
        [WebInvoke(UriTemplate = "/score/calculate/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信,评分计算")]
        BaseRes CreditScoreCalculateForXml(Stream stream);

        [WebInvoke(UriTemplate = "/score/calculate/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信,评分计算")]
        BaseRes CreditScoreCalculateForJson(Stream stream);

        [WebInvoke(UriTemplate = "/score/calculate/CBS201512/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信,评分计算,版本号为CBS201512")]
        BaseRes CreditScoreCalculateCBS201512ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/score/calculate/CBS201512/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信,评分计算,版本号为CBS20151")]
        BaseRes CreditScoreCalculateCBS201512ForJson(Stream stream);

        [WebInvoke(UriTemplate = "/score/calculate/CBS201603/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信,评分计算,版本号为CBS201603")]
        BaseRes CreditScoreCalculateCBS201603ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/score/calculate/CBS201603/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信,评分计算,版本号为CBS201603")]
        BaseRes CreditScoreCalculateCBS201603ForJson(Stream stream);
        #endregion

        [WebInvoke(UriTemplate = "/query/summary/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信查询(fromDB)")]
        BaseRes GetCreditSummaryForXml(Stream stream);

        [WebInvoke(UriTemplate = "/query/summary/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信查询(fromDB)")]
        BaseRes GetCreditSummaryForJson(Stream stream);

        [WebInvoke(UriTemplate = "/query/report/all/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信查询明细数据")]
        BaseRes GetCreditDataAllForJson(Stream stream);
        //[WebInvoke(UriTemplate = "/query/basic/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        //[Description("根据身份证号查询网络版征信基本信息")]
        //BaseRes GetCreditBasicForXml(Stream stream);

        //[WebInvoke(UriTemplate = "/query/basic/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        //[Description("根据身份证号查询网络版征信基本信息")]
        //BaseRes GetCreditBasicForJson(Stream stream);
        [WebGet(UriTemplate = "/query/report/{reportsn}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("央行互联网征信查询根据reportsn获取数据")]
        BaseRes GetCrdhdReportByReportSnForJson(string reportsn);

        [WebGet(UriTemplate = "/query/report/{reportsn}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("央行互联网征信查询根据reportsn获取数据")]
        BaseRes GetCrdhdReportByReportSnForXml(string reportsn);
        #endregion
        #endregion
    }
}
