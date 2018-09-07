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
using System.IO;
using System.Xml;
using System.Web;
using Vcredit.Common.Constants;
using System.Text.RegularExpressions;
using Vcredit.NetSpider.Entity.Mongo.Log;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{
    public class SC : ChinaNet
    {
        #region 公共变量

        string Ticket = String.Empty;
        string TxID = String.Empty;
        string PlatNO = String.Empty;
        string PRODNO = string.Empty;
        string startDayvalue = string.Empty;
        string endDayvalue = string.Empty;
        string PRODTYPE = string.Empty;
        string CITYCODE = string.Empty;
        string sessionid = string.Empty;
        string timetype = string.Empty;

        #endregion

        public override VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_SendSMS, mobileReq.Website);
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

                #region 发送动态验证码校验
                logDtl = new ApplyLogDtl("发送动态验证码校验");

                //获取查看状态
                Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
                postdata = string.Format("fastcode=01881193");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,

                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发送动态验证码校验checkMy189Session");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //获取查询参数
                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10023&toStUrl=http://sc.189.cn/service/bill/myquerynew.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发送动态验证码校验ssoLink");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                var infos = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@type='hidden']", "value");
                if (infos.Count > 0)
                {

                    PRODNO = infos[0];
                    PRODTYPE = PRODNO.Split('~')[1];
                    startDayvalue = infos[2];
                    endDayvalue = infos[3];
                    timetype = "0";
                    CITYCODE = infos[6];
                    sessionid = infos[7];
                }
                //注册登录
                Url = String.Format("http://sc.189.cn/LoginECSSO?PlatNO={0}&Ticket={1}&TxID={2}&SSOURL=http%3a%2f%2fsc.189.cn%2fservice%2fbill%2fmyquerynew.jsp", PlatNO, Ticket, TxID);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do"
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发送动态验证码校验LoginECSSO");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "发送动态验证码校验";
                appLog.LogDtlList.Add(logDtl);

                #endregion

                #region 发送动态验证码
                logDtl = new ApplyLogDtl("发送动态验证码");

                //发送验证码
                postdata = @"<buffalo-call>
                            <method>getPhsSmsCode</method>
                            <map>
                            <type>java.util.HashMap</type>
                            <string>PHONENUM</string>
                            <string>18180572359</string>
                            <string>PRODUCTID</string>
                            <string>50</string>
                            <string>CITYCODE</string>
                            <string>0128</string>
                            </map>

                            </buffalo-call>";
                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10023&toStUrl=http://sc.189.cn/BUFFALO/buffalo/commonOrder";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Method = "Post",
                    CookieCollection = cookies,
                    Postdata = postdata,
                    ContentType = "text/xml;charset=utf-8",
                    Referer = "http://sc.189.cn/service/bill/myquerynew.jsp",
                    ResultCookieType = ResultCookieType.CookieCollection,
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发送动态验证码ssoLink");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://sc.189.cn/BUFFALO/buffalo/commonOrder";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Method = "Post",
                    CookieCollection = cookies,
                    Postdata = postdata,
                    ContentType = "text/xml;charset=utf-8",
                    Referer = "http://sc.189.cn/service/bill/myquerynew.jsp?fastcode=01881193",
                    ResultCookieType = ResultCookieType.CookieCollection,
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发送动态验证码commonOrder");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (httpResult.Html == "本次请求并未返回任何数据")
                {
                    Res.StatusDescription = "本次请求并未返回任何数据";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                string res = getStringByElementName(httpResult.Html, "string");
                if (res != "1")
                {
                    Res.StatusDescription = "短信发送失败";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "发送动态验证码";
                appLog.LogDtlList.Add(logDtl);

                #endregion

                Res.StatusDescription = "四川电信手机验证码发送成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;

                CacheHelper.SetCache("infos", infos);
                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "四川电信手机验证码发送异常";
                Log4netAdapter.WriteError("四川电信手机验证码发送异常", e);

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

        public override BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_CheckSMS, mobileReq.Website);
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            cookies = new CookieCollection();
            string Url = string.Empty;
            string postdata = string.Empty;
            DateTime date = DateTime.Now;

            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                logDtl = new ApplyLogDtl("验证手机验证码");
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                if (CacheHelper.GetCache("infos") != null)
                {
                    var infos = (List<string>)CacheHelper.GetCache("infos");
                    if (infos.Count > 0)
                    {

                        PRODNO = infos[0];
                        PRODTYPE = PRODNO.Split('~')[1];
                        startDayvalue = infos[2];
                        endDayvalue = infos[3];
                        CITYCODE = infos[6];
                        sessionid = infos[7];
                    }
                }

                var s = sessionid.Replace("!", "%21");
                postdata = string.Format("PAGENO=1&PRODNO={0}&smsrand={1}&IdCardInput=&IdCardNo=&ck_month={2}&startDayvalue={3}&endDayvalue={4}&PRODTYPE={5}&timeType={6}&CITYCODE={7}&sessionid={8}&SHOWTICKETPWD=", PRODNO.ToUrlEncode(), mobileReq.Smscode, date.AddMonths(-2).ToString("yyyyMM"), startDayvalue, endDayvalue, PRODTYPE, "0", CITYCODE, s);
                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10023&toStUrl=http://sc.189.cn/service/bill/myQueryBill.jsp?PAGENO=1&" + postdata;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Get",
                    Referer = "http://sc.189.cn/service/bill/myquerynew.jsp?fastcode=01881193",
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
                if (httpResult.Html == "本次请求并未返回任何数据")
                {
                    Res.StatusDescription = "本次请求并未返回任何数据";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                if (!httpResult.Html.Contains("我的详单"))
                {
                    Res.StatusDescription = "四川电信手机验证码验证失败";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "四川电信手机验证码验证成功";
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
                Res.StatusDescription = "四川电信手机验证码验证异常";
                Log4netAdapter.WriteError("四川电信手机验证码验证异常", e);

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

        public override BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {
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

                #region 基本信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Crawler, mobileReq.Website);

                #region  基本信息
                logDtl = new ApplyLogDtl("基本信息");

                Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
                postdata = string.Format("fastcode=01931223");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    CookieCollection = cookies,
                    Postdata = postdata,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息checkMy189Session");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10023&toStUrl=http://sc.189.cn/service/manage/myuserinfo.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    //Method = "Post",
                    Encoding = Encoding.GetEncoding("gb2312"),
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
                #endregion

                #region 套餐
                logDtl = new ApplyLogDtl("套餐查询");

                Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
                postdata = string.Format("fastcode=01881190");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    CookieCollection = cookies,
                    Postdata = postdata,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "套餐查询校验checkMy189Session");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10023&toStUrl=http://sc.189.cn/service/manage/myNewBundle.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    //Method = "Post",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "套餐查询");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "packageInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "套餐查询抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }

                #endregion

                #region  puk
                logDtl = new ApplyLogDtl("PUK查询");

                //PUK码
                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10023&toStUrl=http://sc.189.cn/service/bill/myPuk.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "PUK查询");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "PUKInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "PUK查询抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                #endregion

                #region 积分
                logDtl = new ApplyLogDtl("手机积分");

                Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
                postdata = string.Format("fastcode=01911210");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "手机积分");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = "http://www.189.cn/dqmh/my189/initMy189home.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
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

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 月消费情况
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Crawler, mobileReq.Website);

                CrawlerBill(crawler, mobileReq);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 详单查询

                #region 话费详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.Call, mobileReq, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.SMS, mobileReq, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.Net, mobileReq, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion


                #endregion

                //保存
                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "四川电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusDescription = "四川电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("四川电信手机账单抓取异常", e);

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
        public override BaseRes MobileAnalysis(MobileReq mobileReq, DateTime appDate)
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
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                    var mobilenum = HtmlParser.GetResultFromParser(result, "//table[@class='show_table2']/tr[1]/th", "text")[0].Split('：')[1].Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");
                    mobile.Mobile = mobilenum;
                    results = HtmlParser.GetResultFromParser(result, "//table[@class='show_table2']/tr/td");
                    if (results.Count != 0)
                    {
                        mobile.Regdate = DateTime.Parse(results[5]).ToString(Consts.DateFormatString11);
                        mobile.Address = results[3];
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
                logDtl = new ApplyLogDtl("套餐查询");

                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//table/tr/td", "text");
                    if (results.Count != 0)
                    {
                        mobile.Package = results[0].Split('（')[0].Replace("\r", "").Replace("\n", "").Replace("\t", "");
                        mobile.PackageBrand = results[3].Replace("\r", "").Replace("\n", "").Replace("\t", "");
                    }

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "套餐查询";
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

                #region 积分
                logDtl = new ApplyLogDtl("手机积分");

                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//table[@class='usrr_wallet left']", "text");
                    if (results.Count > 0)
                    {
                        mobile.Integral = results[0].Split('：')[1].Replace("\r", "").Replace("\t", "").Replace("\n", "").Replace("兑换", "").Replace(" ", ""); //积分 
                    }

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "手机积分";
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

                #region PUK码
                logDtl = new ApplyLogDtl("PUK查询");

                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "PUKInfor").FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//span[@id='Puk_One']", "inner");
                    if (results.Count != 0)
                    {
                        mobile.PUK = results[0];
                    }
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "PUK查询";
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

                AnalysisBill(crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 详单查询

                #region 通话详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Analysis, mobileReq.Website);

                AnalysisDeatils(EnumMobileDeatilType.Call, crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region  短信详单
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

                CrawlerCommonFun.CheckColumn(mobile, columnLog, "ChinaMobile");
                //保存
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "四川电信手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "四川电信手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("四川电信手机账单解析异常", e);

                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
                loglist.Add(appLog);
            }
            finally
            {
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
            string acctype = string.Empty;

            logDtl = new ApplyLogDtl("账单抓取校验");
            Url = string.Format("http://www.189.cn/dqmh/my189/checkMy189Session.do");
            postdata = string.Format("fastcode=01881192");
            httpItem = new HttpItem()
            {
                URL = Url,
                Method = "POST",
                CookieCollection = cookies,
                Postdata = postdata,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "账单抓取校验");
            if (httpResult.StatusCode != HttpStatusCode.OK) return;
            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
            logDtl.StatusCode = ServiceConsts.StatusCode_success;
            logDtl.Description = "账单抓取校验校验成功";
            appLog.LogDtlList.Add(logDtl);
            acctype = PRODTYPE;
            //六个月账单
            for (int i = 0; i <= 5; i++)
            {
                date = DateTime.Now.AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月账单抓取");

                Url = string.Format("http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10023&toStUrl=http://sc.189.cn/service/bill/userbillajax.jsp?account={0}&accountType={1}&month={2}&citycode=0128", mobileReq.Mobile, acctype, date.ToString("yyyyMM"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, date.ToString(Consts.DateFormatString7) + "月账单抓取");
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
        private void AnalysisBill(CrawlerData crawler, Basic mobile)
        {
            string PhoneBillStr = string.Empty;
            List<string> infos = new List<string>();
            MonthBill bill = null;
            DateTime date;

            for (var i = 0; i <= 5; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis, date.ToString("yyyy-MM-dd") + "月账单解析");

                try
                {
                    PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    infos = HtmlParser.GetResultFromParser(PhoneBillStr, "//div[@class='ct']");
                    if (infos.Count > 0)
                    {
                        bill = new MonthBill();
                        bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);
                        bill.PlanAmt = HtmlParser.GetResultFromParser(infos[0], "//ul/li/span[2]")[0];
                        bill.TotalAmt = HtmlParser.GetResultFromParser(infos[0], "//div/span[2]")[0];
                        ///添加账单
                        mobile.BillList.Add(bill);
                    }
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = date.ToString("yyyy-MM-dd") + "月账单解析异常：" + e.Message;
                    appLog.LogDtlList.Add(logDtl);
                    continue;
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = date.ToString("yyyy-MM-dd") + "月账单解析成功";
                appLog.LogDtlList.Add(logDtl);
            }
        }

        /// <summary>
        /// 抓取手机详单
        /// </summary>
        /// <param name="queryType">1 获取通话记录，3 上网详单 2 短信详单</param>
        /// <returns></returns>
        private void CrawlerDeatils(EnumMobileDeatilType type, MobileReq mobileReq, CrawlerData crawler)
        {
            //获取缓存
            if (CacheHelper.GetCache("infos") != null)
            {
                var infos = (List<string>)CacheHelper.GetCache("infos");
                if (infos.Count > 0)
                {
                    PRODNO = infos[0];
                    PRODTYPE = PRODNO.Split('~')[1];
                    startDayvalue = infos[2];
                    endDayvalue = infos[3];
                    CITYCODE = infos[6];
                    sessionid = infos[7];
                }
            }
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
                queryType = "4";

            for (int i = 0; i <= 5; i++)
            {
                logDtl = new ApplyLogDtl(date.AddMonths(-i).ToString(Consts.DateFormatString7) + "月详单抓取");

                startDate = new DateTime(date.AddMonths(-i).Year, date.AddMonths(-i).Month, 1).ToString("yyyy-MM-dd");
                endDate = new DateTime(date.AddMonths(-i).Year, date.AddMonths(-i).Month, 1).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
                sessionid = sessionid.Replace("!", "%2");
                endDayvalue = new DateTime(date.Year, date.AddMonths(-i).Month, 1).AddMonths(1).AddDays(-1).Day.ToString();
                if (queryType == "2")
                {
                    postdata = string.Format("IdCardInput=&IdCardNo=&ck_month={0}&PRODTYPE={1}&timeType={2}&QTYPE={3}&CITYCODE={4}&sessionid={5}&SHOWTICKETPWD=&IFCHANGENUM=0&PRODNO={6}&startDayvalue={7}&endDayvalue={8}", date.AddMonths(-i).ToString("yyyyMM"), PRODTYPE, "1", queryType, CITYCODE, sessionid, PRODNO.ToUrlEncode(), startDayvalue, endDayvalue);
                    Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10023&toStUrl=http://sc.189.cn/service/bill/myQueryBill.jsp?PAGENO=1&" + postdata + "#fanyemaoji";
                }
                else if (queryType == "4")
                {
                    postdata = string.Format("IdCardInput=&IdCardNo=&ck_month={0}&PRODTYPE={1}&timeType={2}&QTYPE={3}&CITYCODE={4}&sessionid={5}&SHOWTICKETPWD=&IFCHANGENUM=0&PRODNO={6}&startDayvalue={7}&endDayvalue={8}", date.AddMonths(-i).ToString("yyyyMM"), PRODTYPE, "1", queryType, CITYCODE, sessionid, PRODNO.ToUrlEncode(), startDayvalue, endDayvalue);
                    Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10023&toStUrl=http://sc.189.cn/service/bill/myQueryBill.jsp?PAGENO=1&" + postdata + "#fanyemaoji";
                }
                else
                {
                    postdata = string.Format("PAGENO={9}&PRODNO={0}&smsrand={1}&IdCardInput=&IdCardNo=&ck_month={2}&startDayvalue={3}&endDayvalue={4}&PRODTYPE={5}&timeType={6}&CITYCODE={7}&sessionid={8}&SHOWTICKETPWD=", PRODNO.ToUrlEncode(), mobileReq.Smscode, date.AddMonths(-i).ToString("yyyyMM"), startDayvalue, endDayvalue, PRODTYPE, "0", CITYCODE, sessionid, queryType);
                    Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10023&toStUrl=http://sc.189.cn/service/bill/myQueryBill.jsp?" + postdata;
                }

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postdata,
                    Method = "Post",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, date.AddMonths(-i).ToString(Consts.DateFormatString7) + "月详单抓取");
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + "0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = date.ToString(Consts.DateFormatString7) + "详单抓取成功";
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
            string title = string.Empty;

            for (int i = 0; i <= 5; i++)
            {
                title = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString7);
                logDtl = new ApplyLogDtl(title);
                try
                {
                    PhoneCostStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    var infos = HtmlParser.GetResultFromParser(PhoneCostStr, "//div[@id='dataInfos']/table/tbody/tr", "inner");
                    foreach (var item in infos)
                    {
                        //短信详单
                        if (type == EnumMobileDeatilType.SMS)
                        {
                            List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                            if (tdRow.Count != 4) continue;
                            Sms sms = new Sms();
                            if (tdRow.Count > 0)
                            {
                                sms.StartTime = DateTime.Parse(tdRow[2]).ToString(Consts.DateFormatString11);   //起始时间
                                sms.InitType = tdRow[0];  //通信方式
                                sms.OtherSmsPhone = tdRow[1];  //对方号码
                                sms.SubTotal = tdRow[3].ToDecimal(0);  //总费用
                                mobile.SmsList.Add(sms);
                            }
                        }
                        else if (type == EnumMobileDeatilType.Net)    //上网详单
                        {
                            List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                            if (tdRow.Count != 8) continue;
                            Net gprs = new Net();
                            if (tdRow.Count > 0)
                            {
                                var totalSecond = 0;
                                var usetime = tdRow[2].ToString();
                                if (!string.IsNullOrEmpty(usetime))
                                {
                                    totalSecond = CommonFun.ConvertDate(usetime);
                                }
                                gprs.StartTime = DateTime.Parse(tdRow[1]).ToString(Consts.DateFormatString11);
                                gprs.Place = tdRow[5];
                                gprs.NetType = tdRow[4];
                                gprs.SubTotal = tdRow[7].ToDecimal(0);
                                gprs.SubFlow = tdRow[3].ToString();
                                gprs.UseTime = totalSecond.ToString();
                                gprs.PhoneNetType = tdRow[6];
                                mobile.NetList.Add(gprs);
                            }
                        }
                        else      //通话详单
                        {
                            List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                            if (tdRow.Count != 11) continue;
                            Call call = new Call();
                            if (tdRow.Count > 0)
                            {
                                var totalSecond = 0;
                                var usetime = tdRow[1].ToString();
                                if (!string.IsNullOrEmpty(usetime))
                                {
                                    totalSecond = CommonFun.ConvertDate(usetime);
                                }
                                call.StartTime = DateTime.Parse(tdRow[0]).ToString(Consts.DateFormatString11);  //起始时间
                                call.CallPlace = tdRow[4];  //通话地点
                                var othercallphone = Regex.Replace(tdRow[3], @"[^\d.\d]", "");
                                if (!othercallphone.IsEmpty())
                                {
                                    call.OtherCallPhone = othercallphone;  //对方号码
                                }
                                call.UseTime = totalSecond.ToString();  //通话时长
                                call.CallType = tdRow[5];  //通话类型
                                call.InitType = tdRow[2];  //呼叫类型
                                call.SubTotal = tdRow[10].ToDecimal(0);  //通话费用
                                mobile.CallList.Add(call);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = title + "月详单解析异常：" + e.Message;
                    appLog.LogDtlList.Add(logDtl);

                    continue;
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = title + "月详单解析成功";
                appLog.LogDtlList.Add(logDtl);
            }
        }

        public string getStringByElementName(string soure, string elementname)
        {
            string resultStr = String.Empty;
            using (StringReader strRdr = new StringReader(soure))
            {
                //通过XmlReader.Create静态方法创建XmlReader实例
                using (XmlReader rdr = XmlReader.Create(strRdr))
                {
                    //循环Read方法直到文档结束
                    while (rdr.Read())
                    {
                        //如果是开始节点
                        if (rdr.NodeType == XmlNodeType.Element)
                        {
                            string elementName = rdr.Name;
                            if (elementName == elementname)
                            {
                                //读取到节点内文本内容
                                if (rdr.Read())
                                {
                                    //通过rdr.Value获得文本内容
                                    resultStr = HttpUtility.HtmlDecode(rdr.Value);
                                    //break;

                                }
                            }
                        }
                    }
                }
            }

            return resultStr;
        }

        #endregion

    }
}
