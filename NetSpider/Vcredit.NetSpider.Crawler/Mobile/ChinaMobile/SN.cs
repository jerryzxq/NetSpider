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
using System.Xml.Linq;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.DataAccess.Mongo;
using System.Text.RegularExpressions;
using System.Web;
using Vcredit.Common.Constants;
using Vcredit.NetSpider.Entity.Mongo.Log;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class SN : IMobileCrawler
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
        public SN()
        {
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
        }

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
                Url = "http://www.10086.cn/sn/";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "初始化登录页面");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://10086.cn/service/ip/ipajax.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "初始化登录页面");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "https://sn.ac.10086.cn/login";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "初始化登录页面");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "初始化登录页面成功";
                appLog.LogDtlList.Add(logDtl);
                //第二步，获取验证码
                logDtl = new ApplyLogDtl("获取验证码");
                Url = "https://sn.ac.10086.cn/servlet/CreateImage?1";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    Referer = "https://sn.ac.10086.cn/login",
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

                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;
                Res.StatusDescription = "陕西移动初始化完成";

                CacheHelper.SetCache(Res.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "陕西移动初始化异常";
                Log4netAdapter.WriteError("陕西移动初始化异常", e);

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
                ////获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                logDtl = new ApplyLogDtl("校验验证码");
                Url = String.Format("https://sn.ac.10086.cn/servlet/CheckCode?code={0}", mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (httpResult.Html == "-1")
                {
                    Res.StatusDescription = "验证码错误";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode("验证码错误");

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //第一步
                logDtl = new ApplyLogDtl("登录");
                Url = "https://sn.ac.10086.cn/loginAction";
                postdata = "userName={0}&password={1}&verifyCode={2}&OrCookies=1&loginType=1&fromUrl=uiue%2Flogin_max.jsp&toUrl=http%3A%2F%2Fwww.sn.10086.cn%2Fmy%2Faccount%2F";
                postdata = string.Format(postdata, mobileReq.Mobile, mobileReq.Password.ToBase64(), mobileReq.Vercode);
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
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                var message = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='message']/span", "text");
                if (message.Count() > 0)
                {
                    Res.StatusDescription = message[0];
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode("验证码错误");

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //跳转个人主页
                Url = "http://service.sn.10086.cn/app?service=page/MyMobileBusiness&listener=initPage&random=0.8589307564570456";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "跳转个人主页");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "登录成功";
                appLog.LogDtlList.Add(logDtl);

                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_SendSMS;
                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "陕西移动手机登录异常";
                Log4netAdapter.WriteError("陕西移动手机登录异常", e);

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
            try
            {
                ////获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }
                //发送验证码
                logDtl = new ApplyLogDtl("发手机验证码");
                Url = "http://service.sn.10086.cn/app?service=ajaxDirect/1/DetailedQuery/DetailedQuery/javascript/null&pagename=DetailedQuery&eventname=sendSMS&&serialNumber={0}&partids=null&ajaxSubmitType=get&ajax_randomcode=0.5322220642400511";
                Url = String.Format(Url, mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "陕西移动手机验证码发送成功";
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
                Res.StatusDescription = "陕西移动手机验证码发送异常";
                Log4netAdapter.WriteError("陕西移动手机验证码发送异常", e);

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
            string resultStr = string.Empty;
            try
            {
                ////获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                //校验验证码
                logDtl = new ApplyLogDtl("验证手机验证码");
                Url = "http://service.sn.10086.cn/app?service=ajaxDirect/1/MyMobileSendSms/MyMobileSendSms/javascript/null&pagename=MyMobileSendSms&eventname=forgotPwd&&SMS_NUMBER={0}&partids=null&ajaxSubmitType=get&ajax_randomcode=0.8386197915521436";
                Url = String.Format(Url, mobileReq.Smscode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
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
                resultStr = getStringByElementName(httpResult.Html, "DATASETDATA");
                var result = JsonConvert.DeserializeObject(resultStr);
                JArray js = result as JArray;
                var smsDesc = String.Empty;
                for (var i = 0; i < js.Count; i++)
                {
                    smsDesc = js[i]["SMSDESC"].ToString();
                }
                if (smsDesc != "ok")
                {
                    Res.StatusDescription = "短信码验证失败";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "陕西移动手机验证码验证成功";
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
                Res.StatusDescription = "陕西移动手机验证码验证异常";
                Log4netAdapter.WriteError("陕西移动手机验证码验证异常", e);

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
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                #region 基本信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Crawler, mobileReq.Website);

                #region 基本信息
                logDtl = new ApplyLogDtl("基本信息");
                httpItem = new HttpItem()
                {
                    URL = "http://service.sn.10086.cn/app?service=page/PersonalInfoQuery&listener=initPage&type=5&random=0.8578035789242864",
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "基本信息抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                #endregion

                #region 个人积分
                httpItem = new HttpItem()
                {
                    URL = "http://service.sn.10086.cn/app?service=page/ScoreSumQuery&listener=initPage&type=9&random=0.4304588612363084",
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "个人积分");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "个人积分抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                #endregion

                #region 套餐类型
                httpItem = new HttpItem()
                {
                    URL = "http://service.sn.10086.cn/app?service=page/MyBillQuery&listener=initPage&type=1&random=0.9121211604940703",
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "套餐类型");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "packageInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "套餐类型抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                #endregion

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region  手机账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Crawler, mobileReq.Website);

                CrawlerBill(crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 通话记录
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Crawler, mobileReq.Website);

                CrawlerDeatils(crawler, EnumMobileDeatilType.Call);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Crawler, mobileReq.Website);

                CrawlerDeatils(crawler, EnumMobileDeatilType.SMS);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网流量
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Crawler, mobileReq.Website);

                CrawlerDeatils(crawler, EnumMobileDeatilType.Net);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "陕西移动手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusDescription = "陕西移动手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("陕西移动手机账单查询异常", e);

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
                    //姓名
                    var name = HtmlParser.GetResultFromParser(result, "//input[@id='CUST_NAME']", "value");
                    if (name.Count > 0)
                    {
                        mobile.Name = HttpUtility.HtmlDecode(name[0]);
                    }
                    //手机号
                    var mobilenum = HtmlParser.GetResultFromParser(result, "//input[@id='SERIAL_NUMBER']", "value");
                    if (mobilenum.Count > 0)
                    {
                        mobile.Mobile = mobilenum[0];
                    }
                    //手机号
                    var Regdate = HtmlParser.GetResultFromParser(result, "//input[@id='OPEN_DATE']", "value");
                    if (Regdate.Count > 0)
                    {
                        mobile.Regdate = DateTime.Parse(Regdate[0]).ToString(Consts.DateFormatString11);
                    }
                    //身份类型
                    var Idtype = HtmlParser.GetResultFromParser(result, "//input[@id='PSPT_TYPE_CODE']", "value");
                    if (Idtype.Count > 0)
                    {
                        mobile.Idtype = HttpUtility.HtmlDecode(Idtype[0]);
                    }
                    //身份证
                    var Idcard = HtmlParser.GetResultFromParser(result, "//input[@id='PSPT_ID']", "value");
                    if (Idcard.Count > 0)
                    {
                        mobile.Idcard = Idcard[0];
                    }
                    //地址
                    var address = HtmlParser.GetResultFromParser(result, "//input[@id='POST_ADDRESS']", "value");
                    if (address.Count > 0)
                    {
                        mobile.Address = HttpUtility.HtmlDecode(address[0]);
                    }
                    //邮编
                    var Postcode = HtmlParser.GetResultFromParser(result, "//input[@id='POST_CODE']", "value");
                    if (Postcode.Count > 0)
                    {
                        mobile.Postcode = Postcode[0];
                    }
                    //EMAI
                    var Email = HtmlParser.GetResultFromParser(result, "//input[@id='EMAIL']", "value");
                    if (Email.Count > 0)
                    {
                        mobile.Email = Email[0];
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

                #region 个人积分
                logDtl = new ApplyLogDtl("个人积分");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                    var totoal = Regex.Matches(result, @"积分总计.*");
                    if (totoal.Count > 0)
                    {
                        var Integral = Regex.Matches(totoal[0].ToString(), @"\d+\.?[0-9]*");
                        if (Integral.Count > 0)
                        {
                            mobile.Integral = Integral[0].ToString();
                        }
                    }
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "个人积分解析成功";
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

                #region 套餐类型
                logDtl = new ApplyLogDtl("套餐类型");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault().CrawlerTxt);
                    var Package = HtmlParser.GetResultFromParser(result, "//div[@class='query']/table[@class='listTable1']/tr[2]/td[1]", "inner");
                    if (Package.Count > 0)
                    {
                        mobile.Package = HttpUtility.HtmlDecode(Package[0].ToString());
                    }
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "套餐类型解析成功";
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

                #region  手机账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Analysis, mobileReq.Website);

                AnalysisBill(crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 通话记录
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Analysis, mobileReq.Website);

                AnalysisDeatils(crawler, mobile, EnumMobileDeatilType.Call);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Analysis, mobileReq.Website);

                AnalysisDeatils(crawler, mobile, EnumMobileDeatilType.SMS);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网流量
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Analysis, mobileReq.Website);

                AnalysisDeatils(crawler, mobile, EnumMobileDeatilType.Net);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "陕西移动手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);

            }
            catch (Exception e)
            {
                Res.StatusDescription = "陕西移动手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("陕西移动手机账单解析异常", e);

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
        /// 抓取账单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private void CrawlerBill(CrawlerData crawler)
        {
            string Url = String.Empty;
            var startDate = String.Empty;
            var currdate = String.Empty;
            DateTime now;
            String endnow;

            for (var i = 0; i <= 5; i++)
            {
                now = DateTime.Now.AddMonths(-i);
                logDtl = new ApplyLogDtl(now.ToString(Consts.DateFormatString7) + "月账单抓取");
                startDate = new DateTime(now.Year, now.Month, 1).ToString(Consts.DateFormatString2);
                endnow = new DateTime(now.Year, now.Month, 1).AddMonths(1).ToString(Consts.DateFormatString2);
                currdate = now.ToString(Consts.DateFormatString7);
                var month = currdate;
                Url = String.Format("http://service.sn.10086.cn/app?service=ajaxDirect/1/MyBillQuery/MyBillQuery/javascript/resultShow&pagename=MyBillQuery&eventname=querybyOtherMonth&&selectMonth=&DATE_THISACCT={0}&DATE_LASTACCT={1}&FLAGBY=201506&FLAG=0&piePic=&barPic=&MONTH={2}&partids=resultShow&ajaxSubmitType=get&ajax_randomcode=0.8181526948933608", endnow, startDate, currdate);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, now.ToString(Consts.DateFormatString7) + "月账单抓取");
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = now.ToString(Consts.DateFormatString7) + "账单抓取成功";
                appLog.LogDtlList.Add(logDtl);
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
            MonthBill bill = null;
            DateTime date;

            for (var i = 0; i <= 5; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月账单解析");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault() == null) continue;
                    PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    PhoneBillStr = getStringByElementName(PhoneBillStr, "part");
                    bill = new MonthBill();
                    bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);
                    if (!PhoneBillStr.IsEmpty())
                    {
                        var PlanAmt = HtmlParser.GetResultFromParser(PhoneBillStr, "//div[@class='boxNote padding0']/div/table[@class='listTable1']/tbody/tr[3]/td[2]", "inner");
                        if (PlanAmt.Count > 0)
                        {
                            var PlanAmtStr = Regex.Matches(PlanAmt[0].ToString(), @"\d+\.?[0-9]*");
                            if (PlanAmtStr.Count > 0)
                            {
                                bill.PlanAmt = PlanAmtStr[0].ToString();
                            }
                        }
                        var TotalAmt = HtmlParser.GetResultFromParser(PhoneBillStr, "//div[@class='boxNote padding0']/div/table[@class='listTable1']/tbody/tr[2]/td[2]", "inner");
                        if (TotalAmt.Count > 0)
                        {
                            var TotalAmtStr = Regex.Matches(TotalAmt[0].ToString(), @"\d+\.?[0-9]*");
                            if (TotalAmtStr.Count > 0)
                            {
                                bill.TotalAmt = TotalAmtStr[0].ToString();
                            }
                        }
                    }
                    //添加账单
                    mobile.BillList.Add(bill);
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
        /// <param name="queryType">2 获取通话记录，4 上网详单 3 短信详单</param>
        /// <returns></returns>
        private void CrawlerDeatils(CrawlerData crawler, EnumMobileDeatilType type)
        {
            string Url = String.Empty;
            var startDate = String.Empty;
            DateTime now;
            var resultStr = String.Empty;
            string biil_type = String.Empty;
            string title = String.Empty;

            for (var i = 0; i <= 5; i++)
            {
                now = DateTime.Now.AddMonths(-i);
                startDate = new DateTime(now.Year, now.Month, 1).ToString(Consts.DateFormatString2);
                var month = now.ToString(Consts.DateFormatString7);
                title = now.ToString(Consts.DateFormatString7) + "月详单";
                logDtl = new ApplyLogDtl(title + "分页信息抓取");
                if (type == EnumMobileDeatilType.Call)
                    biil_type = "201";
                else if (type == EnumMobileDeatilType.SMS)
                    biil_type = "202";
                else
                    biil_type = "203";
                Url = String.Format("http://service.sn.10086.cn/app?service=ajaxDirect/1/DetailedQuery/DetailedQuery/javascript/refushBusiSearchResult&pagename=DetailedQuery&eventname=queryAll&&MONTH={0}&MONTH_DAY=&LAST_MONTH_DAY={1}&BILL_TYPE={2}&SHOW_TYPE=0&partids=refushBusiSearchResult&ajaxSubmitType=get&ajax_randomcode=0.6271440942648188", month, startDate, biil_type);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, title + "分页信息抓取");
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                resultStr = getStringByElementName(httpResult.Html, "part");

                if (!resultStr.IsEmpty())
                {
                    var pageCount = HtmlParser.GetResultFromParser(resultStr, "//div[@class='Page']/div[@class='PageCenter']/a");
                    int count = pageCount.Count > 0 ? pageCount.Count - 2 : 1;
                    for (var j = 1; j <= count; j++)
                    {
                        logDtl = new ApplyLogDtl(title + "第" + j + "页抓取");
                        string urlData = String.Format("http://service.sn.10086.cn/app?service=ajaxDirect/1/DetailedQuery/DetailedQuery/javascript/refushBusiSearchResult&pagename=DetailedQuery&eventname=queryAll&pagination_iPage={0}&partids=refushBusiSearchResult&ajaxSubmitType=get&ajax_randomcode=0.5173067574846596", j);
                        httpItem = new HttpItem()
                        {
                            URL = urlData,
                            Method = "get",
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtmlLog(httpItem, appLog, title + "第" + j + "页抓取");
                        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                        if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                        try
                        {
                            var resultResultCount = HtmlParser.GetResultFromParser(getStringByElementName(httpResult.Html, "part"), "//table[@class='listTable1']/tbody/tr");
                            if (resultResultCount.Count <= 0)
                            {
                                logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                                logDtl.Description = title + "第" + j + "页抓取失败";
                                appLog.LogDtlList.Add(logDtl);
                                continue;
                            }
                            crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + (i + 1).ToString() + (j + 1).ToString(), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                            logDtl.StatusCode = ServiceConsts.StatusCode_success;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = title + "第" + j + "页抓取成功";
                            appLog.LogDtlList.Add(logDtl);
                        }
                        catch (Exception e)
                        {
                            logDtl.StatusCode = ServiceConsts.StatusCode_error;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = title + "第" + j + "页抓取异常：" + e.Message + "原始数据：" + httpResult.Html;
                            appLog.LogDtlList.Add(logDtl);

                            continue;
                        }
                    }
                }
            }


        }

        /// <summary>
        /// 解析手机详单
        /// </summary>
        /// <param name="queryType">2 获取通话记录，4 上网详单 3 短信详单</param>
        /// <returns></returns>
        private void AnalysisDeatils(CrawlerData crawler, Basic mobile, EnumMobileDeatilType type)
        {
            List<CrawlerDtlData> PhoneCrawlerDtls = new List<CrawlerDtlData>();
            string PhoneStr = string.Empty;
            DateTime now;
            for (var i = 0; i <= 5; i++)
            {
                now = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(now.ToString(Consts.DateFormatString7) + "月详单解析");
                try
                {
                    PhoneCrawlerDtls = crawler.DtlList.Where(x => x.CrawlerTitle.StartsWith(type + (i + 1).ToString())).OrderBy(x => x.CrawlerTitle).ToList<CrawlerDtlData>();
                    if (PhoneCrawlerDtls == null || PhoneCrawlerDtls.Count <= 0) continue;
                    foreach (CrawlerDtlData item in PhoneCrawlerDtls)
                    {
                        PhoneStr = System.Text.Encoding.Default.GetString(item.CrawlerTxt);
                        PhoneStr = getStringByElementName(PhoneStr, "part");
                        var resultResultCount = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='listTable1']/tbody/tr");
                        for (var k = 1; k <= resultResultCount.Count; k++)
                        {
                            if (type == EnumMobileDeatilType.SMS)
                            {
                                Sms phoneSMS = new Sms();
                                var startTime = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='listTable1']/tbody/tr[" + k + "]/td[1]", "inner");
                                if (startTime.Count > 0)
                                {
                                    phoneSMS.StartTime = DateTime.Parse(now.Year + "-" + startTime[0].ToString().Replace("\n", "")).ToString(Consts.DateFormatString11);
                                }
                                var SubTotal = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='listTable1']/tbody/tr[" + k + "]/td[6]", "inner");
                                if (SubTotal.Count > 0)
                                {
                                    phoneSMS.SubTotal = SubTotal[0].ToDecimal().Value;
                                }
                                var InitType = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='listTable1']/tbody/tr[" + k + "]/td[3]", "inner");
                                if (InitType.Count > 0)
                                {
                                    phoneSMS.InitType = InitType[0];
                                }
                                var OtherSmsPhone = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='listTable1']/tbody/tr[" + k + "]/td[2]", "inner");
                                if (OtherSmsPhone.Count > 0)
                                {
                                    phoneSMS.OtherSmsPhone = OtherSmsPhone[0];
                                }
                                var SmsType = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='listTable1']/tbody/tr[" + k + "]/td[4]", "inner");
                                if (SmsType.Count > 0)
                                {
                                    phoneSMS.SmsType = SmsType[0];
                                }
                                mobile.SmsList.Add(phoneSMS);
                            }
                            else if (type == EnumMobileDeatilType.Call)
                            {
                                Call phoneCall = new Call();
                                var startTime = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='listTable1']/tbody/tr[" + k + "]/td[1]", "inner");
                                if (startTime.Count > 0)
                                {
                                    phoneCall.StartTime = DateTime.Parse(now.Year + "-" + startTime[0].Replace("\n", "")).ToString(Consts.DateFormatString11);
                                }
                                var SubTotal = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='listTable1']/tbody/tr[" + k + "]/td[7]", "inner");
                                if (SubTotal.Count > 0)
                                {
                                    phoneCall.SubTotal = SubTotal[0].ToDecimal().Value;
                                }
                                var UseTime = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='listTable1']/tbody/tr[" + k + "]/td[5]", "inner");
                                if (UseTime.Count > 0)
                                {
                                    phoneCall.UseTime = UseTime[0];
                                }
                                var OtherCallPhone = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='listTable1']/tbody/tr[" + k + "]/td[4]", "inner");
                                if (OtherCallPhone.Count > 0)
                                {
                                    phoneCall.OtherCallPhone = OtherCallPhone[0];
                                }
                                var CallPlace = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='listTable1']/tbody/tr[" + k + "]/td[2]", "inner");
                                if (CallPlace.Count > 0)
                                {
                                    phoneCall.CallPlace = CallPlace[0];
                                }
                                var CallType = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='listTable1']/tbody/tr[" + k + "]/td[6]", "inner");
                                if (CallType.Count > 0)
                                {
                                    phoneCall.CallType = CallType[0];
                                }
                                var InitType = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='listTable1']/tbody/tr[" + k + "]/td[3]", "inner");
                                if (InitType.Count > 0)
                                {
                                    phoneCall.InitType = InitType[0];
                                }
                                mobile.CallList.Add(phoneCall);

                            }
                            else
                            {
                                Net phoneGPRS = new Net();
                                var StartTime = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='listTable1']/tbody/tr[" + k + "]/td[1]", "inner");
                                if (StartTime.Count > 0)
                                {
                                    phoneGPRS.StartTime = DateTime.Parse(now.Year + "-" + StartTime[0].Replace("\n", "")).ToString(Consts.DateFormatString11);
                                }
                                var UseTime = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='listTable1']/tbody/tr[" + k + "]/td[4]", "inner");
                                if (UseTime.Count > 0)
                                {
                                    phoneGPRS.UseTime = UseTime[0];
                                }
                                var Place = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='listTable1']/tbody/tr[" + k + "]/td[2]", "inner");
                                if (Place.Count > 0)
                                {
                                    phoneGPRS.Place = Place[0];
                                }
                                var SubFlow = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='listTable1']/tbody/tr[" + k + "]/td[5]", "inner");
                                if (SubFlow.Count > 0)
                                {
                                    phoneGPRS.SubFlow = SubFlow[0];
                                }
                                var PhoneNetType = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='listTable1']/tbody/tr[" + k + "]/td[3]", "inner");
                                if (PhoneNetType.Count > 0)
                                {
                                    phoneGPRS.PhoneNetType = PhoneNetType[0];
                                }
                                mobile.NetList.Add(phoneGPRS);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = now.ToString(Consts.DateFormatString7) + "月详单解析异常：" + e.Message;
                    appLog.LogDtlList.Add(logDtl);
                    continue;
                }
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = now.ToString(Consts.DateFormatString7) + "月详单解析成功";
                appLog.LogDtlList.Add(logDtl);
            }
        }

        private static bool RemoteCertificateValidate(
           object sender, X509Certificate cert,
             X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }

        #endregion

    }
}
