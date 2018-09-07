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
using Vcredit.NetSpider.Entity.Mongo.Log;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class SC : IMobileCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        LogHelper httpHelper = new LogHelper();
        ApplyLogMongo logMongo = new ApplyLogMongo();
        List<ApplyLog> loglist = new List<ApplyLog>();
        ApplyLog appLog = new ApplyLog();
        ApplyLogDtl logDtl = new ApplyLogDtl();
        #endregion

        #region 私有变量和方法
        string sid = string.Empty;

        /// <summary>
        /// 模拟JS中的gettime()
        /// </summary>
        /// <returns>现在和1970年1月1日的秒数</returns>
        long GetTime()
        {
            return (DateTime.Now.Ticks - 621355968000000000) / 10000;
        }

        #endregion

        /// <summary>
        /// 页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes MobileInit(MobileReq mobileReq = null)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Init, mobileReq.Website);
            VerCodeRes Res = new VerCodeRes();
            Res.Token = CommonFun.GetGuidID();
            string Url = string.Empty;
            try
            {
                //第一步，初始化登录页面
                logDtl = new ApplyLogDtl("初始化登录页面");
                Url = "https://sc.ac.10086.cn/loginNew/index.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "初始化登录页面");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                sid = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='sid']", "value")[0];
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "初始化登录页面成功";
                appLog.LogDtlList.Add(logDtl);
                //第二步，获取验证码
                logDtl = new ApplyLogDtl("获取验证码");
                Url = string.Format("https://sc.ac.10086.cn/loginNew/image_login.jsp?d={0}", GetTime());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "获取验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "获取验证码成功";
                appLog.LogDtlList.Add(logDtl);

                Res.nextProCode = ServiceConsts.NextProCode_Login;
                Res.StatusDescription = "四川移动初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("cookies", cookies);
                dic.Add("sid", sid);
                CacheHelper.SetCache(Res.Token, dic);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "四川移动初始化异常";
                Log4netAdapter.WriteError("四川移动初始化异常", e);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                logMongo.Save(appLog);
            }
            return Res;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes MobileLogin(MobileReq mobileReq)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Login, mobileReq.Website);
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            Dictionary<string, object> dic = null;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    dic = (Dictionary<string, object>)CacheHelper.GetCache(mobileReq.Token);
                    cookies = (CookieCollection)dic["cookies"];
                    sid = (string)dic["sid"];
                }

                logDtl = new ApplyLogDtl("登录");
                Url = "https://sc.ac.10086.cn/servlet/ssologin";
                postdata = string.Format("commend_bunch=&queryEmail=2&loginValue=SingerLogin&dtype=0&pho_nohd=&type_nohd=&sid={0}&dispatch=ssoLogin&pswTypeNew=1&actType=0&phone_no={1}&user_passwd={2}&fakecode={3}&rememberMe=on", sid, mobileReq.Mobile, Convert.ToBase64String(Encoding.Default.GetBytes(mobileReq.Password)), mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Allowautoredirect = false,
                    Referer = "https://sc.ac.10086.cn/loginNew/index.jsp",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    string alert = CommonFun.GetMidStr(httpResult.Html, "alert('", "，");
                    if (alert.IsEmpty())
                        alert = CommonFun.GetMidStr(httpResult.Html, "alert('", "');history.back();");
                    Res.StatusDescription = alert.IsEmpty() ? "登录失败" : alert;
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = httpResult.Header["Location"];
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Allowautoredirect = false,
                    Referer = "https://sc.ac.10086.cn/loginNew/index.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.sc.10086.cn/my/myMobile.shtml";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_Query;
                dic["cookies"] = cookies;
                dic.Add("mobileReq", mobileReq);
                CacheHelper.SetCache(mobileReq.Token, dic);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = Res.StatusCode;
                logDtl.Description = Res.StatusDescription;
                appLog.LogDtlList.Add(logDtl);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "四川移动登录异常";
                Log4netAdapter.WriteError("四川移动登录异常", e);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                logMongo.Save(appLog);
            }
            return Res;
        }

        /// <summary>
        /// 发送短信验证码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_SendSMS, mobileReq.Website);
            VerCodeRes Res = new VerCodeRes();
            Res.Token = mobileReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            Dictionary<string, object> dic = null;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    dic = (Dictionary<string, object>)CacheHelper.GetCache(mobileReq.Token);
                    cookies = (CookieCollection)dic["cookies"];
                }

                logDtl = new ApplyLogDtl("发手机验证码");

                Url = "http://www.sc.10086.cn/app?service=ajaxDirect/1/fee.FeeInfo/fee.FeeInfo/javascript/&pagename=fee.FeeInfo&eventname=getRandPass&cond_GOODS_ID=2014073100001329&cond_GOODS_NAME=%E8%AF%A6%E5%8D%95%E6%9F%A5%E8%AF%A2&cond_queryType=0&cond_queryMonth=201508&cond_date_begin=01&cond_date_end=26&INTERFACE_MODE=12&ID=undefined&PAGERANDOMID=undefined&ajaxSubmitType=post&ajax_randomcode=0.9835208035830514";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Referer = "http://www.sc.10086.cn/service/product/XDCX.shtml",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发手机验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (!httpResult.Html.Contains("详单查询获取短信码成功"))
                {
                    Res.StatusDescription = "获取短信码失败";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "四川移动手机验证码发送成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
                dic["cookies"] = cookies;
                CacheHelper.SetCache(mobileReq.Token, dic);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = Res.StatusCode;
                logDtl.Description = Res.StatusDescription;
                appLog.LogDtlList.Add(logDtl);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "四川移动手机验证码发送异常";
                Log4netAdapter.WriteError("四川移动手机验证码发送异常", e);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                logMongo.Save(appLog);
            }
            return Res;

        }

        /// <summary>
        /// 校验短信验证码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_CheckSMS, mobileReq.Website);
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            Dictionary<string, object> dic = null;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    dic = (Dictionary<string, object>)CacheHelper.GetCache(mobileReq.Token);
                    cookies = (CookieCollection)dic["cookies"];
                }

                logDtl = new ApplyLogDtl("验证手机验证码");
                Url = string.Format("http://www.sc.10086.cn/app?service=ajaxDirect/1/fee.FeeInfo/fee.FeeInfo/javascript/&pagename=fee.FeeInfo&eventname=checkRandPass&cond_pass={0}&cond_GOODS_ID=2014073100001329&cond_GOODS_NAME=%E8%AF%A6%E5%8D%95%E6%9F%A5%E8%AF%A2&INTERFACE_MODE=17&ID=undefined&PAGERANDOMID=undefined&ajaxSubmitType=post&ajax_randomcode=0.15860335512784318", mobileReq.Smscode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Referer = "http://www.sc.10086.cn/service/product/XDCX.shtml",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "验证手机验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (!httpResult.Html.Contains("详单查询验证短信码成功"))
                {
                    Res.StatusDescription = "验证码错误！";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "四川移动手机验证码验证成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_Query;

                dic["cookies"] = cookies;
                CacheHelper.SetCache(mobileReq.Token, dic);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = Res.StatusCode;
                logDtl.Description = Res.StatusDescription;
                appLog.LogDtlList.Add(logDtl);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "四川移动手机验证码校验异常";
                Log4netAdapter.WriteError("四川移动手机验证码校验异常", e);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                logMongo.Save(appLog);
            }
            return Res;
        }

        /// <summary>
        /// 保存抓取的账单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            CrawlerData crawler = new CrawlerData() { Token = mobileReq.Token, IdentityCard = mobileReq.IdentityCard, Name = mobileReq.Name, Mobile = mobileReq.Mobile, UserType = "1" };
            string Url = string.Empty;
            DateTime date = DateTime.Now;
            Dictionary<string, object> dic = null;
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    dic = (Dictionary<string, object>)CacheHelper.GetCache(mobileReq.Token);
                    cookies = (CookieCollection)dic["cookies"];
                }

                #region 获取基本信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Crawler, mobileReq.Website);

                #region 基本信息
                logDtl = new ApplyLogDtl("基本信息");
                Url = "http://www.sc.10086.cn/app?service=ajaxDirect/1/my.MyMobile/my.MyMobile/javascript/&pagename=my.MyMobile&eventname=queryNetAge&ID=undefined&PAGERANDOMID=undefined&ajaxSubmitType=get&ajax_randomcode=0.2586752818724325";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Allowautoredirect = false,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                try
                {
                    if (CommonFun.GetMidStr(httpResult.Html, "[CDATA[[", "]]]").IsEmpty())
                    {
                        Res.StatusDescription = "抓取失败";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;

                        logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "基本信息抓取失败：" + Res.StatusDescription;
                        appLog.LogDtlList.Add(logDtl);
                        appLog.Token = Res.Token;
                        appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                        loglist.Add(appLog);

                        return Res;
                    }
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfo", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "基本信息抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "基本信息抓取异常：" + e.Message;
                    appLog.LogDtlList.Add(logDtl);
                }

                #endregion

                #region 我的套餐
                logDtl = new ApplyLogDtl("我的套餐");
                Url = "http://www.sc.10086.cn/app?service=ajaxDirect/1/fee.FeeInfo/fee.FeeInfo/javascript/&pagename=fee.FeeInfo&eventname=queryRestPakeagesDateInitPage&beginTime=&endTime=&cond_GOODS_ID=2014072500000901&cond_GOODS_NAME=%E5%A5%97%E9%A4%90%E4%BD%99%E9%87%8F%E6%9F%A5%E8%AF%A2&ID=undefined&PAGERANDOMID=undefined&ajaxSubmitType=get&ajax_randomcode=0.562214834200182";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Allowautoredirect = false,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "我的套餐");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "packageInfo", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "我的套餐抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region puk
                logDtl = new ApplyLogDtl("puk");
                Url = "http://www.sc.10086.cn/app?service=ajaxDirect/1/fee.FeeInfo/fee.FeeInfo/javascript/&pagename=fee.FeeInfo&eventname=querySimInformationInitPage&cond_GOODS_ID=2014072500000896&cond_GOODS_NAME=SIM%E5%8D%A1%E4%BF%A1%E6%81%AF%E6%9F%A5%E8%AF%A2&ID=undefined&PAGERANDOMID=undefined&ajaxSubmitType=get&ajax_randomcode=0.586138799962528";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Allowautoredirect = false,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "puk");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "pukInfo", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "puk抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 获取月账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Crawler, mobileReq.Website);

                logDtl = new ApplyLogDtl("月账单抓取校验");
                Url = "http://www.sc.10086.cn/service/product/ZDCX.shtml";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "月账单抓取校验");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                    for (int t = 1; t < 6; t++)
                    {
                        string title = date.AddMonths(-t).ToString(Consts.DateFormatString7) + "月账单抓取";
                        logDtl = new ApplyLogDtl(title);
                        string year_month = date.AddMonths(-t).ToString("yyyyMM");
                        Url = string.Format("http://www.sc.10086.cn/app?service=ajaxDirect/1/fee.FeeInfo/fee.FeeInfo/javascript/&pagename=fee.FeeInfo&eventname=querybillByMonth&cond_month={0}&cond_GOODS_ID=2014073100001332&cond_GOODS_NAME=%E8%B4%A6%E5%8D%95%E6%9F%A5%E8%AF%A2&INTERFACE_MODE=00&ID=undefined&PAGERANDOMID=undefined&ajaxSubmitType=get&ajax_randomcode=0.3271512833395387", year_month);
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtmlLog(httpItem, appLog, title);
                        if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                        try
                        {
                            if (CommonFun.GetMidStr(httpResult.Html, "<DATASETDATA id=\"dataset\"><![CDATA[[", "]]]></DATASETDATA>").IsEmpty())
                            {
                                logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                                logDtl.Description = title + "失败";
                                appLog.LogDtlList.Add(logDtl);
                                continue;
                            }
                            crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "Bill" + year_month, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                            logDtl.StatusCode = ServiceConsts.StatusCode_success;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = title + "成功";
                            appLog.LogDtlList.Add(logDtl);
                        }
                        catch (Exception e)
                        {
                            logDtl.StatusCode = ServiceConsts.StatusCode_error;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = title + "异常：" + e.Message;
                            appLog.LogDtlList.Add(logDtl);
                            continue;
                        }
                    }
                }

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 获取详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Crawler, mobileReq.Website);

                bool IsSuccess = true;//判断是否有月份抓取详单超时
                string NotSuccessMonth = string.Empty;//记录抓取详单超时的年月

                logDtl = new ApplyLogDtl("获取详单抓取校验");
                Url = "http://www.sc.10086.cn/service/product/XDCX.shtml";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Allowautoredirect = false,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "获取详单抓取校验");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                    //每个月的详单需要先POST请求发送短信验证，但实际上不需要验证，直接GET信息就可以
                    for (int t = 0; t < 6; t++)
                    {
                        string title = date.AddMonths(-t).ToString(Consts.DateFormatString7) + "月全部详单抓取";
                        logDtl = new ApplyLogDtl(title);
                        int Year = date.AddMonths(-t).Year;
                        int Month = date.AddMonths(-t).Month;
                        string year_month = date.AddMonths(-t).ToString("yyyyMM");
                        string endtime = t == 0 ? date.ToString("dd") : DateTime.DaysInMonth(Year, Month).ToString();

                        Url = string.Format("http://www.sc.10086.cn/app?service=ajaxDirect/1/fee.FeeInfo/fee.FeeInfo/javascript/&pagename=fee.FeeInfo&eventname=getRandPass&cond_GOODS_ID=2014073100001329&cond_GOODS_NAME=%E8%AF%A6%E5%8D%95%E6%9F%A5%E8%AF%A2&cond_queryType=0&cond_queryMonth={0}&cond_date_begin=01&cond_date_end={1}&INTERFACE_MODE=12&ID=undefined&PAGERANDOMID=undefined&ajaxSubmitType=post&ajax_randomcode=0.8382004578734558", year_month, endtime);
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Referer = "http://www.sc.10086.cn/service/product/XDCX.shtml",
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtmlLog(httpItem, appLog, title + "校验");
                        if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                        bool NeedContinue = true;//判断是否成功获取详单，false为成功可以继续获取详单信息,true为失败直接进入下个月份

                        for (int i = 0; i < 50; i++)//获取每月短信详单时需要GET信息，但会返回“操作频繁”，js中的方法是每隔1秒循环GET，此处也是用相同的方法进行50次
                        {
                            Url = "http://www.sc.10086.cn/app?service=ajaxDirect/1/fee.FeeInfo/fee.FeeInfo/javascript/&pagename=fee.FeeInfo&eventname=getDetailBill&count=1&ID=undefined&PAGERANDOMID=undefined&ajaxSubmitType=get&ajax_randomcode=0.872280861253382";
                            httpItem = new HttpItem()
                            {
                                URL = Url,
                                CookieCollection = cookies,
                                ResultCookieType = ResultCookieType.CookieCollection
                            };
                            httpResult = httpHelper.GetHtmlLog(httpItem, appLog, title);
                            if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                            try
                            {
                                if (httpResult.Html.Contains("频繁"))
                                {
                                    logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                                    logDtl.Description = title + "失败：" + "频繁";
                                    appLog.LogDtlList.Add(logDtl);
                                    System.Threading.Thread.Sleep(1000);
                                    continue;
                                }
                                NeedContinue = false;
                                string Bill = CommonFun.GetMidStr(httpResult.Html, "<![CDATA[[", "]]]></DATASETDATA>");
                                string code = jsonParser.GetResultFromParser(Bill, "X_RESULTCODE");
                                if (HtmlParser.GetResultFromParser(CommonFun.GetMidStr(httpResult.Html, "bills\":\"", "\",\"X_ERRORINFO"), "//body/div/table").Count <= 0 || code != "0") //成功code待确认
                                {
                                    logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                                    logDtl.Description = title + "失败：" + jsonParser.GetResultFromParser(Bill, "msg");
                                    appLog.LogDtlList.Add(logDtl);
                                    break;
                                }
                                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "Detail" + year_month, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                                logDtl.Description = title + "成功";
                                appLog.LogDtlList.Add(logDtl);
                                break;
                            }
                            catch (Exception e)
                            {
                                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                                logDtl.Description = title + "异常：" + e.Message + "原始数据：" + httpResult.Html;
                                appLog.LogDtlList.Add(logDtl);
                                continue;
                            }
                        }
                        if (NeedContinue)
                        {
                            IsSuccess = false;
                            NotSuccessMonth += year_month + "  ";
                            continue;
                        }
                    }
                }

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                //保存
                if (IsSuccess)
                {
                    Res.StatusDescription = "四川移动手机账单抓取成功";
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                }
                else
                {
                    Res.StatusDescription = "四川移动手机详单抓取成功，但结果不完整，超时年月为：" + NotSuccessMonth;
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                }
                crawlerMobileMongo.SaveCrawler(crawler);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "四川移动手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("四川移动手机账单抓取异常", e);

                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
                loglist.Add(appLog);
            }
            finally
            {
                logMongo.SaveList(loglist);
            }
            return Res;
        }

        /// <summary>
        /// 解析抓取的原始数据
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <param name="crawlerDate"></param>
        /// <returns></returns>
        public BaseRes MobileAnalysis(MobileReq mobileReq, DateTime appDate)
        {
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            ApplyLog columnLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Column_Monitoring, mobileReq.Website);
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            MobileMongo mobileMongo = new MobileMongo(appDate);
            CrawlerData crawler = new CrawlerData();
            Basic mobile = new Basic();
            MonthBill bill = null;
            Call call = null;
            Net gprs = null;
            Sms sms = null;
            string result = string.Empty;
            List<string> results = new List<string>();
            DateTime date;
            try
            {
                crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);

                #region 个人信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Analysis, mobileReq.Website);

                mobile.Token = mobileReq.Token;
                mobile.BusName = mobileReq.Name;
                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.Mobile = mobileReq.Mobile;
                mobile.UpdateTime = crawler.CrawlerDate;

                #region baseInfo
                logDtl = new ApplyLogDtl("基本信息");

                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfo").FirstOrDefault().CrawlerTxt);
                    if (!result.IsEmpty())
                    {
                        string jsondata = CommonFun.GetMidStr(result, "[CDATA[[", "]]]");
                        if (!jsondata.IsEmpty())
                        {
                            mobile.Mobile = jsonParser.GetResultFromParser(jsondata, "PHONENUM");//手机号码
                            mobile.Integral = jsonParser.GetResultFromParser(jsondata, "SCORE");//积分
                            mobile.Name = jsonParser.GetResultFromParser(jsondata, "CUSTNAME");//姓名
                            switch (jsonParser.GetResultFromParser(jsondata, "BRAND"))//套餐品牌
                            {
                                case "1":
                                    mobile.PackageBrand = "全球通";
                                    break;
                                case "2":
                                    mobile.PackageBrand = "神州行";
                                    break;
                                default:
                                    mobile.PackageBrand = "动感地带";
                                    break;
                            }
                            switch (jsonParser.GetResultFromParser(jsondata, "CREDIT"))//星级
                            {
                                case "11":
                                    mobile.StarLevel = "一星客户";
                                    break;
                                case "10":
                                    mobile.StarLevel = "二星客户";
                                    break;
                                case "9":
                                    mobile.StarLevel = "三星客户";
                                    break;
                                case "8":
                                    mobile.StarLevel = "四星客户";
                                    break;
                                case "7":
                                    mobile.StarLevel = "五星客户";
                                    break;
                                case "6":
                                    mobile.StarLevel = "五星金客户";
                                    break;
                                case "5":
                                    mobile.StarLevel = "五星钻客户";
                                    break;
                                default:
                                    mobile.StarLevel = "暂无";
                                    break;
                            }
                        }
                    }
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "基本信息解析成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = e.Message;
                    appLog.LogDtlList.Add(logDtl);
                }

                #endregion

                #region packageInfo
                logDtl = new ApplyLogDtl("手机套餐");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfo").FirstOrDefault().CrawlerTxt);
                    if (!result.IsEmpty())
                    {
                        string jsonfee = CommonFun.GetMidStr(result, "FEE_INFO\":[", ",{");
                        if (!jsonfee.IsEmpty())
                            mobile.Package = jsonParser.GetResultFromParser(jsonfee, "PROD_PRC_NAME");//套餐名称
                    }
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "手机套餐解析成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = e.Message;
                    appLog.LogDtlList.Add(logDtl);
                }
                #endregion

                #region pukInfo
                logDtl = new ApplyLogDtl("puk");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "pukInfo").FirstOrDefault().CrawlerTxt);
                    if (!result.IsEmpty())
                    {
                        string jsonpuk = CommonFun.GetMidStr(result, "[CDATA[[", "]]]");
                        if (!jsonpuk.IsEmpty())
                            mobile.PUK = jsonParser.GetResultFromParser(jsonpuk, "PUK_NO1");//PUK码
                    }
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "puk解析成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = e.Message;
                    appLog.LogDtlList.Add(logDtl);
                }
                #endregion

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 月账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Analysis, mobileReq.Website);

                for (int t = 1; t < 6; t++)
                {
                    date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-t);
                    logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月账单解析");
                    string year_month = date.ToString("yyyyMM");
                    try
                    {
                        if (crawler.DtlList.Where(x => x.CrawlerTitle == "Bill" + year_month).FirstOrDefault() == null) continue;
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "Bill" + year_month).FirstOrDefault().CrawlerTxt);
                        string JsonData = CommonFun.GetMidStr(result, "<DATASETDATA id=\"dataset\"><![CDATA[[", "]]]></DATASETDATA>");
                        bill = new MonthBill();
                        if (jsonParser.GetResultFromParser(JsonData, "JFZQ").IsEmpty())
                            continue;
                        bill.BillCycle = DateTime.Now.AddMonths(-t).ToString(Consts.DateFormatString12);
                        bill.PlanAmt = jsonParser.GetResultFromParser(JsonData, "TCJGDF");
                        bill.TotalAmt = jsonParser.GetResultFromParser(jsonParser.GetArrayFromParse(JsonData, "ACCINFO")[0], "FEEPAY");
                        mobile.BillList.Add(bill);
                    }
                    catch (Exception e)
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_error;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = date.ToString(Consts.DateFormatString7) + "月账单解析异常：" + e.Message;
                        appLog.LogDtlList.Add(logDtl);
                        continue;
                    }
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = date.ToString(Consts.DateFormatString7) + "月账单解析成功";
                    appLog.LogDtlList.Add(logDtl);
                }

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Analysis, mobileReq.Website);

                for (int t = 0; t < 6; t++)
                {
                    date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-t);
                    logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月详单解析");
                    string year_month = date.ToString("yyyyMM");
                    try
                    {
                        if (crawler.DtlList.Where(x => x.CrawlerTitle == "Detail" + year_month).FirstOrDefault() == null) continue;
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "Detail" + year_month).FirstOrDefault().CrawlerTxt);
                        string Bill = CommonFun.GetMidStr(result, "bills\":\"", "\",\"X_ERRORINFO");
                        results = HtmlParser.GetResultFromParser(Bill, "//body/div/table");
                        for (int i = 0; i < results.Count; i++)
                        {
                            string type = string.Empty;
                            type = HtmlParser.GetResultFromParser(results[i], "/tr/th")[0];

                            if (type.Contains("通话详单") && i < results.Count - 1)
                            {
                                foreach (string item in HtmlParser.GetResultFromParser(results[i + 1], "/tr"))
                                {
                                    List<string> detail = HtmlParser.GetResultFromParser(item, "/td");
                                    if (detail.Count != 10 || detail[0].Contains("通话"))
                                        continue;
                                    var totalSecond = 0;
                                    var usetime = detail[1].ToString();
                                    if (!string.IsNullOrEmpty(usetime))
                                    {
                                        totalSecond = CommonFun.ConvertDate(usetime);
                                    }
                                    call = new Call();
                                    call.StartTime = DateTime.Parse(detail[0]).ToString(Consts.DateFormatString11);
                                    call.UseTime = totalSecond.ToString();
                                    call.InitType = detail[2].Trim();
                                    call.OtherCallPhone = detail[3].Trim();
                                    call.CallPlace = detail[4].Trim();
                                    call.CallType = detail[5].Trim();
                                    call.SubTotal = detail[6].Trim().ToDecimal(0) + detail[7].Trim().ToDecimal(0) + detail[8].Trim().ToDecimal(0);
                                    mobile.CallList.Add(call);
                                }
                            }
                            if (type.Contains("彩信详单") && i < results.Count - 1)
                            {
                                string smsType = string.Empty;
                                foreach (string item in HtmlParser.GetResultFromParser(results[i + 1], "/tr"))
                                {
                                    List<string> detail = HtmlParser.GetResultFromParser(item, "/td");
                                    if (detail.Count != 5 || detail[0].Contains("主叫"))
                                    {
                                        try
                                        {
                                            List<string> smsTypeTemp = HtmlParser.GetResultFromParser(item, "/th");
                                            if (smsTypeTemp.Count > 0)
                                                smsType = smsTypeTemp[0].Substring(2);
                                        }
                                        catch { }
                                        continue;
                                    }
                                    sms = new Sms();
                                    if (detail[0].Trim() == mobileReq.Mobile)
                                    {
                                        sms.OtherSmsPhone = detail[1].Trim();
                                        sms.InitType = "发送";
                                    }
                                    else
                                    {
                                        if (detail[1].Trim() == mobileReq.Mobile)
                                        {
                                            sms.OtherSmsPhone = detail[0].Trim();
                                            sms.InitType = "接收";
                                        }
                                    }
                                    sms.StartTime = DateTime.Parse(detail[2]).ToString(Consts.DateFormatString11);
                                    sms.SubTotal = detail[3].Trim().ToDecimal(0);
                                    sms.SmsType = smsType.IsEmpty() ? string.Empty : smsType;
                                    mobile.SmsList.Add(sms);
                                }
                            }
                            if (type.Contains("上网详单") && i < results.Count - 1)
                            {
                                foreach (string item in HtmlParser.GetResultFromParser(results[i + 1], "/tr"))
                                {
                                    List<string> detail = HtmlParser.GetResultFromParser(item, "/td");
                                    if (detail.Count != 9 || detail[0].Contains("主叫"))
                                        continue;
                                    gprs = new Net();
                                    gprs.PhoneNetType = detail[1].Trim();
                                    gprs.StartTime = DateTime.Parse(detail[2].Trim()).ToString(Consts.DateFormatString11);
                                    gprs.SubFlow = detail[4].Trim().ToString();
                                    gprs.NetType = detail[5].Trim();
                                    gprs.Place = detail[6].Trim();
                                    gprs.SubTotal = detail[7].Trim().ToDecimal(0);
                                    mobile.NetList.Add(gprs);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_error;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = date.ToString(Consts.DateFormatString7) + "月详单解析异常：" + e.Message;
                        appLog.LogDtlList.Add(logDtl);

                        continue;
                    }

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = date.ToString(Consts.DateFormatString7) + "月详单解析成功";
                    appLog.LogDtlList.Add(logDtl);
                }

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                //保存
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "四川移动手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "四川移动手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("四川移动手机账单解析异常", e);

                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
                loglist.Add(appLog);
            }
            finally
            {
                if (columnLog.LogDtlList.Count > 0)
                    loglist.Add(columnLog);
                logMongo.SaveList(loglist);
            }
            return Res;
        }
    }
}
