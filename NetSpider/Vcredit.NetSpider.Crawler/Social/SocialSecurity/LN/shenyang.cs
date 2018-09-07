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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.LN
{
    public class shenyang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://218.25.120.50/";
        string socialCity = "ln_shenyang";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            string Url = string.Empty;
            Res.Token = token;
            try
            {
                Url = baseUrl + "grjfcx.jsp";
                httpItem = new HttpItem()
                {
                    URL = baseUrl,
                    Method = "get",
                    Referer = baseUrl,
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Username.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录
                Url = baseUrl + "grjfinputjudgeservlet";
                postdata = String.Format("xm={0}&sfz={1}", socialReq.Username, socialReq.Identitycard);
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取基本信息

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//th[@scope='col']/div[1]/span", "inner");
                if (results.Count <= 0)
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.CompanyNo = results[0].Split(':')[1].Split('>')[1].Replace("<strong", "");   //单位编码
                Res.CompanyName = results[0].Split(':')[2].Replace("</strong>", "");   //单位名称
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//th[@scope='col']/p[1]", "inner");
                if (results.Count <= 0)
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.EmployeeNo = results[0].Split(':')[1].Split('>')[1].Replace("<strong", "");  //职工编号
                Res.Name = results[0].Split(':')[2].Split('>')[1].Replace("</strong", "");        //职工姓名
                Res.BirthDate = results[0].Split(':')[3].Split('>')[1].Replace("<strong", "");  //出生日期
                Res.IdentityCard = socialReq.Identitycard;
                #endregion

                #region 查询明细
                //历年缴费
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//th[@scope='col']/table[1]/tr[position()>1]", "inner");
                //今年缴费明细
                results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//th[@scope='col']/table[2]/tr[position()>1]", "inner"));
                //最近2年缴费月数
                Dictionary<int, int> disc = new Dictionary<int, int>();
                DateTime dtNow = DateTime.Now;
                disc.Add(dtNow.Year, 0);
                disc.Add(dtNow.Year - 1, 0);
                disc.Add(dtNow.Year - 2, 0);
                foreach (var item in results)
                {
                    SocialSecurityDetailQueryRes detail = new SocialSecurityDetailQueryRes();
                    List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    switch (@tdRow.Count)
                    {
                        case 4://今年缴费明细（按月份列表）
                            detail.Name = tdRow[1].Split('&')[0];
                            detail.PayTime = tdRow[2].Split('&')[0];
                            detail.SocialInsuranceTime = tdRow[2].Split('&')[0];
                            detail.PensionAmount = tdRow[3].Split('&')[0].ToDecimal(0);
                            detail.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detail.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            break;
                        case 7://历年缴费明细(按整年份列表)
                            //当年
                            if (tdRow[2] == dtNow.Year.ToString())
                            {
                                disc[dtNow.Year] = tdRow[6].Split('&')[0].ToInt(0);
                            }
                            //去年
                            if (tdRow[2] == (dtNow.Year - 1).ToString())
                            {
                                disc[dtNow.Year - 1] = tdRow[6].Split('&')[0].ToInt(0);
                            }
                            //前年
                            if (tdRow[2] == (dtNow.Year - 2).ToString())
                            {
                                disc[dtNow.Year - 2] = tdRow[6].Split('&')[0].ToInt(0);
                            }
                            detail.Name = tdRow[1].Split('&')[0];
                            detail.PayTime = tdRow[2].Split('&')[0];
                            //detail.SocialInsuranceTime = tdRow[2].Split('&')[0];
                            detail.SocialInsuranceBase = tdRow[3].Split('&')[0].ToDecimal(0);
                            detail.PensionAmount = tdRow[4].Split('&')[0].ToDecimal(0);
                            if (disc[dtNow.Year] == dtNow.Month || disc[dtNow.Year] == dtNow.Month - 1)
                            {
                                detail.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                                detail.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            }
                            else if (tdRow[6].Split('&')[0] == "12")
                            {
                                detail.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                                detail.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            }
                            else
                            {
                                detail.PaymentType = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                                detail.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                            }
                            break;
                    }
                    Res.Details.Add(detail);
                    //PaymentMonths += detail.YearPaymentMonths;
                }

                //连续缴费月数
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//th[@scope='col']/table[1]/tr[position()>1]", "inner");
                foreach (var item in results)
                {
                    List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    PaymentMonths += tdRow[6].ToInt(0);
                }
                Res.SocialInsuranceBase = HtmlParser.GetResultFromParser(results[results.Count - 1], "//td", "text", true)[3].ToDecimal(0);
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                #region MyRegion

                //计算最近12个月的缴费情况
                string SocialInsuranceTime = string.Empty;
                string Payment_State = string.Empty;
                int PaymentMonths_Continuous = 0;
                for (int i = 0; i <= dtNow.Month - 1; i++)
                {
                    SocialInsuranceTime = DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString7);

                    var query = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime && o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal).FirstOrDefault();
                    if (query != null)
                    {
                        Payment_State += "/";//缴费
                    }
                    else
                    {
                        Payment_State += "N";//未缴费
                    }
                }
                //今年有缴费,但没有列列明细表
                if (!Payment_State.Contains("/") && disc[dtNow.Year] > 0)
                {
                    //当年每月都正常缴费
                    if (disc[dtNow.Year] == dtNow.Month)
                    {
                        Payment_State = Payment_State.Replace("N", "/");
                    }
                    else if (disc[dtNow.Year] == dtNow.Month - 1)
                    {
                        Payment_State = "N" + Payment_State.Substring(1).Replace("N", "/");
                    }
                    //不确定哪些月未交，标记全部以"*",代替    
                    else
                    {
                        Payment_State = Payment_State.Replace("N", "*");
                        PaymentMonths_Continuous = disc[dtNow.Year];//今年不确定具体月份情况下缴费月数
                    }
                }
                //今年有列明细列表或今年目前每月正常缴费,但没有列列明细表
                if (Payment_State.Contains("/"))
                {
                    //最近12个月连续缴费情况
                    var Continuous = Payment_State.Split('N');
                    foreach (string item in Continuous)
                    {
                        if (!item.IsEmpty())
                        {
                            PaymentMonths_Continuous = item.Split('/').Count() - 1;//今年确定具体月份情况下缴费月数
                            break;
                        }
                    }
                }
                //去年每月正常缴费状态标记
                if (disc[dtNow.Year - 1] == 12)
                {
                    Payment_State = Payment_State + "////////////";
                }
                else
                {
                    Payment_State = Payment_State + "************";
                }
                //前年每月正常缴费状态标记
                if (disc[dtNow.Year - 2] == 12)
                {
                    Payment_State = Payment_State + "////////////";
                }
                else
                {
                    Payment_State = Payment_State + "************";
                }
                //最近36个月总缴费月数
                PaymentMonths_Continuous = PaymentMonths_Continuous + disc[dtNow.Year - 1] + disc[dtNow.Year - 2];

                Res.Payment_State = Payment_State;
                Res.PaymentMonths_Continuous = PaymentMonths_Continuous;
                #endregion
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
