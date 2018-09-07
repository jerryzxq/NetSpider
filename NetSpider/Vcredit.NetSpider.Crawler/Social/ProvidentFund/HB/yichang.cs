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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HB
{
    public class yichang : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://219.139.130.121:9000/";
        string fundCity = "hb_yichang";
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
            //http://219.139.130.121:9000/ValidateCode.aspx
                Url = baseUrl + "ValidateCode.aspx";
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
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
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
                if (fundReq.Identitycard.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "身份证号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
              

                #region 第一步登陆
                Url = baseUrl + "ProvidentFund/Login.aspx";
                string errorMsg = string.Empty;
                postdata = string.Format("__VIEWSTATE=%2FwEPDwUKMTY3NzYzNzcyMA8WAh4EY2FyZAUSNDIwNTAzMTk4MTEwMTgxODM2FgJmD2QWAgIDD2QWAgIBD2QWAgIBD2QWBGYPDxYCHgdWaXNpYmxlaGRkAgIPDxYCHwFnZGQYAQUeX19Db250cm9sc1JlcXVpcmVQb3N0QmFja0tleV9fFgEFJmN0bDAwJENvbnRlbnRQbGFjZUhvbGRlcjEkSW1hZ2VCdXR0b24x%2FrkgxqykYmIF2lm%2Fz1vTOXscMq4%3D&__EVENTVALIDATION=%2FwEWBQLJmKHqDQKOz6bYBgLK7bONCgKBi6uODwK9vIn8DFrdMBseM8UbGRQn04pa21qK7uLH&ctl00%24ContentPlaceHolder1%24txt_sfzhm={0}&ctl00%24ContentPlaceHolder1%24txt_pwd={1}&ctl00%24ContentPlaceHolder1%24Validator={2}&ctl00%24ContentPlaceHolder1%24ImageButton1.x=0&ctl00%24ContentPlaceHolder1%24ImageButton1.y=0", fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "post",
                    Postdata=postdata,
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
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');</script></form>");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 第二步
                Url = baseUrl + "requestpage.aspx?type=pf";
                httpItem = new HttpItem
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
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "');window.location.href='ProvidentFund");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }   
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 第二步
                Url = baseUrl + "providentfund/default.aspx";
                httpItem = new HttpItem
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/div/table/tr[2]/td[1]", "");
                if(results.Count>0)
                {
                    Res.ProvidentFundNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/div/table/tr[2]/td[2]", "");
                if(results.Count>0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/div/table/tr[2]/td[3]", "");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/div/table/tr[2]/td[4]", "");
                if(results.Count>0)
                {
                    Res.CompanyMonthPayAmount =decimal.Parse(results[0])/2;
                    Res.PersonalMonthPayAmount = decimal.Parse(results[0]) / 2;

                }
                Res.IdentityCard = fundReq.Identitycard;
                Res.SalaryBase = Math.Round(Res.PersonalMonthPayAmount / payRate,2);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion


                #region  第三步 获取隐藏信息
                string __VIEWSTATE = string.Empty;
                string __EVENTVALIDATION = string.Empty;
                string startYear = string.Empty;
                string startMonth = string.Empty;
                string endYear = string.Empty;
                string endMonth = string.Empty;
                Url = baseUrl + "providentfund/PersonalDetail.aspx";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "get",
                    Encoding=Encoding.GetEncoding("utf-8"),
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                if(results.Count>0)
                {
                    __VIEWSTATE = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                if (results.Count > 0)
                {
                    __EVENTVALIDATION = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@id='ctl00_ContentPlaceHolder1_WebCalendar1_DropDownList1']/option [@selected='selected']", "value");
                if (results.Count > 0)
                {
                   // startYear = results[0];
                    startYear = "1996";
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@id='ctl00_ContentPlaceHolder1_WebCalendar1_DropDownList2']/option [@selected='selected']", "value");
                if (results.Count > 0)
                {
                    //startMonth = results[0];
                    startMonth = "01";
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@id='ctl00_ContentPlaceHolder1_WebCalendar2_DropDownList1']/option [@selected='selected']", "value");
                if (results.Count > 0)
                {
                    endYear = DateTime.Now.Year.ToString();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@id='ctl00_ContentPlaceHolder1_WebCalendar2_DropDownList2']/option [@selected='selected']", "value");
                if (results.Count > 0)
                {
                    //endMonth = results[0];
                    endMonth = DateTime.Now.ToString("MM");
                }
                #endregion

                int j = 0;
                #region 获取第一页数据
                 Url = baseUrl + "providentfund/PersonalDetail.aspx";
                 postdata =string.Format(
                        "__VIEWSTATE={1}&__EVENTTARGET=&__EVENTARGUMENT=&__EVENTVALIDATION={0}&ctl00%24ContentPlaceHolder1%24WebCalendar1%24DropDownList1={2}&ctl00%24ContentPlaceHolder1%24WebCalendar1%24DropDownList2={3}&ctl00%24ContentPlaceHolder1%24WebCalendar2%24DropDownList1={4}&ctl00%24ContentPlaceHolder1%24WebCalendar2%24DropDownList2={5}&ctl00%24ContentPlaceHolder1%24ImageButton1.x=49&ctl00%24ContentPlaceHolder1%24ImageButton1.y=9", __EVENTVALIDATION.ToUrlEncode(Encoding.GetEncoding("utf-8")),__VIEWSTATE.ToUrlEncode(Encoding.GetEncoding("utf-8")),startYear,startMonth,endYear,endMonth);
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Method = "post",
                        Postdata=postdata,
                        Encoding=Encoding.GetEncoding("utf-8"),
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
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                    if (results.Count > 0)
                    {
                        __VIEWSTATE = results[0];
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                    if (results.Count > 0)
                    {
                        __EVENTVALIDATION = results[0];
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='line1']/tr/td[1]/div/table/tr[position()>1]", "");
                   

                   // results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='line1']/tr/td[1]/div/table/tr[position()>1]", "");
                foreach (var item in results)
                {
                    j++;
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                    if (tdRow.Count != 7)
                    {
                        continue;
                    }
                    detail = new ProvidentFundDetail();

                    detail.PayTime = tdRow[0].ToDateTime();
                    //int j = tdRow[4].IndexOf("汇缴");
                    if (tdRow[4].IndexOf("汇缴") != -1)
                    {
                        detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[5], "[", "]").Replace("-","");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = decimal.Parse(Regex.Replace(tdRow[2], @"[/\&nbsp;\s]", ""))/2; //金额
                        detail.CompanyPayAmount = decimal.Parse(Regex.Replace(tdRow[2], @"[/\&nbsp;\s]", ""))/2; //金额
                        detail.CompanyName = tdRow[6];
                        detail.ProvidentFundBase = (detail.PersonalPayAmount/payRate); //缴费基数
                        PaymentMonths++;
                    }
                    else if (tdRow[4].IndexOf("结息") != -1)
                    {
                        detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[5], "[", "]").Replace("-","");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = decimal.Parse(Regex.Replace(tdRow[2], @"[/\&nbsp;\s]", "")); //金额
                        detail.CompanyName = tdRow[6];
                        detail.Description = tdRow[4];
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.CompanyName = tdRow[6];
                        detail.Description = tdRow[4];
                        detail.PersonalPayAmount = decimal.Parse(Regex.Replace(tdRow[2], @"[/\&nbsp;\s]", "")); //金额;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }

                #endregion


                #region  第四步获取明细
                Url = baseUrl + "providentfund/PersonalDetail.aspx";
                int i = 2;
                
                while(true)
                {
                    postdata = string.Format("__VIEWSTATE={0}&__EVENTTARGET=ctl00%24ContentPlaceHolder1%24AspNetPager1&__EVENTARGUMENT={1}&__EVENTVALIDATION={2}&ctl00%24ContentPlaceHolder1%24WebCalendar1%24DropDownList1={3}&ctl00%24ContentPlaceHolder1%24WebCalendar1%24DropDownList2={4}&ctl00%24ContentPlaceHolder1%24WebCalendar2%24DropDownList1={5}&ctl00%24ContentPlaceHolder1%24WebCalendar2%24DropDownList2={6}", __VIEWSTATE.ToUrlEncode(Encoding.GetEncoding("utf-8")), i, __EVENTVALIDATION.ToUrlEncode(Encoding.GetEncoding("utf-8")), startYear, startMonth, endYear, endMonth);
                    //postdata =string.Format("__VIEWSTATE={1}&__EVENTTARGET=&__EVENTARGUMENT={6}&__EVENTVALIDATION={0}&ctl00%24ContentPlaceHolder1%24WebCalendar1%24DropDownList1={2}&ctl00%24ContentPlaceHolder1%24WebCalendar1%24DropDownList2={3}&ctl00%24ContentPlaceHolder1%24WebCalendar2%24DropDownList1={4}&ctl00%24ContentPlaceHolder1%24WebCalendar2%24DropDownList2={5}&ctl00%24ContentPlaceHolder1%24ImageButton1.x=49&ctl00%24ContentPlaceHolder1%24ImageButton1.y=9", __EVENTVALIDATION.ToUrlEncode(Encoding.GetEncoding("utf-8")),__VIEWSTATE.ToUrlEncode(Encoding.GetEncoding("utf-8")),startYear,startMonth,endYear,endMonth,i);
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Method = "post",
                        Postdata=postdata,
                        Encoding=Encoding.GetEncoding("utf-8"),
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
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                    if (results.Count > 0)
                    {
                        __VIEWSTATE = results[0];
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                    if (results.Count > 0)
                    {
                        __EVENTVALIDATION = results[0];
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='line1']/tr/td[1]/div/table/tr[position()>1]", "");
                    if (results.Count== 0)
                    {
                        break;
                    }
                    string count = CommonFun.GetMidStr(httpResult.Html, "共", "页");
                    int Pagecount = int.Parse(count);
                    if (i > Pagecount) break;
                    i++;
                   // results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='line1']/tr/td[1]/div/table/tr[position()>1]", "");
                    foreach(var item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                        if (tdRow.Count != 7)
                        {
                            continue;
                        }
                        detail = new ProvidentFundDetail();
                        j++;
                        detail.PayTime = tdRow[0].ToDateTime();
                        //int j = tdRow[4].IndexOf("汇缴");
                        if (tdRow[4].IndexOf("汇缴") != -1)
                        {
                            detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[5], "[", "]").Replace("-","");
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            detail.PersonalPayAmount = decimal.Parse(Regex.Replace(tdRow[2], @"[/\&nbsp;\s]", ""))/2;//金额
                            detail.CompanyPayAmount = decimal.Parse(Regex.Replace(tdRow[2], @"[/\&nbsp;\s]", "")) / 2;//金额
                            detail.CompanyName = tdRow[6];
                            detail.ProvidentFundBase = (detail.PersonalPayAmount / payRate);//缴费基数
                            PaymentMonths++;
                        }
                        else if (tdRow[4].IndexOf("结息") != -1)
                        {
                            detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[5], "[", "]").Replace("-", "");
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.PersonalPayAmount = decimal.Parse(Regex.Replace(tdRow[2], @"[/\&nbsp;\s]", ""));//金额
                            detail.CompanyName = tdRow[6];
                            detail.Description = tdRow[4];
                        }
                        else
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.CompanyName = tdRow[6];
                            detail.Description = tdRow[4];
                           
                        }
                        Res.ProvidentFundDetailList.Add(detail);
                    }
                   
                }
                int k = j;
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