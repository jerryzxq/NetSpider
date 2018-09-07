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
    public class NM : ChinaMobile
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
        //string service = string.Empty;
        //string aiparamloginIndex = string.Empty;
        //string ENCRYPTFLAG = string.Empty;
        //string appId = string.Empty;
        //string userType = string.Empty;
        //string loginWay = string.Empty;

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
        //        Url = "http://www.10086.cn/nm/index_471_471.html";
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

        //        Url = "https://nm.ac.10086.cn/login";
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
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='loginForm']/input[@type='hidden']", "value");
        //        if (results.Count > 0)
        //        {
        //            service = results[0];
        //            aiparamloginIndex = results[1];
        //            ENCRYPTFLAG = results[2];
        //            appId = results[3];
        //            userType = results[4];
        //            loginWay = results[6];
        //        }

        //        //获取验证码
        //        Url = "https://nm.ac.10086.cn/createVerifyImageServlet";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Referer = "https://nm.ac.10086.cn/login",
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

        //        Res.StatusDescription = "内蒙古移动初始化完成";

        //        CacheHelper.SetCache(token, cookies);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "内蒙古移动初始化异常";
        //        Log4netAdapter.WriteError("内蒙古移动初始化异常", e);
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

        //        //密码base64加密
        //        byte[] bytes = Encoding.Default.GetBytes(mobileReq.Password);
        //        mobileReq.Password = Convert.ToBase64String(bytes);

        //        Url = "https://nm.ac.10086.cn/login";
        //        postdata = string.Format("service={0}&ai_param_loginIndex={1}&ENCRYPT_FLAG={2}&password={7}&rndPassword=&appId={3}&userType={4}&loginPass=&loginWay={5}&bigloginlabelselect1=&username={6}&loginType=1&verifyCode={8}&rememNum=checkbox&rememNum=checkbox", service, aiparamloginIndex, ENCRYPTFLAG, appId, userType, loginWay, mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
        //            ContentType = "application/x-www-form-urlencoded",
        //            Referer = "https://nm.ac.10086.cn/login",
        //            Postdata = postdata,
        //            Allowautoredirect=false,
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode == HttpStatusCode.Found)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                


        //        Url = "http://www.nm.10086.cn/my";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "Post",
        //            UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
        //            Postdata = postdata,
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
               
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        Url = "http://www.nm.10086.cn/my/";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "Post",
        //            Encoding=Encoding.UTF8,
        //            Postdata = postdata,
        //            UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
               
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


        //        //Url = "https://nm.ac.10086.cn/login?ai_param_loginIndex=9&appId=9&service=http://www.nm.10086.cn/sso/iLoginFrameCas.jsp";
        //        //httpItem = new HttpItem()
        //        //{
        //        //    URL = Url,
        //        //    CookieCollection = cookies,
        //        //    ResultCookieType = ResultCookieType.CookieCollection
        //        //};
        //        //httpResult = httpHelper.GetHtml(httpItem);
               
        //        //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


        //        Url = "http://www.nm.10086.cn/my/account/index.xhtml";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
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
        //        Res.StatusDescription = "内蒙古移动登录异常";
        //        Log4netAdapter.WriteError("内蒙古移动登录异常", e);
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

        //    try
        //    {
        //        //用户资料

        //        Url = "http://www.nm.10086.cn/my/account/index.xhtml";
        //        httpItem = new HttpItem()
        //        {
        //          URL = Url,
        //          CookieCollection = cookies,
        //          ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


        //        Url = "http://www.nm.10086.cn/busicenter/myinfo/MyInfoMenuAction/initBusi.menu?_menuId=10401";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "Post",
        //            Postdata = "divId=myinfo",
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);



        //        #region
        //        ////用户资料
        //        //Url = "https://www.hn.10086.cn/service/customerService/changeuserinfo.jsp";
        //        //httpItem = new HttpItem()
        //        //{
        //        //    URL = Url,
        //        //    CookieCollection = cookies,
        //        //    ResultCookieType = ResultCookieType.CookieCollection
        //        //};
        //        //httpResult = httpHelper.GetHtml(httpItem);
        //        //infos = HtmlParser.GetResultFromParser(httpResult.Html, "//script[1]", "")[2];
        //        //if (!string.IsNullOrEmpty(infos))
        //        //{
        //        //    var json = infos.Split('\'')[1];
        //        //    var resultObject = jsonParser.GetResultFromParser(json, "resultObject");
        //        //    mobile.Name = jsonParser.GetResultFromParser(resultObject, "USRNAME");
        //        //    mobile.Mobile = jsonParser.GetResultFromParser(resultObject, "SERIALNUMBER");

        //        //    //品牌
        //        //    mobile.PackageBrand = jsonParser.GetResultFromParser(resultObject, "CARDQUALITY");

        //        //    mobile.Idtype = jsonParser.GetResultFromParser(resultObject, "USRPIDTYPE");
        //        //    mobile.Regdate = jsonParser.GetResultFromParser(resultObject, "OPENDATE");
        //        //    mobile.Email = jsonParser.GetResultFromParser(resultObject, "EMAILADDRESS");
        //        //    mobile.Address = jsonParser.GetResultFromParser(resultObject, "HOMEADDRESS");
        //        //    mobile.Postcode = jsonParser.GetResultFromParser(resultObject, "HOMEPOSTCODE");
        //        //    mobile.Idcard = jsonParser.GetResultFromParser(resultObject, "USRPID");
        //        //}

        //        ////PUK码
        //        //Url = "https://www.hn.10086.cn/service/customerService/puk.jsp";
        //        //httpItem = new HttpItem()
        //        //{
        //        //    URL = Url,
        //        //    CookieCollection = cookies,
        //        //    ResultCookieType = ResultCookieType.CookieCollection
        //        //};
        //        //httpResult = httpHelper.GetHtml(httpItem);
        //        //infos = HtmlParser.GetResultFromParser(httpResult.Html, "//script", "")[4];
        //        //if (!string.IsNullOrEmpty(infos))
        //        //{
        //        //    var json = infos.Split('\'')[1];
        //        //    var resultObject = jsonParser.GetResultFromParser(json, "resultObject");
        //        //    mobile.PUK = jsonParser.GetResultFromParser(resultObject, "PUK");
        //        //}

        //        ////积分
        //        //Url = "https://www.hn.10086.cn/ajax/points/scoreQuery.jsp";
        //        //httpItem = new HttpItem()
        //        //{
        //        //    URL = Url,
        //        //    Method = "Post",
        //        //    Postdata = "busiId=totalScore&ecbBusiCode=QPZW022&operation=query&operType=3&attr1=201501&attr2=" + date.ToString("yyyyMM") + "&startYear=201501&endYear=" + date.ToString("yyyyMM") + "&queryType=totalScore",
        //        //    CookieCollection = cookies,
        //        //    ResultCookieType = ResultCookieType.CookieCollection
        //        //};
        //        //httpResult = httpHelper.GetHtml(httpItem);
        //        //var souce = jsonParser.GetResultFromParser(httpResult.Html, "resultObject");
        //        //mobile.Integral = jsonParser.GetResultFromParser(souce, "TOTAL_VALUE");




        //        ////套餐
        //        //Url = "https://www.hn.10086.cn/service/promotion/busiQuery.do";
        //        //httpItem = new HttpItem()
        //        //{
        //        //    URL = Url,
        //        //    Method = "Post",
        //        //    Postdata = "busiId=myTaoCan&ecbBusiCode=QC00015&operation=functionQuery",
        //        //    CookieCollection = cookies,
        //        //    ResultCookieType = ResultCookieType.CookieCollection
        //        //};
        //        //httpResult = httpHelper.GetHtml(httpItem);
        //        //var package = jsonParser.GetArrayFromParse(httpResult.Html, "resultList");
        //        //mobile.Package = package[0].Split('}')[0].Split(',')[3].Split(':')[1].Replace("\"", "");


        //        ////账单信息
        //        //Url = "https://www.hn.10086.cn/service/fee/monthBill.jsp?recommendGiftPop=true";
        //        //httpItem = new HttpItem()
        //        //{
        //        //    URL = Url,
        //        //    Method = "get",
        //        //    CookieCollection = cookies,
        //        //    ResultCookieType = ResultCookieType.CookieCollection
        //        //};
        //        //httpResult = httpHelper.GetHtml(httpItem);
        //        //if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
        //        //{
        //        //    Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
        //        //    Res.StatusCode = ServiceConsts.StatusCode_fail;
        //        //    return Res;
        //        //}
        //        //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
        //        //var tokenBill = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='token']", "value");





        //        //for (int i = 0; i < 6; i++)
        //        //{

        //        //    Url = string.Format("https://www.hn.10086.cn/ajax/billservice/monthBillResult.jsp?busiId=monthBill11&operation=query&startDate={0}&token={1}&zqFlag=null)", date.AddMonths(-i).ToString("yyyyMM"), tokenBill[0]);
        //        //    httpItem = new HttpItem()
        //        //    {
        //        //        URL = Url,
        //        //        Method = "Post",
        //        //        Referer = "https://www.hn.10086.cn/service/fee/monthBillResult.jsp",
        //        //        CookieCollection = cookies,
        //        //        ResultCookieType = ResultCookieType.CookieCollection
        //        //    };
        //        //    httpResult = httpHelper.GetHtml(httpItem);
        //        //    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='feiyongTable']/tr", "");
        //        //    if (results.Count>0)
        //        //    {
        //        //        bill = new MonthBill();
        //        //        bill.BillCycle = date.AddMonths(-i).ToString(Consts.DateFormatString12);
        //        //        bill.PlanAmt = HtmlParser.GetResultFromParser(results[1], "//td[2]", "")[0];
        //        //        bill.TotalAmt = HtmlParser.GetResultFromParser(results[9], "//td[2]", "")[0];

        //        //    }
        //        //}

        //        #endregion

        //    #endregion
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "内蒙古移动手机验证码发送异常";
        //        Log4netAdapter.WriteError("内蒙古移动手机验证码发送异常", e);
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
