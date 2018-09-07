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
using System.Text.RegularExpressions;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{
    public class GX : ChinaNet
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

                Url = "http://gx.189.cn/service/bill/getRand.jsp";
                postdata = string.Format("PRODTYPE=2100297&RAND_TYPE=002&BureauCode=1100&ACC_NBR={0}&PROD_TYPE=2100297&PROD_PWD=&REFRESH_FLAG=1&BEGIN_DATE=&END_DATE=&ACCT_DATE={2}&FIND_TYPE=1032&SERV_NO=&QRY_FLAG=1&MOBILE_NAME={1}&OPER_TYPE=CR1&PASSWORD=", mobileReq.Mobile, mobileReq.Mobile, DateTime.Now.ToString("yyyyMM"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                string res = getStringByElementName(httpResult.Html, "flag");
                if (res != "0")
                {
                    Res.StatusDescription = "短信发送失败";
                    var msg = HtmlParser.GetResultFromParser(httpResult.Html, "//msg", "");
                    if (msg.Count != 0)
                    {
                        Res.StatusDescription = msg[0];
                    }
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
                Res.StatusDescription = "广西电信手机验证码发送异常";
                Log4netAdapter.WriteError("广西电信手机验证码发送异常", e);
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
                Url = "http://gx.189.cn/service/bill/fycx/inxxall.jsp";
                postdata = string.Format("PRODTYPE=2100297&RAND_TYPE=002&BureauCode=1100&ACC_NBR={0}&PROD_TYPE=2100297&PROD_PWD=&REFRESH_FLAG=1&BEGIN_DATE=&END_DATE=&ACCT_DATE={3}&FIND_TYPE=1032&SERV_NO=&QRY_FLAG=1&MOBILE_NAME={1}&OPER_TYPE=CR1&PASSWORD={2}", mobileReq.Mobile, mobileReq.Mobile, mobileReq.Smscode, date.ToString("yyyyMM"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var msg = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table']/tr/td[@colspan='2']", "inner");
                if (msg.Count > 0)
                {
                    if (!msg[0].Contains("button"))
                    {
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        Res.StatusDescription = msg[0];
                        return Res;
                    }
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.StatusDescription = "广西电信手机验证码验证成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Query;
                Res.Token = "06fd44e189f049449f0fd02f28e0d96b";
                CacheHelper.SetCache(Res.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "广西电信手机验证码验证异常";
                Log4netAdapter.WriteError("广西电信手机验证码验证异常", e);
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
                #region 基本信息

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Crawler);

                #region  用户基本信息
                Url = "http://gx.189.cn/service/manage/";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    //Referer = location,
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
                Url = "http://gx.189.cn/service/manage/contactslist.jsp?RC=";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    //Method = "Post",
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
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "detailInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                #endregion

                #region PUK查询
                Url = "http://gx.189.cn/cdma133/hotQry/puk_qry.jsp?SERV_NO=FSU-3-4";
                postdata = string.Format("FLAG=1&SERV_KIND=&SERV_TYPE=&USER_ID_NUM={0}&AREA_CODE=2500&AccNbr={1}", mobileReq.Mobile, mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
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
                #endregion

                #region  积分查询
                string ListFlag = string.Empty;
                string AccNbr = string.Empty;
                Url = "http://gx.189.cn/service/bonus/bounsQry.jsp?/service/bonus/index.jsp?SERV_NO=FSE-9-22-1";
                postdata = string.Format("IS_LOGIN=1");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
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

                Url = "http://gx.189.cn/service/bonus/searchBonus.jsp";
                postdata = string.Format("ListFlag=1&AccNbr=40002577782");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
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
                #endregion

                #region 套餐查询
                Url = "http://gx.189.cn/chaxun/ajax/tcsylcx.jsp";
                postdata = string.Format("ACC_NBR={0}&PROD_TYPE=2020966&ACCT_NBR_97=104002702581297", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
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

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "基本信息抓取成功";
                appLog.LogDtlList.Add(logDtl);

                #endregion

                #region  账单查询
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Crawler);

                CrawlerBill(crawler, mobileReq);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 详单查询
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Crawler);
                Url = "http://gx.189.cn/chaxun/ajax/qdcx.jsp";
                postdata = string.Format("ACC_NBR={0}&PROD_TYPE=2100297&ACCT_NBR_97=104002712186885", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                var typelist = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@id='FIND_TYPE']/option", "");
                var valuelist = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@id='FIND_TYPE']/option", "value");

                //详单集合
                var deatillist = new string[] { "市话清单", "短信彩信清单", "长话清单", "国际及港澳台漫游通话清单", "漫游通话清单", "上网及数据通信详单" };
                for (int i = 0; i < typelist.Count; i++)
                {
                    if (deatillist.Contains(typelist[i]))
                    {
                        CrawlerDeatils(valuelist[i], typelist[i], mobileReq, crawler);
                    }

                }
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "详单抓取成功";
                appLog.LogDtlList.Add(logDtl);


                #endregion

                //保存
                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "广西电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusDescription = "广西电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("广西电信手机账单抓取异常", e);

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
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            MobileMongo mobileMongo = new MobileMongo(appDate);
            CrawlerData crawler = new CrawlerData();
            Basic mobile = new Basic();
            List<string> results = new List<string>();
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

                #region 用户基本信息
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                mobile.Mobile = mobileReq.Mobile;
                results = HtmlParser.GetResultFromParser(result, "//table[@class='table']/tbody/tr[1]/td[2]", "");
                if (results.Count > 0)
                {
                    mobile.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(result, "//div[@id='contact_json_div']", "");
                var userinfo = results[0].Replace("&#034;", "").Split('[')[1].Split(']')[0];
                mobile.Address = userinfo.Split(':')[9].Split(',')[0];  //家庭地址
                #endregion

                #region 套餐
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//div[@class='xzMun']/h3", "");
                if (results.Count > 0)
                {
                    mobile.Package = CommonFun.GetMidStr(results[0], "您的套餐：", "");
                }
                #endregion

                #region 积分
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                var point = CommonFun.GetMidStr(result.ToString(), "$(\"#bonus\").html('", "');");
                if (!string.IsNullOrEmpty(point))
                {
                    mobile.Integral = point;
                }
                #endregion

                #region PUK码
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "PUKInfor").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//table[1]/tr[2]/td/table/tbody/tr/td/table/tr[1]/td[1]", "");
                if (results.Count > 0)
                {
                    mobile.PUK = results[0];
                }
                #endregion

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "个人信息解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 账单信息
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis);

                AnalysisBill(crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 详单查询

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Analysis);
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "detailInfor").FirstOrDefault().CrawlerTxt);
                var typelist = HtmlParser.GetResultFromParser(result, "//select[@id='FIND_TYPE']/option", "");
                var valuelist = HtmlParser.GetResultFromParser(result, "//select[@id='FIND_TYPE']/option", "value");

                //详单集合
                var deatillist = new string[] { "市话清单", "短信彩信清单", "长话清单", "国际及港澳台漫游通话清单", "漫游通话清单", "上网及数据通信详单" };

                for (int i = 0; i < typelist.Count; i++)
                {
                    if (deatillist.Contains(typelist[i]))
                    {
                        AnalysisDeatils(crawler, valuelist[i], typelist[i], mobile);
                    }

                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                //保存
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "广西电信手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "广西电信手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("广西电信手机账单解析异常", e);

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
            string acctype = string.Empty;
            for (var i = 0; i <= 5; i++)
            {
                if (i == 0)
                {
                    Url = "http://gx.189.cn/chaxun/ajax/hfcx.jsp";
                    postdata = string.Format("ACC_NBR={0}&PROD_TYPE=2020966&ACCT_NBR_97=104002702581297", mobileReq.Mobile);
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

                    Url = "http://gx.189.cn/public/billinfo.jsp?PHONE_NUM=" + mobileReq.Mobile;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                }
                else
                {
                    DateTime date = DateTime.Now;
                    Url = "http://gx.189.cn/chaxun/ajax/zdcx.jsp";
                    postdata = string.Format("ACC_NBR={0}&PROD_TYPE=2020966&ACCT_NBR_97=104002702581297", mobileReq.Mobile);
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

                    Url = string.Format("http://gx.189.cn/service/bill/fycx/cust_zd.jsp?ACC_NBR={0}&DATE={1}&_=144446871025600", mobileReq.Mobile, date.AddMonths(-i).ToString("yyyyMM"));
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
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
            List<string> infos = new List<string>();
            MonthBill bill = null;
            for (var i = 0; i <= 5; i++)
            {
                PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                bill = new MonthBill();
                bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);
                if (i == 0)
                {
                    infos = HtmlParser.GetResultFromParser(PhoneBillStr, "//table/tr[3]/td[2]", "");
                    if (infos.Count != 0)
                    {
                        bill.PlanAmt = infos[0];
                    }
                    var count = HtmlParser.GetResultFromParser(PhoneBillStr, "//table/tr", "").Count;
                    infos = HtmlParser.GetResultFromParser(PhoneBillStr, "//table/tr[" + count + "]/td", "");
                    if (infos.Count != 0)
                    {
                        bill.TotalAmt = Regex.Replace(infos[0], @"[^\d.\d]", "");
                    }
                }
                else
                {
                    infos = HtmlParser.GetResultFromParser(PhoneBillStr, "//div[@id='divFee']/div/table/tr[1]/td[1]/table/tr[3]/td[1]/div[2]", "");
                    if (infos.Count > 0)
                    {
                        bill.PlanAmt = infos[0];
                    }
                    infos = HtmlParser.GetResultFromParser(PhoneBillStr, "//div[@id='divFee']/div/table/tr[1]/td[1]/table/tr[6]/td", "");
                    if (infos.Count > 0)
                    {
                        bill.TotalAmt = infos[0].Split('<')[0].Split(':')[1].Split('元')[0];
                    }
                }
                ///添加账单
                mobile.BillList.Add(bill);
            }
        }

        /// <summary>
        /// 抓取手机详单
        /// </summary>
        /// <param name="queryType">详单编号</param>
        /// <param name="queryName">详单名称</param>
        /// <returns></returns>
        private void CrawlerDeatils(string queryType, string queryName, MobileReq mobileReq, CrawlerData crawler)
        {
            string Url = string.Empty;
            var postdata = string.Empty;
            string records = string.Empty;
            for (int i = 0; i <= 5; i++)
            {
                DateTime date = DateTime.Now;
                date = date.AddMonths(-i);
                Url = "http://gx.189.cn/service/bill/fycx/inxxall.jsp";
                postdata = string.Format("ACC_NBR={2}&PROD_TYPE=2020966&BEGIN_DATE=&END_DATE=&REFRESH_FLAG=1&FIND_TYPE={1}&radioQryType=on&QRY_FLAG=1&ACCT_DATE={0}&ACCT_DATE_1=201511", date.ToString("yyyyMM"), queryType, mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = queryType + "0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
            }

        }

        /// <summary>
        /// 解析手机详单
        /// </summary>
        /// <param name="queryType">1 获取通话记录，3 上网详单 2 短信详单</param>
        /// <returns></returns>
        private void AnalysisDeatils(CrawlerData crawler, string queryType, string queryName, Basic mobile)
        {
            string PhoneCostStr = string.Empty;
            for (int i = 0; i <= 5; i++)
            {
                PhoneCostStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (queryType + "0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                var infos = HtmlParser.GetResultFromParser(PhoneCostStr, "//table[@id='list_table']/tr", "inner");
                foreach (var item in infos)
                {
                    //短信详单
                    if (queryName == "短信彩信清单")
                    {
                        List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        Sms sms = new Sms();
                        if (tdRow.Count != 8 || tdRow.Contains("序号"))
                        {
                            continue;
                        }
                        if (tdRow.Count > 0)
                        {
                            sms.OtherSmsPhone = tdRow[4];  //对方号码
                            sms.InitType = tdRow[2];  //通信方式（发送接收）
                            sms.StartTime = DateTime.Parse(tdRow[5]).ToString(Consts.DateFormatString11);  //起始时间
                            sms.SubTotal = tdRow[7].ToDecimal(0);  //通信费
                            sms.SmsType = tdRow[1];  //通信类型（短信，彩信）
                            mobile.SmsList.Add(sms);
                        }
                    }
                    else if (queryName == "上网及数据通信详单")  //上网详单
                    {
                        List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        if (tdRow.Count != 7 || tdRow.Contains("序号"))
                        {
                            continue;
                        }
                        Net gprs = new Net();
                        if (tdRow.Count > 0)
                        {
                            gprs.StartTime = DateTime.Parse(tdRow[1]).ToString(Consts.DateFormatString11);  //开始时间
                            gprs.Place = tdRow[4];  //地点
                            gprs.NetType = tdRow[3];  // 网络类型
                            gprs.SubTotal = tdRow[5].ToDecimal(0);  //费用
                            gprs.SubFlow = tdRow[2];  //流量
                            gprs.PhoneNetType = tdRow[6];  //上网方式
                            mobile.NetList.Add(gprs);
                        }
                    }
                    else  //通话详单
                    {

                        List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        if (tdRow.Count != 10 || tdRow.Contains("序号"))
                        {
                            continue;
                        }
                        Call call = new Call();
                        if (tdRow.Count > 0)
                        {
                            call.StartTime = DateTime.Parse(tdRow[5]).ToString(Consts.DateFormatString11);  //起始时间
                            call.OtherCallPhone = tdRow[4];  //对方号码
                            call.UseTime = tdRow[6] + "秒";  //通话时长
                            call.CallType = tdRow[9];  //通话类型
                            call.CallPlace = tdRow[2];  //通话地点
                            call.InitType = tdRow[1];  //呼叫类型（主叫被叫）
                            call.SubTotal = tdRow[8].ToDecimal(0);  //通话费用
                            mobile.CallList.Add(call);
                        }
                    }

                }
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
                                    break;

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
