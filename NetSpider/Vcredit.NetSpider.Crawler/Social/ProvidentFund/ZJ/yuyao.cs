using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.ZJ
{
    public class yuyao : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://183.136.195.150:7001/wscx/";
        string fundCity = "zj_yuyao";
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Res.StatusDescription = fundCity + "无需初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return Res;
        }

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            //ProvidentFundLoanRes ResLoad = new ProvidentFundLoanRes();
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            ProvidentFundDetail detail = null;
            try
            {
                //校验参数
                if (string.IsNullOrEmpty(fundReq.Password) || string.IsNullOrEmpty(fundReq.Identitycard))
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆

                Url = baseUrl + "zfbzgl/zfbzsq/login_hidden.jsp";
                postdata = string.Format("password={0}&sfzh={1}", fundReq.Password, fundReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    Encoding = Encoding.GetEncoding("GBK"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = httpResult.StatusDescription;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string errorInfo = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\");");
                if (!errorInfo.IsEmpty())
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgzh']", "value");
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];
                }
                if (string.IsNullOrEmpty(Res.EmployeeNo))
                {
                    Res.StatusDescription = "登陆失败请核对账号后重试";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.CompanyNo = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dwbm']", "value")[0];
                Res.Name = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgxm']", "value")[0];
                Res.IdentityCard = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='sfzh']", "value")[0];
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region 第二步,公积金基本信息

                Url = baseUrl + "zfbzgl/zfbzsq/main_menu.jsp";
                postdata = string.Format("zgzh={0}&sfzh={1}&zgxm={2}&dwbm={3}&cxyd={5}&pass={4}", Res.EmployeeNo, Res.IdentityCard, Res.Name.ToUrlEncode(Encoding.GetEncoding("GBK")), Res.CompanyNo, fundReq.Password, "%B5%B1%C7%B0%C4%EA%B6%C8");//
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    Encoding = Encoding.GetEncoding("GBK"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", ""), "//table[@class='1']/tr/td/font", "", true);
                if (results.Count == 16)
                {
                    Res.Status = results[2];
                    Res.CompanyName = results[4];
                    Res.SalaryBase = results[6].ToDecimal(0);
                    Res.PersonalMonthPayRate = Res.CompanyMonthPayRate = (results[7].ToDecimal(0) / Res.SalaryBase) / 2;
                    Res.PersonalMonthPayAmount = Res.CompanyMonthPayAmount = results[7].ToDecimal(0) / 2;
                    Res.TotalAmount = results[14].ToDecimal(0);
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region 第三步,公积金明细

                results = new List<string>();
                int yss = 1, totalPages = 1;
                decimal perAccounting = (Res.PersonalMonthPayRate > 0 & Res.CompanyMonthPayRate > 0)
                    ? Res.PersonalMonthPayRate / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)
                    : 0.5M;//个人月缴费占比
                Url = baseUrl + "zfbzgl/gjjmxcx/gjjmx_cx.jsp";
                do
                {
                    postdata = string.Format("sfzh={0}&zgxm={1}2&zgzh={2}&dwbm={3}&cxyd={6}&totalpages={4}=&yss={5}", Res.IdentityCard, Res.Name.ToUrlEncode(Encoding.GetEncoding("GBK")), Res.EmployeeNo, Res.CompanyNo, totalPages, yss, "%B5%B1%C7%B0%C4%EA%B6%C8");
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "Post",
                        Postdata = postdata,
                        Encoding = Encoding.GetEncoding("GBK"),
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (yss == 1)
                    {
                        var pages = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='totalpages']", "value");
                        if (pages.Count > 0)
                        {
                            totalPages = pages[0].ToInt(0);
                        }
                    }
                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[position()>1]", "inner", true));
                    yss++;
                } while (yss <= totalPages);
                Regex regTime = new Regex(@"[0-9]+");
                foreach (var item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text");
                    if (tdRow.Count < 6) continue;
                    detail = new ProvidentFundDetail();
                    detail.PayTime = tdRow[0].ToDateTime();//缴费年月
                    detail.Description = tdRow[1];
                    if (tdRow[2].ToDecimal(0) > 0)
                    {
                        detail.PersonalPayAmount = tdRow[2].ToDecimal(0);
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                    }
                    else if (tdRow[1].IndexOf("汇缴个人单位", StringComparison.Ordinal) > -1)
                    {
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0) * perAccounting;//个人月缴额
                        detail.CompanyPayAmount = tdRow[3].ToDecimal(0) * (1 - perAccounting);//公司月缴额
                        detail.ProvidentFundBase = detail.PersonalPayAmount / (Res.PersonalMonthPayRate > 0 ? Res.PersonalMonthPayRate : payRate);
                        detail.ProvidentFundTime = regTime.Match(tdRow[1].ToTrim("年")).Value;//应属年月
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;

                    }
                    else if (tdRow[1].IndexOf("汇缴单位", StringComparison.Ordinal) > -1)
                    {
                        detail.CompanyPayAmount = tdRow[2].ToDecimal(0) + tdRow[3].ToDecimal(0);//公司缴费
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    }
                    else
                    {
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0);//个人缴费
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion
                // Res.ProvidentFundLoanRes = ResLoad;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
    }
}
