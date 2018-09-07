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
using System.Text.RegularExpressions;
using Vcredit.Common.Constants;
using System.Web.Caching;
using System.Web;
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.NetSpider.Entity.Mongo.Log;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{

    public class GD : IMobileCrawler
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
        public VerCodeRes MobileInit(MobileReq mobileReq = null)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            Res.Token = CommonFun.GetGuidID();
            try
            {

                //第一步，初始化登录页面
                Url = "http://gd.189.cn/common/login.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //登录
                Url = "http://uam.gd.ct10000.com/portal/SSOLoginForWT.do?autoLogin=1&openOldUrl=1&autoLoginUrl=http://gd.189.cn/common/login.jsp&loginOldUri=null";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };

                //登录
                Url = "http://gd.189.cn/common/login.jsp?UATicket=-1&loginOldUri=null";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };

                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //验证码
                Url = "http://gd.189.cn/nCheckCode?kkadf=0.8805893063562878";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                FileOperateHelper.WriteVerCodeImage(Res.Token, httpResult.ResultByte);

                Res.StatusDescription = "广东电信初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;
                CacheHelper.SetCache(Res.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "广东电信初始化异常";
                Log4netAdapter.WriteError("广东电信初始化异常", e);
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

        public BaseRes MobileLogin(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
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
                    //CacheHelper.RemoveCache(mobileReq.Token);
                }
                //校验参数
                if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                var password = mobileReq.Password;
                var randomId = mobileReq.Vercode;
                var username = mobileReq.Mobile;

                Url = "http://gd.189.cn/dwr/exec/newLoginDwr.goLogin.dwr";
                postdata = "callCount=1&c0-scriptName=newLoginDwr&c0-methodName=goLogin&c0-id=5385_" + DateTime.Now.GetHashCode().ToString() + "&c0-param0=boolean%3Afalse&c0-param1=boolean%3Afalse&c0-param2=string%3A{2}&c0-param3=string%3A&c0-param4=string%3A2000004&c0-param5=string%3A{0}&c0-param6=string%3A00&c0-param7=string%3A{1}&c0-param8=string%3A&c0-param9=string%3A&xml=true";
                postdata = String.Format(postdata, mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                var SSORequestXML = Regex.Matches(httpResult.Html, "(?<=s4\\=\").*(?=\"\\;s0\\[3\\]\\=s4\\;)");
                if (SSORequestXML.Count > 0)
                {

                    Url = "https://uam.gd.ct10000.com/portal/SSOLoginForWT.do";
                    postdata = "area=&accountType=2000004&passwordType=00&loginOldUri=%2Fservice%2Fhome%2F&IFdebug=null&errorMsgType=&SSORequestXML={0}&sysType=2&from=new&isShowLoginRand=Y&areaSel=020&accountTypeSel=2000004&account={1}&mobilePassword=custPassword&password={2}&smsCode=&loginCodeRand={3}";
                    postdata = String.Format(postdata, SSORequestXML[0].ToString().ToUrlEncode(), mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        Method = "post",
                        Postdata = postdata,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    var SSL = Regex.Matches(httpResult.Html, "(?<=location.replace\\(\").*(?=\"\\)\\;)");
                    if (SSL.Count > 0)
                    {
                        httpItem = new HttpItem()
                        {
                            URL = SSL[0].ToString(),

                            CookieCollection = cookies,
                            Postdata = postdata,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    }
                    else
                    {
                        var Msg = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='Msg']", "value");
                        if (Msg.Count > 0)
                        {
                            Res.StatusDescription = Msg[0];
                            Res.StatusCode = ServiceConsts.StatusCode_fail;
                            return Res;
                        }
                    }

                }
                else
                {
                    Res.StatusDescription = "登录失败！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = "http://gd.189.cn/service/home/";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    CookieCollection = cookies,
                    Postdata = "loginRedirect=true",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://gd.189.cn/common/getIsLogin.jsp?";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://gd.189.cn/common/getCustInfo.jsp?";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_SendSMSAndVercode;
                CacheHelper.SetCache(Res.Token, cookies);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "广东电信手机登录异常";
                Log4netAdapter.WriteError("广东电信手机登录异常", e);
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

        public VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            VerCodeRes Res = new VerCodeRes();
            Res.Token = mobileReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                ////获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                    //CacheHelper.RemoveCache(token);
                }
                Url = "http://gd.189.cn/service/home/query/xd_index.html?loginRedirect=true";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                //发送验证码
                Url = "http://gd.189.cn/J/J10006.j";
                postdata = "a.c=0&a.u=user&a.p=pass&a.s=ECSS&d.payType=&d.numberTypeCode=";
                postdata = String.Format(postdata, mobileReq.Mobile);

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://gd.189.cn/service/home/query/xd_index.html",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };

                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //发送验证码
                Url = "http://gd.189.cn/J/J10036.j";
                postdata = "a.c=0&a.u=user&a.p=pass&a.s=ECSS&";
                postdata = String.Format(postdata, mobileReq.Mobile);

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "http://gd.189.cn/service/home/query/xd_index.html",
                    ResultCookieType = ResultCookieType.CookieCollection
                };

                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                //发送验证码
                Url = "http://gd.189.cn/J/J10059.j";
                postdata = "a.c=0&a.u=user&a.p=pass&a.s=ECSS";

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "http://gd.189.cn/service/home/query/xd_index.html",
                    ResultCookieType = ResultCookieType.CookieCollection
                };

                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //发送验证码
                Url = "http://gd.189.cn/J/J10038.j";
                postdata = "d.d01=0&a.c=0&a.u=user&a.p=pass&a.s=ECSS";

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "http://gd.189.cn/service/home/query/xd_index.html",
                    ResultCookieType = ResultCookieType.CookieCollection
                };

                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);



                //发送验证码
                Url = "http://gd.189.cn/J/J10060.j";
                postdata = "a.c=0&a.u=user&a.p=pass&a.s=ECSS";
                postdata = String.Format(postdata, mobileReq.Mobile);

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "http://gd.189.cn/service/home/query/xd_index.html",
                    ResultCookieType = ResultCookieType.CookieCollection
                };

                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //发送验证码
                Url = "http://gd.189.cn/J/J10032.j";
                postdata = "a.c=0&a.u=user&a.p=pass&a.s=ECSS";
                postdata = String.Format(postdata, mobileReq.Mobile);

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "http://gd.189.cn/service/home/query/xd_index.html",
                    ResultCookieType = ResultCookieType.CookieCollection
                };

                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //发送验证码
                Url = "http://gd.189.cn/J/J10037.j";
                postdata = "a.c=0&a.u=user&a.p=pass&a.s=ECSS&c.n=" + "客户状态查询".ToUrlDecode() + "&c.t=04&c.i=04-013&";
                postdata = String.Format(postdata, mobileReq.Mobile);

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "http://gd.189.cn/service/home/query/xd_index.html",
                    ResultCookieType = ResultCookieType.CookieCollection
                };

                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //发送验证码
                Url = "http://gd.189.cn/J/J10008.j";
                postdata = "a.c=0&a.u=user&a.p=pass&a.s=ECSS&";
                postdata = String.Format(postdata, mobileReq.Mobile);

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "http://gd.189.cn/service/home/query/xd_index.html",
                    ResultCookieType = ResultCookieType.CookieCollection
                };

                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                //发送验证码
                Url = "http://gd.189.cn/dwr/exec/commonAjax.getRandomCodeOper.dwr";
                postdata = "callCount=1&c0-scriptName=commonAjax&c0-methodName=getRandomCodeOper&c0-id=7427_1439370626122&c0-param0=boolean%3Afalse&c0-param1=boolean%3Afalse&c0-param2=string%3A020&c0-param3=string%3A{0}&c0-param4=string%3ACDMAp&xml=true";
                postdata = String.Format(postdata, mobileReq.Mobile);

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };

                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //验证码
                Url = "http://gd.189.cn/code";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);

                Res.StatusDescription = "短信码已发送";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
                CacheHelper.SetCache(mobileReq.Token, cookies);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "广东电信手机验证码发送异常";
                Log4netAdapter.WriteError("广东电信手机验证码发送异常", e);
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

        public BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            string Url = string.Empty;
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = CacheHelper.GetCache(mobileReq.Token) as CookieCollection;
                }

                var now = DateTime.Now;
                var startDate = new DateTime(now.Year, now.Month, 1).ToString(Consts.DateFormatString5);
                var endnow = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddDays(-1);
                var endDate = now.ToString(Consts.DateFormatString5);
                var month = now.ToString(Consts.DateFormatString7);

                ///积分查询
                Url = "http://gd.189.cn/J/J10009.j";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = String.Format("a.c=0&a.u=user&a.p=pass&a.s=ECSS&c.n=" + "语音清单".ToUrlEncode() + "&c.t=02&c.i=02-005-04&d.d01=call&d.d02={0}&d.d03={1}&d.d04={2}&d.d05=20&d.d06=1&d.d07={3}&d.d08=1", month, startDate, endDate, mobileReq.Smscode),
                    Method = "post",
                    Referer = "http://gd.189.cn/service/home/query/xd_index.html",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };

                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                var result = JsonConvert.DeserializeObject(httpResult.Html);
                JObject js = result as JObject;
                var msg = js["r"]["msg"];
                if (msg != null)
                {
                    if (msg.ToString() == "验证码不正确")
                    {
                        Res.StatusDescription = "验证码不正确！";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }

                Res.StatusDescription = "输入手机验证码和图片验证码，调用手机验证码验证接口";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Query;
                CacheHelper.SetCache(mobileReq.Token, cookies);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "广东电信手机验证码验证异常";
                Log4netAdapter.WriteError("广东电信手机验证码验证异常", e);
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

        public BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            ApplyLogDtl logDtl = new ApplyLogDtl("");
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            cookies = new CookieCollection();
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            CrawlerData crawler = new CrawlerData() { Token = mobileReq.Token, IdentityCard = mobileReq.IdentityCard, Name = mobileReq.Name, Mobile = mobileReq.Mobile, UserType = "1" };
            string Url = string.Empty;
            string postdata = String.Empty;
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                #region 基本信息
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Crawler);

                Url = "http://gd.189.cn/transaction/operApply1.jsp?operCode=ChangeCustInfoNew&in_cmpid=khzy-zcdh-yhzx-grxx-wdzl";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                var operCode = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='operCode']", "value");
                var divId = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='divId']", "value");
                var university = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='university']", "value");
                var custCode = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='custCode']", "value");
                var oldEmail = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='oldEmail']", "value");
                var latn_id = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='latn_id']", "value");
                var ApplyId = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='ApplyId']", "value");
                var number = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='number']", "value");
                var fromPage = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='fromPage']", "value");
                var toPage = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='toPage']", "value");
                var targetChk = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='targetChk']", "value");

                postdata = "operCode={0}&divId={1}&university={2}&custCode={3}&oldEmail={4}&latn_id={5}&ApplyId={6}&number={7}&fromPage={8}&toPage={9}&targetChk={10}";
                postdata = String.Format(postdata, operCode.Count > 0 ? operCode[0] : ""
                                                 , divId.Count > 0 ? divId[0] : ""
                                                 , university.Count > 0 ? university[0] : ""
                                                 , custCode.Count > 0 ? custCode[0] : ""
                                                 , oldEmail.Count > 0 ? oldEmail[0] : ""
                                                 , latn_id.Count > 0 ? latn_id[0] : ""
                                                 , ApplyId.Count > 0 ? ApplyId[0] : ""
                                                 , number.Count > 0 ? number[0] : ""
                                                 , fromPage.Count > 0 ? fromPage[0] : ""
                                                 , toPage.Count > 0 ? toPage[0] : ""
                                                 , targetChk.Count > 0 ? targetChk[0] : "");
                httpItem = new HttpItem()
                {
                    URL = "http://gd.189.cn/OperationInitAction2.do?OperCode=ChangeCustInfoNew&Latn_id=020",
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                httpItem = new HttpItem()
                {
                    URL = "http://gd.189.cn/J/J10006.j",
                    Postdata = "a.c=0&a.u=user&a.p=pass&a.s=ECSS&d.payType=&d.numberTypeCode=CDMA,CDMAp",
                    Method = "post",
                    CookieCollection = cookies,
                    Referer = "http://gd.189.cn/service/home/query/xf_tcsy.html?in_cmpid=khzy-zcdh-fycx-wdxf-tcsycx",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                ///套餐
                Url = "http://gd.189.cn/J/J10041.j";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = "a.c=0&a.u=user&a.p=pass&a.s=ECSS&c.n=" + "套餐使用查询".ToUrlEncode() + "&c.t=02&c.i=02-006&",
                    Method = "post",
                    Referer = "http://gd.189.cn/service/home/query/xf_tcsy.html?in_cmpid=khzy-zcdh-fycx-wdxf-tcsycx",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "packageBrandInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                ///积分查询
                Url = "http://gd.189.cn/J/J10032.j";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = "a.c=0&a.u=user&a.p=pass&a.s=ECSS",
                    Method = "post",
                    Referer = "http://gd.189.cn/service/home/query/jifen.html?in_cmpid=khzy-wdsy-wdjj-jfcx",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                ///PUK
                Url = "http://gd.189.cn/J/J10056.j";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = "a.c=0&a.u=user&a.p=pass&a.s=ECSS&c.n=" + "我的PUK码PIN码".ToUrlEncode() + "&c.t=04&c.i=04-003&",
                    Method = "post",
                    Referer = "http://gd.189.cn/service/home/info/xx_puk.html?in_cmpid==khzy-zcdh-yhzx-grxx-wdpuk",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "pukInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                ///姓名
                Url = "http://gd.189.cn/J/J10036.j";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = "a.c=0&a.u=user&a.p=pass&a.s=ECSS&",
                    Method = "post",
                    Referer = "http://gd.189.cn/service/home/query/xd_index.html",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "nameInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                Url = "http://gd.189.cn/prodInfo/serviceGetCustomerProduct.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://gd.189.cn/prodInfo/custBusiInfo.action?in_cmpid=khzy-yhzx-grxx-wddgyw",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "redateInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });



                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "基本信息抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 月消费情况
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Crawler);

                CrawlerBill(crawler, mobileReq);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  通话详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.Call, mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  短信详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.SMS, mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  上网详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.Net, mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "广东电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "广东电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("广东电信手机账单抓取异常", e);

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
        /// 读取账单
        /// </summary>
        /// <param name="mobileReq"></param>
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

            try
            {
                crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);

                #region 基本信息
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Analysis);

                mobile.Token = mobileReq.Token;
                mobile.BusName = mobileReq.Name;
                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.Mobile = mobileReq.Mobile;
                mobile.UpdateTime = crawler.CrawlerDate;

                crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);

                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "nameInfor").FirstOrDefault().CrawlerTxt);
                //姓名
                var resultjsonName = JsonConvert.DeserializeObject(result);
                {
                    JObject js = resultjsonName as JObject;
                    if (js != null)
                    {
                        JObject bdp = js["r"] as JObject;
                        if (bdp != null)
                        {
                            var r01 = bdp["r01"];
                            if (r01 != null)
                            {
                                mobile.Name = r01.ToString();
                            }

                        }

                    }
                }

                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);

                //证件类型
                var Idtype = HtmlParser.GetResultFromParser(result, "//input[@name='id_type']", "value");
                if (Idtype.Count > 0)
                {
                    mobile.Idtype = Idtype[0];
                }
                //证件号
                var Idcard = HtmlParser.GetResultFromParser(result, "//input[@name='id_num']", "value");
                if (Idcard.Count > 0)
                {
                    mobile.Idcard = Idcard[0];
                }

                //邮编
                var Postcode = HtmlParser.GetResultFromParser(result, "//input[@name='post_code']", "value");
                if (Postcode.Count > 0)
                {
                    mobile.Postcode = Postcode[0];
                }

                //Email
                var Email = HtmlParser.GetResultFromParser(result, "//input[@name='email']", "value");
                if (Email.Count > 0)
                {
                    mobile.Email = Email[0];
                }

                //地址
                var address = HtmlParser.GetResultFromParser(result, "//input[@name='post_addr']", "value");
                if (address.Count > 0)
                {
                    mobile.Address = address[0];
                }

                ///套餐
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageBrandInfor").FirstOrDefault().CrawlerTxt);
                var resultjson = JsonConvert.DeserializeObject(result);
                {
                    JObject js = resultjson as JObject;
                    if (js != null)
                    {
                        JObject bdp = js["r"] as JObject;
                        if (bdp != null)
                        {
                            JArray r0302 = bdp["r03"] as JArray;
                            if (r0302 != null)
                            {
                                if (r0302.Count > 0)
                                {
                                    mobile.Package = r0302[0]["r0302"].ToString();
                                }
                            }

                        }

                    }
                }

                ///装机时间
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "redateInfor").FirstOrDefault().CrawlerTxt);
                resultjson = JsonConvert.DeserializeObject(result);
                {
                    JObject js = resultjson as JObject;
                    if (js != null)
                    {
                        mobile.Regdate = DateTime.Parse(js["sanzhuangDay"].ToString()).ToString(Consts.DateFormatString11);
                    }
                }
                ///积分查询
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                resultjson = JsonConvert.DeserializeObject(result);
                {
                    JObject js = resultjson as JObject;
                    if (js != null)
                    {
                        JObject bdp = js["r"] as JObject;
                        if (bdp != null)
                        {
                            if (bdp["r01"] != null)
                            {
                                mobile.Integral = bdp["r01"].ToString();
                            }
                        }

                    }

                    mobile.Integral = String.IsNullOrWhiteSpace(mobile.Integral) ? "0" : mobile.Integral;

                }
                ///PUK
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "pukInfor").FirstOrDefault().CrawlerTxt);
                resultjson = JsonConvert.DeserializeObject(result);
                {
                    JObject js = resultjson as JObject;
                    if (js != null)
                    {
                        JObject bdp = js["r"] as JObject;
                        if (bdp != null)
                        {
                            if (bdp["r01"] != null)
                            {
                                mobile.PUK = bdp["r01"].ToString();
                            }
                        }

                    }

                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "个人信息解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 月消费情况
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis);

                AnalysisBill(crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 详单查询

                #region 通话详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Analysis);

                AnalysisDeatils(EnumMobileDeatilType.Call, crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 上网详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Analysis);

                AnalysisDeatils(EnumMobileDeatilType.Net, crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  短信详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Analysis);

                AnalysisDeatils(EnumMobileDeatilType.SMS, crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #endregion

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "广东电信手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "广东电信手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("广东电信手机账单解析异常", e);

                logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
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
            for (var i = 0; i <= 5; i++)
            {
                date = DateTime.Now.AddMonths(-i);
                if (i == 0)
                {
                    Url = "http://gd.189.cn/service/home/query/xf_sshf.html?in_cmpid=khzy-zcdh-fycx-wdxf-sshfcx";
                    httpItem = new HttpItem()
                    {
                        Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                        URL = Url,
                        Referer = "http://gd.189.cn/service/home/query/xf_sshf.html?in_cmpid=khzy-zcdh-fycx-wdxf-sshfcx",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                    Url = "http://gd.189.cn/common/getIsLogin.jsp?";
                    httpItem = new HttpItem()
                    {
                        Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                        URL = Url,
                        Referer = "http://gd.189.cn/service/home/query/xf_sshf.html?in_cmpid=khzy-zcdh-fycx-wdxf-sshfcx",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                    Url = "http://gd.189.cn/common/getCustInfo.jsp?";
                    httpItem = new HttpItem()
                    {
                        Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                        URL = Url,
                        Referer = "http://gd.189.cn/service/home/query/xf_sshf.html?in_cmpid=khzy-zcdh-fycx-wdxf-sshfcx",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                    Url = "http://gd.189.cn/common/getIsLogin.jsp?";
                    httpItem = new HttpItem()
                    {
                        Accept = "*/*",
                        URL = Url,
                        Referer = "http://gd.189.cn/service/home/query/xf_sshf.html?in_cmpid=khzy-zcdh-fycx-wdxf-sshfcx",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                    Url = "http://gd.189.cn/J/J10006.j";
                    postdata = string.Format("a.c=0&a.u=user&a.p=pass&a.s=ECSS&d.payType=&d.numberTypeCode=CDMA");
                    httpItem = new HttpItem()
                    {
                        Accept = "application/json, text/javascript, */*; q=0.01",
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Referer = "http://gd.189.cn/service/home/query/xf_sshf.html?in_cmpid=khzy-zcdh-fycx-wdxf-sshfcx",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                    Url = "http://gd.189.cn/J/J10059.j";
                    postdata = string.Format("a.c=0&a.u=user&a.p=pass&a.s=ECSS");
                    httpItem = new HttpItem()
                    {
                        Accept = "application/json, text/javascript, */*; q=0.01",
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Referer = "http://gd.189.cn/service/home/query/xf_sshf.html?in_cmpid=khzy-zcdh-fycx-wdxf-sshfcx",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    string msg = CommonFun.GetMidStr(httpResult.Html, "msg\":\"", "\"}");
                    if (msg == "只支持后付费移动电话号码查询！")
                        return;
                }
                else
                {
                    Url = string.Format("http://gd.189.cn/J/J10053.j");
                    postdata = string.Format("a.c=0&a.u=user&a.p=pass&a.s=ECSS&c.n=璐﹀崟鏌ヨ&c.t=02&c.i=02-004&d.d01={0}&d.d02=1&d.d03=&d.d04=", date.ToString("yyyyMM"));
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                }
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
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
            for (var i = 0; i <= 5; i++)
            {
                if (crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault() == null)
                    continue;
                PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                bill = new MonthBill();
                bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);
                if (i == 0)
                {
                    var js = JsonConvert.DeserializeObject(PhoneBillStr);
                    JObject job = js as JObject;
                    if (job == null) continue;
                    JObject objr = job["r"] as JObject;
                    if (objr == null) continue;
                    if (objr["code"].ToString() == "000")
                    {
                        //总金额
                        JObject obj04 = objr["r04"] as JObject;
                        if (obj04 == null) continue;
                        bill.TotalAmt = obj04["r0404"].ToString();
                        //套餐金额
                        JArray arr06 = objr["r06"] as JArray;
                        if (arr06.Count < 1) continue;
                        for (int j = 0; j < arr06.Count; j++)
                        {
                            if (arr06[j]["r0601"].ToString() == "16150")
                            {
                                bill.PlanAmt = arr06[j]["r0603"].ToString();
                                break;
                            }
                        }
                    }
                }
                else
                {
                    var js = JsonConvert.DeserializeObject(PhoneBillStr);
                    JObject job = js as JObject;
                    if (job == null) continue;
                    JObject objr = job["r"] as JObject;
                    if (objr == null) continue;
                    if (objr["code"].ToString() == "000")
                    {
                        //总金额
                        JObject obj02 = objr["r02"] as JObject;
                        if (obj02 == null) continue;
                        bill.TotalAmt = obj02["r0201"].ToString();
                        //套餐金额
                        JObject obj03 = objr["r03"] as JObject;
                        if (obj03 == null) continue;
                        JArray obj0302 = obj03["r0302"] as JArray;
                        if (obj0302.Count < 1) continue;
                        bill.PlanAmt = obj0302[0]["r030208"][0]["r03020802"][0]["r0302080202"].ToString();
                    }
                }
                ///添加账单
                mobile.BillList.Add(bill);
            }
        }

        /// <summary>
        /// 抓取手机详单
        /// </summary>
        /// <param name="queryType">call 获取通话记录，data 上网详单 note 短信详单</param>
        /// <returns></returns>
        private void CrawlerDeatils(EnumMobileDeatilType type, MobileReq mobileReq, CrawlerData crawler)
        {
            string Url = string.Empty;
            var startDate = String.Empty;
            var endDate = String.Empty;
            var month = String.Empty;
            string postdata = String.Empty;
            DateTime now;
            DateTime endnow;
            var cn = String.Empty;
            DateTime openTime = DateTime.MinValue;
            string queryType = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                queryType = "call";
            else if (type == EnumMobileDeatilType.SMS)
                queryType = "note";
            else
                queryType = "data";

            Url = "http://gd.189.cn/J/J10037.j";
            httpItem = new HttpItem()
            {
                URL = Url,
                Postdata = "a.c=0&a.u=user&a.p=pass&a.s=ECSS&c.n=" + "客户状态查询".ToUrlEncode() + "&c.t=04&c.i=04-013&",
                Method = "post",
                Referer = "http://gd.189.cn/service/home/query/xd_index.html",
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };

            httpResult = httpHelper.GetHtml(httpItem);
            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
            var resultjson = JsonConvert.DeserializeObject(httpResult.Html);
            {
                JObject js = resultjson as JObject;
                if (js != null)
                {
                    JObject bdp = js["r"] as JObject;
                    if (bdp != null)
                    {
                        JArray r01 = bdp["r01"] as JArray;
                        if (r01 != null)
                        {
                            if (r01.Count > 0)
                            {
                                var time = r01[1]["r0104"].ToString();

                                openTime = Convert.ToDateTime(time.Substring(0, 4) + "/" + time.Substring(4, 2) + "/" + time.Substring(6, 2));
                            }
                        }

                    }

                }
            }

            if (queryType == "call")
            {
                cn = "语音清单".ToUrlEncode();
            }
            else if (queryType == "note")
            {
                cn = "短信清单".ToUrlEncode();
            }
            else
            {
                cn = "数据清单".ToUrlEncode();
            }

            for (var i = 0; i <= 5; i++)
            {

                now = DateTime.Now.AddMonths(-i);
                startDate = new DateTime(now.Year, now.Month, 1).ToString(Consts.DateFormatString5);
                endnow = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddDays(-1);
                if (i == 0)
                {
                    endDate = DateTime.Now.ToString(Consts.DateFormatString5);
                }
                else
                {
                    endDate = endnow.ToString(Consts.DateFormatString5);
                }
                if (Convert.ToInt32(openTime.ToString(Consts.DateFormatString7)) > Convert.ToInt32(now.ToString(Consts.DateFormatString7)))
                {
                    continue;
                }
                else if (Convert.ToInt32(openTime.ToString(Consts.DateFormatString7)) == Convert.ToInt32(now.ToString(Consts.DateFormatString7)))
                {
                    startDate = openTime.ToString(Consts.DateFormatString5);
                }
                month = now.ToString(Consts.DateFormatString7);

                Url = "http://gd.189.cn/J/J10009.j";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = String.Format("a.c=0&a.u=user&a.p=pass&a.s=ECSS&c.n=" + cn + "&c.t=02&c.i=02-005-04&d.d01=" + queryType + "&d.d02={0}&d.d03={1}&d.d04={2}&d.d05=20&d.d06=1&d.d07={3}&d.d08=1", month, startDate, endDate, queryType != "data" ? mobileReq.Smscode : mobileReq.Vercode),
                    Method = "post",
                    Referer = "http://gd.189.cn/service/home/query/xd_index.html",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };

                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + "0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

            }
        }

        /// <summary>
        /// 解析手机详单
        /// </summary>
        /// <param name="queryType">call 获取通话记录，data 上网详单 note 短信详单</param>
        /// <returns></returns>
        private void AnalysisDeatils(EnumMobileDeatilType type, CrawlerData crawler, Basic mobile)
        {
            string PhoneCostStr = string.Empty;

            for (var i = 0; i <= 5; i++)
            {
                var resulthtml = crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1))).FirstOrDefault();
                if (resulthtml == null) continue;
                PhoneCostStr = System.Text.Encoding.Default.GetString(resulthtml.CrawlerTxt);
                var result = JsonConvert.DeserializeObject(PhoneCostStr);
                JObject js = result as JObject;
                if (js == null) continue;
                JObject bdp = js["r"] as JObject;
                if (bdp == null) continue;
                JArray r0302 = bdp["r03"] as JArray;
                if (r0302.Count > 0)
                {
                    for (var j = 0; j < r0302.Count; j++)
                    {
                        if (type == EnumMobileDeatilType.Call)
                        {
                            var totalSecond = 0;
                            var usetime = r0302[j][3].ToString();
                            if (!string.IsNullOrEmpty(usetime))
                            {
                                totalSecond = CommonFun.ConvertDate(usetime);
                            }

                            Call phoneCall = new Call();

                            if (r0302[j][0] != null)
                            {
                                phoneCall.CallType = r0302[j][0].ToString();
                            }

                            if (r0302[j][2] != null)
                            {
                                phoneCall.StartTime = DateTime.Parse(r0302[j][2].ToString()).ToString(Consts.DateFormatString11);
                            }

                            if (r0302[j][4] != null)
                            {
                                phoneCall.SubTotal = r0302[j][4].ToString().ToDecimal().Value;
                            }

                            if (r0302[j][3] != null)
                            {
                                phoneCall.UseTime = totalSecond.ToString();
                            }
                            if (r0302[j][1] != null)
                            {
                                phoneCall.OtherCallPhone = r0302[j][1].ToString();
                            }
                            if (r0302[j][6] != null)
                            {
                                phoneCall.CallPlace = r0302[j][6].ToString();
                            }
                            if (r0302[j][5] != null)
                            {
                                phoneCall.InitType = r0302[j][5].ToString();
                            }

                            mobile.CallList.Add(phoneCall);
                        }
                        else if (type == EnumMobileDeatilType.SMS)
                        {
                            Sms phoneSMS = new Sms();
                            if (r0302[j][4] != null)
                            {
                                phoneSMS.SmsType = r0302[j][4].ToString();
                            }
                            if (r0302[j][0] != null)
                            {
                                phoneSMS.OtherSmsPhone = r0302[j][0].ToString();
                            }

                            if (r0302[j][2] != null)
                            {
                                phoneSMS.StartTime = DateTime.Parse(r0302[j][2].ToString()).ToString(Consts.DateFormatString11);
                            }

                            if (r0302[j][3] != null)
                            {
                                phoneSMS.SubTotal = r0302[j][3].ToString().ToDecimal().Value;
                            }
                            mobile.SmsList.Add(phoneSMS);
                        }
                        else if (type == EnumMobileDeatilType.Net)
                        {
                            var totalSecond = 0;
                            var usetime = r0302[j][2].ToString();
                            if (!string.IsNullOrEmpty(usetime))
                            {
                                totalSecond = CommonFun.ConvertDate(usetime);
                            }

                            Net phoneGPRS = new Net();
                            if (r0302[j][1] != null)
                            {
                                phoneGPRS.StartTime = DateTime.Parse(r0302[j][1].ToString()).ToString(Consts.DateFormatString11);
                            }
                            if (r0302[j][7] != null)
                            {
                                phoneGPRS.SubTotal = r0302[j][7].ToString().ToDecimal().Value;
                            }
                            if (r0302[j][2] != null)
                            {
                                phoneGPRS.UseTime = totalSecond.ToString();
                            }
                            if (r0302[j][5] != null)
                            {
                                phoneGPRS.Place = r0302[j][5].ToString();
                            }
                            if (r0302[j][3] != null)
                            {
                                phoneGPRS.SubFlow = r0302[j][3].ToString().ToString();
                            }
                            if (r0302[j][4] != null)
                            {
                                phoneGPRS.NetType = r0302[j][4].ToString();
                            }
                            mobile.NetList.Add(phoneGPRS);
                        }
                    }

                }

            }

        }

        #endregion
    }
}
