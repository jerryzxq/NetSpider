using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.SC
{
    public class yibin : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://cx2.ybxww.cn:6655/";
        string socialCity = "sc_yibin";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "login.asp";
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
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
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
            List<string> results = new List<string>();
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(socialReq.Token);
                    SpiderCacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                Url = baseUrl + "login.asp";
                postdata = String.Format("sfzh={0}&username={1}&pass={2}", socialReq.Identitycard, socialReq.Name.ToUrlEncode(Encoding.GetEncoding("gb2312")), socialReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Encoding = Encoding.GetEncoding("gb2312"),
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    return Res;
                }
                string tipMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');").Trim();
                if (tipMsg.IndexOf("成功", StringComparison.Ordinal) == -1 && !string.IsNullOrEmpty(tipMsg))
                {
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    Res.StatusDescription = tipMsg;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                #endregion
                #region 第二步，个人基本信息
                ////个人基本信息
                //Url = baseUrl + "hrss.asp?id=a1";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                //    Res.StatusDescription = httpResult.Html;
                //    return Res;
                //}
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tb1']/tr/td[2]");
                //if (results.Count == 6)
                //{
                //    Res.EmployeeNo = results[0] == "" ? null : results[0];
                //    Res.Name = results[1] == "" ? null : results[1];
                //    Res.Sex = results[2] == "" ? null : results[2];
                //    Res.IdentityCard = results[3] == "" ? null : results[3];
                //    Res.BirthDate = results[4] == "" ? null : DateTime.ParseExact(results[4], "yyyyMMdd", CultureInfo.InvariantCulture).ToString(Consts.DateFormatString2);
                //    Res.WorkDate = results[5] == "" ? null : DateTime.ParseExact(results[5], "yyyyMMdd", CultureInfo.InvariantCulture).ToString(Consts.DateFormatString2);
                //}
                //养老保险参保信息
                Url = baseUrl + "hrss.asp?id=b1";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//font");
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    Res.StatusDescription = results.Count > 1 ? results[2] : httpResult.Html;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td/table/tr/td[2]");
                if (results.Count == 12)
                {
                    Res.EmployeeNo = results[0] == "" ? null : results[0];
                    Res.Name = results[1] == "" ? null : results[1];
                    Res.IdentityCard = results[2] == "" ? null : results[2];
                    Res.WorkDate = results[3] == "" ? null : DateTime.ParseExact(results[3], "yyyyMMdd", CultureInfo.InvariantCulture).ToString(Consts.DateFormatString2);

                    Res.CompanyNo = results[4];
                    Res.CompanyName = results[5];
                    //Res = results[6];//人员类别 职工
                    //Res.Payment_State = results[7];
                    Res.PaymentMonths = results[8].ToInt(0);
                    Res.SocialInsuranceBase = results[9].ToDecimal(0);
                    Res.DeadlineYearAndMonth = results[10];
                    Res.PersonalInsuranceTotal = results[11].Replace(@"\r\n", "").ToDecimal(0);
                }
                #endregion
                #region 第三步，养老

                Url = baseUrl + "hrss.asp?id=b2";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//font");
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    Res.StatusDescription = results.Count > 1 ? results[2] : httpResult.Html;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td/table/tr[position()>1]");

                foreach (string olderItem in results)
                {
                    bool isSave = false;
                    var tdRow = HtmlParser.GetResultFromParser(olderItem, "//td");
                    if (tdRow.Count < 10)
                    {
                        continue;
                    }
                    detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == tdRow[1] && o.PaymentType == "缴费基数调整补收");
                    if (detailRes == null)
                    {
                        isSave = true;
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.SocialInsuranceTime = tdRow[1];
                        detailRes.SocialInsuranceBase = tdRow[3].ToDecimal(0);
                        detailRes.PensionAmount = tdRow[6].ToDecimal(0);
                        detailRes.CompanyPensionAmount = tdRow[7].ToDecimal(0);
                        detailRes.CompanyName = tdRow[9];

                    }
                    switch (tdRow[2].Trim())
                    {
                        case "正常应缴":
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            detailRes.PaymentType = tdRow[8].ToTrim() == "已缴"
                                ? ServiceConsts.SocialSecurity_PaymentType_Normal
                                : tdRow[8].ToTrim();
                            break;
                        case "缴费基数调整补收":
                            detailRes.SocialInsuranceBase += tdRow[3].ToDecimal(0);
                            detailRes.PensionAmount += tdRow[6].ToDecimal(0);
                            detailRes.CompanyPensionAmount += tdRow[7].ToDecimal(0);
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                            detailRes.PaymentType = tdRow[2].ToTrim();

                            break;
                        default:
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                            detailRes.PaymentType = tdRow[2].ToTrim();
                            break;
                    }
                    if (isSave)
                    {
                        Res.Details.Add(detailRes);
                    }
                }
                #endregion
                #region 第四步，医疗（该网站服务器响应时间过长,该项暂时省去）

                //Url = baseUrl + "hrss.asp?id=c2";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    results = HtmlParser.GetResultFromParser(httpResult.Html, "//font");
                //    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                //    Res.StatusDescription = results.Count > 1 ? results[2] : httpResult.Html;
                //    return Res;
                //}
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//td/table/tr[position()>1]");
                //foreach (string docItem in results)
                //{
                //    bool isSave = false;
                //    var tdRow = HtmlParser.GetResultFromParser(docItem, "//td");
                //    if (tdRow.Count < 13)
                //    {
                //        continue;
                //    }
                //    detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == tdRow[2]);
                //    if (detailRes == null)
                //    {
                //        isSave = true;
                //        detailRes = new SocialSecurityDetailQueryRes();
                //        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                //        detailRes.PaymentType = tdRow[1].ToTrim();
                //        detailRes.SocialInsuranceTime = tdRow[2];
                //        detailRes.SocialInsuranceBase = tdRow[4].ToDecimal(0);
                //        detailRes.MedicalAmount = tdRow[9].ToDecimal(0);
                //        detailRes.CompanyMedicalAmount = tdRow[8].ToDecimal(0);
                //        detailRes.EnterAccountMedicalAmount = tdRow[7].ToDecimal(0);
                //        detailRes.CompanyName = tdRow[12];
                //        Res.Details.Add(detailRes);

                //    }
                //    else
                //    {
                //        detailRes.MedicalAmount += tdRow[9].ToDecimal(0);
                //        detailRes.CompanyMedicalAmount += tdRow[8].ToDecimal(0);
                //        detailRes.EnterAccountMedicalAmount += tdRow[10].ToDecimal(0);
                //    }
                //    if (isSave)
                //    {
                //        Res.Details.Add(detailRes);
                //    }
                //}
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
