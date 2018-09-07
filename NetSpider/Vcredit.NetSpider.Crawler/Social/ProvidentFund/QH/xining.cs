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
using System.Collections;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.QH
{
    public class xining : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
       
        string baseUrl = "http://218.95.230.60/wscx/";
        //string baseUrl = "http://www.xngjj.gov.cn/wscx_hy/";
        string fundCity = "qh_xining";
        #endregion

        #region 私有变量
        string dbname = string.Empty;
        string errorMsg = string.Empty;
        string zgzh = string.Empty;
        string sfzh = string.Empty;
        string zgxm = string.Empty;
        string dwbm = string.Empty;
        string cxydmc = string.Empty;
        private string dlfs = string.Empty;
        private string zgzt = string.Empty;
        private string yss = string.Empty;
        private string totalpages = string.Empty;
        #endregion

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
                //CacheHelper.SetCache(token, cookies);
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
            try
            {
                //获取缓存
                //if (CacheHelper.GetCache(fundReq.Token) != null)
                //{
                //    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                //    CacheHelper.RemoveCache(fundReq.Token);
                //}
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription ="身份证号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Regex reg = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (reg.IsMatch(fundReq.Identitycard)==false)
                {
                    Res.StatusDescription = "请输入15位或18位有效身份证号码";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步
                //http://218.95.230.60/wscx/zfbzgl/zfbzsq/login.jsp
                Url = baseUrl + "zfbzgl/zfbzsq/login.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
                List<string> Years = new List<string>();
                //Years.Add("当前年度");
                //Years = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@name='cxyd']/option", "");
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dbname']", "value");
                if (results.Count > 0)
                {
                    dbname = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='cxydmc']", "value");
                if (results.Count > 0)
                {
                    cxydmc = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@name='dlfs']/option", "value");
                if (results.Count > 0)
                {
                    dlfs = results[0];
                }
                #endregion


                //foreach (string year in Years)
                //{
                    #region 第二步

                //http://218.95.230.60/wscx/zfbzgl/zfbzsq/login_hidden.jsp?password=111111&sfzh=630103197611122036&cxyd=&dbname=wasys350&dlfs=0
                    //Url = baseUrl + string.Format("zfbzgl/zfbzsq/login_hidden.jsp?password={0}&sfzh={1}&cxyd={2}&dbname={3}", fundReq.Password, fundReq.Identitycard, year, dbname);
                Url = baseUrl +
                      string.Format(
                          "zfbzgl/zfbzsq/login_hidden.jsp?password={0}&sfzh={1}&cxyd=&dbname={2}&dlfs=0",fundReq.Password,fundReq.Identitycard,dbname);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    errorMsg = CommonFun.GetMidStr(httpResult.Html, ">alert(\"", "\");");
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        Res.StatusDescription = errorMsg;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    //请求失败后返回
                    if (httpResult.StatusCode != HttpStatusCode.OK)
                    {
                        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                        Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                        return Res;
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgzh']", "value");
                    if (results.Count > 0)
                    {
                        zgzh = results[0];
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='sfzh']", "value");
                    if (results.Count > 0)
                    {
                        sfzh = results[0];
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgxm']", "value");
                    if (results.Count > 0)
                    {
                        zgxm = results[0];
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dwbm']", "value");
                    if (results.Count > 0)
                    {
                        dwbm = results[0];
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgzt']", "value");
                    if (results.Count > 0)
                    {
                        zgzt = results[0];
                    }
                if (string.IsNullOrEmpty(zgzh))
                {
                    Res.StatusDescription = "身份证号或密码不正确";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                    #endregion

                    #region  第三步
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                    //http://218.95.230.60/wscx/zfbzgl/zfbzsq/main_menu.jsp?zgzh=130009990003280&sfzh=630103197611122036&zgxm=%E9%9F%A9**&dwbm=13000000&zgzt=%B7%E2%B4%E6&cxyd=&dbname=wasys350
                   // Url = baseUrl + string.Format("zfbzgl/zfbzsq/main_menu.jsp?zgzh={0}&sfzh={1}&zgxm={2}&dwbm={3}&cxyd={4}&dbname={5}", zgzh, sfzh, zgxm.ToUrlEncode(Encoding.GetEncoding("gbk")), dwbm, 1, dbname);
                Url = baseUrl +
                      string.Format(
                          "zfbzgl/zfbzsq/main_menu.jsp?zgzh={0}&sfzh={1}&zgxm={2}&dwbm={3}&zgzt={4}&cxyd=&dbname={5}",zgzh,sfzh,zgxm,dwbm,zgzt,dbname);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "Get",
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

                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/table/tr/td/table/tr/td/table[1]/tr[1]/td[2]/font", "");
                    if(results.Count>0)
                    {
                        Res.Name = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                    }

                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/table/tr/td/table/tr/td/table[1]/tr[1]/td[4]/font", "");
                    if (results.Count > 0)
                    {
                        Res.BankCardNo = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/table/tr/td/table/tr/td/table[1]/tr[2]/td[2]/font", "");
                    if (results.Count > 0)
                    {
                        Res.IdentityCard = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/table/tr/td/table/tr/td/table[1]/tr[2]/td[4]/font", "");
                    if (results.Count > 0)
                    {
                        Res.ProvidentFundNo = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/table/tr/td/table/tr/td/table[1]/tr[3]/td[2]/font", "");
                    if (results.Count > 0)
                    {
                        Res.CompanyName = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/table/tr/td/table/tr/td/table[1]/tr[3]/td[4]/font", "");
                    if (results.Count > 0)
                    {
                        Res.CompanyDistrict = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/table/tr/td/table/tr/td/table[1]/tr[4]/td[2]/font", "");
                    if (results.Count > 0)
                    {
                        Res.OpenTime = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/table/tr/td/table/tr/td/table[1]/tr[4]/td[4]/font", "");
                    if (results.Count > 0)
                    {
                        Res.Status = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                    }
                   
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/table/tr/td/table/tr/td/table[1]/tr[5]/td[5]/font", "");
                    if (results.Count > 0)
                    {
                        string result = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                        Res.PersonalMonthPayRate = CommonFun.GetMidStr(result, "", "%").ToDecimal(0) / 100;
                        Res.CompanyMonthPayRate = CommonFun.GetMidStr(result, "", "%").ToDecimal(0) / 100;
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/table/tr/td/table/tr/td/table[1]/tr[5]/td[2]/font", "");
                    if (results.Count > 0)
                    {
                        string result = CommonFun.GetMidStr(Regex.Replace(results[0], @"[/\&nbsp;\s]", ""), "", "元");
                        Res.PersonalMonthPayAmount = result.ToDecimal(0) * (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate) / 2;
                        Res.CompanyMonthPayAmount = result.ToDecimal(0) * (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate) / 2;
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/table/tr/td/table/tr/td/table[1]/tr[10]/td[4]/font", "");
                    if (results.Count > 0)
                    {
                        Res.LastProvidentFundTime = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/table/tr/td/table/tr/td/table[1]/tr[9]/td[4]/font", "");
                    if (results.Count > 0)
                    {
                        Res.TotalAmount = CommonFun.GetMidStr(Regex.Replace(results[0], @"[/\&nbsp;\s]", ""), "", "元").ToDecimal(0);
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/table/tr/td/table/tr/td/table[1]/tr[5]/td[2]/font", "");
                    if (results.Count > 0)
                    {
                        Res.SalaryBase = CommonFun.GetMidStr(Regex.Replace(results[0], @"[/\&nbsp;\s]", ""), "", "元").ToDecimal(0);
                    }
                    #endregion

                //    #region  第四步
                //    Url = baseUrl + string.Format("zfbzgl/gjjmxcx/gjjmx_cx.jsp");
                //    postdata = string.Format("sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd=&zgzt={4}", sfzh, zgxm.ToUrlEncode(Encoding.GetEncoding("gbk")), zgzh, dwbm, zgzt.ToUrlEncode(Encoding.GetEncoding("gbk")));
                //    httpItem = new HttpItem()
                //    {
                //        URL = Url,
                //        Method = "post",
                //        Postdata =postdata,
                //        CookieCollection = cookies,
                //        ResultCookieType = ResultCookieType.CookieCollection
                //    };
                  
                //    httpResult = httpHelper.GetHtml(httpItem);
                //    //请求失败后返回
                //    if (httpResult.StatusCode != HttpStatusCode.OK)
                //    {
                //        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //        Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                //        return Res;
                //    }
                //    Years = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@name='cxydone']/option", "");

                //    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='yss']", "value");
                //if (results.Count > 0)
                //{
                //    yss = results[0];
                //}
                ////totalpages
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='totalpages']", "value");
                //if (results.Count > 0)
                //{
                //    totalpages = results[0];
                //}
                //    #endregion

                //    #region 第五步  公积金明细
                //    //Url = baseUrl + string.Format("zfbzgl/gjjmxcx/gjjmx_cx.jsp?sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd={4}", sfzh, zgxm.ToUrlEncode(Encoding.GetEncoding("gbk")), zgzh, dwbm,1);
                //foreach (var year in Years)
                //{

                //    if (year != "当前年度")
                //    {
                //       string[] Year = year.Split('-');

                //        if (int.Parse(Year[0]) > 2007)
                //        {
                //            Url = baseUrl + string.Format("zfbzgl/gjjmxcx/gjjmx_cx.jsp");
                //            postdata =
                //                string.Format(
                //                    "cxydone={0}&cxydtwo={1}&yss={2}&totalpages={3}&cxyd=&zgzh={4}&sfzh={5}&zgxm={6}&dwbm={7}&dbname={8}",
                //                    year, year, yss, totalpages, zgzh, sfzh, zgxm, dwbm, dbname);
                //            httpItem = new HttpItem()
                //            {
                //                URL = Url,
                //                Method = "post",
                //                Postdata = postdata,
                //                CookieCollection = cookies,
                //                ResultCookieType = ResultCookieType.CookieCollection
                //            };

                //            httpResult = httpHelper.GetHtml(httpItem);
                //            //请求失败后返回
                //            if (httpResult.StatusCode != HttpStatusCode.OK)
                //            {
                //                string ss = year;
                //                Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //                Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                //                return Res;
                //            }
                //            results = HtmlParser.GetResultFromParser(httpResult.Html,
                //                "//table[@class='1']/tr[position()>1]", "");
                //            foreach (string item in results)
                //            {
                //                var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                //                if (tdRow.Count != 6)
                //                {
                //                    continue;
                //                }
                //                detail = new ProvidentFundDetail();
                //                detail.PayTime = tdRow[0].ToDateTime();
                //                if (tdRow[5].IndexOf("合计") == -1)
                //                {
                //                    detail.ProvidentFundTime = tdRow[0];
                //                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                //                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                //                    detail.PersonalPayAmount = tdRow[3].ToDecimal(0)/2; //金额
                //                    detail.CompanyPayAmount = tdRow[3].ToDecimal(0)/2;
                //                    detail.ProvidentFundBase = (detail.PersonalPayAmount/payRate); //缴费基数
                //                    PaymentMonths++;
                //                }
                //                else
                //                {
                //                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                //                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                //                    detail.Description = tdRow[5];
                //                    detail.PersonalPayAmount = 0;
                //                }
                //                Res.ProvidentFundDetailList.Add(detail);
                //            }

                //        }
                //    }
                //    else
                //    {
                        
                //          Url = baseUrl + string.Format("zfbzgl/gjjmxcx/gjjmx_cx.jsp");
                //            postdata =
                //                string.Format(
                //                    "cxydone={0}&cxydtwo={1}&yss={2}&totalpages={3}&cxyd=&zgzh={4}&sfzh={5}&zgxm={6}&dwbm={7}&dbname={8}",
                //                    year, year, yss, totalpages, zgzh, sfzh, zgxm, dwbm, dbname);
                //            httpItem = new HttpItem()
                //            {
                //                URL = Url,
                //                Method = "post",
                //                Postdata = postdata,
                //                CookieCollection = cookies,
                //                ResultCookieType = ResultCookieType.CookieCollection
                //            };

                //            httpResult = httpHelper.GetHtml(httpItem);
                //            //请求失败后返回
                //            if (httpResult.StatusCode != HttpStatusCode.OK)
                //            {
                //                string ss = year;
                //                Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //                Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                //                return Res;
                //            }
                //            results = HtmlParser.GetResultFromParser(httpResult.Html,
                //                "//table[@class='1']/tr[position()>1]", "");
                //        foreach (string item in results)
                //        {
                //            var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                //            if (tdRow.Count != 6)
                //            {
                //                continue;
                //            }
                //            detail = new ProvidentFundDetail();
                //            detail.PayTime = tdRow[0].ToDateTime();
                //            if (tdRow[5].IndexOf("合计") != -1)
                //            {
                //                detail.ProvidentFundTime = tdRow[0];
                //                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                //                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                //                detail.PersonalPayAmount = tdRow[3].ToDecimal(0)/2; //金额
                //                detail.CompanyPayAmount = tdRow[3].ToDecimal(0)/2;
                //                detail.ProvidentFundBase = (detail.PersonalPayAmount/payRate); //缴费基数
                //                PaymentMonths++;
                //            }
                //            else
                //            {
                //                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                //                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                //                detail.Description = tdRow[5];
                //                detail.PersonalPayAmount = 0;
                //            }
                //            Res.ProvidentFundDetailList.Add(detail);
                //        }
                //    }
                //    #endregion
                //}

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
