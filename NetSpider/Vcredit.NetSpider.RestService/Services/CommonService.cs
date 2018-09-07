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
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Service;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.RestService.Models.RC;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.RestService.Models.Credit;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.RestService.Models.Mobile;
using Vcredit.NetSpider.ThirdParty.Baidu;

namespace Vcredit.NetSpider.RestService.Services
{
    //Asp.net管道兼容
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class CommonService : ICommonService
    {
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();
        string ClientIp = CommonFun.GetClientIP();

        /// <summary>
        /// 根据身份证号获取户籍地
        /// </summary>
        /// <param name="identitycard"></param>
        /// <returns></returns>
        public BaseRes IdCard_GetAddress(string cardno)
        {
            BaseRes baseRes = new BaseRes();
            Log4netAdapter.WriteInfo("接口：IdCard_GetAddress；客户端IP:" + ClientIp + ",参数：" + cardno);
            try
            {
                BaiduIdcard baiduIdcard = new BaiduIdcard();
                baseRes.Result = baiduIdcard.GetAddress(cardno);
                baseRes.StatusCode = ServiceConstants.StatusCode_success;
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("身份证号户籍地查询异常", e);
                baseRes.StatusCode = ServiceConstants.StatusCode_error;
                baseRes.StatusDescription = "身份证号户籍地查询已禁用";
            }
            return baseRes;
        }
        /// <summary>
        /// 根据手机号获取所属地
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public BaseRes Mobile_GetMobileInfo(string mobile)
        {
            BaseRes baseRes = new BaseRes();
            Log4netAdapter.WriteInfo("接口：Mobile_GetMobileInfo；客户端IP:" + ClientIp + ",参数：" + mobile);

            try
            {
                IMobile_Number service = NetSpiderFactoryManager.GetMobile_NumberService();
                var prefix = mobile.Substring(0, 7);
                var model = service.GetModel(prefix);
                if (model != null)
                {
                    baseRes.Result = jsonService.SerializeObject(new
                    {
                        phone = mobile,
                        prefix = prefix,
                        supplier = model.Supplier,
                        province = model.Province,
                        city = model.City
                    });
                }
                else
                {
                    BaiduMobile baiduMobile = new BaiduMobile();
                    baseRes.Result = baiduMobile.GetMobileInfo(mobile);
                    var baiduMobileResult = jsonService.DeserializeObject<BaiduMobileResult>(baseRes.Result);
                    if (baiduMobileResult != null
                        && !string.IsNullOrEmpty(baiduMobileResult.supplier)
                        && !string.IsNullOrEmpty(baiduMobileResult.province)
                        && !string.IsNullOrEmpty(baiduMobileResult.city)
                        && baiduMobileResult.province != "-"
                        && baiduMobileResult.city != "-")
                    {
                        service.Save(new Mobile_NumberEntity()
                        {
                            Mobile = prefix,
                            Province = baiduMobileResult.province,
                            City = baiduMobileResult.city,
                            Supplier = baiduMobileResult.supplier,
                            Post_Code = "",
                            Area_Code = "",
                            Memo = ""
                        });
                    }
                }
                baseRes.StatusCode = ServiceConstants.StatusCode_success;
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("手机号归属地查询异常", e);
                baseRes.StatusCode = ServiceConstants.StatusCode_error;
                baseRes.StatusDescription = "手机号归属地查询已禁用";
            }
            return baseRes;
        }
    }
}
