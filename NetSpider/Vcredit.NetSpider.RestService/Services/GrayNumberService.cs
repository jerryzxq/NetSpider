using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.RestService.Contracts;
using Vcredit.NetSpider.Service;

namespace Vcredit.NetSpider.RestService.Services
{
    //Asp.net管道兼容
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class GrayNumberService : IGrayNumberService
    {
        #region 声明变量、接口
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
        string ClientIp = CommonFun.GetClientIP();
        #endregion

        public BaseRes GetAllCollectionForXml()
        {
            return GetAllCollection();
        }

        public BaseRes GetAllCollectionForJson()
        {
            return GetAllCollection();
        }

        public BaseRes GetAllCollection()
        {
            Log4netAdapter.WriteInfo("接口：GetAllCollection；客户端IP:" + ClientIp);
            BaseRes res = new BaseRes();
            try
            {
                IList<GrayNumberEntity> list = null;
                if (Vcredit.Common.CacheHelper.GetCache("graynumber") == null)
                {
                    IGrayNumber service = NetSpiderFactoryManager.GetGrayNumberService();
                    list = service.Get();
                    Vcredit.Common.CacheHelper.SetCache("graynumber", list);
                }
                else
                {
                    list = (IList<GrayNumberEntity>)Vcredit.Common.CacheHelper.GetCache("graynumber");
                }
                if (list.Count > 0)
                {
                    res.Result = jsonService.SerializeObject(list.Select(x => x.Phone_Number).ToList());
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