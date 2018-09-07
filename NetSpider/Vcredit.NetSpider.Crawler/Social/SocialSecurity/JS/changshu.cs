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
    public class changshu : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.cssbzx.com/cssbfw/";
        string socialCity = "js_changshu";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            List<string> results = new List<string>();
            try
            {
                Url = baseUrl + "Register/loginUser.aspx";
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
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                string __VIEWSTATE = string.Empty;
                if (results.Count > 0 && !results[0].IsEmpty())
                {
                    __VIEWSTATE = results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.SocialSecurity_InitFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Span_img']/img", "src");
                string src = string.Empty;
                if (results.Count > 0 && !results[0].IsEmpty())
                {
                    src = results[0].Replace("/cssbfw/", "");
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.SocialSecurity_InitFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + src;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl + "Register/loginUser.aspx",
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

                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("cookie", cookies);
                dic.Add("viewstate", __VIEWSTATE);
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
            string idCard = string.Empty;
            string __VIEWSTATE = string.Empty;
            string html = string.Empty;
            List<string> results = new List<string>();
            Dictionary<string, object> dic = new Dictionary<string, object>();
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    dic = (Dictionary<string, object>)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                    cookies = (CookieCollection)dic["cookie"];
                    __VIEWSTATE = dic["viewstate"].ToString();
                }
                //校验参数
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                Url = baseUrl + "Register/loginUser.aspx";
                postdata = String.Format("__VIEWSTATE={0}&txtKey=&txtUserName={1}&txtPwd={2}&txtYzm={3}&btnLogin=%E7%99%BB%E5%BD%95", __VIEWSTATE.ToUrlEncode(), socialReq.Username, socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "Register/loginUser.aspx",
                    Encoding = Encoding.UTF8,
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                if (!httpResult.Html.Contains("登录成功"))
                {
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//body/form/script");
                    if (results.Count > 0)
                    {
                        Res.StatusDescription = CommonFun.GetMidStr(results[0], "alert('", "'");
                    }
                    else
                    {
                        Res.StatusDescription = "登录失败，请重试！";
                    }
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion

                #region 获取基本信息

                Url = baseUrl + "Register/UserInfo.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "Register/loginUser.aspx",
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='txtTrueName']", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='txtSfz']", "value");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='txtPhone']", "value");
                if (results.Count > 0)
                {
                    Res.Phone = results[0];
                }

                Url = baseUrl + "wsfw/search.aspx?type=1";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "Register/UserInfo.aspx",
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tb_runtime']/tr/td[@bgcolor='#f7f7f7']");
                if (results.Count == 21)
                {
                    Res.Sex = CommonFun.ClearFlag(results[2]);
                    Res.Address = CommonFun.ClearFlag(results[9]);
                    Res.ZipCode = CommonFun.ClearFlag(results[12]); 
                    Res.CompanyName = CommonFun.ClearFlag(results[13]);
                    Res.EmployeeStatus = CommonFun.ClearFlag(results[15]);
                    Res.SpecialPaymentType = CommonFun.ClearFlag(results[16]);
                }
                #endregion

                #region 查询明细
                Url = baseUrl + "wsfw/search.aspx?type=2";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "Register/UserInfo.aspx",
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                if (results.Count > 0 && !results[0].IsEmpty())
                {
                    __VIEWSTATE = results[0];
                }

                int pageno = 0;
                int pagecount = 1;
                do
                {
                    pageno++;
                    Url = baseUrl + "wsfw/search.aspx?type=2";
                    postdata = string.Format("__EVENTTARGET=myhs&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE={0}&txtKey=&myhs=20&yh={1}", __VIEWSTATE.ToUrlEncode(), pageno);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Referer = baseUrl + "Register/UserInfo.aspx",
                        Encoding = Encoding.UTF8,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                    if (pagecount == 1)
                    {
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@name='yh']/option");
                        pagecount = results.Count;
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                    if (results.Count > 0 && !results[0].IsEmpty())
                    {
                        __VIEWSTATE = results[0];
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='DataGrid']/tr");
                    if (results.Count > 0)
                    {
                        results.RemoveAt(0);
                    }
                    foreach (string item in results)
                    {
                        List<string> _detail = HtmlParser.GetResultFromParser(item, "/td");

                        if (_detail.Count == 4)
                        {
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.Name = Res.Name;
                            detailRes.PayTime = CommonFun.ClearFlag(_detail[1]);
                            detailRes.SocialInsuranceTime = CommonFun.ClearFlag(_detail[1]);
                            detailRes.CompanyName = CommonFun.ClearFlag(_detail[2]);
                            detailRes.SocialInsuranceBase = CommonFun.ClearFlag(_detail[3]).ToDecimal(0);
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;

                            Res.Details.Add(detailRes);
                        }
                    }
                }
                while (pageno < pagecount);
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
