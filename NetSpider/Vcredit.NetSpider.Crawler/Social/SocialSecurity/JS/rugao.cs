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
    public class rugao : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://58.221.206.133:8080/hso/";
        string socialCity = "js_rugao";
        #endregion


        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "logon_320688.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "authcode";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl + "logon_320688.jsp",
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                Url = baseUrl + "javaScript/plugin/md5.js";
                httpItem = new HttpItem()
                {
                    URL = Url,
                };
                httpResult = httpHelper.GetHtml(httpItem);
                object[] ob = new object[1];
                ob[0] = socialReq.Password;
                string password = JavaScriptHelper.JavaScriptExecute(httpResult.Html, "hex_md5", ob).ToString();

                Url = baseUrl + "logon.do";
                postdata = String.Format("method=doLogon_320688&usertype=1&username={0}&password={1}&validatecode={2}", socialReq.Identitycard, password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "logon_320688.jsp",
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
                if (!httpResult.Html.ToLower().Contains("true"))
                {
                    Res.StatusDescription = httpResult.Html;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取基本信息

                Url = baseUrl + "hsoPer.do?method=enterPerHome";
                httpItem = new HttpItem()
                {
                    URL = Url,
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                string ticket = CommonFun.GetMidStr(httpResult.Html, "__LOGON_TICKET__ = '", "'");
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='TableLine']/tr/td/span ", "inner");

                Url = baseUrl + "hsoPer.do";
                postdata = "method=QueryPersonBaseInfo&__logon_ticket=" + ticket;
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookie

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='xbmc']", "value");
                if(results.Count > 0)
                    Res.Sex = results[0];
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='dwmc']", "value");
                if (results.Count > 0)
                    Res.CompanyName = results[0];
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='xm']", "value");
                if (results.Count > 0)
                    Res.Name = results[0];
                Res.IdentityCard = socialReq.Identitycard;

                Url = baseUrl + "pub.do";
                postdata = "method=protectPwd&__logon_ticket=" + ticket;
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookie

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='phone']", "value");
                if (results.Count > 0)
                    Res.Phone = results[0];

                #endregion

                #region 查询明细
                int thisyear = DateTime.Now.Year;
                List<string> type = new List<string>();
                type.Add("queryZgYanglaozh");
                type.Add("queryMediAccount");
                for (int i = 0; i < 2; i++)
                {
                    results.Clear();
                    for (int year = thisyear; year > thisyear - 5; year--)
                    {
                        Url = baseUrl + "persi.do";
                        postdata = string.Format("method={0}&_xmlString=<?xml version=\"1.0\" encoding=\"UTF-8\"?><p><s nd=\"{1}\"/></p>&__logon_ticket={2}", type[i], year, ticket);
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "POST",
                            Postdata = postdata,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                        List<string> tempresult = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='dataTable']/tr");
                        results.AddRange(tempresult);

                    }

                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//input", "value");
                        if (tdRow.Count < 5)
                            continue;

                        if(tdRow.Count == 6)
                            tdRow.RemoveAt(1);

                        string SocialInsuranceTime = tdRow[0].Replace("-", "");
                        detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                        if (detailRes == null)
                        {
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.Name = Res.Name;
                            detailRes.IdentityCard = Res.IdentityCard;
                            detailRes.PayTime = SocialInsuranceTime;
                            detailRes.SocialInsuranceTime = SocialInsuranceTime;
                            detailRes.SocialInsuranceBase = tdRow[1].ToDecimal(0) > tdRow[3].ToDecimal(0) ? tdRow[1].ToDecimal(0) : tdRow[3].ToDecimal(0);
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            if (i == 0)
                            {
                                detailRes.PensionAmount = tdRow[4].ToDecimal(0);
                                detailRes.CompanyPensionAmount = tdRow[2].ToDecimal(0);
                            }
                            else
                            {
                                detailRes.MedicalAmount = tdRow[4].ToDecimal(0);
                                detailRes.CompanyMedicalAmount = tdRow[2].ToDecimal(0);
                            }
                            Res.Details.Add(detailRes);
                        }
                        else
                        {
                            if (i == 0)
                            {
                                detailRes.PensionAmount += tdRow[4].ToDecimal(0);
                                detailRes.CompanyPensionAmount += tdRow[2].ToDecimal(0);
                            }
                            else
                            {
                                detailRes.MedicalAmount += tdRow[4].ToDecimal(0);
                                detailRes.CompanyMedicalAmount += tdRow[2].ToDecimal(0);
                            }
                        }
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
