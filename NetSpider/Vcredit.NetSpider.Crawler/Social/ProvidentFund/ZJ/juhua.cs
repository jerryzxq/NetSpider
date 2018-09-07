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
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.ZJ
{
    public class juhua : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://jhgjj.jh0051.com/WebSite/";
        string fundCity = "zj_juhua";
        #endregion
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            string Url = string.Empty;
            try
            {
                Url = baseUrl + "WebGjjcx.aspx";
                httpItem = new HttpItem
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
                string viewstate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                string eventvalidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                string viewstategenerator = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATEGENERATOR']", "value")[0];
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Dictionary<string, object> dic = new Dictionary<string, object>
                {
                  {"viewstate",viewstate},{"eventvalidation",eventvalidation},{"viewstategenerator",viewstategenerator},{"cookies",cookies}
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
            ProvidentFundLoanRes ResLoad = new ProvidentFundLoanRes();//贷款
            ProvidentFundReserveRes ReserveRes = new ProvidentFundReserveRes();//补充公积金
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            string viewstate = string.Empty;
            string eventvalidation = string.Empty;
            string viewstategenerator = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    Dictionary<string, object> dic = (Dictionary<string, object>)CacheHelper.GetCache(fundReq.Token);
                    cookies = (CookieCollection)dic["cookies"];
                    viewstate = dic["viewstate"].ToString();
                    eventvalidation = dic["eventvalidation"].ToString();
                    viewstategenerator = dic["viewstategenerator"].ToString();
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步登陆
                Url = baseUrl + "WebGjjcx.aspx";
                postdata = string.Format("__LASTFOCUS=&__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__EVENTVALIDATION={2}&TextBox1={3}&TextBox2=&TextBox3={4}&ImageButton1.x=33&ImageButton1.y=7&HiddenField1=0", viewstate.ToUrlEncode(Encoding.GetEncoding("gb2312")), viewstategenerator.ToUrlEncode(Encoding.GetEncoding("gb2312")), eventvalidation.ToUrlEncode(Encoding.GetEncoding("gb2312")), fundReq.Identitycard, fundReq.Password);
                httpItem = new HttpItem
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
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "')</script>");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region  第二步 公积金,补充公积金基本信息

                Res.Name = fundReq.Name;
                Res.IdentityCard = fundReq.Identitycard;
                Res.CompanyNo = ReserveRes.CompanyNo = CommonFun.GetMidStr(httpResult.Html, "&#39;", "&#39;");
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label2']", "text");
                if (results.Count > 0)
                {
                    Res.Name = results[0].ToTrim("姓名：");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//a[@id='HyperLink1']", "text");
                if (results.Count > 0)
                {
                    Res.CompanyName = ReserveRes.CompanyName = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//a[@id='HyperLink2']", "text");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = ReserveRes.LastProvidentFundTime = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//a[@id='HyperLink3']", "text");
                if (results.Count > 0)
                {
                    Res.SalaryBase = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//a[@id='HyperLink4']", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = Res.CompanyMonthPayAmount = results[0].ToDecimal(0) / 2;
                }
                if (Res.PersonalMonthPayAmount > 0 & Res.SalaryBase > 0)
                {
                    Res.PersonalMonthPayRate = Res.CompanyMonthPayRate = Res.PersonalMonthPayAmount / Res.SalaryBase;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//a[@id='HyperLink6']", "text");
                if (results.Count > 0)
                {
                    ReserveRes.SalaryBase = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//a[@id='HyperLink9']", "text");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);
                }
                #endregion
                #region 第三步 公积金,补充公积金明细

                payRate = Res.PersonalMonthPayRate > 0 ? Res.PersonalMonthPayRate : payRate;
                int pageIndex = 1;
                int pageCount = 1;
                string endTime = DateTime.Now.ToString(Consts.DateFormatString2);
                results = new List<string>();
                Url = baseUrl + string.Format("GjjcxDetail.aspx?count=%27{0}%27", ReserveRes.CompanyNo);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                viewstate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                eventvalidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                viewstategenerator = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATEGENERATOR']", "value")[0];
                do
                {
                    postdata = string.Format("__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__EVENTTARGET=AspNetPager1&__EVENTARGUMENT={3}&__EVENTVALIDATION={2}&time1=1970-01-01&time2={4}", viewstate.ToUrlEncode(Encoding.GetEncoding("gb2312")), viewstategenerator.ToUrlEncode(Encoding.GetEncoding("gb2312")), eventvalidation.ToUrlEncode(Encoding.GetEncoding("gb2312")), pageIndex, endTime);
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Encoding = Encoding.UTF8,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (pageIndex == 1)
                    {
                        pageCount = CommonFun.GetMidStr(httpResult.Html, "页码：1/", "</span>").ToInt(0);
                    }
                    if (pageIndex != pageCount)
                    {
                        viewstate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                        eventvalidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                        viewstategenerator = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATEGENERATOR']", "value")[0];
                    }
                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='GridView1']/tr[position()>1]", "", true));
                    pageIndex++;
                } while (pageIndex <= pageCount);
                foreach (string s in results)
                {
                    List<string> tdRow = HtmlParser.GetResultFromParser(s, "//td", "text");
                    if (tdRow.Count < 9) continue;
                    ProvidentFundDetail detail = new ProvidentFundDetail();
                    ProvidentFundDetail detailRes = new ProvidentFundDetail();
                    detail.ProvidentFundTime = detailRes.ProvidentFundTime = tdRow[0];
                    detail.PayTime = detailRes.PayTime = tdRow[1].ToDateTime();
                    detail.Description = detailRes.Description = tdRow[4];
                    detailRes.PersonalPayAmount = tdRow[6].ToDecimal(0);
                    if (tdRow[4] == "汇缴")
                    {
                        detail.PersonalPayAmount = detail.CompanyPayAmount = tdRow[5].ToDecimal(0) / 2;
                        detail.ProvidentFundBase = detail.PersonalPayAmount / payRate;
                        detail.PaymentFlag = detailRes.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = detailRes.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                    }
                    else if (tdRow[4].IndexOf("取") > -1)
                    {
                        detail.PersonalPayAmount =tdRow[5].ToDecimal(0);
                        detail.PaymentFlag = detailRes.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = detailRes.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                    }
                    else 
                    {
                        detail.PersonalPayAmount = tdRow[5].ToDecimal(0);
                        detail.PaymentFlag = detailRes.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = detailRes.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                    ReserveRes.ProvidentReserveFundDetailList.Add(detailRes);
                }
                #endregion
                //http://jhgjj.jh0051.com/WebSite/WebGjjdkcx.aspx（贷款）
                Res.ProvidentFundReserveRes = ReserveRes;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
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