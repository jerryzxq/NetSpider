using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.CQ
{
    public class chongqing : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.cqgjj.cn/";
        string fundCity = "cq_chongqing";
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
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://www.cqgjj.cn/Member/UserLogin.aspx?type=gr";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                if (httpResult.Html.Contains("function JumpSelf()"))
                {
                    string JumpSelf = CommonFun.GetMidStr(httpResult.Html, "self.location=\"/", "\";}");
                    Url = "http://www.cqgjj.cn/" + JumpSelf;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                    Url = "http://www.cqgjj.cn/Member/UserLogin.aspx?type=gr";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                }

                string VIEWSTATE = string.Empty;
                string EVENTVALIDATION = string.Empty;

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value", true);
                if (results.Count > 0)
                {
                    VIEWSTATE = results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value", true);
                if (results.Count > 0)
                {
                    EVENTVALIDATION = results[0];
                }

                Url = "http://www.cqgjj.cn/Code.aspx";
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
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;

                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;


                //添加缓存
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("VIEWSTATE", VIEWSTATE);
                dics.Add("EVENTVALIDATION", EVENTVALIDATION);
                SpiderCacheHelper.SetCache(token, dics);
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
                string VIEWSTATE = string.Empty;
                string EVENTVALIDATION = string.Empty;
                //获取缓存
                if (SpiderCacheHelper.GetCache(fundReq.Token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)SpiderCacheHelper.GetCache(fundReq.Token);
                    VIEWSTATE = dics["VIEWSTATE"].ToString();
                    EVENTVALIDATION = dics["EVENTVALIDATION"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    SpiderCacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (string.IsNullOrWhiteSpace(fundReq.Username))
                {
                    Res.StatusDescription = "请输入用户名！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Password))
                {
                    Res.StatusDescription = "请输入密码！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Vercode))
                {
                    Res.StatusDescription = "请输入验证码！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录系统
                Url = "http://www.cqgjj.cn/Member/UserLogin.aspx?type=null";
            http://www.cqgjj.cn/Member/UserLogin.aspx?type=gr
                postdata = String.Format("__VIEWSTATE={3}&__EVENTVALIDATION={4}&txt_loginname={0}&txt_pwd={1}&txt_code={2}&but_send=", fundReq.Username, fundReq.Password, fundReq.Vercode, VIEWSTATE.ToUrlEncode(), EVENTVALIDATION.ToUrlEncode());
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
                    Res.StatusDescription = ServiceConsts.ProvidentFund_InitFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='error']", "text", true);//|| httpResult.Html.IndexOf("公积金用户登录成功") == -1
                if (results.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(results[0]))
                    {
                        Res.StatusDescription = results[0];
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                #endregion

                #region 第二步，查询个人基本信息

                Url = "http://www.cqgjj.cn/Member/gr/gjjyecx.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //当前状态
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_ctl0_ContentPlaceHolder1_lb_dwzcs']", "text", true);
                if (results.Count > 0)
                {
                    Res.Status = results[0];
                }
                else
                {
                    Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //多账户时选择正常状态账号查询
                if (Res.Status != "正常")
                {
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@id='_ctl0_ContentPlaceHolder1_DropDownList1']/option", "value");
                    Url = "http://www.cqgjj.cn/Member/gr/gjjyecx.aspx";
                    foreach (string item in results)
                    {
                        if (item == "0") continue;//排除第一个账户
                        VIEWSTATE = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value", true)[0];
                        EVENTVALIDATION = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value", true)[0];
                        postdata = String.Format("__VIEWSTATE={0}&__EVENTVALIDATION={1}&_ctl0%3AContentPlaceHolder1%3ADropDownList1={2}&_ctl0%3AContentPlaceHolder1%3AButton1=%E6%9F%A5+%E8%AF%A2", VIEWSTATE.ToUrlEncode(), EVENTVALIDATION.ToUrlEncode(), item);
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "post",
                            Postdata = postdata,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        //当前状态
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_ctl0_ContentPlaceHolder1_lb_dwzcs']", "text", true);
                        if (results.Count > 0)
                        {
                            Res.Status = results[0];
                        }
                        if (Res.Status == "正常") break;
                    }
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //姓名
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_ctl0_ContentPlaceHolder1_lb_name']", "text", true);
                if (results.Count > 0)
                {
                    Res.Name = results[0].ToTrim();
                }
                //身份证号码：
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_ctl0_ContentPlaceHolder1_Label1']", "text", true);
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0].ToTrim();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_ctl0_ContentPlaceHolder1_lb_khsj']", "text", true);
                if (results.Count > 0)
                {
                    Res.OpenTime = results[0].ToTrim();
                }
                //单位名称：
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_ctl0_ContentPlaceHolder1_lb_dwmc']", "text", true);
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                //个人月缴交额
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_ctl0_ContentPlaceHolder1_lb_grjje']", "text", true);
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].ToDecimal(0);
                }
                //单位月缴交额
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_ctl0_ContentPlaceHolder1_lb_dwyje']", "text", true);
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = results[0].ToDecimal(0);
                }
                //个人公积金帐号：
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_ctl0_ContentPlaceHolder1_lb_grjjjzh']", "text", true);
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0].Trim();
                }
                //当前余额
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_ctl0_ContentPlaceHolder1_lb_dqye']", "text", true);
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);
                }
                Res.SalaryBase = (Res.PersonalMonthPayAmount / payRate).ToString("f2").ToDecimal(0);
                #endregion

                #region 第三步，公积金缴费明细

                Url = "http://www.cqgjj.cn/Member/gr/gjjmxcx.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                VIEWSTATE = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value", true)[0];
                EVENTVALIDATION = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value", true)[0];
                postdata = String.Format("__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE={0}&__EVENTVALIDATION={1}&_ctl0%3AContentPlaceHolder1%3ADropDownList1={2}&_ctl0%3AContentPlaceHolder1%3AButton1=%E6%9F%A5+%E8%AF%A2&_ctl0%3AContentPlaceHolder1%3APageNavigator1%3AtxtNewPageIndex=1", VIEWSTATE.ToUrlEncode(), EVENTVALIDATION.ToUrlEncode(), Res.ProvidentFundNo);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='listinfo']/tbody/tr", "", true);
                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                    if (tdRow.Count != 5)
                    {
                        continue;
                    }

                    detail = new ProvidentFundDetail();
                    detail.Description = tdRow[1];//描述
                    detail.PayTime = tdRow[0].ToDateTime();
                    detail.PersonalPayAmount = tdRow[2].Replace("-", "").ToDecimal(0);//个人
                    detail.CompanyPayAmount = tdRow[3].Replace("-", "").ToDecimal(0);//公司
                    if (tdRow[1].IndexOf("汇缴") != -1)
                    {
                        detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[1], "汇缴[", "]").Replace("-", "");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.ProvidentFundBase = (detail.PersonalPayAmount / payRate).ToString("f2").ToDecimal(0);//缴费基数
                        PaymentMonths++;
                    }
                    else if (tdRow[1].IndexOf("取") > -1)
                    {
                        detail.PersonalPayAmount = detail.CompanyPayAmount;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                    }
                    else
                    {
                        detail.PersonalPayAmount = detail.CompanyPayAmount;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion
                #region 第四步,贷款基本信息

                Url = "http://www.cqgjj.cn/Member/gr/gjjgrdk.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='listinfo']/tr/td[2]", "text", true);
                if (results.Count !=16)
                {
                    Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                    return Res;
                }
                Res_load.Name = results[0];//姓名
                Res_load.IdentityCard = results[1];
                Res_load.Account_Loan = results[2];//公积金贷款账号
                Res_load.Loan_Credit = results[3].ToDecimal(0);//贷款金额
                string[] tempdate = results[4].Split('~');
                if (tempdate.Length==2)
                {
                    Res_load.Loan_Start_Date = tempdate[0];//开款开始日期
                    Res_load.Loan_End_Date = tempdate[1];//贷款结束日期
                }
                Res_load.Repay_Type = results[5];//还款方式
                Res_load.Current_Repay_Total = results[7].ToDecimal(0);//当期还款金额
                Res_load.Period_Payed = results[8].ToInt(0);//已还款期数
                Res_load.Principal_Payed = results[9].ToDecimal(0);//已还本金
                Res_load.Interest_Payed = results[10].ToDecimal(0);//已还利息
                Res_load.Period_Total = results[12];//贷款期数
                Res_load.Description = "是否逾期："+results[13];
                Res_load.Overdue_Period = results[14].ToInt(0);//逾期期数
                Res_load.Overdue_Principal = results[15].ToDecimal(0);//逾期本金
                #endregion

                Res.ProvidentFundLoanRes = Res_load;
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
