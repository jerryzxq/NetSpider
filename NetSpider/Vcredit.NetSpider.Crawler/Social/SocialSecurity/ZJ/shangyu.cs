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
    public class shangyu : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://122.224.43.26/synet/";
        string socialCity = "zj_shangyu";
        #endregion
        #region 私有变量

        /// <summary>
        /// 获取缴费明细
        /// </summary>
        /// <param name="psseno"></param>
        /// <param name="Res"></param>
        private void GetAllDetail(string psseno, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            List<string> results = new List<string>();
            int totalPage = 1;
            int currentPage = 1;
            do
            {
                Url = baseUrl + string.Format("person/person_cost.jsp?psseno={0}&newsPageNo={1}&year=&safe=", psseno, currentPage);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (currentPage == 1)
                {
                    totalPage =CommonFun.GetMidStr(httpResult.Html,"共","页").ToInt(0);
                }
                results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='90%']/tr[position()>1]", ""));
                currentPage++;
            } while (currentPage <= totalPage);
            foreach (string item in results)
            {
                var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                if (tdRow.Count < 7) continue;
                DateTime begindate = DateTime.ParseExact(tdRow[1], "yyyyMM", null);
                string insuranceTime = begindate.ToString(Consts.DateFormatString7);
                SocialSecurityDetailQueryRes detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == insuranceTime);
                bool isSave = false;
                if (detailRes == null)
                {
                    isSave = true;
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.IdentityCard =Res.IdentityCard;
                    detailRes.Name = Res.Name;
                    detailRes.CompanyName = tdRow[0];
                    detailRes.PayTime = detailRes.SocialInsuranceTime = insuranceTime;
                    detailRes.PaymentFlag = tdRow[6] == "足额到帐" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                    detailRes.PaymentType = tdRow[6];
                }
                switch (tdRow[2])
                {
                    case "企业职工基本养老保险":
                        detailRes.PensionAmount += tdRow[5].ToDecimal(0);
                        detailRes.CompanyPensionAmount += tdRow[4].ToDecimal(0);
                        detailRes.SocialInsuranceBase = tdRow[3].ToDecimal(0);
                        break;
                    case "基本医疗保险":
                        detailRes.CompanyMedicalAmount += tdRow[4].ToDecimal(0);
                        detailRes.MedicalAmount += tdRow[5].ToDecimal(0);
                        break;
                    case "失业保险":
                        detailRes.UnemployAmount += (tdRow[4].ToDecimal(0) + tdRow[5].ToDecimal(0));
                        break;
                    case "工伤保险":
                        detailRes.EmploymentInjuryAmount += tdRow[4].ToDecimal(0);
                        break;
                    case "生育保险":
                        detailRes.MaternityAmount += tdRow[4].ToDecimal(0);
                        break;
                    default:
                        if (tdRow[2].IndexOf("病", StringComparison.Ordinal) > -1)
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
                Url = baseUrl + "login.jsp";
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
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || (socialReq.Username.IsEmpty() & socialReq.Citizencard.IsEmpty()))
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                Url = baseUrl + "person/person_chaxun.jsp";
                postdata = String.Format("iscode={0}&agcardno={1}&ylcardno={2}", socialReq.Identitycard, socialReq.Citizencard, socialReq.Username);
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion
                #region 第二步， 获取基本信息

                Res.Name = socialReq.Name;
                Res.IdentityCard = socialReq.Identitycard;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='90%']", "");
                List<string> baselist;
                if (results.Count > 0)
                {
                    baselist = HtmlParser.GetResultFromParser(results[0].Replace("&nbsp;", "").Replace("&nbsp", ""), "//tr/td[2]", "");
                    if (baselist.Count == 5)
                    {
                        Res.EmployeeNo = baselist[0];
                        Res.Name = baselist[1];
                        Res.Sex = baselist[2];
                        Res.Race = baselist[3];
                        Res.Phone = baselist[4];
                    }
                    baselist = HtmlParser.GetResultFromParser(results[0].Replace("&nbsp;", "").Replace("&nbsp", ""), "//tr/td[4]", "");
                    if (baselist.Count == 5)
                    {
                        Res.CompanyName = baselist[0];
                        //Res.IdentityCard = baselist[1];
                        Res.BirthDate = baselist[2];
                        Res.Address = baselist[4];
                    }
                }
                if (string.IsNullOrEmpty(Res.EmployeeNo))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (results.Count > 1)
                {
                    baselist = HtmlParser.GetResultFromParser(results[1].Replace("&nbsp;", "").Replace("&nbsp", ""), "//tr[position()>1]/td", "");
                    for (int i = 0; i < baselist.Count - 3; i = i + 3)
                    {
                        Res.SpecialPaymentType += baselist[i] + ":" + baselist[i + 1] + ";";
                    }
                }

                string psseno = CommonFun.GetMidStr(httpResult.Html, "psseno=", "\">");
                // 养老保险个人帐户信息
                Url = baseUrl + "person/person_yanglao.jsp?psseno=" + psseno;
                postdata = string.Format("aae002={0}&psseno={1}", DateTime.Now.Year, psseno);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table1']/tr[3]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.PaymentMonths = results[0].ToInt(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table1']/tr[17]/td[3]", "");
                if (results.Count > 0)
                {
                    Res.PersonalInsuranceTotal = results[0].ToDecimal(0);
                }
                if (Res.PersonalInsuranceTotal>0)
                {
                    //医疗保险个人帐户信息
                    Url = baseUrl + "person/person_yiliao.jsp?psseno=" + psseno;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@valign='top']/table[@width='90%']/tr[5]/td[2]", "");
                    if (results.Count > 0)
                    {
                        Res.InsuranceTotal = results[0].Replace("&nbsp;", "").Replace("&nbsp", "").ToDecimal(0) + Res.PersonalInsuranceTotal;
                    }
                }
                #endregion
                #region 第三步，查询明细

                GetAllDetail(psseno, ref Res);
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
