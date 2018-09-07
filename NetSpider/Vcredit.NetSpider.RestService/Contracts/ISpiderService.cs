using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.Mobile;

namespace Vcredit.NetSpider.RestService.Contracts
{
    [ServiceContract]
    interface ISpiderService
    {
        #region 数据采集服务初始化
        [WebGet(UriTemplate = "/init", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("数据采集服务初始化")]
        BaseRes SpiderServiceInit();
        #endregion

        #region 解析验证码
        [WebInvoke(UriTemplate = "/vercode/tesseract", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("解析验证码")]
        string GetVercodeByTesseract(Stream stream);
        #endregion

        #region 淘宝商家店铺
        [WebGet(UriTemplate = "/GetTaobaoSellerTotalAmount/{sellerUrl}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("淘宝商家店铺销售总额")]
        TaobaoSellerRes GetTaobaoSellerTotalAmountForJson(string sellerUrl);
        [WebGet(UriTemplate = "/GetTaobaoSellerTotalAmount/{sellerUrl}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("淘宝商家店铺销售总额")]
        TaobaoSellerRes GetTaobaoSellerTotalAmountForXml(string sellerUrl);
        [WebGet(UriTemplate = "/ExecutorSpiderTaobaoSeller/{sellerUrl}/{bid}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("开始执行抓取淘宝商家店铺")]
        BaseRes ExecutorSpiderTaobaoSellerForJson(string sellerUrl, string bid);
        [WebGet(UriTemplate = "/ExecutorSpiderTaobaoSeller/{sellerUrl}/{bid}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("开始执行抓取淘宝商家店铺")]
        BaseRes ExecutorSpiderTaobaoSellerForXml(string sellerUrl, string bid);
        #endregion

        #region 社保数据查询
        [WebGet(UriTemplate = "/12333/init/{city}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("社保数据查询登录页初始化")]
        BaseRes SocialSecurityInitForXml(string city);

        [WebGet(UriTemplate = "/12333/init/{city}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("社保数据查询登录页初始化")]

        BaseRes SocialSecurityInitForJson(string city);

        [WebGet(UriTemplate = "/12333/query/{city}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("执行社保数据查询")]
        SocialSecurityQueryRes SocialSecurityQueryForXml(string city);

        [WebGet(UriTemplate = "/12333/query/{city}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("执行社保数据查询")]
       
        SocialSecurityQueryRes SocialSecurityQueryForJson(string city);
        #endregion

        #region 手机账单查询
        //[WebGet(UriTemplate = "/mobile/init/{mobileNo}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        //[Description("手机账单查询初始化")]
        //VerCodeRes MobilInitForXml(string mobileNo);

        //[WebGet(UriTemplate = "/mobile/init/{mobileNo}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        //[Description("手机账单查询初始化")]
        //VerCodeRes MobilInitForJson(string mobileNo);

        //[WebInvoke(UriTemplate = "/mobile/login/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        //[Description("手机账单查询登录")]
        //BaseRes MobileLoginForXml(Stream stream);

        //[WebInvoke(UriTemplate = "/mobile/login/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        //[Description("手机账单查询登录")]
        //BaseRes MobileLoginForJson(Stream stream);

        //[WebInvoke(UriTemplate = "/mobile/sendsms/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        //[Description("手机账单查询发送短信验证码")]
        //BaseRes MobileSendSmsForXml(Stream stream);

        //[WebInvoke(UriTemplate = "/mobile/sendsms/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        //[Description("手机账单查询发送短信验证码")]
        //BaseRes MobileSendSmsForJson(Stream stream);

        //[WebInvoke(UriTemplate = "/mobile/checksms/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        //[Description("手机账单查询校验短信验证码")]
        //BaseRes MobileCheckSmsForXml(Stream stream);

        //[WebInvoke(UriTemplate = "/mobile/checksms/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        //[Description("手机账单查询校验短信验证码")]
        //BaseRes MobileCheckSmsForJson(Stream stream);

        //[WebInvoke(UriTemplate = "/mobile/query/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        //[Description("手机账单查询")]
        //BaseRes MobileQueryForXml(Stream stream);

        //[WebInvoke(UriTemplate = "/mobile/query/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        //[Description("手机账单查询")]
        //BaseRes MobileQueryForJson(Stream stream);
        #endregion

        #region 公积金查询
        [WebGet(UriTemplate = "/gjj/init/{city}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("社保数据查询登录页初始化")]
        BaseRes ProvidentFundInitForXml(string city);

        [WebGet(UriTemplate = "/gjj/init/{city}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("社保数据查询登录页初始化")]

        BaseRes ProvidentFundInitForJson(string city);

        [WebGet(UriTemplate = "/gjj/query/{city}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("执行公积金查询")]
        ProvidentFundQueryRes ProvidentFundQueryForXml(string city);

        [WebGet(UriTemplate = "/gjj/query/{city}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("执行公积金查询")]
        ProvidentFundQueryRes ProvidentFundQueryForJson(string city);
        #endregion

        #region 央行互联网征信
        [WebGet(UriTemplate = "/pbccrc/init/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("央行互联网征信初始化")]
        BaseRes PbccrcReportInitForXml();

        [WebGet(UriTemplate = "/pbccrc/init/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("央行互联网征信初始化")]
        BaseRes PbccrcReportInitForJson();


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

        [WebGet(UriTemplate = "/pbccrc/query/queryapply/1/Xml", ResponseFormat = WebMessageFormat.Xml)]
        [Description("央行互联网征信,申请信用信息查询")]
        BaseRes PbccrcReportQueryApplicationStep1ForXml();

        [WebGet(UriTemplate = "/pbccrc/query/queryapply/1/Json", ResponseFormat = WebMessageFormat.Json)]
        [Description("央行互联网征信,申请信用信息查询")]
        BaseRes PbccrcReportQueryApplicationStep1ForJson();

        [WebInvoke(UriTemplate = "/pbccrc/query/queryapply/2/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("央行互联网征信,申请信用信息查询")]
        BaseRes PbccrcReportQueryApplicationStep2ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/pbccrc/query/queryapply/2/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("央行互联网征信,申请信用信息查询")]
        BaseRes PbccrcReportQueryApplicationStep2ForJson(Stream stream);

        [WebGet(UriTemplate = "/pbccrc/query/report/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("央行互联网征信查询")]
        CRD_HD_REPORTRes GetPbccrcReportInfoForXml();

        [WebGet(UriTemplate = "/pbccrc/query/report/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("央行互联网征信查询")]
        CRD_HD_REPORTRes GetPbccrcReportForJson();
        #endregion

        #region 维信互联网认证
        [WebGet(UriTemplate = "/vcert/{sort}/{username}/{password}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("维信互联网认证")]
        BaseRes VcreditCertifyForXml(string sort, string username, string password);

        [WebGet(UriTemplate = "/vcert/{sort}/{username}/{password}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("维信互联网认证")]
        BaseRes VcreditCertifyForJson(string sort, string username, string password);
        #endregion
    }
}
