using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.JS
{
    public class suqian : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.sqzfgjj.com/";
        string fundCity = "js_suqian";
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                vcRes.StatusDescription = "所选城市无需初始化";
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                vcRes.StatusCode = ServiceConsts.StatusCode_error;
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return vcRes;
        }

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string LASTFOCUS = string.Empty;
            string EVENTTARGET = string.Empty;
            string EVENTARGUMENT = string.Empty;
            string VIEWSTATE = string.Empty;
            string Button_Query = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || fundReq.Password.IsEmpty() || fundReq.Username.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆
                Url = baseUrl + "Query_Sspersons.aspx";
                httpItem = new HttpItem()
                  {
                      URL = Url,
                      Method = "GET",
                      Referer = baseUrl + "ask/",
                      CookieCollection = cookies,
                      ResultCookieType = ResultCookieType.CookieCollection
                  };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__LASTFOCUS']", "value");
                if (results.Count > 0)
                {
                    LASTFOCUS = results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryError;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTTARGET']", "value");
                if (results.Count > 0)
                {
                    EVENTTARGET = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTARGUMENT']", "value");
                if (results.Count > 0)
                {
                    EVENTARGUMENT = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                if (results.Count > 0)
                {
                    VIEWSTATE = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='Button_Query']", "value");
                if (results.Count > 0)
                {
                    Button_Query = results[0];
                }
                Encoding encode = Encoding.GetEncoding("gb2312");
                Url = baseUrl + "Query_Sspersons.aspx";
                postdata = string.Format("__LASTFOCUS={0}&__EVENTTARGET={1}&__EVENTARGUMENT={2}&__VIEWSTATE={3}&ZLTextBox_spidno={4}&ZLTextBox_spcard={5}&Button_Query={6}", LASTFOCUS.ToUrlEncode(), EVENTTARGET.ToUrlEncode(), EVENTARGUMENT.ToUrlEncode(), VIEWSTATE.ToUrlEncode(), fundReq.Identitycard, fundReq.Password.ToUrlEncode().Replace("%20", "+"), Button_Query.ToUrlEncode().Replace("%20", "+"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    Referer = baseUrl + "Query_Sspersons.aspx",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "');");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                #region 第二步,获取基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label_spidno']", "text");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0]; //身份账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label_spname']", "text");
                if (results.Count > 0)
                {
                    Res.Name = results[0]; //姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label_spcode']", "text");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0]; //公积金账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label_spsingl']", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = results[0].ToDecimal(0); //个人缴存比例
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label_spjcbl']", "text");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = results[0].ToDecimal(0); //单位缴存比例
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label_spmfact']", "text");
                if (results.Count > 0)
                {
                    decimal monthPay = results[0].ToDecimal(0);//月缴存额
                    Res.PersonalMonthPayAmount = monthPay * (Res.PersonalMonthPayRate * (decimal)0.01);//个人月缴额
                    Res.CompanyMonthPayAmount = monthPay * (Res.CompanyMonthPayRate * (decimal)0.01);//单位月缴额
                    if (Res.PersonalMonthPayRate > 0 && Res.CompanyMonthPayRate > 0)
                    {
                        decimal totalRate = (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate) * (decimal)0.01;
                        Res.SalaryBase = monthPay / totalRate;//基本薪资
                    }
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label_spjym']", "text");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0]; //缴至年月
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label_sncode']", "text");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0]; //单位代码
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label_spmend']", "text");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0); //目前余额
                }
                #endregion
                #region 缴费明细，网站未提供
                //5566
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
