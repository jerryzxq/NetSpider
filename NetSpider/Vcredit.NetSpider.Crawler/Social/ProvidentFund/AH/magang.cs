using System;
using System.Collections.Generic;
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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.AH
{
    public class magang : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.masgjj.gov.cn:85/";
        string fundCity = "ah_magang";
        #endregion
        /// <summary>
        /// 解析验证码
        /// </summary>
        /// <returns></returns>
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            string viewstate = string.Empty;
            string eventvalidation = string.Empty;
            try
            {
                Url = baseUrl + "search.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Encoding = Encoding.GetEncoding("utf-8"),
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
                viewstate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                eventvalidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                Dictionary<string, object> dic = new Dictionary<string, object>
                {
                    {"viewstate",viewstate},
                    {"eventvalidation",eventvalidation},
                    {"cookies",cookies}
                };
                CacheHelper.SetCache(token, dic);
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
            Res.Token = fundReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            ProvidentFundDetail detail = null;
            int PaymentMonths = 0;
            string errorMsg = string.Empty;
            string viewstate = string.Empty;
            string eventvalidation = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    Dictionary<string, object> dic = (Dictionary<string, object>)CacheHelper.GetCache(fundReq.Token);
                    cookies = (CookieCollection)dic["cookies"];
                    viewstate = dic["viewstate"].ToString();
                    eventvalidation = dic["eventvalidation"].ToString();
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第二步
                Url = baseUrl + "search.aspx";
                postdata = string.Format("__EVENTTARGET=RadioButtonList1%241&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE={0}&__EVENTVALIDATION={1}&RadioButtonList1=2&TextBox_dwzh=&TextBox_grzh=", viewstate.ToUrlEncode(), eventvalidation.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Encoding = Encoding.GetEncoding("utf-8"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "<script>alert(", "');window.history.go");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                viewstate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                eventvalidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 第三步

                Url = baseUrl + "search.aspx";
                postdata = string.Format("__LASTFOCUS=&__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE={0}&__EVENTVALIDATION={1}&RadioButtonList1=2&TextBox_sfzh={2}&ImageButton1.x=37&ImageButton1.y=8", viewstate.ToUrlEncode(), eventvalidation.ToUrlEncode(), fundReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Encoding = Encoding.GetEncoding("utf-8"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string dwzh = CommonFun.GetMidStr(httpResult.Html, "?dwzh=", " &");

                string qrzh = CommonFun.GetMidStr(httpResult.Html, "&grzh=", "'</script>");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region  第四步
                // Url = baseUrl + string.Format("inputmima.aspx?dwzh={0}&grzh={1}", (dwzh + " ").ToUrlEncode(), qrzh);
                Url = baseUrl + CommonFun.GetMidStr(httpResult.Html, "parent.location.href='", "'</script>");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("utf-8"),
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
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                viewstate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                eventvalidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion


                #region 第五步
                Url = baseUrl + "inputmima.aspx?dwzh=" + dwzh + "&grzh=" + qrzh + "";
                postdata = string.Format("__VIEWSTATE={0}&__EVENTVALIDATION={1}&txtPass={2}&ImageButton_gjj.x=78&ImageButton_gjj.y=15", viewstate.ToUrlEncode(), eventvalidation.ToUrlEncode(), fundReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    Encoding = Encoding.GetEncoding("utf-8"),
                    Referer = "http://www.masgjj.gov.cn:83/inputmima.aspx?dwzh=1003110%20&grzh=00533",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[1]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[1]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[2]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[3]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.BankCardNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[2]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[4]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[5]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[7]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    if (CommonFun.GetMidStr(results[0], "", "%").ToDecimal(0) == 0)
                        Res.PersonalMonthPayRate = payRate;
                    else
                        Res.CompanyMonthPayRate = CommonFun.GetMidStr(results[0], "", "%").ToDecimal(0) / 100;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[7]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    if (CommonFun.GetMidStr(results[0], "", "%").ToDecimal(0) == 0)
                        Res.CompanyMonthPayRate = payRate;
                    else
                        Res.PersonalMonthPayRate = CommonFun.GetMidStr(results[0], "", "%").ToDecimal(0) / 100;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[4]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[6]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    decimal pay = results[0].ToDecimal(0);
                    Res.PersonalMonthPayAmount = pay / 2;
                    Res.CompanyMonthPayAmount = pay / 2;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[8]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.Bank = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[6]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.Status = results[0];
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并
                #endregion
                int j = 0;
                #region  第六步 获取第一页数据
                Url = baseUrl + string.Format("cx_jieguo.aspx?dwzh={0}&grzh={1}", dwzh, qrzh);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("utf-8"),
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
                viewstate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                eventvalidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];

                //请求失败后返回
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr[1]/td/table/tr/td[1]/table/tr/td/div/table/tr[position()>1]", "inner");
                foreach (string item in results)
                {
                    j++;
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                    if (tdRow.Count != 5)
                    {
                        continue;
                    }
                    detail = new ProvidentFundDetail();

                    detail.PayTime = tdRow[0].ToDateTime();
                    if (tdRow[1].IndexOf("汇缴") != -1)
                    {
                        detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[1], "", "[").Replace("-", "");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = decimal.Parse(Regex.Replace(tdRow[3], @"[/\&nbsp;\,\s]", "")) / 2;//金额
                        detail.CompanyPayAmount = decimal.Parse(Regex.Replace(tdRow[3], @"[/\&nbsp;\,\s]", "")) / 2;//金额
                        detail.ProvidentFundBase = (detail.PersonalPayAmount / Res.PersonalMonthPayRate);//缴费基数
                        PaymentMonths++;
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.Description = tdRow[1];
                        detail.PersonalPayAmount = decimal.Parse(Regex.Replace(tdRow[2], @"[/\&nbsp;\,\s]", "")) + decimal.Parse(Regex.Replace(tdRow[3], @"[/\&nbsp;\,\s]", ""));
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion

                #region  第六步


                int PageCount = int.Parse(CommonFun.GetMidStr(httpResult.Html, "总共", "页").ToString());
                int i = 2;
                while (true)
                {
                    Url = baseUrl + string.Format("cx_jieguo.aspx?dwzh={0}&grzh={1}", dwzh, qrzh);
                    postdata = string.Format("__EVENTTARGET=btnNext&__EVENTARGUMENT=&__VIEWSTATE={0}&__VIEWSTATEENCRYPTED=&__EVENTVALIDATION={1}", viewstate.ToUrlEncode(), eventvalidation.ToUrlEncode());
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        Encoding = Encoding.GetEncoding("utf-8"),
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
                    viewstate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                    eventvalidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                    if (i > PageCount) break;
                    i++;
                    //请求失败后返回
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr[1]/td/table/tr/td[1]/table/tr/td/div/table/tr[position()>1]", "inner");
                    foreach (string item in results)
                    {
                        j++;
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                        if (tdRow.Count != 5)
                        {
                            continue;
                        }
                        detail = new ProvidentFundDetail();

                        detail.PayTime = tdRow[0].ToDateTime();
                        if (tdRow[1].IndexOf("汇缴") != -1)
                        {
                            detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[1], "", "[").Replace("-", "");
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            detail.PersonalPayAmount = decimal.Parse(Regex.Replace(tdRow[3], @"[/\&nbsp;\,\s]", "")) / 2;//金额
                            detail.CompanyPayAmount = decimal.Parse(Regex.Replace(tdRow[3], @"[/\&nbsp;\,\s]", "")) / 2;//金额
                            detail.ProvidentFundBase = (detail.PersonalPayAmount / Res.PersonalMonthPayRate);//缴费基数
                            PaymentMonths++;
                        }
                        else if (tdRow[1].IndexOf("结息") != -1)
                        {
                            detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[1], "", "[");
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.PersonalPayAmount = decimal.Parse(Regex.Replace(tdRow[2], @"[/\&nbsp;\,\s]", "")) + decimal.Parse(Regex.Replace(tdRow[3], @"[/\&nbsp;\,\s]", ""));//金额
                            detail.Description = tdRow[1];

                        }
                        else
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.Description = tdRow[1];
                            detail.PersonalPayAmount = decimal.Parse(Regex.Replace(tdRow[2], @"[/\&nbsp;\,\s]", "")) + decimal.Parse(Regex.Replace(tdRow[3], @"[/\&nbsp;\,\s]", ""));//金额;
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