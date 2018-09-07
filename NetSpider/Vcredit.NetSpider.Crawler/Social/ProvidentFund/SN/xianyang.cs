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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SN
{
    /// <summary>
    /// 网站未提供缴费明细查询[5566]
    /// </summary>
    public class xianyang : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://zfgjj.xys.gov.cn/";
        string fundCity = "sn_xianyang";
        #endregion
        #region 私有变量
        int PaymentMonths = 0;
        Regex reg = new Regex(@"[\&nbsp;\s;\,;\u5143]*");//空格,换行,元
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
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = fundCity;
            string GetType = string.Empty;//查询类型[0:个人,1:单位]
            string button = string.Empty;//提交按钮value
            string url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
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
                if (fundReq.Identitycard.IsEmpty() || fundReq.Name.IsEmpty())
                {
                    Res.StatusDescription = "真实姓名，身份证号不能为空!";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 初始化
                url = baseUrl + "chaxun_geren.asp";
                httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='GetType']", "value");
                if (results.Count > 0)
                {
                    GetType = results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_InitError;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='button']", "value");
                if (results.Count > 0)
                {
                    button = results[0];
                }
                #endregion

                #region 第一步,登陆

                url = baseUrl + "chaxun.asp";
                postdata = string.Format("RealName={0}&UserIDCard={1}&GetType={2}&button={3}", fundReq.Name.ToUrlEncode(Encoding.GetEncoding("gb2312")), fundReq.Identitycard, GetType, button.ToUrlEncode(Encoding.GetEncoding("gb2312")).Replace("%20", "+"));
                httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "Post",
                    Postdata = postdata,
                    Encoding = Encoding.GetEncoding("gb2312"),
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
                if (httpResult.Html.Contains("出错原因"))
                {
                    string errorMsg = CommonFun.GetMidStr(httpResult.Html, "<font color=\"ff0000\"><b>", "</b></font></td></tr>");
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                #region 第二步,获取基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@cellspacing='1']/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyName = reg.Replace(results[0], "");//所在单位
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@cellspacing='1']/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyNo = reg.Replace(results[0], "");//单位账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@cellspacing='1']/tr[4]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Name = reg.Replace(results[0], "");//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@cellspacing='1']/tr[5]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = reg.Replace(results[0], "");//个人账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@cellspacing='1']/tr[6]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.IdentityCard = reg.Replace(results[0], "");//身份证号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@cellspacing='1']/tr[7]/td[2]", "text");
                if (results.Count > 0)
                {
                    decimal monthPay = reg.Replace(results[0], "").ToDecimal(0);//月缴存额
                    Res.PersonalMonthPayRate = Res.CompanyMonthPayRate = payRate;
                    Res.PersonalMonthPayAmount = monthPay / 2;
                    Res.CompanyMonthPayAmount = monthPay / 2;
                    Res.SalaryBase = monthPay / (payRate * 2);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@cellspacing='1']/tr[8]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.TotalAmount = reg.Replace(results[0], "").ToDecimal(0);//余额
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@cellspacing='1']/tr[9]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = Regex.Match(results[0], @"[0-9]{4}-[0-9]{2}").Value.Replace("-", "");//缴至年月
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@cellspacing='1']/tr[10]/td[2]", "text");
                if (results.Count > 0)
                {
                    string states = new Regex(@"[0-9]{1}[1-9]*").Match(results[0]).Value;//[(0表示账号状态正常 1、2表示封存)]
                    string statesStr = string.Empty;
                    switch (states)
                    {
                        case "0":
                            statesStr = "正常";
                            break;
                        default:
                            statesStr = "封存";
                            break;
                    }
                    Res.Status = statesStr;//状态
                }
                #endregion
                #region 未提供明细查询
                #endregion
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
    }
}
