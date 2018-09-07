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
using Vcredit.NetSpider.Entity.Mongo.Log;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{
    public class SN : IMobileCrawler
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

        public VerCodeRes MobileInit(MobileReq mobileReq = null)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            VerCodeRes Res = new VerCodeRes();
            cookies = new CookieCollection();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://login.189.cn/login";
                httpItem = new HttpItem
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //验证登录是否需要验证码
                if (CheckNeedVerify(mobileReq))
                {
                    //第二步，获取验证码
                    Url = "http://login.189.cn/captcha?5837ad2cc7ee45a9815bfb14defef0db&source=login&width=100&height=37&0.9509423255575052";
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
                    FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                    Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                }

                Res.StatusDescription = "陕西电信初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "陕西电信初始化异常";
                Log4netAdapter.WriteError("陕西电信初始化异常", e);
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
            cookies = new CookieCollection();
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

                #region 老版登录
                //var password = mobileReq.Password;
                //var randomId = mobileReq.Vercode;
                //var username = mobileReq.Mobile;

                //var key = AES.GetMD5("login.189.cn");
                //AES.Key = key;
                //AES.IV = "1234567812345678";
                //mobileReq.Password = AES.AESEncrypt(mobileReq.Password);

                //Url = "http://login.189.cn/login";
                //postdata = "Account={0}&UType=201&ProvinceID=27&AreaCode=&CityNo=&RandomFlag=0&Password={1}&Captcha=";
                //postdata = String.Format(postdata, mobileReq.Mobile, mobileReq.Password.ToUrlEncode());
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "post",
                //    Postdata = postdata,
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //string location = httpResult.Header["Location"];
                //Url = location;
                //httpItem = new HttpItem
                //{
                //    URL = Url,
                //    Referer = "http://login.189.cn/login",
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //var message = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='loginForm']", "data-errmsg");
                //if (message.Count > 0)
                //{
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    Res.StatusDescription = message[0];
                //    return Res;
                //}
                ////   <form id="loginForm" method="post" data-result="1" data-resultcode="8105" data-errmsg="用户密码过于简单">
                //Url = "http://www.189.cn/dqmh/cms/index/login_jx.jsp";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
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
                if (ProvinceID != "27")
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
                postdata = string.Format("Account={0}&UType=201&ProvinceID={1}&AreaCode=&CityNo=&RandomFlag=0&Password={2}&Captcha={3}", mobileReq.Mobile, "27", PasswordAES.ToUrlEncode(), mobileReq.Vercode);
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
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = "http://login.189.cn/login/ajax";
                postdata = string.Format("m=loadlogincaptcha&Account={0}&UType=201&ProvinceID=27&AreaCode=&CityNo=", mobileReq.Mobile);
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
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = "http://www.189.cn/sn";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = location,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://sn.189.cn/pages/login/sypay_group_new.jsp";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://sn.189.cn/pages/login/sypay_group_new.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/dqmh/flowrecommend.do";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/sn/",
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

                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_SendSMS;
                CacheHelper.SetCache(Res.Token, cookies);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "陕西电信手机登录异常";
                Log4netAdapter.WriteError("陕西电信手机登录异常", e);
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

            List<string> results = new List<string>();
            try
            {
                ////获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                    //CacheHelper.RemoveCache(token);
                }

                //发送验证码
                Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = "fastcode=10000202",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //发送验证码
                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10027&toStUrl=http://sn.189.cn/service/bill/sendValidReq.action?mobileNum={0}&listType=1";
                httpItem = new HttpItem()
                {
                    URL = String.Format(Url, mobileReq.Mobile),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                var msg = jsonParser.GetResultFromParser(httpResult.Html, "success");
                if (msg != "True")
                {
                    Res.StatusDescription = "短信验证码发送失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Res.StatusDescription = "短信码已发送";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "陕西电信手机验证码发送异常";
                Log4netAdapter.WriteError("陕西电信手机验证码发送异常", e);
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
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                    //CacheHelper.RemoveCache(mobileReq.Token);
                }

                //第六步，验证手机验证码
                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10027&toStUrl=http://sn.189.cn/service/bill/validCDMANum.action?mobileNum={0}&rondomCode={1}&_=1439190513951";
                Url = String.Format(Url, mobileReq.Mobile, mobileReq.Smscode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string result = jsonParser.GetResultFromParser(httpResult.Html, "success");
                if (result == "False")
                {
                    Res.StatusDescription = "验证码错误！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Res.StatusDescription = "陕西电信手机验证码验证成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Query;

                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "陕西电信手机验证码校验异常";
                Log4netAdapter.WriteError("陕西电信手机验证码校验异常", e);
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
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                #region 个人信息
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Crawler);

                Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = "fastcode=10000429",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //基本信息
                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10027&toStUrl=http://sn.189.cn/service/manage/showCustInfo.action?fastcode=10000429";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                //积分
                Url = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=10000199";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                //套餐
                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10027&toStUrl=http://sn.189.cn/service/bill/initQueryMealCharges.action?rnd=157826";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };

                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "packageBrandInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "基本信息抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  账单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Crawler);

                CrawlerBill(mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  通话费用
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.Call, mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  短信费用
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.SMS, mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  上网费用
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.Net, mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "陕西电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "陕西电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("陕西电信手机账单抓取异常", e);

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
        /// 读取账单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes MobileAnalysis(MobileReq mobileReq, DateTime appDate)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            ApplyLogDtl logDtl = new ApplyLogDtl("");
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            MobileMongo mobileMongo = new MobileMongo(appDate);
            CrawlerData crawler = new CrawlerData();
            Basic mobile = new Basic();
            string result = string.Empty;
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

                //个人信息
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                var myinfo = HtmlParser.GetResultFromParser(result, "//table");
                if (myinfo.Count >= 4)
                {
                    //姓名
                    var name = HtmlParser.GetResultFromParser(myinfo[0].ToString(), "//tr[1]/td[1]", "inner");
                    if (name.Count > 0)
                    {
                        mobile.Name = name[0];
                    }
                    //邮编
                    var Postcode = HtmlParser.GetResultFromParser(myinfo[2].ToString(), "//tr[1]/td[1]", "inner");
                    if (Postcode.Count > 0)
                    {
                        mobile.Postcode = Postcode[0];
                    }
                    //Email
                    var Email = HtmlParser.GetResultFromParser(myinfo[2].ToString(), "//tr[2]/td[1]", "inner");
                    if (Email.Count > 0)
                    {
                        mobile.Email = Email[0];
                    }
                    //地址
                    var address = HtmlParser.GetResultFromParser(myinfo[2].ToString(), "//tr[4]/td[1]", "inner");
                    if (address.Count > 0)
                    {
                        mobile.Address = address[0];
                    }
                }

                ///积分
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                var Integral = HtmlParser.GetResultFromParser(result, "//table[@class='usrr_wallet left']/tbody/tr/td[1]", "inner");
                if (Integral.Count > 0)
                {
                    mobile.Integral = Integral[0];
                }
                ///套餐
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageBrandInfor").FirstOrDefault().CrawlerTxt);
                var PackageBrand = HtmlParser.GetResultFromParser(result, "//table[@class='tab_1 m20']/tbody/tr[2]/td[1]", "inner");
                if (PackageBrand.Count > 0)
                {
                    mobile.Package = PackageBrand[0];
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "个人信息解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  账单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis);

                AnalysisBill(crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  通话费用
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Analysis);

                AnalysisDeatils(EnumMobileDeatilType.Call, crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  短信费用
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Analysis);

                AnalysisDeatils(EnumMobileDeatilType.SMS, crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单解析成功";
                appLog.LogDtlList.Add(logDtl);

                #endregion

                #region  上网费用
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Analysis);

                AnalysisDeatils(EnumMobileDeatilType.Net, crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单解析成功";
                appLog.LogDtlList.Add(logDtl);

                #endregion

                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "陕西电信手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "陕西电信手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("陕西电信手机账单解析异常", e);

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
            string Url = String.Empty;
            var postdata = String.Empty;
            var currdate = String.Empty;
            for (var i = 1; i <= 6; i++)
            {
                currdate = DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString7);
                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10027&toStUrl=http://sn.189.cn/service/bill/billDetail.action?billtype=1&month={0}&areacode=290&accnbr={1}&productid=41010300";
                Url = String.Format(Url, currdate, mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
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
            DateTime now;
            MonthBill bill = null;
            for (var i = 1; i <= 6; i++)
            {
                now = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                var result = crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault();
                if (result == null) continue;
                PhoneBillStr = System.Text.Encoding.Default.GetString(result.CrawlerTxt);
                bill = new MonthBill();
                bill.BillCycle = new DateTime(now.Year, now.Month, 1).ToString(Consts.DateFormatString12);
                var planamt = HtmlParser.GetResultFromParser(PhoneBillStr, "//table[@class='mytable']/tr[7]/td[2]", "inner");
                if (planamt.Count > 0)
                {
                    bill.PlanAmt = planamt[0];
                }
                var sum = Regex.Matches(PhoneBillStr, @"(?<=总费用\:)\d+(.)?\d+");
                if (sum.Count > 0)
                {
                    bill.TotalAmt = sum[0].ToString();
                }
                ///添加账单
                mobile.BillList.Add(bill);
            }
        }

        /// <summary>
        /// 抓取手机详单
        /// </summary>
        /// <param name="queryType">1 获取通话记录，12 上网详单 2 短信详单</param>
        /// <returns></returns>
        private void CrawlerDeatils(EnumMobileDeatilType type, MobileReq mobileReq, CrawlerData crawler)
        {
            string Url = string.Empty;
            var startDate = String.Empty;
            var endDate = String.Empty;
            DateTime now;
            string queryType = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                queryType = "1";
            else if (type == EnumMobileDeatilType.SMS)
                queryType = "2";
            else
                queryType = "12";

            for (var i = 0; i <= 5; i++)
            {
                now = DateTime.Now.AddMonths(-i);
                startDate = new DateTime(now.Year, now.Month, 1).ToString(Consts.DateFormatString2);
                endDate = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddDays(-1).ToString(Consts.DateFormatString2);
                if (type == EnumMobileDeatilType.Net)
                {
                    Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10027&toStUrl=http://sn.189.cn/service/bill/queryDetail.action?currentPage=1&pageSize=15&effDate={0}&expDate={1}&serviceNbr={2}&operListId={3}&sendSmsFlag=true&validCode={4}";
                }
                else
                {
                    Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10027&toStUrl=http://sn.189.cn/service/bill/feeDetailrecordList.action?currentPage=1&pageSize=1000&effDate={0}&expDate={1}&serviceNbr={2}&operListID={3}&isPrepay=0&pOffrType=481&sendSmsFlag=true&validCode={4}";
                }
                Url = String.Format(Url, startDate, endDate, mobileReq.Mobile, queryType, mobileReq.Smscode);
                //详单
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + "0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
            }

        }

        /// <summary>
        /// 解析手机详单
        /// </summary>
        /// <param name="queryType">1 获取通话记录，12 上网详单 2 短信详单</param>
        /// <returns></returns>
        private void AnalysisDeatils(EnumMobileDeatilType type, CrawlerData crawler, Basic mobile)
        {
            string PhoneCostStr = string.Empty;
            for (var i = 0; i <= 5; i++)
            {
                var result = crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1))).FirstOrDefault();
                if (result == null) continue;
                PhoneCostStr = System.Text.Encoding.Default.GetString(result.CrawlerTxt);
                var tr = HtmlParser.GetResultFromParser(PhoneCostStr, "//table[@class='mt10 transact_tab']/tr", "inner");
                if (tr.Count > 0)
                {
                    for (var j = 0; j < tr.Count; j++)
                    {
                        if (type == EnumMobileDeatilType.Call)
                        {
                            Call phoneCall = new Call();
                            var CallType = HtmlParser.GetResultFromParser(tr[j], "//td[1]", "inner");
                            if (CallType.Count > 0)
                            {
                                phoneCall.CallType = CallType[0];
                            }
                            var StartTime = HtmlParser.GetResultFromParser(tr[j], "//td[4]", "inner");
                            if (StartTime.Count > 0)
                            {
                                phoneCall.StartTime = DateTime.Parse(StartTime[0]).ToString(Consts.DateFormatString11);
                            }
                            var SubTotal = HtmlParser.GetResultFromParser(tr[j], "//td[9]", "inner");
                            if (SubTotal.Count > 0)
                            {
                                phoneCall.SubTotal = SubTotal[0].ToDecimal().Value;
                            }
                            var UseTime = HtmlParser.GetResultFromParser(tr[j], "//td[3]", "inner");
                            if (UseTime.Count > 0)
                            {
                                phoneCall.UseTime = UseTime[0] + "秒";
                            }
                            var OtherCallPhone = HtmlParser.GetResultFromParser(tr[j], "//td[2]", "inner");
                            if (OtherCallPhone.Count > 0)
                            {
                                phoneCall.OtherCallPhone = OtherCallPhone[0];
                            }
                            var CallPlace = HtmlParser.GetResultFromParser(tr[j], "//td[6]", "inner");
                            if (CallPlace.Count > 0)
                            {
                                phoneCall.CallPlace = CallPlace[0];
                            }
                            var InitType = HtmlParser.GetResultFromParser(tr[j], "//td[5]", "inner");
                            if (InitType.Count > 0)
                            {
                                phoneCall.InitType = InitType[0];
                            }
                            mobile.CallList.Add(phoneCall);
                        }
                        else if (type == EnumMobileDeatilType.SMS)
                        {
                            Sms phoneSMS = new Sms();
                            var SmsType = HtmlParser.GetResultFromParser(tr[j], "//td[1]", "inner");
                            if (SmsType.Count > 0)
                            {
                                phoneSMS.SmsType = SmsType[0];
                            }
                            var OtherSmsPhone = HtmlParser.GetResultFromParser(tr[j], "//td[2]", "inner");
                            if (OtherSmsPhone.Count > 0)
                            {
                                phoneSMS.OtherSmsPhone = OtherSmsPhone[0];
                            }
                            var StartTime = HtmlParser.GetResultFromParser(tr[j], "//td[3]", "inner");
                            if (StartTime.Count > 0)
                            {
                                phoneSMS.StartTime = DateTime.Parse(StartTime[0]).ToString(Consts.DateFormatString11);
                            }
                            var SubTotal = HtmlParser.GetResultFromParser(tr[j], "//td[4]", "inner");
                            if (SubTotal.Count > 0)
                            {
                                phoneSMS.SubTotal = SubTotal[0].ToDecimal().Value;
                            }
                            mobile.SmsList.Add(phoneSMS);
                        }
                        else
                        {
                            Net phoneGPRS = new Net();
                            var StartTime = HtmlParser.GetResultFromParser(tr[j], "//td[2]", "inner");
                            if (StartTime.Count > 0)
                            {
                                phoneGPRS.StartTime = DateTime.Parse(StartTime[0]).ToString(Consts.DateFormatString11);
                            }
                            var SubTotal = HtmlParser.GetResultFromParser(tr[j], "//td[2]", "inner");
                            if (SubTotal.Count > 0)
                            {
                                phoneGPRS.SubTotal = SubTotal[9].ToDecimal().Value;
                            }
                            var UseTime = HtmlParser.GetResultFromParser(tr[j], "//td[3]", "inner");
                            if (UseTime.Count > 0)
                            {
                                phoneGPRS.UseTime = UseTime[0];
                            }
                            var Place = HtmlParser.GetResultFromParser(tr[j], "//td[7]", "inner");
                            if (Place.Count > 0)
                            {
                                phoneGPRS.Place = Place[0];
                            }
                            var SubFlow = HtmlParser.GetResultFromParser(tr[j], "//td[4]", "inner");
                            if (SubFlow.Count > 0)
                            {
                                phoneGPRS.SubFlow = SubFlow[0];
                            }
                            var NetType = HtmlParser.GetResultFromParser(tr[j], "//td[6]", "inner");
                            if (NetType.Count > 0)
                            {
                                phoneGPRS.NetType = NetType[0];
                            }
                            mobile.NetList.Add(phoneGPRS);
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
                postdata = string.Format("m=loadlogincaptcha&Account={0}&UType=201&ProvinceID=06&AreaCode=&CityNo=", mobileReq.Mobile);
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
                JObject jsonObj = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                int FailTimes = jsonObj["FailTimes"].ToString().ToInt(0);
                int LockTimes = jsonObj["LockTimes"].ToString().ToInt(0);
                if (FailTimes >= 3)
                {
                    return true;
                }

            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("山西电信校验异常", e);
            }
            return false;
        }

        #endregion

    }
}
