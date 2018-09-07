using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Mongo.Log;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.Entity.Service.Mobile;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaMobile
{
    public class SX : IMobileCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        ApplyLogMongo logMongo = new ApplyLogMongo();
        #endregion
        #region 私有变量
        private string cookieStr = string.Empty;
        #endregion
        /// <summary>
        /// 页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes MobileInit(MobileReq mobileReq = null)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            List<string> results = new List<string>();
            string spid = string.Empty;
            try
            {
                Url = "https://sx.ac.10086.cn/login";
                httpItem = new HttpItem()
                {
                    URL = Url
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //基础连接已经关闭: 发送时发生错误。
                if (httpResult.Html == "基础连接已经关闭: 发送时发生错误。")
                {
                    Res.StatusDescription = "页面加载错误,请刷新后重试";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew("", httpResult.Cookie);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='spid']", "value");
                if (results.Count > 0)
                {
                    spid = results[0];
                }
                Url = "https://sx.ac.10086.cn/common/image.jsp?l=0.41641839899490696";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "https://sx.ac.10086.cn/login",
                    ResultType = ResultType.Byte,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                //Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                //Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                Res.nextProCode = ServiceConsts.NextProCode_Login;
                Res.StatusDescription = "山西移动初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("spid", spid);
                dic.Add("cookieStr", cookieStr);
                CacheHelper.SetCache(token, dic);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "山西移动初始化异常";
                Log4netAdapter.WriteError("山西移动初始化异常", e);
                appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_Init)
                {
                    CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                    StatusCode = ServiceConsts.StatusCode_error,
                    Description = e.Message
                });
            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                if (appLog.LogDtlList.Count < 1)
                {
                    appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_Init)
                    {
                        CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                        StatusCode = Res.StatusCode,
                        Description = Res.StatusDescription
                    });
                }
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
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string spid = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    Dictionary<string, object> dic = (Dictionary<string, object>)CacheHelper.GetCache(mobileReq.Token);
                    cookieStr = dic["cookieStr"].ToString();
                    spid = dic["spid"].ToString();
                }
                //校验参数
                if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登陆
                Url = string.Format("https://sx.ac.10086.cn/common/getYzm.jsp?callback=jQuery1800004496308630765111_1442991270345&catType=1&validCode={0}&_=1442991285934", mobileReq.Vercode);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "https://sx.ac.10086.cn/login",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "([{\"yzm\":\"", "\"}])");
                if (errorMsg.ToLower() == "n")
                {
                    Res.StatusDescription = "验证码不正确！";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                    return Res;
                }
                Url = "https://sx.ac.10086.cn/Login";
                postdata = string.Format("type=B&backurl=https%3A%2F%2Fsx.ac.10086.cn%2F4login%2FbackPage.jsp&errorurl=https%3A%2F%2Fsx.ac.10086.cn%2F4login%2FerrorPage.jsp&spid={3}&RelayState=type%3DB%3Bbackurl%3Dhttp%3A%2F%2Fservice.sx.10086.cn%2Fmy%2F%3Bnl%3D3%3BloginFrom%3Dhttp%3A%2F%2Fwww.10086.cn%2Fsx%2Findex_351_351.html&webPassword=&mobileNum={0}&displayPic=&isValidateCode=&mobileNum_temp={0}&servicePassword={1}&smsValidCode=%C7%EB%CA%E4%C8%EB%B6%CC%D0%C5%C3%DC%C2%EB&validCode={2}&remPwd1=", mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode, spid);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "https://sx.ac.10086.cn/login",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (!httpResult.Html.Contains("SAMLart"))
                {
                    Res.StatusDescription = "手机号或密码输入不正确";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                string SAMLart = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value");
                if (results.Count > 0)
                {
                    SAMLart = results[0];
                }
                string displayPic = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='displayPic']", "value");
                if (results.Count > 0)
                {
                    displayPic = results[0];
                }
                string RelayState = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value");
                if (results.Count > 0)
                {
                    RelayState = results[0];
                }
                Url = "https://sx.ac.10086.cn/4login/backPage.jsp";
                postdata = string.Format("SAMLart={0}&displayPic={1}&RelayState={2}", SAMLart, displayPic, RelayState.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "https://sx.ac.10086.cn/Login",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                SAMLart = CommonFun.GetMidStr(httpResult.Html, "onLoad=\"parent.callBackurl('", "')");

                Url = "http://service.sx.10086.cn/my/";
                postdata = string.Format("SAMLart={0}&RelayState={1}", SAMLart, (RelayState.ToUrlDecode()).ToUrlEncode());
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie) + string.Format("CmProvid=sx;I4WU={0},sx;", mobileReq.Mobile);
                string loginType = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='loginType']", "value");
                if (results.Count > 0)
                {
                    loginType = results[0];
                }
                string jumpMenu = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='jumpMenu']", "value");
                if (results.Count > 0)
                {
                    jumpMenu = results[0];
                }
                string phoneNo = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='phoneNo']", "value");
                if (results.Count > 0)
                {
                    phoneNo = results[0];
                }
                string loginPasswordType = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='loginPasswordType']", "value");
                if (results.Count > 0)
                {
                    loginPasswordType = results[0];
                }
                string returl = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='returl']", "value");
                if (results.Count > 0)
                {
                    returl = results[0].ToUrlEncode();
                }
                //Url = "http://service.sx.10086.cn/my/hfcx.html";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Host = "service.sx.10086.cn",
                //    Cookie = cookieStr
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                //cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                //
                Url = "http://service.sx.10086.cn/login/toLoginSso.action";
                postdata = string.Format("loginType={0}&jumpMenu={1}&phoneNo={2}&loginPasswordType={3}&returl={4}", loginType, jumpMenu, phoneNo, loginPasswordType, returl);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://service.sx.10086.cn/my/",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Query;//NextProCode_SendSMSAndVercode
                CacheHelper.SetCache(mobileReq.Token, cookieStr);
                #endregion
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "山西移动初始化异常";
                Log4netAdapter.WriteError("山西移动初始化异常", e);
                appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_Login)
                {
                    CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                    StatusCode = ServiceConsts.StatusCode_error,
                    Description = e.Message
                });
            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                if (appLog.LogDtlList.Count < 1)
                {
                    appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_Login)
                    {
                        CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                        StatusCode = Res.StatusCode,
                        Description = Res.StatusDescription
                    });
                }
                logMongo.Save(appLog);
            }
            return Res;
        }
        /// <summary>
        /// 发送短信验证码（可以跳过）
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            VerCodeRes Res = new VerCodeRes();
            Res.Token = mobileReq.Token;
            cookieStr = string.Empty;
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);

                Url = "http://service.sx.10086.cn/my/xd.html";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "service.sx.10086.cn",
                    // Referer = "http://service.sx.10086.cn/my/hfcx.html",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                //生成短信验证码图片
                Url = "http://service.sx.10086.cn/checkimage.shtml";
                httpItem = new HttpItem
                {
                    Accept = "image/png,image/*;q=0.8,*/*;q=0.5",
                    URL = Url,
                    Host = "service.sx.10086.cn",
                    // Referer = "http://service.sx.10086.cn/my/hfcx.html",
                    ResultType = ResultType.Byte,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(mobileReq.Token, httpResult.ResultByte);

                //校验图片验证码，发送验证短信
                Url = "http://service.sx.10086.cn/enhance/operate/pwdModify/checkRandCode.action";
                postdata = string.Format("seccodeverify={0}", mobileReq.Vercode);
                httpItem = new HttpItem
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Host = "service.sx.10086.cn",
                    // Referer = "http://service.sx.10086.cn/my/hfcx.html",
                    Method = "post",
                    Postdata = postdata,
                    Cookie = cookieStr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                if (!httpResult.Html.Trim().StartsWith("{") && !httpResult.Html.Trim().EndsWith("{"))
                {
                    Res.StatusDescription = "获取验证码失败，请稍后再试！!";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                    return Res;
                }
                JObject jsonObj = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                string errorMsg = jsonObj["retCode"].ToString();
                if (errorMsg != "0")
                {
                    Res.StatusDescription = "输入的图片验证码与提示不符,获取短信验证码失败";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                    return Res;
                }
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = "输入手机验证码，调用手机验证码验证接口";
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;

                CacheHelper.SetCache(mobileReq.Token, cookieStr);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "山西移动手机验证码发送异常";
                Log4netAdapter.WriteError("山西移动手机验证码发送异常", e);
                appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_SendSMS)
                {
                    CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                    StatusCode = ServiceConsts.StatusCode_error,
                    Description = e.Message
                });
            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                if (appLog.LogDtlList.Count < 1)
                {
                    appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_SendSMS)
                    {
                        CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                        StatusCode = Res.StatusCode,
                        Description = Res.StatusDescription
                    });
                }
                logMongo.Save(appLog);
            }
            return Res;
        }
        /// <summary>
        /// 校验短信验证码（可以跳过）
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            cookieStr = string.Empty;
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);

                //校验参数
                if (mobileReq.Mobile.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //校验短信验证码
                Url = "http://service.sx.10086.cn/enhance/operate/pwdModify/randomPwdCheck.action";
                postdata = string.Format("randomPwd={0}", mobileReq.Smscode);
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Host = "service.sx.10086.cn",
                    // Referer = "http://service.sx.10086.cn/my/hfcx.html",
                    Method = "post",
                    Postdata = postdata,
                    Cookie = cookieStr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                if (!httpResult.Html.StartsWith("{") && !httpResult.Html.EndsWith("{"))
                {
                    Res.StatusDescription = "短信验证码验证失败,刷新页面后重试";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                    return Res;
                }
                string errorMsg = jsonParser.GetResultFromMultiNode(httpResult.Html, "retMsg");
                if (errorMsg != "ok!")
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                    return Res;
                }
                Res.StatusDescription = "山西移动手机验证码验证成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Query;

                CacheHelper.SetCache(mobileReq.Token, cookieStr);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "山西移动手机验证码验证异常";
                Log4netAdapter.WriteError("山西移动手机验证码验证异常", e);
                appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_CheckSMS)
                {
                    CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                    StatusCode = ServiceConsts.StatusCode_error,
                    Description = e.Message
                });
            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                if (appLog.LogDtlList.Count < 1)
                {
                    appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_CheckSMS)
                    {
                        CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                        StatusCode = Res.StatusCode,
                        Description = Res.StatusDescription
                    });
                }
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
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            ApplyLogDtl logDtl = new ApplyLogDtl("");
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            CrawlerData crawler = new CrawlerData() { Token = mobileReq.Token, IdentityCard = mobileReq.IdentityCard, Name = mobileReq.Name, Mobile = mobileReq.Mobile, UserType = "1" };
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
                }
                #region 个人信息

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Crawler);
                Url = "http://service.sx.10086.cn/my/hfcx.html";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "StarLevelInfor", CrawlerTxt = Encoding.Default.GetBytes(httpResult.Html) });

                //积分
                string requestFlag = CommonFun.GetMidStr(httpResult.Html, "data:  \"requestFlag=", "\",");
                Url = "http://service.sx.10086.cn/enhance/points/queryMonthScore.action";
                postdata = string.Format("requestFlag={0}", requestFlag);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "IntegralInfor", CrawlerTxt = Encoding.Default.GetBytes(httpResult.Html) });

                Url = "http://service.sx.10086.cn/my/grzl.html";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://service.sx.10086.cn/my/hfcx.html",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = Encoding.Default.GetBytes(httpResult.Html) });

                //puk
                Url = "http://service.sx.10086.cn/my/sim.html";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://service.sx.10086.cn/my/hfcx.html",
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "pukInfor", CrawlerTxt = Encoding.Default.GetBytes(httpResult.Html) });

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "基本信息抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 月消费情况

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Crawler);
                DateTime dtNow = DateTime.Now;
                int i = 0;
                bool flag = true;
                do
                {
                    string searchMonth = dtNow.AddMonths(-i).ToString("yyyyMM");
                    //计费周期 套餐金额
                    Url = string.Format("http://service.sx.10086.cn/monthbill/tofeeBillIndex.action?startMonth={0}", searchMonth);
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Cookie = cookieStr
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "billBillCyclePlanAmtInfor" + (searchMonth), CrawlerTxt = Encoding.Default.GetBytes(httpResult.Html) });

                    if (i == 0)
                    {
                        //6个月的总金额数据列表
                        Url = "http://service.sx.10086.cn/monthbill/sixConsumeForJson.action";
                        postdata = string.Format("startMonth={0}", dtNow.ToString("yyyyMM"));
                        httpItem = new HttpItem
                        {
                            URL = Url,
                            Method = "post",
                            Postdata = postdata,
                            Cookie = cookieStr
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                        crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "billTotalAmtListInfor", CrawlerTxt = Encoding.Default.GetBytes(httpResult.Html) });

                        //套餐品牌  当前手机套餐
                        Url = "http://service.sx.10086.cn/promotion/custom/usermodel.action";
                        postdata = string.Format("userModelClass={0}", "userphone");
                        httpItem = new HttpItem
                        {
                            URL = Url,
                            Method = "post",
                            Postdata = postdata,
                            Cookie = cookieStr
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                        crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "brandInfor", CrawlerTxt = Encoding.Default.GetBytes(httpResult.Html) });
                    }
                    //各月总金额
                    //if (sixMonthInfoList.Count == 6)
                    //{
                    //    JObject monthInfo = JObject.Parse(sixMonthInfoList[5-i]);
                    //    bill.TotalAmt = monthInfo["currfeeInfo"].ToString();//总金额
                    //}

                    if (i == 5)
                    {
                        flag = false;
                    }
                    i++;
                    // mobile.BillList.Add(bill);
                } while (flag);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "月账单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                //detailType:1套餐固定费,2语音详单,3上网详单,4短/彩信详单
                //zqType:1按月查询
                #region 通话详单

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Crawler);

                GetHtmlDetails(crawler, EnumMobileDeatilType.Call);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 流量详单

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Crawler);

                GetHtmlDetails(crawler, EnumMobileDeatilType.Net);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 短信详单

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Crawler);

                GetHtmlDetails(crawler, EnumMobileDeatilType.SMS);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单抓取成功";
                appLog.LogDtlList.Add(logDtl);

                #endregion
                //保存
                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "山西移动手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                CacheHelper.SetCache(mobileReq.Token, cookieStr);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "山西移动手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("山西移动手机账单抓取异常", e);

                logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                if (Res.StatusCode == ServiceConsts.StatusCode_fail && logDtl.Description.IsEmpty())
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);
                }
                logMongo.Save(appLog);
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
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            ApplyLogDtl logDtl = new ApplyLogDtl("");
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            ApplyLog columnLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Column_Monitoring, mobileReq.Website);
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            MobileMongo mobileMongo = new MobileMongo(appDate);
            CrawlerData crawler = new CrawlerData();
            Basic mobile = new Basic();
            string result = string.Empty;
            List<string> results = new List<string>();
            Regex regex = new Regex(@"[\&nbsp;\s]");

            try
            {
                crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);
                #region 基本信息查询

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Analysis);
                mobile.Token = mobileReq.Token;
                mobile.BusName = mobileReq.Name;
                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.Mobile = mobileReq.Mobile;
                mobile.UpdateTime = crawler.CrawlerDate;

                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "StarLevelInfor").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//td[@class='last']/div/p[3]/span", "text");
                if (results.Count > 0)
                {
                    mobile.StarLevel = regex.Replace(results[0], "");//星级
                }
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "IntegralInfor").FirstOrDefault().CrawlerTxt);
                JObject Infojs = (JObject)JsonConvert.DeserializeObject(result);
                mobile.Integral = Infojs["current_point"].ToString();//积分
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//div[@class='main']/div[1]/div/span/b", "text");
                if (results.Count > 0)
                {
                    mobile.Regdate = DateTime.ParseExact(results[0].ToTrim("入网时间："), "yyyy年MM月dd日", CultureInfo.InvariantCulture).ToString(Consts.DateFormatString11);//入网时间
                }
                results = HtmlParser.GetResultFromParser(result, "//form[@id='thisform']/input[@id='contact_email']", "value");
                if (results.Count > 0)
                {
                    mobile.Email = results[0];//邮箱
                }
                mobile.Name = CommonFun.GetMidStr(CommonFun.GetMidStr(result, "<th name=\"userName\">", ""), "", "</th>");//姓名
                if (mobile.Name.IsEmpty())
                {
                    results = HtmlParser.GetResultFromParser(result, "//form[@name='thisform']/input[@id='cust_name']", "value");
                    if (results.Count > 0)
                    {
                        mobile.Name = results[0];//姓名
                    }
                }
                mobile.Idcard = CommonFun.GetMidStr(CommonFun.GetMidStr(result, "<td class=\"td-title\" name=\"userPhone\">证件号码：</td><th>", ""), "", "</th>");//证件号
                if (mobile.Idcard.IsEmpty())
               {
                   results = HtmlParser.GetResultFromParser(result, "//form[@id='thisform']/input[@id='id_iccid']", "value");
                   if (results.Count > 0)
                   {
                       mobile.Idcard = results[0];//证件号
                   }
               }

                results = HtmlParser.GetResultFromParser(result, "//form[@id='thisform']/input[@id='contact_post']", "value");
                if (results.Count > 0)
                {
                    mobile.Postcode = results[0];//邮政编码
                }
                results = HtmlParser.GetResultFromParser(result, "//form[@id='thisform']/input[@id='contact_address']", "value");
                if (results.Count > 0)
                {
                    mobile.Address = results[0];//联系地址
                }
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "pukInfor").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//div[@class='clearfix w-745']/ul/li[5]/span", "text");
                if (results.Count > 0)
                {
                    mobile.PUK = results[0];//PUK
                }
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "brandInfor").FirstOrDefault().CrawlerTxt);
                try
                {
                    JObject jsonObj = (JObject)JsonConvert.DeserializeObject(jsonParser.GetResultFromMultiNode(result, "userModel"));
                    mobile.PackageBrand = jsonObj["brand_name"].ToString();//套餐品牌
                    mobile.Package = jsonObj["prod_prc_name"].ToString();//当前手机套餐
                }
                catch { }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "个人信息解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 账单查询

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis);
                DateTime dtNow = DateTime.Parse(crawler.CrawlerDate);
                MonthBill bill = null;
                List<string> sixMonthInfoList = new List<string>();//最近6个月总金额集合
                int i = 0;
                bool flag = true;
                do
                {
                    try
                    {
                        bill = new MonthBill();
                        string searchMonth = dtNow.AddMonths(-i).ToString("yyyyMM");
                        result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "billBillCyclePlanAmtInfor" + searchMonth).FirstOrDefault().CrawlerTxt);

                        results = HtmlParser.GetResultFromParser(result, "//div[@class='main']/table[1]/tr/td[1]/p[4]", "text");
                        if (results.Count > 0)
                        {
                            string temp = regex.Replace(results[0], "").Replace("计费周期：", "");//2015年09月01日至2015年09月30日
                            if (temp.Contains("至"))
                            {
                                bill.BillCycle = (Convert.ToDateTime(temp.Split('至')[0])).ToString(Consts.DateFormatString12);//计费周期
                            }
                        }
                        results = HtmlParser.GetResultFromParser(result, "//div[@class='main']/table[2]/tr/td[1]/table/tr[1]/td/b/span", "text");
                        if (results.Count > 0)
                        {
                            bill.PlanAmt = results[0];//套餐金额（包括基本月租和订购业务费用）
                        }

                        //请求一次即可获得6个月的总金额数据
                        if (i == 0)
                        {
                            result = Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "billTotalAmtListInfor").FirstOrDefault().CrawlerTxt);

                            sixMonthInfoList = jsonParser.GetArrayFromParse(jsonParser.GetResultFromMultiNode(result, "billSixMonthOut:sixConsumeInfo"), "sixMonthInfoList");
                        }
                        if (sixMonthInfoList.Count == 6)
                        {
                            JObject monthInfo = JObject.Parse(sixMonthInfoList[5 - i]);
                            bill.TotalAmt = monthInfo["currfeeInfo"].ToString();//总金额
                        }
                        if (i == 5)
                        {
                            flag = false;
                        }
                        mobile.BillList.Add(bill);
                    }
                    catch { }
                    i++;
                } while (flag);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 通话详单

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Analysis);
                i = 0;
                do
                {
                    string searchMonth = dtNow.AddMonths(-i).ToString("yyyyMM");
                    string yearStr = dtNow.AddMonths(-i).Year.ToString();
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == EnumMobileDeatilType.Call + searchMonth).FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//div[@id='aaaa']/table/tbody/tr[position()>1]", "inner");
                    foreach (string items in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(items, "//td", "text");
                        if (tdRow.Count != 8)
                        {
                            continue;
                        }
                        var totalSecond = 0;
                        var usetime = tdRow[4].ToString();
                        if (!string.IsNullOrEmpty(usetime))
                        {
                            totalSecond = CommonFun.ConvertDate(usetime);
                        }
                        Call call = new Call()
                        {
                            StartTime = DateTime.Parse(yearStr + "-" + tdRow[0]).ToString(Consts.DateFormatString11),
                            CallPlace = tdRow[1],
                            InitType = tdRow[2],
                            OtherCallPhone = tdRow[3],
                            UseTime = totalSecond.ToString(),
                            CallType = tdRow[5],
                            SubTotal = tdRow[7].ToDecimal(0)
                        };
                        mobile.CallList.Add(call);
                    }
                    i++;
                } while (i < 6);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 上网详单

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Analysis);
                i = 0;
                do
                {
                    string searchMonth = dtNow.AddMonths(-i).ToString("yyyyMM");
                    string yearStr = dtNow.AddMonths(-i).Year.ToString();
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == EnumMobileDeatilType.Net + searchMonth).FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//div[@id='aaaa']/table[2]/tbody/tr[position()>1]", "inner");
                    foreach (string items in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(items, "//td", "text");
                        if (tdRow[0].Contains("合计"))
                        {
                            break;
                        }
                        if (tdRow.Count != 7)
                        {
                            continue;
                        }

                        var totalFlow = CommonFun.ConvertGPRS(tdRow[4].ToString());
                        Net gprs = new Net()
                        {
                            StartTime = DateTime.Parse(yearStr + "-" + tdRow[0]).ToString(Consts.DateFormatString11),
                            Place = tdRow[1],
                            PhoneNetType = tdRow[2],
                            SubFlow = totalFlow.ToString(),
                            SubTotal = tdRow[6].ToDecimal(0)
                        };
                        mobile.NetList.Add(gprs);
                    }

                    i++;
                } while (i < 6);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 短信详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Analysis);
                i = 0;
                do
                {
                    string searchMonth = dtNow.AddMonths(-i).ToString("yyyyMM");
                    string yearStr = dtNow.AddMonths(-i).Year.ToString();
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == EnumMobileDeatilType.SMS + searchMonth).FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//div[@id='aaaa']/table[2]/tbody/tr[position()>1]", "inner");
                    foreach (string items in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(items, "//td", "text");
                        if (tdRow.Count != 7)
                        {
                            continue;
                        }
                        Sms sms = new Sms()
                        {
                            StartTime = DateTime.Parse(yearStr + "-" + tdRow[0]).ToString(Consts.DateFormatString11),
                            OtherSmsPhone = tdRow[1],
                            InitType = tdRow[2],
                            SmsType = tdRow[4],
                            SubTotal = tdRow[6].ToDecimal(0)
                        };
                        mobile.SmsList.Add(sms);
                    }
                    i++;
                } while (i < 6);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "山西移动手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "山西移动手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("山西移动手机账单解析异常", e);

                logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            finally
            {
                if (columnLog.LogDtlList.Count > 0)
                    logMongo.Save(columnLog);
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                logMongo.Save(appLog);
            }
            return Res;
        }
        /// <summary>
        /// 抓取保存详单页面
        /// </summary>
        /// <param name="crawler"></param>
        /// <param name="detailsType">详单类型：1套餐固定费,2语音详单,3上网详单,4短/彩信详单</param>
        /// <param name="saveTitle">自定义保存标签</param>
        private void GetHtmlDetails(CrawlerData crawler, EnumMobileDeatilType type)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            DateTime dateNow = DateTime.Now;//当前时间
            int times = 0;
            Url = "http://service.sx.10086.cn/enhance/fee/queryDetail/queryDetail!queryLocalDetail22.action";
            string detailsType = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                detailsType = "2";
            else if (type == EnumMobileDeatilType.SMS)
                detailsType = "4";
            else
                detailsType = "3";

            do
            {
                string timestyle1 = dateNow.AddMonths(-times).ToString("yyyy-MM");
                string timestyle2 = dateNow.AddMonths(-times).ToString("yyyyMM");
                postdata = string.Format("beginMonth={0}-01&endMonth={1}01&detailType={2}&zqType=1&show_type=all&smcheck=no", timestyle1, timestyle2, detailsType);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Cookie = cookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + timestyle2, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                times++;
            } while (times < 6);
        }
    }
}
