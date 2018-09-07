﻿using System;
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
    public class yichang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string _eventvalidation = string.Empty;
        string _viewstate = string.Empty;
        string baseUrl = "http://219.139.130.121:9000/";
        string socialCity = "hb_yichang";

        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            string cookieStr = string.Empty;
            try
            {
                Url = baseUrl + "laborbureau/login.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                _eventvalidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                _viewstate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];


                Url = baseUrl + "ValidateCode.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
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
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("cookies", cookies);
                dic.Add("_eventvalidation", _eventvalidation);
                dic.Add("_viewstate", _viewstate);
                CacheHelper.SetCache(token, dic);
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
            Dictionary<string, object> dic = null;
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    dic = (Dictionary<string, object>)CacheHelper.GetCache(socialReq.Token);
                    cookies = (CookieCollection)dic["cookies"];
                    _eventvalidation = (string)dic["_eventvalidation"];
                    _viewstate = (string)dic["_viewstate"];
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                Url = baseUrl + "laborbureau/login.aspx";
                postdata = String.Format("__VIEWSTATE={0}&__EVENTVALIDATION={1}&ctl00%24ContentPlaceHolder1%24TextBox1={2}&ctl00%24ContentPlaceHolder1%24TextBox2={3}&ctl00%24ContentPlaceHolder1%24Validator={4}&ctl00%24ContentPlaceHolder1%24DDList_cbd=420501&ctl00%24ContentPlaceHolder1%24ImageButton1.x=0&ctl00%24ContentPlaceHolder1%24ImageButton1.y=0", _viewstate.ToUrlEncode(), _eventvalidation.ToUrlEncode(), socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "laborbureau/login.aspx?cbd=",
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
                string err = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "');</script></form>");
                if (!err.IsEmpty())
                {
                    Res.StatusDescription = err;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取基本信息
                Url = baseUrl + "laborbureau/userinfo.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "requestpage.aspx?type=lb",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//body/table[@class='line1']/tr/td/span", "inner");
                if (results.Count <= 0 && results[2].IsEmpty())
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }


                Res.EmployeeNo = results[2].Trim();//编号
                Res.Name = results[3].Trim();//姓名
                Res.Sex = results[4].Trim();//性别
                Res.BirthDate = results[5].Trim();//出生日期
                Res.Race = results[6].Trim();//民族
                Res.IdentityCard = results[1].Trim();//身份证号
                Res.EmployeeStatus = results[16].Trim();//工作状态
                //Res.Phone = results[19].Trim();//电话
                Res.CompanyName = results[0].Trim();//公司

                #endregion

                #region 查询明细
                int pageIndex = 1;
                int pageCount = 0;
                DateTime date = DateTime.Now;
              
                Url = baseUrl + "laborbureau/payinfo.aspx?type=11";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                _eventvalidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                _viewstate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                Url = baseUrl + "laborbureau/payinfo.aspx?type=11";
                do
                {
                    postdata = pageIndex == 1 ? string.Format("__VIEWSTATE={0}&__EVENTTARGET=&__EVENTARGUMENT=&__EVENTVALIDATION={1}&ctl00%24ContentPlaceHolder1%24WebCalendar1%24DropDownList1={2}&ctl00%24ContentPlaceHolder1%24WebCalendar1%24DropDownList2=06&ctl00%24ContentPlaceHolder1%24WebCalendar2%24DropDownList1={4}&ctl00%24ContentPlaceHolder1%24WebCalendar2%24DropDownList2=06&ctl00%24ContentPlaceHolder1%24dpl1=%E5%85%A8%E9%83%A8%E9%99%A9%E7%A7%8D&ctl00%24ContentPlaceHolder1%24ImageButton1.x=40&ctl00%24ContentPlaceHolder1%24ImageButton1.y=11", _viewstate.ToUrlEncode(), _eventvalidation.ToUrlEncode(), date.Year-5, date.Month.ToString("D2"), date.Year) : string.Format("__VIEWSTATE={0}&__EVENTTARGET=ctl00%24ContentPlaceHolder1%24AspNetPager1&__EVENTARGUMENT={1}&__EVENTVALIDATION={2}&ctl00%24ContentPlaceHolder1%24WebCalendar1%24DropDownList1={3}&ctl00%24ContentPlaceHolder1%24WebCalendar1%24DropDownList2={4}&ctl00%24ContentPlaceHolder1%24WebCalendar2%24DropDownList1={5}&ctl00%24ContentPlaceHolder1%24WebCalendar2%24DropDownList2={4}&ctl00%24ContentPlaceHolder1%24dpl1=%E5%85%A8%E9%83%A8%E9%99%A9%E7%A7%8D", _viewstate.ToUrlEncode(), pageIndex, _eventvalidation.ToUrlEncode(), date.Year - 5, date.Month.ToString("D2"), date.Year);
                    Byte[] postByte = Encoding.UTF8.GetBytes(postdata);
                    httpItem = new HttpItem()
                    {
                        Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                        URL = Url,
                        //Host = "219.139.130.121:9000",
                        Method = "POST",
                        PostdataByte=postByte,
                        PostDataType=PostDataType.Byte,
                        ContentType = "application/x-www-form-urlencoded",
                        Timeout=10000,
                        Referer = baseUrl + "laborbureau/payinfo.aspx?type=11",
                        UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0",
                        ProtocolVersion = HttpVersion.Version10,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    _eventvalidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value")[0];
                    _viewstate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                    pageCount = CommonFun.GetMidStr(CommonFun.GetMidStr(httpResult.Html, "共", "页，"), "/", "").ToInt(0);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='ctl00_ContentPlaceHolder1_GridView1']/tr", "inner");
                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 7)
                        {
                            continue;
                        }
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.PayTime = tdRow[0];
                        detailRes.SocialInsuranceTime = tdRow[0];
                        detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                        detailRes.PensionAmount = tdRow[3].ToDecimal(0);
                        detailRes.PaymentType = tdRow[5] != "正常应缴" ? tdRow[5] : ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = tdRow[6] == "已实缴" && detailRes.PaymentType == ServiceConsts.SocialSecurity_PaymentType_Normal ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                        Res.Details.Add(detailRes);
                        //PaymentMonths++;
                    }
                    pageIndex++;

                }
                while (pageIndex <= pageCount);
                #endregion

                //Res.PaymentMonths = PaymentMonths;
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
