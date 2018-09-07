using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Web.UI;
using Vcredit.Common;
using Vcredit.Common.Ext;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using System.Web;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HN
{
    /// <summary>
    /// 
    /// </summary>
    public class changsha : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.csgjj.com.cn:8001/";//
        string fundCity = "hn_changsha";
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                Url = baseUrl;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "CaptchaImg";
                httpItem = new HttpItem()
                {
                    URL = Url,
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                vcRes.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
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
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            ProvidentFundLoanRes Res_Loan = new ProvidentFundLoanRes();
            ProvidentFundDetail detail = null;
            List<string> results = new List<string>();
            Res.ProvidentFundCity = fundCity;
            string url = string.Empty;

            string postData = string.Empty;
            decimal payRate = (decimal)0.08;
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Username.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //登陆
                url = baseUrl + "login.do";
                postData = string.Format("username={0}&password={1}&loginType=4&vertcode={2}&bsr=firefox%2F46.0&vertype=1", fundReq.Username, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "Post",
                    Postdata = postData,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);//
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryError;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (httpResult.Html.StartsWith("{") && httpResult.Html.EndsWith("}"))
                {
                    Res.StatusDescription = jsonService.GetResultFromParser(httpResult.Html, "msg");
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //基本信息
                url = baseUrl + "per/queryPerInfo.do";
                httpItem = new HttpItem()
                {
                    URL = url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                Res.CompanyName = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='corpcode2']")[0];
                Res.CompanyNo = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='corpcode']")[0];
                Res.PersonalMonthPayRate = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='perperscale']")[0].ToDecimal(0) * 0.01M;
                Res.CompanyMonthPayRate = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='percorpscale']")[0].Replace("%", "").ToDecimal(0) * 0.01M;
                var d = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='bkname']");
                if (d.Count > 1)
                {
                    Res.Bank = d[0];
                    Res.SalaryBase = d[1].Replace("元", "").ToDecimal(0);
                }
                Res.PersonalMonthPayAmount = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='perdepmny']")[0].ToDecimal(0);
                Res.CompanyMonthPayAmount = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='corpdepmny']")[0].ToDecimal(0);
                Res.TotalAmount = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='accbal']")[0].Replace("元", "").ToDecimal(0);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='regtime']");
                if (results.Count > 0)
                {
                    Res.OpenTime = Convert.ToDateTime(results[0]).ToString(Consts.DateFormatString2);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='payendmnh']");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = Convert.ToDateTime(results[0]).ToString(Consts.DateFormatString7);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='bkcard']");
                if (results.Count > 0)
                {
                    Res.BankCardNo = results[0].Trim();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='glitem first']/div");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='glitem']/div");
                if (results.Count > 1)
                {
                    Res.ProvidentFundNo = results[0];
                    Res.Phone = results[1];
                }
                //个人明细
                List<ChangShadetail> details = new List<ChangShadetail>();
                url = baseUrl + "/per/queryPerDeposit!queryPerByYear.do";
                int index = 0;
                int pages = 0;
                while (true)
                {
                    postData = string.Format("gridInfo%5B'dataList_limit'%5D={1}&gridInfo%5B'dataList_start'%5D={0}&gridInfo%5B'dataList_limit'%5D={1}&gridInfo%5B'dataList_start'%5D={0}&gridInfo%5B'dataList_selected'%5D=%5B%5D&gridInfo%5B'dataList_modified'%5D=%5B%5D&gridInfo%5B'dataList_removed'%5D=%5B%5D&gridInfo%5B'dataList_added'%5D=%5B%5D&dto%5B'__pager'%5D=1", index * 1000, 1000);
                    httpItem = new HttpItem()
                    {
                        URL = url,
                        Method = "Post",
                        Postdata = postData,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (jsonParser.GetResultFromParser(httpResult.Html, "success").ToLower() != "true")
                    {
                        break;
                    }
                    if (index == 0)
                    {
                        var total = jsonParser.GetResultFromMultiNode(httpResult.Html, "lists:dataList:total");
                        if (total == "0")
                        {
                            break;
                        }
                        pages = (int)Math.Ceiling(double.Parse(total) / 1000);
                    }
                    details.AddRange(jsonParser.DeserializeObject<List<ChangShadetail>>(jsonParser.GetResultFromMultiNode(httpResult.Html, "lists:dataList:list")));
                    index++;
                    if (index >= pages)
                    {
                        break;
                    }
                }
                decimal perRate = decimal.Zero;
                decimal comRate = decimal.Zero;
                if (Res.PersonalMonthPayRate > 0 && Res.CompanyMonthPayRate > 0)
                {
                    perRate = Res.PersonalMonthPayRate / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate);
                    comRate = 1 - perRate;
                }
                else
                {
                    perRate = comRate = 0.5M;
                }
                foreach (ChangShadetail item in details)
                {
                    detail = new ProvidentFundDetail();
                    detail.PayTime = item.acctime.ToDateTime();
                    detail.CompanyName = item.corpname;
                    detail.Description = item.remark;
                    if (item.remark.Contains("汇缴"))
                    {
                        detail.ProvidentFundTime = item.accmnh;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = item.income.ToDecimal(0) * perRate;
                        detail.CompanyPayAmount = item.income.ToDecimal(0) * comRate;
                        detail.ProvidentFundBase = detail.PersonalPayAmount / perRate;//缴费基数
                    }
                    else if (item.remark.Contains("还贷") || item.remark.Contains("取"))
                    {
                        detail.ProvidentFundTime = item.accmnh;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                        detail.PersonalPayAmount = item.outcome.ToDecimal(0);//金额

                    }
                    else if (item.remark.Contains("补缴"))
                    {
                        detail.ProvidentFundTime = item.accmnh;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Back;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Back;
                        detail.CompanyPayAmount = item.income.ToDecimal(0) + item.outcome.ToDecimal(0);//金额
                    }
                    else
                    {
                        detail.ProvidentFundTime = item.accmnh;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = item.income.ToDecimal(0) + item.outcome.ToDecimal(0);//金额

                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                //贷款基本信息
                url = baseUrl + "per/queryPerAppState.do";
                httpItem = new HttpItem()
                {
                    URL = url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string basestr = CommonFun.GetMidStr(httpResult.Html, "var loanData = '[", "]';");
                if (!basestr.StartsWith("{") && !basestr.EndsWith("}"))
                {
                    goto End;
                }
                ChangShaLoadBase loadBase = jsonService.DeserializeObject<ChangShaLoadBase>(basestr);
                Res_Loan.Name = Res.Name;
                Res_Loan.IdentityCard = Res.IdentityCard;
                Res_Loan.Phone = Res.Phone;
                Res_Loan.House_Purchase_Address = loadBase.address;
                Res_Loan.Con_No = loadBase.loancontractno;
                Res_Loan.Account_Repay = loadBase.loanacc;
                Res_Loan.Loan_Credit = loadBase.loanmny.ToDecimal(0);
                Res_Loan.Loan_Balance = loadBase.loanbal.ToDecimal(0);
                Res_Loan.Loan_Rate = (loadBase.rate.ToDecimal(0) * 0.01M).ToString(CultureInfo.InvariantCulture);
                Res_Loan.Period_Total = loadBase.loanmnhs;
                Res_Loan.Period_Payed = loadBase.paymnhs.ToInt(0);
                Res_Loan.Repay_Type = loadBase.repayway;
                Res_Loan.Current_Repay_Date = loadBase.ydhkr;
                Res_Loan.Bank_Delegate = loadBase.loanbk;
                Res_Loan.Status = loadBase.loanstate;
                //贷款明细
                ProvidentFundLoanDetail Res_LoanDetail = new ProvidentFundLoanDetail();
                List<ChangShaLoadDetail> Loandetails = new List<ChangShaLoadDetail>();
                string enddate = DateTime.Now.ToString(Consts.DateFormatString2);
                url = baseUrl + "per/queryPerLoanPay!queryPerPay.do";
                index = 0;
                pages = 0;
                while (true)
                {
                    postData = string.Format("dto%5B'startdate'%5D=1990-01-01&dto%5B'enddate'%5D={0}&dto%5B'addressSel'%5D={1}&dto%5B'addressSel_md5list'%5D=&dto%5B'addressSel_desc'%5D={2}&gridInfo%5B'dataList_limit'%5D={4}&gridInfo%5B'dataList_start'%5D={3}&dto%5B'loancardcode'%5D={1}", enddate, Res_Loan.Con_No, Res_Loan.House_Purchase_Address.ToUrlEncode(), index * 1000, 1000);
                    httpItem = new HttpItem()
                    {
                        URL = url,
                        Method = "Post",
                        Postdata = postData,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (jsonParser.GetResultFromParser(httpResult.Html, "success").ToLower() != "true")
                    {
                        break;
                    }
                    if (index == 0)
                    {
                        var total = jsonParser.GetResultFromMultiNode(httpResult.Html, "lists:dataList:total");
                        if (total == "0")
                        {
                            break;
                        }
                        pages = (int)Math.Ceiling(double.Parse(total) / 1000);
                    }
                    Loandetails.AddRange(jsonParser.DeserializeObject<List<ChangShaLoadDetail>>(jsonParser.GetResultFromMultiNode(httpResult.Html, "lists:dataList:list")));
                    index++;
                    if (index >= pages)
                    {
                        break;
                    }
                }
                foreach (ChangShaLoadDetail item in Loandetails)
                {
                    ProvidentFundLoanDetail detailRes = Res_Loan.ProvidentFundLoanDetailList.FirstOrDefault(o => o.Record_Month == item.bkdebitmnt);
                    bool needSave = false;
                    if (detailRes == null)
                    {
                        needSave = true;
                        detailRes = new ProvidentFundLoanDetail();
                        detailRes.Description = item.debittype;//描述-还款类型
                        detailRes.Record_Month = item.bkdebitmnt;//还款年月
                        detailRes.Record_Date = item.bkdebitdate;//记账年月
                        detailRes.Record_Period = item.loanmnhs;//还款期数
                        detailRes.Balance = item.loanbal.ToDecimal(0);//贷款余额
                    }
                    detailRes.Balance = detailRes.Balance <= item.loanbal.ToDecimal(0) ? detailRes.Balance : item.loanbal.ToDecimal(0);//贷款余额
                    detailRes.Base += item.repaytotal.ToDecimal(0);
                    detailRes.Overdue_Interest += item.comrate.ToDecimal(0);
                    detailRes.Interest_Penalty += item.punishratemny.ToDecimal(0);
                    detailRes.Interest += item.repayratemny.ToDecimal(0);
                    detailRes.Principal += item.repaymny.ToDecimal(0);
                    if (needSave)
                    {
                        Res_Loan.ProvidentFundLoanDetailList.Add(detailRes);
                    }
                }
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
    /// <summary>
    /// 公积金明细
    /// </summary>
    internal class ChangShadetail
    {
        /// <summary>
        /// 入账时间
        /// </summary>
        public string acctime { get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public string remark { get; set; }
        /// <summary>
        /// 收入
        /// </summary>
        public string income { get; set; }
        /// <summary>
        /// 支出
        /// </summary>
        public string outcome { get; set; }
        /// <summary>
        /// 个人账号
        /// </summary>
        public string percode { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string pername { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string corpname { get; set; }
        /// <summary>
        /// 公司编号
        /// </summary>
        public string corpcode { get; set; }
        /// <summary>
        /// 当前余额
        /// </summary>
        public string accbal { get; set; }
        /// <summary>
        /// 缴存年月
        /// </summary>
        public string accmnh { get; set; }
    }
    /// <summary>
    /// 贷款基本信息
    /// </summary>
    internal struct ChangShaLoadBase
    {
        /// <summary>
        /// 下次还款金额
        /// </summary>
        public string nextpaymny { get; set; }
        /// <summary>
        /// 贷款合同号
        /// </summary>
        public string loancontractno { get; set; }
        /// <summary>
        /// 约定还款日
        /// </summary>
        public string ydhkr { get; set; }
        /// <summary>
        /// 已还期数
        /// </summary>
        public string paymnhs { get; set; }
        /// <summary>
        /// 贷款金额
        /// </summary>
        public string loanmny { get; set; }
        /// <summary>
        /// 还款账号
        /// </summary>
        public string loanacc { get; set; }
        /// <summary>
        /// 还款方式
        /// </summary>
        public string repayway { get; set; }
        /// <summary>
        /// 贷款期数
        /// </summary>
        public string loanmnhs { get; set; }
        /// <summary>
        /// 贷款进度
        /// </summary>
        public string loanstate { get; set; }
        /// <summary>
        /// 下次还款日期
        /// </summary>
        public string nextpaytime { get; set; }
        /// <summary>
        /// 贷款银行
        /// </summary>
        public string loanbk { get; set; }
        /// <summary>
        /// 贷款利率
        /// </summary>
        public string rate { get; set; }
        /// <summary>
        /// 房屋地址
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// 剩余本金
        /// </summary>
        public string loanbal { get; set; }
    }
    /// <summary>
    /// 贷款明细
    /// </summary>
    internal class ChangShaLoadDetail
    {
        /// <summary>
        /// 罚息
        /// </summary>
        public string punishratemny { get; set; }
        /// <summary>
        /// 利息
        /// </summary>
        public string repayratemny { get; set; }
        /// <summary>
        /// 对应年月
        /// </summary>
        public string bkdebitmnt { get; set; }
        /// <summary>
        /// 1
        /// </summary>
        public string debitstate { get; set; }
        /// <summary>
        /// 还款日期
        /// </summary>
        public string bkdebitdate { get; set; }

        public string rate { get; set; }
        /// <summary>
        /// 贷款房屋地址
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// 还款类型
        /// </summary>
        public string debittype { get; set; }
        /// <summary>
        /// 本金
        /// </summary>
        public string repaymny { get; set; }
        /// <summary>
        /// 贷款本金余额
        /// </summary>
        public string loanbal { get; set; }
        /// <summary>
        /// 还款合计
        /// </summary>
        public string repaytotal { get; set; }
        /// <summary>
        /// 贷款合同号
        /// </summary>
        public string loancardcode { get; set; }
        /// <summary>
        /// 复利
        /// </summary>
        public string comrate { get; set; }
        /// <summary>
        /// 还款期数
        /// </summary>
        public string loanmnhs { get; set; }
    }
}
