using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.Mobile;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.DataAccess.Mongo;
namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class GZ : ChinaMobile
    {
        //#region 公共变量
        //IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        //IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        //IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        //CookieCollection cookies = new CookieCollection();
        //HttpHelper httpHelper = new HttpHelper();
        //HttpResult httpResult = null;
        //HttpItem httpItem = null;
        //MobileMongo mobileMongo = new MobileMongo();
        //#endregion

        //#region 私有变量
        //string ai_param_loginIndex = string.Empty;
        //string ai_param_loginTypes = string.Empty;
        //string filter_rule = string.Empty;
        //string appId = string.Empty;
        //string csrfToken = string.Empty;
        ////string SAMLart = string.Empty;
        //#endregion

        ///// <summary>
        ///// 页面初始化
        ///// </summary>
        ///// <returns></returns>
        //public VerCodeRes MobileInit(MobileReq mobileReq = null)
        //{
        //    VerCodeRes Res = new VerCodeRes();
        //    string Url = string.Empty;
        //    List<string> results = new List<string>();
        //    string token = CommonFun.GetGuidID();
        //    Res.Token = token;
        //    try
        //    {
        //        Url = "https://gz.ac.10086.cn/login";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        ai_param_loginIndex = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='ai_param_loginIndex']", "value")[0];
        //        ai_param_loginTypes = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='ai_param_loginTypes']", "value")[0];
        //        filter_rule = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='filter_rule']", "value")[0];
        //        appId = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='appId']", "value")[0];
        //        //errorurl = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='errorurl']", "value")[0];

        //        Url = "https://gz.ac.10086.cn/aicas/createVerifyImageServlet?1442554097759";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            ResultType = ResultType.Byte,
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
        //        Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
        //        //保存验证码图片在本地
        //        FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
        //        Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.nextProCode = ServiceConsts.NextProCode_Two;

        //        Res.StatusDescription = "贵州移动初始化完成";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;

        //        CacheHelper.SetCache(token, cookies);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "贵州移动初始化异常";
        //        Log4netAdapter.WriteError("贵州移动初始化异常", e);
        //    }
        //    return Res;
        //}
        ///// <summary>
        ///// 登录
        ///// </summary>
        ///// <param name="mobileReq"></param>
        ///// <returns></returns>
        //public BaseRes MobileLogin(MobileReq mobileReq)
        //{
        //    BaseRes Res = new BaseRes();
        //    string Url = string.Empty;
        //    string postdata = string.Empty;
        //    string passwordType = string.Empty;
        //    string errorMsg = string.Empty;
        //    string errFlag = string.Empty;
        //    string telNum = string.Empty;
        //    List<string> results = new List<string>();
        //    try
        //    {
        //        //获取缓存
        //        if (CacheHelper.GetCache(mobileReq.Token) != null)
        //        {
        //            cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
        //            //CacheHelper.RemoveCache(token);
        //        }
        //        //校验参数
        //        if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
        //        {
        //            Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        string pwdchange = CommonFun.GetEncodeBase64(Encoding.Default, mobileReq.Password);

        //        Url = "https://gz.ac.10086.cn/aicas/login";
        //        postdata = string.Format("service=null&lt=null&ai_param_loginIndex={0}&ai_param_loginTypes={1}&filter_rule={2}&appId={3}&VERIFY_CODE_FLAG=0&ENCRYPT_FLAG=1&forceLogoutUrl=null&username={4}&loginType=1&password={5}&rndPassword=&verifyCode={6}", ai_param_loginIndex, ai_param_loginTypes, filter_rule, appId, mobileReq.Mobile, pwdchange, mobileReq.Vercode);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
        //            Referer = "https://gz.ac.10086.cn/login",
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
        //        string errorcode = CommonFun.GetMidStr(httpResult.Html, "META name=\"WT.failType\" content=\"", "\">");
        //        if (errorcode != "")
        //        {
        //            Res.StatusDescription = errorcode;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }

        //        string ticket = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='ticket']", "value")[0];
        //        string host = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='host']", "value")[0];

        //        Url = "http://www.gz.10086.cn/my";
        //        postdata = string.Format("ticket={0}&host={1}", ticket, host);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
        //            Allowautoredirect = false,
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        Url = "http://www.gz.10086.cn/my/";
        //        postdata = string.Format("ticket={0}&host={1}", ticket, host);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        Url = "https://gz.ac.10086.cn/login?service=http%3A%2F%2Fwww.gz.10086.cn%2FpmsV4-web%2Fpms%2FshowCasLogin2.do&ai_param_loginTypes=2,1,3&username=&ai_param_loginIndex=6";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            CookieCollection = cookies,
        //            Referer = "http://www.gz.10086.cn/my/",
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        ticket = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='ticket']", "value")[0];
        //        host = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='host']", "value")[0];

        //        Url = "http://www.gz.10086.cn/pmsV4-web/pms/showCasLogin2.do";
        //        postdata = string.Format("ticket={0}&host={1}", ticket, host);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        Url = "http://www.gz.10086.cn/my/";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            CookieCollection = cookies,
        //            Referer = "http://www.gz.10086.cn/pmsV4-web/pms/showCasLogin2.do",
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        //Url = "http://www.gz.10086.cn/pmsV4-web/pms/check.do";
        //        //httpItem = new HttpItem()
        //        //{
        //        //    URL = Url,
        //        //    CookieCollection = cookies,
        //        //    Referer = "http://www.gz.10086.cn/my/",
        //        //    ResultCookieType = ResultCookieType.CookieCollection
        //        //};
        //        //httpResult = httpHelper.GetHtml(httpItem);
        //        //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        Url = "http://www.gz.10086.cn/pmsV4-web/pms/checkSession.do";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Referer = "http://www.gz.10086.cn/my/",
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
        //        if (httpResult.Html.Contains("未登录"))
        //        {
        //            Res.StatusDescription = "登录失败";
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }

        //        //获取个人信息
        //        Url = "http://www.gz.10086.cn/pmsV4-web/pms/userInfo.do";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Referer = "http://www.gz.10086.cn/my/",
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        Res.StatusDescription = "登录成功";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.nextProCode = ServiceConsts.NextProCode_Three;
        //        CacheHelper.SetCache(mobileReq.Token, cookies);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "贵州移动登录异常";
        //        Log4netAdapter.WriteError("贵州移动登录异常", e);
        //    }
        //    return Res;
        //}

        ///// <summary>
        ///// 发送短信验证码
        ///// </summary>
        ///// <param name="mobileReq"></param>
        ///// <returns></returns>
        //public VerCodeRes MobileSendSms(MobileReq mobileReq)
        //{
        //    return null;
        //}

        ///// <summary>
        ///// 校验短信验证码
        ///// </summary>
        ///// <param name="mobileReq"></param>
        ///// <returns></returns>
        //public BaseRes MobileCheckSms(MobileReq mobileReq)
        //{
        //    return null;
        //}

        ///// <summary>
        ///// 保存抓取的账单
        ///// </summary>
        ///// <param name="mobileReq"></param>
        ///// <returns></returns>
        //public BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        //{
        //    BaseRes Res = new BaseRes();
        //    Basic mobile = new Basic();
        //    Call call = null;
        //    Net gprs = null;
        //    Sms sms = null;
        //    MonthBill bill = null;//当前月份不能查询
        //    string Url = string.Empty;
        //    string postdata = string.Empty;
        //    DateTime date = DateTime.Now;
        //    List<string> results = new List<string>();
        //    List<string[]> deatils = new List<string[]>();
        //    try
        //    {
        //        if (CacheHelper.GetCache(mobileReq.Token) != null)
        //        {
        //            cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
        //        }
        //        Url = "http://www.gz.10086.cn/service/fee/jfcx.jsp?service=page/feeServiceList&listener=initPage&csrfToken=67AEED167E96FC&NAVTYPE=1&ID=948";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Referer = "http://www.gz.10086.cn/my/",
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        Url = "http://www.gz.10086.cn/service/fee/jfcx.jsp?service=page/fee.QueryMonthBill&listener=initPage&csrfToken=B5D53850B3D136&ID=4047";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Referer = "http://www.gz.10086.cn/service/fee/jfcx.jsp?service=page/feeServiceList&listener=initPage&csrfToken=67AEED167E96FC&NAVTYPE=1&ID=948",
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        for (int i = 1; i < 6; i++)
        //        {
        //            string YearMonth = DateTime.Now.AddMonths(-i).ToString("yyyyMM");
        //            Url = "http://www.gz.10086.cn/service/app?service=ajaxDirect/1/fee.QueryMonthBill/fee.QueryMonthBill/javascript/&pagename=fee.QueryMonthBill&eventname=query&&ID=4047&csrfToken=E52668A0E3266A&partids=&ajaxSubmitType=post&ajax_randomcode=0.566792561716093&autoType=false";
        //            postdata = "select_month=" + YearMonth;
        //            httpItem = new HttpItem()
        //            {
        //                URL = Url,
        //                Method = "POST",
        //                Postdata = postdata,
        //                Referer = "http://www.gz.10086.cn/service/fee/jfcx.jsp?service=page/fee.QueryMonthBill&listener=initPage&csrfToken=B5D53850B3D136&ID=4047",
        //                CookieCollection = cookies,
        //                ResultCookieType = ResultCookieType.CookieCollection
        //            };
        //            httpResult = httpHelper.GetHtml(httpItem);
        //            if (httpResult.StatusCode != HttpStatusCode.OK)
        //            {
        //                Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //                Res.StatusCode = ServiceConsts.StatusCode_fail;
        //                return Res;
        //            }
        //            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusDescription = "贵州移动手机账单抓取异常";
        //        Res.StatusCode = ServiceConsts.StatusCode_fail;
        //        Log4netAdapter.WriteError("贵州移动手机账单抓取异常", e);
        //    }
        //    return Res;
        //}

        ///// <summary>
        ///// 解析抓取的原始数据
        ///// </summary>
        ///// <param name="mobileReq"></param>
        ///// <param name="crawlerDate"></param>
        ///// <returns></returns>
        //public BaseRes MobileAnalysis(MobileReq mobileReq, DateTime appDate)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
