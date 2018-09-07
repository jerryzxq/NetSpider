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
    interface ICommonService
    {
        [WebGet(UriTemplate = "/idcard/getaddress/{cardno}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("根据身份证号获取户籍地")]
        BaseRes IdCard_GetAddress(string cardno);

        [WebGet(UriTemplate = "/mobile/getinfo/{mobile}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("根据手机号获取所属地")]
        BaseRes Mobile_GetMobileInfo(string mobile);
    }
}
