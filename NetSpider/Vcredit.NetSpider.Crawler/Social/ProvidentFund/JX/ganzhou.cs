using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.JX
{
    public class ganzhou : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.gzszfgjj.com/wscx/";
        string fundCity = "jx_ganzhou";
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
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            ProvidentFundDetail detail = null;
            int PaymentMonths = 0;
            string cxyd = string.Empty;
            string dbname = string.Empty;
            string yzm = string.Empty;
            string zgzh = string.Empty;
            string zgxm = string.Empty;
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
                if (string.IsNullOrEmpty(fundReq.Identitycard) || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "身份证号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //15位或18位身份证验证
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (regex.IsMatch(fundReq.Identitycard)==false)
                {
                    Res.StatusDescription = "请输入有效的身份证号";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步登陆

                Url = baseUrl + "zfbzgl/wscx/login.jsp";
                httpItem = new HttpItem
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='cxyd']", "value");
                if (results.Count > 0)
                {
                    cxyd = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dbname']", "value");
                if (results.Count > 0)
                {
                    dbname = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='yzm']", "value");
                if (results.Count > 0)
                {
                    yzm = results[0];
                }
                //登陆
                Url = baseUrl + string.Format("zfbzgl/wscx/login_hidden.jsp?password={0}&sfzh={1}&cxyd={2}&dbname={3}&dlfs=01&yzm={4}", fundReq.Password, fundReq.Identitycard, cxyd, dbname, yzm);
                httpItem = new HttpItem
                {
                    URL = Url,
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
                //错误提示
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\");");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgzh']", "value");
                if (results.Count > 0)
                {
                    zgzh = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgxm']", "value");
                if (results.Count > 0)
                {
                    zgxm = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dwbm']", "value");
                if (results.Count > 0)
                {
                    dwbm = results[0];
                }
                Res.ProvidentFundNo = zgzh;
                Res.CompanyNo = dwbm;
                #endregion
                #region 第二步，获取个人信息

                Res.Name = fundReq.Name;
                Res.IdentityCard = fundReq.Identitycard;
                Res.ProvidentFundNo = fundReq.FundAccount;
                Res.Phone = fundReq.Mobile;

                Url = baseUrl + "zfbzgl/gjjxxcx/gjjxxcx.jsp";
                postdata = string.Format("sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd={4}&zgzt=null", fundReq.Identitycard, zgxm.ToUrlEncode(Encoding.GetEncoding("GBK")), zgzh, dwbm, cxyd);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='zhengwen']/div[3]/table/tr[1]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='zhengwen']/div[3]/table/tr[1]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.BankCardNo = results[0].Replace("-", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='zhengwen']/div[3]/table/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='zhengwen']/div[3]/table/tr[2]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='zhengwen']/div[3]/table/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='zhengwen']/div[3]/table/tr[4]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.OpenTime = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='zhengwen']/div[3]/table/tr[4]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.Status = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='zhengwen']/div[3]/table/tr[5]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.SalaryBase = results[0].Replace("元", "").ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='zhengwen']/div[3]/table/tr[5]/td[4]", "text");
                if (results.Count > 0)
                {
                    string[] arrry = results[0].Replace("%", "").Split('|');//（个人|单位|财政）
                    if (arrry.Length > 0)
                    {
                        Res.PersonalMonthPayRate = arrry[0].ToDecimal(0) * 0.01M;
                        Res.CompanyMonthPayRate = arrry[1].ToDecimal(0) * 0.01M;
                    }
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='zhengwen']/div[3]/table/tr[7]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].Replace("元", "").ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='zhengwen']/div[3]/table/tr[8]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = results[0].Replace("元", "").ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='zhengwen']/div[3]/table/tr[10]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].Replace("元", "").ToDecimal(0);
                }
                #endregion
                #region 第三步，历史账户明细

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
                results = new List<string>();
                Url = "http://www.gzszfgjj.com/wscx/zfbzgl/gjjmxcx/gjjmxcx.jsp";
                for (int i = 0; i < 3; i++)
                {
                    int year = DateTime.Now.AddYears(-i).Year;
                    int totalpages = 1;
                    int yss = 1;
                    do
                    {
                        postdata = string.Format("cxydtwo={0}&yss={1}&totalpages={2}&cxyd={3}&zgzh={4}&sfzh={5}&zgxm={6}&dwbm={7}", year, yss, totalpages, cxyd, zgzh, fundReq.Identitycard, zgxm.ToUrlEncode(Encoding.GetEncoding("GBK")), dwbm);
                        httpItem = new HttpItem
                        {
                            URL = Url,
                            Method = "Post",
                            Postdata = postdata,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                        if (yss == 1)
                        {
                            List<string> pages = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@name='form11']/input[@name='totalpages']", "value");
                            if (pages.Count > 0)
                            {
                                totalpages = pages[0].ToInt(0);
                            }
                        }
                        results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='zhengwen']/div[3]/table/tr[position()>1]", "inner"));
                        yss++;
                    } while (yss <= totalpages);
                }
                regex = new Regex(@"[0-9][0-9]*");
                foreach (var items in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(items, "//td", "text");
                    if (tdRow.Count < 5)
                    {
                        continue;
                    }
                    detail = new ProvidentFundDetail();
                    detail.Description = tdRow[1];
                    detail.PayTime = tdRow[0].ToDateTime();
                    if (tdRow[1].Contains("汇缴"))
                    {
                        detail.PersonalPayAmount = Math.Round(tdRow[3].ToDecimal(0) * perAccounting, 2); //个人月缴额
                        detail.CompanyPayAmount = Math.Round(tdRow[3].ToDecimal(0) * comAccounting, 2); //公司月缴额
                        detail.ProvidentFundBase = Math.Round(tdRow[3].ToDecimal(0) / totalRate, 2); //缴费基数
                        detail.ProvidentFundTime = regex.Matches(tdRow[1])[0].Value ; //应属年月
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        PaymentMonths++;
                    }
                    else
                    {//（补缴，结息etc，数据不精确，只做参考用）
                        detail.PersonalPayAmount = Math.Abs(tdRow[2].ToDecimal(0) - tdRow[3].ToDecimal(0)); //个人缴费
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
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
    }
}
