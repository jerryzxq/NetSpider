using System;
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
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.FJ
{
    public class fjsheng : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://cx.fjszgjj.com/";
        string fundCity = "fj_fjsheng";
        int PaymentMonths = 0;
        private decimal payRate = 0.08M;
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            string Url = string.Empty;
            try
            {
                Url = baseUrl + "Admin_Login.aspx";
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
                string eventvalidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                string viewstategenerator = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATEGENERATOR']", "value")[0];
                string viewstate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "Validate.aspx";
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
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                //添加缓存
                Dictionary<string, object> dic = new Dictionary<string, object>
                {
                   {"eventvalidation",eventvalidation} ,
                   {"viewstategenerator",viewstategenerator} ,
                   {"viewstate",viewstate} ,
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

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            ProvidentFundReserveRes ReserveRes = new ProvidentFundReserveRes();
            ProvidentFundDetail detailRes = null;
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string eventvalidation = string.Empty;
            string viewstategenerator = string.Empty;
            string viewstate = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    Dictionary<string, object> dic = (Dictionary<string, object>)CacheHelper.GetCache(fundReq.Token);
                    cookies = (CookieCollection)dic["cookies"];
                    eventvalidation = (string)dic["eventvalidation"];
                    viewstategenerator = (string)dic["viewstategenerator"];
                    viewstate = (string)dic["viewstate"];
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数 
                if (fundReq.Identitycard.IsEmpty() || fundReq.Password.IsEmpty() || fundReq.Vercode.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 登录

                Url = baseUrl + "Admin_Login.aspx";
                postdata = string.Format("__EVENTTARGET=&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__EVENTVALIDATION={2}&ddlUserType=03&txtName={3}&txtPass={4}&txtCheckCode={5}&btnLogin=%E7%99%BB%E5%BD%95", viewstate.ToUrlEncode(Encoding.GetEncoding("gb2312")), viewstategenerator.ToUrlEncode(Encoding.GetEncoding("gb2312")), eventvalidation.ToUrlEncode(Encoding.GetEncoding("gb2312")), fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Encoding = Encoding.UTF8,
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
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region 普通账号基本信息

                Url = baseUrl + "grjbxx.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.IdentityCard = fundReq.Identitycard;
                Res.Name = fundReq.Name;

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='ctl00$ContentPlace$txtXm']", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='ctl00$ContentPlace$txtGrZjh']", "value");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='ctl00$ContentPlace$txtYddh']", "value");
                if (results.Count > 0)
                {
                    Res.Phone = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='ctl00$ContentPlace$txtJtdh']", "value");
                if (results.Count > 0)
                {
                    Res.Phone = string.IsNullOrEmpty(Res.Phone) ? results[0] : Res.Phone;
                }
                //个人账户信息
                Url = baseUrl + "grzhxx_list.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //来往明细
                string laiwangLink = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='ctl00_ContentPlace_ShowTxt']/table/tr[2]/td[5]/a", "href")[0];
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='ctl00_ContentPlace_ShowTxt']/table/tr[2]/td", "text");
                if (results.Count == 11)
                {
                    Res.ProvidentFundNo = results[0];
                    Res.CompanyNo = results[5];
                    Res.OpenTime = Convert.ToDateTime(results[8]).ToString(Consts.DateFormatString5);
                    Res.Status = results[9];
                }
                //账户信息
                Url = baseUrl + "grzhxx.aspx?&RowIndex=0&isSkip=0";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //公积金账户余额
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='ctl00$ContentPlace$txtgjjzhye']", "value");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);
                }
                //公积金核定月工资额
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='ctl00$ContentPlace$txtgjjhdygze']", "value");
                if (results.Count > 0)
                {
                    Res.SalaryBase = results[0].ToDecimal(0);
                }
                //公积金账户月缴额
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='ctl00$ContentPlace$txtgjjhdyjje']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = Res.PersonalMonthPayAmount = results[0].ToDecimal(0) / 2;
                    if (Res.SalaryBase > 0)
                    {
                        Res.CompanyMonthPayRate = Res.PersonalMonthPayRate = Math.Round(Res.PersonalMonthPayAmount / Res.SalaryBase, 2);
                    }
                }
                Res.PersonalMonthPayRate = Res.CompanyMonthPayRate = Res.PersonalMonthPayRate > payRate ? Res.PersonalMonthPayRate : payRate;
                #endregion
                #region 补贴账户基本信息

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='ctl00_ContentPlace_txtgrbtzh']", "value");
                if (results.Count > 0)
                {
                    ReserveRes.ProvidentFundNo = results[0];//个人补贴账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='ctl00_ContentPlace_txtdwgjjzh']", "value");
                if (results.Count > 0)
                {
                    ReserveRes.CompanyNo = results[0];//单位公积金账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='ctl00_ContentPlace_txtdwgjjzh']", "value");
                if (results.Count > 0)
                {
                    ReserveRes.Status = results[0];//补贴账户状态
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='ctl00_ContentPlace_txtbtzhye']", "value");
                if (results.Count > 0)
                {
                    ReserveRes.TotalAmount = results[0].ToDecimal(0);//补贴账户余额
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='ctl00_ContentPlace_txtbthdygze']", "value");
                if (results.Count > 0)
                {
                    ReserveRes.SalaryBase = results[0].ToDecimal(0);//补贴核定月工资额
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='ctl00_ContentPlace_txtbthdyjje']", "value");
                if (results.Count > 0)
                {
                    ReserveRes.CompanyMonthPayAmount = results[0].ToDecimal(0);//补贴账户月缴额
                    if (ReserveRes.SalaryBase > 0)
                    {
                        ReserveRes.CompanyMonthPayRate = Math.Round(ReserveRes.CompanyMonthPayAmount / Res.SalaryBase, 2);
                    }
                }
                ReserveRes.OpenTime = Res.OpenTime;
                ReserveRes.CompanyMonthPayRate = ReserveRes.CompanyMonthPayRate > 0 ? ReserveRes.CompanyMonthPayRate : payRate;
                #endregion
                #region 查询明细

                Url = baseUrl + laiwangLink;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                eventvalidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                viewstategenerator = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATEGENERATOR']", "value")[0];
                viewstate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                int currentPage = 1;
                int totalPage = 1;
                results = new List<string>();
                int beginYear = string.IsNullOrEmpty(Res.OpenTime) ? DateTime.Now.AddYears(-10).Year : DateTime.ParseExact(Res.OpenTime, "yyyyMMdd", null).Year;
                do
                {
                    postdata = string.Format("__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__EVENTVALIDATION={2}&ctl00%24ContentPlace%24ddlBYear={4}&ctl00%24ContentPlace%24ddlBMonth=01&ctl00%24ContentPlace%24ddlEYear={5}&ctl00%24ContentPlace%24ddlEMonth={6}&ctl00%24ContentPlace%24txtmybs=100&ctl00%24ContentPlace{3}", viewstate.ToUrlEncode(Encoding.GetEncoding("gb2312")), viewstategenerator.ToUrlEncode(Encoding.GetEncoding("gb2312")), eventvalidation.ToUrlEncode(Encoding.GetEncoding("gb2312")), currentPage == 1 ? "%24btncx=%E6%9F%A5%E8%AF%A2" : "%24btnViewNext=%E4%B8%8B%E4%B8%80%E9%A1%B5", beginYear, DateTime.Now.Year, DateTime.Now.ToString("MM"));
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    eventvalidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                    viewstategenerator = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATEGENERATOR']", "value")[0];
                    viewstate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                    if (currentPage == 1)
                    {
                        totalPage = CommonFun.GetMidStr(httpResult.Html, "页/共", "页").ToInt(0);
                    }
                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", ""), "//div[@id='ctl00_ContentPlace_ShowTxt']/table/tr[position()>1]", "", true));
                    currentPage++;
                } while (currentPage <= totalPage);
                foreach (string s in results)
                {
                    List<string> tdRow = HtmlParser.GetResultFromParser(s, "//td", "");
                    if (tdRow.Count < 7) continue;
                    DateTime payTime = Convert.ToDateTime(tdRow[3]);
                    string fundTime = string.Empty;
                    if (tdRow[4] == "汇缴" || tdRow[4] == "补缴")
                    {
                        fundTime = payTime.ToString(Consts.DateFormatString7);
                    }
                    else if (tdRow[4].IndexOf("汇缴", StringComparison.Ordinal) > -1 || tdRow[4].IndexOf("补缴", StringComparison.Ordinal) > -1)
                    {
                        fundTime = tdRow[4].ToTrim("汇缴").ToTrim("补缴").Trim();
                    }
                   
                    detailRes = Res.ProvidentFundDetailList.FirstOrDefault(o => o.ProvidentFundTime == fundTime && !string.IsNullOrEmpty(fundTime)&&o.PaymentType==tdRow[1]);

                    bool isSave = false;
                    if (detailRes == null)
                    {
                        isSave = true;
                        detailRes = new ProvidentFundDetail();
                        detailRes.PayTime = payTime;
                        detailRes.ProvidentFundTime = !string.IsNullOrEmpty(fundTime)
                            ? fundTime
                            : payTime.ToString(Consts.DateFormatString7);
                        if (tdRow[4].IndexOf("汇缴", StringComparison.Ordinal) > -1 || tdRow[4].IndexOf("补缴", StringComparison.Ordinal) > -1)
                        {
                            detailRes.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detailRes.PaymentType = tdRow[1];
                        }
                        else if (tdRow[4].IndexOf("支取", StringComparison.Ordinal) > -1)
                        {
                            detailRes.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                            detailRes.PaymentType = tdRow[1];
                        }
                        else
                        {
                            detailRes.PaymentFlag = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detailRes.PaymentType = tdRow[1];
                        }
                        switch (tdRow[1])
                        {
                            case "公积金":
                                if (tdRow[4].IndexOf("汇缴", StringComparison.Ordinal) > -1 || tdRow[4].IndexOf("补缴", StringComparison.Ordinal) > -1)
                                {
                                    detailRes.ProvidentFundBase = (tdRow[6].ToDecimal(0) / 2) / Res.PersonalMonthPayRate;
                                }
                                break;
                            case "补贴":
                                if (tdRow[4].IndexOf("汇缴", StringComparison.Ordinal) > -1 || tdRow[4].IndexOf("补缴", StringComparison.Ordinal) > -1)
                                {
                                    detailRes.ProvidentFundBase = tdRow[6].ToDecimal(0) / ReserveRes.CompanyMonthPayRate;
                                }
                                break;
                        }
                    }
                    switch (tdRow[1])
                    {
                        case "公积金":
                            if (tdRow[4].IndexOf("汇缴", StringComparison.Ordinal) > -1 ||
                                tdRow[4].IndexOf("补缴", StringComparison.Ordinal) > -1)
                            {
                                detailRes.PersonalPayAmount = detailRes.CompanyPayAmount += tdRow[6].ToDecimal(0) / 2;
                            }
                            else
                            {
                                detailRes.PersonalPayAmount += tdRow[6].ToTrim("-").ToDecimal(0) / 2;
                            }
                            break;
                        case "补贴":
                            detailRes.CompanyPayAmount += tdRow[6].ToTrim("-").ToDecimal(0);
                            break;
                    }
                    if (string.IsNullOrEmpty(detailRes.Description) || detailRes.Description.IndexOf(tdRow[4]) > -1)
                    {
                        detailRes.Description = tdRow[4];
                    }
                    else
                    {
                        detailRes.Description += ";" + tdRow[4];
                    }
                    if (!isSave) continue;
                    switch (tdRow[1])
                    {
                        case "公积金":
                            Res.ProvidentFundDetailList.Add(detailRes);
                            break;
                        case "补贴":
                            ReserveRes.ProvidentReserveFundDetailList.Add(detailRes);
                            break;
                    }
                }
                #endregion
                Res.ProvidentFundReserveRes = ReserveRes;
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

