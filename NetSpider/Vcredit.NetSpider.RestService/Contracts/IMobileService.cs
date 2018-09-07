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
    interface IMobileService
    {
        #region 手机账单查询
        [WebGet(UriTemplate = "/init/{mobileNo}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("手机账单查询初始化")]
        VerCodeRes MobilInitForXml(string mobileNo);

        [WebGet(UriTemplate = "/init/{mobileNo}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("手机账单查询初始化")]
        VerCodeRes MobilInitForJson(string mobileNo);


        [WebInvoke(UriTemplate = "/init/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("手机账单查询初始化")]
        VerCodeRes MobilInitJson(Stream stream);


        [WebInvoke(UriTemplate = "/login/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("手机账单查询登录")]
        BaseRes MobileLoginForXml(Stream stream);

        [WebInvoke(UriTemplate = "/login/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("手机账单查询登录")]
        BaseRes MobileLoginForJson(Stream stream);

        [WebInvoke(UriTemplate = "/sendsms/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("手机账单查询发送短信验证码")]
        VerCodeRes MobileSendSmsForXml(Stream stream);

        [WebInvoke(UriTemplate = "/sendsms/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("手机账单查询发送短信验证码")]
        VerCodeRes MobileSendSmsForJson(Stream stream);

        [WebInvoke(UriTemplate = "/checksms/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("手机账单查询校验短信验证码")]
        BaseRes MobileCheckSmsForXml(Stream stream);

        [WebInvoke(UriTemplate = "/checksms/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("手机账单查询校验短信验证码")]
        BaseRes MobileCheckSmsForJson(Stream stream);
        #endregion

        #region 手机信息查询

        [WebInvoke(UriTemplate = "/query/basic/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("手机账单查询")]
        BaseRes MobileQueryForXml(Stream stream);

        [WebInvoke(UriTemplate = "/query/basic/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("手机账单查询")]
        BaseRes MobileQueryForJson(Stream stream);

        [WebInvoke(UriTemplate = "/query/summary/fromtoken/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("手机账单汇总信息查询")]
        BaseRes MobileSummaryQueryFromTokenForXml(Stream stream);

        [WebInvoke(UriTemplate = "/query/summary/fromtoken/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("手机账单汇总信息查询")]
        BaseRes MobileSummaryQueryFromTokenForJson(Stream stream);

        [WebInvoke(UriTemplate = "/query/summary/old/fromtoken/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("手机账单汇总信息查询")]
        BaseRes MobileSummaryOldQueryFromTokenForJson(Stream stream);

        [WebInvoke(UriTemplate = "/query/summary/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("手机账单汇总信息查询")]
        BaseRes MobileSummaryQueryForXml(Stream stream);

        [WebInvoke(UriTemplate = "/query/summary/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("手机账单汇总信息查询")]
        BaseRes MobileSummaryQueryForJson(Stream stream);

        [WebGet(UriTemplate = "/mobilecatname/{mobile}/Xml", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Xml)]
        [Description("手机运行商查询")]
        BaseRes MobileCatNameForXml(string mobile);

        [WebGet(UriTemplate = "/mobilecatname/{mobile}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("手机运行商查询")]
        BaseRes MobileCatNameForJson(string mobile);

        [WebInvoke(UriTemplate = "/query/calls/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("手机通话清单查询")]
        BaseRes MobileCallForJson(Stream stream);

        #region 根据sourceid获取

        [WebInvoke(UriTemplate = "/query/summaryById/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("手机变量汇总信息查询")]
        BaseRes MobileVariableSummaryQueryForXml(Stream stream);

        [WebInvoke(UriTemplate = "/query/summaryById/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("手机变量汇总信息查询")]
        BaseRes MobileVariableSummaryQueryForJson(Stream stream);


        [WebInvoke(UriTemplate = "/query/GetCrawlerStateById/Xml", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("根据ID，获取采集状态")]
        BaseRes MobileCrawlerStateQueryForXml(Stream stream);

        [WebInvoke(UriTemplate = "/query/GetCrawlerStateById/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("根据ID，获取采集状态")]
        BaseRes MobileCrawlerStateQueryForJson(Stream stream);


        [WebInvoke(UriTemplate = "/query/basicById/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("根据ID，获取手机基本信息")]
        BaseRes MobileQueryByIdForXml(Stream stream);

        [WebInvoke(UriTemplate = "/query/basicById/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("根据ID，获取手机基本信息")]
        BaseRes MobileQueryByIdForJson(Stream stream);


        [WebInvoke(UriTemplate = "/query/callsById/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("根据ID，获取手机通话清单查询")]
        BaseRes MobileCallByIdForXml(Stream stream);

        [WebInvoke(UriTemplate = "/query/callsById/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("根据ID，获取手机通话清单查询")]
        BaseRes MobileCallByIdForJson(Stream stream);

        #endregion

        #endregion

        #region 手机信息设置

        [WebInvoke(UriTemplate = "/mobilesetting/save/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("手机运行商信息保存")]
        BaseRes MobileSetingSave(Stream stream);

        [WebInvoke(UriTemplate = "/mobilesetting/update/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("手机运行商信息更新")]
        BaseRes MobileSetingUpdate(Stream stream);

        [WebInvoke(UriTemplate = "/mobilesetting/GetMobileSeting/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取手机运行商信息")]
        BaseRes GetMobileSeting(Stream stream);

        #endregion

        #region 抓取步骤原始数据

        [WebInvoke(UriTemplate = "/originalhtml/save/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("手机运行商信息保存")]
        BaseRes OriginalHtmlSave(Stream stream);

        [WebInvoke(UriTemplate = "/originalhtml/update/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("手机运行商信息保存")]
        BaseRes OriginalHtmlUpdate(Stream stream);

        #endregion

        #region 重置密码

        [WebInvoke(UriTemplate = "/resetInit/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("手机密码重置初始化")]
        VerCodeRes ResetInitJson(Stream stream);

        [WebInvoke(UriTemplate = "/resetsendsms/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("发送动态验证码")]
        VerCodeRes ResetSendSmsJson(Stream stream);

        [WebInvoke(UriTemplate = "/resetPassword/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("手机密码重置")]
        BaseRes ResetPassWordJson(Stream stream);

        #endregion

        #region 其他

        [WebInvoke(UriTemplate = "/mobilesetting/GetMobileSetingForWebsite/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取手机运行商信息")]
        BaseRes GetMobileSetingForWebsite(string website);

        [WebInvoke(UriTemplate = "/query/GetCrawlerState/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取采集状态")]
        BaseRes GetCrawlerState(Stream stream);

        [WebInvoke(UriTemplate = "/query/GetCollectRecords/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取采集记录")]
        BaseRes GetCollectRecords(Stream stream);


        [WebInvoke(UriTemplate = "/mobilesetting/GetLogSeting/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取日志记录信息")]
        BaseRes GetLogSeting(Stream stream);

        #endregion
    }
}
