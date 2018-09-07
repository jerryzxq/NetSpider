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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.Entity.Mongo.Log;
using Vcredit.Common.Constants;
using System.Configuration;
using System.Net.Configuration;
using System.Reflection;
using System.Net;
using System.Threading;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class ChinaMobile : IMobileCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        string cookieStr = string.Empty;
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        LogHelper httpHelper = new LogHelper();
        ApplyLogMongo logMongo = new ApplyLogMongo();
        List<ApplyLog> loglist = new List<ApplyLog>();
        ApplyLog appLog = new ApplyLog();
        ApplyLogDtl logDtl = new ApplyLogDtl();
        #endregion

        /// <summary>
        /// 页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes MobileInit(MobileReq mobileReq = null)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Init, mobileReq.Website);
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            Res.Token = CommonFun.GetGuidID();
            try
            {
                //第一步，初始化登录页面
                logDtl = new ApplyLogDtl("初始化登录页面");
                Url = "https://login.10086.cn/html/login/login.html?channelID=12002&backUrl=http%3A%2F%2Fshop.10086.cn%2Fmall_100_100.html%3Fforcelogin%3D1";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Cookie = "CmProvid=bj;",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "初始化登录页面");
                cookieStr = CommonFun.GetCookieStringNew("", httpResult.Cookie);
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "初始化登录页面成功";
                appLog.LogDtlList.Add(logDtl);

                if (CheckNeedVerify(mobileReq))
                {
                    //第二步，获取验证码
                    logDtl = new ApplyLogDtl("获取验证码");

                    Url = "https://login.10086.cn/captchazh.htm?type=05";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        ResultType = ResultType.Byte,
                        Referer = "https://login.10086.cn/html/login/login.html?channelID=12002&backUrl=http%3A%2F%2Fshop.10086.cn%2Fmall_100_100.html%3Fforcelogin%3D1",
                        Cookie = cookieStr
                    };
                    httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "获取验证码");
                    if (httpResult.StatusCode != HttpStatusCode.OK)
                    {
                        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                        Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                        return Res;
                    }
                    cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                    Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                    Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.Description = "获取验证码成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                Res.nextProCode = ServiceConsts.NextProCode_Login;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = GetProvince(mobileReq.Website) + "移动初始化完成";

                CacheHelper.SetCache(Res.Token, cookieStr);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = GetProvince(mobileReq.Website) + "移动初始化异常";
                Log4netAdapter.WriteError(GetProvince(mobileReq.Website) + "移动初始化异常", e);

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
            string artifact = string.Empty;
            string Url = string.Empty;
            string result = string.Empty;
            try
            {
                //获取缓存CacheHelper
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
                }

                #region 登录

                if (!mobileReq.Vercode.IsEmpty())
                {
                    logDtl = new ApplyLogDtl("校验验证码");

                    Url = string.Format("https://login.10086.cn/verifyCaptcha?inputCode={0}", mobileReq.Vercode);
                    httpItem = new HttpItem()
                    {
                        Accept = "application/json, text/javascript, */*; q=0.01",
                        URL = Url,
                        Referer = "https://login.10086.cn/html/login/login.html?channelID=12002&backUrl=http%3A%2F%2Fshop.10086.cn%2Fmall_100_100.html%3Fforcelogin%3D1",
                        Cookie = cookieStr
                    };
                    httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验验证码");
                    if (httpResult.StatusCode != HttpStatusCode.OK)
                    {
                        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                        Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                        return Res;
                    }
                    cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                    result = CommonFun.GetMidStr(httpResult.Html, "resultCode\":\"", "\"}");
                    if (!result.IsEmpty() && result != "0")
                    {
                        Res.StatusDescription = "验证码错误！";
                        Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode("验证码错误");

                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.StatusCode = Res.StatusCode;
                        logDtl.Description = Res.StatusDescription;
                        appLog.LogDtlList.Add(logDtl);

                        return Res;
                    }
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.Description = "校验验证码成功";
                    appLog.LogDtlList.Add(logDtl);
                }

                logDtl = new ApplyLogDtl("登录");

                Url = string.Format("https://login.10086.cn/login.htm?accountType=01&account={0}&password={1}&pwdType=01&inputCode={2}&backUrl=http%3A%2F%2Fshop.10086.cn%2Fmall_100_100.html%3Fforcelogin%3D1&rememberMe=0&channelID=12002&protocol=https%3A&timestamp={3}", mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode, GetTimeStamp());
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Referer = "https://login.10086.cn/html/login/login.html?channelID=12002&backUrl=http%3A%2F%2Fshop.10086.cn%2Fmall_100_100.html%3Fforcelogin%3D1",
                    Cookie = cookieStr
                };
                SetAllowUnsafeHeaderParsing();  //设置useUnsafeHeaderParsing
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                result = CommonFun.GetMidStr(httpResult.Html, "result\":\"", "\"");
                if (!result.IsEmpty() && result != "0")
                {
                    Res.StatusDescription = jsonParser.GetResultFromParser(httpResult.Html, "desc");
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                string code = CommonFun.GetMidStr(httpResult.Html, "code\":\"", "\"");
                if (!code.IsEmpty() && code != "0000")
                {
                    Res.StatusDescription = "请关闭登录保护后再试！";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = "登录保护";
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                artifact = jsonParser.GetResultFromParser(httpResult.Html, "artifact");
                var uid = jsonParser.GetResultFromParser(httpResult.Html, "uid");

                Url = "https://login.10086.cn/login.html?channelID=12003&backUrl=http://shop.10086.cn/i/?f=home";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Referer = "http://shop.10086.cn/i/?f=home",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录校验");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Cookie cCookie = new Cookie("c", uid, "/", ".10086.cn");
                new CookieCollection().Add(cCookie);
                Url = "https://login.10086.cn/checkUidAvailable.action";
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Method = "Post",
                    Postdata = "uid=uid",
                    Referer = "https://login.10086.cn/login.html?channelID=12003&backUrl=http://shop.10086.cn/i/?f=home",
                    Cookie = cookieStr,
                    Host = "login.10086.cn"
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录校验");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "登录成功";
                appLog.LogDtlList.Add(logDtl);

                #endregion

                #region 校验移动商城
                logDtl = new ApplyLogDtl("校验移动商城");

                Url = string.Format("http://shop.10086.cn/sso/getartifact.php?backUrl=http%3A%2F%2Fshop.10086.cn%2Fmall_100_100.html%3Fforcelogin%3D1&artifact={0}", artifact);
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验移动商城（getartifact）");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Url = "http://shop.10086.cn/mall_100_100.html";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验移动商城（mall_100_100）");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Url = "http://shop.10086.cn/ajax/user/userinfo.json?update=1&province_id=100&city_id=100&callback=initUser";
                httpItem = new HttpItem()
                {
                    Accept = "application/javascript, */*;q=0.8",
                    URL = Url,
                    Referer = "http://shop.10086.cn/mall_100_100.html",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验移动商城（userinfo）");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Url = "http://search.10086.cn/shop/defword_jsonpcallback.json?jsonpcallback=jQuery11020553242172670676_" + GetTimeStamp() + "&areacode=100";
                httpItem = new HttpItem()
                {
                    Accept = "application/javascript, */*;q=0.8",
                    URL = Url,
                    Referer = "http://shop.10086.cn/mall_100_100.html",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验移动商城（defword_jsonpcallback）");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Url = "http://shop.10086.cn/ajax/recharge/recharge.json?province_id=100";
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Referer = "http://shop.10086.cn/mall_100_100.html",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验移动商城（recharge）");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "校验移动商城成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 校验个人中心
                logDtl = new ApplyLogDtl("校验个人中心");

                Url = "http://shop.10086.cn/i/?f=home";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验个人中心（home）");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Url = "http://shop.10086.cn/i/v1/auth/loginfo?time=201512413280429";
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Referer = "http://shop.10086.cn/i/?f=home",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验个人中心（loginfo）");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Url = "https://login.10086.cn/SSOCheck.action?channelID=12003&backUrl=http://shop.10086.cn/i/?f=home";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    Allowautoredirect = false,
                    URL = Url,
                    Referer = "http://shop.10086.cn/i/?f=home",
                    Cookie = cookieStr
                };
                httpResult = new HttpHelper().GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                artifact = CommonFun.GetMidStr(CommonFun.GetMidStr(httpResult.RedirectUrl, "artifact=", ""), "", "&");

                Url = string.Format("http://shop.10086.cn/i/v1/auth/getArtifact?artifact={0}&backUrl=http%3A%2F%2Fshop.10086.cn%2Fi%2F%3Ff%3Dhome", artifact);
                httpItem = new HttpItem()
                {
                    Accept = "text/html, application/xhtml+xml, */*",
                    URL = Url,
                    Referer = "http://shop.10086.cn/i/?f=home",
                    Cookie = cookieStr

                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验个人中心（getArtifact）");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Url = "http://shop.10086.cn/i/v1/auth/loginfo?time=201512413280429";
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Referer = "http://shop.10086.cn/i/?f=home",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验个人中心（loginfo）");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Url = string.Format("http://shop.10086.cn/i/v1/auth/loginfoaccess?time=20151110938210", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Referer = "http://shop.10086.cn/i/?f=home",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验个人中心（loginfoaccess）");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                result = CommonFun.GetMidStr(httpResult.Html, "retMsg\":\"", "\"");
                if (result != "success")
                {
                    Res.StatusDescription = "用户未登录";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "校验个人中心成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                Res.StatusDescription = GetProvince(mobileReq.Website) + "移动登录成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_SendSMS;

                CacheHelper.SetCache(mobileReq.Token, cookieStr);
                CacheHelper.SetCache(mobileReq.Token + "pwd", mobileReq.Password);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = GetProvince(mobileReq.Website) + "移动登录异常";
                Log4netAdapter.WriteError(GetProvince(mobileReq.Website) + "移动登录异常", e);

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
            cookieStr = string.Empty;
            string Url = string.Empty;
            string result = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token + "isFirst") != null)
                {
                    Thread.Sleep(35000);
                }
                else
                {
                    CacheHelper.SetCache(mobileReq.Token + "isFirst", "false");
                }
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
                }
                //第五步，发手机验证码
                logDtl = new ApplyLogDtl("发手机验证码");

                Url = string.Format("https://login.10086.cn/sendSMSpwd.action?callback=result&userName={0}&_=1447036853654", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    URL = Url,
                    Referer = "http://shop.10086.cn/i/?f=billdetailqry",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发手机验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                result = CommonFun.GetMidStr(httpResult.Html, "resultCode\":\"", "\"}");
                if (result != "0")
                {
                    Res.StatusDescription = "验证码发送失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }

                Res.StatusDescription = GetProvince(mobileReq.Website) + "移动手机验证码发送成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;

                CacheHelper.SetCache(mobileReq.Token, cookieStr);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = Res.StatusCode;
                logDtl.Description = Res.StatusDescription;
                appLog.LogDtlList.Add(logDtl);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = GetProvince(mobileReq.Website) + "移动手机验证码发送异常";
                Log4netAdapter.WriteError(GetProvince(mobileReq.Website) + "移动手机验证码发送异常", e);

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
            cookieStr = string.Empty;
            string Url = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
                if (CacheHelper.GetCache(mobileReq.Token + "pwd") != null && mobileReq.Password.IsEmpty())
                    mobileReq.Password = (string)CacheHelper.GetCache(mobileReq.Token + "pwd");

                //第六步，验证手机验证码
                logDtl = new ApplyLogDtl("验证手机验证码");

                Url = string.Format("https://login.10086.cn/temporaryauthSMSandService.action?callback=result&account={0}&servicePwd={1}&smsPwd={2}&accountType=01&backUrl=&channelID=12003&businessCode=01&_=1447036497014", mobileReq.Mobile, mobileReq.Password, mobileReq.Smscode);
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    URL = Url,
                    Referer = "http://shop.10086.cn/i/?f=billdetailqry",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "验证手机验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                string result = CommonFun.GetMidStr(httpResult.Html, "result\":\"", "\"}");
                if (!result.IsEmpty() && result != "0000")
                {
                    Res.StatusDescription = CommonFun.GetMidStr(httpResult.Html, "desc\":\"", "\",");
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Res.StatusDescription = GetProvince(mobileReq.Website) + "移动手机验证码验证成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_Query;

                CacheHelper.SetCache(mobileReq.Token, cookieStr);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = Res.StatusCode;
                logDtl.Description = Res.StatusDescription;
                appLog.LogDtlList.Add(logDtl);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = GetProvince(mobileReq.Website) + "移动手机验证码验证异常";
                Log4netAdapter.WriteError(GetProvince(mobileReq.Website) + "移动手机验证码验证异常", e);

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
        /// 手机抓取
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {
            Thread.Sleep(30000);
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            CrawlerData crawler = new CrawlerData() { Token = mobileReq.Token, IdentityCard = mobileReq.IdentityCard, Name = mobileReq.Name, Mobile = mobileReq.Mobile, UserType = "1" };
            string Url = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);

                #region 个人信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Crawler, mobileReq.Website);

                #region 基本信息
                logDtl = new ApplyLogDtl("基本信息");

                Url = string.Format("http://shop.10086.cn/i/v1/cust/info/{0}?time=201511911294663", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Referer = "http://shop.10086.cn/i/?f=home",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    string retCode = CommonFun.GetMidStr(httpResult.Html, "retCode\":\"", "\",");
                    if (!retCode.IsEmpty() && retCode != "000000")
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "基本信息抓取失败";
                        appLog.LogDtlList.Add(logDtl);
                    }
                    else
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "基本信息抓取成功";
                        appLog.LogDtlList.Add(logDtl);
                    }
                }
                #endregion

                #region 手机积分
                logDtl = new ApplyLogDtl("手机积分");

                //积分
                Url = string.Format("http://shop.10086.cn/i/v1/point/sum/{0}?time=201511911294663", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Referer = "http://shop.10086.cn/i/?f=home",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "手机积分");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    string retCode = CommonFun.GetMidStr(httpResult.Html, "retCode\":\"", "\",");
                    if (!retCode.IsEmpty() && retCode != "000000")
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "手机积分抓取失败";
                        appLog.LogDtlList.Add(logDtl);
                    }
                    else
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "手机积分抓取成功";
                        appLog.LogDtlList.Add(logDtl);
                    }
                }

                #endregion

                #region 套餐

                Thread.Sleep(10000);
                GetDeatils(EnumMobileDeatilType.Other, mobileReq.Mobile, crawler);

                #endregion

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 话费帐单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Crawler, mobileReq.Website);

                GetMobileBill(mobileReq.Mobile, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 通话详单
                Thread.Sleep(10000);
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Crawler, mobileReq.Website);

                GetDeatils(EnumMobileDeatilType.Call, mobileReq.Mobile, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信详单
                Thread.Sleep(10000);
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Crawler, mobileReq.Website);

                GetDeatils(EnumMobileDeatilType.SMS, mobileReq.Mobile, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网详单
                Thread.Sleep(10000);
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Crawler, mobileReq.Website);

                GetDeatils(EnumMobileDeatilType.Net, mobileReq.Mobile, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = GetProvince(mobileReq.Website) + "移动手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusDescription = GetProvince(mobileReq.Website) + "移动手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(GetProvince(mobileReq.Website) + "移动手机账单抓取异常", e);

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
        /// 手机解析
        /// </summary>
        /// <param name="mobileReq"></param>
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
            Call call = null;
            Net gprs = null;
            Sms sms = null;
            string result = string.Empty;
            string retCode = string.Empty;
            object Infoobj = null;
            JObject Infojs = null;

            try
            {
                crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);

                #region 个人信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Analysis, mobileReq.Website);

                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.BusName = mobileReq.Name;
                mobile.Token = mobileReq.Token;
                mobile.Mobile = mobileReq.Mobile;
                mobile.UpdateTime = crawler.CrawlerDate;

                #region 基本信息
                logDtl = new ApplyLogDtl("基本信息");

                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                    retCode = CommonFun.GetMidStr(result, "retCode\":\"", "\",");
                    if (!retCode.IsEmpty() && retCode == "000000")
                    {
                        Infoobj = JsonConvert.DeserializeObject(result);
                        Infojs = Infoobj as JObject;
                        Infojs = Infojs["data"] as JObject;
                        if (Infojs != null)
                        {
                            mobile.Name = Infojs["name"].ToString();
                            mobile.PackageBrand = Infojs["brand"].ToString() == "01" ? "全球通" : (Infojs["brand"].ToString() == "03" ? "动感地带" : "神州行");
                            // mobile.Integral = Infojs[2].ToString();
                            mobile.StarLevel = Infojs["starLevel"].ToString();
                            mobile.Regdate = Infojs["inNetDate"].ToString();
                            if (!mobile.Regdate.IsEmpty())
                                mobile.Regdate = DateTime.Parse(mobile.Regdate.Substring(0, 4) + "-" + mobile.Regdate.Substring(4, 2) + "-" + mobile.Regdate.Substring(6, 2)).ToString(Consts.DateFormatString11);
                            mobile.Address = Infojs["address"].ToString();
                            mobile.Email = Infojs["email"].ToString();
                            mobile.Postcode = Infojs["zipCode"].ToString();
                        }

                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "基本信息";
                        appLog.LogDtlList.Add(logDtl);
                    }
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = e.Message;
                    appLog.LogDtlList.Add(logDtl);
                }
                #endregion

                #region 积分
                logDtl = new ApplyLogDtl("手机积分");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                    retCode = CommonFun.GetMidStr(result, "retCode\":\"", "\",");
                    if (!retCode.IsEmpty() && retCode == "000000")
                    {
                        Infoobj = JsonConvert.DeserializeObject(result);
                        Infojs = Infoobj as JObject;
                        if (Infojs != null)
                        {
                            JObject data = Infojs["data"] as JObject;
                            if (data != null)
                                mobile.Integral = data["pointValue"].ToString();
                        }
                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "手机积分";
                        appLog.LogDtlList.Add(logDtl);
                    }
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = e.Message;
                    appLog.LogDtlList.Add(logDtl);
                }
                #endregion

                #region 套餐
                logDtl = new ApplyLogDtl("手机套餐");

                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault().CrawlerTxt);
                    if (result.Split('(').Count() > 1)
                    {
                        result = result.Split('(')[1].Split(')')[0];
                    }
                    try
                    {
                        Infoobj = JsonConvert.DeserializeObject(result);
                        Infojs = Infoobj as JObject;
                        if (Infojs != null)
                        {
                            JArray arrdata = Infojs["data"] as JArray;
                            if (arrdata != null)
                            {
                                mobile.Package = arrdata[0]["mealName"] != null ? arrdata[0]["mealName"].ToString() : "";  //套餐
                            }
                        }
                    }
                    catch
                    {
                        Infoobj = JsonConvert.DeserializeObject(result.Replace("[", "").Replace("]", ""));
                        Infojs = Infoobj as JObject;
                        if (Infojs != null)
                        {
                            JObject data = Infojs["data"] as JObject;
                            if (data != null)
                            {
                                mobile.Package = data["mealName"].ToString();  //套餐
                            }
                        }
                    }
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "手机套餐";
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

                #region 话费帐单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Analysis, mobileReq.Website);

                ReadMobileBill(crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 话费详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Analysis, mobileReq.Website);

                ReadDeatils(EnumMobileDeatilType.Call, crawler, delegate(JObject bill, string year)
                {
                    JArray detailList = bill["data"] as JArray;
                    if (detailList != null && detailList.Count > 0)
                    {
                        for (int i = 0; i < detailList.Count; i++)
                        {
                            JObject detail = detailList[i] as JObject;
                            if (detail != null)
                            {
                                var totalSecond = 0;
                                var usetime = detail["commTime"].ToString();
                                if (!string.IsNullOrEmpty(usetime))
                                {
                                    totalSecond = CommonFun.ConvertDate(usetime);
                                }
                                call = new Call();
                                try
                                {
                                    call.StartTime = DateTime.Parse(detail["startTime"].ToString()).ToString(Consts.DateFormatString11);
                                }
                                catch
                                {
                                    call.StartTime = DateTime.Parse(year + "-" + detail["startTime"].ToString()).ToString(Consts.DateFormatString11);
                                }
                                call.CallPlace = detail["commPlac"].ToString();
                                call.InitType = detail["commMode"].ToString();
                                call.OtherCallPhone = detail["anotherNm"].ToString();
                                call.UseTime = totalSecond.ToString();
                                call.CallType = detail["commType"].ToString();
                                call.SubTotal = detail["commFee"].ToString().ToDecimal(0);
                                mobile.CallList.Add(call);
                            }
                        }
                    }
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Analysis, mobileReq.Website);

                ReadDeatils(EnumMobileDeatilType.SMS, crawler, delegate(JObject bill, string year)
                {
                    JArray detailList = bill["data"] as JArray;
                    if (detailList != null && detailList.Count > 0)
                    {
                        for (int i = 0; i < detailList.Count; i++)
                        {
                            JObject detail = detailList[i] as JObject;
                            if (detail != null)
                            {
                                sms = new Sms();
                                try
                                {
                                    sms.StartTime = DateTime.Parse(detail["startTime"].ToString()).ToString(Consts.DateFormatString11);
                                }
                                catch
                                {
                                    sms.StartTime = DateTime.Parse(year + "-" + detail["startTime"].ToString()).ToString(Consts.DateFormatString11);
                                }
                                sms.SmsPlace = detail["commPlac"].ToString();
                                sms.OtherSmsPhone = detail["anotherNm"].ToString();
                                sms.InitType = detail["commMode"].ToString();
                                sms.SmsType = detail["infoType"].ToString();
                                sms.SubTotal = detail["commFee"].ToString().ToDecimal(0);
                                mobile.SmsList.Add(sms);
                            }
                        }
                    }
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Analysis, mobileReq.Website);

                ReadDeatils(EnumMobileDeatilType.Net, crawler, delegate(JObject bill, string year)
                {
                    JArray detailList = bill["data"] as JArray;
                    if (detailList != null && detailList.Count > 0)
                    {
                        for (int i = 0; i < detailList.Count; i++)
                        {
                            JObject detail = detailList[i] as JObject;
                            if (detail != null)
                            {
                                var totalSecond = 0;
                                var usetime = detail["commTime"].ToString();
                                if (!string.IsNullOrEmpty(usetime))
                                {
                                    totalSecond = CommonFun.ConvertDate(usetime);
                                }

                                gprs = new Net();
                                try
                                {
                                    gprs.StartTime = DateTime.Parse(year + "-" + detail["startTime"].ToString()).ToString(Consts.DateFormatString11);
                                }
                                catch
                                {
                                    gprs.StartTime = DateTime.Parse(detail["startTime"].ToString()).ToString(Consts.DateFormatString11);
                                }
                                gprs.Place = detail["commPlac"].ToString();
                                gprs.PhoneNetType = detail["netPlayType"].ToString();
                                gprs.NetType = detail["netType"].ToString();
                                gprs.UseTime = totalSecond.ToString();
                                gprs.SubFlow = CommonFun.ConvertGPRS(detail["sumFlow"].ToString()).ToString();
                                gprs.SubTotal = detail["commFee"].ToString().ToDecimal(0);
                                mobile.NetList.Add(gprs);
                            }
                        }
                    }
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion


                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog, "ChinaMobile");

                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = GetProvince(mobileReq.Website) + "移动手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);

            }
            catch (Exception e)
            {
                Res.StatusDescription = GetProvince(mobileReq.Website) + "移动手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(GetProvince(mobileReq.Website) + "移动手机账单解析异常", e);

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

        #region 私有方法

        /// <summary>
        /// 抓取手机详单
        /// </summary>
        /// <param name="queryType">GSM:通话；GPRS:上网；SMS:短信/彩信；RC 套餐</param>
        /// <returns></returns>
        private void GetDeatils(EnumMobileDeatilType type, string mobile, CrawlerData crawler)
        {
            DateTime date = DateTime.Now;
            string Url = string.Empty;
            string title = string.Empty;
            string queryType = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                queryType = "02";
            else if (type == EnumMobileDeatilType.SMS)
                queryType = "03";
            else if (type == EnumMobileDeatilType.Net)
                queryType = "04";
            else
                queryType = "01";

            for (int i = 0; i < 6; i++)
            {
                int curCuror = 1;
                int totalNum = 0;
                int pageSize = 50;
                int pageIndex = 1;
                int httpcou = 0;
                if (queryType == "01")
                {
                    title = "手机套餐";
                    date = DateTime.Now.AddMonths(-1);
                }
                else
                {
                    date = DateTime.Now.AddMonths(-i);
                    title = date.ToString(Consts.DateFormatString7) + "月详单抓取";
                }
                do
                {
                    logDtl = new ApplyLogDtl(title);
                    Url = string.Format("https://shop.10086.cn/i/v1/fee/detailbillinfojsonp/{0}?callback=result&curCuror={3}&step=100&qryMonth={1}&billType={2}&time=2015119104451249&_=1447037867618", mobile, date.ToString("yyyyMM"), queryType, curCuror);
                    httpItem = new HttpItem()
                    {
                        Accept = "*/*",
                        URL = Url,
                        Referer = "http://shop.10086.cn/i/?welcome=1459828801892",
                        Cookie = cookieStr
                    };
                    httpResult = httpHelper.GetHtmlLog(httpItem, appLog, title);
                    if (httpResult.StatusCode != HttpStatusCode.OK && httpcou < 3)
                    {
                        httpcou++;
                        Thread.Sleep(5000);
                        continue;
                    }
                    httpcou = 0;
                    //手机套餐
                    if (queryType == "01")
                    {
                        crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "packageInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = title + "抓取成功";
                        appLog.LogDtlList.Add(logDtl);
                        return;
                    }
                    //详单抓取
                    try
                    {
                        object obj;
                        try
                        {
                            try
                            {
                                string Str = httpResult.Html.Split('(')[1].Split(')')[0];
                                obj = JsonConvert.DeserializeObject(Str);
                            }
                            catch
                            {
                                string Str = httpResult.Html.Substring(7, httpResult.Html.Length - 8);
                                obj = JsonConvert.DeserializeObject(Str);
                            }
                        }
                        catch
                        {
                            obj = JsonConvert.DeserializeObject(httpResult.Html);
                        }
                        JObject bill = obj as JObject;
                        if (bill["retCode"].ToString() != "000000")
                        {
                            logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = title + "第" + Math.Ceiling(curCuror.ToString().ToDecimal(1) / 50) + "页失败：" + bill["retCode"].ToString();
                            appLog.LogDtlList.Add(logDtl);
                            if (curCuror > totalNum)
                                break;
                        }
                        else
                        {
                            crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + "0" + (i + 1) + pageIndex, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                            logDtl.StatusCode = ServiceConsts.StatusCode_success;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = title + "成功";
                            appLog.LogDtlList.Add(logDtl);
                        }
                        totalNum = bill["totalNum"].ToString().ToInt(0);
                        pageIndex = Math.Ceiling(curCuror.ToString().ToDecimal(1) / pageSize).ToString().ToInt(1);
                        curCuror = curCuror + pageSize;
                    }
                    catch (Exception e)
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_error;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = title + "第" + Math.Ceiling(curCuror.ToString().ToDecimal(1) / 50) + "页异常：" + e.Message + "原始数据：" + httpResult.Html;
                        appLog.LogDtlList.Add(logDtl);
                        pageIndex = Math.Ceiling(curCuror.ToString().ToDecimal(1) / pageSize).ToString().ToInt(1);
                        curCuror = curCuror + pageSize;

                        continue;
                    }
                }
                while (curCuror <= totalNum);

            }
        }

        /// <summary>
        /// 解析手机详单
        /// </summary>
        /// <param name="queryType">GSM:通话；GPRS:上网；SMS:短信/彩信</param>
        /// <returns></returns>
        private void ReadDeatils(EnumMobileDeatilType type, CrawlerData crawler, Action<JObject, string> action)
        {
            List<CrawlerDtlData> PhoneCrawlerDtls = new List<CrawlerDtlData>();
            string PhoneCostStr = string.Empty;
            string Str = string.Empty;
            string year = DateTime.Now.Year.ToString();
            object obj;
            DateTime date;

            for (int i = 0; i < 6; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7));
                try
                {
                    PhoneCrawlerDtls = crawler.DtlList.Where(x => x.CrawlerTitle.StartsWith(type + "0" + (i + 1))).OrderBy(x => x.CrawlerTitle).ToList<CrawlerDtlData>();
                    if (PhoneCrawlerDtls != null && PhoneCrawlerDtls.Count > 0)
                    {
                        foreach (CrawlerDtlData item in PhoneCrawlerDtls)
                        {
                            PhoneCostStr = System.Text.Encoding.Default.GetString(item.CrawlerTxt);
                            if (PhoneCostStr.IsEmpty()) continue;
                            try
                            {
                                try
                                {
                                    Str = PhoneCostStr.Split('(')[1].Split(')')[0];
                                    obj = JsonConvert.DeserializeObject(Str);
                                }
                                catch
                                {
                                    Str = PhoneCostStr.Substring(7, PhoneCostStr.Length - 8);
                                    obj = JsonConvert.DeserializeObject(Str);
                                }
                            }
                            catch
                            {
                                obj = JsonConvert.DeserializeObject(PhoneCostStr);
                            }
                            JObject bill = obj as JObject;
                            if (bill["retCode"].ToString() != "000000") continue;
                            if (bill != null)
                            {
                                year = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).Year.ToString();
                                action(bill, year);
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
        }

        /// <summary>
        /// 账单
        /// </summary>
        /// <param name=ServiceConsts.SpiderType_Mobile></param>
        private void GetMobileBill(string mobile, CrawlerData crawler)
        {
            logDtl = new ApplyLogDtl("账单抓取");

            string Url = String.Format("http://shop.10086.cn/i/v1/fee/billinfo/{0}?time=201511911748417", mobile);
            httpItem = new HttpItem()
            {
                Accept = "application/json, text/javascript, */*; q=0.01",
                URL = Url,
                Referer = "http://shop.10086.cn/i/?f=home",
                Cookie = cookieStr
            };
            httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "账单抓取");
            if (httpResult.StatusCode == HttpStatusCode.OK)
            {
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "单抓取成功";
                appLog.LogDtlList.Add(logDtl);
            }
        }

        /// <summary>
        /// 账单
        /// </summary>
        /// <param name=ServiceConsts.SpiderType_Mobile></param>
        private void ReadMobileBill(CrawlerData crawler, Basic mobile)
        {
            string PhoneBillStr = string.Empty;
            object Infoobj = null;
            MonthBill bill = null;

            logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis, "账单解析");
            try
            {
                if (crawler.DtlList.Where(x => x.CrawlerTitle == "bill").FirstOrDefault() == null) return;
                PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "bill").FirstOrDefault().CrawlerTxt);
                Infoobj = JsonConvert.DeserializeObject(PhoneBillStr);
                JObject Infojs = Infoobj as JObject;
                if (Infojs != null)
                {
                    if (Infojs["retCode"].ToString() == "000000")
                    {
                        JArray detailList = Infojs["data"] as JArray;
                        if (detailList != null)
                        {
                            for (int i = 0; i < detailList.Count; i++)
                            {
                                bill = new MonthBill();
                                bill.BillCycle = detailList[i]["billMonth"].ToString();
                                if (!bill.BillCycle.IsEmpty())
                                    bill.BillCycle = DateTime.Parse(bill.BillCycle.Substring(0, 4) + "-" + bill.BillCycle.Substring(4, 2) + "-" + "01").ToString(Consts.DateFormatString12);
                                bill.TotalAmt = detailList[i]["billFee"].ToString();
                                JArray billMaterials = detailList[i]["billMaterials"] as JArray;
                                bill.PlanAmt = billMaterials[0]["billEntriyValue"].ToString();
                                mobile.BillList.Add(bill);
                            }
                        }
                    }
                }
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单解析成功";
                appLog.LogDtlList.Add(logDtl);
            }
            catch (Exception e)
            {
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单解析异常：" + e.Message;
                appLog.LogDtlList.Add(logDtl);
            }

        }

        /// <summary>
        /// 设置useUnsafeHeaderParsing
        /// </summary>
        /// <returns></returns>
        public static bool SetAllowUnsafeHeaderParsing()
        {
            //Get the assembly that contains the internal class
            Assembly aNetAssembly = Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection));
            if (aNetAssembly != null)
            {
                //Use the assembly in order to get the internal type for 
                // the internal class
                Type aSettingsType = aNetAssembly.GetType("System.Net.Configuration.SettingsSectionInternal");
                if (aSettingsType != null)
                {
                    //Use the internal static property to get an instance 
                    // of the internal settings class. If the static instance 
                    // isn't created allready the property will create it for us.
                    object anInstance = aSettingsType.InvokeMember("Section",
                    BindingFlags.Static | BindingFlags.GetProperty
                    | BindingFlags.NonPublic, null, null, new object[] { });
                    if (anInstance != null)
                    {
                        //Locate the private bool field that tells the 
                        // framework is unsafe header parsing should be 
                        // allowed or not
                        FieldInfo aUseUnsafeHeaderParsing = aSettingsType.GetField(
                        "useUnsafeHeaderParsing",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                        if (aUseUnsafeHeaderParsing != null)
                        {
                            aUseUnsafeHeaderParsing.SetValue(anInstance, true);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 校验是否需要验证码
        /// </summary>
        /// <returns></returns>
        private bool CheckNeedVerify(MobileReq mobileReq)
        {
            string Url = string.Empty;
            string flag = string.Empty;
            try
            {
                logDtl = new ApplyLogDtl("校验是否需要验证码");

                Url = "https://login.10086.cn/needVerifyCode.htm?accountType=01&account={0}&timestamp={1}";
                Url = string.Format(Url, mobileReq.Mobile, GetTimeStamp());
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Referer = "https://login.10086.cn/html/login/login.html?channelID=12002&backUrl=http%3A%2F%2Fshop.10086.cn%2Fmall_100_100.html%3Fforcelogin%3D1",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验是否需要验证码");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                    flag = CommonFun.GetMidStr(httpResult.Html, "needVerifyCode\":\"", "\"}").Trim();

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.Description = "校验是否需要验证码成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                return flag != "0" ? true : false;
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(GetProvince(mobileReq.Website) + "移动校验异常", e);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            return true;
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        private static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 01, 01, 00, 00, 00, 0000);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }

        /// <summary>
        /// 获取采集网站
        /// </summary>
        /// <param name="website">区域码</param>
        /// <returns></returns>
        private string GetProvince(string website)
        {
            string region = string.Empty;
            string[] mobileStr = website.Split('_');
            region = CommonFun.GetProvinceName(mobileStr[1]);
            return region;
        }
        #endregion
    }
}
