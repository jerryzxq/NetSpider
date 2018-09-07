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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.BJ
{
    public class guoguan : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://wzchx.zzz.gov.cn/queryinfo/";
        string fundCity = "bj_guoguan";
        #endregion
        #region 私有变量
        string Url = string.Empty;
        string postdata = string.Empty;
        decimal payRate = (decimal)0.12;
        List<string> results = new List<string>();
        ProvidentFundDetail detail = null;
        
        #endregion
        public VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
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

                Url = baseUrl + "main/getCaptcha";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "main/login.jsp",
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
                if (fundReq.Identitycard.IsEmpty() || fundReq.Username.IsEmpty() || fundReq.Name.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录系统
                Url = baseUrl + "main/login";
                postdata = string.Format("user.persAcctNo={0}&user.uidentifyno={1}&user.uname={2}&user.password={3}&validateno={4}", fundReq.Username, CommonFun.GetMd5Str(fundReq.Identitycard), fundReq.Name.ToUrlEncode(Encoding.GetEncoding("gb2312")), CommonFun.GetMd5Str(fundReq.Password), fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = baseUrl + "main/login.jsp",
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='a']", "value");
                if (results.Count == 0)
                {
                    Res.StatusDescription = "登录失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，查询个人基本信息

                Url = baseUrl + "reserved/evdetail?pers_acct_no=" + fundReq.Username;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tttttttttt']/tr/td");
                if (results.Count == 39)
                {
                    Res.Name = results[1];
                    Res.ProvidentFundNo = results[3];
                    Res.IdentityCard = fundReq.Identitycard;
                    Res.BankCardNo = results[7];
                    Res.CompanyName = results[9];
                    Res.OpenTime = results[13];
                    Res.Status = results[15];
                    Res.SalaryBase = results[17].ToDecimal(0);
                    List<string> _rate_list = results[20].Split('/').ToList<string>();
                    if (_rate_list.Count == 2)
                    {
                        Res.PersonalMonthPayRate = _rate_list[0].Replace("%", "").ToDecimal(0);
                        Res.CompanyMonthPayRate = _rate_list[1].Replace("%", "").ToDecimal(0);
                    }
                    Res.PersonalMonthPayAmount = results[22].ToDecimal(0) / 2;
                    Res.CompanyMonthPayAmount = Res.PersonalMonthPayAmount;
                }
                #endregion

                #region 第三步，缴费明细
                Url = baseUrl + "reserved/evdtl?pers_acct_no=" + fundReq.Username;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tttttttttt']/tr");
                int PaymentMonths = 0;
                foreach (string item in results)
                {
                    List<string> _detail = HtmlParser.GetResultFromParser(CommonFun.ClearFlag(item), "td");
                    if (_detail.Count != 5 || _detail[0] == "日期")
                    {
                        continue;
                    }
                    ProvidentFundDetail _ProvidentFundDetail = new ProvidentFundDetail();
                    _ProvidentFundDetail.PayTime = DateTime.ParseExact(_detail[0], "yyyy-MM-dd", null);
                    string Description = HttpUtility.HtmlDecode(_detail[1]);
                    if (Description.IndexOf("汇缴") != -1)//正常汇缴
                    {
                        _ProvidentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        _ProvidentFundDetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        _ProvidentFundDetail.ProvidentFundTime = Description.Substring(2, 6);
                        _ProvidentFundDetail.PersonalPayAmount = _detail[3].ToDecimal(0) / 2;//金额
                        _ProvidentFundDetail.CompanyPayAmount = _ProvidentFundDetail.PersonalPayAmount;//金额
                        _ProvidentFundDetail.ProvidentFundBase = (_ProvidentFundDetail.PersonalPayAmount / payRate).ToString("f2").ToDecimal(0);//缴费基数
                        _ProvidentFundDetail.Description = Description;
                        PaymentMonths++;
                    }
                    else if (Description.Contains("提取"))
                    {
                        _ProvidentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        _ProvidentFundDetail.PaymentType = Description;
                        _ProvidentFundDetail.Description = Description;
                        _ProvidentFundDetail.PersonalPayAmount = _detail[2].ToDecimal(0) == 0 ? _detail[3].ToDecimal(0) : _detail[2].ToDecimal(0);//金额
                    }
                    else
                    {
                        _ProvidentFundDetail.PaymentFlag = Description;
                        _ProvidentFundDetail.PaymentType = Description;
                        _ProvidentFundDetail.Description = Description;
                        _ProvidentFundDetail.PersonalPayAmount = _detail[2].ToDecimal(0) == 0 ? _detail[3].ToDecimal(0) : _detail[2].ToDecimal(0);//金额
                    }

                    Res.ProvidentFundDetailList.Add(_ProvidentFundDetail);

                }

                #endregion

                #region 第四步，贷款基本信息
                Url = baseUrl + "reserved/loaninfo?pers_acct_no=" + fundReq.Username;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tttttttttt']/tr/td");
                if (results.Count == 47)
                {
                    Res_Loan.Loan_Sid = results[2];
                    Res_Loan.ProvidentFundNo = results[8];
                    Res_Loan.House_Purchase_Address = results[13];
                    Res_Loan.Loan_Credit = results[15].ToDecimal(0);
                    Res_Loan.Period_Total = results[17];
                    Res_Loan.Loan_Rate = results[19];
                    Res_Loan.Repay_Type = results[21];
                    Res_Loan.Loan_Start_Date = results[23];
                    Res_Loan.Current_Repay_Date = results[25];
                    Res_Loan.Bank_Delegate = results[27];
                    Res_Loan.Status = results[30];
                    Res_Loan.Account_Repay = results[34];
                    Res_Loan.Current_Repay_Total = results[38].ToDecimal(0);
                    Res_Loan.Principal_Payed = results[40].ToDecimal(0);
                    Res_Loan.Period_Payed = results[42].ToInt(0);
                    Res_Loan.Loan_Balance = results[46].ToDecimal(0);
                }
                #endregion

                #region 第五步，贷款明细
                Url = baseUrl + "reserved/repayinfo?pers_acct_no=" + fundReq.Username;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                int pageno = 1;
                int pagecount = 0;
                int total = 0;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='Page_pageCount']", "value");
                if (results.Count > 0)
                {
                    pagecount = results[0].ToInt(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='Page_totle']", "value");
                if (results.Count > 0)
                {
                    total = results[0].ToInt(0);
                }

                while (pageno <= pagecount)
                {
                    Url = baseUrl + "reserved/repayinfo";
                    postdata = string.Format("page.page={0}&page.perPageCount=10&page.pageCount={1}&page.totle={2}", pageno, pagecount, total);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tttttttttt']/tr");
                    foreach (string item in results)
                    {
                        List<string> _detail = HtmlParser.GetResultFromParser(CommonFun.ClearFlag(item), "td");
                        if (_detail.Count != 8 || _detail[0] == "贷款合同编号")
                        {
                            continue;
                        }

                        string period = _detail[2];
                        ProvidentFundLoanDetail _ProvidentFundLoanDetail = Res_Loan.ProvidentFundLoanDetailList.Where(o => o.Record_Period == period).FirstOrDefault();

                        bool NeedAdd = false;
                        if (_ProvidentFundLoanDetail == null)
                        {
                            _ProvidentFundLoanDetail = new ProvidentFundLoanDetail();
                            _ProvidentFundLoanDetail.Record_Period = period;
                            _ProvidentFundLoanDetail.Bill_Date = _detail[1];
                            _ProvidentFundLoanDetail.Record_Date = _detail[1];

                            NeedAdd = true;
                        }

                        _ProvidentFundLoanDetail.Principal += _detail[4].ToDecimal(0);
                        _ProvidentFundLoanDetail.Interest += _detail[5].ToDecimal(0);
                        _ProvidentFundLoanDetail.Interest_Penalty += _detail[6].ToDecimal(0);
                        _ProvidentFundLoanDetail.Base += _detail[7].ToDecimal(0);

                        if (_ProvidentFundLoanDetail.Principal != 0)
                        {
                            _ProvidentFundLoanDetail.Bill_Date = _detail[1];
                            _ProvidentFundLoanDetail.Record_Date = _detail[1];
                            _ProvidentFundLoanDetail.Balance = _detail[3].ToDecimal(0) - _ProvidentFundLoanDetail.Principal;
                        }

                        if (NeedAdd)
                        {
                            Res_Loan.ProvidentFundLoanDetailList.Add(_ProvidentFundLoanDetail);
                        }
                    }
                    pageno++;
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
