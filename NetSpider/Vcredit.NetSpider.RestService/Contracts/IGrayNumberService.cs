using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Vcredit.NetSpider.Entity.Service;

namespace Vcredit.NetSpider.RestService.Contracts
{
    [ServiceContract]
    interface IGrayNumberService
    {

        [WebGet(UriTemplate = "/query/getallcollection/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("催收信息获取")]
        BaseRes GetAllCollectionForXml();

        [WebGet(UriTemplate = "/query/getallcollection/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("催收信息获取")]
        BaseRes GetAllCollectionForJson();
    }
}
