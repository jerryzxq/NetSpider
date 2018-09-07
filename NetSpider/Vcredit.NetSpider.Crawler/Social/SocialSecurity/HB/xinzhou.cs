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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.HB
{
    public class xinzhou : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://61.183.226.190:7000/";
        string socialCity = "hb_xinzhou";
        string __LASTFOCUS = string.Empty;
        string __VIEWSTATE = string.Empty;
        string __EVENTTARGET = string.Empty;
        string __EVENTARGUMENT = string.Empty;
        string __EVENTVALIDATION = string.Empty;
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                httpItem = new HttpItem()
                {
                    URL = baseUrl,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = socialCity + ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                var result = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@type='hidden']", "value");
                

                CacheHelper.SetCache("loginInfo", result);

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
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //获取缓存
                if (CacheHelper.GetCache("loginInfo") != null)
                {
                    results = (List<string>)CacheHelper.GetCache("loginInfo");
                    CacheHelper.RemoveCache("loginInfo");
                }
                if (results.Count > 0)
                {
                    __LASTFOCUS = results[0];
                    __VIEWSTATE = results[1];
                    __EVENTTARGET = results[2];
                    __EVENTARGUMENT = results[3];
                    __EVENTVALIDATION = results[4];
                }

                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = baseUrl + "Default.aspx";
                postdata = String.Format(@"__LASTFOCUS={0}&__VIEWSTATE={1}&__EVENTTARGET={2}&__EVENTARGUMENT={3}&__EVENTVALIDATION={4}&TextBox_%E5%A7%93%E5%90%8D={5}&TextBox_%E4%B8%AA%E4%BA%BA%E7%BC%96%E5%8F%B7={6}&Button_%E7%99%BB%E5%BD%95="
                         , __LASTFOCUS.ToUrlEncode(), __VIEWSTATE.ToUrlEncode(), __EVENTTARGET.ToUrlEncode(), __EVENTARGUMENT.ToUrlEncode(), __EVENTVALIDATION.ToUrlEncode(), socialReq.Name.ToUrlEncode(), socialReq.Identitycard, "登录".ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var errormsg = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "');</script>");
                if (!errormsg.IsEmpty())
                {
                    Res.StatusDescription = errormsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 第二步， 获取基本信息
                //第一步登录已得到基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label_个人社保编号']", "");
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];//个人编号
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label_姓名']", "");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label_性别']", "");
                if (results.Count > 0)
                {
                    Res.Sex = results[0];//性别
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label_身份证']", "");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label_单位名称']", "");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];//单位名称
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label_出生日期']", "");
                if (results.Count > 0)
                {
                    Res.BirthDate = results[0];//出生日期
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label_参加工作日期']", "");
                if (results.Count > 0)
                {
                    Res.WorkDate = results[0];//参加工作时间
                }


                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label_职工状态']", "");
                if (results.Count > 0)
                {
                    Res.EmployeeStatus = results[0];//职工状态
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@type='hidden']", "value");
                if (results.Count > 0)
                {
                    __LASTFOCUS = results[0];
                    __VIEWSTATE = results[1];
                    __EVENTTARGET = results[2];
                    __EVENTARGUMENT = results[3];
                    __EVENTVALIDATION = results[4];
                }

                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label_退休日期']", "");
                //if (results.Count > 0)
                //{
                //    Res.Race = results[0];//退休日期
                //}
                #endregion


                #region 第三步，查询明细(如果存在养老保险，则以养老保险为主，否则为医疗保险) 系统只提供从2014-01-01至今的五险缴费信息
                //查询养老保险
                Url = baseUrl + "Table.aspx";
                postdata = string.Format("__LASTFOCUS={0}&__VIEWSTATE={1}&__EVENTTARGET={2}&__EVENTARGUMENT={3}&__EVENTVALIDATION={4}&%E9%99%A9%E7%A7%8D=RadioButton_%E5%85%BB%E8%80%81&Button_%E6%9F%A5%E8%AF%A2=%E6%9F%A5+%E8%AF%A2",
                    __LASTFOCUS.ToUrlEncode(), __VIEWSTATE.ToUrlEncode(), __EVENTTARGET.ToUrlEncode(), __EVENTARGUMENT.ToUrlEncode(), __EVENTVALIDATION.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='GridView_缴费记录']/tr", "");

                //查询医疗保险
                postdata = string.Format("__LASTFOCUS={0}&__VIEWSTATE={1}&__EVENTTARGET={2}&__EVENTARGUMENT={3}&__EVENTVALIDATION={4}&%E9%99%A9%E7%A7%8D=RadioButton_%E5%8C%BB%E7%96%97&Button_%E6%9F%A5%E8%AF%A2=%E6%9F%A5+%E8%AF%A2",
                  __LASTFOCUS.ToUrlEncode(), __VIEWSTATE.ToUrlEncode(), __EVENTTARGET.ToUrlEncode(), __EVENTARGUMENT.ToUrlEncode(), __EVENTVALIDATION.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var MedicalList = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='GridView_缴费记录']/tr", "");

                //查询失业保险
                postdata = string.Format("__LASTFOCUS={0}&__VIEWSTATE={1}&__EVENTTARGET={2}&__EVENTARGUMENT={3}&__EVENTVALIDATION={4}&%E9%99%A9%E7%A7%8D=RadioButton_%E5%A4%B1%E4%B8%9A&Button_%E6%9F%A5%E8%AF%A2=%E6%9F%A5+%E8%AF%A2",
                  __LASTFOCUS.ToUrlEncode(), __VIEWSTATE.ToUrlEncode(), __EVENTTARGET.ToUrlEncode(), __EVENTARGUMENT.ToUrlEncode(), __EVENTVALIDATION.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var UnemployList = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='GridView_缴费记录']/tr", "");

                //查询工伤保险
                postdata = string.Format("__LASTFOCUS={0}&__VIEWSTATE={1}&__EVENTTARGET={2}&__EVENTARGUMENT={3}&__EVENTVALIDATION={4}&%E9%99%A9%E7%A7%8D=RadioButton_%E5%B7%A5%E4%BC%A4&Button_%E6%9F%A5%E8%AF%A2=%E6%9F%A5+%E8%AF%A2",
               __LASTFOCUS.ToUrlEncode(), __VIEWSTATE.ToUrlEncode(), __EVENTTARGET.ToUrlEncode(), __EVENTARGUMENT.ToUrlEncode(), __EVENTVALIDATION.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var EmploymentInjuryList = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='GridView_缴费记录']/tr", "");

                //查询生育保险
                postdata = string.Format("__LASTFOCUS={0}&__VIEWSTATE={1}&__EVENTTARGET={2}&__EVENTARGUMENT={3}&__EVENTVALIDATION={4}&%E9%99%A9%E7%A7%8D=RadioButton_%E7%94%9F%E8%82%B2&Button_%E6%9F%A5%E8%AF%A2=%E6%9F%A5+%E8%AF%A2",
              __LASTFOCUS.ToUrlEncode(), __VIEWSTATE.ToUrlEncode(), __EVENTTARGET.ToUrlEncode(), __EVENTARGUMENT.ToUrlEncode(), __EVENTVALIDATION.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var MaternityList = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='GridView_缴费记录']/tr", "");
                foreach (var item in results)
                {
                    detailRes = new SocialSecurityDetailQueryRes();
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                    if (tdRow.Count != 8)
                    {
                        continue;
                    }
                    detailRes.PayTime = tdRow[3];  //缴费年月
                    detailRes.CompanyName = tdRow[1];  // 单位名称
                    detailRes.SocialInsuranceBase = tdRow[5].ToDecimal(0);  //缴费基数
                    if (tdRow[6].ToDecimal(0) != 0)
                    {
                        detailRes.PensionAmount = tdRow[6].ToDecimal(0) / 2; //个人缴费
                        detailRes.CompanyPensionAmount = tdRow[6].ToDecimal(0) / 2; //公司缴费
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;

                        //医疗保险
                        var EnterAccountMedicalAmountString = MedicalList.Where(e => HtmlParser.GetResultFromParser(e, "//td").Count > 0 && HtmlParser.GetResultFromParser(e, "//td")[3] == tdRow[3]);
                        if (EnterAccountMedicalAmountString.Count() > 0)
                        {
                            detailRes.EnterAccountMedicalAmount = HtmlParser.GetResultFromParser(EnterAccountMedicalAmountString.ToList()[0], "//td", "")[6].ToDecimal(0);
                        }

                        //失业保险
                        var UnemployString = UnemployList.Where(e => HtmlParser.GetResultFromParser(e, "//td").Count > 0 && HtmlParser.GetResultFromParser(e, "//td")[3] == tdRow[3]);
                        if (UnemployString.Count() > 0)
                        {
                            detailRes.UnemployAmount = HtmlParser.GetResultFromParser(UnemployString.ToList()[0], "//td", "")[6].ToDecimal(0);
                        }

                        //工伤保险
                        var EmploymentInjuryString = EmploymentInjuryList.Where(e => HtmlParser.GetResultFromParser(e, "//td").Count > 0 && HtmlParser.GetResultFromParser(e, "//td")[3] == tdRow[3]);
                        if (EmploymentInjuryString.Count() > 0)
                        {
                            detailRes.EmploymentInjuryAmount = HtmlParser.GetResultFromParser(EmploymentInjuryString.ToList()[0], "//td", "")[6].ToDecimal(0);
                        }

                        //生育保险
                        var MaternityString = MaternityList.Where(e => HtmlParser.GetResultFromParser(e, "//td").Count > 0 && HtmlParser.GetResultFromParser(e, "//td")[3] == tdRow[3]);
                        if (MaternityString.Count() > 0)
                        {
                            detailRes.MaternityAmount = HtmlParser.GetResultFromParser(MaternityString.ToList()[0], "//td", "")[6].ToDecimal(0);
                        }
                        PaymentMonths++;
                    }
                    else
                    {
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                    }

                    Res.Details.Add(detailRes);

                }
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
