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
    interface IProvidentFundService
    {

        #region 公积金查询
        [WebGet(UriTemplate = "/init/{city}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("公积金查询登录页初始化")]
        VerCodeRes ProvidentFundInitForXml(string city);

        [WebGet(UriTemplate = "/init/{city}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("公积金查询登录页初始化")]

        VerCodeRes ProvidentFundInitForJson(string city);

        [WebInvoke(UriTemplate = "/login/{city}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml,Method="POST")]
        [Description("公积金登录，并查询")]
        ProvidentFundQueryRes ProvidentFundLoginForXml(Stream stream,string city);

        [WebInvoke(UriTemplate = "/login/{city}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("公积金登录，并查询")]
        ProvidentFundQueryRes ProvidentFundLoginForJson(Stream stream, string city);

        [WebGet(UriTemplate = "/formsetting/province/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("获取全国城市")]
        BaseRes QueryProvinceForJson();

        [WebGet(UriTemplate = "/formsetting/query/{city}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("公积金城市对应表单")]
        BaseRes ProvidentFundFormsettingQueryForXml(string city);

        [WebGet(UriTemplate = "/formsetting/query/{city}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("公积金城市对应表单")]
        BaseRes ProvidentFundFormsettingQueryForJson(string city);

        [WebInvoke(UriTemplate = "/formsetting/save/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("公积金登录，并查询")]
        BaseRes ProvidentFundFormsettingSave(Stream stream);

        [WebInvoke(UriTemplate = "/formsetting/update/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("公积金登录，并查询")]
        BaseRes ProvidentFundFormsettingUpdate(Stream stream);

        [WebInvoke(UriTemplate = "/data/query/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("公积金城市对应表单更新")]
        BaseRes GetProvidentFundInfo(Stream stream);

        [WebInvoke(UriTemplate = "/data/query/all/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("公积金城市对应表单更新")]
        BaseRes GetProvidentFundAll(Stream stream);
        #endregion

        #region 数据接口
        [WebInvoke(UriTemplate = "/score/calculate/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("评分计算")]
        BaseRes ScoreCalculateForXml(Stream stream);

        [WebInvoke(UriTemplate = "/score/calculate/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("评分计算")]
        BaseRes ScoreCalculateForJson(Stream stream);

        #endregion
    }
}
