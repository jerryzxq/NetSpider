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
namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class GX : IMobileCrawler
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
        string spid = string.Empty;
        string RelayState = string.Empty;
        string backurl = string.Empty;
        string errorurl = string.Empty;
        string SAMLart = string.Empty;
        string busi_code_uni = string.Empty;
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
            List<string> results = new List<string>();
            try
            {
                logDtl = new ApplyLogDtl("初始化登录页面");
                Url = "https://gx.ac.10086.cn/login";
                httpItem = new HttpItem()
                {
                    URL = Url
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "初始化登录页面");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                RelayState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value")[1];
                spid = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='spid']", "value")[0];
                CacheHelper.SetCache("spid", spid);
                backurl = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='backurl']", "value")[0];
                CacheHelper.SetCache("backurl", backurl);
                errorurl = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='errorurl']", "value")[0];
                CacheHelper.SetCache("errorurl", errorurl);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "初始化登录页面成功";
                appLog.LogDtlList.Add(logDtl);

                logDtl = new ApplyLogDtl("获取验证码");
                Url = "https://gx.ac.10086.cn/common/image.jsp";
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
                Res.StatusDescription = "广西移动初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(Res.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "广西移动初始化异常";
                Log4netAdapter.WriteError("广西移动初始化异常", e);

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
            string errorMsg = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                    spid = CacheHelper.GetCache("spid").ToString();
                    backurl = CacheHelper.GetCache("backurl").ToString();
                    errorurl = CacheHelper.GetCache("errorurl").ToString();
                }

                logDtl = new ApplyLogDtl("登录");
                Url = "https://gx.ac.10086.cn/Login";
                postdata = string.Format("type=B&backurl={0}&errorurl={1}&spid={2}&RelayState={3}&mobileNum={4}&servicePassword={5}&smsValidCode=&validCode={6}&isValidateCode=1", backurl, errorurl, spid, RelayState, mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "https://gx.ac.10086.cn/login",
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
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\");");
                if (errorMsg == "尊敬的客户，您输入的验证码错误，请您确认后重试。" || errorMsg.Contains("10086")
                    || errorMsg.Contains("尊敬的客户，您输入的服务密码错误，请您确认手机号码、服务密码后重试") || errorMsg.Contains("密码解锁"))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(errorMsg);

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                SAMLart = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value")[0];
                RelayState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value")[0];

                Url = "https://gx.ac.10086.cn/4logingx/backPage.jsp";
                postdata = string.Format("RelayState={0}&SAMLart={1}&displayPic=1", RelayState, SAMLart);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "https://gx.ac.10086.cn/Login",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录校验backPage");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.gx.10086.cn/wodeyidong/";
                postdata = @"SAMLart={0}&RelayState=type%3DA%3Bbackurl%3Dhttp%3A%2F%2Fwww
