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
    public class taicang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.tchrss.gov.cn/tclss/wsbsdt/";
        string socialCity = "js_taicang";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "grwsywcx.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

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
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                Url = baseUrl + "logincheck.jsp";
                postdata = String.Format("type=2&EAC012={0}&password={1}&AAC002={2}&Submit=%C8%B7+%B6%A8", socialReq.Username, socialReq.Password, socialReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "grwsywcx.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode == HttpStatusCode.InternalServerError)
                {
                    Res.StatusDescription = "人员编号或社保识别号错误";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                }
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                string msg = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\"");
                if (!httpResult.Html.Contains("view_grjcxx.jsp") && !msg.IsEmpty())
                {
                    Res.StatusDescription = msg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取基本信息

                Url = baseUrl + "view_grjcxx.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "logincheck.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_wsbsdt']/tr/td");
                if (results.Count == 13)
                {
                    Res.Name = results[4];
                    Res.IdentityCard = results[6];
                    Res.CompanyNo = results[8];
                    Res.CompanyName = results[10];
                    Res.EmployeeStatus = results[12];
                }

                #endregion

                #region 查询明细

                List<List<string>> YangLaoList = new List<List<string>>();
                List<List<string>> YiLiaoList = new List<List<string>>();
                List<List<string>> ShiYeList = new List<List<string>>();
                List<List<string>> GongShangList = new List<List<string>>();
                List<List<string>> ShengYuList = new List<List<string>>();

                for (int i = 0; i < 5; i++)
                {
                    Url = baseUrl + "view_grjaofei.jsp";
                    postdata = string.Format("AKA101={0}&Submit=%C8%B7+%B6%A8", DateTime.Now.AddYears(-i).Year);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        //Referer = baseUrl + "checkPortalLogin.do?method=login",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_wsbsdt']/tr");
                    foreach (string item in results)
                    {
                        List<string> row = HtmlParser.GetResultFromParser(item, "/td");
                        if (row.Count != 4)
                        {
                            continue;
                        }
                        if(row[0] == "年月")
                        {
                            continue;
                        }
                        List<string> _detail = new List<string>();
                        _detail.Add(row[0]);
                        _detail.Add(row[2]);
                        switch (row[1])
                        {
                            case "企业养老保险":
                                YangLaoList.Add(_detail);
                                break;
                            case "基本医疗保险":
                                YiLiaoList.Add(_detail);
                                break;
                            case "失业保险":
                                ShiYeList.Add(_detail);
                                break;
                            case "工伤保险":
                                GongShangList.Add(_detail);
                                break;
                            case "生育保险":
                                ShengYuList.Add(_detail);
                                break;
                        }
                    }
                }

                List<List<string>> CalList = new List<List<string>>();
                if (YangLaoList.Count > 0)
                {
                    CalList = YangLaoList;
                }
                else if (YiLiaoList.Count > 0)
                {
                    CalList = YiLiaoList;
                }
                else if (ShiYeList.Count > 0)
                {
                    CalList = ShiYeList;
                }
                else if (GongShangList.Count > 0)
                {
                    CalList = GongShangList;
                }
                else if (ShengYuList.Count > 0)
                {
                    CalList = ShengYuList;
                }
                foreach (List<string> temp in CalList)
                {
                    detailRes = Res.Details.Where(o => o.SocialInsuranceTime == temp[0]).FirstOrDefault();
                    if (detailRes != null)
                        continue;
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;
                    detailRes.SocialInsuranceBase = temp[1].ToDecimal(0);
                    detailRes.PayTime = temp[0];
                    detailRes.SocialInsuranceTime = temp[0];
                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;

                    Res.Details.Add(detailRes);
                }

                #endregion

                Res.PaymentMonths = CalList.Count;

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
