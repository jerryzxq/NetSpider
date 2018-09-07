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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.GX
{
    public class hezhou : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.gxhzgjj.com:7001/";
        string fundCity = "gx_hezhou";
        #endregion


        #region  私有变量
        string cxyd = string.Empty;
        string zgzh = string.Empty;
        string sfzh = string.Empty;
        string zgxm = string.Empty;
        string dwbm = string.Empty;
        string errorMsg = string.Empty;
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
                    Res.StatusDescription = "身份证号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }



                #region 第一步
                Url = baseUrl + "hzgjjweb/zfbzgl/zfbzsq/login.jsp";
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='cxydmc']", "value");
                if(results.Count>0)
                {
                    cxyd = results[0];
                }
                #endregion
                #region 第二步
                 Url = baseUrl +string.Format("hzgjjweb/zfbzgl/zfbzsq/login_hidden.jsp?password={0}&sfzh={1}&cxyd={2}",fundReq.Password,fundReq.Identitycard,cxyd.ToUrlEncode(Encoding.GetEncoding("gbk")));
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
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\");");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
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
                #endregion


                #region  第三步
                Url = baseUrl + string.Format("hzgjjweb/zfbzgl/zfbzsq/main_menu.jsp?zgzh={0}&sfzh={1}&zgxm={2}&dwbm={3}&cxyd={4}", zgzh, sfzh, zgxm.ToUrlEncode(Encoding.GetEncoding("gbk")), dwbm, cxyd.ToUrlEncode(Encoding.GetEncoding("gbk")));
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[1]/td/table[4]/tr/td/table/tr/td/table/tr[1]/td[4]/font", "");
                if(results.Count>0)
                {
                    Res.Status = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[1]/td/table[4]/tr/td/table/tr/td/table/tr[1]/td[2]/font", "");
                if (results.Count > 0)
                {
                    Res.Name = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[1]/td/table[4]/tr/td/table/tr/td/table/tr[2]/td[2]/font", "");
                if (results.Count > 0)
                {
                    Res.IdentityCard = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[1]/td/table[4]/tr/td/table/tr/td/table/tr[2]/td[4]/font", "");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[1]/td/table[4]/tr/td/table/tr/td/table/tr[3]/td[2]/font", "");
                if (results.Count > 0)
                {
                    Res.CompanyName = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[1]/td/table[4]/tr/td/table/tr/td/table/tr[3]/td[4]/font", "");
                if (results.Count > 0)
                {
                    Res.ProvidentFundCity = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[1]/td/table[4]/tr/td/table/tr/td/table/tr[4]/td[2]/font", "");
                if (results.Count > 0)
                {
                    Res.BankCardNo = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[1]/td/table[4]/tr/td/table/tr/td/table/tr[4]/td[4]/font", "");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = Regex.Replace(results[0], @"[/\&nbsp;\s]", "").ToDecimal(0)/2;
                    Res.CompanyMonthPayAmount = Regex.Replace(results[0], @"[/\&nbsp;\s]", "").ToDecimal(0) / 2;
                    Res.SalaryBase = Math.Round(Res.PersonalMonthPayAmount/payRate,2);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[1]/td/table[4]/tr/td/table/tr/td/table/tr[8]/td[2]/font", "");
                if (results.Count > 0)
                {
                    Res.TotalAmount = Regex.Replace(results[0], @"[/\&nbsp;\s]", "").ToDecimal(0);
                }
                ////sfzh = CommonFun.GetMidStr(httpResult.Html, "", "");
                #endregion


                #region  第四步
                Url = baseUrl + string.Format("hzgjjweb/zfbzgl/gjjmxcx/gjjmx_cx.jsp?sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd={4}", sfzh, zgxm.ToUrlEncode(Encoding.GetEncoding("gbk")), zgzh, dwbm, cxyd.ToUrlEncode(Encoding.GetEncoding("gbk")));
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[position()>1]", "");
                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                    if (tdRow.Count != 6)
                    {
                        continue;
                    }
                    detail = new ProvidentFundDetail();
                    detail.PayTime = tdRow[0].ToDateTime();
                    if (tdRow[1].IndexOf("汇缴") != -1)
                    {
                        detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[1], "缴", "月").Replace("年","");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0);//金额
                        detail.ProvidentFundBase = Math.Round(detail.PersonalPayAmount / payRate,2);//缴费基数
                        detail.Description = tdRow[1];
                        PaymentMonths++;
                    }
                    else if (tdRow[1].IndexOf("补缴") != -1)
                    {
                        detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[1], "缴", "月").Replace("年","");;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.CompanyPayAmount = tdRow[3].ToDecimal(0);//金额
                        detail.Description = tdRow[1];
                    }
                    else if (tdRow[1].IndexOf("结息") != -1 || tdRow[1].IndexOf("合计")!=-1)
                    {
                        
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0) + tdRow[2].ToDecimal(0);//金额
                        detail.Description = tdRow[1];
                    }
                    else if (tdRow[1].IndexOf("购买") != -1)
                    {
                       
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0) + tdRow[2].ToDecimal(0);//金额
                        detail.Description = tdRow[1];
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.Description = tdRow[1];
                        detail.PersonalPayAmount = 0;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
              
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
