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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.JS
{
    public class lianyungang : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://search.lygzfgjj.com.cn/";
        string fundCity = "js_lianyungang";
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Host = "search.lygzfgjj.com.cn",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "ValidateCode.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl,
                    Host = "search.lygzfgjj.com.cn",
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
            ProvidentFundLoanRes Res_Loan = new ProvidentFundLoanRes();
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal rate = (decimal)0.12;
            List<string> results = new List<string>();
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
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆
                Url = baseUrl + "DefaultBack.aspx";
                postdata = string.Format("userName={0}&password={1}&checkCode={2}", fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl,
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
                string errorcode = jsonParser.GetResultFromParser(httpResult.Html, "result");
                if (errorcode != "1")
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region 基本信息&明细
                for (int year = DateTime.Now.Year; year > DateTime.Now.AddYears(-5).Year; year--)
                {
                    Url = baseUrl + "Secure/Ndmx.aspx?Year=" + year;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (Res.Name.IsEmpty() || Res.IdentityCard.IsEmpty())
                    {
                        string name_id_str = CommonFun.GetMidStr(httpResult.Html, "姓名：", "<").Replace("&nbsp;", "");
                        Res.Name = CommonFun.GetMidStr(name_id_str, "", "身份证号码：");
                        Res.IdentityCard = CommonFun.GetMidStr(name_id_str, "身份证号码：", "");
                    }
                    if (Res.CompanyName.IsEmpty())
                    {
                        string company_str = CommonFun.GetMidStr(httpResult.Html, "最后缴交单位：", "<").Replace("&nbsp;", "");
                        Regex companyno_regx = new Regex("^（[0-9]{4,}）");
                        if (companyno_regx.IsMatch(company_str))
                        {
                            Res.CompanyNo = CommonFun.GetMidStr(company_str, "（", "）");
                            Res.CompanyName = CommonFun.GetMidStr(company_str, "）", "");
                        }
                        else
                        {
                            Res.CompanyName = company_str;
                        }
                    }
                    if (Res.EmployeeNo.IsEmpty())
                    {
                        Res.EmployeeNo = CommonFun.GetMidStr(httpResult.Html, "最后缴交单位个人账号：", "<");
                    }

                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr");
                    foreach (string item in results)
                    {
                        List<string> tdrow = HtmlParser.GetResultFromParser(item.Replace("&nbsp;", ""), "td");

                        if (tdrow.Count != 8 || tdrow[1] == "日期" || tdrow[2].Contains("上年结余"))
                        {
                            continue;
                        }

                        ProvidentFundDetail _ProvidentFundDetail = new ProvidentFundDetail();

                        try
                        {
                            _ProvidentFundDetail.PayTime = DateTime.ParseExact(year + "-" + tdrow[1], "yyyy-MM-dd", null);
                        }
                        catch
                        { }

                        if (tdrow[2].StartsWith("汇缴"))
                        {
                            _ProvidentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            _ProvidentFundDetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            _ProvidentFundDetail.PersonalPayAmount = tdrow[4].ToDecimal(0) / 2;
                            _ProvidentFundDetail.CompanyPayAmount = _ProvidentFundDetail.PersonalPayAmount;
                            _ProvidentFundDetail.ProvidentFundBase = _ProvidentFundDetail.PersonalPayAmount / rate;

                            string fundtime = CommonFun.GetMidStr(tdrow[2], "[", "]").ToTrim();
                            if (fundtime.Length == 6)
                            {
                                _ProvidentFundDetail.ProvidentFundTime = fundtime.Replace("-", "0");
                            }
                            else if (fundtime.Length == 7)
                            {
                                _ProvidentFundDetail.ProvidentFundTime = DateTime.ParseExact(fundtime, "yyyy-MM", null).ToString("yyyyMM");
                            }
                        }
                        else if (tdrow[2].Contains("提取") || tdrow[2].Contains("支取") || !tdrow[3].IsEmpty())
                        {
                            _ProvidentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                            _ProvidentFundDetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                            _ProvidentFundDetail.PersonalPayAmount = tdrow[3].ToDecimal(0);
                        }
                        else
                        {
                            _ProvidentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            _ProvidentFundDetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            _ProvidentFundDetail.PersonalPayAmount = tdrow[4].ToDecimal(0);
                        }

                        _ProvidentFundDetail.Description = tdrow[2];

                        if (tdrow[7] == Res.CompanyNo)
                        {
                            _ProvidentFundDetail.CompanyName = Res.CompanyName;
                        }

                        Res.ProvidentFundDetailList.Add(_ProvidentFundDetail);
                    }
                }
                #endregion

                #region 贷款基本信息
                Url = baseUrl + "Secure/Yhdkqk.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//a[@class='middleFont']", "href");
                if (results.Count == 0)
                {
                    goto End;
                }

                Url = baseUrl + "Secure/" + results[0];
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string accout_str = CommonFun.GetMidStr(httpResult.Html, "中心贷款账号：", "<");
                Res_Loan.Name = Res.Name;
                Res_Loan.IdentityCard = Res.IdentityCard;
                Res_Loan.Account_Loan = CommonFun.GetMidStr(accout_str, "", "；");
                Res_Loan.Account_CommercialLoan = CommonFun.GetMidStr(accout_str, "银行贷款账号：", "；");

                string loan_baseinfo = CommonFun.GetMidStr(httpResult.Html, "⑴", "</TD>");
                Res_Loan.Loan_Credit = CommonFun.GetMidStr(loan_baseinfo, "贷款金额：", "元").ToDecimal(0);
                Res_Loan.Period_Total = CommonFun.GetMidStr(loan_baseinfo, "期限：", "；");
                Res_Loan.Loan_Start_Date = CommonFun.GetMidStr(loan_baseinfo, "起止日期：", "──");
                Res_Loan.Loan_End_Date = CommonFun.GetMidStr(loan_baseinfo, "──", "；");
                Res_Loan.Bank_Repay = CommonFun.GetMidStr(loan_baseinfo, "放贷银行：", "；");
                Res_Loan.Phone = CommonFun.GetMidStr(loan_baseinfo, "贷款提醒手机号：", "；");
                Res.Phone = Res_Loan.Phone;
                Res_Loan.Period_Payed = Res_Loan.Period_Total.Replace("月", "").ToInt(0) - CommonFun.GetMidStr(loan_baseinfo, "当前剩余期数：", "。").ToInt(0);

                loan_baseinfo = CommonFun.GetMidStr(httpResult.Html, "剩余本金：", "</TD>").ToTrim().Replace("&nbsp;", "");
                Res_Loan.Loan_Balance = CommonFun.GetMidStr(loan_baseinfo, "", "，").ToDecimal(0);
                Res_Loan.Principal_Payed = CommonFun.GetMidStr(loan_baseinfo, "还本金：", "还利息").ToDecimal(0);
                Res_Loan.Interest_Payed = CommonFun.GetMidStr(loan_baseinfo, "还利息：", "<br>").ToDecimal(0);

                Res_Loan.LatestRepayTime = CommonFun.GetMidStr(httpResult.Html, "上次还款日期：", "<br>");
                Res_Loan.Overdue_Period = CommonFun.GetMidStr(httpResult.Html, "欠还次数：", "&nbsp;").ToInt(0); 
                #endregion

                #region 贷款明细
                Url = baseUrl + "Secure/Dkmx.aspx?Num=" + Res_Loan.Account_Loan;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr");
                foreach(string item in results)
                {
                    List<string> tdrow = HtmlParser.GetResultFromParser(item.Replace("&nbsp;", ""), "td");
                    if (tdrow.Count != 8 || tdrow[1] == "日期" || tdrow[1].IsEmpty())
                    {
                        continue;
                    }

                    ProvidentFundLoanDetail _ProvidentFundLoanDetail = new ProvidentFundLoanDetail();
                    _ProvidentFundLoanDetail.Bill_Date = tdrow[1];
                    _ProvidentFundLoanDetail.Record_Date = tdrow[1];
                    _ProvidentFundLoanDetail.Principal = tdrow[3].ToDecimal(0);
                    _ProvidentFundLoanDetail.Interest = tdrow[5].ToDecimal(0);
                    _ProvidentFundLoanDetail.Base = tdrow[6].ToDecimal(0);
                    Res_Loan.ProvidentFundLoanDetailList.Add(_ProvidentFundLoanDetail);
                }

                #endregion

            End:
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
