using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Mongo.Log;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.Entity.Service.Mobile;
using Vcredit.NetSpider.PluginManager.Impl;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{
    public class SX : IMobileCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult;
        HttpItem httpItem;
        ApplyLogMongo logMongo = new ApplyLogMongo();
        #endregion
        #region 私有变量

        private string cookieStr = string.Empty;
        #endregion
        /// <summary>
        /// 页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes MobileInit(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            string Url = string.Empty;
            try
            {

                Url = "http://login.189.cn/login";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
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
                        Cookie = cookieStr,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (httpResult.StatusCode != HttpStatusCode.OK)
                    {
                        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                    FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                    Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                }
                //else
                //{
                //    Url = "http://sx.189.cn/authImg?type=2";//5
                //    httpItem = new HttpItem()
                //    {
                //        Accept = "image/png,image/*;q=0.8,*/*;q=0.5",
                //        URL = Url,
                //        Host = "sx.189.cn",
                //        //Referer = "http://sx.189.cn/service/bill/toDetailBill.action",
                //        Cookie = cookieStr,
                //        ResultType = ResultType.Byte
                //    };
                //    httpResult = httpHelper.GetHtml(httpItem);
                //    if (httpResult.StatusCode != HttpStatusCode.OK)
                //    {
                //        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //        Res.StatusCode = ServiceConsts.StatusCode_fail;
                //        return Res;
                //    }
                //    cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                //    Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //    //保存验证码图片在本地
                //    FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                //    Res.StatusDescription = "山西电信初始化完成,请输入正确的图片验证码,否则会造成详单获取失败";
                //}

                Res.StatusDescription = "山西电信初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;
                CacheHelper.SetCache(token, cookieStr);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "山西电信初始化异常";
                Log4netAdapter.WriteError("山西电信初始化异常", e);
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
        /// 登录
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
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
                    cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
                    CacheHelper.RemoveCache(mobileReq.Token);
                }
                //校验参数

                if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }


                #region 执行第一遍登录
                Url = "http://login.189.cn/login/ajax";
                postdata = string.Format("m=checkphone&phone={0}", mobileReq.Mobile);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                JObject jsonObj = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                string ProvinceID = jsonObj["ProvinceID"].ToString();
                if (ProvinceID != "06")
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
                postdata = string.Format("Account={0}&UType=201&ProvinceID={1}&AreaCode=&CityNo=&RandomFlag=0&Password={2}&Captcha={3}", mobileReq.Mobile, "06", PasswordAES.ToUrlEncode(), mobileReq.Vercode);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Allowautoredirect = false,
                    Referer = "http://login.189.cn/login",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                string location = httpResult.Header["Location"];
                Url = location;
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://login.189.cn/login",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='loginForm']", "data-errmsg");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = "http://login.189.cn/login/ajax";
                postdata = string.Format("m=loadlogincaptcha&Account={0}&UType=201&ProvinceID=06&AreaCode=&CityNo=", mobileReq.Mobile);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://login.189.cn/login",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                jsonObj = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                int FailTimes = jsonObj["FailTimes"].ToString().ToInt(0);
                int LockTimes = jsonObj["LockTimes"].ToString().ToInt(0);
                if (FailTimes != 0)
                {
                    Res.StatusDescription = string.Format("您的密码错误！再连续{0}次输入错误，账号将被锁！", LockTimes - FailTimes);
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }



                Url = "http://www.189.cn/sx";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://login.189.cn/login",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Url = "http://www.189.cn/html/login/index.html";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/sx/",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);


                Url = "http://sx.189.cn/pages/login/sypay_group_new.jsp";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://sx.189.cn/pages/login/sypay_group_new.jsp",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Url = "http://www.189.cn/dqmh/flowrecommend.do";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/sx/",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                #endregion

                #region 执行第二遍登录
                Url = "http://login.189.cn/login/ajax";
                postdata = string.Format("m=checkphone&phone={0}", mobileReq.Mobile);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                jsonObj = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                ProvinceID = jsonObj["ProvinceID"].ToString();
                if (ProvinceID != "06")
                {
                    Res.StatusDescription = "用户名不存在";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                key = AES.GetMD5("login.189.cn");
                AES.Key = key;
                AES.IV = "1234567812345678";
                PasswordAES = AES.AESEncrypt(mobileReq.Password);
                Url = "http://login.189.cn/login";
                postdata = string.Format("Account={0}&UType=201&ProvinceID={1}&AreaCode=&CityNo=&RandomFlag=0&Password={2}&Captcha={3}", mobileReq.Mobile, "06", PasswordAES.ToUrlEncode(), mobileReq.Vercode);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Allowautoredirect = false,
                    Referer = "http://login.189.cn/login",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                location = httpResult.Header["Location"];
                Url = location;
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://login.189.cn/login",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='loginForm']", "data-errmsg");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = "http://login.189.cn/login/ajax";
                postdata = string.Format("m=loadlogincaptcha&Account={0}&UType=201&ProvinceID=06&AreaCode=&CityNo=", mobileReq.Mobile);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://login.189.cn/login",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                jsonObj = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                FailTimes = jsonObj["FailTimes"].ToString().ToInt(0);
                LockTimes = jsonObj["LockTimes"].ToString().ToInt(0);
                if (FailTimes != 0)
                {
                    Res.StatusDescription = string.Format("您的密码错误！再连续{0}次输入错误，账号将被锁！", LockTimes - FailTimes);
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }



                Url = "http://www.189.cn/sx";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://login.189.cn/login",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Url = "http://www.189.cn/html/login/index.html";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/sx/",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);


                Url = "http://sx.189.cn/pages/login/sypay_group_new.jsp";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://sx.189.cn/pages/login/sypay_group_new.jsp",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Url = "http://www.189.cn/dqmh/flowrecommend.do";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/sx/",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                #endregion


                Url = "http://www.189.cn/login/index.do";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/html/login/index.html",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);



                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10007&toStUrl=http://sx.189.cn/service/bill/realtimeFeeQuery.action";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/sx/",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Vercode;
                CacheHelper.SetCache(mobileReq.Token, cookieStr);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "山西电信初始化异常";
                Log4netAdapter.WriteError("山西电信初始化异常", e);
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


        /// <summary>
        /// 发送短信验证码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            VerCodeRes Res = new VerCodeRes();
            Res.Token = mobileReq.Token;
            cookieStr = string.Empty;
            string Url = string.Empty;
            string postdata = string.Empty;

            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
                //发送图片验证码
                if (CacheHelper.GetCache(mobileReq.Token + "Flage") == null)
                {
                    CacheHelper.SetCache(mobileReq.Token + "Flage", "1");
                    //第一次验证是否存在图片验证码，没有则先生成验证码
                    Url = "http://sx.189.cn/authImg?type=2";
                    httpItem = new HttpItem()
                    {
                        Accept = "image/png,image/*;q=0.8,*/*;q=0.5",
                        URL = Url,
                        Host = "sx.189.cn",
                        //Referer = "http://sx.189.cn/service/bill/toDetailBill.action",
                        Cookie = cookieStr,
                        ResultType = ResultType.Byte
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (httpResult.StatusCode != HttpStatusCode.OK)
                    {
                        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                    Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                    Res.VerCodeUrl = CommonFun.GetVercodeUrl(Res.Token);
                    Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                    FileOperateHelper.WriteVerCodeImage(Res.Token, httpResult.ResultByte);

                    Res.StatusDescription = "山西电信详单查询图片验证码发送成功";
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                    Res.nextProCode = ServiceConsts.NextProCode_SendSMS;
                    CacheHelper.SetCache(mobileReq.Token, cookieStr);
                    return Res;
                }

                //跳转至详单链接验证开始
                Url = "http://www.189.cn/sx/service";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/sx/",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10007&toStUrl=http://sx.189.cn/service/bill/billQuery.action";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/sx/service/",
                    Allowautoredirect = false,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                string location = httpResult.Header["Location"];

                Url = location;
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/sx/service/",
                    Allowautoredirect = false,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                location = httpResult.Header["Location"];

                Url = location;
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/sx/service/",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                //跳转至详单链接验证结束
                Url = "http://sx.189.cn/service/bill/toDetailBill.action";//4 billQuery
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "sx.189.cn",
                    Cookie = cookieStr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);



                //*******************************************************************
                //     验证码生成,放在初始化界面了,在登录界面输入,这里直接拿来用了
                //*******************************************************************

                //校验图片验证码，发送验证短信
                Url = "http://sx.189.cn/service/bill/sendRandomcode.action";
                postdata = string.Format("randCode={0}", mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    Method = "post",
                    Postdata = postdata,
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Host = "sx.189.cn",
                    Cookie = cookieStr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                if (!httpResult.Html.Trim().StartsWith("{") && !httpResult.Html.Trim().EndsWith("{"))
                {
                    Res.StatusDescription = "验证码获取错误,请刷新页面后重试";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                JObject jsonObj = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                string errorMsg = jsonObj["tip"].ToString();
                if (errorMsg.IndexOf("成功", StringComparison.Ordinal) == -1)
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = "输入手机验证码，调用手机验证码验证接口";
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;

                CacheHelper.SetCache(mobileReq.Token, cookieStr);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "山西电信手机验证码发送异常";
                Log4netAdapter.WriteError("山西电信手机验证码发送异常", e);
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
        /// <summary>
        /// 校验短信验证码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            cookieStr = string.Empty;
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);

                //验证短信
                Url = "http://sx.189.cn/checkrand/checkRand.action";
                postdata = string.Format("randValue={0}", mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    Host = "sx.189.cn",
                    Cookie = cookieStr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                if (!httpResult.Html.StartsWith("{") && !httpResult.Html.EndsWith("{"))
                {
                    Res.StatusDescription = "图片验证码错误,刷新页面后重试";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                JObject JsonObj = (JObject)JsonConvert.DeserializeObject(httpResult.Html);

                if (JsonObj["randValue"].ToString() != mobileReq.Vercode)
                {
                    Res.StatusDescription = "图片验证码错误,刷新页面后重试";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = "http://sx.189.cn/service/bill/validateRandomcode.action";
                postdata = string.Format("sRandomCode={0}&requestFlag={1}&randCode={2}&cardId=", mobileReq.Smscode, "synchronization", mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    Host = "sx.189.cn",
                    Cookie = cookieStr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                if (!httpResult.Html.StartsWith("{") && !httpResult.Html.EndsWith("{"))
                {
                    Res.StatusDescription = "短信验证码验证失败,刷新页面后重试";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                JsonObj = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                string errorMsg = JsonObj["tip"].ToString();
                if (errorMsg.IndexOf("成功", StringComparison.Ordinal) == -1 && !string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.StatusDescription = "山西电信手机验证码验证成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Query;

                CacheHelper.SetCache(mobileReq.Token, cookieStr);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "山西电信手机验证码验证异常";
                Log4netAdapter.WriteError("山西电信手机验证码验证异常", e);
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
            string Url = string.Empty;
            string postdata = string.Empty;

            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
                    CacheHelper.RemoveCache(mobileReq.Token);
                }
                #region 基本信息

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Crawler);

                //用户中心
                Basic mobile = new Basic();
                Url = "http://sx.189.cn/service/account/customerHome.action";
                httpItem = new HttpItem
                {
                    URL = Url,
                    //Referer = location,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='tip-common-2 customer-panel']/span[@class='span-1']", "text");

                //积分
                Url = "http://sx.189.cn/service/jf/integralSearch.action?requestFlag=asynchronism";
                postdata = string.Format("time={0}", DateTime.Now);//Fri Oct 09 2015 15:52:52 GMT+0800
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://sx.189.cn/service/account/customerHome.action",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                //mobile.Integral = jsonParser.GetResultFromMultiNode(httpResult.Html, "pointDeposit:usablePoint");//积分

                Url = "http://sx.189.cn/service/manage/modifyUserInfo.action";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://sx.189.cn/service/account/customerHome.action",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                // results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table-form margin-top']/tbody/tr[3]/td", "text");

                #region 套餐(为运行效率省略该步,在下步历史账单查询步骤中,可以获取)
                //Url = "http://sx.189.cn/service/manage/packageListQuery.action";
                //postdata = string.Format("requestFlag=asynchronism&prodNumber={0}&prodType=4&time={1}", mobileReq.Mobile, DateTime.Now);
                //httpItem = new HttpItem
                //{
                //    URL = Url,
                //    Method = "post",
                //    Postdata = postdata,
                //    Referer = "http://sx.189.cn/service/account/customerHome.action",
                //    Cookie = cookieStr
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                //JArray typeNewsList = (JArray)JsonConvert.DeserializeObject(jsonParser.GetResultFromMultiNode(httpResult.Html, "retInfo:list"));
                //foreach (JToken typeNews in typeNewsList)
                //{
                //    if (typeNews["offerType"].ToString() == "套餐销售品")
                //    {
                //        string temp = typeNews["offerName"].ToString();
                //        mobile.PackageBrand = temp.Substring(0, temp.LastIndexOf("-", StringComparison.Ordinal));// 套餐品牌
                //        mobile.Package = temp.Substring(temp.LastIndexOf("-", StringComparison.Ordinal) + 1, temp.Length - temp.LastIndexOf("-", StringComparison.Ordinal) - 1);//当前手机套餐
                //    }
                //}
                #endregion

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "基本信息抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region  历史账单

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Crawler);
                Url = "http://sx.189.cn/service/bill/billQuery.action";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://sx.189.cn/service/account/customerHome.action",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                int dtNow = int.Parse(DateTime.Now.AddMonths(-1).ToString("yyyyMM"));
                for (int i = 0; i < 6; i++)
                {
                    Url = string.Format("http://sx.189.cn/service/bill/queryBillInfo.action?billCycle={0}", dtNow - i);
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Referer = "http://sx.189.cn/service/bill/billQuery.action",
                        Cookie = cookieStr
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "monthBillInfor" + (dtNow - i), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                }
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "月账单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                //billDetailType:2003语音详单，2004短信详单，2005无线宽带详单
                #region

                #region 语音详单

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Crawler);

                GetHtmlDetails(EnumMobileDeatilType.Call, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 短信详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Crawler);

                GetHtmlDetails(EnumMobileDeatilType.SMS, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 流量详单

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Crawler);

                GetHtmlDetails(EnumMobileDeatilType.Net, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #endregion
                //保存
                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "山西电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                CacheHelper.SetCache(mobileReq.Token, cookieStr);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "山西电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("山西电信手机账单抓取异常", e);

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
            Regex regex = new Regex(@"[\&nbsp;\s]");
            int dateNowBill = int.Parse(DateTime.Now.AddMonths(-1).ToString("yyyyMM"));
            int endTimeBill = dateNowBill - 5;

            try
            {
                crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);
                #region 基本信息查询

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Analysis);
                mobile.Token = mobileReq.Token;
                mobile.BusName = mobileReq.Name;
                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.Mobile = mobileReq.Mobile;
                mobile.UpdateTime = crawler.CrawlerDate;

                //当前积分
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                mobile.Integral = jsonParser.GetResultFromMultiNode(result, "pointDeposit:usablePoint");//积分
                //基本资料
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//b[@class='font-orange']", "text");
                if (results.Count > 0)
                {
                    mobile.Mobile = results[0];//联系电话
                }
                results = HtmlParser.GetResultFromParser(result, "//table[@class='table-form margin-top']/tbody/tr[4]/td", "text");
                if (results.Count > 0)
                {
                    mobile.Idtype = results[0];//证件类型
                }
                results = HtmlParser.GetResultFromParser(result, "//table[@class='table-form margin-top']/tbody/tr[5]/td", "text");
                if (results.Count > 0)
                {
                    mobile.Idcard = results[0];//证件号码
                }
                results = HtmlParser.GetResultFromParser(result, "//table[@class='table-form margin-top']/tbody/tr[6]/td", "text");
                if (results.Count > 0)
                {
                    mobile.Address = results[0].Trim();//地址
                }
                results = HtmlParser.GetResultFromParser(result, "//table[@class='table-form margin-top']/tbody/tr[7]/td", "text");
                if (results.Count > 0)
                {
                    mobile.Postcode = results[0].Trim();//邮政编码
                }
                results = HtmlParser.GetResultFromParser(result, "//table[@class='table-form margin-top']/tbody/tr[8]/td", "text");
                if (results.Count > 0)
                {
                    mobile.Email = results[0].Trim();//邮箱
                }
                results = HtmlParser.GetResultFromParser(result, "//table[@class='table-form margin-top']/tbody/tr[10]/td", "text");
                if (results.Count > 0)
                {
                    string birth = results[0].Trim();
                    if (mobile.Idtype.Contains("身份证") && !string.IsNullOrEmpty(mobile.Idcard) && birth.Length == 8)
                    {
                        mobile.Idcard = mobile.Idcard.Replace("********", birth);//身份证
                    }
                }
                results = HtmlParser.GetResultFromParser(result, "//table[@class='table-form margin-top']/tbody/tr[17]/td", "text");
                if (results.Count > 0)
                {
                    mobile.Regdate = DateTime.ParseExact(results[0].Trim(), "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None).ToString(Consts.DateFormatString11);//入网时间
                }
                // 用户名,手机套餐、品牌
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "monthBillInfor" + dateNowBill).FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//div[@class='bill-wrap']/table[2]/tr[1]/td/p", "text");
                if (results.Count > 0)
                {
                    mobile.Name = regex.Replace(results[0], "").Replace("客户名称：", "");//姓名
                }
                results = HtmlParser.GetResultFromParser(result, "//div[@class='bill-wrap']/table[6]/tr[1]/td/p", "text");
                if (results.Count > 0)
                {
                    string temp = results[0].Trim();//	天翼-乐享3G套餐-201311上网版389元  || 201407乐享4G 169元套餐
                    temp = temp.Replace(" ", "-");
                    if (temp.Contains("-"))
                    {
                        mobile.PackageBrand = temp.Substring(0, temp.LastIndexOf("-", StringComparison.Ordinal));// 套餐品牌
                        mobile.Package = temp.Substring(temp.LastIndexOf("-", StringComparison.Ordinal) + 1, temp.Length - temp.LastIndexOf("-", StringComparison.Ordinal) - 1);//当前手机套餐
                    }
                }
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "个人信息解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 账单查询

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis);
                for (int i = dateNowBill; i >= endTimeBill; i--)
                {
                    MonthBill monthBill = new MonthBill();
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "monthBillInfor" + i).FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//div[@class='bill-wrap']/table[2]/tr[2]/td/p", "text");
                    if (results.Count > 0)
                    {
                        string temp = regex.Replace(results[0], "").Replace("帐单周期：", "");//2015年09月01日至2015年09月30日
                        if (temp.Contains("至"))
                        {
                            monthBill.BillCycle = (Convert.ToDateTime(temp.Split('至')[0])).ToString(Consts.DateFormatString12);//计费周期
                        }
                    }
                    //判断表
                    List<string> tables = HtmlParser.GetResultFromParser(result, "//div[@class='bill-wrap']/table", "inner");
                    foreach (var table in tables)
                    {
                        //根据标题取表格数据
                        var title = HtmlParser.GetResultFromParser(table, "//tr[1]/td", "text")[0];
                        if (title.Contains("套餐及套餐叠加包月基本费"))
                        {
                            results = HtmlParser.GetResultFromParser(table, "//tr", "inner");
                            foreach (string item in results)
                            {
                                var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text");
                                if (tdRow.Count == 2)
                                {
                                    if (regex.Replace(tdRow[0], "") == "套餐月租费")
                                    {
                                        monthBill.PlanAmt = regex.Replace(tdRow[1], "");//套餐金额
                                    }
                                    if (regex.Replace(tdRow[0], "") == "本项小计：")
                                    {
                                        monthBill.TotalAmt = regex.Replace(tdRow[1], "");//总金额
                                    }
                                }
                                if (!string.IsNullOrEmpty(monthBill.TotalAmt))
                                {
                                    break;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(monthBill.TotalAmt))
                        {
                            break;
                        }
                    }
                    mobile.BillList.Add(monthBill);
                }
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region

                #region 通话详单

                DateTime dateNow = DateTime.Now;//当前时间
                int j = 0;
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Analysis);
                do
                {
                    string saveTitleTime = dateNow.AddMonths(-j).ToString("yyyyMM");
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == EnumMobileDeatilType.Call.ToString() + saveTitleTime + "page" + 1).FirstOrDefault().CrawlerTxt);
                    string tips = jsonParser.GetResultFromMultiNode(result, "tip");

                    if (!string.IsNullOrEmpty(tips))//查询日期内无详单记录
                    {
                        j++;
                        continue;
                    }
                    int pages = jsonParser.GetResultFromMultiNode(result, "resultBillDetail:totalPage").ToInt(0);
                    for (int i = 1; i <= pages; i++)
                    {
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == EnumMobileDeatilType.Call.ToString() + saveTitleTime + "page" + i).FirstOrDefault().CrawlerTxt);
                        tips = jsonParser.GetResultFromMultiNode(result, "tip");
                        if (tips.Contains("异常"))//查询详单异常
                        {
                            continue;
                        }
                        string jsonList = jsonParser.GetResultFromMultiNode(result, "resultBillDetail:items");
                        JArray jsonObjList = (JArray)JsonConvert.DeserializeObject(jsonList);
                        foreach (JToken item in jsonObjList)
                        {
                            var totalSecond = 0;
                            var usetime = item["durationsx"].ToString();
                            if (!string.IsNullOrEmpty(usetime))
                            {
                                totalSecond = CommonFun.ConvertDate(usetime);
                            }

                            Call call = new Call();
                            call.StartTime = DateTime.Parse(item["callDate"].ToString()).ToString(Consts.DateFormatString11);//起始时间  2015-10-14 17：53：12
                            call.InitType = item["callType"].ToString();//呼叫类型(主被叫)
                            call.CallType = item["teleType"].ToString();//通信类型（市话长途）
                            call.CallPlace = item["call_address"].ToString();//通信地点
                            call.OtherCallPhone = item["calledAccNbr"].ToString();//对方号码
                            call.UseTime = totalSecond.ToString();//通信时长
                            call.SubTotal = item["communicationFee"].ToString().ToDecimal(0);//通话费用
                            mobile.CallList.Add(call);
                        }
                    }
                    j++;
                } while (j <= 5);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 短信详单

                j = 0;
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Analysis);
                do
                {
                    string saveTitleTime = dateNow.AddMonths(-j).ToString("yyyyMM");
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == EnumMobileDeatilType.SMS.ToString() + saveTitleTime + "page" + 1).FirstOrDefault().CrawlerTxt);
                    string tips = jsonParser.GetResultFromMultiNode(result, "tip");
                    if (!string.IsNullOrEmpty(tips))//查询日期内无详单记录
                    {
                        j++;
                        continue;
                    }
                    int pages = jsonParser.GetResultFromMultiNode(result, "resultBillDetail:totalPage").ToInt(0);
                    for (int i = 1; i <= pages; i++)
                    {
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == EnumMobileDeatilType.SMS.ToString() + saveTitleTime + "page" + i).FirstOrDefault().CrawlerTxt);
                        tips = jsonParser.GetResultFromMultiNode(result, "tip");
                        if (tips.Contains("异常"))//查询详单异常
                        {
                            continue;
                        }
                        string jsonList = jsonParser.GetResultFromMultiNode(result, "resultBillDetail:items");
                        JArray jsonObjList = (JArray)JsonConvert.DeserializeObject(jsonList);
                        foreach (JToken item in jsonObjList)
                        {
                            Sms sms = new Sms();
                            sms.StartTime = DateTime.Parse(item["sendDate"].ToString().Replace("T", " ")).ToString(Consts.DateFormatString11);//起始时间  2015-10-14 17：53：12
                            //sms.SmsPlace = item["net_address"].ToString();//通信地点
                            sms.OtherSmsPhone = item["calledAccNbr"].ToString();//对方号码
                            sms.InitType = item["transceiverType"].ToString();//通信方式
                            sms.SmsType = item["businessType"].ToString();//信息类型 
                            sms.SubTotal = item["messageFee"].ToString().ToDecimal(0);//通信费 
                            mobile.SmsList.Add(sms);
                        }
                    }
                    j++;
                } while (j <= 5);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 上网详单

                //logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Analysis);
                //j = 0;
                //do
                //{
                //    string saveTitleTime = dateNow.AddMonths(-j).ToString("yyyyMM");
                //    //var a = crawler.DtlList.Where(x => x.CrawlerTitle.Contains("NGQryNetBill")).ToList<CrawlerDtlData>();
                //    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle ==EnumMobileDeatilType.Net.ToString() + saveTitleTime + "page" + 1).FirstOrDefault().CrawlerTxt);
                //    string tips = jsonParser.GetResultFromMultiNode(result, "tip");
                //    if (!string.IsNullOrEmpty(tips))//查询日期内无详单记录
                //    {
                //        j++;
                //        continue;
                //    }
                //    int pages = jsonParser.GetResultFromMultiNode(result, "resultBillDetail:totalPage").ToInt(0);
                //    for (int i = 1; i <= pages; i++)
                //    {
                //        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle ==EnumMobileDeatilType.Net.ToString() + saveTitleTime + "page" + i).FirstOrDefault().CrawlerTxt);
                //        tips = jsonParser.GetResultFromMultiNode(result, "tip");
                //        if (tips.Contains("异常"))//查询详单异常
                //        {
                //            continue;
                //        }
                //        string jsonList = jsonParser.GetResultFromMultiNode(result, "resultBillDetail:items");
                //        JArray jsonObjList = (JArray)JsonConvert.DeserializeObject(jsonList);
                //        foreach (JToken item in jsonObjList)
                //        {

                //            var totalSecond = 0;
                //            var usetime = item["durationsx"].ToString();
                //            if (!string.IsNullOrEmpty(usetime))
                //            {
                //                totalSecond = CommonFun.ConvertDate(usetime);
                //            }

                //            var totalFlow = CommonFun.ConvertGPRS(item["flowUni"].ToString());


                //            Net gprs = new Net();
                //            gprs.StartTime = DateTime.Parse(item["netDatesx"].ToString()).ToString(Consts.DateFormatString11);//起始时间  2015-10-14 17：53：12
                //            gprs.PhoneNetType = item["businessType"].ToString();//上网方式
                //            gprs.NetType = item["netType"].ToString();//网络类型
                //            gprs.Place = item["roamType"].ToString();//通信地点
                //            gprs.UseTime = totalSecond.ToString();//上网时长 
                //            gprs.SubFlow = totalFlow.ToString();//单次流量
                //            gprs.SubTotal = item["feeFlow"].ToString().ToTrim("（元）").ToDecimal(0);//单次费用
                //            mobile.NetList.Add(gprs);
                //        }
                //    }
                //    j++;
                //} while (j <= 5);
                ////NowMonth = dateNow.Month;//当前月份
                //logDtl.StatusCode = ServiceConsts.StatusCode_success;
                //logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                //logDtl.Description = "上网详单解析成功";
                //appLog.LogDtlList.Add(logDtl);
                #endregion

                #endregion
                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "山西电信手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "山西电信手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("山西电信手机账单解析异常", e);

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
        /// 抓取保存详单页面
        /// </summary>
        /// <param name="crawler"></param>
        /// <param name="detailsType">详单类型：2003语音详单，2004短信详单，2005无线宽带详单</param>
        /// <param name="saveTitle">自定义保存标签</param>
        private void GetHtmlDetails(EnumMobileDeatilType type, CrawlerData crawler)
        {
            string Url = string.Empty;
            Url = "http://sx.189.cn/service/bill/queryDetailBill.action";
            string detailsType = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                detailsType = "2003";
            else if (type == EnumMobileDeatilType.SMS)
                detailsType = "2004";
            else
                detailsType = "2005";

            for (int i = 0; i <= 5; i++)
            {
                int page = 1;//开始页
                int pages = 0;//查询月总页数
                DateTime searchDate = DateTime.Now.AddMonths(-i);//查询时间
                int days = DateTime.DaysInMonth(searchDate.Year, searchDate.Month);//当前月份总天数
                string beginTime = searchDate.ToString(Consts.DateFormatString12);//2015-11-01
                string endTime = searchDate.ToString("yyyy-MM-" + days);//2015-11-30
                string saveTitleTime = searchDate.ToString("yyyyMM");
                do
                {
                    string postdata = string.Format("billDetailValidate=true&billDetailType={0}&startTime={1}&endTime={2}&currentPhoneNum={3}&billPage={4}", detailsType, beginTime, endTime, crawler.Mobile, page);
                    httpItem = new HttpItem
                    {
                        Accept = "application/json, text/javascript, */*; q=0.01",
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Cookie = cookieStr,
                        Host = "sx.189.cn",
                        Referer = "http://sx.189.cn/service/bill/toDetailBill.action"
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                    //saveTitle + i + page :账单类型+时间（201506）+页码
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + saveTitleTime + "page" + page, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    string tips = jsonParser.GetResultFromMultiNode(httpResult.Html, "tip");
                    if (page == 1 && !tips.Contains("查询日期内无详单记录"))
                    {
                        //提示:返回数据概要信息相同，列表详细不同，这里页码信息格式相同，所以这么写
                        if (!tips.Contains("异常"))//查询详单异常
                        {
                            pages = jsonParser.GetResultFromMultiNode(httpResult.Html, "resultBillDetail:totalPage").ToInt(0);
                        }
                    }
                    page++;
                } while (page <= pages);
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
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
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
    }
}
