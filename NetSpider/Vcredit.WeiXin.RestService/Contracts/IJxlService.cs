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
using Vcredit.WeiXin.RestService.Models.Res;

namespace Vcredit.WeiXin.RestService.Contracts
{
    [ServiceContract]
    interface IJxlService
    {
        [WebInvoke(UriTemplate = "/mobile/login/success/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("聚信立手机账单登录成功后回调")]
        BaseRes JxlSubmitSuccessForForXml(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/login/success/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("聚信立手机账单登录成功后回调")]
        BaseRes JxlSubmitSuccessForJson(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/query/isauth/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("校验是否实名认证")]
        BaseRes MobileIsAuthForXml(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/query/isauth/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("校验是否实名认证")]
        BaseRes MobileIsAuthForJson(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/query/summary/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("获取手机账单统计信息")]
        BaseRes GetMobileSummaryForXml(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/query/summary/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取手机账单统计信息")]
        BaseRes GetMobileSummaryForJson(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/query/report/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("获取聚信立手机账单报告")]
        BaseRes GetMobileReportForXml(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/query/report/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取聚信立手机账单报告")]
        BaseRes GetMobileReportForJson(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/query/blacklist/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("获取聚信立黑名单结果")]
        BaseRes GetBlacklistResultForXml(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/query/blacklist/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取聚信立黑名单结果")]
        BaseRes GetBlacklistResultForJson(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/query/calls/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("获取聚信立手机账单报告")]
        BaseRes GetMobileCallsOneMonthForXml(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/query/calls/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取聚信立手机账单报告")]
        BaseRes GetMobileCallsOneMonthForJson(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/query/myselfcalls/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("获取公司内部接口手机通话详单")]
        BaseRes GetMobileMySelfCallsForXml(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/query/myselfcalls/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取公司内部接口手机通话详单")]
        BaseRes GetMobileMySelfCallsForJson(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/query/usemonth/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("获取聚信立手机账单报告")]
        BaseRes GetMobileUseMonthForXml(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/query/usemonth/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取聚信立手机账单报告")]
        BaseRes GetMobileUseMonthForJson(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/query/isself/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("查询手机认证是否公司内部接口")]
        BaseRes GetMobileInfoIsSelfForXml(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/query/isself/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("查询手机认证是否公司内部接口")]
        BaseRes GetMobileInfoIsSelfForJson(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/pushdata/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取聚信立推送数据")]
        JxlPushDataRes JxlPustData(Stream stream);

        [WebInvoke(UriTemplate = "/mobile/pushreport/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取聚信立推送报告")]
        JxlPushDataRes JxlPustReport(Stream stream);

        /// <summary>
        /// 新聚信立手机账单推送数据
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        [WebInvoke(UriTemplate = "/kuaipi/pushdata/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取聚信立推送数据")]
        JxlPushDataRes JxlPushData(Stream stream);

        /// <summary>
        /// 新聚信立手机账单推送报告
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        [WebInvoke(UriTemplate = "/kuaipi/pushreport/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取聚信立推送报告")]
        JxlPushDataRes JxlPushReport(Stream stream);

        //[WebInvoke(UriTemplate = "/edu/geteducationinfo", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        //[Description("获取学历信息")]
        //BaseRes GetEducationInfo(Stream stream);
    }
}
