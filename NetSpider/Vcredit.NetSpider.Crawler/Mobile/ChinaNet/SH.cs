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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.Common.Constants;
using System.Text.RegularExpressions;
using Vcredit.NetSpider.Entity.Mongo.Log;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{
    public class SH : ChinaNet
    {
        public override VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            VerCodeRes Res = new VerCodeRes();
            Res.Token = mobileReq.Token;
            cookies = new CookieCollection();
            string Url = string.Empty;
            try
            {
                ////获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                    //CacheHelper.RemoveCache(token);
                }
                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=93507&toStUrl=http://service.sh.189.cn/service/mobileLogin";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://service.sh.189.cn/service/mobileLogin";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                Url = "http://service.sh.189.cn/service/account";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://service.sh.189.cn/service/service/authority/query/billdetail/sendCode.do?flag=1&devNo={0}&dateType=&startDate=&endDate=";
                httpItem = new HttpItem()
                {
                    URL = String.Format(Url, mobileReq.Mobile),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection

                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                object result;
                string success = string.Empty;
                try
                {
                    result = JsonConvert.DeserializeObject(httpResult.Html);
                    JObject js = result as JObject;
                    if (js != null)
                    {
                        success = js["RESULT"]["result"].ToString();
                    }
                }
                catch
                {
                    success = "False";
                }
                if (success != "True")
                {
                    Res.StatusDescription = "短信码发送失败";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                    return Res;
                }

                Res.StatusDescription = "短信码已发送";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "上海电信手机验证码发送异常";
                Log4netAdapter.WriteError("上海电信手机验证码发送异常", e);
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
            appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            cookies = new CookieCollection();
            string Url = string.Empty;

            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                    //CacheHelper.RemoveCache(mobileReq.Token);
                }

                Url = "http://service.sh.189.cn/service/service/authority/query/billdetail/validate.do?input_code={0}&selDevid={1}&flag=nocw";
                httpItem = new HttpItem()
                {
                    URL = String.Format(Url, mobileReq.Smscode, mobileReq.Mobile),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                object result;
                string success = string.Empty;
                try
                {
                    result = JsonConvert.DeserializeObject(httpResult.Html);
                    JObject js = result as JObject;
                    if (js != null)
                    {
                        success = js["CODE"].ToString();
                    }
                }
                catch
                {
                    success = "1";
                }
                if (success != "0")
                {
                    Res.StatusDescription = "短信码验证失败";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription);
                    return Res;
                }


                Res.StatusDescription = "短信码验证成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Query;
                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "上海电信手机验证码验证异常";
                Log4netAdapter.WriteError("上海电信手机验证码验证异常", e);
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
            appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            ApplyLogDtl logDtl = new ApplyLogDtl("");
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            CrawlerData crawler = new CrawlerData() { Token = mobileReq.Token, IdentityCard = mobileReq.IdentityCard, Name = mobileReq.Name, Mobile = mobileReq.Mobile, UserType = "1" };
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

                #region 基本信息
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Crawler);

                //个人信息
                Url = "http://service.sh.189.cn/service/mytelecom/cusInfo";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = "http://service.sh.189.cn/service/my/basicinfo.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                //手机套餐
                Url = "http://service.sh.189.cn/service/mytelecom/deviceInfo";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = "http://service.sh.189.cn/service/my/deviceinfo.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = String.Format("devNo={0}", mobileReq.Mobile),
                    Method = "post",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                    Referer = "http://service.sh.189.cn/service/mytelecom/cusInfo"
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "packageInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                //积分
                Url = "http://service.sh.189.cn/service/service/authority/queryInfo/getMyPointsByCrmid.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Method = "post",
                    Postdata = String.Format("DeviceId={0}", mobileReq.Mobile),
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                Url = "http://service.sh.189.cn/service/service/authority/query/queryPuk";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = "value=" + mobileReq.Mobile,
                    Method = "post",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "pukInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });


                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "基本信息抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 账单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Crawler);

                CrawlerBill(crawler, mobileReq);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 通话详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.Call, mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 短信详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.SMS, mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 流量详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Crawler);

                CrawlerDeatils(EnumMobileDeatilType.Net, mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "上海电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "上海电信手机账单抓取异常";
                Log4netAdapter.WriteError("上海电信手机账单抓取异常", e);

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
            appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            ApplyLogDtl logDtl = new ApplyLogDtl("");
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            MobileMongo mobileMongo = new MobileMongo(appDate);
            CrawlerData crawler = new CrawlerData();
            Basic mobile = new Basic();
            string result = string.Empty;

            try
            {
                crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);

                #region 个人信息
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Analysis);

                mobile.Token = mobileReq.Token;
                mobile.BusName = mobileReq.Name;
                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.Mobile = mobileReq.Mobile;
                mobile.UpdateTime = crawler.CrawlerDate;

                //个人信息
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                    var baseresult = JsonConvert.DeserializeObject(result);
                    {
                        JObject js = baseresult as JObject;
                        if (js != null && js["RESULT"] != null)
                        {
                            mobile.Idcard = js["RESULT"]["MainIdenNumber"].ToString();
                            mobile.Idtype = js["RESULT"]["MainIdenType"].ToString();
                            mobile.Name = js["RESULT"]["CustNAME"].ToString();
                            mobile.Address = js["RESULT"]["PrAddrName"].ToString();
                            mobile.Email = js["RESULT"]["MainEmailAddr"].ToString();
                            mobile.Postcode = js["RESULT"]["PrAddrZipCode"].ToString();
                        }
                    }
                }
                catch { }

                //手机套餐
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault().CrawlerTxt);
                    var packageresult = JsonConvert.DeserializeObject(result);
                    {
                        JObject js = packageresult as JObject;
                        if (js != null && js["RESULT"] != null)
                        {
                            mobile.Regdate = DateTime.Parse(js["RESULT"]["InstallDate"].ToString()).ToString(Consts.DateFormatString11);
                            mobile.Package = js["RESULT"]["mainPromotionName"].ToString();
                            mobile.PackageBrand = js["RESULT"]["ProductName"].ToString();
                        }
                    }
                }
                catch { }

                //手机积分
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                    var integralresult = JsonConvert.DeserializeObject(result);
                    {
                        JObject js = integralresult as JObject;
                        if (js != null)
                        {
                            mobile.Integral = js["useablePoints"] != null ? (js["useablePoints"].ToString() == "[]" ? "" : js["useablePoints"].ToString()) : "";
                        }
                    }
                }
                catch { }

                //puk
                try
                {
                    result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "pukInfor").FirstOrDefault().CrawlerTxt);
                    var pukresult = JsonConvert.DeserializeObject(result);
                    {
                        JObject js = pukresult as JObject;
                        if (js != null)
                        {
                            if (js["Uchv"] != null)
                                mobile.PUK = js["Uchv"].ToString();
                        }
                    }
                }
                catch { }



                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "个人信息解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 账单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis);

                AnalysisBill(crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 通话详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Analysis);

                AnalysisDeatils(EnumMobileDeatilType.Call, crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 短信详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Analysis);

                AnalysisDeatils(EnumMobileDeatilType.SMS, crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 流量详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Analysis);

                AnalysisDeatils(EnumMobileDeatilType.Net, crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "上海电信手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "上海电信手机账单解析异常";
                Log4netAdapter.WriteError("上海电信手机账单解析异常", e);
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
            string Url = string.Empty;
            var month = String.Empty;
            string postdata = String.Empty;

            Url = "http://service.sh.189.cn/service/mobileBill.do";
            httpItem = new HttpItem()
            {
                URL = Url,
                Postdata = String.Format("device={0}&acctNum=", mobileReq.Mobile),
                Method = "post",
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
            var result3 = JsonConvert.DeserializeObject(httpResult.Html);
            {
                JObject js = result3 as JObject;
                if (js != null)
                {
                    var echartsInvoice = js["echartsInvoice"];

                    if (echartsInvoice != null)
                    {
                        var invoiceNo = jsonParser.DeserializeObject<JObject>(echartsInvoice.ToString());
                        if (js["fzxhNO"] == null) return;
                        var invoiceNoobj = invoiceNo["invoiceNo"];
                        var fzxhNO = js["fzxhNO"].ToString();
                        for (var i = 0; i <= 5; i++)
                        {
                            month = DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString7);
                            Url = "http://service.sh.189.cn/service/invoiceJump";
                            postdata = String.Format("dateArray=201504%2C201505%2C201506%2C201507%2C201508%2C201509&jeArray=238.30%2C258.60%2C253.50%2C241.10%2C246.30%2C261.40&breadcolors=-1%2C-1%2C-1%2C-1%2C-1%2C-1&noPayDate=&accNbr={0}&billingCycle={1}&balanceDue=0.00&invoiceNo={2}&deviceNum={3}", fzxhNO,
                               month, invoiceNoobj.ToString(), mobileReq.Mobile);
                            httpItem = new HttpItem()
                            {
                                URL = Url,
                                Postdata = postdata,
                                Method = "post",
                                CookieCollection = cookies,
                                ResultCookieType = ResultCookieType.CookieCollection
                            };
                            httpResult = httpHelper.GetHtml(httpItem);
                            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                            crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                        }
                    }
                }
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
                var billhtml = crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault();
                if (billhtml == null) continue;
                PhoneBillStr = System.Text.Encoding.Default.GetString(billhtml.CrawlerTxt);
                bill = new MonthBill();
                var TotalAmt = Regex.Matches(PhoneBillStr, @"(?<=本期费用合计\：).*(?=元)");
                bill.TotalAmt = TotalAmt.Count > 0 ? TotalAmt[0].ToString() : "";
                var PlanAmt = Regex.Matches(PhoneBillStr, @"(?<=套餐\（小计\：\&\#165\;).*(?=元)");
                if (PlanAmt.Count > 0)
                {
                    var result = 0d;
                    var sum = 0d;
                    foreach (var item in PlanAmt)
                    {
                        double.TryParse(item.ToString(), out result);
                        sum += result;
                    }
                    PlanAmt = Regex.Matches(PlanAmt[0].ToString(), @"\d+(.)?.*\d");
                    bill.PlanAmt = sum.ToString();
                }
                bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);
                mobile.BillList.Add(bill);
            }
        }

        /// <summary>
        /// 抓取手机详单
        /// </summary>
        /// <param name="queryType">SCP:通话；AAA:上网；SMSC:短信</param>
        /// <returns></returns>
        private void CrawlerDeatils(EnumMobileDeatilType type, MobileReq mobileReq, CrawlerData crawler)
        {
            string Url = string.Empty;
            var month = String.Empty;
            string bill_type = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                bill_type = "SCP";
            else if (type == EnumMobileDeatilType.SMS)
                bill_type = "SMSC";
            else
                bill_type = "AAA";

            for (var i = 0; i <= 5; i++)
            {
                month = DateTime.Now.AddMonths(-i).ToString("yyyy/MM");

                Url = "http://service.sh.189.cn/service/service/authority/query/billdetailQuery.do?begin=0&end=10&flag=1&devNo={0}&dateType=his&bill_type={1}&queryDate={2}&startDate=&endDate=";
                Url = String.Format(Url, mobileReq.Mobile, bill_type, month.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                var result2 = JsonConvert.DeserializeObject(httpResult.Html);
                {
                    JObject js = result2 as JObject;


                    if (js != null)
                    {

                        var countHttp = 0;
                        while (js["RESULT"] == null && countHttp < 4)
                        {
                            // System.Threading.Thread.Sleep(2000);
                            httpItem = new HttpItem()
                            {
                                URL = String.Format(Url, mobileReq.Mobile, bill_type, month.ToUrlEncode()),
                                CookieCollection = cookies,
                                ResultCookieType = ResultCookieType.CookieCollection
                            };
                            httpResult = httpHelper.GetHtml(httpItem);
                            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                            result2 = JsonConvert.DeserializeObject(httpResult.Html);
                            js = result2 as JObject;
                            countHttp++;
                        }
                        if (js["RESULT"] == null)
                        {
                            continue;
                        }
                        var rowarray = js["RESULT"]["pagedResult"] as JArray;
                        if (rowarray.Count > 0)
                        {
                            var rownub = Convert.ToInt32(rowarray[0]["sumRow"].ToString());
                            var page = Math.Ceiling(rownub / 10.0);
                            for (var k = 0; k <= page; k++)
                            {
                                var begin = k > 0 ? (k * 10) + 1 : 0;
                                var end = (k * 10) + 10;
                                Url = "http://service.sh.189.cn/service/service/authority/query/billdetailQuery.do?begin={3}&end={4}&flag=1&devNo={0}&dateType=his&bill_type={1}&queryDate={2}&startDate=&endDate=";
                                Url = String.Format(Url, mobileReq.Mobile, bill_type, month.ToUrlEncode(), begin, end);
                                httpItem = new HttpItem()
                                {
                                    URL = Url,
                                    CookieCollection = cookies,
                                    ResultCookieType = ResultCookieType.CookieCollection
                                };
                                httpResult = httpHelper.GetHtml(httpItem);
                                var result3 = JsonConvert.DeserializeObject(httpResult.Html);
                                {
                                    JObject js3 = result3 as JObject;
                                    var countdetail = 0;
                                    while (js3["RESULT"] == null && countdetail < 4)
                                    {
                                        //System.Threading.Thread.Sleep(500);
                                        httpItem = new HttpItem()
                                        {
                                            URL = String.Format(Url, mobileReq.Mobile, bill_type, month.ToUrlEncode(), begin, end),
                                            CookieCollection = cookies,
                                            ResultCookieType = ResultCookieType.CookieCollection
                                        };
                                        httpResult = httpHelper.GetHtml(httpItem);
                                        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                                        result3 = JsonConvert.DeserializeObject(httpResult.Html);
                                        js3 = result3 as JObject;
                                        countdetail++;
                                    }
                                    if (js3["RESULT"] == null)
                                    {
                                        continue;
                                    }
                                }
                                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + (i + 1).ToString() + (k + 1).ToString(), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 解析手机详单
        /// </summary>
        /// <param name="queryType">SCP:通话；AAA:上网；SMSC:短信</param>
        /// <returns></returns>
        private void AnalysisDeatils(EnumMobileDeatilType type, CrawlerData crawler, Basic mobile)
        {
            List<CrawlerDtlData> PhoneCrawlerDtls = new List<CrawlerDtlData>();
            string PhoneStr = string.Empty;
            Call phoneCall;
            Sms phoneSMS;
            Net phoneGPRS;
            for (var i = 0; i <= 5; i++)
            {
                PhoneCrawlerDtls = crawler.DtlList.Where(x => x.CrawlerTitle.StartsWith(type + (i + 1).ToString())).OrderBy(x => x.CrawlerTitle).ToList<CrawlerDtlData>();
                if (PhoneCrawlerDtls != null && PhoneCrawlerDtls.Count > 0)
                {
                    foreach (CrawlerDtlData item in PhoneCrawlerDtls)
                    {
                        PhoneStr = System.Text.Encoding.Default.GetString(item.CrawlerTxt);
                        var result = JsonConvert.DeserializeObject(PhoneStr);
                        {
                            JObject js = result as JObject;
                            if (js != null && js["RESULT"] != null)
                            {
                                JArray array = js["RESULT"]["pagedResult"] as JArray;
                                if (type == EnumMobileDeatilType.Call)
                                {
                                    for (var j = 1; j < array.Count; j++)
                                    {
                                        var totalSecond = 0;
                                        var usetime = array[j]["callDuriation"].ToString();
                                        if (!string.IsNullOrEmpty(usetime))
                                        {
                                            totalSecond = CommonFun.ConvertDate(usetime);
                                        }
                                        phoneCall = new Call();
                                        phoneCall.StartTime = DateTime.Parse(array[j]["beginTime"].ToString()).ToString(Consts.DateFormatString11);
                                        phoneCall.SubTotal = array[j]["totalFee"].ToString().ToDecimal().Value;
                                        phoneCall.UseTime = totalSecond.ToString();
                                        phoneCall.OtherCallPhone = array[j]["targetParty"].ToString();
                                        phoneCall.CallPlace = array[j]["callingPartyVisitedCity"].ToString();
                                        phoneCall.CallType = array[j]["longDistanceType"].ToString();
                                        phoneCall.InitType = array[j]["callType"].ToString();
                                        mobile.CallList.Add(phoneCall);
                                    }
                                }
                                else if (type == EnumMobileDeatilType.SMS)
                                {
                                    for (var j = 1; j < array.Count; j++)
                                    {
                                        phoneSMS = new Sms();
                                        phoneSMS.StartTime = DateTime.Parse(array[j]["beginTime"].ToString()).ToString(Consts.DateFormatString11);
                                        phoneSMS.SubTotal = array[j]["fee1"].ToString().ToDecimal().Value;
                                        phoneSMS.OtherSmsPhone = array[j]["targetParty"].ToString();
                                        phoneSMS.SmsType = array[j]["callType"].ToString();
                                        mobile.SmsList.Add(phoneSMS);
                                    }
                                }
                                else if (type == EnumMobileDeatilType.Net)
                                {
                                    for (var j = 1; j < array.Count; j++)
                                    {
                                        //单位换算
                                        var totalSecond = 0;
                                        var usetime = array[j]["duration"].ToString();
                                        if (!string.IsNullOrEmpty(usetime))
                                        {
                                            totalSecond = CommonFun.ConvertDate(usetime);
                                        }

                                        var subFlow = array[j]["totalVolumn"].ToString();
                                        var toatalFlow = subFlow;
                                        if (subFlow.Contains("MB"))
                                        {
                                            subFlow = subFlow.TrimEnd('B').TrimEnd('M');
                                            toatalFlow = (subFlow.ToDecimal(0) * 1024).ToString();
                                        }

                                        if (subFlow.Contains("GB"))
                                        {
                                            subFlow = subFlow.TrimEnd('B').TrimEnd('G');
                                            toatalFlow = (subFlow.ToDecimal(0) * 1024 * 1024).ToString();
                                        }


                                        phoneGPRS = new Net();
                                        phoneGPRS.StartTime = DateTime.Parse(array[j]["beginTime"].ToString()).ToString(Consts.DateFormatString11);
                                        phoneGPRS.SubTotal = array[j]["fee1"].ToString().ToDecimal().Value;
                                        phoneGPRS.UseTime = totalSecond.ToString();
                                        phoneGPRS.SubFlow = toatalFlow;
                                        phoneGPRS.PhoneNetType = array[j]["cdmaChargingType"].ToString();
                                        if (array[j]["roamingStatus"] != null)
                                            phoneGPRS.NetType = array[j]["roamingStatus"].ToString();
                                        mobile.NetList.Add(phoneGPRS);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

    }
}
