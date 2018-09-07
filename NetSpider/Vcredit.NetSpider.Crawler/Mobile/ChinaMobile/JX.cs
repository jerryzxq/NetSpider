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
    public class JX : ChinaMobile
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
        //string from = string.Empty;
        //string sid = string.Empty;
        //string type = string.Empty;
        //string backurl = string.Empty;
        //string errorurl = string.Empty;
        //string spid = string.Empty;
        //string RelayState = string.Empty;

        //string SAMLart = string.Empty;
        //string displayPic = string.Empty;

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
        //        Url = "https://jx.ac.10086.cn/login";
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
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='offical-user']/input[@type='hidden']", "value");
        //        if (results.Count > 0)
        //        {
        //            from = results[0];
        //            backurl = results[1];
        //            spid = results[2];
        //            errorurl = results[3];
        //            sid = results[4];
        //            RelayState = results[5];
        //            type = results[6];
        //        }

        //        Url = "https://jx.ac.10086.cn/common/image.jsp";
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

        //        Res.StatusDescription = "江西移动初始化完成";

        //        CacheHelper.SetCache(token, cookies);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "江西移动初始化异常";
        //        Log4netAdapter.WriteError("江西移动初始化异常", e);
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

        //        Url = "https://jx.ac.10086.cn/Login";
        //        postdata = string.Format("from={0}&sid={1}&type={2}&backurl={3}&errorurl={4}&spid={5}&RelayState={6}&mobileNum={7}&servicePassword={8}&smsValidCode=&validCode={9}", from, sid, type, backurl.ToUrlEncode(), errorurl.ToUrlEncode(), spid, RelayState, mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode);
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
        //            SAMLart = results[2];
        //            displayPic = results[0];
        //        }



        //        Url = "https://jx.ac.10086.cn/4login/backPage.jsp";
        //        postdata = string.Format("displayPics=&displayPic={0}&RelayState={1}&SAMLart={2}", displayPic, RelayState.ToUrlEncode(), SAMLart);
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


        //        Url = "http://www.jx.10086.cn/my/";
        //        postdata = string.Format("SAMLart={0}&RelayState={1}", SAMLart, RelayState);
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
        //        Res.StatusDescription = "登录成功";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.nextProCode = ServiceConsts.NextProCode_Three;
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "江西移动登录异常";
        //        Log4netAdapter.WriteError("江西移动登录异常", e);
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


        //    #region 基本信息

        //    //用户资料
        //    Url = "http://www.jx.10086.cn/my/queryXXNew2.do";
        //    httpItem = new HttpItem()
        //    {
        //        URL = Url,
        //        Method = "Post",
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
        //    mobile.Name = jsonParser.GetResultFromParser(httpResult.Html, "name");
        //    mobile.Postcode = jsonParser.GetResultFromParser(httpResult.Html, "post");
        //    mobile.Address = jsonParser.GetResultFromParser(httpResult.Html, "addr");
        //    mobile.Mobile = jsonParser.GetResultFromParser(httpResult.Html, "home");
        //    mobile.Regdate = jsonParser.GetResultFromParser(httpResult.Html, "cdate");
        //    mobile.Idcard = jsonParser.GetResultFromParser(httpResult.Html, "regn");
        //    //品牌
        //    mobile.PackageBrand = jsonParser.GetResultFromParser(httpResult.Html, "brand");
        //    //套餐
        //    mobile.Package = jsonParser.GetResultFromParser(httpResult.Html, "zffa");

        //    //积分
        //    Url = "http://www.jx.10086.cn/my/indexInit1New.do";
        //    httpItem = new HttpItem()
        //    {
        //        URL = Url,
        //        Method = "Post",
        //        CookieCollection = cookies,
        //        ResultCookieType = ResultCookieType.CookieCollection
        //    };
        //    httpResult = httpHelper.GetHtml(httpItem);
        //    mobile.Integral = jsonParser.GetResultFromParser(httpResult.Html, "nousefen");

        //    #region 域名注册
        //    Url = "http://www.jx.10086.cn/my/jx139/SRegSSOID.do";
        //    httpItem = new HttpItem()
        //    {
        //        URL = Url,
        //        Method = "Post",
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



        //    Url = "http://service.jx.10086.cn/service/queryMonthBillN.action?menuid=000200010002";
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
        //    var results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@name='postartifact']/input[@type='hidden']", "value");
        //    if (results.Count > 0)
        //    {
        //        SAMLart = results[0];
        //        displayPic = results[1];
        //    }

        //    Url = "https://jx.ac.10086.cn/POST";
        //    postdata = string.Format("SAMLRequest={0}&RelayState={1}", SAMLart.ToUrlEncode(), RelayState.ToUrlEncode());
        //    httpItem = new HttpItem()
        //    {
        //        URL = Url,
        //        Postdata = postdata,
        //        Method = "post",
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

        //    RelayState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value")[0];
        //    SAMLart = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value")[0];
        //    Url = "http://service.jx.10086.cn/service/queryCurrentScoreN.action";
        //    postdata = string.Format("displayPics=&displayPic=0&RelayState={0}&SAMLart={1}&menuid=000300060001", RelayState.ToUrlEncode(), SAMLart.ToUrlEncode());
        //    httpItem = new HttpItem()
        //    {
        //        URL = Url,
        //        Postdata = postdata,
        //        Method = "post",
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
        //    #endregion

        //    //PUK码
        //    Url = "http://service.jx.10086.cn/service/queryPUKCodeN.action?menuid=000200030005";
        //    httpItem = new HttpItem()
        //    {
        //        URL = Url,
        //        CookieCollection = cookies,
        //        ResultCookieType = ResultCookieType.CookieCollection
        //    };
        //    httpResult = httpHelper.GetHtml(httpItem);
        //    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='biaoge11']/table/tbody/tr/td", "inner");
        //    if (results.Count>0)
        //    {
        //        mobile.PUK = results[0];
        //    }

        //    //当月账单
        //    Url = "http://service.jx.10086.cn/service/queryMonthBillN.action?menuid=000200010002";
        //    httpItem = new HttpItem()
        //    {
        //        URL = Url,
        //        CookieCollection = cookies,
        //        ResultCookieType = ResultCookieType.CookieCollection
        //    };
        //    httpResult = httpHelper.GetHtml(httpItem);
        //    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='feeTab']", "inner");
        //    if (results.Count > 0)
        //    {
        //        bill = new MonthBill();
        //        bill.BillCycle = date.ToString(Consts.DateFormatString12);
        //        bill.TotalAmt = HtmlParser.GetResultFromParser(results[0], "/tbody/tr[2]/tr/td[2]/div", "")[0].Replace("\r\n", "").Replace("\t", "");  //月账单总计
        //        bill.PlanAmt = HtmlParser.GetResultFromParser(results[0], "/tbody/tr[3]/tr/td[2]/div", "")[0].Replace("\r\n", "").Replace("\t", ""); //月账单固定套餐费
        //        mobile.BillList.Add(bill);
        //    }

        //    //前五个月账单信息
        //    for (int i = 1; i < 6; i++)
        //    {

        //        Url = string.Format("http://service.jx.10086.cn/service/queryWebPageInfo.action?requestStartTime=&menuid=00890104&queryMonth={0}&s=0.42182053866992", date.AddMonths(-i).ToString("yyyyMM"));
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);

        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tableclo']/tr/td/div[@style='float: none; padding-left: 255px;']", "inner");
        //        if (results.Count > 0)
        //        {
        //            bill = new MonthBill();
        //            bill.BillCycle = date.AddMonths(-i).ToString(Consts.DateFormatString12);
        //            bill.TotalAmt = results[19].Replace("\r\n", "").Replace("\t", "").Split('￥')[1];
        //            bill.PlanAmt = results[0].Replace("\r\n", "").Replace("\t", "").Split('￥')[1];
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
