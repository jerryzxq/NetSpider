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
    public class daqing : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www1.dqgjj.gov.cn:7001/wscx_dq/";
        string fundCity = "hl_daqing";
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        ProvidentFundDetail detail = null;
        List<string> results = new List<string>();
        #endregion
        #region 私有变量
        decimal perAccounting = 0;//个人占比
        decimal comAccounting = 0;//公司占比
        decimal totalRate = 0;//总缴费比率
        decimal payRate = (decimal)0.08;
        int PaymentMonths = 0;
        Regex reg = new Regex(@"[\s;\&nbsp;\,;\%]");
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
            string Url = string.Empty;
            string postdata = string.Empty;
            string cxyd = string.Empty;
            string cxydmc = string.Empty;
            string dbname = string.Empty;
            string zgzh = string.Empty;
            string dwbm = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                //15位或18位身份证验证
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (regex.IsMatch(fundReq.Identitycard) == false)
                {
                    Res.StatusDescription = "输入的身份证号长度不对，或者号码不符合规定！15位号码应全为数字，18位号码末位可以为数字或X。";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Name))
                {
                    Res.StatusDescription = "请输入姓名";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Password))
                {
                    Res.StatusDescription = "密码不能为空!";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆

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
                Url = baseUrl + string.Format("zfbzgl/zfbzsq/login_hidden.jsp?password={0}&sfzh={1}&cxyd={2}&dbname={3}&zgxm={4}", fundReq.Password, fundReq.Identitycard, cxyd.ToUrlEncode(Encoding.GetEncoding("GBK")), dbname.ToUrlEncode(Encoding.GetEncoding("GBK")), fundReq.Name.ToUrlEncode(Encoding.GetEncoding("GBK")));
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='sfzh']", "value");
                if (results.Count > 0)
                {
                    if (string.IsNullOrWhiteSpace(results[0].Trim()))
                    {
                        Res.StatusDescription = "身份证号不能为空";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = baseUrl + "zfbzgl/zfbzsq/login.jsp";
                postdata = string.Format("cxydmc={0}&dbname={1}&sfzh={2}&zgxm={3}&password={4}&yes.x=26&yes.y=9", cxydmc.ToUrlEncode(Encoding.GetEncoding("GBK")), dbname.ToUrlEncode(Encoding.GetEncoding("GBK")), fundReq.Identitycard, fundReq.Name.ToUrlEncode(Encoding.GetEncoding("GBK")), fundReq.Password.ToUrlEncode(Encoding.GetEncoding("GBK")));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    Referer = baseUrl + "zfbzgl/zfbzsq/login.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = baseUrl + string.Format("zfbzgl/zfbzsq/main_menu.jsp?zgzh={0}&sfzh={1}&zgxm={2}&dwbm={3}&cxyd={4}&pass1={5}&dbname={6}", zgzh, fundReq.Identitycard, fundReq.Name.ToUrlEncode(Encoding.GetEncoding("GBK")), dwbm, cxyd.ToUrlEncode(Encoding.GetEncoding("GBK")), fundReq.Password, dbname.ToUrlEncode(Encoding.GetEncoding("GBK")));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("GBK"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                #endregion
                #region 第二步,获取基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='myTable']/tr[1]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Name = reg.Replace(results[0], ""); //姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='myTable']/tr[1]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.BankCardNo = reg.Replace(results[0], "");//银行账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='myTable']/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.IdentityCard = reg.Replace(results[0], ""); //身份账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='myTable']/tr[2]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = reg.Replace(results[0], "");//职工账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='myTable']/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyName = reg.Replace(results[0], ""); //所在单位
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='myTable']/tr[4]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.OpenTime = reg.Replace(results[0], ""); //开户日期
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='myTable']/tr[4]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.Status = reg.Replace(results[0], "");//当前状态
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='myTable']/tr[5]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.SalaryBase = reg.Replace(results[0], "").ToDecimal(0); //月缴基数
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='myTable']/tr[5]/td[5]", "text");
                if (results.Count > 0)
                {
                    MatchCollection matchs = new Regex(@"[0-9.]{3,4}[1-9]*").Matches(reg.Replace(results[0], ""));
                    if (matchs.Count == 2)
                    {
                        Res.PersonalMonthPayRate = matchs[0].Value.ToDecimal(0) * 0.01M; //个人缴费比率
                        Res.CompanyMonthPayRate = matchs[1].Value.ToDecimal(0) * 0.01M; //公司缴费比率
                        if (Res.SalaryBase > 0)
                        {
                            Res.PersonalMonthPayAmount = (Res.SalaryBase * Res.PersonalMonthPayRate).ToString("f2").ToDecimal(0);//个人月缴额
                            Res.CompanyMonthPayAmount = (Res.SalaryBase * Res.CompanyMonthPayRate).ToString("f2").ToDecimal(0); //公司月缴额
                        }
                    }
                    else
                    {
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='myTable']/tr[6]/td[2]", "text");
                        if (results.Count > 0)
                        {
                            Res.PersonalMonthPayAmount = Res.CompanyMonthPayAmount = (reg.Replace(results[0], "").ToDecimal(0) / 2).ToString("f2").ToDecimal(0);//月缴额
                        }
                    }
                }
                #endregion
                #region 第三步，查询缴费明细

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
                List<string> searchYear = new List<string>();
                //当前年度
                Url = baseUrl + "zfbzgl/gjjmxcx/gjjmx_cx.jsp";
                postdata = string.Format("zgxm={0}&sfzh={1}&zgzh={2}&dwbm={3}&cxyd={4}", fundReq.Name.ToUrlDecode(Encoding.GetEncoding("gbk")), fundReq.Identitycard, zgzh, dwbm, cxyd.ToUrlEncode(Encoding.GetEncoding("GBK")));
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@name='cxydone']/option", "text");
                if (results.Count > 0)
                {
                    searchYear = results;
                }

                Res = GetNowPageDetails(httpResult);
                //剩余至多4年
                int beginTime = searchYear.Count;
                int endTime = beginTime - 4;
                for (int i = beginTime; i >= endTime; i--)
                {
                    //string test = searchYear[i - 1];
                    if (searchYear[i - 1].Contains("当前年度"))
                    {
                        continue;
                    }
                    Url = baseUrl + string.Format("zfbzgl/gjjmxcx/gjjmx_cx2.jsp?cxydone={0}&cxydtwo={1}&cxyd={2}&zgzh={3}&sfzh={4}&zgxm={5}&dwbm={6}", searchYear[i - 1], searchYear[i - 1], cxyd.ToUrlEncode(Encoding.GetEncoding("GBK")), zgzh, fundReq.Identitycard, fundReq.Name.ToUrlEncode(Encoding.GetEncoding("GBK")), dwbm);
                    postdata = string.Format("cxydone={0}&cxydtwo={1}&cxyd={2}&zgzh={3}&sfzh={4}&zgxm={5}&dwbm={6}", searchYear[i - 1], searchYear[i - 1], "", zgzh, fundReq.Identitycard, "", dwbm);
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
                    Res = GetNowPageDetails(httpResult);
                }
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
        /// <summary>
        /// 处理缴费明细
        /// </summary>
        /// <param name="httpResult">http返回类</param>
        /// <returns>Res</returns>
        private Entity.Service.ProvidentFundQueryRes GetNowPageDetails(HttpResult httpResult)
        {
            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='myTable']/tr[position()>1]", "inner");
            foreach (string item in results)
            {
                detail = new ProvidentFundDetail();
                List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                if (tdRow.Count != 5)
                {
                    continue;
                }
                detail.Description = tdRow[1];//描述
                detail.PayTime = tdRow[0].ToDateTime();//缴费年月
                if (tdRow[1].IndexOf("汇缴", System.StringComparison.Ordinal) > -1)
                {
                    detail.ProvidentFundTime = new Regex(@"[^0-9;1-9]*").Replace(tdRow[1], "");//应属年月
                    detail.PersonalPayAmount = (reg.Replace(tdRow[3], "").ToDecimal(0) * perAccounting).ToString("f2").ToDecimal(0);//个人缴费金额
                    detail.CompanyPayAmount = (reg.Replace(tdRow[3], "").ToDecimal(0) * comAccounting).ToString("f2").ToDecimal(0);//企业缴费金额
                    detail.ProvidentFundBase = (reg.Replace(tdRow[3], "").ToDecimal(0) / totalRate).ToString("f2").ToDecimal(0);//缴费基数
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                    PaymentMonths++;
                }
                else if (tdRow[1].IndexOf("提取", System.StringComparison.Ordinal) > -1 || tdRow[1].IndexOf("支取", System.StringComparison.Ordinal) > -1)
                {
                    detail.PersonalPayAmount = (reg.Replace(tdRow[3], "").ToDecimal(0) * perAccounting).ToString("f2").ToDecimal(0);//个人缴费金额
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;//缴费标志
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;//缴费类型
                    Res.Description = "有支取，请人工校验。";
                }
                else
                {//（补缴，结息etc，数据不精确，只做参考用）
                    detail.PersonalPayAmount = Math.Abs(reg.Replace(tdRow[2], "").ToDecimal(0) - reg.Replace(tdRow[3], "").ToDecimal(0));//个人缴费金额
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                }
                Res.ProvidentFundDetailList.Add(detail);
            }
            return Res;
        }
    }
}
