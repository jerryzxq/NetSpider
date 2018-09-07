using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web;
using Vcredit.NetSpider.Entity.Service;

namespace Vcredit.NetSpider.RestService.Contracts
{
    [ServiceContract]
    public interface IJobService
    {
        [WebGet(UriTemplate = "/query/salary/mobile/{mobileNo}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("根据手机号获取地区平均工资")]
        BaseRes QuerySalaryByMobileForJson(string mobileNo);

        [WebGet(UriTemplate = "/query/salary/mobile/{mobileNo}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("根据手机号获取地区平均工资")]
        BaseRes QuerySalaryByMobileForXml(string mobileNo);

        [WebGet(UriTemplate = "/query/salary/city/{city}/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("根据城市获取地区平均工资")]
        BaseRes QuerySalaryByCityForJson(string city);

        [WebGet(UriTemplate = "/query/salary/city/{city}/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("根据城市获取地区平均工资")]
        BaseRes QuerySalaryByCityForXml(string city);

    }
}