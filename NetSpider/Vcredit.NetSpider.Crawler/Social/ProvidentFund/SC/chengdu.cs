using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SC
{
    public class chengdu : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.cdzfgjj.gov.cn/";
        string fundCity = "sc_chengdu";
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
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "api.php?op=checkcode&code_len=4&font_size=20&width=130&height=50";
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
            ProvidentFundLoanRes Res_load = new ProvidentFundLoanRes();
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
                if (fundReq.Username.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "联名卡号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录系统
                Url = baseUrl + "index.php?m=content&c=gjj&a=login";
                postdata = String.Format("cardNo={0}&password={1}&verifyCode={2}", fundReq.Username, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK || results.Count > 0)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='content guery']", "");
                if ( results.Count > 0)
                {
                    Res.StatusDescription = "登录失败," + results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (fundReq.LoginType == "1")
                {
                    Res.BankCardNo = fundReq.Username;//联名卡号登陆方式
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，查询个人基本信息
                Url = baseUrl + "index.php?m=content&c=gjj&a=info";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='form-table']/tr/td[@class='c']", "");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];
                    Res.Name = results[1];
                    Res.IdentityCard = results[3];
                    Res.Phone = results[5];
                }
                else
                {
                    Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion

                #region 第三步，查询公司信息、公积金信息
                Url = baseUrl + "index.php?m=content&c=gjj&a=account";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='form-table']/tr/td[@class='c']", "");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];
                    Res.CompanyName = results[1];

                    if (!results[3].IsEmpty())
                    {
                        Res.SalaryBase = results[3].ToDecimal(0);
                    }
                    if (!results[4].IsEmpty())
                    {
                        Res.CompanyMonthPayAmount = results[4].ToDecimal(0);
                    }
                    if (!results[5].IsEmpty())
                    {
                        Res.PersonalMonthPayAmount = results[5].ToDecimal(0);
                    }
                    if (!results[6].IsEmpty())
                    {
                        Res.TotalAmount = results[6].ToDecimal(0);
                    }
                    Res.Status = results[7];
                }
                #endregion

                #region 第四步，查询缴费明细
                string stime = "2000-01-01";
                string etime = DateTime.Now.ToString(Consts.DateFormatString2);
                Url = baseUrl + "index.php?m=content&c=gjj&a=detailquery";
                postdata = string.Format("startDate={0}&endDate={1}", stime, etime);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//tbody/tr", "");
                ProvidentFundDetail detail = null;
                int PaymentMonths = 0;
                foreach (string item in results)
                {
                    var strDetail = HtmlParser.GetResultFromParser(item, "//td", "");
                    if (strDetail.Count > 0)
                    {
                        detail = new ProvidentFundDetail();
                        detail.CompanyName = strDetail[1];
                        detail.PayTime = DateTime.ParseExact(strDetail[2], "yyyyMMdd", null);
                        if (strDetail[3].IndexOf("汇缴") != -1)
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            detail.PersonalPayAmount = strDetail[4].ToDecimal(0) / 2;//金额
                            detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                            detail.ProvidentFundTime = CommonFun.GetMidStr(strDetail[3], "汇缴", "");
                            PaymentMonths++;
                        }
                        else
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.Description = strDetail[3];
                            detail.PersonalPayAmount = strDetail[4].ToDecimal(0);
                        }
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion

                #region 第五步，贷款基本信息查询

                Url = baseUrl + "index.php?m=content&c=gjj&a=agreement";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='form-table']/tr/td[@class='c']");
                if (results.Count!=11)
                {
                    Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                    return Res;
                }
                Res_load.Con_No = results[0];//合同号
                Res_load.Account_Loan = results[1];//个人客户号
                Res_load.Name = results[2];//姓名
                if (Res.Name == Res_load.Name)
                {
                    Res_load.IdentityCard= Res.IdentityCard;
                }
                Res_load.Account_Repay = results[3];//还款账号
                Res_load.Loan_Credit = results[5].ToDecimal(0);//公积金贷款金额
                Res_load.Loan_Rate = (results[6].ToTrim("%").ToDecimal(0) * 0.01M).ToString(CultureInfo.InvariantCulture);//贷款年利率
                Res_load.Period_Total = results[7].ToTrim("期");//还款期数
                Res_load.Repay_Type = results[9];//还款方式
           
                Url = baseUrl + "index.php?m=content&c=gjj&a=loandetail";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table-det']/tbody/tr[1]/td");
                if (results.Count==6)
                {
                    Res_load.Loan_Balance = results[0].ToDecimal(0);//贷款余额
                    Res_load.Period_Payed = results[1].ToTrim("期").ToInt(0);//已还款期数
                    Res_load.Principal_Payed = results[3].ToDecimal(0);//已还本金
                    Res_load.Interest_Payed = results[4].ToDecimal(0);//已还利息
                    Res_load.Interest_UnPayed = results[5].ToDecimal(0);//逾期未还罚息
                }

                #endregion

                #region 第六步,还款明细

                Url = baseUrl + "index.php?m=content&c=gjj&a=loanall";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table-det']/tbody/tr");
                foreach (string s in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(s, "//td", "text");
                    if (tdRow.Count < 5) continue;
                    ProvidentFundLoanDetail loanDetail = new ProvidentFundLoanDetail();
                    loanDetail.Record_Month = tdRow[3].Substring(0,6);
                    loanDetail.Record_Date = tdRow[3];
                    loanDetail.Principal = tdRow[0].ToDecimal(0);
                    loanDetail.Interest = tdRow[1].ToDecimal(0);
                    loanDetail.Interest_Penalty = tdRow[2].ToDecimal(0);
                    loanDetail.Record_Period = tdRow[4];
                    Res_load.ProvidentFundLoanDetailList.Add(loanDetail);
                }
                #endregion
                Res.ProvidentFundLoanRes = Res_load;
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
