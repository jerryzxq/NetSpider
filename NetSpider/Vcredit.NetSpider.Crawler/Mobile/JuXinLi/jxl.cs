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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using Vcredit.NetSpider.Entity.Mongo.Log;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.Common.Constants;
namespace Vcredit.NetSpider.Crawler.Mobile.JuXinLi
{
    public class jxl : IMobileCrawler, IResetMobilePassWord
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        LogHelper httpHelper = new LogHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        ApplyLogMongo logMongo = new ApplyLogMongo();
        List<ApplyLog> loglist = new List<ApplyLog>();
        ApplyLog appLog = new ApplyLog();
        ApplyLogDtl logDtl = new ApplyLogDtl();
        #endregion

        public VerCodeRes MobileInit(MobileReq mobileReq = null)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_JxlMobile, ServiceConsts.Step_Init, mobileReq.Website);
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                logDtl = new ApplyLogDtl("初始化");

                postdata = "{\"selected_website\":[],\"basic_info\":{\"name\":\"" + mobileReq.Name + "\",\"id_card_num\":\"" + mobileReq.IdentityCard + "\",\"cell_phone_num\":\"" + mobileReq.Mobile + "\"}}";
                //第一步，初始化登录页面
                Url = "https://www.juxinli.com/orgApi/rest/v2/applications/vcredit";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postdata,
                    Method = "post",
                    ContentType = "application/json;charset=utf-8",
                    Encoding = Encoding.UTF8,
                    PostEncoding = Encoding.UTF8,
                    Referer = "https://www.juxinli.com/orgApi/",
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0",
                    ResultCookieType = ResultCookieType.CookieCollection,
                    Accept = "application/json, text/plain, */*"

                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "聚信立手机接口初始化");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                object obj = JsonConvert.DeserializeObject(httpResult.Html);
                JObject js = obj as JObject;
                JObject data = js["data"] as JObject;
                if (data != null)
                {
                    var token = data["token"].ToString();
                    var website = data["datasource"]["website"].ToString();
                    Res.Token = token;
                    mobileReq.Website = Res.Website = website;
                }
                if (!Convert.ToBoolean(js["success"].ToString()))
                {
                    var message = js["message"];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    Res.StatusDescription = GetWebsiteName(mobileReq.Website) + "初始化失败:" + message.ToString();
                    return Res;
                }

                Res.StatusDescription = GetWebsiteName(mobileReq.Website) + "初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = GetWebsiteName(mobileReq.Website) + "初始化失败";
                Log4netAdapter.WriteError("聚信立手机接口初始化异常", e);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            finally
            {
                if (Res.StatusCode != ServiceConsts.StatusCode_error)
                {
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);
                }
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                logMongo.Save(appLog);
            }
            return Res;

        }

        public BaseRes MobileLogin(MobileReq mobileReq)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_JxlMobile, ServiceConsts.Step_Login, mobileReq.Website);
            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            Res.Token = mobileReq.Token;
            try
            {
                logDtl = new ApplyLogDtl("登录");
                postdata = "{\"token\":\"" + mobileReq.Token + "\",\"account\":\"" + mobileReq.Mobile + "\",\"password\":\"" + mobileReq.Password + "\",\"captcha\":\"\",\"contact1\":\"\",\"contact2\":\"\",\"contact3\":\"\",\"type\":\"\",\"website\":\"" + mobileReq.Website + "\"}";
                //第一步，初始化登录页面
                Url = "https://www.juxinli.com/orgApi/rest/v2/messages/collect/req";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postdata,
                    Method = "post",
                    Encoding = Encoding.UTF8,
                    PostEncoding = Encoding.UTF8,
                    ContentType = "application/json;charset=utf-8",
                    Referer = "https://www.juxinli.com/orgApi/",
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0",

                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "聚信立手机接口登录");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                object obj = JsonConvert.DeserializeObject(httpResult.Html);
                JObject js = obj as JObject;
                var success = Convert.ToBoolean(js["success"].ToString());
                if (!success)
                {
                    Res.StatusDescription = js["message"].ToString();
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                    return Res;
                }
                var finish = Convert.ToBoolean(js["data"]["finish"].ToString());
                var process_code = js["data"]["process_code"].ToString();
                if (process_code != "10002" && process_code != "10008")
                {
                    Res.StatusDescription = js["data"]["content"].ToString();
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                    return Res;
                }
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                if (!finish)
                {
                    Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
                    Res.StatusDescription = GetWebsiteName(mobileReq.Website) + "登录成功";
                }
                else
                {
                    Res.nextProCode = ServiceConsts.NextProCode_Query;
                    Res.StatusDescription = GetWebsiteName(mobileReq.Website) + "开始采集行为数据";
                }

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = GetWebsiteName(mobileReq.Website) + "登录失败";
                Log4netAdapter.WriteError("聚信立手机接口登录异常", e);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.Description = httpResult.Html;
                appLog.LogDtlList.Add(logDtl);
            }
            finally
            {
                if (Res.StatusCode != ServiceConsts.StatusCode_error)
                {
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);
                }
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                logMongo.Save(appLog);
            }
            return Res;
        }

        public VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_JxlMobile, ServiceConsts.Step_SendSMS, mobileReq.Website);
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            Res.Token = mobileReq.Token;
            try
            {
                logDtl = new ApplyLogDtl("动态短信发送");

                postdata = "{\"token\":\"" + mobileReq.Token + "\",\"account\":\"" + mobileReq.Mobile + "\",\"password\":\"" + mobileReq.Password + "\",\"captcha\":\"\",\"type\":\"RESEND_CAPTCHA\",\"website\":\"" + mobileReq.Website + "\"}";
                //第一步，初始化登录页面
                Url = "https://www.juxinli.com/orgApi/rest/v2//messages/collect/req";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postdata,
                    Method = "post",
                    Encoding = Encoding.UTF8,
                    PostEncoding = Encoding.UTF8,
                    ContentType = "application/json;charset=utf-8",
                    Referer = "https://www.juxinli.com/orgApi/",
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0",
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "聚信立手机接口动态短信发送");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                object obj = JsonConvert.DeserializeObject(httpResult.Html);
                JObject js = obj as JObject;
                var success = Convert.ToBoolean(js["success"].ToString());
                if (!success)
                {
                    Res.StatusDescription = "动态短信发送失败";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                    return Res;
                }

                Res.StatusDescription = "动态短信发送成功,输入动态密码";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = GetWebsiteName(mobileReq.Website) + "动态短信发送异常";
                Log4netAdapter.WriteError("聚信立手机接口动态短信发送异常", e);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            finally
            {
                if (Res.StatusCode != ServiceConsts.StatusCode_error)
                {
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);
                }
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                logMongo.Save(appLog);
            }
            return Res;
        }

        public BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_JxlMobile, ServiceConsts.Step_CheckSMS, mobileReq.Website);
            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            Res.Token = mobileReq.Token;
            try
            {
                logDtl = new ApplyLogDtl("动态短信验证");

                postdata = "{\"token\":\"" + mobileReq.Token + "\",\"account\":\"" + mobileReq.Mobile + "\",\"password\":\"" + mobileReq.Password + "\",\"captcha\":\"" + mobileReq.Smscode + "\",\"type\":\"SUBMIT_CAPTCHA\",\"website\":\"" + mobileReq.Website + "\"}";
                //第一步，初始化登录页面
                Url = "https://www.juxinli.com/orgApi/rest/v2//messages/collect/req";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postdata,
                    Method = "post",
                    Encoding = Encoding.UTF8,
                    PostEncoding = Encoding.UTF8,
                    ContentType = "application/json;charset=utf-8",
                    Referer = "https://www.juxinli.com/orgApi/",
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0",

                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "聚信立手机接口动态短信验证");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                object obj = JsonConvert.DeserializeObject(httpResult.Html);
                JObject js = obj as JObject;
                var success = Convert.ToBoolean(js["success"].ToString());
                if (!success && js["message"].ToString() != "抓取完成")
                {
                    Res.StatusDescription = js["message"].ToString();
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                    return Res;
                }
                var finish = Convert.ToBoolean(js["data"]["finish"].ToString());
                var process_code = js["data"]["process_code"].ToString();
                if (process_code != "10008" && js["data"]["content"].ToString() != "抓取完成")
                {
                    Res.StatusDescription = js["data"]["content"].ToString();
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                    return Res;
                }
                if (!finish)
                {
                    Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
                    Res.StatusDescription = GetWebsiteName(mobileReq.Website) + "校验成功";
                }
                else
                {
                    Res.nextProCode = ServiceConsts.NextProCode_Query;
                    Res.StatusDescription = GetWebsiteName(mobileReq.Website) + "开始采集行为数据";
                }

                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                //Res.nextProCode = ServiceConsts.NextProCode_Query;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = GetWebsiteName(mobileReq.Website) + "动态短信验证失败";
                Log4netAdapter.WriteError("聚信立手机接口动态短信验证异常", e);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.Description = httpResult.Html;
                appLog.LogDtlList.Add(logDtl);
            }
            finally
            {
                if (Res.StatusCode != ServiceConsts.StatusCode_error)
                {
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);
                }
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                logMongo.Save(appLog);
            }
            return Res;
        }

        public BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {
            throw new NotImplementedException();
        }

        public BaseRes MobileAnalysis(MobileReq mobileReq, DateTime appDate)
        {
            throw new NotImplementedException();
        }

        public VerCodeRes ResetInit(MobileReq mobileReq)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_JxlMobileReset, ServiceConsts.Step_ResetInit, mobileReq.Website);
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                logDtl = new ApplyLogDtl("手机重置密码初始化");

                postdata = "{\"selected_website\":[],\"basic_info\":{\"name\":\"" + mobileReq.Name + "\",\"id_card_num\":\"" + mobileReq.IdentityCard + "\",\"cell_phone_num\":\"" + mobileReq.Mobile + "\"}}";
                //第一步，初始化页面
                Url = "https://www.juxinli.com/orgApi/rest/v2/applications/vcredit";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postdata,
                    Method = "post",
                    ContentType = "application/json;charset=utf-8",
                    Encoding = Encoding.UTF8,
                    PostEncoding = Encoding.UTF8,
                    Referer = "https://www.juxinli.com/orgApi/",
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0",
                    ResultCookieType = ResultCookieType.CookieCollection,
                    Accept = "application/json, text/plain, */*"

                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "聚信立手机重置密码接口初始化");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                object obj = JsonConvert.DeserializeObject(httpResult.Html);
                JObject js = obj as JObject;
                JObject data = js["data"] as JObject;
                if (data != null)
                {
                    var token = data["token"].ToString();
                    var website = data["datasource"]["website"].ToString();
                    Res.Token = token;
                    mobileReq.Website = Res.Website = website;
                }
                if (!Convert.ToBoolean(js["success"].ToString()))
                {
                    var message = js["message"];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    Res.StatusDescription = GetWebsiteName(mobileReq.Website) + "手机重置密码初始化失败:" + message.ToString();
                    return Res;
                }
                string reset_pwd_method = data["datasource"]["reset_pwd_method"].ToString();
                if (reset_pwd_method == "1")
                {
                    Res.StatusCode = ServiceConsts.CrawlerStatusCode_SmsCodeReset;
                    Res.StatusDescription = "短信验证码重置";
                }
                else if (reset_pwd_method == "2")
                {
                    Res.StatusCode = ServiceConsts.CrawlerStatusCode_SmsServiceCodeReset;
                    Res.StatusDescription = "短信验证码和服务密码重置";
                }
                else if (reset_pwd_method == "3")
                {
                    Res.StatusCode = ServiceConsts.CrawlerStatusCode_SmsServiceCodeOtherReset;
                    Res.StatusDescription = "短信验证码和服务密码和3个5天以前一个月以内有通话记录的手机号重置";
                }
                else
                {
                    Res.StatusCode = ServiceConsts.CrawlerStatusCode_NoReset;
                    Res.StatusDescription = "不支持密码重置";
                }
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = GetWebsiteName(mobileReq.Website) + "手机重置密码初始化失败";
                Log4netAdapter.WriteError("聚信立手机重置密码接口初始化异常", e);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            finally
            {
                if (Res.StatusCode != ServiceConsts.StatusCode_error)
                {
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);
                }
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                logMongo.Save(appLog);
            }
            return Res;

        }

        public VerCodeRes ResetSendSms(MobileReq mobileReq)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_JxlMobileReset, ServiceConsts.Step_ResetSendSMS, mobileReq.Website);
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            Res.Token = mobileReq.Token;
            try
            {
                logDtl = new ApplyLogDtl("手机密码重置发送动态验证码");
                postdata = "{\"token\":\"" + mobileReq.Token + "\",\"account\":\"" + mobileReq.Mobile + "\",\"password\":\"" + mobileReq.Password + "\",\"captcha\":\"\",\"type\":\"SUBMIT_RESET_PWD\",\"website\":\"" + mobileReq.Website + "\"}";
                //第一步，初始化登录页面
                Url = "https://www.juxinli.com/orgApi/rest/v2/messages/reset/req";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postdata,
                    Method = "post",
                    Encoding = Encoding.UTF8,
                    PostEncoding = Encoding.UTF8,
                    ContentType = "application/json;charset=utf-8",
                    Referer = "https://www.juxinli.com/orgApi/",
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0",
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "聚信立手机密码重置发送动态验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                object obj = JsonConvert.DeserializeObject(httpResult.Html);
                JObject js = obj as JObject;
                var success = Convert.ToBoolean(js["success"].ToString());
                if (!success)
                {
                    Res.StatusDescription = js["message"].ToString();
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                var process_code = js["data"]["process_code"].ToString();
                if (process_code != "10008" && process_code != "10002")
                {
                    Res.StatusDescription = js["data"]["content"].ToString();
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Res.StatusDescription = "发送动态验证码成功";
                Res.StatusCode = ServiceConsts.CrawlStatus_Success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = GetWebsiteName(mobileReq.Website) + "手机密码重置发送动态验证码失败";
                Log4netAdapter.WriteError("聚信立手机密码重置发送动态验证码异常", e);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            finally
            {
                if (Res.StatusCode != ServiceConsts.StatusCode_error)
                {
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);
                }
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                logMongo.Save(appLog);
            }
            return Res;
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes ResetPassWord(MobileReq mobileReq)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_JxlMobileReset, ServiceConsts.Step_ResetCheck, mobileReq.Website);
            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            Res.Token = mobileReq.Token;
            try
            {
                logDtl = new ApplyLogDtl("手机密码重置");
                if (mobileReq.Website == "chinamobilegd")
                {
                    postdata = "{\"token\":\"" + mobileReq.Token + "\",\"account\":\"" + mobileReq.Mobile + "\",\"password\":\"" + mobileReq.Password + "\",\"captcha\":\"" + mobileReq.Smscode + "\",\"type\":\"SUBMIT_RESET_PWD\",\"website\":\"" + mobileReq.Website + "\"}";
                }
                else
                {
                    postdata = "{\"token\":\"" + mobileReq.Token + "\",\"account\":\"" + mobileReq.Mobile + "\",\"password\":\"" + mobileReq.Password + "\",\"captcha\":\"" + mobileReq.Smscode + "\",\"type\":\"SUBMIT_RESET_PWD\",\"website\":\"" + mobileReq.Website + "\"}";
                }
                //第一步，初始化登录页面
                Url = "https://www.juxinli.com/orgApi/rest/v2/messages/reset/req";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postdata,
                    Method = "post",
                    Encoding = Encoding.UTF8,
                    PostEncoding = Encoding.UTF8,
                    ContentType = "application/json;charset=utf-8",
                    Referer = "https://www.juxinli.com/orgApi/",
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0",
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "聚信立手机密码重置");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                object obj = JsonConvert.DeserializeObject(httpResult.Html);
                JObject js = obj as JObject;
                var success = Convert.ToBoolean(js["success"].ToString());
                if (!success)
                {
                    Res.StatusDescription = js["message"].ToString();
                    Res.StatusCode = ServiceConsts.CrawlerStatusCode_ResetPWDFail;
                    return Res;
                }
                var process_code = js["data"]["process_code"].ToString();
                if (process_code != "11000")
                {
                    Res.StatusDescription = js["data"]["content"].ToString();
                    Res.StatusCode = ServiceConsts.CrawlerStatusCode_ResetPWDFail;
                    return Res;
                }

                Res.StatusDescription = GetWebsiteName(mobileReq.Website) + "手机密码重置成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_ResetPWDSuccess;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = GetWebsiteName(mobileReq.Website) + "手机密码重置失败";
                Log4netAdapter.WriteError("聚信立手机密码重置异常", e);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            finally
            {
                if (Res.StatusCode != ServiceConsts.StatusCode_error)
                {
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);
                }
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                logMongo.Save(appLog);
            }
            return Res;
        }

        /// <summary>
        /// 获取采集网站中文名
        /// </summary>
        /// <param name="website">运行商</param>
        /// <returns></returns>
        private string GetWebsiteName(string website)
        {
            if (string.IsNullOrEmpty(website))
            {
                throw new NotImplementedException("采集网站不能为空！");
            }
            string region = string.Empty;
            if (website.Contains("chinaunicom"))
            {
                region = "中国联通";
            }
            else if (website.Contains("chinamobile"))
            {
                region = CommonFun.GetProvinceName(website.Substring(website.Length - 2).ToUpper()) + "移动";
            }
            else if (website.Contains("chinatelecom"))
            {
                region = CommonFun.GetProvinceName(website.Substring(website.Length - 2).ToUpper()) + "电信";
            }
            return region;
        }

    }
}

