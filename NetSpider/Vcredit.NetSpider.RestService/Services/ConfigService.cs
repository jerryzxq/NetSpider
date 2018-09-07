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
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Service;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.RestService.Models.RC;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.RestService.Models.Credit;
using Vcredit.NetSpider.Entity.Mongo.Seting;

namespace Vcredit.NetSpider.RestService.Services
{
    //Asp.net管道兼容
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class ConfigService : IConfigService
    {
        #region 声明变量、接口
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
        string ClientIp = CommonFun.GetClientIP();
        #endregion

        public ConfigService()
        {
        }

        public BaseRes CollectionSaveForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            CollectionPhone entity = reqText.DeserializeXML<CollectionPhone>();

            return CollectionSave(entity);
        }
        public BaseRes CollectionSaveForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            CollectionPhone entity = jsonService.DeserializeObject<CollectionPhone>(reqText);

            return CollectionSave(entity);
        }
        public BaseRes CollectionSave(CollectionPhone entity)
        {
            Log4netAdapter.WriteInfo("接口：CollectionSave；客户端IP:" + ClientIp + ",参数：" + jsonService.SerializeObject(entity, true));
            BaseRes baseRes = new BaseRes();
            try
            {
                CollectionPhoneMongo mongo = new CollectionPhoneMongo();
                mongo.Save(entity);
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "信息保存完毕";
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = "信息保存出错";
                Log4netAdapter.WriteError("信息保存出错", e);
            }
            baseRes.EndTime = DateTime.Now.ToString();
            return baseRes;
        }

        public BaseRes CollectionUpdateForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            CollectionPhone entity = reqText.DeserializeXML<CollectionPhone>();

            return CollectionUpdate(entity);
        }
        public BaseRes CollectionUpdateForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            CollectionPhone entity = jsonService.DeserializeObject<CollectionPhone>(reqText);

            return CollectionUpdate(entity);
        }
        public BaseRes CollectionUpdate(CollectionPhone entity)
        {
            Log4netAdapter.WriteInfo("接口：CollectionUpdate；客户端IP:" + ClientIp + ",参数：" + jsonService.SerializeObject(entity, true));
            BaseRes baseRes = new BaseRes();
            try
            {
                CollectionPhoneMongo mongo = new CollectionPhoneMongo();
                mongo.Update(entity);
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "信息修改完毕";
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = "信息修改出错";
                Log4netAdapter.WriteError("信息修改出错", e);
            }
            baseRes.EndTime = DateTime.Now.ToString();
            return baseRes;
        }

        public BaseRes GetCollectionForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            Dictionary<string, string> dic = null;
            if (!reqText.IsEmpty())
                dic = reqText.DeserializeXML<Dictionary<string, string>>();

            return GetCollection(dic);
        }
        public BaseRes GetCollectionForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            Dictionary<string, string> dic = null;
            if (!reqText.IsEmpty())
                dic = jsonService.DeserializeObject<Dictionary<string, string>>(reqText);

            return GetCollection(dic);
        }
        public BaseRes GetCollection(Dictionary<string, string> dic)
        {
            Log4netAdapter.WriteInfo("接口：GetCollection；客户端IP:" + ClientIp + ",参数：" + jsonService.SerializeObject(dic, true));
            BaseRes res = new BaseRes();
            try
            {
                if (dic != null)
                {
                    CollectionPhoneMongo mongo = new CollectionPhoneMongo();
                    var list = mongo.Get(dic);
                    res.Result = jsonService.SerializeObject(list);
                    res.StatusCode = ServiceConsts.StatusCode_success;
                    res.StatusDescription = "查询成功";
                }
                else
                {
                    res.StatusCode = ServiceConsts.StatusCode_fail;
                    res.StatusDescription = "缺少必填项";
                }
            }
            catch (Exception e)
            {
                res.StatusCode = ServiceConsts.StatusCode_error;
                res.StatusDescription = "接口：GetCollection异常";
                Log4netAdapter.WriteError("接口：GetCollection异常：", e);
            }
            res.EndTime = DateTime.Now.ToString();
            return res;
        }

        public BaseRes GetAllCollectionForXml()
        {
            //获得请求内容
            return GetAllCollection();
        }
        public BaseRes GetAllCollectionForJson()
        {
            //获得请求内容
            return GetAllCollection();
        }
        public BaseRes GetAllCollection()
        {
            Log4netAdapter.WriteInfo("接口：GetAllCollection；客户端IP:" + ClientIp);
            BaseRes res = new BaseRes();
            try
            {
                CollectionPhoneMongo mongo = new CollectionPhoneMongo();
                var list = mongo.Get();
                if (list.Count > 0)
                {
                    res.Result = jsonService.SerializeObject(list.Select(x => x.Phone).ToList());
                }
                res.StatusCode = ServiceConsts.StatusCode_success;
                res.StatusDescription = "查询成功";
            }
            catch (Exception e)
            {
                res.StatusCode = ServiceConsts.StatusCode_error;
                res.StatusDescription = "接口：GetAllCollection异常";
                Log4netAdapter.WriteError("接口：GetAllCollection异常：", e);
            }
            res.EndTime = DateTime.Now.ToString();
            return res;
        }
    }
}