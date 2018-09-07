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
    public class wuxi : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://58.215.195.18:10010/";//http://www.hzgjj.gov.cn:8080/
        string fundCity = "js_wuxi";
        #endregion
        #region 私有变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        string Url = string.Empty;
        string postdata = string.Empty;
        decimal payRate = (decimal)0.12;
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
                Url = baseUrl + "jcaptcha";
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
            ProvidentFundLoanRes Res_Loan = new ProvidentFundLoanRes();
            Res.ProvidentFundCity = fundCity;
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(fundReq.Token);
                    SpiderCacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数  
                if ((fundReq.LoginType == "1" && fundReq.Identitycard.IsEmpty()) || (fundReq.LoginType == "2" && fundReq.Username.IsEmpty()) || fundReq.Password.IsEmpty())
                {
                    if (fundReq.LoginType == "1")
                    {
                        Res.StatusDescription = "身份证号或密码不能为空";
                    }
                    else
                    {
                        Res.StatusDescription = "公积金账号或密码不能为空";
                    }
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录系统
                //320324198109192527   012329
                Url = baseUrl + "logon.do";
                if (fundReq.LoginType == "1")
                {
                    postdata = String.Format("logontype=1&loginname={0}&type=person&password={1}&_login_checkcode={2}&x=36&y=9", fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
                }
                else
                {
                    postdata = String.Format("logontype=2&loginname={0}&type=person&password={1}&_login_checkcode={2}&x=36&y=9", fundReq.Username, fundReq.Password, fundReq.Vercode);
                }
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
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

                string errorInfo = CommonFun.GetMidStr(httpResult.Html, "出现错误！", "，请核实！");
                if (!errorInfo.IsEmpty())
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                errorInfo = CommonFun.GetMidStr(httpResult.Html, "<FONT size=\"3\"> ", "</FONT>");
                if (!errorInfo.IsEmpty())
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                #endregion

                #region 第二步，查询个人基本信息

                Url = baseUrl + "phoneNum.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/form/table/tr[1]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/form/table/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/form/table/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Phone = results[0];
                }

                Url = baseUrl + "zg_info.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].ToTrim("(元)").ToTrim().ToDecimal(0) / 2;
                    Res.CompanyMonthPayAmount = Res.PersonalMonthPayAmount;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[4]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToTrim("(元)").ToTrim().ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[5]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.SalaryBase = results[0].ToTrim().ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[6]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = results[0].ToTrim("%").ToTrim().ToDecimal(0) / 100;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[7]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = results[0].ToTrim("%").ToTrim().ToDecimal(0) / 100;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[8]/td[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = CommonFun.GetMidStr(results[0], "var aa = \"", "\";aa");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[9]/td[2]", "text");
                if (results.Count > 0)
                {
                    string timeTemp = new Regex(@"[0-9]{8}").Match(results[0]).Value;
                    if (timeTemp.Length == 8)
                    {
                        Res.OpenTime = Convert.ToDateTime(timeTemp.ToDateTime(Consts.DateFormatString5)).ToString("yyyy-MM-dd");
                    }
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[2]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }

                #endregion

                #region 第三步，公积金缴费明细

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
                Url = baseUrl + "mx_info.do";
                postdata = "zjlx=1&hjstatus=%D5%FD%B3%A3%BB%E3%BD%C9&submit=%B2%E9++%D1%AF";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/table/tr/td[2]/div/table/tr[position()>1]", "");

                string payTime = string.Empty;
                string fundTime = string.Empty;
                Regex regex = new Regex(@"[\&nbsp;\,\s]");
                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow.Count != 7)
                    {
                        continue;
                    }
                    payTime = CommonFun.GetMidStr(tdRow[1], "cc = \"", "\";");
                    fundTime = CommonFun.GetMidStr(tdRow[2], "bb = \"", "\";");
                    detail = new ProvidentFundDetail();
                    detail.CompanyName = tdRow[0];
                    detail.PayTime = payTime.ToDateTime(Consts.DateFormatString5);
                    detail.Description = tdRow[3];
                    detail.ProvidentFundTime = fundTime;
                    if (tdRow[3] == "正常汇缴")
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = regex.Replace(tdRow[4], "").ToDecimal(0) * perAccounting.ToString("f2").ToDecimal(0);//金额
                        detail.CompanyPayAmount = regex.Replace(tdRow[4], "").ToDecimal(0) * comAccounting.ToString("f2").ToDecimal(0);//金额
                        detail.ProvidentFundBase = (regex.Replace(tdRow[4], "").ToDecimal(0) / totalRate).ToString("f2").ToDecimal(0);//缴费基数
                        PaymentMonths++;
                    }
                    else if (tdRow[3].Contains("支取"))
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = tdRow[3];
                        detail.PersonalPayAmount = regex.Replace(tdRow[5], "").ToDecimal(0);//金额
                    }
                    else if (tdRow[3].IndexOf("单位", StringComparison.Ordinal) > -1)
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.CompanyPayAmount = regex.Replace(tdRow[4], "").ToDecimal(0);//金额
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = regex.Replace(tdRow[4], "").ToDecimal(0) + regex.Replace(tdRow[5], "").ToDecimal(0);//金额
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion
                #region 第四步,贷款基本信息

                //贷款银行 支取方式
                Url = baseUrl + "tqhdList.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //无贷款记录
                if (httpResult.Html.Contains("暂无记录"))
                {
                    goto End;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='listView']/tr/td", "text", true);
                if (results.Count > 4)
                {
                    Res_Loan.Bank_Delegate = results[2];//委托银行
                    Res_Loan.Withdrawal_Type = CommonFun.GetMidStr(results[4], "", "还款月份").Trim();//支取还贷类型
                    Res_Loan.Name = results[0];
                    Res_Loan.Account_Repay = results[1];//还款账号
                    Res_Loan.Loan_Start_Date = CommonFun.GetMidStr(results[3], "var aa = \"", "\";if");//贷款开始日期
                    Res_Loan.Current_Repay_Date = CommonFun.GetMidStr(results[4], "还款月份:", "");//当期还款日期
                }
                //if (results.Count > 0)
                //{
                //    Res_Loan.Bank_Delegate = results[0];//委托银行
                //}
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='listView']/tr[3]/td", "text", true);
                //if (results.Count > 0)
                //{
                //    Res_Loan.Withdrawal_Type = CommonFun.GetMidStr(results[0], "", " <!--").Trim();//支取还贷类型
                //}
                Url = baseUrl + "grdk_query.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='listViewPagination']/div[1]", "text", true);
                if (results.Count > 0)
                {
                    if (int.Parse(CommonFun.GetMidStr(results[0], "总共有：", "条数据")) == 0)
                    {
                        goto End;
                    }
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='listView']/tr[2]/td[7]/center/a", "href", true);
                string pmaccount = CommonFun.GetMidStr(httpResult.Html, "query.do?pmaccount=", "&&");
                string sdcode = CommonFun.GetMidStr(httpResult.Html, "sdcode=", "&");
                Url = baseUrl + results[0];
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='listView']/td", "text", true);
                if (results.Count > 0)
                {
                    Res_Loan.Loan_Type = CommonFun.GetMidStr(results[0], "个人", "贷款");//贷款类型 
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='listView']/tr[1]/td[2]", "inner");
                if (results.Count > 0)
                {
                    Res_Loan.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='listView']/tr[1]/td[4]", "inner");
                if (results.Count > 0)
                {
                    Res_Loan.Account_Repay = results[0];//还款账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='listView']/tr[2]/td[2]", "inner");
                if (results.Count > 0)
                {
                    Res_Loan.Loan_Start_Date = results[0].Replace(".", "");//贷款开始日期
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='listView']/tr[2]/td[4]", "inner");
                if (results.Count > 0)
                {
                    Res_Loan.Loan_End_Date = CommonFun.GetMidStr(results[0], "aa = \"", "\";");//贷款结束日期
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='listView']/tr[3]/td[2]", "inner");
                if (results.Count > 0)
                {
                    Res_Loan.Loan_Credit = results[0].ToDecimal(0);//贷款金额
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='listView']/tr[4]/td[2]", "inner");
                if (results.Count > 0)
                {
                    Res_Loan.Loan_Balance = results[0].ToDecimal(0);//贷款余额
                }
                #endregion
                #region 第五步,贷款明细

                DateTime dt = DateTime.Now;
                List<string> years = new List<string>() { dt.Year.ToString() };
                Url = baseUrl + "hkmx_query.do";
                do
                {
                    postdata = string.Format("pmaccount={0}&sdcode={1}&years={2}&submit=%B2%E9%D1%AF", pmaccount, sdcode, years[0]);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (dt.Year.ToString() == years[0])
                    {
                        years = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@name='years']/option", "value");
                    }
                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='listView']/tr[position()>2]", "inner", true));
                    years.Remove(years[0]);
                } while (years.Count > 0);
                foreach (var item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "inner");
                    if (tdRow.Count < 8)
                    {
                        continue;
                    }
                    string Record_Date = CommonFun.GetMidStr(tdRow[0], "var aa = \"", "\";aa");
                    string Record_Month = CommonFun.GetMidStr(tdRow[1], "var bb = \"", "\";bb");
                    ProvidentFundLoanDetail loanDetail = Res_Loan.ProvidentFundLoanDetailList.FirstOrDefault(o => o.Record_Month == Record_Month && !string.IsNullOrEmpty(Record_Month));
                    bool isSave = false;
                    if (loanDetail == null)
                    {
                        isSave = true;
                        loanDetail = new ProvidentFundLoanDetail();
                        loanDetail.Record_Date = Record_Date;
                        loanDetail.Record_Month = Record_Month;
                        loanDetail.Balance = tdRow[7].ToDecimal(0);
                    }
                    loanDetail.Principal += tdRow[2].ToDecimal(0);
                    loanDetail.Interest += tdRow[3].ToDecimal(0);
                    loanDetail.Interest_Penalty += tdRow[4].ToDecimal(0);
                    loanDetail.Overdue_Interest += tdRow[5].ToDecimal(0); //逾期利息
                    loanDetail.Base += tdRow[6].ToDecimal(0);
                    loanDetail.Balance = tdRow[7].ToDecimal(0) <= loanDetail.Balance ? tdRow[7].ToDecimal(0) : loanDetail.Balance;
                    if (isSave)
                    {
                        Res_Loan.ProvidentFundLoanDetailList.Add(loanDetail);
                    }
                }
                #endregion

            End: Res.ProvidentFundLoanRes = Res_Loan;
                // Res.PaymentMonths = PaymentMonths;
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
