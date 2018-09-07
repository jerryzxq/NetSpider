using System;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
using Vcredit.NetSpider.Entity.Mongo.Log;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class TJ : IMobileCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        LogHelper httpHelper = new LogHelper();
        ApplyLogMongo logMongo = new ApplyLogMongo();
        List<ApplyLog> loglist = new List<ApplyLog>();
        ApplyLog appLog = new ApplyLog();
        ApplyLogDtl logDtl = new ApplyLogDtl();
        private string cookieStr = string.Empty;
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
            try
            {
                logDtl = new ApplyLogDtl("初始化登录页面");
                Url = "https://tj.ac.10086.cn/login/";
                httpItem = new HttpItem()
                {
                    URL = Url
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "初始化登录页面");
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "初始化登录页面成功";
                appLog.LogDtlList.Add(logDtl);
                //第二步，获取验证码
                logDtl = new ApplyLogDtl("获取验证码");
                Url = "https://tj.ac.10086.cn/CheckCodeImage?";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "获取验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "获取验证码成功";
                appLog.LogDtlList.Add(logDtl);

                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;
                Res.StatusDescription = "天津移动初始化完成";
                CacheHelper.SetCache(Res.Token, cookieStr);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "天津移动初始化异常";
                Log4netAdapter.WriteError("天津移动初始化异常", e);

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
                    cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
                }
                logDtl = new ApplyLogDtl("登录");
                Url = "https://tj.ac.10086.cn/login/loginHandlerV2.jsp";
                postdata = string.Format("issuer=http%3A%2F%2Fservice.tj.10086.cn&RelayState=MyHome&type1=index&loginType=phoneNo&mp={0}&passwordType=service&password={1}&checkCode={2}&smsPwd=", mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "https://tj.ac.10086.cn/login/",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='loginCont container pos_r']");
                if (results.Count > 0)
                {
                    results = HtmlParser.GetResultFromParser(results[0], "div/font");
                    Res.StatusDescription = results[0].ToTrim("[").ToTrim("]");
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Url = CommonFun.GetMidStr(httpResult.Html, "parent.location.href='", "';");//302
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "service.tj.10086.cn",
                    Allowautoredirect = false,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                Url = httpResult.Header["Location"];//302
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "service.tj.10086.cn",
                    Allowautoredirect = false,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                Url = httpResult.Header["Location"];//200 http://service.tj.10086.cn/app?service=page/MyHome&listener=initPage
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "service.tj.10086.cn",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_Vercode;

                CacheHelper.SetCache(mobileReq.Token, cookieStr);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = Res.StatusCode;
                logDtl.Description = Res.StatusDescription;
                appLog.LogDtlList.Add(logDtl);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "天津移动登录异常";
                Log4netAdapter.WriteError("天津移动登录异常", e);

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
            string Url = string.Empty;
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);

                if (string.IsNullOrEmpty((string)CacheHelper.GetCache(mobileReq.Token + "Flage")))
                {
                    CacheHelper.SetCache(mobileReq.Token + "Flage", "0");
                    logDtl = new ApplyLogDtl("生成短信验证码图片");
                    Url = "http://service.tj.10086.cn/app?service=page/feequery.VoiceQueryNew&listener=initPage";
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Host = "service.tj.10086.cn",
                        Referer = "http://service.tj.10086.cn/app?service=page/MyHome&listener=initPage",
                        Cookie = cookieStr
                    };
                    httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "生成短信验证码图片校验");
                    cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                    //生成短信验证码图片
                    Url = "http://service.tj.10086.cn/image?mode=validate&width=60&height=25";
                    httpItem = new HttpItem
                    {
                        Accept = "image/png,image/*;q=0.8,*/*;q=0.5",
                        URL = Url,
                        Host = "service.tj.10086.cn",
                        Referer = "http://service.tj.10086.cn/app?service=page/feequery.VoiceQueryNew&listener=initPage",
                        Cookie = cookieStr,
                        ResultType = ResultType.Byte
                    };
                    httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "生成短信验证码图片");
                    cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                    Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                    //保存验证码图片在本地
                    Res.StatusDescription = "图片验证码生成成功，调用手机验证码验证接口";
                    Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                }
                else
                {
                    //发送短信验证码
                    logDtl = new ApplyLogDtl("发送短信验证码");
                    Url = string.Format("http://service.tj.10086.cn/app?service=ajaxDirect/1/personalinfo.CheckedSms/personalinfo.CheckedSms/javascript/undefined&pagename=personalinfo.CheckedSms&eventname=sendSms&VERIFY_CODE=undefined&EFFICACY_CODE1={0}&partids=undefined&ajaxSubmitType=get&ajax_randomcode=0.6382617928993175", mobileReq.Vercode);
                    httpItem = new HttpItem
                    {
                        Accept = "application/xml, text/xml, */*; q=0.01",
                        UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:42.0) Gecko/20100101 Firefox/42.0",
                        URL = Url,
                        Encoding = Encoding.GetEncoding("GBK"),
                        Host = "service.tj.10086.cn",
                        Referer = "http://service.tj.10086.cn/app?service=page/feequery.VoiceQueryNew&listener=initPage",
                        //Allowautoredirect = false,
                        Cookie = cookieStr,
                        ResultCookieType = ResultCookieType.String
                    };
                    httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发送短信验证码");
                    cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                    Res.StatusDescription = "校验短信验证码，调用手机验证码验证接口";
                    Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                }
                CacheHelper.SetCache(mobileReq.Token, cookieStr);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = Res.StatusCode;
                logDtl.Description = Res.StatusDescription;
                appLog.LogDtlList.Add(logDtl);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "天津移动手机验证码发送异常";
                Log4netAdapter.WriteError("天津移动手机验证码发送异常", e);

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
            string Url = string.Empty;

            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
                if ((string)CacheHelper.GetCache(mobileReq.Token + "Flage") == "0")
                {
                    //验证图片验证码
                    logDtl = new ApplyLogDtl("验证图片验证码");
                    Url = string.Format(
                            "http://service.tj.10086.cn/app?service=ajaxDirect/1/npcollection.IdentifyingCode/npcollection.IdentifyingCode/javascript/SER_VERIFY_CODE&pagename=npcollection.IdentifyingCode&eventname=getVerifyCode&&verify_code_front={0}&partids=SER_VERIFY_CODE&ajaxSubmitType=get&ajax_randomcode=0.5010778741204093",
                            mobileReq.Vercode);
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Host = "service.tj.10086.cn",
                        Referer = "http://service.tj.10086.cn/app?service=page/feequery.VoiceQueryNew&listener=initPage",
                        Cookie = cookieStr
                    };
                    httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "验证图片验证码");
                    cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                    if (!httpResult.Html.Contains("{\"results\":\"true\"}"))
                    {
                        Res.StatusDescription = "请输入正确的图片验证码！";
                        Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.StatusCode = Res.StatusCode;
                        logDtl.Description = Res.StatusDescription;
                        appLog.LogDtlList.Add(logDtl);

                        return Res;
                    }
                    Res.StatusDescription = "发送短信验证码，调用手机验证码发送接口";
                    Res.nextProCode = ServiceConsts.NextProCode_SendSMS;
                    CacheHelper.SetCache(mobileReq.Token + "Flage", "1");
                }
                else
                {
                    //校验短信验证码
                    logDtl = new ApplyLogDtl("校验短信验证码");
                    Url = "http://service.tj.10086.cn/app";
                    string postdata = string.Format("mpageName=feequery.VoiceQueryNew&service=direct%2F1%2Fpersonalinfo.CheckedSms%2FSmsForm&sp=S1&Form1=EFFICACY_CODE1%2Cbforgot&EFFICACY_CODE1={0}&SMS_NUMBER={1}&bforgot=%C8%B7++%C8%CF", mobileReq.Vercode, mobileReq.Smscode);
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Host = "service.tj.10086.cn",
                        Method = "Post",
                        Referer = "http://service.tj.10086.cn/app?service=page/feequery.VoiceQueryNew&listener=initPage",
                        Postdata = postdata,
                        Cookie = cookieStr
                    };
                    httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验短信验证码");
                    cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                    List<string> results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='alert']", "text");
                    if (results.Count > 0)
                    {
                        Res.StatusDescription = System.Web.HttpUtility.HtmlDecode(results[0]).Trim();
                        Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.StatusCode = Res.StatusCode;
                        logDtl.Description = Res.StatusDescription;
                        appLog.LogDtlList.Add(logDtl);

                        return Res;
                    }
                    Res.StatusDescription = "天津移动手机短信验证码验证成功，调用查询账单接口";
                    Res.nextProCode = ServiceConsts.NextProCode_Query;
                }
                Res.StatusCode = ServiceConsts.StatusCode_success;
                CacheHelper.SetCache(mobileReq.Token, cookieStr);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = Res.StatusCode;
                logDtl.Description = Res.StatusDescription;
                appLog.LogDtlList.Add(logDtl);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "天津移动手机验证码验证异常";
                Log4netAdapter.WriteError("天津移动手机验证码验证异常", e);

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
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
                }

                #region 基本信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Crawler, mobileReq.Website);

                #region 基本信息
                logDtl = new ApplyLogDtl("基本信息");
                Url = "http://service.tj.10086.cn/app?service=page/MyHome&listener=initPage";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInforPart1", CrawlerTxt = Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "基本信息抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                #endregion

                #region 入网时间
                logDtl = new ApplyLogDtl("入网时间");
                Url = "http://service.tj.10086.cn/app?service=page/personalinfo.CustInfoQuery&listener=initPage";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "service.tj.10086.cn",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                Url = "http://service.tj.10086.cn/app?service=ajaxDirect/1/personalinfo.CustInfoQuery/personalinfo.CustInfoQuery/javascript/custInfoPart&pagename=personalinfo.CustInfoQuery&eventname=queryCustInfo&partids=custInfoPart&ajaxSubmitType=get&ajax_randomcode=0.784106246961732";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "service.tj.10086.cn",
                    Referer = "http://service.tj.10086.cn/app?service=page/personalinfo.CustInfoQuery&listener=initPage",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "入网时间");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInforPart2", CrawlerTxt = Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "入网时间抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                #endregion

                #region 通信地址 姓名
                logDtl = new ApplyLogDtl("通信地址姓名");
                Url = "http://service.tj.10086.cn/app?service=page/personalinfo.UserCustAcctInfoQuery&listener=initPage";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "service.tj.10086.cn",
                    Referer = "http://service.tj.10086.cn/app?service=page/personalinfo.CustInfoQuery&listener=initPage",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "通信地址姓名");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInforPart3", CrawlerTxt = Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "通信地址姓名抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                #endregion

                #region puk
                logDtl = new ApplyLogDtl("puk");
                Url = "http://service.tj.10086.cn/app?service=page/svcquery.PinQuery&listener=initPage";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "service.tj.10086.cn",
                    Referer = "http://service.tj.10086.cn/app?service=page/personalinfo.UserCustAcctInfoQuery&listener=initPage",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "puk");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "PukInfor", CrawlerTxt = Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "puk抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                #endregion

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 每月账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Crawler, mobileReq.Website);

                CrawlerBill(crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 通话详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Crawler, mobileReq.Website);

                CrawlerDeatils(crawler, EnumMobileDeatilType.Call);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Crawler, mobileReq.Website);

                CrawlerDeatils(crawler, EnumMobileDeatilType.SMS);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Crawler, mobileReq.Website);

                CrawlerDeatils(crawler, EnumMobileDeatilType.Net);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "天津移动手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                CacheHelper.SetCache(mobileReq.Token, cookieStr);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "天津移动手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("天津移动手机账单抓取异常", e);

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
            List<string> results = new List<string>();
            string result = string.Empty;
            Regex regex = new Regex(@"[\&nbsp;\s]*");

            try
            {
                crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);

                #region 基本信息查询
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
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInforPart1").FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//p[@class='wdydTitel']");
                    mobile.Name = System.Web.HttpUtility.HtmlDecode(CommonFun.GetMidStr(results[0], "</span>", "先生").ToTrim("，"));
                    results = HtmlParser.GetResultFromParser(result, "//ul[@class='basicInformation']/li");
                    foreach (string item in results)
                    {
                        string name = HtmlParser.GetResultFromParser(item, "//div")[0];
                        switch (@name)
                        {
                            case "我的资费套餐":
                                mobile.Package = System.Web.HttpUtility.HtmlDecode(HtmlParser.GetResultFromParser(item, "//a")[0]);
                                break;
                            case "我的星级":
                                string StarLevel = HtmlParser.GetResultFromParser(item, "//input", "value")[0];
                                switch (@StarLevel)
                                {
                                    case "1":
                                        mobile.StarLevel = "一星";
                                        break;
                                    case "2":
                                        mobile.StarLevel = "二星";
                                        break;
                                    case "3":
                                        mobile.StarLevel = "三星";
                                        break;
                                    case "4":
                                        mobile.StarLevel = "四星";
                                        break;
                                    case "5":
                                        mobile.StarLevel = "五星";
                                        break;
                                    default:
                                        mobile.StarLevel = "无";
                                        break;
                                }
                                break;
                            case "我的积分":
                                mobile.Integral = System.Web.HttpUtility.HtmlDecode(HtmlParser.GetResultFromParser(item, "//a")[0]);
                                break;
                        }
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

                #region 入网时间
                logDtl = new ApplyLogDtl("入网时间");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInforPart2").FirstOrDefault().CrawlerTxt);
                    result = System.Web.HttpUtility.HtmlDecode(result).ToTrim("<![CDATA[").ToTrim("]]>");
                    results = HtmlParser.GetResultFromParser(result, "//part[@id='custInfoPart']/div/ul/li[3]/dl/dd", "text");
                    if (results.Count > 0)
                    {
                        mobile.Regdate = DateTime.Parse(results[0].Trim()).ToString(Consts.DateFormatString11);
                    }
                    results = HtmlParser.GetResultFromParser(result, "//part[@id='custInfoPart']/div/ul/li[5]/dl/dd", "text");
                    if (results.Count > 0)
                    {
                        mobile.PackageBrand = regex.Replace(results[0].Trim(), "");
                    }
                    results = HtmlParser.GetResultFromParser(result, "//part[@id='custInfoPart']/div/ul/li[8]/dl/dd", "text");
                    if (results.Count > 0)
                    {
                        mobile.Email = regex.Replace(results[0].Trim(), "");
                    }

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "入网时间解析成功";
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

                #region 通信地址 姓名
                logDtl = new ApplyLogDtl("通信地址姓名");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInforPart3").FirstOrDefault().CrawlerTxt);
                    result = System.Web.HttpUtility.HtmlDecode(result);
                    results = HtmlParser.GetResultFromParser(result, "//div[@class='m15']/ul/li/dl/dd", "text");
                    if (results.Count > 6)
                    {
                        mobile.Name = results[1];
                        mobile.Address = results[6];
                    }

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "通信地址姓名解析成功";
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

                #region puk
                logDtl = new ApplyLogDtl("puk");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "PukInfor").FirstOrDefault().CrawlerTxt);
                    result = System.Web.HttpUtility.HtmlDecode(result);
                    results = HtmlParser.GetResultFromParser(result, "//div[@class='m15']/ul/li[3]/dl/dd", "text");
                    if (results.Count > 0)
                    {
                        mobile.PUK = results[0];
                    }

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "puk解析成功";
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

                #region 手机账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Analysis, mobileReq.Website);

                AnalysisBill(crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 通话详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.Call, crawler, delegate(List<string> itemList)
                {
                    foreach (var items in itemList)
                    {
                        List<string> tdRow = HtmlParser.GetResultFromParser(items, "//td", "text");
                        if (tdRow.Count < 8)
                        {
                            continue;
                        }
                        var totalSecond = 0;
                        var usetime = tdRow[4].ToString();
                        if (!string.IsNullOrEmpty(usetime))
                        {
                            totalSecond = CommonFun.ConvertDate(usetime);
                        }
                        Call call = new Call();
                        call.StartTime = DateTime.Parse(tdRow[0]).ToString(Consts.DateFormatString11);
                        call.CallPlace = tdRow[1];
                        call.InitType = tdRow[2];
                        call.OtherCallPhone = tdRow[3];
                        call.UseTime = totalSecond.ToString();
                        call.CallType = tdRow[5];
                        call.SubTotal = tdRow[7].ToDecimal(0);
                        mobile.CallList.Add(call);
                    }
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.SMS, crawler, delegate(List<string> itemList)
                {
                    foreach (var items in results)
                    {
                        List<string> tdRow = HtmlParser.GetResultFromParser(items, "//td", "text");
                        if (tdRow.Count < 8)
                        {
                            continue;
                        }
                        Sms sms = new Sms();
                        sms.StartTime = DateTime.Parse(tdRow[0]).ToString(Consts.DateFormatString11);
                        sms.SmsPlace = tdRow[1];
                        sms.OtherSmsPhone = tdRow[2];
                        sms.InitType = tdRow[3];
                        sms.SmsType = tdRow[4];
                        sms.SubTotal = tdRow[7].ToDecimal(0);
                        mobile.SmsList.Add(sms);
                    }
                });
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.Net, crawler, delegate(List<string> itemList)
                {
                    foreach (var items in results)
                    {
                        List<string> tdRow = HtmlParser.GetResultFromParser(items, "//td", "text");
                        if (tdRow.Count < 7)
                        {
                            continue;
                        }

                        var totalSecond = 0;
                        var usetime = tdRow[3].ToString();
                        if (!string.IsNullOrEmpty(usetime))
                        {
                            totalSecond = CommonFun.ConvertDate(usetime);
                        }
                        Net gprs = new Net();
                        gprs.StartTime = DateTime.Parse(tdRow[0]).ToString(Consts.DateFormatString11);
                        gprs.Place = tdRow[1];
                        gprs.PhoneNetType = tdRow[2];
                        gprs.UseTime = totalSecond.ToString();
                        gprs.SubFlow = CommonFun.ConvertGPRS(tdRow[4]).ToString();
                        gprs.SubTotal = tdRow[6].ToDecimal(0);
                        mobile.NetList.Add(gprs);
                    }
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "天津移动手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "天津移动手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("天津移动手机账单解析异常", e);

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
        private void CrawlerBill(CrawlerData crawler)
        {
            string Url = string.Empty;
            string postdata = string.Empty;

            logDtl = new ApplyLogDtl("月账单抓取校验");
            Url = "http://service.tj.10086.cn/app?service=page/feequery.AcctFeeInfo&listener=initPage";
            httpItem = new HttpItem()
            {
                URL = Url,
                Cookie = cookieStr
            };
            httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "月账单抓取校验");
            if (httpResult.StatusCode == HttpStatusCode.OK)
            {
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                for (int i = 0; i < 6; i++)
                {
                    string title = DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString7) + "月账单抓取";
                    logDtl = new ApplyLogDtl(title);
                    string YearMonth = DateTime.Now.AddMonths(-i).ToString("yyyyMM");
                    Url = "http://service.tj.10086.cn/app";
                    postdata = string.Format("service=direct%2F1%2Ffeequery.AcctFeeInfo%2F%24Form&sp=S1&Form1=%24RadioGroup%2Csubmit&MONTH={0}&submit=%B2%E9++%D1%AF", i + 1);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Referer = "http://service.tj.10086.cn/app?service=page/feequery.AcctFeeInfo&listener=initPage",
                        Cookie = cookieStr
                    };
                    httpResult = httpHelper.GetHtmlLog(httpItem, appLog, title);
                    if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                    cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                    try
                    {
                        if (HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='tt']/table/tr/td/table").Count < 1)
                        {
                            logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = title + "失败";
                            appLog.LogDtlList.Add(logDtl);
                            continue;
                        }
                        crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "billMonthInfor" + YearMonth, CrawlerTxt = Encoding.Default.GetBytes(httpResult.Html) });
                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = title + "成功";
                        appLog.LogDtlList.Add(logDtl);
                    }
                    catch (Exception e)
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_error;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = title + "异常：" + e.Message;
                        appLog.LogDtlList.Add(logDtl);
                        continue;
                    }
                }
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
            List<string> results = new List<string>();
            DateTime date;
            for (int i = 0; i < 6; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月账单解析");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == "billMonthInfor" + date.ToString("yyyyMM")).FirstOrDefault() == null) continue;
                    PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "billMonthInfor" + date.ToString("yyyyMM")).FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(PhoneBillStr, "//div[@id='tt']/table/tr/td/table");
                    if (results.Count > 1)
                    {
                        MonthBill bill = new MonthBill();
                        decimal total = 0;
                        results = HtmlParser.GetResultFromParser(results[1], "tr");
                        foreach (string item in results)
                        {
                            List<string> info = HtmlParser.GetResultFromParser(item, "//strong");
                            if (info.Count == 0)
                                continue;
                            decimal planamt = 0;
                            if (info[1].Contains("span"))
                                planamt = HtmlParser.GetResultFromParser(info[1], "span")[0].ToDecimal(0);
                            else
                                planamt = info[1].ToDecimal(0);
                            if (bill.PlanAmt.IsEmpty())
                            {
                                if (info[0] == "套餐及固定费用")
                                    bill.PlanAmt = planamt.ToString();
                            }
                            if (info[0] == "套餐及固定费用" || info[0] == "套餐外费用" || info[0] == "代收费业务费用" || info[0] == "其他")
                                total += planamt;
                        }
                        bill.TotalAmt = total.ToString();
                        bill.BillCycle = date.ToString(Consts.DateFormatString12);
                        mobile.BillList.Add(bill);
                    }
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
        /// 抓取保存详单页面
        /// </summary>
        /// <param name="crawler"></param>
        /// <param name="detailsType">详单类型(BILL_TYPE):1000套餐及固定费,1001通话详单,1002短/彩信详单,1003上网详单</param>
        /// <param name="saveTitle">自定义保存标签</param>
        private void CrawlerDeatils(CrawlerData crawler, EnumMobileDeatilType type)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            string title = string.Empty;
            DateTime dtNow = DateTime.Now;
            Url = "http://service.tj.10086.cn/app";
            string detailsType = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                detailsType = "1001";
            else if (type == EnumMobileDeatilType.SMS)
                detailsType = "1002";
            else
                detailsType = "1003";

            for (int i = 0; i < 6; i++)
            {
                title = dtNow.AddMonths(-i).ToString(Consts.DateFormatString7) + "月详单抓取";
                logDtl = new ApplyLogDtl(title);
                string searchTime = dtNow.AddMonths(-i).ToString("yyyyMM");
                postdata = string.Format("service=direct%2F1%2Ffeequery.VoiceQueryNew%2F%24Form&sp=S1&Form1=bquery&MONTH={0}&BILL_TYPE={1}&CALL_TYPE=+&ROAM_TYPE=+&LONG_TYPE=+&bquery=%B2%E9%D1%AF", searchTime, detailsType);//bquery:查询
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "service.tj.10086.cn",
                    Method = "post",
                    Postdata = postdata,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, title);
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                try
                {
                    if (HtmlParser.GetResultFromParser(System.Web.HttpUtility.HtmlDecode(httpResult.Html), "//table[@id='table']/tbody/tr", "inner").Count < 1)
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = title + "失败";
                        appLog.LogDtlList.Add(logDtl);
                        continue;
                    }
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + searchTime, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = title + "成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = title + "异常：" + e.Message + "原始数据：" + httpResult.Html;
                    appLog.LogDtlList.Add(logDtl);

                    continue;
                }
            }
        }

        /// <summary>
        /// 解析手机详单
        /// </summary>
        /// <param name="queryType">3:通话；4:上网；5:短信</param>
        /// <returns></returns>
        private void AnalysisDeatils(EnumMobileDeatilType type, CrawlerData crawler, Action<List<string>> action)
        {
            string PhoneCostStr = string.Empty;
            List<string> results = new List<string>();
            DateTime date;
            for (int i = 0; i < 6; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月详单解析");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == type + date.ToString("yyyyMM")).FirstOrDefault() == null) continue;
                    PhoneCostStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == type + date.ToString("yyyyMM")).FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(System.Web.HttpUtility.HtmlDecode(PhoneCostStr), "//table[@id='table']/tbody/tr", "inner");
                    if (results != null)
                    {
                        action(results);
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
}
