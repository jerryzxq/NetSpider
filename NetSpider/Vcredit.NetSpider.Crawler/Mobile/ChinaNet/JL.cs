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
using Vcredit.NetSpider.PluginManager.Impl;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{
    public class JL : IMobileCrawler
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

        #region  私用变量
        public string contactID = "";

        #endregion
        public VerCodeRes MobileInit(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            string postdata = string.Empty;
            Res.Token = token;
            try
            {
                //第一步，初始化登录页面
                Url = "http://login.189.cn/login";
                httpItem = new HttpItem()
                {
                    URL = Url
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
                    FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                }
                Res.StatusDescription = "吉林电信初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "吉林电信初始化异常";
                Log4netAdapter.WriteError("吉林电信初始化异常", e);
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
                //Url = "http://login.189.cn/login";
                //var key = AES.GetMD5("login.189.cn");
                //AES.Key = key;
                //AES.IV = "1234567812345678";
                //mobileReq.Password = AES.AESEncrypt(mobileReq.Password);
                //Url = "http://login.189.cn/login";
                //postdata = string.Format("Account={0}&UType=201&ProvinceID=09&AreaCode=&CityNo=&RandomFlag=0&Password={1}&Captcha={2}", mobileReq.Mobile, mobileReq.Password.ToUrlEncode(), mobileReq.Vercode);
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "POST",
                //    Postdata = postdata,
                //    CookieCollection = cookies,
                //    Referer = "http://login.189.cn/login",
                //    Allowautoredirect = false,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='loginForm']", "data-errmsg");
                //if (results.Count > 0 && !results[0].IsEmpty())
                //{
                //    Res.StatusDescription = results[0];
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                //string location = httpResult.Header["Location"];
                //Url = location;
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "get",
                //    Referer = "http://login.189.cn/login",
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //string errorMsg = CommonFun.GetMidStr(httpResult.Html, "参数名: ", "");
                //if (errorMsg == "requestUriString")
                //{
                //    Res.StatusDescription = "你输入的密码有误！";
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //Url = "http://www.189.cn/jl/";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "get",
                //    Referer = location,
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

                //Url = "http://www.189.cn/dqmh/cms/index/login_jx.jsp";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "get",
                //    CookieCollection = cookies,
                //    Referer = "http://www.189.cn/jl/",
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

                //Url = "http://www.189.cn/jl/iframe/v2_head_search/index.html";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "get",
                //    CookieCollection = cookies,
                //    Referer = "http://www.189.cn/jl/",
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
                //    Method = "get",
                //    CookieCollection = cookies,
                //    Referer = "http://www.189.cn/jl/",
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
                if (ProvinceID != "09")
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
                postdata = string.Format("Account={0}&UType=201&ProvinceID={1}&AreaCode=&CityNo=&RandomFlag=0&Password={2}&Captcha={3}", mobileReq.Mobile, "09", PasswordAES.ToUrlEncode(), mobileReq.Vercode);
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
                postdata = string.Format("m=loadlogincaptcha&Account={0}&UType=201&ProvinceID=09&AreaCode=&CityNo=", mobileReq.Mobile);
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

                Url = "http://www.189.cn/jl";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = location,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://jl.189.cn/pages/login/sypay_group_new.jsp";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://jl.189.cn/pages/login/sypay_group_new.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/dqmh/flowrecommend.do";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/jl/",
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

                Url = "http://www.189.cn/login/sso/ecs.do?method=linkTo&platNo=10030&toStUrl=http://jl.189.cn/service/manage/modifyUserInfoFra.action?fastcode=00700588";
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
                Res.nextProCode = ServiceConsts.NextProCode_Vercode;
                CacheHelper.SetCache(Res.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "吉林电信手机登录异常";
                Log4netAdapter.WriteError("吉林电信手机登录异常", e);
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

                if (CacheHelper.GetCache(mobileReq.Token + "Flage") == null)
                {
                    CacheHelper.SetCache(mobileReq.Token + "Flage", "1");
                    #region  第一次发送图片验证码

                    Url = "http://jl.189.cn/authImg";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        ResultType = ResultType.Byte,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    //Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                    //Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                    //Res.VerCodeUrl = CommonFun.GetVercodeUrl(Res.Token);
                    //Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
                    //FileOperateHelper.WriteVerCodeImage(Res.Token, httpResult.ResultByte);
                    Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                    Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                    //保存验证码图片在本地
                    FileOperateHelper.WriteVerCodeImage(Res.Token, httpResult.ResultByte);
                    Res.VerCodeUrl = CommonFun.GetVercodeUrl(Res.Token);
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                    Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
                }
                else
                {
                    #region 第二次发送图片验证码
                    Url = "http://jl.189.cn/authImg?0.8461936176383773";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        ResultType = ResultType.Byte,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                    Res.VerCodeUrl = CommonFun.GetVercodeUrl(Res.Token);
                    Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                    FileOperateHelper.WriteVerCodeImage(Res.Token, httpResult.ResultByte);
                    #endregion
                    Res.StatusDescription = "吉林电信手机验证码发送成功";
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                    Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
                    CacheHelper.RemoveCache(mobileReq.Token + "Flage");
                    return Res;
                }
                    #endregion

                Url = "http://www.189.cn/jl/service/";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://www.189.cn/jl/",
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

                Url = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=00700588";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://www.189.cn/jl/service/",
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

                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10030&toStUrl=http://jl.189.cn/service/manage/modifyUserInfoFra.action?fastcode=00700588";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=00700588",
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
                #region
                Url = "http://jl.189.cn/service/bill/toDetailBillFra.action?fastcode=00710602";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);


                #endregion

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "吉林电信手机验证码发送异常";
                Log4netAdapter.WriteError("吉林电信手机验证码发送异常", e);
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
            DateTime date = DateTime.Now;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                    //CacheHelper.RemoveCache(mobileReq.Token);
                }


                if (CacheHelper.GetCache(mobileReq.Token + "Flage") == "1")
                {
                    Url = "http://jl.189.cn/commonquery/sendRandomcodeFra.action";
                    postdata = string.Format("randCode={0}", mobileReq.Vercode);
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
                    string tip = jsonParser.GetResultFromParser(httpResult.Html, "tip");
                    if (!tip.Contains("随机密码发送成功"))
                    {
                        Res.StatusDescription = tip;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    Res.nextProCode = ServiceConsts.NextProCode_SendSMSAndVercode;
                    Res.StatusDescription = "吉林电信手机验证码验证成功";
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                }
                else
                {
                    Url = "http://jl.189.cn/service/bill/doDetailBillFra.action";
                    postdata = string.Format("sRandomCode={0}&randCode={1}", mobileReq.Smscode, mobileReq.Vercode);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    string tip = jsonParser.GetResultFromParser(httpResult.Html, "tip");
                    string billDetailValidate = jsonParser.GetResultFromParser(httpResult.Html, "billDetailValidate");
                    if (billDetailValidate == "false")
                    {
                        Res.StatusDescription = tip;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    Res.StatusDescription = "吉林电信手机验证码验证成功";
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                    Res.nextProCode = ServiceConsts.NextProCode_Query;
                    return Res;
                }


                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "吉林电信手机验证码校验异常";
                Log4netAdapter.WriteError("吉林电信手机验证码校验异常", e);
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
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            CrawlerData crawler = new CrawlerData() { Token = mobileReq.Token, IdentityCard = mobileReq.IdentityCard, Name = mobileReq.Name, Mobile = mobileReq.Mobile, UserType = "1" };
            string carrier = string.Empty;//手机号归属地
            string Url = string.Empty;
            string Uid = string.Empty;
            string PhoneCostStr = string.Empty;
            string year = string.Empty;
            string filterfield = string.Empty;
            List<string> results = new List<string>();
            DateTime first = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            string postdata = String.Empty;
            DateTime last = DateTime.Now;
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }
                Url = "http://www.189.cn/jl/service/";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://www.189.cn/jl/",
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

                #region 基本信息
                Url = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=00700588";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://www.189.cn/jl/service/",
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

                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10030&toStUrl=http://jl.189.cn/service/manage/modifyUserInfoFra.action?fastcode=00700588";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=00700588",
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
                //基本信息
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "nameInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "基本信息抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  积分查询
                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10030&toStUrl=http://jl.189.cn/service/jf/pointQueryFra.action?fastcode=00700589";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=00700588",
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
                //积分查询
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "积分抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 套餐
                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10030&toStUrl=http://jl.189.cn/service/bill/cumulationInfoQueryFra.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=00700588",
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
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "packageBrandInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "套餐抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  账单查询

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Crawler);

                CrawlerBill(crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单抓取成功";
                appLog.LogDtlList.Add(logDtl);

                #endregion

                #region  ========详单========

                #region 语音账单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Crawler);

                CrawlerDeatils(mobileReq, "0", crawler);//漫游详单
                CrawlerDeatils(mobileReq, "1", crawler);//长途详单
                CrawlerDeatils(mobileReq, "2", crawler);//市话详单

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "语音账单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 短信账单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Crawler);

                CrawlerDeatils(mobileReq, "5", crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信账单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion



                #region 上网账单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Crawler);

                CrawlerDeatils(mobileReq, "4", crawler);//数据详单

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #endregion
                //保存
                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "吉林电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "吉林电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("吉林电信手机账单抓取异常", e);

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
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            MobileMongo mobileMongo = new MobileMongo(appDate);
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            CrawlerData crawler = new CrawlerData();
            Basic mobile = new Basic();
            string result = string.Empty;
            List<string> results = new List<string>();
            try
            {
                crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);

                #region  个人基本信息
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Analysis);
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "nameInfor").FirstOrDefault().CrawlerTxt);
                mobile.Token = mobileReq.Token;
                mobile.BusName = mobileReq.Name;
                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.Mobile = mobileReq.Mobile;
                mobile.UpdateTime = crawler.CrawlerDate;

                results = HtmlParser.GetResultFromParser(result, "//table[@class='ued-table-nobor mgt-15']/tr[1]/td[1]", "");
                if (results.Count > 0)
                {
                    mobile.Name = results[0].ToTrim("\t").ToTrim("\r\n");
                }
                results = HtmlParser.GetResultFromParser(result, "//table[@class='ued-table-nobor mgt-15']/tr[3]/td[1]", "");
                if (results.Count > 0)
                {
                    mobile.Idtype = results[0].ToTrim("\t").ToTrim("\r\n"); ;
                }
                results = HtmlParser.GetResultFromParser(result, "//table[@class='ued-table-nobor mgt-15']/tr[4]/td[1]", "");
                if (results.Count > 0)
                {
                    mobile.Idcard = results[0].ToTrim("\t").ToTrim("\r\n");
                }
                results = HtmlParser.GetResultFromParser(result, "//table[@class='ued-table-nobor mgt-15']/tr[4]/td[1]", "");
                if (results.Count > 0)
                {
                    mobile.Idcard = results[0].ToTrim("\t").ToTrim("\r\n");
                }
                results = HtmlParser.GetResultFromParser(result, "//table[@class='ued-table-nobor mgt-15']/tr[5]/td[1]/label/input[@class='textStyle1']", "value");
                if (results.Count > 0)
                {
                    mobile.Address = results[0].ToTrim("\t").ToTrim("\r\n");
                }
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "基本信息解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  积分
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//table[@class='table-form-12']/tr[1]/th[1]/span", "");
                if (results.Count > 0)
                {
                    mobile.Integral = results[0];
                }
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "积分解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  套餐
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageBrandInfor").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//p[@id='cumulation_mealSetInfo']", "");
                if (results.Count > 0)
                {
                    mobile.Package = results[0].ToTrim("\t").ToTrim("\r\n");
                }
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "套餐解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  账单解析
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis);

                AnalysisBill(crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  ========详单解析========

                #region 语音账单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Analysis);

                AnalysisDeatils(crawler, "0", mobile);//漫游详单解析
                AnalysisDeatils(crawler, "1", mobile);//长途详单
                AnalysisDeatils(crawler, "2", mobile);//市话详单

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 短信账单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Analysis);

                AnalysisDeatils(crawler, "5", mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion



                #region 上网账单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Analysis);

                AnalysisDeatils(crawler, "4", mobile);//数据详单

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #endregion
                //保存

                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "吉林电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "吉林电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("吉林电信手机账单抓取异常", e);
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
        /// <param name="crawler"></param>
        private void CrawlerBill(CrawlerData crawler)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> Months = new List<string>();
            string month = string.Empty;

            Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
            postdata = "fastcode=00710601";
            httpItem = new HttpItem()
            {
                URL = Url,
                Method = "post",
                Postdata = postdata,
                CookieCollection = cookies,
                Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=00710602",
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);

            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
            Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10030&toStUrl=http://jl.189.cn/service/bill/toBillQueryFra.action";
            httpItem = new HttpItem()
            {
                URL = Url,
                Method = "get",
                CookieCollection = cookies,
                Encoding = Encoding.GetEncoding("utf-8"),
                Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=00710602",
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);

            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
            for (int i = 1; i <= 6; i++)
            {

                if (DateTime.Now.AddMonths(-i).Month < 10)
                {
                    month = DateTime.Now.AddMonths(-i).Year.ToString() + "0" + DateTime.Now.AddMonths(-i).Month.ToString();
                }
                else
                {
                    month = DateTime.Now.AddMonths(-i).Year.ToString() + DateTime.Now.AddMonths(-i).Month.ToString();
                }
                Url = string.Format("http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10030&toStUrl=http://jl.189.cn/service/bill/queryBillInfoFra.action?billingCycle={0}", month);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://jl.189.cn/service/bill/toBillQueryFra.action",
                    ResultCookieType = ResultCookieType.CookieCollection,
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8"

                };
                httpResult = httpHelper.GetHtml(httpItem);

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
            }

            Url = "http://jl.189.cn/service/bill/realtimeFeeQueryFra.action?fastcode=00710598";
            httpItem = new HttpItem()
            {
                URL = Url,
                Method = "get",
                CookieCollection = cookies,
                Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=00710598",
                ResultCookieType = ResultCookieType.CookieCollection,
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8"

            };
            httpResult = httpHelper.GetHtml(httpItem);

            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
            crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (0), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
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
            MonthBill bill = null;
            string Month = string.Empty;

            for (var i = 0; i <= 5; i++)
            {
                PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                bill = new MonthBill();
                if (DateTime.Now.AddMonths(-(i + 1)).Month < 10)
                {
                    Month = DateTime.Now.AddMonths(-i).Year.ToString() + "0" + DateTime.Now.AddMonths(-(i + 1)).Month.ToString();
                    bill.BillCycle = DateTime.Parse(Month.Insert(4, "-") + "-01").ToString(Consts.DateFormatString12);
                }
                else
                {
                    Month = DateTime.Now.AddMonths(-i).Year.ToString() + DateTime.Now.AddMonths(-(i + 1)).Month.ToString();
                    bill.BillCycle = DateTime.Parse(Month.Insert(4, "-") + "-01").ToString(Consts.DateFormatString12);
                }
                string SXX = jsonParser.GetResultFromMultiNode(PhoneBillStr, "billItemList").TrimStart('[').TrimEnd(']');
                bill.TotalAmt = jsonParser.GetResultFromParser(SXX, "billFee");
                List<monthbill> Detail = jsonParser.DeserializeObject<List<monthbill>>(jsonParser.GetResultFromMultiNode(SXX, "acctItems"));
                bill.PlanAmt = Detail[0].acctItemFee;
                mobile.BillList.Add(bill);
            }
            PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (0))).FirstOrDefault().CrawlerTxt);
            bill = new MonthBill();
            if (DateTime.Now.AddMonths(0).Month < 10)
            {
                Month = DateTime.Now.AddMonths(0).Year.ToString() + "0" + DateTime.Now.AddMonths(0).Month.ToString();
                bill.BillCycle = DateTime.Parse(Month.Insert(4, "-") + "-01").ToString(Consts.DateFormatString12);
            }
            else
            {
                Month = DateTime.Now.AddMonths(0).Year.ToString() + DateTime.Now.AddMonths(0).Month.ToString();
                bill.BillCycle = DateTime.Parse(Month.Insert(4, "-") + "-01").ToString(Consts.DateFormatString12);
            }
            results = HtmlParser.GetResultFromParser(PhoneBillStr, "//span[@class='font-ore']", "");
            if (results.Count > 0)
            {
                bill.TotalAmt = results[1];
            }
            results = HtmlParser.GetResultFromParser(PhoneBillStr, "//table[@class='table-form-39 mgt-1']/tr[2]/td[2]", "");
            if (results.Count > 0)
            {
                bill.PlanAmt = results[0].ToTrim("\r").ToTrim("\n").ToTrim("\t");
            }
            mobile.BillList.Add(bill);
        }

        /// <summary>
        /// 抓取手机详单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <param name="bill_type"></param>
        /// <param name="crawler"></param>
        private void CrawlerDeatils(MobileReq mobileReq, String bill_type, CrawlerData crawler)
        {
            string Url = string.Empty;
            string pastdata = string.Empty;
            DateTime date = DateTime.Now;
            DateTime first = date;
            DateTime last = date;
            string result = String.Empty;
            string postdata = string.Empty;
            //int pageIndex = 1;
            //int pageCount = 0;

            //http://jl.189.cn/service/bill/doDetailBillFra.action
            //循环月份
            for (var i = 0; i <= 5; i++)
            {
                first = new DateTime(date.Year, date.Month, 1).AddMonths(-i);
                last = first.AddMonths(1).AddDays(-1);
                // month = first.ToString("yyyy-MM");
                int CurrentPage = 0;//当前页
                int totalPage = 0;//总页数
                //循环页数
                while (true)
                {
                    Url = "http://jl.189.cn/service/bill/billDetailQueryFra.action";
                    postdata = string.Format("billDetailValidate=true&billDetailType={0}&startTime={1}&endTime={2}&pagingInfo.currentPage={3}&contactID={4}", bill_type, first.ToString("yyyy-MM-dd"), last.ToString("yyyy-MM-dd"), CurrentPage.ToString(), contactID);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        Referer = "http://jl.189.cn/service/bill/toDetailBillFra.action?fastcode=00710602",
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    contactID = jsonParser.GetResultFromParser(httpResult.Html, "contactID");
                    totalPage = int.Parse(jsonParser.GetResultFromMultiNode(httpResult.Html, "pagingInfo:totalPage")); //获取总页数
                    CurrentPage = int.Parse(jsonParser.GetResultFromMultiNode(httpResult.Html, "pagingInfo:currentPage"));//获取当前页
                    if (CurrentPage == 0)//如果当前页没有数据，
                        CurrentPage = 1;
                    //类型+月份+页数
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = bill_type + first.Month + (CurrentPage < 10 ? "0" + CurrentPage.ToString() : CurrentPage.ToString()), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    CurrentPage++;
                    if (CurrentPage > totalPage) break;
                }

            }

        }
        /// <summary>
        /// 解析手机详单
        /// </summary>
        /// <param name="crawler"></param>
        /// <param name="bill_type"></param>
        /// <param name=ServiceConsts.SpiderType_Mobile></param>
        private void AnalysisDeatils(CrawlerData crawler, String bill_type, Basic mobile)
        {
            string PhoneCrawlerDtls = string.Empty;
            string PhoneStr = string.Empty;
            DateTime date = DateTime.Now;
            DateTime dt = date;
            Call phoneCall;//语音
            Sms phoneSMS;//短息
            Net phoneGPRS;//上网
            //循环月份
            for (var i = 0; i <= 5; i++)
            {
                int CurrentPage = 1;
                int totalPage = 0;
                //循环页数
                while (true)
                {
                    dt = new DateTime(date.Year, date.Month, 1).AddMonths(-i);
                    PhoneCrawlerDtls = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (bill_type + dt.Month + (CurrentPage < 10 ? "0" + CurrentPage.ToString() : CurrentPage.ToString()))).FirstOrDefault().CrawlerTxt);
                    totalPage = int.Parse(jsonParser.GetResultFromMultiNode(PhoneCrawlerDtls, "pagingInfo:totalPage")); //总页数
                    if (totalPage == 0)//如果当前页无数据，跳出
                        break;
                    if (bill_type == "5")//短信账单
                    {
                        phoneSMS = new Sms();
                        List<DuanXin> duanxinDetail = jsonParser.DeserializeObject<List<DuanXin>>(jsonParser.GetResultFromMultiNode(PhoneCrawlerDtls, "items"));
                        for (int j = 0; j < duanxinDetail.Count; j++)
                        {
                            phoneSMS.OtherSmsPhone = duanxinDetail[j].calledAccNbr;
                            phoneSMS.InitType = duanxinDetail[j].callType;
                            phoneSMS.SubTotal = duanxinDetail[j].messageFee.ToDecimal(0);
                            phoneSMS.StartTime = DateTime.Parse(duanxinDetail[j].beginTime.Replace("T", " ")).ToString(Consts.DateFormatString11);
                            mobile.SmsList.Add(phoneSMS);
                        }
                    }
                    else if (bill_type == "4")//上网详单 数据详单
                    {
                        phoneGPRS = new Net();
                        List<ShangWang> shangwangDetail = jsonParser.DeserializeObject<List<ShangWang>>(jsonParser.GetResultFromMultiNode(PhoneCrawlerDtls, "items"));
                        for (int j = 0; j < shangwangDetail.Count; j++)
                        {


                            phoneGPRS.StartTime = DateTime.Parse(shangwangDetail[j].beginTime.Replace("T", " ")).ToString(Consts.DateFormatString11);
                            phoneGPRS.Place = shangwangDetail[j].city;
                            phoneGPRS.NetType = shangwangDetail[j].netType;
                            phoneGPRS.SubTotal = shangwangDetail[j].messageFee.ToDecimal(0);
                            phoneGPRS.SubFlow = shangwangDetail[j].flow + "kb";
                            // TimeSpan? TS = shangwangDetail[j].endTime.ToDateTime() - shangwangDetail[j].beginTime.ToDateTime();
                            TimeSpan TS = (Convert.ToDateTime(shangwangDetail[j].endTime) - Convert.ToDateTime(shangwangDetail[j].beginTime));
                            phoneGPRS.UseTime = (int.Parse(TS.Minutes.ToString()) * 60 + int.Parse(TS.Seconds.ToString())) + "秒";
                            mobile.NetList.Add(phoneGPRS);
                        }
                    }
                    else  //"0"漫游详单解析,"1"长途详单,"2"市话详单
                    {
                        phoneCall = new Call();
                        List<YuYin> yuyinDetail = jsonParser.DeserializeObject<List<YuYin>>(jsonParser.GetResultFromMultiNode(PhoneCrawlerDtls, "items"));
                        for (int j = 0; j < yuyinDetail.Count; j++)
                        {
                            phoneCall.StartTime = DateTime.Parse(yuyinDetail[j].beginTime.Replace("T", " ")).ToString(Consts.DateFormatString11);
                            phoneCall.OtherCallPhone = yuyinDetail[j].calledAccNbr;
                            phoneCall.SubTotal = yuyinDetail[j].messageFee.ToDecimal(0);
                            phoneCall.UseTime = yuyinDetail[j].duration + "秒";
                            phoneCall.InitType = yuyinDetail[j].callType;
                            mobile.CallList.Add(phoneCall);
                        }
                    }
                    CurrentPage++;
                    //判断是否结束
                    if (CurrentPage > totalPage) break;
                }
            }
        }

        /// <summary>
        /// 校验验证码
        /// </summary>
        /// <param name="mobileReq"></param>
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
                Log4netAdapter.WriteError("吉林电信校验异常", e);
            }
            return true;
        }
        #endregion
        public class monthbill
        {
            public string acctItemFee { get; set; }
            public string acctItemID { get; set; }
            public string acctItemName { get; set; }
            public string billingID { get; set; }
            public string billingState { get; set; }
            public string billingTime { get; set; }
            public string channelID { get; set; }
            public string contactID { get; set; }
            public string method { get; set; }
            public string @operator { get; set; }
            public string serialID { get; set; }
            public string staffCode { get; set; }
            public string systemID { get; set; }
            public string upAcctItemID { get; set; }
        }

        public class DuanXin//短信
        {
            public string accNbr { get; set; }
            public string beginTime { get; set; }
            public string calledAccNbr { get; set; }
            public string callType { get; set; }
            public string messageFee { get; set; }
        }
        public class ShangWang
        {
            public string accNbr { get; set; }
            public string beginTime { get; set; }
            public string business { get; set; }
            public string city { get; set; }
            public string communicationFee { get; set; }
            public string endTime { get; set; }
            public string flow { get; set; }
            public string messageFee { get; set; }
            public string type { get; set; }
            public string netType { get; set; }
        }
        public class YuYin
        {
            public string accNbr { get; set; }
            public string beginTime { get; set; }
            public string callType { get; set; }
            public string calledAccNbr { get; set; }
            public string communicationFee { get; set; }
            public string duration { get; set; }
            public string endTime { get; set; }
            public string messageFee { get; set; }
            public string type { get; set; }
        }
    }
}
