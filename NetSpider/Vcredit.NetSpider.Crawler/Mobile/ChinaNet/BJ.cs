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
    public class BJ : ChinaNet
    {
        public override VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_SendSMS, mobileReq.Website);
            VerCodeRes Res = new VerCodeRes();
            Res.Token = mobileReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            string result = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                #region 发送动态验证码校验
                logDtl = new ApplyLogDtl("发送动态验证码校验");

                Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
                postdata = "fastcode=01390638";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do",
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

                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10001&toStUrl=http://bj.189.cn/iframe/feequery/detailBillIndex.action?fastcode=01390638";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do",
                    CookieCollection = cookies,
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

                Url = "http://login.189.cn/redirect/ECSSSOTransit?Req=10036%24tDqux0%2BMqe3I5RlfvHVUU97UN40QKFcGUlMqbxHFRcHBrNQQv4llfI5ee7RorbtA9BE3GZSF9YQ0%0AjEzPeqR7wZn4Bq%2B8G7n1WL6Vr25E9tLQY9%2FWK%2FSLQBoE2oL%2FXVR2DO43004yUJXlJZfaZc0Z1Pdb%0AC0jwdRwsDmVw45VZZupVWYAvIRoGpXDcDe4pRPxDpgag6Mluj8egJTY%2FoDJ%2BQNcPdJKRikRtJw%2F4%0AmhkqRSnswr19GvIqgmwaUXiNTqCstgpAW4qtuVs%3D";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发送动态验证码校验ECSSSOTransit");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://bj.189.cn/service/account/customerHome.action?PlatNO=90000&Ticket=900001524285bfc649999c3447e89d4121418f74e631&TxID=9000002fe58948d564e7981cf2ead285338a6&SSOURL=http%3a%2f%2fbj.189.cn%2fiframe%2ffeequery%2fdetailBillIndex.action%3ffastcode%3d01390638";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发送动态验证码校验customerHome");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://bj.189.cn/iframe/feequery/detailBillIndex.action?fastcode=01390638";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发送动态验证码校验detailBillIndex");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://bj.189.cn/iframe/feequery/detailBillIndex.action?tab=tab1&time=1447377321800";
                httpItem = new HttpItem()
                {
                    Accept = "text/html, */*; q=0.01",
                    URL = Url,
                    Referer = "http://bj.189.cn/iframe/feequery/detailBillIndex.action?fastcode=01390638",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发送动态验证码校验detailBillIndex");
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

                Url = "http://bj.189.cn/iframe/feequery/smsRandCodeSend.action";
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Method = "POST",
                    Postdata = "accNum=" + mobileReq.Mobile,
                    Referer = "http://bj.189.cn/iframe/feequery/detailBillIndex.action?fastcode=01390638",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发送动态验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                result = CommonFun.GetMidStr(httpResult.Html, "tip\":", ",");
                if (result != "null")
                {
                    Res.StatusDescription = result;
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                logDtl.Description = "发送动态验证码校验";
                appLog.LogDtlList.Add(logDtl);

                #endregion

                Res.StatusDescription = "北京电信手机验证码发送成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;

                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "北京电信手机验证码发送异常";
                Log4netAdapter.WriteError("北京电信手机验证码发送异常", e);

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
            string result = string.Empty;

            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                #region 发送动态验证码校验
                logDtl = new ApplyLogDtl("验证手机验证码");

                Url = "http://bj.189.cn/iframe/feequery/billDetailQuery.action";
                postdata = string.Format("requestFlag=asynchronism&accNum={0}&sRandomCode={1}", mobileReq.Mobile, mobileReq.Smscode);
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://bj.189.cn/iframe/feequery/detailBillIndex.action?fastcode=01390638",
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
                result = CommonFun.GetMidStr(httpResult.Html, "tip\":\"", "\",");
                if (!result.IsEmpty() && result != "开始日期为空")
                {
                    Res.StatusDescription = result;
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
                logDtl.Description = "验证手机验证码";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                Res.StatusDescription = "北京电信手机验证码验证成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_Query;

                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "北京电信手机验证码验证异常";
                Log4netAdapter.WriteError("北京电信手机验证码验证异常", e);

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
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            CrawlerData crawler = new CrawlerData() { Token = mobileReq.Token, IdentityCard = mobileReq.IdentityCard, Name = mobileReq.Name, Mobile = mobileReq.Mobile, UserType = "1" };
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                #region 基本信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Crawler, mobileReq.Website);

                #region  基本信息
                logDtl = new ApplyLogDtl("基本信息");

                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10001&toStUrl=http://bj.189.cn/iframe/custservice/modifyUserInfo.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    var results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='ued-table-nobor mgt-30']/tbody/tr[1]/td[1]", "");
                    if (results.Count > 0)
                    {
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
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                #endregion

                #region   积分查询
                logDtl = new ApplyLogDtl("手机积分");

                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10001&toStUrl=http://bj.189.cn/iframe/custquery/qryPoint.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region  套餐查询
                logDtl = new ApplyLogDtl("套餐查询");

                Url = "http://bj.189.cn/iframe/feequery/custBusinessIndex.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    //Referer = "http://bj.189.cn/iframe/feequery/custBusinessIndex.action",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "套餐查询校验custBusinessIndex");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://bj.189.cn/iframe/feequery/cumulationInfoQuery.action";
                postdata = string.Format("accNum={0}&requestFlag=synchronization", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://bj.189.cn/iframe/feequery/custBusinessIndex.action",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "套餐查询校验cumulationInfoQuery");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://bj.189.cn/iframe/custquery/orderRelaQuery.action";
                postdata = "requestFlag=asynchronism";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://bj.189.cn/iframe/feequery/custBusinessIndex.action",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                #endregion

                #region  puk
                logDtl = new ApplyLogDtl("PUK查询");

                Url = "http://bj.189.cn/iframe/local/ajaxRequestPukInfo.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = "1",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "PUK查询");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "pukInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "PUK查询抓取成功";
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

                CrawlerBill(crawler, mobileReq);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 详单

                #region 通话详单
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

                #region 流量详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.Net, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #endregion

                //保存
                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "北京电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "北京电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("北京电信手机账单抓取异常", e);

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

                #region  基本信息获取
                logDtl = new ApplyLogDtl("基本信息");

                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//table[@class='ued-table-nobor mgt-30']/tbody/tr[1]/td[1]", "");
                if (results.Count > 0)
                {
                    mobile.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(result, "//table[@class='ued-table-nobor mgt-30']/tbody/tr[2]/td[2]/div/span", "");
                if (results.Count > 0)
                {
                    mobile.Address = results[0];
                }
                results = HtmlParser.GetResultFromParser(result, "//table[@class='ued-table-nobor mgt-30']/tbody/tr[3]/td[1]/div/span", "");
                if (results.Count > 0)
                {
                    mobile.Postcode = results[0];
                }
                results = HtmlParser.GetResultFromParser(result, "//table[@class='ued-table-nobor mgt-30']/tbody/tr[3]/td[2]/div/span", "");
                if (results.Count > 0)
                {
                    mobile.Email = results[0];
                }
                results = HtmlParser.GetResultFromParser(result, "//table[@class='ued-table-nobor mgt-30']/tbody/td", "");
                if (results.Count > 0)
                {
                    mobile.Regdate = results[0];
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "基本信息";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region   积分查询
                logDtl = new ApplyLogDtl("手机积分");

                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//p[@class='fs-14']/span[1]", "");
                if (results.Count > 0)
                {
                    mobile.Integral = results[0];
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "手机积分";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  套餐查询
                logDtl = new ApplyLogDtl("套餐查询");

                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//table/tbody/tr[1]/td[1]", "");
                if (results.Count > 0)
                {
                    mobile.Package = results[0];
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "套餐查询";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  puk
                logDtl = new ApplyLogDtl("PUK查询");

                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "pukInfor").FirstOrDefault().CrawlerTxt);
                    object obj = JsonConvert.DeserializeObject(result);
                    JObject js = obj as JObject;
                    if (js != null)
                    {
                        JObject bdp = js["pukInfoSet"] as JObject;
                        if (bdp != null)
                        {
                            mobile.PUK = bdp["puk1"].ToString();
                        }
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

                #region  账单查询
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Analysis, mobileReq.Website);

                AnalysisBill(crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 详单

                #region 通话详单
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

                #region 流量详单
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
                Res.StatusDescription = "北京电信手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "北京电信手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;

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
            string Url = string.Empty;
            var month = String.Empty;
            string postdata = String.Empty;
            DateTime date = DateTime.Now;

            logDtl = new ApplyLogDtl("账单抓取校验");
            Url = "http://bj.189.cn/iframe/feequery/billQuery.action";
            postdata = string.Format("accNum={0}&requestFlag=synchronization", mobileReq.Mobile);
            httpItem = new HttpItem()
            {
                URL = Url,
                Method = "post",
                Postdata = postdata,
                Referer = "http://bj.189.cn/iframe/feequery/billQueryIndex.action",
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "账单抓取校验");
            if (httpResult.StatusCode != HttpStatusCode.OK) return;
            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
            logDtl.StatusCode = ServiceConsts.StatusCode_success;
            logDtl.Description = "账单抓取校验校验成功";
            appLog.LogDtlList.Add(logDtl);

            for (int i = 0; i < 6; i++)
            {
                date = DateTime.Now.AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月账单抓取");

                if (i == 0)
                {
                    Url = string.Format("http://bj.189.cn/iframe/feequery/qryRealtimeFee.action?requestFlag=synchronization&p1QueryFlag=2&accNum={0}&time=1447639443170", mobileReq.Mobile);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtmlLog(httpItem, appLog, date.ToString(Consts.DateFormatString7) + "月账单抓取");
                    if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                }
                else
                {
                    if (DateTime.Now.AddMonths(-i).Month < 10)
                        month = DateTime.Now.AddMonths(-i).Year.ToString() + "0" + DateTime.Now.AddMonths(-i).Month.ToString();
                    else
                        month = DateTime.Now.AddMonths(-i).Year.ToString() + DateTime.Now.AddMonths(-i).Month.ToString();
                    Url = string.Format("http://bj.189.cn/iframe/feequery/billInfoQuery.action?billCycle={0}&billReqType=3&accNum={1}", month, mobileReq.Mobile);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtmlLog(httpItem, appLog, date.ToString(Consts.DateFormatString7) + "月账单抓取");
                    if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                }
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
            CrawlerDtlData billhtml = null;
            List<string> results = new List<string>();
            MonthBill bill = null;
            DateTime date;

            for (var i = 0; i <= 5; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis, date.ToString("yyyy-MM-dd") + "月账单解析");
                try
                {
                    billhtml = crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault();
                    if (billhtml == null) continue;
                    PhoneBillStr = System.Text.Encoding.Default.GetString(billhtml.CrawlerTxt);
                    bill = new MonthBill();
                    if (i == 0)
                    {
                        bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);

                        results = HtmlParser.GetResultFromParser(PhoneBillStr, "//td[@id='r_v1']", "inner");
                        if (results.Count > 0)
                        {
                            bill.TotalAmt = results[0];
                            if (!bill.TotalAmt.IsEmpty())
                                bill.TotalAmt = bill.TotalAmt.Replace("￥", "").ToTrim();
                        }
                        results = HtmlParser.GetResultFromParser(PhoneBillStr, "//table[2]/tbody/tr/td[3]", "inner");
                        if (results.Count > 0)
                        {
                            bill.PlanAmt = results[0];
                            if (!bill.PlanAmt.IsEmpty())
                                bill.PlanAmt = bill.PlanAmt.Replace("￥", "").ToTrim();
                        }
                    }
                    else
                    {
                        results = HtmlParser.GetResultFromParser(PhoneBillStr, "//div[@class='PageNext']/table/tbody/tr/td/table/tbody/tr[3]/td[1]/table/tbody/tr[3]/td[2]", "");
                        if (results.Count > 0)
                        {
                            bill.BillCycle = CommonFun.GetMidStr(results[0], "计费周期:", "--");//计费周期
                        }
                        results = HtmlParser.GetResultFromParser(PhoneBillStr, "//div[@class='PageNext']/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[5]/td[2]", "");
                        if (results.Count > 0)
                        {
                            bill.PlanAmt = results[0];
                        }
                        bill.TotalAmt = CommonFun.GetMidStr(PhoneBillStr, "本期费用合计：", "元</td>");
                    }
                    mobile.BillList.Add(bill);
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
        /// <param name="queryType">1:通话；3:上网；2:短信</param>
        /// <returns></returns>
        private void CrawlerDeatils(EnumMobileDeatilType type, CrawlerData crawler)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            var month = String.Empty;
            DateTime date = DateTime.Now;
            int pageIndex = 1;
            int pageCount = 0;
            string result = String.Empty;
            string bill_type = string.Empty;
            string title = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                bill_type = "1";
            else if (type == EnumMobileDeatilType.SMS)
                bill_type = "2";
            else
                bill_type = "3";

            for (var i = 0; i <= 5; i++)
            {
                if (i != 0)
                {
                    date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-i);
                    date = date.AddMonths(1).AddDays(-1);
                }
                month = date.ToString("yyyy年MM");
                do
                {
                    title = date.ToString(Consts.DateFormatString7) + "月详单抓取";
                    logDtl = new ApplyLogDtl(title);

                    Url = "http://bj.189.cn/iframe/feequery/billDetailQuery.action";
                    postdata = "requestFlag=synchronization&billDetailType={0}&qryMonth={1}&startTime=1&endTime={2}&billPage={3}";
                    postdata = String.Format(postdata, bill_type, month.ToUrlEncode(), date.Day, pageIndex);
                    httpItem = new HttpItem()
                    {
                        Accept = "text/html, */*; q=0.01",
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Referer = "http://bj.189.cn/iframe/feequery/detailBillIndex.action?fastcode=01390638",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtmlLog(httpItem, appLog, title);
                    if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + (i + 1).ToString() + (pageIndex < 10 ? "0" + pageIndex.ToString() : pageIndex.ToString()), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    result = CommonFun.GetMidStr(httpResult.Html, "共<span class=\"color-6 fs-16\">", "</span>条");
                    pageCount = Math.Ceiling((double)(result.ToDecimal(0) / 50)).ToString().ToInt(1);
                    pageIndex++;

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = title + "成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                while (pageIndex <= pageCount);
            }
        }

        /// <summary>
        /// 解析手机详单
        /// </summary>
        /// <param name="queryType">1:通话；3:上网；2:短信</param>
        /// <returns></returns>
        private void AnalysisDeatils(EnumMobileDeatilType type, CrawlerData crawler, Basic mobile)
        {
            List<CrawlerDtlData> PhoneCrawlerDtls = new List<CrawlerDtlData>();
            string PhoneStr = string.Empty;
            Call phoneCall;
            Sms phoneSMS;
            Net phoneGPRS;
            string title = string.Empty;

            for (var i = 0; i <= 5; i++)
            {
                title = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString7);
                logDtl = new ApplyLogDtl(title);
                try
                {
                    PhoneCrawlerDtls = crawler.DtlList.Where(x => x.CrawlerTitle.StartsWith(type + (i + 1).ToString())).OrderBy(x => x.CrawlerTitle).ToList<CrawlerDtlData>();
                    if (PhoneCrawlerDtls != null && PhoneCrawlerDtls.Count > 0)
                    {
                        foreach (CrawlerDtlData item in PhoneCrawlerDtls)
                        {
                            PhoneStr = System.Text.Encoding.Default.GetString(item.CrawlerTxt);
                            var results = HtmlParser.GetResultFromParser(PhoneStr, "//table[@class='ued-table']/tr", "inner");
                            foreach (string itemtr in results)
                            {
                                var tdRow = HtmlParser.GetResultFromParser(itemtr, "//td");
                                if (tdRow.Count <= 1) continue;
                                if (type == EnumMobileDeatilType.Call)
                                {
                                    phoneCall = new Call();
                                    phoneCall.StartTime = DateTime.Parse(tdRow[5]).ToString(Consts.DateFormatString11);
                                    phoneCall.SubTotal = tdRow[9].ToDecimal(0);
                                    phoneCall.UseTime = tdRow[8];
                                    phoneCall.OtherCallPhone = tdRow[4];
                                    phoneCall.CallPlace = tdRow[3];
                                    phoneCall.CallType = tdRow[2];
                                    phoneCall.InitType = tdRow[1];
                                    mobile.CallList.Add(phoneCall);
                                }
                                else if (type == EnumMobileDeatilType.SMS)
                                {
                                    phoneSMS = new Sms();
                                    phoneSMS.StartTime = DateTime.Parse(tdRow[4]).ToString(Consts.DateFormatString11);
                                    phoneSMS.SubTotal = tdRow[5].ToDecimal(0);
                                    phoneSMS.OtherSmsPhone = tdRow[3];
                                    phoneSMS.SmsType = tdRow[1];
                                    phoneSMS.InitType = tdRow[2];
                                    mobile.SmsList.Add(phoneSMS);
                                }
                                else if (type == EnumMobileDeatilType.Net)
                                {

                                    var totalSecond = 0;
                                    var usetime = tdRow[2].ToString();
                                    if (!string.IsNullOrEmpty(usetime))
                                    {
                                        totalSecond = CommonFun.ConvertDate(usetime);
                                    }
                                    var totalFlow = CommonFun.ConvertGPRS(tdRow[3].ToString());

                                    phoneGPRS = new Net();
                                    phoneGPRS.StartTime = DateTime.Parse(tdRow[1]).ToString(Consts.DateFormatString11);
                                    phoneGPRS.SubTotal = tdRow[6].ToDecimal(0);
                                    phoneGPRS.UseTime = totalSecond.ToString();
                                    phoneGPRS.SubFlow = totalFlow.ToString();
                                    phoneGPRS.PhoneNetType = tdRow[7];
                                    phoneGPRS.NetType = tdRow[4];
                                    phoneGPRS.Place = tdRow[5];
                                    mobile.NetList.Add(phoneGPRS);
                                }
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


        #endregion
    }
}
