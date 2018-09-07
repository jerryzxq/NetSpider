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
using Vcredit.NetSpider.Crawler.Mobile;
using Vcredit.NetSpider.DataAccess.Mongo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.Entity.Mongo.Log;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class JS : IMobileCrawler
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
            Res.Token = CommonFun.GetGuidID();
            cookies = new CookieCollection();
            string Url = string.Empty;
            try
            {

                //第一步，初始化登录页面
                logDtl = new ApplyLogDtl("初始化登录页面");
                Url = "https://js.ac.10086.cn/jsauth/dzqd/mh/index.html?v=1";
                httpItem = new HttpItem()
                {
                    URL = Url
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "初始化登录页面");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = "https://js.ac.10086.cn/jsauth/dzqd/addCookie";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = string.Format("1"),
                    Referer = "https://js.ac.10086.cn/jsauth/dzqd/mh/index.html?v=1",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "初始化登录页面addCookie");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "初始化登录页面成功";
                appLog.LogDtlList.Add(logDtl);

                //第二步，获取验证码
                logDtl = new ApplyLogDtl("获取验证码");
                Url = "https://js.ac.10086.cn/jsauth/dzqd/zcyzm?t=new&ik=l_image_code&ss=0.21816414275490148&l_key=9T2DPMIQBT119JQP6S64L5O19F1EEAUL";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "https://js.ac.10086.cn/jsauth/dzqd/mh/index.html?v=1",
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

                Res.StatusDescription = "江苏移动初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;

                CacheHelper.SetCache(Res.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "江苏移动初始化异常";
                Log4netAdapter.WriteError("江苏移动初始化异常", e);

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
            string postdata = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                //第三步
                logDtl = new ApplyLogDtl("登录");
                Url = "https://js.ac.10086.cn/jsauth/popDoorPopLogonServletNewNew";
                postdata = string.Format("mobile={0}&password={1}&icode={2}&l_key=9T2DPMIQBT119JQP6S64L5O19F1EEAUL", mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "https://js.ac.10086.cn/jsauth/dzqd/mh/index.html?v=1",
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
                string rcode = jsonParser.GetResultFromParser(httpResult.Html, "rcode");
                if (rcode != "0000")
                {
                    Res.StatusDescription = rcode == "-8" ? "验证码错误" : (rcode == "-2232" ? "密码错误" : "登录失败");
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
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
                Res.StatusDescription = "江苏移动登录异常";
                Log4netAdapter.WriteError("江苏移动登录异常", e);

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
            string postdata = string.Empty;
            string success = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                logDtl = new ApplyLogDtl("发手机验证码");
                Url = "http://service.js.10086.cn/my/sms.do";
                postdata = "busiNum=QDCX";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://service.js.10086.cn/my/MY_QDCX.html?t=1438318444076",
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
                success = jsonParser.GetResultFromParser(httpResult.Html, "success");
                if (!bool.Parse(success))
                {
                    Res.StatusDescription = jsonParser.GetResultFromParser(httpResult.Html, "resultMsg");
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "江苏移动手机验证码发送成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
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
                Res.StatusDescription = "江苏移动手机验证码发送异常";
                Log4netAdapter.WriteError("江苏移动手机验证码发送异常", e);

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
            string postdata = string.Empty;
            DateTime date = DateTime.Now;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                logDtl = new ApplyLogDtl("验证手机验证码");
                Url = "http://service.js.10086.cn/my/actionDispatcher.do";
                postdata = string.Format("reqUrl=MY_QDCXQueryNew&busiNum=QDCX&queryMonth={0}&queryItem=1&qryPages=&qryNo=1&operType=3&queryBeginTime={1}&queryEndTime={2}&smsNum={3}&confirmFlg=1", date.ToString("yyyyMM"), date.ToString(Consts.DateFormatString12), date.ToString("yyyy-MM-dd"), mobileReq.Smscode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "验证手机验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string success = jsonParser.GetResultFromParser(httpResult.Html, "success");
                if (!bool.Parse(success))
                {
                    Res.StatusDescription = jsonParser.GetResultFromParser(httpResult.Html, "resultMsg");
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "江苏移动手机验证码验证成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
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
                Res.StatusDescription = "江苏移动手机验证码验证异常";
                Log4netAdapter.WriteError("江苏移动手机验证码验证异常", e);

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
            string Url = string.Empty;
            string postdata = string.Empty;
            List<JObject> results = new List<JObject>();
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                #region 个人信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Crawler, mobileReq.Website);

                #region 基本信息
                logDtl = new ApplyLogDtl("基本信息");
                Url = "http://service.js.10086.cn/my/MY_WDJF.html?t=1438241101579";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    try
                    {
                        var result = CommonFun.GetMidStr(httpResult.Html, "window.top.BmonPage.commonBusiCallBack(", ", 'MY_WDJF')</script>");
                        if (result.Contains("系统忙"))
                        {
                            Res.StatusDescription = "系统忙，请稍候再试！";
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
                        object Infoobj = JsonConvert.DeserializeObject(result);
                        JObject Infojs = Infoobj as JObject;
                        JObject Infobdp = Infojs["resultObj"] as JObject;
                        JObject Infodata = Infobdp["userInfo"] as JObject;
                        if (Infodata == null)
                        {
                            Res.StatusDescription = "抓取失败";
                            Res.StatusCode = ServiceConsts.StatusCode_fail;

                            logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = "基本信息抓取失败：" + Res.StatusDescription;
                            appLog.LogDtlList.Add(logDtl);

                            appLog.Token = Res.Token;
                            appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                            logMongo.Save(appLog);
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

                #region puk
                logDtl = new ApplyLogDtl("puk");
                Url = "http://service.js.10086.cn/my/actionDispatcher.do";
                postdata = string.Format("reqUrl=MY_GRZLGLQuery&operNum=2&operType=1&busiNum=GRZLGL_PUKMCX");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "puk");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "pukInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "puk抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                #endregion

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 月消费情况
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Crawler, mobileReq.Website);

                GetBill(crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 话费详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Crawler, mobileReq.Website);

                GetDeatils(EnumMobileDeatilType.Call, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Crawler, mobileReq.Website);

                GetDeatils(EnumMobileDeatilType.SMS, crawler);//短信
                GetDeatils(EnumMobileDeatilType.Other, crawler);//彩信

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Crawler, mobileReq.Website);

                GetDeatils(EnumMobileDeatilType.Net, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                //保存
                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "江苏移动手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "江苏移动手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("江苏移动手机账单抓取异常", e);

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
        /// 读取账单
        /// </summary>
        /// <param name="mobileReq"></param>
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
                    result = CommonFun.GetMidStr(result, "window.top.BmonPage.commonBusiCallBack(", ", 'MY_WDJF')</script>");
                    if (!result.IsEmpty())
                    {
                        object Infoobj = JsonConvert.DeserializeObject(result);
                        JObject Infojs = Infoobj as JObject;
                        JObject Infobdp = Infojs["resultObj"] as JObject;
                        JObject Infodata = Infobdp["userInfo"] as JObject;
                        if (Infodata != null)
                        {
                            mobile.Idtype = Infodata["custIcType"].ToString();
                            mobile.Idcard = Infodata["custIcNo"].ToString();
                            mobile.Mobile = Infodata[ServiceConsts.SpiderType_Mobile].ToString();
                            mobile.Name = Infodata["contactName"].ToString();
                            if (mobile.Name.IsEmpty())
                                mobile.Name = Infodata["userName"].ToString();
                            mobile.Address = Infodata["contactAddr"].ToString();
                            mobile.Email = Infodata["email"].ToString();
                            mobile.Postcode = Infodata["postCode"].ToString();
                            mobile.Regdate = Infodata["userApplyDate"].ToString();
                            if (!mobile.Regdate.IsEmpty())
                                mobile.Regdate = DateTime.Parse(mobile.Regdate).ToString(Consts.DateFormatString11);
                            //mobile.Package = Infodata["realfee"].ToString();
                            mobile.PackageBrand = Infodata["brand_busiNum_name"].ToString();
                            mobile.Integral = Infodata["score"].ToString().IsEmpty() ? "0" : Infodata["score"].ToString();
                            mobile.StarLevel = (mobile.Integral.ToInt(0) > 700) ? "五星钻" : mobile.Integral.ToInt(0) > 500 ? "五金星" : mobile.Integral.ToInt(0) > 350 ? "五星" : mobile.Integral.ToInt(0) > 250 ? "四星" : mobile.Integral.ToInt(0) > 110 ? "三星" : mobile.Integral.ToInt(0) > 60 ? "二星" : mobile.Integral.ToInt(0) > 25 ? "异形" : "准星";
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

                #region puk
                logDtl = new ApplyLogDtl("puk");

                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault() != null)
                    {
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                        if (!result.IsEmpty())
                        {
                            object pukobj = JsonConvert.DeserializeObject(result);
                            JObject pukjs = pukobj as JObject;
                            JObject pukbdp = pukjs["resultObj"] as JObject;
                            JObject pukdata = pukbdp["result"] as JObject;
                            if (pukdata != null)
                                mobile.PUK = pukdata["puk"].ToString();
                        }
                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "puk解析成功";
                        appLog.LogDtlList.Add(logDtl);
                    }
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

                #region 月消费情况
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Analysis, mobileReq.Website);

                ReadBill(crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 话费详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Analysis, mobileReq.Website);

                ReadDeatils(EnumMobileDeatilType.Call, crawler, delegate(JObject itemarr)
                {
                    JArray detailList = itemarr["gsmBillDetail"] as JArray;
                    if (detailList != null && detailList.Count > 0)
                    {
                        detailList.RemoveAt(0);
                        for (int i = 0; i < detailList.Count; i++)
                        {
                            JObject detail = detailList[i] as JObject;
                            if (detail != null)
                            {
                                call = new Call();
                                call.StartTime = DateTime.Parse(detail["startTime"].ToString()).ToString(Consts.DateFormatString11);
                                call.CallPlace = detail["visitArear"].ToString();
                                call.InitType = detail["roamType"].ToString();
                                call.OtherCallPhone = detail["otherParty"].ToString();
                                call.UseTime = detail["callDuration"].ToString();
                                call.CallType = detail["statusType"].ToString();
                                call.SubTotal = detail["totalFee"].ToString().ToDecimal(0);
                                mobile.CallList.Add(call);
                            }
                        }
                    }
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Analysis, mobileReq.Website);

                //短信
                ReadDeatils(EnumMobileDeatilType.SMS, crawler, delegate(JObject itemarr)
                {
                    JArray detailList = itemarr["smsBillDetail"] as JArray;
                    if (detailList != null && detailList.Count > 0)
                    {
                        detailList.RemoveAt(0);
                        for (int i = 0; i < detailList.Count; i++)
                        {
                            JObject detail = detailList[i] as JObject;
                            if (detail != null)
                            {
                                sms = new Sms();
                                sms.StartTime = DateTime.Parse(detail["startTime"].ToString()).ToString(Consts.DateFormatString11);
                                sms.SmsPlace = detail["visitArear"].ToString();
                                sms.OtherSmsPhone = detail["otherParty"].ToString();
                                sms.InitType = detail["statusType"].ToString();
                                sms.SmsType = detail["visitArear"].ToString();
                                sms.SubTotal = detail["totalFee"].ToString().ToDecimal(0);
                                mobile.SmsList.Add(sms);
                            }
                        }
                    }
                });

                //彩信
                ReadDeatils(EnumMobileDeatilType.Other, crawler, delegate(JObject itemarr)
                {
                    JArray detailList = itemarr["mmsBillDetail"] as JArray;
                    if (detailList != null && detailList.Count > 0)
                    {
                        detailList.RemoveAt(0);
                        for (int i = 0; i < detailList.Count; i++)
                        {
                            JObject detail = detailList[i] as JObject;
                            if (detail != null)
                            {
                                sms = new Sms();
                                sms.StartTime = DateTime.Parse(detail["startTime"].ToString()).ToString(Consts.DateFormatString11);
                                sms.SmsPlace = detail["visitArear"].ToString();
                                sms.OtherSmsPhone = detail["otherParty"].ToString();
                                sms.InitType = detail["statusType"].ToString();
                                sms.SmsType = detail["visitArear"].ToString();
                                sms.SubTotal = detail["totalFee"].ToString().ToDecimal(0);
                                mobile.SmsList.Add(sms);
                            }
                        }
                    }
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Analysis, mobileReq.Website);

                ReadDeatils(EnumMobileDeatilType.Net, crawler, delegate(JObject itemarr)
                {
                    JArray detailList = itemarr["gprsBillDetail"] as JArray;
                    if (detailList != null && detailList.Count > 0)
                    {
                        detailList.RemoveAt(0);
                        for (int i = 0; i < detailList.Count; i++)
                        {
                            JObject detail = detailList[i] as JObject;
                            if (detail != null)
                            {
                                gprs = new Net();
                                gprs.StartTime = DateTime.Parse(detail["startTime"].ToString()).ToString(Consts.DateFormatString11);
                                gprs.Place = detail["visitArear"].ToString();
                                gprs.PhoneNetType = detail["cdrApnni"].ToString();
                                gprs.UseTime = detail["duration"].ToString();
                                gprs.SubFlow = detail["busyData"].ToString();
                                gprs.SubTotal = detail["totalFee"].ToString().ToDecimal(0);
                                mobile.NetList.Add(gprs);
                            }
                        }
                    }
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                //保存
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "江苏移动手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);

            }
            catch (Exception e)
            {
                Res.StatusDescription = "江苏移动手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("江苏移动手机账单解析异常", e);

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
        /// 抓取手机帐单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public void GetBill(CrawlerData crawler)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            DateTime date = DateTime.Now;
            string title = string.Empty;
            for (int i = 0; i <= 5; i++)
            {
                title = date.AddMonths(-i).ToString(Consts.DateFormatString7) + "月账单抓取";
                logDtl = new ApplyLogDtl(title);
                Url = "http://service.js.10086.cn/my/actionDispatcher.do";
                postdata = string.Format("reqUrl=MY_GRZDQuery&busiNum=ZDCX&methodName={0}", (i == 0 ? "getMobileRealTimeBill" : ("getMobileHistoryBill&beginDate=" + date.AddMonths(-i).ToString("yyyyMM"))));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, title);
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = title + "成功";
                appLog.LogDtlList.Add(logDtl);
            }
        }

        /// <summary>
        /// 解析手机帐单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public void ReadBill(CrawlerData crawler, Basic mobile)
        {
            string PhoneBillStr = string.Empty;
            MonthBill bill = null;
            DateTime date;
            for (int i = 0; i <= 5; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月账单解析");
                try
                {
                    PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    if (PhoneBillStr.IsEmpty()) continue;
                    object obj = JsonConvert.DeserializeObject(PhoneBillStr);
                    JObject js = obj as JObject;
                    JObject resultObj = js["resultObj"] as JObject;
                    JObject billBeanObj = resultObj["billBean"] as JObject;
                    JObject billRetDetail = billBeanObj["billRetDetail"] as JObject;
                    if (billRetDetail != null)
                    {
                        bill = new MonthBill();
                        bill.BillCycle = billRetDetail["cycle"].ToString();
                        bill.BillCycle = bill.BillCycle.Substring(0, 4) + "-" + bill.BillCycle.Substring(4, 2) + "-" + "01";
                        if (bill.BillCycle.IsEmpty())
                            bill.BillCycle = DateTime.Parse(bill.BillCycle).ToString(Consts.DateFormatString12);
                        bill.TotalAmt = (billRetDetail["totalFee"].ToString().ToDecimal(0) / 100).ToString();
                        if (mobile.Package.IsEmpty())
                            mobile.Package = billRetDetail["mainprodName"].ToString();
                        JArray nowfeeDetailList = billRetDetail["feeDetailList"] as JArray;
                        if (nowfeeDetailList != null && nowfeeDetailList.Count > 0)
                        {
                            bill.PlanAmt = (((JObject)(nowfeeDetailList[0]))["fee"].ToString().ToDecimal(0) / 100).ToString();
                        }
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
        /// 抓取手机详单
        /// </summary>
        /// <param name="queryType">1:通话；7:上网；6:短信；5:彩信</param>
        /// <returns></returns>
        public void GetDeatils(EnumMobileDeatilType type, CrawlerData crawler)
        {
            string PhoneCostStr = string.Empty;
            DateTime first = DateTime.Now;
            DateTime last = DateTime.Now;
            DateTime date = DateTime.Now;
            string Url = "http://service.js.10086.cn/my/actionDispatcher.do";
            string postdata = string.Empty;
            string referer = "http://service.js.10086.cn/my/MY_QDCX.html?t=1438308866196";
            string queryType = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                queryType = "1";
            else if (type == EnumMobileDeatilType.SMS)
                queryType = "5";
            else if (type == EnumMobileDeatilType.Net)
                queryType = "7";
            else
                queryType = "6";

            for (int i = 0; i < 6; i++)
            {
                first = new DateTime(date.Year, date.Month, 1).AddMonths(-i);
                last = first.AddMonths(1).AddDays(-1);
                logDtl = new ApplyLogDtl(first.ToString(Consts.DateFormatString7) + "月详单抓取");
                postdata = string.Format("reqUrl=MY_QDCXQueryNew&busiNum=QDCX&queryMonth={1}&queryItem={0}&qryPages=1%3A1002%3A-1&qryNo=1&operType=3&queryBeginTime={2}&queryEndTime={3}", queryType, first.ToString("yyyyMM"), first.ToString("yyyy-MM-dd"), last.ToString("yyyy-MM-dd"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = referer,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, first.ToString(Consts.DateFormatString7) + "月详单抓取");
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                try
                {
                    object obj = JsonConvert.DeserializeObject(PhoneCostStr);
                    JObject js = obj as JObject;
                    if (js == null)
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = first.ToString(Consts.DateFormatString7) + "月详单抓取失败：" + js["X_RESULTINFO"].ToString();
                        appLog.LogDtlList.Add(logDtl);
                        continue;
                    }
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + "0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = first.ToString(Consts.DateFormatString7) + "详单抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = first.ToString(Consts.DateFormatString7) + "月详单抓取异常：" + e.Message + "原始数据：" + httpResult.Html;
                    appLog.LogDtlList.Add(logDtl);

                    continue;
                }
            }
        }

        /// <summary>
        /// 解析手机详单
        /// </summary>
        /// <param name="queryType">1:通话；7:上网；6:短信；5:彩信</param>
        /// <returns></returns>
        public void ReadDeatils(EnumMobileDeatilType type, CrawlerData crawler, Action<JObject> action)
        {
            string PhoneCostStr = string.Empty;
            DateTime date;
            for (int i = 0; i < 6; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7));
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1))).FirstOrDefault() == null) continue;
                    PhoneCostStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    if (PhoneCostStr.IsEmpty()) continue;
                    object obj = JsonConvert.DeserializeObject(PhoneCostStr);
                    JObject js = obj as JObject;
                    if (js != null)
                    {
                        JObject resultObj = js["resultObj"] as JObject;
                        if (js != null)
                        {
                            JObject itemarr = resultObj["qryResult"] as JObject;
                            action(itemarr);
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
