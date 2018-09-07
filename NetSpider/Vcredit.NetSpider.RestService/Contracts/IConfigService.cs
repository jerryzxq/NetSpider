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
    interface IConfigService
    {
        #region 催收

        [WebInvoke(UriTemplate = "/collection/save/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("催收信息保存")]
          BaseRes CollectionSaveForXml(Stream stream);

        [WebInvoke(UriTemplate = "/collection/save/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("催收信息保存")]
        BaseRes CollectionSaveForJson(Stream stream);

        [WebInvoke(UriTemplate = "/collection/update/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("催收信息修改")]
        BaseRes CollectionUpdateForXml(Stream stream);

        [WebInvoke(UriTemplate = "/collection/update/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("催收信息修改")]
        BaseRes CollectionUpdateForJson(Stream stream);

        [WebInvoke(UriTemplate = "/query/getcollection/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("催收信息获取")]
        BaseRes GetCollectionForXml(Stream stream);

        [WebInvoke(UriTemplate = "/query/getcollection/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("催收信息获取")]
        BaseRes GetCollectionForJson(Stream stream);

        [WebGet(UriTemplate = "/query/getallcollection/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("催收信息获取")]
        BaseRes GetAllCollectionForXml();

        [WebGet(UriTemplate = "/query/getallcollection/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("催收信息获取")]
        BaseRes GetAllCollectionForJson();

        #endregion
    }
}