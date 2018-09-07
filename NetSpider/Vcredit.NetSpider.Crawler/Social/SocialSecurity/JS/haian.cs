using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using Vcredit.Common.Helper;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.JS
{
    public class haian : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://61.177.198.28/halss/wsbs/";
        string socialCity = "js_haian";
        #endregion


        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            List<string> results = new List<string>();
            Dictionary<string, object> dic = new Dictionary<string, object>();
            try
            {
                string vercode = string.Empty;
                Url = baseUrl + "login.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='num']", "value");
                if (results.Count > 0)
                {
                    vercode = results[0];
                }
                if (vercode.IsEmpty())
                {
                    Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                dic.Add("cookie", cookies);
                dic.Add("vercode", vercode);

                CacheHelper.SetCache(token, dic);
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
            string vercode = string.Empty;
            List<string> results = new List<string>();
            Dictionary<string, object> dic = new Dictionary<string, object>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    dic = (Dictionary<string, object>)CacheHelper.GetCache(socialReq.Token);
                    cookies = (CookieCollection)dic["cookie"];
                    vercode = dic["vercode"].ToString();
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 登录、获取基本信息

                Url = baseUrl + "logincheck.jsp";
                postdata = string.Format("iscode={0}&errors=&password={1}&num12={2}&num={2}&Submit=%C8%B7+%B6%A8", socialReq.Identitycard, socialReq.Password, vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                string msg = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\"");
                if (!msg.IsEmpty() && !msg.Contains("html"))
                {
                    if (msg.Contains("海安县"))
                    {
                        msg = "身份证号码或社保编码错误，请核对后重新输入！";
                    }
                    if (!msg.Contains("身份证号码或社保编码错误"))
                    {
                        msg = "登录失败！";
                    }
                    Res.StatusDescription = msg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_wssb']/tr/td");
                if (results.Count > 24)
                {
                    Res.EmployeeNo = results[2];
                    Res.Name = results[4];
                    Res.IdentityCard = results[6];
                    Res.Sex = results[8];
                    Res.CompanyName = results[10];
                    Res.Phone = results[14];
                    Res.Address = results[16];
                    if (results[22] == "企业养老保险")
                    {
                        Res.SocialInsuranceBase = results[23].ToDecimal(0);
                    }
                }
                #endregion

                #region 查询明细
                int pagenumber = 0;
                int pagecount = 0;
                results.Clear();
                do
                {
                    pagenumber++;
                    Url = baseUrl + "perprint.jsp?spage=" + pagenumber;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                    if(pagecount == 0)
                        pagecount = CommonFun.GetMidStr(httpResult.Html, "</font>/", "页").ToInt(0);
                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//tr[@class='hfxx_tr']"));
                }
                while (pagenumber < pagecount);

                foreach (var item in results)
                {
                    List<string> info = HtmlParser.GetResultFromParser(item, "//td");
                    if (info.Count == 9)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;
                        detailRes.PayTime = info[4];
                        detailRes.SocialInsuranceTime = info[4];
                        detailRes.CompanyName = info[3];
                        if (Res.CompanyNo.IsEmpty() && !info[3].IsEmpty() && Res.CompanyName == info[3])
                        {
                            Res.CompanyNo = info[2];
                        }
                        detailRes.CompanyPensionAmount = info[6].ToDecimal(0);
                        detailRes.PensionAmount = info[7].ToDecimal(0);
                        detailRes.SocialInsuranceBase = (detailRes.CompanyPensionAmount * 5) > (detailRes.PensionAmount * (decimal)12.5) ? (detailRes.CompanyPensionAmount * 5) : (detailRes.PensionAmount * (decimal)12.5);
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = info[8] == "已到帐" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : info[8];
                        Res.Details.Add(detailRes);
                    }
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
