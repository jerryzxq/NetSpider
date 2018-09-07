using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.AH
{
    public class luan : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://218.23.88.205:8001/";
        public string socialCity = "ah_luan";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "index_yl.asp";//养老
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                CacheHelper.SetCache(token, cookies);
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
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Name.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                //养老
                postdata = String.Format("AAC003={0}&AAC002={1}&AKC020={2}&B2=+%B2%E9+%D1%AF+", socialReq.Name.ToUrlEncode(Encoding.GetEncoding("gb2312")), socialReq.Identitycard, socialReq.Password.ToUrlEncode(Encoding.GetEncoding("gb2312")));

                Url = baseUrl + "yl_si_search.asp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                List<string> resultstable = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='table26']", "", true);
                if (resultstable.Count < 1)
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                #endregion

                #region 获取基本信息

                results = HtmlParser.GetResultFromParser(resultstable[0], "//tr[1]//table/tr[1]/td[4]", "text", true);
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(resultstable[0], "//tr[1]//table/tr[1]/td[6]", "text", true);
                if (results.Count > 0)
                {
                    Res.Sex = results[0];
                }
                results = HtmlParser.GetResultFromParser(resultstable[0], "//tr[2]//table/tr[1]/td[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];//社保卡号
                }
                results = HtmlParser.GetResultFromParser(resultstable[0], "//tr[2]//table/tr[1]/td[4]", "text", true);
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                results = HtmlParser.GetResultFromParser(resultstable[0], "//tr[3]//table/tr[1]/td[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(resultstable[0], "//tr[3]//table/tr[1]/td[4]", "text", true);
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                results = HtmlParser.GetResultFromParser(resultstable[0], "//tr[4]//table/tr[1]/td[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.WorkDate = Convert.ToDateTime(results[0]).ToString(Consts.DateFormatString2);
                }
                results = HtmlParser.GetResultFromParser(resultstable[0], "//tr[4]//table/tr[1]/td[4]", "text", true);
                if (results.Count > 0)
                {
                    if (!string.IsNullOrEmpty(results[0]))
                    {
                        Res.SocialInsuranceBase = results[0].ToDecimal(0);
                    }
                }
                results = HtmlParser.GetResultFromParser(resultstable[0], "//tr[4]//table/tr[1]/td[6]", "text", true);
                if (results.Count > 0)
                {
                    Res.Payment_State = results[0];
                }
                results = HtmlParser.GetResultFromParser(resultstable[0], "//tr[5]//table/tr[1]/td[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.ZipCode = results[0];
                }
                //医疗
                //Url = baseUrl + "yb_si_search.asp";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "POST",
                //    Postdata = postdata,
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //resultstable = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='table26']", "", true);
                //if (resultstable.Count == 1)
                //{
                //   //
                //}

                #endregion

                #region ===无缴费明细===

                #endregion

                Res.PaymentMonths = PaymentMonths;
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