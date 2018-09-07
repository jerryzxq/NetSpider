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
using Vcredit.NetSpider.Entity.Mongo.Log;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{
    public class SD : ChinaNet
    {
        public override VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            VerCodeRes Res = new VerCodeRes();
            Res.Token = mobileReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            DateTime date = DateTime.Now;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                //校验跨域登录
                BaseRes baseRes = CheckLogin(mobileReq);
                if (baseRes.StatusCode != ServiceConsts.StatusCode_success)
                {
                    Res.StatusDescription = baseRes.StatusDescription;
                    Res.StatusCode = baseRes.StatusCode;
                    return Res;
                }
                
                //验证
                Url = "http://sd.189.cn/selfservice/bill/checkName";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = "{}",
                    Referer = "http://sd.189.cn/selfservice/bill/",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://sd.189.cn/selfservice/bill/checkSms";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = "{}",
                    Referer = "http://sd.189.cn/selfservice/bill/",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://sd.189.cn/selfservice/bill/sendBillSmsRandom";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    CookieCollection = cookies,
                    Postdata = "{\"orgInfo\":\"" + mobileReq.Mobile + "\",\"nbrType\":\"4\"}",
                    ContentType = "application/json; charset=UTF-8",
                    Referer = "http://sd.189.cn/selfservice/bill/",
                    ResultCookieType = ResultCookieType.CookieCollection,
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

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.StatusDescription = "输入手机验证码，调用手机验证码验证接口";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;

                CacheHelper.SetCache(mobileReq.Token, cookies);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "山东电信手机验证码发送异常";
                Log4netAdapter.WriteError("山东电信手机验证码发送异常", e);
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

        public override BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            DateTime date = DateTime.Now;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                Url = "http://sd.189.cn/selfservice/bill/checkBillSmsRandom";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Referer = "http://sd.189.cn/selfservice/bill/",
                    Postdata = "{\"code\":\"" + mobileReq.Smscode + "\",\"accNbrorg\":\"" + mobileReq.Mobile + "\"}",
                    ContentType = "application/json; charset=UTF-8",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (!httpResult.Html.Contains("{\"flag\":\"1\"}"))
                {
                    Res.StatusDescription = "短信验证码错误";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = "http://sd.189.cn/selfservice/bill/serverQuery";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Referer = "http://sd.189.cn/selfservice/bill/",
                    Postdata = "{\"accNbr\":\"" + mobileReq.Mobile + "\",\"areaCode\":\"0531\",\"accNbrType\":\"4\"}",
                    ContentType = "application/json; charset=UTF-8",
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
                Res.StatusDescription = "山东电信手机验证码验证成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Query;

                CacheHelper.SetCache(mobileReq.Token, cookies);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "山东电信手机验证码验证异常";
                Log4netAdapter.WriteError("山东电信手机验证码验证异常", e);
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

        public override BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            ApplyLogDtl logDtl = new ApplyLogDtl("");
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            CrawlerData crawler = new CrawlerData() { Token = mobileReq.Token, IdentityCard = mobileReq.IdentityCard, Name = mobileReq.Name, Mobile = mobileReq.Mobile, UserType = "1" };
            Basic mobile = new Basic();
            string Url = string.Empty;
            string postdata = string.Empty;
            DateTime date = DateTime.Now;
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                #region 个人基本信息
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Crawler);
                //个人基本信息
                Url = "http://sd.189.cn/selfservice/cust/querymanage?100";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = "{}",
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
                var userinfo = jsonParser.GetResultFromParser(httpResult.Html, "result");
                userinfo = jsonParser.GetResultFromParser(userinfo, "prodRecords");
                userinfo = jsonParser.GetResultFromParser(userinfo, "prodRecord");
                userinfo = jsonParser.GetResultFromParser(userinfo, "custInfo");
                var areaCode = jsonParser.GetResultFromParser(userinfo, "areaId").Remove(2);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                //入网时间
                Url = "http://sd.189.cn/selfservice/cust/loadMyProductInfo";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    ContentType = "application/json; charset=UTF-8",
                    Postdata = "{\"accNbr\":\"" + mobileReq.Mobile + "\",\"areaCode\":\"05" + areaCode + "\",\"accNbrType\":\"4\",\"queryType\":\"2\",\"queryMode\":\"1\"}",
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
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "registDateInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                //积分
                Url = "http://sd.189.cn/selfservice/jf/queryPointBasicInfo";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    ContentType = "application/json; charset=UTF-8",
                    Postdata = "{\"accNbr\":\"" + mobileReq.Mobile + "\",\"accNbrType\":\"4\",\"areaCode\":\"05" + areaCode + "\"}",
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
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                //PUK码
                Url = "http://sd.189.cn/selfservice/support/queryPUK";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    ContentType = "application/json; charset=UTF-8",
                    Postdata = "{}",
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
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "PUKInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                //套餐
                Url = "http://sd.189.cn/selfservice/cust/queryOfferInfo";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    ContentType = "application/json; charset=UTF-8",
                    Postdata = "{\"accNbr\":\"" + mobileReq.Mobile + "\",\"areaCode\":\"05" + areaCode + "\",\"producttype\":\"4\"}",
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
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "packageInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                #endregion

                #region 月账单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Crawler);

                CrawlerBill(crawler, mobileReq);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单抓取成功";
                appLog.LogDtlList.Add(logDtl);

                #endregion

                #region 详单查询

                #region 通话详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.Call, mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  短信详单
                CrawlerDeatils(EnumMobileDeatilType.SMS, mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 上网详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.Net, mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #endregion

                //保存
                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "山东电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;



            }
            catch (Exception e)
            {
                Res.StatusDescription = "山东电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("山东电信手机账单抓取异常", e);
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
        public override BaseRes MobileAnalysis(MobileReq mobileReq, DateTime appDate)
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

                #region 用户基本信息
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                var userinfo = jsonParser.GetResultFromParser(result, "result");
                userinfo = jsonParser.GetResultFromParser(userinfo, "prodRecords");
                userinfo = jsonParser.GetResultFromParser(userinfo, "prodRecord");
                userinfo = jsonParser.GetResultFromParser(userinfo, "custInfo");

                mobile.Name = jsonParser.GetResultFromParser(userinfo, "name");  //姓名
                mobile.Idtype = jsonParser.GetResultFromParser(userinfo, "indentNbrTypeName");  //证件类型
                mobile.Idcard = jsonParser.GetResultFromParser(userinfo, "indentNbr");  //证件号码
                mobile.Address = jsonParser.GetResultFromParser(userinfo, "addr");  //客户地址


                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "registDateInfor").FirstOrDefault().CrawlerTxt);
                var retString = jsonParser.GetResultFromParser(result, "retString");
                var prodRecords = jsonParser.GetResultFromParser(retString, "prodRecords");
                var prodRecord = jsonParser.GetResultFromParser(prodRecords, "prodRecord");
                var productInfo = jsonParser.GetResultFromParser(prodRecord, "productInfo");
                mobile.Regdate = jsonParser.GetResultFromParser(productInfo, "servCreateDate");//入网时间
                if (!mobile.Regdate.IsEmpty())
                {
                    mobile.Regdate = DateTime.Parse(mobile.Regdate.Substring(0, 4) + "-" + mobile.Regdate.Substring(4, 2) + "-" + mobile.Regdate.Substring(6, 2)).ToString(Consts.DateFormatString11);//入网时间
                }
                mobile.PackageBrand = jsonParser.GetResultFromParser(productInfo, "prodSpecName");  //套餐品牌
                #endregion

                #region 套餐
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault().CrawlerTxt);
                try
                {
                    mobile.Package = jsonParser.GetResultFromParser(result, "offerItems").Split(',')[6].Replace("\r\n", "").Replace("\"", "").Replace(" ", "");
                }
                catch { }
                #endregion

                #region 积分
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                try
                {
                    mobile.Integral = jsonParser.GetResultFromParser(result, "UsableBonus");
                }
                catch { }
                #endregion

                #region PUK码
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "PUKInfor").FirstOrDefault().CrawlerTxt);
                try
                {
                    mobile.PUK = jsonParser.GetResultFromParser(result, "accessNumber");
                }
                catch { }
                #endregion

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

                #region  短信详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Analysis);

                AnalysisDeatils(EnumMobileDeatilType.Call, crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 上网详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Analysis);

                AnalysisDeatils(EnumMobileDeatilType.Call, crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #endregion

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                //保存
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "山东电信手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "山东电信手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("山东电信手机账单解析异常", e);

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
        private BaseRes CheckLogin(MobileReq mobileReq)
        {
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {
                logDtl = new ApplyLogDtl("登录校验");

                Url = "http://www.189.cn/bj";
                httpItem = new HttpItem
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录校验bj");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://bj.189.cn/pages/login/sypay_group_new.jsp";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://hb.189.cn/pages/login/sypay_group_new.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录校验sypay_group_new");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/dqmh/flowrecommend.do";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/hb/",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录校验flowrecommend");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/login/index.do";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/html/login/right.html",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录校验login/index.do");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "登录校验成功";
                appLog.LogDtlList.Add(logDtl);

                Res.StatusDescription = "登录校验成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "北京电信登录校验异常";
                Log4netAdapter.WriteError("北京电信登录校验异常", e);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            return Res;

        }

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
            for (int i = 0; i < 6; i++)
            {
                Url = "http://sd.189.cn/selfservice/bill/queryTwoBill";
                postdata = "{\"valueType\":\"1\",\"value\":\"" + mobileReq.Mobile + "\",\"billingCycle\":\"" + date.AddMonths(-i).ToString("yyyyMM") + "\",\"areaCode\":\"0537\",\"queryType\":\"5\",\"proType\":\"4\"}";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    ContentType = "application/json; charset=UTF-8",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
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
            List<string> infos = new List<string>();
            MonthBill bill = null;
            for (var i = 0; i <= 5; i++)
            {
                if (crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault() == null) continue;
                PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                var msg = jsonParser.GetResultFromParser(PhoneBillStr, "resultMsg");
                if (msg == "成功")
                {
                    bill = new MonthBill();
                    bill.TotalAmt = jsonParser.GetResultFromParser(PhoneBillStr, "total");
                    bill.PlanAmt = jsonParser.GetResultFromParser(jsonParser.GetArrayFromParse(PhoneBillStr, "items")[0], "value");
                    bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);
                    mobile.BillList.Add(bill);
                }
            }
        }

        /// <summary>
        /// 抓取手机详单
        /// </summary>
        /// <param name="queryType">1 获取通话记录，3 上网详单 2 短信详单</param>
        /// <returns></returns>
        private void CrawlerDeatils(EnumMobileDeatilType type, MobileReq mobileReq, CrawlerData crawler)
        {
            string Url = string.Empty;
            var postdata = string.Empty;
            string records = string.Empty;
            string queryType = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                queryType = "0";
            else if (type == EnumMobileDeatilType.SMS)
                queryType = "1";
            else
                queryType = "3";

            for (int i = 0; i <= 5; i++)
            {
                DateTime date = DateTime.Now;
                date = date.AddMonths(-i);
                Url = "http://sd.189.cn/selfservice/bill/checkSms";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Referer = "http://sd.189.cn/selfservice/bill/",
                    Postdata = "{}",
                    ContentType = "application/json; charset=UTF-8",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                Url = "http://sd.189.cn/selfservice/bill/queryBillDetailNum";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Referer = "http://sd.189.cn/selfservice/bill/",
                    Postdata = "{\"accNbr\":\"" + mobileReq.Mobile + "\",\"billingCycle\":\"" + date.ToString("yyyyMM") + "\",\"ticketType\":\"" + queryType + "\"}",
                    ContentType = "application/json; charset=UTF-8",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                records = jsonParser.GetResultFromParser(httpResult.Html, "records");

                Url = "http://sd.189.cn/selfservice/bill/queryBillDetail";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    ContentType = "application/json; charset=UTF-8",
                    Postdata = "{\"accNbr\":\"" + mobileReq.Mobile + "\",\"billingCycle\":\"" + date.ToString("yyyyMM") + "\",\"pageRecords\":\"" + records + "\",\"pageNo\":\"1\",\"qtype\":\"" + queryType + "\",\"totalPage\":\"1\",\"queryType\":\"6\"}",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                var msg = jsonParser.GetResultFromParser(httpResult.Html, "resultMsg");
                if (msg == "成功")
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + "0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                }
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
            for (int i = 0; i <= 5; i++)
            {
                if (crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1))).FirstOrDefault() == null) continue;
                PhoneCostStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                var infos = jsonParser.GetArrayFromParse(PhoneCostStr, "items");
                foreach (var item in infos)
                {
                    //短信详单
                    if (type == EnumMobileDeatilType.SMS)
                    {

                        Sms sms = new Sms();
                        sms.StartTime = DateTime.Parse(jsonParser.GetResultFromParser(item, "startTime")).ToString(Consts.DateFormatString11);  //开始时间
                        //sms.SmsPlace = jsonParser.GetResultFromParser(item, "position");  //通信地点
                        sms.OtherSmsPhone = jsonParser.GetResultFromParser(item, "calledNbr");  //对方号码
                        sms.SmsType = jsonParser.GetResultFromParser(item, "eventType");  //通信类型
                        sms.InitType = jsonParser.GetResultFromParser(item, "callTyp");  //通信方式
                        sms.SubTotal = jsonParser.GetResultFromParser(item, "charge").ToDecimal(0);  //通信费
                        mobile.SmsList.Add(sms);

                    }
                    else if (type == EnumMobileDeatilType.Net)    //上网详单
                    {
                        var totalSecond = 0;
                        var usetime = jsonParser.GetResultFromParser(item, "duration").ToString();
                        if (!string.IsNullOrEmpty(usetime))
                        {
                            totalSecond = CommonFun.ConvertDate(usetime);
                        }
                        var totalFlow = CommonFun.ConvertGPRS(jsonParser.GetResultFromParser(item, "flow").ToString());

                        Net gprs = new Net();
                        gprs.StartTime = DateTime.Parse(jsonParser.GetResultFromParser(item, "startTime")).ToString(Consts.DateFormatString11);  //开始时间
                        gprs.Place = jsonParser.GetResultFromParser(item, "position");  //通信地点
                        gprs.NetType = jsonParser.GetResultFromParser(item, "netType");  //网络类型
                        gprs.PhoneNetType = jsonParser.GetResultFromParser(item, "eventType");  //上网方式
                        gprs.SubTotal = jsonParser.GetResultFromParser(item, "charge").ToDecimal(0);  //单次费用
                        gprs.SubFlow = totalFlow.ToString();  //单次流量
                        gprs.UseTime = totalSecond.ToString();  //上网时长
                        mobile.NetList.Add(gprs);

                    }
                    else      //通话详单
                    {
                        Call call = new Call();
                        call.CallPlace = jsonParser.GetResultFromParser(item, "position");  //通话地点
                        call.StartTime = DateTime.Parse(jsonParser.GetResultFromParser(item, "startTime")).ToString(Consts.DateFormatString11);  //开始时间
                        call.UseTime = jsonParser.GetResultFromParser(item, "duration");  //通话时长
                        call.SubTotal = jsonParser.GetResultFromParser(item, "charge").ToDecimal(0);  //通话费用
                        call.OtherCallPhone = jsonParser.GetResultFromParser(item, "calledNbr");  //对方号码
                        call.InitType = jsonParser.GetResultFromParser(item, "eventType");  //通话类型
                        call.CallType = jsonParser.GetResultFromParser(item, "callType"); //呼叫类型
                        mobile.CallList.Add(call);
                    }
                }
            }
        }

        #endregion


    }
}
