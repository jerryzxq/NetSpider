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
using Newtonsoft.Json.Linq;
using Vcredit.NetSpider.Entity.Mongo.Log;
using Vcredit.Common.Constants;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class HN : IMobileCrawler
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

        #region 私有变量
        string url = string.Empty;
        string continueUrl = string.Empty;
        string loginTokenId = string.Empty;
        string needAttachCode = string.Empty;
        string strutstokenname = string.Empty;
        string stoken = string.Empty;
        string mode = string.Empty;

        #endregion
        public HN()
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
            string Url = string.Empty;
            List<string> results = new List<string>();
            try
            {
                logDtl = new ApplyLogDtl("初始化登录页面");
                Url = "https://www.hn.10086.cn/login.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "初始化登录页面");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='loginForm']/input[@type='hidden']", "value");

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "初始化登录页面成功";
                appLog.LogDtlList.Add(logDtl);

                //获取验证码
                logDtl = new ApplyLogDtl("获取验证码");
                Url = "https://www.hn.10086.cn/attachCode";
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

                Res.StatusDescription = "湖南移动初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;

                CacheHelper.SetCache("resultscookie", results);
                CacheHelper.SetCache(Res.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "湖南移动初始化异常";
                Log4netAdapter.WriteError("湖南移动初始化异常", e);

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
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }
                if (CacheHelper.GetCache("resultscookie") != null)
                {
                    var infos = (List<string>)CacheHelper.GetCache("resultscookie");
                    if (infos.Count > 0)
                    {
                        url = infos[0];
                        continueUrl = infos[1];
                        mode = infos[2];
                        loginTokenId = infos[3];
                        needAttachCode = infos[5];
                        strutstokenname = infos[6];
                        stoken = infos[7];
                    }
                }

                logDtl = new ApplyLogDtl("登录");
                Url = "https://www.hn.10086.cn/login/dologin.jsp";
                postdata = string.Format("mobileNum={0}&servicePWD={1}&randomNum=&attachCode={2}&rememberMobileCK=on&url={3}&continue={4}&mode={5}&loginTokenId={6}&needAttachCode={7}&struts.token.name={8}&token={9}", mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode, url.ToUrlEncode(), continueUrl.ToUrlEncode(), mode, loginTokenId, needAttachCode, strutstokenname, stoken);
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                var checkurl = HtmlParser.GetResultFromParser(httpResult.Html, "//script", "")[0].Split('=')[1].Replace("\"", "");


                Url = "https://www.hn.10086.cn" + checkurl;
                httpItem = new HttpItem()
               {
                   URL = Url,
                   CookieCollection = cookies,
                   ResultCookieType = ResultCookieType.CookieCollection
               };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录checkurl");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                var msg = CommonFun.GetMidStr(httpResult.Html, "showTip('", "');");
                if (msg != "" && !httpResult.Html.Contains(mobileReq.Mobile))
                {
                    Res.StatusDescription = msg;
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_SendSMS;

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = Res.StatusCode;
                logDtl.Description = Res.StatusDescription;
                appLog.LogDtlList.Add(logDtl);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "湖南移动登录异常";
                Log4netAdapter.WriteError("湖南移动登录异常", e);

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
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                //第五步，发手机验证码
                logDtl = new ApplyLogDtl("发手机动态验证码");
                httpItem = new HttpItem()
                {
                    URL = string.Format("https://www.hn.10086.cn/ajax/checkHnMobileNumByDb.jsp?mobileNum={0}", mobileReq.Mobile),
                    Method = "Post",
                    CookieCollection = cookies,
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
                string Str = httpResult.Html.Split(':')[1].Replace("\"", "").Replace("}", "");
                if (Str != "true")
                {
                    Res.StatusDescription = "短信验证码发送失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                httpItem = new HttpItem()
                {
                    URL = string.Format("https://www.hn.10086.cn/ajax/pwdRadomSms.jsp?mobileNum={0}&busiId=detailBill", mobileReq.Mobile),
                    Method = "Post",
                    CookieCollection = cookies,
                    Referer = "https://www.hn.10086.cn/service/fee/detailBill.jsp",
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
                Str = httpResult.Html.Split(':')[1];
                if (!Str.Contains(mobileReq.Mobile))
                {
                    Res.StatusDescription = "短信验证码发送失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                Res.StatusDescription = "湖南移动手机验证码发送成功";
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
                Res.StatusDescription = "湖南移动手机验证码发送异常";
                Log4netAdapter.WriteError("湖南移动手机验证码发送异常", e);

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
            string postdata = string.Empty;

            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                logDtl = new ApplyLogDtl("验证手机动态验证码");
                Url = "https://www.hn.10086.cn/ajax/validateBusinessRandom.jsp?random={0}&busiId=detailBill11";
                httpItem = new HttpItem()
                {
                    URL = String.Format(Url, mobileReq.Smscode),
                    Method = "post",
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
                string str = httpResult.Html.Split(':')[1];
                if (!str.Contains("false"))
                {
                    Res.StatusDescription = "短信验证出错";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.StatusDescription = "湖南移动手机验证码验证成功";
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
                Res.StatusDescription = "湖南移动手机验证码验证异常";
                Log4netAdapter.WriteError("湖南移动手机验证码验证异常", e);

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
            DateTime date = DateTime.Now;
            string infos = string.Empty;
            List<string> results = new List<string>();

            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                #region 个人信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Crawler, mobileReq.Website);

                #region 基本信息
                logDtl = new ApplyLogDtl("基本信息");
                Url = "https://www.hn.10086.cn/service/customerService/changeuserinfo.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
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
                else
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "基本信息抓取失败";
                    appLog.LogDtlList.Add(logDtl);
                }

                #endregion

                #region PUK
                logDtl = new ApplyLogDtl("PUK");
                Url = "https://www.hn.10086.cn/service/customerService/puk.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "PUK");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "PUKInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "PUK抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }

                #endregion

                #region 手机积分
                logDtl = new ApplyLogDtl("手机积分");
                Url = "https://www.hn.10086.cn/ajax/points/scoreQuery.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = "busiId=totalScore&ecbBusiCode=QPZW022&operation=query&operType=3&attr1=201501&attr2=" + date.ToString("yyyyMM") + "&startYear=201501&endYear=" + date.ToString("yyyyMM") + "&queryType=totalScore",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "手机积分");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "手机积分抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                #endregion

                #region 我的套餐
                logDtl = new ApplyLogDtl("我的套餐");

                Url = "https://www.hn.10086.cn/service/promotion/busiQuery.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = "busiId=myTaoCan&ecbBusiCode=QC00015&operation=functionQuery",
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

                #region 账单信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Crawler, mobileReq.Website);

                CrawlerBill(crawler, mobileReq);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 详单信息
                var token = string.Empty;

                var startDate = new DateTime(date.Year, date.Month, 1).ToString("yyyy-MM-dd");
                var endDate = new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
                //获取token
                Url = "https://www.hn.10086.cn/service/fee/detailBill.jsp";
                httpItem = new HttpItem()
                {
                    URL = String.Format(Url, mobileReq.Smscode),
                    Method = "post",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                token = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='token']", "value")[0];

                //获取querytoken
                Url = "https://www.hn.10086.cn/service/fee/detailBillInfo.jsp";
                postdata = string.Format("operation=query&serviceArea=&busiId=detailBill11&subId=&detailType={0}&detailTypeSe={1}&token={2}&querymonth={3}&startTime={4}&endTime={5}", "1", "语音详单".ToUrlEncode(), token, date.ToString("yyyyMM"), startDate, endDate);
                httpItem = new HttpItem()
                {
                    URL = String.Format(Url, mobileReq.Smscode),
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                var querytoken = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='token']", "value")[0];

                #region 话费详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.Call, mobileReq, crawler, querytoken);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.SMS, mobileReq, crawler, querytoken);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.Net, mobileReq, crawler, querytoken);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #endregion

                //保存
                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "湖南移动手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "湖南移动手机账单抓取异常";
                Log4netAdapter.WriteError("湖南移动手机账单抓取异常", e);

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
            string result = string.Empty;
            List<string> results = new List<string>();

            try
            {
                crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);

                #region 基本信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Analysis, mobileReq.Website);

                mobile.Token = mobileReq.Token;
                mobile.BusName = mobileReq.Name;
                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.Mobile = mobileReq.Mobile;
                mobile.UpdateTime = crawler.CrawlerDate;

                #region 用户基本信息
                logDtl = new ApplyLogDtl("基本信息");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault() != null)
                    {
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                        var infos = HtmlParser.GetResultFromParser(result, "//script[1]", "")[2];
                        if (!string.IsNullOrEmpty(infos))
                        {
                            var json = infos.Split('\'')[1];
                            var resultObject = jsonParser.GetResultFromParser(json, "resultObject");
                            mobile.Name = jsonParser.GetResultFromParser(resultObject, "PAYNAME");
                            mobile.Mobile = jsonParser.GetResultFromParser(resultObject, "SERIALNUMBER");
                            //品牌
                            mobile.PackageBrand = jsonParser.GetResultFromParser(resultObject, "CARDQUALITY");
                            mobile.Idtype = jsonParser.GetResultFromParser(resultObject, "PASSPORTTYPE");
                            mobile.Regdate = DateTime.Parse(jsonParser.GetResultFromParser(resultObject, "OPENDATE")).ToString(Consts.DateFormatString11);
                            mobile.Email = jsonParser.GetResultFromParser(resultObject, "EMAILADDRESS");
                            mobile.Address = jsonParser.GetResultFromParser(resultObject, "PASSPORTADDRESS");
                            mobile.Postcode = jsonParser.GetResultFromParser(resultObject, "HOMEPOSTCODE");
                            mobile.Idcard = jsonParser.GetResultFromParser(resultObject, "USRPID");
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

                #region 套餐
                logDtl = new ApplyLogDtl("套餐信息");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault() != null)
                    {
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault().CrawlerTxt);
                        var package = jsonParser.GetArrayFromParse(result, "resultList");
                        mobile.Package = package[0].Split('}')[0].Split(',')[3].Split(':')[1].Replace("\"", "");

                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "套餐信息解析成功";
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
                logDtl = new ApplyLogDtl("积分信息");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault() != null)
                    {
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                        var souce = jsonParser.GetResultFromParser(result, "resultObject");
                        mobile.Integral = jsonParser.GetResultFromParser(souce, "TOTAL_VALUE");
                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "积分信息解析成功";
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

                #region PUK码
                logDtl = new ApplyLogDtl("PUK");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault() != null)
                    {
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "PUKInfor").FirstOrDefault().CrawlerTxt);
                        var infos = HtmlParser.GetResultFromParser(result, "//script", "")[4];
                        if (!string.IsNullOrEmpty(infos))
                        {
                            var json = infos.Split('\'')[1];
                            var resultObject = jsonParser.GetResultFromParser(json, "resultObject");
                            mobile.PUK = jsonParser.GetResultFromParser(resultObject, "PUK");
                        }
                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "PUK解析成功";
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

                #region 账单信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Analysis, mobileReq.Website);

                AnalysisBill(crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 详单查询

                #region 话费详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.Call, crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.SMS, crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.Net, crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #endregion

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                //保存
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "湖南移动手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "湖南移动手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("湖南移动手机账单解析异常", e);

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
        private void CrawlerBill(CrawlerData crawler, MobileReq mobileReq)
        {
            string Url = String.Empty;
            var postdata = String.Empty;
            DateTime date = DateTime.Now;

            logDtl = new ApplyLogDtl("账单抓取校验");
            Url = "https://www.hn.10086.cn/service/fee/monthBill.jsp?recommendGiftPop=true";
            httpItem = new HttpItem()
            {
                URL = Url,
                Method = "get",
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "账单抓取校验");
            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
            var tokenBill = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='token']", "value");
            for (int i = 0; i < 6; i++)
            {
                date = DateTime.Now.AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月账单抓取");
                Url = string.Format("https://www.hn.10086.cn/ajax/billservice/monthBillResult.jsp?busiId=monthBill11&operation=query&startDate={0}&token={1}&zqFlag=null)", date.ToString("yyyyMM"), tokenBill[0]);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Referer = "https://www.hn.10086.cn/service/fee/monthBillResult.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, date.ToString(Consts.DateFormatString7) + "月账单抓取");
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                try
                {
                    var infos = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='feiyongTable']/tr", "");
                    if (infos.Count <= 0)
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = date.ToString(Consts.DateFormatString7) + "月账单抓取失败";
                        appLog.LogDtlList.Add(logDtl);
                        continue;
                    }
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) }); crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
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
            List<string> infos = new List<string>();
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
                    infos = HtmlParser.GetResultFromParser(PhoneBillStr, "//table[@id='feiyongTable']/tr", "");
                    if (infos.Count > 0)
                    {
                        bill = new MonthBill();
                        bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);
                        bill.PlanAmt = HtmlParser.GetResultFromParser(infos[1], "//td[2]", "")[0];
                        bill.TotalAmt = HtmlParser.GetResultFromParser(infos[9], "//td[2]", "")[0];
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
        /// <param name="queryType">1 获取通话记录，3 上网详单 2 短信详单</param>
        /// <returns></returns>
        private void CrawlerDeatils(EnumMobileDeatilType type, MobileReq mobileReq, CrawlerData crawler, string token)
        {
            string Url = string.Empty;
            var postdata = string.Empty;
            DateTime date = DateTime.Now;
            var startDate = String.Empty;
            var endDate = String.Empty;
            string queryType = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                queryType = "1";
            else if (type == EnumMobileDeatilType.SMS)
                queryType = "2";
            else
                queryType = "3";

            for (int i = 0; i <= 5; i++)
            {
                startDate = new DateTime(date.AddMonths(-i).Year, date.AddMonths(-i).Month, 1).ToString("yyyy-MM-dd");
                endDate = new DateTime(date.AddMonths(-i).Year, date.AddMonths(-i).Month, 1).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
                logDtl = new ApplyLogDtl(date.AddMonths(-i).ToString(Consts.DateFormatString7) + "月详单抓取");
                //查询详单
                Url = string.Format("https://www.hn.10086.cn/ajax/billservice/detailBillInfo.jsp?busiId=detailBill11&operation=query&month={0}&startDate={1}&endDate={2}&detailType={3}&detailBillPwd=undefined&token={4}", date.AddMonths(-i).ToString("yyyyMM"), startDate, endDate, queryType, token);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, date.AddMonths(-i).ToString(Consts.DateFormatString7) + "月详单抓取");
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + "0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) }); logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = date.AddMonths(-i).ToString(Consts.DateFormatString7) + "详单抓取成功";
                appLog.LogDtlList.Add(logDtl);

            }
        }

        /// <summary>
        /// 解析手机详单
        /// </summary>
        /// <param name="queryType">1 获取通话记录，3 上网详单 2 短信详单</param>
        /// <returns></returns>
        private void AnalysisDeatils(EnumMobileDeatilType type, CrawlerData crawler, Basic mobile)
        {
            string PhoneCostStr = string.Empty;
            DateTime date;
            for (int i = 0; i <= 5; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7));
                try
                {
                    PhoneCostStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    List<string> infos = new List<string>();
                    if (type == EnumMobileDeatilType.Call)
                    {
                        infos = HtmlParser.GetResultFromParser(PhoneCostStr, "//table[@id='tonghua']/tbody/tr[position()>0]", "inner");
                    }
                    else if (type == EnumMobileDeatilType.SMS)
                    {
                        infos = HtmlParser.GetResultFromParser(PhoneCostStr, "//table[@id='daishou']/tbody/tr[position()>0]", "inner");
                    }
                    else
                    {
                        infos = HtmlParser.GetResultFromParser(PhoneCostStr, "//table[3]/tr[position()>0]", "inner");
                        if (infos.Contains("系统忙"))
                        {
                            continue;
                        }
                    }
                    //定义年月
                    var yearandmonth = string.Empty;
                    foreach (var item in infos)
                    {
                        //短信详单
                        if (type == EnumMobileDeatilType.SMS)
                        {
                            List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                            if (tdRow.Count != 7)
                            {
                                yearandmonth = tdRow[0];
                                continue;
                            }
                            Sms sms = new Sms();
                            if (tdRow.Count > 0)
                            {
                                if (!tdRow[0].Contains("合计"))
                                {
                                    sms.StartTime = DateTime.Parse(yearandmonth + " " + tdRow[0]).ToString(Consts.DateFormatString11);   //起始时间
                                    sms.InitType = tdRow[2];  //通信方式
                                    sms.OtherSmsPhone = tdRow[1];  //对方号码
                                    sms.SmsType = tdRow[3];   //通信类型
                                    sms.SubTotal = tdRow[6].ToDecimal(0);  //总费用
                                    mobile.SmsList.Add(sms);
                                }
                            }
                        }
                        else if (type == EnumMobileDeatilType.Net)    //上网详单
                        {
                            List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                            if (tdRow[0].Contains("起始时间") || tdRow[0].Contains("合计") || tdRow.Count == 2)
                            {
                                continue;
                            }

                            if (tdRow.Count != 5)
                            {
                                yearandmonth = tdRow[0];
                                continue;
                            }
                            Net gprs = new Net();
                            if (tdRow.Count > 0)
                            {
                                gprs.StartTime = DateTime.Parse(yearandmonth + " " + tdRow[0]).ToString(Consts.DateFormatString11);
                                gprs.Place = tdRow[1];
                                gprs.NetType = tdRow[2];
                                //湖南移动流量单位为 Mb 转换成Kb*1024
                                gprs.SubFlow = (tdRow[4].ToDecimal(0) * 1024).ToString();
                                mobile.NetList.Add(gprs);
                            }
                        }
                        else      //通话详单
                        {
                            List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                            if (tdRow.Count != 8)
                            {
                                yearandmonth = tdRow[0];
                                continue;
                            }
                            Call call = new Call();
                            if (tdRow.Count > 0)
                            {

                                var totalSecond = 0;
                                var usetime = tdRow[4].ToString();
                                if (!string.IsNullOrEmpty(usetime))
                                {
                                    totalSecond = CommonFun.ConvertDate(usetime);
                                }

                                call.StartTime = DateTime.Parse(yearandmonth + " " + tdRow[0]).ToString(Consts.DateFormatString11);  //起始时间
                                call.CallPlace = tdRow[1];  //通话地点
                                call.OtherCallPhone = tdRow[3];  //对方号码
                                call.UseTime = totalSecond.ToString();  //通话时长
                                call.InitType = tdRow[2];  //通话类型
                                call.CallType = tdRow[5];  //呼叫类型
                                call.SubTotal = tdRow[7].ToDecimal(0);  //通话费用
                                mobile.CallList.Add(call);
                            }
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

        private static bool RemoteCertificateValidate(
           object sender, X509Certificate cert,
             X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }

        #endregion

    }
}
