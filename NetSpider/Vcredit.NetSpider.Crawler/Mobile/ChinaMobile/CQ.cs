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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.Entity.Mongo.Log;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class CQ : IMobileCrawler
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
        #endregion

        /// <summary>
        /// 页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes MobileInit(MobileReq mobileReq = null)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Init, mobileReq.Website);
            VerCodeRes Res = new VerCodeRes();
            Res.Token =  CommonFun.GetGuidID();
            cookies = new CookieCollection();
            string Url = string.Empty;
            try
            {
                //第一步，初始化登录页面
                logDtl = new ApplyLogDtl("初始化登录页面");
                Url = "https://service.cq.10086.cn/httpsFiles/pageLogin.html";
                httpItem = new HttpItem()
                {
                    URL = Url
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "初始化登录页面");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "初始化登录页面成功";
                appLog.LogDtlList.Add(logDtl);
                //第二步，获取验证码
                logDtl = new ApplyLogDtl("获取验证码");
                Url = "https://service.cq.10086.cn/servlet/ImageServlet?random=0.3519415712960807";
                httpItem = new HttpItem()
                {
                    URL = Url,
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
                Res.StatusDescription = "重庆移动初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(Res.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "重庆移动初始化异常";
                Log4netAdapter.WriteError("重庆移动初始化异常", e);

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

            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                logDtl = new ApplyLogDtl("登录");
                Url = string.Format("https://service.cq.10086.cn/ics?service=ajaxDirect/1/login/login/javascript/&pagename=login&eventname=interfaceLogin&cond_REMEMBER_TAG=false&cond_LOGIN_TYPE=2&cond_SERIAL_NUMBER={0}&cond_USER_PASSWD={1}&cond_USER_PASSSMS=&cond_VALIDATE_CODE={2}&ajaxSubmitType=post&ajax_randomcode=0.0225083720477548", mobileReq.Mobile, MultiKeyDES.EncryptDES(mobileReq.Password, mobileReq.Mobile.Substring(0, 8), mobileReq.Mobile.Substring(1, 8), mobileReq.Mobile.Substring(3, 8)), mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = string.Format("1"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string msg = CommonFun.GetMidStr(CommonFun.GetMidStr(httpResult.Html, "RESULTINFO\":\"", ""), "", "\",\"");
                if (msg != "登陆成功。")
                {
                    Res.StatusDescription = msg;
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(msg);

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "重庆移动登录成功";
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
                Res.StatusDescription = "重庆移动登录异常";
                Log4netAdapter.WriteError("重庆移动登录异常", e);

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
            string resultCode = string.Empty;

            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                logDtl = new ApplyLogDtl("发手机验证码");

                Url = string.Format("http://service.cq.10086.cn/ics?service=ajaxDirect/1/secondValidate/secondValidate/javascript/&pagename=secondValidate&eventname=getTwoVerification&GOODSNAME=%E7%94%A8%E6%88%B7%E8%AF%A6%E5%8D%95&DOWHAT=QUE&ajaxSubmitType=post&ajax_randomcode=0.09048542821422556");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = string.Format("1"),
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
                resultCode = CommonFun.GetMidStr(httpResult.Html, "FLAG\":\"", "\"");
                if (!bool.Parse(resultCode))
                {
                    Res.StatusDescription = CommonFun.GetMidStr(httpResult.Html, "RESULTINFO\":\"", "\"");
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.StatusDescription = "重庆移动手机验证码发送成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
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
                Res.StatusDescription = "重庆移动手机验证码发送异常";
                Log4netAdapter.WriteError("重庆移动手机验证码发送异常", e);

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
            string resultCode = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                //验证手机验证码
                logDtl = new ApplyLogDtl("验证手机验证码");
                Url = string.Format("http://service.cq.10086.cn/ics?service=ajaxDirect/1/secondValidate/secondValidate/javascript/&pagename=secondValidate&eventname=checkSMSINFO&cond_USER_PASSSMS={0}&cond_CHECK_TYPE=DETAIL_BILL&cond_loginType=2&ajaxSubmitType=post&ajax_randomcode=0.802890406564971", mobileReq.Smscode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = string.Format("1"),
                    Referer = "http://service.cq.10086.cn/myMobile/detailBill.html",
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
                resultCode = CommonFun.GetMidStr(httpResult.Html, "FLAG\":\"", "\"");
                if (!bool.Parse(resultCode))
                {
                    Res.StatusDescription = CommonFun.GetMidStr(httpResult.Html, "RESULTINFO\":\"", "\"");
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);
                    
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "重庆移动手机验证码验证成功";
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
                Res.StatusDescription = "重庆移动手机验证码发送异常";
                Log4netAdapter.WriteError("重庆移动手机验证码发送异常", e);

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
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                #region 个人信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Crawler, mobileReq.Website);

                #region 基本信息
                logDtl = new ApplyLogDtl("基本信息");
                httpItem = new HttpItem()
                {
                    URL = "http://service.cq.10086.cn/ics?service=ajaxDirect/1/myMobile/myMobile/javascript/&pagename=myMobile&eventname=userInfo&cond_GOODS_ENAME=GRXX&cond_GOODS_NAME=%E4%B8%AA%E4%BA%BA%E4%BF%A1%E6%81%AF&cond_TRANS_TYPE=Q&cond_GOODS_ID=2015061800000665&ajaxSubmitType=get&ajax_randomcode=0.4705127124108265",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    try
                    {
                        object Infoobj = JsonConvert.DeserializeObject(CommonFun.GetMidStr(httpResult.Html, "<DATASETDATA id=\"dataset\"><![CDATA[[", "]]]></DATASETDATA>"));
                        JObject Infojs = Infoobj as JObject;
                        if (Infojs["X_RESULTCODE"].ToString() != "0")
                        {
                            Res.StatusDescription = Infojs["X_RESULTINFO"].ToString();
                            Res.StatusCode = ServiceConsts.StatusCode_fail;

                            logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = "基本信息抓取失败：" + Res.StatusDescription;
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

                #region 我的套餐
                logDtl = new ApplyLogDtl("我的套餐");
                httpItem = new HttpItem()
                {
                    URL = "http://service.cq.10086.cn/ics?service=ajaxDirect/1/myMobile/myMobile/javascript/&pagename=myMobile&eventname=getMyTariff&cond_GOODS_ENAME=WDZF&cond_GOODS_NAME=%E6%88%91%E7%9A%84%E8%B5%84%E8%B4%B9&cond_TRANS_TYPE=Q&cond_GOODS_ID=2015060500000073&ajaxSubmitType=get&ajax_randomcode=0.505008391866306",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "我的套餐");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "packageInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

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

                #region 手机账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Crawler, mobileReq.Website);

                CrawlerBill(crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 话费详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.Call, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.SMS, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.Net, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                //保存
                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "重庆移动手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "重庆移动手机账单抓取异常";
                Log4netAdapter.WriteError("重庆移动手机账单抓取异常", e);

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

        public BaseRes MobileAnalysis(MobileReq mobileReq, DateTime appDate)
        {
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            ApplyLog columnLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Column_Monitoring, mobileReq.Website);
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            MobileMongo mobileMongo = new MobileMongo(appDate);
            CrawlerData crawler = new CrawlerData();
            Basic mobile = new Basic();
            Call call = null;
            Net gprs = null;
            Sms sms = null;
            string result = string.Empty;


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
                    object Infoobj = JsonConvert.DeserializeObject(CommonFun.GetMidStr(result, "<DATASETDATA id=\"dataset\"><![CDATA[[", "]]]></DATASETDATA>"));
                    JObject Infojs = Infoobj as JObject;
                    if (Infojs != null)
                    {
                        mobile.Name = Infojs["NAME"] != null ? Infojs["NAME"].ToString() : Infojs["TELNUM"].ToString();
                        mobile.Integral = Infojs["queryScoreContent"] == null ? "" : Infojs["queryScoreContent"].ToString();
                        mobile.Regdate = Infojs["NETAGE"] == null ? "" : Infojs["NETAGE"].ToString();
                        if (!mobile.Regdate.IsEmpty())
                        {
                            mobile.Regdate = DateTime.Now.AddYears(-mobile.Regdate.Substring(0, 1).ToInt(0)).AddMonths(-mobile.Regdate.Substring(2, 1).ToInt(0)).ToString(Consts.DateFormatString11);
                        }
                    }

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "基本信息";
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
                logDtl = new ApplyLogDtl("手机套餐");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault().CrawlerTxt);
                    object Infoobj = JsonConvert.DeserializeObject(CommonFun.GetMidStr(result, "<DATASETDATA id=\"dataset\"><![CDATA[[", "]]]></DATASETDATA>"));
                    JObject Infojs = Infoobj as JObject;
                    if (Infojs != null)
                    {
                        mobile.Package = Infojs["MAINPRIVNAME"] == null ? "" : Infojs["MAINPRIVNAME"].ToString();
                    }
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "手机套餐";
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

                #region 话费详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.Call, crawler, delegate(JObject bill, string year)
                {
                    var totalSecond = 0;
                    var usetime = bill["c4"].ToString();
                    if (!string.IsNullOrEmpty(usetime))
                    {
                        totalSecond = CommonFun.ConvertDate(usetime);
                    }

                    call = new Call();
                    call.StartTime = DateTime.Parse(year + "-" + bill["c0"].ToString()).ToString(Consts.DateFormatString11);
                    call.CallPlace = bill["c1"].ToString();
                    call.InitType = bill["c2"].ToString();
                    call.OtherCallPhone = bill["c3"].ToString();
                    call.UseTime = totalSecond.ToString();
                    call.CallType = bill["c5"].ToString();
                    call.SubTotal = bill["c7"].ToString().ToDecimal(0);
                    mobile.CallList.Add(call);
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.SMS, crawler, delegate(JObject bill, string year)
                {
                    sms = new Sms();
                    sms.StartTime = DateTime.Parse(year + "-" + bill["c0"].ToString()).ToString(Consts.DateFormatString11);
                    sms.SmsPlace = bill["c1"].ToString();
                    sms.OtherSmsPhone = bill["c2"].ToString();
                    sms.InitType = bill["c3"].ToString();
                    sms.SmsType = bill["c4"].ToString();
                    if (!sms.SmsType.IsEmpty())
                        sms.SmsType = sms.SmsType.Substring(sms.SmsType.Length - 2, 2);
                    sms.SubTotal = bill["c6"].ToString().ToDecimal(0);
                    mobile.SmsList.Add(sms);
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.Net, crawler, delegate(JObject bill, string year)
                {

                    var totalSecond = 0;
                    var usetime = bill["c3"].ToString();
                    if (!string.IsNullOrEmpty(usetime))
                    {
                        totalSecond = CommonFun.ConvertDate(usetime);
                    }

                    gprs = new Net();
                    gprs.StartTime = DateTime.Parse(year + "-" + bill["c0"].ToString()).ToString(Consts.DateFormatString11);
                    gprs.Place = bill["c1"].ToString();
                    gprs.PhoneNetType = bill["c7"].ToString();
                    gprs.NetType = bill["c2"].ToString();
                    gprs.UseTime = totalSecond.ToString();
                    gprs.SubFlow = bill["c4"].ToString();
                    //gprs.SubFlow = !gprs.SubFlow.IsEmpty() ? gprs.SubFlow.Split('|')[0] : "0";
                    gprs.SubTotal = bill["c6"].ToString().ToDecimal(0);
                    mobile.NetList.Add(gprs);
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                //保存
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "重庆移动手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "重庆移动手机账单解析异常";
                Log4netAdapter.WriteError("重庆移动手机账单解析异常", e);

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
            string Url = String.Empty;
            DateTime date = DateTime.Now;
            for (var i = 0; i <= 5; i++)
            {
                date = DateTime.Now.AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月账单抓取");

                Url = String.Format("http://service.cq.10086.cn/ics?service=ajaxDirect/1/myMobile/myMobile/javascript/&pagename=myMobile&eventname=getUserBill2&cond_QUERY_DATE={0}&cond_GOODS_ENAME=WDZD&cond_GOODS_NAME=%E6%88%91%E7%9A%84%E8%B4%A6%E5%8D%95&cond_TRANS_TYPE=Q&cond_GOODS_ID=2015060500000080&ajaxSubmitType=post&ajax_randomcode=0.9076174679034924", date.ToString("yyyyMM"));
                httpItem = new HttpItem()
                {
                    Accept = "application/xml, text/xml, */*; q=0.01",
                    URL = Url,
                    Method = "POST",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, date.ToString(Consts.DateFormatString7) + "月账单抓取");
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                try
                {
                    object Infoobj = JsonConvert.DeserializeObject(CommonFun.GetMidStr(httpResult.Html, "<DATASETDATA id=\"dataset\"><![CDATA[[", "]]]></DATASETDATA>"));
                    JObject Infojs = Infoobj as JObject;
                    JObject detailarr = Infojs["SCORE_INFO"] as JObject;
                    if (detailarr["X_RESULTCODE"].ToString() != "0")
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = date.ToString(Consts.DateFormatString7) + "月账单抓取失败：" + detailarr["X_RESULTINFO"].ToString();
                        appLog.LogDtlList.Add(logDtl);
                        continue;
                    }
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = date.ToString(Consts.DateFormatString7) + "账单抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = date.ToString(Consts.DateFormatString7) + "月账单抓取异常：" + e.Message;
                    appLog.LogDtlList.Add(logDtl);
                    continue;
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
            DateTime date;
            for (var i = 0; i <= 5; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月账单解析");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault() == null) continue;
                    PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    object Infoobj = JsonConvert.DeserializeObject(CommonFun.GetMidStr(PhoneBillStr, "<DATASETDATA id=\"dataset\"><![CDATA[[", "]]]></DATASETDATA>"));
                    JObject Infojs = Infoobj as JObject;
                    if (Infojs == null) continue;
                    JArray detailarr = Infojs["FEE_DETAIL"] as JArray;
                    if (detailarr != null)
                    {
                        mobile.BillList.Add(new MonthBill() { BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12), PlanAmt = detailarr[0]["FEE"].ToString(), TotalAmt = detailarr[detailarr.Count - 1]["FEE"].ToString() });
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
        /// 抓取手机详单
        /// </summary>
        /// <param name="queryType">3:通话；4:上网；5:短信</param>
        /// <returns></returns>
        private void CrawlerDeatils(EnumMobileDeatilType type, CrawlerData crawler)
        {
            string Url = string.Empty;
            DateTime date = DateTime.Now;
            string queryType = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                queryType = "3";
            else if (type == EnumMobileDeatilType.SMS)
                queryType = "5";
            else
                queryType = "4";

            for (int i = 0; i < 6; i++)
            {
                date = DateTime.Now.AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月详单抓取");
                Url = string.Format("http://service.cq.10086.cn/ics?service=ajaxDirect/1/myMobile/myMobile/javascript/&pagename=myMobile&eventname=getDetailBill&cond_DETAIL_TYPE={0}&cond_QUERY_TYPE=0&cond_QUERY_MONTH={1}&cond_GOODS_ENAME=XFMX&cond_GOODS_NAME=%E6%B6%88%E8%B4%B9%E6%98%8E%E7%BB%86&cond_TRANS_TYPE=Q&cond_GOODS_ID=2015060500000083&ajaxSubmitType=post&ajax_randomcode=0.4744389237720791", queryType, date.ToString("yyyyMM"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = string.Format("1"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, date.ToString(Consts.DateFormatString7) + "月详单抓取");
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                try
                {
                    object obj = JsonConvert.DeserializeObject(CommonFun.GetMidStr(httpResult.Html, "<DATASETDATA id=\"dataset\"><![CDATA[[", "]]]></DATASETDATA>"));
                    JObject js = obj as JObject;
                    string isSuccess = js["X_RESULTCODE"].ToString();
                    if (isSuccess != "0")
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = date.ToString(Consts.DateFormatString7) + "月详单抓取失败：" + js["X_RESULTINFO"].ToString();
                        appLog.LogDtlList.Add(logDtl);
                        continue;
                    }
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + "0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = date.ToString(Consts.DateFormatString7) + "详单抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = date.ToString(Consts.DateFormatString7) + "月详单抓取异常：" + e.Message + "原始数据：" + httpResult.Html;
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
        private void AnalysisDeatils(EnumMobileDeatilType type, CrawlerData crawler, Action<JObject, string> action)
        {
            string PhoneCostStr = string.Empty;
            string year = DateTime.Now.Year.ToString();
            DateTime date;
            for (int i = 0; i < 6; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月详单解析");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1))).FirstOrDefault() == null) continue;
                    PhoneCostStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    object obj = JsonConvert.DeserializeObject(CommonFun.GetMidStr(PhoneCostStr, "<DATASETDATA id=\"dataset\"><![CDATA[[", "]]]></DATASETDATA>"));
                    JObject js = obj as JObject;
                    if (js != null)
                    {
                        JArray resultObj = js["resultData"] as JArray;
                        if (resultObj == null) continue;
                        for (int j = 0; j < resultObj.Count; j++)
                        {                          
                            string str=string.Empty;
                            JObject bill = resultObj[j] as JObject;
                            year = year = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).Year.ToString();
                            action(bill, year);
                        }
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
