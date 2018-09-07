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
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.DataAccess.Mongo;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Vcredit.NetSpider.Entity.Mongo.Log;
using Vcredit.Common.Constants;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class GD : IMobileCrawler
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
        string key = String.Empty;
        #endregion

        public GD()
        {
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
        }

        /// <summary>
        /// 页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes MobileInit(MobileReq mobileReq = null)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Init, mobileReq.Website);
            VerCodeRes Res = new VerCodeRes();
            cookies = new CookieCollection();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            Dictionary<string, object> dic = new Dictionary<string, object>();
            try
            {
                //第一步，初始化登录页面
                logDtl = new ApplyLogDtl("初始化登录页面");
                Url = "https://gd.ac.10086.cn/ucs/login/signup.jsps";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "初始化登录页面");
                var matches = Regex.Matches(httpResult.Html, @"[var\s\w[rsa]+[\s+]?=[\s+]?[{]]*.\w.*[}]");
                if (matches.Count > 0)
                {
                    var matchesRSA = Regex.Matches(matches[0].ToString(), @"[[{]]*.\w.*[}]");
                    if (matchesRSA.Count > 0)
                    {
                        key = jsonParser.GetResultFromParser(matchesRSA[0].ToString(), "n");
                    }
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "初始化登录页面成功";
                appLog.LogDtlList.Add(logDtl);

                //第二步，获取验证码
                logDtl = new ApplyLogDtl("获取验证码");
                Url = "https://gd.ac.10086.cn/ucs/captcha/image/reade.jsps?sds=1436323782142";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "https://gd.ac.10086.cn/ucs/login/signup.jsps",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "获取验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
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
                Res.StatusDescription = "广东移动初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                dic.Add("key", key);
                dic["cookies"] = cookies;
                CacheHelper.SetCache(token, dic);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "广东移动初始化异常";
                Log4netAdapter.WriteError("广东移动初始化异常", e);

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
            cookies = new CookieCollection();
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
                    key = (string)dic["key"];
                }

                logDtl = new ApplyLogDtl("登录");
                Url = "https://gd.ac.10086.cn/ucs/login/register.jsps";
                postdata = "loginType=2&mobile={0}&password={1}&imagCaptcha={2}&cookieMobile=on&bizagreeable=true&exp=&cid=&area=&resource=&channel=0&reqType=0&backURL=";
                string pwd = RSAHelper.EncryptStringByRsaJS(mobileReq.Password, key);
                postdata = string.Format(postdata, mobileReq.Mobile, pwd, mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "https://gd.ac.10086.cn/ucs/login/signup.jsps",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                var errors = CommonFun.GetMidStr(httpResult.Html, "<span class=\"errors\">", "</span><br/>");
                errors = CommonFun.GetMidStr(errors, "错误信息【", "】").IsEmpty() ? errors : CommonFun.GetMidStr(errors, "错误信息【", "】");
                if (!errors.IsEmpty())
                {
                    Res.StatusDescription = errors;
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_SendSMS;

                CacheHelper.SetCache(mobileReq.Token, cookies);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = Res.StatusCode;
                logDtl.Description = Res.StatusDescription;
                appLog.LogDtlList.Add(logDtl);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "广东移动手机验证码异常";
                Log4netAdapter.WriteError("广东移动手机验证码异常", e);

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
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = CacheHelper.GetCache(mobileReq.Token) as CookieCollection;

                //第二步，发手机验证码
                logDtl = new ApplyLogDtl("发手机验证码");
                Url = "https://gd.ac.10086.cn/ucs/captcha/dpwd/send.jsps";
                postdata = String.Format("mobile={0}&dt=3", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "https://gd.ac.10086.cn/ucs/second/loading.jsps?reqType=0&channel=0&cid=10003&backURL=http://gd.10086.cn/my/REALTIME_LIST_SEARCH.shtml?dt=1438704000000&type=2",
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@class='errors']", "inner");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.StatusDescription = "广东移动手机验证码发送成功";
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;

                CacheHelper.SetCache(mobileReq.Token, cookies);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = Res.StatusCode;
                logDtl.Description = Res.StatusDescription;
                appLog.LogDtlList.Add(logDtl);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "广东移动手机验证码发送异常";
                Log4netAdapter.WriteError("广东移动手机验证码发送异常", e);

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
            string postdata = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                //第六步，验证手机验证码
                logDtl = new ApplyLogDtl("验证手机验证码");
                Url = "https://gd.ac.10086.cn/ucs/second/index.jsps";
                postdata = "cid=10003&channel=0&reqType=0&backURL=http%3A%2F%2Fgd.10086.cn%2Fmy%2FREALTIME_LIST_SEARCH.shtml%3Fdt%3D1438704000000&type=2";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "https://gd.ac.10086.cn/ucs/second/loading.jsps?reqType=0&channel=0&cid=10003&backURL=http://gd.10086.cn/my/REALTIME_LIST_SEARCH.shtml?dt=1438704000000&type=2",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "验证手机验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                var matches = Regex.Matches(httpResult.Html, @"[var\s\w[rsa]+[\s+]?=[\s+]?[{]]*.\w.*[}]");
                if (matches.Count > 0)
                {
                    var matchesRSA = Regex.Matches(matches[0].ToString(), @"[[{]]*.\w.*[}]");
                    if (matchesRSA.Count > 0)
                    {
                        key = jsonParser.GetResultFromParser(matchesRSA[0].ToString(), "n");
                    }
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "https://gd.ac.10086.cn/ucs/second/authen.jsps";
                postdata = string.Format("dpwd={0}&type=2&cid=10003&channel=0&reqType=0&backURL=http%3A%2F%2Fgd.10086.cn%2Fmy%2FREALTIME_LIST_SEARCH.shtml", RSAHelper.EncryptStringByRsaJS(mobileReq.Smscode, key));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "https://gd.ac.10086.cn/ucs/second/loading.jsps?reqType=0&channel=0&cid=10003&backURL=http://gd.10086.cn/my/REALTIME_LIST_SEARCH.shtml?dt=1438704000000&type=2",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "验证手机验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                List<string> errors = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@class='errors']", "inner");
                if (errors.Count > 0)
                {
                    Res.StatusDescription = errors[0];
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "广东移动手机验证码验证成功";
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
                Res.StatusDescription = "广东移动手机验证码验证异常";
                Log4netAdapter.WriteError("广东移动手机验证码验证异常", e);

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
        /// 查询账单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            cookies = new CookieCollection();
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            CrawlerData crawler = new CrawlerData() { Token = mobileReq.Token, IdentityCard = mobileReq.IdentityCard, Name = mobileReq.Name, Mobile = mobileReq.Mobile, UserType = "1" };
            string Url = string.Empty;
            string postdata = string.Empty;
            string content = string.Empty;
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                #region 初始化数据

                Url = "http://gd.10086.cn/commodity/servicio/track/servicioDcstrack/query.jsps";
                postdata = "servCode=ACCOUNTS_BALANCE_SEARCH";
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    ContentType = "application/x-www-form-urlencoded;charset=UTF-8",
                    Referer = "http://gd.10086.cn/my/ACCOUNTS_BALANCE_SEARCH.shtml?dt=1389283200000",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://gd.10086.cn/common/include/public/isOnline.jsp?_=1438740087801";
                httpItem = new HttpItem()
                {
                    Accept = "	text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Method = "POST",
                    Postdata = string.Format("1"),
                    Referer = "http://gd.10086.cn/my/ACCOUNTS_BALANCE_SEARCH.shtml?dt=1389283200000",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "https://gd.ac.10086.cn/ucs/login/loading.jsps?reqType=0&channel=0&cid=10003&area=%2Fcommodity&resource=%2Fcommodity%2Fservicio%2FservicioForwarding%2FqueryData.jsps&loginType=2&optional=true&exp=&backURL=http%3A%2F%2Fgd.10086.cn%2Fmy%2FACCOUNTS_BALANCE_SEARCH.shtml%3Fdt%3D1389283200000";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Referer = "http://gd.10086.cn/my/ACCOUNTS_BALANCE_SEARCH.shtml?dt=1389283200000",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://gd.10086.cn/common/include/public/isOnline.jsp?_=1438670752394";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Referer = "http://gd.10086.cn/my/ACCOUNTS_BALANCE_SEARCH.shtml?dt=1389283200000",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "https://gd.ac.10086.cn/ucs/login/signup.jsps";
                postdata = "backURL=http%3A%2F%2Fgd.10086.cn%2Fmy%2FACCOUNTS_BALANCE_SEARCH.shtml%3Fdt%3D1389283200000&reqType=0&channel=0&cid=10003&area=%2Fcommodity&resource=%2Fcommodity%2Fservicio%2FservicioForwarding%2FqueryData.jsps&loginType=2&optional=on&exp=";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "https://gd.ac.10086.cn/ucs/login/signup.jsps",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                content = jsonParser.GetResultFromParser(httpResult.Html, "content");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = content;
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                #endregion

                #region 个人信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Crawler, mobileReq.Website);

                #region 基本信息
                logDtl = new ApplyLogDtl("基本信息");
                Url = "http://gd.10086.cn/commodity/servicio/servicioForwarding/queryData.jsps";
                postdata = "servCode=MY_BASICINFO&operaType=QUERY";
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    ContentType = "application/x-www-form-urlencoded;charset=UTF-8",
                    Referer = "http://gd.10086.cn/my/myService/myBasicInfo.shtml",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    try
                    {
                        if (HtmlParser.GetResultFromParser(httpResult.Html, "//table/tbody/tr/td", "inner").Count <= 0)
                        {
                            Res.StatusDescription = "抓取失败";
                            Res.StatusCode = ServiceConsts.StatusCode_fail;

                            logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = "基本信息抓取失败:" + CommonFun.GetMidStr(httpResult.Html, "<span class=\"errors\">", "</span>");
                            appLog.LogDtlList.Add(logDtl);
                            appLog.Token = Res.Token;
                            appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                            loglist.Add(appLog);

                            return Res;
                        }
                        crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
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
                }
                #endregion

                #region 我的积分
                logDtl = new ApplyLogDtl("我的套餐");
                Url = "http://gd.10086.cn/commodity/servicio/servicioForwarding/queryData.jsps";
                postdata = string.Format("servCode=INTE_GRAL_CURR_YEAR&operaType=QUERY&Payment_startDate={0}&Payment_endDate={1}", DateTime.Now.ToString("yyyyMMdd000000"), DateTime.Now.ToString("yyyyMMdd235959"));
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    ContentType = "application/x-www-form-urlencoded;charset=UTF-8",
                    Referer = "http://gd.10086.cn/my/INTE_GRAL_CURR_YEAR.shtml",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "我的套餐");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "我的套餐抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                #endregion

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 消费情况（短信、通话、上网详单；月账单）
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Crawler, mobileReq.Website);

                Get(crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                crawlerMobileMongo.SaveCrawler(crawler);

                Res.StatusDescription = "广东移动手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "广东移动手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("广东移动手机账单抓取异常", e);

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
            List<string> results = new List<string>();

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

                #region 基本信息
                logDtl = new ApplyLogDtl("基本信息");

                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//table/tbody/tr/td", "inner");
                    if (results.Count > 0)
                    {
                        mobile.Idcard = results[2];
                        mobile.Name = results[1];
                        mobile.Regdate = DateTime.Parse(results[4]).ToString(Consts.DateFormatString11);
                        mobile.PUK = results[3];
                    }
                    results = HtmlParser.GetResultFromParser(result, "//table/tbody/tr/th", "inner");
                    if (results.Count > 0)
                    {
                        mobile.Idtype = results[2];
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

                #region 套餐
                logDtl = new ApplyLogDtl("我的积分");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                    string integral = CommonFun.GetMidStr(result, ">当前可用积分：<br /><i class=\"color_8\">", "</i>");
                    if (!integral.IsEmpty())
                        mobile.Integral = integral;

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "我的积分解析成功";
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

                #region 消费情况（短信、通话、上网详单；月账单）
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Analysis, mobileReq.Website);

                Read(crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                mobileMongo.SaveBasic(mobile);

                Res.StatusDescription = "广东移动手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Res.StatusDescription = "广东移动手机账单解析异常";
                Log4netAdapter.WriteError("广东移动手机账单解析异常", e);

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
        /// 抓取账单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private void Get(CrawlerData crawler)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            DateTime date = DateTime.Now;
            DateTime first = DateTime.Now;
            DateTime last = DateTime.Now;
            for (int i = 0; i < 6; i++)
            {
                first = new DateTime(date.Year, date.Month, 1).AddMonths(-i);
                last = first.AddMonths(1).AddDays(-1);
                logDtl = new ApplyLogDtl(first.ToString(Consts.DateFormatString7) + "月消费情况抓取");

                Url = "http://gd.10086.cn/commodity/servicio/nostandardserv/realtimeListSearch/ajaxRealQuery.jsps";
                postdata = string.Format("startTimeReal={0}&endTimeReal={1}&uniqueTag={2}&month={3}&monthListType=0&isChange=", first.ToString("yyyyMMdd000000"), last.ToString("yyyyMMdd235959"), date.ToString("yyyyMMddHHmmssSSS "), first.ToString("yyyyMM"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://www.hb.10086.cn/my/billdetails/queryInvoice.action",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, first.ToString(Consts.DateFormatString7) + "月账单抓取");
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = first.ToString(Consts.DateFormatString7) + "账单抓取成功";
                appLog.LogDtlList.Add(logDtl);
            }
        }

        /// <summary>
        /// 读取账单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private void Read(CrawlerData crawler, Basic mobile)
        {
            string PhoneBillStr = string.Empty;
            ApplyLogDtl logDtl = new ApplyLogDtl("");
            MonthBill bill = null;
            Call call = null;
            Net gprs = null;
            Sms sms = null;
            DateTime date = DateTime.Now;

            for (int i = 0; i <= 5; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月消费情况解析");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault() == null) continue;
                    PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    if (!CommonFun.GetMidStr(CommonFun.GetMidStr(PhoneBillStr, "<span class=\"errors\">", "</span></span><br/>"), "<span style=\"font-size:10.5pt;font-family:宋体;\">", "").IsEmpty()) continue;
                    object obj = JsonConvert.DeserializeObject(PhoneBillStr);
                    JObject jobj = obj as JObject;
                    JObject contentjobj = jobj["content"] as JObject;
                    if (contentjobj == null) continue;
                    JObject deatils = contentjobj["realtimeListSearchRspBean"] as JObject;
                    if (deatils == null) continue;
                    //我的套餐和品牌
                    if (mobile.Package.IsEmpty())
                    {
                        //品牌
                        mobile.PackageBrand = contentjobj["brand"].ToString();
                        //套餐
                        JObject taocandeatils = deatils["taocan"] as JObject;
                        JArray detailList = taocandeatils["taocanfeelist"] as JArray;
                        if (detailList.Count > 0)
                            mobile.Package = ((JObject)detailList[0])["feename"].ToString();
                    }
                    //月消费情况
                    if (contentjobj != null)
                    {
                        bill = new MonthBill();
                        bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);
                        bill.PlanAmt = contentjobj["taocanfeecount"].ToString().Trim();
                        bill.TotalAmt = contentjobj["total"].ToString().Trim();
                        mobile.BillList.Add(bill);
                    }

                    //通话详单
                    JObject calldeatils = deatils["calldetail"] as JObject;
                    JArray calldetailList = calldeatils["calldetaillist"] as JArray;
                    foreach (JObject item in calldetailList)
                    {
                        if (!item["time"].ToString().IsEmpty())
                        {

                            var totalSecond = 0;
                            var usetime = item["period"].ToString();
                            if (!string.IsNullOrEmpty(usetime))
                            {
                                totalSecond = CommonFun.ConvertDate(usetime);
                            }
                            call = new Call();
                            call.StartTime = DateTime.Parse(date.Year + "-" + item["time"].ToString()).ToString(Consts.DateFormatString11);
                            call.CallPlace = item["place"].ToString();
                            call.InitType = item["becall"].ToString();
                            call.OtherCallPhone = item["contnum"].ToString();
                            call.UseTime = totalSecond.ToString();
                            call.CallType = item["conttype"].ToString();
                            call.SubTotal = item["chargefee"].ToString().ToDecimal(0);
                            mobile.CallList.Add(call);
                        }
                    }

                    //短信详单
                    JObject smsdeatils = deatils["smsdetail"] as JObject;
                    JArray smsdetailList = smsdeatils["smsdetaillist"] as JArray;
                    foreach (JObject item in smsdetailList)
                    {
                        if (!item["time"].ToString().IsEmpty())
                        {
                            if (item["smsnum"].ToString().IsEmpty() || item["smsnum"].ToString() == "-") continue;
                            sms = new Sms();
                            sms.StartTime = DateTime.Parse(date.Year + "-" + item["time"].ToString()).ToString(Consts.DateFormatString11);
                            sms.SmsPlace = item["place"].ToString();
                            sms.OtherSmsPhone = item["smsnum"].ToString();
                            sms.InitType = item["send"].ToString();
                            sms.SmsType = item["smstype"].ToString();
                            sms.SubTotal = item["fee"].ToString().ToDecimal(0);
                            mobile.SmsList.Add(sms);
                        }
                    }

                    //上网详单
                    JObject netdeatils = deatils["netdetail"] as JObject;
                    JArray netdetailList = netdeatils["netdetaillist"] as JArray;
                    foreach (JObject item in netdetailList)
                    {
                        if (!item["time"].ToString().IsEmpty())
                        {

                            var totalSecond = 0;
                            var usetime = item["perid"].ToString();
                            if (!string.IsNullOrEmpty(usetime))
                            {
                                totalSecond = CommonFun.ConvertDate(usetime);
                            }

                            var totalFlow = CommonFun.ConvertGPRS(item["netcount"].ToString());

                            gprs = new Net();
                            gprs.StartTime = DateTime.Parse(date.Year + "-" + item["time"].ToString()).ToString(Consts.DateFormatString11);
                            gprs.Place = item["place"].ToString();
                            gprs.NetType = item["servicetype"].ToString();
                            gprs.UseTime = totalSecond.ToString();
                            gprs.SubFlow = totalFlow.ToString();
                            gprs.SubTotal = item["fee"].ToString().ToDecimal(0);
                            mobile.NetList.Add(gprs);
                        }
                    }
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = date.ToString(Consts.DateFormatString7) + "月消费情况解析异常：" + e.Message;
                    appLog.LogDtlList.Add(logDtl);
                    continue;
                }
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = date.ToString(Consts.DateFormatString7) + "月消费情况解析成功";
                appLog.LogDtlList.Add(logDtl);
            }
        }

        private static bool RemoteCertificateValidate(
           object sender, X509Certificate cert,
             X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }
        #endregion

    }
}
