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
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.Mobile;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.NetSpider.Entity.Mongo.Log;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class NX : ChinaMobile
    {
        //#region 公共变量
        //IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        //IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        //IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        //CookieCollection cookies = new CookieCollection();
        //HttpHelper httpHelper = new HttpHelper();
        //HttpResult httpResult = null;
        //HttpItem httpItem = null;
        //MobileMongo mobileMongo = new MobileMongo();
        //ApplyLogMongo logMongo = new ApplyLogMongo();
        //string cookieStr = string.Empty;
        //string Url = string.Empty;
        //string postdata = string.Empty;
        //List<string> results = new List<string>();
        //#endregion

        ///// <summary>
        ///// 页面初始化
        ///// </summary>
        ///// <returns></returns>
        //public VerCodeRes MobileInit(MobileReq mobileReq = null)
        //{
        //    ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
        //    VerCodeRes Res = new VerCodeRes();
        //    string token = CommonFun.GetGuidID();
        //    Res.Token = token;
        //    try
        //    {
        //        Url = "https://nx.ac.10086.cn/login";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Accept="Accept	text/html, application/xhtml+xml, */*",
        //            Cookie = "CmProvid=nx;",
        //            Host = "nx.ac.10086.cn"
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew("", httpResult.Cookie);

        //        Url = "https://nx.ac.10086.cn/SSO/img?codeType=0";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            ResultType = ResultType.Byte,
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
        //        Res.VerCodeBase64 = Convert.ToBase64String(httpResult.ResultByte);
        //        //保存验证码图片在本地
        //        FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);

        //        Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.nextProCode = ServiceConsts.NextProCode_Login;
        //        Res.StatusDescription = "宁夏移动初始化完成";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        SpiderCacheHelper.SetCache(token, cookieStr);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "宁夏移动初始化异常";
        //        Log4netAdapter.WriteError("宁夏移动初始化异常", e);
        //        appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_Init)
        //        {
        //            CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
        //            StatusCode = ServiceConsts.StatusCode_error,
        //            Step = ServiceConsts.Step_Init,
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
        //                Step = ServiceConsts.Step_Init,
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
        //    try
        //    {
        //        //获取缓存
        //        if (SpiderCacheHelper.GetCache(mobileReq.Token) != null)
        //        {
        //            cookieStr = (string)SpiderCacheHelper.GetCache(mobileReq.Token);
        //        }
        //        //校验参数
        //        if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
        //        {
        //            Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        //登陆开始
        //        Url = "https://nx.ac.10086.cn/SSO/loginbox";
        //        postdata = string.Format("accountType=0&username={0}&passwordType=1&password={1}&smsRandomCode=&emailusername=%E8%AF%B7%E8%BE%93%E5%85%A5%E7%99%BB%E5%BD%95%E5%B8%90%E5%8F%B7&emailpassword=&validateCode={2}&action=%2FSSO%2Floginbox&style=mymobile&service=emall&continue=&submitMode=login&guestIP=210.22.124.10", mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Referer = "https://nx.ac.10086.cn/login",
        //            Host = "nx.ac.10086.cn",
        //            Method = "POST",
        //            Postdata = postdata,
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_httpfail;
        //            return Res;
        //        }
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='login_title']", "value");
        //        if (results.Count > 0)
        //        {
        //            Res.StatusDescription = results[0];
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        string SAMLart = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value")[0];
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

        //        Url = "http://www.nx.10086.cn/service/postLogin.action";
        //        postdata = string.Format("relayState=&SAMLart={0}&PasswordType=1", SAMLart);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Host = "www.nx.10086.cn",
        //            Method = "POST",
        //            Postdata = postdata,
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

        //        Url = "http://www.nx.10086.cn/service/index.action";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Host = "www.nx.10086.cn",
        //            Referer = "http://www.nx.10086.cn/service/postLogin.action",
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
        //        //登陆完成
        //        Res.StatusDescription = "登录成功";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.nextProCode = ServiceConsts.NextProCode_SendSMSAndVercode;
        //        SpiderCacheHelper.SetCache(mobileReq.Token, cookieStr);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "宁夏移动登录异常";
        //        Log4netAdapter.WriteError("宁夏移动登录异常", e);
        //        appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_Login)
        //        {
        //            CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
        //            StatusCode = ServiceConsts.StatusCode_error,
        //            Step = ServiceConsts.Step_Login,
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
        //                Step = ServiceConsts.Step_Login,
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
        //    VerCodeRes Res = new VerCodeRes();
        //    Res.Token = mobileReq.Token;
        //    string RelayState = string.Empty;
        //    string SAMLRequest = string.Empty;
        //    string SAMLart = string.Empty;
        //    string PasswordType = string.Empty;
        //    try
        //    {
        //        if (SpiderCacheHelper.GetCache(mobileReq.Token) != null)
        //            cookieStr = (string)SpiderCacheHelper.GetCache(mobileReq.Token);
        //        #region 跳转验证

        //        Url = "http://www.nx.10086.cn/my/queryBillDetail.action?menuid=billdetails";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Host = "www.nx.10086.cn",
        //            Referer = "http://www.nx.10086.cn/service/index.action",
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);


        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value");
        //        if (results.Count > 0)
        //        {
        //            RelayState = results[0];
        //        }

        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLRequest']", "value");
        //        if (results.Count > 0)
        //        {
        //            SAMLRequest = results[0];
        //        }
        //        Url = "https://nx.ac.10086.cn/SSO/check";
        //        postdata = string.Format("SAMLRequest={0}&RelayState={1}", SAMLRequest.ToUrlEncode(), RelayState.ToUrlEncode());
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Host = "nx.ac.10086.cn",
        //            Referer = "http://www.nx.10086.cn/my/queryBillDetail.action?menuid=billdetails",
        //            Method = "Post",
        //            Postdata = postdata,
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_httpfail;
        //            return Res;
        //        }
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='relayState']", "value");
        //        if (results.Count > 0)
        //        {
        //            RelayState = results[0];
        //        }

        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value");
        //        if (results.Count > 0)
        //        {
        //            SAMLart = results[0];
        //        }

        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='PasswordType']", "value");
        //        if (results.Count > 0)
        //        {
        //            PasswordType = results[0];
        //        }
        //        Url = "http://www.nx.10086.cn/my/notify.action";
        //        postdata = string.Format("relayState={0}&SAMLart={1}&PasswordType={2}", RelayState.ToUrlEncode(), SAMLart.ToUrlEncode(), PasswordType);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Host = "www.nx.10086.cn",
        //            Method = "Post",
        //            Postdata = postdata,
        //            Allowautoredirect = false,
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
        //        Url = httpResult.Header["Location"];
        //        if (string.IsNullOrEmpty(Url))
        //        {
        //            Res.StatusDescription = "获取个人资料失败";
        //            Res.StatusCode = ServiceConsts.StatusCode_httpfail;
        //            return Res;
        //        }
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Host = "www.nx.10086.cn",
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
        //        #endregion
        //        #region 生成验证图片,发送短信验证码
        //        //生成图片验证码
        //        Url = "http://www.nx.10086.cn/my/RandomCodeImage";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            ResultType = ResultType.Byte,
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
        //        Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
        //        FileOperateHelper.WriteVerCodeImage(mobileReq.Token, httpResult.ResultByte);
        //        //发送短信验证码
        //        Url = "http://www.nx.10086.cn/my/sendSms.action?menuid=billdetails";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            //Referer = "http://www.nx.10086.cn/my/checkSmsPass.action?menuid=billdetails",
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
        //        if (!httpResult.Html.Contains("已经发送，请注意查收"))
        //        {
        //            JObject objJson = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
        //            JArray objArray = (JArray)objJson["returnArr"][0];
        //            Res.StatusDescription = objArray[1].ToString();
        //            Res.StatusCode = ServiceConsts.StatusCode_httpfail;
        //            return Res;
        //        }
        //        #endregion
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.StatusDescription = "验证短信验证码，调用手机验证码验证接口";
        //        Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
        //        SpiderCacheHelper.SetCache(mobileReq.Token, cookieStr);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "宁夏移动手机验证码发送异常";
        //        Log4netAdapter.WriteError("宁夏移动手机验证码发送异常", e);
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
        //    try
        //    {
        //        //获取缓存
        //        if (SpiderCacheHelper.GetCache(mobileReq.Token) != null)
        //            cookieStr = (string)SpiderCacheHelper.GetCache(mobileReq.Token);

        //        Url = "http://www.nx.10086.cn/my/checkSmsPass_commit.action";
        //        postdata = string.Format("menuid=billdetails&fieldErrFlag=&contextPath=%2Fmy&randomSms={0}&confirmCode={1}",mobileReq.Smscode,mobileReq.Vercode);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "Post",
        //            Postdata = postdata,
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='busiContent']/ul/li", "text");
        //        if (results.Count>0)
        //        {
        //            Res.StatusDescription = results[0].Trim();
        //            Res.StatusCode = ServiceConsts.StatusCode_httpfail;
        //            return Res;
        //        }
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
     
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        Res.StatusDescription = "发送短信验证码，调用手机验证码发送接口";
        //        Res.nextProCode = ServiceConsts.NextProCode_Query;
        //        SpiderCacheHelper.SetCache(mobileReq.Token, cookieStr);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "宁夏移动手机验证码验证异常";
        //        Log4netAdapter.WriteError("宁夏移动手机验证码验证异常", e);
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
        ///// 保存抓取的账单
        ///// </summary>
        ///// <param name="mobileReq"></param>
        ///// <returns></returns>
        //public BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        //{
        //    ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
        //    ApplyLogDtl logDtl = new ApplyLogDtl("");
        //    CrawlerData crawler = new CrawlerData() { Token = mobileReq.Token, IdentityCard = mobileReq.IdentityCard, Name = mobileReq.Name, Mobile = mobileReq.Mobile, UserType = "1", CrawlerDate = DateTime.Now.ToString(Consts.DateFormatString11) };
        //    BaseRes Res = new BaseRes();
        //    Res.Token = mobileReq.Token;
        //    Basic mobile = new Basic();
        //    try
        //    {
        //        if (SpiderCacheHelper.GetCache(mobileReq.Token) != null)
        //        {
        //            cookieStr = (string)SpiderCacheHelper.GetCache(mobileReq.Token);
        //        }
        //        #region 基本信息

        //        Url = "http://www.nx.10086.cn/my/qryUserInfo_init.action?menuid=qryUserInfo";
        //        httpItem = new HttpItem
        //        {
        //            URL = Url,
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='message']/span", "text");
        //        if (results.Count > 0)
        //        {
        //            mobile.Mobile = results[0];//手机号
        //        }
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='message']/a", "title");
        //        if (results.Count > 0)
        //        {
        //            mobile.StarLevel = results[0];//星级
        //        }
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='personInfo_tableBorder']/table/tr[8]/td[2]", "text");
        //        if (results.Count > 0)
        //        {
        //            mobile.Regdate = results[0].Trim();//入网时间
        //        }
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='personInfo_tableBorder']/table/tr[12]/td[2]", "text");
        //        if (results.Count > 0)
        //        {
        //            mobile.Integral = results[0].Trim();//积分
        //        }
        //        Url = "http://www.nx.10086.cn/my/updateAccount_init.action?menuid=modifyAccount";
        //        httpItem = new HttpItem
        //        {
        //            URL = Url,
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='personInfo_tableBorder']/table/tr[1]/tr[1]/tr[1]/td[2]", "text");
        //        if (results.Count > 0)
        //        {
        //            mobile.Name = results[0].Trim();//姓名
        //        }
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='personInfo_tableBorder']/table/tr[1]/tr[1]/tr[2]/td[2]", "text");
        //        if (results.Count > 0)
        //        {
        //            mobile.Idtype = results[0].Trim();//证件类型
        //        }
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='personInfo_tableBorder']/table/tr[1]/tr[1]/tr[2]/td[4]", "text");
        //        if (results.Count > 0)
        //        {
        //            mobile.Idcard = results[0].Trim();//证件号
        //        }
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='personInfo_tableBorder']/table/tr[1]/tr[1]/tr[3]/td[2]", "text");
        //        if (results.Count > 0)
        //        {
        //            mobile.Address = results[0].Trim();//地址
        //        }
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='postCode']", "value");
        //        if (results.Count > 0)
        //        {
        //            mobile.Postcode = results[0].Trim();//邮编
        //        }
        //        //puk
        //        Url = " http://www.nx.10086.cn/my/qryPukCode_result.action?menuid=queryPukCode";
        //        httpItem = new HttpItem
        //        {
        //            URL = Url,
        //            Cookie = cookieStr
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
        //        JObject objJson = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
        //        JArray objArray = (JArray)objJson["returnArr"];
        //        objArray = (JArray)objArray[0];
        //        mobile.PUK = objArray[1].ToString().ToTrim("PUK1：");
        //        #endregion
        //        #region 月账单

        //        DateTime dtNow = DateTime.Now;
        //        for (int i = 0; i < 6; i++)
        //        {
        //            MonthBill bill = new MonthBill();
        //            DateTime dtSearchTime = dtNow.AddMonths(-i);
        //            bill.BillCycle = dtSearchTime.ToString(Consts.DateFormatString12);
        //            int daysOfMonth = DateTime.DaysInMonth(dtSearchTime.Year, dtSearchTime.Month); ;
        //            Url = "http://www.nx.10086.cn/my/queryBill_billInfo.action";
        //            postdata = string.Format("menuid=queryBill&fieldErrFlag=&contextPath=%2Fmy&feeType=&customInfo.custName={0}&customInfo.brandName=&customInfo.prodName=&customInfo.subsId=&cycleMap.cycle_{1}={2}&cycleMap.startDate_{1}={1}&cycleMap.endDate_{1}={3}&cycleMap.acctId_{1}=&cycleMap.unionacct_{1}=0&cycleStartDate={1}&retMonth={2}&month={2}", mobile.Name.ToUrlEncode(), dtSearchTime.ToString("yyyyMM01"), dtSearchTime.ToString("yyyyMM"), dtSearchTime.ToString("yyyyMM") + daysOfMonth);
        //            httpItem = new HttpItem
        //            {
        //                URL = Url,
        //                Method = "Post",
        //                Postdata = postdata,
        //                Cookie = cookieStr
        //            };
        //            httpResult = httpHelper.GetHtml(httpItem);
        //            cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

        //            results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='queryAfter']/table[1]/tr[2]/td[1]/div[1]/table/tr[position()>2]", "inner");
        //            decimal totalfee = 0;
        //            foreach (var item in results)
        //            {
        //                var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text");
        //                if (tdRow.Count < 2)
        //                {
        //                    continue;
        //                }
        //                switch (@tdRow[0].Trim().Replace("&nbsp;", ""))
        //                {
        //                    case "套餐及固定费":
        //                        bill.PlanAmt = tdRow[1].Trim().Replace("&nbsp;", "");
        //                        totalfee += bill.PlanAmt.ToDecimal(0);
        //                        break;
        //                    case "优惠减免":
        //                        totalfee -= tdRow[1].Trim().Replace("&nbsp;", "").ToDecimal(0);
        //                        break;
        //                    case "个人代付":
        //                        break;
        //                    case "单位代付":
        //                        break;
        //                    default:
        //                        totalfee += tdRow[1].Trim().Replace("&nbsp;", "").ToDecimal(0);
        //                        break;
        //                }
        //            }
        //            bill.TotalAmt = totalfee.ToString(CultureInfo.InvariantCulture);
        //            mobile.BillList.Add(bill);
        //            if (i == 0)
        //            {
        //                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='queryAfter']/table[1]/tr[1]/td[1]/div[1]/table/tr[4]/td[2]", "inner");
        //                if (results.Count > 0)
        //                {
        //                    mobile.PackageBrand = results[0].Trim().Replace("&nbsp;", "");//套餐品牌
        //                }
        //                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='feeDetailListOne']/tr[3]/td", "text");
        //                if (results.Count > 0)
        //                {
        //                    mobile.Package = results[0].Trim().Replace("&nbsp;", "");//当前手机套餐
        //                }
        //            }
        //        }
        //        #endregion
        //        #region 通话详单
        //        logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Crawler);

        //        //GetHtmlDetails(crawler, 1001, "NGQryCallBill");

        //        logDtl.StatusCode = ServiceConsts.StatusCode_success;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = "通话详单抓取成功";
        //        appLog.LogDtlList.Add(logDtl);
        //        #endregion
        //        #region 短信详单

        //        logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Crawler);

        //        //GetHtmlDetails(crawler, 1002, "NGQrySMSBill");

        //        logDtl.StatusCode = ServiceConsts.StatusCode_success;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = "短信详单抓取成功";
        //        appLog.LogDtlList.Add(logDtl);

        //        #endregion
        //        #region 流量详单

        //        logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Crawler);

        //        //GetHtmlDetails(crawler, 1003, "NGQryNetBill");

        //        logDtl.StatusCode = ServiceConsts.StatusCode_success;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = "上网详单抓取成功";
        //        appLog.LogDtlList.Add(logDtl);
        //        #endregion
        //        crawlerMobileMongo.SaveCrawler(crawler);
        //        Res.StatusDescription = "宁夏移动手机账单抓取成功";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //        CacheHelper.SetCache(mobileReq.Token, cookieStr);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusDescription = "宁夏移动手机账单抓取异常";
        //        Res.StatusCode = ServiceConsts.StatusCode_fail;
        //        Log4netAdapter.WriteError("宁夏移动手机账单抓取异常", e);
        //        logDtl.Step = ServiceConsts.NextProCode_Query;
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
        //            logDtl.Step = ServiceConsts.NextProCode_Query;
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
        ///// 解析抓取的原始数据
        ///// </summary>
        ///// <param name="mobileReq"></param>
        ///// <param name="crawlerDate"></param>
        ///// <returns></returns>
        //public BaseRes MobileAnalysis(MobileReq mobileReq, DateTime appDate)
        //{
        //    ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
        //    ApplyLogDtl logDtl = new ApplyLogDtl("");
        //    CrawlerData crawler = new CrawlerData();
        //    BaseRes Res = new BaseRes();
        //    Res.Token = mobileReq.Token;
        //    Regex regex = new Regex(@"[\&nbsp;\s]*");
        //    DateTime dtNow = DateTime.Now;
        //    try
        //    {
        //        if (SpiderCacheHelper.GetCache(mobileReq.Token) != null)
        //        {
        //            cookieStr = (string)SpiderCacheHelper.GetCache(mobileReq.Token);
        //        }
        //        crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);
        //        #region 基本信息查询

        //        logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Analysis);
        //        Basic mobile = new Basic();
        //        mobile.Token = mobileReq.Token;
        //        mobile.BusName = mobileReq.Name;
        //        mobile.BusIdentityCard = mobileReq.IdentityCard;
        //        mobile.Mobile = mobileReq.Mobile;
        //        //result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInforPart1").FirstOrDefault().CrawlerTxt);

        //        logDtl.StatusCode = ServiceConsts.StatusCode_success;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = "个人信息解析成功";
        //        appLog.LogDtlList.Add(logDtl);
        //        #endregion
        //        #region 账单查询

        //        logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis);


        //        logDtl.StatusCode = ServiceConsts.StatusCode_success;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = "账单解析成功";
        //        appLog.LogDtlList.Add(logDtl);
        //        #endregion
        //        #region 通话详单

        //        logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Analysis);

        //        logDtl.StatusCode = ServiceConsts.StatusCode_success;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = "通话详单解析成功";
        //        appLog.LogDtlList.Add(logDtl);
        //        #endregion
        //        #region 短信详单
        //        logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Analysis);

        //        logDtl.StatusCode = ServiceConsts.StatusCode_success;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = "短信详单解析成功";
        //        appLog.LogDtlList.Add(logDtl);
        //        #endregion
        //        #region 上网详单

        //        logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Analysis);

        //        logDtl.StatusCode = ServiceConsts.StatusCode_success;
        //        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
        //        logDtl.Description = "上网详单解析成功";
        //        appLog.LogDtlList.Add(logDtl);
        //        #endregion

        //        mobileMongo.SaveBasic(mobile);
        //        Res.StatusDescription = "宁夏移动手机账单解析成功";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;
        //    }
        //    catch (Exception e)
        //    {

        //        Res.StatusDescription = "宁夏移动手机账单解析异常";
        //        Res.StatusCode = ServiceConsts.StatusCode_fail;
        //        Log4netAdapter.WriteError("宁夏移动手机账单解析异常", e);

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
    }
}
