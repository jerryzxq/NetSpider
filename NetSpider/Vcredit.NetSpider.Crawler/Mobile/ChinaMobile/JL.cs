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
using Newtonsoft.Json.Linq;
namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class JL : IMobileCrawler
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

        #region 私有方法
        string RelayState = string.Empty;
        string SAMLart = string.Empty;


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
            List<string> results = new List<string>();
            try
            {
                logDtl = new ApplyLogDtl("初始化登录页面");
                Url = "http://www.10086.cn/jl/index_431_431.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "初始化登录页面");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "https://jl.ac.10086.cn/login";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://www.10086.cn/jl/index_431_431.html",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "初始化登录页面");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "初始化登录页面成功";
                appLog.LogDtlList.Add(logDtl);
                //第二步，获取验证码
                logDtl = new ApplyLogDtl("获取验证码");
                Url = "https://jl.ac.10086.cn/SSO/FuJaMaJlWww?0.9241395473899875";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "获取验证码");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "获取验证码成功";
                appLog.LogDtlList.Add(logDtl);

                Res.StatusDescription = "吉林移动初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;

                CacheHelper.SetCache(Res.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "吉林移动初始化异常";
                Log4netAdapter.WriteError("吉林移动初始化异常", e);

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
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                logDtl = new ApplyLogDtl("检验手机号");
                Url = string.Format("https://jl.ac.10086.cn/SSO/CheckMobileAddress.jsp?mobile={0}&_={1}", mobileReq.Mobile, CommonFun.GetTimeStamp());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "https://jl.ac.10086.cn/SSO/login",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "检验手机号");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "检验手机号成功";
                appLog.LogDtlList.Add(logDtl);

                logDtl = new ApplyLogDtl("登录");
                Url = "https://jl.ac.10086.cn/SSO/LoginAuthenticate";
                postdata = string.Format("returnUrl=null&loginType=1&userId={0}&userPassword={1}&fujama={2}", mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "https://jl.ac.10086.cn/SSO/login",
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
                string msg = CommonFun.GetMidStr(httpResult.Html, "var msg=\"", "\"");
                string error = CommonFun.GetMidStr(httpResult.Html, "var error=\"", "\"");
                if (error == "5") Res.StatusDescription = "对不起，系统忙请稍后再试！";
                else if (error == "6") Res.StatusDescription = "对不起，您的IP访问已经受限！";
                else if (error == "2") Res.StatusDescription = "您输入的验证码有误！";
                else if (error == "3") Res.StatusDescription = "您输入的短信随机码有误！";
                else if (error == "4") Res.StatusDescription = msg;
                if (!Res.StatusDescription.IsEmpty())
                {
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "登录成功";
                appLog.LogDtlList.Add(logDtl);

                logDtl = new ApplyLogDtl("登录校验");
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value");
                if (results.Count > 0)
                {
                    SAMLart = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value");
                if (results.Count > 0)
                {
                    RelayState = results[0];
                }
                Url = "http://www.jl.10086.cn/my/SsoPost_10086.jsp";
                postdata = string.Format("RelayState={0}&SAMLart={1}", RelayState, SAMLart);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                    Referer = "https://jl.ac.10086.cn/SSO/LoginAuthenticate"
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录校验my/SsoPost_10086");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = "http://www.jl.10086.cn/my/index.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "http://www.jl.10086.cn/my/SsoPost_10086.jsp",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录校验my/index");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.jl.10086.cn/my/account/index.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://www.jl.10086.cn/my/index.jsp",
                    //Allowautoredirect = false,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录校验account/index");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "登录校验成功";
                appLog.LogDtlList.Add(logDtl);

                Res.StatusDescription = "吉林移动登录成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_SendSMS;

                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "吉林移动登录异常";
                Log4netAdapter.WriteError("吉林移动登录异常", e);

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
            cookies = new CookieCollection();
            string Url = string.Empty;
            string resultCode = string.Empty;
            string postdata = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                #region   发送短信校验
                logDtl = new ApplyLogDtl("发送短信校验");
                Url = "http://www.jl.10086.cn/service/fee/QueryFirstBill2.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://www.jl.10086.cn/my/account/index.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发送短信校验QueryFirstBill2");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.jl.10086.cn/service/onlineservice/isLogin.jsp?rnd=0.5578534239094175";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://www.jl.10086.cn/service/fee/QueryFirstBill2.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发送短信校验isLogin");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.jl.10086.cn/service/weblogin_pop.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://www.jl.10086.cn/service/fee/QueryFirstBill2.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发送短信校验weblogin_pop");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string SAMLRequest = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLRequest']", "value")[0];

                Url = "https://jl.ac.10086.cn/SSO/POST";
                postdata = string.Format("SAMLRequest={0}&RelayState=http%3A%2F%2Fwww.jl.10086.cn%2Fservice%2FSsoPost_pop.jsp&returnUrl=http%3A%2F%2Fwww.jl.10086.cn%2Fservice%2FSsoPost_pop.jsp", SAMLRequest.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://www.jl.10086.cn/service/weblogin_pop.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发送短信校验SSO/POST");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                SAMLart = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value")[0];
                RelayState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value")[0];

                Url = "http://www.jl.10086.cn/service/SsoPost_pop.jsp";
                postdata = string.Format("SAMLart={0}&RelayState={1}", SAMLart, RelayState);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://www.jl.10086.cn/service/weblogin_pop.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发送短信校验SsoPost_pop");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.jl.10086.cn/service/fee/QueryDetailList_3303.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发送短信校验成功QueryDetailList_3303");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "发送短信校验成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion


                #region 发手机动态验证码
                logDtl = new ApplyLogDtl("发手机动态验证码");
                Url = string.Format("http://www.jl.10086.cn/service/operate/action/SendSmsCheckCode_sendSmsCode?randomStr=1446800222398&type=query");
                //发手机验证码
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发手机动态验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "发手机动态验证码成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                Res.StatusDescription = "吉林移动手机验证码发送成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;

                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "吉林移动手机验证码发送异常";
                Log4netAdapter.WriteError("吉林移动手机验证码发送异常", e);

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
            cookies = new CookieCollection();
            string Url = string.Empty;
            string resultCode = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                //验证手机动态验证码
                Url = string.Format("http://www.jl.10086.cn/service/operate/json/CheckSmsAndLetter_handleJson.json?checkCodeBean.checkRange=checkSms&checkCodeBean.smsCheckCode={0}&rnd=0.7013361996764497", mobileReq.Smscode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = "http://www.jl.10086.cn/service/fee/QueryDetailList_3301.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "验证手机动态验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //string erroeMsg = jsonParser.GetResultFromMultiNode(httpResult.Html, "checkCodeBean:checkReason");
                //if (erroeMsg.Contains("短信验证码填写有误，请重新发送并填写") || erroeMsg.Contains("请先发送短信验证码再进行验证")||erroeMsg.Contains("请正确填写短信验证码"))
                //{
                //    Res.StatusDescription =erroeMsg;
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                Res.StatusDescription = "吉林移动手机验证码验证成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_Query;

                CacheHelper.SetCache(mobileReq.Token, cookies);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = Res.StatusCode;
                logDtl.Description = Res.StatusDescription;
                appLog.LogDtlList.Add(logDtl);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "吉林移动手机验证码验证异常";
                Log4netAdapter.WriteError("吉林移动手机验证码验证异常", e);

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
            string postdata = string.Empty;
            DateTime date = DateTime.Now;
            List<string> results = new List<string>();
            List<string[]> deatils = new List<string[]>();
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                #region 个人信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Crawler, mobileReq.Website);

                #region   基本信息
                logDtl = new ApplyLogDtl("基本信息");
                Url = "http://www.jl.10086.cn/service/fee/QueryFirstBill2.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://www.jl.10086.cn/my/account/index.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "nameInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "v_name", CrawlerTxt = System.Text.Encoding.Default.GetBytes(cookies["v_name"].Value.ToUrlDecode()) });
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "v_vcreditname", CrawlerTxt = System.Text.Encoding.Default.GetBytes(cookies["v_vcreditname"].Value.ToUrlDecode()) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "基本信息抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                #endregion

                #region  PUK查询
                logDtl = new ApplyLogDtl("PUK");
                Url = "http://www.jl.10086.cn/service/fee/QueryPuk.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    //Referer = "http://www.jl.10086.cn/my/account/index.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "PUK QueryPuk");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = "http://www.jl.10086.cn/service/fee/json/QueryPuk_queryJson.json?serviceBean.serviceType=52&rnd=0.25978024666827304";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    //Referer = "http://www.jl.10086.cn/my/account/index.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "PUK");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "PUKInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "PUK抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                #endregion

                #region  我的套餐
                logDtl = new ApplyLogDtl("我的套餐");
                Url = "http://www.jl.10086.cn/service/fee/json/QueryFav_queryJson.json?serviceBean.serviceType=87&rnd=0.49352936280039206";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://www.jl.10086.cn/service/fee/QueryFav.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "我的套餐");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "packageBrandInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "我的套餐抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                #endregion

                #region 积分
                logDtl = new ApplyLogDtl("积分");
                Url = "http://www.jl.10086.cn/my/operate/json/MyIndex_queryJFJson.json?integralBean.serviceType=20&rnd=0.16005620940689747";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = "http://www.jl.10086.cn/my/account/index.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "积分");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "积分抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                #endregion

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 手机账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Crawler, mobileReq.Website);

                CrawlerBill(crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region  ========详单========

                #region  通话详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.Call, crawler, mobileReq);  //通话

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.Net, crawler, mobileReq);//GPRS详单查询

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.SMS, crawler, mobileReq);//短信详单
                CrawlerDeatils(EnumMobileDeatilType.Other, crawler, mobileReq);//彩信详单

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #endregion

                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "吉林移动手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "吉林移动手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("吉林移动手机账单抓取异常", e);

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
            string result = string.Empty;
            //保存
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

                #region  个人基本信息
                logDtl = new ApplyLogDtl("基本信息解析");

                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == "nameInfor").FirstOrDefault() != null)
                    {
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "nameInfor").FirstOrDefault().CrawlerTxt);
                        //mobile.Name = cookies["v_name"].Value.ToUrlDecode();//姓名
                        //mobile.StarLevel = cookies["v_vcreditname"].Value.ToUrlDecode(); //星级
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "v_name").FirstOrDefault().CrawlerTxt);
                        mobile.Name = result;
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "v_vcreditname").FirstOrDefault().CrawlerTxt);
                        mobile.StarLevel = result;

                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "基本信息解析成功";
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

                #region PUK 解析
                logDtl = new ApplyLogDtl("PUK解析");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == "PUKInfor").FirstOrDefault() != null)
                    {
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "PUKInfor").FirstOrDefault().CrawlerTxt);
                        mobile.PUK = jsonParser.GetResultFromMultiNode(result, "model:serviceBean:res_content:ROOT:BODY:OUT_DATA:PUK");

                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "PUK解析成功";
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
                logDtl = new ApplyLogDtl("套餐信息解析");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == "packageBrandInfor").FirstOrDefault() != null)
                    {
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageBrandInfor").FirstOrDefault().CrawlerTxt);
                        string ss = jsonParser.GetResultFromMultiNode(result, "model:res_content:serviceBean:res_content:ROOT:BODY");
                        List<taocan> Detail = jsonParser.DeserializeObject<List<taocan>>(jsonParser.GetResultFromMultiNode(ss, "OUT_DATA:FAV_ONLY_BILL"));
                        mobile.Package = Detail[0].PRODUCT_NAME;

                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "套餐信息解析成功";
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

                #region  积分
                logDtl = new ApplyLogDtl("积分信息解析");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault() != null)
                    {
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                        mobile.Integral = jsonParser.GetResultFromMultiNode(result, "integralBean:res_content:ROOT:BODY:OUT_DATA:ACCTSCORE_INFO:CANUSE_SCORE");

                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "积分信息解析成功";
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

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region  手机账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Analysis, mobileReq.Website);

                AnalysisBill(crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region  ========详单解析========

                #region 语音账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.Call, crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.Net, crawler, mobile); //数据详单

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.SMS, crawler, mobile);//短信
                AnalysisDeatils(EnumMobileDeatilType.Other, crawler, mobile);//彩信

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #endregion

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                //保存
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "吉林移动手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "吉林移动手机账单解析异常";
                Log4netAdapter.WriteError("吉林移动手机账单解析异常", e);

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

        public string GetMonthBillResults(string YearMonth, string type, string logtitle)
        {
            string Url = string.Format("http://www.jl.10086.cn/service/fee/json/QueryFirstBill2_queryJson.json?serviceBean.serviceType=73&serviceBean.BILL_CYCLE={0}&serviceBean.STATUS={1}&rnd=0.9410387733033597", YearMonth, type);
            httpItem = new HttpItem()
            {
                URL = Url,
                Referer = "http://www.jl.10086.cn/service/fee/QueryFirstBill2.jsp",
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtmlLog(httpItem, appLog, logtitle);
            if (httpResult.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }
            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

            return httpResult.Html;

        }

        /// <summary>
        /// 抓取账单
        /// </summary>
        /// <param name="crawler"></param>
        private void CrawlerBill(CrawlerData crawler)
        {
            string results = string.Empty; ;
            List<string> _results = new List<string>();
            DateTime date = DateTime.Now;
            string title = string.Empty; ;
            for (int i = 0; i < 6; i++)
            {
                date = DateTime.Now.AddMonths(-i);
                title = date.ToString(Consts.DateFormatString7) + "月账单抓取";
                logDtl = new ApplyLogDtl(title);
                string YearMonth = date.ToString("yyyyMM");
                results = GetMonthBillResults(YearMonth, "2", title + "(已缴)");//已缴查询
                if (results.IsEmpty()) continue;

                string json = jsonParser.GetResultFromParser(results, "serviceBean");
                json = jsonParser.GetResultFromParser(json, "res_content");
                json = jsonParser.GetResultFromParser(json, "ROOT");
                json = jsonParser.GetResultFromParser(json, "BODY");
                json = jsonParser.GetResultFromParser(json, "OUT_DATA");
                _results = jsonParser.GetArrayFromParse(json, "BILL_LIST");
                if (_results.Count == 0)
                {
                    results = GetMonthBillResults(YearMonth, "0", title + "(未缴)");//未缴查询
                    if (results.IsEmpty()) continue;
                }
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(results) });

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = title + "成功";
                appLog.LogDtlList.Add(logDtl);
            }

        }

        /// <summary>
        /// 解析账单
        /// </summary>
        /// <param name="crawler"></param>
        /// <param name=ServiceConsts.SpiderType_Mobile></param>
        private void AnalysisBill(CrawlerData crawler, Basic mobile)
        {
            string PhoneBillStr = string.Empty;
            List<string> results = new List<string>();
            MonthBill bill = new MonthBill();
            DateTime date;

            for (int i = 0; i < 6; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月账单解析");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault() == null) continue;
                    decimal total = 0;
                    PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    string json = jsonParser.GetResultFromParser(PhoneBillStr, "serviceBean");
                    json = jsonParser.GetResultFromParser(json, "res_content");
                    json = jsonParser.GetResultFromParser(json, "ROOT");
                    json = jsonParser.GetResultFromParser(json, "BODY");
                    json = jsonParser.GetResultFromParser(json, "OUT_DATA");
                    results = jsonParser.GetArrayFromParse(json, "BILL_LIST");
                    bill = new MonthBill();
                    bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);//2010-10-01

                    foreach (string item in results)
                    {
                        string bill_info = jsonParser.GetResultFromParser(item, "BILL_INFO");
                        decimal should_pay = jsonParser.GetResultFromParser(bill_info, "SHOULD_PAY").ToDecimal(0);
                        if (jsonParser.GetResultFromParser(bill_info, "ITEM_NAME").Contains("套餐"))
                            bill.PlanAmt = (should_pay / 100).ToString();
                        total += should_pay;
                    }
                    bill.TotalAmt = (total / 100).ToString();
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
        }

        /// <summary>
        /// 抓取手机详单
        /// </summary>
        /// <param name="queryType">3:通话；4:上网；5:短信</param>
        /// <returns></returns>
        private void CrawlerDeatils(EnumMobileDeatilType type, CrawlerData crawler, MobileReq mobileReq)
        {
            DateTime dateNow = DateTime.Now;
            DateTime date = DateTime.Now;
            string month = string.Empty;
            string Url = string.Empty;
            string queryType = string.Empty;
            string s = "COUNT\\" + "\"" + ":";

            if (type == EnumMobileDeatilType.Call)
                queryType = "3300";
            else if (type == EnumMobileDeatilType.SMS)
                queryType = "3301";
            else if (type == EnumMobileDeatilType.Net)
                queryType = "3303";
            else
                queryType = "3302";

            logDtl = new ApplyLogDtl("详单抓取校验");
            Url = string.Format("http://www.jl.10086.cn/service/operate/json/CheckSmsAndLetter_handleJson.json?checkCodeBean.checkRange=checkSms&checkCodeBean.smsCheckCode={0}&rnd=0.40855941800856654", mobileReq.Smscode);
            httpItem = new HttpItem()
            {
                URL = Url,
                Referer = "http://www.jl.10086.cn/service/fee/QueryDetailList_3303.jsp",
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "详单抓取校验");
            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

            //循环月份
            for (int i = 0; i <= 5; i++)
            {
                date = dateNow.AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月详单抓取");
                month = date.ToString("yyyyMM");
                Url = string.Format("http://www.jl.10086.cn/service/fee/json/QueryDetailList_queryJson.json?serviceBean.serviceType={0}&serviceBean.DATE_TYPE=1&serviceBean.RADIO_TIME={1}&serviceBean.PASSWORD={2}&rnd=0.7629008125693522", queryType, month, mobileReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://www.jl.10086.cn/service/fee/QueryDetailList_3301.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (queryType != "3303")
                {
                    httpItem.Encoding = Encoding.GetEncoding("utf-8");
                }
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, date.ToString(Consts.DateFormatString7) + "月详单抓取");
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                try
                {
                    if (httpResult.Html.Contains("未找到对应的详单资料") || int.Parse(CommonFun.GetMidStr(httpResult.Html, s, ",")) == 0)
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = date.ToString(Consts.DateFormatString7) + "月详单抓取失败：" + httpResult.Html;
                        appLog.LogDtlList.Add(logDtl);
                        continue;
                    }
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + month, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = date.ToString(Consts.DateFormatString7) + "详单抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = date.ToString(Consts.DateFormatString7) + "月详单抓取异常：" + e.Message + "原始数据：" + httpResult.Html;
                    appLog.LogDtlList.Add(logDtl);

                    continue;
                }

            }
        }

        /// <summary>
        /// 解析手机详单
        /// </summary>
        /// <param name="queryType"></param>
        /// <returns></returns>
        private void AnalysisDeatils(EnumMobileDeatilType type, CrawlerData crawler, Basic mobile)
        {
            string PhoneCrawlerDtls = string.Empty;
            DateTime date = DateTime.Now;
            string month = string.Empty;
            Call phoneCall;//语音
            Sms phoneSMS;//短息
            Net phoneGPRS;//上网
            string sss = "\\" + "\"" + "DETAIL_MSG" + "\\" + "\"" + ":[\\";

            for (int i = 0; i <= 5; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月详单解析");
                month = date.ToString("yyyyMM");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == (type + month)).FirstOrDefault() == null) continue;
                    PhoneCrawlerDtls = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (type + month)).FirstOrDefault().CrawlerTxt);
                    string[] arry = CommonFun.GetMidStr(PhoneCrawlerDtls, sss, "]").Split(new char[] { ',' });
                    for (int j = 0; j < arry.Length; j++)
                    {
                        string[] ary = arry[j].ToTrim("\\").ToTrim("\"").Split(new char[] { '|' });
                        if (type == EnumMobileDeatilType.Call)//通话详单
                        {
                            phoneCall = new Call();
                            var totalSecond = 0;
                            var usetime = ary[2].ToString();
                            if (!string.IsNullOrEmpty(usetime))
                                totalSecond = CommonFun.ConvertDate(usetime);
                            phoneCall.StartTime = DateTime.Parse(ary[0]).ToString(Consts.DateFormatString11);
                            phoneCall.InitType = ary[1];
                            phoneCall.UseTime = totalSecond.ToString();
                            phoneCall.OtherCallPhone = ary[3];
                            phoneCall.CallPlace = ary[4];
                            phoneCall.CallType = ary[6];
                            phoneCall.SubTotal = ary[10].ToDecimal(0);
                            mobile.CallList.Add(phoneCall);
                        }
                        else if (type == EnumMobileDeatilType.Net)//GPRS详单查询
                        {
                            phoneGPRS = new Net();
                            phoneGPRS.StartTime = DateTime.Parse(ary[1]).ToString(Consts.DateFormatString11);
                            phoneGPRS.Place = ary[7];
                            phoneGPRS.NetType = ary[3];
                            phoneGPRS.SubTotal = ary[10].ToDecimal(0);
                            phoneGPRS.SubFlow = CommonFun.ConvertGPRS(ary[2].ToString()).ToString();
                            //phoneGPRS.UseTime = ary[2];
                            mobile.NetList.Add(phoneGPRS);
                        }
                        else //3301短信详单,3302彩信详单
                        {
                            phoneSMS = new Sms();
                            if (type == EnumMobileDeatilType.SMS)
                                phoneSMS.SmsType = "短信";
                            else
                                phoneSMS.SmsType = "彩信";
                            phoneSMS.StartTime = DateTime.Parse(ary[3]).ToString(Consts.DateFormatString11);
                            phoneSMS.OtherSmsPhone = ary[1];
                            phoneSMS.InitType = ary[2];
                            phoneSMS.SubTotal = ary[7].ToDecimal(0);
                            mobile.SmsList.Add(phoneSMS);
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

        #endregion

    }

    public class taocan
    {
        public string FAV_TYPE { get; set; }
        public string PRODUCT_NAME { get; set; }
    }
}
