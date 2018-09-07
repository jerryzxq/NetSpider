using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.Common.Constants;
namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.JX
{
    public class nanchang : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.ncgjj.com.cn/";
        string fundCity = "jx_nanchang";
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
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
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            ProvidentFundDetail detail = null;
            int PaymentMonths = 0;
            Regex reg = new Regex(@"[^0-9]*");
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                string txtNumber;//登陆账号

                //校验参数
                if (fundReq.LoginType == "2")
                {
                    if (string.IsNullOrEmpty(fundReq.Identitycard) || fundReq.Password.IsEmpty())
                    {
                        Res.StatusDescription = "身份证号或密码不能为空";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    //15位或18位身份证验证
                    Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                    if (!regex.IsMatch(fundReq.Identitycard))
                    {
                        Res.StatusDescription = "请输入有效的身份证号";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    Res.IdentityCard = txtNumber = fundReq.Identitycard.Trim(); //身份账号
                }
                else
                {
                    if (string.IsNullOrEmpty(fundReq.Username) || fundReq.Password.IsEmpty())
                    {
                        Res.StatusDescription = "用户名或密码不能为空";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    //15位或18位身份证验证
                    Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                    if (regex.IsMatch(fundReq.Username))
                    {
                        Res.StatusDescription = "无效的用户名";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    Res.Loginname = txtNumber = fundReq.Username;//用户名
                }
                #region 第一步登陆
                Url = baseUrl + "tools/submit_ajax.ashx?action=user_login";//
                postdata = String.Format("txtUserName={0}&txtPassword={1}&chkRemember=false", txtNumber.ToUrlEncode(), fundReq.Password.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                string errorInfo = CommonFun.GetMidStr(httpResult.Html, "\"msgbox\":\"", "\"}");
                if (!errorInfo.IsEmpty() && !errorInfo.Contains("会员登录成功"))
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region 第二步，获取个人信息
                Url = baseUrl + "tools/api_ajax.ashx?action=139";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "/div/dl[1]/dd", "innertext");
                //if (results.Count > 0)
                //{
                //    //Res.EmployeeNo = results[0];//职工账号
                //}
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/div/dl[2]/dd", "innertext");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/div/dl[3]/dd", "innertext");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//证件号码（身份证）
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/div/dl[4]/dd", "innertext");
                if (results.Count > 0)
                {
                    Res.OpenTime = results[0];//开户时间
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/div/dl[5]/dd", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];//单位编号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/div/dl[6]/dd", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];//公司名称
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/div/dl[7]/dd", "innertext");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = results[0].Replace("%", "").ToDecimal(0) * 0.01M;//个人缴存比率
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/div/dl[8]/dd", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = results[0].Replace("%", "").ToDecimal(0) * 0.01M;//单位缴存比率
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/div/dl[9]/dd", "innertext");
                if (results.Count > 0)
                {
                    Res.SalaryBase = results[0].ToDecimal(0);//月均工资
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/div/dl[10]/dd", "innertext");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].ToDecimal(0);//个人月缴费
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/div/dl[11]/dd", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = results[0].ToDecimal(0);//公司月缴费
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/div/dl[13]/dd", "innertext");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0];//缴至年月
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/div/dl[15]/dd", "innertext");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);//账户余额
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/div/dl[16]/dd", "innertext");
                if (results.Count > 0)
                {
                    Res.Status = results[0];//账户状态
                }
                Url = baseUrl + "user/center/proinfo.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='txtMobile']", "value");
                if (results.Count > 0)
                {
                    Res.Phone = results[0];//电话
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='txtCodeID']", "value");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];//缴存账号
                }
                #endregion
                #region 第三步，公积金明细

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
                    totalRate = payRate * 2;//0.16
                    perAccounting = comAccounting = 0.50M;
                }
                #region 个人账户明细

                Url = baseUrl + "tools/api_ajax.ashx?temp=0.3274338960882348&action=142&s=&no-cache=0.8506160405183796";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    Referer = baseUrl + "user/grgjj/142.aspx",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr[position()>1]", "inner", true);
                if (results.Count > 0)
                {
                    foreach (var items in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(items, "//td", "text", true);
                        if (tdRow.Count != 5)
                        {
                            continue;
                        }
                        detail = new ProvidentFundDetail();
                        detail.PayTime = tdRow[0].Trim().ToDateTime();
                        detail.Description = tdRow[1];
                        if (tdRow[1].StartsWith("汇缴") == true && tdRow[1].Trim().EndsWith("公积金") == true)
                        {
                            detail.PersonalPayAmount = tdRow[2].ToDecimal(0) * perAccounting; //个人月缴额
                            detail.CompanyPayAmount = tdRow[2].ToDecimal(0) * comAccounting; //公司月缴额
                            detail.ProvidentFundBase = tdRow[2].ToDecimal(0) / (totalRate); //缴费基数
                            detail.ProvidentFundTime = reg.Replace(tdRow[1], ""); //应属年月
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            PaymentMonths++;
                        }
                        else if (tdRow[1].IndexOf("取", StringComparison.Ordinal) > -1)
                        {
                            detail.PersonalPayAmount =tdRow[3].ToDecimal(0);
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                        }
                        else
                        {//（补缴，结息etc，数据不精确，只做参考用）
                            detail.PersonalPayAmount = tdRow[2].ToDecimal(0) + tdRow[3].ToDecimal(0); //个人缴费
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        }
                        Res.ProvidentFundDetailList.Add(detail);
                    }
                }
                else
                {
                    Res.StatusDescription = "暂无账户明细";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #endregion
                #region 历史年度明细

                Url = baseUrl + "page/gjj/grmx.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                string VIEWSTATE = string.Empty;
                string VIEWSTATEGENERATOR = string.Empty;
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATEGENERATOR']", "value", true);
                if (results.Count > 0)
                {
                    VIEWSTATEGENERATOR = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value", true);
                if (results.Count > 0)
                {
                    EVENTVALIDATION = results[0];
                }
                //获取select往年年份
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@id='ddl_year']/option", "value", true);
                if (results.Count > 0)
                {
                    Url = baseUrl + "page/gjj/grmx.aspx";
                    int index = 0;
                    foreach (var item in results)
                    {
                        postdata = string.Format("__EVENTTARGET=ddl_year&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__EVENTVALIDATION={2}&ddl_year={3}", VIEWSTATE.ToUrlEncode(), VIEWSTATEGENERATOR.ToUrlEncode(), EVENTVALIDATION.ToUrlEncode(), item.ToUrlEncode());
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "Post",
                            Postdata = postdata,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='form1']/table[2]/tr[position()>1]", "inner", true);
                        if (index == 0)
                        {
                            if (results.Count == 0)
                            {
                                Res.StatusDescription = "该用户暂无明细记录";
                                Res.StatusCode = ServiceConsts.StatusCode_fail;
                                return Res;
                            }
                            index++;
                        }
                        foreach (var items in results)
                        {
                            var tdRow = HtmlParser.GetResultFromParser(items, "//td", "text", true);
                            if (tdRow.Count != 5)
                            {
                                continue;
                            }
                            detail = new ProvidentFundDetail();
                            detail.PayTime = tdRow[0].Trim().ToDateTime(Consts.DateFormatString5);
                            detail.Description = tdRow[2];
                            if (tdRow[2].StartsWith("汇缴") == true && tdRow[2].Trim().EndsWith("公积金") == true)
                            {
                                detail.PersonalPayAmount = tdRow[3].ToDecimal(0) * perAccounting;//个人月缴额
                                detail.CompanyPayAmount = tdRow[3].ToDecimal(0) * comAccounting;//公司月缴额
                                detail.ProvidentFundBase = tdRow[3].ToDecimal(0) / (totalRate);//缴费基数
                                detail.ProvidentFundTime = reg.Replace(tdRow[2], "");//应属年月
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                                PaymentMonths++;
                            }
                            else if (tdRow[2].Contains("单位"))
                            {
                                detail.CompanyPayAmount = tdRow[3].ToDecimal(0);//公司缴费
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            }
                            else if (tdRow[1].IndexOf("取", StringComparison.Ordinal) > -1)
                            {
                                detail.PersonalPayAmount = tdRow[4].ToDecimal(0);
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                            }
                            else
                            {//（补缴，结息etc，数据不精确，只做参考用）
                                detail.PersonalPayAmount = Math.Abs(tdRow[3].ToDecimal(0) - tdRow[4].ToDecimal(0));//个人缴费
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            }
                            Res.ProvidentFundDetailList.Add(detail);
                        }
                    }
                }
                #endregion
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
