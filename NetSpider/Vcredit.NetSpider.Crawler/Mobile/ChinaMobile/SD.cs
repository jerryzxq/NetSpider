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
using System.Text.RegularExpressions;
using Vcredit.Common.Constants;
using System.Collections;
using System.IO;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using System.Threading.Tasks;
using Vcredit.NetSpider.Entity.Mongo.Log;
namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class SD : IMobileCrawler
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
            cookies = new CookieCollection();
            try
            {

                //第一步，初始化登录页面
                logDtl = new ApplyLogDtl("初始化登录页面");
                Url = "https://sd.ac.10086.cn/login/";
                httpItem = new HttpItem()
                {
                    URL = Url
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "初始化登录页面");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "山东移动初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;
                CacheHelper.SetCache(Res.Token, cookies);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "初始化登录页面成功";
                appLog.LogDtlList.Add(logDtl);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "山东移动初始化异常";
                Log4netAdapter.WriteError("山东移动初始化异常", e);

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
            cookies = new CookieCollection();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                logDtl = new ApplyLogDtl("登录");
                postdata = "mobileNum={0}&servicePWD={1}&randCode=请点击&smsRandomCode=&submitMode=2&logonMode=1&FieldID=1&ReturnURL=" + ("www.sd.10086.cn/eMobile/jsp/common/prior.jsp").ToUrlEncode() + "&ErrorUrl=" + ("../mainLogon.do").ToUrlEncode() + "&entrance=IndexBrief&codeFlag=0&openFlag=1";
                postdata = string.Format(postdata, mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = "https://sd.ac.10086.cn/portal/servlet/LoginServlet",
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
                string result = httpResult.Html;
                if (result != "0")
                {
                    Res.StatusDescription = result;
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "登录成功";
                appLog.LogDtlList.Add(logDtl);

                //第二步
                logDtl = new ApplyLogDtl("登录校验");
                Url = "http://www.sd.10086.cn/portal/servlet/CookieServlet?FieldID=2";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录校验CookieServlet");
                var attritd = String.Empty;
                var html = httpResult.Html;
                if (html.Contains("var"))
                {
                    int startindex = html.IndexOf("'");
                    int lastindex = html.LastIndexOf("'");
                    attritd = html.Substring(startindex + 1, lastindex - (startindex + 1));
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //第三步
                Url = String.Format("http://www.sd.10086.cn/eMobile/loginSSO.action?Attritd={0}", attritd);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    //ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录校验loginSSO");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "登录校验成功";
                appLog.LogDtlList.Add(logDtl);

                #region 校验是否首次登录

                logDtl = new ApplyLogDtl("校验是否首次登录");
                Url = "http://www.sd.10086.cn/eMobile/index.action?menuid=index&pageid=1664607048";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验是否首次登录index");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                if (httpResult.Html.Contains("firstLogin"))
                {
                    Url = "http://www.sd.10086.cn/eMobile/firstLogin_commit.action";
                    postdata = "menuid=&fieldErrFlag=&contextPath=%2FeMobile";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验是否首次登录firstLogin_commit");
                    if (httpResult.StatusCode != HttpStatusCode.OK)
                    {
                        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                }

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "校验是否首次登录成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_SendSMSAndVercode;
                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "山东移动登录异常";
                Log4netAdapter.WriteError("山东移动登录异常", e);

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

        public VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_SendSMS, mobileReq.Website);
            VerCodeRes Res = new VerCodeRes();
            Res.Token = mobileReq.Token;
            cookies = new CookieCollection();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = CacheHelper.GetCache(mobileReq.Token) as CookieCollection;
                }
                //账单查询
                httpItem = new HttpItem()
                {
                    URL = "http://www.sd.10086.cn/eMobile/checkSmsPass.action?menuid=billdetails",
                    Method = "GET",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //第二步，发手机验证码
                logDtl = new ApplyLogDtl("发手机动态验证码");
                Url = "http://www.sd.10086.cn/eMobile/sendSms.action?menuid=billdetails&pageid=0.6980846068745118";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://www.sd.10086.cn/eMobile/checkSmsPass.action?menuid=billdetails",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发手机动态验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "发手机动态验证码成功";
                appLog.LogDtlList.Add(logDtl);
                //第一步，获取验证码
                logDtl = new ApplyLogDtl("获取验证码");
                Url = "http://www.sd.10086.cn/eMobile/RandomCodeImage";
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

                Res.StatusDescription = "山东移动手机验证码发送成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "山东移动手机验证码发送异常";
                Log4netAdapter.WriteError("山东移动手机验证码发送异常", e);

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
        public BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_CheckSMS, mobileReq.Website);
            BaseRes Res = new BaseRes();
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

                //第六步，验证手机验证码
                logDtl = new ApplyLogDtl("验证手机验证码");
                Url = "http://www.sd.10086.cn/eMobile/checkSmsPass_commit.action";
                postdata = String.Format("menuid=billdetails&fieldErrFlag=&contextPath=%2FeMobile&randomSms={0}&confirmCode={1}", mobileReq.Smscode, mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Referer = "http://www.sd.10086.cn/eMobile/checkSmsPass.action?menuid=billdetails",
                    Postdata = postdata,
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                var message = HtmlParser.GetResultFromParser(httpResult.Html, "//ul[@class='errorMessage']/li/span");//错误信息
                if (message.Count > 0)
                {
                    Res.StatusDescription = message[0];
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }

                httpItem = new HttpItem()
                {
                    URL = "http://www.sd.10086.cn/eMobile/queryBillDetail.action?menuid=billdetails&pageId=1898660840",
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "验证手机验证码验证");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "山东移动手机验证码验证成功";
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
                Res.StatusDescription = "山东移动手机验证码发送异常";
                Log4netAdapter.WriteError("山东移动手机验证码发送异常", e);

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

        public BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            cookies = new CookieCollection();
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            CrawlerData crawler = new CrawlerData() { Token = mobileReq.Token, IdentityCard = mobileReq.IdentityCard, Name = mobileReq.Name, Mobile = mobileReq.Mobile, UserType = "1" };
            string Url = string.Empty;
            string monthlyStatement = string.Empty;
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                #region 个人信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Crawler, mobileReq.Website);

                #region 基本信息
                logDtl = new ApplyLogDtl("基本信息");
                httpItem = new HttpItem()
                {
                    URL = "http://www.sd.10086.cn/eMobile/qryCustInfo_result.action?menuid=customerinfo&pageid=0.6012416742787831",
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    try
                    {
                        monthlyStatement = CommonFun.GetMidStr(httpResult.Html, "月结日：", "日<br");
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

                #region 基本信息other
                logDtl = new ApplyLogDtl("基本信息other");
                httpItem = new HttpItem()
                {
                    URL = "http://www.sd.10086.cn/eMobile/updateAccount_init.action?menuid=modifyAccount&pageId=1502994010",
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "otherInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "PUK抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 积分
                logDtl = new ApplyLogDtl("积分");
                httpItem = new HttpItem()
                {
                    URL = "http://www.sd.10086.cn/eMobile/queryScore_new.action?menuid=queryScore&C0AE0B10BE8&pageid=0.2738207315596679",
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "PUK抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                #endregion

                #region PUK
                logDtl = new ApplyLogDtl("PUK");
                httpItem = new HttpItem()
                {
                    URL = "http://www.sd.10086.cn/eMobile/qryPukCode_result.action?menuid=queryPukCode&pageid=0.3113141249280743",
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "pukInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "PUK抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 月消费情况
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Crawler, mobileReq.Website);

                CrawlerBill(crawler, monthlyStatement);

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

                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "山东移动手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusDescription = "山东移动手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("山东移动手机账单抓取异常", e);

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
            Call phoneCall = null;
            Net phoneGPRS = null;
            Sms phoneSMS = null;
            string result = string.Empty;
            string hanzi = @"[\u4e00-\u9fa5]"; //汉字

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
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault() != null)
                    {
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                        if (result.Contains("returnArr"))
                        {
                            //手机基本信息
                            result = result.Replace("\r\n", "").Replace(" ", "");

                            if (result != null && !result.IsEmpty())
                            {
                                var list = jsonParser.DeserializeObject<List<string[]>>(jsonParser.GetResultFromParser(result, "returnArr"));
                                if (list.Count() >= 4)
                                {
                                    string[] re0 = list[0];
                                    mobile.PackageBrand = re0[3];//手机套餐

                                    string[] re1 = list[1];
                                    mobile.Idtype = re1[1];//证件类型
                                    mobile.Idcard = re1[3];//证件号

                                    string[] re2 = list[2];
                                    if (!String.IsNullOrWhiteSpace(re2[3]))
                                    {
                                        mobile.Regdate = DateTime.Parse(re2[3]).ToString(Consts.DateFormatString11);//入网时间
                                    }
                                    string[] re3 = list[3];
                                    var matches1 = Regex.Matches(re3[3], @"[\u4E00-\u9FFF]+");
                                    if (matches1.Count > 0)
                                    {
                                        mobile.StarLevel = matches1[0].ToString();//星级用户
                                    }
                                }
                            }
                        }

                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "基本信息解析成功";
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

                #region 基本信息other
                logDtl = new ApplyLogDtl("基本信息other");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == "otherInfor").FirstOrDefault() != null)
                    {
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "otherInfor").FirstOrDefault().CrawlerTxt);
                        mobile.Name = CommonFun.GetMidStr(CommonFun.GetMidStr(result, "账户名称</span></td><td><span class=\"floatLeft\">", ""), "", "</span>");
                        if (mobile.Name.IsEmpty() && !Regex.IsMatch(mobile.Name.Trim(), hanzi))
                        {
                            var name = HtmlParser.GetResultFromParser(result, "//input[@id='linkman']", "value");//姓名
                            if (name.Count > 0 && Regex.IsMatch(name[0].Trim(), hanzi))
                            {
                                mobile.Name = name[0];
                            }

                        }
                        var address = HtmlParser.GetResultFromParser(result, "//input[@id='address']", "value"); //地址
                        if (address.Count > 0)
                        {
                            mobile.Address = address[0];
                        }
                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "基本信息other解析成功";
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

                #region 积分
                logDtl = new ApplyLogDtl("积分");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault() != null)
                    {
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                        if (!result.IsEmpty())
                        {
                            var obj = JsonConvert.DeserializeObject(MultiKeyDES.DecryptDES(result, "1"));
                            JObject js = obj as JObject;
                            JArray bdp = js["returnArr"] as JArray;
                            if (bdp.Count > 0)
                            {
                                mobile.Integral = bdp[0][1].ToString();
                            }
                            logDtl.StatusCode = ServiceConsts.StatusCode_success;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = "积分信息解析成功";
                            appLog.LogDtlList.Add(logDtl);
                        }
                        else
                        {
                            logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = "手机套餐信息解析失败";
                            appLog.LogDtlList.Add(logDtl);
                        }
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

                #region PUK
                logDtl = new ApplyLogDtl("PUK");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == "pukInfor").FirstOrDefault() != null)
                    {
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "pukInfor").FirstOrDefault().CrawlerTxt);
                        if (!result.IsEmpty())
                        {
                            var obj = JsonConvert.DeserializeObject(result);
                            JObject js = obj as JObject;
                            JArray bdp = js["returnArr"] as JArray;
                            if (bdp.Count > 0)
                            {
                                mobile.PUK = bdp[0][1].ToString();
                                if (!mobile.PUK.IsEmpty() && mobile.PUK.Split('：').Length > 1)
                                    mobile.PUK = mobile.PUK.Split('：')[1];
                            }
                            logDtl.StatusCode = ServiceConsts.StatusCode_success;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = "PUK解析成功";
                            appLog.LogDtlList.Add(logDtl);
                        }
                        else
                        {
                            logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = "手机套餐信息解析失败";
                            appLog.LogDtlList.Add(logDtl);
                        }

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

                #region 手机套餐
                logDtl = new ApplyLogDtl("手机套餐");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault().CrawlerTxt);
                    if (!result.IsEmpty())
                    {
                        mobile.Package = result;
                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "手机套餐解析成功";
                        appLog.LogDtlList.Add(logDtl);
                    }
                    else
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "手机套餐信息解析失败";
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

                AnalysisBill(crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 话费详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.Call, crawler, delegate(List<string[]> strlist, string datevalue)
                {
                    //获取通话记录
                    strlist.ForEach(e =>
                    {
                        var totalSecond = 0;
                        var usetime = e[4].ToString();
                        if (!string.IsNullOrEmpty(usetime))
                        {
                            totalSecond = CommonFun.ConvertDate(usetime);
                        }

                        phoneCall = new Call();
                        phoneCall.StartTime = DateTime.Parse(datevalue + " " + e[0]).ToString(Consts.DateFormatString11);
                        phoneCall.SubTotal = e[7].ToDecimal().Value;
                        phoneCall.UseTime = totalSecond.ToString();
                        phoneCall.OtherCallPhone = e[3];
                        phoneCall.CallPlace = e[1];
                        phoneCall.CallType = e[5];
                        phoneCall.InitType = e[2];
                        mobile.CallList.Add(phoneCall);
                    });
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.SMS, crawler, delegate(List<string[]> strlist, string datevalue)
                {
                    //短信详单
                    strlist.ForEach(e =>
                    {
                        phoneSMS = new Sms();
                        phoneSMS.StartTime = DateTime.Parse(datevalue + " " + e[0]).ToString(Consts.DateFormatString11);
                        phoneSMS.SubTotal = e[7].ToDecimal().Value;
                        phoneSMS.InitType = e[3];
                        phoneSMS.OtherSmsPhone = e[2];
                        phoneSMS.SmsPlace = e[1];
                        phoneSMS.SmsType = e[4];
                        mobile.SmsList.Add(phoneSMS);
                    });
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.Net, crawler, delegate(List<string[]> strlist, string datevalue)
                {
                    //上网详单
                    strlist.ForEach(e =>
                    {
                        var totalSecond = 0;
                        var usetime = e[3].ToString();
                        if (!string.IsNullOrEmpty(usetime))
                        {
                            totalSecond = CommonFun.ConvertDate(usetime);
                        }
                        phoneGPRS = new Net();
                        phoneGPRS.StartTime = DateTime.Parse(datevalue + " " + e[0]).ToString(Consts.DateFormatString11);
                        phoneGPRS.SubTotal = e[8].ToDecimal().Value;
                        phoneGPRS.UseTime = totalSecond.ToString();
                        phoneGPRS.Place = e[1];
                        phoneGPRS.SubFlow = e[4];
                        phoneGPRS.SubFlow = !phoneGPRS.SubFlow.IsEmpty() ? CommonFun.ConvertGPRS(phoneGPRS.SubFlow).ToString() : "0";
                        phoneGPRS.PhoneNetType = e[2];
                        phoneGPRS.NetType = e[6];
                        mobile.NetList.Add(phoneGPRS);
                    });
                });


                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "山东移动手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);

            }
            catch (Exception e)
            {
                Res.StatusDescription = "山东移动手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("山东移动手机账单解析异常", e);

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
        private void CrawlerBill(CrawlerData crawler, string monthlyStatement)
        {
            string Url = String.Empty;
            var postdata = String.Empty;
            var startDate = String.Empty;
            var currdate = String.Empty;
            string title = String.Empty;
            bool b = false;
            DateTime now;
            DateTime date = DateTime.Now;
            String endnow;
            for (var i = 0; i <= 5; i++)
            {
                now = date.AddMonths(-i);
                startDate = new DateTime(now.Year, now.Month, monthlyStatement.ToInt(1)).ToString(Consts.DateFormatString5);
                endnow = new DateTime(now.Year, now.Month, monthlyStatement.ToInt(1)).AddMonths(1).AddDays(-1).ToString(Consts.DateFormatString5); ;
                currdate = now.ToString(Consts.DateFormatString7);
                title = now.ToString(Consts.DateFormatString7) + "月账单抓取";
                //获取本月账单
                logDtl = new ApplyLogDtl(title + "校验");
                Url = "http://www.sd.10086.cn/eMobile/queryBill_custInfo.action?pageid=0.3059107992545811";
                postdata = String.Format("menuid=queryBill&fieldErrFlag=&contextPath=%2FeMobile&feeType=&customInfo.custName=&customInfo.brandName=&customInfo.prodName=&customInfo.subsId=&cycleStartDate=&retMonth=&month={0}", currdate);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Referer = "http://www.sd.10086.cn/eMobile/checkSmsPass.action?menuid=billdetails",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, title + "校验");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                var feeType = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='feeType']", "value");
                var customInfo_custName = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='queryBill_customInfo_custName']", "value");
                var customInfo_brandName = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='queryBill_customInfo_brandName']", "value");
                var customInfo_prodName = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='queryBill_customInfo_prodName']", "value");
                var queryBill_customInfo_subsId = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='queryBill_customInfo_subsId']", "value");
                var cycleMap_cycle = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='queryBill_cycleMap_cycle_" + startDate + "']", "value");
                var cycleMap_startDate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='queryBill_cycleMap_startDate_" + startDate + "']", "value");
                var cycleMap_endDate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='queryBill_cycleMap_endDate_" + startDate + "']", "value");
                var cycleMap_acctId = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='queryBill_cycleMap_acctId_" + startDate + "']", "value");
                var cycleMap_unionacct = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='queryBill_cycleMap_unionacct_" + startDate + "']", "value");
                var cycleStartDate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='queryBill_cycleStartDate']", "value");
                var retMonth = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='retMonth']", "value");
                var month = currdate;

                if (feeType.Count == 0) { feeType.Add(""); }
                if (customInfo_custName.Count == 0) { customInfo_custName.Add(""); }
                if (customInfo_prodName.Count == 0) { customInfo_prodName.Add(""); }
                if (customInfo_brandName.Count == 0) { customInfo_brandName.Add(""); }
                if (queryBill_customInfo_subsId.Count == 0) { queryBill_customInfo_subsId.Add(""); }
                if (cycleMap_acctId.Count == 0) { cycleMap_acctId.Add(""); }
                //手机套餐
                if (customInfo_prodName.Count > 0 && !b)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "packageInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(customInfo_prodName[0]) });
                    b = true;
                }
                //获取本月账单
                logDtl = new ApplyLogDtl(title);
                Url = "http://www.sd.10086.cn/eMobile/queryBill_scheduled.action?pageid=0.4505968501695913";
                postdata = "menuid=queryBill&fieldErrFlag=&contextPath=%2FeMobile&feeType=" + feeType[0].ToUrlEncode() + "&customInfo.custName=" + customInfo_custName[0].ToUrlEncode() + "&customInfo.brandName=" + customInfo_brandName[0].ToUrlEncode() + "&customInfo.prodName=" + customInfo_prodName[0].ToUrlEncode() + "&customInfo.subsId=" + queryBill_customInfo_subsId[0] + "&cycleMap.cycle_" + startDate + "=" + startDate + "&cycleMap.startDate_" + startDate + "=" + startDate + "&cycleMap.endDate_" + startDate + "=" + endnow + "&cycleMap.acctId_" + startDate + "=" + cycleMap_acctId[0] + "&cycleMap.unionacct_" + startDate + "=0&cycleStartDate=" + startDate + "&retMonth=" + currdate + "&month=" + currdate;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Referer = "http://www.sd.10086.cn/eMobile/checkSmsPass.action?menuid=billdetails",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, title);
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = title;
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
            DateTime now;
            for (var i = 0; i <= 5; i++)
            {
                now = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(now.ToString(Consts.DateFormatString7) + "月账单解析");
                try
                {
                    PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    var TotalAmt = HtmlParser.GetResultFromParser(PhoneBillStr, "//input[@name='billFixedMap']", "value");
                    decimal planamt = 0;
                    decimal sum = 0;
                    if (TotalAmt.Count > 0)
                    {
                        for (var j = 0; j < TotalAmt.Count; j++)
                        {
                            decimal result = 0;

                            var totoal = Regex.Matches(TotalAmt[j], @"\d+(.)?.*");
                            var namebill = Regex.Matches(TotalAmt[j], @"[\u4E00-\u9FFF]+");
                            if (totoal.Count > 0)
                            {

                                decimal.TryParse(totoal[0].ToString(), out result);
                                if (namebill.Count > 0)
                                {
                                    if (namebill[0].ToString() == "套餐及固定费")
                                    {
                                        planamt = result;
                                    }
                                }
                                sum += result;
                            }
                        }
                    }
                    MonthBill biill = new MonthBill();
                    biill.BillCycle = new DateTime(now.Year, now.Month, 1).ToString(Consts.DateFormatString12);

                    biill.PlanAmt = planamt.ToString();
                    biill.TotalAmt = sum.ToString();

                    ///添加账单
                    mobile.BillList.Add(biill);
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = now.ToString(Consts.DateFormatString7) + "月账单解析异常：" + e.Message;
                    appLog.LogDtlList.Add(logDtl);
                    continue;
                }
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = now.ToString(Consts.DateFormatString7) + "月账单解析成功";
                appLog.LogDtlList.Add(logDtl);
            }
        }

        /// <summary>
        /// 抓取手机详单
        /// </summary>
        /// <param name="queryType">2 获取通话记录，4 上网详单 3 短信详单</param>
        /// <returns></returns>
        private void CrawlerDeatils(EnumMobileDeatilType type, CrawlerData crawler)
        {
            string Url = string.Empty;
            var startDate = String.Empty;
            var endDate = String.Empty;
            var month = String.Empty;
            var cycle = String.Empty;
            DateTime now;
            DateTime endnow;
            string queryType = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                queryType = "2";
            else if (type == EnumMobileDeatilType.SMS)
                queryType = "3";
            else
                queryType = "4";

            for (var i = 0; i <= 5; i++)
            {
                now = DateTime.Now.AddMonths(-i);
                startDate = new DateTime(now.Year, now.Month, 1).ToString(Consts.DateFormatString5);
                endnow = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddDays(-1);
                endDate = endnow.ToString(Consts.DateFormatString5);
                month = now.ToString(Consts.DateFormatString7);
                cycle = startDate;

                logDtl = new ApplyLogDtl(now.ToString(Consts.DateFormatString7) + "月详单抓取");

                Url = "http://www.sd.10086.cn/eMobile/%5Bobject%20HTMLInputElement%5D/queryBillDetail_detailBillAjax.action?dateType=byMonth&startDate={0}&menuid=billdetails&endDate={1}&month={2},{3}&cycle={4}&pageid=0.3376738370410276&queryType={5}";
                Url = String.Format(Url, startDate, endDate, month, month, cycle, queryType);
                //详单
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, now.ToString(Consts.DateFormatString7) + "月详单抓取");
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + "0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = now.ToString(Consts.DateFormatString7) + "详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
            }

        }

        /// <summary>
        /// 解析手机详单
        /// </summary>
        /// <param name="queryType">2 获取通话记录，4 上网详单 3 短信详单</param>
        /// <returns></returns>
        private void AnalysisDeatils(EnumMobileDeatilType type, CrawlerData crawler, Action<List<string[]>, string> action)
        {
            string PhoneCostStr = string.Empty;
            DateTime date;
            for (var i = 0; i <= 5; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7));
                try
                {
                    PhoneCostStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    if (!PhoneCostStr.IsEmpty())
                    {
                        object obj = JsonConvert.DeserializeObject(PhoneCostStr);
                        JObject js = obj as JObject;
                        JObject bdp = js["bdp"] as JObject;
                        JArray billdata = bdp["billdata"] as JArray;
                        List<string> uidlist = new List<string>();
                        if (billdata == null) continue;
                        for (int j = 0; j < billdata.Count; j++)
                        {
                            JObject bill = billdata[j] as JObject;
                            List<string[]> strlist = JsonConvert.DeserializeObject<List<string[]>>(bill["strlist"].ToString());
                            var datevalue = bill["datevalue"].ToString();
                            action(strlist, datevalue);
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
