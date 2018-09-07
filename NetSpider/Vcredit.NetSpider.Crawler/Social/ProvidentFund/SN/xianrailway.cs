using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SN
{
    /// <summary>
    /// 网站已经合并到西安公积金中心,目前查询的是网站合并前的数据
    /// </summary>
    public class xianrailway : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;

        string url = string.Empty;
        string fundCity = "sn_xianrailway";
        int PaymentMonths = 0;
        decimal perAccounting;//个人占比
        decimal comAccounting;//公司占比
        decimal totalRate;//总缴费比率
        #endregion
        
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            Res.Token = CommonFun.GetGuidID();
            string viewstate = string.Empty;
            string eventvalidation = string.Empty;
            try
            {
                url = "http://61.232.201.76:7001/xtwscx/zfbzgl/zfbzsq/login.jsp";
                httpItem = new HttpItem()
                {
                    URL = url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = fundCity + "公积金初始化完成";
                CacheHelper.SetCache(Res.Token, cookies);
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
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
          
            try
            {
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (string.IsNullOrWhiteSpace(fundReq.Identitycard) || string.IsNullOrWhiteSpace(fundReq.Username)||string.IsNullOrEmpty(fundReq.Password))
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
            
                #region 第一步,登陆

                url = " http://61.232.201.76:7001/xtwscx/zfbzgl/zfbzsq/login_hidden.jsp";
                postdata = string.Format("password={0}&sfzh={1}&cxyd=%B5%B1%C7%B0%C4%EA%B6%C8&zgzh={2}",fundReq.Password,fundReq.Identitycard,fundReq.Username);
                httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\");");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    Res.StatusDescription = errorMsg;
                    return Res;
                }
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string zgxm = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgxm']", "value")[0];
                string dwbm = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dwbm']", "value")[0];
                if (string.IsNullOrEmpty(zgxm)||string.IsNullOrEmpty(dwbm))
                {
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    Res.StatusDescription = "账户信息不存在";
                    return Res;
                }
              
                #endregion
                #region 第二步,获取基本信息
                url = "http://61.232.201.76:7001/xtwscx/zfbzgl/zfbzsq/main_menu.jsp";
                postdata = string.Format("zgzh={0}&sfzh={1}&zgxm={2}&dwbm={3}&cxyd=%B5%B1%C7%B0%C4%EA%B6%C8", fundReq.Username, fundReq.Identitycard, zgxm.ToUrlEncode(Encoding.GetEncoding("GBK")), dwbm);
                httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table//table[@class='1']/tr[1]/td[2]", "text",true);
                if (results.Count >0)
                {
                    Res.Name = results[0].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table//table[@class='1']/tr[1]/td[4]", "text", true);
                if (results.Count > 0)
                {
                    Res.BankCardNo = results[0].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table//table[@class='1']/tr[2]/td[2]", "text", true);
                if (results.Count > 0)
                {
                  Res.IdentityCard = results[0].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table//table[@class='1']/tr[2]/td[4]", "text", true);
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table//table[@class='1']/tr[3]/td[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0].Replace("&nbsp;", "");
                }
                
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table//table[@class='1']/tr[4]/td[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.OpenTime = results[0].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table//table[@class='1']/tr[4]/td[4]", "text", true);
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table//table[@class='1']/tr[5]/td[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.SalaryBase = results[0].Replace("&nbsp;", "").ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table//table[@class='1']/tr[5]/td[5]", "text", true);
                if (results.Count > 0)
                {
                    string[] payrate = results[0].Replace("&nbsp;", "").Split('/');
                    if (payrate.Length == 2)
                    {
                        Res.CompanyMonthPayRate = (payrate[1].Replace("%", "").ToDecimal(0)) * 0.01M;
                        Res.PersonalMonthPayRate = (payrate[0].Replace("%", "").ToDecimal(0)) * 0.01M;
                        Res.CompanyMonthPayAmount = Math.Round(Res.SalaryBase * Res.CompanyMonthPayRate, 2);
                        Res.PersonalMonthPayAmount = Math.Round(Res.SalaryBase * Res.PersonalMonthPayRate, 2);
                    }
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table//table[@class='1']/tr[10]/td[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].Replace("&nbsp;", "").ToDecimal(0);
                }
                #endregion

                #region 缴费明细
                if (Res.PersonalMonthPayRate > 0 && Res.CompanyMonthPayRate > 0)
                {
                    totalRate = Res.PersonalMonthPayRate + Res.CompanyMonthPayRate;
                    perAccounting = (Res.PersonalMonthPayRate / totalRate);
                    comAccounting = (Res.CompanyMonthPayRate / totalRate);
                }
                else
                {
                    totalRate = (payRate) * 2;//0.16
                    perAccounting = comAccounting = (decimal)0.50;
                }
                //当年
                url = "http://61.232.201.76:7001/xtwscx/zfbzgl/gjjmxcx/gjjmx_cx.jsp";
                postdata = string.Format("sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd={4}", fundReq.Identitycard, zgxm.ToUrlEncode(Encoding.GetEncoding("GBK")), fundReq.Username, dwbm, "%B5%B1%C7%B0%C4%EA%B6%C8");
                httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table//table[@class='1']/tr[position()>1]", "inner", true);
                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow.Count < 6)
                    {
                        continue;
                    }
                    ProvidentFundDetail detail = new ProvidentFundDetail();
                    detail.PayTime = tdRow[0].ToDateTime(Consts.DateFormatString2);
                    detail.Description = tdRow[1];
                    if (tdRow[1].IndexOf("汇缴个人单位", StringComparison.Ordinal) > -1)
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        MatchCollection yyyyMM = new Regex(@"[0-9]\d*").Matches(tdRow[1]);
                        if (yyyyMM.Count==2)
                        {
                            detail.ProvidentFundTime = yyyyMM[0].Value + yyyyMM[1].Value;
                        }
                        detail.PersonalPayAmount = Math.Round(tdRow[3].ToDecimal(0) * perAccounting, 2);
                        detail.CompanyPayAmount = Math.Round(tdRow[3].ToDecimal(0) * comAccounting, 2);
                        detail.ProvidentFundBase = Math.Round(tdRow[3].ToDecimal(0) / totalRate, 2);
                        PaymentMonths++;
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0);
                    }
                    Res.ProvidentFundDetailList.Add(detail);
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
            //Res.Result = jsonParser.SerializeObject(Res);
            return Res;
        }
    }
}

