using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Cn.Vcredit.VBS.Model;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.WeiXin.RestService.Models;

namespace Vcredit.WeiXin.RestService.Contracts
{
    [ServiceContract]
    interface IKKDService
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

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/result/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信,申请查询码结果查询")]
        BaseRes PbccrcReportQueryApplicationResultForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/result/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信,申请查询码结果查询")]
        BaseRes PbccrcReportQueryApplicationResultForJson(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/report/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信查询")]
        BaseRes GetPbccrcReportInfoForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/report/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信查询")]
        BaseRes GetPbccrcReportForJson(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/querycode/sendagain/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信查询")]
        BaseRes GetPbccrcReportSendQuerycode(Stream stream);

        #region 通过银联卡获取查询码
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
        #endregion

        #region 获取银联卡认证码
        [WebInvoke(UriTemplate = "/pbccrc/query/apply/unionpay/init/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信,申请信用信息查询")]
        BaseRes PbccrcQueryApplicationGetUnionPayCodeInitForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/unionpay/init/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信,申请信用信息查询")]
        BaseRes PbccrcQueryApplicationGetUnionPayCodeInitForJson(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/unionpay/check/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信,申请信用信息查询")]
        BaseRes PbccrcQueryApplicationGetUnionPayCodeStep1ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/unionpay/check/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信,申请信用信息查询")]
        BaseRes PbccrcQueryApplicationGetUnionPayCodeStep1ForJson(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/unionpay/sendsms/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信,申请信用信息查询")]
        BaseRes PbccrcQueryApplicationGetUnionPayCodeStep2ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/unionpay/sendsms/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信,申请信用信息查询")]
        BaseRes PbccrcQueryApplicationGetUnionPayCodeStep2ForJson(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/unionpay/submit/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信,申请信用信息查询")]
        BaseRes PbccrcQueryApplicationGetUnionPayCodeStep3ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/apply/unionpay/submit/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信,申请信用信息查询")]
        BaseRes PbccrcQueryApplicationGetUnionPayCodeStep3ForJson(Stream stream);
        #endregion
        #endregion

        #region 决策系统对接
        [WebInvoke(UriTemplate = "/rc/credit/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("卡卡贷全国授信")]
        BaseRes GetRCCreditForJson(Stream stream);

        [WebInvoke(UriTemplate = "/rc/zhice/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("卡卡贷智策版")]
        BaseRes GetRCZhiCeForJson(Stream stream);

        [WebInvoke(UriTemplate = "/rc/social/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("卡卡贷提升额度, 社保版")]
        BaseRes GetRCWithSocial(Stream stream);
        #endregion

        #region 聚信立手机账单相关接口
        [WebInvoke(UriTemplate = "/jxl/mobile/isauth/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("校验是否实名认证")]
        BaseRes JxlIsAuthForJson(Stream stream);
        #endregion

        #region 从数据库查询数据给VBS展示
        [WebGet(UriTemplate = "/vbs/shebao/query/{bid}/Json", ResponseFormat = WebMessageFormat.Json)]
        [Description("社保数据从数据库查询")]
        SocialSecurityQueryRes SocialSecurityQueryFromDBForJson(string bid);

        [WebGet(UriTemplate = "/vbs/gjj/query/{bid}/Json", ResponseFormat = WebMessageFormat.Json)]
        [Description("公积金数据从数据库查询")]
        ProvidentFundQueryRes ProvidentFundQueryFromDBForJson(string bid);

        [WebGet(UriTemplate = "/vbs/pbccrc/query/{bid}/Json", ResponseFormat = WebMessageFormat.Json)]
        [Description("征信报告数据从数据库查询")]
        CRD_HD_REPORTRes GetPbccrcReportFromDBForJson(string bid);

        [WebGet(UriTemplate = "/vbs/mobile/filepath/{bid}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("获取手机账单报告目录")]
        object GetMobileReceiveFilePathFromDBForJson(string bid);

        [WebGet(UriTemplate = "/vbs/mobile/summary/{bid}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("获取手机账单")]
        BaseRes GetMobileSummaryFromDBForJson(string bid);

        [WebGet(UriTemplate = "/vbs/mobile/summary/{identitycard}/{mobile}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("获取手机账单")]
        BaseRes GetMobileSummaryByIdentityCardForJson(string identitycard, string mobile);

        [WebGet(UriTemplate = "/vbs/mobile/jxlreport/{bid}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("获取聚信立手机账单报告")]
        BaseRes GetMobileReportForJson(string bid);

        [WebGet(UriTemplate = "/vbs/mobile/jxlreport/{identitycard}/{mobile}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("获取聚信立手机账单报告")]
        BaseRes GetMobileReportByIdentityCardForJson(string identitycard, string mobile);
        #endregion

        #region 身份证OCR解析
        [WebInvoke(UriTemplate = "/ocr/IdentityCard/byte/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("OCR解析身份证图片(二级制流)")]
        IdentityCard IdentityCardFromBytes(Stream stream);

        [WebInvoke(UriTemplate = "/ocr/IdentityCard/url/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("OCR解析身份证图片(图片链接)")]
        IdentityCard IdentityCardFromUrl(Stream stream);


        #endregion
    }
}
