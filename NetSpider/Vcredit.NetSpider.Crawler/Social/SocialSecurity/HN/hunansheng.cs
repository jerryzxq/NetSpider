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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.HN
{
    public class hunansheng : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.hn12333.com:81/";
        public  string socialCity = "hn_hunansheng";
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
                Url = baseUrl + "ylcxwz/bsjylbxgrzh_query.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "image";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl + "ylcxwz/bsjylbxgrzh_query.jsp",
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
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录

                Url = baseUrl + "ylcxwz/bsjylbxgrzh_check.jsp";
                postdata = String.Format("sfzhm={0}&xm={1}&mm={2}&yzm={3}", socialReq.Identitycard, socialReq.Name.ToUrlEncode(Encoding.GetEncoding("gbk")), socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "ylcxwz/bsjylbxgrzh_query.jsp",
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
                string err = CommonFun.GetMidStr(httpResult.Html.ToTrim(), "alert(\"", "\");");
                if (!err.IsEmpty())
                {
                    Res.StatusDescription = err;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                err = CommonFun.GetMidStr(httpResult.Html.ToTrim(), "alert('", "');");
                if (!err.IsEmpty())
                {
                    Res.StatusDescription = err;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取基本信息

                #region 校验

                Url = baseUrl + "ZzjbqkcxServlet";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "ylcxwz/qyylcx_zzjf.jsp",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies


                #endregion

                #region 取基本信息

                Url = baseUrl + "/ylcxwz/qyylcx_zz.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "ylcxwz/qyylcx_zzjf.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@bgcolor='#5395B5']/tr/td", "inner");
                if (results.Count <= 0 || string.IsNullOrEmpty(CommonFun.GetMidStr(results[3], "", "<a")))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                Res.EmployeeNo = CommonFun.GetMidStr(results[3], "", "<a").Trim().Replace("&nbsp;", "");//编号
                Res.Name = results[5].Trim().Replace("&nbsp;", "");//姓名
                Res.BirthDate = results[9].Trim().Replace("&nbsp;", "");//出生日期
                Res.Sex = results[7].Trim().Replace("&nbsp;", "");//性别
                Res.IdentityCard = results[1].Trim().Replace("&nbsp;", "");//身份证号
                Res.CompanyName = results[21].Trim().Replace("&nbsp;", "");//单位名称
                Res.WorkDate = results[11].Trim().Replace("&nbsp;", "");//参加工作时间

                #endregion

                #endregion

                #region 查询明细

                int pageIndex = 1;
                int pageCount = 0;

                do
                {

                    #region 校验

                    Url = String.Format("{0}ZcjfxxcxServlet?pagesize={1}&", baseUrl, pageIndex);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Referer = baseUrl + "ylcxwz/qyylcx_zzjf.jsp",
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
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies


                    #endregion

                    #region 取明细

                    Url = baseUrl + "ylcxwz/qyylcx_zzjf.jsp";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Referer = baseUrl + "ylcxwz/qyylcx_zzjf.jsp",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    pageCount = CommonFun.GetMidStr(CommonFun.GetMidStr(httpResult.Html, "当前页", "&nbsp;&nbsp;共"), "/", "").ToInt(0);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@align='center']", "inner");
                    results = HtmlParser.GetResultFromParser(results[5], "//tr", "inner");
                    results.RemoveAt(0);
                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 7)
                            continue;
                        if (Res.SocialInsuranceBase == 0)
                            Res.SocialInsuranceBase = tdRow[5].ToDecimal(0);//社保基数

                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.PayTime = tdRow[0];
                        detailRes.SocialInsuranceTime = tdRow[0];
                        detailRes.SocialInsuranceBase = tdRow[5].ToDecimal(0);
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = tdRow[2] != "已缴" ? tdRow[2] : ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detailRes.PensionAmount = tdRow[6].ToDecimal(0);
                        Res.Details.Add(detailRes);

                        if (detailRes.PaymentFlag != "断保")
                            PaymentMonths++;
                    }

                    #endregion

                    pageIndex++;

                }
                while (pageIndex <= pageCount);

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
