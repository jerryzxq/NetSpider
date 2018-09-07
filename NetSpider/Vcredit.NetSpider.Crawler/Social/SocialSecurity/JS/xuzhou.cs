using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.JS
{
    public class xuzhou : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string socialCity = "js_xuzhou";
        #endregion
        private static bool RemoteCertificateValidate(
           object sender, X509Certificate cert,
             X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }
        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = "所选城市无需初始化";
                CacheHelper.SetCache(token, cookies);
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
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Citizencard.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 获取基本信息
                //养老
                Url = "http://www.jsxz.lss.gov.cn/ylzh.jsp";
                postdata = string.Format("grbh={0}&sfzhm={1}", socialReq.Citizencard, socialReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://www.jsxz.lss.gov.cn/login.jsp?model=ylzh",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr/td ", "inner");
                if (results.Count <= 0 || string.IsNullOrEmpty(results[1]))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.EmployeeNo =  results[1].Trim();//编号
                Res.Name = results[3].Trim();//姓名
                Res.IdentityCard =results[5].Trim();//身份证号
                Res.CompanyName = results[9].Trim();//单位名称
                Res.CompanyNo = results[7].Trim();//单位编号
                Res.InsuranceTotal = results[11].Trim().ToDecimal(0);//账户累计储存额
                Res.SocialInsuranceBase = results[13].Trim().ToDecimal(0);//缴费基数
                Res.PaymentMonths = results[15].Trim().ToInt(0);//缴费月数
                //医疗
                ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
                Url = "https://www.jsxz.hrss.gov.cn:8143/personal/yilzh.jsp";
                postdata = string.Format("grbh={0}&sfzhm={1}", socialReq.Citizencard, socialReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host="www.jsxz.hrss.gov.cn:8143",
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr[6]/td[2]", "inner");
                if (results.Count>0)
                {
                    Res.SocialInsuranceBase = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr[9]/td[2]", "inner");
                if (results.Count > 0)
                {
                    Res.InsuranceTotal = results[0].ToDecimal(0) > Res.InsuranceTotal ? results[0].ToDecimal(0) : Res.InsuranceTotal;
                }
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
