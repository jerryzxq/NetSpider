using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.HA
{
    public class luoyang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://222.88.66.57:9000/";
        string socialCity = "ha_luoyang";

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
        /// <summary>
        ///  将各种保险对应的页面所需信息存入PageHash
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, new string[] { "PerPensionInfo.aspx" });
            PageHash.Add(InfoType.医疗保险, new string[] { "PerMedicalInfo.aspx" });
            PageHash.Add(InfoType.失业保险, new string[] { "PerUnemploymentInfo.aspx" });
            PageHash.Add(InfoType.工伤保险, new string[] { "PerInjurylInfo.aspx" });
            PageHash.Add(InfoType.生育保险, new string[] { "PerBearInfo.aspx" });
        }
        private void GetAllDetail(InfoType type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            List<string> results = new List<string>();
            Url = baseUrl + ((string[])PageHash[type])[0];
            httpItem = new HttpItem()
            {
                URL = Url,
                Referer = baseUrl + "default.aspx?sg=2",
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);

            if (type == InfoType.养老保险)
            {
                results = HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", ""), "//tr[@id='error']//span[@id='Label11']");
                if (results.Count == 0)
                {
                    Res.InsuranceTotal = HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", ""), "//span[@id='siv006']", "text")[0]
                                     .ToDecimal(0);
                    Res.PaymentMonths = HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", ""), "//span[@id='siv005']", "text")[0].ToInt(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", ""), "//div[@class='main']/table[1]/tr[position()>2]/td", "", true);
                if (results.Count >= 5)
                {
                    Res.CompanyNo = results[1];
                    Res.CompanyName = results[2];
                }
            }
            results = HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", ""), "//div[@class='main']/table[2]/tr[position()>2]", "", true);
            foreach (string item in results)
            {
                SocialSecurityDetailQueryRes detailRes = null;
                var tdRow = HtmlParser.GetResultFromParser(item, "//td", "");
                if (tdRow.Count < 10) continue;
                if (tdRow[7] == "欠缴") continue;
                detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == tdRow[1]);
                bool isSave = false;
                if (detailRes == null)
                {
                    isSave = true;
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.CompanyName = tdRow[9];
                    detailRes.PayTime = tdRow[0];
                    detailRes.SocialInsuranceTime = tdRow[1];
                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    detailRes.PaymentType = tdRow[7];
                }
                switch (type)
                {
                    case InfoType.养老保险:
                        detailRes.CompanyPensionAmount += tdRow[3].ToDecimal(0);
                        detailRes.PensionAmount += tdRow[4].ToDecimal(0);
                        detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                        break;
                    case InfoType.医疗保险:
                        detailRes.CompanyMedicalAmount += tdRow[3].ToDecimal(0);
                        detailRes.MedicalAmount += tdRow[4].ToDecimal(0);
                        detailRes.EnterAccountMedicalAmount += tdRow[5].ToDecimal(0);
                        break;
                    case InfoType.失业保险:
                        detailRes.UnemployAmount += tdRow[5].ToDecimal(0);
                        break;
                    case InfoType.工伤保险:
                        detailRes.EmploymentInjuryAmount += tdRow[5].ToDecimal(0);
                        break;
                    case InfoType.生育保险:
                        detailRes.MaternityAmount += tdRow[5].ToDecimal(0);
                        break;
                }
                if (isSave)
                {
                    Res.Details.Add(detailRes);
                }
            }
        }
        #endregion
        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "login.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "validateCode.yzm?t=2";
                httpItem = new HttpItem()
                {
                    URL = Url,
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
                if (socialReq.Password.IsEmpty() || socialReq.Identitycard.IsEmpty() || socialReq.Vercode.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                Url = baseUrl + "AjaxLogin/Personloginin.cspx";
                postdata = String.Format("IdCard={0}&Password={1}&validatecode={2}", socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "login.html",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header["X-Requested-With"] = "XMLHttpRequest";
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                String errorMsg = jsonParser.GetResultFromParser(httpResult.Html, "Success");
                if (errorMsg != "True")
                {
                    Res.StatusDescription = jsonParser.GetResultFromParser(httpResult.Html, "Message");
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                Url = baseUrl + "default.aspx?sg=2";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "login.html",
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
                #endregion
                #region 第二步 获取基本信息

                //基本信息设置
                Url = baseUrl + "PerBasicInfo.aspx";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = baseUrl + "default.aspx?sg=2",
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='F002']", "text");
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='F004']", "text");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='F007ZH']", "text");
                if (results.Count > 0)
                {
                    Res.Sex = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='F008ZH']", "text");
                if (results.Count > 0)
                {
                    Res.Race = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='F003']", "text");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='F009']", "text");
                if (results.Count > 0)
                {
                    Res.BirthDate = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='F010']", "text");
                if (results.Count > 0)
                {
                    Res.WorkDate = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='F021']", "text");
                if (results.Count > 0)
                {
                    Res.EmployeeStatus = results[0];
                }
                //results = HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", ""), "//div[@class='main']/table[2]/tr[position()>2]", "",true);
                //foreach (string item in results)
                //{
                //    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text");
                //    if (tdRow.Count<10)
                //    {
                //        continue;
                //    }
                //    if (string.IsNullOrEmpty(Res.CompanyName))
                //    {
                //        Res.CompanyName = tdRow[8];
                //    }
                //    Res.SpecialPaymentType += tdRow[0] + "(" + tdRow[1] + "～" + tdRow[2] + ")" + tdRow[4]+";";
                //}

                //实名认证 
                Url = baseUrl + "Approve/CertifyCheck.aspx";
                httpItem = new HttpItem
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='_F007']", "text");
                if (results.Count > 0)
                {
                    Res.Phone = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='F010']", "value");
                if (results.Count > 0)
                {
                    Res.Address = results[0];
                }
                #endregion
                #region 第三步 缴费详细

                InitPageHash();
                foreach (InfoType type in Enum.GetValues(typeof(InfoType)))
                {
                    try
                    {
                        GetAllDetail(type, ref Res);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                }
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
