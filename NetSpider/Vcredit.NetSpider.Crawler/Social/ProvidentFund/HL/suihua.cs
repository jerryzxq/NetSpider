using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HL
{
    public class suihua : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://222.32.91.14/wscx/";
        string fundCity = "hl_suihua";
        #endregion
        #region 私有变量
        decimal perAccounting = 0;//个人占比
        decimal comAccounting = 0;//公司占比
        decimal totalRate = 0;//总缴费比率
        decimal payRate = (decimal)0.08;
        int PaymentMonths = 0;
        Regex reg = new Regex(@"[\s;\&nbsp;\,;\%]");
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        ProvidentFundDetail detail = null;
        int beginYear = 0;//开户日期开户年份
        int endYear = 0;//查询结束年份
        Regex regYear = new Regex(@"[0-9]{4}");
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
            List<string> results = new List<string>();
            List<string> searchYear = new List<string>();
            string Url = string.Empty;
            string postdata = string.Empty;
            string cxyd = string.Empty;
            string cxydmc = string.Empty;
            string dbname = string.Empty;
            string zgzh = string.Empty;
            string dwbm = string.Empty;
            string zgxm = string.Empty;
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

                #region 初始化
                
                Url = baseUrl + "zfbzgl/zfbzsq/login.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("GBK"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='cxydmc']", "value", true);
                if (results.Count > 0)
                {
                    cxyd = cxydmc = results[0];
                }
                else
                {
                    Res.StatusDescription = "页面初始化失败,请核对网站是否改版";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dbname']", "value", true);
                if (results.Count > 0)
                {
                    dbname = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@name='cxyd']/option", "text", true);
                if (results.Count > 0)
                {
                    searchYear = results;
                    searchYear.Remove("当前年度");
                    searchYear.Add("当前年度");
                }
                #endregion
                int index = 0;
                for (int i = searchYear.Count - 1; i >= i - 4; i--)
                {
                    //根据开户日期,以确定是否继续查询剩余年份公积金明细
                    if (!string.IsNullOrEmpty(Res.OpenTime))
                    {
                        if (regYear.Matches(Res.OpenTime).Count>0)
                        {
                            beginYear = regYear.Matches(Res.OpenTime)[0].Value.ToInt(0);
                        }
                        if (regYear.Matches(searchYear[i]).Count > 0)
                        {
                            endYear = regYear.Matches(searchYear[i])[0].Value.ToInt(0);
                        }
                        if (beginYear > endYear)
                        {
                            break;
                        }
                    }
                    if (searchYear[i] == "当前年度")
                    {
                        cxyd = cxyd.ToUrlEncode(Encoding.GetEncoding("GBK"));
                    }
                    #region 第一步,登陆
                    Url = baseUrl + string.Format("zfbzgl/zfbzsq/login_hidden.jsp?password={0}&sfzh={1}&cxyd={2}&dbname={3}", fundReq.Password, fundReq.Identitycard, cxyd, dbname);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        Referer = baseUrl + "zfbzgl/zfbzsq/login.jsp",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\");");
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        Res.StatusDescription = errorMsg;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgzh']", "value");
                    if (results.Count > 0)
                    {
                        zgzh = results[0];
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dwbm']", "value");
                    if (results.Count > 0)
                    {
                        dwbm = results[0];
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgxm']", "value");
                    if (results.Count > 0)
                    {
                        zgxm = results[0];
                    }
                    if (searchYear[i] != "当前年度")
                    {
                        dbname = dbname + "_" + searchYear[i];
                    }
               
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    Url = baseUrl + string.Format("zfbzgl/zfbzsq/main_menu.jsp?zgzh={0}&sfzh={1}&zgxm={2}&dwbm={3}&cxyd={4}&dbname={5}", zgzh, fundReq.Identitycard, zgxm.ToUrlEncode(Encoding.GetEncoding("GBK")), dwbm, cxyd, dbname);
                    postdata = string.Format("zgzh={0}&sfzh={1}&zgxm={4}&dwbm={2}&cxyd={5}&dbname={3}", zgzh, fundReq.Identitycard, dwbm, dbname, "", "");
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "Post",
                        Postdata = postdata,
                        Encoding = Encoding.GetEncoding("GBK"),
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    #endregion
                    #region 第二步,获取基本信息

                    if (index < 1)
                    {
                        index++;
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='580']/tr[1]/td[2]", "text");
                        if (results.Count > 0)
                        {
                            Res.Name = reg.Replace(results[0], ""); //姓名
                        }
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='580']/tr[1]/td[4]", "text");
                        if (results.Count > 0)
                        {
                            Res.BankCardNo = reg.Replace(results[0], "");//银行账号
                        }
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='580']/tr[2]/td[2]", "text");
                        if (results.Count > 0)
                        {
                            Res.IdentityCard = reg.Replace(results[0], ""); //身份账号
                        }
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='580']/tr[2]/td[4]", "text");
                        if (results.Count > 0)
                        {
                            Res.ProvidentFundNo = reg.Replace(results[0], "");//职工账号
                        }
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='580']/tr[3]/td[2]", "text");
                        if (results.Count > 0)
                        {
                            Res.CompanyName = reg.Replace(results[0], ""); //所在单位
                        }
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='580']/tr[4]/td[2]", "text");
                        if (results.Count > 0)
                        {
                            Res.OpenTime = reg.Replace(results[0], ""); //开户日期
                        }
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='580']/tr[4]/td[4]", "text");
                        if (results.Count > 0)
                        {
                            Res.Status = reg.Replace(results[0], "");//当前状态
                        }
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='580']/tr[5]/td[2]", "text");
                        if (results.Count > 0)
                        {
                            Res.SalaryBase = reg.Replace(results[0], "").ToDecimal(0); //月缴基数
                        }
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='580']/tr[5]/td[5]", "text");
                        if (results.Count > 0)
                        {
                            MatchCollection matchs = new Regex(@"[0-9.]{3,4}[1-9]*").Matches(reg.Replace(results[0], ""));
                            if (matchs.Count == 2)
                            {
                                Res.PersonalMonthPayRate = matchs[0].Value.ToDecimal(0); //个人缴费比率
                                Res.CompanyMonthPayRate = matchs[1].Value.ToDecimal(0); //公司缴费比率
                                if (Res.SalaryBase > 0)
                                {
                                    Res.PersonalMonthPayAmount = Res.SalaryBase * (Res.PersonalMonthPayRate * (decimal)0.01);//个人月缴额
                                    Res.CompanyMonthPayAmount = Res.SalaryBase * (Res.CompanyMonthPayRate * (decimal)0.01); //公司月缴额
                                }
                            }
                            else
                            {
                                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='580']/tr[5]/td[2]", "text");
                                if (results.Count > 0)
                                {
                                    Res.PersonalMonthPayAmount = Res.CompanyMonthPayAmount = reg.Replace(results[0], "").ToDecimal(0) / 2;//月缴额
                                }
                            }
                        }
                    }
                    #endregion
                    #region 第三步，查询缴费明细
                    //当前年度
                    Url = baseUrl + string.Format("zfbzgl/gjjmxcx/gjjmx_cx.jsp?sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd={4}", fundReq.Identitycard, zgxm.ToUrlDecode(Encoding.GetEncoding("gbk")), zgzh, dwbm, cxyd.ToUrlDecode(Encoding.GetEncoding("gbk")));
                    postdata = string.Format("sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd={4}", fundReq.Identitycard, "", zgzh, dwbm, "");
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "Post",
                        Postdata = postdata,
                        Encoding = Encoding.GetEncoding("GBK"),
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (Res.PersonalMonthPayRate > 0 && Res.CompanyMonthPayRate > 0)
                    {
                        totalRate = Res.PersonalMonthPayRate + Res.CompanyMonthPayRate;
                        perAccounting = (Res.PersonalMonthPayRate / totalRate);
                        comAccounting = (Res.CompanyMonthPayRate / totalRate);
                    }
                    else
                    {
                        totalRate = (payRate * 100) * 2;//16
                        perAccounting = comAccounting = (decimal)0.50;
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@cellspacing='1']/tr[position()>1]", "inner");
                    foreach (string item in results)
                    {
                        detail = new ProvidentFundDetail();
                        List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        if (tdRow.Count != 6)
                        {
                            continue;
                        }
                        detail.Description = tdRow[5];//描述
                        detail.PayTime = tdRow[0].ToDateTime();//缴费年月
                        if (tdRow[1].IndexOf("汇缴", System.StringComparison.Ordinal) > -1)
                        {
                            detail.ProvidentFundTime = new Regex(@"[^0-9;1-9]*").Replace(tdRow[5], "");//应属年月
                            detail.PersonalPayAmount = reg.Replace(tdRow[2], "").ToDecimal(0) * perAccounting;//个人缴费金额
                            detail.CompanyPayAmount = reg.Replace(tdRow[2], "").ToDecimal(0) * comAccounting;//企业缴费金额
                            detail.ProvidentFundBase = reg.Replace(tdRow[2], "").ToDecimal(0) / (totalRate * (decimal)0.01);//缴费基数
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                            PaymentMonths++;
                        }
                        else
                        {//（补缴，结息etc，数据不精确，只做参考用）
                            detail.PersonalPayAmount = reg.Replace(tdRow[1], "").ToDecimal(0) + reg.Replace(tdRow[2], "").ToDecimal(0);//个人缴费金额
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                        }
                        Res.ProvidentFundDetailList.Add(detail);
                    }
                    #endregion
                }
                   
                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                return Res;
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
