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
using Newtonsoft.Json.Linq;
using Vcredit.NetSpider.DataAccess.Mongo;
using Newtonsoft.Json;
using Vcredit.NetSpider.Entity.Mongo.Log;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{
    public class JS : IMobileCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        ApplyLogMongo logMongo = new ApplyLogMongo();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        #endregion

        public VerCodeRes MobileInit(MobileReq mobileReq = null)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            VerCodeRes Res = new VerCodeRes();
            cookies = new CookieCollection();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                //第一步，初始化登录页面
                Url = "http://189.cn/dqmh/login/login/virtualInclude.jsp";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Referer = "http://189.cn/",
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //第二步，获取验证码
                Url = "http://189.cn/dqmh/createCheckCode.do?method=checkcode&date=" + GetTimeStamp();
                httpItem = new HttpItem()
                {
                    Accept = "image/png,image/*;q=0.8,*/*;q=0.5",
                    URL = Url,
                    Referer = "http://189.cn/dqmh/login/login/virtualInclude.jsp",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);

                Res.StatusDescription = "江苏电信初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "江苏电信初始化异常";
                Log4netAdapter.WriteError("江苏电信初始化异常", e);
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
            cookies = new CookieCollection();
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                //校验参数
                if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = "http://189.cn/dqmh/chongzhi.do?method=queryPhoneNumberAccount";
                postdata = string.Format("phoneNumber={0}", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    Referer = "http://189.cn/dqmh/login/login/virtualInclude.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                var password = mobileReq.Password;
                var mobile = mobileReq.Mobile;
                AES.Key = "login.189.cn";
                AES.IV = "1234567812345678";
                mobile = AES.AESEncryptByMD5(mobile);
                password = AES.AESEncryptByMD5(password);
                //password = "6LMmoFBDEVE3/G4/tih7LQ==";
                //mobile = "0hN2lpr8IboMv7zrDdG5IA==";

                Url = "http://189.cn/dqmh/login/virtual.do?method=JiangSuLogin";
                postdata = string.Format("account={0}&accounttype=201&password={1}&passwdtype=01&verificationCode={2}&tjren=%E5%8F%AF%E4%B8%8D%E5%A1%AB&provincecode=11", mobile, password, mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://189.cn/dqmh/login/login/virtualInclude.jsp",
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string code = CommonFun.GetMidStr(httpResult.Html, "code\": \"", "\",");
                if (code != "0")
                {
                    Res.StatusDescription = CommonFun.GetMidStr(httpResult.Html, "msg\": \"", "\",");
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/dqmh/my189/initMy189home.do";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Referer = "http://www.189.cn/dqmh/login/login/virtualInclude.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_SendSMS;
                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "江苏电信登录异常";
                Log4netAdapter.WriteError("江苏电信登录异常", e);
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
            cookies = new CookieCollection();
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
                postdata = string.Format("fastcode=00740810");
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://js.189.cn/queryCheckSecondPwdAction.action";
                postdata = string.Format("inventoryVo.accNbr={0}&inventoryVo.productId=2000004", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    Accept = "text/plain, */*; q=0.01",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    Referer = "http://js.189.cn/feeQuery/query_qd.action?fastcode=00740810",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://js.189.cn/queryValidateSecondPwdAction.action";
                postdata = string.Format("inventoryVo.accNbr={0}&inventoryVo.productId=2000004&inventoryVo.action=generate&inventoryVo.inputRandomPwd=", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://js.189.cn/feeQuery/query_qd.action?fastcode=00740810",
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (httpResult.Html == "本次请求并未返回任何数据")
                {
                    Res.StatusDescription = "本次请求并未返回任何数据";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string code = jsonParser.GetResultFromParser(httpResult.Html, "TSR_CODE");
                if (code != "0000")
                {
                    Res.StatusDescription = jsonParser.GetResultFromParser(httpResult.Html, "TSR_MSG");
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "输入手机验证码，调用手机验证码验证接口";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;

                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "江苏电信手机验证码发送异常";
                Log4netAdapter.WriteError("江苏电信手机验证码发送异常", e);
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
            cookies = new CookieCollection();
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                Url = "http://js.189.cn/queryValidateSecondPwdAction.action";
                postdata = string.Format("inventoryVo.accNbr={0}&inventoryVo.productId=2000004&inventoryVo.action=check&inventoryVo.inputRandomPwd={1}", mobileReq.Mobile, mobileReq.Smscode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://js.189.cn/service/bill?tabFlag=billing2",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (httpResult.Html == "本次请求并未返回任何数据")
                {
                    Res.StatusDescription = "本次请求并未返回任何数据";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string code = jsonParser.GetResultFromParser(httpResult.Html, "TSR_CODE");
                if (code != "0000")
                {
                    Res.StatusDescription = jsonParser.GetResultFromParser(httpResult.Html, "TSR_MSG");
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.StatusDescription = "江苏电信手机验证码验证成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Query;

                CacheHelper.SetCache(mobileReq.Token, cookies);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "江苏电信手机验证码验证异常";
                Log4netAdapter.WriteError("江苏电信手机验证码验证异常", e);
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
            string postdata = string.Empty;
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                #region 个人信息
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Crawler);

                Url = "http://js.189.cn/getSessionInfo.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                //积分
                Url = "http://js.189.cn/infoQuery_queryintegralInfoOfData.action";
                postdata = string.Format("Action=post&Name=queryintegralInfoOfData");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                //套餐
                Url = string.Format("http://js.189.cn/queryWare.action?userID={0}&userType=2000004", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "packageInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                object packageobj = JsonConvert.DeserializeObject(httpResult.Html);
                JArray packagebdp = packageobj as JArray;

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "基本信息抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 月消费情况
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Crawler);

                CrawlerBill(crawler, mobileReq.Mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 话费详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.Call, mobileReq.Mobile, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 短信详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.SMS, mobileReq.Mobile, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 上网详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.Net, mobileReq.Mobile, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                //保存
                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "江苏电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusDescription = "江苏电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("江苏电信手机账单抓取异常", e);

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
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            MobileMongo mobileMongo = new MobileMongo(appDate);
            CrawlerData crawler = new CrawlerData();
            Basic mobile = new Basic();
            Call call = null;
            Net gprs = null;
            Sms sms = null;
            string result = string.Empty;
            List<JObject> results = new List<JObject>();
            try
            {
                crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);

                #region 个人信息
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Analysis);

                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                object Infoobj = JsonConvert.DeserializeObject(result);
                JObject Infojs = Infoobj as JObject;
                if (Infojs != null)
                {
                    mobile.Mobile = Infojs["productId"].ToString();
                    mobile.Email = Infojs["email"].ToString();
                    mobile.Idcard = Infojs["indentCode"].ToString();
                    object pcobj = JsonConvert.DeserializeObject(Infojs["productCollection"].ToString().Substring(2, Infojs["productCollection"].ToString().Length - 4));
                    JObject Infobdp = pcobj as JObject;
                    mobile.Name = ((JObject)Infobdp["accoutInfo"])["partyName"].ToString();
                    JObject productInfo = Infobdp["productInfo"] as JObject;
                    mobile.Address = productInfo["address"].ToString();
                    mobile.Regdate = productInfo["servCreateDate"].ToString();
                    if (!mobile.Regdate.IsEmpty())
                        mobile.Regdate = DateTime.Parse(mobile.Regdate.Substring(0, 4) + "-" + mobile.Regdate.Substring(4, 2) + "-" + mobile.Regdate.Substring(6, 2)).ToString(Consts.DateFormatString11);
                    mobile.StarLevel = ((JObject)((JObject)productInfo["prodSegments"])["prodSegment"])["segmentName"].ToString();
                }

                //积分
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                mobile.Integral = httpResult.Html != "ERROR" ? httpResult.Html : "0";

                //套餐
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault().CrawlerTxt);
                object packageobj = JsonConvert.DeserializeObject(httpResult.Html);
                JArray packagebdp = packageobj as JArray;
                mobile.Package = ((JObject)packagebdp[0])["offerName"].ToString();
               
                mobile.Token = mobileReq.Token;
                mobile.BusName = mobileReq.Name;
                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.Mobile = mobileReq.Mobile;
                mobile.UpdateTime = crawler.CrawlerDate;

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

                #region 话费详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Analysis);

                results = GetAnalysisDeatils(EnumMobileDeatilType.Call, crawler);
                foreach (JObject arrItem in results)
                {
                    if (arrItem != null)
                    {
                        call = new Call();
                        call.StartTime = DateTime.Parse(arrItem["startDateNew"].ToString() + " " + arrItem["startTimeNew"].ToString()).ToString(Consts.DateFormatString11);
                        call.CallPlace = arrItem["areaCode"].ToString();
                        call.InitType = arrItem["ticketType"].ToString().Substring(0, 2);
                        call.OtherCallPhone = arrItem["nbr"].ToString();
                        call.UseTime = arrItem["duartionCh"].ToString();
                        call.CallType = arrItem["ticketType"].ToString().Substring(2, 2);
                        call.SubTotal = arrItem["ticketChargeCh"].ToString().ToDecimal(0);
                        mobile.CallList.Add(call);
                    }
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 短信详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Analysis);

                results = GetAnalysisDeatils(EnumMobileDeatilType.SMS, crawler);//短信
                foreach (JObject arrItem in results)
                {
                    if (arrItem != null)
                    {
                        sms = new Sms();
                        call.StartTime = DateTime.Parse(arrItem["startDateNew"].ToString() + " " + arrItem["startTimeNew"].ToString()).ToString(Consts.DateFormatString11);
                        sms.OtherSmsPhone = arrItem["nbr"].ToString();
                        sms.InitType = arrItem["ticketType"].ToString().Substring(2, 1);
                        sms.SmsType = arrItem["ticketType"].ToString().Substring(0, 2);
                        sms.SubTotal = arrItem["ticketChargeCh"].ToString().ToDecimal(0);
                        mobile.SmsList.Add(sms);
                    }
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 上网详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Analysis);

                results = GetAnalysisDeatils(EnumMobileDeatilType.Net, crawler);
                foreach (JObject arrItem in results)
                {
                    if (arrItem != null)
                    {
                        gprs = new Net();
                        call.StartTime = DateTime.Parse(arrItem["START_TIME"].ToString()).ToString(Consts.DateFormatString11);
                        gprs.Place = arrItem["TICKET_TYPE"].ToString();
                        gprs.PhoneNetType = arrItem["SERVICE_TYPE"].ToString().Substring(3, 3);
                        gprs.NetType = arrItem["SERVICE_TYPE"].ToString().Substring(0, 2);
                        gprs.UseTime = arrItem["DURATION_CH"].ToString();
                        gprs.SubFlow = arrItem["BYTES_CNT"].ToString();
                        gprs.SubTotal = arrItem["TICKET_CHARGE_CH"].ToString().ToDecimal(0);
                        mobile.NetList.Add(gprs);
                    }
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                //保存
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "江苏电信手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);

            }
            catch (Exception e)
            {
                Res.StatusDescription = "江苏电信手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("江苏电信手机账单解析异常", e);

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
        /// 抓取手机详单
        /// </summary>
        /// <param name="queryType">queryVoiceMsgAction:通话；queryNewDataMsgListAction:上网；mobileInventoryActio:短信</param>
        /// <param name="queryType">手机号</param>
        /// <returns></returns>
        public List<JObject> GetDeatils(string queryType, string mobile)
        {
            List<JObject> results = new List<JObject>();
            string PhoneCostStr = string.Empty;
            DateTime first = DateTime.Now;
            DateTime last = DateTime.Now;
            string postdata = string.Empty;
            for (int i = 0; i < 6; i++)
            {
                first = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-i);
                last = first.AddMonths(1).AddDays(-1);
                string Url = string.Format("http://js.189.cn/{0}.action", queryType);
                postdata = string.Format("inventoryVo.accNbr={0}&inventoryVo.getFlag={1}&inventoryVo.begDate={2}&inventoryVo.endDate={3}&inventoryVo.family=4&inventoryVo.accNbr97=&inventoryVo.productId=4&inventoryVo.acctName={0}", mobile, (i == 0 ? "0" : "1"), first.ToString("yyyyMMdd"), last.ToString("yyyyMMdd"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.Html == "远程服务器返回错误: (500) 内部服务器错误。") continue;
                object obj = JsonConvert.DeserializeObject(httpResult.Html);
                JObject js = obj as JObject;
                if (js != null && js["TSR_CODE"].ToString() == "0")
                {
                    JArray Result = js["items"] as JArray;
                    foreach (JObject jobj in Result)
                        results.Add(jobj);
                }
            }
            return results;
        }

        /// <summary>
        /// 抓取账单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private void CrawlerBill(CrawlerData crawler, string mobile)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            DateTime date = DateTime.Now;
            for (int i = 0; i <= 5; i++)
            {
                if (i == 0)
                {
                    //当月
                    Url = string.Format("http://js.189.cn/chargeQuery/chargeQuery_queryRealTimeCharges.action?productType=4&changeUserID={0}", mobile);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                }
                else
                {
                    //前五个月账单
                    date = date.AddMonths(-1);
                    Url = string.Format("http://js.189.cn/chargeQuery/chargeQuery_queryCustBill.action?billingCycleId={0}&queryFlag=0&productId=2&accNbr={1}", date.ToString("yyyyMM"), mobile);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                }
                httpResult = httpHelper.GetHtml(httpItem);
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
            for (int i = 0; i <= 5; i++)
            {
                PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                bill = new MonthBill();
                if (i == 0)
                {
                    object obj = JsonConvert.DeserializeObject(httpResult.Html);
                    JObject js = obj as JObject;
                    JArray billdata = js["items"] as JArray;
                    if (billdata != null && billdata.Count > 0)
                    {
                        bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).ToString(Consts.DateFormatString12);
                        bill.PlanAmt = (((JObject)billdata[0])["acctItemCharge"].ToString().ToDecimal(0) / 100).ToString();
                        bill.TotalAmt = (js["chargeCnt"].ToString().ToDecimal(0) / 100).ToString();
                        mobile.BillList.Add(bill);
                    }
                }
                else
                {
                    object historyobj = JsonConvert.DeserializeObject(httpResult.Html);
                    JObject historyjs = historyobj as JObject;
                    if (historyjs == null) continue;
                    JArray feeCountAndPresendtedFee = historyjs["feeCountAndPresendtedFee"] as JArray;
                    if (feeCountAndPresendtedFee != null && feeCountAndPresendtedFee.Count > 0)
                    {
                        bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);
                        JArray newStatisticalList = historyjs["newStatisticalList"] as JArray;
                        if (newStatisticalList != null && newStatisticalList.Count > 0)
                            bill.PlanAmt = newStatisticalList[0]["itemCharge"].ToString();
                        bill.TotalAmt = feeCountAndPresendtedFee[0]["itemCharge"].ToString();
                        mobile.BillList.Add(bill);
                    }
                }
                mobile.BillList.Add(bill);
            }
        }

        /// <summary>
        /// 抓取手机详单
        /// </summary>
        /// <param name="queryType">queryVoiceMsgAction:通话；queryNewDataMsgListAction:上网；mobileInventoryActio:短信</param>
        /// <param name="queryType">手机号</param>
        /// <returns></returns>
        public void CrawlerDeatils(EnumMobileDeatilType type, string mobile, CrawlerData crawler)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            DateTime first = DateTime.Now;
            DateTime last = DateTime.Now;
            string queryType = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                queryType = "queryVoiceMsgAction";
            else if (type == EnumMobileDeatilType.SMS)
                queryType = "mobileInventoryAction";
            else
                queryType = "queryNewDataMsgListAction";

            for (int i = 0; i < 6; i++)
            {
                first = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-i);
                last = first.AddMonths(1).AddDays(-1);
                Url = string.Format("http://js.189.cn/{0}.action", queryType);
                postdata = string.Format("inventoryVo.accNbr={0}&inventoryVo.getFlag={1}&inventoryVo.begDate={2}&inventoryVo.endDate={3}&inventoryVo.family=4&inventoryVo.accNbr97=&inventoryVo.productId=4&inventoryVo.acctName={0}", mobile, (i == 0 ? "0" : "1"), first.ToString("yyyyMMdd"), last.ToString("yyyyMMdd"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + "0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
            }
        }

        /// <summary>
        /// 解析手机详单
        /// </summary>
        /// <param name="queryType">queryVoiceMsgAction:通话；queryNewDataMsgListAction:上网；mobileInventoryActio:短信</param>
        /// <param name="queryType">手机号</param>
        /// <returns></returns>
        public List<JObject> GetAnalysisDeatils(EnumMobileDeatilType type, CrawlerData crawler)
        {
            List<JObject> results = new List<JObject>();
            string PhoneCostStr = string.Empty;
            for (int i = 0; i < 6; i++)
            {
                PhoneCostStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                if (PhoneCostStr == "远程服务器返回错误: (500) 内部服务器错误。") continue;
                object obj = JsonConvert.DeserializeObject(httpResult.Html);
                JObject js = obj as JObject;
                if (js != null && js["TSR_CODE"].ToString() == "0")
                {
                    JArray Result = js["items"] as JArray;
                    foreach (JObject jobj in Result)
                        results.Add(jobj);
                }
            }
            return results;
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        private static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 01, 01, 00, 00, 00, 0000);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }

        #endregion
    }
}
