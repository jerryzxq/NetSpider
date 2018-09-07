using System;
using System.Collections;
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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.ZJ
{
    public class lanxi : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.zjlxlss.gov.cn/lxlss/";
        public string socialCity = "zj_lanxi";
        public List<string> commonDetails = new List<string>();
        #endregion
        #region 私有变量


        #endregion
        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "operation/login.jsp";
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string num = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='num']", "value")[0];//验证码
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Dictionary<string, object> dic = new Dictionary<string, object>
                {
                    {"num",num},
                    {"cookies",cookies}
                };
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
            List<string> results = new List<string>();
            string num = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    Dictionary<string, object> dic = (Dictionary<string, object>)CacheHelper.GetCache(socialReq.Token);
                    cookies = (CookieCollection)dic["cookies"];
                    num = (string)dic["num"];
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

                Url = baseUrl + "operation/UserLogin";
                postdata = String.Format("type=2&ucardcode={0}&uname={1}&password={2}&num_in={3}&num={3}&Submit=%B5%C7+++%C2%BC", socialReq.Identitycard, socialReq.Name.ToUrlEncode(Encoding.GetEncoding("GBK")), socialReq.Password, num);
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='errors']", "value");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion
                #region 第二步， 获取基本信息

                //参保人员基本信息
                Url = baseUrl + "operation/person/personBase.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", ""), "//td/table[@class='table_style02'][1]/tr[position()>1]/td", "text", true);
                if (results.Count != 18)
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                Res.Name = socialReq.Name;
                Res.IdentityCard = socialReq.Identitycard;
                Res.IdentityCard = results[1];
                Res.Name = results[3];
                Res.Sex = results[5];
                Res.Race = results[7];
                Res.BirthDate = results[9];
                Res.EmployeeStatus = results[11];
                Res.Address = results[13];
                Res.Phone = results[15];
                Res.CompanyName = results[17];
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td/table[@class='table_style02'][last()]//tr[position()>1]/td", "text", true);
                for (int i = 0; i <= results.Count - 3; i = i + 3)
                {
                    Res.SpecialPaymentType += results[i] + ":" + results[i + 2] + ";";
                }
                Url = baseUrl + "operation/person/personbeforepay.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td/table[@class='table_style02']/tr[2]/td[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.OldPaymentMonths = results[0].ToTrim("&nbsp;").ToInt(0);
                }
                #endregion
                #region 第三步，查询明细

                results = new List<string>();
                Url = baseUrl + "operation/person/personpay.jsp";
                int spage = 1;
                int pageCount = 1;
                do
                {
                    postdata = string.Format("spage={0}", spage);
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
                        break;
                    }
                    List<string> breakPoint = HtmlParser.GetResultFromParser(httpResult.Html, "//tr[@class='table_tr_01']", "", true);
                    if (breakPoint.Count == 0) break;
                    breakPoint.RemoveAt(breakPoint.Count - 1);
                    results.AddRange(breakPoint);
                    if (spage == 1)
                    {
                        pageCount = CommonFun.GetMidStr(httpResult.Html, "</font>/", "页").ToInt(0);
                    }
                    spage++;
                } while (spage <= pageCount);
                //保存缴费明细
                string[] needType = { "养老保险", "基本养老保险", "机关事业养老保险", "医疗保险", "基本医疗保险", "失业保险", "工伤保险", "生育保险" };
                foreach (string s in results)
                {
                    List<string> tdRow = HtmlParser.GetResultFromParser(s, "//td", "");
                    if (tdRow.Count != 6) continue;
                    if (!needType.Contains(tdRow[2]) & tdRow[2].IndexOf("大病") == -1 & tdRow[2].IndexOf("公务员") == -1) continue;
                    detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == tdRow[0]);
                    bool isSave = false;
                    if (detailRes == null)
                    {
                        isSave = true;
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;
                        detailRes.SocialInsuranceTime = tdRow[0];
                        detailRes.CompanyName = tdRow[1];
                        detailRes.PayTime = tdRow[5];
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    }
                    switch (tdRow[2])
                    {
                        case "养老保险":
                        case "基本养老保险":
                        case "机关事业养老保险":
                            detailRes.PensionAmount += tdRow[4].ToDecimal(0);
                            detailRes.CompanyPensionAmount += tdRow[4].ToDecimal(0) * 0.19M / 0.08M;
                            detailRes.SocialInsuranceBase = tdRow[3].ToDecimal(0);
                            break;
                        case "医疗保险":
                        case "基本医疗保险":
                            detailRes.MedicalAmount += tdRow[4].ToDecimal(0);
                            detailRes.CompanyMedicalAmount += tdRow[4].ToDecimal(0) * 0.08M / 0.02M;
                            detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : tdRow[3].ToDecimal(0);
                            break;
                        case "失业保险":
                            detailRes.UnemployAmount += tdRow[4].ToDecimal(0);
                            break;
                        case "工伤保险":
                            detailRes.EmploymentInjuryAmount += tdRow[4].ToDecimal(0);
                            break;
                        case "生育保险":
                            detailRes.MaternityAmount += tdRow[4].ToDecimal(0);
                            break;
                        default:
                            if (tdRow[2].IndexOf("大病", StringComparison.Ordinal) > -1)
                            {
                                detailRes.IllnessMedicalAmount += tdRow[4].ToDecimal(0);
                            }
                            else if (tdRow[2].IndexOf("公务员", StringComparison.Ordinal) > -1)
                            {
                                detailRes.CivilServantMedicalAmount += tdRow[4].ToDecimal(0);
                            }
                            break;
                    }
                    detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : tdRow[3].ToDecimal(0);
                    if (!isSave) continue;
                    Res.Details.Add(detailRes);
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
