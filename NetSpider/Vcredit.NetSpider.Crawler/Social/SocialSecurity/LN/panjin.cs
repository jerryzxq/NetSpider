using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.Common.Constants;
using Vcredit.NetSpider.DataAccess.Cache;
using System.Text.RegularExpressions;
namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.LN
{
    public class panjin : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://123.190.194.146:8096/";
        string socialCity = "ln_panjin";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {

                Url = baseUrl + "captcha.svl?d=1458106670068";
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

                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitError;
                Log4netAdapter.WriteError(socialCity + ServiceConsts.SocialSecurity_InitError, e);
            }
            return Res;
        }

        public SocialSecurityQueryRes SocialSecurityQuery(SocialSecurityReq socialReq)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = socialCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(socialReq.Token);
                    SpiderCacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Password.IsEmpty() || socialReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = "身份证号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = baseUrl + "loginvalidate.html";
                postdata = String.Format("type=1&account={0}&password={1}&captcha={2}", socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
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
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (!httpResult.Html.Contains("success"))
                {
                    Res.StatusDescription = "用户名或密码错误";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，个人基本信息
                //人员基本信息
                Url = baseUrl + "person/personRYJBXX.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='grid']/tr[2]/td", "");
                if (results.Count > 0)
                {
                    Res.Loginname = results[1].Split('&')[0];//社保卡卡号
                    Res.EmployeeNo = results[2].Split('&')[0];  //个人编号
                    Res.Name = results[4].Split('&')[0];  // 姓名
                    Res.Sex = results[5].Split('&')[0];  //性别
                    Res.Race = results[6].Split('&')[0];  //民族
                    Res.BirthDate = results[7].Split('&')[0];  //出生日期
                    Res.EmployeeStatus = results[8].Split('&')[0];  //状态
                }
                //参保信息
                Url = baseUrl + "person/web_personYLCBJFXXCX_query.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='divgrid']/table[@class='grid']/tr[2]/td", "");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[5].Split('&')[0];  //单位编号
                    Res.CompanyName = results[6].Split('&')[0]; //单位名称
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='divgrid2']/table[@class='grid']/tr[2]/td", "");
                if (results.Count > 0)
                {
                    Res.SocialInsuranceBase = results[9].Split('&')[0].ToDecimal(0);  //养老保险缴费基数
                }
                #endregion

                #region 第三步，养老缴费明细

                #region 养老保险
                //得到养老保险缴费页数
                Url = baseUrl + "person/web_personYLCBJFXXCX_query.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                int pagecount = 0;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tab2']/tr/td/table[@id='queryResult']/tr/td/table/tr/td[@class='page_num']", "");
                if (results.Count > 0)
                {
                    var pageString = results[0].Split('<')[0].Split('|')[2].Split('/')[1];
                    string number = Regex.Replace(pageString, "\\D+", "");
                    pagecount = number.ToInt(0);
                }

                //查询养老保险缴费详细
                for (int i = 1; i <= pagecount; i++)
                {
                    Url = baseUrl + "person/web_personYLCBJFXXCX_query.html";
                    postdata = string.Format("pageNo=1&pageNo1={0}&pageNo2=1&pageNo3=1&tzid=2&datanum=1&datanum=100&datanum=10&datanum=1", i);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "Post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='divgrid1']/table/tr", "");
                    foreach (var item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        detailRes = new SocialSecurityDetailQueryRes();
                        if (tdRow.Count != 15 || tdRow[0].Contains("&"))
                        {
                            continue;
                        }

                        detailRes.IdentityCard = Res.IdentityCard;  //身份证号
                        detailRes.Name = tdRow[5].Split('&')[0];  //姓名
                        detailRes.CompanyName = tdRow[7].Split('&')[0]; ;  //公司名称
                        if (tdRow[8].Split('&')[0].Contains("正常"))
                        {
                            detailRes.PaymentType = tdRow[8].Split('&')[0];  //缴费类型
                            detailRes.PaymentFlag = tdRow[10].Split('&')[0];  //缴费标志
                            detailRes.PayTime = tdRow[9].Split('&')[0];   //缴费年月
                            detailRes.SocialInsuranceTime = tdRow[14].Split('&')[0];   //应属年月
                            detailRes.SocialInsuranceBase = tdRow[11].Split('&')[0].ToDecimal(0);   //缴费基数
                            detailRes.PensionAmount = tdRow[12].Split('&')[0].ToDecimal(0);   //个人基数
                            detailRes.CompanyPensionAmount = tdRow[13].Split('&')[0].ToDecimal(0);   //单位基数
                            PaymentMonths++;
                        }
                        else
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;  //缴费类型
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;  //缴费标志
                            detailRes.PayTime = tdRow[9].Split('&')[0];   //缴费年月
                            detailRes.PensionAmount = tdRow[12].Split('&')[0].ToDecimal(0);   //个人基数
                        }

                        Res.Details.Add(detailRes);

                    }

                }

                #endregion

                #region 医疗保险

                //得到医疗保险缴费页数
                Url = baseUrl + "person/web_personYILCBJFXX_query.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                pagecount = 0;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tab3']/tr/td/table[@id='queryResult']/tr/td/table/tr/td[@class='page_num']", "");
                if (results.Count > 0)
                {
                    var pageString = results[0].Split('<')[0].Split('|')[2].Split('/')[1];
                    string number = Regex.Replace(pageString, "\\D+", "");
                    pagecount = number.ToInt(0);
                }

                //查询医疗保险缴费详细
                for (int i = 1; i <= pagecount; i++)
                {
                    Url = baseUrl + "person/web_personYILCBJFXX_query.html";
                    postdata = string.Format("pageNo=1&pageNo1=1&pageNo2={0}&pageNo3=1&tzid=3&datanum=1&datanum=1&datanum=10", i);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "Post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='divgrid2']/table/tr", "");
                    foreach (var item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 15 || tdRow[0].Contains("&"))
                        {
                            continue;
                        }
                        if (tdRow[2].Split('&')[0].Contains("基本医疗保险") && tdRow[10].Split('&')[0].Contains("已实缴"))
                        {
                            var payTime = tdRow[9].Split('&')[0];   //缴费年月
                            if (Res.Details.Where(e => e.PayTime == payTime).Count() > 0)
                            {
                                Res.Details.Where(e => e.PayTime == payTime).ToList()[0].MedicalAmount = tdRow[12].Split('&')[0].ToDecimal(0);  //个人医疗缴费
                                Res.Details.Where(e => e.PayTime == payTime).ToList()[0].CompanyMedicalAmount = tdRow[13].Split('&')[0].ToDecimal(0); //单位医疗缴费
                            }
                        }
                    }
                }

                #endregion

                #region 失业保险
                //得到失业保险缴费页数
                Url = baseUrl + "person/web_personSYCBJFXX_query.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                pagecount = 0;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tab3']/tr/td/table[@id='queryResult']/tr/td/table/tr/td[@class='page_num']", "");
                if (results.Count > 0)
                {
                    var pageString = results[0].Split('<')[0].Split('|')[2].Split('/')[1];
                    string number = Regex.Replace(pageString, "\\D+", "");
                    pagecount = number.ToInt(0);
                }

                //查询失业保险缴费详细
                for (int i = 1; i <= pagecount; i++)
                {
                    Url = baseUrl + "person/web_personSYCBJFXX_query.html";
                    postdata = string.Format("pageNo=1&pageNo1=1&pageNo2={0}&pageNo3=1&tzid=3&datanum=1&datanum=1&datanum=10", i);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "Post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='divgrid2']/table/tr", "");
                    foreach (var item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 15 || tdRow[0].Contains("&"))
                        {
                            continue;
                        }
                        if (tdRow[2].Split('&')[0].Contains("失业保险") && tdRow[10].Split('&')[0].Contains("已实缴"))
                        {
                            var payTime = tdRow[9].Split('&')[0];   //缴费年月
                            if (Res.Details.Where(e => e.PayTime == payTime).Count() > 0)
                            {
                                Res.Details.Where(e => e.PayTime == payTime).ToList()[0].UnemployAmount = tdRow[12].Split('&')[0].ToDecimal(0) + tdRow[13].Split('&')[0].ToDecimal(0);  //失业保险（个人缴费加上单位缴费）
                            }
                        }
                    }

                }
                #endregion

                #region 工伤保险
                //得到工伤保险缴费页数
                Url = baseUrl + "person/web_personGSCBJFXX_query.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                pagecount = 0;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tab3']/tr/td/table[@id='queryResult']/tr/td/table/tr/td[@class='page_num']", "");
                if (results.Count > 0)
                {
                    var pageString = results[0].Split('<')[0].Split('|')[2].Split('/')[1];
                    string number = Regex.Replace(pageString, "\\D+", "");
                    pagecount = number.ToInt(0);
                }

                //查询工伤保险缴费详细
                for (int i = 1; i <= pagecount; i++)
                {
                    Url = baseUrl + "person/web_personGSCBJFXX_query.html";
                    postdata = string.Format("pageNo=1&pageNo1=1&pageNo2={0}&pageNo3=1&tzid=3&datanum=1&datanum=1&datanum=10", i);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "Post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='divgrid2']/table/tr", "");
                    foreach (var item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 15 || tdRow[0].Contains("&"))
                        {
                            continue;
                        }
                        if (tdRow[2].Split('&')[0].Contains("工伤") && tdRow[10].Split('&')[0].Contains("已实缴"))
                        {
                            var payTime = tdRow[9].Split('&')[0];   //缴费年月
                            if (Res.Details.Where(e => e.PayTime == payTime).Count() > 0)
                            {
                                Res.Details.Where(e => e.PayTime == payTime).ToList()[0].EmploymentInjuryAmount = tdRow[12].Split('&')[0].ToDecimal(0) + tdRow[13].Split('&')[0].ToDecimal(0);  //工伤保险（个人缴费加上单位缴费）
                            }
                        }
                    }
                }

                #endregion

                #region 生育保险
                //得到生育保险缴费页数

                Url = baseUrl + "person/web_personSHYCBJFXX_query.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                pagecount = 0;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tab3']/tr/td/table[@id='queryResult']/tr/td/table/tr/td[@class='page_num']", "");
                if (results.Count > 0)
                {
                    var pageString = results[0].Split('<')[0].Split('|')[2].Split('/')[1];
                    string number = Regex.Replace(pageString, "\\D+", "");
                    pagecount = number.ToInt(0);
                }

                //查询生育保险缴费详细
                for (int i = 1; i <= pagecount; i++)
                {
                    Url = baseUrl + "person/web_personSHYCBJFXX_query.html";
                    postdata = string.Format("pageNo=1&pageNo1={0}&pageNo2=1&pageNo3=1&tzid=3&datanum=1&datanum=1&datanum=10", i);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "Post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='divgrid2']/table/tr", "");
                    foreach (var item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 15 || tdRow[0].Contains("&"))
                        {
                            continue;
                        }
                        if (tdRow[2].Split('&')[0].Contains("生育") && tdRow[10].Split('&')[0].Contains("已实缴"))
                        {
                            var payTime = tdRow[9].Split('&')[0];   //缴费年月
                            if (Res.Details.Where(e => e.PayTime == payTime).Count() > 0)
                            {
                                Res.Details.Where(e => e.PayTime == payTime).ToList()[0].MaternityAmount = tdRow[12].Split('&')[0].ToDecimal(0) + tdRow[13].Split('&')[0].ToDecimal(0);  //工伤保险（个人缴费加上单位缴费）
                            }
                        }
                    }

                }
                #endregion


                #endregion
                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(socialCity + ServiceConsts.SocialSecurity_QueryError, e);
            }
            return Res;
        }
    }
}
