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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.ZJ
{
    public class pinghu : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://220.191.221.83:8080/synet/";
        string socialCity = "zj_pinghu";
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
                Url = baseUrl + "login.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
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
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录

                Url = baseUrl + "login_check.jsp";
                postdata = String.Format("ccc001={0}&Submit=%D3%C3%BB%A7%B5%C7%C2%BC&code={1}&ccc004={2}", socialReq.Identitycard, socialReq.Username, socialReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "login.jsp",
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//script");
                if (results.Count == 1)
                {
                    string msg = CommonFun.GetMidStr(results[0], "alert(\"", "\"");
                    if (msg != "登录成功！")
                    {
                        Res.StatusDescription = msg;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                else
                {
                    Res.StatusDescription = "登录失败！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 基本信息
                Url = baseUrl + "person/person_message.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "login_check.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@bgcolor='#9db5d4']/tr/td");
                if (results.Count != 12)
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Res.Name = results[1].ToTrim().ToTrim("&nbsp");
                Res.EmployeeNo = results[3].ToTrim().ToTrim("&nbsp");
                Res.IdentityCard = results[5].ToTrim().ToTrim("&nbsp");
                Res.Phone = results[7].ToTrim().ToTrim("&nbsp");
                Res.Sex = results[9].ToTrim().ToTrim("&nbsp");
                Res.BirthDate = results[11].ToTrim().ToTrim("&nbsp");
#endregion
                #region 明细
                int pageindex = 1;
                int pagecount = 0;

                do
                {
                    Url = baseUrl + "person/person_yanglao.jsp?&newsPageNo=" + pageindex;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (pagecount == 0)
                    {
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='90%']");
                        if (results.Count == 2)
                        {
                            pagecount = CommonFun.GetMidStr(results[1], "页 共", "页").ToInt(0);
                        }
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@style='border: 1px #C5C5C5 solid;']/tr");
                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 9 || tdRow[0] == "缴费年月")
                        {
                            continue;
                        }
                        if (Res.Name.IsEmpty())
                            Res.Name = tdRow[0].Trim().Replace("&nbsp;", "");//姓名
                        if (Res.CompanyName.IsEmpty())
                            Res.CompanyName = tdRow[3].Trim().Replace("&nbsp;", "");//单位名称

                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.PayTime = tdRow[0];
                        detailRes.SocialInsuranceTime = tdRow[0];
                        detailRes.CompanyName = tdRow[3].Trim().Replace("&nbsp;", "");
                        detailRes.SocialInsuranceBase = tdRow[4].ToDecimal(0);
                        detailRes.PaymentType = tdRow[2] == "正常应缴" ? ServiceConsts.SocialSecurity_PaymentType_Normal : tdRow[2];
                        detailRes.PaymentFlag = tdRow[2] == "正常应缴" || tdRow[2] == "补缴" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : tdRow[2];
                        detailRes.CompanyPensionAmount = tdRow[5].ToDecimal(0);
                        detailRes.PensionAmount = tdRow[6].ToDecimal(0);
                        
                        Res.Details.Add(detailRes);

                        if (tdRow[2] == "补缴" && Res.Description.IsEmpty())
                        {
                            Res.Description = "有断缴、补缴，请人工校验";
                        }
                    }
                    pageindex++;
                } while (pageindex <= pagecount);

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
