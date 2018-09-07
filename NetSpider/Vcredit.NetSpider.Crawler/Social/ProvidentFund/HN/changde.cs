using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HN
{
    public class changde : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://218.75.134.69:9999/wscx/zfbzgl/";
        string fundCity = "hn_changde";
        #endregion
        #region 私有变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        string _url = string.Empty;
        string _postdata = string.Empty;
        decimal payRate = (decimal)0.08;
        List<string> _results = new List<string>();
        ProvidentFundDetail detail = null;
        int PaymentMonths = 0;
        Regex reg = new Regex(@"[\&nbsp;\s;\,;\u5143]*");//空格,换行,元
        private string YearMonth = string.Empty;
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            string cxydmc = string.Empty;
            string dbname = string.Empty;
            try
            {
                //初始化页面开始
                _url = baseUrl + "zfbzsq/login.jsp";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='cxydmc']", "value");
                if (_results.Count > 0)
                {
                    cxydmc = _results[0];
                }
                else
                {
                    vcRes.StatusDescription = ServiceConsts.ProvidentFund_InitError;
                    vcRes.StatusCode = ServiceConsts.StatusCode_fail;
                    return vcRes;
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dbname']", "value");
                if (_results.Count > 0)
                {
                    dbname = _results[0];
                }
                //初始化页面结束
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                //合并缓存
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //保存缓存
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("cxydmc", cxydmc);
                dics.Add("dbname", dbname);
                CacheHelper.SetCache(token, dics);
            }
            catch (Exception e)
            {
                vcRes.StatusCode = ServiceConsts.StatusCode_error;
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return vcRes;
        }

        public ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            Res.ProvidentFundCity = fundCity;
            string cxydmc = string.Empty;//当前年度
            string dbname = string.Empty;//wasys350_wscx
            string zgzh = string.Empty;//职工账号
            string zgxm = string.Empty;//职工姓名
            string dwbm = string.Empty;//单位编号
            string zgzt = string.Empty;//职工状态
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(fundReq.Token);
                    cxydmc = dics["cxydmc"].ToString();
                    dbname = dics["dbname"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                //15位或18位身份证验证
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (!regex.IsMatch(fundReq.Identitycard))
                {
                    Res.StatusDescription = "请输入有效的15位或18位身份证号";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "密码不能为空!";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆

                _url = baseUrl + string.Format("zfbzsq/login_hidden.jsp?password={0}&sfzh={1}&cxyd=&dbname={2}&dlfs=0", fundReq.Password, fundReq.Identitycard, dbname);
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "GET",
                    Encoding = Encoding.GetEncoding("GBK"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "<script type=\"text/javascript\">alert(\"", "\");");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgzh']", "value");
                if (_results.Count < 1)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                zgzh = _results[0];
                if (string.IsNullOrWhiteSpace(zgzh))
                {
                    Res.StatusDescription = "系统未登记该账号";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgxm']", "value");
                if (_results.Count > 0)
                {
                    zgxm = _results[0];
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dwbm']", "value");
                if (_results.Count > 0)
                {
                    dwbm = _results[0];
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgzt']", "value");
                if (_results.Count > 0)
                {
                    zgzt = _results[0];
                }
                #endregion
                #region 第二步,获取基本信息
                _url = baseUrl + string.Format("zfbzsq/main_menu.jsp?zgzh={0}&sfzh={1}&zgxm={2}&dwbm={3}&zgzt={4}&cxyd=&dbname={5}", zgzh, fundReq.Identitycard, zgxm.ToUrlEncode(Encoding.GetEncoding("GBK")), dwbm, zgzt.ToUrlEncode(Encoding.GetEncoding("GBK")), dbname);
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "GET",
                    Encoding = Encoding.GetEncoding("GBK"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[1]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.Name = reg.Replace(_results[0], "");//姓名
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[1]/td[4]", "text");
                if (_results.Count > 0)
                {
                    Res.BankCardNo = reg.Replace(_results[0], "");//银行卡号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[2]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.IdentityCard = reg.Replace(_results[0], "");//身份证号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[2]/td[4]", "text");
                if (_results.Count > 0)
                {
                    Res.ProvidentFundNo = reg.Replace(_results[0], "");//职工账号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[3]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.CompanyName = reg.Replace(_results[0], "");//所在单位
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[4]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.OpenTime = reg.Replace(_results[0], "");//开户日期
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[4]/td[4]", "text");
                if (_results.Count > 0)
                {
                    Res.Status = reg.Replace(_results[0], "");//当前状态
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[5]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.SalaryBase = reg.Replace(_results[0], "").ToDecimal(0);//基本薪资
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[5]/td[5]", "text");
                if (_results.Count > 0)
                {
                    MatchCollection matches = Regex.Matches(_results[0], @"[0-9.]{4}[1-9]*");
                    if (matches.Count == 2)
                    {
                        Res.PersonalMonthPayRate = matches[0].Value.ToDecimal(0) / 100;//个人缴费比率
                        Res.CompanyMonthPayRate = matches[1].Value.ToDecimal(0) / 100;//公司缴费比率
                    }
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[7]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = reg.Replace(_results[0], "").ToDecimal(0);//单位月缴额
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[8]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = reg.Replace(_results[0], "").ToDecimal(0);//个人月缴额
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[9]/td[4]", "text");
                if (_results.Count > 0)
                {
                    Res.TotalAmount = reg.Replace(_results[0], "").ToDecimal(0);//公积金余额
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[10]/td[4]", "text");
                if (_results.Count > 0)
                {
                    Res.LastProvidentFundTime = reg.Replace(_results[0], "");//缴至年月
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgxm']", "value");
                if (_results.Count > 0)
                {
                    zgxm = _results[0];//???
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgzt']", "value");
                if (_results.Count > 0)
                {
                    zgzt = _results[0];//??
                }
                #endregion
                #region 第三步,查询公积金缴费明细

                List<string> years = new List<string>();
                int yss = 1;//当前页[默认为1]
                int totalpages = 1;//总页码[默认为1]
                _url = baseUrl + "gjjmxcx/gjjmx_cx.jsp";
                _postdata = string.Format("sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd=&zgzt={4}", Res.IdentityCard, zgxm.ToUrlEncode(Encoding.GetEncoding("GBK")), zgzh, dwbm, zgzt.ToUrlEncode(Encoding.GetEncoding("GBK")));
                httpResult = CommonDepart(_url, _postdata);

                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@name='cxydone']/option", "text");
                if (_results.Count > 0)
                {
                    years = _results;
                }
                years.Remove(years[0]);//排除已选择的当前年份
                foreach (var year in years)
                {
                    _url = baseUrl + "gjjmxcx/gjjmx_cx.jsp";

                    _postdata = string.Format("cxydone={0}&cxydtwo={1}&yss={2}&totalpages={3}&cxyd=&zgzh={4}&sfzh={5}&zgxm={6}&dwbm={7}&dbname={8}", year, year, yss, totalpages, zgzh, Res.IdentityCard, zgxm.ToUrlEncode(Encoding.GetEncoding("GBK")), dwbm, dbname);

                    httpResult = CommonDepart(_url, _postdata);

                    _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='yss']", "value");
                    if (_results.Count > 0)
                    {
                        yss = _results[0].ToInt(0);
                    }
                    _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='totalpages']", "value");
                    if (_results.Count > 0)
                    {
                        totalpages = _results[0].ToInt(0);
                    }
                    if (totalpages > 1)
                    {
                        for (int i = 2; i <= totalpages; i++)
                        {
                            _url = baseUrl + "gjjmxcx/gjjmx_cx.jsp";
                            _postdata = string.Format("cxydone={0}&cxydtwo={1}&yss={2}&totalpages={3}&cxyd=&zgzh={4}&sfzh={5}&zgxm={6}&dwbm={7}&dbname={8}", year, year, i, totalpages, zgzh, Res.IdentityCard, zgxm.ToUrlEncode(Encoding.GetEncoding("GBK")), dwbm, dbname);

                            httpResult = CommonDepart(_url, _postdata);
                        }
                    }
                }
                #endregion


                #region 第四步，查询贷款基本信息
                ProvidentFundLoanRes Res_Loan = new ProvidentFundLoanRes();
                _url = baseUrl + "dkxxcx/dkxx_cx.jsp";
                _postdata = string.Format("sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd=&zgzt=null", fundReq.Identitycard, zgxm.ToUrlEncode(Encoding.GetEncoding("GBK")), zgzh, dwbm);
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "Post",
                    Postdata = _postdata,
                    Encoding = Encoding.GetEncoding("GBK"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//legend[@id='title']");
                if (_results.Count > 0)
                {
                    if (_results[0].Contains("没有贷款")) goto End;
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr/td/font", "");
                if (_results.Count > 0)
                {
                    Res_Loan.Con_No = _results[0].Split(';')[1];  //合同编号
                    Res_Loan.Name = _results[1].Split(';')[1];  //姓名
                    Res_Loan.IdentityCard = Res.IdentityCard;  //身份证号
                    Res_Loan.Loan_Credit = Regex.Replace(_results[2].Split(';')[1], @"[^\d.\d]", "").ToDecimal(0);  //贷款金额
                    Res_Loan.Period_Total = _results[3].Split(';')[1];  //贷款期限
                    Res_Loan.Principal_Payed = Regex.Replace(_results[4].Split(';')[1], @"[^\d.\d]", "").ToDecimal(0);  //已还本金
                    Res_Loan.Interest_Payed = Regex.Replace(_results[5].Split(';')[1], @"[^\d.\d]", "").ToDecimal(0); //已还利息
                    Res_Loan.Loan_Balance = Regex.Replace(_results[6].Split(';')[1], @"[^\d.\d]", "").ToDecimal(0); //贷款余额
                    Res_Loan.Current_Repay_Total = Regex.Replace(_results[7].Split(';')[1], @"[^\d.\d]", "").ToDecimal(0); //将月最低还款金额 作为 当期还款金额
                    Res_Loan.Overdue_Principal = Regex.Replace(_results[8].Split(';')[1], @"[^\d.\d]", "").ToDecimal(0); //将当前逾期金额 作为 逾期本金
                    Res_Loan.Overdue_Interest = Regex.Replace(_results[9].Split(';')[1], @"[^\d.\d]", "").ToDecimal(0);  //将当前逾期利息 作为 逾期利息
                    Res_Loan.Current_Repay_Date = _results[10].Split(';')[1];  //当期还款日期
                    Res_Loan.Loan_End_Date = _results[11].Split(';')[1];  //贷款结束日期
                    Res_Loan.Loan_Start_Date = _results[12].Split(';')[1];  //贷款开始日期
                    Res_Loan.Bank_Delegate = _results[13].Split(';')[1];  //委托银行
                    Res_Loan.Loan_Rate = _results[14].Split(';')[1];  //贷款利率
                    Res_Loan.Overdue_Period = Regex.Replace(_results[15].Split(';')[1], @"[^\d.\d]", "").ToInt(0); //逾期期数
                    Res_Loan.Repay_Type = _results[16].Split(';')[1];  //还款方式
                    Res_Loan.House_Purchase_Type = _results[18].Split(';')[1];  //购房类型
                    Res_Loan.Overdue_Period = Regex.Replace(_results[20].Split(';')[1], @"[^\d.\d]", "").ToInt(0);   //逾期期数
                    Res_Loan.Status = _results[21].Split(';')[1];  //贷款状态
                }
                #endregion


                #region 第五步，查询贷款详细信息
                _url = baseUrl + "dkhkcx/dkhk_cx.jsp";
                _postdata = string.Format("startRow=0&endRow=16&sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd=&zgzt=null", fundReq.Identitycard, zgxm.ToUrlEncode(Encoding.GetEncoding("GBK")), zgzh, dwbm);
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "Post",
                    Postdata = _postdata,
                    Encoding = Encoding.GetEncoding("GBK"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr", "");
                foreach (var item in _results)
                {
                    ProvidentFundLoanDetail Res_LoanDetail = new ProvidentFundLoanDetail();
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow.Count != 10 || tdRow[0].Contains("日期"))
                    {
                        continue;
                    }
                    if (tdRow[9].Contains("还款"))
                    {
                        Res_LoanDetail.Record_Date = tdRow[0];  //记账日期
                        Res_LoanDetail.Record_Period = tdRow[1];  //还款期数
                        Res_LoanDetail.Principal = tdRow[2].ToDecimal(0);  //还款本金
                        Res_LoanDetail.Interest = tdRow[3].ToDecimal(0);  //还款利息
                        Res_LoanDetail.Interest_Penalty_ToPay = tdRow[4].ToDecimal(0);  //偿还的罚息
                        Res_LoanDetail.Base = tdRow[5].ToDecimal(0);  //还款本息
                        Res_LoanDetail.Balance = tdRow[8].ToDecimal(0);  //贷款余额
                        Res_LoanDetail.Description = tdRow[9]; //描述
                    }
                    else
                    {
                        Res_LoanDetail.Record_Date = tdRow[0];  //记账日期
                        Res_LoanDetail.Balance = tdRow[8].ToDecimal(0);  //贷款余额
                        Res_LoanDetail.Description = tdRow[9]; //描述
                    }
                    Res_Loan.ProvidentFundLoanDetailList.Add(Res_LoanDetail);
                }
                #endregion

                Res.ProvidentFundLoanRes = Res_Loan;
                End:
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
        /// <summary>
        /// 公共处理部分
        /// </summary>
        /// <param name="_url">请求链接</param>
        /// <param name="postdata">post数据</param>
        /// <returns>HttpResult</returns>
        public Vcredit.Common.Helper.HttpResult CommonDepart(string _url, string postdata)
        {
            httpItem = new HttpItem()
            {
                URL = _url,
                Method = "Post",
                Postdata = _postdata,
                Encoding = Encoding.GetEncoding("GBK"),
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[@class='jtpsoft']", "inner");

            for (int i = 0; i < _results.Count; i++)
            {
                Regex regTime = new Regex(@"[0-9]{6}[1-9]*");
                ProvidentFundDetail providentFundDetail = new ProvidentFundDetail();
                var tdRow = HtmlParser.GetResultFromParser(_results[i], "//td", "text", true);
                if (tdRow.Count != 6)
                {
                    continue;
                }
                providentFundDetail.PayTime = tdRow[0].ToDateTime();//缴费年月
                providentFundDetail.Description = tdRow[5];//描述
                if (tdRow[5].IndexOf("汇缴", StringComparison.Ordinal) > -1)
                {
                    //if (i == _results.Count - 1)
                    //{
                    //    break;
                    //}
                    var tdRow2 = HtmlParser.GetResultFromParser(_results[i], "//td", "text", true);
                    if (tdRow2.Count != 6)
                    {
                        continue;
                    }
                    if (CommonFun.GetMidStr(tdRow[5], "汇缴", "公积金") == CommonFun.GetMidStr(tdRow2[5], "汇缴", "公积金"))
                    {
                        providentFundDetail.ProvidentFundTime = regTime.Match(tdRow[5]).Value;//应属年月
                        providentFundDetail.PersonalPayAmount = (reg.Replace(tdRow2[2], "").ToDecimal(0)) / 2;//个人缴费金额
                        providentFundDetail.CompanyPayAmount = (reg.Replace(tdRow2[2], "").ToDecimal(0)) / 2;//企业缴费金额
                        providentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                        providentFundDetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                        PaymentMonths++;
                        //i++;
                    }
                    else
                    {
                        providentFundDetail.ProvidentFundTime = regTime.Match(tdRow[5]).Value;//应属年月
                        providentFundDetail.PersonalPayAmount = reg.Replace(tdRow[2], "").ToDecimal(0) / 2;//个人缴费金额
                        providentFundDetail.CompanyPayAmount = reg.Replace(tdRow[2], "").ToDecimal(0) / 2;//企业缴费金额
                        providentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                        providentFundDetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                        PaymentMonths++;
                    }

                }
                else if (tdRow[5].IndexOf("补缴", StringComparison.Ordinal) > -1)
                {
                    providentFundDetail.ProvidentFundTime = regTime.Match(tdRow[5]).Value;//应属年月
                    providentFundDetail.CompanyPayAmount = reg.Replace(tdRow[2], "").ToDecimal(0);//个人缴费金额
                    providentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费标志
                    providentFundDetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                }
                else if (tdRow[5].Contains("结息") || tdRow[5].Contains("结转"))
                {
                    //providentFundDetail.ProvidentFundTime = regTime.Match(tdRow[5]).Value;//应属年月
                    providentFundDetail.PersonalPayAmount = reg.Replace(tdRow[2], "").ToDecimal(0);//个人缴费金额
                    providentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费标志
                    providentFundDetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                }
                else if (tdRow[5].Contains("合计"))
                {
                    //providentFundDetail.ProvidentFundTime = regTime.Match(tdRow[5]).Value;//应属年月
                    providentFundDetail.PersonalPayAmount = reg.Replace(tdRow[1], "").ToDecimal(0);//个人缴费金额
                    providentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费标志
                    providentFundDetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                }
                else if (tdRow[5].Contains("提取"))
                {
                    providentFundDetail.PersonalPayAmount = reg.Replace(tdRow[1], "").ToDecimal(0);//个人缴费金额
                    providentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;//缴费标志
                    providentFundDetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;//缴费类型
                    Res.Description = "有支取，请人工校验。";
                }
                else
                {//（补缴，结息。。。，数据不精确，只做参考用）
                    //providentFundDetail.PersonalPayAmount = reg.Replace(tdRow[1], "").ToDecimal(0);//个人缴费金额
                    providentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                    providentFundDetail.PaymentType = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费类型
                }
                Res.ProvidentFundDetailList.Add(providentFundDetail);
            }
            return httpResult;
        }
    }
}
