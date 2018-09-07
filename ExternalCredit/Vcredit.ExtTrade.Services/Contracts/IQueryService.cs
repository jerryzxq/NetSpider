using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Web;
using Vcredit.ExtTrade.ModelLayer;
using System.IO;
using System.ComponentModel;

namespace Vcredit.ExtTrade.Services
{
     [ServiceContract]
    public interface IQueryService
    {
		[OperationContract]
		[WebGet(UriTemplate = "/Get/ReportIdAndReportSn/{identityno}", ResponseFormat = WebMessageFormat.Json)]
		[Description("根据身份号获取reportid和reportsn")]
		BaseRes GetReportIdAndReportSnByIdentityNo(string identityno);

		[OperationContract]
		[WebGet(UriTemplate = "/Get/ReportState/{identityno}", ResponseFormat = WebMessageFormat.Json)]
		[Description("根据身份号获取提交报告状态")]
		BaseRes GetReportStateByIdentityNo(string identityno);

        [WebInvoke(UriTemplate = "/summary", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("机构版征信衍生变量查询")]
        BaseRes GetExtCreditSummary(Stream creditInfo);


        [WebInvoke(UriTemplate = "/RequestRecord", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("机构版征信申请记录查询查询")]
        BaseRes QueryExtCreditReqest(Stream request);


        [OperationContract]
        [WebInvoke(UriTemplate = "/BusTypeEnum", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取表中所有业务类型")]
        BaseRes GetBusTypeEnum(Stream request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/StateInfo", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("状态征信状态信息")]
        BaseRes GetStateInfo(Stream request);


        [OperationContract]
        [WebInvoke(UriTemplate = "/ReportInfo", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("报告信息")]
        BaseRes GetReportInfo(Stream request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/ReportInfoNoLimit", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("报告信息（无时间限制）")]
        BaseRes GetReportInfoNoLimit(Stream request);


        [OperationContract]
        [WebInvoke(UriTemplate = "/IdentityInfo", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("征信用户信息")]
        BaseRes GetIdentityInfo(Stream request);


        [OperationContract]
        [WebInvoke(UriTemplate = "/CreditInfo", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("征信信息")]
        BaseRes  QueryCreditInfo(Stream request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/CreditAddressInfo", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取征信地址信息")]
        BaseRes QueryCreditAddressInfo(Stream request);


        [OperationContract]
        [WebInvoke(UriTemplate = "/CreditAddress", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取征信地址信息")]
        BaseRes QueryCreditAddressByCert_NO(Stream request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/ForceExctn", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取强制执行数量")]
        BaseRes QueryForceExctnNum(Stream request);

    }
}