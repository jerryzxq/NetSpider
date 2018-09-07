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
    public class LN : ChinaMobile
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
        //        Url = "http://ln.ac.10086.cn/login";
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

        //        Url = "http://ln.ac.10086.cn/createVerifyImageServlet";
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

        //        Res.StatusDescription = "辽宁移动初始化完成";

        //        CacheHelper.SetCache(token, cookies);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "辽宁移动初始化异常";
        //        Log4netAdapter.WriteError("辽宁移动初始化异常", e);
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

        //        Url = "http://ln.ac.10086.cn/login";
        //        postdata = string.Format("service=&ai_param_loginIndex=6&password={0}&rndPassword=&appId=4&userType=1&loginPass={0}&bigloginlabelselect1=&username={1}&loginType=1&verifyCode={2}", CommonFun.GetEncodeBase64(Encoding.Default, mobileReq.Password), mobileReq.Mobile, mobileReq.Vercode);
        //        //postdata = string.Format("type=B&backurl={0}&errorurl={1}&spid={2}&RelayState={3}&mobileNum={4}&servicePassword={5}&smsValidCode=&validCode=0000&Password-type=&button={6}", backurl.ToUrlEncode(), errorurl.ToUrlEncode(), spid, RelayState.ToUrlEncode(), mobileReq.Mobile, MultiKeyDES.EncryptDES(mobileReq.Password, "YHXWWLKJYXGS", "ZFCHHYXFL10C", "DES"), "登  录");
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

        //        Url = "http://www.ln.10086.cn/sso/iLoginFrameCas.jsp";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Allowautoredirect = false,
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);

        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


        //        Url = "https://ln.ac.10086.cn/login?service=http%3A%2F%2Fwww.ln.10086.cn%2Fsso%2FiLoginFrameCas.jsp&ai_param_loginIndex=6&appId=4";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);

        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        string host = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='host']", "value")[0];
        //        string ticket = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='ticket']", "value")[0];

        //        Url = "http://www.ln.10086.cn/sso/iLoginFrameCas.jsp";
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

        //        Url = "http://www.ln.10086.cn/my/index.xhtml";
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
        //        CacheHelper.SetCache(mobileReq.Token, cookies);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "辽宁移动登录异常";
        //        Log4netAdapter.WriteError("辽宁移动登录异常", e);
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
        //            cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
        //        }

        //        #region 每月账单
        //        for (int i = 1; i < 6; i++)
        //        {
        //            string YearMonth = DateTime.Now.AddMonths(-i).ToString("yyyyMM");
        //            Url = string.Format("http://www.ln.10086.cn/busicenter/fee/monthbill/MonthBillMenuAction/initBusi.menu?_menuId=1050344&billMonth={0}&flag=999", YearMonth);
        //            httpItem = new HttpItem()
        //            {
        //                URL = Url,
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

        //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//ul[@class='userinfo']//span[@class='userinfo2']");

        //            bill = new MonthBill();
        //            if(results.Count == 5)
        //            {
        //                if (mobile.Name.IsEmpty())
        //                    mobile.Name = results[0];
        //                if (mobile.PackageBrand.IsEmpty())
        //                    mobile.PackageBrand = results[2];
        //                if (mobile.StarLevel.IsEmpty())
        //                    mobile.StarLevel = results[3];
        //                bill.BillCycle = results[4];
        //            }

        //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//ul[@class='feesalllist_gotone']/li");
        //            if (results.Count > 0)
        //            {
        //                foreach(string item in results)
        //                {
        //                    if(item.Contains("套餐"))
        //                        bill.PlanAmt = HtmlParser.GetResultFromParser(item, "//span[@class='feesalllist2']")[0];

        //                    if(item.Contains("合计"))
        //                        bill.TotalAmt = HtmlParser.GetResultFromParser(item, "//span[@class='feesalllist2']/strong")[0];
        //                }
        //            }

        //            mobile.BillList.Add(bill);
        //        }
        //        #endregion
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusDescription = "辽宁移动手机账单抓取异常";
        //        Res.StatusCode = ServiceConsts.StatusCode_fail;
        //        Log4netAdapter.WriteError("辽宁移动手机账单抓取异常", e);
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
