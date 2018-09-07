using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Web;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.AH
{
    public class luan : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.lagjj.cn/wscx/";
        string fundCity = "ah_luan";
        #endregion
        #region 私有变量

        private decimal payRate = 0.08M;
        #endregion
        public VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;

            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "yzm.php";
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
            ProvidentFundDetail detail = null;
            Res.ProvidentFundCity = fundCity;
            Res.Token = fundReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(fundReq.Token);
                    SpiderCacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Password.IsEmpty() || fundReq.Identitycard.IsEmpty() || fundReq.Vercode.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登陆

                Url = baseUrl + "login.php";
                postdata = string.Format("username={0}&password={1}&dlfs=1&yzm={2}&imageField.x=69&imageField.y=35", fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
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
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');history").Trim();
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #endregion
                #region 第二步，查询个人基本信息

                Url = baseUrl + "la_result.php";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //分析个人基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='news_2']/table[2]/tr/td", "inner", true);
                decimal monthPay = decimal.Zero;
                int count = results.Count - 1;
                for (int i = 0; i <= count; i = i + 2)
                {
                    switch (@results[i].Replace("&nbsp;", "").Trim())
                    {
                        case "姓名：":
                            Res.Name = results[i + 1];
                            break;
                        case "单位名称：":
                            Res.CompanyName = results[i + 1];
                            break;
                        case "单位帐号：":
                            Res.CompanyNo = results[i + 1];
                            break;
                        case "个人帐号：":
                            Res.ProvidentFundNo = results[i + 1];
                            break;
                        case "工资基数：":
                            Res.SalaryBase = results[i + 1].ToDecimal(0);
                            break;
                        case "身份证：":
                            Res.IdentityCard = results[i + 1];
                            break;
                        case "月缴额：":
                            monthPay = results[i + 1].ToDecimal(0);
                            Res.CompanyMonthPayAmount = Res.PersonalMonthPayAmount = Math.Round(monthPay / 2, 2);
                            break;
                        case "余  额：":
                            Res.TotalAmount = results[i + 1].ToDecimal(0);
                            break;
                    }
                }
                if (monthPay > 0 && Res.SalaryBase > 0)
                {
                    payRate = Math.Round(Res.PersonalMonthPayAmount / Res.SalaryBase, 2);
                }
                string detailLink = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='news_2']/table[2]/tr[last()]//a", "href")[0];
                #endregion
                #region 第三步，缴费明细

                Url = baseUrl + "la_result.php" + detailLink;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", ""), "//div[@id='news_1']//tr[position()>1]", "", true);
                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow.Count < 5)
                    {
                        continue;
                    }
                    detail = new ProvidentFundDetail();
                    if (tdRow[0].IndexOf("汇缴", StringComparison.Ordinal) > -1)
                    {
                        int payMonths = CommonFun.GetMidStr(tdRow[1], "[", "]").ToInt(0) > 1 ? CommonFun.GetMidStr(tdRow[1], "[", "]").ToInt(0) : 1;

                        DateTime dtFundTime = DateTime.ParseExact(tdRow[0].Substring(0, 10), "yyyy-MM[d]", null);
                        for (int i = payMonths; i > 0; i--)
                        {
                            if (tdRow[1].IndexOf("-") == 4)
                            {
                                detail.PayTime = DateTime.ParseExact(tdRow[1], "yyyy-MM-dd", null);
                            }
                            detail.Description = payMonths > 1 ? "拆分显示:" + tdRow[0] : tdRow[0];
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            detail.ProvidentFundTime = dtFundTime.AddMonths(i).ToString(Consts.DateFormatString7);
                            detail.PersonalPayAmount = detail.CompanyPayAmount = Math.Round(tdRow[3].ToDecimal(0) / 2, 2); //金额
                            detail.ProvidentFundBase = (detail.PersonalPayAmount / payRate).ToString("f2").ToDecimal(0);//缴费基数
                            Res.ProvidentFundDetailList.Add(detail);
                        }
                    }
                    else
                    {
                        if (tdRow[1].IndexOf("-") == 4)
                        {
                            detail.PayTime = DateTime.ParseExact(tdRow[1], "yyyy-MM-dd", null);
                        }
                        detail.Description = tdRow[0]; //描述
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0);
                        Res.ProvidentFundDetailList.Add(detail);
                    }
                }
                #endregion
                //Res.PaymentMonths = PaymentMonths;
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
