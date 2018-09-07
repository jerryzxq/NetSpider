using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.GD
{
    public class huizhou : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://oa.hzgjj.cn:8083/vgsGjj-port/";
        string fundCity = "gd_huizhou";
        int PaymentMonths = 0;
        private decimal payRate = 0.08M;
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Res.StatusDescription = "惠州公积金无需初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;
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

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            ProvidentFundDetail detailRes = null;
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数 实际登陆手机号码可为空
                if (fundReq.Name.IsEmpty() || fundReq.Username.IsEmpty() || fundReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 登录

                Url = baseUrl + "psnquery/psnqueryPsninfoAction.shtml";
                postdata = String.Format("method=login&perName={0}&perAccount={1}&perNo={2}&perMobile={3}", fundReq.Name.ToUrlEncode(), fundReq.Username, fundReq.Identitycard, fundReq.Mobile);
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='psnqueryForm']/table[2]/tr[1]/td[1]", "text");
                string errorMsg = string.Empty;
                if (results.Count > 0)
                {
                    errorMsg = results[0].Trim();
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        Res.StatusDescription = errorMsg;
                        Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                        return Res;
                    }
                }
                #endregion
                #region 获取基本信息

                Res.IdentityCard = fundReq.Identitycard;
                Res.Name = fundReq.Name;
                Res.ProvidentFundNo = fundReq.Username;
                Res.Phone = fundReq.Mobile;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[3]/tr[1]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Name = results[0].Trim();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[3]/tr[1]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0].Trim();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[3]/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0].Trim();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[3]/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0].Trim();
                    if (string.IsNullOrEmpty(Res.CompanyNo) || Res.CompanyNo == "空")
                    {
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[3]/tr[3]/td[4]", "text");
                        if (results.Count > 0)
                        {
                            Res.CompanyNo = results[0].Trim();
                        }
                    }
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[3]/tr[4]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0].Trim();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[3]/tr[5]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Status = results[0].Trim();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[3]/tr[5]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.SalaryBase = results[0].Trim().ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[3]/tr[6]/td[2]", "text");
                if (results.Count > 0)
                {
                    decimal monthPay = results[0].Trim().ToDecimal(0);
                    if (Res.SalaryBase > 0 && monthPay > 0)
                    {
                        Res.PersonalMonthPayRate = Res.CompanyMonthPayRate = Math.Round((monthPay / Res.SalaryBase) / 2, 2);
                    }
                    else
                    {
                        Res.PersonalMonthPayRate = Res.CompanyMonthPayRate = payRate;
                    }
                    Res.PersonalMonthPayAmount = Res.CompanyMonthPayAmount = Math.Round(monthPay / 2, 2);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[3]/tr[6]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].Trim().ToDecimal(0);
                }

                #endregion
                #region 查询明细

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='listtable']/tbody/tr", "inner");
                foreach (string item in @results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text");
                    if (tdRow.Count < 5)
                    {
                        continue;
                    }
                    ProvidentFundDetail details = new ProvidentFundDetail();
                    details.PayTime = tdRow[0].Trim().ToDateTime();
                    details.Description = tdRow[1].Trim();
                    if (details.Description == "汇缴")
                    {
                        details.ProvidentFundTime = tdRow[4].Trim().Replace("-", "");
                        details.ProvidentFundBase = Math.Round(tdRow[2].Trim().ToDecimal(0) / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate), 2);
                        details.PersonalPayAmount = Math.Round(details.ProvidentFundBase * Res.PersonalMonthPayRate, 2);
                        details.CompanyPayAmount = Math.Round(details.ProvidentFundBase * Res.CompanyMonthPayRate, 2);
                        details.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        details.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        PaymentMonths++;
                    }
                    else if (details.Description.IndexOf("取",StringComparison.Ordinal)>-1)
                    {
                        details.PersonalPayAmount = tdRow[2].Trim().ToDecimal(0);
                        details.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        details.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                    }
                    else
                    {
                        details.PersonalPayAmount = tdRow[2].Trim().ToDecimal(0);
                        details.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        details.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    }
                    Res.ProvidentFundDetailList.Add(details);
                }
                #endregion
                Res.PaymentMonths = PaymentMonths;
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
