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
    public class GS : ChinaMobile
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
        //string timestamp = string.Empty;
        ////string spid = string.Empty;
        ////string RelayState = string.Empty;
        ////string backurl = string.Empty;
        ////string errorurl = string.Empty;
        ////string SAMLart = string.Empty;

        //string CookiesToCookieStr(CookieCollection cookies)
        //{
        //    string CookieStr = string.Empty;
        //    for (int i = 0; i < cookies.Count; i++)
        //    {

        //        CookieStr += cookies[i] + "; ";

        //    }
        //    return CookieStr;
        //}
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
        //        Url = "https://gs.ac.10086.cn/login";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            //Referer = "http://www.gs.10086.cn/service/index.html",
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
        //        timestamp = CommonFun.GetMidStr(httpResult.Html, "timestamp = ", " ;");
        //        cookies.Add(new Cookie("timestamp", timestamp, "/", "gs.ac.10086.cn"));

        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.nextProCode = ServiceConsts.NextProCode_Two;

        //        Res.StatusDescription = "甘肃移动初始化完成";

        //        CacheHelper.SetCache(token, cookies);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "甘肃移动初始化异常";
        //        Log4netAdapter.WriteError("甘肃移动初始化异常", e);
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
        //    string Url = string.Empty;
        //    string postdata = string.Empty;
        //    string passwordType = string.Empty;
        //    string errorMsg = string.Empty;
        //    string errFlag = string.Empty;
        //    string telNum = string.Empty;
        //    List<string> results = new List<string>();
        //    string CookieStr = string.Empty;
        //    try
        //    {
        //        //获取缓存
        //        if (CacheHelper.GetCache(mobileReq.Token) != null)
        //        {
        //            cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
        //            //CacheHelper.RemoveCache(mobileReq.Token);
        //        }
        //        //校验参数
        //        if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
        //        {
        //            Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        //timestamp = CommonFun.GetTimeStamp();
        //        //timestamp = "1442973954817";
                

        //        Url = "https://gs.ac.10086.cn/getNumMsg";
        //        postdata = "mobile=" + mobileReq.Mobile;
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
        //            Referer = "https://gs.ac.10086.cn/login",
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.Html != "1")
        //        {
        //            Res.StatusDescription = "手机号错误！";
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        Url = "https://gs.ac.10086.cn/getNumMsg";
        //        postdata = "mobile=" + mobileReq.Mobile;
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
        //            Referer = "https://gs.ac.10086.cn/login",
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.Html != "1")
        //        {
        //            Res.StatusDescription = "手机号错误！";
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        CookieStr = CookiesToCookieStr(cookies);
                

        //        Url = "https://gs.ac.10086.cn/popDoorPopLogonNew";
        //        postdata = string.Format("mobile={0}&password={1}&loginType=1&icode=&fromFlag=doorPage&isHasV=false&access=&redirectUrl=&timestamp={2}", mobileReq.Mobile, CommonFun.GetMd5Str(mobileReq.Password), timestamp);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
        //            Referer = "https://gs.ac.10086.cn/login",
        //            ResultCookieType = ResultCookieType.String
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (jsonParser.GetResultFromParser(httpResult.Html, "rcode") != "1000")
        //        {
        //            Res.StatusDescription = "登录失败！";
        //            Res.StatusCode = ServiceConsts.StatusCode_fail; 
        //            return Res;
        //        }
        //        //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
        //        //cookies = CommonFun.RemoveCookie(cookies, "timestamp");
        //        CookieStr += httpResult.Cookie;

        //        Url = "http://www.gs.10086.cn/service/index.html";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            //CookieCollection = cookies,
        //            //ResultCookieType = ResultCookieType.CookieCollection
        //            Cookie = CookieStr,
        //            ResultCookieType = ResultCookieType.String
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
        //        CookieStr += httpResult.Cookie;

        //        Res.StatusDescription = "登录成功";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.nextProCode = ServiceConsts.NextProCode_Three;
        //        CacheHelper.SetCache(mobileReq.Token, CookieStr);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "甘肃移动登录异常";
        //        Log4netAdapter.WriteError("甘肃移动登录异常", e);
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
        //    Res.Token = mobileReq.Token;
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
        //    string CookieStr = string.Empty;
        //    try
        //    {
        //        if (CacheHelper.GetCache(mobileReq.Token) != null)
        //        {
        //            //cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
        //            CookieStr = CacheHelper.GetCache(mobileReq.Token).ToString();
        //        }

        //        Url = "http://www.gs.10086.cn/service/my/myBill.html";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Referer = "http://www.gs.10086.cn/service/index.html",
        //            //CookieCollection = cookies,
        //            //ResultCookieType = ResultCookieType.CookieCollection
        //            Cookie = CookieStr,
        //            ResultCookieType = ResultCookieType.String
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
        //        CookieStr += httpResult.Cookie;

        //        Url = "http://www.gs.10086.cn/service/businessDeal.do";
        //        postdata = "jsonParam=%5B%7B%22dynamicParameter%22%3A%7B%22method%22%3A%22getUserInfo%22%2C%22handleNum%22%3A%22defaultHandle%22%2C%22applyNum%22%3A%22memberInfoCDS%22%7D%2C%22dynamicDataNodeName%22%3A%22loginCallBackActionNode%22%7D%5D";
        //        //postdata = string.Format(postdata, i == 0 ? "当月准实时".ToUrlEncode() : YearMonth);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
        //            Referer = "http://www.gs.10086.cn/service/my/myBill.html",
        //            //CookieCollection = cookies,
        //            //ResultCookieType = ResultCookieType.CookieCollection
        //            Cookie = CookieStr,
        //            ResultCookieType = ResultCookieType.String
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
        //        CookieStr += httpResult.Cookie;
        //        mobile.Integral = jsonParser.GetResultFromMultiNode(httpResult.Html, "loginCallBackActionNode:result:JF");

        //        for (int i = 0; i <6; i++)
        //        {
        //            string YearMonth = DateTime.Now.AddMonths(-i).ToString("yyyyMM");
        //            Url = "http://www.gs.10086.cn/service/businessDeal.do";
        //            postdata = "jsonParam=%5B%7B%22dynamicParameter%22%3A%7B%22method%22%3A%22queryBillInfo%22%2C%22handleNum%22%3A%22defaultHandle%22%2C%22applyNum%22%3A%22queryBillInfoCDS%22%2C%22monthnum%22%3A%22{0}%22%7D%2C%22dynamicDataNodeName%22%3A%22loginCallBackActionNode%22%7D%5D";
        //            postdata = i == 0 ? "jsonParam=%5B%7B%22dynamicParameter%22%3A%7B%22method%22%3A%22queryBillInfo%22%2C%22handleNum%22%3A%22defaultHandle%22%2C%22applyNum%22%3A%22queryBillInfoCDS%22%2C%22monthnum%22%3A%22%E5%BD%93%E6%9C%88%E5%87%86%E5%AE%9E%E6%97%B6%22%7D%2C%22dynamicDataNodeName%22%3A%22loginCallBackActionNode%22%7D%5D" : string.Format(postdata,YearMonth);
        //            string a = postdata.ToUrlDecode();
        //            httpItem = new HttpItem()
        //            {
        //                URL = Url,
        //                Method = "POST",
        //                Postdata = postdata,
        //                Referer = "http://www.gs.10086.cn/service/my/myBill.html",
        //                //CookieCollection = cookies,
        //                //ResultCookieType = ResultCookieType.CookieCollection
        //                Cookie = CookieStr,
        //                ResultCookieType = ResultCookieType.String
        //            };
        //            httpResult = httpHelper.GetHtml(httpItem);
        //            if (httpResult.StatusCode != HttpStatusCode.OK)
        //            {
        //                Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //                Res.StatusCode = ServiceConsts.StatusCode_fail;
        //                return Res;
        //            }
        //            //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
        //            CookieStr += httpResult.Cookie;
        //            if (httpResult.Html.Contains("系统忙，请稍后再试"))
        //                continue;

        //            string json_base = jsonParser.GetResultFromMultiNode(httpResult.Html, "loginCallBackActionNode:result");
        //            string json_bill = jsonParser.GetResultFromParser(json_base, "MonthBill");
        //            results = jsonParser.GetArrayFromParse(json_bill, "feeDetailList");

        //            if (results.Count > 0)
        //            {
        //                bill = new MonthBill();
        //                foreach (string item in results)
        //                {
        //                    if (jsonParser.GetResultFromParser(item, "feeTypeName").Contains("套餐"))
        //                    {
        //                        bill.PlanAmt = jsonParser.GetResultFromParser(item, "totalCost");
        //                        break;
        //                    }
        //                }
        //                bill.TotalAmt = jsonParser.GetResultFromParser(json_base, "feeNow");
        //                bill.BillCycle = jsonParser.GetResultFromParser(json_base, "feeTime");

        //                if (mobile.Name.IsEmpty())
        //                    mobile.Name = jsonParser.GetResultFromParser(json_base, "userName");
        //                if (mobile.PackageBrand.IsEmpty())
        //                    mobile.PackageBrand = jsonParser.GetResultFromParser(json_base, "brandName");
        //                if (mobile.Package.IsEmpty())
        //                    mobile.Package = jsonParser.GetResultFromParser(json_base, "planName");
        //                if (mobile.StarLevel.IsEmpty())
        //                {
        //                    switch(jsonParser.GetResultFromParser(json_base, "creditClass"))
        //                    {
        //                        case "1":
        //                            mobile.StarLevel = "AAA星级";
        //                            break;
        //                        case "2":
        //                            mobile.StarLevel = "AA星级";
        //                            break;
        //                        case "3":
        //                            mobile.StarLevel = "A星级";
        //                            break;
        //                        case "51":
        //                            mobile.StarLevel = "准星级";
        //                            break;
        //                        case "52":
        //                            mobile.StarLevel = "1星级";
        //                            break;
        //                        case "53":
        //                            mobile.StarLevel = "2星级";
        //                            break;
        //                        case "54":
        //                            mobile.StarLevel = "3星级";
        //                            break;
        //                        case "55":
        //                            mobile.StarLevel = "4星级";
        //                            break;
        //                        case "56":
        //                            mobile.StarLevel = "5星级";
        //                            break;
        //                        case "57":
        //                            mobile.StarLevel = "5星金";
        //                            break;
        //                        case "58":
        //                            mobile.StarLevel = "5星钻";
        //                            break;

        //                    }
        //                }

        //                mobile.BillList.Add(bill);
        //            }
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusDescription = "甘肃移动手机账单抓取异常";
        //        Res.StatusCode = ServiceConsts.StatusCode_fail;
        //        Log4netAdapter.WriteError("甘肃移动手机账单抓取异常", e);
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
