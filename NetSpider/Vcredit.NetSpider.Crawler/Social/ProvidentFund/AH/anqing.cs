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
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.Common.Constants;
using System.Text.RegularExpressions;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.AH
{
    public class anqing : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://121.40.42.25:7777/";
        string fundCity = "ah_anqing";
        #endregion
        /// <summary>
        /// 解析验证码
        /// </summary>
        /// <returns></returns>
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = "所选城市无需初始化";
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return Res;
        }

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            ProvidentFundDetail detail = null;
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Username.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "登录名或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                string errorMsg = string.Empty;

                #region 第一步，登录

                Url = baseUrl + "controller.aspx?action=login";
                postdata = String.Format("loginName={0}&loginPwd={1}&button=+", fundReq.Username.ToUrlEncode(Encoding.GetEncoding("utf-8")), fundReq.Password);
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
                #endregion

                errorMsg = CommonFun.GetMidStr(httpResult.Html, "<div style=\"color:red;text-align:center\">", "</div>");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第二步，保存基本信息


                //保存详细内容
                //Res.ProvidentFundNo = fundReq.Username;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[1]/td[2]/table[1]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[2]", "inner");
                if (results.Count > 0)
                {
                    Res.Name = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "table[2]/tr[1]/td[2]/table[1]/tr[2]/td[1]/table/tr/td/table/tr[2]/td[2]", "inner");
                if (results.Count > 0)
                {
                    Res.CompanyName = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "table[2]/tr[1]/td[2]/table[1]/tr[2]/td[1]/table/tr/td/table/tr[3]/td[2]", "inner");
                if (results.Count > 0)
                {
                    Res.CompanyNo = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "table[2]/tr[1]/td[2]/table[1]/tr[2]/td[1]/table/tr/td/table/tr[3]/td[4]", "inner");
                if (results.Count > 0)
                {
                    Res.EmployeeNo = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }


                results = HtmlParser.GetResultFromParser(httpResult.Html, "table[2]/tr[1]/td[2]/table[1]/tr[2]/td[1]/table/tr/td/table/tr[4]/td[4]", "inner");
                if (results.Count > 0)
                {
                    Res.BankCardNo = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "table[2]/tr[1]/td[2]/table[1]/tr[2]/td[1]/table/tr/td/table/tr[5]/td[2]", "inner");
                if (results.Count > 0)
                {
                    Res.Bank = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "table[2]/tr[1]/td[2]/table[1]/tr[2]/td[1]/table/tr/td/table/tr[6]/td[4]", "inner");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = decimal.Parse(CommonFun.GetMidStr(Regex.Replace(results[0], @"[/\&nbsp;\s]", ""), "", "%")) / 100 / 2;
                    Res.CompanyMonthPayRate = Res.PersonalMonthPayRate;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "table[2]/tr[1]/td[2]/table[1]/tr[2]/td[1]/table/tr/td/table/tr[6]/td[2]", "inner");
                if (results.Count > 0)
                {
                    Res.Status = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "table[2]/tr[1]/td[2]/table[1]/tr[2]/td[1]/table/tr/td/table/tr[5]/td[4]", "inner");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "table[2]/tr[1]/td[2]/table[1]/tr[2]/td[1]/table/tr/td/table/tr[7]/td[2]", "inner");
                if (results.Count > 0)
                {
                    //Math.Round( d,2 ).ToString();
                    //Res.PersonalMonthPayAmount = (Regex.Replace(results[0], @"[/\&nbsp;\s]", "").ToDecimal(0) / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)) * Res.PersonalMonthPayRate;
                    decimal personalpay = (Regex.Replace(results[0], @"[/\&nbsp;\s]", "").ToDecimal(0) /
                                           (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)) *
                                          Res.PersonalMonthPayRate;
                    Res.PersonalMonthPayAmount = Math.Round(personalpay, 2);
                    //Res.CompanyMonthPayAmount = (Regex.Replace(results[0], @"[/\&nbsp;\s]", "").ToDecimal(0) / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)) * Res.CompanyMonthPayRate;
                    decimal companypay = (Regex.Replace(results[0], @"[/\&nbsp;\s]", "").ToDecimal(0) /
                                          (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)) * Res.CompanyMonthPayRate;
                    Res.CompanyMonthPayAmount = Math.Round(companypay, 2);
                    Res.SalaryBase = Math.Round(Res.PersonalMonthPayAmount / Res.PersonalMonthPayRate, 2);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "table[2]/tr[1]/td[2]/table[1]/tr[2]/td[1]/table/tr/td/table/tr[7]/td[4]", "inner");
                if (results.Count > 0)
                {
                    Res.TotalAmount = Regex.Replace(results[0], @"[/\&nbsp;\s]", "").ToDecimal(0);
                }
                #endregion

                #region 第三步，获取明细
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                int i = 1;
                while (true)
                {
                    Url = baseUrl + "details.aspx?page=" + i;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
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

                    string count = CommonFun.GetMidStr(httpResult.Html, "共", "页");
                    int PageCount = int.Parse(count);
                    if (i > PageCount) break;
                    i++;
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[1]/td[2]/table/tr[2]/td[1]/table/tr/td/table/tr[position()>1]", "inner");
                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                        if (tdRow.Count != 6)
                        {
                            continue;
                        }
                        detail = new ProvidentFundDetail();

                        detail.PayTime = tdRow[2].ToDateTime();

                        if (tdRow[1].IndexOf("汇缴") != -1)
                        {

                            string time = CommonFun.GetMidStr(tdRow[2], "-", "-");
                            detail.ProvidentFundTime = time.ToInt() < 10 ? tdRow[2].Replace("-", "").Substring(0, 4) + "0" + tdRow[2].Replace("-", "").Substring(4, 1) : tdRow[2].Replace("-", "").Substring(0, 6);
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            detail.PersonalPayAmount = (decimal.Parse(Regex.Replace(tdRow[4], @"[/\&nbsp;\s]", "")) / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)) * Res.PersonalMonthPayRate;//金额
                            detail.CompanyPayAmount = (decimal.Parse(Regex.Replace(tdRow[4], @"[/\&nbsp;\s]", "")) / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)) * Res.CompanyMonthPayRate;//金额
                            detail.ProvidentFundBase = Math.Round(detail.PersonalPayAmount / Res.PersonalMonthPayRate, 2);//缴费基数
                            PaymentMonths++;
                        }
                        else if (tdRow[1].IndexOf("结息") != -1 || tdRow[1].IndexOf("转结") != -1)
                        {
                            string time = CommonFun.GetMidStr(tdRow[2], "-", "-");
                            detail.ProvidentFundTime = time.ToInt() < 10 ? tdRow[2].Replace("-", "").Substring(0, 4) + "0" + tdRow[2].Replace("-", "").Substring(4, 1) : tdRow[2].Replace("-", "").Substring(0, 6);
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.PersonalPayAmount = decimal.Parse(Regex.Replace(tdRow[4], @"[/\&nbsp;\s]", ""));//金额
                            detail.Description = tdRow[1];
                        }
                        else if (tdRow[1].IndexOf("补缴") != -1)
                        {
                            string time = CommonFun.GetMidStr(tdRow[2], "-", "-");
                            detail.ProvidentFundTime = time.ToInt() < 10 ? tdRow[2].Replace("-", "").Substring(0, 4) + "0" + tdRow[2].Replace("-", "").Substring(4, 1) : tdRow[2].Replace("-", "").Substring(0, 6);
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.CompanyPayAmount = decimal.Parse(Regex.Replace(tdRow[4], @"[/\&nbsp;\s]", ""));//金额
                            detail.Description = tdRow[1];
                        }
                        else
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.Description = tdRow[1];
                        }
                        Res.ProvidentFundDetailList.Add(detail);
                    }

                }
                Res.PaymentMonths = PaymentMonths;
                #endregion
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
