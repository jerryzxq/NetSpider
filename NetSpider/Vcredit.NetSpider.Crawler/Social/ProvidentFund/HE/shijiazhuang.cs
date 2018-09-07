using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HE
{
    /// <summary>
    /// 石家庄公积金网站17：30后不提供查询业务
    /// </summary>
    public class shijiazhuang : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://110.249.253.234/webQ/netQuery/";
        string fundCity = "he_shijiazhuang";
        #endregion
        #region 私有变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        List<string> _results = new List<string>();
        ProvidentFundDetail detail = null;
        string _url = string.Empty;
        string _postdata = string.Empty;
        decimal payRate = (decimal)0.08;
        int PaymentMonths = 0;
        private Regex reg = new Regex(@"[\s;\,\&nbsp;\-;]*");
        #endregion
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                DateTime dt = DateTime.Now;
                string utcNow = Regex.Replace((dt.ToString("r") + dt.ToString("zzz")), @"[\,;\:]", "").ToUrlEncode();//格林威治时间 
                _url = baseUrl + "include/ranCode.jsp?tab=yzm&t=" + utcNow;
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    vcRes.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    vcRes.StatusCode = ServiceConsts.StatusCode_fail;
                    return vcRes;
                }
                vcRes.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                vcRes.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                //合并缓存
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //保存缓存
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                vcRes.StatusCode = ServiceConsts.StatusCode_error;
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return vcRes;
        }

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(ProvidentFundReq fundReq)
        {
            Res.ProvidentFundCity = fundCity;
            string trancode = string.Empty;
            string loginType = string.Empty;//登录方式[1:身份证,2:公积金账号]
            string reTrancode = string.Empty;//多账号选择标志
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.LoginType == "2")
                {
                    loginType = "1";
                    if (string.IsNullOrWhiteSpace(fundReq.Identitycard) || string.IsNullOrWhiteSpace(fundReq.Password))
                    {
                        Res.StatusDescription = "身份证或密码不能为空";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                else
                {
                    loginType = "2";
                    if (string.IsNullOrWhiteSpace(fundReq.Username) || string.IsNullOrWhiteSpace(fundReq.Password))
                    {
                        Res.StatusDescription = "公积金账号或密码不能为空";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    fundReq.Identitycard = "";
                    Res.ProvidentFundNo = fundReq.Username.Trim();
                }
                #region 第一步,登陆

                string errorMsc = string.Empty;
                //初始化页面开始
                _url = baseUrl + "queryPage/netQuery1.jsp";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='trancode']", "value");
                if (_results.Count > 0)
                {
                    trancode = _results[0];
                }
                //初始化页面结束
                _url = baseUrl + "queryPage/webQ.login";
                _postdata = string.Format("trancode={6}&type={5}&certinum={0}&accnum={1}&cardno=&accname={2}&pwd={3}&tYzm={4}", fundReq.Identitycard, fundReq.Username, fundReq.Name.ToUrlEncode(), fundReq.Password, fundReq.Vercode, loginType, trancode);
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Postdata = _postdata,
                    Method = "Post",
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//fieldset[@style='width:80%;text-align:center']", "text");
                if (_results.Count > 0)
                {
                    Res.StatusDescription = reg.Replace(_results[0], "");
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string loancontrnum = string.Empty;
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='list1']/tr/td");
                if (_results.Count == 24)
                {
                    loancontrnum = _results[6];
                }
                string ProvidentFundNo = string.Empty;
                //身份证登陆 可能存在多账号  获取正常账号信息
                if (loginType == "1")
                {
                    //单账号
                    ProvidentFundNo = CommonFun.GetMidStr(httpResult.Html, "accnum='", "';");
                    if (ProvidentFundNo.Contains("html"))
                    {
                        ProvidentFundNo = "";
                    }
                    if (!string.IsNullOrWhiteSpace(ProvidentFundNo))
                    {
                        Res.ProvidentFundNo = ProvidentFundNo.Trim(); //公积金账号 html
                    }
                    else
                    {//多账号获取 正常公积金账号
                        _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='trancode']", "value");
                        if (_results.Count < 1)
                        {
                            Res.StatusDescription = "账号信息列表页已改版,信息获取失败";
                            Res.StatusCode = ServiceConsts.StatusCode_fail;
                            return Res;
                        }
                        reTrancode = _results[0];

                        _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='list1']/tr[position()>1]", "inner");
                        if (_results.Count < 1)
                        {
                            Res.StatusDescription = "未找到用户公积金信息,请核对";
                            Res.StatusCode = ServiceConsts.StatusCode_fail;
                            return Res;
                        }
                        //用户有多个账户,但由于ProvidentFundQueryRes类未完善只能保存循环最后一条正常账户的信息
                        foreach (string item in _results)
                        {
                            var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text");
                            if (tdRow[5].Trim() == "0")//[0:正常,2:空账,9:销户]空账,销户信息不提供查询
                            {
                                Res.ProvidentFundNo = tdRow[1]; //公积金账号
                            }
                        }
                    }
                }
                #endregion

                #region 第二步,查询基本信息

                //点击多账号列表中的正常状态账号
                if (loginType == "1" && string.IsNullOrEmpty(ProvidentFundNo))
                {
                    _url = baseUrl + "queryPage/webQ.login";
                    _postdata = string.Format("trancode={0}&accnum={1}&cardno={2}&loancontrnum=&state={3}", reTrancode, Res.ProvidentFundNo, Res.ProvidentFundNo, "0");//state=0   标志账户状态:正常
                    httpItem = new HttpItem()
                    {
                        URL = _url,
                        Postdata = _postdata,
                        Method = "Post",
                        Encoding = Encoding.UTF8,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    errorMsc = CommonFun.GetMidStr(httpResult.Html, "</LEGEND><br>", "<br><br><br>");
                    if (!string.IsNullOrEmpty(errorMsc.Trim()))
                    {
                        Res.StatusDescription = errorMsc;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                string link = CommonFun.GetMidStr(httpResult.Html, "f1.action = '../", "';");
                if (string.IsNullOrEmpty(link))
                {
                    Res.StatusDescription = "网站个人基本信息链接发生改动";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                _url = baseUrl + link;
                _postdata = string.Format("loancontrnum=&accnum={0}", Res.ProvidentFundNo);
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Postdata = _postdata,
                    Method = "Post",
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                errorMsc = CommonFun.GetMidStr(httpResult.Html, "</LEGEND><br>", "<br><br><br>");
                if (!string.IsNullOrEmpty(errorMsc.Trim()))
                {
                    Res.StatusDescription = errorMsc;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='mycenter']/div[2]/div[2]/table[1]/tr[1]", "text");
                if (_results.Count > 0)
                {
                    Res.Name = Regex.Replace(_results[0], @"[\u5c0a\u656c\u7684\u4f60\u597d\uff01\s]", "");//姓名
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='mycenter']/div[2]/div[2]/table[1]/tr[2]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.IdentityCard = _results[0];//身份证号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='mycenter']/div[2]/div[2]/table[1]/tr[2]/td[4]", "text");
                if (_results.Count > 0)
                {
                    Res.ProvidentFundNo = _results[0];//公积金账号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='mycenter']/div[2]/div[2]/table[1]/tr[3]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.CompanyName = _results[0];//单位名称
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='mycenter']/div[2]/div[2]/table[1]/tr[3]/td[4]", "text");
                if (_results.Count > 0)
                {
                    Res.CompanyNo = _results[0];//单位编号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='mycenter']/div[2]/div[2]/table[1]/tr[4]/td[2]", "text");
                if (_results.Count > 0)
                {
                    string state = string.Empty;//账户状态
                    switch (Regex.Replace(_results[0], @"[^0-9]", ""))
                    {
                        case "0":
                            state = "正常";
                            break;
                        case "1":
                            state = "封存";
                            break;
                        case "2":
                            state = "空账";
                            break;
                        case "9":
                            state = "销户";
                            break;
                        default:
                            state = "";
                            break;
                    }
                    Res.Status = state;//账户状态
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='mycenter']/div[2]/div[2]/table[1]/tr[4]/td[4]", "text");
                if (_results.Count > 0)
                {
                    Res.SalaryBase = _results[0].ToDecimal(0);//缴存基数
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='mycenter']/div[2]/div[2]/table[1]/tr[5]/td[2]", "text");
                if (_results.Count > 0)
                {
                    decimal monthPay = _results[0].ToDecimal(0);//月缴存额
                    if (monthPay > 0)
                    {
                        Res.CompanyMonthPayAmount = monthPay / 2;//公司月缴额
                        Res.PersonalMonthPayAmount = monthPay / 2;//个人月缴额
                    }
                    //默认个人和公司缴费比率相等的情况下计算得出 个人和公司缴费比率
                    if (Res.SalaryBase > 0 && monthPay > 0)
                    {
                        Res.PersonalMonthPayRate =
                           Res.CompanyMonthPayRate = ((monthPay / Res.SalaryBase) / 2).ToString("f2").ToDecimal(0);  //个人和公司缴费比率
                    }
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='mycenter']/div[2]/div[2]/table[1]/tr[5]/td[4]", "text");
                if (_results.Count > 0)
                {
                    Res.TotalAmount = _results[0].ToDecimal(0);//余额
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='mycenter']/div[2]/div[2]/table[1]/tr[6]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.OpenTime = _results[0];//开户日期
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='mycenter']/div[2]/div[2]/table[1]/tr[6]/td[4]", "text");
                if (_results.Count > 0)
                {
                    Res.LastProvidentFundTime = _results[0];//缴至年月
                }
                #endregion
                #region 第三步,公积金缴费明细
                _url = baseUrl + "queryPage/grmx.jsp";
                _postdata = string.Format("loancontrnum=&accnum={0}", Res.ProvidentFundNo);
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Postdata = _postdata,
                    Method = "Post",
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string detaillink = CommonFun.GetMidStr(httpResult.Html, "action=\"../", "\"");
                if (string.IsNullOrEmpty(link))
                {
                    Res.StatusDescription = "公积金明细信息查询链接发生改动";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                DateTime endTime = DateTime.Now;
                DateTime beginTime = (DateTime)Res.OpenTime.ToDateTime(Consts.DateFormatString2);
                DateTime dt = endTime.AddYears(-4);
                if (dt > beginTime)
                {
                    beginTime = (DateTime)(dt.Year + "-01" + "-01").ToDateTime(Consts.DateFormatString2);
                }
                _url = baseUrl + detaillink;
                _postdata = string.Format("begdate={0}&enddate={1}", beginTime.ToString("yyyy-MM-dd"), endTime.ToString("yyyy-MM-dd"));
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Postdata = _postdata,
                    Method = "Post",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='result']/tr[position()>1]", "inner");
                decimal perAccounting = 0;//个人占比
                decimal comAccounting = 0;//公司占比
                decimal totalRate = 0;//总缴费比率
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
                foreach (string items in _results)
                {
                    ProvidentFundDetail providentFundDetail = new ProvidentFundDetail();
                    var tdRow = HtmlParser.GetResultFromParser(items, "//td", "text", true);
                    if (tdRow.Count != 5)
                    {
                        continue;
                    }
                    providentFundDetail.PayTime = tdRow[0].ToDateTime();//缴费年月
                    providentFundDetail.ProvidentFundTime = DateTime.ParseExact(tdRow[0], "yyyy-MM-dd", null).ToString("yyyyMM");
                    providentFundDetail.Description = tdRow[1];//描述
                    if (tdRow[1].IndexOf("汇缴", StringComparison.Ordinal) > -1)
                    {
                        providentFundDetail.PersonalPayAmount = (tdRow[2].ToDecimal(0) * perAccounting).ToString("f2").ToDecimal(0);//个人缴费金额
                        providentFundDetail.CompanyPayAmount = (tdRow[2].ToDecimal(0) * comAccounting).ToString("f2").ToDecimal(0);//企业缴费金额
                        providentFundDetail.ProvidentFundBase = (tdRow[2].ToDecimal(0) / (totalRate)).ToString("f2").ToDecimal(0);//缴费基数
                        providentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                        providentFundDetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                        PaymentMonths++;
                    }
                    else if (tdRow[1].IndexOf("提取", StringComparison.Ordinal) > -1)
                    {
                        providentFundDetail.PersonalPayAmount = tdRow[3].ToDecimal(0);//个人缴费金额
                        providentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;//缴费标志
                        providentFundDetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;//缴费类型

                    }
                    else
                    {//（补缴，结息etc，数据不精确，只做参考用）
                        providentFundDetail.PersonalPayAmount = Math.Abs(tdRow[2].ToDecimal(0) - tdRow[3].ToDecimal(0));//个人缴费金额
                        providentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                        providentFundDetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                    }
                    Res.ProvidentFundDetailList.Add(providentFundDetail);
                }
                #endregion

                #region 第四步，贷款基本信息
                _url = baseUrl + "include/send.jsp?task=dkxx&trancode=700004";
                _postdata = string.Format("loancontrnum={0}&accnum={1}", loancontrnum, Res.ProvidentFundNo);
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Postdata = _postdata,
                    Method = "Post",
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='jj_wen']/table/tr/td");

                ProvidentFundLoanRes Res_Loan = new ProvidentFundLoanRes();

                if (_results.Count == 27)
                {
                    Res_Loan.Account_Loan = _results[4];
                    Res_Loan.Con_No = _results[6];
                    Res_Loan.Account_Repay = _results[8];
                    Res_Loan.Current_Repay_Date = _results[10];
                    Res_Loan.Loan_Credit = _results[12].ToDecimal(0);
                    Res_Loan.Loan_Actual_End_Date = CommonFun.ClearFlag(_results[14]);
                    Res_Loan.Loan_Start_Date = CommonFun.ClearFlag(_results[16]);
                    string dkzhzt = string.Empty;
                    switch (CommonFun.GetMidStr(_results[18], "state='", "'"))
                    {
                        case "1":
                            dkzhzt = "正常";
                            break;
                        case "2":
                            dkzhzt = "逾期";
                            break;
                        case "3":
                            dkzhzt = "部分逾期";
                            break;
                        case "4":
                            dkzhzt = "停息挂账";
                            break;
                        case "5":
                            dkzhzt = "清户";
                            break;
                        case "6":
                            dkzhzt = "销户";
                            break;
                        case "7":
                            dkzhzt = "核销";
                            break;
                    }
                    Res_Loan.Status = dkzhzt;
                    Res_Loan.Period_Total = _results[20];
                    Res_Loan.Interest_Payed = _results[22].ToDecimal(0);
                    Res_Loan.Principal_Payed = _results[24].ToDecimal(0);
                    Res_Loan.Loan_Balance = _results[26].ToDecimal(0);
                }
                #endregion

                #region 第五步，贷款明细
                _url = baseUrl + "include/send.jsp?task=dkmx&trancode=700005";
                _postdata = string.Format("loancontrnum={0}&begdate={1}&enddate={2}", loancontrnum, Res_Loan.Loan_Start_Date.IsEmpty() ? "1990-01-01" : Res_Loan.Loan_Start_Date, DateTime.Now.ToString("yyyy-MM-dd"));
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Postdata = _postdata,
                    Method = "Post",
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='jj_wen']//table/tr");
                List<string> PayMonth = new List<string>();
                bool IsOverdue = false;//是否逾期
                bool IsPenalty = false;//是否罚息

                foreach (string item in _results)
                {
                    List<string> _detail_loan = HtmlParser.GetResultFromParser(item, "/td");
                    if (_detail_loan.Count != 5)
                    {
                        continue;
                    }

                    ProvidentFundLoanDetail _ProvidentFundLoanDetail = Res_Loan.ProvidentFundLoanDetailList.Where(o => o.Bill_Date == _detail_loan[0]).FirstOrDefault();

                    if (!_detail_loan[0].IsEmpty())//将所有交易时间已“yyyyMM”形式存入List中
                    {
                        string paymonth = DateTime.ParseExact(_detail_loan[0], "yyyy-MM-dd", null).ToString("yyyyMM");
                        if (!PayMonth.Contains(paymonth))
                        {
                            PayMonth.Add(paymonth);
                        }
                    }
                    bool NeedAdd = false;
                    if (_ProvidentFundLoanDetail == null)
                    {
                        _ProvidentFundLoanDetail = new ProvidentFundLoanDetail();
                        _ProvidentFundLoanDetail.Bill_Date = _detail_loan[0];
                        _ProvidentFundLoanDetail.Record_Date = _detail_loan[0];
                        NeedAdd = true;
                    }
                    _ProvidentFundLoanDetail.Principal += _detail_loan[1].ToDecimal(0);
                    _ProvidentFundLoanDetail.Interest += _detail_loan[2].ToDecimal(0);
                    _ProvidentFundLoanDetail.Interest_Penalty += _detail_loan[3].ToDecimal(0);
                    _ProvidentFundLoanDetail.Balance = _detail_loan[4].ToDecimal(0);

                    if (NeedAdd)
                    {
                        Res_Loan.ProvidentFundLoanDetailList.Add(_ProvidentFundLoanDetail);
                    }

                    if (_ProvidentFundLoanDetail.Interest_Penalty != 0 && !IsPenalty)
                    {
                        IsPenalty = true;
                    }
                }
                PayMonth.Sort();//对所有交易年月进行排序
                for (int i = 0; i < PayMonth.Count - 1; i++)
                {
                    if (DateTime.ParseExact(PayMonth[i], "yyyyMM", null).AddMonths(1).ToString("yyyyMM") != PayMonth[i + 1])//如果两月不连续
                    {
                        IsOverdue = true;//则有逾期
                        break;
                    }
                }
                if (IsOverdue || IsPenalty)
                {
                    Res_Loan.Description = (IsOverdue ? "有逾期，" : "") + (IsPenalty ? "有罚息，" : "") + "请人工校验";
                }
                #endregion

                    Res.PaymentMonths = PaymentMonths;
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
}
