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
    public class CQ : IMobileCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        ApplyLogMongo logMongo = new ApplyLogMongo();
        #endregion

        public VerCodeRes MobileInit(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                //第一步，初始化登录页面
                Url = "http://login.189.cn/login";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                if (CheckNeedVerify(mobileReq))
                {
                    //第二步，获取验证码
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
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (httpResult.StatusCode != HttpStatusCode.OK)
                    {
                        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                    Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                }

                Res.StatusDescription = "重庆电信初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "重庆电信初始化异常";
                Log4netAdapter.WriteError("重庆电信初始化异常", e);
                appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_Init)
                {
                    CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                    StatusCode = ServiceConsts.StatusCode_error,
                    Description = e.Message
                });
            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                if (appLog.LogDtlList.Count < 1)
                {
                    appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_Init)
                    {
                        CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                        StatusCode = Res.StatusCode,
                        Description = Res.StatusDescription
                    });
                }
                logMongo.Save(appLog);
            }
            return Res;
        }

        public BaseRes MobileLogin(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
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
                    //CacheHelper.RemoveCache(mobileReq.Token);
                }
                //校验参数
                if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region  老版登录
                //var key = AES.GetMD5("login.189.cn");
                //AES.Key = key;
                //AES.IV = "1234567812345678";
                //string password = AES.AESEncrypt(mobileReq.Password);
                //Url = "http://login.189.cn/login";
                //postdata = string.Format("Account={0}&UType=201&ProvinceID=04&AreaCode=&CityNo=&RandomFlag=0&Password={1}&Captcha={2}", mobileReq.Mobile, password.ToUrlEncode(), mobileReq.Vercode);
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "POST",
                //    Postdata = postdata,
                //    CookieCollection = cookies,
                //    Allowautoredirect = false,
                //    Referer = "http://login.189.cn/login",
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='loginForm']", "data-errmsg");
                //if (results.Count > 0 && !results[0].IsEmpty())
                //{
                //    Res.StatusDescription = results[0];
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //string location = httpResult.Header["Location"];

                //Url = location;
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "get",
                //    CookieCollection = cookies,
                //    Referer = "http://login.189.cn/login",
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //Url = "http://www.189.cn/cq/";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "get",
                //    CookieCollection = cookies,
                //    Referer = location,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                #endregion

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
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                JObject jsonObj = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                string ProvinceID = jsonObj["ProvinceID"].ToString();
                if (ProvinceID != "04")
                {
                    Res.StatusDescription = "用户名不存在";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
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
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='loginForm']", "data-errmsg");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
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
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                jsonObj = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                int FailTimes = jsonObj["FailTimes"].ToString().ToInt(0);
                int LockTimes = jsonObj["LockTimes"].ToString().ToInt(0);
                if (FailTimes != 0)
                {
                    Res.StatusDescription = string.Format("您的密码错误！再连续{0}次输入错误，账号将被锁！", LockTimes - FailTimes);
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                    return Res;
                }

                Url = "http://www.189.cn/cq";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = location,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://cq.189.cn/pages/login/sypay_group_new.jsp";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://cq.189.cn/pages/login/sypay_group_new.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/dqmh/flowrecommend.do";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/cq/",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/login/index.do";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/html/login/right.html",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10018&toStUrl=http://hb.189.cn/SSOtoWSSNew?toWssUrl=/pages/selfservice/custinfo/userinfo/userInfo.action&trackPath=FWBK";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://hb.189.cn/pages/login/sypay_group_new.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);



                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_SendSMS;
                CacheHelper.SetCache(Res.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "重庆电信手机登录异常";
                Log4netAdapter.WriteError("重庆电信手机登录异常", e);
                appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_Login)
                {
                    CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                    StatusCode = ServiceConsts.StatusCode_error,
                    Description = e.Message
                });

            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                if (appLog.LogDtlList.Count < 1)
                {
                    appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_Login)
                    {
                        CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                        StatusCode = Res.StatusCode,
                        Description = Res.StatusDescription
                    });
                }
                logMongo.Save(appLog);
            }
            return Res;
        }

        public VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            VerCodeRes Res = new VerCodeRes();
            Res.Token = mobileReq.Token;
            cookies = new CookieCollection();
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
                postdata = string.Format("fastcode=02031273");
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=02031272",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10004&toStUrl=http://cq.189.cn/new-bill/bill_xd?fastcode=02031273";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Allowautoredirect = false,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=02031272",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string location = httpResult.Header["Location"];

                Url = location;
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Allowautoredirect = false,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=02031272",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                location = httpResult.Header["Location"];

                Url = location;
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Allowautoredirect = false,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=02031272",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                location = httpResult.Header["Location"];

                Url = location;
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Allowautoredirect = false,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=02031272",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                location = httpResult.Header["Location"];

                Url = location;
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Allowautoredirect = false,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=02031272",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                location = httpResult.Header["Location"];

                Url = location;
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=02031272",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://cq.189.cn/new-bill/bill_XDCX";
                postdata = string.Format("accNbr={0}&productId=208511296&billingModeId=1200", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://cq.189.cn/new-bill/bill_xd?fastcode=02031273&ticket=ST-154013-FXdutCAZVnXqt6GmjcJi",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://cq.189.cn/new-bill/bill_DXYZM";
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    URL = Url,
                    Method = "post",
                    Referer = "http://cq.189.cn/new-bill/bill_xd?fastcode=02031273&ticket=ST-154013-FXdutCAZVnXqt6GmjcJi",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.Html != "0")
                {
                    Res.StatusDescription = "手机验证码发送失败！";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "重庆电信手机验证码发送成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
                CacheHelper.SetCache(mobileReq.Token, cookies);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "重庆电信手机验证码发送异常";
                Log4netAdapter.WriteError("重庆电信手机验证码发送异常", e);
                appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_SendSMS)
                {
                    CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                    StatusCode = ServiceConsts.StatusCode_error,
                    Description = e.Message
                });
            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                if (appLog.LogDtlList.Count < 1)
                {
                    appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_SendSMS)
                    {
                        CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                        StatusCode = Res.StatusCode,
                        Description = Res.StatusDescription
                    });
                }
                logMongo.Save(appLog);
            }
            return Res;
        }

        public BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            cookies = new CookieCollection();
            string Url = string.Empty;
            string postdata = string.Empty;
            string result = string.Empty;
            DateTime date = DateTime.Now;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                    //CacheHelper.RemoveCache(mobileReq.Token);
                }

                //Url = "http://cq.189.cn/new-bill/bill_XDCX";
                //postdata = string.Format("accNbr={0}&productId=208511296&billingModeId=1200", mobileReq.Mobile);
                //httpItem = new HttpItem()
                //{
                //    Accept = "*/*",
                //    URL = Url,
                //    Method = "post",
                //    Postdata = postdata,
                //    Referer = "http://cq.189.cn/new-bill/bill_xd?fastcode=02031273&ticket=ST-154013-FXdutCAZVnXqt6GmjcJi",
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //第六步，验证手机验证码
                Url = "http://cq.189.cn/new-bill/bill_XDCXNR";
                postdata = String.Format("accNbr={0}&productId=208511296&month={2}&callType=01&listType=300001&beginTime={3}&endTime={4}&rc={1}", mobileReq.Mobile, mobileReq.Smscode, date.ToString("yyyy-MM"), date.ToString(Consts.DateFormatString12), DateTime.Parse(date.ToString(Consts.DateFormatString12)).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd"));
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://cq.189.cn/new-bill/bill_xd?fastcode=02031273&ticket=ST-134996-rylDVpTrJbxsboxSuBqR",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                result = CommonFun.GetMidStr(httpResult.Html, "result\":\"", "\",");
                if (result != "0")
                {
                    result = jsonParser.GetResultFromParser(httpResult.Html, "message");
                    if (result == "2333")
                        result = "验证码错误！";
                    Res.StatusDescription = result;
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                    return Res;
                }

                Res.StatusDescription = "重庆电信手机验证码验证成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Query;

                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "重庆电信手机验证码校验异常";
                Log4netAdapter.WriteError("重庆电信手机验证码校验异常", e);
                appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_CheckSMS)
                {
                    CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                    StatusCode = ServiceConsts.StatusCode_error,
                    Description = e.Message
                });
            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                if (appLog.LogDtlList.Count < 1)
                {
                    appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_CheckSMS)
                    {
                        CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                        StatusCode = Res.StatusCode,
                        Description = Res.StatusDescription
                    });
                }
                logMongo.Save(appLog);
            }
            return Res;
        }

        public BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            ApplyLogDtl logDtl = new ApplyLogDtl("");
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            cookies = new CookieCollection();
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            CrawlerData crawler = new CrawlerData() { Token = mobileReq.Token, IdentityCard = mobileReq.IdentityCard, Name = mobileReq.Name, Mobile = mobileReq.Mobile, UserType = "1" };
            string Url = string.Empty;
            string postdata = String.Empty;
            List<string> results = new List<string>();
            try
            {

                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                #region 个人信息
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Crawler);

                #region   积分查询
                Url = "http://www.189.cn/cq/service";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://www.189.cn/cq/",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10004&toStUrl=http://cq.189.cn/integral/index.htm";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://www.189.cn/cq/service",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                #endregion

                #region    基本资料

                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10004&toStUrl=http://cq.189.cn/account/userInfo.htm";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://www.189.cn/cq/service/",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10004&toStUrl=http://cq.189.cn/account/dwr/engine.js?v=20151006123154";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://cq.189.cn/account/userInfo.htm",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string origScriptID = CommonFun.GetMidStr(httpResult.Html, "dwr.engine._origScriptSessionId = \"", "\";") + "816";

                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10004&toStUrl=http://cq.189.cn/sso/action/components?method=log_state&callback=jsonp1444354033845";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Referer = "http://cq.189.cn/account/userInfo.htm",
                    ResultCookieType = ResultCookieType.CookieCollection,
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:42.0) Gecko/20100101 Firefox/42.0"

                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://cq.189.cn/account/dwr/call/plaincall/userInfoQueryDwr.getCustInfoNew.dwr";
                postdata = string.Format("callCount=1\npage=/account/userInfo.htm\nhttpSessionId=\nscriptSessionId={0}\nc0-scriptName=userInfoQueryDwr\nc0-methodName=getCustInfoNew\nc0-id=0\nbatchId=0", origScriptID);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "http://cq.189.cn/account/userInfo.htm",
                    ResultCookieType = ResultCookieType.CookieCollection,
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:42.0) Gecko/20100101 Firefox/42.0"
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                #endregion

                #region  套餐查询
                string lType = string.Empty;
                Url = "http://cq.189.cn/account/userProd.htm";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='lType']", "value");
                if (results.Count > 0)
                {
                    lType = results[0];
                }
                Url = "http://cq.189.cn/account/dwr/call/plaincall/acceptQueryDwr.getfavourInfo.dwr";
                postdata = string.Format(@"callCount=1&page=/account/userProd.htm&httpSessionId=&scriptSessionId=2E8006A9061C0D33FA0146B1AC4DA20D212&c0-scriptName=acceptQueryDwr&c0-methodName=getfavourInfo&c0-id=0&c0-param0=string:{0}&c0-param1=string:{1}&batchId=9", mobileReq.Mobile, lType);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "packageInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                #endregion

                #region  星级

                Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Method = "POST",
                    Postdata = string.Format("fastcode=20000006"),
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=0202",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10004&toStUrl=http://cq.189.cn/new-account/creditValue?fastcode=20000006";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=0202",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "starLevelInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                #endregion

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "基本信息抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  账单查询
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Crawler);

                CrawlerBill(mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 详单

                #region 通话账单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.Call, mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 短信账单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.SMS, mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 上网账单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.Net, mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #endregion

                //保存
                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "重庆电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "重庆电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("重庆电信手机账单抓取异常", e);

                logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);

            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                if (Res.StatusCode == ServiceConsts.StatusCode_fail && logDtl.Description.IsEmpty())
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);
                }
                logMongo.Save(appLog);
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
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            ApplyLogDtl logDtl = new ApplyLogDtl("");
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
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Analysis);

                mobile.Token = mobileReq.Token;
                mobile.BusName = mobileReq.Name;
                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.Mobile = mobileReq.Mobile;
                mobile.UpdateTime = crawler.CrawlerDate;

                #region   积分查询
                if (crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault() != null)
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//table[1]/tr[2]/td[1]", "");
                    if (results.Count > 0)
                    {
                        mobile.Integral = results[0];
                    }
                }
                #endregion

                #region    基本资料
                if (crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault() != null)
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                    result = CommonFun.GetMidStr(result, "('0','0',", ");");
                    mobile.Name = jsonParser.GetResultFromParser(result, "userName");
                    mobile.Address = jsonParser.GetResultFromParser(result, "custAddr");
                    mobile.Idtype = jsonParser.GetResultFromParser(result, "idCardType");
                    mobile.Idcard = jsonParser.GetResultFromParser(result, "showNumber");
                }
                #endregion

                #region  套餐查询
                if (crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault() != null)
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault().CrawlerTxt);
                    mobile.Package = System.Text.RegularExpressions.Regex.Unescape(CommonFun.GetMidStr(result, "s0.offerCompName=\"", "\";"));
                }
                #endregion

                #region  星级
                if (crawler.DtlList.Where(x => x.CrawlerTitle == "starLevelInfor").FirstOrDefault() != null)
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "starLevelInfor").FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//table[@class='style11']/tbody/tr/td", "");
                    mobile.StarLevel = results[1];
                }
                #endregion


                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "个人信息解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  账单查询
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis);

                AnalysisBill(crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 详单

                #region 通话详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Analysis);

                AnalysisDeatils(EnumMobileDeatilType.Call, crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 短信详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Analysis);

                AnalysisDeatils(EnumMobileDeatilType.SMS, crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 流量详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Analysis);

                AnalysisDeatils(EnumMobileDeatilType.Net, crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #endregion

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                //保存
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "重庆电信手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "重庆电信手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("重庆电信手机账单解析异常", e);
                logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
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

        #region 私有方法

        /// <summary>
        /// 抓取账单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private void CrawlerBill(MobileReq mobileReq, CrawlerData crawler)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            var month = String.Empty;

            for (int i = 0; i <= 6; i++)
            {
                if (i == 0)
                {
                    Url = "http://cq.189.cn/new-bill/getSSHF";
                    postdata = string.Format("accNbr={0}&productId=208511296", mobileReq.Mobile);
                    httpItem = new HttpItem()
                    {
                        Accept = "*/*",
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                }
                else
                {
                    month = DateTime.Now.AddMonths(-i).ToString("yyyy-MM");
                    Url = "http://cq.189.cn/new-bill/bill_ZZDCX";
                    postdata = string.Format("accNbr={0}&productId=208511296&month={1}&queryType=2", mobileReq.Mobile, month);
                    httpItem = new HttpItem()
                    {
                        Accept = "*/*",
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    if (CommonFun.GetMidStr(httpResult.Html, "result\":\"", "\"") == "not0")
                    {
                        crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                        continue;
                    }
                    Url = "http://cq.189.cn/new-bill/bill_ZDCX";
                    postdata = String.Format("page=1&rows=100");
                    httpItem = new HttpItem()
                    {
                        Accept = "application/json, text/javascript, */*; q=0.01",
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Referer = "http://cq.189.cn/new-bill/bill_zd?fastcode=02031272&ticket=ST-169747-SrVGv9w2gNh2Y2AP4wfO",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                }
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
            }
        }

        /// <summary>
        /// 读取账单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private void AnalysisBill(CrawlerData crawler, Basic mobile)
        {
            string PhoneBillStr = string.Empty;
            MonthBill bill = null;

            for (var i = 0; i <= 5; i++)
            {
                if (crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault() == null)
                    continue;
                PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                if (PhoneBillStr.IsEmpty()) continue;
                bill = new MonthBill();
                bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);
                if (i == 0)
                {
                    var resultobj = JsonConvert.DeserializeObject(PhoneBillStr);
                    JArray rows = resultobj as JArray;
                    for (int j = 0; j < rows.Count; j++)
                    {
                        if (rows[j]["type"].ToString().Contains("套餐费"))
                            bill.PlanAmt = (bill.PlanAmt.ToDecimal(0) + rows[j]["money"].ToString().ToTrim().ToDecimal(0)).ToString();
                        if (bill.PlanAmt.IsEmpty())
                        {
                            string message = mobile.Package.Substring(mobile.Package.Length - 3, 2).ToTrim();
                            System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(@"^\d+$");
                            if (rex.IsMatch(message))
                                bill.PlanAmt = message;
                        }
                        if (rows[j]["type"].ToString() == "话费总额")
                            bill.TotalAmt = rows[j]["money"].ToString().ToTrim();
                    }
                }
                else
                {
                    if (CommonFun.GetMidStr(PhoneBillStr, "result\":\"", "\"") == "not0")
                        continue;
                    var resultobj = JsonConvert.DeserializeObject(PhoneBillStr);
                    JObject js = resultobj as JObject;
                    JArray rows = js["rows"] as JArray;
                    for (int j = 0; j < rows.Count; j++)
                    {
                        if (rows[j]["billItem"].ToString().Contains("套餐费"))
                            bill.PlanAmt = (bill.PlanAmt.ToDecimal(0) + rows[j]["billAmount"].ToString().ToTrim().ToDecimal(0)).ToString();
                        if (rows[j]["billItem"].ToString() == "合计")
                            bill.TotalAmt = rows[j]["billAmount"].ToString().ToTrim();
                    }
                }
                mobile.BillList.Add(bill);
            }
        }

        /// <summary>
        /// 抓取手机详单
        /// </summary>
        /// <param name="queryType">300001:通话；300003:上网；300002:短信</param>
        /// <returns></returns>
        private void CrawlerDeatils(EnumMobileDeatilType type, MobileReq mobileReq, CrawlerData crawler)
        {
            string Url = string.Empty;
            string pastdata = string.Empty;
            DateTime date = DateTime.Now;
            DateTime first = date;
            DateTime last = date;
            var month = String.Empty;
            string result = String.Empty;
            string postdata = String.Empty;
            int pageIndex = 1;
            int pageCount = 0;
            string bill_type = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                bill_type = "300001";
            else if (type == EnumMobileDeatilType.SMS)
                bill_type = "300002";
            else
                bill_type = "300003";

            for (var i = 0; i <= 5; i++)
            {
                first = new DateTime(date.Year, date.Month, 1).AddMonths(-i);
                last = first.AddMonths(1).AddDays(-1);
                month = first.ToString("yyyy-MM");

                Url = "http://cq.189.cn/new-bill/bill_XDCXNR";
                postdata = String.Format("accNbr={0}&productId=208511296&month={2}&callType=01&listType={1}&beginTime={3}&endTime={4}&rc=", mobileReq.Mobile, bill_type, month, first.ToString(Consts.DateFormatString12), last.ToString("yyyy-MM-dd"));
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://cq.189.cn/new-bill/bill_xd?fastcode=02031273&ticket=ST-134996-rylDVpTrJbxsboxSuBqR",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                result = CommonFun.GetMidStr(httpResult.Html, "result\":\"", "\"");
                if (result != "0")
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + (i + 1).ToString() + "00", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    continue;
                }
                do
                {
                    Url = "http://cq.189.cn/new-bill/bill_XDCX_Page";
                    pastdata = String.Format("page={0}&rows=1000", pageIndex);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = pastdata,
                        Referer = "http://cq.189.cn/new-bill/bill_xd?fastcode=02031273&ticket=ST-134996-rylDVpTrJbxsboxSuBqR",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + (i + 1).ToString() + (pageIndex < 10 ? "0" + pageIndex.ToString() : pageIndex.ToString()), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    result = CommonFun.GetMidStr(httpResult.Html, "total\":", "}");
                    pageCount = Math.Ceiling((double)(result.ToDecimal(0) / 1000)).ToString().ToInt(1);
                    pageIndex++;
                }
                while (pageIndex <= pageCount);
            }
        }

        /// <summary>
        /// 解析手机详单
        /// </summary>
        /// <param name="queryType">300001:通话；300003:上网；300002:短信</param>
        /// <returns></returns>
        private void AnalysisDeatils(EnumMobileDeatilType type, CrawlerData crawler, Basic mobile)
        {
            List<CrawlerDtlData> PhoneCrawlerDtls = new List<CrawlerDtlData>();
            string PhoneStr = string.Empty;
            Call phoneCall;
            Sms phoneSMS;
            Net phoneGPRS;
            object result = null;
            for (var i = 0; i <= 5; i++)
            {
                if (crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1) + "00")).FirstOrDefault() != null)
                    continue;
                PhoneCrawlerDtls = crawler.DtlList.Where(x => x.CrawlerTitle.StartsWith(type + (i + 1).ToString())).OrderBy(x => x.CrawlerTitle).ToList<CrawlerDtlData>();
                if (PhoneCrawlerDtls != null && PhoneCrawlerDtls.Count > 0)
                {
                    foreach (CrawlerDtlData item in PhoneCrawlerDtls)
                    {
                        PhoneStr = System.Text.Encoding.Default.GetString(item.CrawlerTxt);
                        try
                        {
                            result = JsonConvert.DeserializeObject(PhoneStr);
                        }
                        catch
                        {
                            continue;
                        }
                        JObject js = result as JObject;
                        if (js != null)
                        {
                            JArray rows = js["rows"] as JArray;
                            if (rows == null)
                                continue;
                            if (type == EnumMobileDeatilType.Call)
                            {
                                for (var j = 0; j < rows.Count; j++)
                                {
                                    if (rows[j]["对方号码"].ToString() == "合计")
                                        continue;
                                    phoneCall = new Call();
                                    phoneCall.StartTime = DateTime.Parse(rows[j]["起始时间"].ToString()).ToString(Consts.DateFormatString11);
                                    phoneCall.SubTotal = rows[j]["费用（元）"].ToString().ToDecimal().Value;
                                    phoneCall.UseTime = rows[j]["通话时长（秒）"].ToString();
                                    phoneCall.OtherCallPhone = rows[j]["对方号码"].ToString();
                                    phoneCall.CallPlace = rows[j]["使用地点"].ToString();
                                    phoneCall.CallType = rows[j]["通话类型"].ToString();
                                    phoneCall.InitType = rows[j]["呼叫类型"].ToString();
                                    mobile.CallList.Add(phoneCall);
                                }
                            }
                            else if (type == EnumMobileDeatilType.SMS)
                            {
                                for (var j = 0; j < rows.Count; j++)
                                {
                                    if (rows[j]["业务类型"].ToString() == "合计")
                                        continue;
                                    phoneSMS = new Sms();
                                    phoneSMS.StartTime = DateTime.Parse(rows[j]["发送时间"].ToString()).ToString(Consts.DateFormatString11);
                                    phoneSMS.SubTotal = rows[j]["费用（元）"].ToString().ToDecimal().Value;
                                    phoneSMS.OtherSmsPhone = rows[j]["对方号码"].ToString();
                                    phoneSMS.SmsType = rows[j]["业务类型"].ToString();
                                    if (!phoneSMS.SmsType.IsEmpty())
                                        phoneSMS.SmsType = phoneSMS.SmsType.Substring(phoneSMS.SmsType.Length - 2, 2);
                                    mobile.SmsList.Add(phoneSMS);
                                }
                            }
                            else if (type == EnumMobileDeatilType.Net)
                            {
                                for (var j = 0; j < rows.Count; j++)
                                {
                                    if (rows[j]["开始时间"].ToString() == "合计")
                                        continue;
                                    phoneGPRS = new Net();
                                    phoneGPRS.StartTime = DateTime.Parse(rows[j]["开始时间"].ToString()).ToString(Consts.DateFormatString11);
                                    phoneGPRS.SubTotal = rows[j]["费用（元）"].ToString().ToDecimal().Value;
                                    phoneGPRS.UseTime = rows[j]["上网时长（秒）"].ToString();
                                    phoneGPRS.SubFlow = rows[j]["流量（KB）"].ToString();
                                    phoneGPRS.PhoneNetType = rows[j]["业务类型"].ToString();
                                    phoneGPRS.NetType = rows[j]["网络类型"].ToString();
                                    phoneGPRS.Place = rows[j]["通信地点"].ToString();
                                    mobile.NetList.Add(phoneGPRS);
                                }
                            }
                        }
                    }
                }
            }
        }

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
                httpResult = httpHelper.GetHtml(httpItem);
                flag = CommonFun.GetMidStr(httpResult.Html, "CaptchaFlag\":", ",").Trim();
                return flag == "1" ? true : false;
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("重庆电信校验异常", e);
            }
            return true;
        }

        #endregion

    }
}
