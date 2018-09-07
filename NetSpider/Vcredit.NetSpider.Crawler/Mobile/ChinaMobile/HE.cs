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
    public class HE : IMobileCrawler
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
        string CookieStr = string.Empty;
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
            string spid = string.Empty;
            string RelayState = string.Empty;
            string displayPics = string.Empty;
            string displayPic = string.Empty;
            string type = string.Empty;
            string formertype = string.Empty;
            string backurl = string.Empty;
            string warnurl = string.Empty;
            string isEncodePassword = string.Empty;
            try
            {
                //第一步，初始化登录页面
                Url = "http://he.ac.10086.cn/login";
                httpItem = new HttpItem()
                {
                    URL = Url,
                };
                httpResult = httpHelper.GetHtml(httpItem);
                CookieStr = CommonFun.GetCookieStringNew("", httpResult.Cookie);

                displayPics = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='displayPics']", "value")[0];
                displayPic = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='displayPic']", "value")[0];
                type = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='type']", "value")[0];
                formertype = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='formertype']", "value")[0];
                backurl = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='backurl']", "value")[0];
                warnurl = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='warnurl']", "value")[0];
                RelayState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value")[0];
                spid = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='spid']", "value")[0];

                CacheHelper.SetCache(Res.Token + "displayPics", displayPics);
                CacheHelper.SetCache(Res.Token + "displayPic", displayPic);
                CacheHelper.SetCache(Res.Token + "type", type);
                CacheHelper.SetCache(Res.Token + "formertype", formertype);
                CacheHelper.SetCache(Res.Token + "backurl", backurl);
                CacheHelper.SetCache(Res.Token + "warnurl", warnurl);
                CacheHelper.SetCache(Res.Token + "spid", spid);
                CacheHelper.SetCache(Res.Token + "RelayState", RelayState);

                //第二步，获取验证码
                Url = "http://he.ac.10086.cn/common/image.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    Cookie = CookieStr,
                    Referer = "http://he.ac.10086.cn/login"
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);

                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "河北移动初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, CookieStr);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "河北移动初始化异常";
                Log4netAdapter.WriteError("河北移动初始化异常", e);
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
            string SAMLart = string.Empty;
            string SAMLRequest = string.Empty;
            string RelayState = string.Empty;
            string displayPics = string.Empty;
            string displayPic = string.Empty;
            string type = string.Empty;
            string formertype = string.Empty;
            string backurl = string.Empty;
            string warnurl = string.Empty;
            string isEncodePassword = string.Empty;
            WebHeaderCollection addheader = new WebHeaderCollection();
            addheader.Set("Origin", "http://he.ac.10086.cn");
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    CookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
                }
                //校验参数
                if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = string.Format("https://he.ac.10086.cn/validImageCode?r_0.39103143994381495&imageCode={0}", mobileReq.Vercode);
                httpItem = new HttpItem
                {
                    Accept = "*/*",
                    URL = Url,
                    Referer = "https://he.ac.10086.cn/login",
                    Cookie = CookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                if (httpResult.Html.Trim() != "1")
                {
                    Res.StatusDescription = "请输入正确的验证码";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                    return Res;
                }
                displayPics = (string)CacheHelper.GetCache(mobileReq.Token + "displayPics");
                displayPic = (string)CacheHelper.GetCache(mobileReq.Token + "displayPic");
                type = (string)CacheHelper.GetCache(mobileReq.Token + "type");
                formertype = (string)CacheHelper.GetCache(mobileReq.Token + "formertype");
                backurl = (string)CacheHelper.GetCache(mobileReq.Token + "backurl");
                warnurl = (string)CacheHelper.GetCache(mobileReq.Token + "warnurl");
                spid = (string)CacheHelper.GetCache(mobileReq.Token + "spid");
                RelayState = (string)CacheHelper.GetCache(mobileReq.Token + "RelayState");
                //第三步
                Url = "http://he.ac.10086.cn/Login";
                postdata = string.Format
                    ("displayPics={4}&displayPic={5}&type={6}&formertype={7}&backurl={8}&warnurl={9}&spid={0}&RelayState={10}&mobileNum={1}&userIdTemp={1}&servicePassword={2}&emailPwd=&smsValidCode=&login_pwd_type=&email=输入Email邮箱地址&validCode={3}&emailPwd=请输入密码&servicePassword=请输入6位数字的服务密码&smsValidCode=",
                    spid, mobileReq.Mobile, MultiKeyDES.EncryptDES(mobileReq.Password, "YHXWWLKJYXGS", "ZFCHHYXFL10C", "DES"), mobileReq.Vercode, displayPics, displayPic, type, formertype, backurl, warnurl, RelayState);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://he.ac.10086.cn/login",
                    Cookie = CookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='cover_content1']", "text");
                if (results.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(results[0]))
                    {
                        Res.StatusDescription = results[0];
                        Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                        return Res;
                    }
                }
                SAMLart = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value")[0];
                RelayState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value")[0];
                displayPics = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='displayPics']", "value")[0];
                isEncodePassword = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='isEncodePassword']", "value")[0];

                Url = "http://he.ac.10086.cn/hblogin/backPage.jsp";
                postdata = string.Format("SAMLart={0}&isEncodePassword={4}&displayPic={1}&RelayState={2}&displayPics={3}", SAMLart, displayPic, RelayState.ToUrlEncode(), displayPics.ToUrlEncode(), isEncodePassword);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Cookie = CookieStr,
                    Referer = "http://he.ac.10086.cn/Login"
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);

                SAMLart = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value")[0];
                RelayState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value")[0];

                Url = "http://www.he.10086.cn/my";
                postdata = string.Format("SAMLart={0}&RelayState={1}", SAMLart, RelayState.ToUrlEncode());
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Allowautoredirect = false,
                    Referer = "http://he.ac.10086.cn/login",
                    Cookie = CookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);

                Url = "http://www.he.10086.cn/my/";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://he.ac.10086.cn/login",
                    Cookie = CookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);

                SAMLRequest = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLRequest']", "value")[0];
                RelayState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value")[0];

                Url = "http://he.ac.10086.cn/POST";
                postdata = string.Format("SAMLRequest={0}&RelayState={1}", SAMLRequest.ToUrlEncode(), RelayState.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://www.he.10086.cn/my/",
                    Cookie = CookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);

                SAMLart = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value")[0];
                RelayState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value")[0];

                Url = "http://www.he.10086.cn/my/";
                postdata = string.Format("SAMLart={0}&isEncodePassword=2&displayPic=0&RelayState={1}&displayPics={2}", SAMLart, RelayState.ToUrlEncode(), displayPics.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://he.ac.10086.cn/POST",
                    Cookie = CookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);

                Url = "http://www.he.10086.cn/my/account/";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://www.he.10086.cn/my/",
                    Cookie = CookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);

                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_SendSMSAndVercode;

                CacheHelper.SetCache(mobileReq.Token, CookieStr);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "河北移动登录异常";
                Log4netAdapter.WriteError("河北移动登录异常", e);
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
        /// 发送短信验证码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            VerCodeRes Res = new VerCodeRes();
            Res.Token = mobileReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                //校验参数
                if (mobileReq.Mobile.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.MobileEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (((string)CacheHelper.GetCache(mobileReq.Token + "UserSMS")).IsEmpty())
                {
                    CacheHelper.SetCache(mobileReq.Token + "UserSMS", "0");
                    //获取保存首次成功登陆缓存
                    if (CacheHelper.GetCache(mobileReq.Token) != null)
                    {
                        CookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
                        CacheHelper.SetCache(mobileReq.Token, CookieStr);
                    }
                    #region 个人资料短信验证发送
                    Url = "http://www.he.10086.cn/my/individualInfoServiceAction!init.action?menuid=individualInformation&pageId=0.5801392904888254";
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Host = "www.he.10086.cn",
                        Cookie = CookieStr
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                    //图片验证码
                    Url = " http://www.he.10086.cn/my/image.action";
                    httpItem = new HttpItem
                    {
                        Accept = "image/png,image/*;q=0.8,*/*;q=0.5",
                        URL = Url,
                        Host = "www.he.10086.cn",
                        Referer = "http://www.he.10086.cn/my/individualInfoServiceAction!init.action?menuid=individualInformation&pageId=0.5801392904888254",
                        Cookie = CookieStr,
                        ResultType = ResultType.Byte
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                    Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                    Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);

                    Url = " http://www.he.10086.cn/my/customize/heb/jsp/common/businessCodeNew.jsp?menuid=businessQuery&six=99";
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Host = "www.he.10086.cn",
                        Referer = "http://www.he.10086.cn/my/individualInfoServiceAction!init.action?menuid=individualInformation&pageId=0.5801392904888254",
                        Cookie = CookieStr,
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);

                    Url = "http://www.he.10086.cn/my/account/accountSettingHeb!shopCarIfLogin.action?date=Wed%20Oct%2014%202015%2011:15:58%20GMT+0800";
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Host = "www.he.10086.cn",
                        Referer = "http://www.he.10086.cn/my/individualInfoServiceAction!init.action?menuid=individualInformation&pageId=0.5801392904888254",
                        Cookie = CookieStr,
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);

                    Url = "http://www.he.10086.cn/my/login!qryPerRecommData.action?r=0.9485211235327501";
                    httpItem = new HttpItem
                    {
                        Accept = "application/json, text/javascript, */*; q=0.01",
                        URL = Url,
                        Host = "www.he.10086.cn",
                        Referer = "http://www.he.10086.cn/my/individualInfoServiceAction!init.action?menuid=individualInformation&pageId=0.5801392904888254",
                        Cookie = CookieStr,
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                    //发送短信验证码（调试注意：每天限制发送10次）
                    Url = " http://www.he.10086.cn/my/authorize!sendRandomCode.action";
                    postdata = "phoneNumber=undefined";
                    httpItem = new HttpItem
                    {
                        Accept = "application/json, text/javascript, */*; q=0.01",
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        Host = "www.he.10086.cn",
                        Referer = "http://www.he.10086.cn/my/individualInfoServiceAction!init.action?menuid=individualInformation&pageId=0.5801392904888254",
                        Cookie = CookieStr
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);

                    if (!httpResult.Html.StartsWith("{") || !httpResult.Html.EndsWith("}"))
                    {
                        Res.StatusDescription = "获取个人资料短信校验码失败";
                        Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                        return Res;
                    }
                    JObject jResults = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                    if (!jResults["result"].ToString().Contains("success"))
                    {
                        if (jResults["result"].ToString().Contains("outOfCount"))
                        {
                            Res.StatusDescription = jResults["checkCount"].ToString();
                            Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                            return Res;
                        }
                        Res.StatusDescription = "获取个人资料短信校验码失败";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                    Res.StatusDescription = "获取个人资料短信和图片验证码，调用手机验证码验证接口";
                    Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
                    CacheHelper.SetCache(mobileReq.Token + "userBase", CookieStr);
                    #endregion

                }
                else
                {
                    //获取保存首次登陆缓存
                    if (CacheHelper.GetCache(mobileReq.Token) != null)
                    {
                        CookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
                        CacheHelper.SetCache(mobileReq.Token, CookieStr);
                    }
                    #region 详单短信验证发送
                    //重登陆验证开始
                    Url = "http://www.he.10086.cn/service/fee/qryDetailBill.action";
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Host = "www.he.10086.cn",
                        Referer = "http://www.he.10086.cn/my/account/",
                        Cookie = CookieStr
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                    string SAMLRequest = string.Empty;
                    List<string> results = new List<string>();
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLRequest']", "value");
                    if (results.Count == 0)
                    {
                        Res.StatusDescription = "获取详单短信失败,请重新登录尝试";
                        Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                        return Res;
                    }
                    SAMLRequest = results[0];
                    string RelayState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value")[0];
                    Url = "http://he.ac.10086.cn/POST";
                    postdata = string.Format("SAMLRequest={0}&RelayState={1}", SAMLRequest.ToUrlEncode(), RelayState.ToUrlEncode());
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        Referer = "http://www.he.10086.cn/service/fee/qryDetailBill.action",
                        Cookie = CookieStr
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                    string SAMLart = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value")[0];
                    string isEncodePassword = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='isEncodePassword']", "value")[0];
                    string displayPic = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='displayPic']", "value")[0];
                    RelayState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value")[0];
                    string displayPics = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='displayPics']", "value")[0];

                    Url = " http://www.he.10086.cn/service/login!initLogin.action";
                    postdata = string.Format("SAMLart={0}&isEncodePassword={1}&displayPic={2}&RelayState={3}&displayPics={4}", SAMLart, isEncodePassword, displayPic, RelayState.ToUrlEncode(), displayPics.ToUrlEncode());
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        Referer = "http://he.ac.10086.cn/POST",
                        Cookie = CookieStr
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);

                    Url = " http://www.he.10086.cn/service/fee/qryDetailBill.action";
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Host = "www.he.10086.cn",
                        Referer = "http://www.he.10086.cn/service/login!initLogin.action",
                        Cookie = CookieStr
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                    //重登陆验证结束
                    //发送验证码 
                    Url = "http://www.he.10086.cn/service/loginValidate.action";
                    postdata = string.Format("validateUrl=&continue_url={0}", "http%3A%2F%2Fwww.he.10086.cn%2Fservice%2Ffee%2FqryDetailBill.action");
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        Referer = "http://www.he.10086.cn/service/fee/qryDetailBill.action",
                        Cookie = CookieStr
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);

                    Url = "http://www.he.10086.cn/service/fee/fee/qryDetailBill!sendRandomCode.action?r=0.5515655386142357";
                    httpItem = new HttpItem
                    {
                        Accept = "application/json, text/javascript, */*; q=0.01",
                        URL = Url,
                        Cookie = CookieStr,
                        Referer = "http://www.he.10086.cn/service/fee/qryDetailBill.action"
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (!httpResult.Html.StartsWith("{") && !httpResult.Html.EndsWith("}"))
                    {
                        Res.StatusDescription = "获取详单短信校验码失败";
                        Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                        return Res;
                    }
                    JObject jResults = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                    if (!jResults["result"].ToString().Contains("success"))
                    {
                        Res.StatusDescription = "查询手机详单短信验证码发送失败,请重新登陆尝试";
                        Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                        return Res;
                    }
                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                    Res.StatusDescription = "获取手机详单验证码，调用手机验证码验证接口";
                    Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
                    #endregion
                    CacheHelper.SetCache(mobileReq.Token + "userDetails", CookieStr);
                }
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "河北移动手机验证码发送异常";
                Log4netAdapter.WriteError("河北移动手机验证码发送异常", e);
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
        /// 校验短信验证码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                if (((string)CacheHelper.GetCache(mobileReq.Token + "UserSMS")) == "0")
                {
                    CacheHelper.SetCache(mobileReq.Token + "UserSMS", "1");

                    if (CacheHelper.GetCache(mobileReq.Token + "userBase") != null)
                    {
                        CookieStr = (string)CacheHelper.GetCache(mobileReq.Token + "userBase");
                    }
                    #region 个人资料短信验证
                    Url = "http://www.he.10086.cn/my/authorize!validateRandomCode.action";
                    postdata = string.Format("smsRandomCode={0}&validateCode={1}", mobileReq.Smscode, mobileReq.Vercode);
                    httpItem = new HttpItem
                    {
                        Accept = "application/json, text/javascript, */*; q=0.01",
                        ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Referer = "http://www.he.10086.cn/my/individualInfoServiceAction!init.action?menuid=individualInformation&pageId=0.5801392904888254",
                        Cookie = CookieStr
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (!httpResult.Html.StartsWith("{") || !httpResult.Html.EndsWith("}"))
                    {
                        Res.StatusDescription = "个人资料手机验证码验证失败";
                        Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                        return Res;
                    }
                    JObject jResults = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                    if (!jResults["result"].ToString().Contains("smsValidateSucc"))
                    {
                        Res.StatusDescription = "个人资料手机验证码验证失败";
                        Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                        return Res;
                    }
                    Res.StatusDescription = "河北移动个人资料手机验证码验证成功";
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                    Res.nextProCode = ServiceConsts.NextProCode_SendSMS;
                    #endregion
                    CacheHelper.SetCache(mobileReq.Token + "userBase", CookieStr);
                }
                else
                {
                    if (CacheHelper.GetCache(mobileReq.Token + "userDetails") != null)
                    {
                        CookieStr = (string)CacheHelper.GetCache(mobileReq.Token + "userDetails");
                    }
                    if (CacheHelper.GetCache(mobileReq.Token) != null)
                    {
                        CacheHelper.SetCache(mobileReq.Token, CookieStr);
                    }
                    #region 详单验证码验证码
                    Url = "http://www.he.10086.cn/service/fee/qryDetailBill!checkSmsCode.action?r=0.1920248509135819";
                    postdata = string.Format("smsrandom={0}", mobileReq.Smscode);
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        Referer = "http://www.he.10086.cn/service/fee/qryDetailBill.action",
                        Cookie = CookieStr
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                    JObject jResults = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                    if (!string.IsNullOrWhiteSpace(jResults["desc"].ToString()))
                    {
                        Res.StatusDescription = jResults["desc"].ToString();
                        Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                        return Res;
                    }
                    Url = "http://www.he.10086.cn/service/fee/qryDetailBill.action";
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Referer = "http://www.he.10086.cn/service/fee/qryDetailBill!checkSmsCode.action?r=0.1920248509135819",
                        Cookie = CookieStr
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                    Res.StatusDescription = "河北移动获取详单短信验证码验证成功";
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                    Res.nextProCode = ServiceConsts.NextProCode_Query;
                    #endregion
                    CacheHelper.SetCache(mobileReq.Token + "userDetails", CookieStr);
                }
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "河北移动手机验证码校验异常";
                Log4netAdapter.WriteError("河北移动手机验证码校验异常", e);
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
        /// 查询账单
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
            List<string> results = new List<string>();
            string spid = string.Empty;
            string SAMLart = string.Empty;
            string SAMLRequest = string.Empty;
            string RelayState = string.Empty;
            string displayPics = string.Empty;
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    CookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
                    CacheHelper.RemoveCache(mobileReq.Token);
                }

                #region 基本信息查询
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Crawler);

                Url = "http://www.he.10086.cn/my/account/accountSettingHeb!shopCarIfLogin.action?date=Wed%20Oct%2014%202015%2014:53:28%20GMT+0800";
                httpItem = new HttpItem
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Host = "www.he.10086.cn",
                    Referer = "http://www.he.10086.cn/my/individualInfoServiceAction!init.action?menuid=individualInformation&pageId=0.5801392904888254",
                    Cookie = CookieStr,
                };
                httpResult = httpHelper.GetHtml(httpItem);
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);

                //积分
                Url = "http://www.he.10086.cn/my/account/myscoreheb!init.action?groupId=myGrades&menuid=myScore&pageId=0.5801392904888254";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "www.he.10086.cn",
                    Referer = "http://www.he.10086.cn/my/account/",
                    Cookie = CookieStr,
                };
                httpResult = httpHelper.GetHtml(httpItem);
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);

                Url = "http://www.he.10086.cn/my/account/qryScoreAll!queryScoreAll.action";
                httpItem = new HttpItem
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Host = "www.he.10086.cn",
                    Referer = "http://www.he.10086.cn/my/account/myscoreheb!init.action?groupId=myGrades&menuid=myScore&pageId=0.5801392904888254",
                    Cookie = CookieStr,
                };
                httpResult = httpHelper.GetHtml(httpItem);
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                //星级
                Url = "http://www.he.10086.cn/my/account/belongInfoQueryAction!findBaseInfo.action";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "www.he.10086.cn",
                    Referer = "http://www.he.10086.cn/my/account/",
                    Cookie = CookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "starLevelInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                //puk 
                Url = "http://www.he.10086.cn/my/account/quryPuk.action";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "www.he.10086.cn",
                    Referer = "http://www.he.10086.cn/my/account/belongInfoQueryAction!findBaseInfo.action",
                    Cookie = CookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "pukInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                //套餐品牌
                Url = "http://www.he.10086.cn/my/account/mysuiteMgr!initQueryProdAndServList.action?menuid=suitesel&pageId=0.5503451292680075";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "www.he.10086.cn",
                    Referer = "http://www.he.10086.cn/my/account/",
                    Cookie = CookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "packageBrandInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//input [@id='currentProdName']", "value");

                //获取个人资料缓存
                if (CacheHelper.GetCache(mobileReq.Token + "userBase") != null)
                {
                    CookieStr = (string)CacheHelper.GetCache(mobileReq.Token + "userBase");
                    CacheHelper.RemoveCache(mobileReq.Token + "userBase");
                }
                //个人资料
                Url = "http://www.he.10086.cn/my/individualInfoServiceAction!init.action?pageId=0.5801392904888254&menuid=individualInformation";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "www.he.10086.cn",
                    Referer = "http://www.he.10086.cn/my/individualInfoServiceAction!init.action?menuid=individualInformation&pageId=0.5801392904888254",
                    Cookie = CookieStr,
                };
                httpResult = httpHelper.GetHtml(httpItem);
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "基本信息抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 账单查询
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Crawler);

                Url = "http://www.he.10086.cn/service/fee/qryMyBill.action?groupId=tabGroupBill&menuid=myBill&pageId=0.9461502220295995";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://www.he.10086.cn/my/account/",
                    Cookie = CookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);

                SAMLRequest = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLRequest']", "value")[0];
                RelayState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value")[0];

                Url = "http://he.ac.10086.cn/POST";
                postdata = string.Format("SAMLRequest={0}&RelayState={1}", SAMLRequest, RelayState);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://www.he.10086.cn/service/fee/qryMyBill.action?groupId=tabGroupBill&menuid=myBill&pageId=0.9461502220295995",
                    Cookie = CookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);

                SAMLart = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SAMLart']", "value")[0];
                RelayState =
                    HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RelayState']", "value")[0];

                Url = "http://www.he.10086.cn/service/login!initLogin.action";
                postdata = string.Format("SAMLart={0}&isEncodePassword=2&displayPic=0&RelayState={1}&displayPics={2}",
                        SAMLart, RelayState.ToUrlEncode(), displayPics.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://he.ac.10086.cn/POST",
                    Cookie = CookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);

                Url = "http://www.he.10086.cn/service/fee/qryMyBill.action?groupId=tabGroupBill";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://www.he.10086.cn/service/login!initLogin.action",
                    Cookie = CookieStr
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                spid = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='spid']", "value")[0];

                for (int i = 0; i < 6; i++)
                {
                    Url = "http://www.he.10086.cn/service/fee/qryMyBill!qryBillAllInfo.action?cycle=" +
                          DateTime.Now.AddMonths(-i).ToString("yyyyMM");
                    postdata = @"type=B&backurl=&warnurl=http%3A%2F%2Fwww.he.10086.cn%2Fservice%2FwarnPage.jsp&spid={0}
&RelayState=&loginType=1&validCode=&mobileNum=%E6%89%8B%E6%9C%BA%E5%8F%B7%E7%A0%81%2F%E6%97%A0%E7%BA
%BF%E5%BA%A7%E6%9C%BA%E5%8F%B7%E7%A0%81&email=%E8%BE%93%E5%85%A5Email%E9%82%AE%E7%AE%B1%E5%9C%B0%E5%9D
%80&validCode1=%E7%82%B9%E5%87%BB%E8%8E%B7%E5%8F%96%E9%AA%8C%E8%AF%81%E7%A0%81&servicePassword=&smsValidCode
=&emailPwd=";
                    postdata = string.Format(postdata, spid);
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Referer = "http://www.he.10086.cn/service/fee/qryMyBill.action?groupId=tabGroupBill",
                        Cookie = CookieStr
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (httpResult.StatusCode != HttpStatusCode.OK)
                    {
                        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 通话详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Crawler);

                //获取详单缓存
                if (CacheHelper.GetCache(mobileReq.Token + "userDetails") != null)
                {
                    CookieStr = (string)CacheHelper.GetCache(mobileReq.Token + "userDetails");
                    CacheHelper.RemoveCache(mobileReq.Token + "userDetails");
                }
                Regex regex = new Regex(@"[\&nbsp\s]");
                int dateNow = int.Parse(DateTime.Now.ToString("yyyyMM"));
                int endTime = dateNow - 5;
                //string nowYear = DateTime.Now.Year.ToString();//当前年
                //电话详单  000:全部

                Url = "http://www.he.10086.cn/service/fee/qryDetailBill!qryNewBill.action?smsrandom=&r=0.2841774755155093";
                for (int i = dateNow; i >= endTime; i--)
                {
                    postdata = string.Format("menuid=&fieldErrFlag=&selectncode=&ncodestatus=&operatype=&groupId=&theMonth={0}&queryType={1}&qryscope=0&selectTaken=&regionstate=1&onlinetime=201204&qryType=10&qryDate=000&callPlace=000&opTelnum=000&callWay=000&callType=000", i, "NGQryCallBill");
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Cookie = CookieStr,
                        Referer = "http://www.he.10086.cn/service/fee/qryDetailBill.action"
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = EnumMobileDeatilType.Call.ToString() + i, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='bill_page']/tr[position()>1]", "inner");
                #endregion

                #region 上网详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Crawler);

                for (int i = dateNow; i >= endTime; i--)
                {
                    Url = "http://www.he.10086.cn/service/fee/qryDetailBill!qryNewBill.action?r=0.19987846217485705";
                    postdata = string.Format("menuid=&fieldErrFlag=&selectncode=&ncodestatus=&operatype=&groupId=&theMonth={0}&queryType={1}&qryscope=0&selectTaken=&regionstate=1&onlinetime=201204&qryType=10&qryDate=000&callPlace=000&opTelnum=000&callWay=000&callType=000", i, "NGQryNetBill");
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Cookie = CookieStr,
                        Referer = "http://www.he.10086.cn/service/fee/qryDetailBill!qryNewBill.action?smsrandom=&r=0.2841774755155093"
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = EnumMobileDeatilType.Net.ToString() + i, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 短信详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Crawler);

                for (int i = dateNow; i >= endTime; i--)
                {
                    Url = "http://www.he.10086.cn/service/fee/qryDetailBill!qryNewBill.action?r=0.699883647254205";
                    postdata = string.Format("menuid=&fieldErrFlag=&selectncode=&ncodestatus=&operatype=&groupId=&theMonth={0}&queryType={1}&qryscope=0&selectTaken=&regionstate=1&onlinetime=201204&qryType=10&qryDate=000&callPlace=000&opTelnum=000&callWay=000&callType=000", i, "NGQrySMSBill");
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Cookie = CookieStr,
                        Referer = "http://www.he.10086.cn/service/fee/qryDetailBill!qryNewBill.action?r=0.19987846217485705"
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = EnumMobileDeatilType.SMS.ToString() + i, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 手机套餐

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Crawler);

                for (int i = dateNow; i >= endTime; i--)
                {
                    Url = "http://www.he.10086.cn/service/fee/qryDetailBill!qryNewBill.action?r=0.699883647254205";
                    postdata = string.Format("menuid=&fieldErrFlag=&selectncode=&ncodestatus=&operatype=&groupId=&theMonth={0}&queryType={1}&qryscope=0&selectTaken=&regionstate=1&onlinetime=201204&qryType=10&qryDate=000&callPlace=000&opTelnum=000&callWay=000&callType=000", i, "NGQryTaoCanBill");
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Cookie = CookieStr,
                        Referer = "http://www.he.10086.cn/service/fee/qryDetailBill!qryNewBill.action?r=0.699883647254205"
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    CookieStr = CommonFun.GetCookieStringNew(CookieStr, httpResult.Cookie);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='bill_page']/tr[2]/td[3]", "text");
                    string Package = string.Empty;
                    if (results.Count > 0)
                    {
                        Package = regex.Replace(results[0], "");//手机套餐
                    }
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "PackageInfor" + i, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    if (!String.IsNullOrEmpty(Package))
                    {
                        break;
                    }
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "手机套餐抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                //保存
                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "河北移动手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "河北移动手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("河北移动手机账单抓取异常", e);

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
            string nowYear = string.Empty;//当前年
            Regex regex = new Regex(@"[\&nbsp\s]");
            int dateNow = int.Parse(DateTime.Now.ToString("yyyyMM"));
            int endTime = dateNow - 5;
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    CookieStr = (string)CacheHelper.GetCache(mobileReq.Token);
                    CacheHelper.RemoveCache(mobileReq.Token);
                }
                crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);
                if (crawler != null)
                    nowYear = DateTime.Parse(crawler.CrawlerDate).Year.ToString();

                #region 基本信息查询
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Analysis);

                mobile.Token = mobileReq.Token;
                mobile.BusName = mobileReq.Name;
                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.Mobile = mobileReq.Mobile;
                mobile.UpdateTime = crawler.CrawlerDate;

                //当前积分
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                if (result.StartsWith("{") && result.EndsWith("}"))
                {
                    JObject leftScore = (JObject)JsonConvert.DeserializeObject(result);
                    if (leftScore["result"].ToString() == "success")
                    {
                        mobile.Integral = leftScore["leftScore"].ToString(); //当前积分
                    }
                }
                //星级
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "starLevelInfor").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//div[@class='userStar']/span/font",
                    "text");
                if (results.Count > 0)
                {
                    mobile.StarLevel = results[0].Trim(); //星级
                }
                //puk 
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "pukInfor").FirstOrDefault().CrawlerTxt);
                if (result.StartsWith("{") && result.EndsWith("}"))
                {
                    JObject puk1 = (JObject)JsonConvert.DeserializeObject(result);
                    if (puk1["result"].ToString() == "success")
                    {
                        mobile.PUK = puk1["puk1"].ToString().Replace("PUK1:", ""); //PUK
                    }
                }
                //套餐品牌
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageBrandInfor").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//input [@id='currentProdName']", "value");
                if (results.Count > 0)
                {
                    mobile.PackageBrand = results[0].Trim(); //套餐品牌
                }

                //手机套餐
                for (int i = dateNow; i >= endTime; i--)
                {

                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "PackageInfor" + i).FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//table[@id='bill_page']/tr[2]/td[3]", "text");
                    if (results.Count > 0)
                    {
                        mobile.Package = regex.Replace(results[0], "");//手机套餐
                    }
                    if (!string.IsNullOrEmpty(mobile.Package))
                    {
                        break;
                    }
                }
                //获取个人资料缓存
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//div[@class='FramePerInfor']/*/table/tr[1]/td[3]", "text");
                if (results.Count > 0)
                {
                    mobile.Name = regex.Replace(results[0], ""); //姓名
                }
                results = HtmlParser.GetResultFromParser(result, "//div[@class='FramePerInfor']/*/table/tr[2]/td[2]", "inner");
                if (results.Count > 0)
                {
                    mobile.Mobile = results[0]; //手机号
                }
                results = HtmlParser.GetResultFromParser(result, "//div[@class='FramePerInfor']/*/table/tr[4]/td[2]", "inner");
                if (results.Count > 0)
                {
                    mobile.Regdate = DateTime.Parse(results[0]).ToString(Consts.DateFormatString11); //入网时间
                }
                results = HtmlParser.GetResultFromParser(result, "//div[@class='FramePerInfor']/*/table/tr[5]/td[3]", "inner");
                if (results.Count > 0)
                {
                    mobile.Idcard = results[0]; //证件号
                }
                results = HtmlParser.GetResultFromParser(result, "//div[@class='FramePerInfor']/*/table/tr[6]/td[3]", "inner");
                if (results.Count > 0)
                {
                    mobile.Address = results[0]; //地址
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "个人信息解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 账单查询
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis);

                for (int i = 0; i < 6; i++)
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//div[@class='and_netHallEbillTable2 floatLeft and_alertnateRows']/table/tr");
                    if (results.Count > 0)
                    {
                        MonthBill bill = new MonthBill();
                        foreach (string item in results)
                        {
                            List<string> info = HtmlParser.GetResultFromParser(item, "//span");
                            if (info.Count == 2)
                            {
                                if (info[0] == "套餐及固定费用")
                                {
                                    bill.PlanAmt = info[1];
                                }
                                if (info[0] == "合计")
                                {
                                    bill.TotalAmt = info[1].ToTrim().ToTrim("\r").ToTrim("\n").ToTrim("\t");
                                }
                            }
                        }
                        results = HtmlParser.GetResultFromParser(result, "//div[@class='and_netHallEbillTable1 gray0']/table/tr");
                        if (results.Count > 0)
                        {
                            foreach (string item in results)
                            {
                                List<string> info = HtmlParser.GetResultFromParser(item, "//td");
                                if (info.Count == 2)
                                {
                                    if (info[0] == "计费周期：")
                                    {
                                        if (info[1].Contains("至"))
                                        {
                                            bill.BillCycle = (Convert.ToDateTime(info[1].Split('至')[0])).ToString(Consts.DateFormatString12);
                                        }

                                    }
                                }
                            }
                        }
                        mobile.BillList.Add(bill);
                    }
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 通话详单

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Analysis);

                for (int i = dateNow; i >= endTime; i--)
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == EnumMobileDeatilType.Call.ToString() + i).FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//table[@id='bill_page']/tr[position()>1]", "inner");
                    foreach (var item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text");
                        if (tdRow.Count < 8)
                        {
                            continue;
                        }


                        var totalSecond = 0;
                        var usetime = regex.Replace(tdRow[5], "");
                        if (!string.IsNullOrEmpty(usetime))
                        {
                            totalSecond = CommonFun.ConvertDate(usetime);
                        }
                        Call call = new Call();
                        call.StartTime = DateTime.Parse(nowYear + "-" + regex.Replace(tdRow[0], "") + " " + regex.Replace(tdRow[1], "")).ToString(Consts.DateFormatString11);//起始时间  2015-10-14 17：53：12
                        call.CallPlace = regex.Replace(tdRow[2], "");//通信地点
                        call.InitType = regex.Replace(tdRow[3], "");//呼叫类型
                        call.OtherCallPhone = regex.Replace(tdRow[4], "");//对方号码
                        call.UseTime = totalSecond.ToString();//通信时长
                        call.CallType = regex.Replace(tdRow[6], "");//通信类型
                        if (tdRow.Count == 9)
                        {
                            call.SubTotal = regex.Replace(tdRow[8], "").ToTrim("元").ToDecimal(0);//实收通信费 
                        }
                        mobile.CallList.Add(call);
                    }
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 上网详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Analysis);

                for (int i = dateNow; i >= endTime; i--)
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == EnumMobileDeatilType.Net.ToString() + i).FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//table[@id='bill_page']/tr[position()>1]", "inner");
                    foreach (var item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text");
                        if (tdRow.Count < 8)
                        {
                            continue;
                        }
                        var totalSecond = 0;
                        var usetime = regex.Replace(tdRow[5], "");
                        if (!string.IsNullOrEmpty(usetime))
                        {
                            totalSecond = CommonFun.ConvertDate(usetime);
                        }

                        var totalFlow = CommonFun.ConvertGPRS(regex.Replace(tdRow[6], ""));

                        Net gprs = new Net();
                        gprs.StartTime = DateTime.Parse(nowYear + "-" + regex.Replace(tdRow[0], "") + " " + regex.Replace(tdRow[1], "")).ToString(Consts.DateFormatString11);//起始时间  2015-10-14 17：53：12
                        gprs.Place = regex.Replace(tdRow[2], "");//通信地点
                        gprs.PhoneNetType = regex.Replace(tdRow[3], "");//上网方式
                        gprs.NetType = regex.Replace(tdRow[4], "");//网络类型
                        gprs.UseTime = totalSecond.ToString();//上网时长 
                        gprs.SubFlow = totalFlow.ToString();//单次流量
                        mobile.NetList.Add(gprs);
                    }
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 短信详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Analysis);

                for (int i = dateNow; i >= endTime; i--)
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == EnumMobileDeatilType.SMS.ToString() + i).FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//table[@id='bill_page']/tr[position()>1]", "inner");
                    foreach (var item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text");
                        if (tdRow.Count < 8)
                        {
                            continue;
                        }
                        Sms sms = new Sms();
                        sms.StartTime = DateTime.Parse(nowYear + "-" + regex.Replace(tdRow[0], "") + " " + regex.Replace(tdRow[1], "")).ToString(Consts.DateFormatString11);//起始时间  2015-10-14 17：53：12
                        sms.SmsPlace = regex.Replace(tdRow[2], "");//通信地点
                        sms.OtherSmsPhone = regex.Replace(tdRow[3], "");//对方号码
                        sms.InitType = regex.Replace(tdRow[4], "");//通信方式
                        sms.SmsType = regex.Replace(tdRow[5], "");//信息类型 
                        if (tdRow.Count == 9)
                        {
                            sms.SubTotal = regex.Replace(tdRow[8], "").ToTrim("元").ToDecimal(0);//通信费 
                        }
                        mobile.SmsList.Add(sms);
                    }
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "河北移动手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "河北移动手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("河北移动手机账单解析异常", e);

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
    }
}
