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
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class AH : ChinaMobile
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

        //string Url = string.Empty;
        //string postdata = string.Empty;
        //List<string> results = new List<string>();
        //#endregion

        ////https://ah.ac.10086.cn/login
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
        //        Url = "https://ah.ac.10086.cn/login";
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
        //        string spid = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='spid']", "value")[0];

        //        Url = "https://ah.ac.10086.cn/common/image.jsp?l=0.19269580154904942";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            ResultType = ResultType.Byte,
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        Url = "http://ah.ac.10086.cn/common/image10.jsp?l=0.8303377508123336";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            ResultType = ResultType.Byte,
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
        //        //保存验证码图片在本地
        //        FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
        //        Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.nextProCode = ServiceConsts.NextProCode_Two;

        //        Res.StatusDescription = "安徽移动初始化完成";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;

        //        Dictionary<string, object> dics = new Dictionary<string, object>();
        //        dics.Add("spid", spid);
        //        dics.Add("cookie", cookies);
        //        SpiderCacheHelper.SetCache(token, dics);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "安徽移动初始化异常";
        //        Log4netAdapter.WriteError("安徽移动初始化异常", e);
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
        //    Res.Token = mobileReq.Token;

        //    try
        //    {
        //        Dictionary<string, object> dics = new Dictionary<string, object>();
        //        string spid = string.Empty;
        //        //获取缓存
        //        if (SpiderCacheHelper.GetCache(mobileReq.Token) != null)
        //        {
        //            dics = (Dictionary<string, object>)SpiderCacheHelper.GetCache(mobileReq.Token);
        //            cookies = (CookieCollection)dics["cookie"];
        //            spid = dics["spid"].ToString();
        //        }

        //        //校验参数
        //        if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
        //        {
        //            Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        //校验验证码
        //        Url = "https://ah.ac.10086.cn/validImageCode?r_0.6067561629729676&imageCode=" + mobileReq.Vercode;
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Referer = "https://ah.ac.10086.cn/login",
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
        //        if (CommonFun.ClearFlag(httpResult.Html) != "1")
        //        {
        //            Res.StatusDescription = "验证码不正确";
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        //登录
        //        Url = "https://ah.ac.10086.cn/Login";
        //        postdata = string.Format("type=B&formertype=A&backurl=https%3A%2F%2Fah.ac.10086.cn%2F4login%2FbackPage.jsp&errorurl=https%3A%2F%2Fah.ac.10086.cn%2F4login%2FerrorPage.jsp&spid={3}&RelayState=type%3DA%3Bbackurl%3Dhttp%3A%2F%2Fservice.ah.10086.cn%2FLoginSso%3Bnl%3D3%3BloginFrom%3Dhttp%3A%2F%2Fservice.ah.10086.cn%2FLoginSso&mobileNum={0}&login_type_ah=&login_pwd_type=2&loginBackurl=&timestamp={4}&validCode_state=true&loginType=0&servicePassword={1}&servicePassword_1=&smsValidCode=&validCode={2}", mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode, spid, CommonFun.GetTimeStamp());
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
        //            Referer = "https://ah.ac.10086.cn/login",
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
        //        if (httpResult.Html.Contains("if(\"4001\" == \"3009\"){"))
        //        {
        //            Res.StatusDescription = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\");");
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value");
        //        if (results.Count == 0)
        //        {
        //            Res.StatusDescription = "SAMLart值为空";
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        string SAMLart = results[0];

        //        //登录后跳转页面
        //        Url = "https://ah.ac.10086.cn/4login/backPage.jsp";
        //        postdata = string.Format("SAMLart={0}&isEncodePassword=1&displayPic=1&RelayState=type%3DA%3Bbackurl%3Dhttp%253A%252F%252Fservice.ah.10086.cn%252FLoginSso%3Bnl%3D3%3BloginFrom%3Dhttp%3A%2F%2Fservice.ah.10086.cn%2FLoginSso&displayPics=", SAMLart);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
        //            Referer = "https://ah.ac.10086.cn/login",
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
        //        if (results.Count == 0)
        //        {
        //            Res.StatusDescription = "SAMLart值为空";
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        SAMLart = results[0];

        //        //登录后跳转页面
        //        Url = "http://service.ah.10086.cn/LoginSso";
        //        postdata = string.Format("SAMLart={0}&RelayState=type%3DA%3Bbackurl%3Dhttp%3A%2F%2Fservice.ah.10086.cn%2FLoginSso%3Bnl%3D3%3BloginFrom%3Dhttp%3A%2F%2Fservice.ah.10086.cn%2FLoginSso", SAMLart);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
        //            Referer = "https://ah.ac.10086.cn/login",
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
        //        if (httpResult.Html.Contains("http://service.ah.10086.cn/index.html"))
        //        {
        //            Res.StatusDescription = "登录成功";
        //            Res.StatusCode = ServiceConsts.StatusCode_success;
        //        }
        //        //抓取数据
        //        CrawlerMobileInfo(cookies, mobileReq);

        //        Res.nextProCode = ServiceConsts.NextProCode_SendSMS;
        //        Res.Token = mobileReq.Token;
        //        CacheHelper.SetCache(mobileReq.Token, cookies);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "上海移动登录异常";
        //        Log4netAdapter.WriteError("上海移动登录异常", e);
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
        //    return null;
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

        //#region
        //private void CrawlerMobileInfo(CookieCollection cookies, MobileReq mobileReq)
        //{
        //    try
        //    {
        //        //查询套餐
        //        Url = "http://service.ah.10086.cn/common/pageInfoInit?kind=200011523&f=200021511&url=%2Fpub-page%2Fbusi%2FcreditGradeIndex.html&_=1442812012308";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            ContentType = "application/json",
        //            Referer = "http://service.ah.10086.cn/pub-page/busi/creditGradeIndex.html?f=200021511&kind=200011523&area=cd",
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        for (int i = 0; i < 6; i++)
        //        {
        //            //获取账单
        //            Url = "http://service.ah.10086.cn/qry/qryMonthBillIndex?beginDate=" + DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString7);
        //            httpItem = new HttpItem()
        //            {
        //                URL = Url,
        //                ContentType = "application/json",
        //                Referer = "http://service.ah.10086.cn/pub-page/qry/qryMonthBill/qryMonthBillIndex.html?kind=200011522&f=200011536&area=cd",
        //                CookieCollection = cookies,
        //                ResultCookieType = ResultCookieType.CookieCollection
        //            };
        //            httpResult = httpHelper.GetHtml(httpItem);
        //            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //            var query = jsonParser.DeserializeObject<AH_Bill>(httpResult.Html);
        //        }

        //    }
        //    catch (Exception e)
        //    { }
        //}
        //#endregion
    }

    //class AH_Bill
    //{
    //    public string retMsg { get; set; }
    //    public List<AH_AccountDetail> list_AccountDetail { get; set; }
    //}
    //class AH_AccountDetail
    //{
    //    /// <summary>
    //    /// 账户项目
    //    /// </summary>
    //    public string accounts { get; set; }
    //    /// <summary>
    //    /// 本期末余额
    //    /// </summary>
    //    public string balance { get; set; }
    //    /// <summary>
    //    /// 消费支出
    //    /// </summary>
    //    public string consume { get; set; }
    //    /// <summary>
    //    /// 本期可用
    //    /// </summary>
    //    public string currAble { get; set; }
    //}
}
