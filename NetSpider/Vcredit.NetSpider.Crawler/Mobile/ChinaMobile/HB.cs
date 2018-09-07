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
using Vcredit.NetSpider.Entity.Mongo.Log;
using Vcredit.Common.Constants;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class HB : IMobileCrawler
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

        public HB()
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
                Url = "https://hb.ac.10086.cn/login";
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
                Url = "https://hb.ac.10086.cn/SSO/img?codeType=0&rand=1436324066233";
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
                Res.StatusDescription = "湖北移动初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(Res.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "湖北移动初始化异常";
                Log4netAdapter.WriteError("湖北移动初始化异常", e);

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
            string samlart = string.Empty;
            string relayState = string.Empty;
            string passwordType = string.Empty;
            string errorMsg = string.Empty;
            string errFlag = string.Empty;
            string telNum = string.Empty;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                //第三步
                logDtl = new ApplyLogDtl("登录");
                Url = "https://hb.ac.10086.cn/SSO/loginbox";
                postdata = string.Format("accountType=0&username={0}&passwordType=1&password={1}&smsRandomCode=&emailusername=%E8%AF%B7%E8%BE%93%E5%85%A5%E7%99%BB%E5%BD%95%E5%B8%90%E5%8F%B7&emailpassword=&validateCode={2}&action=%2FSSO%2Floginbox&style=mymobile&service=servicenew&continue=http%3A%2F%2Fwww.hb.10086.cn%2Fservicenew%2Findex.action&submitMode=login&guestIP=210.22.124.10", mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode);
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='login_title']", "value");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                samlart = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value")[0];
                relayState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value")[0];
                passwordType = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='PasswordType']", "value")[0];
                errorMsg = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='errorMsg']", "value")[0];
                errFlag = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='errFlag']", "value")[0];
                telNum = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='telNum']", "value")[0];

                //第四步
                Url = "https://hb.ac.10086.cn/servicenew/postLogin.action?timeStamp=1437620340954";
                postdata = string.Format("RelayState={0}&SAMLart={1}&PasswordType={2}&errorMsg={3}&errFlag={4}&telNum={5}", relayState, samlart, passwordType, errorMsg, errFlag, telNum);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录校验");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "湖北移动登录成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_SendSMS;

                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("cookies", cookies);
                dic.Add("mobileReq", mobileReq);
                CacheHelper.SetCache(mobileReq.Token, dic);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = Res.StatusCode;
                logDtl.Description = Res.StatusDescription;
                appLog.LogDtlList.Add(logDtl);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "湖北移动登录异常";
                Log4netAdapter.WriteError("湖北移动登录异常", e);

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
            Dictionary<string, object> dic = null;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    dic = (Dictionary<string, object>)CacheHelper.GetCache(mobileReq.Token);
                    cookies = (CookieCollection)dic["cookies"];
                }

                logDtl = new ApplyLogDtl("发手机验证码");
                Url = "https://hb.ac.10086.cn/login?service=my&style=mymobile&continue=http%3A%2F%2Fwww.hb.10086.cn%3A80%2Fmy%2Fbilldetails%2FqueryInvoice.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发手机验证码style=mymobile");
                string samlart = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value")[0];
                string relayState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value")[0];
                string passwordType = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='PasswordType']", "value")[0];
                string errorMsg = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='errorMsg']", "value")[0];
                string errFlag = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='errFlag']", "value")[0];
                string telNum = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='telNum']", "value")[0];
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.hb.10086.cn/my/notify.action?timeStamp=1437644706578";
                postdata = string.Format("RelayState={0}&SAMLart={1}&PasswordType={2}&errorMsg={3}&errFlag={4}&telNum={5}", relayState.ToUrlEncode(Encoding.GetEncoding("utf-8")), samlart, passwordType, errorMsg, errFlag, telNum);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发手机验证码notify");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.hb.10086.cn/my/billdetails/queryInvoice.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发手机验证码queryInvoice");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //第五步，发手机验证码
                Url = "http://www.hb.10086.cn/my/account/smsRandomPass!sendSmsCheckCode.action?menuid=myDetailBill";
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Method = "POST",
                    Postdata = string.Format("1"),
                    Referer = "http://www.hb.10086.cn/my/billdetails/queryInvoice.action",
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
                if (httpResult.Html == "基础连接已经关闭: 连接被意外关闭。")
                {
                    Res.StatusDescription = httpResult.Html;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                string result = jsonParser.GetResultFromParser(httpResult.Html, "result");
                if (httpResult.StatusCode != HttpStatusCode.OK || result != "1")
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "湖北移动手机验证码发送成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
                dic["cookies"] = cookies;
                CacheHelper.SetCache(mobileReq.Token, dic);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = Res.StatusCode;
                logDtl.Description = Res.StatusDescription;
                appLog.LogDtlList.Add(logDtl);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "湖北移动手机验证码发送异常";
                Log4netAdapter.WriteError("湖北移动手机验证码发送异常", e);

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
            List<string> results = new List<string>();
            Dictionary<string, object> dic = null;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    dic = (Dictionary<string, object>)CacheHelper.GetCache(mobileReq.Token);
                    cookies = (CookieCollection)dic["cookies"];
                }
                mobileReq.Password = ((MobileReq)dic["mobileReq"]).Password;

                //第六步，验证手机验证码
                logDtl = new ApplyLogDtl("验证手机验证码");
                Url = "http://www.hb.10086.cn/my/billdetails/billDetailQry.action?postion=outer";
                postdata = string.Format("detailBean.billcycle={0}&detailBean.selecttype=0&detailBean.flag=GSM&menuid=myDetailBill&groupId=tabs3&detailBean.password={1}&detailBean.chkey={2}", DateTime.Now.ToString("yyyyMM"), mobileReq.Password, mobileReq.Smscode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://www.hb.10086.cn/my/billdetails/queryInvoice.action",
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr/td[@align='left']", "inner");
                if (results != null && results.Count < 3)
                {
                    Res.StatusDescription = results[1];
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "湖北移动手机验证码验证成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_Query;

                dic["cookies"] = cookies;
                CacheHelper.SetCache(mobileReq.Token, dic);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = Res.StatusCode;
                logDtl.Description = Res.StatusDescription;
                appLog.LogDtlList.Add(logDtl);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "湖北移动手机验证码校验异常";
                Log4netAdapter.WriteError("湖北移动手机验证码校验异常", e);

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
            cookies = new CookieCollection();
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            CrawlerData crawler = new CrawlerData() { Token = mobileReq.Token, IdentityCard = mobileReq.IdentityCard, Name = mobileReq.Name, Mobile = mobileReq.Mobile, UserType = "1" };
            string Url = string.Empty;
            string postdata = string.Empty;
            Dictionary<string, object> dic = null;
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    dic = (Dictionary<string, object>)CacheHelper.GetCache(mobileReq.Token);
                    cookies = (CookieCollection)dic["cookies"];
                }
                mobileReq.Password = ((MobileReq)dic["mobileReq"]).Password;

                #region 个人信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Crawler, mobileReq.Website);

                #region 基本信息
                logDtl = new ApplyLogDtl("基本信息");
                Url = "http://www.hb.10086.cn/my/account/basicInfoAction.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://www.hb.10086.cn/my/account/basicInfoAction.action",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    try
                    {
                        if ((HtmlParser.GetResultFromParser(httpResult.Html, "//table/tbody/tr/td", "inner")).Count <= 0)
                        {
                            Res.StatusDescription = "基本信息抓取失败";
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
                logDtl = new ApplyLogDtl("我的积分");
                Url = "http://www.hb.10086.cn/my/score/myScr_qryScr.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://www.hb.10086.cn/my/account/basicInfoAction.action",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "我的积分");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "我的积分抓取成功";
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

                GetDeatils(EnumMobileDeatilType.Call, mobileReq.Password, mobileReq.Smscode, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Crawler, mobileReq.Website);

                GetDeatils(EnumMobileDeatilType.SMS, mobileReq.Password, mobileReq.Smscode, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog); ;
                #endregion

                #region 上网详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Crawler, mobileReq.Website);

                GetDeatils(EnumMobileDeatilType.Net, mobileReq.Password, mobileReq.Smscode, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "湖北移动手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusDescription = "湖北移动手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("湖北移动手机账单抓取异常", e);

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
            List<string> results = new List<string>();

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
                    results = HtmlParser.GetResultFromParser(result, "//table/tbody/tr/td", "inner");
                    if (results.Count > 0)
                    {
                        mobile.Idcard = results[7];
                        mobile.Name = results[0];
                        mobile.Address = results[10];
                        mobile.Regdate = DateTime.Parse(results[9]).ToString(Consts.DateFormatString11);
                        mobile.PUK = results[1];
                        mobile.StarLevel = results[5];
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
                logDtl = new ApplyLogDtl("我的积分");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                    result = CommonFun.GetMidStr(result, "<h4>可用积分</h4><p><span>", "</span>分</p>");
                    if (!result.IsEmpty())
                        mobile.Integral = result;

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "我的积分";
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

                ReadDeatils(EnumMobileDeatilType.Call, crawler, delegate(string[] strArr)
                {
                    var totalSecond = 0;
                    var usetime = strArr[4].Replace("&nbsp;", "").Trim().ToString();
                    if (!string.IsNullOrEmpty(usetime))
                    {
                        totalSecond = CommonFun.ConvertDate(usetime);
                    }
                    call = new Call();
                    call.StartTime = DateTime.Parse(strArr[0].Replace("&nbsp;", "").Trim()).ToString(Consts.DateFormatString11);
                    call.CallPlace = strArr[1].Replace("&nbsp;", "").Trim();
                    call.InitType = strArr[2].Replace("&nbsp;", "").Trim();
                    call.OtherCallPhone = strArr[3].Replace("&nbsp;", "").Trim();
                    call.UseTime = totalSecond.ToString();
                    call.CallType = strArr[5].Replace("&nbsp;", "").Trim();
                    call.SubTotal = strArr[6].Replace("&nbsp;", "").Trim().ToDecimal(0);
                    mobile.CallList.Add(call);
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Analysis, mobileReq.Website);

                ReadDeatils(EnumMobileDeatilType.SMS, crawler, delegate(string[] strArr)
                {
                    sms = new Sms();
                    sms.StartTime = DateTime.Parse(strArr[0].Replace("&nbsp;", "").Trim()).ToString(Consts.DateFormatString11);
                    sms.SmsPlace = strArr[1].Replace("&nbsp;", "").Trim();
                    sms.OtherSmsPhone = strArr[2].Replace("&nbsp;", "").Trim();
                    sms.InitType = strArr[3].Replace("&nbsp;", "").Trim();
                    sms.SmsType = strArr[4].Replace("&nbsp;", "").Trim();
                    sms.SubTotal = strArr[5].Replace("&nbsp;", "").Trim().ToDecimal(0);
                    mobile.SmsList.Add(sms);
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Analysis, mobileReq.Website);

                ReadDeatils(EnumMobileDeatilType.Net, crawler, delegate(string[] strArr)
                {
                    var totalSecond = 0;
                    var usetime = strArr[3].Replace("&nbsp;", "").Trim().ToString();
                    if (!string.IsNullOrEmpty(usetime))
                    {
                        totalSecond = CommonFun.ConvertDate(usetime);
                    }
                    var totalFlow = CommonFun.ConvertGPRS(strArr[4].Replace("&nbsp;", "").Trim().ToString());

                    gprs = new Net();
                    gprs.StartTime = DateTime.Parse(strArr[0].Replace("&nbsp;", "").Trim()).ToString(Consts.DateFormatString11);
                    gprs.Place = strArr[1].Replace("&nbsp;", "").Trim();
                    gprs.PhoneNetType = strArr[2].Replace("&nbsp;", "").Trim();
                    gprs.UseTime = totalSecond.ToString();
                    gprs.SubFlow = totalFlow.ToString();
                    mobile.NetList.Add(gprs);
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "湖北移动手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);

            }
            catch (Exception e)
            {
                Res.StatusDescription = "湖北移动手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("湖北移动手机账单解析异常", e);

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
        /// 抓取详单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private void GetBill(CrawlerData crawler)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            DateTime date = DateTime.Now;
            for (int i = 0; i < 6; i++)
            {
                logDtl = new ApplyLogDtl(date.AddMonths(-i).ToString(Consts.DateFormatString7) + "月账单抓取");
                Url = "http://www.hb.10086.cn/my/billdetails/showbillMixQuery.action?postion=outer";
                postdata = string.Format("qryMonthType=current&theMonth={0}&menuid=myBill&groupId=tabs3", date.AddMonths(-i).ToString("yyyyMM"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://www.hb.10086.cn/my/billdetails/queryInvoice.action",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
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
        /// 解析账单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private void ReadBill(CrawlerData crawler, Basic mobile)
        {
            string PhoneBillStr = string.Empty;
            List<string> results = new List<string>();
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
                    results = HtmlParser.GetResultFromParser(PhoneBillStr, "//div [@class='tab']/div [@class='fyxx']/table/tr/td", "inner");
                    if (results.Count > 3)
                    {
                        bill = new MonthBill();
                        bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);
                        bill.PlanAmt = results[5].Replace("&nbsp;", "").Trim();
                        bill.TotalAmt = results[32].Replace("&nbsp;", "").Trim();
                        mobile.BillList.Add(bill);
                    }

                    //我的套餐和品牌
                    if (mobile.Package.IsEmpty())
                    {
                        //品牌
                        string packageBrand = CommonFun.GetMidStr(PhoneBillStr, "品&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;牌：</label><label>&nbsp;&nbsp;", "</label><br /><label>&nbsp;&nbsp;&nbsp;&nbsp;计费周期：");
                        if (!packageBrand.IsEmpty())
                            mobile.PackageBrand = packageBrand;
                        //套餐
                        results = HtmlParser.GetResultFromParser(PhoneBillStr, "//div [@class='tab4']/div [@class='di_ul']/ul/li", "inner");
                        if (results.Count > 0)
                            mobile.Package = results[3].Replace("&nbsp;", "").Trim();
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
        /// 抓取详单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private void GetDeatils(EnumMobileDeatilType type, string passWord, string smsCode, CrawlerData crawler)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            DateTime date = DateTime.Now;
            string queryType = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                queryType = "GSM";
            else if (type == EnumMobileDeatilType.SMS)
                queryType = "SMS";
            else
                queryType = "GPRSWLAN";
            for (int i = 0; i < 6; i++)
            {
                date = DateTime.Now.AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月详单抓取");
                Url = "http://www.hb.10086.cn/my/billdetails/billDetailQry.action?postion=outer";
                postdata = string.Format("detailBean.billcycle={0}&detailBean.selecttype=0&detailBean.flag={1}&menuid=myDetailBill&groupId=tabs3&detailBean.password={2}&detailBean.chkey={3}", date.ToString("yyyyMM"), queryType, passWord, smsCode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://www.hb.10086.cn/my/billdetails/queryInvoice.action",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, date.AddMonths(-i).ToString(Consts.DateFormatString7) + "月详单抓取");
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + "0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = date.ToString(Consts.DateFormatString7) + "详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
            }
        }

        /// <summary>
        /// 解析详单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private void ReadDeatils(EnumMobileDeatilType type, CrawlerData crawler, Action<string[]> action)
        {
            string PhoneCostStr = string.Empty;
            List<string> deatils = new List<string>();
            DateTime date;
            for (int i = 0; i < 6; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7));
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1))).FirstOrDefault() == null) continue;
                    PhoneCostStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    if (type == EnumMobileDeatilType.Net)
                        deatils = HtmlParser.GetResultFromParser(PhoneCostStr, "//table[@id='GPRS_Item']/tr", "inner");
                    else
                    {
                        deatils = HtmlParser.GetResultFromParser(PhoneCostStr, "//table[@id='table6']/tr", "inner");
                        if (deatils.Count <= 0)
                            deatils = HtmlParser.GetResultFromParser(PhoneCostStr, "//table[@id='table2']/tr", "inner");
                    }
                    if (deatils.Count > 0)
                    {
                        deatils.RemoveAt(0);
                        deatils.RemoveAt(deatils.Count - 1);
                        if (type == EnumMobileDeatilType.Net && deatils.Count > 0)
                            deatils.RemoveAt(0);
                        deatils.ForEach(item =>
                        {
                            string[] strArr = HtmlParser.GetResultFromParser(item, "//td").ToArray();
                            action(strArr);
                        });
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

        private static bool RemoteCertificateValidate(
           object sender, X509Certificate cert,
             X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }
        #endregion

    }
}
