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
namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.HL
{
    public class qiqihaer : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.qqhryb.com/";
        string socialCity = "hl_qiqihaer";

     
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
                Res.StatusDescription = socialCity + "无需初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

               // CacheHelper.SetCache(token, cookies);
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

            string __VIEWSTATE = string.Empty;
            string __LASTFOCUS = string.Empty;
            string __EVENTTARGET = string.Empty;
            string __EVENTARGUMENT = string.Empty;
            try
            {
                //获取缓存
                //if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                //{
                //    cookies = (CookieCollection)SpiderCacheHelper.GetCache(socialReq.Token);
                //    SpiderCacheHelper.RemoveCache(socialReq.Token);
                //}
                //校验参数
                if (socialReq.Username.IsEmpty() || socialReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = "身份证号或用户名不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 初始化
                 Url = baseUrl + "yblb.aspx?id=208&zid=238";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var result = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTTARGET']", "value");
                if (result.Count > 0)
                {
                    __EVENTTARGET = result[0];
                }
                result = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTARGUMENT']", "value");
                if (result.Count > 0)
                {
                    __EVENTARGUMENT = result[0];
                }
                result = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__LASTFOCUS']", "value");
                if (result.Count > 0)
                {
                    __LASTFOCUS = result[0];
                }
                result = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                if (result.Count > 0)
                {
                    __VIEWSTATE = result[0];
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region 第一步，登录
                //var password = CommonFun.GetMd5Str(socialReq.Password);  //密码通过md5加密
                Url = baseUrl + "yblb.aspx?id=208&zid=238";
                postdata = String.Format("__EVENTTARGET={0}&__EVENTARGUMENT={1}&__LASTFOCUS={2}&__VIEWSTATE={3}&TextBox6=&TextBox1=&TextBox7={4}&TextBox8={5}&Button3=%C8%B7%C8%CF%CC%E1%BD%BB", __EVENTTARGET, __EVENTARGUMENT, __LASTFOCUS, __VIEWSTATE.ToUrlEncode(), socialReq.Username, socialReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                __VIEWSTATE = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                var errormsg = HtmlParser.GetResultFromParser(httpResult.Html, "//scrpit[@language='JavaScript']", "");
                if (errormsg.Count > 0)
                {
                    Res.StatusDescription = errormsg[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，个人基本信息
                //人员基本信息在登录中已经获取
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='a_AAC001']", "");
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];  //个人编号
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='a_AAE135']", "");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];  //身份证号
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='a_AAC003']", "");
                if (results.Count > 0)
                {
                    Res.Name = results[0];  //姓名
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='a_AAC004']", "");
                if (results.Count > 0)
                {
                    Res.Sex = results[0];  //性别
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='a_AAC005']", "");
                if (results.Count > 0)
                {
                    Res.Race = results[0];  //民族
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='a_AAC006']", "");
                if (results.Count > 0)
                {
                    Res.BirthDate = results[0];  //出生日期
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='a_AAC007']", "");
                if (results.Count > 0)
                {
                    Res.WorkDate = results[0];  //参加工作日期
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='a_AAE044']", "");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];  //单位名称
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='a_BAB304']", "");
                if (results.Count > 0)
                {
                    Res.Address = results[0];  //地址
                }
                #endregion



                #region 第三步，养老缴费明细
                int pagecount = 0;
                Url = baseUrl + string.Format("ybqs.aspx?bh={0}&sf={1}&jg=3&sj={2}", socialReq.Username, socialReq.Identitycard, Res.EmployeeNo);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='GridView3_ctl53_lblPageCount']", "");
                if (results.Count > 0)
                {
                    pagecount = results[0].ToInt(0);
                }
                __VIEWSTATE = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];

                for (int i = 0; i < pagecount; i++)
                {
                    //http://www.qqhryb.com/yblb_cx.aspx?bh=5066592&sf=23020219721124221X&jg=3&sj=1025066592
                    Url = baseUrl + string.Format("yblb_cx.aspx?bh={0}&sf={1}&jg=3&sj={2}", socialReq.Username, socialReq.Identitycard, Res.EmployeeNo);
                    postdata = string.Format("__EVENTTARGET=GridView3%24ctl53%24DropDownList{2}&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE={0}&GridView3%24ctl53%24DropDownList1={1}", __VIEWSTATE.ToUrlEncode(), i,i);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "Post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='GridView3']/tr", "");
                    var dateFlag = "";
                    detailRes = new SocialSecurityDetailQueryRes();
                    foreach (var item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                        if (tdRow.Count != 14)
                        {
                            continue;
                        }

                        if (dateFlag != tdRow[2].ToString())
                        {
                            if (!string.IsNullOrEmpty(detailRes.CompanyName))
                            {
                                Res.Details.Add(detailRes);
                                PaymentMonths++;
                            }
                            detailRes = new SocialSecurityDetailQueryRes();
                        }
                        dateFlag = tdRow[2];
                        detailRes.PayTime = tdRow[2];  //缴费日期
                        detailRes.SocialInsuranceTime = tdRow[3];  //所属日期
                        detailRes.CompanyName = tdRow[1];  //单位名称
                        detailRes.Name = tdRow[0];  //姓名
                        if (tdRow[4].Contains("城镇职工医疗保险"))
                        {
                            if (tdRow[6].Contains("正常应缴"))
                            {
                                detailRes.SocialInsuranceBase = tdRow[9].ToDecimal(0);  //缴费基数
                                detailRes.MedicalAmount = tdRow[10].ToDecimal(0);  //个人缴纳医疗保险
                                detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                                detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            }
                            else
                            {
                                detailRes.SocialInsuranceBase = tdRow[9].ToDecimal(0);  //缴费基数
                                detailRes.MedicalAmount = tdRow[10].ToDecimal(0);  //个人缴纳医疗保险
                                detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                                detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                            }
                        }
                        else if (tdRow[4].Contains("大额补助医疗保险"))
                        {
                            detailRes.IllnessMedicalAmount = tdRow[10].ToDecimal(0);
                        }
                        else if (tdRow[4].Contains("工伤保险"))
                        {
                            detailRes.EmploymentInjuryAmount = tdRow[10].ToDecimal(0);
                        }
                        else if (tdRow[4].Contains("生育保险"))
                        {
                            detailRes.MaternityAmount = tdRow[10].ToDecimal(0);
                        }

                        if (item.Equals(results[results.Count - 1]))  //当为循环最后一个时添加
                        {
                            Res.Details.Add(detailRes);
                            PaymentMonths++;
                        }
                    }
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
