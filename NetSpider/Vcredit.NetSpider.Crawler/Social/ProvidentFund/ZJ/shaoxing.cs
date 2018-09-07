using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common.Ext;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.ZJ
{
    public class shaoxing : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://115.239.233.150/wscx_sxx/";
        string fundCity = "zj_shaoxing";
        #endregion
        #region 私有变量
        string zgzh = string.Empty;//职工账号
        string sfzh = string.Empty;//身份证号()
        string zgxm = string.Empty;//职工姓名()
        string dwbm = string.Empty;//单位编码
        string zgzt = string.Empty;//职工状态
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "zfbzgl/zfbzsq/login.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Encoding = Encoding.GetEncoding("GBK"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string cxyd = string.Empty;//当前年度
                string cxydmc = string.Empty;//当前年度
                string dbname = string.Empty;//gjjmx9
                //string dlfs = string.Empty;//01身份证，02公积金号
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='cxyd']", "value", true);
                if (results.Count > 0)
                {
                    cxyd = results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='cxydmc']", "value", true);
                if (results.Count > 0)
                {
                    cxydmc = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dbname']", "value", true);
                if (results.Count > 0)
                {
                    dbname = results[0];
                }
                Res.StatusDescription = fundCity +ServiceConsts.ProvidentFund_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                //添加缓存
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("cxyd", cxyd);
                dics.Add("cxydmc", cxydmc);
                dics.Add("dbname", dbname);
                CacheHelper.SetCache(token, dics);
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
            ProvidentFundLoanRes ResLoad = new ProvidentFundLoanRes();
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            ProvidentFundDetail detail = null;
            int PaymentMonths = 0;
            Regex reg = new Regex(@"[\&nbsp;\s;\,;\u5143]*");
            try
            {
                string cxyd = string.Empty;//当前年度
                string cxydmc = string.Empty;//当前年度
                string dbname = string.Empty;//gjjmx9
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(fundReq.Token);
                    cxyd = dics["cxyd"].ToString();
                    cxydmc = dics["cxydmc"].ToString();
                    dbname = dics["dbname"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (string.IsNullOrEmpty(fundReq.Name) || string.IsNullOrEmpty(fundReq.Password) || string.IsNullOrEmpty(fundReq.Identitycard))
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //裘海钟 33062319750322511X 111111
                #region 第一步，登陆（身份证号登陆方式）
                Url = baseUrl + "zfbzgl/zfbzsq/login_hidden.jsp?dlfs=0";//dlfs=0（01:身份证号登陆，02:公积金号登陆）
                postdata = string.Format("cxyd={0}&cxydmc={1}&dbname={2}&xm={3}&dlfs={4}&sfzh={5}&password={6}&yes.x=23&yes.y=8", cxyd.ToUrlEncode(Encoding.GetEncoding("gbk")), cxydmc.ToUrlEncode(Encoding.GetEncoding("gbk")), dbname.ToUrlEncode(), fundReq.Name.ToUrlEncode(Encoding.GetEncoding("gbk")), "01", fundReq.Identitycard.ToUrlEncode(), fundReq.Password.ToUrlEncode());
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
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = httpResult.StatusDescription;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string errorInfo = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\");");
                if (!errorInfo.IsEmpty())
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgzh']", "value", true);
                if (results.Count > 0)
                {
                    zgzh = results[0];
                }
                if (string.IsNullOrEmpty(zgzh))
                {
                    Res.StatusDescription = "登陆失败请核对账号后重试";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.ProvidentFundNo = zgzh;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dwbm']", "value", true);
                if (results.Count > 0)
                {
                    Res.CompanyNo = dwbm = results[0];
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region 第二步   获取公积金号
                ////http://115.239.233.150/wscx_sxx/zfbzgl/zfbzsq/sfzh_cf.jsp
                //Url = baseUrl + "zfbzgl/zfbzsq/sfzh_cf.jsp";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "GET",
                //    Referer = baseUrl + string.Format("zfbzgl/zfbzsq/sfzh_cf_hidden.jsp?zh={0}", zgzh),
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};

                //httpResult = httpHelper.GetHtml(httpItem);
                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    Res.StatusDescription = httpResult.StatusDescription;
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                ////results = CommonFun.GetMidStr(httpResult.Html, "", "");
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td/table[5]/tr[position()>1]", "");
                //foreach (var item in results)
                //{
                //    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "inner", true);
                //    if (tdRow[3] == "正常")
                //    {
                //        zgzh = CommonFun.GetMidStr(tdRow[0], "sfzh_cf('", "')");
                //        break;
                //    }
                //    else
                //    {
                //        zgzh = tdRow[0];
                //    }
                //}
                #endregion
                #region  第三步

                //Url = baseUrl + string.Format("zfbzgl/zfbzsq/sfzh_cf_hidden.jsp?zh={0}",zgzh);
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "get",
                //    Encoding = Encoding.GetEncoding("GBK"),
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    Res.StatusDescription = httpResult.StatusDescription;
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}

                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgzh']", "value", true);
                //if (results.Count > 0)
                //{
                //    zgzh = results[0];
                //}
                //else
                //{

                //    Res.StatusDescription = "登陆失败请核对账号后重试";
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dwbm']", "value", true);
                //if (results.Count > 0)
                //{
                //    dwbm = results[0];
                //}
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='sfzh']", "value", true);
                //if (results.Count > 0)
                //{
                //    sfzh = results[0];
                //}
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgxm']", "value", true);
                //if (results.Count > 0)
                //{
                //    zgxm = results[0];
                //}
                #endregion
                #region 第四步，公积金基本信息
                Url =
                    baseUrl + string.Format("zfbzgl/gjjxxcx/gjjxx_cx.jsp?zgzh={0}&sfzh={1}&zgxm={2}&dwbm={3}&cxyd={4}&dbname={5}", zgzh, sfzh, zgxm.ToUrlEncode(Encoding.GetEncoding("gbk")), dwbm, cxyd.ToUrlEncode(Encoding.GetEncoding("gbk")), dbname);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    Referer = baseUrl + string.Format("zfbzgl/zfbzsq/sfzh_cf_hidden.jsp?zh={0}", zgzh),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[1]/td[2]/font", "innertext");
                if (results.Count > 0)
                {
                    Res.Name = Regex.Replace(results[0], @"[\&nbsp;\s;\,;]*", "");//职工姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[1]/td[4]/font", "innertext");
                if (results.Count > 0)
                {
                    Res.BankCardNo = reg.Replace(results[0], "");//银行账号
                    // Res.ProvidentFundNo = reg.Replace(results[0], "");//公积金卡号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[2]/td[2]/font", "innertext");
                if (results.Count > 0)
                {
                    Res.IdentityCard = reg.Replace(results[0], "");//身份证号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[2]/td[4]/font", "innertext");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = reg.Replace(results[0], "");//职工账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[3]/td[2]/font", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyName = Regex.Replace(results[0], @"[\&nbsp;\s;\,;]*", "");//所在单位
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[4]/td[2]/font", "innertext");
                if (results.Count > 0)
                {
                    Res.OpenTime = reg.Replace(results[0], "");//开户日期
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[4]/td[4]/font", "innertext");
                if (results.Count > 0)
                {
                    Res.Status = reg.Replace(results[0], "");//当前状态
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[5]/td[5]/font", "innertext");
                if (results.Count > 0)
                {
                    //个人/企业
                    MatchCollection matchs = Regex.Matches(results[0], @"[0-9.]{4,5}[1-9]*");
                    if (matchs.Count == 2)
                    {
                        Res.PersonalMonthPayRate = matchs[0].ToString().ToDecimal(0) * 0.01M;//个人缴费比率
                        Res.CompanyMonthPayRate = matchs[1].ToString().ToDecimal(0) * 0.01M;//公司缴费比率
                    }
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[6]/td[2]/font", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = reg.Replace(results[0], "").ToDecimal(0);//单位月缴额
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[7]/td[2]/font", "innertext");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = reg.Replace(results[0], "").ToDecimal(0);//个人月缴额
                    decimal perPayRate = Res.PersonalMonthPayRate > 0 ? Res.PersonalMonthPayRate : payRate;
                    Res.SalaryBase = Math.Round(Res.PersonalMonthPayAmount / perPayRate, 2);//缴费基数
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[8]/td[4]/font", "innertext");
                if (results.Count > 0)
                {
                    Res.TotalAmount = reg.Replace(results[0], "").ToDecimal(0);//公积金余额
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[9]/td[2]/font", "innertext");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = reg.Replace(results[0], "");//缴存截止年月
                }
                #endregion
                #region 第五步，公积金详细查询
                //
                reg = new Regex(@"[\&nbsp;\s;\,;\u4e00-\u9fa5]*");
                decimal perAccounting = 0;//个人占比
                decimal comAccounting = 0;//公司占比
                decimal totalRate = 0;//总缴费比率
                if (Res.PersonalMonthPayRate > 0 && Res.CompanyMonthPayRate > 0)
                {
                    totalRate = Res.PersonalMonthPayRate + Res.CompanyMonthPayRate;
                    perAccounting = (Res.PersonalMonthPayRate / totalRate);
                    comAccounting = (Res.CompanyMonthPayRate / totalRate);
                }
                else
                {
                    totalRate = payRate * 2;//16
                    perAccounting = comAccounting = (decimal)0.50;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                int yss = 1;//当前页数
                int totalPages = 1;//总页数
                results = new List<string>();
                Url = baseUrl + "zfbzgl/gjjmxcx/gjjmx_cx.jsp";
                do
                {
                    postdata = string.Format("sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd={4}&zgzt={5}&cxydone={4}&cxydtwo=&totalpages{6}=&yss={7}", sfzh, zgxm.ToUrlEncode(Encoding.GetEncoding("GBK")), zgzh, dwbm, cxyd.ToUrlEncode(Encoding.GetEncoding("GBK")), zgzt.ToUrlEncode(Encoding.GetEncoding("GBK")), totalPages, yss);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "Post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (yss == 1)
                    {
                        var pages = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='totalpages']", "value");
                        if (pages.Count > 0)
                        {
                            totalPages = pages[0].ToInt(0);
                        }
                    }
                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[position()>1]", "inner"));
                    yss++;
                } while (yss <= totalPages);

                foreach (var item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow.Count != 5)
                    {
                        continue;
                    }
                    detail = new ProvidentFundDetail();
                    detail.PayTime = tdRow[0].ToDateTime();//缴费年月
                    detail.Description = tdRow[1];
                    if (tdRow[1].IndexOf("汇缴", System.StringComparison.Ordinal) > -1)
                    {
                        detail.PersonalPayAmount = reg.Replace(tdRow[3], "").ToDecimal(0) * perAccounting;//个人月缴额
                        detail.CompanyPayAmount = reg.Replace(tdRow[3], "").ToDecimal(0) * comAccounting;//公司月缴额
                        detail.ProvidentFundBase = Math.Round(reg.Replace(tdRow[3], "").ToDecimal(0) / totalRate, 2);//缴费基数
                        detail.ProvidentFundTime = reg.Replace(tdRow[1], "");//应属年月
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        PaymentMonths++;
                    }
                    else
                    {//（补缴，结息etc,数据不精确，只做参考用）
                        detail.PersonalPayAmount = Math.Abs(reg.Replace(tdRow[3], "").ToDecimal(0) - reg.Replace(tdRow[2], "").ToDecimal(0));//个人缴费
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                //
                #endregion
                #region 贷款基本信息

                Url = baseUrl + "zfbzgl/dkxxcx/dkxx_cx.jsp";
                postdata = string.Format("sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd=%B5%B1%C7%B0%C4%EA%B6%C8&zgzt={4}", Res.IdentityCard, Res.Name.ToUrlEncode(Encoding.GetEncoding("gbk")), Res.ProvidentFundNo, Res.CompanyNo, Res.Status.ToUrlEncode(Encoding.GetEncoding("gbk")));
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//legend[@id='title']", "text");
                if (results.Count > 0)
                {
                    if (!string.IsNullOrEmpty(results[0]))
                    {
                        Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                        Res.StatusCode = ServiceConsts.StatusCode_success;
                        return Res;
                    }
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", "").ToTrim("元"), "//table[@class='1']/tr/td", "text");
                if (results.Count == 32)
                {
                    ResLoad.Con_No = results[1];//合同号
                    ResLoad.Name = results[3];
                    ResLoad.Loan_Credit = results[5].ToDecimal(0);//贷款金额
                    ResLoad.Period_Total = (results[7].ToTrim("年").ToInt(0) * 12).ToString();//贷款期数
                    ResLoad.Principal_Payed = results[9].ToDecimal(0);//已还本金
                    ResLoad.Interest_Payed = results[11].ToDecimal(0);//已还利息
                    ResLoad.Loan_Balance = results[13].ToDecimal(0);//贷款余额
                    ResLoad.House_Purchase_Type = results[15];//购房类型
                    ResLoad.Loan_Start_Date = results[17];//放款日期
                    ResLoad.House_Purchase_Address = results[19];//房贷地址
                    ResLoad.Loan_Rate = (results[21].ToTrim("%").ToDecimal(0) * 0.01M).ToString(CultureInfo.InvariantCulture);//贷款利率
                    ResLoad.Bank_Delegate = results[23];//委托银行
                    ResLoad.Repay_Type = results[25];//还款方式
                    ResLoad.Description = "担保方式:" + results[27];//担保方式
                    ResLoad.Status = results[29];//还款状态
                }

                #endregion
                #region 贷款明细
                int intPage = 1;//当前页数
                int intPageCount = 1;//总页数
                int intRowCount = 11;//明细条数
                results = new List<string>();
                Url = baseUrl + "zfbzgl/dkhkcx/dkhk_cx.jsp";
                do
                {
                    postdata = string.Format("intPage={3}&startRow={4}&endRow={5}&intRowCount={6}&intPageSize=12&sfzh={0}&zgzh={1}&dwbm={2}", Res.IdentityCard, zgzh, dwbm, intPage, (intPage - 1) * 12, (11 + (intPage - 1) * 12) > intRowCount ? intRowCount : (11 + (intPage - 1) * 12), intRowCount);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "Post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (intPage == 1)
                    {
                        var pages = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='intPageCount']", "value");
                        if (pages.Count > 0)
                        {
                            intPageCount = pages[0].ToInt(0);
                        }
                        intRowCount = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='intRowCount']", "value")[0].ToInt(0);
                    }
                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[position()>1]", "", true));
                    intPage++;
                } while (intPage <= intPageCount);
                foreach (string s in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(s, "//td", "text");
                    if (tdRow.Count < 7) continue;
                    ProvidentFundLoanDetail loanDetail = new ProvidentFundLoanDetail();
                    loanDetail.Record_Date = tdRow[0];//记账日期
                    loanDetail.Record_Month = tdRow[1];//还款年月
                    loanDetail.Principal = tdRow[2].ToDecimal(0);//还款本金
                    loanDetail.Interest = tdRow[3].ToDecimal(0);//还款利息
                    loanDetail.Base = tdRow[4].ToDecimal(0);//还款本息
                    loanDetail.Balance = tdRow[6].ToDecimal(0);//贷款余额
                    ResLoad.ProvidentFundLoanDetailList.Add(loanDetail);
                }
                #endregion
                Res.ProvidentFundLoanRes = ResLoad;
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
