using System;
using System.Collections;
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
namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.JS
{
    public class zhangjiagang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.12333zjg.gov.cn/Searchzjgsb/";
        string socialCity = "js_zhangjiagang";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            Dictionary<string, object> _dic = new Dictionary<string, object>();
            try
            {
                Url = baseUrl + "SearchForm1.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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

                string __VIEWSTATE = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                if (results.Count > 0)
                {
                    __VIEWSTATE = results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.SocialSecurity_InitFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string __EVENTVALIDATION = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                if (results.Count > 0)
                {
                    __EVENTVALIDATION = results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.SocialSecurity_InitFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "VerifyCode.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl + "SearchForm1.aspx",
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

                _dic.Add("cookie", cookies);
                _dic.Add("__VIEWSTATE", __VIEWSTATE);
                _dic.Add("__EVENTVALIDATION", __EVENTVALIDATION);

                SpiderCacheHelper.SetCache(token, _dic);
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
            string __VIEWSTATE = string.Empty;
            string __EVENTVALIDATION = string.Empty;
            List<string> results = new List<string>();
            Dictionary<string, object> _dic = new Dictionary<string, object>();
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    _dic = (Dictionary<string, object>)SpiderCacheHelper.GetCache(socialReq.Token);
                    cookies = (CookieCollection)_dic["cookie"];
                    __VIEWSTATE = _dic["__VIEWSTATE"].ToString();
                    __EVENTVALIDATION = _dic["__EVENTVALIDATION"].ToString();
                    SpiderCacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if ((socialReq.Identitycard.IsEmpty() && socialReq.LoginType == "1") || (socialReq.Username.IsEmpty() && socialReq.LoginType == "2") || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                Url = baseUrl + "SearchForm1.aspx";
                if (socialReq.LoginType == "1")
                {
                    postdata = String.Format("__VIEWSTATE={0}&__EVENTVALIDATION={1}&ctl00%24ContentPlaceHolder1%24RadioButtonList1=%E5%B1%85%E6%B0%91%E8%BA%AB%E4%BB%BD%E8%AF%81%E5%8F%B7&ctl00%24ContentPlaceHolder1%24TextBox4={2}&ctl00%24ContentPlaceHolder1%24TextBox2={3}&ctl00%24ContentPlaceHolder1%24TextBox3={4}&ctl00%24ContentPlaceHolder1%24Button1=%E7%99%BB%E5%BD%95", __VIEWSTATE.ToUrlEncode(), __EVENTVALIDATION.ToUrlEncode(), socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
                }
                else if (socialReq.LoginType == "2")
                {
                    postdata = string.Format("__VIEWSTATE={0}&__EVENTVALIDATION={1}&ctl00%24ContentPlaceHolder1%24RadioButtonList1=%E4%B8%AA%E4%BA%BA%E5%8F%82%E4%BF%9D%E7%BC%96%E5%8F%B7&ctl00%24ContentPlaceHolder1%24TextBox4={2}&ctl00%24ContentPlaceHolder1%24TextBox2={3}&ctl00%24ContentPlaceHolder1%24TextBox3={4}&ctl00%24ContentPlaceHolder1%24Button1=%E7%99%BB%E5%BD%95", __VIEWSTATE.ToUrlEncode(), __EVENTVALIDATION.ToUrlEncode(), socialReq.Username, socialReq.Password, socialReq.Vercode);
                }
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Encoding = Encoding.UTF8,
                    Postdata = postdata,
                    Referer = baseUrl + "SearchForm1.aspx",
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label1']");
                if (results.Count > 0 && !results[0].Contains("养老保险") && !results[0].Contains("医保"))
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                if (results.Count > 0)
                {
                    __VIEWSTATE = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                if (results.Count > 0)
                {
                    __EVENTVALIDATION = results[0];
                }
                #endregion

                #region 第二步，个人基本信息
                Res.Name = socialReq.Name;
                Res.CompanyName = socialReq.Identitycard;
                #endregion

                #region 第三步，查询明细
                for (int i = 0; i < 5; i++)
                {
                    if (i == 0)
                    {
                        Url = baseUrl + "SearchForm2.aspx";
                        postdata = String.Format("__VIEWSTATE={0}&__EVENTVALIDATION={1}&ctl00%24ContentPlaceHolder1%24Button3=%E5%85%BB%E8%80%81%E4%BF%9D%E9%99%A9%E7%BC%B4%E8%B4%B9%E6%B5%81%E6%B0%B4&ctl00%24ContentPlaceHolder1%24DropDownList2={2}&ctl00%24ContentPlaceHolder1%24DropDownList3={3}&ctl00%24ContentPlaceHolder1%24TextBox4=&ctl00%24ContentPlaceHolder1%24TextBox1=&ctl00%24ContentPlaceHolder1%24TextBox2=&ctl00%24ContentPlaceHolder1%24TextBox3=", __VIEWSTATE.ToUrlEncode(), __EVENTVALIDATION.ToUrlEncode(), DateTime.Now.AddYears(-i).Year, DateTime.Now.Month);
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "POST",
                            Encoding = Encoding.UTF8,
                            Postdata = postdata,
                            Referer = baseUrl + "SearchForm2.aspx",
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                        if (results.Count > 0)
                        {
                            __VIEWSTATE = results[0];
                        }
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                        if (results.Count > 0)
                        {
                            __EVENTVALIDATION = results[0];
                        }
                    }

                    postdata = String.Format("__VIEWSTATE={0}&__EVENTVALIDATION={1}&ctl00%24ContentPlaceHolder1%24DropDownList2={2}&ctl00%24ContentPlaceHolder1%24DropDownList3={3}&ctl00%24ContentPlaceHolder1%24Button2=%E6%9F%A5++%E8%AF%A2&ctl00%24ContentPlaceHolder1%24TextBox4=&ctl00%24ContentPlaceHolder1%24TextBox1=&ctl00%24ContentPlaceHolder1%24TextBox2=&ctl00%24ContentPlaceHolder1%24TextBox3=", __VIEWSTATE.ToUrlEncode(), __EVENTVALIDATION.ToUrlEncode(), DateTime.Now.AddYears(-i).Year, DateTime.Now.Month);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Encoding = Encoding.UTF8,
                        Postdata = postdata,
                        Referer = baseUrl + "SearchForm2.aspx",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                    if (results.Count > 0)
                    {
                        __VIEWSTATE = results[0];
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                    if (results.Count > 0)
                    {
                        __EVENTVALIDATION = results[0];
                    }

                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='ctl00_ContentPlaceHolder1_GridView1']/tr");
                    foreach(string item in results)
                    {
                        List<string> _detail = HtmlParser.GetResultFromParser(item, "/td");
                        if (_detail.Count == 5)
                        {
                            string[] Time = _detail[0].Split('/');
                            if (Time.Count() != 3)
                            {
                                continue;
                            }

                            string SocialInsuranceTime = Time[0] + Time[1];
                            detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                            if(detailRes == null)
                            {
                                detailRes = new SocialSecurityDetailQueryRes();
                                detailRes.Name = Res.Name;
                                detailRes.IdentityCard = Res.IdentityCard;
                                detailRes.PayTime = SocialInsuranceTime;
                                detailRes.SocialInsuranceTime = SocialInsuranceTime;
                                detailRes.SocialInsuranceBase = _detail[2].ToDecimal(0);
                                detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                                detailRes.PaymentFlag = _detail[4].Contains("正常") ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : _detail[4];

                                Res.Details.Add(detailRes);
                            }
                        }
                    }
                }


                #endregion

                    //Res.PaymentMonths = PaymentMonths;
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
