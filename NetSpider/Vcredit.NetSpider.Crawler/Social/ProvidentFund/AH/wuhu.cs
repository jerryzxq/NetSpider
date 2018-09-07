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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.AH
{
    public class wuhu : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://60.169.88.225:8088/";
        string fundCity = "ah_wuhu";
        #endregion

        /// <summary>
        /// 解析验证码
        /// </summary>
        /// <returns></returns>
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
            ProvidentFundLoanRes Res_Loan = new ProvidentFundLoanRes();
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            ProvidentFundDetail detail = null;
            int PaymentMonths = 0;
            string viewstate = string.Empty;
            string enentvalidation = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = "身份证号不允许为空!";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //15位或18位身份证验证
                Regex reg = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (reg.IsMatch(fundReq.Identitycard) == false)
                {
                    Res.StatusDescription = "改身份证号未匹配到您的个人信息,请检查身份证是否正确！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "密码不允许为空!!!";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region  第一步 初始化

                string errorMsg = string.Empty;
                string _VIEWSTATE = string.Empty;
                string _EVENTVALIDATION = string.Empty;
                Url = baseUrl + "search_wh.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("utf-8"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');");
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                if (results.Count > 0)
                {
                    _VIEWSTATE = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                if (results.Count > 0)
                {
                    _EVENTVALIDATION = results[0];

                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 第二步 选择身份证登陆
                Url = baseUrl + "search_wh.aspx";
                postdata = string.Format("__EVENTTARGET=RadioButtonList1%241&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE={0}&__EVENTVALIDATION={1}&RadioButtonList1=2&TextBox_dwzh=", _VIEWSTATE.ToUrlEncode(), _EVENTVALIDATION.ToUrlEncode());
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                if (results.Count > 0)
                {
                    _VIEWSTATE = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                if (results.Count > 0)
                {
                    _EVENTVALIDATION = results[0];

                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 第三步 输入身份证提交
                Url = baseUrl + "search_wh.aspx";
                postdata = string.Format(@"__LASTFOCUS=&__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE={0}&__EVENTVALIDATION={1}&RadioButtonList1=2&TextBox_sfzh={2}&ImageButton1.x=44&ImageButton1.y=6", _VIEWSTATE.ToUrlEncode(), _EVENTVALIDATION.ToUrlEncode(), fundReq.Identitycard);
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
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "');</script>");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string dwzh = CommonFun.GetMidStr(httpResult.Html, "?dwzh=", " &");
                string grzh = CommonFun.GetMidStr(httpResult.Html, "&grzh=", "'</script>");
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                if (results.Count > 0)
                {
                    _VIEWSTATE = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                if (results.Count > 0)
                {
                    _EVENTVALIDATION = results[0];
                }
                //選項 cookies
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第四步 身份证对应多个账号,选择进入相应账号输入密码界面，保存相应信息

                List<Acount> acountList = new List<Acount>();
                Acount acount = new Acount();
                string needHtml = string.Empty;//最近时间段的公积金基本信息html
                Acount realAcount = new Acount();//实际要选择的账号信息
                DateTime startTime = new DateTime();// 起缴月份
                Url = baseUrl + CommonFun.GetMidStr(httpResult.Html, "parent.location.href='", "'</script>");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("utf-8"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                if (results.Count > 0)
                {
                    viewstate = _VIEWSTATE = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                if (results.Count > 0)
                {
                    acount.enentvalidation = _EVENTVALIDATION = results[0];
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (Url.IndexOf("select_fromsfzhm.aspx") > -1)
                {
                    //账号列表
                    results = HtmlParser.GetResultFromParser(httpResult.Html,
                        "//table[@id='GridView1']/tr[position()>1]", "onclick", true);
                    foreach (string s in results)
                    {
                        string[] acountMsg = s.Replace("#39;", "").Split('&');
                         acount = new Acount {dwzh = acountMsg[1], grzh = acountMsg[3]};
                        Url = baseUrl + string.Format("inputmima.aspx?dwzh={0}&grzh={1}", acount.dwzh, acount.grzh);
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "get",
                            Encoding = Encoding.GetEncoding("utf-8"),
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');");
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
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                        if (results.Count > 0)
                        {
                            acount.viewstate = _VIEWSTATE = results[0];
                        }
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']",
                            "value");
                        if (results.Count > 0)
                        {
                            acount.enentvalidation = _EVENTVALIDATION = results[0];
                        }
                        //选择cookie,公积金/贷款
                        acount.cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                        acountList.Add(acount);
                    }
                   
                    foreach (Acount items in acountList)
                    {
                        Url = baseUrl + "inputmima.aspx?dwzh=" + items.dwzh + "&grzh=" + items.grzh + "";
                        postdata = string.Format("__VIEWSTATE={0}&__EVENTVALIDATION={1}&txtPass={2}&ImageButton_gjj.x=101&ImageButton_gjj.y=17", items.viewstate.ToUrlEncode(), items.enentvalidation.ToUrlEncode(), fundReq.Password);
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "Post",
                            Postdata = postdata,
                            Encoding = Encoding.GetEncoding("utf-8"),
                            CookieCollection = items.cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');");
                        if (!string.IsNullOrEmpty(errorMsg))
                        {
                            Res.StatusDescription = errorMsg;
                            Res.StatusCode = ServiceConsts.StatusCode_fail;
                            return Res;
                        }
                        //保存基本信息頁面參數
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='qjrqLabel']", "", true);
                        if (results.Count <= 0) continue;
                        if (DateTime.Parse(results[0]) <= startTime) continue;
                        needHtml = httpResult.Html;
                        realAcount = items;
                        startTime = DateTime.Parse(results[0]);
                        viewstate = _VIEWSTATE = realAcount.viewstate;
                        enentvalidation = _EVENTVALIDATION = realAcount.enentvalidation;
                        dwzh = realAcount.dwzh;
                        grzh = realAcount.grzh;
                    }
                }
                else
                {
                    Url = baseUrl + "inputmima.aspx?dwzh=" + dwzh + "&grzh=" +grzh + "";
                    postdata = string.Format("__VIEWSTATE={0}&__EVENTVALIDATION={1}&txtPass={2}&ImageButton_gjj.x=101&ImageButton_gjj.y=17", _VIEWSTATE.ToUrlEncode(), _EVENTVALIDATION.ToUrlEncode(), fundReq.Password);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "Post",
                        Postdata = postdata,
                        Encoding = Encoding.GetEncoding("utf-8"),
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');");
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        Res.StatusDescription = errorMsg;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                    if (results.Count > 0)
                    {
                        viewstate = _VIEWSTATE = results[0];
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                    if (results.Count > 0)
                    {
                        enentvalidation = _EVENTVALIDATION = results[0];
                    }
                    needHtml = httpResult.Html;
                }
                
               
                #endregion

                #region 第五步 输入密码提交，查询,保留[最近时间段]公积金基本信息

              
               
                results = HtmlParser.GetResultFromParser(needHtml, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[1]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(needHtml, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[1]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(needHtml, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[2]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                results = HtmlParser.GetResultFromParser(needHtml, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[2]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(needHtml, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[3]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.BankCardNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(needHtml, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[4]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }

                results = HtmlParser.GetResultFromParser(needHtml, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[5]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = Convert.ToDateTime(results[0]).ToString("yyyyMM");
                }

                results = HtmlParser.GetResultFromParser(needHtml, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[6]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].ToDecimal(0);
                }

                results = HtmlParser.GetResultFromParser(needHtml, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[6]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.Status = results[0];
                }
                results = HtmlParser.GetResultFromParser(needHtml, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[7]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = CommonFun.GetMidStr(results[0], "", "%").ToDecimal(0) / 100;
                }
                results = HtmlParser.GetResultFromParser(needHtml, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[7]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = CommonFun.GetMidStr(results[0], "", "%").ToDecimal(0) / 100;
                }
                results = HtmlParser.GetResultFromParser(needHtml, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[6]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].ToDecimal(0) / 2;
                    Res.CompanyMonthPayAmount = results[0].ToDecimal(0) / 2;
                }
                results = HtmlParser.GetResultFromParser(needHtml, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[8]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.Bank = results[0];
                }
                results = HtmlParser.GetResultFromParser(needHtml, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[4]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(needHtml, "//input[@id='__VIEWSTATE']", "value");
                if (results.Count > 0)
                {
                    _VIEWSTATE = results[0];
                }
                results = HtmlParser.GetResultFromParser(needHtml, "//input[@id='__EVENTVALIDATION']", "value");
                if (results.Count > 0)
                {
                    _EVENTVALIDATION = results[0];
                }
                #endregion

                #region 第六步 最近時段公積金明細信息

                List<string> detailResults = new List<string>();
                detailResults.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='GridViewZm']//tr[position()>1]", ""));
                int PageCount = int.Parse(CommonFun.GetMidStr(httpResult.Html, "总共", "页").ToString());
                int i = 2;
                while (true)
                {
                    if (i > PageCount) break;
                    Url = baseUrl + string.Format("cx_jieguo.aspx?dwzh={0}&grzh={1}", dwzh, grzh);
                    postdata = string.Format("__EVENTTARGET=btnNext&__EVENTARGUMENT=&__VIEWSTATE={0}&__VIEWSTATEENCRYPTED=&__EVENTVALIDATION={1}", _VIEWSTATE.ToUrlEncode(), _EVENTVALIDATION.ToUrlEncode());
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        Encoding = Encoding.GetEncoding("utf-8"),
                        CookieCollection = realAcount.cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (httpResult.StatusCode != HttpStatusCode.OK)
                    {
                        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                        Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                        return Res;
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                    if (results.Count>0)
                    {
                        _VIEWSTATE = results[0];
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                    if (results.Count > 0)
                    {
                        _EVENTVALIDATION = results[0];
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html,"//table[@id='GridViewZm']//tr[position()>1]");
                    detailResults.AddRange(results);
                    i++;
                }
                foreach (string item in detailResults)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                    if (tdRow.Count != 5)
                    {
                        continue;
                    }
                    detail = new ProvidentFundDetail();
                    detail.Description = tdRow[1];
                    detail.PayTime = tdRow[0].ToDateTime();
                    if (tdRow[1].IndexOf("汇缴") > -1)
                    {
                        detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[1], "", "[").Replace("-", "");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = decimal.Parse(Regex.Replace(tdRow[3], @"[/\&nbsp;\,\s]", "")) / 2;//金额
                        detail.CompanyPayAmount = decimal.Parse(Regex.Replace(tdRow[3], @"[/\&nbsp;\,\s]", "")) / 2;//金额
                        detail.ProvidentFundBase = (detail.PersonalPayAmount / Res.PersonalMonthPayRate);//缴费基数
                        PaymentMonths++;
                    }
                    else if (tdRow[1].IndexOf("结息") > -1)
                    {
                        //detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[1], "", "[");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = decimal.Parse(Regex.Replace(tdRow[3], @"[/\&nbsp;\,\s]", ""));//金额
                        detail.Description = tdRow[1];
                    }
                    else if (tdRow[1].IndexOf("补缴") > -1)
                    {
                        //detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[1], "", "[");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = decimal.Parse(Regex.Replace(tdRow[3], @"[/\&nbsp;\,\s]", ""));//金额
                        detail.Description = tdRow[1];
                    }
                    else if (tdRow[1].IndexOf("支取") > -1)
                    {
                        //detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[1], "", "[");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = decimal.Parse(Regex.Replace(tdRow[3], @"[/\&nbsp;\,\s]", ""));//金额
                        detail.Description = tdRow[1];

                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = decimal.Parse(Regex.Replace(tdRow[2], @"[/\&nbsp;\,\s]", ""));
                        detail.Description = tdRow[1];
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion

                #region 贷款基本信息

                Url = baseUrl + "inputmima.aspx?dwzh=" + dwzh + "&grzh=" + grzh + "";
                postdata = string.Format("__VIEWSTATE={0}&__EVENTVALIDATION={1}&txtPass={2}&ImageButton_daikuan.x=45&ImageButton_daikuan.y=14", viewstate.ToUrlEncode(), enentvalidation.ToUrlEncode(), fundReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    Encoding = Encoding.GetEncoding("utf-8"),
                    //CookieCollection = selectCookie,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='DataList1_ctl00_s_grzhLabel']", "text");
                if (results.Count > 0)
                {
                    Res_Loan.Name = results[0];
                }
                else
                {
                    Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='DataList1_ctl00_s_sfzhmLabel']", "text");
                if (results.Count > 0)
                {
                    Res_Loan.IdentityCard = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='DataList1_ctl00_s_gjj_grzhLabel']", "text");
                if (results.Count > 0)
                {
                    Res_Loan.Account_Loan = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='DataList1_ctl00_s_gjj_dwzhLabel']", "text");
                if (results.Count > 0)
                {
                    Res_Loan.Loan_Credit = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='DataList1_ctl00_dt_dkLabel']", "text");
                if (results.Count > 0)
                {
                    Res_Loan.Loan_Start_Date = Convert.ToDateTime(results[0]).ToString(Consts.DateFormatString2);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='DataList1_ctl00_dc_dkjeLabel']", "text");
                if (results.Count > 0)
                {
                    Res_Loan.Period_Total = (results[0].ToInt(0) * 12).ToString();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='DataList1_ctl00_dc_yhbjLabel']", "text");
                if (results.Count > 0)
                {
                    Res_Loan.Principal_Payed = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='DataList1_ctl00_Label2']", "text");
                if (results.Count > 0)
                {
                    Res_Loan.Loan_Balance = results[0].ToDecimal(0);
                }
                #endregion
                #region 贷款明细
                //表1 贷款帐目
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='GridView1']/tr[position()>1]", "", true);
                foreach (string s in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(s, "//td", "text");
                    if (tdRow.Count < 5) continue;
                    string Record_Month = Convert.ToDateTime(tdRow[1]).ToString(Consts.DateFormatString7);
                    ProvidentFundLoanDetail loanDetail = Res_Loan.ProvidentFundLoanDetailList.FirstOrDefault(o => o.Record_Month == Record_Month && !string.IsNullOrEmpty(Record_Month));
                    bool isSave = false;
                    if (loanDetail == null)
                    {
                        isSave = true;
                        loanDetail = new ProvidentFundLoanDetail();
                        loanDetail.Description = tdRow[0];
                        loanDetail.Record_Date = tdRow[1];
                        loanDetail.Record_Month = Record_Month;
                        loanDetail.Balance = tdRow[4].ToDecimal(0);
                    }
                    //结转的部分不给予累加
                    if (tdRow[0].IndexOf("结转") == -1)
                    {
                        loanDetail.Principal += tdRow[3].ToDecimal(0);
                    }
                    //添加描述
                    if (loanDetail.Description.IndexOf(tdRow[0]) == -1)
                    {
                        if (tdRow[0].IndexOf("结转") > -1)
                        {
                            loanDetail.Description += "；" + tdRow[0] + ":" + tdRow[3];
                        }
                        else
                        {
                            loanDetail.Description += "；" + tdRow[0];
                        }

                    }
                    if (isSave)
                    {
                        Res_Loan.ProvidentFundLoanDetailList.Add(loanDetail);
                    }
                }
                //表2 利息帐目
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='GridView2']/tr[2]/td[1]", "text", true);
                if (results.Count > 0)
                {
                    Res_Loan.Account_Loan = results[0];//贷款账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='GridView2']/tr[position()>1]", "", true);
                foreach (string s in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(s, "//td", "text");
                    if (tdRow.Count < 3) continue;
                    string Record_Month = Convert.ToDateTime(tdRow[1]).ToString(Consts.DateFormatString7);
                    ProvidentFundLoanDetail loanDetail = Res_Loan.ProvidentFundLoanDetailList.FirstOrDefault(o => o.Record_Month == Record_Month && !string.IsNullOrEmpty(Record_Month));
                    bool isSave = false;
                    if (loanDetail == null)
                    {
                        isSave = true;
                        loanDetail = new ProvidentFundLoanDetail();
                        loanDetail.Description = "个人贷款帐号:" + tdRow[0] + "利息帐目";
                        loanDetail.Record_Date = tdRow[1];
                        loanDetail.Record_Month = Record_Month;
                    }
                    loanDetail.Interest += tdRow[2].ToDecimal(0);
                    if (isSave)
                    {
                        Res_Loan.ProvidentFundLoanDetailList.Add(loanDetail);
                    }
                }
                #endregion
                Res.ProvidentFundLoanRes = Res_Loan;
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
        /// <summary>
        /// 输入密码,选择查询信息类型页面 
        /// </summary>
        public class Acount
        {
            public string dwzh { get; set; }
            public string grzh { get; set; }
            public string viewstate { get; set; }
            public string enentvalidation { get; set; }
    
            public CookieCollection cookies { get; set; }
        }
    }
}
