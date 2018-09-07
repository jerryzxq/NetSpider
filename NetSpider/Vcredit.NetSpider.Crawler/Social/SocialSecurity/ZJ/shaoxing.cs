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
using System.Collections;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.ZJ
{
    public class shaoxing : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.zjsx.si.gov.cn/";
        string socialCity = "zj_shaoxing";
        #endregion
        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                httpItem = new HttpItem()
                {
                    URL = baseUrl,
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
            Res.SocialSecurityCity = socialCity;
            string Url = string.Empty;
            string postdata = string.Empty;
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 登录

                Url = baseUrl + "sxlss/wsbs/queryjiaofeiYZ.jsp";
                postdata = String.Format("utype=1&username={1}&aac003={0}&aac001=", socialReq.Name, socialReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
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

                if (httpResult.Html.Contains("alert"))
                {
                    string alert = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\");");
                    Res.StatusDescription = alert;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion
                #region 获取基本信息
                Url = baseUrl + "sxlss/wsbs/grjbxx.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl + "sxlss/wsbs/queryjiaofeiYZ.jsp",
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//tr[@class='table_tr_03']/td");
                Res.EmployeeNo = results[1].Trim();//员工编号
                Res.IdentityCard = results[3].Trim();//身份证
                Res.Name = results[5].Trim();//姓名
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//tr[@class='table_tr_01']");
                foreach (string item in results)
                {
                    if (item.Contains("职工基本养老保险"))
                    {
                        Res.SocialInsuranceBase = HtmlParser.GetResultFromParser(item, "/td")[1].ToDecimal(0);
                        break;
                    }
                }
                List<string> joinType = new List<string>();//参加险种信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//tr[@class='table_tr_01']/td");
                if (results.Count > 2)
                {
                    for (int i = 0; i <= results.Count - 3; i = i + 3)
                    {
                        Res.SpecialPaymentType += results[i] + ":" + results[i + 2] + ";";//特殊缴费类型
                        switch (results[i])
                        {
                            case "职工基本养老保险":
                            case "基本医疗保险":
                            case "失业保险":
                            case "工伤保险":
                            case "生育保险":
                            case "大病救助医疗保险":
                            case "公务员补助医疗保险":
                                joinType.Add(results[i]);
                                break;
                        }
                    }
                }
                #endregion
                #region 获取详细信息

                int pageIndex = 1;
                int pageTotal = 1;
                results = new List<string>();
                do
                {
                    Url = baseUrl + string.Format("sxlss/wsbs/grjiaofei.jsp?spage={0}&aac001={1}&AAE002=&AAE140=&AAE111=", pageIndex, Res.EmployeeNo);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (pageIndex == 1)
                    {
                        pageTotal = CommonFun.GetMidStr(httpResult.Html, "</font>/", "页").ToInt(0);
                    }
                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//tr[@class='table_tr_01']"));
                    pageIndex++;
                } while (pageIndex <= pageTotal);
                foreach (string s in results)
                {
                    List<string> tdRow = HtmlParser.GetResultFromParser(s, "/td");
                    if (tdRow.Count < 6) continue;
                    if (!joinType.Contains(tdRow[1])) continue;
                    SocialSecurityDetailQueryRes detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == tdRow[0]);
                    bool isSave = false;
                    if (detailRes == null)
                    {
                        isSave = true;
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.IdentityCard = Res.IdentityCard;
                        detailRes.Name = Res.Name;
                        detailRes.PayTime = detailRes.SocialInsuranceTime = tdRow[0];
                        detailRes.PaymentFlag = tdRow[5] != "未到帐" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                        detailRes.PaymentType = tdRow[5];
                    }
                    switch (tdRow[1])
                    {
                        case "职工基本养老保险":
                            detailRes.PensionAmount += tdRow[4].ToDecimal(0);
                            detailRes.CompanyPensionAmount += tdRow[3].ToDecimal(0);
                            detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                            break;
                        case "基本医疗保险":
                            detailRes.CompanyMedicalAmount += tdRow[3].ToDecimal(0);
                            detailRes.MedicalAmount += tdRow[4].ToDecimal(0);
                            break;
                        case "失业保险":
                            detailRes.UnemployAmount += (tdRow[3].ToDecimal(0) + tdRow[4].ToDecimal(0));
                            break;
                        case "工伤保险":
                            detailRes.EmploymentInjuryAmount += tdRow[3].ToDecimal(0);
                            break;
                        case "生育保险":
                            detailRes.MaternityAmount += tdRow[3].ToDecimal(0);
                            break;
                        case "大病救助医疗保险":
                            detailRes.IllnessMedicalAmount += tdRow[3].ToDecimal(0);
                            break;
                        case "公务员补助医疗保险":
                            detailRes.CivilServantMedicalAmount += tdRow[3].ToDecimal(0);
                            break;
                    }
                    detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : tdRow[2].ToDecimal(0);
                    if (!isSave) continue;
                    Res.Details.Add(detailRes);
                }
                #endregion
                Res.StatusDescription = ServiceConsts.ProvidentFund_QuerySuccess;
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
