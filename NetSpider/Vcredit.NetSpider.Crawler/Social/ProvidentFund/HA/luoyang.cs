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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HA
{
    public class luoyang : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.lyzfgjj.com/";
        string fundCity = "ha_luoyang";
        #endregion
        #region  私有变量

        private string __EVENTVALIDATION = string.Empty;
        private string __VIEWSTATE = string.Empty;
        private string dtpick1 = string.Empty;
        private string dtpick2 = string.Empty;
        #endregion
        /// <summary>
        /// 解析验证码
        /// </summary>
        /// <returns></returns>
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
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
            string number = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection) CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                if (!fundReq.Username.IsEmpty())
                {
                    number = fundReq.Username;
                }
                if (!fundReq.Name.IsEmpty())
                {
                    number = fundReq.Name;
                }
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || number.IsEmpty())
                {
                    Res.StatusDescription = "身份证号或公积金或姓名不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string errorMsg = string.Empty;
                #region 第一步

                Url =baseUrl+string.Format("zxcx.aspx?userid={0}&sfz={1}&lmk=",number.ToUrlEncode(Encoding.GetEncoding("utf-8")),fundReq.Identitycard);
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//ul/li/span", "inner");
                if (results.Count > 0)
                {
                    errorMsg = CommonFun.GetMidStr(results[0], "", "<li>");
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        Res.StatusDescription = errorMsg;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                Res.IdentityCard = fundReq.Identitycard;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table4']/tbody/tr[1]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table4']/tbody/tr[1]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.Status = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table4']/tbody/tr[2]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table4']/tbody/tr[3]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table4']/tbody/tr[4]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = results[1].ToDecimal(0);
                    Res.PersonalMonthPayAmount = results[2].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table4']/tbody/tr[5]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate =results[0].ToDecimal(0)/100;
                    
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table4']/tbody/tr[5]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = results[0].ToDecimal(0) / 100;
                    Res.SalaryBase = Math.Round(Res.PersonalMonthPayAmount / Res.PersonalMonthPayRate,2);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table4']/tbody/tr[6]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='__EVENTVALIDATION']", "value");
                if (results.Count > 0)
                {
                   __EVENTVALIDATION = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='__VIEWSTATE']", "value");
                if (results.Count > 0)
                {
                    __VIEWSTATE = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='dtpick1']", "value");
                if (results.Count > 0)
                {
                    dtpick1 = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='dtpick2']", "value");
                if (results.Count > 0)
                {
                    dtpick2 = results[0];
                }
                #endregion

                #region  第二步  获取明细  ==官网上不能点击分页，分页数据未完成==

                Url =baseUrl+
                    string.Format("zxcx.aspx?userid=%u4e8e%u4f1a%u6c11&sfz=410327196702209651&lmk=");
                postdata=string.Format("ScriptManager1=ScriptManager1|ImageButton1&__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE={0}&__EVENTVALIDATION={1}&dtpick1={2}&dtpick2={3}&HiddenField1=&ImageButton1.x=8&ImageButton1.y=11",__VIEWSTATE.ToUrlEncode(Encoding.GetEncoding("utf-8")),__EVENTVALIDATION.ToUrlEncode(Encoding.GetEncoding("utf-8")),dtpick1,dtpick2);
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
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr[1]/td/font", "inner");
                if (results.Count > 0)
                {
                    errorMsg = results[0];
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        Res.StatusDescription = errorMsg;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='objWebDataWindowControl1_datawindow']/tr[position()>1]", "inner");
                int i = 0;
                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td/table/tr/td", "", true);
                    if (tdRow.Count != 7)
                    {
                        continue;
                    }
                    detail = new ProvidentFundDetail();

                    detail.PayTime = tdRow[1].ToDateTime();

                    if (tdRow[2].IndexOf("汇缴") != -1)
                    {
                        detail.ProvidentFundTime = tdRow[6].Replace("-","");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = Math.Round(((HtmlParser.GetResultFromParser(tdRow[3], "input[@name='compute_3_"+i.ToString()+"']", "value")[0].ToDecimal(0)) / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)) * Res.PersonalMonthPayRate,2);//金额
                        detail.CompanyPayAmount = Math.Round(((HtmlParser.GetResultFromParser(tdRow[3], "input[@name='compute_3_"+i.ToString()+"']", "value")[0].ToDecimal(0)) / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)) * Res.CompanyMonthPayRate,2);//金额
                        detail.ProvidentFundBase = Math.Round(detail.PersonalPayAmount / Res.PersonalMonthPayRate,2);//缴费基数
                        PaymentMonths++;
                    }
                    else if (tdRow[2].IndexOf("单位补缴") != -1)
                    {
                        detail.ProvidentFundTime = tdRow[6].Replace("-", "");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = Math.Round(((HtmlParser.GetResultFromParser(tdRow[3], "input[@name='compute_3_" + i.ToString() + "']", "value")[0].ToDecimal(0)) / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)) * Res.PersonalMonthPayRate, 2);//金额
                        detail.CompanyPayAmount = Math.Round(((HtmlParser.GetResultFromParser(tdRow[3], "input[@name='compute_3_" + i.ToString() + "']", "value")[0].ToDecimal(0)) / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)) * Res.CompanyMonthPayRate, 2);//金额
                        detail.ProvidentFundBase = Math.Round(detail.PersonalPayAmount / Res.PersonalMonthPayRate, 2);//缴费基数
                        PaymentMonths++;

                    }
                    else if (tdRow[2].IndexOf("结息") != -1)
                    {
                        detail.ProvidentFundTime = tdRow[6].Replace("-","");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;

                        detail.PersonalPayAmount = HtmlParser.GetResultFromParser(tdRow[3], "input[@name='compute_3_" + i.ToString() + "']", "value")[0].ToDecimal(0); //金额
                        detail.Description = tdRow[2];
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.Description = tdRow[2];
                    }
                    i++;
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
            return Res;
        }
    }
}
