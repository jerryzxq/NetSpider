using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Ext;
using System.Collections;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SD
{
    public class weifang : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.wfgjj.gov.cn/";
        string fundCity = "sd_weifang";
        #endregion
        #region  私有变量

        private string errorMsg = string.Empty;
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            cookies = null;
            httpItem = null;
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {

                Url = baseUrl + "captcha.svl?d=Fri Aug 28 2015 17:41:59 GMT+0800";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.Number);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
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
                //15位或18位身份证验证
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (regex.IsMatch(fundReq.Identitycard) == false)
                {
                    Res.StatusDescription = "请输入正确的18位身份证号";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Password))
                {
                    Res.StatusDescription = "请输入6位密码";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region  第一步
                Url = baseUrl + "personal/personLogin.jspx";
                postdata = string.Format("idNo={0}&password={1}&captcha={2}", fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
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

                errorMsg = CommonFun.GetMidStr(httpResult.Html, "</script><script>alert(\"", "\");</script></body></html>");
                if (!string.IsNullOrEmpty(errorMsg) && !errorMsg.Contains("alert"))
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


                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tab_list']/tr[1]/td[1]", "");

                if (results.Count > 0)
                {
                    Res.Name = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tab_list']/tr[1]/td[2]", "");
                if (results.Count == 0)
                {
                    Res.StatusDescription = "请输入正确的18位身份证号";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (results.Count > 0)
                {
                    Res.IdentityCard = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tab_list']/tr[2]/td[1]", "");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tab_list']/tr[2]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.CompanyName = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tab_list']/tr[3]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = Regex.Replace(results[0], @"[/\&nbsp;\s]", "").Trim('%').ToDecimal(0) / 100;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tab_list']/tr[4]/td[1]", "");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = Regex.Replace(results[0], @"[/\&nbsp;\s]", "").Trim('%').ToDecimal(0) / 100;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tab_list']/tr[5]/td[1]", "");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = Math.Round(Regex.Replace(results[0], @"[/\&nbsp;\s]", "").Trim('元').ToDecimal(0) * Res.PersonalMonthPayRate / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate), 2);
                    Res.CompanyMonthPayAmount = Math.Round(Regex.Replace(results[0], @"[/\&nbsp;\s]", "").Trim('元').ToDecimal(0) * Res.CompanyMonthPayRate / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate), 2);
                    Res.SalaryBase = Res.PersonalMonthPayAmount / Res.PersonalMonthPayRate;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tab_list']/tr[5]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.TotalAmount = Regex.Replace(results[0], @"[/\&nbsp;\s]", "").Trim('元').ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tab_list']/tr[6]/td[1]", "");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tab_list']/tr[6]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.Status = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tab_list']/tr[7]/td[1]", "");
                if (results.Count > 0)
                {
                    Res.OpenTime = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tab_list']/tr[7]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.CompanyDistrict = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tab_list']/tr[8]/td[1]", "");
                if (results.Count > 0)
                {
                    Res.Phone = Regex.Replace(results[0], @"[/\&nbsp;\s]", "");
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region   第二步
                Url = baseUrl + string.Format("site_view_getOnly.jspx");
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region 第三步 明细

                int j = 0;
                List<string> Year = new List<string>();
                for (int i = 2010; i <= DateTime.Now.Year; i++)
                {
                    Year.Add(i.ToString());
                }
                //http://www.wfgjj.gov.cn/personal/personDetail.jspx
                foreach (string year in Year)
                {
                    Url = baseUrl + string.Format("personal/personDetail.jspx?year={0}&spcode={1}", year, Res.ProvidentFundNo);
                    httpItem = new HttpItem()
                    {
                        Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                        Method = "POST",
                        URL = Url,
                        Referer = "http://www.wfgjj.gov.cn/personal/personDetail.jspx?year=2015&spcode=211464890",
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
                    results = HtmlParser.GetResultFromParser(httpResult.Html,"//table[@class='cha_list']/tr[position()>1]", "inner");
                    foreach (string item in results)
                    {
                        j++;
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                        if (tdRow.Count != 7)
                        {
                            continue;
                        }
                        detail = new ProvidentFundDetail();
                        detail.PayTime = Regex.Replace(tdRow[1], @"[/\&nbsp;\s]", "").ToDateTime();
                        if (tdRow[3].IndexOf("汇缴") != -1)
                        {
                            detail.ProvidentFundTime = Regex.Replace(tdRow[2], @"[/\&nbsp;\s]", "").Replace("-", "");
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            detail.PersonalPayAmount = ((decimal.Parse(Regex.Replace(tdRow[4], @"[/\&nbsp;\s]", "")) /
                                                        (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)) *
                                                       Res.PersonalMonthPayRate).ToString("f2").ToDecimal(0); //金额
                            detail.CompanyPayAmount = ((decimal.Parse(Regex.Replace(tdRow[4], @"[/\&nbsp;\s]", "")) /
                                                       (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)) *
                                                      Res.CompanyMonthPayRate).ToString("f2").ToDecimal(0); //金额
                            detail.ProvidentFundBase = (detail.PersonalPayAmount / Res.PersonalMonthPayRate).ToString("f2").ToDecimal(0); //缴费基数
                            PaymentMonths++;
                        }
                        else if (tdRow[3].IndexOf("补缴") != -1)
                        {
                            detail.ProvidentFundTime = Regex.Replace(tdRow[2], @"[/\&nbsp;\s]", "").Replace("-", "");
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Back;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Back;
                            detail.CompanyPayAmount = (decimal.Parse(Regex.Replace(tdRow[4], @"[/\&nbsp;\s]", "")));
                            detail.Description = tdRow[3];
                            //if (Res.Description.IsEmpty())
                            //{
                            //    Res.Description = "有断缴、补缴、请人工校验";
                            //}
                        }
                        else if (tdRow[3].IndexOf("结息") != -1)
                        {
                            detail.ProvidentFundTime = Regex.Replace(tdRow[2], @"[/\&nbsp;\s]", "").Replace("-", "");
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.PersonalPayAmount = (decimal.Parse(Regex.Replace(tdRow[4], @"[/\&nbsp;\s]", "")));
                            detail.Description = tdRow[3];
                        }
                        else if (tdRow[3].Contains("个人补欠缴"))
                        {
                            detail.ProvidentFundTime = Regex.Replace(tdRow[2], @"[/\&nbsp;\s]", "").Replace("-", "");
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.PersonalPayAmount = (decimal.Parse(Regex.Replace(tdRow[4], @"[/\&nbsp;\s]", "")));
                            detail.Description = tdRow[3];
                        }
                        else
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.Description = tdRow[3];
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
    }
}
