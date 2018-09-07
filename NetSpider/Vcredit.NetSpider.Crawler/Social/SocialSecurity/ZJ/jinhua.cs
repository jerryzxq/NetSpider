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
    public class jinhua : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.jhlss.gov.cn/wx/operation/";
        string socialCity = "zj_jinhua";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "login.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "logout.jsp",
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录

                Url = baseUrl + "UserLogin";
                postdata = String.Format("errors=&type=2&ucardcode={0}&uname={1}&password={2}&num_in={3}&num={3}&Submit=%B5%C7+++%C2%BD", socialReq.Identitycard, socialReq.Name.ToUrlEncode(Encoding.GetEncoding("gbk")), socialReq.Password, "9279");
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
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='errors']", "value").Count > 0)
                {
                    string message = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='errors']", "value")[0];
                    if (!message.IsEmpty())
                    {
                        Res.StatusDescription = message;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取基本信息

                Url = baseUrl + "person/personBase.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "UserLogin",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_style02']/tr/td", "inner");
                if (results.Count <= 0 || string.IsNullOrEmpty(results[2]))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                //Res.EmployeeNo = results[2].Trim().Replace("&nbsp;", "");//编号
                Res.Name = results[4].Trim().Replace("&nbsp;", "");//姓名
                Res.BirthDate = DateTime.Parse(results[10].Trim().Replace("&nbsp;", "")).ToString("yyyy-MM-dd");//出生日期
                Res.Sex = results[6].Trim().Replace("&nbsp;", "");//性别
                Res.IdentityCard = results[2].Trim().Replace("&nbsp;", "");//身份证号
                Res.Race = results[8].Replace("&nbsp;", "");//民族
                Res.CompanyName = results[18].Trim().Replace("&nbsp;", "");//单位名称
                Res.EmployeeStatus = results[12].Trim().Replace("&nbsp;", "");//人员状态
                Res.Phone = results[16].Trim().Replace("&nbsp;", "");//联系电话
                Res.Address = results[14].Trim().Replace("&nbsp;", "");//通讯地址
                results = HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", ""), "//table[@class='table_style02'][2]//tr[position()>1]/td", "");
                for (int i = 0; i <= results.Count - 3; i = i + 3)
                {
                    Res.SpecialPaymentType += results[i] + ":" + results[i + 2] + ";";
                    if (results[i].IndexOf("养老保险") > -1)
                    {
                        Res.SocialInsuranceBase = results[i + 1].ToDecimal(0);
                    }
                }
                #endregion

                #region 查询明细

                results = new List<string>();
                Url = baseUrl + "person/personpay.jsp";
                int spage = 1;
                int pageCount = 1;
                do
                {
                    postdata = string.Format("spage={0}", spage);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Host = "www.jhlss.gov.cn",
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
                            detailRes.PensionAmount = tdRow[4].ToDecimal(0) * 8 / 22;
                            detailRes.CompanyPensionAmount = tdRow[4].ToDecimal(0) * 14 / 22;
                            detailRes.SocialInsuranceBase = tdRow[3].ToDecimal(0);
                            break;
                        case "医疗保险":
                        case "基本医疗保险":
                            //detailRes.MedicalAmount += tdRow[8].ToDecimal(0);
                            detailRes.CompanyMedicalAmount += tdRow[4].ToDecimal(0);
                            detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : tdRow[3].ToDecimal(0);
                            break;
                        case "失业保险":
                            detailRes.UnemployAmount += tdRow[4].ToDecimal(0) ;
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
