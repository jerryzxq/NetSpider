using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading;
using System.Web;
//using System.Web.Providers.Entities;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.WorkFlow;
using Vcredit.NetSpider.Parser;
using Vcredit.NetSpider.Processor;
using Vcredit.NetSpider.RestService.Contracts;
using Vcredit.NetSpider.WorkFlow;
using Vcredit.Common.Ext;
using System.Data;
using Vcredit.Common;
using Vcredit.NetSpider.RestService.Operation;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.Entity.Service.Mobile;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.Common.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.NetSpider.Service;

namespace Vcredit.NetSpider.RestService.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class JobService : IJobService
    {
        IJobExecutor pbccrcExecutor = ExecutorManager.GetJobExecutor();

        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
        string ClientIp = CommonFun.GetClientIP();


        public BaseRes QuerySalaryByMobileForJson(string mobileNo)
        {
            return GetSalaryByMobile(mobileNo);
        }


        public BaseRes GetSalaryByMobile(string mobileNo)
        {
            Log4netAdapter.WriteInfo("接口名：QuerySalaryByMobileForJson；IP:" + CommonFun.GetClientIP() + ",参数：" + mobileNo);
            BaseRes baseRes = new BaseRes();
            try
            {
                var city = GetCityByMobile(mobileNo);
                var say = GetSalaryByCity(city);

                baseRes.Result = jsonService.SerializeObject(new { City = city, Salary = say });
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("", e);
                baseRes.StatusDescription = e.Message;
            }

            return baseRes;
        
        }
        public BaseRes GetSalaryByToCity(string city)
        {
            Log4netAdapter.WriteInfo("接口名：QuerySalaryByMobileForJson；IP:" + CommonFun.GetClientIP() + ",参数：" + city);
            BaseRes baseRes = new BaseRes();
            try
            {
                var say = GetSalaryByCity(city);

                baseRes.Result = jsonService.SerializeObject(new { City = city, Salary = say });
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("", e);
                baseRes.StatusDescription = e.Message;
            }

            return baseRes;

        }
        public BaseRes QuerySalaryByMobileForXml(string mobileNo)
        {
            return GetSalaryByMobile(mobileNo);
        }

        public BaseRes QuerySalaryByCityForJson(string city)
        {
            return GetSalaryByToCity(city);
        }

        public BaseRes QuerySalaryByCityForXml(string city)
        {
            return GetSalaryByToCity(city);
        }

        string GetCityByMobile(string mobile)
        {
            string city=string.Empty;
            try
            {
                //string Url = "https://sp0.baidu.com/8aQDcjqpAAV3otqbppnN2DJv/api.php?query={0}&co=&resource_id=6004&t=1451031584105&ie=utf8&oe=gbk&cb=op_aladdin_callback&format=json&tn=baidu&cb=&_=1451031555214";

                //HttpHelper httpHelper = new HttpHelper();
                //HttpItem httpItem = new HttpItem()
                //{
                //    URL = String.Format(Url, mobile)
                //};
                //var httpResult = httpHelper.GetHtml(httpItem);
                //var obj = JsonConvert.DeserializeObject(httpResult.Html);
                //JObject js = obj as JObject;
                //JArray bdp = js["data"] as JArray;
                //if (bdp.Count > 0)
                //{
                //    return bdp[0]["city"].ToString();
                //}
                IMobile_Number service = NetSpiderFactoryManager.GetMobile_NumberService();
                var prefix = mobile.Substring(0, 7);
                var model = service.GetModel(prefix);
                if (model != null)
                {
                    city = model.City.ToTrim("市");
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("从百度获取手机归属地", ex);
            }

            return city;
        }

        decimal GetSalaryByCity(string city)
        {
            return pbccrcExecutor.GetSalaryByCity(city);
        }
    }


}