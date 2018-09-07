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
using System.Text.RegularExpressions;
using Vcredit.Common.Constants;
using System.Web.Caching;
using System.Web;
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.NetSpider.Entity.Mongo.Log;


namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{
    public class ChinaNet : IMobileCrawler
    {
        #region 公共变量
        protected IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        protected IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        protected IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        protected CookieCollection cookies = new CookieCollection();
        protected HttpResult httpResult = null;
        protected HttpItem httpItem = null;
        protected LogHelper httpHelper = new LogHelper();
        protected ApplyLogMongo logMongo = new ApplyLogMongo();
        protected List<ApplyLog> loglist = new List<ApplyLog>();
        protected ApplyLog appLog = new ApplyLog();
        protected ApplyLogDtl logDtl = new ApplyLogDtl();
        #endregion

        public VerCodeRes MobileInit(MobileReq mobileReq)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Init, mobileReq.Website);
            VerCodeRes Res = new VerCodeRes();
            Res.Token = CommonFun.GetGuidID();
            string Url = string.Empty;
            try
            {
                logDtl = new ApplyLogDtl("初始化登录页面");

                Url = "http://login.189.cn/login";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "初始化登录页面");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "初始化登录页面成功";
                appLog.LogDtlList.Add(logDtl);

                if (CheckNeedVerify(mobileReq))
                {
                    //第二步，获取验证码
                    logDtl = new ApplyLogDtl("获取验证码");
                    Url = "http://login.189.cn/captcha?40de47880cd14474b4f6630333f75ba3&source=login&width=100&height=37&0.3426654600437933";
                    httpItem = new HttpItem()
                    {
                        Accept = "image/png,image/*;q=0.8,*/*;q=0.5",
                        URL = Url,
                        ResultType = ResultType.Byte,
                        Referer = "http://login.189.cn/login",
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
                }

                Res.StatusDescription = GetProvince(mobileReq.Website) + "电信初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;

                CacheHelper.SetCache(Res.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = GetProvince(mobileReq.Website) + "电信初始化异常";
                Log4netAdapter.WriteError(GetProvince(mobileReq.Website) + "电信初始化异常", e);

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

        public BaseRes MobileLogin(MobileReq mobileReq)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Login, mobileReq.Website);
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            Dictionary<string, string> provinceInfo = CrawlerCommonFun.GetProvinceInfoByWebsite(mobileReq.Website);
            string provinceCode = provinceInfo["ProvinceCode"];

            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                #region 校验用户
                logDtl = new ApplyLogDtl("校验用户");

                Url = "http://login.189.cn/login/ajax";
                postdata = string.Format("m=checkphone&phone={0}", mobileReq.Mobile);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验用户");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                JObject jsonObj = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                string ProvinceID = jsonObj["ProvinceID"].ToString();
                if (ProvinceID != provinceInfo["ProvinceID"])
                {
                    Res.StatusDescription = "用户名不存在";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "校验用户成功";
                appLog.LogDtlList.Add(logDtl);

                #endregion

                #region 登录

                logDtl = new ApplyLogDtl("登录");

                string key = AES.GetMD5("login.189.cn");
                AES.Key = key;
                AES.IV = "1234567812345678";
                string PasswordAES = AES.AESEncrypt(mobileReq.Password);
                Url = "http://login.189.cn/login";
                postdata = string.Format("Account={0}&UType=201&ProvinceID={1}&AreaCode=&CityNo=&RandomFlag=0&Password={2}&Captcha={3}", mobileReq.Mobile, "04", PasswordAES.ToUrlEncode(), mobileReq.Vercode);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Allowautoredirect = false,
                    Referer = "http://login.189.cn/login",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string location = httpResult.Header["Location"];
                Url = location;
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://login.189.cn/login",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录校验location");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='loginForm']", "data-errmsg");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }

                Url = "http://login.189.cn/login/ajax";
                postdata = string.Format("m=loadlogincaptcha&Account={0}&UType=201&ProvinceID=04&AreaCode=&CityNo=", mobileReq.Mobile);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://login.189.cn/login",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录校验");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                jsonObj = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                int FailTimes = jsonObj["FailTimes"].ToString().ToInt(0);
                int LockTimes = jsonObj["LockTimes"].ToString().ToInt(0);
                if (FailTimes != 0)
                {
                    Res.StatusDescription = string.Format("您的密码错误！再连续{0}次输入错误，账号将被锁！", LockTimes - FailTimes);
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "登录成功";
                appLog.LogDtlList.Add(logDtl);

                #endregion

                #region 跨域登录校验

                logDtl = new ApplyLogDtl("跨域登录校验");

                Url = string.Format("http://www.189.cn/{0}", provinceCode);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = location,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "跨域登录校验bj");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = string.Format("http://{0}.189.cn/pages/login/sypay_group_new.jsp", provinceCode);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = string.Format("http://{0}.189.cn/pages/login/sypay_group_new.jsp", provinceCode),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/dqmh/flowrecommend.do";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = string.Format("http://www.189.cn/{0}/", provinceCode),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "跨域登录校验flowrecommend");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/login/index.do";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/html/login/right.html",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "跨域登录校验login/index.do");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                #endregion

                Res.StatusDescription = GetProvince(mobileReq.Website) + "电信登录成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_SendSMS;

                CacheHelper.SetCache(Res.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = GetProvince(mobileReq.Website) + "电信登录异常";
                Log4netAdapter.WriteError(GetProvince(mobileReq.Website) + "电信登录异常", e);

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

        public virtual VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            throw new NotImplementedException();
        }

        public virtual BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            throw new NotImplementedException();
        }

        public virtual BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {
            throw new NotImplementedException();
        }

        public virtual BaseRes MobileAnalysis(MobileReq mobileReq, DateTime appDate)
        {
            throw new NotImplementedException();
        }

        #region 私有方法

        /// <summary>
        /// 校验是否需要验证码
        /// </summary>
        /// <returns></returns>
        private bool CheckNeedVerify(MobileReq mobileReq)
        {
            string Url = string.Empty;
            string flag = string.Empty;
            string postdata = string.Empty;
            try
            {
                logDtl = new ApplyLogDtl("校验是否需要验证码");

                Url = "http://login.189.cn/login/ajax";
                postdata = string.Format("m=loadlogincaptcha&Account={0}&UType=201&ProvinceID=04&AreaCode=&CityNo=", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://login.189.cn/login",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验是否需要验证码");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    flag = CommonFun.GetMidStr(httpResult.Html, "CaptchaFlag\":", ",").Trim();

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.Description = "校验是否需要验证码成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                return flag == "1" ? true : false;
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(GetProvince(mobileReq.Website) + "电信校验异常", e);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            return true;
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
