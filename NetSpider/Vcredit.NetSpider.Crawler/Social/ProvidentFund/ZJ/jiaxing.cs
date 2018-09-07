using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.ZJ
{
    public class jiaxing : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://122.225.26.86:7001/wscx_jx/";
        string fundCity = "zj_jiaxing";
        string _url = string.Empty;
        string _postData = string.Empty;
        List<string> _results = new List<string>();
        private int PaymentMonths = 0;
        #endregion
        #region 私有变量
        string _cxyd = string.Empty;
        string _cxydmc = string.Empty;
        string _dbname = string.Empty;
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        private Regex regSpace = new Regex(@"[/\&nbsp;\s;\t]*");//去除空格
        private Regex reg = new Regex(@"[^0-9.1-9.]*");//去除非数字
        private decimal payRate = (decimal)0.08;
        decimal perAccounting;//个人占比
        decimal comAccounting;//公司占比
        decimal totalRate;//总缴费比率
        int index = 0;
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                vcRes.StatusDescription = "所选城市无需初始化";
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

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            Res.ProvidentFundCity = fundCity;
            string zgzh = string.Empty;//职工账号
            string zgxm = string.Empty;//职工姓名
            string dwbm = string.Empty;//单位编号
            string zgzt = string.Empty;//职工状态
            string loginType = string.Empty;//登录方式[01:身份证,02:公积金账号]
            string txtNumber = string.Empty;//登录账号
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (string.IsNullOrEmpty(fundReq.Password))
                {
                    Res.StatusDescription = "密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //15位或18位身份证验证
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (fundReq.LoginType == "2")
                {
                    if (regex.IsMatch(fundReq.Identitycard) == false)
                    {
                        Res.StatusDescription = "请输入有效身份证号";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    loginType = "01";
                    txtNumber = fundReq.Identitycard;
                }
                else
                {
                    if (regex.IsMatch(fundReq.Username) || string.IsNullOrWhiteSpace(fundReq.Username))
                    {
                        Res.StatusDescription = "请输入有效的公积金账号";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    loginType = "02";
                    txtNumber = fundReq.Username;
                }
                #region 第一步,登陆
                #region 初始化页面
                _url = baseUrl + "zfbzgl/zfbzsq/login.jsp";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "GET",
                    Encoding = Encoding.GetEncoding("GBK"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='cxyd']", "value");
                if (_results.Count > 0)
                {
                    _cxyd = _results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='cxydmc']", "value");
                _cxydmc = _results.Count > 0 ? _results[0] : "";
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dbname']", "value");
                _dbname = _results.Count > 0 ? _results[0] : "";
                #endregion

                _url = baseUrl + "zfbzgl/zfbzsq/login_hidden.jsp?dlfs=0";
                _postData = string.Format("cxyd={0}&cxydmc={1}&dbname={2}&dlfs={3}&sfzh={4}&password={5}&yes.x=51&yes.y=16", _cxyd.ToUrlEncode(Encoding.GetEncoding("gbk")), _cxydmc.ToUrlEncode(Encoding.GetEncoding("gbk")), _dbname.ToUrlEncode(Encoding.GetEncoding("gbk")), loginType, txtNumber, fundReq.Password.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "Post",
                    Postdata = _postData,
                    Encoding = Encoding.GetEncoding("gbk"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string errorMsc = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\");");
                if (!string.IsNullOrEmpty(errorMsc.Trim()))
                {
                    Res.StatusDescription = errorMsc;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgzh']", "value");
                zgzh = _results.Count > 0 ? _results[0] : "";
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgxm']", "value");
                zgxm = _results.Count > 0 ? _results[0] : "";
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dwbm']", "value");
                dwbm = _results.Count > 0 ? _results[0] : "";
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgzt']", "value");
                zgzt = _results.Count > 0 ? _results[0] : "";
                #endregion
                #region 第二步,获取基本信息

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                _url = baseUrl + string.Format("/zfbzgl/gjjxxcx/gjjxx_cx.jsp?zgzh={0}&sfzh={1}&zgxm={2}&dwbm={3}&zgzt={4}&cxyd={5}&dbname={6}", zgzh, txtNumber, zgxm.ToUrlEncode(Encoding.GetEncoding("GBK")), dwbm, zgzt.ToUrlEncode(Encoding.GetEncoding("GBK")), _cxyd.ToUrlEncode(Encoding.GetEncoding("GBK")), _dbname);
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "GET",
                    Encoding = Encoding.GetEncoding("gbk"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[1]/td[2]", "text");
                Res.Name = _results.Count > 0 ? regSpace.Replace(_results[0], "") : null;//职工姓名
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[1]/td[4]", "text");
                Res.BankCardNo = _results.Count > 0 ? regSpace.Replace(_results[0], "") : null;//联名卡号
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[2]/td[2]", "text");
                Res.IdentityCard = _results.Count > 0 ? regSpace.Replace(_results[0], "") : null;//身份证号
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[2]/td[4]", "text");
                Res.ProvidentFundNo = _results.Count > 0 ? regSpace.Replace(_results[0], "") : null;//公积金账号
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[3]/td[2]", "text");
                Res.CompanyName = _results.Count > 0 ? regSpace.Replace(_results[0], "") : null;//单位名称
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[4]/td[2]", "text");
                Res.Phone = _results.Count > 0 ? regSpace.Replace(_results[0], "") : null;//电话
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[4]/td[4]", "text");
                Res.Status = _results.Count > 0 ? regSpace.Replace(_results[0], "") : null;//当前状态
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[5]/td[2]", "text");
                Res.SalaryBase = _results.Count > 0 ? reg.Replace(_results[0], "").ToDecimal(0) : decimal.Zero;//月缴工资基数
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[5]/td[5]", "text");
                if (_results.Count > 0)
                {
                    MatchCollection matches = new Regex(@"[0-9.1-9.]{3,4}").Matches(_results[0]);
                    if (matches.Count == 2)
                    {
                        Res.CompanyMonthPayRate = matches[0].Value.ToDecimal(0) * 0.01M;
                        Res.PersonalMonthPayRate = matches[1].Value.ToDecimal(0) * 0.01M;
                    }
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[6]/td[2]", "text");
                Res.CompanyMonthPayAmount = _results.Count > 0 ? reg.Replace(_results[0], "").ToDecimal(0) : decimal.Zero;//单位月缴额
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[7]/td[2]", "text");
                Res.PersonalMonthPayAmount = _results.Count > 0 ? reg.Replace(_results[0], "").ToDecimal(0) : decimal.Zero;//个人月缴额
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[8]/td[4]", "text");
                Res.TotalAmount = _results.Count > 0 ? reg.Replace(_results[0], "").ToDecimal(0) : decimal.Zero;//余额
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[9]/td[4]", "text");
                Res.LastProvidentFundTime = _results.Count > 0 ? _results[0] : null;//缴至年月
                #endregion
                #region 第三步,公积金缴费明细
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
                int totalPage = 0;//明细总页数
                string cxydone = string.Empty;
                _url = baseUrl + "zfbzgl/gjjmxcx/gjjmx_cx.jsp";
                _postData = string.Format("sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd={4}&zgzt={5}", txtNumber, zgxm.ToUrlEncode(Encoding.GetEncoding("gbk")), zgzh, dwbm, _cxyd.ToUrlEncode(Encoding.GetEncoding("gbk")), zgzt.ToUrlEncode(Encoding.GetEncoding("gbk")));
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "Post",
                    Postdata = _postData,
                    Encoding = Encoding.GetEncoding("gbk"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='totalpages']", "value");
                if (_results.Count == 0)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                totalPage = _results[0].ToInt(0);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='cxydone']", "value");
                if (_results.Count > 0)
                {
                    cxydone = _results[0];
                }
                Res = DealWithHttpResults(httpResult);
                if (totalPage > 1)
                {
                    for (int i = 2; i <= totalPage; i++)
                    {
                        _postData = string.Format("cxydone={0}&cxydtwo=&yss={1}&totalpages={2}&cxyd={3}&zgzh={4}&sfzh={5}&zgxm={6}&dwbm={7}", cxydone.ToUrlEncode(Encoding.GetEncoding("gbk")), i, totalPage, cxydone.ToUrlEncode(Encoding.GetEncoding("gbk")), zgzh, txtNumber, zgxm.ToUrlEncode(Encoding.GetEncoding("gbk")), dwbm);
                        httpItem = new HttpItem()
                        {
                            URL = _url,
                            Method = "Post",
                            Postdata = _postData,
                            Encoding = Encoding.GetEncoding("gbk"),
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        Res = DealWithHttpResults(httpResult);
                    }
                }

                #endregion

                #region 贷款基本信息
                _url = baseUrl + "zfbzgl/dkxxcx/dkxx_cx.jsp";
                _postData = string.Format("sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd={4}&zgzt={5}", txtNumber, zgxm.ToUrlEncode(Encoding.GetEncoding("gbk")), zgzh, dwbm, _cxyd.ToUrlEncode(Encoding.GetEncoding("gbk")), zgzt.ToUrlEncode(Encoding.GetEncoding("gbk")));
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "Post",
                    Postdata = _postData,
                    Encoding = Encoding.GetEncoding("gbk"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", ""), "//tr[@class='jtpsoft']/td/font");
                ProvidentFundLoanRes Res_Loan = new ProvidentFundLoanRes();
                if (_results.Count == 18)
                {
                    Res_Loan.Loan_Sid = _results[0];
                    Res_Loan.Name = _results[1];
                    Res_Loan.Loan_Credit = _results[2].Replace("元", "").ToDecimal(0); 
                    Res_Loan.Period_Total = _results[3];
                    Res_Loan.Principal_Payed = _results[4].Replace("元", "").ToDecimal(0);
                    Res_Loan.Interest_Payed = _results[5].Replace("元", "").ToDecimal(0);
                    Res_Loan.Loan_Balance = _results[6].Replace("元", "").ToDecimal(0);
                    Res_Loan.House_Purchase_Type = _results[7];
                    Res_Loan.Loan_Start_Date = _results[8];
                    Res_Loan.House_Purchase_Address = _results[9];
                    Res_Loan.Loan_Rate = _results[10];
                    Res_Loan.Bank_Delegate = _results[11];
                    Res_Loan.Repay_Type = _results[12];
                    Res_Loan.Account_Loan = _results[13];
                    Res_Loan.Status = _results[14];
                    Res_Loan.Loan_Type = _results[15];
                    Res_Loan.Withdrawal_Type = _results[16] == "是" ? "月冲还贷" : "";
                    Res_Loan.Current_Repay_Date = _results[17];
                }
                #endregion

                #region 贷款还款明细
                int intPage = 1;
                int tdrowcount = 0;
                do
                {
                    _url = baseUrl + "zfbzgl/dkhkcx/dkhk_cx.jsp";
                    _postData = string.Format("intPage={0}&startRow={1}&endRow={2}&intRowCount=&intPageCount=&intPageSize=500&sfzh={3}&zgzh={4}&zgxm=%CD%F2%C7%E5&dwbm={5}&flag=1", intPage, (intPage - 1) * 500, intPage * 500 - 1, txtNumber, zgzh, dwbm);
                    httpItem = new HttpItem()
                    {
                        URL = _url,
                        Method = "Post",
                        Postdata = _postData,
                        Encoding = Encoding.GetEncoding("gbk"),
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    _results = HtmlParser.GetResultFromParser(httpResult.Html, "//tr[@class='jtpsoft']");
                    foreach (string item in _results)
                    {
                        List<string> tdrow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdrow.Count != 7 || tdrow[1].IsEmpty())
                        {
                            continue;
                        }

                        ProvidentFundLoanDetail _ProvidentFundLoanDetail = new ProvidentFundLoanDetail();
                        _ProvidentFundLoanDetail.Bill_Date = tdrow[0];
                        _ProvidentFundLoanDetail.Record_Date = tdrow[0];
                        _ProvidentFundLoanDetail.Record_Month = tdrow[1];
                        _ProvidentFundLoanDetail.Principal = tdrow[2].ToDecimal(0);
                        _ProvidentFundLoanDetail.Interest = tdrow[3].ToDecimal(0);
                        _ProvidentFundLoanDetail.Interest_Penalty = tdrow[4].ToDecimal(0);
                        _ProvidentFundLoanDetail.Base = tdrow[5].ToDecimal(0);
                        _ProvidentFundLoanDetail.Balance = tdrow[6].ToDecimal(0);

                        Res_Loan.ProvidentFundLoanDetailList.Add(_ProvidentFundLoanDetail);
                    }
                    tdrowcount = _results.Count;
                    intPage++;
                }
                while (tdrowcount == 500);
                #endregion

                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return Res;
        }
        /// <summary>
        /// 处理httpResult
        /// </summary>
        /// <param name="httpResult">http返回参数类</param>
        /// <returns>Res</returns>
        private Vcredit.NetSpider.Entity.Service.ProvidentFundQueryRes DealWithHttpResults(HttpResult httpResult)
        {
            List<string> results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[position()>1]", "inner");
            if (index == 0)
            {
                index++;
                if (results.Count == 0)
                {
                    Res.StatusDescription = "该账号暂无账户明细";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
            }
            foreach (string item in results)
            {
                ProvidentFundDetail detail = new ProvidentFundDetail();
                List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                if (tdRow.Count != 5)
                {
                    continue;
                }
                //排除最后合计一行
                if (tdRow[1].Contains("合计"))
                {
                    continue;
                }
                detail.PayTime = tdRow[0].ToDateTime();//缴费年月
                detail.Description = tdRow[1];//描述
                if (tdRow[1].IndexOf("汇缴", System.StringComparison.Ordinal) > -1)
                {
                    detail.PersonalPayAmount = reg.Replace(tdRow[2], "").ToDecimal(0) * perAccounting;//个人缴费金额
                    detail.CompanyPayAmount = reg.Replace(tdRow[2], "").ToDecimal(0) * comAccounting;//企业缴费金额
                    detail.ProvidentFundBase = (reg.Replace(tdRow[2], "").ToDecimal(0) / (totalRate)).ToString("f2").ToDecimal(0);//基本薪资
                    detail.ProvidentFundTime = reg.Replace(tdRow[1], "");//应属年月
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                    PaymentMonths++;
                }
                else if (tdRow[1].IndexOf("提取", System.StringComparison.Ordinal) > -1)
                {
                    detail.PersonalPayAmount = reg.Replace(tdRow[3], "").ToDecimal(0);//个人缴费金额
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                }
                else
                {//（补缴，结息etc，数据不精确，只做参考用）
                    detail.PersonalPayAmount = reg.Replace(tdRow[2], "").ToDecimal(0);//个人缴费金额
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                }
                Res.ProvidentFundDetailList.Add(detail);
            }
            return Res;
        }
    }
}
