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

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class YN : IMobileCrawler
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
            string Url = string.Empty;
            try
            {
                logDtl = new ApplyLogDtl("初始化登录页面");
                Url = "http://www.yn.10086.cn/service/login.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "初始化登录页面");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "初始化登录页面成功";
                appLog.LogDtlList.Add(logDtl);
                //第二步，获取验证码
                logDtl = new ApplyLogDtl("获取验证码");
                Url = "http://www.yn.10086.cn/service/imageVerifyCode?t=new";
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
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "获取验证码成功";
                appLog.LogDtlList.Add(logDtl);

                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;
                Res.StatusDescription = "云南移动初始化完成";

                CacheHelper.SetCache(Res.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "云南移动初始化异常";
                Log4netAdapter.WriteError("云南移动初始化异常", e);

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
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                logDtl = new ApplyLogDtl("登录");
                Url = "http://www.yn.10086.cn/service/actionDispatcher.do";
                postdata = string.Format("reqUrl=login&busiNum=LOGIN&operType=0&mobile={0}&password={1}&verifyCode={2}&isSavePasswordVal=0&passwordType=1", mobileReq.Mobile, CommonFun.GetEncodeBase64(Encoding.Default, mobileReq.Password), mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
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
                string msg = jsonParser.GetResultFromMultiNode(httpResult.Html, "resultMsg");
                if (!msg.ToLower().Contains("ok"))
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

                Res.StatusDescription = "云南移动登录成功";
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
                Res.StatusDescription = "云南移动登录异常";
                Log4netAdapter.WriteError("云南移动登录异常", e);

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
            string postdata = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                logDtl = new ApplyLogDtl("发手机验证码");
                Url = "http://www.yn.10086.cn/service/my/sms.do";
                postdata = "busiNum=QDCX";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发手机验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string msg = jsonParser.GetResultFromMultiNode(httpResult.Html, "resultMsg");
                if (!msg.IsEmpty())
                {
                    Res.StatusDescription = msg;
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.StatusDescription = "云南移动手机验证码发送成功";
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
                Res.StatusDescription = "云南移动手机验证码发送异常";
                Log4netAdapter.WriteError("云南移动手机验证码发送异常", e);

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
            logDtl = new ApplyLogDtl("验证手机验证码");
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

                Url = "http://www.yn.10086.cn/service/my/actionDispatcher.do";
                postdata = string.Format("reqUrl=QDCXQuery&busiNum=QDCX&queryMonth={0}&queryItem=1&operType=3&smsNum={1}&confirmFlg=1", DateTime.Now.ToString("yyyyMM"), mobileReq.Smscode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
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

                string msg = jsonParser.GetResultFromMultiNode(httpResult.Html, "resultMsg");
                string success = jsonParser.GetResultFromMultiNode(httpResult.Html, "success");
                if (!msg.ToLower().Contains("ok"))
                {
                    Res.StatusDescription = msg;
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                if (success.ToLower() == "false")
                {
                    Res.StatusDescription = "短信验证码错误！";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }

                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.StatusDescription = "云南移动手机验证码校验成功";
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
                Res.StatusDescription = "云南移动手机验证码校验异常";
                Log4netAdapter.WriteError("云南移动手机验证码校验异常", e);

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
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                #region 基本信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Crawler, mobileReq.Website);

                #region 基本信息
                logDtl = new ApplyLogDtl("基本信息");
                Url = "http://www.yn.10086.cn/service/my/actionDispatcher.do";
                postdata = string.Format("reqUrl=MY_GRZLGLQuery&funName=getUserInfo&busiNum=GRZLGL_UserInfoQuery");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                try
                {
                    object Infoobj = JsonConvert.DeserializeObject(httpResult.Html);
                    JObject Infojs = Infoobj as JObject;
                    if (!bool.Parse(Infojs["success"].ToString()))
                    {
                        Res.StatusDescription = Infojs["resultMsg"].ToString();
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
                #endregion

                #region PUK码
                logDtl = new ApplyLogDtl("PUK码");
                Url = "http://www.yn.10086.cn/service/my/actionDispatcher.do";
                postdata = string.Format("reqUrl=MY_GRZLGLQuery&funName=getPuk&busiNum=GRZLGL_PUKMCX");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "PUK码");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "pukInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "PUK码抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                #endregion

                #region 手机积分
                logDtl = new ApplyLogDtl("手机积分");
                Url = "http://www.yn.10086.cn/service/my/actionDispatcher.do";
                postdata = string.Format("reqUrl=JFDHCX_JFCXQuery&busiNum=JFDHCX_JFCX&operType=3&chooseFlag=jfffmx");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "手机积分");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "手机积分抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                #endregion

                #region 星级、套餐
                logDtl = new ApplyLogDtl("星级、套餐");
                Url = string.Format("http://www.yn.10086.cn/service/my/index.html?t={0}#home", CommonFun.GetTimeStamp());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "星级、套餐");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor2", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "星级、套餐抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                #endregion

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 抓取账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Crawler, mobileReq.Website);

                CrawlerBill(crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 抓取详单

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

                #endregion

                //保存
                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "云南移动手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "云南移动手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("云南移动手机账单抓取异常", e);

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
            string result = string.Empty;

            try
            {
                crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);

                #region 基本信息查询
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Analysis, mobileReq.Website);

                mobile.Token = mobileReq.Token;
                mobile.BusName = mobileReq.Name;
                mobile.Mobile = mobileReq.Mobile;
                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.Mobile = mobileReq.Mobile;
                mobile.UpdateTime = crawler.CrawlerDate;

                #region 基本信息
                logDtl = new ApplyLogDtl("基本信息");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                    result = jsonParser.GetResultFromMultiNode(result, "resultObj", ':');
                    if (!string.IsNullOrEmpty(result))
                    {
                        mobile.Name = jsonParser.GetResultFromParser(result, "userName");
                        mobile.Address = jsonParser.GetResultFromParser(result, "homeAddr");
                        if(mobile.Address.IsEmpty())
                            mobile.Address = jsonParser.GetResultFromParser(result, "psptAddr");
                        mobile.Email = jsonParser.GetResultFromParser(result, "contactAddr");
                        mobile.Postcode = jsonParser.GetResultFromParser(result, "postCode");
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

                #region 手机积分
                logDtl = new ApplyLogDtl("手机积分");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                    result = jsonParser.GetResultFromMultiNode(result, "resultObj", ':');
                    if (!string.IsNullOrEmpty(result))
                    {
                        mobile.Integral = jsonParser.GetResultFromParser(result, "totalScore");
                    }
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "手机积分解析成功";
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
                logDtl = new ApplyLogDtl("puk");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "pukInfor").FirstOrDefault().CrawlerTxt);
                    result = jsonParser.GetResultFromMultiNode(result, "resultObj", ':');
                    if (!string.IsNullOrEmpty(result))
                    {
                        mobile.PUK = jsonParser.GetResultFromParser(result, "puk");
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

                #region 星级、套餐、注册时间
                logDtl = new ApplyLogDtl("星级、套餐、注册时间");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor2").FirstOrDefault().CrawlerTxt);
                    int starlevel = CommonFun.GetMidStr(result, "startLevel\":\"", "\"").ToInt(0);
                    if (starlevel == 6)
                        mobile.StarLevel = "5星金";
                    else if (starlevel == 7)
                        mobile.StarLevel = "5星钻";
                    else
                        mobile.StarLevel = starlevel.ToString() + "星";
                    mobile.PackageBrand = CommonFun.GetMidStr(result, "userBrandName\":\"", "\"");
                    mobile.Package = CommonFun.GetMidStr(result, "userProductName\":\"", "\"");
                    mobile.Regdate = DateTime.Parse(CommonFun.GetMidStr(result, "userApplyDate\":\"", "\"")).ToString(Consts.DateFormatString11);
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "星级、套餐、注册时间解析成功";
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

                #region 账单查询
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Analysis, mobileReq.Website);

                AnalysisBill(crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 话费详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.Call, crawler, delegate(List<string> detailinfo)
                {
                    foreach (string detail in detailinfo)
                    {
                        var totalSecond = 0;
                        var usetime = jsonParser.GetResultFromParser(detail, "callTime").ToString();
                        if (!string.IsNullOrEmpty(usetime))
                        {
                            totalSecond = CommonFun.ConvertDate(usetime);
                        }

                        Call call = new Call();
                        call.StartTime = DateTime.Parse(jsonParser.GetResultFromParser(detail, "startTime")).ToString(Consts.DateFormatString11);
                        call.OtherCallPhone = jsonParser.GetResultFromParser(detail, "otherParty");
                        call.UseTime = totalSecond.ToString();
                        call.InitType = jsonParser.GetResultFromParser(detail, "callType");
                        call.CallPlace = jsonParser.GetResultFromParser(detail, "visitAreaCode");
                        call.CallType = jsonParser.GetResultFromParser(detail, "longType");
                        call.SubTotal = jsonParser.GetResultFromParser(detail, "fee").ToDecimal(0);

                        mobile.CallList.Add(call);
                    }
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.SMS, crawler, delegate(List<string> detailinfo)
                {
                    foreach (string detail in detailinfo)
                    {
                        Sms sms = new Sms();
                        sms.StartTime = DateTime.Parse(jsonParser.GetResultFromParser(detail, "startTime")).ToString(Consts.DateFormatString11);
                        sms.SmsPlace = jsonParser.GetResultFromParser(detail, "visitAreaCode");
                        sms.OtherSmsPhone = jsonParser.GetResultFromParser(detail, "otherParty");
                        sms.SmsType = jsonParser.GetResultFromParser(detail, "pubinfoType");
                        sms.InitType = jsonParser.GetResultFromParser(detail, "callType");
                        sms.SubTotal = jsonParser.GetResultFromParser(detail, "fee").ToDecimal(0);

                        mobile.SmsList.Add(sms);
                    }
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.Net, crawler, delegate(List<string> detailinfo)
                {
                    foreach (string detail in detailinfo)
                    {
                        var totalSecond = 0;
                        var usetime = jsonParser.GetResultFromParser(detail, "callTime").ToString();
                        if (!string.IsNullOrEmpty(usetime))
                        {
                            totalSecond = CommonFun.ConvertDate(usetime);
                        }

                        Net gprs = new Net();
                        gprs.StartTime = DateTime.Parse(jsonParser.GetResultFromParser(detail, "startTime")).ToString(Consts.DateFormatString11);
                        gprs.Place = jsonParser.GetResultFromParser(detail, "visitAreaCode");
                        gprs.NetType = jsonParser.GetResultFromParser(detail, "apnni");
                        gprs.PhoneNetType = jsonParser.GetResultFromParser(detail, "modeName");
                        gprs.SubFlow = (int.Parse(jsonParser.GetResultFromParser(detail, "dataValue").ToString()) / 1024).ToString();
                        gprs.UseTime = totalSecond.ToString();
                        gprs.SubTotal = jsonParser.GetResultFromParser(detail, "fee").ToDecimal(0);

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
                Res.Result = jsonParser.SerializeObject(mobile);
                Res.StatusDescription = "云南移动手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "云南移动手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("云南移动手机账单解析异常", e);

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
            string postdata = String.Empty;
            DateTime dateNow = DateTime.Now;
            DateTime date = DateTime.Now;

            //账单信息
            logDtl = new ApplyLogDtl("月账单抓取校验");
            Url = "http://www.yn.10086.cn/service/my/actionDispatcher.do";
            postdata = string.Format("reqUrl=GRZDQuery&busiNum=ZDCX&methodName=real");
            httpItem = new HttpItem()
            {
                URL = Url,
                Method = "Post",
                Postdata = postdata,
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "月账单抓取校验");
            if (httpResult.StatusCode != HttpStatusCode.OK) return;
            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

            for (int i = 0; i < 6; i++)
            {
                date = dateNow.AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月账单抓取");
                Url = "http://www.yn.10086.cn/service/my/actionDispatcher.do";
                postdata = string.Format("reqUrl=GRZDQuery&busiNum=ZDCX&methodName=history&queryMonth={0}", date.ToString("yyyyMM"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, date.ToString(Consts.DateFormatString7) + "月账单抓取");
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                try
                {
                    object Infoobj = JsonConvert.DeserializeObject(httpResult.Html);
                    JObject Infojs = Infoobj as JObject;
                    if (!bool.Parse(Infojs["success"].ToString()))
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = date.ToString(Consts.DateFormatString7) + "月账单抓取失败：" + Infojs["resultMsg"].ToString();
                        appLog.LogDtlList.Add(logDtl);
                        continue;
                    }
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill" + i, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
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
            for (int i = 0; i < 6; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月账单解析");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == ("bill" + i)).FirstOrDefault() == null) continue;
                    PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill" + i)).FirstOrDefault().CrawlerTxt);
                    PhoneBillStr = jsonParser.GetResultFromMultiNode(PhoneBillStr, "resultObj", ':');
                    if (!string.IsNullOrEmpty(PhoneBillStr))
                    {
                        MonthBill bill = new MonthBill();
                        bill.BillCycle = DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString12);
                        bill.TotalAmt = (jsonParser.GetResultFromParser(jsonParser.GetResultFromParser(PhoneBillStr, "realBillInfo"), "newSpayFee").ToDecimal(0) / 100).ToString();
                        bill.PlanAmt = (jsonParser.GetResultFromParser(jsonParser.GetArrayFromParse(jsonParser.GetResultFromParser(PhoneBillStr, "realBillInfo"), "accountBillInfo")[0], "fee").ToDecimal(0) / 100).ToString();
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
        /// <param name="queryType">3:通话；4:上网；5:短信</param>
        /// <returns></returns>
        private void CrawlerDeatils(EnumMobileDeatilType type, CrawlerData crawler)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            DateTime dateNow = DateTime.Now;
            DateTime date = DateTime.Now;
            string queryType = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                queryType = "1";
            else if (type == EnumMobileDeatilType.SMS)
                queryType = "2";
            else
                queryType = "3";

            for (int i = 0; i < 6; i++)
            {
                date = dateNow.AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月详单抓取");
                Url = "http://www.yn.10086.cn/service/my/actionDispatcher.do";
                postdata = string.Format("reqUrl=QDCXQuery&busiNum=QDCX&queryMonth={0}&queryItem={1}&operType=3", date.ToString("yyyyMM"), queryType);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, date.ToString(Consts.DateFormatString7) + "月详单抓取");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                try
                {
                    object Infoobj = JsonConvert.DeserializeObject(httpResult.Html);
                    JObject Infojs = Infoobj as JObject;
                    if (!bool.Parse(Infojs["success"].ToString()))
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = date.ToString(Consts.DateFormatString7) + "月详单抓取失败：" + Infojs["resultMsg"].ToString();
                        appLog.LogDtlList.Add(logDtl);
                        continue;
                    }
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type.ToString() + i, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
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
        private void AnalysisDeatils(EnumMobileDeatilType type, CrawlerData crawler, Action<List<string>> action)
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
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == type.ToString() + i).FirstOrDefault() == null) continue;
                    PhoneCostStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == type.ToString() + i).FirstOrDefault().CrawlerTxt);
                    PhoneCostStr = jsonParser.GetResultFromParser(PhoneCostStr, "resultObj");
                    if (!PhoneCostStr.IsEmpty())
                    {
                        List<string> detailinfo = jsonParser.GetArrayFromParse(PhoneCostStr, "callsInfoList");
                        action(detailinfo);
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
