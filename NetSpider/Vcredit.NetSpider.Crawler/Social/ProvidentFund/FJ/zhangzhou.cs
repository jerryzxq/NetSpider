using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.FJ
{
    public class zhangzhou : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.zzgjj.gov.cn/";
        string fundCity = "fj_zhangzhou";
        #endregion
        #region 私有变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        string VIEWSTATE = string.Empty;
        string EVENTVALIDATION = string.Empty;
        List<string> results = new List<string>();
        ProvidentFundDetail detail = null;
        int PaymentMonths = 0;
        decimal payRate = (decimal)0.08;
        private Regex reg = new Regex(@"[^0-9;1-9]*");
        #endregion
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "yuanjian/grlogin.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Encoding = Encoding.GetEncoding("utf-8"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value", true);
                if (results.Count > 0)
                {
                    VIEWSTATE = results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value", true);
                if (results.Count > 0)
                {
                    EVENTVALIDATION = results[0];
                }
                Url = baseUrl + "yuanjian/ValidateImage.ashx";//验证码地址
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
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                //添加缓存
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("VIEWSTATE", VIEWSTATE);
                dics.Add("EVENTVALIDATION", EVENTVALIDATION);
                CacheHelper.SetCache(token, dics);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return Res;
        }
        public ProvidentFundQueryRes ProvidentFundQuery(ProvidentFundReq fundReq)
        {
            ProvidentFundLoanRes Resload = new ProvidentFundLoanRes();
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            VIEWSTATE = string.Empty;
            EVENTVALIDATION = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(fundReq.Token);
                    VIEWSTATE = dics["VIEWSTATE"].ToString();
                    EVENTVALIDATION = dics["EVENTVALIDATION"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (string.IsNullOrWhiteSpace(fundReq.Username))
                {
                    Res.StatusDescription = "公积金账号不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Password))
                {
                    Res.StatusDescription = "密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Vercode))
                {
                    Res.StatusDescription = "验证码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录
                Url = baseUrl + "yuanjian/grlogin.aspx";
                postdata = String.Format("__VIEWSTATE={3}&__EVENTVALIDATION={4}&txtAccount={0}&txtPassword={1}&txtCode={2}&login.x=30&login.y=15", fundReq.Username, fundReq.Password, fundReq.Vercode, VIEWSTATE.ToUrlEncode(), EVENTVALIDATION.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = Url,
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
                string errorInfo = CommonFun.GetMidStr(httpResult.Html, "<script>alert(", "');</script>").Trim();
                if (!errorInfo.IsEmpty())
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                #region 第二步，公积金基本信息
                Url = baseUrl + "yuanjian/Personal/PersonalInfo.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label1']", "innertext");
                if (results.Count > 0)
                {
                    Res.Status = results[0];//账户状态
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label2']", "innertext");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];//公积金账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label3']", "innertext");
                if (results.Count > 0)
                {
                    Res.OpenTime = results[0];//开户日期
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label4']", "innertext");
                if (results.Count > 0)
                {
                    Res.Bank = results[0];//开户银行
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label5']", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];//单位名称
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label6']", "innertext");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//职工姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label7']", "innertext");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label9']", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = results[0].ToDecimal(0) * 0.01M;//单位缴费比率
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label10']", "innertext");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = results[0].ToDecimal(0) * 0.01M;//个人缴费比率
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label11']", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = results[0].ToDecimal(0);//单位月缴费
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label12']", "innertext");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].ToDecimal(0);//个人月缴费
                    if (Res.PersonalMonthPayRate > 0)
                    {
                        Res.SalaryBase = (Res.PersonalMonthPayAmount / (Res.PersonalMonthPayRate)).ToString("f2").ToDecimal(0);//基本薪资
                    }
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label14']", "innertext");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0];//缴至年月
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label17']", "innertext");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);//账户总额
                }
                #endregion
                #region 第三步,公积金缴费明细

                int totalPages;
                httpResult = null;
                Url = baseUrl + "yuanjian/Personal/PersonalList.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                if (results.Count > 0)
                {
                    VIEWSTATE = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                if (results.Count > 0)
                {
                    EVENTVALIDATION = results[0];
                }
                //当前年份
                Res = GetPageResults(httpResult);
                int temp;
                //剩余页
                if (int.TryParse(CommonFun.GetMidStr(httpResult.Html, "当前第1页/", "页").Trim(), out temp))
                {
                    totalPages = temp;
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                    if (results.Count > 0)
                    {
                        VIEWSTATE = results[0];
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                    if (results.Count > 0)
                    {
                        EVENTVALIDATION = results[0];
                    }
                    for (int j = 2; j <= totalPages; j++)
                    {
                        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                        postdata = string.Format("__EVENTTARGET=ctl00%24ContentPlaceHolder1%24AspNetPager1&__EVENTARGUMENT={0}&__VIEWSTATE={1}&__EVENTVALIDATION={2}&ctl00%24ContentPlaceHolder2%24DropDownList1={3}", j, VIEWSTATE.ToUrlEncode(), EVENTVALIDATION.ToUrlEncode(), DateTime.Now.Year);
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "post",
                            Postdata = postdata,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        Res = GetPageResults(httpResult);
                    }
                }
                int thisYear = DateTime.Now.Year;
                int startYear = thisYear - 1;
                int endYear = 0;//开户时间(年) 
                if (int.TryParse(Res.OpenTime.Substring(0, 4), out temp))
                {
                    endYear = temp > thisYear - 5 ? temp : (thisYear - 5);
                }
                else
                {
                    endYear = thisYear - 5;//只查最近5年记录
                }
                //剩余年份
                for (int i = startYear; i >= endYear; i--)
                {
                    postdata = string.Format("__EVENTTARGET=ctl00%24ContentPlaceHolder2%24LinkButton1&__EVENTARGUMENT=&__VIEWSTATE={0}&__EVENTVALIDATION={1}&ctl00%24ContentPlaceHolder2%24DropDownList1={2}", VIEWSTATE.ToUrlEncode(), EVENTVALIDATION.ToUrlEncode(), i);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    Res = GetPageResults(httpResult);
                    //剩余页
                    if (int.TryParse(CommonFun.GetMidStr(httpResult.Html, "当前第1页/", "页").Trim(), out temp))
                    {
                        totalPages = temp;
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                        if (results.Count > 0)
                        {
                            VIEWSTATE = results[0];
                        }
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                        if (results.Count > 0)
                        {
                            EVENTVALIDATION = results[0];
                        }
                        for (int j = 2; j <= totalPages; j++)
                        {
                            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                            postdata = string.Format("__EVENTTARGET=ctl00%24ContentPlaceHolder1%24AspNetPager1&__EVENTARGUMENT={0}&__VIEWSTATE={1}&__EVENTVALIDATION={2}&ctl00%24ContentPlaceHolder2%24DropDownList1={3}", j, VIEWSTATE.ToUrlEncode(), EVENTVALIDATION.ToUrlEncode(), i);
                            httpItem = new HttpItem()
                            {
                                URL = Url,
                                Method = "post",
                                Postdata = postdata,
                                CookieCollection = cookies,
                                ResultCookieType = ResultCookieType.CookieCollection
                            };
                            httpResult = httpHelper.GetHtml(httpItem);
                            Res = GetPageResults(httpResult);
                        }
                    }
                }
                #endregion
                #region 第四步,贷款基本信息

                Url = baseUrl + "yuanjian/Personal/Personaldkinfo.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label19']", "text");
                if (results.Count > 0)
                {
                    if (string.IsNullOrEmpty(results[0].Trim()))
                    {
                        Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                        Res.StatusCode = ServiceConsts.StatusCode_success;
                        return Res;
                    }
                    Resload.IdentityCard = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label20']", "text");
                if (results.Count > 0)
                {
                    Resload.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label21']", "text");
                if (results.Count > 0)
                {
                    Resload.Couple_IdentifyCard = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label22']", "text");
                if (results.Count > 0)
                {
                    Resload.Couple_Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label25']", "text");
                if (results.Count > 0)
                {
                    Resload.Account_Loan = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label26']", "text");
                if (results.Count > 0)
                {
                    Resload.Repay_Type = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label27']", "text");
                if (results.Count > 0)
                {
                    Resload.Loan_Credit = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label28']", "text");
                if (results.Count > 0)
                {
                    Resload.Period_Total = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label29']", "text");
                if (results.Count > 0)
                {
                    Resload.Principal_Payed = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label30']", "text");
                if (results.Count > 0)
                {
                    Resload.Interest_Payed = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label31']", "text");
                if (results.Count > 0)
                {
                    Resload.Period_Payed = results[0].ToInt(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label32']", "text");
                if (results.Count > 0)
                {
                    Resload.Loan_Balance = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label33']", "text");
                if (results.Count > 0)
                {
                    Resload.Loan_Rate = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label34']", "text");
                if (results.Count > 0)
                {
                    Resload.Overdue_Period = results[0].ToInt(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label35']", "text");
                if (results.Count > 0)
                {
                    Resload.Overdue_Principal = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label36']", "text");
                if (results.Count > 0)
                {
                    Resload.Overdue_Interest = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label37']", "text");
                if (results.Count > 0)
                {
                    Resload.Current_Repay_Principal = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label39']", "text");
                if (results.Count > 0)
                {
                    Resload.Loan_Start_Date = Convert.ToDateTime(results[0]).ToString(Consts.DateFormatString5);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label40']", "text");
                if (results.Count > 0)
                {
                    Resload.Loan_End_Date = Convert.ToDateTime(results[0]).ToString(Consts.DateFormatString5);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label41']", "text");
                if (results.Count > 0)
                {
                    Resload.Address = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label43']", "text");
                if (results.Count > 0)
                {
                    Resload.Phone = results[0].Trim();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ctl00_ContentPlaceHolder1_Label44']", "text");
                if (results.Count > 0)
                {
                    if (string.IsNullOrEmpty(Resload.Phone))
                    {
                        Resload.Phone = results[0].Trim();
                    }
                }
                #endregion
                #region 第五步,贷款详细信息

                List<string> loadDetail = new List<string>();
                //初始化获得当年明细
                Url = baseUrl + "yuanjian/Personal/Personaldklist.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                VIEWSTATE = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                EVENTVALIDATION = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                //display:none;
                string display = HtmlParser.GetResultFromParser(httpResult.Html, "//tr[@id='ctl00_ContentPlaceHolder1_norow1']", "style")[0].ToLower();
                if (display.IndexOf("none") > -1)
                {
                    loadDetail = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='baselistTable']/tr[position()>1]", "");
                }
                int beginYear = DateTime.Now.AddYears(-1).Year;
                int times = 0;
                do
                {
                    postdata = string.Format("__EVENTTARGET=ctl00%24ContentPlaceHolder2%24LinkButton1&__EVENTARGUMENT=&__VIEWSTATE={0}&__EVENTVALIDATION={1}&ctl00%24ContentPlaceHolder2%24DropDownList1={2}", VIEWSTATE.ToUrlEncode(), EVENTVALIDATION.ToUrlEncode(), beginYear);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    display = HtmlParser.GetResultFromParser(httpResult.Html, "//tr[@id='ctl00_ContentPlaceHolder1_norow1']", "style")[0].ToLower();
                    if (display.IndexOf("none") > -1)
                    {
                        loadDetail.AddRange(HtmlParser.GetResultFromParser(httpResult.Html,
                            "//table[@class='baselistTable']/tr[position()>1]", ""));
                    }
                    else
                    {
                        times++;
                    }
                    VIEWSTATE = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                    EVENTVALIDATION = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                    beginYear--;
                } while ((beginYear >= 2000) & times < 2);
                foreach (string s in loadDetail)
                {
                    List<string> tdRow = HtmlParser.GetResultFromParser(s, "//td", "text");
                    if (tdRow.Count < 10) continue;

                    string record_Month = Convert.ToDateTime(tdRow[1]).ToString(Consts.DateFormatString7);
                    ProvidentFundLoanDetail detailRes = Resload.ProvidentFundLoanDetailList.FirstOrDefault(o=>o.Record_Month==record_Month);
                    bool needSave = false;
                    if (detailRes == null)
                    {
                        needSave = true;
                        detailRes = new ProvidentFundLoanDetail();
                        detailRes.Record_Month = record_Month;
                        detailRes.Description = tdRow[1];
                        detailRes.Record_Date = tdRow[1];
                        detailRes.Balance = tdRow[9].ToDecimal(0);
                    }
                    detailRes.Base += tdRow[2].ToDecimal(0);
                    detailRes.Interest += tdRow[4].ToDecimal(0);
                    detailRes.Overdue_Principal += tdRow[5].ToDecimal(0);
                    detailRes.Principal += tdRow[6].ToDecimal(0) + tdRow[8].ToDecimal(0);
                    detailRes.Overdue_Interest += tdRow[7].ToDecimal(0);
                    //描述
                    detailRes.Description = detailRes.Description.IndexOf(tdRow[1], StringComparison.Ordinal) > -1
                        ? detailRes.Description
                        : detailRes.Description + ";" + tdRow[1];
                    //记录日期
                    detailRes.Record_Date = Convert.ToDateTime(detailRes.Record_Date) > Convert.ToDateTime(tdRow[1])
                        ? tdRow[1]
                        : detailRes.Record_Date;
                    //余额
                    detailRes.Balance = detailRes.Balance < tdRow[9].ToDecimal(0) ? detailRes.Balance : tdRow[9].ToDecimal(0);
                    if (needSave)
                    {
                        Resload.ProvidentFundLoanDetailList.Add(detailRes);
                    }
                }
                #endregion
                Res.ProvidentFundLoanRes = Resload;
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
        /// 获取公积金页面明细信息
        /// </summary>
        /// <param name="httpResult">Http返回参数类</param>
        /// <returns>Res</returns>
        private ProvidentFundQueryRes GetPageResults(HttpResult httpResult)
        {
            if (httpResult != null)
            {
                List<string> detailList = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='baselistTable']/tr[position()>1]", "inner");
                if (detailList.Count == 0 || detailList[0].Contains("没有相关数据"))
                {
                    Res.StatusDescription = "暂无账户明细";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                SaveDetails(detailList);
            }
            return Res;
        }
        /// <summary>
        /// 保存公积金缴费明细
        /// </summary>
        /// <param name="detailList">缴费明细信息</param>
        private void SaveDetails(List<string> detailList)
        {
            if (detailList.Count > 0)
            {
                decimal perAccounting;//个人占比
                decimal comAccounting;//公司占比
                decimal totalRate;//总缴费比率
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
                foreach (var item in detailList)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow.Count < 8) continue;
                    ProvidentFundDetail detail = new ProvidentFundDetail();
                    detail.PayTime = Convert.ToDateTime(tdRow[3]);
                    detail.Description = tdRow[4];
                    if (tdRow[4].Trim().IndexOf("汇缴", System.StringComparison.Ordinal) == 0)
                    {
                        detail.ProvidentFundTime = string.IsNullOrEmpty(reg.Replace(tdRow[4], "")) ? Convert.ToDateTime(tdRow[3]).ToString("yyyyMM") : ("20" + reg.Replace(tdRow[4], ""));
                        detail.PersonalPayAmount = tdRow[6].ToDecimal(0) * perAccounting;//个人缴费金额
                        detail.CompanyPayAmount = tdRow[6].ToDecimal(0) * comAccounting;//企业缴费金额
                        detail.ProvidentFundBase = (tdRow[6].ToDecimal(0) / totalRate);//缴费基数
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        PaymentMonths++;
                    }
                    else if (tdRow[4].Trim().IndexOf("支取", System.StringComparison.Ordinal) > -1)
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = tdRow[5].ToDecimal(0);//金额
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = tdRow[6].ToDecimal(0);//金额
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
            }
        }
    }
}
