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
    public class FJ : ChinaMobile
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
        //string spid = string.Empty;
        //string RelayState = string.Empty;
        //string backurl = string.Empty;
        //string errorurl = string.Empty;
        //string SAMLart = string.Empty;

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
        //        Url = "https://fj.ac.10086.cn/login";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
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

        //        spid = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='spid']", "value")[0];
        //        RelayState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value")[0];
        //        backurl = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='backurl']", "value")[0];
        //        errorurl = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='errorurl']", "value")[0];

        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.nextProCode = ServiceConsts.NextProCode_Two;

        //        Res.StatusDescription = "福建移动初始化完成";

        //        CacheHelper.SetCache(token, cookies);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "福建移动初始化异常";
        //        Log4netAdapter.WriteError("福建移动初始化异常", e);
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
        //    Basic mobile = new Basic();
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
        //            CacheHelper.RemoveCache(mobileReq.Token);
        //        }
        //        //校验参数
        //        if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
        //        {
        //            Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }

        //        Url = "https://fj.ac.10086.cn/Login";
        //        postdata = string.Format("type=B&backurl={0}&errorurl={1}&spid={2}&RelayState={3}&mobileNum={4}&servicePassword={5}&smsValidCode=&validCode=0000&Password-type=&button={6}", backurl.ToUrlEncode(), errorurl.ToUrlEncode(), spid, RelayState.ToUrlEncode(), mobileReq.Mobile, MultiKeyDES.EncryptDES(mobileReq.Password, "YHXWWLKJYXGS", "ZFCHHYXFL10C", "DES"), "登  录");
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
        //            Referer = "https://fj.ac.10086.cn/login",
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
        //        SAMLart = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value")[0];
        //        RelayState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value")[0];

        //        Url = "https://fj.ac.10086.cn/4login/backPage.jsp";
        //        postdata = string.Format("SAMLart={0}&displayPic=1&RelayState={1}", SAMLart, RelayState);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
        //            Referer = "https://fj.ac.10086.cn/Login",
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

        //        Url = string.Format("http://www.fj.10086.cn/my/?SAMLart={0}&RelayState={1}", SAMLart, RelayState);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            CookieCollection = cookies,
        //            Allowautoredirect = false,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        Url = "http://www.fj.10086.cn/my";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            CookieCollection = cookies,
        //            Allowautoredirect = false,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        //主页面，能获取一些基本信息
        //        Url = "http://www.fj.10086.cn/my/";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
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
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='personal_bg']");

        //        if(results.Count > 0)
        //        {
        //            mobile.Package = HtmlParser.GetResultFromParser(results[0], "//p[@class='tac lineheight16 huise']")[0];
        //            mobile.StarLevel = HtmlParser.GetResultFromParser(results[0], "//span[@id='12580_id']")[0];
        //            //mobile.Integral = CommonFun.GetMidStr(httpResult.Html, "我的积分:", "积分");
        //        }

        //        Res.StatusDescription = "登录成功";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.nextProCode = ServiceConsts.NextProCode_Three;
        //        CacheHelper.SetCache(mobileReq.Token, cookies);
        //        Dictionary<string, object> dic = new Dictionary<string, object>();
        //        dic.Add("cookies", cookies);
        //        dic.Add(ServiceConsts.SpiderType_Mobile, mobile);
        //        CacheHelper.SetCache(mobileReq.Token, dic);
        //        //if (results.Count > 0)
        //        //{
                    
        //        //}
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "福建移动登录异常";
        //        Log4netAdapter.WriteError("福建移动登录异常", e);
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
        //    throw new Exception();
        //}

        ///// <summary>
        ///// 校验短信验证码
        ///// </summary>
        ///// <param name="mobileReq"></param>
        ///// <returns></returns>
        //public BaseRes MobileCheckSms(MobileReq mobileReq)
        //{
        //    throw new Exception();
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
        //    Dictionary<string, object> dic = null;
        //    try
        //    {
        //        if (CacheHelper.GetCache(mobileReq.Token) != null)
        //        {
        //            dic = (Dictionary<string, object>)CacheHelper.GetCache(mobileReq.Token);
        //            cookies = (CookieCollection)dic["cookies"];
        //            mobile = (Basic)dic[ServiceConsts.SpiderType_Mobile];
        //        }

        //        Url = "http://www.fj.10086.cn/my/fee/query/queryServiceYYFee.do";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
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

        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_box left']//li");
        //        bill = new MonthBill();
        //        foreach (string item in results)
        //        {
        //            if (item.Contains("套餐及固定费"))
        //                bill.PlanAmt = HtmlParser.GetResultFromParser(item, "span")[0];
        //            if (item.Contains("合计"))
        //                bill.TotalAmt = HtmlParser.GetResultFromParser(item, "span")[0];
        //        }
        //        bill.BillCycle = CommonFun.GetMidStr(httpResult.Html, "计费周期：", "</p>");
        //        mobile.BillList.Add(bill);

        //        for (int i = 2; i < 7; i++)
        //        {
        //            string YearMonth = DateTime.Now.AddMonths(-i).ToString("yyyyMM");
        //            string time = getTime();
        //            Url = string.Format("http://www.fj.10086.cn/my/fee/query/queryServiceFee.do?friendTel={1}&query_month={0}&search={1}", YearMonth, time);

        //            //问题：必须先将 ContentLength 字节写入请求流，然后再调用 [Begin]GetResponse
        //            //解决方法：原先的POST方法，将Postdata变为URL参数，使用GET方法获取结果
        //            httpItem = new HttpItem()
        //            {
        //                URL = Url,
        //                //Method = "POST",
        //                //Postdata = postdata,
        //                CookieCollection = cookies,
        //                Referer = "http://www.fj.10086.cn/my/fee/query/queryServiceYYFee.do",
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
        //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_box left']//li");
        //            bill = new MonthBill();
        //            foreach (string item in results)
        //            {
        //                if (item.Contains("套餐及固定费"))
        //                    bill.PlanAmt = HtmlParser.GetResultFromParser(item, "span")[0];
        //                if (item.Contains("合计"))
        //                    bill.TotalAmt = HtmlParser.GetResultFromParser(item, "span")[0];
        //            }
        //            bill.BillCycle = CommonFun.GetMidStr(httpResult.Html, "计费周期：", "</p>");
        //            mobile.BillList.Add(bill);
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusDescription = "福建移动手机账单抓取异常";
        //        Res.StatusCode = ServiceConsts.StatusCode_fail;
        //        Log4netAdapter.WriteError("福建移动手机账单抓取异常", e);
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

        //string getTime()
        //{
        //    string time = DateTime.Now.GetDateTimeFormats('r')[0].ToString().ToTrim(","); ;
        //    string[] time_each_list = time.Split(' ');
        //    time = time_each_list[0];
        //    for (int i = 1; i < time_each_list.Count(); i++)
        //    {
        //        if (i == 1)
        //        {
        //            time += (" " + time_each_list[2]);
        //        }
        //        else if (i == 2)
        //        {
        //            time += (" " + time_each_list[1]);
        //        }
        //        else
        //        {
        //            time += (" " + time_each_list[i]);
        //        }
        //    }

        //    return time + "+0800";
        //}
    }
}
