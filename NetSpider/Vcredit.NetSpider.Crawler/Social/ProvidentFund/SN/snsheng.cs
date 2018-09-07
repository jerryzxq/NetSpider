using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SN
{
    public class snsheng : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;

        string url = string.Empty;
        string fundCity = "sn_snsheng";
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
                url = " http://www.sxgjj.com/seach/Sigin.aspx";
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
                viewstate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                eventvalidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = fundCity + "公积金初始化完成";
                Dictionary<string, object> dic = new Dictionary<string, object>
                {
                    {"viewstate", viewstate},
                    {"eventvalidation", eventvalidation},
                    {"cookies", cookies}
                };
                CacheHelper.SetCache(Res.Token, dic);
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
            string viewstate = string.Empty;
            string eventvalidation = string.Empty;
            try
            {
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    dic = (Dictionary<string, object>)CacheHelper.GetCache(fundReq.Token);
                    viewstate = dic["viewstate"].ToString();
                    eventvalidation = dic["eventvalidation"].ToString();
                    cookies = dic["cookies"] as CookieCollection;
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                string loginAcount;
                switch (fundReq.LoginType)
                {
                    case "1":
                        loginAcount = fundReq.Identitycard;
                        break;
                    case "2":
                        loginAcount = fundReq.Username;
                        break;
                    default:
                        loginAcount = string.Empty;
                        break;
                }
                if (string.IsNullOrWhiteSpace(loginAcount) || string.IsNullOrWhiteSpace(fundReq.Password))
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                
              
                #region 第一步,登陆

                url = "http://www.sxgjj.com/seach/Sigin.aspx";
                postdata = string.Format("__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE={0}&__EVENTVALIDATION={1}&txtzhanghao={2}&txtpwd={3}&Button1=%C8%B7%C8%CF", viewstate.ToUrlEncode(Encoding.GetEncoding("gb2312")), eventvalidation.ToUrlEncode(Encoding.GetEncoding("gb2312")), loginAcount, fundReq.Password.ToUrlEncode(Encoding.GetEncoding("gb2312")));
                httpItem = new HttpItem()
                {
                    Allowautoredirect = false,
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "')</script>");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    Res.StatusDescription = errorMsg;
                    return Res;
                }
                if (httpResult.StatusCode != HttpStatusCode.Found)
                {
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                url = httpResult.RedirectUrl;
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
                #endregion
                #region 第二步,获取基本信息

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='ctl00_ContentPlaceHolder1_DataList1']//table/tr/td[2]", "text");
                if (results.Count != 16)
                {
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    Res.StatusDescription = "基本信息页面已改动,请审核页面信息";
                    return Res;
                }
                Res.Name = results[0];
                Res.CompanyNo = results[1];
                Res.CompanyName = results[2];
                Res.ProvidentFundNo = results[3];
                Res.Bank = results[4];
                Res.OpenTime = DateTime.ParseExact(results[5], "yyyy年MM月dd日", CultureInfo.InvariantCulture).ToString(Consts.DateFormatString2);
                Res.LastProvidentFundTime = DateTime.ParseExact(results[6], "yyyy年MM月", CultureInfo.InvariantCulture).ToString(Consts.DateFormatString7);
                Res.SalaryBase = results[7].ToDecimal(0);
                string[] payrate = results[8].Split('/');
                if (payrate.Length == 2)
                {
                    Res.CompanyMonthPayRate = (payrate[0].Replace("单位", "").Replace("%", "").ToDecimal(0)) * 0.01M;
                    Res.PersonalMonthPayRate = (payrate[1].Replace("个人", "").Replace("%", "").ToDecimal(0)) * 0.01M;
                    Res.CompanyMonthPayAmount = Math.Round(Res.SalaryBase * Res.CompanyMonthPayRate, 2);
                    Res.PersonalMonthPayAmount = Math.Round(Res.SalaryBase * Res.PersonalMonthPayRate, 2);
                }
                Res.TotalAmount = results[14].ToDecimal(0);
                Res.Status = results[15];
                Res.IdentityCard = fundReq.Identitycard;
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
                url = "http://www.sxgjj.com/seach/Gerendnmx.aspx";
                httpItem = new HttpItem()
                {
                    URL = url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='ctl00_ContentPlaceHolder1_GridView1']/tr[position()>1]", "inner");
                //上年
                url = "http://www.sxgjj.com/seach/Gerensnmx.aspx";
                httpItem = new HttpItem()
                {
                    URL = url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='ctl00_ContentPlaceHolder1_GridView1']/tr[position()>1]", "inner"));
                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text",true);
                    if (tdRow.Count<6)
                    {
                        continue;
                    }
                    ProvidentFundDetail detail = new ProvidentFundDetail();
                    detail.PayTime = tdRow[0].ToDateTime(Consts.DateFormatString2);
                    detail.Description = tdRow[1];
                    if (tdRow[1].IndexOf("汇缴", StringComparison.Ordinal) > -1 || tdRow[1].Trim()=="补缴")
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        if (detail.PayTime != null)
                            detail.ProvidentFundTime = tdRow[1].Trim() == "补缴" ? ((DateTime)detail.PayTime).ToString(Consts.DateFormatString7) : tdRow[1].Replace("汇缴", "");
                        detail.PersonalPayAmount = Math.Round(tdRow[3].ToDecimal(0) * perAccounting,2);
                        detail.CompanyPayAmount = Math.Round(tdRow[3].ToDecimal(0) * comAccounting, 2);
                        detail.ProvidentFundBase = Math.Round(tdRow[3].ToDecimal(0) / totalRate,2);
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
