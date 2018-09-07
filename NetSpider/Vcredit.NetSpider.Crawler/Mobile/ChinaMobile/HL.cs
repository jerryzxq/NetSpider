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
    public class HL : ChinaMobile
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
        //string uid = string.Empty;
        //string service = string.Empty;
        //string style = string.Empty;
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
        //        Url = "https://hl.ac.10086.cn/login";
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
        //        Url = "https://hl.ac.10086.cn/SSO/img";
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

        //        Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
        //        //保存验证码图片在本地
        //        FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
        //        Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.nextProCode = ServiceConsts.NextProCode_Two;

        //        Res.StatusDescription = "黑龙江移动初始化完成";

        //        CacheHelper.SetCache(token, cookies);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "黑龙江移动初始化异常";
        //        Log4netAdapter.WriteError("黑龙江移动初始化异常", e);
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

        //        //登录
        //        Url = "https://hl.ac.10086.cn/login";
        //        postdata = string.Format("service=newportal&continue=&style=portal&submitMode=login&goto=&fromCode=sso&rememberNum=false&getReadtime=30&username={0}&passwordType=2&pType=2&password={1}&smsRandomCode=&validateCode={2}&verifyno=&on=", mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
        //            Referer = "https://hl.ac.10086.cn/login",
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
               

        //        if (HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='uid']", "value").Count > 0)
        //        {
        //            uid = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='uid']", "value")[0];
        //        }

        //        if (HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='service']", "value").Count > 0)
        //        {
        //            service = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='service']", "value")[0];
        //        }

        //        if (HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='style']", "value").Count > 0)
        //        {
        //            style = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='style']", "value")[0];
        //        }

        //        //验证是否重复登录
        //        if (!string.IsNullOrEmpty(uid))
        //        {
        //            Url = "https://hl.ac.10086.cn/SSO/clearAndLogin";
        //            postdata = string.Format("contextName=&uid={0}&passwordType=2&service={1}&continue=&style={2}", uid, service, style);
        //            httpItem = new HttpItem()
        //            {
        //                URL = Url,
        //                Method = "POST",
        //                Postdata = postdata,
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

        //        if (HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value").Count > 0)
        //        {
        //            SAMLart = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value")[0];
        //        }

        //        if (HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value").Count > 0)
        //        {
        //            RelayState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value")[0];
        //        }

        //        if (HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='PasswordType']", "value").Count > 0)
        //        {
        //            passwordType = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='PasswordType']", "value")[0];
        //        }

        //        Url = "http://hl.10086.cn/login/sso/callBack";
        //        postdata = string.Format("RelayState={0}&SAMLart={1}&PasswordType={2}", RelayState, SAMLart, passwordType);
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

              

        //        Url = "http://hl.10086.cn/resource/pub-page/login/my.html";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

               
        //        Res.StatusDescription = "登录成功";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.nextProCode = ServiceConsts.NextProCode_Three;
               
               
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "黑龙江移动登录异常";
        //        Log4netAdapter.WriteError("黑龙江移动登录异常", e);
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
        //    List<JObject> results = new List<JObject>();
        //    List<string> infos = new List<string>();


        //    #region 月消费情况
        //    for (int i = 0; i < 6; i++)
        //    {
        //        Url = string.Format("http://hl.10086.cn/qry/bill/billQueryRes/se610WS?year_month={0}", date.AddMonths(-i).ToString("yyyyMM"));
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);

        //        var list = jsonParser.GetResultFromMultiNode(jsonParser.GetResultFromMultiNode(httpResult.Html, "data", ':'), "item_money",':');
        //        var begindate = jsonParser.GetResultFromMultiNode(jsonParser.GetResultFromMultiNode(httpResult.Html, "data", ':'), "begin_date", ':');
        //        var enddate = jsonParser.GetResultFromMultiNode(jsonParser.GetResultFromMultiNode(httpResult.Html, "data", ':'), "end_date", ':');
        //        if (list.Split(',').Length > 1)
        //        {
        //            bill = new MonthBill();
        //            bill.TotalAmt = list.Split(',')[5].Replace("\r","").Replace("\n","").Replace("\"","").TrimStart(' ',' ');
        //            bill.BillCycle = date.AddMonths(-i).ToString(Consts.DateFormatString12);
        //            mobile.BillList.Add(bill);
        //        }
               
        //    }
        //    #endregion

          
           
        //    Url = "http://hl.10086.cn/busi/pkg/mainpkgchange/index";
        //    httpItem = new HttpItem()
        //    {
        //        URL = Url,
        //        CookieCollection = cookies,
        //        ResultCookieType = ResultCookieType.CookieCollection
        //    };
        //    httpResult = httpHelper.GetHtml(httpItem);
        //    //套餐
        //    mobile.Package = jsonParser.GetResultFromMultiNode(jsonParser.GetResultFromMultiNode(httpResult.Html, "data", ':'), "oldFeeName", ':');
        //    //品牌
        //    mobile.PackageBrand = jsonParser.GetResultFromMultiNode(jsonParser.GetResultFromMultiNode(httpResult.Html, "data", ':'), "brandName", ':');

         


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
