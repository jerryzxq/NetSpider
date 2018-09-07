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
using System.Text.RegularExpressions;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.SN
{
    public class xian : ISocialSecurityCrawler
    {

        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://shbxcx.sn12333.gov.cn/";
        public string socialCity = "sn_xian";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "sxlssLogin.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.IdentitycardOrNameEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录

                Url = baseUrl + "sxlssLogin.do";
                postdata = String.Format("uname={0}&aac003={1}&PSINPUT=jcte&Icon2.x=38&Icon2.y=15", socialReq.Identitycard, socialReq.Name);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://shbxcx.sn12333.gov.cn/",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK && httpResult.Html != "1")
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取基本信息
                Url = baseUrl + "jsp/personInfoQuery.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://shbxcx.sn12333.gov.cn/sxlssLogin.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tabletitlestyle01']/tr", "inner");
                if (results.Count <= 1)
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                var name = HtmlParser.GetResultFromParser(results[1].ToString(), "//td[2]", "inner");
                if (name.Count > 0)
                {
                    Res.Name = name[0].ToString();
                }


                Res.IdentityCard = socialReq.Identitycard;//身份证号
                //Res.CompanyName = results[7].Trim().Replace("&nbsp;", "");//单位名称
                var workdate = HtmlParser.GetResultFromParser(results[2].ToString(), "//td[2]", "inner");
                if (workdate.Count > 0)
                {
                    Res.WorkDate = workdate[0].ToString().Replace("&nbsp;", "");//参加工作时间

                }

                var EmployeeStatus = HtmlParser.GetResultFromParser(results[1].ToString(), "//td[4]", "inner");
                if (EmployeeStatus.Count > 0)
                {
                    Res.EmployeeStatus = EmployeeStatus[0].ToString().Replace("&nbsp;", "");//状态

                }
                var InsuranceTotal = HtmlParser.GetResultFromParser(results[2].ToString(), "//td[4]", "inner");
                if (InsuranceTotal.Count > 0)
                {
                    Res.PersonalInsuranceTotal = InsuranceTotal[0].ToString().Replace("&nbsp;", "").ToDecimal().Value;
                }
                #endregion

                #region 查询明细

                Url = baseUrl + "paymentQuery.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tabletitlestyle02']/tbody/tr", "inner");

                for (var i = 0; i < results.Count - 1; i++)
                {

                    detailRes = new SocialSecurityDetailQueryRes();
                    var PayTime = HtmlParser.GetResultFromParser(results[i].ToString(), "//td[8]", "inner");
                    if (PayTime.Count > 0)
                    {
                        var datepay = DateTime.Now;
                       //if (!DateTime.TryParse(PayTime[0], out datepay)) continue;
                        detailRes.PayTime = PayTime[0];
                    }
                    var SocialInsuranceTime = HtmlParser.GetResultFromParser(results[i].ToString(), "//td[1]", "inner");
                    if (SocialInsuranceTime.Count > 0)
                    {
                        detailRes.SocialInsuranceTime = SocialInsuranceTime[0];
                    }
                    var SocialInsuranceBase = HtmlParser.GetResultFromParser(results[i].ToString(), "//td[3]", "inner");
                    if (SocialInsuranceBase.Count > 0)
                    {
                        detailRes.SocialInsuranceBase = SocialInsuranceBase[0].ToString().ToDecimal().Value;
                    }
                    var PensionAmount = HtmlParser.GetResultFromParser(results[i].ToString(), "//td[7]", "inner");
                    if (PensionAmount.Count > 0)
                    {
                        detailRes.PensionAmount = PensionAmount[0].ToString().ToDecimal().Value;
                    }
                    var companybase = HtmlParser.GetResultFromParser(results[i].ToString(), "//td[5]", "inner");
                    if (companybase.Count > 0)
                    {
                        var cbase = Convert.ToDecimal(Regex.Matches(companybase[0].ToString(), @"\d+(.)?.*\d")[0].ToString());
                        detailRes.CompanyPensionAmount = detailRes.SocialInsuranceBase * (cbase / 100);
                    }


                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    var PaymentFlag = HtmlParser.GetResultFromParser(results[i].ToString(), "//td[2]", "inner");
                    if (PaymentFlag.Count > 0)
                    {
                        detailRes.PaymentFlag = PaymentFlag[0] != "正常应缴" ? ServiceConsts.SocialSecurity_PaymentFlag_Adjust : ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    }
                    Res.Details.Add(detailRes);

                }
                #endregion

                Url = baseUrl + "jsp/personInfoQuery.jsp";
                httpItem = new HttpItem()
                {
                    URL = "http://shbxcx.sn12333.gov.cn/personAccountQuery.do",
                    Referer = "http://shbxcx.sn12333.gov.cn/template/sxlssleft.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tabletitlestyle02']/tbody/tr", "inner");
                if (results.Count >= 2)
                {
                    var paymentmonths = HtmlParser.GetResultFromParser(results[0].ToString(), "//td[6]", "inner");
                    var paymentTady = HtmlParser.GetResultFromParser(results[0].ToString(), "//td[2]", "inner");
                    if (paymentmonths.Count > 0 && paymentTady.Count > 0)
                    {
                        PaymentMonths = paymentmonths[0].ToString().ToInt().Value + paymentTady[0].ToInt().Value;
                    }

                }
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