.gx.10086.cn%2Fwodeyidong%2Flogin.jsp%3Bnl%3D3%3BloginFrom%3Dhttp%3A%2F%2Fwww.gx.10086.cn%2Fwodeyidong
%2Flogin.jsp&myaction=http%3A%2F%2Fwww.gx.10086.cn%2Fwodeyidong%2Flogin.jsp&netaction=http%3A%2F%2Fwww
.gx.10086.cn%2Fpadhallclient%2Fnetclient%2Fcustomer%2FbusinessDealing";
                postdata = string.Format(postdata, SAMLart);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录校验wodeyidong1");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.gx.10086.cn/wodeyidong";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Referer = "http://www.gx.10086.cn/wodeyidong/login.jsp",
                    //Allowautoredirect = false,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录校验wodeyidong2");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_SendSMS;
                CacheHelper.SetCache(mobileReq.Token, cookies);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "广西移动登录成功";
                appLog.LogDtlList.Add(logDtl);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "广西移动登录异常";
                Log4netAdapter.WriteError("广西移动登录异常", e);

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
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                logDtl = new ApplyLogDtl("发手机验证码");
                Url = "http://www.gx.10086.cn/wodeyidong/mymob/xiangdan.jsp";
                postdata = "busiNum=QDCX";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='busi_code_uni']", "value");
                if (results.Count > 0)
                {
                    busi_code_uni = results[0];
                    CacheHelper.SetCache("busi_code_uni", busi_code_uni);
                }
                Url = "http://www.gx.10086.cn/wodeyidong/public/ColorCloudAction/queryCloudOpen.action";
                postdata = string.Format("ajaxType=json&_tmpDate={1}&_menuId={0}&_buttonId=", busi_code_uni, gettime().ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.gx.10086.cn/wodeyidong/ecrm/queryDetailInfo/QueryDetailInfoAction/initBusi.menu";
                postdata = string.Format("is_first_render=true&_menuId={0}&=&_zoneId=busimain&_tmpDate={1}&_buttonId=", busi_code_uni, gettime().ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://www.gx.10086.cn/wodeyidong/mymob/xiangdan.jsp",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                Url = "http://www.gx.10086.cn/wodeyidong/ecrm/queryDetailInfo/QueryDetailInfoAction/sendSecondPsw.menu";
                postdata = string.Format("ajaxType=json&_tmpDate={1}&_menuId={0}&_buttonId=", busi_code_uni, gettime().ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://www.gx.10086.cn/wodeyidong/mymob/xiangdan.jsp",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.StatusDescription = "广西移动手机验证码发送成功";
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
                Res.StatusDescription = "广西移动手机验证码发送异常";
                Log4netAdapter.WriteError("广西移动手机验证码发送异常", e);

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
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                logDtl = new ApplyLogDtl("验证手机验证码");
                Url = string.Format("http://www.gx.10086.cn/wodeyidong/ecrm/queryDetailInfo/QueryDetailInfoAction/checkSecondPsw.menu");
                postdata = string.Format("input_random_code={0}&input_svr_pass={1}&is_first_render=true&_zoneId=_sign_errzone&_tmpDate={2}&_menuId=2014101000001106&_buttonId=other_sign_btn", mobileReq.Smscode, mobileReq.Password, gettime().ToUrlEncode().Replace("%20", "+"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://www.gx.10086.cn/wodeyidong/mymob/xiangdan.jsp",
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//p[@class='ui-tipbox-explain']", "");
                if (results.Count > 0 && !results[0].Contains("同意"))
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.gx.10086.cn/wodeyidong/ecrm/queryDetailInfo/QueryDetailInfoAction/initBusi.menu";
                busi_code_uni = CacheHelper.GetCache("busi_code_uni").ToString();
                postdata = string.Format("_DELAY_INIT_HANDLER=true&is_first_render=true&_zoneId=busimain&_tmpDate={0}&_menuId={1}&_buttonId=other_sign_btn", gettime().ToUrlEncode(), busi_code_uni);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    Referer = "http://www.gx.10086.cn/wodeyidong/mymob/xiangdan.jsp",
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

                Res.StatusDescription = "广西移动手机验证码验证成功";
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
                Res.StatusDescription = "广西移动手机验证码验证异常";
                Log4netAdapter.WriteError("广西移动手机验证码验证异常", e);

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
            List<string> results = new List<string>();
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                #region  ========详单抓取========
                #region  通话详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.Call, crawler, mobileReq);  //通话

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.Net, crawler, mobileReq);//上网查询

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.SMS, crawler, mobileReq);//短/彩信详单

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion
                #endregion

                #region 个人信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Crawler, mobileReq.Website);

                #region  基本信息
                logDtl = new ApplyLogDtl("基本信息");
                Url = "http://www.gx.10086.cn/wodeyidong/mymob/zdcx.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Referer = "http://www.gx.10086.cn/wodeyidong/",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "nameInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "基本信息抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                else
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "基本信息抓取失败：" + Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                #endregion

                #region  PUK码
                logDtl = new ApplyLogDtl("PUK");
                Url = "http://www.gx.10086.cn/wodeyidong/mymob/querypuk.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://www.gx.10086.cn/wodeyidong/",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "PUK抓取校验");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='busi_code_uni']", "value");
                string _menuId = string.Empty;
                if (results.Count > 0)
                {
                    _menuId = results[0];
                }
                Url = "http://www.gx.10086.cn/wodeyidong/ecrm/querypuk/QueryPukAction/initBusi.menu";
                postdata = string.Format("is_first_render=true&_menuId={1}&=&_zoneId=busimain&_tmpDate={0}&_buttonId=", gettime().ToUrlEncode().Replace("%20", "+"), _menuId);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postdata,
                    Method = "post",
                    CookieCollection = cookies,
                    Referer = "http://www.gx.10086.cn/wodeyidong/",
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
                else
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "PUK抓取失败：" + Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region  账单查询
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Crawler, mobileReq.Website);

                CrawlerBill(crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                //保存
                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "广西移动手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "广西移动手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("广西移动手机账单抓取异常", e);

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
            string Url = string.Empty;
            string postdata = string.Empty;
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

                #region  基本信息
                logDtl = new ApplyLogDtl("基本信息");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "nameInfor").FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//div[@class='ui-box-content']/div");
                    foreach (string item in results)
                    {
                        if (item.Contains("姓名"))
                            mobile.Name = HtmlParser.GetResultFromParser(item, "/span")[0];
                        if (item.Contains("套餐名称"))
                            mobile.Package = HtmlParser.GetResultFromParser(item, "/span")[0];
                        if (item.Contains("用户星级"))
                            mobile.StarLevel = HtmlParser.GetResultFromParser(item, "//span")[0];
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

                #region PUK
                logDtl = new ApplyLogDtl("PUK");
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "PUKInfor").FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//fieldset/div[2]/label[2]", "");
                    if (results.Count > 0)
                    {
                        mobile.PUK = results[0].ToTrim("\r").ToTrim("\n").ToTrim("\t");
                    }
                    results = HtmlParser.GetResultFromParser(result, "//fieldset/div[7]/label[2]", "");
                    if (results.Count > 0)
                    {
                        mobile.Regdate = DateTime.Parse(results[0].ToTrim("\r").ToTrim("\n").ToTrim("\t")).ToString(Consts.DateFormatString11);
                    }
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "PUK解析成功";
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

                #region  解析账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Analysis, mobileReq.Website);

                AnalysisBill(crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region  ========详单解析========

                #region  通话详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.Call, crawler, mobile);  //通话

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.SMS, crawler, mobile);//短/彩信详单

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.Net, crawler, mobile);//上网查询

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #endregion

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "广西移动手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "广西移动手机账单解析异常";
                Log4netAdapter.WriteError("广西移动手机账单解析异常", e);

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

        private void CrawlerBill(CrawlerData crawler)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            DateTime date = DateTime.Now;
            string yearMonth = string.Empty;
            string title = string.Empty;

            logDtl = new ApplyLogDtl("账单抓取校验");
            Url = "http://www.gx.10086.cn/wodeyidong/ecrm/querymonthbill/QueryMonthBillAction/initBusi.menu";
            postdata = string.Format("is_first_render=true&_menuId=2014101000001105&=&_zoneId=busimain&_tmpDate={0}&_buttonId=", getTime());
            httpItem = new HttpItem()
            {
                URL = Url,
                Method = "POST",
                Postdata = postdata,
                CookieCollection = cookies,
                Referer = "http://www.gx.10086.cn/wodeyidong/mymob/zdcx.jsp",
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "账单抓取校验");
            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

            for (int i = 1; i < 6; i++)
            {
                yearMonth = date.AddMonths(-i).ToString("yyyyMM");
                title = date.AddMonths(-i).ToString(Consts.DateFormatString7) + "月账单抓取";
                logDtl = new ApplyLogDtl(title);
                Url = "http://www.gx.10086.cn/wodeyidong/ecrm/querymonthbill/QueryMonthBillAction/queryBusi.menu";
                string postdatatemp = @"begin={0}-{1}&queryDetail=yes&isMyMob=true&month_type_value={4}&query_type_value=1&month_selected_type={2}&query_type_selected=1&is_first_render=true&_zoneId=step-2-container&_tmpDate={3}&_menuId=2014101000001105&_buttonId=queryBtn";
                postdata = string.Format(postdatatemp, DateTime.Now.Year, DateTime.Now.ToString("MM"), yearMonth, getTime(), yearMonth);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "http://www.gx.10086.cn/wodeyidong/mymob/zdcx.jsp",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, title);
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                try
                {
                    if ((HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='user_info'][1]/tr")).Count == 0)
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = title + "失败";
                        appLog.LogDtlList.Add(logDtl);
                        continue;
                    }
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
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

        /// <summary>
        /// 解析账单
        /// </summary>
        /// <param name="crawler"></param>
        /// <param name=ServiceConsts.SpiderType_Mobile></param>
        private void AnalysisBill(CrawlerData crawler, Basic mobile)
        {
            MonthBill bill = null;
            DateTime date;
            string title = string.Empty;
            string PhoneBillStr = string.Empty;
            List<string> results = new List<string>();

            for (int i = 0; i < 5; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                title = date.ToString(Consts.DateFormatString7) + "月账单解析";
                logDtl = new ApplyLogDtl(title);
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault() == null) continue;
                    PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    bill = new MonthBill();
                    results = HtmlParser.GetResultFromParser(PhoneBillStr, "//table[@class='user_info'][1]/tr");
                    if (results.Count == 0)
                        continue;
                    bill = new MonthBill();
                    bill.BillCycle = date.ToString(Consts.DateFormatString12);
                    results = HtmlParser.GetResultFromParser(PhoneBillStr, "//table[@class='charge_info'][1]/tr");
                    foreach (string item in results)
                    {
                        if (item.Contains("套餐"))
                        {
                            List<string> planamt = HtmlParser.GetResultFromParser(item, "//td");
                            if (planamt.Count == 2)
                                bill.PlanAmt = CommonFun.GetMidStr(HtmlParser.GetResultFromParser(item, "//td")[1].ToTrim("\r").ToTrim("\t").ToTrim("\n"), "￥", "");
                        }
                        if (item.Contains("合计"))
                            bill.TotalAmt = CommonFun.GetMidStr(HtmlParser.GetResultFromParser(item, "//td")[1].ToTrim("\r").ToTrim("\t").ToTrim("\n"), "￥", "");
                    }
                    mobile.BillList.Add(bill);
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = title + "异常：" + e.Message;
                    appLog.LogDtlList.Add(logDtl);
                    continue;
                }
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = title + "成功";
                appLog.LogDtlList.Add(logDtl);
            }
        }

        /// <summary>
        /// 抓取手机详单
        /// </summary>
        /// <param name="queryType">3:通话；4:上网；5:短信</param>
        /// <returns></returns>
        private void CrawlerDeatils(EnumMobileDeatilType type, CrawlerData crawler, MobileReq mobileReq)
        {
            DateTime date = DateTime.Now;
            string startDateTime = string.Empty;
            string endDateTime = string.Empty;
            string month = string.Empty;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string queryType = string.Empty;
            string title = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                queryType = "1";
            else if (type == EnumMobileDeatilType.SMS)
                queryType = "3";
            else if (type == EnumMobileDeatilType.Net)
                queryType = "2";
            //循环月份
            for (int i = 0; i <= 5; i++)
            {
                int httpcou = 0;
                int iPage = 1;
                int totalPage = 1;
                startDateTime = (new DateTime(date.Year, date.Month, 1).AddMonths(-i)).ToString("yyyy-MM-dd");
                endDateTime = (new DateTime(date.Year, date.Month, 1).AddMonths(-i).AddMonths(1).AddDays(-1)).ToString("yyyy-MM-dd");
                month = (new DateTime(date.Year, date.Month, 1).AddMonths(-i).AddMonths(1).AddDays(-1)).ToString("yyyy-MM");
                title = DateTime.Parse(startDateTime).ToString(Consts.DateFormatString7) + "月详单抓取";

                busi_code_uni = CacheHelper.GetCache("busi_code_uni").ToString();
                while (true)
                {
                    logDtl = new ApplyLogDtl(title);
                    Url = string.Format("http://www.gx.10086.cn/wodeyidong/ecrm/queryDetailInfo/QueryDetailInfoAction/queryDetailDoPage.menu");
                    postdata = string.Format("queryMonth={0}&queryType=2&detailType={1}&oldPhoneId=&oldRegId=&svnsn=&bankflag=&start_time={2}&end_time={3}&iPage={8}&queryTypeList=2&queryMonthList={4}&queryDetailTypeList={5}&linkType=5&iPara=&is_first_render=true&_zoneId=queryResult&_tmpDate={6}&_menuId={7}&_buttonId=", month, queryType, startDateTime, endDateTime, month, queryType, gettime().ToUrlEncode().Replace("%20", "+"), busi_code_uni, iPage);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        Referer = "http://www.gx.10086.cn/wodeyidong/mymob/xiangdan.jsp",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtmlLog(httpItem, appLog, title);
                    if (httpResult.StatusCode != HttpStatusCode.OK)
                    {
                        httpcou++;
                        continue;
                    }
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                    httpcou = 0;
                    try
                    {
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@class='ui-paging-bold']", "");
                        if (results.Count > 0)//当有分页时，获取分页的总页数
                        {
                            totalPage = int.Parse(CommonFun.GetMidStr(results[0], "/", ""));
                        }
                        crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + startDateTime + iPage, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = title + "第" + iPage + "页成功";
                        appLog.LogDtlList.Add(logDtl);

                        if (totalPage == 1)//如果只有一页数据时，跳出页数的循环
                            break;
                        iPage++;
                        if (iPage > totalPage) //如果分页
                            break;
                    }
                    catch (Exception e)
                    {
                        logDtl.StatusCode = ServiceConsts.StatusCode_error;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = title + "第" + iPage + "页异常：" + e.Message + "原始数据：" + httpResult.Html;
                        appLog.LogDtlList.Add(logDtl);
                        iPage++;

                        continue;
                    }
                }

            }
        }

        /// <summary>
        /// 解析手机详单
        /// </summary>
        /// <param name="queryType"></param>
        /// <returns></returns>
        private void AnalysisDeatils(EnumMobileDeatilType type, CrawlerData crawler, Basic mobile)
        {
            List<CrawlerDtlData> PhoneCrawlerDtls = new List<CrawlerDtlData>();
            string PhoneCostStr = string.Empty;
            DateTime date = DateTime.Parse(crawler.CrawlerDate);
            DateTime dt = date;
            string month = string.Empty;
            List<string> results = new List<string>();
            Call phoneCall;//语音
            Sms phoneSMS;//短息
            Net phoneGPRS;//上网

            //循环月数
            for (int i = 0; i <= 5; i++)
            {
                //if(i==0)
                dt = new DateTime(date.Year, date.Month, 1).AddMonths(-i);
                logDtl = new ApplyLogDtl(dt.ToString(Consts.DateFormatString7));
                try
                {
                    PhoneCrawlerDtls = crawler.DtlList.Where(x => x.CrawlerTitle.StartsWith(type + dt.ToString("yyyy-MM-dd"))).OrderBy(x => x.CrawlerTitle).ToList<CrawlerDtlData>();
                    if (PhoneCrawlerDtls != null && PhoneCrawlerDtls.Count > 0)
                    {
                        foreach (CrawlerDtlData crawleritem in PhoneCrawlerDtls)
                        {
                            PhoneCostStr = System.Text.Encoding.Default.GetString(crawleritem.CrawlerTxt);
                            if (PhoneCostStr.IsEmpty()) continue;
                            if (type == EnumMobileDeatilType.Call)//1是语音详单 2是上网详单  3是彩信详单
                            {
                                results = HtmlParser.GetResultFromParser(PhoneCostStr, "//table[@id='payTable']/tbody/tr", "");
                                if (results.Count > 0)
                                {
                                    foreach (string item in results)
                                    {
                                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);

                                        var totalSecond = 0;
                                        var usetime = tdRow[4].ToString();
                                        if (!string.IsNullOrEmpty(usetime))
                                        {
                                            totalSecond = CommonFun.ConvertDate(usetime);
                                        }

                                        phoneCall = new Call();
                                        phoneCall.StartTime = DateTime.Parse(dt.Year + "-" + tdRow[0]).ToString(Consts.DateFormatString11);
                                        phoneCall.CallPlace = tdRow[1];
                                        phoneCall.OtherCallPhone = tdRow[3];
                                        phoneCall.UseTime = totalSecond.ToString();
                                        phoneCall.CallType = tdRow[5];
                                        phoneCall.InitType = tdRow[2];
                                        phoneCall.SubTotal = tdRow[6].ToDecimal(0);
                                        mobile.CallList.Add(phoneCall);
                                    }
                                }
                            }
                            else if (type == EnumMobileDeatilType.Net)
                            {
                                results = HtmlParser.GetResultFromParser(PhoneCostStr, "//table[@id='payTable']/tbody/tr", "");

                                if (results.Count > 0)
                                {
                                    foreach (string item in results)
                                    {

                                        phoneGPRS = new Net();
                                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);

                                        var totalSecond = 0;
                                        var usetime = tdRow[4].ToString();
                                        if (!string.IsNullOrEmpty(usetime))
                                        {
                                            totalSecond = CommonFun.ConvertDate(usetime);
                                        }

                                        var totalFlow = CommonFun.ConvertGPRS(tdRow[5].ToString());

                                        phoneGPRS.StartTime = DateTime.Parse(dt.Year + "-" + tdRow[1]).ToString(Consts.DateFormatString11);
                                        phoneGPRS.Place = tdRow[2];
                                        phoneGPRS.PhoneNetType = tdRow[3];
                                        phoneGPRS.SubTotal = tdRow[6].ToDecimal(0);
                                        phoneGPRS.SubFlow = totalFlow.ToString();
                                        phoneGPRS.UseTime = totalSecond.ToString();
                                        mobile.NetList.Add(phoneGPRS);
                                    }
                                }
                            }
                            else if (type == EnumMobileDeatilType.SMS)
                            {
                                results = HtmlParser.GetResultFromParser(PhoneCostStr, "//table[@id='payTable']/tbody/tr", "");

                                if (results.Count > 0)
                                {
                                    foreach (string item in results)
                                    {
                                        phoneSMS = new Sms();
                                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                                        phoneSMS.StartTime = DateTime.Parse(dt.Year + "-" + tdRow[0]).ToString(Consts.DateFormatString11);
                                        phoneSMS.SmsPlace = tdRow[1];
                                        phoneSMS.OtherSmsPhone = tdRow[3];
                                        phoneSMS.SmsType = tdRow[4];
                                        phoneSMS.InitType = tdRow[2];
                                        phoneSMS.SubTotal = tdRow[5].ToDecimal(0);
                                        mobile.SmsList.Add(phoneSMS);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = dt.ToString(Consts.DateFormatString7) + "月详单解析异常：" + e.Message;
                    appLog.LogDtlList.Add(logDtl);

                    continue;
                }
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = dt.ToString(Consts.DateFormatString7) + "月详单解析成功";
                appLog.LogDtlList.Add(logDtl);
            }
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        private string gettime()
        {
            DateTime dt = DateTime.Now;
            string s = DateTime.Now.ToUniversalTime().ToString("r");
            s = dt.ToString("r") + dt.ToString("zzz").Replace(":", "");
            s = s.ToTrim(",");
            string[] arry = s.Split(new char[] { ' ' });
            s = arry[0] + " " + arry[2] + " " + arry[1];
            for (int i = 3; i < arry.Length; i++)
            {

                s = s + " " + arry[i];
            }
            return s;
        }

        string getTime()
        {
            string time = DateTime.Now.GetDateTimeFormats('r')[0].ToString().ToTrim(",");
            string[] time_each_list = time.Split(' ');
            time = time_each_list[0];
            for (int i = 1; i < time_each_list.Count(); i++)
            {
                if (i == 1)
                {
                    time += ("+" + time_each_list[2]);
                }
                else if (i == 2)
                {
                    time += ("+" + time_each_list[1]);
                }
                else
                {
                    time += ("+" + time_each_list[i].ToUrlEncode());
                }
            }

            return time + "%2B0800";
        }
        #endregion
    }
}
