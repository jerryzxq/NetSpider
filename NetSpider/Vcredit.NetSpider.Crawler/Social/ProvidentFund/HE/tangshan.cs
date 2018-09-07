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
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HE
{
    public class tangshan : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.tsgjj.com/";
        string fundCity = "he_tangshan";
        int PaymentMonths = 0;
        List<string> results = new List<string>();
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        #endregion
        #region 私有变量
        decimal payRate = (decimal)0.08;
        #endregion
        /// <summary>
        /// 解析保存验证码
        /// </summary>
        /// <returns></returns>
        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "validatecode.asp";
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
                //添加缓存
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return Res;
        }
        /// <summary>
        /// 抓取页面数据
        /// </summary>
        /// <param name="fundReq"></param>
        /// <returns></returns>
        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            ProvidentFundDetail detail = null;
            ProvidentFundLoanRes Res_load = new ProvidentFundLoanRes();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (string.IsNullOrEmpty(fundReq.Username) || string.IsNullOrEmpty(fundReq.Password))
                {
                    Res.StatusDescription = "联名卡号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆
                Url = baseUrl + "found_login_do.asp";
                postdata = string.Format("atype=person&uid={0}&pwd={1}&cert={2}", fundReq.Username, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    Encoding = Encoding.GetEncoding("utf-8"),
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
                if (httpResult.Html.Contains("请注意正确输入验证码"))
                {
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    Res.StatusDescription = "请注意正确输入验证码";
                    return Res;
                }
                if (httpResult.Html.Contains("此身份证号无对应账户信息"))
                {
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    Res.StatusDescription = "此身份证号无对应账户信息，请确认！";
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//tr[@height='678']/td[@width='970']", "text");
                if (results.Count > 0)
                {
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    Res.StatusDescription = "输入的账号或密码有误,请核对后再输";
                    return Res;
                }
                //httpResult.Html.Contains("账号或密码有误，请检查") ||
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region 第二步,获取个人基本信息
                //Url = baseUrl + "found_person.aspx";获取该界面数据（因上个界面已包含该界面，故该步骤可以省略了）。
                decimal monthPay = decimal.Zero;//月缴额
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='mainwords']/tr[1]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.Name = results[0].Trim();//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='mainwords']/tr[2]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0].Trim();//单位名称
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='mainwords']/tr[3]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0].Trim();//单位账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='mainwords']/tr[4]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0].Trim();//职工账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='mainwords']/tr[5]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.BankCardNo = results[0].Trim();//磁卡卡号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='mainwords']/tr[6]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = results[0].ToTrim("%").ToDecimal(0) * 0.01M;//单位缴存比例

                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='mainwords']/tr[7]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = results[0].ToTrim("%").ToDecimal(0) * 0.01M;//个人缴存比例
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='mainwords']/tr[9]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    monthPay = results[0].Trim().ToDecimal(0);//月缴额
                }
                if (Res.CompanyMonthPayRate != 0 & Res.PersonalMonthPayRate != 0 & monthPay != 0)
                {
                    if (Res.CompanyMonthPayRate > 0 && Res.PersonalMonthPayRate > 0)
                    {
                        decimal temp = Res.CompanyMonthPayRate + Res.PersonalMonthPayRate;
                        Res.CompanyMonthPayAmount = (monthPay * (Res.CompanyMonthPayRate / temp)).ToString("#0.00").ToDecimal(0);//单位月缴额
                        Res.PersonalMonthPayAmount = (monthPay * (Res.PersonalMonthPayRate / temp)).ToString("f2").ToDecimal(0);//个人月缴额
                        Res.SalaryBase = (Res.PersonalMonthPayAmount / (Res.PersonalMonthPayRate)).ToString("f2").ToDecimal(0);//基本薪资
                    }
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='mainwords']/tr[8]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = Convert.ToDateTime(results[0]).ToString("yyyyMM");//缴款截止月份new Regex("[^0-9;1-9]*").Replace(results[0], "")
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='mainwords']/tr[10]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);//余额
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='mainwords']/tr[11]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.Status = results[0].Trim();//账户状态
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='mainwords']/tr[12]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0].Trim();//身份证号
                }
                #endregion
                #region 第三步,获取缴费明细
                // http://www.tsgjj.com/found_person.aspx
                Url = baseUrl + "found_person.aspx";
                postdata = string.Format("mx={0}", "all");//option 的value为all,查询全部明细
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postdata,
                    Method = "Post",
                    Encoding = Encoding.GetEncoding("utf-8"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.Html.Contains("登录超时"))
                {
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    Res.StatusDescription = "登录超时,请重试";
                    return Res;
                }
                int totalPageNum = 0;//明细总页码
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form/table/tr/td[2]/table/tr[2]/td[2]/p[3]", "innertext");
                Regex regex;
                MatchCollection matchs;
                if (results.Count > 0)
                {
                    regex = new Regex("[0-9]{1,2}");
                    matchs = regex.Matches(results[0]);
                    if (matchs.Count == 2)
                    {
                        totalPageNum = matchs[1].Value.ToInt(0);
                    }
                    //totalPageNum = int.Parse(results[0].Trim().Substring(results[0].Trim().IndexOf("共", StringComparison.Ordinal) + 1, 1));
                    if (totalPageNum > 0)
                    {
                        //首先获取第一页数据
                        Res = GetNowPageDetails(httpResult, 1 - 1);
                        //页码大于1时,获取剩余页码数据
                        if (totalPageNum > 1)
                        {
                            for (int i = 1; i < totalPageNum; i++)
                            {
                                Url = baseUrl + "found_person.aspx?sig=sig&pageact=down&pagect=" + i + "";//(从0开始)
                                httpItem = new HttpItem()
                                {
                                    URL = Url,
                                    Method = "GET",
                                    Encoding = Encoding.GetEncoding("utf-8"),
                                    CookieCollection = cookies,
                                    ResultCookieType = ResultCookieType.CookieCollection
                                };
                                httpResult = httpHelper.GetHtml(httpItem);
                                if (httpResult.Html.Contains("登录超时"))
                                {
                                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                                    Res.StatusDescription = "登录超时,请重试";
                                    return Res;
                                }
                                Res = GetNowPageDetails(httpResult, i);
                            }
                        }
                    }
                }
                #endregion
                #region 第四步,贷款基本信息

                regex = new Regex(@"[0-9][0-9,.-]+");
                Url = baseUrl + "found_daikuan_login_do.asp";
                postdata = string.Format("uid={0}&pwd={1}&cert={2}", fundReq.Username, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    Encoding = Encoding.GetEncoding("utf-8"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='mainwords']//div[@align='left']");
                if (results.Count == 0 || (string.IsNullOrEmpty(results[0])))
                {
                    Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                    return Res;
                }
                Res_load.Con_No = results[0];//合同号
                Res_load.Name = results[1];//姓名
                Res_load.IdentityCard = results[2];
                Res_load.Couple_Name = results[3];//借款人配偶
                Res_load.Couple_IdentifyCard = results[4];//配偶身份证号码
                Res_load.Address = results[13];//抵押物地址
                Res_load.Loan_Credit = results[14].ToTrim("元").ToDecimal(0);//公积金贷款金额
                matchs = regex.Matches(results[15]);
                if (matchs.Count == 4)
                {
                    Res_load.Loan_Start_Date = DateTime.ParseExact(matchs[0].ToString(), "yyyy-M", null).ToString(Consts.DateFormatString7);
                    Res_load.Loan_End_Date = DateTime.ParseExact(matchs[1].ToString(), "yyyy-M", null).ToString(Consts.DateFormatString7);
                    Res_load.Period_Total = matchs[2].ToString();//还款期数(总期数)
                }
                matchs = regex.Matches(results[16]);
                if (matchs.Count == 4)
                {

                    Res_load.Principal_Payed = matchs[1].ToString().ToDecimal(0);//已还本金
                    Res_load.Interest_Payed = matchs[2].ToString().ToDecimal(0);//已还利息
                    Res_load.Interest_Penalty = matchs[3].ToString().ToDecimal(0);//罚息
                }
                Res_load.Loan_Balance = results[17].ToTrim("元").ToDecimal(0);//贷款余额

                #endregion
                #region 第五步,贷款明细
                //首页
                Url = baseUrl + "found_daikuan_mx.aspx";
                postdata = string.Format("select=found_daikuan_mx.aspx&ht={0}", Res_load.Con_No);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='mainwords']/tr[position()>1]");
                int remainingPage = CommonFun.GetMidStr(httpResult.Html, "共", "页").ToInt(0) - 1;
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //剩余页
                for (int i = 1; i <= remainingPage; i++)
                {
                    Url = baseUrl + "found_daikuan_mx.aspx?sig=sig&pageact=down&pagect=" + i;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Encoding = Encoding.UTF8,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='mainwords']/tr[position()>1]"));
                }
                foreach (string s in results)
                {
                    List<string> tdRow = HtmlParser.GetResultFromParser(s,"//td");
                    if (tdRow.Count != 11) continue;
                    ProvidentFundLoanDetail loanDetail = new ProvidentFundLoanDetail();
                    loanDetail.Record_Period = tdRow[0];
                    loanDetail.Record_Month = Convert.ToDateTime(tdRow[5]).ToString(Consts.DateFormatString7);
                    loanDetail.Record_Date = tdRow[5];
                    loanDetail.Principal = tdRow[7].ToDecimal(0);
                    loanDetail.Interest = tdRow[8].ToDecimal(0);
                    loanDetail.Interest_Penalty = tdRow[9].ToDecimal(0);
                    loanDetail.Description = tdRow[10];
                    Res_load.ProvidentFundLoanDetailList.Add(loanDetail);
                }
                #endregion
                Res.ProvidentFundLoanRes = Res_load;
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
        /// <summary>
        /// 获取当前页码缴费明细
        /// </summary>
        /// <param name="httpResult">Http返回参数类</param>
        /// <param name="pageNum">页码</param>
        /// <returns></returns>
        private Entity.Service.ProvidentFundQueryRes GetNowPageDetails(HttpResult httpResult, int pageNum)
        {
            if (httpResult != null && pageNum >= 0)
            {
                decimal perAccounting;//个人占比
                decimal comAccounting;//公司占比
                decimal totalRate;//总缴费比率
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
                List<string> results = HtmlParser.GetResultFromParser(httpResult.Html, "//form/table/tr/td[2]/table/tr[2]/td[2]/table[2]/tr[position()>1]", "inner");
                if (results.Count == 0)
                {
                    Res.StatusDescription = "暂无账户明细";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                }
                else
                {
                    foreach (string item in results)
                    {
                        ProvidentFundDetail detail = new ProvidentFundDetail();
                        List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        if (tdRow.Count != 4)
                        {
                            continue;
                        }
                        detail.Description = tdRow[3];//描述
                        detail.PayTime = tdRow[0].ToDateTime();//缴费年月
                        detail.ProvidentFundTime = Convert.ToDateTime(tdRow[0]).ToString("yyyyMM");
                        if (tdRow[3].IndexOf("汇缴", System.StringComparison.Ordinal) > -1)
                        {
                            detail.PersonalPayAmount = tdRow[1].ToDecimal(0) * perAccounting.ToString("f2").ToDecimal(0);//个人缴费金额
                            detail.CompanyPayAmount = tdRow[1].ToDecimal(0) * comAccounting.ToString("f2").ToDecimal(0);//企业缴费金额
                            detail.ProvidentFundBase = (tdRow[1].ToDecimal(0) / (totalRate)).ToString("f2").ToDecimal(0);//缴费基数
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                            PaymentMonths++;
                        }
                        else if (tdRow[3].IndexOf("取", System.StringComparison.Ordinal) > -1)
                        {
                            detail.PersonalPayAmount = tdRow[1].ToDecimal(0) * perAccounting;//个人缴费金额
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;//缴费类型
                        }
                        else
                        {//（补缴，结息etc，数据不精确，只做参考用）
                            detail.PersonalPayAmount = tdRow[1].ToDecimal(0);//个人缴费金额
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                        }
                        Res.ProvidentFundDetailList.Add(detail);
                    }
                }
            }
            return Res;
        }
    }
}
