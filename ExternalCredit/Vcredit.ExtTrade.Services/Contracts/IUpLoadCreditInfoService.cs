using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.EnterpriseServices;
using Vcredit.ExtTrade.ModelLayer;
using System.IO;
namespace Vcredit.ExtTrade.Services
{
     [ServiceContract]
    public interface ICreditInfoService
    {

        [OperationContract]
        [WebGet(UriTemplate = "/Get/ReportIdAndReportSn/{identityno}", ResponseFormat = WebMessageFormat.Json)]
        [Description("根据身份号获取reportid和reportsn")]
        BaseRes GetReportIdAndReportSnByIdentityNo(string identityno);

        [OperationContract]
        [WebGet(UriTemplate = "/Get/ReportState/{identityno}", ResponseFormat = WebMessageFormat.Json)]
        [Description("根据身份号获取提交报告状态")]
        BaseRes GetReportStateByIdentityNo(string identityno);

        [OperationContract]
        [WebInvoke(UriTemplate = "/Add/CreditInfo", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("外贸征信信息上传")]
        BaseRes AddCreditUserInfo(Stream userInfo);


        [OperationContract]
        [WebInvoke(UriTemplate = "/Upload/AuthFile", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("上传授权文件")]
        BaseRes UpLoadAuthFile(Stream userInfo);


        [OperationContract]
        [WebInvoke(UriTemplate = "/Update/ApplyID", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("给没有ApplyID的数据添加ApplyID")]
        BaseRes UpdateApplyID(Stream applyInfo);
    }
}