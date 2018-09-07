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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.JS
{
    public class nantong : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://58.221.92.100:8889/";
        string fundCity = "js_nantong";
        #endregion

        #region 私有变量
        string errorMsg = string.Empty;
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
                Url = "http://www.ntgjj.com/gjjcx.aspx?UrlOneClass=78";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "jcaptcha?onlynum=true";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);

                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
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
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {

                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (string.IsNullOrWhiteSpace(fundReq.Identitycard))
                {
                    Res.StatusDescription = "请输入身份证号";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Password))
                {
                    Res.StatusDescription = "请输入密码";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region  第一步

                Url = string.Format("http://58.221.92.100:8889/searchPersonLogon.do?spidno={0}&sppassword={1}&rand={2}", fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
                //Url = string.Format("http://www.ntgjj.com/gjjcx1.aspx?UrlOneClass=78&spidno={0}&sppassword={1}&typeId=1", GetBase64string(fundReq.Identitycard), GetBase64string(fundReq.Password));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    //Encoding = Encoding.GetEncoding("utf-8"),
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
                if (!httpResult.Html.Contains("searchMain.do"))
                {
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//script");
                    if (results.Count > 0)
                    {
                        string msg = CommonFun.GetMidStr(results[0], "alert('", "'");
                        if (string.IsNullOrWhiteSpace(msg))
                        {
                            msg = "登录失败，请重试";
                        }
                        Res.StatusDescription = msg;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region  第二步
                //Url = baseUrl + string.Format("searchPersonLogon.do?spidno={0}&sppassword={1}", fundReq.Identitycard, fundReq.Password);//
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "get",
                //    Encoding = Encoding.GetEncoding("utf-8"),
                //    Referer = "http://www.ntgjj.com/gjjcx1.aspx?UrlOneClass=78&spidno=MzIwNjIzMTk3MzExMDk4MjQ5&sppassword=MDAwMDA=&typeId=1",
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //errorMsg = CommonFun.GetMidStr(httpResult.Html, "<script> alert('", "');</script>");
                //if (!string.IsNullOrEmpty(errorMsg))
                //{
                //    Res.StatusDescription = errorMsg;
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                ////请求失败后返回
                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                //    return Res;
                //}
                #endregion

                #region   第三步
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //2015-08-11T13:42:38.472+0800
                Url = baseUrl + "searchMenuView.do";
                postdata = "MENUCODE=0&node=ynode-7";
                httpItem = new HttpItem
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
                #endregion

                #region  第四步
                Url = baseUrl + "searchGrye.do?logon=2015-08-11T14:37:07.762+0800";
                httpItem = new HttpItem
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[1]/td[1]", "");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[1]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[2]/td[1]", "");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[2]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[3]/td[1]", "");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[3]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.SalaryBase = results[0].Replace("&nbsp;元", "").ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[4]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.TotalAmount = CommonFun.GetMidStr(Regex.Replace(results[0], @"[/\&nbsp;\s]", ""), "", "元").ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[4]/td[1]", "");
                if (results.Count > 0)
                {
                    decimal pay = CommonFun.GetMidStr(Regex.Replace(results[0], @"[/\&nbsp;\s]", ""), "", "元").ToDecimal(0);
                    Res.PersonalMonthPayAmount = pay / 2;
                    Res.CompanyMonthPayAmount = pay / 2;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[6]/td[1]", "");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[6]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.Status = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                Res.PersonalMonthPayRate = Res.PersonalMonthPayAmount/Res.SalaryBase;
                Res.CompanyMonthPayRate = Res.CompanyMonthPayAmount/Res.SalaryBase;
                #endregion


                #region  第四步  获取年份
                Url = baseUrl + "searchGrmx.do?logon=2015-08-11T15:31:51.074+0800";
                List<string> Years = new List<string>();
                httpItem = new HttpItem
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
                Years = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@ name='select']/option", "");
                #endregion

                #region 第五步  获取公积金明细
                foreach (string year in Years)
                {
                    Url = baseUrl + string.Format("searchGrmx.do?year={0}", year);
                    httpItem = new HttpItem
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
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju']/tr[position()>1]", "");
                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                        if (tdRow.Count != 10)
                        {
                            continue;
                        }
                        detail = new ProvidentFundDetail();
                        detail.PayTime = Regex.Replace(tdRow[1], @"[/\&nbsp;\s]", "").ToDateTime();
                        if (tdRow[3].IndexOf("汇缴") != -1)
                        {
                            detail.ProvidentFundTime = Regex.Replace(tdRow[2], @"[/\&nbsp;\s]", "").Replace("-","");
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            detail.PersonalPayAmount = Regex.Replace(tdRow[4], @"[/\&nbsp;\s]", "").ToDecimal(0) / 2;//金额
                            detail.CompanyPayAmount = Regex.Replace(tdRow[4], @"[/\&nbsp;\s]", "").ToDecimal(0) / 2;//金额
                            detail.ProvidentFundBase = (detail.PersonalPayAmount / Res.PersonalMonthPayRate).ToString("f2").ToDecimal(0);//缴费基数
                            PaymentMonths++;
                        }
                        else if (tdRow[3].IndexOf("补缴") != -1)
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.Description = Regex.Replace(tdRow[3], @"[/\&nbsp;\s]", "");
                            detail.CompanyPayAmount = Regex.Replace(tdRow[4], @"[/\&nbsp;\s]", "").ToDecimal(0);//金额
                        }
                        else if (tdRow[3].IndexOf("结息") != -1)
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.Description = Regex.Replace(tdRow[3], @"[/\&nbsp;\s]", "");
                            detail.PersonalPayAmount = Regex.Replace(tdRow[4], @"[/\&nbsp;\s]", "").ToDecimal(0);//金额
                        }
                        else
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.Description = Regex.Replace(tdRow[3], @"[/\&nbsp;\s]", "");

                        }
                        Res.ProvidentFundDetailList.Add(detail);
                    }
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

        public string GetBase64string(string toString)
        {
            byte[] bytes = Encoding.Default.GetBytes(toString);
            string str = Convert.ToBase64String(bytes);
            return str;
        }
    }
}
