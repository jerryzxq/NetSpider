using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
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
    public class ZJ :ChinaMobile
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
        //string cookieStr = string.Empty;
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
        //        Url = "https://zj.ac.10086.cn/ImgDisp";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            ResultType = ResultType.Byte,
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieString("", httpResult.Cookie);
        //        Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
        //        //保存验证码图片在本地
        //        FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
        //        Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.nextProCode = ServiceConsts.NextProCode_Two;

        //        Res.StatusDescription = "浙江移动初始化完成";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;

        //        CacheHelper.SetCache(token, cookieStr);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "浙江移动初始化异常";
        //        Log4netAdapter.WriteError("浙江移动初始化异常", e);
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
        //    string samlart = string.Empty;
        //    string relayState = string.Empty;
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
        //            cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
        //            CacheHelper.RemoveCache(mobileReq.Token);
        //        }
        //        //校验参数
        //        if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
        //        {
        //            Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        Url = "https://zj.ac.10086.cn/loginbox";
        //        postdata = string.Format("service=my&continue=%2Fmy%2Flogin%2FloginSuccess.do&failurl=https%3A%2F%2Fzj.ac.10086.cn%2Flogin&style=1&pwdType=2&SMSpwdType=0&billId={0}&mima=fuwumima&passwd1=%CD%FC%BC%C7%C3%DC%C2%EB%A3%BF%BF%C9%D3%C3%B6%CC%D0%C5%D1%E9%D6%A4%C2%EB%B5%C7%C2%BC&passwd={1}&validCodeId1=5%B8%F6%D7%D6%B7%FB&validCode={2}", mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Encoding = Encoding.GetEncoding("gbk"),
        //            Postdata = postdata,
        //            Referer = "https://zj.ac.10086.cn/login",
        //            Cookie = cookieStr,
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        if (httpResult.Html.Contains("验证码不正确"))
        //        {
        //            Res.StatusDescription = "验证码不正确！";
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        if (httpResult.Html.Contains("用户名或密码错误"))
        //        {
        //            Res.StatusDescription = "用户名或密码错误！";
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        if (httpResult.Html.Contains("失败"))
        //        {
        //            Res.StatusDescription = "登录失败！";
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        string SAMLart = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value")[0];
        //        cookieStr = CommonFun.GetCookieString(cookieStr, httpResult.Cookie);

        //        Url = "http://www.zj.10086.cn/my/sso";
        //        postdata = string.Format("SAMLart={0}&RelayState=%2Fmy%2Flogin%2FloginSuccess.do&submit=%CC%E1%BD%BB%B2%E9%D1%AF", SAMLart);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
        //            Cookie = cookieStr,
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookieStr = CommonFun.GetCookieString(cookieStr, httpResult.Cookie);
        //        SAMLart = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value")[0];
        //        cookieStr = "advPosition2=ok; WEBTRENDS_ID=210.22.124.10-721021584.30471846; VU_ID=PPBswWfm218l; VU_CHIDS=|6|27; " + cookieStr;
        //        //这里开始会出现和河北移动一样的问题
        //        Url = "http://www.zj.10086.cn/my/UnifiedLoginClientServlet";
        //        postdata = string.Format("RelayState=%252Fmy%252Flogin%252FloginSuccess.do&SAMLart={0}&jumpUrl=%252Fmy%252Flogin%252FloginSuccess.do&loginUrl=http%253A%252F%252Fwww.zj.10086.cn%252Fmy%252Flogin%252Flogin.jsp&submit=%CC%E1%BD%BB%B2%E9%D1%AF", SAMLart);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
        //            Cookie = cookieStr,
        //            UserAgent = "Mozilla/5.0 (compatible;Windows NT 6.1; WOW64;Trident/6.0;MSIE 9.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.27 Safari/537.36",
        //            Referer = "http://www.zj.10086.cn/my/sso",
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieString(cookieStr, httpResult.Cookie);


        //        //Url = "http://www.zj.10086.cn:80/my/login/loginSuccess.do";
        //        //httpItem = new HttpItem()
        //        //{
        //        //    URL = Url,
        //        //    Method = "get",
        //        //    Cookie = cookieStr,
        //        //    Referer = "http://www.zj.10086.cn/my/sso",
        //        //};
        //        //httpResult = httpHelper.GetHtml(httpItem);

        //        //if (httpResult.StatusCode != HttpStatusCode.OK)
        //        //{
        //        //    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //        //    Res.StatusCode = ServiceConsts.StatusCode_fail;
        //        //    return Res;
        //        //}
        //        //cookieStr = "cityhtml=571;currentcity=571;" + CommonFun.GetCookieString(cookieStr, httpResult.Cookie);

        //        //Url = "http://www.zj.10086.cn/my/login/redirectPage.jsp";
        //        //httpItem = new HttpItem()
        //        //{
        //        //    URL = Url,
        //        //    Method = "get",
        //        //    Cookie = cookieStr,
        //        //    Referer = "http://www.zj.10086.cn/my/sso",
        //        //};
        //        //httpResult = httpHelper.GetHtml(httpItem);
        //        //if (httpResult.StatusCode != HttpStatusCode.OK)
        //        //{
        //        //    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //        //    Res.StatusCode = ServiceConsts.StatusCode_fail;
        //        //    return Res;
        //        //}
        //        //cookieStr = CommonFun.GetCookieString(cookieStr, httpResult.Cookie);

        //        Url = "http://www.zj.10086.cn/my/index.jsp";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "get",
        //            Cookie = cookieStr,
        //            Referer = "http://www.zj.10086.cn/my/login/redirectPage.jsp",
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookieStr = CommonFun.GetCookieString(cookieStr, httpResult.Cookie);

        //        Url = "http://www.zj.10086.cn/my/index.do";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "get",
        //            Cookie = cookieStr,
        //            Referer = "http://www.zj.10086.cn/my/index.jsp",
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookieStr = CommonFun.GetCookieString(cookieStr, httpResult.Cookie);

        //        CacheHelper.SetCache(mobileReq.Token, cookieStr);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "浙江移动登录异常";
        //        Log4netAdapter.WriteError("浙江移动登录异常", e);
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
        //    try
        //    {
        //        if (CacheHelper.GetCache(mobileReq.Token) != null)
        //        {
        //            cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
        //        }

        //        Url = "http://www.zj.10086.cn/my/include/mybill.jsp";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "get",
        //            Cookie = cookieStr,
        //            Referer = "http://www.zj.10086.cn/my/index.do",
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookieStr = CommonFun.GetCookieString(cookieStr, httpResult.Cookie);

        //        Url = "http://service.zj.10086.cn/yw/myBillAnalysis/myBillAnalysisQuery.do?menuId=13003";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "get",
        //            Cookie = cookieStr,
        //            Referer = "http://www.zj.10086.cn/my/include/mybill.jsp",
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookieStr = CommonFun.GetCookieString(cookieStr, httpResult.Cookie);

        //        for (int i = 1; i < 7; i++)
        //        {
        //            Url = string.Format("http://service.zj.10086.cn/yw/bill/billDetail.do?menuId=13003&bid=BD399F39E69148CFE044001635842132&month={0}-{1}", DateTime.Now.AddMonths(-i).ToString("MM"), DateTime.Now.AddMonths(-i).ToString("yyyy"));
        //            httpItem = new HttpItem()
        //            {
        //                URL = Url,
        //                Method = "get",
        //                Cookie = cookieStr,
        //                Referer = "http://service.zj.10086.cn/yw/myBillAnalysis/myBillAnalysisQuery.do?menuId=13003",
        //            };
        //            httpResult = httpHelper.GetHtml(httpItem);
        //            if (httpResult.StatusCode != HttpStatusCode.OK)
        //            {
        //                Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //                Res.StatusCode = ServiceConsts.StatusCode_fail;
        //                return Res;
        //            }
        //            cookieStr = CommonFun.GetCookieString(cookieStr, httpResult.Cookie);
        //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tb1']/tr");
        //            bill = new MonthBill();
        //            if (results.Count > 0)
        //            {
        //                foreach (string item in results)
        //                {
        //                    switch (HtmlParser.GetResultFromParser(item, "th/strong")[0])
        //                    {
        //                        case "客　　户":
        //                            if (mobile.Name.IsEmpty())
        //                                mobile.Name = HtmlParser.GetResultFromParser(item, "td")[0].ToTrim();
        //                            break;
        //                        case "套餐名称":
        //                            if (mobile.Package.IsEmpty())
        //                                mobile.Package = HtmlParser.GetResultFromParser(item, "td")[0].ToTrim();
        //                            break;
        //                        case "客户星级":
        //                            if (mobile.StarLevel.IsEmpty())
        //                                mobile.StarLevel = HtmlParser.GetResultFromParser(item, "td")[0].ToTrim();
        //                            break;
        //                        case "计费周期":
        //                            bill.BillCycle = HtmlParser.GetResultFromParser(item, "td")[0].ToTrim();
        //                            break;
        //                    }
        //                }
        //            }

        //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tb3']//table[@class='tb3']");
        //            if (results.Count == 4)
        //            {
        //                if (HtmlParser.GetResultFromParser(results[0], "tr/td")[0] == "个人费用信息")
        //                {
        //                    results = HtmlParser.GetResultFromParser(results[0], "tr");
        //                    foreach (string item in results)
        //                    {
        //                        if (item.Contains("套餐及固定费"))
        //                            bill.PlanAmt = HtmlParser.GetResultFromParser(item, "td")[0];
        //                        if(item.Contains("合计"))
        //                            bill.TotalAmt = HtmlParser.GetResultFromParser(item, "td")[0];
        //                    }
        //                    mobile.BillList.Add(bill);
        //                }
        //            }

        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusDescription = "浙江移动手机账单抓取异常";
        //        Res.StatusCode = ServiceConsts.StatusCode_fail;
        //        Log4netAdapter.WriteError("浙江移动手机账单抓取异常", e);
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
