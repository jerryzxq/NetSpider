using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.ZJ
{
    public class pujiang : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://gjj.pj.gov.cn/";
        string fundCity = "zj_pujiang";
        #endregion
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
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

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            //ProvidentFundLoanRes ResLoad = new ProvidentFundLoanRes();//贷款
            //ProvidentFundReserveRes ReserveRes = new ProvidentFundReserveRes();//补充公积金
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.12;
            List<string> results = new List<string>();
            try
            {
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,初始化

                Url = baseUrl + "loginPage.aspx?TypeID=1";
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
                string viewstate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='__VIEWSTATE']", "value")[0].ToUrlEncode(Encoding.GetEncoding("gb2312"));
                string viewstategenerator = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='__VIEWSTATEGENERATOR']", "value")[0].ToUrlEncode(Encoding.GetEncoding("gb2312"));
                string eventvalidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='__EVENTVALIDATION']", "value")[0].ToUrlEncode(Encoding.GetEncoding("gb2312"));

                #endregion
                #region 第二步,登陆

                //Url = baseUrl + "loginPage.aspx?TypeID=1";
                postdata = string.Format("__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__VIEWSTATEENCRYPTED=&__EVENTVALIDATION={2}&news%3A_ctl0%3AtxtUserName={3}&news%3A_ctl0%3AtxtPass={4}&news%3A_ctl0%3AbtnLogin=%E7%99%BB%E5%BD%95", viewstate, viewstategenerator, eventvalidation, fundReq.Identitycard, fundReq.Password);
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
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');</script>");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region  第三步,公积金基本信息

                Res.Name = fundReq.Name;
                Res.IdentityCard = fundReq.Identitycard;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='dataGrid_GJ']/tr[2]/td");
                if (results.Count != 6)
                {
                    Res.StatusDescription = "无公积金信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.Name = results[0];
                Res.IdentityCard = results[1];
                Res.CompanyName = results[2];
                Res.CompanyMonthPayAmount = Res.PersonalMonthPayAmount = results[3].ToDecimal(0) / 2;
                Res.PersonalMonthPayRate = Res.CompanyMonthPayRate = payRate;
                Res.SalaryBase = Res.PersonalMonthPayAmount / payRate;
                Res.TotalAmount = results[4].ToDecimal(0);
                Res.LastProvidentFundTime = results[5].Substring(0, 6);

                #endregion
                #region 第四步,公积金明细

                Url = baseUrl + "searchPsersonPage.aspx";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='dataGrid_MX']/tr[position()>1]", "", true);
                foreach (string s in results)
                {
                    List<string> tdRow = HtmlParser.GetResultFromParser(s, "//td", "");
                    if (tdRow.Count != 5) continue;
                    ProvidentFundDetail detail = new ProvidentFundDetail();
                    detail.PayTime = DateTime.ParseExact(tdRow[0], "yyyyMMdd", null);
                    detail.Description = tdRow[3];
                    if (detail.Description == "托收")
                    {
                        detail.Description = tdRow[3];
                        detail.ProvidentFundTime = tdRow[4];
                        detail.PersonalPayAmount = detail.CompanyPayAmount = tdRow[1].ToDecimal(0)/2;
                        detail.ProvidentFundBase = detail.PersonalPayAmount/payRate;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                    }
                    else
                    {
                        detail.Description = tdRow[3]+tdRow[4];
                        detail.PersonalPayAmount = tdRow[1].ToDecimal(0);
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion
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