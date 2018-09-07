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
    interface IRCService
    {
        [WebInvoke(UriTemplate = "/xxqd/credit/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("聚信立手机账单登录成功后回调")]
        BaseRes XXQDCreditForForXml(Stream stream);

        [WebInvoke(UriTemplate = "/xxqd/credit/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("聚信立手机账单登录成功后回调")]
        BaseRes XXQDCreditForJson(Stream stream);

        [WebInvoke(UriTemplate = "/xxqd/eduloan/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("聚信立手机账单登录成功后回调")]
        BaseRes XXQDEduloan(Stream stream);

    }
}
