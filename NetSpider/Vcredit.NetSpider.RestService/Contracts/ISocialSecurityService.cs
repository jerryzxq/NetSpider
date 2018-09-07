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
    interface ISocialSecurityService
    {

        #region 社保查询

        [WebGet(UriTemplate = "/init/{city}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("社保数据查询登录页初始化")]
        VerCodeRes SocialSecurityInitForXml(string city);

        [WebGet(UriTemplate = "/init/{city}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("社保数据查询登录页初始化")]

        VerCodeRes SocialSecurityInitForJson(string city);

        [WebInvoke(UriTemplate = "/login/{city}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("执行社保数据查询")]
        SocialSecurityQueryRes SocialSecurityLoginForXml(Stream stream, string city);

        [WebInvoke(UriTemplate = "/login/{city}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("执行社保数据查询")]

        SocialSecurityQueryRes SocialSecurityLoginForJson(Stream stream, string city);

        [WebGet(UriTemplate = "/formsetting/query/{city}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("社保城市对应表单")]
        BaseRes FormsettingQueryForXml(string city);

        [WebGet(UriTemplate = "/formsetting/query/{city}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("社保城市对应表单")]
        BaseRes FormsettingQueryForJson(string city);

        [WebGet(UriTemplate = "/formsetting/province/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("获取全国城市")]
        BaseRes QueryProvinceForJson();


        [WebInvoke(UriTemplate = "/formsetting/save/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("社保城市对应表单更新")]
        BaseRes FormsettingSave(Stream stream);

        [WebInvoke(UriTemplate = "/formsetting/update/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("社保城市对应表单更新")]
        BaseRes FormsettingUpdate(Stream stream);

        [WebInvoke(UriTemplate = "/data/query/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("社保城市对应表单更新")]
        BaseRes GetSocialSecurityInfo(Stream stream);

        [WebInvoke(UriTemplate = "/data/query/all/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("社保城市对应表单更新")]
        BaseRes GetSocialSecurityAll(Stream stream);
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
