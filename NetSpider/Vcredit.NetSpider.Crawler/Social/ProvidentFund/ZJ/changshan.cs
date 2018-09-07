using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.ZJ
{
    public class changshan : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.csfgb.cn/";
        string fundCity = "zj_changshan";
        #endregion
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Res.StatusDescription = fundCity + "无需初始化";
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

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            ProvidentFundLoanRes resLoad = new ProvidentFundLoanRes();
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            try
            {
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || fundReq.Password.IsEmpty() || fundReq.FundAccount.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步登陆
                Url = baseUrl + "cxgjj.asp";
                postdata = string.Format("cx=gjj&sfz={0}&zgzh={1}&psw={2}&Submit1=%E6%9F%A5%E8%AF%A2", fundReq.Identitycard, fundReq.FundAccount, fundReq.Password);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@border='1']/tr[2]/td", "text", true);
                if (results.Count != 5)
                {
                    Res.StatusDescription = "无公积金信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                #region  第二步 公积金基本信息

                Res.ProvidentFundNo = fundReq.FundAccount;
                Res.Name = results[0];
                Res.IdentityCard = results[1];
                Res.CompanyName = results[2];
                Res.SalaryBase = results[3].Replace("元", "").ToDecimal(0);
                Res.TotalAmount = results[4].Replace("元", "").ToDecimal(0);
                #endregion
                #region 第三步 贷款基本信息
                Url = baseUrl + "cxgjj.asp";
                postdata = string.Format("cx=dk&sfz={0}&zgzh={1}&psw={2}&Submit1=%E6%9F%A5%E8%AF%A2", fundReq.Identitycard, fundReq.FundAccount, fundReq.Password);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@border='1']/tr[position()>1]/td", "text", true);
                if (results.Count != 15)
                {
                    Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                    return Res;
                }
                Res.ProvidentFundNo = fundReq.FundAccount;
                resLoad.Name = results[0];
                resLoad.IdentityCard = results[1];
                resLoad.Bank_Delegate = results[2];//委托银行
                resLoad.Loan_Credit = results[3].Replace("元","").ToDecimal(0);//贷款金额
                resLoad.Principal_Payed = results[4].Replace("元", "").ToDecimal(0);//已还本金
                resLoad.Interest_Payed = results[10].Replace("元", "").ToDecimal(0);//已还利息
                resLoad.Loan_Balance = results[11].Replace("元", "").ToDecimal(0) + results[12].Replace("元", "").ToDecimal(0);//贷款余额
                #endregion
                Res.ProvidentFundLoanRes = resLoad;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
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

