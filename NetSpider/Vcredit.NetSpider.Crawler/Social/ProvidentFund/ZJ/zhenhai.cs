using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.ZJ
{
    public class zhenhai : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://zhgjj.zhxww.net/gaer/";
        string fundCity = "zj_zhenhai";
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
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

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            //ProvidentFundLoanRes ResLoad = new ProvidentFundLoanRes();
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            ProvidentFundDetail detail = null;
            try
            {
                //校验参数
                if (string.IsNullOrEmpty(fundReq.Password) || string.IsNullOrEmpty(fundReq.Identitycard))
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆

                Url = baseUrl + "gjjsearchx.asp";
                postdata = string.Format("keyword={0}&password={1}", fundReq.Identitycard, fundReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = httpResult.StatusDescription;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string errorInfo = CommonFun.GetMidStr(httpResult.Html, "500px;><BR>", "<BR><BR><a");
                if (!errorInfo.IsEmpty())
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region 第二步,公积金基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", ""), "//table[@cellpadding='3']/tr/td", "", true);
                if (results.Count != 30)
                {
                    Res.StatusDescription = "无公积金信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.Name = fundReq.Name;
                Res.IdentityCard = fundReq.Identitycard;
                Res.Name = results[1];
                Res.CompanyName = results[8];
                Res.CompanyMonthPayRate = results[11].ToDecimal(0) * 0.01M;
                Res.CompanyMonthPayAmount = results[12].ToDecimal(0);
                Res.TotalAmount = results[13].ToDecimal(0);
                Res.PersonalMonthPayRate = results[15].ToDecimal(0) * 0.01M;
                Res.PersonalMonthPayAmount = results[16].ToDecimal(0);
                Res.ProvidentFundNo = results[18];
                Res.SalaryBase = results[24].ToDecimal(0);
                Res.Status = results[26];
                #endregion
                #region 第三步,公积金明细

                decimal perAccounting = (Res.PersonalMonthPayRate > 0 & Res.CompanyMonthPayRate > 0)
                    ? Res.PersonalMonthPayRate / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)
                    : 0.5M;//个人月缴费占比
                string year = string.Empty;
                results = new List<string>();
                while (true)
                {
                    Url = baseUrl + string.Format("gjjsearchml{0}.asp?idp={1}", year, Res.ProvidentFundNo);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    var breakResults = HtmlParser.GetResultFromParser(CommonFun.GetMidStr(httpResult.Html, "类别</td>", "</table><script>name"), "//tr", "");
                    //满页9行数据
                    if (breakResults.Count < 9)
                    {
                        results.AddRange(breakResults);
                        break;
                    }
                    results.AddRange(breakResults);
                    year = CommonFun.GetMidStr(httpResult.Html, "gjjsearchml", ".asp?idp");
                }
                Regex regTime = new Regex(@"[0-9]+");
                foreach (var item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text");
                    if (tdRow.Count < 5) continue;
                    detail = new ProvidentFundDetail();
                    detail.PayTime = tdRow[0].ToDateTime();//缴费年月
                    detail.Description = tdRow[1];
                    if (tdRow[2].ToDecimal(0) > 0)
                    {
                        detail.PersonalPayAmount = tdRow[2].ToDecimal(0);
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                    }
                    else if (tdRow[1].IndexOf("汇缴", StringComparison.Ordinal) > -1)
                    {
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0) * perAccounting;//个人月缴额
                        detail.CompanyPayAmount = tdRow[3].ToDecimal(0) * (1 - perAccounting);//公司月缴额
                        detail.ProvidentFundBase = detail.PersonalPayAmount / (Res.PersonalMonthPayRate > 0 ? Res.PersonalMonthPayRate : payRate);
                        detail.ProvidentFundTime = regTime.Match(tdRow[1]).Value;//应属年月
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                    }
                    else
                    {
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0);//个人缴费
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion
                // Res.ProvidentFundLoanRes = ResLoad;
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
