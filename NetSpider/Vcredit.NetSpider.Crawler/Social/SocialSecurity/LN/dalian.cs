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
using Vcredit.NetSpider.DataAccess.Cache;
namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.LN
{
    public class dalian : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "";
        string socialCity = "ln_dalian";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Res.StatusDescription = "无需验证码";
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
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(socialReq.Token);
                    SpiderCacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Username.IsEmpty() || socialReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = "身份证号或个人编号不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                postdata = String.Format("code={0}&IdCard={1}&type=0&P_Type=0", socialReq.Username, socialReq.Identitycard);
                Url = "http://www.dl12333.gov.cn/_layouts/LssbwebSite/A04/A0402/List_QY_Tab.aspx?" + postdata;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                string message = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "');window.open");
                if (!message.IsEmpty())
                {
                    Res.StatusDescription = message;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，个人基本信息
                postdata = string.Format("AACOO1={0}&AAC002={1}", socialReq.Username, socialReq.Identitycard);
                Url = "http://www.dl12333.gov.cn/_layouts/LssbwebSite/A04/A0402/A040253/List.aspx?" + postdata;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//label[@id='AAC001']", "text", true);
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];//编号
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//label[@id='AAC003']", "text", true);
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//label[@id='AAC002']", "text", true);
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//label[@id='AKC021']", "text", true);
                if (results.Count > 0)
                {
                    Res.EmployeeStatus = results[0];//账户状态
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//label[@id='AAB004']", "text", true);
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];//公司名称
                }
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//label[@id='AKC087']", "text", true);
                //if (results.Count > 0)
                //{
                //    Res.PersonalInsuranceTotal = results[0].ToDecimal(0);//个人帐号余额
                //}
                #endregion

                #region 第三步，养老

                postdata = string.Format("AACOO1={0}&AAC002={1}", socialReq.Username, socialReq.Identitycard);
                Url = "http://www.dl12333.gov.cn/_layouts/LssbwebSite/A04/A0402/A040272/List_JF.aspx?" + postdata;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                int pageIndex = 0;
                int pageCount = CommonFun.GetMidStr(httpResult.Html, "页 共 ", " 页").ToInt(0);
                string pagelistajax = string.Empty;
                do
                {
                    string viewState = string.Empty;
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value", true);
                    if (results.Count > 0)
                    {
                        viewState = results[0];
                    }
                    pageIndex++;
                    postdata = string.Format("AACOO1={0}&AAC002={1}", socialReq.Username, socialReq.Identitycard);
                    Url = "http://www.dl12333.gov.cn/_layouts/LssbwebSite/A04/A0402/A040272/List_JF.aspx?" + postdata;

                    postdata = string.Format("__VIEWSTATE={0}&__EVENTTARGET={1}&__EVENTARGUMENT={2}", viewState.ToUrlEncode(), "A040272_JF_pager", pageIndex);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='lssb_sub_Table_01']/tr[position()>2]", "inner", true);

                    string month = string.Empty;
                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 11)
                        {
                            continue;
                        }
                        detailRes = new SocialSecurityDetailQueryRes();

                        detailRes.PayTime = tdRow[10].ToTrim("&nbsp;");
                        detailRes.SocialInsuranceTime = tdRow[0].ToTrim("&nbsp;") + tdRow[1].ToTrim("&nbsp;");
                        detailRes.SocialInsuranceBase = tdRow[3].ToTrim("&nbsp;").ToDecimal(0);
                        if (tdRow[7].ToTrim("&nbsp;") == "正常缴费" && tdRow[8].ToTrim("&nbsp;") != "欠缴" && month != tdRow[1])
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            PaymentMonths++;
                        }
                        else
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                        }
                        detailRes.PensionAmount = tdRow[6].ToTrim("&nbsp;").ToDecimal(0);

                        //单位缴费
                        decimal CompanyInsuranceBase = tdRow[4].ToTrim("&nbsp;").ToDecimal(0);
                        decimal PersonInsuranceBase = tdRow[5].ToTrim("&nbsp;").ToDecimal(0);
                        detailRes.CompanyPensionAmount =Math.Round((detailRes.PensionAmount / (PersonInsuranceBase / (PersonInsuranceBase + CompanyInsuranceBase))) * (CompanyInsuranceBase/(PersonInsuranceBase + CompanyInsuranceBase)),2);
                        Res.SpecialPaymentType = tdRow[9].ToTrim("&nbsp;");
                        Res.Details.Add(detailRes);
                        month = tdRow[1];
                    }
                }
                while (pageIndex < pageCount);

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
