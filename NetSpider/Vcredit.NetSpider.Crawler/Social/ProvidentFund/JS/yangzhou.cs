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
using Vcredit.NetSpider.DataAccess;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.JS
{
    public class yangzhou : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://58.220.193.178:8880/";  //http://58.220.193.178:8880/yw/login.asp
        string fundCity = "js_yangzhou";
        #endregion


        //解析验证码
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
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            ProvidentFundDetail detail = null;
            int PaymentMonths = 0;
            string errorMsg = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                if (fundReq.Identitycard.IsEmpty() && fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription =ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region  第一步
                //http://58.220.193.178:8880/yw/login.asp?username=5&password=5
                Url = baseUrl + string.Format("yw/login.asp?username={0}&password={1}", fundReq.Identitycard, fundReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    //Encoding = Encoding.GetEncoding("gbk"),
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//html/body/div[2]/div/div[2]/div/div/div[2]/table/tbody/tr[1]/td[2]", "inner");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                else
                {
                    Res.StatusDescription ="无公积金信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//html/body/div[2]/div/div[2]/div/div/div[2]/table/tbody/tr[1]/td[4]", "inner");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//html/body/div[2]/div/div[2]/div/div/div[2]/table/tbody/tr[2]/td[2]", "inner");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//html/body/div[2]/div/div[2]/div/div/div[2]/table/tbody/tr[2]/td[4]", "inner");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//html/body/div[2]/div/div[2]/div/div/div[2]/table/tbody/tr[3]/td[2]", "inner");
                if (results.Count > 0)
                {
                    Res.Status = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//html/body/div[2]/div/div[2]/div/div/div[2]/table/tbody/tr[3]/td[4]", "inner");
                if (results.Count > 0)
                {

                    Res.PersonalMonthPayRate = Res.CompanyMonthPayRate= decimal.Parse(CommonFun.GetMidStr(results[0], "", "%")) / 100;
                    if (Res.PersonalMonthPayRate<=0)
                    {
                        Res.PersonalMonthPayRate = Res.CompanyMonthPayRate = payRate;
                    }
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//html/body/div[2]/div/div[2]/div/div/div[2]/table/tbody/tr[6]/td[2]", "inner");
                if (results.Count > 0)
                {

                    Res.TotalAmount = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//html/body/div[2]/div/div[2]/div/div/div[2]/table/tbody/tr[4]/td[2]", "inner");
                if (results.Count > 0)
                {
                    decimal pay = decimal.Parse(results[0]);
                    Res.SalaryBase = Math.Round(pay / (Res.PersonalMonthPayRate * 2), 2);
                    Res.PersonalMonthPayAmount = Res.CompanyMonthPayAmount = Math.Round(Res.SalaryBase * Res.PersonalMonthPayRate, 2);
                }
                //Res.PaymentMonths = PaymentMonths;
                #endregion



                #region 第二步
                List<string> Years = new List<string>();
                Url = baseUrl + "yw/detail_p.asp";
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    //Referer = baseUrl + "yw/main_p.asp",
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
                Years = HtmlParser.GetResultFromParser(httpResult.Html, "//select/option", "inner");
                #endregion

            
                #region  第三步   ==获取明细==   数据不够，无法显示分页

                foreach (string year in Years)
                {
                    int i = 1;
                    while (true)
                    {

                        Url = baseUrl + string.Format("yw/detail_p.asp?year={0}&PageNo={1}", year, i);
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "GET",
                            CookieCollection = cookies,
                            //Referer = baseUrl + "yw/main_p.asp",
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
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='content']/table/tbody/tr", "");
                        if (results.Count == 0) break;
                        i++;
                        foreach (string item in results)
                        {
                            var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                            if (tdRow.Count != 12)
                            {
                                continue;
                            }
                            detail = new ProvidentFundDetail();
                            detail.PayTime = tdRow[0].ToDateTime(Consts.DateFormatString5);
                            if (tdRow[1].IndexOf("汇缴") != -1 || tdRow[1].IndexOf("单位补缴") != -1)
                            {
                                detail.ProvidentFundTime = tdRow[2];
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                                //detail.PersonalPayAmount = Math.Round((tdRow[3].ToDecimal(0)/ (payRate + Res.PersonalMonthPayRate)) * Res.PersonalMonthPayRate, 2);//金额
                                //detail.CompanyPayAmount = Math.Round((tdRow[3].ToDecimal(0) / (Res.PersonalMonthPayRate + payRate)) * payRate, 2);//金额
                                //detail.ProvidentFundBase = Math.Round((detail.PersonalPayAmount / Res.PersonalMonthPayRate),2);//缴费基数
                                detail.PersonalPayAmount = detail.CompanyPayAmount = Math.Round(tdRow[3].ToDecimal(0) / 2, 2);
                                detail.ProvidentFundBase = Math.Round(
                                    detail.PersonalPayAmount/Res.PersonalMonthPayRate, 2);
                                detail.Description = tdRow[1];
                                PaymentMonths++;
                            }
                            else if (tdRow[1].IndexOf("个人补缴") != -1 || tdRow[1].IndexOf("结息")!=-1)
                            {
                                detail.ProvidentFundTime = tdRow[2];
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                                detail.PersonalPayAmount = tdRow[3].ToDecimal(0);//金额
                                detail.Description = tdRow[1];
                                PaymentMonths++;
                            }
                                //判断为正常缴费 修改时间：2015年12月30日10:16:09
                            //else if (tdRow[1].IndexOf("单位补缴") != -1)
                            //{
                            //    detail.ProvidentFundTime = tdRow[2];
                            //    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            //    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            //    detail.CompanyPayAmount = tdRow[3].ToDecimal(0);//金额
                            //    detail.Description = tdRow[1];
                            //}
                            else if (tdRow[1].IndexOf("支付") != -1)
                            {
                                detail.ProvidentFundTime = tdRow[2];

                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                                detail.PersonalPayAmount = tdRow[4].ToDecimal(0);//金额
                                detail.Description = tdRow[1];
                            }
                            else
                            {
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                                detail.Description = tdRow[1];
                                detail.PersonalPayAmount = tdRow[3].ToDecimal(0);
                            }
                            Res.ProvidentFundDetailList.Add(detail);
                        }
                    }
                }
                //缴存基数调整查询 (没有分页测试账号，只查询了1页)
                //Url = baseUrl + string.Format("yw/jishu_p.asp");
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "GET",
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='sample-table-sortable']/tbody/tr", "inner");
                //foreach (var item in results)
                //{
                //    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                //    if (tdRow.Count != 7)
                //    {
                //        continue;
                //    }
                //    detail = new ProvidentFundDetail();
                //    detail.CompanyName = tdRow[0];
                //    detail.PayTime = tdRow[1].ToDateTime(Consts.DateFormatString);
                //    detail.ProvidentFundTime = Convert.ToDateTime(detail.PayTime).ToString("yyyyMM");
                //    detail.PersonalPayAmount = tdRow[4].ToDecimal(0) + tdRow[6].ToDecimal(0);
                //    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                //    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                //    Res.ProvidentFundDetailList.Add(detail);
                //}
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
