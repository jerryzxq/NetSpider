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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.JS
{
    public class jiangyin : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://58.214.13.116/wsbsdt/";
        string socialCity = "js_jiangyin";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "frontdesk/GeRen/grywcx.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "Backstage/ValidateCode.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl + "frontdesk/GeRen/grywcx.aspx",
                    ResultType = ResultType.Byte,
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);

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
            string idCard = string.Empty;
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
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                Url = baseUrl + "frontdesk/GeRen/grywcx.aspx";
                postdata = String.Format("__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE=%2FwEPDwUJNDkyNTY5ODAxD2QWAgIDD2QWAgIBD2QWBAIBDw8WAh4EVGV4dAUP5oC76K6%2F6Zeu6YeP77yaZGQCAg8PFgIfAAUW5LuK5pel6K6%2F6Zeu6YeP77yaMjAyMWRkZLgbs0dvfov89Sgg1dczxsDHUgSU&__EVENTVALIDATION=%2FwEWCQLW2KLiDwLaiZDACgLaiYTACgLr0dH4BgLEmf2wDQK8sI3bCAKquayOAwKxyO3%2BBQKGmYjABxZ9HseYzOhOSGa7wAkQEJVPmQgI&Tbgrbh={0}&Tbsfid={1}&Tbpassw={2}&TxtCheckNum={3}&Btload=%E7%99%BB++%E5%BD%95&select=---+%E5%9B%BD%E5%86%85%E5%8A%B3%E5%8A%A8%E4%BF%9D%E9%9A%9C%E7%BD%91%E7%AB%99+---&select2=---+%E7%9C%81%E5%86%85%E5%8A%B3%E5%8A%A8%E4%BF%9D%E9%9A%9C%E7%BD%91%E7%AB%99+---&select3=-------+%E5%B8%82%E5%86%85%E7%BD%91%E7%AB%99+-------&select4=-----+%E5%9B%BD%E5%86%85%E5%85%B6%E4%BB%96%E7%BD%91%E7%AB%99+-----", socialReq.Username, socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "frontdesk/GeRen/grywcx.aspx",
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
                string msg = CommonFun.GetMidStr(httpResult.Html, "<script type=\"text/javascript\">//<![CDATA[alert(","')");
                if (msg.StartsWith("'"))
                {
                    Res.StatusDescription = msg.Remove(0,1);
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取基本信息

                Url = baseUrl + "frontdesk/GeRen/GRINFO.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "frontdesk/GeRen/GRQuery.aspx",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='TableLine']/tr/td/span ", "inner");
                if (results.Count <= 0 || string.IsNullOrEmpty(results[2]))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Res.EmployeeNo = results[2].Trim().Replace("&nbsp;", "");//编号 
                Res.Name = results[4].Trim().Replace("&nbsp;", "");//姓名
                Res.Sex = results[8].Trim().Replace("&nbsp;", "");//性别
                Res.BirthDate = results[10].Trim().Replace("&nbsp;", "");//出生日期  
                Res.IdentityCard = results[6].Trim().Replace("&nbsp;", "");//身份证号
                Res.WorkDate = results[12].Trim().Replace("&nbsp;", "");//参加工作时间
                Res.IdentityCard = socialReq.Identitycard;

                #endregion

                #region 查询明细
                int pageCount = 0;
                int pageIndex = 1;
                string viewState = string.Empty;
                string eventValidation = string.Empty;

                Url = baseUrl + "frontdesk/GeRen/GRJFInfo.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "frontdesk/GeRen/GRQuery.aspx",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                viewState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                eventValidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                pageCount = CommonFun.GetMidStr(httpResult.Html, "/共", "页, &nbsp;第").ToInt(0);
                Res.EmployeeStatus = HtmlParser.GetResultFromParser(httpResult.Html, "//span [@id='Labcbzt']", "inner")[0]; //人员状态
                Res.CompanyName = HtmlParser.GetResultFromParser(httpResult.Html, "//span [@id='Labdwmc']", "inner")[0]; //公司名称
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//tr[@class='GridFooter_Telerik']/td");
                if (results.Count == 5)
                {
                    Res.InsuranceTotal = results[2].ToDecimal(0);
                    Res.PersonalInsuranceTotal = results[4].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//tbody/tr");
                //results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//tbody/tr[@class='GridAltRow_Telerik']"));

                while (pageIndex < pageCount)
                {
                    Url = baseUrl + "frontdesk/GeRen/GRJFInfo.aspx";
                    postdata = String.Format("__EVENTTARGET={0}&__EVENTARGUMENT=&__VIEWSTATE={1}&__EVENTVALIDATION={2}&GRJFGridPostDataValue=", "GRJFGrid%24ctl01%24ctl03%24ctl01%24ctl01", viewState.ToUrlEncode(Encoding.GetEncoding("utf-8")), eventValidation);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Referer = baseUrl + "frontdesk/GeRen/GRJFInfo.aspx",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                    viewState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                    eventValidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='GRJFGrid_ctl01']/tbody/tr", "inner"));
                    pageIndex++;
                }

                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                    if (tdRow.Count != 5)
                        continue;

                    string SocialInsuranceTime = tdRow[0];
                    detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                    if (detailRes == null)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;
                        detailRes.PayTime = tdRow[0];
                        detailRes.SocialInsuranceTime = tdRow[0];
                        detailRes.SocialInsuranceBase = tdRow[1].ToDecimal(0);
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detailRes.PensionAmount = tdRow[4].ToDecimal(0);
                        detailRes.CompanyPensionAmount = tdRow[3].ToDecimal(0);
                        Res.Details.Add(detailRes);
                    }
                    else
                    {
                        detailRes.SocialInsuranceBase += tdRow[1].ToDecimal(0);
                        detailRes.PensionAmount += tdRow[4].ToDecimal(0);
                        detailRes.CompanyPensionAmount += tdRow[3].ToDecimal(0);
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
