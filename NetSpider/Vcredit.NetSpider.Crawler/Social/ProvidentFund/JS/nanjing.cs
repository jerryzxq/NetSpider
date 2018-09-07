using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.JS
{
    public class nanjing : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.njgjj.com/";
        string fundCity = "js_nanjing";
        #endregion
        #region 私有变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        string Url = string.Empty;
        string postdata = string.Empty;
        decimal payRate = (decimal)0.07;
        List<string> results = new List<string>();
        ProvidentFundDetail detail = null;
        int PaymentMonths = 0;
        #endregion
        public VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "vericode.jsp";
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

                SpiderCacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return Res;
        }

        public ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            ProvidentFundReserveRes Res_Reserve = new ProvidentFundReserveRes();
            ProvidentFundLoanRes Res_Loan = new ProvidentFundLoanRes();
            Res.ProvidentFundCity = fundCity;
            bool HasReserve = false;
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(fundReq.Token);
                    SpiderCacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "身份证号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录系统
                Url = baseUrl + "per.login";
                postdata = String.Format("certinum={0}&perpwd={1}&vericode={2}", fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='WTLoginError']/ul/li[@class='text']", "text");

                if (httpResult.StatusCode != HttpStatusCode.OK || results.Count > 0)
                {
                    Res.StatusDescription = "登录失败," + results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (httpResult.Html.Contains("补贴账号"))//判定是否有补贴账号
                {
                    HasReserve = true;
                }
                #endregion

                #region 第二步，查询个人基本信息

                Url = baseUrl + "init.summer?_PROCID=80000003";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string allbankhtml = httpResult.Html;

                string jsonStr = CommonFun.GetMidStr(httpResult.Html, "poolSelect = {", "};");
                jsonStr = "{" + jsonStr + "}";
                IDictionary<string, string> dict = jsonParser.GetStringDictFromParser(jsonStr);

                string accname = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='accname']", "value")[0];
                string certinum = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='certinum']", "value")[0];
                string accnum = dict["_ACCNUM"];
                if (accname.IsEmpty())
                {
                    Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                postdata = "";
                foreach (KeyValuePair<string, string> item in dict)
                {
                    postdata += HttpUtility.UrlEncode(item.Key) + "=" + HttpUtility.UrlEncode(item.Value.ToString()) + "&";
                }
                string reservepostdata = postdata;
                postdata += "accname=" + HttpUtility.UrlEncode(accname);
                postdata += "&prodcode=1";//普通公积金
                postdata += "&accnum=" + accnum;

                Url = baseUrl + "command.summer";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ContentType = "application/x-www-form-urlencoded; charset=utf-8",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                #endregion

                #region 分析个人基本信息
                jsonStr = jsonParser.GetResultFromParser(httpResult.Html, "data");
                Res.Name = accname;//姓名
                Res.ProvidentFundNo = accnum;//公积金账号
                Res.IdentityCard = certinum;//身份证
                Res.CompanyNo = jsonParser.GetResultFromParser(jsonStr, "unitaccnum");//公司编号
                Res.CompanyName = HttpUtility.HtmlDecode(jsonParser.GetResultFromParser(jsonStr, "unitaccname"));//公司名称
                Res.OpenTime = jsonParser.GetResultFromParser(jsonStr, "opnaccdate");//开户时间
                Res.TotalAmount = jsonParser.GetResultFromParser(jsonStr, "amt1").ToDecimal(0);//账号余额
                Res.PersonalMonthPayRate = jsonParser.GetResultFromParser(jsonStr, "indiprop").ToDecimal(0);//个人比例：
                Res.PersonalMonthPayAmount = jsonParser.GetResultFromParser(jsonStr, "amt2").ToDecimal(0) / 2;//个人缴费金额
                Res.CompanyMonthPayRate = jsonParser.GetResultFromParser(jsonStr, "unitprop").ToDecimal(0);//单位比例
                Res.CompanyMonthPayAmount = Res.PersonalMonthPayAmount;//单位缴费金额
                string bankcode = jsonParser.GetResultFromParser(jsonStr, "instcode");
                List<string> bank = HtmlParser.GetResultFromParser(allbankhtml, "//option[@value='" + bankcode + "']");
                if (bank.Count > 0)
                    Res.Bank = bank[0];//开户行
                Res.BankCardNo = jsonParser.GetResultFromParser(jsonStr, "cardnocsp");//银行卡号
                Res.Phone = jsonParser.GetResultFromParser(jsonStr, "linkphone");//联系电话
                if (Res.PersonalMonthPayRate != 0)
                {
                    Res.SalaryBase = Res.PersonalMonthPayAmount / Res.PersonalMonthPayRate;
                }
                //账户状态
                switch (jsonParser.GetResultFromParser(jsonStr, "indiaccstate"))
                {
                    case "0": Res.Status = "正常"; break;
                    case "1": Res.Status = "封存"; break;
                    case "3": Res.Status = "空账"; break;
                    case "9": Res.Status = "销户"; break;
                }
                #endregion
                
                #region 第三步，缴费明细
                Url = baseUrl + "init.summer?_PROCID=70000002";

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                jsonStr = CommonFun.GetMidStr(httpResult.Html, "poolSelect = {", "};");
                jsonStr = "{" + jsonStr + "}";
                dict = jsonParser.GetStringDictFromParser(jsonStr);
                string DATAlISTGHOST = HtmlParser.GetResultFromParser(httpResult.Html, "//textarea[@name='DATAlISTGHOST']", "", true)[0];
                string _DATAPOOL_ = HtmlParser.GetResultFromParser(httpResult.Html, "//textarea[@name='_DATAPOOL_']", "", true)[0];

                postdata = "";
                foreach (KeyValuePair<string, string> item in dict)
                {
                    postdata += HttpUtility.UrlEncode(item.Key) + "=" + HttpUtility.UrlEncode(item.Value.ToString()) + "&";
                }
                postdata += "&begdate=2005-01-01";
                postdata += "&enddate=" + DateTime.Now.ToString(Consts.DateFormatString2);
                postdata += "&accname=" + accname.ToUrlEncode();
                postdata += "&accnum=" + accnum;
                Url = baseUrl + "command.summer";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);


                int currentPage = 0;
                int pageCount = 0;
                ProvidentFundDetail detail = null;
                int PaymentMonths = 0;
                string ProvidentFundTime = string.Empty;
                do
                {
                    postdata = "dynamicTable_id=datalist2";
                    postdata += "&dynamicTable_currentPage=" + currentPage;
                    postdata += "&dynamicTable_pageSize=10";
                    postdata += "&dynamicTable_nextPage=" + (currentPage + 1);
                    postdata += "&dynamicTable_page=%2Fydpx%2F70000002%2F700002_01.ydpx";
                    postdata += "&dynamicTable_paging=true";
                    postdata += "&dynamicTable_configSqlCheck=0";
                    postdata += "&errorFilter=1%3D1";
                    postdata += "&begdate=2005-01-01";
                    postdata += "&enddate=" + DateTime.Now.ToString(Consts.DateFormatString2);
                    postdata += "&accnum=" + accnum;
                    postdata += "&accname=" + accname.ToUrlEncode();
                    postdata += "&_APPLY=0&_CHANNEL=1&_PROCID=70000002";
                    postdata += "&DATAlISTGHOST=" + DATAlISTGHOST.ToUrlEncode();
                    postdata += "&_DATAPOOL_=" + _DATAPOOL_.ToUrlEncode();
                    currentPage++;
                    Url = "http://www.njgjj.com/dynamictable";

                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    jsonStr = jsonParser.GetResultFromParser(httpResult.Html, "data");
                    currentPage = jsonParser.GetResultFromParser(jsonStr, "currentPage").ToInt(0);
                    pageCount = jsonParser.GetResultFromParser(jsonStr, "pageCount").ToInt(0);

                    jsonStr = jsonParser.GetResultFromParser(jsonStr, "data");
                    var details = jsonParser.DeserializeObject<List<NanJingDetail>>(jsonStr);
                    //缴费信息
                    foreach (var item in details)
                    {
                        detail = new ProvidentFundDetail();
                        detail.PayTime = item.transdate.ToDateTime();
                        detail.CompanyName = item.unitaccname;
                        detail.Description = item.reason;
                        if (item.reason.IndexOf("汇缴", StringComparison.Ordinal) != -1)
                        {
                            var ptimes = CommonFun.GetMidStr(item.reason.ToTrim(), "汇缴[", "]").Split('-');
                            if (ptimes.Length == 2)
                            {
                                int pmonth = ptimes[1].ToInt(0);
                                ProvidentFundTime = ptimes[0] + (pmonth >= 10 ? pmonth.ToString() : "0" + pmonth);
                            }
                            detail.ProvidentFundTime = ProvidentFundTime;
                            detail.PersonalPayAmount = item.basenum.ToDecimal(0) / 2;//金额
                            detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                            detail.ProvidentFundBase = (detail.PersonalPayAmount / Res.PersonalMonthPayRate).ToString("f2").ToDecimal(0);
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            PaymentMonths++;
                        }
                        else if (item.reason.Contains("还贷") || item.reason.Contains("提取"))
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                            detail.PersonalPayAmount = item.basenum.ToDecimal(0);
                        }
                        else
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.PersonalPayAmount = item.basenum.ToDecimal(0);
                        }
                        Res.ProvidentFundDetailList.Add(detail);
                    }
                }
                while (currentPage != pageCount);
                
                #endregion


                #region 贷款

                #region 第四步，贷款基本信息
                Url = baseUrl + "init.summer?_PROCID=60000005";
                httpItem = new HttpItem
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input", "outerhtml");
                string _html_dic = httpResult.Html;
                jsonStr = CommonFun.GetMidStr(httpResult.Html, "poolSelect = {", "};");
                jsonStr = "{" + jsonStr + "}";
                try
                {
                    dict = jsonParser.GetStringDictFromParser(jsonStr);
                }
                catch
                {
                    goto End;
                }
                foreach (string input in results)
                {
                    if (!dict.ContainsKey(HtmlParser.GetResultFromParser(input, "input", "name")[0]))
                    {
                        dict.Add(HtmlParser.GetResultFromParser(input, "input", "name")[0], HtmlParser.GetResultFromParser(input, "input", "value")[0]);
                    }
                    else
                    {
                        if (!HtmlParser.GetResultFromParser(input, "input", "value")[0].IsEmpty())
                        {
                            dict[HtmlParser.GetResultFromParser(input, "input", "name")[0]] = HtmlParser.GetResultFromParser(input, "input", "value")[0];
                        }
                    }
                }

                postdata = "";
                foreach (KeyValuePair<string, string> item in dict)
                {
                    postdata += HttpUtility.UrlEncode(item.Key) + "=" + HttpUtility.UrlEncode(item.Value.ToString()) + "&";
                }

                Url = baseUrl + "command.summer?uuid=" + CommonFun.GetTimeStamp();
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                if (!jsonParser.GetResultFromParser(httpResult.Html, "message").IsEmpty())//获取失败
                {
                    goto End;
                }

                jsonStr = jsonParser.GetResultFromParser(httpResult.Html, "data");

                Res_Loan.Account_Loan = jsonParser.GetResultFromParser(jsonStr, "oldrepayaccnum");//贷款账号

                string Loan_Type = jsonParser.GetResultFromParser(jsonStr, "commloanflag");
                results = HtmlParser.GetResultFromParser(_html_dic, "//select[@name='commloanflag']/option[@value='" + Loan_Type + "']");
                if (results.Count > 0 && !results[0].Contains("请选择"))
                {
                    Res_Loan.Loan_Type = results[0];//贷款类型
                }
                string House_Purchase_Type = jsonParser.GetResultFromParser(jsonStr, "buyhousetype");
                results = HtmlParser.GetResultFromParser(_html_dic, "//select[@name='buyhousetype']/option[@value='" + House_Purchase_Type + "']");
                if (results.Count > 0 && !results[0].Contains("请选择"))
                {
                    Res_Loan.House_Purchase_Type = results[0];//购房类型
                }
                string House_Type = jsonParser.GetResultFromParser(jsonStr, "housetype");
                results = HtmlParser.GetResultFromParser(_html_dic, "//select[@name='housetype']/option[@value='" + House_Type + "']");
                if (results.Count > 0 && !results[0].Contains("请选择"))
                {
                    Res_Loan.House_Type = results[0];//房屋类型
                }
                string Status = jsonParser.GetResultFromParser(jsonStr, "loanstate");
                results = HtmlParser.GetResultFromParser(_html_dic, "//select[@name='loanstate']/option[@value='" + Status + "']");
                if (results.Count > 0 && !results[0].Contains("请选择"))
                {
                    Res_Loan.Status = results[0];//状态
                }
                Res_Loan.Name = jsonParser.GetResultFromParser(jsonStr, "accname");//姓名
                Res_Loan.IdentityCard = jsonParser.GetResultFromParser(jsonStr, "certinum");//身份证
                Res_Loan.Couple_Name = jsonParser.GetResultFromParser(jsonStr, "matename");//配偶姓名
                Res_Loan.Couple_IdentifyCard = jsonParser.GetResultFromParser(jsonStr, "matecertinum");//配偶身份证
                Res_Loan.Couple_CompanyName = jsonParser.GetResultFromParser(jsonStr, "oldunitaccname");//配偶单位名称
                Res_Loan.Couple_Phone = jsonParser.GetResultFromParser(jsonStr, "linkphone");//配偶手机号
                Res_Loan.Couple_ProvidentFundNo = jsonParser.GetResultFromParser(jsonStr, "accnum3");//配偶公积金账号
                Res_Loan.Couple_MonthPay = jsonParser.GetResultFromParser(jsonStr, "repayamt").ToDecimal(0);//配偶月缴额
                string Couple_Status = jsonParser.GetResultFromParser(jsonStr, "accstate");
                results = HtmlParser.GetResultFromParser(_html_dic, "//select[@name='accstate']/option[@value='" + Couple_Status + "']");
                if (results.Count > 0 && !results[0].Contains("请选择"))
                {
                    Res_Loan.Couple_Status = results[0];//状态
                }
                Res_Loan.Couple_Balance = jsonParser.GetResultFromParser(jsonStr, "usebal").ToDecimal(0);//配偶账户余额
                Res_Loan.Loan_Credit = jsonParser.GetResultFromParser(jsonStr, "loanamt").ToDecimal(0);//贷款金额
                Res_Loan.Loan_Rate = jsonParser.GetResultFromParser(jsonStr, "loanrate");//贷款利率
                Res_Loan.Principal_Payed = jsonParser.GetResultFromParser(jsonStr, "repayprin").ToDecimal(0);//已还本金
                Res_Loan.Interest_Payed = jsonParser.GetResultFromParser(jsonStr, "repayint").ToDecimal(0);//已还利息
                Res_Loan.Loan_Start_Date = jsonParser.GetResultFromParser(jsonStr, "begdate");//贷款开始日期
                Res_Loan.Loan_End_Date = jsonParser.GetResultFromParser(jsonStr, "enddate");//贷款结束日期
                Res_Loan.Loan_Actual_End_Date = jsonParser.GetResultFromParser(jsonStr, "enddate2");//实际终止日期
                Res_Loan.LatestRepayTime = jsonParser.GetResultFromParser(jsonStr, "lasttransdate");//最近还款日期
                Res_Loan.Account_Repay = jsonParser.GetResultFromParser(jsonStr, "earnstbankaccnum");//开户银行账号
                Res_Loan.Period_Payed = jsonParser.GetResultFromParser(jsonStr, "termnum").ToInt(0);//已还款期数
                Res_Loan.Period_Total = (jsonParser.GetResultFromParser(jsonStr, "remainterm").ToInt(0) + Res_Loan.Period_Payed).ToString();//还款期数
                Res_Loan.Current_Repay_Date = jsonParser.GetResultFromParser(jsonStr, "repaydate");//当期还款日期
                Res_Loan.Current_Repay_Total = jsonParser.GetResultFromParser(jsonStr, "plandedprin").ToDecimal(0);//当期还款金额
                Res_Loan.Current_Repay_Interest = jsonParser.GetResultFromParser(jsonStr, "planint").ToDecimal(0);//当期还款利息
                Res_Loan.Current_Repay_Principal = jsonParser.GetResultFromParser(jsonStr, "planprin").ToDecimal(0);//当期还款本金
                Res_Loan.Overdue_Period = jsonParser.GetResultFromParser(jsonStr, "owecnt").ToInt(0);//逾期期数（网站）
                Res_Loan.Overdue_Interest = jsonParser.GetResultFromParser(jsonStr, "oweprin").ToDecimal(0);//逾期利息（网站）
                Res_Loan.Overdue_Principal = jsonParser.GetResultFromParser(jsonStr, "amt3").ToDecimal(0);//逾期本金（网站）
                Res_Loan.Interest_UnPayed = jsonParser.GetResultFromParser(jsonStr, "oweint").ToDecimal(0);//应还未还利息
                Res_Loan.Interest_Penalty = jsonParser.GetResultFromParser(jsonStr, "repaypun").ToDecimal(0);//罚息
                string Withdrawal_Type = jsonParser.GetResultFromParser(jsonStr, "fundrpykind");
                results = HtmlParser.GetResultFromParser(_html_dic, "//select[@name='fundrpykind']/option[@value='" + Withdrawal_Type + "']");
                if (results.Count > 0 && !results[0].Contains("请选择"))
                {
                    Res_Loan.Withdrawal_Type = results[0];//支取还贷类型
                }
                string Repay_Type = jsonParser.GetResultFromParser(jsonStr, "repaymode");
                results = HtmlParser.GetResultFromParser(_html_dic, "//select[@name='repaymode']/option[@value='" + Repay_Type + "']");
                if (results.Count > 0 && !results[0].Contains("请选择"))
                {
                    Res_Loan.Repay_Type = results[0];//还款方式
                }
                Res_Loan.Delegate_Date = jsonParser.GetResultFromParser(jsonStr, "transdate");//委托划转签订日期
                Res_Loan.ProvidentFundNo = jsonParser.GetResultFromParser(jsonStr, "cardaccnum");//公积金账号
                #endregion
                
                #region 第五步，贷款明细
                Url = baseUrl + "init.summer?_PROCID=60000001";
                httpItem = new HttpItem
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                
                try
                {
                    DATAlISTGHOST = HtmlParser.GetResultFromParser(httpResult.Html, "//textarea[@name='DATAlISTGHOST']", "", true)[0];
                    _DATAPOOL_ = HtmlParser.GetResultFromParser(httpResult.Html, "//textarea[@name='_DATAPOOL_']", "", true)[0];

                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input", "outerhtml");
                    jsonStr = CommonFun.GetMidStr(httpResult.Html, "poolSelect = {", "};");
                    jsonStr = "{" + jsonStr + "}";
                    dict = jsonParser.GetStringDictFromParser(jsonStr);
                }
                catch
                {
                    goto End;
                }
                foreach (string input in results)
                {
                    if (!dict.ContainsKey(HtmlParser.GetResultFromParser(input, "input", "name")[0]))
                    {
                        dict.Add(HtmlParser.GetResultFromParser(input, "input", "name")[0], HtmlParser.GetResultFromParser(input, "input", "value")[0]);
                    }
                    else
                    {
                        if (!HtmlParser.GetResultFromParser(input, "input", "value")[0].IsEmpty())
                        {
                            dict[HtmlParser.GetResultFromParser(input, "input", "name")[0]] = HtmlParser.GetResultFromParser(input, "input", "value")[0];
                        }
                    }
                }
                dict["begdate"] = Res_Loan.Loan_Start_Date.IsEmpty() ? "1990-01-01" : Res_Loan.Loan_Start_Date;
                dict["enddate"] = DateTime.Now.ToString(Consts.DateFormatString2);

                postdata = "";
                foreach (KeyValuePair<string, string> item in dict)
                {
                    postdata += HttpUtility.UrlEncode(item.Key) + "=" + HttpUtility.UrlEncode(item.Value.ToString()) + "&";
                }
                bool IsOK = false;
                for (int i = 0; i < 5; i++)
                {
                    Url = baseUrl + "command.summer?uuid=" + CommonFun.GetTimeStamp();
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    try
                    {
                        if (jsonParser.GetResultFromParser(httpResult.Html, "returnCode") == "0")//获取成功
                        {
                            IsOK = true;
                            break;
                        }
                        if (jsonParser.GetResultFromParser(httpResult.Html, "message") == "\u51fa\u9519:\u8d37\u6b3e\u8fd8\u6b3e\u660e\u7ec6\u4fe1\u606f\u4e0d\u5b58\u5728\uff0c\u8bf7\u6838\u5bf9\uff01")//出错,无信息
                        {
                            goto End;
                        }
                    }
                    catch
                    {
                    }
                }
                if (!IsOK)
                {
                    goto End;
                }

                currentPage = 0;
                pageCount = 0;
                do
                {
                    postdata = "dynamicTable_id=datalist";
                    postdata += "&dynamicTable_currentPage=" + currentPage;
                    postdata += "&dynamicTable_pageSize=1000";
                    postdata += "&dynamicTable_nextPage=" + (currentPage + 1);
                    postdata += "&dynamicTable_page=%2Fydpx%2F60000001%2F600001_01.ydpx";
                    postdata += "&dynamicTable_paging=true";
                    postdata += "&dynamicTable_configSqlCheck=0";
                    postdata += "&errorFilter=1%3D1";
                    postdata += "&begdate=" + (Res_Loan.Loan_Start_Date.IsEmpty() ? "1990-01-01" : Res_Loan.Loan_Start_Date);
                    postdata += "&enddate=" + DateTime.Now.ToString(Consts.DateFormatString2);
                    postdata += "&loanaccnum=";
                    postdata += "&accname=" + accname.ToUrlEncode();
                    postdata += "&_APPLY=0&_CHANNEL=1&_PROCID=60000001";
                    postdata += "&DATAlISTGHOST=" + DATAlISTGHOST.ToUrlEncode();
                    postdata += "&_DATAPOOL_=" + _DATAPOOL_.ToUrlEncode();
                    currentPage++;

                    Url = baseUrl + "dynamictable?uuid=" + CommonFun.GetTimeStamp();
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    try
                    {
                        if (!jsonParser.GetResultFromParser(httpResult.Html, "message").IsEmpty())//获取失败
                        {
                            goto End;
                        }
                    }
                    catch
                    {
                        goto End;
                    }

                    jsonStr = jsonParser.GetResultFromParser(httpResult.Html, "data");
                    if (pageCount == 0)
                    {
                        pageCount = jsonParser.GetResultFromParser(jsonStr, "pageCount").ToInt(0);
                    }

                    //还款类型： '01':'本金','02':'逾期本金','03':'利息','13':'逾期利息','18':'罚息'
                    //还贷来源：'1':'现金','2':'住房公积金','3':'现金和公积金','4':'保证金','5':'贷款风险准备金'
                    List<string> _loan_detail_list = jsonParser.GetArrayFromParse(jsonStr, "data");
                    foreach (string item in _loan_detail_list)
                    {
                        string period = jsonParser.GetResultFromParser(item, "termnum");
                        if(period.IsEmpty())
                        {
                            continue;
                        }
                        ProvidentFundLoanDetail _loan_detail = Res_Loan.ProvidentFundLoanDetailList.Where(o => o.Record_Period == period).FirstOrDefault();
                        bool NeedAdd = false;
                        if (_loan_detail == null)
                        {
                            _loan_detail = new ProvidentFundLoanDetail();
                            _loan_detail.Record_Period = period;
                            NeedAdd = true;
                        }
                        decimal cash = jsonParser.GetResultFromParser(item, "transamt").ToDecimal(0);
                        switch (jsonParser.GetResultFromParser(item, "loanfundtype"))
                        {
                            case "01":
                                _loan_detail.Principal += cash;
                                _loan_detail.Record_Date = jsonParser.GetResultFromParser(item, "transdate");
                                _loan_detail.Bill_Date = _loan_detail.Record_Date;
                                break;
                            case "02":
                                _loan_detail.Overdue_Principal += cash;
                                break;
                            case "03":
                                _loan_detail.Interest += cash;
                                break;
                            case "13":
                                _loan_detail.Overdue_Interest += cash;
                                break;
                            case "18":
                                _loan_detail.Interest_Penalty += cash;
                                break;
                        }
                        _loan_detail.Base += cash;
                        string cash_type = string.Empty;
                        switch (jsonParser.GetResultFromParser(item, "fundsource"))
                        {
                            case "1":
                                cash_type = "现金";
                                break;
                            case "2":
                                cash_type = "住房公积金";
                                break;
                            case "3":
                                cash_type = "现金和公积金";
                                break;
                            case "4":
                                cash_type = "保证金";
                                break;
                            case "5":
                                cash_type = "现金贷款风险准备金";
                                break;
                        }
                        if (!cash_type.IsEmpty())
                        {
                            if (_loan_detail.Description.IsEmpty())
                            {
                                _loan_detail.Description += cash_type;
                            }
                            else if (!_loan_detail.Description.Contains(cash_type))
                            {
                                _loan_detail.Description += "；" + cash_type;
                            }
                        }
                        if (NeedAdd)
                        {
                            Res_Loan.ProvidentFundLoanDetailList.Add(_loan_detail);
                        }
                    }

                }
                while (currentPage < pageCount);
                #endregion

                #endregion
                

                End:
                #region 补充公积金
                if (HasReserve)
                {
                    #region 基本信息
                    Url = baseUrl + "init.summer?_PROCID=80000003";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    allbankhtml = httpResult.Html;

                    jsonStr = CommonFun.GetMidStr(httpResult.Html, "poolSelect = {", "};");
                    jsonStr = "{" + jsonStr + "}";
                    dict = jsonParser.GetStringDictFromParser(jsonStr);

                    accname = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='accname']", "value")[0];
                    certinum = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='certinum']", "value")[0];
                    accnum = dict["_ACCNUM"];
                    if (accname.IsEmpty())
                    {
                        Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryFail;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }

                    postdata = "";
                    foreach (KeyValuePair<string, string> item in dict)
                    {
                        postdata += HttpUtility.UrlEncode(item.Key) + "=" + HttpUtility.UrlEncode(item.Value.ToString()) + "&";
                    }

                    postdata += "accname=" + HttpUtility.UrlEncode(accname);
                    postdata += "&prodcode=2";//补贴公积金
                    postdata += "&accnum=" + accnum;

                    Url = baseUrl + "command.summer";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ContentType = "application/x-www-form-urlencoded; charset=utf-8",
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    jsonStr = jsonParser.GetResultFromParser(httpResult.Html, "data");
                    Res_Reserve.ProvidentFundNo = accnum;//公积金账号
                    Res_Reserve.CompanyNo = jsonParser.GetResultFromParser(jsonStr, "unitaccnum");//公司编号
                    Res_Reserve.CompanyName = HttpUtility.HtmlDecode(jsonParser.GetResultFromParser(jsonStr, "unitaccname"));//公司名称
                    Res_Reserve.OpenTime = jsonParser.GetResultFromParser(jsonStr, "opnaccdate");//开户时间
                    Res_Reserve.TotalAmount = jsonParser.GetResultFromParser(jsonStr, "amt1").ToDecimal(0);//账号余额
                    Res_Reserve.PersonalMonthPayRate = jsonParser.GetResultFromParser(jsonStr, "indiprop").ToDecimal(0);//个人比例
                    Res_Reserve.CompanyMonthPayRate = jsonParser.GetResultFromParser(jsonStr, "unitprop").ToDecimal(0);//单位比例
                    decimal p_rate = 0;
                    if (Res_Reserve.PersonalMonthPayRate == 0)//个人缴费比例为0
                    {
                        Res_Reserve.CompanyMonthPayAmount = jsonParser.GetResultFromParser(jsonStr, "amt2").ToDecimal(0);//单位缴费金额
                    }
                    else if (Res_Reserve.CompanyMonthPayRate == 0)//单位缴费比例为0
                    {
                        Res_Reserve.PersonalMonthPayAmount = jsonParser.GetResultFromParser(jsonStr, "amt2").ToDecimal(0);//个人缴费金额
                    }
                    else
                    {
                        p_rate = Res_Reserve.PersonalMonthPayRate / (Res_Reserve.CompanyMonthPayRate + Res_Reserve.PersonalMonthPayRate);//个人缴费占月缴费比例

                        Res_Reserve.PersonalMonthPayAmount = jsonParser.GetResultFromParser(jsonStr, "amt2").ToDecimal(0) * p_rate;//个人缴费金额
                        Res_Reserve.CompanyMonthPayAmount = jsonParser.GetResultFromParser(jsonStr, "amt2").ToDecimal(0) - Res_Reserve.PersonalMonthPayAmount;//单位缴费金额
                    }
                    bankcode = jsonParser.GetResultFromParser(jsonStr, "instcode");
                    bank = HtmlParser.GetResultFromParser(allbankhtml, "//option[@value='" + bankcode + "']");
                    if (bank.Count > 0)
                        Res_Reserve.Bank = bank[0];//开户行
                    Res_Reserve.BankCardNo = jsonParser.GetResultFromParser(jsonStr, "cardnocsp");//银行卡号
                    if (Res_Reserve.PersonalMonthPayRate != 0)
                    {
                        Res_Reserve.SalaryBase = Res_Reserve.PersonalMonthPayAmount / Res_Reserve.PersonalMonthPayRate;
                    }
                    else if (Res_Reserve.CompanyMonthPayAmount != 0)
                    {
                        Res_Reserve.SalaryBase = Res_Reserve.CompanyMonthPayAmount / Res_Reserve.CompanyMonthPayRate;
                    }
                    //账户状态
                    switch (jsonParser.GetResultFromParser(jsonStr, "indiaccstate"))
                    {
                        case "0": Res_Reserve.Status = "正常"; break;
                        case "1": Res_Reserve.Status = "封存"; break;
                        case "3": Res_Reserve.Status = "空账"; break;
                        case "9": Res_Reserve.Status = "销户"; break;
                    }

                    #endregion

                    #region  明细
                    Url = baseUrl + "platform/frames.jsp?systype=1&acctype=1";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    Url = baseUrl + "init.summer?_PROCID=70000002";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    jsonStr = CommonFun.GetMidStr(httpResult.Html, "poolSelect = {", "};");
                    jsonStr = "{" + jsonStr + "}";
                    dict = jsonParser.GetStringDictFromParser(jsonStr);
                    DATAlISTGHOST = HtmlParser.GetResultFromParser(httpResult.Html, "//textarea[@name='DATAlISTGHOST']", "", true)[0];
                    _DATAPOOL_ = HtmlParser.GetResultFromParser(httpResult.Html, "//textarea[@name='_DATAPOOL_']", "", true)[0];
                    accnum = dict["_ACCNUM"];

                    postdata = "";
                    foreach (KeyValuePair<string, string> item in dict)
                    {
                        postdata += HttpUtility.UrlEncode(item.Key) + "=" + HttpUtility.UrlEncode(item.Value.ToString()) + "&";
                    }
                    postdata += "&begdate=2005-01-01";
                    postdata += "&enddate=" + DateTime.Now.ToString(Consts.DateFormatString2);
                    postdata += "&accname=" + accname.ToUrlEncode();
                    postdata += "&accnum=" + accnum;
                    Url = baseUrl + "command.summer";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);


                    currentPage = 0;
                    pageCount = 0;
                    detail = null;
                    int PaymentMonths_Reserve = 0;
                    ProvidentFundTime = string.Empty;
                    do
                    {
                        postdata = "dynamicTable_id=datalist2";
                        postdata += "&dynamicTable_currentPage=" + currentPage;
                        postdata += "&dynamicTable_pageSize=10";
                        postdata += "&dynamicTable_nextPage=" + (currentPage + 1);
                        postdata += "&dynamicTable_page=%2Fydpx%2F70000002%2F700002_01.ydpx";
                        postdata += "&dynamicTable_paging=true";
                        postdata += "&dynamicTable_configSqlCheck=0";
                        postdata += "&errorFilter=1%3D1";
                        postdata += "&begdate=2005-01-01";
                        postdata += "&enddate=" + DateTime.Now.ToString(Consts.DateFormatString2);
                        postdata += "&accnum=" + accnum;
                        postdata += "&accname=" + accname.ToUrlEncode();
                        postdata += "&_APPLY=0&_CHANNEL=1&_PROCID=70000002";
                        postdata += "&DATAlISTGHOST=" + DATAlISTGHOST.ToUrlEncode();
                        postdata += "&_DATAPOOL_=" + _DATAPOOL_.ToUrlEncode();
                        currentPage++;
                        Url = "http://www.njgjj.com/dynamictable";

                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "post",
                            Postdata = postdata,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        jsonStr = jsonParser.GetResultFromParser(httpResult.Html, "data");
                        currentPage = jsonParser.GetResultFromParser(jsonStr, "currentPage").ToInt(0);
                        pageCount = jsonParser.GetResultFromParser(jsonStr, "pageCount").ToInt(0);

                        jsonStr = jsonParser.GetResultFromParser(jsonStr, "data");
                        var details = jsonParser.DeserializeObject<List<NanJingDetail>>(jsonStr);
                        //缴费信息
                        foreach (var item in details)
                        {
                            detail = new ProvidentFundDetail();
                            detail.PayTime = item.transdate.ToDateTime();
                            detail.CompanyName = item.unitaccname;
                            detail.Description = item.reason;
                            if (item.reason.IndexOf("汇缴", StringComparison.Ordinal) != -1)
                            {
                                var ptimes = CommonFun.GetMidStr(item.reason.ToTrim(), "汇缴[", "]").Split('-');
                                if (ptimes.Length == 2)
                                {
                                    int pmonth = ptimes[1].ToInt(0);
                                    ProvidentFundTime = ptimes[0] + (pmonth >= 10 ? pmonth.ToString() : "0" + pmonth);
                                }
                                detail.ProvidentFundTime = ProvidentFundTime;
                                if (Res_Reserve.PersonalMonthPayRate == 0)
                                {
                                    detail.CompanyPayAmount = item.basenum.ToDecimal(0);//单位缴费金额
                                    detail.ProvidentFundBase = (detail.CompanyPayAmount / Res_Reserve.CompanyMonthPayRate).ToString("f2").ToDecimal(0);
                                }
                                else if (Res_Reserve.CompanyMonthPayRate == 0)
                                {
                                    detail.PersonalPayAmount = item.basenum.ToDecimal(0);//个人缴费金额
                                    detail.ProvidentFundBase = (detail.PersonalPayAmount / Res_Reserve.PersonalMonthPayRate).ToString("f2").ToDecimal(0);
                                }
                                else if (p_rate != 0)
                                {
                                    detail.PersonalPayAmount = item.basenum.ToDecimal(0) * p_rate;//个人缴费金额
                                    detail.CompanyPayAmount = item.basenum.ToDecimal(0) - detail.PersonalPayAmount;//单位缴费金额
                                    detail.ProvidentFundBase = (detail.PersonalPayAmount / Res_Reserve.PersonalMonthPayRate).ToString("f2").ToDecimal(0);
                                }

                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                                PaymentMonths_Reserve++;
                            }
                            else if (item.reason.Contains("还贷") || item.reason.Contains("提取"))
                            {
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                                detail.PersonalPayAmount = item.basenum.ToDecimal(0);
                            }
                            else
                            {
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                                detail.PersonalPayAmount = item.basenum.ToDecimal(0);
                            }
                            Res_Reserve.ProvidentReserveFundDetailList.Add(detail);
                        }
                    }
                    while (currentPage != pageCount);
                    #endregion
                }
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.ProvidentFundReserveRes = Res_Reserve;
                Res.ProvidentFundLoanRes = Res_Loan;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = e.ToString();
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
    }
    internal class NanJingDetail
    {
        /// <summary>
        /// 单位名称
        /// </summary>
        public string unitaccname { get; set; }
        /// <summary>
        /// 交易日期
        /// </summary>
        public string transdate { get; set; }
        /// <summary>
        /// 缴费说明（有公积金所属年月）
        /// </summary>
        public string reason { get; set; }
        /// <summary>
        ///余额
        /// </summary>
        public string payvouamt { get; set; }
        /// <summary>
        /// 缴费金额
        /// </summary>
        public string basenum { get; set; }
    }
}
