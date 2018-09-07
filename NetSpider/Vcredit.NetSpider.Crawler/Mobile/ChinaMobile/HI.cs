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
using Newtonsoft.Json.Linq;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class HI : ChinaMobile
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
        //string loginType = string.Empty;
        //string backurl = string.Empty;
        //string errorurl = string.Empty;
        //string spid = string.Empty;
        //string RelayState = string.Empty;

        //string SAMLart = string.Empty;
        //string displayPic = string.Empty;
        //string SAMLRequest = string.Empty;
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

        //        Url = "https://hi.ac.10086.cn/login";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            CookieCollection = cookies,
        //            Referer = "http://www.10086.cn/hi/index_898_898.html",
        //            UserAgent = "Mozilla/5.0 (compatible;Windows NT 6.1; WOW64;Trident/6.0;MSIE 9.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.27 Safari/537.36",
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
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='oldLogin']/input[@type='hidden']", "value");
        //        if (results.Count > 0)
        //        {
        //            loginType = results[0];
        //            backurl = results[1];
        //            errorurl = results[2];
        //            spid = results[3];
        //            RelayState = results[4];
        //        }

        //        Url = "https://hi.ac.10086.cn/sso3/common/image.jsp";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            ResultType = ResultType.Byte,
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
        //        Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
        //        //保存验证码图片在本地
        //        FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
        //        Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.nextProCode = ServiceConsts.NextProCode_Two;

        //        Res.StatusDescription = "海南移动初始化完成";

        //        CacheHelper.SetCache(token, cookies);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "海南移动初始化异常";
        //        Log4netAdapter.WriteError("海南移动初始化异常", e);
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


        //        Url = "https://hi.ac.10086.cn/sso3/Login";
        //        postdata = string.Format("type={0}&backurl={1}&errorurl={2}&spid={3}&RelayState={4}&mobileNum={5}&Password-type=&nickName=&email=&servicePassword={6}&Password=&emailPwd=&ssoPwd=&website_pwd_email=&smsValidCode=&validCode=0000", loginType, backurl.ToUrlEncode(), errorurl.ToUrlEncode(), spid, RelayState.ToUrlEncode(), mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode);
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
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@name='postartifact']/input[@type='hidden']", "value");
        //        if (results.Count > 0)
        //        {
        //            SAMLart = results[0];
        //            displayPic = results[1];
        //            RelayState = results[2];
        //        }

        //        #region 域名注册
        //        Url = "https://hi.ac.10086.cn/sso3/4login/backPage.jsp";
        //        postdata = string.Format("SAMLart={0}&displayPic={1}&RelayState={2}", SAMLart, displayPic, RelayState.ToUrlEncode());
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
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


        //        Url = "http://www.hi.10086.cn/my/";
        //        postdata = string.Format("SAMLart={0}&weaktype=&loginmodelparam=&RelayState={1}", SAMLart, RelayState.ToUrlEncode());
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
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
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@name='postartifact']/input[@type='hidden']", "value");
        //        if (results.Count > 0)
        //        {
        //            SAMLRequest = results[0];
        //            RelayState = results[1];
        //        }

        //        Url = "http://hi.ac.10086.cn/sso3/POST";
        //        postdata = string.Format("SAMLRequest={0}&RelayState={1}", SAMLRequest.ToUrlEncode(), RelayState.ToUrlEncode());
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
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
        //        #endregion

        //        Url = "http://www.hi.10086.cn/service/mainQuery.do";
        //        postdata = string.Format("SAMLart={0}&displayPic={1}&RelayState={2}", SAMLart, displayPic, RelayState.ToUrlEncode());
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
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
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@class='green fontb']", "inner");
        //        if (results.Count < 0)
        //        {
        //            Res.StatusCode = ServiceConsts.StatusCode_error;
        //            Res.StatusDescription = "海南移动登录失败";
        //            return Res;
        //        }

        //        Res.StatusDescription = "登录成功";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.nextProCode = ServiceConsts.NextProCode_Three;
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "海南移动登录异常";
        //        Log4netAdapter.WriteError("海南移动登录异常", e);
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
        //    MonthBill bill = null;
        //    string Url = string.Empty;
        //    string postdata = string.Empty;
        //    DateTime date = DateTime.Now;
        //    string infos = string.Empty;
        //    List<string> results = new List<string>();

        //    #region 基本信息

        //    //用户资料
        //    Url = "http://www.hi.10086.cn/service/mainQuery.do";
        //    httpItem = new HttpItem()
        //    {
        //        URL = Url,
        //        CookieCollection = cookies,
        //        ResultCookieType = ResultCookieType.CookieCollection
        //    };
        //    httpResult = httpHelper.GetHtml(httpItem);
        //    if (httpResult.StatusCode != HttpStatusCode.OK)
        //    {
        //        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //        Res.StatusCode = ServiceConsts.StatusCode_fail;
        //        return Res;
        //    }
        //    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='my_message']/p/span[@class='green fontb']", "inner");
        //    if (results.Count > 0)
        //    {
        //        mobile.Name = results[0];    //姓名
        //    }

        //    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='my_message']/p[@class='h30 ml10'][1]", "inner");
        //    if (results.Count > 0)
        //    {
        //        mobile.PackageBrand = results[0].Split('[')[1].Replace("]","");    //品牌
        //    }

        //    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='my_message']/p/span[@class='fl']", "inner");
        //    if (results.Count > 0)
        //    {
        //        mobile.Regdate = results[1].Split('：')[1];    //入网时间
        //    }

        //    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='my_message']/p/span[@class='ml20']", "inner");
        //    if (results.Count > 0)
        //    {
        //        mobile.Integral = results[1].Split('：')[1];    //积分
        //    }

        //    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='my_tcylcx']/table/tr[2]/td[1]", "inner");
        //    if (results.Count > 0)
        //    {
        //        mobile.Package = results[0];    //套餐
        //    }



        //    //PUK码
        //    Url = "http://www.hi.10086.cn/service/ocs/queryPUK.do";
        //    httpItem = new HttpItem()
        //    {
        //        URL = Url,
        //        CookieCollection = cookies,
        //        ResultCookieType = ResultCookieType.CookieCollection
        //    };
        //    httpResult = httpHelper.GetHtml(httpItem);
        //    if (httpResult.StatusCode != HttpStatusCode.OK)
        //    {
        //        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //        Res.StatusCode = ServiceConsts.StatusCode_fail;
        //        return Res;
        //    }
        //    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
        //    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='ser_buyNumber_query']/table/tr[2]/td[2]/b", "inner");
        //    if (results.Count>0)
        //    {
        //        mobile.PUK = results[0];
        //    }



        //    //月账单查询
        //    for (int i = 0; i < 6; i++)
        //    {

        //        Url = "http://www.hi.10086.cn/service/bill/billNewQuery.do";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method="Post",
        //            Postdata="month="+date.AddMonths(-i).ToString("yyyyMM"),
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tabConxx'][2]", "inner");
        //        if (results.Count > 0)
        //        {
        //            bill = new MonthBill();
        //            bill.BillCycle = date.AddMonths(-i).ToString(Consts.DateFormatString12);
        //            var totalAmt = HtmlParser.GetResultFromParser(results[0], "//tr[3]/td[2]", "inner");
        //            if (totalAmt.Count>0)
        //            {
        //                bill.TotalAmt = totalAmt[0].Split('￥')[1];
        //            }
        //            var planAmt = HtmlParser.GetResultFromParser(results[0], "//tr[13]/td[1]/b", "inner");
        //            if (planAmt.Count > 0)
        //            {
        //                bill.PlanAmt = planAmt[0].Split('￥')[1].Replace(")","");
        //            }
        //            mobile.BillList.Add(bill);
        //        }

        //    }

        //    #endregion


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
