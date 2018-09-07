using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web;
using Vcredit.ExtTrade.ModelLayer;

namespace Vcredit.ExtTrade.Services
{
    [ServiceContract]
    public interface IShangHaiLoanService
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "/Add/CreditApply", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("上海小贷征信查询")]
        BaseRes AddQueryInfo(Stream userInfo);

    }
}