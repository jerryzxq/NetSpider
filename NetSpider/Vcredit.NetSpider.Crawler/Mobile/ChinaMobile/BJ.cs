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
using System.Threading;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class BJ : ChinaMobile
    {
        //#region 公共变量
        //IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        //IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        //IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        //CookieCollection cookies = new CookieCollection();
        //string cookieStr = string.Empty;
        //HttpHelper httpHelper = new HttpHelper();
        //HttpResult httpResult = null;
        //HttpItem httpItem = null;
        //MobileMongo mobileMongo = new MobileMongo();
        //ApplyLogMongo logMongo = new ApplyLogMongo();
        //#endregion


        ///// <summary>
        ///// 页面初始化
        ///// </summary>
        ///// <returns></returns>
        //public VerCodeRes MobileInit(MobileReq mobileReq = null)
        //{
        //    VerCodeRes Res = new VerCodeRes();
        //    cookies = new CookieCollection();
        //    ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
        //    string Url = string.Empty;
        //    string token = CommonFun.GetGuidID();
        //    Res.Token = token;
        //    try
        //    {
        //        //第一步，初始化登录页面
        //        Url = "https://login.10086.cn/html/login/login.html?channelID=12002&backUrl=http%3A%2F%2Fshop.10086.cn%2Fmall_100_100.html%3Fforcelogin%3D1";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Cookie = "CmProvid=bj;",
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew("", httpResult.Cookie);

        //        if (CheckNeedVerify(mobileReq))
        //        {
        //            //第二步，获取验证码
        //            Url = "https://login.10086.cn/captchazh.htm?type=05";
        //            httpItem = new HttpItem()
        //            {
        //                URL = Url,
        //                ResultType = ResultType.Byte,
        //                Referer = "https://login.10086.cn/html/login/login.html?channelID=12002&backUrl=http%3A%2F%2Fshop.10086.cn%2Fmall_100_100.html%3Fforcelogin%3D1",
        //                Cookie = cookieStr
        //            };
        //            httpResult = httpHelper.GetHtml(httpItem);
        //            if (httpResult.StatusCode != HttpStatusCode.OK)
        //            {
        //                Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //                Res.StatusCode = ServiceConsts.StatusCode_fail;
        //                return Res;
        //            }
        //            cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

        //            FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
        //            Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
        //            Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
        //        }

        //        // FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
        //        Res.nextProCode = ServiceConsts.NextProCode_Login;
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.StatusDescription = "北京移动初始化完成";

        //        CacheHelper.SetCache(token, cookieStr);
        //        //CacheHelper.SetCache(token, cookies);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "北京移动初始化异常";
        //        Log4netAdapter.WriteError("北京移动初始化异常", e);
        //        appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_Init)
        //        {
        //            CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
        //            StatusCode = ServiceConsts.StatusCode_error,
        //            Description = e.Message
        //        });
        //    }
        //    finally
        //    {
        //        appLog.Token = Res.Token;
        //        appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
        //        if (appLog.LogDtlList.Count < 1)
        //        {
        //            appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_Init)
        //            {
        //                CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
        //                StatusCode = Res.StatusCode,
        //                Description = Res.StatusDescription
        //            });
        //        }
        //        logMongo.Save(appLog);
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
        //    ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
        //    BaseRes Res = new BaseRes();
        //    Res.Token = mobileReq.Token;
        //    string artifact = string.Empty;
        //    string Url = string.Empty;
        //    string result = string.Empty;
        //    try
        //    {
        //        //获取缓存CacheHelper
        //        if (CacheHelper.GetCache(mobileReq.Token) != null)
        //        {
        //            cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
        //        }
        //        //校验参数
        //        if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
        //        {
        //            Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }

        //        #region 登录

        //        if (!mobileReq.Vercode.IsEmpty())
        //        {
        //            Url = string.Format("https://login.10086.cn/verifyCaptcha?inputCode={0}", mobileReq.Vercode);
        //            httpItem = new HttpItem()
        //            {
        //                Accept = "application/json, text/javascript, */*; q=0.01",
        //                URL = Url,
        //                Referer = "https://login.10086.cn/html/login/login.html?channelID=12002&backUrl=http%3A%2F%2Fshop.10086.cn%2Fmall_100_100.html%3Fforcelogin%3D1",
        //                Cookie = cookieStr
        //            };
        //            httpResult = httpHelper.GetHtml(httpItem);
        //            result = CommonFun.GetMidStr(httpResult.Html, "resultCode\":\"", "\"}");
        //            if (!result.IsEmpty() && result != "0")
        //            {
        //                Res.StatusDescription = "验证码错误！";
        //                Res.StatusCode = ServiceConsts.StatusCode_fail;
        //                return Res;
        //            }
        //            cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
        //        }


        //        Url = string.Format("https://login.10086.cn/needVerifyCode.htm?accountType=01&account={0}&timestamp=1449216601653", mobileReq.Mobile);
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "application/json, text/javascript, */*; q=0.01",
        //            URL = Url,
        //            Referer = "ttps://login.10086.cn/login.html?channelID=12003&backUrl=http://shop.10086.cn/i/?f=home",
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

        //        Url = string.Format("https://login.10086.cn/login.htm?accountType=01&account={0}&password={1}&pwdType=01&inputCode={2}&backUrl=http%3A%2F%2Fshop.10086.cn%2Fmall_100_100.html%3Fforcelogin%3D1&rememberMe=0&channelID=12002&protocol=https%3A&timestamp={3}", mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode, GetTimeStamp());
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "application/json, text/javascript, */*; q=0.01",
        //            URL = Url,
        //            Referer = "https://login.10086.cn/html/login/login.html?channelID=12002&backUrl=http%3A%2F%2Fshop.10086.cn%2Fmall_100_100.html%3Fforcelogin%3D1",
        //            Cookie = cookieStr
        //        };
        //        SetAllowUnsafeHeaderParsing();  //设置useUnsafeHeaderParsing
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        result = CommonFun.GetMidStr(httpResult.Html, "result\":\"", "\"");
        //        if (!result.IsEmpty() && result != "0")
        //        {
        //            Res.StatusDescription = jsonParser.GetResultFromParser(httpResult.Html, "desc");
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
        //        artifact = jsonParser.GetResultFromParser(httpResult.Html, "artifact");
        //        var uid = jsonParser.GetResultFromParser(httpResult.Html, "uid");

        //        Url = "https://login.10086.cn/login.html?channelID=12003&backUrl=http://shop.10086.cn/i/?f=home";
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        //            URL = Url,
        //            Referer = "http://shop.10086.cn/i/?f=home",
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);


        //        Cookie cCookie = new Cookie("c", uid, "/", ".10086.cn");
        //        cookies.Add(cCookie);
        //        Url = "https://login.10086.cn/checkUidAvailable.action";
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "application/json, text/javascript, */*; q=0.01",
        //            URL = Url,
        //            Method = "Post",
        //            Postdata = "uid=uid",
        //            Referer = "https://login.10086.cn/login.html?channelID=12003&backUrl=http://shop.10086.cn/i/?f=home",
        //            Cookie = cookieStr,
        //            Host = "login.10086.cn"
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

        //        #endregion

        //        #region 校验移动商城

        //        Url = string.Format("http://shop.10086.cn/sso/getartifact.php?backUrl=http%3A%2F%2Fshop.10086.cn%2Fmall_100_100.html%3Fforcelogin%3D1&artifact={0}", artifact);
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        //            URL = Url,
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

        //        Url = "http://shop.10086.cn/mall_100_100.html";
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        //            URL = Url,
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

        //        Url = "http://shop.10086.cn/ajax/user/userinfo.json?update=1&province_id=100&city_id=100&callback=initUser";
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "application/javascript, */*;q=0.8",
        //            URL = Url,
        //            Referer = "http://shop.10086.cn/mall_100_100.html",
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

        //        Url = "http://search.10086.cn/shop/defword_jsonpcallback.json?jsonpcallback=jQuery11020553242172670676_" + GetTimeStamp() + "&areacode=100";
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "application/javascript, */*;q=0.8",
        //            URL = Url,
        //            Referer = "http://shop.10086.cn/mall_100_100.html",
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

        //        Url = "http://shop.10086.cn/ajax/recharge/recharge.json?province_id=100";
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "application/json, text/javascript, */*; q=0.01",
        //            URL = Url,
        //            Referer = "http://shop.10086.cn/mall_100_100.html",
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

        //        #endregion

        //        #region 校验个人中心

        //        Url = "http://shop.10086.cn/i/?f=home";
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        //            URL = Url,
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

        //        Url = "http://shop.10086.cn/i/v1/auth/loginfo?time=201512413280429";
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "application/json, text/javascript, */*; q=0.01",
        //            URL = Url,
        //            Referer = "http://shop.10086.cn/i/?f=home",
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

        //        Url = "https://login.10086.cn/SSOCheck.action?channelID=12003&backUrl=http://shop.10086.cn/i/?f=home";
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        //            Allowautoredirect = false,
        //            URL = Url,
        //            Referer = "http://shop.10086.cn/i/?f=home",
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
        //        artifact = CommonFun.GetMidStr(CommonFun.GetMidStr(httpResult.RedirectUrl, "artifact=", ""), "", "&");

        //        Url = string.Format("http://shop.10086.cn/i/v1/auth/getArtifact?artifact={0}&backUrl=http%3A%2F%2Fshop.10086.cn%2Fi%2F%3Ff%3Dhome", artifact);
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "text/html, application/xhtml+xml, */*",
        //            URL = Url,
        //            Referer = "http://shop.10086.cn/i/?f=home",
        //            Cookie = cookieStr

        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

        //        Url = "http://shop.10086.cn/i/v1/auth/loginfo?time=201512413280429";
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "application/json, text/javascript, */*; q=0.01",
        //            URL = Url,
        //            Referer = "http://shop.10086.cn/i/?f=home",
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

        //        Url = string.Format("http://shop.10086.cn/i/v1/auth/loginfoaccess?time=20151110938210", mobileReq.Mobile);
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "application/json, text/javascript, */*; q=0.01",
        //            URL = Url,
        //            Referer = "http://shop.10086.cn/i/?f=home",
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        result = CommonFun.GetMidStr(httpResult.Html, "retMsg\":\"", "\"}");
        //        if (result != "success")
        //        {
        //            Res.StatusDescription = "用户未登录";
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

        //        #endregion

        //        Res.StatusDescription = "北京移动登录成功";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.nextProCode = ServiceConsts.NextProCode_SendSMS;

        //        CacheHelper.SetCache(mobileReq.Token, cookieStr);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "北京移动登录异常";
        //        Log4netAdapter.WriteError("北京移动登录异常", e);
        //        appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_Login)
        //        {
        //            CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
        //            StatusCode = ServiceConsts.StatusCode_error,
        //            Description = e.Message
        //        });
        //    }
        //    finally
        //    {
        //        appLog.Token = Res.Token;
        //        appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
        //        if (appLog.LogDtlList.Count < 1)
        //        {
        //            appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_Login)
        //            {
        //                CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
        //                StatusCode = Res.StatusCode,
        //                Description = Res.StatusDescription
        //            });
        //        }
        //        logMongo.Save(appLog);
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

        //    ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
        //    Thread.Sleep(35000);
        //    VerCodeRes Res = new VerCodeRes();
        //    Res.Token = mobileReq.Token;
        //    cookieStr = string.Empty;
        //    string Url = string.Empty;
        //    string result = string.Empty;
        //    try
        //    {
        //        //获取缓存
        //        if (CacheHelper.GetCache(mobileReq.Token) != null)
        //        {
        //            cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
        //        }
        //        //校验参数
        //        if (mobileReq.Mobile.IsEmpty())
        //        {
        //            Res.StatusDescription = "手机号不能为空";
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }

        //        //第五步，发手机验证码
        //        Url = string.Format("https://login.10086.cn/sendSMSpwd.action?callback=result&userName={0}&_=1447036853654", mobileReq.Mobile);
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "*/*",
        //            URL = Url,
        //            Referer = "http://shop.10086.cn/i/?f=billdetailqry",
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        result = CommonFun.GetMidStr(httpResult.Html, "resultCode\":\"", "\"}");
        //        if (result != "0")
        //        {
        //            Res.StatusDescription = "验证码发送失败";
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

        //        Res.StatusDescription = "北京移动手机验证码发送成功";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;

        //        CacheHelper.SetCache(mobileReq.Token, cookieStr);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "北京移动手机验证码发送异常";
        //        Log4netAdapter.WriteError("北京移动手机验证码发送异常", e);
        //        appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_SendSMS)
        //        {
        //            CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
        //            StatusCode = ServiceConsts.StatusCode_error,
        //            Description = e.Message
        //        });
        //    }
        //    finally
        //    {
        //        appLog.Token = Res.Token;
        //        appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
        //        if (appLog.LogDtlList.Count < 1)
        //        {
        //            appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_SendSMS)
        //            {
        //                CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
        //                StatusCode = Res.StatusCode,
        //                Description = Res.StatusDescription
        //            });
        //        }
        //        logMongo.Save(appLog);
        //    }
        //    return Res;
        //}

        ///// <summary>
        ///// 校验短信验证码
        ///// </summary>
        ///// <param name="mobileReq"></param>
        ///// <returns></returns>
        //public BaseRes MobileCheckSms(MobileReq mobileReq)
        //{
        //    ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
        //    BaseRes Res = new BaseRes();
        //    Res.Token = mobileReq.Token;
        //    cookieStr = string.Empty;
        //    string Url = string.Empty;
        //    try
        //    {
        //        //获取缓存
        //        if (CacheHelper.GetCache(mobileReq.Token) != null)
        //            cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
        //        //校验参数
        //        if (mobileReq.Mobile.IsEmpty())
        //        {
        //            Res.StatusDescription = ServiceConsts.MobileEmpty;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }

        //        //第六步，验证手机验证码
        //        Url = string.Format("https://login.10086.cn/temporaryauthSMSandService.action?callback=result&account={0}&servicePwd={1}&smsPwd={2}&accountType=01&backUrl=&channelID=12003&businessCode=01&_=1447036497014", mobileReq.Mobile, mobileReq.Password, mobileReq.Smscode);
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "*/*",
        //            URL = Url,
        //            Referer = "http://shop.10086.cn/i/?f=billdetailqry",
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        string result = CommonFun.GetMidStr(httpResult.Html, "result\":\"", "\"}");
        //        if (!result.IsEmpty() && result != "0000")
        //        {
        //            Res.StatusDescription = CommonFun.GetMidStr(httpResult.Html, "desc\":\"", "\",");
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

        //        Res.StatusDescription = "北京移动手机验证码验证成功";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.nextProCode = ServiceConsts.NextProCode_Query;

        //        CacheHelper.SetCache(mobileReq.Token, cookieStr);

        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "北京移动手机验证码验证异常";
        //        Log4netAdapter.WriteError("北京移动手机验证码验证异常", e);
        //        appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_CheckSMS)
        //        {
        //            CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
        //            StatusCode = ServiceConsts.StatusCode_error,
        //            Description = e.Message
        //        });
        //    }
        //    finally
        //    {
        //        appLog.Token = Res.Token;
        //        appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
        //        if (appLog.LogDtlList.Count < 1)
        //        {
        //            appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_CheckSMS)
        //            {
        //                CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
        //                StatusCode = Res.StatusCode,
        //                Description = Res.StatusDescription
        //            });
        //        }
        //        logMongo.Save(appLog);
        //    }
        //    return Res;
        //}

        ///// <summary>
        ///// 手机抓取
        ///// </summary>
        ///// <param name="mobileReq"></param>
        ///// <returns></returns>
        //public BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        //{
        //    Thread.Sleep(30000);
        //    ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
        //    ApplyLogDtl logDtl = new ApplyLogDtl("");
        //    BaseRes Res = new BaseRes();
        //    Res.Token = mobileReq.Token;
        //    cookies = new CookieCollection();
        //CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
        //CrawlerData crawler = new CrawlerData() { Token = mobileReq.Token, IdentityCard = mobileReq.IdentityCard, Name = mobileReq.Name, Mobile = mobileReq.Mobile, UserType = "1" };
        //    string Url = string.Empty;
        //    try
        //    {
        //        //获取缓存
        //        if (CacheHelper.GetCache(mobileReq.Token) != null)
        //            cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);

        //        #region 个人信息

        //        logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Crawler);
        //        Url = string.Format("http://shop.10086.cn/i/v1/cust/info/{0}?time=201511911294663", mobileReq.Mobile);
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "application/json, text/javascript, */*; q=0.01",
        //            URL = Url,
        //            Referer = "http://shop.10086.cn/i/?f=home",
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

        //        //积分
        //        Url = string.Format("http://shop.10086.cn/i/v1/point/sum/{0}?time=201511911294663", mobileReq.Mobile);
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "application/json, text/javascript, */*; q=0.01",
        //            URL = Url,
        //            Referer = "http://shop.10086.cn/i/?f=home",
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

        //        logDtl.StatusCode = ServiceConsts.StatusCode_success;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = "基本信息抓取成功";
        //        appLog.LogDtlList.Add(logDtl);



        //        #endregion

        //        #region 话费帐单
        //        logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Crawler);

        //        GetMobileBill(mobileReq.Mobile, crawler);

        //        logDtl.StatusCode = ServiceConsts.StatusCode_success;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = "账单抓取成功";
        //        appLog.LogDtlList.Add(logDtl);
        //        #endregion
        //        Thread.Sleep(10000);
        //        //套餐
        //        GetDeatils("01", mobileReq.Mobile, crawler);

        //        #region 通话详单
        //        logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Crawler);

        //        GetDeatils("02", mobileReq.Mobile, crawler);

        //        logDtl.StatusCode = ServiceConsts.StatusCode_success;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = "通话详单抓取成功";
        //        appLog.LogDtlList.Add(logDtl);
        //        #endregion
        //        Thread.Sleep(10000);
        //        #region 短信详单
        //        logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Crawler);

        //        GetDeatils("03", mobileReq.Mobile, crawler);

        //        logDtl.StatusCode = ServiceConsts.StatusCode_success;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = "短信详单抓取成功";
        //        appLog.LogDtlList.Add(logDtl);
        //        #endregion
        //        Thread.Sleep(10000);
        //        #region 上网详单
        //        logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Crawler);

        //        GetDeatils("04", mobileReq.Mobile, crawler);

        //        logDtl.StatusCode = ServiceConsts.StatusCode_success;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = "上网详单抓取成功";
        //        appLog.LogDtlList.Add(logDtl);
        //        #endregion

        //        crawlerMobileMongo.SaveCrawler(crawler);
        //        Res.StatusDescription = "北京移动手机账单抓取成功";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;

        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusDescription = "北京移动手机账单抓取异常";
        //        Res.StatusCode = ServiceConsts.StatusCode_fail;
        //        Log4netAdapter.WriteError("北京移动手机账单抓取异常", e);

        //        logDtl.StatusCode = ServiceConsts.StatusCode_fail;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = e.Message;
        //        appLog.LogDtlList.Add(logDtl);
        //    }
        //    finally
        //    {
        //        appLog.Token = Res.Token;
        //        appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
        //        if (Res.StatusCode == ServiceConsts.StatusCode_fail && logDtl.Description.IsEmpty())
        //        {
        //            logDtl.StatusCode = ServiceConsts.StatusCode_fail;
        //            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //            logDtl.Description = Res.StatusDescription;
        //            appLog.LogDtlList.Add(logDtl);
        //        }
        //        logMongo.Save(appLog);
        //    }
        //    return Res;
        //}

        ///// <summary>
        ///// 手机解析
        ///// </summary>
        ///// <param name="mobileReq"></param>
        ///// <returns></returns>
        //public BaseRes MobileAnalysis(MobileReq mobileReq, DateTime appDate)
        //{
        //    BaseRes Res = new BaseRes();
        //    ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
        //    ApplyLogDtl logDtl = new ApplyLogDtl("");
        //    CrawlerData crawler = new CrawlerData();
        //    Basic mobile = new Basic();
        //    Call call = null;
        //    Net gprs = null;
        //    Sms sms = null;
        //    Res.Token = mobileReq.Token;
        //    string result = string.Empty;
        //    object Infoobj = null;
        //    JObject Infojs = null;
        //    try
        //    {
        //        if (CacheHelper.GetCache(mobileReq.Token) != null)
        //            cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
        //        crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);

        //        #region 个人信息
        //        logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Analysis);

        //        //基本信息
        //        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
        //        mobile.BusIdentityCard = mobileReq.IdentityCard;
        //        mobile.BusName = mobileReq.Name;
        //        mobile.Token = mobileReq.Token;
        //        mobile.Mobile = mobileReq.Mobile;
         //       mobile.UpdateTime = crawler.CrawlerDate;
        //        string retCode = CommonFun.GetMidStr(result, "retCode\":\"", "\",");
        //        if (!retCode.IsEmpty() && retCode == "000000")
        //        {
        //            try
        //            {
        //                Infoobj = JsonConvert.DeserializeObject(result);
        //                Infojs = Infoobj as JObject;
        //                Infojs = Infojs["data"] as JObject;
        //                if (Infojs != null)
        //                {
        //                    mobile.Name = Infojs["name"].ToString();
        //                    mobile.PackageBrand = Infojs["brand"].ToString() == "01" ? "全球通" : (Infojs["brand"].ToString() == "03" ? "动感地带" : "神州行");
        //                    // mobile.Integral = Infojs[2].ToString();
        //                    mobile.StarLevel = Infojs["starLevel"].ToString();
        //                    mobile.Regdate = Infojs["inNetDate"].ToString();
        //                    if (!mobile.Regdate.IsEmpty())
        //                        mobile.Regdate = DateTime.Parse(mobile.Regdate.Substring(0, 4) + "-" + mobile.Regdate.Substring(4, 2) + "-" + mobile.Regdate.Substring(6, 2)).ToString(Consts.DateFormatString11);
        //                    mobile.Address = Infojs["address"].ToString();
        //                    mobile.Email = Infojs["email"].ToString();
        //                    mobile.Postcode = Infojs["zipCode"].ToString();
        //                }
        //            }
        //            catch { }
        //        }

        //        //积分
        //        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
        //        retCode = CommonFun.GetMidStr(result, "retCode\":\"", "\",");
        //        if (!retCode.IsEmpty() && retCode == "000000")
        //        {
        //            try
        //            {
        //                Infoobj = JsonConvert.DeserializeObject(result);
        //                Infojs = Infoobj as JObject;
        //                if (Infojs != null)
        //                {
        //                    JObject data = Infojs["data"] as JObject;
        //                    if (data != null)
        //                        mobile.Integral = data["pointValue"].ToString();
        //                }
        //            }
        //            catch { }
        //        }

        //        //套餐
        //        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault().CrawlerTxt);
        //        if (result.Split('(').Count() > 1)
        //        {
        //            result = result.Split('(')[1].Split(')')[0];
        //        }
        //        try
        //        {
        //            Infoobj = JsonConvert.DeserializeObject(result);
        //            Infojs = Infoobj as JObject;
        //            if (Infojs != null)
        //            {
        //                JArray arrdata = Infojs["data"] as JArray;
        //                if (arrdata != null)
        //                {
        //                    mobile.Package = arrdata[0]["mealName"] != null ? arrdata[0]["mealName"].ToString() : "";  //套餐
        //                }
        //            }
        //        }
        //        catch
        //        {
        //            Infoobj = JsonConvert.DeserializeObject(result.Replace("[", "").Replace("]", ""));
        //            Infojs = Infoobj as JObject;
        //            if (Infojs != null)
        //            {
        //                JObject data = Infojs["data"] as JObject;
        //                if (data != null)
        //                {
        //                    mobile.Package = data["mealName"].ToString();  //套餐
        //                }
        //            }
        //        }


        //        logDtl.StatusCode = ServiceConsts.StatusCode_success;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = "个人信息解析成功";
        //        appLog.LogDtlList.Add(logDtl);
        //        #endregion

        //        #region 话费帐单
        //        logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis);

        //        ReadMobileBill(crawler, mobile);

        //        logDtl.StatusCode = ServiceConsts.StatusCode_success;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = "账单解析成功";
        //        appLog.LogDtlList.Add(logDtl);
        //        #endregion

        //        #region 话费详单
        //        logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Analysis);

        //        ReadDeatils(crawler, "02", delegate(JObject bill, string year)
        //        {
        //            JArray detailList = bill["data"] as JArray;
        //            if (detailList != null && detailList.Count > 0)
        //            {
        //                for (int i = 0; i < detailList.Count; i++)
        //                {
        //                    JObject detail = detailList[i] as JObject;
        //                    if (detail != null)
        //                    {
        //                        var totalSecond = 0;
        //                        var usetime = detail["commTime"].ToString();
        //                        if (!string.IsNullOrEmpty(usetime))
        //                        {
        //                            totalSecond = CommonFun.ConvertDate(usetime);
        //                        }


        //                        call = new Call();
        //                        call.StartTime = DateTime.Parse(year + "-" + detail["startTime"].ToString()).ToString(Consts.DateFormatString11);
        //                        call.CallPlace = detail["commPlac"].ToString();
        //                        call.InitType = detail["commMode"].ToString();
        //                        call.OtherCallPhone = detail["anotherNm"].ToString();
        //                        call.UseTime = totalSecond + "秒";
        //                        call.CallType = detail["commType"].ToString();
        //                        call.SubTotal = detail["commFee"].ToString().ToDecimal(0);
        //                        mobile.CallList.Add(call);
        //                    }
        //                }
        //            }
        //        });

        //        logDtl.StatusCode = ServiceConsts.StatusCode_success;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = "通话详单解析成功";
        //        appLog.LogDtlList.Add(logDtl);
        //        #endregion

        //        #region 短信详单
        //        logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Analysis);

        //        ReadDeatils(crawler, "03", delegate(JObject bill, string year)
        //        {
        //            JArray detailList = bill["data"] as JArray;
        //            if (detailList != null && detailList.Count > 0)
        //            {
        //                for (int i = 0; i < detailList.Count; i++)
        //                {
        //                    JObject detail = detailList[i] as JObject;
        //                    if (detail != null)
        //                    {
        //                        sms = new Sms();
        //                        sms.StartTime = DateTime.Parse(year + "-" + detail["startTime"].ToString()).ToString(Consts.DateFormatString11);
        //                        sms.SmsPlace = detail["commPlac"].ToString();
        //                        sms.OtherSmsPhone = detail["anotherNm"].ToString();
        //                        sms.InitType = detail["commMode"].ToString();
        //                        sms.SmsType = detail["infoType"].ToString();
        //                        sms.SubTotal = detail["commFee"].ToString().ToDecimal(0);
        //                        mobile.SmsList.Add(sms);
        //                    }
        //                }
        //            }
        //        });

        //        logDtl.StatusCode = ServiceConsts.StatusCode_success;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = "短信详单解析成功";
        //        appLog.LogDtlList.Add(logDtl);
        //        #endregion

        //        #region 上网详单
        //        logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Analysis);

        //        ReadDeatils(crawler, "04", delegate(JObject bill, string year)
        //        {
        //            JArray detailList = bill["data"] as JArray;
        //            if (detailList != null && detailList.Count > 0)
        //            {
        //                for (int i = 0; i < detailList.Count; i++)
        //                {
        //                    JObject detail = detailList[i] as JObject;
        //                    if (detail != null)
        //                    {
        //                        var totalSecond = 0;
        //                        var usetime = detail["commTime"].ToString();
        //                        if (!string.IsNullOrEmpty(usetime))
        //                        {
        //                            totalSecond = CommonFun.ConvertDate(usetime);
        //                        }

        //                        gprs = new Net();
        //                        gprs.StartTime = DateTime.Parse(year + "-" + detail["startTime"].ToString()).ToString(Consts.DateFormatString11);
        //                        gprs.Place = detail["commPlac"].ToString();
        //                        gprs.PhoneNetType = detail["netPlayType"].ToString();
        //                        gprs.NetType = detail["netType"].ToString();
        //                        gprs.UseTime = totalSecond + "秒";
        //                        gprs.SubFlow = detail["sumFlow"].ToString();
        //                        gprs.SubTotal = detail["commFee"].ToString().ToDecimal(0);
        //                        mobile.NetList.Add(gprs);
        //                    }
        //                }
        //            }
        //        });

        //        logDtl.StatusCode = ServiceConsts.StatusCode_success;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = "上网详单解析成功";
        //        appLog.LogDtlList.Add(logDtl);
        //        #endregion

        //        mobileMongo.SaveBasic(mobile);
        //        Res.StatusDescription = "北京移动手机账单解析成功";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;

        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusDescription = "北京移动手机账单解析异常";
        //        Res.StatusCode = ServiceConsts.StatusCode_fail;
        //        Log4netAdapter.WriteError("北京移动手机账单解析异常", e);

        //        logDtl.StatusCode = ServiceConsts.StatusCode_fail;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = e.Message;
        //        appLog.LogDtlList.Add(logDtl);
        //    }
        //    finally
        //    {
        //        appLog.Token = Res.Token;
        //        appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logMongo.Save(appLog);
        //    }
        //    return Res;
        //}

        //#region 私有方法

        ///// <summary>
        ///// 抓取手机详单
        ///// </summary>
        ///// <param name="queryType">GSM:通话；GPRS:上网；SMS:短信/彩信；RC 套餐</param>
        ///// <returns></returns>
        //private void GetDeatils(string queryType, string mobile, CrawlerData crawler)
        //{
        //    DateTime date = DateTime.Now;
        //    string Url = string.Empty;
        //    for (int i = 0; i < 6; i++)
        //    {
        //        date = DateTime.Now.AddMonths(-i);
        //        Url = string.Format("https://shop.10086.cn/i/v1/fee/detailbillinfojsonp/{0}?callback=result&curCuror=1&step=100&qryMonth={1}&billType={2}&time=2015119104451249&_=1447037867618", mobile, date.ToString("yyyyMM"), queryType);
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "*/*",
        //            URL = Url,
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (queryType == "01")
        //        {
        //            crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "packageInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
        //            return;
        //        }
        //        crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = queryType + "0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
        //    }
        //}

        ///// <summary>
        ///// 解析手机详单
        ///// </summary>
        ///// <param name="queryType">GSM:通话；GPRS:上网；SMS:短信/彩信</param>
        ///// <returns></returns>
        //private void ReadDeatils(CrawlerData crawler, string queryType, Action<JObject, string> action)
        //{
        //    string PhoneCostStr = string.Empty;
        //    string year = DateTime.Now.Year.ToString();
        //    for (int i = 0; i < 6; i++)
        //    {
        //        PhoneCostStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (queryType + "0" + (i + 1))).FirstOrDefault().CrawlerTxt);
        //        PhoneCostStr = PhoneCostStr.Split('(')[1].Split(')')[0];
        //        object obj = JsonConvert.DeserializeObject(PhoneCostStr);
        //        JObject bill = obj as JObject;
        //        if (bill != null)
        //        {
        //            year = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).Year.ToString();
        //            action(bill, year);
        //        }
        //    }
        //}

        ///// <summary>
        ///// 账单
        ///// </summary>
        ///// <param name=ServiceConsts.SpiderType_Mobile></param>
        //private void GetMobileBill(string mobile, CrawlerData crawler)
        //{
        //    string Url = String.Format("http://shop.10086.cn/i/v1/fee/billinfo/{0}?time=201511911748417", mobile);
        //    httpItem = new HttpItem()
        //    {
        //        Accept = "application/json, text/javascript, */*; q=0.01",
        //        URL = Url,
        //        Referer = "http://shop.10086.cn/i/?f=home",
        //        Cookie = cookieStr
        //    };
        //    httpResult = httpHelper.GetHtml(httpItem);
        //    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
        //}

        ///// <summary>
        ///// 账单
        ///// </summary>
        ///// <param name=ServiceConsts.SpiderType_Mobile></param>
        //private void ReadMobileBill(CrawlerData crawler, Basic mobile)
        //{
        //    string PhoneBillStr = string.Empty;
        //    object Infoobj = null;
        //    MonthBill bill = null;

        //    PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "bill").FirstOrDefault().CrawlerTxt);
        //    try
        //    {
        //        Infoobj = JsonConvert.DeserializeObject(PhoneBillStr);
        //    }
        //    catch { }
        //    JObject Infojs = Infoobj as JObject;
        //    if (Infojs != null)
        //    {
        //        if (Infojs["retCode"].ToString() == "000000")
        //        {
        //            JArray detailList = Infojs["data"] as JArray;
        //            if (detailList != null)
        //            {
        //                for (int i = 0; i < detailList.Count; i++)
        //                {
        //                    //decimal totalbill = 0;
        //                    bill = new MonthBill();
        //                    bill.BillCycle = detailList[i]["billMonth"].ToString();
        //                    if (!bill.BillCycle.IsEmpty())
        //                        bill.BillCycle = DateTime.Parse(bill.BillCycle.Substring(0, 4) + "-" + bill.BillCycle.Substring(4, 2) + "-" + "01").ToString(Consts.DateFormatString12);
        //                    bill.TotalAmt = detailList[i]["billFee"].ToString();
        //                    JArray billMaterials = detailList[i]["billMaterials"] as JArray;
        //                    //for (int j = 0; j < detailList.Count; j++)
        //                    //    totalbill += (billMaterials[j]["billEntriyValue"].ToString().ToDecimal(0));
        //                    bill.PlanAmt = billMaterials[0]["billEntriyValue"].ToString();
        //                    mobile.BillList.Add(bill);
        //                }
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 设置useUnsafeHeaderParsing
        ///// </summary>
        ///// <returns></returns>
        //public static bool SetAllowUnsafeHeaderParsing()
        //{
        //    //Get the assembly that contains the internal class
        //    Assembly aNetAssembly = Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection));
        //    if (aNetAssembly != null)
        //    {
        //        //Use the assembly in order to get the internal type for 
        //        // the internal class
        //        Type aSettingsType = aNetAssembly.GetType("System.Net.Configuration.SettingsSectionInternal");
        //        if (aSettingsType != null)
        //        {
        //            //Use the internal static property to get an instance 
        //            // of the internal settings class. If the static instance 
        //            // isn't created allready the property will create it for us.
        //            object anInstance = aSettingsType.InvokeMember("Section",
        //            BindingFlags.Static | BindingFlags.GetProperty
        //            | BindingFlags.NonPublic, null, null, new object[] { });
        //            if (anInstance != null)
        //            {
        //                //Locate the private bool field that tells the 
        //                // framework is unsafe header parsing should be 
        //                // allowed or not
        //                FieldInfo aUseUnsafeHeaderParsing = aSettingsType.GetField(
        //                "useUnsafeHeaderParsing",
        //                BindingFlags.NonPublic | BindingFlags.Instance);
        //                if (aUseUnsafeHeaderParsing != null)
        //                {
        //                    aUseUnsafeHeaderParsing.SetValue(anInstance, true);
        //                    return true;
        //                }
        //            }
        //        }
        //    }
        //    return false;
        //}

        ///// <summary>
        ///// 校验是否需要验证码
        ///// </summary>
        ///// <returns></returns>
        //private bool CheckNeedVerify(MobileReq mobileReq)
        //{
        //    string Url = string.Empty;
        //    string flag = string.Empty;
        //    try
        //    {
        //        Url = "https://login.10086.cn/needVerifyCode.htm?accountType=01&account={0}&timestamp={1}";
        //        Url = string.Format(Url, mobileReq.Mobile, GetTimeStamp());
        //        httpItem = new HttpItem()
        //        {
        //            Accept = "application/json, text/javascript, */*; q=0.01",
        //            URL = Url,
        //            Referer = "https://login.10086.cn/html/login/login.html?channelID=12002&backUrl=http%3A%2F%2Fshop.10086.cn%2Fmall_100_100.html%3Fforcelogin%3D1",
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
        //        flag = CommonFun.GetMidStr(httpResult.Html, "needVerifyCode\":\"", "\"}").Trim();
        //        return flag != "0" ? true : false;
        //    }
        //    catch (Exception e)
        //    {
        //        Log4netAdapter.WriteError("北京移动校验异常", e);
        //    }
        //    return true;
        //}

        ///// <summary>
        ///// 获取时间戳
        ///// </summary>
        ///// <returns></returns>
        //private static string GetTimeStamp()
        //{
        //    TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 01, 01, 00, 00, 00, 0000);
        //    return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        //}
        //#endregion
    }
}
