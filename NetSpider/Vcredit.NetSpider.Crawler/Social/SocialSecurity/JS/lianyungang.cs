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
    public class lianyungang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://zjdt.lyghrss.gov.cn:7001/lygzjdt/pages/";
        string socialCity = "js_lianyungang";
        #endregion

        #region 私有变量
        enum InfoType
        {
            养老保险,
            医疗保险,
            失业保险,
            工伤保险,
            生育保险
        }

        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息

        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, new string[] { "11" });
            PageHash.Add(InfoType.医疗保险, new string[] { "31" });
            PageHash.Add(InfoType.失业保险, new string[] { "21" });
            PageHash.Add(InfoType.工伤保险, new string[] { "41" });
            PageHash.Add(InfoType.生育保险, new string[] { "51" });
        }

        void GetAllDetail(InfoType Type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            SocialSecurityDetailQueryRes detailRes = new SocialSecurityDetailQueryRes();
            DateTime datenow = DateTime.Now;

            int pageno = 1;
            int pagecount = 1;

            while (pageno <= pagecount)
            {
                if (pageno == 1)
                {
                    Url = baseUrl + "/person/payment/pay.action";
                    postdata = string.Format("aac001={0}&aae140={1}&aae0031={2}&aae0032={3}", Res.EmployeeNo, ((string[])PageHash[Type])[0], datenow.AddMonths(1).AddYears(-5).ToString("yyyy-MM"), datenow.ToString("yyyy-MM"));
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Encoding = Encoding.UTF8,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    List<string> pagecountlist = HtmlParser.GetResultFromParser(httpResult.Html, "//td/font");
                    if (pagecountlist.Count > 0)
                    {
                        pagecount = pagecountlist[0].ToInt(0);
                    }
                }
                else
                {
                    Url = string.Format("{0}person/payment/pay.action?aae140={1}&aae0031={2}&aae0032={3}&pageno={4}", baseUrl, ((string[])PageHash[Type])[0], datenow.AddMonths(1).AddYears(-5).ToString("yyyy-MM"), datenow.ToString("yyyy-MM"), pageno);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Encoding = Encoding.UTF8,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                }
                results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='myPrintArea']//tr"));
                pageno++;
            }

            for (int i = 0; i < results.Count; i++)
            {
                var tdRow = HtmlParser.GetResultFromParser(results[i], "//td");
                if (tdRow.Count != 8)
                {
                    continue;
                }
                if (tdRow[0] == "单位名称")
                {
                    continue;
                }

                string SocialInsuranceTime = tdRow[3];
                detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                bool NeedAddNew = false;
                if (detailRes == null)
                {
                    detailRes = new SocialSecurityDetailQueryRes();
                    NeedAddNew = true;
                }
                switch (Type)
                {
                    case InfoType.养老保险:
                        detailRes.CompanyPensionAmount = tdRow[5].ToTrim().ToDecimal(0);
                        detailRes.PensionAmount = tdRow[6].ToTrim().ToDecimal(0);
                        break;
                    case InfoType.医疗保险:
                        detailRes.CompanyMedicalAmount = tdRow[5].ToTrim().ToDecimal(0);
                        detailRes.MedicalAmount = tdRow[6].ToTrim().ToDecimal(0);
                        break;
                    case InfoType.失业保险:
                        detailRes.UnemployAmount = tdRow[5].ToTrim().ToDecimal(0) + tdRow[6].ToTrim().ToDecimal(0);
                        break;
                    case InfoType.生育保险:
                        detailRes.MaternityAmount = tdRow[5].ToTrim().ToDecimal(0) + tdRow[6].ToTrim().ToDecimal(0);
                        break;
                    case InfoType.工伤保险:
                        detailRes.EmploymentInjuryAmount = tdRow[5].ToTrim().ToDecimal(0) + tdRow[6].ToTrim().ToDecimal(0);
                        break;
                }
                if (NeedAddNew)
                {
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;
                    detailRes.PayTime = SocialInsuranceTime;
                    detailRes.SocialInsuranceTime = SocialInsuranceTime;
                    detailRes.CompanyName = System.Web.HttpUtility.HtmlDecode(tdRow[0].Replace("&nbsp;", ""));
                    detailRes.SocialInsuranceBase = tdRow[4].ToDecimal(0);
                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    detailRes.PaymentFlag = tdRow[7] == "已实缴" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : tdRow[7];
                    Res.Details.Add(detailRes);
                }
            }
        }
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
                Url = baseUrl + "login/plogin.html";
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://zjdt.lyghrss.gov.cn:7001/lygzjdt/jcaptcha";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl + "login/plogin.html",
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

                SpiderCacheHelper.SetCache(token, cookies);
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
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                Url = baseUrl + "lygzj/person/personLogin.action";
                postdata = String.Format("servicepage=&account={0}&password={1}&jcaptcha=8888", socialReq.Identitycard, socialReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Encoding = Encoding.UTF8,
                    Postdata = postdata,
                    Referer = baseUrl + "login/plogin.html",
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
                string actionErrorsJson = CommonFun.GetMidStr(httpResult.Html, "var actionErrorsJson = [\"", "\"");
                string fieldErrorsJson = CommonFun.GetMidStr(httpResult.Html, "var fieldErrorsJson = {\"", "\"]};").Replace("password\":[\"", "").Replace("idcard\":[\"", "").Replace("jcaptcha\":[\"", "").Trim();
                if ((!actionErrorsJson.IsEmpty() && actionErrorsJson != "w3.org/1999/xhtml") || !string.IsNullOrEmpty(fieldErrorsJson))
                {
                    Res.StatusDescription = !string.IsNullOrEmpty(fieldErrorsJson) ? fieldErrorsJson : actionErrorsJson;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，个人基本信息
                Url = baseUrl + "person/pi/personInfo.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Encoding = Encoding.UTF8,
                    Postdata = postdata,
                    Referer = baseUrl + "main.html?loginField=0&servicepage=",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='myPrintArea']//td");
                if (results.Count == 17)
                {
                    Res.Name = System.Web.HttpUtility.HtmlDecode(results[2].Replace("&nbsp;", ""));
                    Res.IdentityCard = results[4].Replace("&nbsp;", "");
                    Res.EmployeeNo = results[6].Replace("&nbsp;", "");
                    Res.CompanyName = System.Web.HttpUtility.HtmlDecode(results[8].Replace("&nbsp;", ""));
                    Res.WorkDate = results[10].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html,"//input[@id='aac010']", "value");
                if (results.Count > 0)
                {
                    Res.Address = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='aae005']", "value");
                if (results.Count > 0)
                {
                    Res.Phone = results[0];
                }
                #endregion

                #region 第三步，查询明细
                InitPageHash();
                foreach (InfoType type in Enum.GetValues(typeof(InfoType)))
                {
                    GetAllDetail(type, ref Res);
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
