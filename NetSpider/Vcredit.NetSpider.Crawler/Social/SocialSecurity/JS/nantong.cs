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
    public class nantong : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.jsnt.lss.gov.cn:7777/siinfo/";
        string socialCity = "js_nantong";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "plogin.do?method=begin";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "CreateImage?Image=48|54|48|56|54|52|50|55|&Rgb=105|200|100";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl + "plogin.do?method=begin",
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
                //Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                ////保存验证码图片在本地
                //FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                //Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                //Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);

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

                Url = baseUrl + "checkPortalLogin.do?method=login";
                postdata = String.Format("cardType=B&username={0}&password={1}&attach=7738&get_attach=7738&loginType=P", socialReq.Username, socialReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "plogin.do?method=begin",
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
                if (httpResult.Html.Contains("请输入用户名"))
                {
                    Res.StatusDescription = "用户名或密码错误";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取基本信息

                Url = baseUrl + "persionbase.do?method=begin";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "checkPortalLogin.do?method=login",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//dataobj");
                foreach (string item in results)
                {
                    List<string> info = HtmlParser.GetResultFromParser(item, "//attribute", "newvalue");
                    if (info.Count == 8 && !info[0].IsEmpty())
                    {
                        Res.EmployeeNo = info[0];
                        Res.IdentityCard = info[2];
                        Res.Name = System.Web.HttpUtility.HtmlDecode(info[3]);
                        Res.Sex = info[4] == "1" ? "男" : "女";
                        List<string> race = HtmlParser.GetResultFromParser(httpResult.Html, "//option[@value='" + info[5] +"']");
                        if (race.Count > 0)
                        {
                            Res.Race = race[0];
                        }
                    }
                    if (info.Count == 19 && !info[0].IsEmpty())
                    {
                        Res.CompanyName = System.Web.HttpUtility.HtmlDecode(info[2]);
                        Res.CompanyNo = System.Web.HttpUtility.HtmlDecode(info[1]);
                        List<string> status = HtmlParser.GetResultFromParser(httpResult.Html, "//option[@value='" + info[8] + "']");
                        if (status.Count > 0)
                        {
                            Res.EmployeeStatus = status[0];
                        }
                        break;
                    }
                }

                #endregion

                #region 查询明细
                
                //Url = baseUrl + "personfactcapture.do?method=begin";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Referer = baseUrl + "checkPortalLogin.do?method=login",
                //    Host = "www.jsnt.lss.gov.cn:7777",
                //    UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/7.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.3; .NET4.0C; .NET4.0E)",
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dwid_siquery.SI_TOUCH_GRYJSJJL']", "value");
                //if(results.Count > 0)
                //    postdata = "dwid_siquery.SI_TOUCH_GRYJSJJL=" + results[0] + "&dwName=siquery.SI_TOUCH_GRYJSJJL";

                //results.Clear();
                //results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//dataobj"));

                //Url = "http://www.jsnt.lss.gov.cn:7777/siinfo/DataWindowMgrAction.do?method=loadFirstPage&isPartlyRefresh=true";
                ////Url = baseUrl + "DataWindowMgrAction.do?method=loadNextPage&isPartlyRefresh=true";
                
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "POST",
                //    Accept = "*/*",
                //    Postdata = postdata,
                //    Referer = baseUrl + "personfactcapture.do?method=begin",
                //    Host = "www.jsnt.lss.gov.cn:7777",
                //    UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/7.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.3; .NET4.0C; .NET4.0E)",
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                Url = baseUrl + "personcaptureinf.do?method=begin";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "checkPortalLogin.do?method=login",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//dataobj");

                List<List<string>> YangLaoList = new List<List<string>>();
                List<List<string>> YiLiaoList = new List<List<string>>();
                List<List<string>> ShiYeList = new List<List<string>>();
                List<List<string>> GongShangList = new List<List<string>>();
                List<List<string>> ShengYuList = new List<List<string>>();
                foreach (string item in results)
                {
                    List<string> row = HtmlParser.GetResultFromParser(item, "//attribute", "newvalue");
                    if (row.Count == 14)
                    {
                        if (row[6].ToInt(0) == 0)
                            continue;
                        int startyear = row[6].Substring(0,4).ToInt(0);
                        int startmonth = row[6].Substring(4, 2).ToInt(0);
                        string endtime = row[5].ToInt(0) == 0 ? DateTime.Now.ToString("yyyyMM") : row[5];
                        int endyear = endtime.Substring(0, 4).ToInt(0);
                        int endmonth = endtime.Substring(4, 2).ToInt(0);
                        string type = row[12];

                        for (int i = endyear; i >= startyear; i--)
                        {
                            int calstartmonth = 1;
                            int calendmonth = 12;
                            if(i == startyear)
                            {
                                calstartmonth = startmonth;
                            }
                            if(i == endyear)
                            {
                                calendmonth = endmonth;
                            }
                            for (int j = calendmonth; j >= calstartmonth; j--)
                            {
                                List<string> detail = new List<string>();
                                detail.Add((i*100 + j).ToString());
                                detail.Add(row[4]);

                                switch (type)
                                {
                                    case "11":
                                        YangLaoList.Add(detail);
                                        break;
                                    case "31":
                                        YiLiaoList.Add(detail);
                                        break;
                                    case "21":
                                        ShiYeList.Add(detail);
                                        break;
                                    case "41":
                                        GongShangList.Add(detail);
                                        break;
                                    case "51":
                                        ShengYuList.Add(detail);
                                        break;
                                }
                            }
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
