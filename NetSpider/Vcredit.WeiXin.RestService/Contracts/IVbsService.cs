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
    interface IVbsService
    {
        [WebInvoke(UriTemplate ="/chsi/query/info/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("学信网基本信息")]
        BaseRes GetChsiInfoForXml(Stream stream);

        [WebInvoke(UriTemplate = "/chsi/query/info/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("学信网基本信息")]
        BaseRes GetChsiInfoJson(Stream stream);

        [WebInvoke(UriTemplate = "/chsi/query/photo/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("获取电子合同")]
        Stream GetChsiPhotoForXml(Stream stream);

        [WebInvoke(UriTemplate = "/chsi/query/photo/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("获取电子合同")]
        Stream GetChsiPhotoForJson(Stream stream);
    }
}
