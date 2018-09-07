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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.NetSpider.DataAccess.Mongo;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.RegularExpressions;
using Vcredit.Common.Constants;
using Vcredit.NetSpider.Entity.Mongo.Log;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{
    public class YN : IMobileCrawler
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
            List<string> results = new List<string>();
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

                Res.StatusDescription = "云南电信初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "云南电信初始化异常";
                Log4netAdapter.WriteError("云南电信初始化异常", e);
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

        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes MobileLogin(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            Res.Token = mobileReq.Token;
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

                //Url = "http://login.189.cn/login";
                //var key = AES.GetMD5("login.189.cn");
                //AES.Key = key;
                //AES.IV = "1234567812345678";
                //postdata = string.Format("Account={0}&UType=201&ProvinceID=25&AreaCode=&CityNo=&RandomFlag=0&Password={1}&Captcha=", mobileReq.Mobile, AES.AESEncrypt(mobileReq.Password).ToUrlEncode());
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "post",
                //    Postdata = postdata,
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='loginForm']", "data-errmsg");
                //if (results.Count > 0)
                //{
                //    if (!results[0].IsEmpty())
                //    {
                //        Res.StatusDescription = results[0];
                //        Res.StatusCode = ServiceConsts.StatusCode_fail;
                //        return Res;
                //    }
                //}
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //Url = "http://www.189.cn/dqmh/cms/index/login_jx.jsp";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    CookieCollection = cookies,
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

                //Url = "http://www.189.cn/dqmh/login.do?method=loginright";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    CookieCollection = cookies,
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

                //Url = "http://www.189.cn/yn/service";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Referer = "http://www.189.cn/yn/",
                //    CookieCollection = cookies,
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
                if (ProvinceID != "25")
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
                postdata = string.Format("Account={0}&UType=201&ProvinceID={1}&AreaCode=&CityNo=&RandomFlag=0&Password={2}&Captcha={3}", mobileReq.Mobile, "25", PasswordAES.ToUrlEncode(), mobileReq.Vercode);
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
                postdata = string.Format("m=loadlogincaptcha&Account={0}&UType=201&ProvinceID=25&AreaCode=&CityNo=", mobileReq.Mobile);
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

                Url = "http://www.189.cn/yn";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = location,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://yn.189.cn/pages/login/sypay_group_new.jsp";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://yn.189.cn/pages/login/sypay_group_new.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/dqmh/flowrecommend.do";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/yn/",
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


                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10025&toStUrl=http://yn.189.cn/service/jt/bill/actionjt/ifr_myIndex.jsp?fastcode=01971241";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Query;
                CacheHelper.SetCache(Res.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "云南电信手机登录异常";
                Log4netAdapter.WriteError("云南电信手机登录异常", e);
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
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                Url = "http://yn.189.cn/SsoEscWt?SysID=25&toStUrl=http://yn.189.cn/service/bill/feeQuery.jsp?SERV_NO=SHQD1";
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

                Url = CommonFun.GetMidStr(httpResult.Html, "href='", "'");
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
                string user_id = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='USER_NO']", "value")[0];

                Url = "http://yn.189.cn/service/bill/action/ifr_bill_detailslist_em_new.jsp";
                postdata = string.Format("NUM={0}&AREA_CODE=0871&PROD_NO=4217&USER_NAME=&ACCT_ID=415578369&USER_ID={1}", mobileReq.Mobile, user_id);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://yn.189.cn/public/postValidCode.jsp";
                postdata = string.Format("NUM={0}&AREA_CODE=0871&LOGIN_TYPE=21&OPER_TYPE=CR0&RAND_TYPE=004", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "http://yn.189.cn/service/bill/feeQuery.jsp?SERV_NO=SHQD1",
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

                string msg = CommonFun.GetMidStr(httpResult.Html, "<actionMsg>", "</actionMsg> ");
                if (!msg.IsEmpty())
                {
                    Res.StatusDescription = msg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = "获取手机详单验证码，调用手机验证码验证接口";
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;

                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "云南电信手机验证码发送异常";
                Log4netAdapter.WriteError("云南电信手机验证码发送异常", e);
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
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                Url = "http://yn.189.cn/public/pwValid.jsp";
                postdata = @"_FUNC_ID_=WB_PAGE_PRODPASSWDQRY&PROD_PASS={1}&MOBILE_CODE={2}&ACC_NBR={0}&AREA_CODE=0871&LOGIN_TYPE=21&PASSWORD={1}&MOBILE_FLAG=1&MOBILE_LOGON_NAME={0}&MOBILE_CODE={2}&PROD_NO=4217";
                postdata = string.Format(postdata, mobileReq.Mobile, mobileReq.Password, mobileReq.Smscode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "http://yn.189.cn/service/bill/feeQuery.jsp?SERV_NO=SHQD1",
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

                string flag = CommonFun.GetMidStr(httpResult.Html, "<rsFlag>", "</rsFlag>");
                if (flag != "2")
                {
                    Res.StatusDescription = "短信验证码错误！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = "获取手机详单验证码，调用手机验证码验证接口";
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;

                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "云南电信手机验证码校验异常";
                Log4netAdapter.WriteError("云南电信手机验证码校验异常", e);

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

        /// <summary>
        /// 查询账单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            ApplyLogDtl logDtl = new ApplyLogDtl("");
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            CrawlerData crawler = new CrawlerData() { Token = mobileReq.Token, IdentityCard = mobileReq.IdentityCard, Name = mobileReq.Name, Mobile = mobileReq.Mobile, UserType = "1" };
            string carrier = string.Empty;//手机号归属地
            string Url = string.Empty;
            string Uid = string.Empty;
            string PhoneCostStr = string.Empty;
            string year = string.Empty;
            string filterfield = string.Empty;
            List<string[]> results = new List<string[]>();
            List<string> _result = new List<string>();
            DateTime first = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            string postdata = String.Empty;
            DateTime last = DateTime.Now;
            try
            {

                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                #region 第一步   基本信息
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Crawler);

                #region 个人资料
                Url = "http://yn.189.cn/service/manage/index.jsp?FLAG=4";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://www.189.cn/yn/service/",
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

                Url = "http://yn.189.cn/SsoEscWt?SysID=25&toStUrl=http://yn.189.cn/service/manage/index.jsp?FLAG=4&SERV_KIND=&SERV_NO=&SUB_FUN_ID=";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://yn.189.cn/service/manage/index.jsp?FLAG=4",
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

                Url = CommonFun.GetMidStr(httpResult.Html, "href='", "'");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://yn.189.cn/SsoEscWt?SysID=25&toStUrl=http://yn.189.cn/service/manage/index.jsp?FLAG=4&SERV_KIND=&SERV_NO=&SUB_FUN_ID=",
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

                Url = "http://yn.189.cn/service/manage/index.jsp?FLAG=4&SERV_KIND=&SERV_NO=&SUB_FUN_ID=";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = httpResult.ResponseUri,
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

                Url = "http://yn.189.cn/service/manage/my_selfinfo.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://yn.189.cn/service/manage/index.jsp?FLAG=4&SERV_KIND=&SERV_NO=&SUB_FUN_ID=",
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
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                #endregion

                #region 套餐查询
                Url = "http://yn.189.cn/service/manage/myprod_sm.jsp";
                postdata = string.Format("BUREAUCODE=0871&USER_PASSWD=&ACCNBR={0}&AuthFlag=0&PROD_TYPE=4", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
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

                Url = "http://yn.189.cn/SsoEscWt?SysID=25&toStUrl=http://yn.189.cn/service/bill/feeQuery.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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

                Url = "http://www.189.cn:80/dqmh/ssoLink.do?method=linkTo&platNo=10025&toStUrl=http://yn.189.cn/service/bill/feeQuery.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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

                string Logon_Name = string.Empty;
                string CUSTBRAND = string.Empty;
                string USE_PROTOCOL = string.Empty;
                string LOGIN_TYPE = string.Empty;
                string USER_NO = string.Empty;
                Url = "http://yn.189.cn/login/user_protocol.jsp";
                postdata = string.Format("Logon_Name={0}&CUSTBRAND={1}&USE_PROTOCOL={2}&LOGIN_TYPE={3}&USER_NO={4}", Logon_Name, CUSTBRAND, USE_PROTOCOL, LOGIN_TYPE, USER_NO);
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

                Url = string.Format("http://yn.189.cn/public/query_realnameInfo.jsp?NUM={0}&AREA_CODE=&accNbrType=9", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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

                string USER_ID = string.Empty;
                string NUM = string.Empty;
                string AREA_CODE = string.Empty;
                Url = "http://yn.189.cn/service/bill/feeQuery.jsp?SERV_NO=9A001";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                    Referer = "http://yn.189.cn/service/bill/feeQuery.jsp?SERV_NO=SHQD1",
                    //UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:41.0) Gecko/20100101 Firefox/41.0"
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                _result = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='bill_content']/table/tbody/tr");
                if (_result.Count > 0)
                {
                    foreach (string item in _result)
                    {
                        List<string> bill_content = HtmlParser.GetResultFromParser(item, "/td");
                        if (bill_content.Count == 5)
                        {
                            if (bill_content[1] == mobileReq.Mobile)
                            {
                                string nextpostdata = bill_content[0];

                                string[] Arry = new string[] { };
                                Arry = nextpostdata.Split(new char[] { ',', '\'' });
                                if (Arry.Count() > 13)
                                {
                                    USER_ID = Arry[13];
                                    AREA_CODE = Arry[4];
                                }
                                break;
                            }
                        }
                    }
                }
                else
                {
                    string nextpostdata = CommonFun.GetMidStr(httpResult.Html, "<script>doQuery(", ");");
                    string[] Arry = new string[] { };
                    Arry = nextpostdata.Replace("'", "").Split(new char[] { ',' });
                    if (Arry.Count() > 5)
                    {
                        AREA_CODE = Arry[1];
                        USER_ID = Arry[5];
                    }
                }
                #endregion


                string PROD_NO = string.Empty;
                Url = "http://yn.189.cn/jf/getUserBonus.jsp";
                postdata = "LAN_CODE=&ACC_MONTH=&ACCOUNT_TYPE=&ACCOUNT_NO=&SERV_NO=JFCX";
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
                _result = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='bill_content']/table/tbody/tr");
                if (_result.Count > 0)
                {
                    foreach (string item in _result)
                    {
                        List<string> bill_content = HtmlParser.GetResultFromParser(item, "/td");
                        if (bill_content.Count == 5)
                        {
                            if (bill_content[1] == mobileReq.Mobile)
                            {
                                string nextpostdata = bill_content[0];

                                string[] Arry1 = new string[] { };
                                Arry1 = nextpostdata.Split(new char[] { ',', '\'' });
                                PROD_NO = Arry1[7];
                                break;
                            }
                        }
                    }
                }
                else
                {
                    string nextpostdata = CommonFun.GetMidStr(httpResult.Html, "<script>doQuery(", ");");
                    string[] Arry = new string[] { };
                    Arry = nextpostdata.Replace("'", "").Split(new char[] { ',' });
                    PROD_NO = Arry[2];
                }
                #region 积分查询
                Url = "http://yn.189.cn/jf/ifr_query_scores.jsp";
                postdata = string.Format("NUM={0}&AREA_CODE={1}&PROD_NO={2}&USER_NAME=&ACCT_ID={3}", mobileReq.Mobile, AREA_CODE, PROD_NO, USER_ID);
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
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                #endregion

                #region puk查询
                Url = "http://www.189.cn/dqmh/userCenter/bmhelp.do?method=pUK";
                postdata = "phonenumber=" + mobileReq.Mobile;
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
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "pukInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "基本信息抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 获取账单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Crawler);

                for (int i = 1; i <= 6; i++)
                {
                    string Month = DateTime.Now.AddMonths(-i).ToString("yyyyMM");
                    Url = "http://yn.189.cn/service/bill/action/ifr_bill_hislist_em.jsp";
                    postdata = string.Format("TEMPLATE_ID=&BILLING_CYCLE={0}&USER_ID={1}&NUM={2}&AREA_CODE={3}", Month, USER_ID, mobileReq.Mobile, AREA_CODE);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill" + i, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                }
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 获取通话详单
                Url = "http://yn.189.cn/service/bill/action/ifr_bill_detailslist_em_new.jsp";

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Crawler);

                for (int i = 0; i < 6; i++)
                {
                    string time = DateTime.Now.AddMonths(-i).ToString("yyyyMM");
                    postdata = string.Format("NUM={0}&AREA_CODE=0871&CYCLE_BEGIN_DATE=&CYCLE_END_DATE=&BILLING_CYCLE={1}&QUERY_TYPE=10", mobileReq.Mobile, time);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        Referer = "http://yn.189.cn/service/bill/feeQuery.jsp?SERV_NO=SHQD1",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = EnumMobileDeatilType.Call.ToString() + i, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 获取上网详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Crawler);

                List<string[]> time_pair = new List<string[]>();
                string cylcestr = string.Empty;
                for (int i = 0; i < 6; i++)
                {
                    DateTime time = DateTime.Now.AddMonths(-i);
                    string[] pair = new string[2];
                    pair[0] = time.ToString(Consts.DateFormatString12);
                    pair[1] = time.ToString("yyyy-MM-") + DateTime.DaysInMonth(time.Year, time.Month).ToString();
                    time_pair.Add(pair);
                    cylcestr += string.Format("CYCLE_BEGIN_END_DATE={0}%7C{1}&", pair[0], pair[1]);
                }

                for (int i = 0; i < 6; i++)
                {
                    string time = DateTime.Now.AddMonths(-i).ToString("yyyyMM");
                    postdata = string.Format("NUM={0}&AREA_CODE=0871&CYCLE_BEGIN_DATE={1}&CYCLE_END_DATE={2}&BILLING_CYCLE={3}&{4}QUERY_TYPE=20", mobileReq.Mobile, time_pair[i][0], time_pair[i][1], time, cylcestr);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        Referer = "http://yn.189.cn/service/bill/feeQuery.jsp?SERV_NO=SHQD1",
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = EnumMobileDeatilType.Net.ToString() + i, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 获取短信详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Crawler);

                for (int i = 0; i < 6; i++)
                {
                    string time = DateTime.Now.AddMonths(-i).ToString("yyyyMM");
                    postdata = string.Format("NUM={0}&AREA_CODE=0871&CYCLE_BEGIN_DATE=&CYCLE_END_DATE=&BILLING_CYCLE={1}&QUERY_TYPE=30", mobileReq.Mobile, time);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        Referer = "http://yn.189.cn/service/bill/feeQuery.jsp?SERV_NO=SHQD1",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = EnumMobileDeatilType.SMS.ToString() + i, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                //保存
                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "云南电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "云南电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("云南电信手机账单抓取异常", e);

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
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            MobileMongo mobileMongo = new MobileMongo(appDate);
            CrawlerData crawler = new CrawlerData();
            Basic mobile = new Basic();
            string result = string.Empty;
            List<string[]> results = new List<string[]>();
            List<string> _result = new List<string>();
            string nowYear = string.Empty;//当前年
            string infos = string.Empty;

            try
            {
                crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);
                if (crawler != null)
                    nowYear = DateTime.Parse(crawler.CrawlerDate).Year.ToString();

                #region 基本信息查询
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Analysis);

                mobile.Token = mobileReq.Token;
                mobile.BusName = mobileReq.Name;
                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.Mobile = mobileReq.Mobile;
                mobile.UpdateTime = crawler.CrawlerDate;

                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                _result = HtmlParser.GetResultFromParser(result, "//td[@id='NAMEA']");
                if (_result.Count > 0)
                {
                    mobile.Name = _result[0];//姓名
                }

                _result = HtmlParser.GetResultFromParser(result, "//td[@id='CUSTCARDNO']");
                if (_result.Count > 0)
                {
                    mobile.Idcard = _result[0];//证件号
                }

                _result = HtmlParser.GetResultFromParser(result, "//span[@id='span_RelaAddress']");
                if (_result.Count > 0)
                {
                    mobile.Address = _result[0];//地址
                }

                _result = HtmlParser.GetResultFromParser(result, "//span[@id='span_RelaMailNum']");
                if (_result.Count > 0)
                {
                    mobile.Postcode = _result[0];//邮编
                }

                _result = HtmlParser.GetResultFromParser(result, "//span[@id='span_RelaEmail']");
                if (_result.Count > 0)
                {
                    mobile.Email = _result[0];//邮箱
                }
                _result = HtmlParser.GetResultFromParser(result, "//table/tr");
                foreach (string item in _result)
                {
                    List<string> _result_th = HtmlParser.GetResultFromParser(item, "/th");
                    if (_result_th.Count > 0)
                    {
                        if (_result_th[0] == "创建日期：")
                        {
                            mobile.Regdate = HtmlParser.GetResultFromParser(item, "/td")[0];//创建时间
                            if (!mobile.Regdate.IsEmpty())
                                mobile.Regdate = new DateTime(mobile.Regdate.Substring(0, 4).ToInt(0), mobile.Regdate.Substring(4, 2).ToInt(0), mobile.Regdate.Substring(6, 2).ToInt(0)).ToString(Consts.DateFormatString11);//创建时间
                            break;
                        }

                    }
                }
                //套餐
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault().CrawlerTxt);
                _result = HtmlParser.GetResultFromParser(result, "//table[@name='tabs_tcinfo'][1]/tr[1]/td[2]");
                if (_result.Count > 0)
                {
                    mobile.Package = _result[0];//套餐（有多个套餐就选第一个）
                }

                //当前积分
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                _result = HtmlParser.GetResultFromParser(result, "//span");
                if (_result.Count > 0)
                {
                    mobile.Integral = _result[0];
                }

                //puk
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "pukInfor").FirstOrDefault().CrawlerTxt);
                string[] puk = result.Split(',');
                if (puk.Count() > 0)
                    mobile.PUK = puk[0];

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "个人信息解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 账单查询
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis);

                for (int i = 1; i <= 6; i++)
                {
                    decimal planAmt = 0;
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill" + i)).FirstOrDefault().CrawlerTxt);

                    MonthBill bill = new MonthBill();
                    bill.BillCycle = DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString12);
                    _result = HtmlParser.GetResultFromParser(result, "//table/tr/td/table[2]/tr");
                    if (_result.Count > 0)
                    {
                        foreach (string item in _result)
                        {
                            List<string> detail = HtmlParser.GetResultFromParser(item, "/td");
                            for (int j = 0; j < detail.Count; j++)
                            {
                                if (detail[j].Contains("套餐月基本费"))
                                {
                                    planAmt += detail[j + 1].Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace(" ", "").ToDecimal(0);
                                    break;
                                }
                            }
                        }
                    }

                    bill.PlanAmt = planAmt.ToString();
                    bill.TotalAmt = CommonFun.GetMidStr(result, "本期费用合计：", "&nbsp;");
                    mobile.BillList.Add(bill);
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 通话详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Analysis);

                for (int i = 0; i < 6; i++)
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (EnumMobileDeatilType.Call.ToString() + i)).FirstOrDefault().CrawlerTxt);
                    _result = HtmlParser.GetResultFromParser(result, "//table[@id='details_table']/tr");

                    if (_result.Count > 0)
                    {
                        foreach (string item in _result)
                        {
                            List<string> detail = HtmlParser.GetResultFromParser(item, "/td");
                            if (detail.Count != 9)
                                continue;

                            var totalSecond = 0;
                            var usetime = detail[5].ToString();
                            if (!string.IsNullOrEmpty(usetime))
                            {
                                totalSecond = CommonFun.ConvertDate(usetime);
                            }

                            Call call = new Call();
                            call.CallPlace = detail[1];
                            if (detail[2] == mobileReq.Mobile)
                            {
                                call.InitType = "主叫";
                                call.OtherCallPhone = detail[3];
                            }
                            else
                            {
                                call.InitType = "被叫";
                                call.OtherCallPhone = detail[2];
                            }
                            string[] temp_time = detail[4].Split('-');
                            if (temp_time.Count() == 4)
                                call.StartTime = DateTime.Parse(string.Format("{0}-{1}-{2} {3}", temp_time[0], temp_time[1], temp_time[2], temp_time[3])).ToString(Consts.DateFormatString11);
                            call.UseTime = totalSecond + "秒";
                            call.SubTotal = detail[6].ToDecimal(0) + detail[7].ToDecimal(0);
                            call.CallType = detail[8];

                            mobile.CallList.Add(call);
                        }
                    }
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 上网详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Analysis);

                for (int i = 0; i < 6; i++)
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (EnumMobileDeatilType.Net.ToString() + i)).FirstOrDefault().CrawlerTxt);
                    _result = HtmlParser.GetResultFromParser(result, "//table[@id='details_table']/tr");

                    if (_result.Count > 0)
                    {
                        foreach (string item in _result)
                        {
                            List<string> detail = HtmlParser.GetResultFromParser(item, "/td");
                            if (detail.Count != 8)
                                continue;
                            var totalSecond = 0;
                            var usetime = detail[3].ToString();
                            if (!string.IsNullOrEmpty(usetime))
                            {
                                totalSecond = CommonFun.ConvertDate(usetime);
                            }

                            Net net = new Net();
                            net.Place = detail[1];
                            string[] temp_time = detail[2].Split('-');
                            if (temp_time.Count() == 4)
                                net.StartTime = DateTime.Parse(string.Format("{0}-{1}-{2} {3}", temp_time[0], temp_time[1], temp_time[2], temp_time[3])).ToString(Consts.DateFormatString11);
                            net.UseTime = totalSecond + "秒";
                            net.SubFlow = detail[4] + "Kb";
                            net.NetType = detail[5];
                            net.SubTotal = detail[7].ToDecimal(0);

                            mobile.NetList.Add(net);
                        }
                    }
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 短信详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Analysis);

                for (int i = 0; i < 6; i++)
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (EnumMobileDeatilType.SMS.ToString() + i)).FirstOrDefault().CrawlerTxt);
                    _result = HtmlParser.GetResultFromParser(result, "//table[@id='details_table']/tr");

                    if (_result.Count > 0)
                    {
                        foreach (string item in _result)
                        {
                            List<string> detail = HtmlParser.GetResultFromParser(item, "/td");
                            if (detail.Count != 6)
                                continue;

                            Sms sms = new Sms();
                            sms.OtherSmsPhone = detail[1] == mobileReq.Mobile ? detail[2] : detail[1];
                            string[] temp_time = detail[3].Split('-');
                            if (temp_time.Count() == 4)
                                sms.StartTime = DateTime.Parse(string.Format("{0}-{1}-{2} {3}", temp_time[0], temp_time[1], temp_time[2], temp_time[3])).ToString(Consts.DateFormatString11);
                            sms.SubTotal = detail[4].ToDecimal(0);
                            sms.InitType = detail[5];

                            mobile.SmsList.Add(sms);
                        }
                    }
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                //保存
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "云南电信手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);

            }
            catch (Exception e)
            {
                Res.StatusDescription = "云南电信手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("云南电信手机账单解析异常", e);
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
                Log4netAdapter.WriteError("云南电信校验异常", e);
            }
            return false;
        }
    }
}
