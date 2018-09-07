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
using Newtonsoft.Json.Linq;
using Vcredit.NetSpider.DataAccess.Mongo;
using Newtonsoft.Json;
using System.IO;
using System.Xml;
using System.Web;
using Vcredit.Common.Constants;
using Vcredit.NetSpider.Entity.Mongo.Log;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{
    public class HE : IMobileCrawler
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
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
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

                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;
                Res.StatusDescription = "河北电信初始化完成";


                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "河北电信初始化异常";
                Log4netAdapter.WriteError("河北电信初始化异常", e);
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
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                //校验参数
                if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 老版登录
                ////第三步
                //Url = "http://login.189.cn/login";
                //var key = AES.GetMD5("login.189.cn");
                //AES.Key = key;
                //AES.IV = "1234567812345678";
                //mobileReq.Password = AES.AESEncrypt(mobileReq.Password);
                //postdata = string.Format("Account={0}&UType=201&ProvinceID=05&AreaCode=&CityNo=&RandomFlag=0&Password={1}&Captcha=", mobileReq.Mobile, mobileReq.Password.ToUrlEncode());
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "POST",
                //    Postdata = postdata,
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);

                //var msg = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='loginForm']", "data-errmsg");
                //if (msg.Count > 0)
                //{
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    Res.StatusDescription = msg[0];
                //    return Res;
                //}

                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                //Url = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=0043";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "GET",
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
                if (ProvinceID != "05")
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
                postdata = string.Format("Account={0}&UType=201&ProvinceID={1}&AreaCode=&CityNo=&RandomFlag=0&Password={2}&Captcha={3}", mobileReq.Mobile, "05", PasswordAES.ToUrlEncode(), mobileReq.Vercode);
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
                postdata = string.Format("m=loadlogincaptcha&Account={0}&UType=201&ProvinceID=05&AreaCode=&CityNo=", mobileReq.Mobile);
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

                Url = "http://www.189.cn/he";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = location,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://he.189.cn/pages/login/sypay_group_new.jsp";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://he.189.cn/pages/login/sypay_group_new.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/dqmh/flowrecommend.do";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/he/",
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

                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/club/mainclub/pointServ_iframe.jsp?DFlag=0&fastcode=00410426";
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
                Res.nextProCode = ServiceConsts.NextProCode_SendSMS;

                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "河北电信登录异常";
                Log4netAdapter.WriteError("河北电信登录异常", e);
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
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
                postdata = string.Format("fastcode=00380407");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                bool flag = true;
                while (flag)
                {
                    Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/public/postValidCode.jsp?NUM={0}&AREA_CODE=188&LOGIN_TYPE=21&OPER_TYPE=CR0&RAND_TYPE=002";
                    Url = string.Format(Url, mobileReq.Mobile);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "Get",
                        CookieCollection = cookies,
                        Referer = "http://he.189.cn/service/bill/feeQuery_iframe.jsp?SERV_NO=SHQD1",
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    if (!httpResult.Html.Contains("错误日志序号") && !httpResult.Html.Contains("notice"))
                    {
                        flag = false;
                        string res = getStringByElementName(httpResult.Html, "actionFlag");
                        if (res != "0")
                        {
                            Res.StatusDescription = getStringByElementName(httpResult.Html, "actionMsg");
                            Res.StatusCode = ServiceConsts.StatusCode_fail;
                            return Res;
                        }
                    }

                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.StatusDescription = "输入手机验证码，调用手机验证码验证接口";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "河北电信手机验证码发送异常";
                Log4netAdapter.WriteError("河北电信手机验证码发送异常", e);
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
            DateTime date = DateTime.Now;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                bool flag = true;
                while (flag)
                {
                    Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/public/pwValid.jsp?_FUNC_ID_=WB_VALID_RANDPWD&MOBILE_CODE={0}&ACC_NBR={1}&AREA_CODE=188&LOGIN_TYPE=21&MOBILE_FLAG=&MOBILE_LOGON_NAME=&MOBILE_CODE={2}";
                    Url = string.Format(Url, mobileReq.Smscode, mobileReq.Mobile, mobileReq.Smscode);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "GET",
                        Referer = "http://he.189.cn/service/bill/feeQuery_iframe.jsp?SERV_NO=SHQD1",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    if (!httpResult.Html.Contains("错误日志序号") && !httpResult.Html.Contains("notice"))
                    {
                        flag = false;
                        string res = getStringByElementName(httpResult.Html, "actionFlag");
                        if (res != "0")
                        {
                            Res.StatusDescription = getStringByElementName(httpResult.Html, "actionMsg");
                            Res.StatusCode = ServiceConsts.StatusCode_fail;
                            return Res;
                        }
                    }
                }

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.StatusDescription = "河北电信手机验证码验证成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Query;

                CacheHelper.SetCache(mobileReq.Token, cookies);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "河北电信手机验证码验证异常";
                Log4netAdapter.WriteError("河北电信手机验证码验证异常", e);
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
            string postdata = string.Empty;
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                #region 基本信息
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Crawler);

                #region 用户基本信息

                Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
                postdata = string.Format("fastcode=	00420429");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    CookieCollection = cookies,
                    Postdata = postdata,
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

                bool flag = true;
                while (flag)
                {
                    Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/service/manage/index_iframe.jsp?FLAG=1";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    if (httpResult.Html.Contains("我的资料"))
                    {
                        var idcard = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='CUSTCARDNO']", "")[0];
                        if (!string.IsNullOrEmpty(idcard))
                        {
                            flag = false;
                        }
                    }
                }
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                #endregion

                #region 套餐
                Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
                postdata = string.Format("fastcode=00420427");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    CookieCollection = cookies,
                    Postdata = postdata,
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

                Url = "http://login.189.cn/redirect/ECSSSOTransit?Req=10036%24Gof%2B26znbn46uddRn%2FvywlkmvyXpaH8xmi2EjAcLaKacQUjU2fA9r2rQT1%2F%2BEfHq17NdUy3BIGcx%0AO9Mph5z0tchfnIiI8kovhdSPMVOzXX5LDUIdg9IiCdFB4wXdWPk8gTS49Q2KknX%2FFxQsCMOEiCuH%0A6QVgEGXphJKd4u0CnhUhjWfwsNYMbbwwujeXwURMTjOI17Ecj9Gw7YMZSTPj2fPmBnmgsSfYqn9Y%0AVHiVQjVxGsfd4CDX8A%3D%3D";
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

                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/service/manage/mySelfProdIndex_iframe.jsp";
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

                flag = true;
                while (flag)
                {
                    Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/service/manage/prod_baseinfo_iframe.jsp";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "GET",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (!httpResult.Html.Contains("错误日志序号") && !httpResult.Html.Contains("notice"))
                    {
                        flag = false;
                    }

                }
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "otherInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                #endregion

                #region 积分
                Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
                postdata = string.Format("fastcode=00410426");
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

                Url = "http://login.189.cn/redirect/ECSSSOTransit?Req=10036%24Gof%2B26znbn46uddRn%2FvywlkmvyXpaH8xmi2EjAcLaKacQUjU2fA9r2rQT1%2F%2BEfHq17NdUy3BIGcx%0AO9Mph5z0tchfnIiI8kovhdSPMVOzXX5LDUIdg9IiCdFB4wXdWPk8gTS49Q2KknX%2FFxQsCMOEiCuH%0A6QVgEGXphJKd4u0CnhUhjWfwsNYMbbwwujeXwURMTjOI17Ecj9Gw7YMZSTPj2fPmBnmgsSfYqn9Y%0AVHiVQjVxGsfd4CDX8A%3D%3D";
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

                flag = true;
                while (flag)
                {
                    Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/club/mainclub/pointServ_iframe.jsp?DFlag=0";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (!httpResult.Html.Contains("错误日志序号") && !httpResult.Html.Contains("notice"))
                    {
                        flag = false;
                    }

                }
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                #endregion

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "基本信息抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 月消费情况
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Crawler);

                CrawlerBill(crawler, mobileReq);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 详单查询

                Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
                postdata = "fastcode=00380407";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    CookieCollection = cookies,
                    Postdata = postdata,
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

                #region 话费详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.Call, mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 短信详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.SMS, mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 上网详单
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
                Res.StatusDescription = "河北电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusDescription = "河北电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("河北电信手机账单抓取异常", e);

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
            List<string> results = new List<string>();
            try
            {
                crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);

                #region 基本信息
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Analysis);

                mobile.Token = mobileReq.Token;
                mobile.BusName = mobileReq.Name;
                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.Mobile = mobileReq.Mobile;
                mobile.UpdateTime = crawler.CrawlerDate;

                #region 基本信息查询

                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//table/tr/td");
                if (results.Count != 0)
                {
                    mobile.Name = results[0];
                    mobile.StarLevel = results[3];
                    mobile.Idtype = results[4];
                    mobile.Idcard = results[5];
                    mobile.Address = HtmlParser.GetResultFromParser(results[6], "//span")[0]; ;
                }
                #endregion

                #region 套餐
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "otherInfor").FirstOrDefault().CrawlerTxt);
                var tableid = "tab_prodinfo_" + mobileReq.Mobile;
                results = HtmlParser.GetResultFromParser(result, "//table[@id='" + tableid + "']/tr/td");
                if (results.Count != 0)
                {
                    mobile.Package = results[5];
                    mobile.PackageBrand = results[3];
                    mobile.Regdate = DateTime.Parse(results[9]).ToString(Consts.DateFormatString11);
                }

                #endregion

                #region 积分
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//span[@class='sum'][1]");
                if (results.Count != 0)
                {
                    mobile.Integral = results[0]; //积分
                }
                #endregion

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "个人信息解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 月消费情况
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis);

                AnalysisBill(crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 详单查询

                #region 通话详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Analysis);

                AnalysisDeatils(EnumMobileDeatilType.Call, crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 上网详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Analysis);

                AnalysisDeatils(EnumMobileDeatilType.SMS, crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  短信详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Analysis);

                AnalysisDeatils(EnumMobileDeatilType.Net, crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #endregion

                //保存
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "河北电信手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);

            }
            catch (Exception e)
            {
                Res.StatusDescription = "河北电信手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("河北电信手机账单解析异常", e);

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


        private void CrawlerBill(CrawlerData crawler, MobileReq mobileReq)
        {
            string Url = String.Empty;
            var postdata = String.Empty;
            DateTime date = DateTime.Now;
            string AREA_CODE = string.Empty;
            string NUM = string.Empty;
            string PROD_NO = string.Empty;
            string ServiceKind = string.Empty;
            string USER_ID = string.Empty;
            string USER_NAME = string.Empty;

            //校验
            Url = string.Format("http://www.189.cn/dqmh/my189/checkMy189Session.do");
            postdata = string.Format("fastcode=00380405");
            httpItem = new HttpItem()
            {
                URL = Url,
                Method = "POST",
                CookieCollection = cookies,
                Postdata = postdata,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
            for (var i = 0; i <= 5; i++)
            {

                if (i == 0)
                {
                    Url = " http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/service/bill/feeQuery_iframe.jsp?SERV_NO=sshf";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    List<string> infos = HtmlParser.GetResultFromParser(httpResult.Html, "//script[2]");
                    if (infos.Count > 0)
                    {
                        NUM = infos[1].Split(',')[0].Replace("'", "").Split('(')[1];
                        AREA_CODE = infos[1].Split(',')[1].Replace("'", "");
                        PROD_NO = infos[1].Split(',')[2].Replace("'", "");
                        USER_NAME = infos[1].Split(',')[3].Replace("'", "");
                        ServiceKind = infos[1].Split(',')[4].Replace("'", "");
                        USER_ID = infos[1].Split(',')[5].Replace("'", "").Split(')')[0];
                    }
                    bool flag = true;
                    while (flag)
                    {
                        Url = " http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/service/bill/action/ifr_rt_iframe.jsp?NUM={0}&AREA_CODE={1}&PROD_NO={2}&USER_NAME={3}&ServiceKind={4}&USER_ID={5}";
                        Url = string.Format(Url, NUM, AREA_CODE, PROD_NO, USER_NAME.ToUrlEncode(), ServiceKind, USER_ID);
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "GET",
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        if (!httpResult.Html.Contains("错误日志序号") && !httpResult.Html.Contains("notice"))
                        {
                            flag = false;
                        }
                    }

                }
                else
                {
                    date = date.AddMonths(-1);
                    bool flag = true;
                    while (flag)
                    {
                        Url = string.Format("http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/service/bill/action/bill_month_list_detail_iframe.jsp?ACC_NBR={0}&SERVICE_KIND=8&feeDate={1}&retCode=0000", mobileReq.Mobile, date.ToString("yyyyMM"));
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "GET",
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        if (!httpResult.Html.Contains("错误日志序号") && !httpResult.Html.Contains("notice"))
                        {
                            flag = false;
                        }
                    }
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
            List<string> infos = new List<string>();
            MonthBill bill = null;
            for (var i = 0; i <= 5; i++)
            {
                PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                bill = new MonthBill();
                bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);
                if (i == 0)
                {
                    infos = HtmlParser.GetResultFromParser(PhoneBillStr, "//div[@class='bill_content']");
                    if (infos.Count != 0)
                    {
                        bill.PlanAmt = HtmlParser.GetResultFromParser(infos[0], "//table/tr[2]/td[2]")[0];
                        bill.TotalAmt = HtmlParser.GetResultFromParser(infos[0], "//p/span[1]")[0];
                    }
                }
                else
                {
                    infos = HtmlParser.GetResultFromParser(PhoneBillStr, "//table");
                    if (infos.Count > 0)
                    {
                        var TotalAmtInfo = HtmlParser.GetResultFromParser(PhoneBillStr, "//table/tr/th/div");
                        if (TotalAmtInfo.Count != 0)
                        {
                            bill.TotalAmt = TotalAmtInfo[0].Split('：')[1].Replace("元", "");
                        }
                        var PlanAmtInfo = HtmlParser.GetResultFromParser(PhoneBillStr, "//table[@class='tableLeft']/tr[2]/td[2]/span");
                        if (PlanAmtInfo.Count > 0)
                        {
                            bill.PlanAmt = PlanAmtInfo[0];
                        }
                    }
                }
                ///添加账单
                mobile.BillList.Add(bill);
            }
        }

        /// <summary>
        /// 抓取手机详单
        /// </summary>
        /// <param name="queryType">2 获取通话记录，4 上网详单 3 短信详单</param>
        /// <returns></returns>
        private void CrawlerDeatils(EnumMobileDeatilType type, MobileReq mobileReq, CrawlerData crawler)
        {
            string Url = string.Empty;
            var startDate = String.Empty;
            var endDate = String.Empty;
            string queryType = string.Empty;
            string queryName = string.Empty;

            if (type == EnumMobileDeatilType.Call)
            {
                queryType = "1";
                queryName = "移动语音详单";
            }
            else if (type == EnumMobileDeatilType.SMS)
            {
                queryType = "2";
                queryName = "移动短信详单";
            }
            else
            {
                queryType = "3";
                queryName = "移动上网详单";
            }

            for (int i = 0; i <= 5; i++)
            {
                DateTime date = DateTime.Now;
                date = date.AddMonths(-i);
                startDate = new DateTime(date.Year, date.Month, 1).ToString("yyyy-MM-dd");
                endDate = new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
                Url = string.Format("http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/service/bill/action/ifr_bill_detailslist_em_new_iframe.jsp?ACC_NBR={0}&CITY_CODE=188&BEGIN_DATE={1}&END_DATE={2}&FEE_DATE={3}&SERVICE_KIND=8&retCode=0000&QUERY_TYPE_NAME={4}&QUERY_TYPE={5}&radioQryType=on&QRY_FLAG=1&ACCT_DATE={6}&ACCT_DATE_1={7}&openFlag=1", mobileReq.Mobile, startDate + "+00%3A00%3A00", endDate + "+23%3A59%3A59", date.ToString("yyyyMM"), queryName.ToUrlEncode(), queryType, date.ToString("yyyyMM"), date.ToString("yyyyMM"));

                bool flag = true;
                while (flag)
                {
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "GET",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (!string.IsNullOrEmpty(httpResult.Html) && !httpResult.Html.Contains("错误日志序号") && !httpResult.Html.Contains("notice"))
                    {
                        flag = false;
                        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                        crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + "0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    }
                }
            }

        }

        /// <summary>
        /// 解析手机详单
        /// </summary>
        /// <param name="queryType">1 获取通话记录，3 上网详单 2 短信详单</param>
        /// <returns></returns>
        private void AnalysisDeatils(EnumMobileDeatilType type, CrawlerData crawler, Basic mobile)
        {
            string PhoneCostStr = string.Empty;
            for (int i = 0; i <= 5; i++)
            {
                PhoneCostStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                var infos = HtmlParser.GetResultFromParser(PhoneCostStr, "//table[@id='details_table']/tbody/tr", "inner");
                foreach (var item in infos)
                {
                    //短信详单
                    if (type == EnumMobileDeatilType.SMS)
                    {
                        List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        Sms sms = new Sms();
                        if (tdRow.Count > 0)
                        {
                            sms.OtherSmsPhone = tdRow[1];
                            sms.InitType = tdRow[2];
                            sms.StartTime = DateTime.Parse(tdRow[3]).ToString(Consts.DateFormatString11);
                            sms.SubTotal = tdRow[4].ToDecimal(0);
                            mobile.SmsList.Add(sms);
                        }
                    }
                    else if (type == EnumMobileDeatilType.Net)    //上网详单
                    {
                        List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        Net gprs = new Net();
                        if (tdRow.Count > 0)
                        {

                            var totalSecond = 0;
                            var usetime = tdRow[2].ToString();
                            if (!string.IsNullOrEmpty(usetime))
                            {
                                totalSecond = CommonFun.ConvertDate(usetime);
                            }
                            var totalFlow = CommonFun.ConvertGPRS(tdRow[3].ToString());

                            gprs.StartTime = DateTime.Parse(tdRow[1]).ToString(Consts.DateFormatString11);
                            gprs.Place = tdRow[5];
                            gprs.NetType = tdRow[4];
                            gprs.SubTotal = tdRow[7].ToDecimal(0);
                            gprs.SubFlow = totalFlow.ToString();
                            gprs.UseTime = totalSecond.ToString();
                            gprs.PhoneNetType = tdRow[6];
                            mobile.NetList.Add(gprs);
                        }
                    }
                    else      //通话详单
                    {
                        List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        Call call = new Call();
                        if (tdRow.Count > 0)
                        {
                            call.StartTime = DateTime.Parse(tdRow[3]).ToString(Consts.DateFormatString11);
                            call.OtherCallPhone = tdRow[1];
                            call.UseTime = tdRow[4].ToString();
                            call.CallType = tdRow[9];
                            call.InitType = tdRow[2];
                            call.SubTotal = tdRow[10].ToDecimal(0);
                            mobile.CallList.Add(call);
                        }
                    }
                }

            }

        }

        public string getStringByElementName(string soure, string elementname)
        {
            string resultStr = String.Empty;
            using (StringReader strRdr = new StringReader(soure))
            {
                //通过XmlReader.Create静态方法创建XmlReader实例
                using (XmlReader rdr = XmlReader.Create(strRdr))
                {
                    //循环Read方法直到文档结束
                    while (rdr.Read())
                    {
                        //如果是开始节点
                        if (rdr.NodeType == XmlNodeType.Element)
                        {
                            string elementName = rdr.Name;
                            if (elementName == elementname)
                            {
                                //读取到节点内文本内容
                                if (rdr.Read())
                                {
                                    //通过rdr.Value获得文本内容
                                    resultStr = HttpUtility.HtmlDecode(rdr.Value);
                                    break;

                                }
                            }
                        }
                    }
                }
            }

            return resultStr;
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
                Log4netAdapter.WriteError("湖北电信校验异常", e);
            }
            return false;
        }
        #endregion

    }
}
