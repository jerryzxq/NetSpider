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
using Vcredit.NetSpider.Entity.Mongo.Log;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class SH : IMobileCrawler
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
        string key1 = "YHXWWLKJYXGS";
        string key2 = "ZFCHHYXFL10C";
        string key3 = "DES";
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
                Url = "https://sh.ac.10086.cn/login";
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
                Url = "https://sh.ac.10086.cn/validationCode";
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
                Res.StatusDescription = "上海移动初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(Res.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "上海移动初始化异常";
                Log4netAdapter.WriteError("上海移动初始化异常", e);

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

                string desmobile = MultiKeyDES.EncryptDES(mobileReq.Mobile, key1, key2, key3);
                string despassword = MultiKeyDES.EncryptDES(mobileReq.Password, key1, key2, key3);
                //第三步，登录
                logDtl = new ApplyLogDtl("登录");
                Url = "https://sh.ac.10086.cn/loginex?act=2";
                postdata = string.Format("telno={0}&password={1}&authLevel=2&validcode={2}&ctype=1&decode=1&source=wsyyt", desmobile, despassword, mobileReq.Vercode);
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
                string Uid = jsonParser.GetResultFromParser(httpResult.Html, "uid");
                if (Uid.IsEmpty())
                {
                    Res.StatusDescription = jsonParser.GetResultFromParser(httpResult.Html, "message"); ;
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //第四步
                Url = string.Format("http://www.sh.10086.cn/sh/wsyyt/ac/forward.jsp?uid={0}&tourl=http://www.sh.10086.cn/sh/service/", Uid);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    //ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录校验uid");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "上海移动登录成功";
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
                Res.StatusDescription = "上海移动登录异常";
                Log4netAdapter.WriteError("上海移动登录异常", e);

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
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                //第五步，发手机验证码
                logDtl = new ApplyLogDtl("发手机验证码");
                httpItem = new HttpItem()
                {
                    URL = string.Format("https://sh.ac.10086.cn/loginex?iscb=1&act=1&telno={0}", mobileReq.Mobile),
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
                string Str = CommonFun.GetMidStr(httpResult.Html, "ssoCreateDtmCallback(", ")");
                if (Str != "")
                {
                    object obj = JsonConvert.DeserializeObject(Str);
                    JObject js = obj as JObject;
                    if (js["result"].ToString() == "1")
                    {
                        Res.StatusDescription = js["message"].ToString();
                        Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.StatusCode = Res.StatusCode;
                        logDtl.Description = Res.StatusDescription;
                        appLog.LogDtlList.Add(logDtl);

                        return Res;
                    }
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "输入手机验证码，调用手机验证码验证接口";
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
                Res.StatusDescription = "上海移动手机验证码发送异常";
                Log4netAdapter.WriteError("上海移动手机验证码发送异常", e);

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
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                string desmobile = MultiKeyDES.EncryptDES(mobileReq.Mobile, key1, key2, key3);
                string despassword = MultiKeyDES.EncryptDES(mobileReq.Smscode, key1, key2, key3);

                //第六步，验证手机验证码
                logDtl = new ApplyLogDtl("验证手机动态验证码");
                Url = string.Format("https://sh.ac.10086.cn/loginex?iscb=1&act=2&telno={0}&password={1}&authLevel=1&validcode=&decode=1", desmobile, despassword);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://www.sh.10086.cn/sh/wsyyt/ac/loginbox.jsp?al=1&telno=" + mobileReq.Mobile,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "验证手机动态验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                string Uid = CommonFun.GetMidStr(httpResult.Html, "ssoLoginCallback(", ")");
                try
                {
                    Uid = jsonParser.GetResultFromParser(Uid, "uid");
                }
                catch { }
                if (Uid.IsEmpty())
                {
                    Res.StatusDescription = "验证失败";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                httpItem = new HttpItem()
                {
                    URL = string.Format("http://www.sh.10086.cn/sh/wsyyt/busi.json?sid=WF000022"),
                    Postdata = "uid=" + Uid,
                    Method = "POST",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "验证手机动态验证码校验uid");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "上海移动手机验证码验证成功";
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
                Res.StatusDescription = "上海移动手机验证码验证异常";
                Log4netAdapter.WriteError("上海移动手机验证码验证异常", e);

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
            List<string[]> results = new List<string[]>();
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                #region 个人信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Crawler, mobileReq.Website);

                #region 基本信息
                logDtl = new ApplyLogDtl("基本信息");
                Url = "http://www.sh.10086.cn/sh/wsyyt/action?act=myarea.getinfoManageMore";
                httpItem = new HttpItem()
                {
                    URL = Url,
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
                try
                {
                    string code = CommonFun.GetMidStr(httpResult.Html, "code\":", ",\"");
                    if (code != "0")
                    {
                        Res.StatusDescription = jsonParser.GetResultFromParser(httpResult.Html, "message");
                        Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);

                        logDtl.StatusCode = Res.StatusCode;
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

                #region 其他基本信息
                logDtl = new ApplyLogDtl("其他基本信息");
                Url = "http://www.sh.10086.cn/sh/wsyyt/action?act=myarea.getinfoManage";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "其他基本信息");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "otherbaseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "其他基本信息抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }

                #endregion

                #region 我的星级
                logDtl = new ApplyLogDtl("我的星级");
                Url = "http://www.sh.10086.cn/sh/wsyyt/action?act=my.getmycredit";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "我的星级");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "starLevelInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "我的星级抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }

                #endregion

                #region 套餐、品牌、积分
                logDtl = new ApplyLogDtl("套餐品牌积分");
                Url = "http://www.sh.10086.cn/sh/wsyyt/action?act=my.getaccountinfo";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "套餐品牌积分");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "otherInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "套餐品牌积分抓取成功";
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

                GetDeatils(EnumMobileDeatilType.SMS, crawler);

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
                Res.StatusDescription = "上海移动手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "上海移动手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("上海移动手机账单抓取异常", e);

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
                    string PersonInfo = jsonParser.GetResultFromParser(result, "value");
                    if (!PersonInfo.IsEmpty())
                    {
                        mobile.Name = jsonParser.GetResultFromParser(PersonInfo, "name");
                        mobile.Idcard = jsonParser.GetResultFromParser(PersonInfo, "zjNum");
                        mobile.Idtype = jsonParser.GetResultFromParser(PersonInfo, "zjType");
                        mobile.Regdate = jsonParser.GetResultFromParser(PersonInfo, "creaateDate");
                        if (!mobile.Regdate.IsEmpty())
                            mobile.Regdate = DateTime.Parse(mobile.Regdate).ToString(Consts.DateFormatString11);
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

                #region 其他基本信息
                logDtl = new ApplyLogDtl("其他基本信息");

                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "otherbaseInfor").FirstOrDefault().CrawlerTxt);
                    string PersonInfo = jsonParser.GetResultFromParser(result, "value");
                    if (!PersonInfo.IsEmpty())
                    {
                        mobile.Address = jsonParser.GetResultFromParser(PersonInfo, "address");
                        mobile.Email = jsonParser.GetResultFromParser(PersonInfo, "email");
                        mobile.Postcode = jsonParser.GetResultFromParser(PersonInfo, "postcode");
                        mobile.PUK = jsonParser.GetResultFromParser(PersonInfo, "puk");
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

                #region 我的星级
                logDtl = new ApplyLogDtl("我的星级");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "starLevelInfor").FirstOrDefault().CrawlerTxt);

                    object starobj = JsonConvert.DeserializeObject(result);
                    JObject starjs = starobj as JObject;
                    if (starjs != null)
                    {
                        JObject starbilldata = starjs["value"] as JObject;
                        if (starbilldata != null)
                            mobile.StarLevel = starbilldata["credit_level"] != null ? starbilldata["credit_level"].ToString() : "";
                    }

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "我的星级解析成功";
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

                #region 套餐、品牌、积分
                logDtl = new ApplyLogDtl("套餐品牌积分");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "otherInfor").FirstOrDefault().CrawlerTxt);

                    object obj = JsonConvert.DeserializeObject(result);
                    JObject js = obj as JObject;
                    JObject billdata = js["value"] as JObject;
                    mobile.Package = billdata["plan_name"].ToString();
                    mobile.PackageBrand = billdata["brand_name"].ToString();
                    mobile.Integral = billdata["jifen_point"].ToString();

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "套餐品牌积分解析成功";
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

                #region 月消费情况
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Analysis, mobileReq.Website);

                ReadBill(crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 话费详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Analysis, mobileReq.Website);

                ReadDeatils(EnumMobileDeatilType.Call, crawler, delegate(List<string[]> itemList, string year)
                {
                    itemList.ForEach(item =>
                    {

                        var totalSecond = 0;
                        var usetime = item[5].ToString();
                        if (!string.IsNullOrEmpty(usetime))
                        {
                            totalSecond = CommonFun.ConvertDate(usetime);
                        }

                        call = new Call();
                        call.StartTime = DateTime.Parse(year + "-" + item[1]).ToString(Consts.DateFormatString11);
                        call.CallPlace = item[2];
                        call.InitType = item[3];
                        call.OtherCallPhone = item[4];
                        call.UseTime = totalSecond.ToString();
                        call.CallType = item[6];
                        call.SubTotal = item[8].ToDecimal(0);
                        mobile.CallList.Add(call);
                    });
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Analysis, mobileReq.Website);

                ReadDeatils(EnumMobileDeatilType.SMS, crawler, delegate(List<string[]> itemList, string year)
                {
                    itemList.ForEach(item =>
                    {
                        sms = new Sms();
                        sms.StartTime = DateTime.Parse(year + "-" + item[1]).ToString(Consts.DateFormatString11);
                        sms.SmsPlace = item[2];
                        sms.OtherSmsPhone = item[3];
                        sms.InitType = item[4];
                        sms.SmsType = item[5];
                        sms.SubTotal = item[7].ToDecimal(0);
                        mobile.SmsList.Add(sms);
                    });
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Analysis, mobileReq.Website);

                ReadDeatils(EnumMobileDeatilType.Net, crawler, delegate(List<string[]> itemList, string year)
                {
                    itemList.ForEach(item =>
                    {

                        var totalSecond = 0;
                        var usetime = item[4].ToString();
                        if (!string.IsNullOrEmpty(usetime))
                        {
                            totalSecond = CommonFun.ConvertDate(usetime);
                        }

                        gprs = new Net();
                        gprs.StartTime = DateTime.Parse(year + "-" + item[1]).ToString(Consts.DateFormatString11);
                        gprs.Place = item[2];
                        gprs.PhoneNetType = item[3];
                        gprs.UseTime = totalSecond.ToString();
                        gprs.SubFlow = CommonFun.ConvertGPRS(item[5]).ToString();
                        gprs.SubTotal = item[7].ToDecimal(0);
                        gprs.NetType = item[8];
                        mobile.NetList.Add(gprs);
                    });
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                //保存
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "上海移动手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "上海移动手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("上海移动手机账单解析异常", e);

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
        /// 抓取手机详单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private void GetDeatils(EnumMobileDeatilType type, CrawlerData crawler)
        {
            DateTime first = DateTime.Now;
            DateTime last = DateTime.Now;
            DateTime date = DateTime.Now;
            string Url = string.Empty;
            string postdata = string.Empty;
            string referer = "http://www.sh.10086.cn/sh/wsyyt/busi/2002_14.jsp";
            string queryType = string.Empty;
            string phoneStr = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                queryType = "NEW_GSM";
            else if (type == EnumMobileDeatilType.SMS)
                queryType = "NEW_SMS";
            else
                queryType = "NEW_GPRS";

            for (int i = 0; i <= 5; i++)
            {
                first = new DateTime(date.Year, date.Month, 1).AddMonths(-i);
                last = first.AddMonths(1).AddDays(-1);
                logDtl = new ApplyLogDtl(first.ToString(Consts.DateFormatString7) + "月详单抓取");
                Url = string.Format("http://www.sh.10086.cn/sh/wsyyt/busi/historySearch.do?method={0}", (i == 0 ? "getOneBillDetailAjax" : "getFiveBillDetailAjax"));
                postdata = string.Format("billType={0}&startDate={1}&endDate={2}{3}&searchStr=-1&index=0&isCardNo=0&gprsType=", queryType, first.ToString("yyyy-MM-dd"), last.ToString("yyyy-MM-dd"), (i == 0 ? "&jingque=" : "&&filterfield=输入对方号码：&filterValue="));
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
                    if (i == 0)
                        phoneStr = CommonFun.GetMidStr(httpResult.Html, "value =\"", "\";members");
                    else
                        phoneStr = CommonFun.GetMidStr(httpResult.Html, "<span>保 存</span></a></div><script type=\"text/javascript\">var value = [];value =", ";members");
                    if (!phoneStr.IsEmpty())
                    {
                        if (jsonParser.DeserializeObject<List<string[]>>(phoneStr) != null && jsonParser.DeserializeObject<List<string[]>>(phoneStr).Count() > 0)
                        {
                            crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + "0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                            logDtl.StatusCode = ServiceConsts.StatusCode_success;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = first.ToString(Consts.DateFormatString7) + "月详单抓取成功";
                            appLog.LogDtlList.Add(logDtl);
                        }
                        else
                        {
                            logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = first.ToString(Consts.DateFormatString7) + "月详单抓取失败";
                            appLog.LogDtlList.Add(logDtl);
                        }
                    }
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = first.ToString(Consts.DateFormatString7) + "月详单抓取异常：" + e.Message;
                    appLog.LogDtlList.Add(logDtl);
                    continue;
                }
            }
        }

        /// <summary>
        /// 读取详单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private void ReadDeatils(EnumMobileDeatilType type, CrawlerData crawler, Action<List<string[]>, string> action)
        {
            List<string[]> itemList = null;
            string PhoneCostStr = string.Empty;
            string year = string.Empty;
            DateTime date;
            for (int i = 0; i <= 5; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月详单解析解析");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1))).FirstOrDefault() == null) continue;
                    PhoneCostStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    if (i == 0)
                        PhoneCostStr = CommonFun.GetMidStr(PhoneCostStr, "value =\"", "\";members");
                    else
                        PhoneCostStr = CommonFun.GetMidStr(PhoneCostStr, "<span>保 存</span></a></div><script type=\"text/javascript\">var value = [];value =", ";members");
                    if (!PhoneCostStr.IsEmpty())
                        itemList = jsonParser.DeserializeObject<List<string[]>>(PhoneCostStr);
                    if (itemList != null && itemList.Count() > 0)
                    {
                        year = date.Year.ToString();
                        action(itemList, year);
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

        /// <summary>
        /// 抓取账单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private void GetBill(CrawlerData crawler)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            DateTime nowDate = DateTime.Now;
            DateTime date = DateTime.Now;
            for (int i = 0; i <= 5; i++)
            {
                date = nowDate.AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月账单抓取");
                if (i == 0)
                {
                    //当月账单
                    Url = "http://www.sh.10086.cn/sh/wsyyt/busi/historySearch.do?method=getFiveBillAll&showType=0&firstPage=y&uniqueKey=15&uniqueName=%E8%B4%A6%E5%8D%95%E6%9F%A5%E8%AF%A2";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = string.Format("1"),
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                }
                else
                {
                    //前五个月账单
                    Url = "http://www.sh.10086.cn/sh/wsyyt/busi/historySearch.do?method=FiveBillAllNewAjax";
                    postdata = string.Format("dateTime={0}&tab=tab1_15&isPriceTaxSeparate=null&showType=0&r=1438223701743", date.ToString("yyyy年MM月"));
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                }
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, date.ToString(Consts.DateFormatString7) + "月账单抓取");
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = date.ToString(Consts.DateFormatString7) + "账单抓取成功";
                appLog.LogDtlList.Add(logDtl);
            }
        }

        /// <summary>
        /// 读取账单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private void ReadBill(CrawlerData crawler, Basic mobile)
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
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault() == null) continue;
                    PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    bill = new MonthBill();
                    bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);
                    if (i == 0)
                    {
                        bill.PlanAmt = CommonFun.GetMidStr(PhoneBillStr, "套餐及固定费</p></span></td><td style=\"text-align:left;padding-left:40px;\">", "</td></tr></tbody>").Replace("&nbsp;", "").Trim();
                        bill.TotalAmt = CommonFun.GetMidStr(PhoneBillStr, "<b>费用合计：", "</b></div></td></tr>").Replace("&nbsp;", "").Trim();
                    }
                    else
                    {
                        bill.PlanAmt = CommonFun.GetMidStr(PhoneBillStr, "套餐及固定费</p></span></td><td style='text-align:left;padding-left:40px;'>￥", "</td></tr><tr><td style='text-align:left;width:60%;padding-left:60px;'><span class='pd-list'>").Replace("&nbsp;", "").Trim();
                        if (bill.PlanAmt.IsEmpty())
                            bill.PlanAmt = CommonFun.GetMidStr(PhoneBillStr, "套餐及固定费</p></span></td><td style='text-align:left;padding-left:40px;'>￥", "</td></tr></table></td>").Replace("&nbsp;", "").Trim();
                        bill.TotalAmt = CommonFun.GetMidStr(PhoneBillStr, "<b>费用合计：", "</b></td></tr><tr><td colspan='2'><div style='border-bottom: 1px solid #9FD0ED;border-top: 1px solid #9FD0ED;height:1px;'>&nbsp;").Replace("&nbsp;", "").Trim();
                    }
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

        #endregion

    }
}
