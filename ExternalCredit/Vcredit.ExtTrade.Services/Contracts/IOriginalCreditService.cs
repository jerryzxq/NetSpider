using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web;
using Vcredit.ExtTrade.ModelLayer;

namespace Vcredit.ExtTrade.Services.Contracts
{
    [ServiceContract]
    public interface IOriginalCreditService
    {

        [OperationContract]
        [WebInvoke(UriTemplate = "/Add/CreditInfo", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("外贸征信信息上传")]
        BaseRes AddCreditUserInfo(Stream userInfo);

        [OperationContract]
        [WebInvoke(UriTemplate = "/Upload/AuthFile", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("上传授权文件")]
        BaseRes UpLoadAuthFile(Stream fileInfo);

    }
}