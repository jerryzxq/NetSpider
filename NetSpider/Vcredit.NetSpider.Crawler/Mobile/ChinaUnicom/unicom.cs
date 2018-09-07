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
using Vcredit.NetSpider.Entity.Mongo.Log;
using Vcredit.NetSpider.DataAccess.Mongo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.Common.Constants;
using System.Text.RegularExpressions;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaUnicom
{
    public class unicom : IMobileCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = null;
        LogHelper httpHelper = new LogHelper();
        ApplyLogMongo logMongo = new ApplyLogMongo();
        List<ApplyLog> loglist = new List<ApplyLog>();
        ApplyLog appLog = new ApplyLog();
        ApplyLogDtl logDtl = new ApplyLogDtl();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        #endregion

        /// <summary>
        /// 页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes MobileInit(MobileReq mobileReq)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Init, mobileReq.Website);
            VerCodeRes Res = new VerCodeRes();
            Res.Token = CommonFun.GetGuidID();
            cookies = new CookieCollection();
            if (CheckNeedVerify(mobileReq))    //判断是否需要验证码，并根据不同情况初始化页面
            {
                Res = MobilePageInit(Res.Token);
            }
            else
            {
                logDtl = new ApplyLogDtl("初始化登录页面");

                Res.StatusDescription = "中国联通初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = Res.StatusCode;
                logDtl.Description = "初始化登录页面成功";
                appLog.LogDtlList.Add(logDtl);

            }
            CacheHelper.SetCache(Res.Token, cookies);

            if (Res.StatusCode == ServiceConsts.StatusCode_success)
                Res.nextProCode = ServiceConsts.NextProCode_Login;

            appLog.Token = Res.Token;
            appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
            logMongo.Save(appLog);

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
            cookies = new CookieCollection();
            //判断是否需要验证码，并根据不同情况登录
            if (mobileReq.Vercode != "none" && !string.IsNullOrEmpty(mobileReq.Vercode))
                Res = MobileLoginByVerCode(mobileReq);
            else
                Res = MobileLoginNoVerCode(mobileReq);

            if (Res.StatusCode == ServiceConsts.CrawlerStatusCode_CheckSuccess)
                Res.nextProCode = ServiceConsts.NextProCode_Query;

            appLog.Token = Res.Token;
            appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
            logMongo.Save(appLog);

            return Res;
        }

        /// <summary>
        /// 发送短信验证码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 验证短信验证码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 手机抓取
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

            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                #region 个人信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Crawler, mobileReq.Website);

                #region 基本信息
                logDtl = new ApplyLogDtl("基本信息");

                try
                {
                    Url = string.Format("http://iservice.10010.com/e3/static/check/checklogin/?_={0}", GetTimeStamp());
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = string.Format("1"),
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息");
                    if (httpResult.StatusCode == HttpStatusCode.OK)
                    {
                        string isLogin = jsonParser.GetResultFromParser(httpResult.Html, "isLogin");
                        if (!bool.Parse(isLogin))
                        {
                            Res.StatusDescription = "中国联通该手机未登录";
                            Res.StatusCode = ServiceConsts.StatusCode_fail;

                            logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = Res.StatusDescription;
                            appLog.LogDtlList.Add(logDtl);
                            appLog.Token = Res.Token;
                            appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                            loglist.Add(appLog);

                            return Res;
                        }
                        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                        crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = "基本信息抓取成功";
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

                #region 手机积分
                logDtl = new ApplyLogDtl("手机积分");

                Url = "http://iservice.10010.com/e3/static/header";
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Method = "POST",
                    Postdata = string.Format("1"),
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

                #region puk、证件类型
                logDtl = new ApplyLogDtl("puk、证件类型");

                Url = "http://iservice.10010.com/e3/static/query/searchPerInfo/?_=1445590236559&menuid=000100010012";
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Method = "POST",
                    Postdata = string.Format("1"),
                    ContentType = "application/x-www-form-urlencoded;charset=UTF-8",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "puk、证件类型");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "pukInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "puk、证件类型抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                #endregion

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 话费帐单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Crawler, mobileReq.Website);

                GetBill(crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 话费详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Crawler, mobileReq.Website);

                GetDeatils(EnumMobileDeatilType.Call, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Crawler, mobileReq.Website);

                GetDeatils(EnumMobileDeatilType.SMS, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Crawler, mobileReq.Website);

                GetDeatils(EnumMobileDeatilType.Net, crawler);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                crawlerMobileMongo.SaveCrawler(crawler);

                Res.StatusDescription = "中国联通手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusDescription = "中国联通手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("中国联通手机账单抓取异常", e);

                logDtl.StatusCode = ServiceConsts.StatusCode_fail;
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
        /// 手机解析
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
            Call call = null;
            Net gprs = null;
            Sms sms = null;
            string result = string.Empty;

            try
            {
                crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);

                #region 个人信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Analysis, mobileReq.Website);

                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.BusName = mobileReq.Name;
                mobile.Token = mobileReq.Token;
                mobile.Mobile = mobileReq.Mobile;
                mobile.UpdateTime = crawler.CrawlerDate;

                #region 基本信息
                logDtl = new ApplyLogDtl("基本信息");

                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                    result = jsonParser.GetResultFromParser(result, "userInfo");
                    if (!result.IsEmpty())
                    {
                        string hanzi = @"[\u4e00-\u9fa5]"; //汉字
                        if (Regex.IsMatch(jsonParser.GetResultFromParser(result, "nickName").Trim(), hanzi))
                            mobile.Name = jsonParser.GetResultFromParser(result, "nickName");
                        else if (Regex.IsMatch(jsonParser.GetResultFromParser(result, "custName").Trim(), hanzi))
                            mobile.Name = jsonParser.GetResultFromParser(result, "custName");

                        mobile.Idcard = jsonParser.GetResultFromParser(result, "certnum");
                        mobile.PackageBrand = jsonParser.GetResultFromParser(result, "brand_name");
                        mobile.Package = jsonParser.GetResultFromParser(result, "packageName");
                        mobile.Address = jsonParser.GetResultFromParser(result, "certaddr");
                        mobile.StarLevel = jsonParser.GetResultFromParser(result, "custlvl");
                        mobile.Regdate = jsonParser.GetResultFromParser(result, "opendate");
                        if (!mobile.Regdate.IsEmpty())
                        {
                            mobile.Regdate = DateTime.Parse(mobile.Regdate.Substring(0, 4) + "-" + mobile.Regdate.Substring(4, 2) + "-" + mobile.Regdate.Substring(6, 2)).ToString(Consts.DateFormatString11);
                        }
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

                #region  手机积分
                logDtl = new ApplyLogDtl("手机积分");

                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                    string rspArgs = jsonParser.GetResultFromParser(result, "rspArgs");
                    if (!rspArgs.IsEmpty())
                        mobile.Integral = jsonParser.GetResultFromParser(rspArgs, "sore");

                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "手机积分解析成功";
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

                #region puk、证件类型
                logDtl = new ApplyLogDtl("puk、证件类型");

                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "pukInfor").FirstOrDefault().CrawlerTxt);
                    result = jsonParser.GetResultFromParser(result, "result");
                    mobile.PUK = jsonParser.GetResultFromParser(result, "pukcode");

                    string rspArgs = jsonParser.GetResultFromParser(result, "MyDetail");
                    if (!rspArgs.IsEmpty())
                    {
                        mobile.Idtype = jsonParser.GetResultFromParser(rspArgs, "certtype");
                        if (mobile.Name.IsEmpty())
                            mobile.Name = jsonParser.GetResultFromParser(rspArgs, "custname");
                        if (mobile.Regdate.IsEmpty())
                            mobile.Regdate = DateTime.Parse(jsonParser.GetResultFromParser(rspArgs, "opendate")).ToString(Consts.DateFormatString11);
                        if (mobile.Idtype.IsEmpty())
                            mobile.Idtype = jsonParser.GetResultFromParser(rspArgs, "certtype");
                        if (mobile.Idcard.IsEmpty())
                            mobile.Idcard = jsonParser.GetResultFromParser(rspArgs, "certnum");
                        if (mobile.PackageBrand.IsEmpty())
                            mobile.PackageBrand = jsonParser.GetResultFromParser(rspArgs, "brand");
                        if (mobile.StarLevel.IsEmpty())
                            mobile.StarLevel = jsonParser.GetResultFromParser(rspArgs, "custlvl");
                        if (mobile.Package.IsEmpty())
                            mobile.Package = jsonParser.GetResultFromParser(result, "productname");
                        if (mobile.Address.IsEmpty())
                            mobile.Address = jsonParser.GetResultFromParser(result, "certaddr");
                    }
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "puk、证件类型解析成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = e.Message;
                    appLog.LogDtlList.Add(logDtl);
                }
                #endregion

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 话费帐单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Analysis, mobileReq.Website);

                ReadBill(crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 话费详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Analysis, mobileReq.Website);

                ReadDeatils<CallDetail>(EnumMobileDeatilType.Call, crawler, delegate(List<CallDetail> callDetails)
                {
                    foreach (CallDetail item in callDetails)
                    {
                        var totalSecond = 0;
                        var usetime = item.calllonghour.ToString();
                        if (!string.IsNullOrEmpty(usetime))
                        {
                            totalSecond = CommonFun.ConvertDate(usetime);
                        }

                        call = new Call();
                        call.StartTime = DateTime.Parse(item.calldate + " " + item.calltime).ToString(Consts.DateFormatString11);
                        call.CallPlace = item.homeareaName;
                        call.InitType = item.calltypeName;
                        call.OtherCallPhone = item.othernum;
                        call.UseTime = totalSecond.ToString().Trim();
                        call.CallType = item.landtype;
                        call.SubTotal = item.totalfee.ToDecimal(0);
                        mobile.CallList.Add(call);
                    }
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Analysis, mobileReq.Website);

                ReadDeatils<SmsDetail>(EnumMobileDeatilType.SMS, crawler, delegate(List<SmsDetail> smsDetails)
                {
                    foreach (SmsDetail item in smsDetails)
                    {
                        sms = new Sms();
                        sms.StartTime = DateTime.Parse(item.smsdate + " " + item.smstime).ToString(Consts.DateFormatString11);
                        sms.SmsPlace = item.homearea;
                        sms.OtherSmsPhone = item.othernum;
                        sms.InitType = item.businesstype;
                        sms.SmsType = item.businesstype;
                        sms.SubTotal = item.fee.ToDecimal(0);
                        mobile.SmsList.Add(sms);
                    }
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网详单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Analysis, mobileReq.Website);

                ReadDeatils<CallFlowDetail>(EnumMobileDeatilType.Net, crawler, delegate(List<CallFlowDetail> callFlowDetails)
                {
                    foreach (CallFlowDetail arrItem in callFlowDetails)
                    {
                        gprs = new Net();
                        gprs.StartTime = DateTime.Parse(arrItem.begindate + " " + arrItem.begintime).ToString(Consts.DateFormatString11);
                        gprs.Place = arrItem.homeareaName;
                        gprs.PhoneNetType = arrItem.svcname;
                        gprs.UseTime = arrItem.longhour;
                        gprs.SubFlow = arrItem.totalbytes.ToDecimal(0).ToString();
                        gprs.SubTotal = arrItem.totalfee.ToDecimal(0);
                        gprs.NetType = arrItem.nettype;
                        mobile.NetList.Add(gprs);
                    }
                });

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);

                mobileMongo.SaveBasic(mobile);
                Res.Result = jsonParser.SerializeObject(mobile);
                Res.StatusDescription = "中国联通手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "中国联通手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("中国联通手机账单解析异常", e);

                logDtl.StatusCode = ServiceConsts.StatusCode_fail;
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
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        private static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 01, 01, 00, 00, 00, 0000);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }

        /// <summary>
        /// 校验是否需要验证码
        /// </summary>
        /// <returns></returns>
        private bool CheckNeedVerify(MobileReq mobileReq)
        {
            bool result = false;
            string Url = string.Empty;
            string PhoneCostStr = string.Empty;
            try
            {
                logDtl = new ApplyLogDtl("校验是否需要验证码");

                Url = string.Format("https://uac.10010.com/portal/Service/CheckNeedVerify?callback=jQuery17206572243681382435_1436150485257&userName={0}&pwdType=01&_=1436147499660", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    IfModifiedSince = DateTime.Now,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验是否需要验证码");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    PhoneCostStr = CommonFun.GetMidStr(httpResult.Html, "jQuery17206572243681382435_1436150485257(", ");");
                    string resultCode = jsonParser.GetResultFromParser(PhoneCostStr, "resultCode");
                    result = bool.Parse(resultCode);

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.Description = "校验是否需要验证码成功";
                    appLog.LogDtlList.Add(logDtl);
                }

            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("中国联通校验异常", e);
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            return result;
        }

        /// <summary>
        /// 页面初始化(需要验证码)
        /// </summary>
        /// <returns></returns>
        private VerCodeRes MobilePageInit(string token)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            Res.Token = token;
            try
            {
                //第一步，初始化登录页面
                logDtl = new ApplyLogDtl("初始化登录页面");

                Url = "https://uac.10010.com/portal/mallLogin.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "初始化登录页面");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.Description = "初始化登录页面成功";
                    appLog.LogDtlList.Add(logDtl);

                    //第二步，获取验证码
                    logDtl = new ApplyLogDtl("获取验证码");

                    Url = "https://uac.10010.com/portal/Service/CreateImage";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        ResultType = ResultType.Byte,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "获取验证码");
                    if (httpResult.StatusCode == HttpStatusCode.OK)
                    {
                        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                        Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                        Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);

                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.Description = "获取验证码成功";
                        appLog.LogDtlList.Add(logDtl);

                        Res.StatusDescription = "中国联通初始化完成";
                        Res.StatusCode = ServiceConsts.StatusCode_success;
                    }
                }
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "中国联通初始化异常";
                Log4netAdapter.WriteError("中国联通初始化异常", e);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            return Res;
        }

        /// <summary>
        /// 登录（需要验证码）
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private BaseRes MobileLoginByVerCode(MobileReq mobileReq)
        {
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            string Url = string.Empty;
            string resultCode = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                //第三步 校验验证码
                logDtl = new ApplyLogDtl("校验验证码");

                string uvc = string.Empty;
                Url = string.Format("http://uac.10010.com/portal/Service/CtaIdyChk?verifyCode={0}&verifyType=1", mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "校验验证码");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                resultCode = jsonParser.GetResultFromParser(httpResult.Html, "resultCode");
                if (!bool.Parse(resultCode))
                {
                    Res.StatusDescription = "验证码错误";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode("验证码错误");

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                if (cookies["uacverifykey"].Name.Equals("uacverifykey"))
                    uvc = cookies["uacverifykey"].Value;

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "校验验证码成功";
                appLog.LogDtlList.Add(logDtl);

                //第四步 登录
                logDtl = new ApplyLogDtl("登录");

                Url = string.Format("https://uac.10010.com/portal/Service/MallLogin?userName={0}&password={1}&pwdType=01&productType=01&verifyCode={2}&redirectType=03&uvc={3}", mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode, uvc);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                resultCode = CommonFun.GetMidStr(httpResult.Html, "resultCode:\"", "\",");
                if (resultCode != "0000")
                {
                    string msg = jsonParser.GetResultFromParser(httpResult.Html, "msg");
                    string domhtml = string.Empty;
                    if (msg.Contains("用户名或密码不正确"))
                    {
                        msg = CommonFun.GetMidStr(msg, "", "<a href");
                    }
                    else
                    {
                        domhtml = CommonFun.GetMidStr(msg, "<a", "\">");
                        if (!domhtml.IsEmpty())
                        {
                            msg = msg.ToTrim("<a" + domhtml + "\">").ToTrim("</a>");
                        }
                    }
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(msg);
                    Res.StatusDescription = msg;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;

                CacheHelper.SetCache(mobileReq.Token, cookies);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = Res.StatusCode;
                logDtl.Description = Res.StatusDescription;
                appLog.LogDtlList.Add(logDtl);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "中国联通登录异常";
                Log4netAdapter.WriteError("中国联通登录异常", e);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            return Res;
        }

        /// <summary>
        /// 登录（不需要验证码）
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private BaseRes MobileLoginNoVerCode(MobileReq mobileReq)
        {
            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            Res.Token = mobileReq.Token;
            List<string> results = new List<string>();
            try
            {
                //登录
                logDtl = new ApplyLogDtl("登录");

                Url = string.Format("https://uac.10010.com/portal/Service/MallLogin?userName={0}&password={1}&pwdType=01&productType=01&redirectType=01&rememberMe=1", mobileReq.Mobile, mobileReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "登录");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string resultCode = CommonFun.GetMidStr(httpResult.Html, "resultCode:\"", "\",");
                if (resultCode != "0000")
                {
                    string msg = jsonParser.GetResultFromParser(httpResult.Html, "msg");
                    string domhtml = string.Empty;
                    if (msg.Contains("用户名或密码不正确"))
                    {
                        msg = CommonFun.GetMidStr(msg, "", "<a href");
                    }
                    else
                    {
                        domhtml = CommonFun.GetMidStr(msg, "<a", "\">");
                        if (!domhtml.IsEmpty())
                        {
                            msg = msg.ToTrim("<a" + domhtml + "\">").ToTrim("</a>");
                        }
                    }
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(msg);
                    Res.StatusDescription = msg;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }
                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;

                CacheHelper.SetCache(Res.Token, cookies);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = Res.StatusCode;
                logDtl.Description = Res.StatusDescription;
                appLog.LogDtlList.Add(logDtl);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "中国联通登录异常";
                Log4netAdapter.WriteError("中国联通登录异常", e);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_error;
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            return Res;
        }

        /// <summary>
        /// 查询账单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private void GetBill(CrawlerData crawler)
        {
            DateTime date = DateTime.Now;
            string Url = string.Empty;
            string postdata = string.Empty;

            for (int i = 0; i <= 5; i++)
            {
                date = DateTime.Now.AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7) + "月账单抓取");

                try
                {
                    if (i == 0)
                    {
                        Url = string.Format("http://iservice.10010.com/e3/static/query/currentFee?_={0}&menuid=000100010001", GetTimeStamp());
                        postdata = string.Format("1");
                    }
                    else
                    {
                        Url = string.Format("http://iservice.10010.com/e3/static/query/queryHistoryBill?_={0}&menuid=000100020001", GetTimeStamp());
                        postdata = string.Format("querytype=0001&querycode=0001&billdate={0}", date.ToString("yyyyMM"));
                    }
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtmlLog(httpItem, appLog, date.ToString(Consts.DateFormatString7) + "月账单抓取");
                    if (httpResult.StatusCode == HttpStatusCode.OK)
                    {
                        Object obj = JsonConvert.DeserializeObject(httpResult.Html);
                        JObject historyjs = obj as JObject;
                        string issuccess = "true";
                        string err = string.Empty;
                        if (historyjs["issuccess"] != null)
                        {
                            issuccess = historyjs["issuccess"].ToString();
                            if (!bool.Parse(issuccess.ToString()))
                                err = historyjs["errormessage"]["errmessage"].ToString();
                        }
                        else if (historyjs["historyResultState"] != null)
                        {
                            issuccess = historyjs["historyResultState"].ToString() == "success" ? "true" : "false";
                            if (!bool.Parse(issuccess.ToString()))
                                err = "";
                        }
                        else if (historyjs["rspArgs"]["isrealtimeError"] != null)
                        {
                            issuccess = historyjs["rspArgs"]["isrealtimeError"].ToString();
                            if (!bool.Parse(issuccess.ToString()))
                                err = historyjs["rspArgs"]["errorMessage"].ToString();
                        }
                        if (!bool.Parse(issuccess.ToString()))
                        {
                            logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = date.ToString("yyyy-MM-dd") + "月账单抓取失败：" + err;
                            appLog.LogDtlList.Add(logDtl);
                            continue;
                        }
                        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                        crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                        logDtl.StatusCode = ServiceConsts.StatusCode_success;
                        logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                        logDtl.Description = date.ToString(Consts.DateFormatString7) + "账单抓取成功";
                        appLog.LogDtlList.Add(logDtl);
                    }
                }
                catch (Exception e)
                {
                    logDtl.StatusCode = ServiceConsts.StatusCode_error;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = date.ToString("yyyy-MM-dd") + "月账单抓取异常：" + e.Message;
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
        private void ReadBill(CrawlerData crawler, Basic mobile)
        {
            string PhoneBillStr = string.Empty;
            MonthBill bill = null;
            DateTime date;
            object obj = null;
            for (int i = 0; i <= 5; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis, date.ToString("yyyy-MM-dd") + "月账单解析");
                try
                {
                    PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    obj = JsonConvert.DeserializeObject(PhoneBillStr);
                    if (i == 0)
                    {
                        JObject js = obj as JObject;
                        if (js["rspArgs"]["isrealtimeError"] != null && !bool.Parse(js["rspArgs"]["isrealtimeError"].ToString())) continue;
                        JObject bdp = js["rspArgs"] as JObject;
                        JArray billdata = bdp["result"] as JArray;
                        if (billdata != null)
                        {
                            int billLength = billdata.Count;
                            bill = new MonthBill();
                            bill.BillCycle = date.ToString(Consts.DateFormatString12);
                            bill.PlanAmt = ((JObject)billdata[0])["fee"] != null ? ((JObject)billdata[0])["fee"].ToString() : "0.00";
                            if (billLength > 0)
                            {
                                bill.TotalAmt = ((JObject)billdata[billLength - 1])["fee"] != null ? ((JObject)billdata[billLength - 1])["fee"].ToString() : "0.00";
                                JArray list = (JArray)((JObject)billdata[billLength - 1])["content"];
                                if (list != null && list.Count > 0)
                                    bill.PlanAmt = list[list.Count - 1][1].ToString();
                            }
                            if (bdp["realfee"] != null)
                            {
                                bill.TotalAmt = bdp["realfee"].ToString();
                                bill.PlanAmt = ((JObject)billdata[billLength - 1])["fee"] != null ? ((JObject)billdata[billLength - 1])["fee"].ToString() : "0.00";
                            }
                        }
                    }
                    else
                    {
                        JObject historyjs = obj as JObject;
                        string issuccess = "true";
                        string err = string.Empty;
                        if (historyjs["issuccess"] != null)
                            issuccess = historyjs["issuccess"].ToString();
                        else if (historyjs["historyResultState"] != null)
                            issuccess = historyjs["historyResultState"].ToString() == "success" ? "true" : "false";
                        if (!bool.Parse(issuccess)) continue;
                        JObject result = historyjs["result"] as JObject;
                        JArray historybilldata = historyjs["historyResultList"] as JArray;
                        if (historybilldata == null)
                        {
                            JArray billinfo = result["billinfo"] as JArray;
                            if (billinfo != null)
                            {
                                bill = new MonthBill();
                                bill.BillCycle = date.ToString(Consts.DateFormatString12);
                                bill.PlanAmt = ((JObject)billinfo[0])["fee"].ToString();
                                bill.TotalAmt = result["allfee"] != null ? result["allfee"].ToString() : "0.00";
                            }
                        }
                        else
                        {
                            if (historybilldata != null)
                            {
                                bill = new MonthBill();
                                bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);
                                bill.PlanAmt = ((JObject)historybilldata[0])["value"].ToString();
                                bill.TotalAmt = historyjs["nowFee"] != null ? historyjs["nowFee"].ToString() : "0.00";
                            }
                        }
                    }
                    if (!bill.PlanAmt.IsEmpty())
                        bill.PlanAmt = bill.PlanAmt.Trim();
                    if (!bill.TotalAmt.IsEmpty())
                        bill.TotalAmt = bill.TotalAmt.Trim();
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
        /// 查询详单
        /// </summary>
        /// <param name="queryType"></param>
        /// <param name="menuid"></param>
        /// <param name="refererType"></param>
        /// <returns></returns>
        private void GetDeatils(EnumMobileDeatilType type, CrawlerData crawler)
        {
            string postdata = string.Empty;
            string url = string.Empty;
            string referer = string.Empty;
            DateTime date = DateTime.Now;
            DateTime first = date;
            DateTime last = date;
            string queryType = string.Empty;
            string menuid = string.Empty;
            string refererType = string.Empty;
            string title = string.Empty;

            if (type == EnumMobileDeatilType.Call)
            {
                queryType = "callDetail";
                menuid = "000100030001";
                refererType = "call_dan";
            }
            else if (type == EnumMobileDeatilType.SMS)
            {
                queryType = "sms";
                menuid = "000100030002";
                refererType = "call_sms";
            }
            else
            {
                queryType = "callFlow";
                menuid = "000100030004";
                refererType = "call_flow";
            }

            #region 抓取校验
            title = "抓取校验校验";
            url = String.Format("http://iservice.10010.com/e3/static/query/checkmapExtraParam?_={0}", GetTimeStamp());
            referer = string.Format("http://iservice.10010.com/e3/query/{0}.html", refererType);
            postdata = String.Format("menuId={0}", menuid);
            httpItem = new HttpItem()
            {
                URL = url,
                Method = "POST",
                Postdata = postdata,
                Referer = referer,
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection,
                Accept = "application/json, text/javascript, */*; q=0.01",
            };
            httpResult = httpHelper.GetHtmlLog(httpItem, appLog, title);
            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
            #endregion

            //前五个月详单
            for (int i = 0; i <= 5; i++)
            {

                int totalPages = 1;
                int pageSize = 500;
                int pageIndex = 1;
                first = new DateTime(date.Year, date.Month, 1).AddMonths(-i);
                last = first.AddMonths(1).AddDays(-1);
                if (i == 0 && queryType == "sms")
                    last = DateTime.Now;
                do
                {
                    title = date.AddMonths(-i).ToString(Consts.DateFormatString7) + "月详单第" + pageIndex + "页抓取";
                    logDtl = new ApplyLogDtl(title);

                    url = string.Format("http://iservice.10010.com/e3/static/query/{0}?_={2}&menuid={1}", queryType, menuid, GetTimeStamp());
                    referer = string.Format("http://iservice.10010.com/e3/query/{0}.html", refererType);
                    if (queryType == "sms")
                        postdata = string.Format("pageNo={2}&pageSize={3}&begindate={0}&enddate={1}", first.ToString("yyyyMMdd"), last.ToString("yyyyMMdd"), pageIndex, pageSize);
                    else
                        postdata = string.Format("pageNo={2}&pageSize={3}&beginDate={0}&endDate={1}", first.ToString("yyyy-MM-dd"), last.ToString("yyyy-MM-dd"), pageIndex, pageSize);

                    httpItem = new HttpItem()
                    {
                        URL = url,
                        Method = "POST",
                        Postdata = postdata,
                        Referer = referer,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection,
                        Accept = "application/json, text/javascript, */*; q=0.01",
                    };
                    httpResult = httpHelper.GetHtmlLog(httpItem, appLog, title);
                    if (httpResult.StatusCode == HttpStatusCode.OK)
                    {
                        try
                        {
                            object obj = JsonConvert.DeserializeObject(httpResult.Html);
                            JObject bill = obj as JObject;
                            string isSuccess = (queryType == "callFlow" ? bill["issuccess"] : bill["isSuccess"]).ToString();
                            if (!bool.Parse(isSuccess))
                            {
                                string errorMessage = queryType == "callFlow" ? CommonFun.GetMidStr(httpResult.Html, "errmessage\":\"", "\"},") : CommonFun.GetMidStr(httpResult.Html, "respDesc\":\"", "\"},");
                                logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                                logDtl.Description = title + "失败：" + errorMessage;
                                appLog.LogDtlList.Add(logDtl);
                                pageIndex = pageIndex + 1;
                                continue;
                            }
                            crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + "0" + (i + 1) + pageIndex, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                            if (type == EnumMobileDeatilType.Net)
                                totalPages = bill["result"]["pageMap"]["totalPages"].ToString().ToInt(0);
                            else
                                totalPages = bill["pageMap"]["totalPages"].ToString().ToInt(0);

                            logDtl.StatusCode = ServiceConsts.StatusCode_success;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = title + "成功";
                            appLog.LogDtlList.Add(logDtl);
                        }
                        catch (Exception e)
                        {
                            logDtl.StatusCode = ServiceConsts.StatusCode_error;
                            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                            logDtl.Description = title + "异常：" + e.Message + "原始数据：" + httpResult.Html;
                            appLog.LogDtlList.Add(logDtl);
                            pageIndex = pageIndex + 1;

                            continue;
                        }
                    }
                    pageIndex = pageIndex + 1;
                    if (pageIndex >= totalPages) break;
                }
                while (pageIndex <= totalPages);
            }
        }

        /// <summary>
        /// 读取详单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private void ReadDeatils<T>(EnumMobileDeatilType type, CrawlerData crawler, Action<List<T>> action)
        {
            List<CrawlerDtlData> PhoneCrawlerDtls = new List<CrawlerDtlData>();
            string PhoneCostStr = string.Empty;
            DateTime date;

            for (int i = 0; i <= 5; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7));
                PhoneCrawlerDtls = crawler.DtlList.Where(x => x.CrawlerTitle.StartsWith(type + "0" + (i + 1))).OrderBy(x => x.CrawlerTitle).ToList<CrawlerDtlData>();
                if (PhoneCrawlerDtls == null || PhoneCrawlerDtls.Count <= 0) continue;
                foreach (CrawlerDtlData item in PhoneCrawlerDtls)
                {
                    try
                    {
                        PhoneCostStr = System.Text.Encoding.Default.GetString(item.CrawlerTxt);
                        string isSuccess = type == EnumMobileDeatilType.Net ? jsonParser.GetResultFromParser(PhoneCostStr, "issuccess") : jsonParser.GetResultFromParser(PhoneCostStr, "isSuccess");
                        if (!bool.Parse(isSuccess)) continue;
                        if (type == EnumMobileDeatilType.Net)
                        {
                            PhoneCostStr = jsonParser.GetResultFromParser(PhoneCostStr, "result");
                            PhoneCostStr = jsonParser.GetResultFromParser(PhoneCostStr, "pagemap");
                        }
                        else
                        {
                            PhoneCostStr = jsonParser.GetResultFromParser(PhoneCostStr, "pageMap");
                        }
                        PhoneCostStr = jsonParser.GetResultFromParser(PhoneCostStr, "result");
                        action(jsonParser.DeserializeObject<List<T>>(PhoneCostStr));
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
        }

        #endregion

    }

    #region  实体

    /// <summary>
    /// 话费详单
    /// </summary>
    internal class CallDetail
    {
        public string calldate { get; set; }
        public string calllonghour { get; set; }
        public string calltime { get; set; }
        public string calltype { get; set; }
        public string calltypeName { get; set; }
        public string cellid { get; set; }
        public string deratefee { get; set; }
        public string homearea { get; set; }
        public string homeareaName { get; set; }
        public string homenum { get; set; }
        public string landfee { get; set; }
        public string landtype { get; set; }
        public string longtype { get; set; }
        public string nativefee { get; set; }
        public string otherarea { get; set; }
        public string otherareaName { get; set; }
        public string otherfee { get; set; }
        public string othernum { get; set; }
        public string roamfee { get; set; }
        public string romatype { get; set; }
        public string romatypeName { get; set; }
        public string thtype { get; set; }
        public string thtypeName { get; set; }
        public string totalfee { get; set; }
        public string twoplusfee { get; set; }
    }

    /// <summary>
    /// 短信详单
    /// </summary>
    internal class SmsDetail
    {
        public string amount { get; set; }
        public string businesstype { get; set; }
        public string deratefee { get; set; }
        public string fee { get; set; }
        public string homearea { get; set; }
        public string otherarea { get; set; }
        public string othernum { get; set; }
        public string smsdate { get; set; }
        public string smstime { get; set; }
        public string smstype { get; set; }
    }

    /// <summary>
    /// 上网详单
    /// </summary>
    internal class CallFlowDetail
    {
        public string begindate { get; set; }
        public string begintime { get; set; }
        public string deratefee { get; set; }
        public string extpara { get; set; }
        public string fee { get; set; }
        public string feetype { get; set; }
        public string homearea { get; set; }
        public string homeareaName { get; set; }
        public string longhour { get; set; }
        public string nettype { get; set; }
        public string roamstat { get; set; }
        public string svcname { get; set; }
        public string totalbytes { get; set; }
        public string totalfee { get; set; }
    }

    #endregion

}
