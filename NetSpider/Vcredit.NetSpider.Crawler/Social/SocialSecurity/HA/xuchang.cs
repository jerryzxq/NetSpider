using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.HA
{
   public class xuchang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string socialCity = "ha_xuchang";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = "所选城市无需初始化";
               // CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = socialCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(socialCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return Res;
        }

        public SocialSecurityQueryRes SocialSecurityQuery(SocialSecurityReq socialReq)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            string Url = string.Empty;
            Res.SocialSecurityCity = socialCity;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {
                //校验参数
                if (socialReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 获取基本信息

                Url = "http://www.haxc.lss.gov.cn/searchYl.action";
                postdata = string.Format("indiId={0}&cxfs=%E8%BA%AB%E4%BB%BD%E8%AF%81%E5%8F%B7",socialReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "www.haxc.lss.gov.cn",
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header["X-Requested-With"] = "XMLHttpRequest";
                httpResult = httpHelper.GetHtml(httpItem);
               
                JObject jsonObj = (JObject) JsonConvert.DeserializeObject(httpResult.Html);
                JToken jsonToken = jsonObj["lists"].First;
                if (jsonToken == null)
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Res.District = jsonToken["TCQ"].ToString();//编号
                Res.Name = jsonToken["NAME"].ToString();//编号
                Res.Sex = jsonToken["SEX"].ToString() == "1" ? "男" : "女";//编号
                Res.IdentityCard = jsonToken["INSR_CODE"].ToString();//身份证号
                Res.CompanyName = jsonToken["CORP_NAME"].ToString();//单位名称 
                Res.CompanyNo = jsonToken["CORP_CODE"].ToString();//单位账号 
                Res.Payment_State = jsonToken["USE_STATUS"].ToString()=="1"?"正常":"";//缴费状态
                Res.InsuranceTotal = jsonToken["AGE_LAST_BALANCE"].ToString().ToDecimal(0);//账户累计储存额
                Res.DeadlineYearAndMonth = jsonToken["YEAR_MONTH"].ToString();//截止缴费年月
                Res.PaymentMonths = jsonToken["PAY_MONS"].ToString().ToInt(0);//缴费月数 

                #endregion

                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(socialCity + ServiceConsts.SocialSecurity_QueryError, e);
            }
            return Res;
        }

    }
}
