using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.YN
{
    /// <summary>
    /// 532401197010270027李燕萍 zhang:0402001951 mi:701027   玉溪:5304 (该案例不可用)
    /// </summary>
    public class ynshengyiliao : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.yn12333.gov.cn/";
        string socialCity = "zj_ynshengyiliao";
        #endregion
        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Res.StatusDescription = socialCity + "无需初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitError;
                Log4netAdapter.WriteError(socialCity + ServiceConsts.SocialSecurity_InitError, e);
            }
            return Res;
        }

        public SocialSecurityQueryRes SocialSecurityQuery(SocialSecurityReq socialReq)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = socialCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {

                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Username.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                //int[] areaCode = { 5300, 5301, 5303, 5304, 5306, 5307, 5323, 5325, 5326, 5327, 5328, 5329, 5330, 5331, 5333, 5334, 5335 }; 
                Url = baseUrl + string.Format("Ajax/QuerHandler.ashx?action=yb_query&ybcrad={0}&userage={1}&regionid={2}", socialReq.Username, socialReq.Identitycard.Substring(8, 6), "5304");//5304玉溪
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Host = "www.yn12333.gov.cn",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header["X-Requested-With"] = "XMLHttpRequest";
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (jsonParser.GetResultFromParser(httpResult.Html, "status") != "0")
                {
                    Res.StatusDescription = CommonFun.GetMidStr(httpResult.Html,"msg\":\"","\"}");
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion
                #region 第二步， 获取基本信息

                //个人基本信息
                Url = baseUrl + "Insurance/HealthCare/personInfo.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                Res.Name = socialReq.Name;
                Res.IdentityCard = socialReq.Identitycard;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_style02']/tr[2]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_style02']/tr[2]/td[4]", "");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_style02']/tr[4]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.Sex = results[0].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_style02']/tr[4]/td[4]", "");
                if (results.Count > 0)
                {
                    Res.Race = results[0].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_style02']/tr[5]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.BirthDate = results[0].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_style02']/tr[5]/td[4]", "");
                if (results.Count > 0)
                {
                    Res.EmployeeStatus = results[0].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='AAE005']", "value");
                if (results.Count > 0)
                {
                    Res.Phone = results[0] != "0" ? results[0] : null;//
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='PHONE']", "value");
                if (results.Count > 0)
                {
                    Res.Phone = results[0] != "0" & string.IsNullOrEmpty(Res.Phone) ? results[0] : Res.Phone;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='AAE006']", "value");
                if (results.Count > 0)
                {
                    Res.Address = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='AAE007']", "value");
                if (results.Count > 0)
                {
                    Res.ZipCode = results[0] != "0" ? results[0] : null;
                }
                #endregion
                #region 第三步，查询明细

              
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
