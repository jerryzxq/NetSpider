using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.RestService.Contracts;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Service;

namespace Vcredit.NetSpider.RestService.Services
{
    //Asp.net管道兼容
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class DangerNumberService : IDangerNumberService
    {
        #region 声明变量、接口
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
        string ClientIp = CommonFun.GetClientIP();
        #endregion

        public DangerNumberService()
        {
        }

        public BaseRes CollectionSaveForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            DangernumberEntity entity = reqText.DeserializeXML<DangernumberEntity>();

            return CollectionSave(entity);
        }
        public BaseRes CollectionSaveForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            DangernumberEntity entity = jsonService.DeserializeObject<DangernumberEntity>(reqText);

            return CollectionSave(entity);
        }
        public BaseRes CollectionSave(DangernumberEntity entity)
        {
            Log4netAdapter.WriteInfo("接口：CollectionSave；客户端IP:" + ClientIp + ",参数：" + jsonService.SerializeObject(entity, true));
            BaseRes baseRes = new BaseRes();
            try
            {
                IDangernumber service = NetSpiderFactoryManager.GetDangernumberService();
                entity.IPAddr = ClientIp;
                service.Save(entity);
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
            DangernumberEntity entity = reqText.DeserializeXML<DangernumberEntity>();

            return CollectionUpdate(entity);
        }
        public BaseRes CollectionUpdateForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            DangernumberEntity entity = jsonService.DeserializeObject<DangernumberEntity>(reqText);

            return CollectionUpdate(entity);
        }
        public BaseRes CollectionUpdate(DangernumberEntity entity)
        {
            Log4netAdapter.WriteInfo("接口：CollectionUpdate；客户端IP:" + ClientIp + ",参数：" + jsonService.SerializeObject(entity, true));
            BaseRes baseRes = new BaseRes();
            try
            {
                IDangernumber service = NetSpiderFactoryManager.GetDangernumberService();
                entity.IPAddr = ClientIp;
                service.Update(entity);
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
                IList<DangernumberEntity> list = null;
                if (Vcredit.Common.CacheHelper.GetCache("dangernum") == null)
                {
                    IDangernumber service = NetSpiderFactoryManager.GetDangernumberService();
                    list = service.Get();
                    Vcredit.Common.CacheHelper.SetCache("dangernum", list);
                }
                else
                {
                    list = (IList<DangernumberEntity>)Vcredit.Common.CacheHelper.GetCache("dangernum");
                }

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