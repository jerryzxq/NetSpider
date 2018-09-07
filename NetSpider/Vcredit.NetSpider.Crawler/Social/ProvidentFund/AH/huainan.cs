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
    /// <summary>
    /// 与马鞍山相同
    /// </summary>
    public class huainan : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.hnzfgjj.com/gjjcx/";
        string fundCity = "ah_huainan";
        #endregion
        /// <summary>
        /// 解析验证码
        /// </summary>
        /// <returns></returns>
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            res.Token = token;
            try
            {
                res.StatusCode = ServiceConsts.StatusCode_success;
                res.StatusDescription = "所选城市无需初始化";
                //CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                res.StatusCode = ServiceConsts.StatusCode_error;
                res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return res;
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
            string errorMsg = string.Empty;
            try
            {
                //获取缓存
                //if (CacheHelper.GetCache(fundReq.Token) != null)
                //{
                //    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                //    CacheHelper.RemoveCache(fundReq.Token);
                //}
                //校验参数
                if (fundReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = "身份证号不允许为空！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "密码不允许为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region  第一步 初始化参数
                string _VIEWSTATE = string.Empty;
                string _EVENTVALIDATION = string.Empty;
                Url = baseUrl ;
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
                    _VIEWSTATE = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                if (results.Count > 0)
                {
                    _EVENTVALIDATION = results[0];

                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 第二步,选择身份证查询

                Url = baseUrl ;
                postdata = string.Format("__EVENTTARGET=RadioButtonList1%241&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE={0}&__EVENTVALIDATION={1}&RadioButtonList1=2&TextBox_dwzh=&TextBox_grzh=", _VIEWSTATE.ToUrlEncode(), _EVENTVALIDATION.ToUrlEncode());
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
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "<script>alert(", "');window.history.go");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
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

                #region 第三步,输入身份证号,提交

                Url = baseUrl ;
                postdata = string.Format("__LASTFOCUS=&__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE={0}&__EVENTVALIDATION={1}&RadioButtonList1=2&TextBox_sfzh={2}&ImageButton1.x=37&ImageButton1.y=8", _VIEWSTATE.ToUrlEncode(), _EVENTVALIDATION.ToUrlEncode(), fundReq.Identitycard);
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
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string dwzh = CommonFun.GetMidStr(httpResult.Html, "?dwzh=", " &");
                string qrzh = CommonFun.GetMidStr(httpResult.Html, "&grzh=", "'</script>");
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

                #region  第四步,输入密码界面,之后选择查询公积金/贷款
                //Url = baseUrl + string.Format("inputmima.aspx?dwzh={0}&grzh={1}", (dwzh + " ").ToUrlEncode(), qrzh);
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
                string viewstate = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                if (results.Count > 0)
                {
                    viewstate = _VIEWSTATE = results[0];
                }
                string eventvalidation = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                if (results.Count > 0)
                {
                    eventvalidation = _EVENTVALIDATION = results[0];

                }
                //选择查询类型cookie
                CookieCollection selectCookies = new CookieCollection();
                selectCookies = cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第五步,公积金基本信息

                Url = baseUrl + "inputmima.aspx?dwzh=" + dwzh + "&grzh=" + qrzh + "";
                postdata = string.Format("__VIEWSTATE={0}&__EVENTVALIDATION={1}&txtPass={2}&ImageButton_gjj.x=78&ImageButton_gjj.y=15", _VIEWSTATE.ToUrlEncode(), _EVENTVALIDATION.ToUrlEncode(), fundReq.Password);
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[1]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[1]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[2]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[3]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.BankCardNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[2]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[4]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[5]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[7]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    if (CommonFun.GetMidStr(results[0], "", "%").ToDecimal(0) == 0)
                        Res.PersonalMonthPayRate = payRate;
                    else
                        Res.CompanyMonthPayRate = CommonFun.GetMidStr(results[0], "", "%").ToDecimal(0) / 100;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[7]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    if (CommonFun.GetMidStr(results[0], "", "%").ToDecimal(0) == 0)
                        Res.CompanyMonthPayRate = payRate;
                    else
                        Res.PersonalMonthPayRate = CommonFun.GetMidStr(results[0], "", "%").ToDecimal(0) / 100;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[4]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[6]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    decimal pay = results[0].ToDecimal(0);
                    Res.PersonalMonthPayAmount = pay / 2;
                    Res.CompanyMonthPayAmount = pay / 2;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[8]/td[2]/span", "inner");
                if (results.Count > 0)
                {
                    Res.Bank = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td[1]/table/tr/td/table/tr[1]/td[1]/table/tr[6]/td[4]/span", "inner");
                if (results.Count > 0)
                {
                    Res.Status = results[0];
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并
                #endregion

                #region  第六步 公积金缴费明细

                //详细页面跳转首页 获取第一页数据
                Url = baseUrl + string.Format("cx_jieguo.aspx?dwzh={0}&grzh={1}", dwzh, qrzh);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Encoding = Encoding.GetEncoding("utf-8"),
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
                results = HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", ""), "//table[@id='GridViewZm']/tr[position()>1]", "inner", true);
                int PageCount = int.Parse(CommonFun.GetMidStr(httpResult.Html, "总共", "页"));
                //剩余页面详细信息
                Url = baseUrl + "cx_jieguo.aspx";
                for (int i = 2; i <= PageCount; i++)
                {
                    postdata = string.Format("__EVENTTARGET=btnNext&__EVENTARGUMENT=&__VIEWSTATE={0}&__VIEWSTATEENCRYPTED=&__EVENTVALIDATION={1}", _VIEWSTATE.ToUrlEncode(), _EVENTVALIDATION.ToUrlEncode());
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
                    _VIEWSTATE = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                    _EVENTVALIDATION = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", ""), "//table[@id='GridViewZm']/tr[position()>1]", "inner", true));
                }
                //处理缴费明细
                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "");
                    if (tdRow.Count < 5)
                    {
                        continue;
                    }
                    detail = new ProvidentFundDetail();
                    if (tdRow[1].IndexOf("汇缴") != -1)
                    {
                        int payMonths = CommonFun.GetMidStr(tdRow[1], "[", "]").ToInt(0) > 1 ? CommonFun.GetMidStr(tdRow[1], "[", "]").ToInt(0) : 1;
                        DateTime dtFundTime = DateTime.ParseExact(CommonFun.GetMidStr(tdRow[1], "", "["), "yyyy-MM", null);
                        for (int i = payMonths; i > 0; i--)
                        {
                            detail = new ProvidentFundDetail();
                            detail.PayTime = tdRow[0].ToDateTime();
                            detail.Description = payMonths > 1 ? "拆分显示:" + tdRow[1] : tdRow[1];
                            detail.ProvidentFundTime = dtFundTime.AddMonths(i).ToString(Consts.DateFormatString7);
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            detail.PersonalPayAmount = detail.CompanyPayAmount = (tdRow[3].ToDecimal(0) / payMonths) / 2;//金额
                            detail.ProvidentFundBase = (detail.PersonalPayAmount / Res.PersonalMonthPayRate);//缴费基数
                            Res.ProvidentFundDetailList.Add(detail);
                        }
                    }
                    else if (tdRow[1].IndexOf("结息") != -1)
                    {
                        detail.PayTime = tdRow[0].ToDateTime();
                        detail.Description = tdRow[1];
                        detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[1], "", "[");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = tdRow[2].ToDecimal(0) + tdRow[3].ToDecimal(0);//金额;
                        Res.ProvidentFundDetailList.Add(detail);
                    }
                    else if (tdRow[1].IndexOf("支取") > -1)
                    {
                        detail.PayTime = tdRow[0].ToDateTime();
                        detail.Description = tdRow[1];
                        detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[1], "", "[");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                        detail.PersonalPayAmount = tdRow[2].ToDecimal(0);//金额;
                        Res.Description = "有支取，请人工校验";
                        Res.ProvidentFundDetailList.Add(detail);
                    }
                    else
                    {
                        detail.PayTime = tdRow[0].ToDateTime();
                        detail.Description = tdRow[1];
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = tdRow[2].ToDecimal(0) + tdRow[3].ToDecimal(0);//金额;
                        Res.ProvidentFundDetailList.Add(detail);
                    }
                }
                #endregion

                #region  第七步,公积金贷款基本信息

                string dw = dwzh.ToUrlDecode();
                string gr = qrzh.ToUrlDecode();
                Url = baseUrl + string.Format("inputmima.aspx?dwzh={0}&grzh={1}", dw.ToUrlEncode(), gr.ToUrlEncode());
                postdata = string.Format("__VIEWSTATE={0}&__EVENTVALIDATION={1}&txtPass={2}&ImageButton_daikuan.x=93&ImageButton_daikuan.y=19", viewstate.ToUrlEncode(), eventvalidation.ToUrlEncode(), fundReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    Encoding = Encoding.GetEncoding("utf-8"),
                    CookieCollection = selectCookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
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
                #region 第八步 贷款明细
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
    }
    #region old
    //public class huainan:IProvidentFundCrawler
    //{
    //    #region 公共变量
    //    IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
    //    CookieCollection cookies = new CookieCollection();
    //    HttpHelper httpHelper = new HttpHelper();
    //    HttpResult httpResult = null;
    //    HttpItem httpItem = null;
    //    string baseUrl = "http://www.hnzfgjj.com/";
    //    string fundCity = "ah_huainan";
    //    List<string> results =new List<string>();
    //    #endregion

    //    #region 私有变量
    //    string _VIEWSTATE = string.Empty;
    //    string _EVENTVALIDATION = string.Empty;
    //    string _VIEWSTATEGENERATOR = string.Empty;
    //    string dwzh = string.Empty;
    //    string grzh = string.Empty;
    //    string errorMsg = string.Empty;
    //    #endregion

    //    public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
    //    {
    //        VerCodeRes Res = new VerCodeRes();
    //        string Url = string.Empty;
    //        string postdata = string.Empty;
    //        List<string> results = new List<string>();
    //        string token = CommonFun.GetGuidID();
    //        Res.Token = token;
    //        try
    //        {
    //            Res.StatusCode = ServiceConsts.StatusCode_success;
    //            Res.StatusDescription = "所选城市无需初始化";
    //            CacheHelper.SetCache(token, cookies);

    //            CacheHelper.SetCache(token, cookies);
    //        }
    //        catch (Exception e)
    //        {
    //            Res.StatusCode = ServiceConsts.StatusCode_error;
    //            Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
    //            Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
    //        }
    //        return Res;
    //    }

    //    public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
    //    {
    //        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
    //        Res.ProvidentFundCity = fundCity;
    //        string Url = string.Empty;
    //        string postdata = string.Empty;
    //        decimal payRate = (decimal)0.08;
    //        List<string> results = new List<string>();
    //        ProvidentFundDetail detail = null;
    //        int PaymentMonths = 0;
    //        try
    //        {
    //            //获取缓存
    //            if (CacheHelper.GetCache(fundReq.Token) != null)
    //            {

    //                cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
    //                CacheHelper.RemoveCache(fundReq.Token);
    //            }
    //            //校验参数
    //            if (fundReq.Identitycard.IsEmpty() || fundReq.Password.IsEmpty())
    //            {
    //                Res.StatusDescription = "身份证号或密码不能为空";
    //                Res.StatusCode = ServiceConsts.StatusCode_fail;
    //                return Res;
    //            }
    //            #region 第一步
    //              Url = baseUrl + "gjjcx/search_wh.aspx";
    //            httpItem = new HttpItem()
    //            {
    //                URL = Url,
    //                Method = "get",
    //                Encoding = Encoding.GetEncoding("utf-8"),
    //                CookieCollection = cookies,
    //                ResultCookieType = ResultCookieType.CookieCollection
    //            };
    //            httpResult = httpHelper.GetHtml(httpItem);
    //            errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');");
    //            if(!string.IsNullOrEmpty(errorMsg))
    //            {
    //                Res.StatusDescription = errorMsg;
    //                Res.StatusCode = ServiceConsts.StatusCode_fail;
    //                return Res;
    //            }

    //            //请求失败后返回
    //            if (httpResult.StatusCode != HttpStatusCode.OK)
    //            {
    //                Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
    //                Res.StatusCode = ServiceConsts.StatusCode_httpfail;
    //                return Res;
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
    //            if(results.Count>0)
    //            {
    //                _VIEWSTATE = results[0];
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
    //            if (results.Count > 0)
    //            {
    //                _EVENTVALIDATION = results[0];
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATEGENERATOR']", "value");
    //            if (results.Count > 0)
    //            {
    //                _VIEWSTATEGENERATOR = results[0];
    //            }
    //            #endregion
    //            #region 第二步
    //            Url = baseUrl + "gjjcx/search_wh.aspx";
    //            postdata = string.Format("__EVENTTARGET=RadioButtonList1%241&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__EVENTVALIDATION={2}&RadioButtonList1=2&TextBox_dwzh=", _VIEWSTATE.ToUrlDecode(), _VIEWSTATEGENERATOR, _EVENTVALIDATION.ToUrlEncode());
    //            httpItem = new HttpItem()
    //            {
    //                URL = Url,
    //                Method = "post",
    //                Postdata=postdata,
    //                Encoding = Encoding.GetEncoding("utf-8"),
    //                CookieCollection = cookies,
    //                ResultCookieType = ResultCookieType.CookieCollection
    //            };
    //            httpResult = httpHelper.GetHtml(httpItem);
    //            //请求失败后返回
    //            if (httpResult.StatusCode != HttpStatusCode.OK)
    //            {
    //                Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
    //                Res.StatusCode = ServiceConsts.StatusCode_httpfail;
    //                return Res;
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
    //            if (results.Count > 0)
    //            {
    //                _VIEWSTATE = results[0];
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
    //            if (results.Count > 0)
    //            {
    //                _EVENTVALIDATION = results[0];
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATEGENERATOR']", "value");
    //            if (results.Count > 0)
    //            {
    //                _VIEWSTATEGENERATOR = results[0];
    //            }
    //            #endregion
    //            #region 第三步
    //            Url = baseUrl + "gjjcx/search_wh.aspx";
    //            postdata = string.Format("__LASTFOCUS=&__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__EVENTVALIDATION={2}&RadioButtonList1=2&TextBox_sfzh={3}&ImageButton1.x=45&ImageButton1.y=16", _VIEWSTATE.ToUrlEncode(), _VIEWSTATEGENERATOR, _EVENTVALIDATION.ToUrlEncode(),fundReq.Identitycard);
    //            httpItem = new HttpItem()
    //            {
    //                URL = Url,
    //                Method = "post",
    //                Postdata = postdata,
    //                Encoding = Encoding.GetEncoding("utf-8"),
    //                CookieCollection = cookies,
    //                ResultCookieType = ResultCookieType.CookieCollection
    //            };
    //            httpResult = httpHelper.GetHtml(httpItem);
    //            errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');");
    //            if (!string.IsNullOrEmpty(errorMsg))
    //            {
    //                Res.StatusDescription = errorMsg;
    //                Res.StatusCode = ServiceConsts.StatusCode_fail;
    //                return Res;
    //            }
    //            //请求失败后返回
    //            if (httpResult.StatusCode != HttpStatusCode.OK)
    //            {
    //                Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
    //                Res.StatusCode = ServiceConsts.StatusCode_httpfail;
    //                return Res;
    //            }
    //            dwzh = CommonFun.GetMidStr(httpResult.Html, "aspx?dwzh=", "&grzh").Trim();
    //            grzh = CommonFun.GetMidStr(httpResult.Html, "&grzh=", "'");
    //            #endregion

    //            #region 第四步
    //            Url = baseUrl + string.Format("gjjcx/inputmima.aspx?dwzh={0}&grzh={1}",dwzh,grzh);
    //            httpItem = new HttpItem
    //            {
    //                URL = Url,
    //                Method = "post",
    //                Postdata = postdata,
    //                Encoding = Encoding.GetEncoding("utf-8"),
    //                CookieCollection = cookies,
    //                ResultCookieType = ResultCookieType.CookieCollection
    //            };
    //            httpResult = httpHelper.GetHtml(httpItem);
               
    //            //请求失败后返回
    //            if (httpResult.StatusCode != HttpStatusCode.OK)
    //            {
    //                Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
    //                Res.StatusCode = ServiceConsts.StatusCode_httpfail;
    //                return Res;
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
    //            if(results.Count>0)
    //            {
    //                _VIEWSTATE = results[0];
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
    //            if (results.Count > 0)
    //            {
    //                _EVENTVALIDATION = results[0];
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATEGENERATOR']", "value");
    //            if (results.Count > 0)
    //            {
    //                _VIEWSTATEGENERATOR = results[0];
    //            }
    //            #endregion

    //            #region 第五步
    //            Url = baseUrl + string.Format("gjjcx/inputmima.aspx?dwzh={0}&grzh={1}",dwzh.ToUrlEncode(),grzh);
    //            cookies = CommonFun.GetCookieCollection(cookies,httpResult.CookieCollection);
    //            postdata = string.Format("__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__EVENTVALIDATION={2}&txtPass={3}&ImageButton_gjj.x=89&ImageButton_gjj.y=16",_VIEWSTATE.ToUrlEncode(),_VIEWSTATEGENERATOR,_EVENTVALIDATION.ToUrlEncode(),fundReq.Password);
    //            httpItem = new HttpItem
    //            {
    //                URL = Url,
    //                Method = "post",
    //                Postdata = postdata,
                  
    //                Encoding = Encoding.GetEncoding("utf-8"),
    //                CookieCollection = cookies,
    //                ResultCookieType = ResultCookieType.CookieCollection
    //            };
    //            httpResult = httpHelper.GetHtml(httpItem);
    //            errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');");
    //            if (!string.IsNullOrEmpty(errorMsg))
    //            {
    //                Res.StatusDescription = errorMsg;
    //                Res.StatusCode = ServiceConsts.StatusCode_fail;
    //                return Res;
    //            }
    //            //请求失败后返回
    //            if (httpResult.StatusCode != HttpStatusCode.OK)
    //            {
    //                Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
    //                Res.StatusCode = ServiceConsts.StatusCode_httpfail;
    //                return Res;
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td/table/tr/td/table/tr/td/table/tr[1]/td[2]/span", "");
    //            if(results.Count>0)
    //            {
    //                Res.CompanyNo = results[0];
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td/table/tr/td/table/tr/td/table/tr[1]/td[4]/span", "");
    //            if (results.Count > 0)
    //            {
    //                Res.EmployeeNo = results[0];
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td/table/tr/td/table/tr/td/table/tr[4]/td[4]/span", "");
    //            if (results.Count > 0)
    //            {
    //                Res.TotalAmount = results[0].ToDecimal(0);
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td/table/tr/td/table/tr/td/table/tr[2]/td[2]/span", "");
    //            if (results.Count > 0)
    //            {
    //                Res.CompanyName = results[0];
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td/table/tr/td/table/tr/td/table/tr[2]/td[4]/span", "");
    //            if (results.Count > 0)
    //            {
    //                Res.Name = results[0];
    //            }

    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td/table/tr/td/table/tr/td/table/tr[3]/td[4]/span", "");
    //            if (results.Count > 0)
    //            {
    //                Res.BankCardNo = results[0];
    //            }

    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td/table/tr/td/table/tr/td/table/tr[4]/td[2]/span", "");
    //            if (results.Count > 0)
    //            {
    //                Res.IdentityCard = results[0];
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td/table/tr/td/table/tr/td/table/tr[4]/td[4]/span", "");
    //            if (results.Count > 0)
    //            {
    //                Res.TotalAmount = results[0].ToDecimal(0);
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td/table/tr/td/table/tr/td/table/tr[5]/td[4]/span", "");
    //            if (results.Count > 0)
    //            {
    //                Res.LastProvidentFundTime = results[0];
    //            }

               
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td/table/tr/td/table/tr/td/table/tr[6]/td[4]/span", "");
    //            if (results.Count > 0)
    //            {
    //                Res.Status = results[0];
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td/table/tr/td/table/tr/td/table/tr[7]/td[2]/span", "");
    //            if (results.Count > 0)
    //            {
    //                Res.CompanyMonthPayRate = decimal.Parse(CommonFun.GetMidStr(results[0],"","%"))/100;
    //             }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td/table/tr/td/table/tr/td/table/tr[7]/td[4]/span", "");
    //            if (results.Count > 0)
    //            {
    //                Res.PersonalMonthPayRate = decimal.Parse(CommonFun.GetMidStr(results[0], "", "%")) / 100;
    //            }

    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td/table/tr/td/table/tr/td/table/tr[6]/td[2]/span", "");
    //            if (results.Count > 0)
    //            {
    //                decimal pay = results[0].ToDecimal(0);//月缴金额
    //                Res.PersonalMonthPayAmount =pay/2;
    //                Res.CompanyMonthPayAmount = pay/2;
    //                Res.SalaryBase = Math.Round(Res.PersonalMonthPayAmount / Res.PersonalMonthPayRate,2);
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]/td/table/tr/td/table/tr/td/table/tr[8]/td[2]/span", "");
    //            if (results.Count > 0)
    //            {
    //                Res.Bank = results[0];
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
    //            if(results.Count>0)
    //            {
    //                _VIEWSTATE = results[0];
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
    //            if (results.Count > 0)
    //            {
    //                _EVENTVALIDATION = results[0];
    //            }
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATEGENERATOR']", "value");
    //            if (results.Count > 0)
    //            {
    //                _VIEWSTATEGENERATOR = results[0];
    //            }
    //            string count = CommonFun.GetMidStr(httpResult.Html, "总共", "页");

    //            #region  第六步 获取刚加载的数据
    //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/table/tr/td/table/tr[5]/td/table/tr/td/div/table/tr[position()>1]", "");
    //            foreach (var itemRow in results)
    //            {
    //                detail = new ProvidentFundDetail();
    //                List<string> tdRow = HtmlParser.GetResultFromParser(itemRow, "//td", "text", true);
    //                if (tdRow.Count != 5)
    //                {
    //                    continue;
    //                }
    //                detail.PayTime = tdRow[0].ToDateTime();
    //                if (tdRow[1].Trim().Contains("汇缴"))
    //                {
    //                    detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[1], "", "[").Replace("-","");//缴费年月
    //                    detail.Description = tdRow[1];//描述
    //                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
    //                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
    //                    //detail.PersonalPayAmount = (decimal.Parse(tdRow[3]) / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)) * Res.PersonalMonthPayRate;
    //                    detail.PersonalPayAmount = (Res.PersonalMonthPayRate / ((Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)))*tdRow[3].ToDecimal(0);

    //                    //detail.CompanyPayAmount = (decimal.Parse(tdRow[3]) / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)) * Res.CompanyMonthPayRate;
    //                    detail.CompanyPayAmount = (Res.CompanyMonthPayRate / ((Res.PersonalMonthPayRate + Res.CompanyMonthPayRate))) * tdRow[3].ToDecimal(0);
    //                    detail.ProvidentFundBase = tdRow[3].ToDecimal(0) / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate);
    //                    PaymentMonths++;
    //                }
    //                else if (tdRow[1].Trim().Contains("结息"))
    //                {
    //                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
    //                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
    //                    detail.Description = tdRow[1];
    //                    detail.PersonalPayAmount = tdRow[3].ToDecimal(0);
    //                }
    //                else
    //                {
    //                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
    //                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
    //                    detail.Description = tdRow[1];
    //                    detail.PersonalPayAmount = 0;
    //                    detail.CompanyPayAmount = 0;
    //                }
    //                Res.ProvidentFundDetailList.Add(detail);
    //            }
    //            #endregion
    //            #endregion

    //            #region  第七步获取除了第一页的数据
    //            int i = 2;
    //            while (true)
    //            {
    //                Url =baseUrl+ string.Format("gjjcx/cx_jieguo.aspx?dwzh={0}&grzh={1}", dwzh, grzh);
    //                postdata = string.Format("__EVENTTARGET=btnNext&__EVENTARGUMENT=&__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__VIEWSTATEENCRYPTED=&__EVENTVALIDATION={2}", _VIEWSTATE.ToUrlEncode(), _VIEWSTATEGENERATOR.ToUrlEncode(), _EVENTVALIDATION.ToUrlEncode());
                   
    //                httpItem = new HttpItem
    //                {
    //                    URL = Url,
    //                    Method = "post",
    //                    Postdata = postdata,
    //                    Encoding = Encoding.GetEncoding("utf-8"),
    //                    CookieCollection = cookies,
    //                    ResultCookieType = ResultCookieType.CookieCollection
    //                };
    //                httpResult = httpHelper.GetHtml(httpItem);
    //                //请求失败后返回
    //                if (httpResult.StatusCode != HttpStatusCode.OK)
    //                {
    //                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
    //                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
    //                    return Res;
    //                }
    //                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
    //                if (results.Count > 0)
    //                {
    //                    _VIEWSTATE = results[0];
    //                }
    //                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
    //                if (results.Count > 0)
    //                {
    //                    _EVENTVALIDATION = results[0];
    //                }
    //                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATEGENERATOR']", "value");
    //                if (results.Count > 0)
    //                {
    //                    _VIEWSTATEGENERATOR = results[0];
    //                }
    //                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/table/tr/td/table/tr[5]/td/table/tr/td/div/table/tr[position()>1]", "");
    //                if (i>int.Parse(count))
    //                    break;
    //                i++;
    //                foreach (var itemRow in results)
    //                {
    //                    detail = new ProvidentFundDetail();
    //                    List<string> tdRow = HtmlParser.GetResultFromParser(itemRow, "//td", "text", true);
    //                    if (tdRow.Count != 5)
    //                    {
    //                        continue;
    //                    }
    //                    detail.PayTime = tdRow[0].ToDateTime();
    //                    if (tdRow[1].Trim().Contains("汇缴"))
    //                    {
    //                        detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[1], "", "[").Replace("-","");//缴费年月
    //                        detail.Description = tdRow[1];//描述
    //                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
    //                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                    
    //                        detail.PersonalPayAmount = (Res.PersonalMonthPayRate / ((Res.PersonalMonthPayRate + Res.CompanyMonthPayRate))) * tdRow[3].ToDecimal(0);

                            
    //                        detail.CompanyPayAmount = (Res.CompanyMonthPayRate / ((Res.PersonalMonthPayRate + Res.CompanyMonthPayRate))) * tdRow[3].ToDecimal(0);
    //                        detail.ProvidentFundBase = tdRow[3].ToDecimal(0) / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate);//缴费基数
    //                        PaymentMonths++;
    //                    }
    //                    else if (tdRow[1].Trim().Contains("结息"))
    //                    {
    //                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
    //                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
    //                        detail.Description = tdRow[1];
    //                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0);
    //                    }
    //                    else
    //                    {
    //                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
    //                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
    //                        detail.Description = tdRow[1];
    //                        detail.PersonalPayAmount = 0;
    //                        detail.CompanyPayAmount = 0;
    //                    }
    //                    Res.ProvidentFundDetailList.Add(detail);
    //                }
    //            }
    //            Res.PaymentMonths = PaymentMonths;
    //            #endregion
    //            Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_QuerySuccess;
    //            Res.StatusCode = ServiceConsts.StatusCode_success;


    //        }
    //        catch (Exception e)
    //        {
    //            Res.StatusCode = ServiceConsts.StatusCode_error;
    //            Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_QueryError, e);
    //        }
    //        return Res;
    //    }
    //}
    #endregion
}
