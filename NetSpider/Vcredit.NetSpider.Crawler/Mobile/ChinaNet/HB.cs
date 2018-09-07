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
using Newtonsoft.Json;
using Vcredit.NetSpider.Entity.Mongo.Log;
using Newtonsoft.Json.Linq;
namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{
    public class HB : ChinaNet
    {
        /// <summary>
        /// 发送短信验证码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
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
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                #region 发送动态验证码校验
                logDtl = new ApplyLogDtl("发送动态验证码校验");

                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10018&toStUrl=http://hb.189.cn/SSOtoWSSNew?toWssUrl=/pages/selfservice/custinfo/userinfo/userInfo.action&trackPath=FWBK";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://hb.189.cn/pages/login/sypay_group_new.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "发送动态验证码校验");
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

                Url = "http://hb.189.cn/feesquery_sentPwd.action";
                postdata = "productNumber=" + mobileReq.Mobile + "&oderType=&cityCode=0127&sentType=C&ip=0";
                httpItem = new HttpItem()
                {
                    Method = "post",
                    Postdata = postdata,
                    URL = Url,
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = "发送动态验证码校验";
                appLog.LogDtlList.Add(logDtl);

                #endregion

                Res.StatusDescription = "湖北电信手机验证码发送成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;

                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "湖北电信手机验证码发送异常";
                Log4netAdapter.WriteError("湖北电信手机验证码发送异常", e);

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

        /// <summary>
        /// 校验短信验证码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public override BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_CheckSMS, mobileReq.Website);
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            cookies = new CookieCollection();
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

                //第六步，验证手机验证码
                logDtl = new ApplyLogDtl("验证手机验证码");

                Url = "http://hb.189.cn/feesquery_checkCDMAFindWeb.action";
                postdata = String.Format("random={0}&sentType=C", mobileReq.Smscode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Host = "hb.189.cn",
                    Referer = "http://hb.189.cn/pages/selfservice/feesquery/detailListQuery.jsp",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.Html != "1")
                {
                    Res.StatusDescription = "验证码错误！";
                    Res.StatusCode = CrawlerCommonFun.GetMobleCrawlerStatusCode(Res.StatusDescription); ;

                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.StatusCode = Res.StatusCode;
                    logDtl.Description = Res.StatusDescription;
                    appLog.LogDtlList.Add(logDtl);

                    return Res;
                }

                Res.StatusDescription = "湖北电信手机验证码验证成功";
                Res.StatusCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
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
                Res.StatusDescription = "湖北电信手机验证码校验异常";
                Log4netAdapter.WriteError("湖北电信手机验证码校验异常", e);

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

        /// <summary>
        /// 保存抓取的账单
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
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

                #region 个人信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Crawler, mobileReq.Website);

                #region  基本信息
                logDtl = new ApplyLogDtl("基本信息");

                Url = "http://hb.189.cn/pages/selfservice/custinfo/userinfo/userInfo.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "基本信息");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    var mename = Regex.Matches(httpResult.Html, @"(?<=机主姓名\：).+(?=\<)");
                    if (mename.Count > 0)
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

                Url = "http://hb.189.cn/hbuserCenter.action";
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region  手机品牌
                logDtl = new ApplyLogDtl("手机品牌");

                Url = "http://hb.189.cn/pages/selfservice/feesquery/getTaocanDetail.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "手机品牌");
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "packageInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                    logDtl.StatusCode = ServiceConsts.StatusCode_success;
                    logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                    logDtl.Description = "手机品牌查询抓取成功";
                    appLog.LogDtlList.Add(logDtl);
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                #endregion

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                //系统中只能查询两个月的账单
                #region  账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Crawler, mobileReq.Website);

                CrawlerBill(crawler, mobileReq);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 电话账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Call_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.Call, crawler, mobileReq.Smscode);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 短信账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Sms_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.SMS, crawler, mobileReq.Smscode);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 上网账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Net_Crawler, mobileReq.Website);

                CrawlerDeatils(EnumMobileDeatilType.Net, crawler, mobileReq.Smscode);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "湖北电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "湖北电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("湖北电信手机账单抓取异常", e);

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

            try
            {
                crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);

                #region 基本信息
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Infor_Analysis, mobileReq.Website);

                mobile.Token = Res.Token;
                mobile.BusName = mobileReq.Name;
                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.Mobile = mobileReq.Mobile;
                mobile.UpdateTime = crawler.CrawlerDate;

                #region  基本信息获取
                logDtl = new ApplyLogDtl("基本信息");

                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                //姓名
                var mename = Regex.Matches(result, @"(?<=机主姓名\：).+(?=\<)");
                if (mename.Count > 0)
                {
                    mobile.Name = mename[0].ToString();
                }
                //证件类型
                var Idtype = Regex.Matches(result, @"(?<=证件类型\：).+(?=\<)");
                if (Idtype.Count > 0)
                {
                    mobile.Idtype = Idtype[0].ToString();
                }
                //证件号码
                var Idcard = Regex.Matches(result, @"(?<=证件号码\：).+(?=\<)");
                if (Idcard.Count > 0)
                {
                    mobile.Idcard = Idcard[0].ToString();
                }
                //通信地址
                var Address = Regex.Matches(result, @"(?<=通信地址\：).+(?=\<)");
                if (Address.Count > 0)
                {
                    mobile.Address = Address[0].ToString();
                }
                //邮政编码
                var Postcode = Regex.Matches(result, @"(?<=邮政编码\：).+(?=\<)");
                if (Postcode.Count > 0)
                {
                    mobile.Postcode = Postcode[0].ToString();
                }
                //mail
                var Email = Regex.Matches(result, @"(?<=E-mail\：).+(?=\<)");
                if (Email.Count > 0)
                {
                    mobile.Email = Email[0].ToString();
                }
                //创建日期
                var Regdate = Regex.Matches(result, @"(?<=创建日期\：).+(?=\<)");
                if (Regdate.Count > 0)
                {
                    mobile.Regdate = DateTime.Parse(Regdate[0].ToString()).ToString(Consts.DateFormatString11);
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "基本信息";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region   积分查询
                logDtl = new ApplyLogDtl("手机积分");

                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                var Integral = HtmlParser.GetResultFromParser(result, "//div[@class='le3']/div[@class='qt']/div[@class='qt2']", "inner");
                if (Integral.Count > 0)
                {
                    mobile.Integral = Integral[1];
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "手机积分";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region  套餐查询
                logDtl = new ApplyLogDtl("套餐查询");

                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault().CrawlerTxt);
                var Package = HtmlParser.GetResultFromParser(result, "//table/tr[2]/td[1]", "text");
                if (Package.Count > 0)
                {
                    mobile.Package = Package[0].ToString();
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "套餐查询";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

                #region 账单
                appLog = new ApplyLog(ServiceConsts.SpiderType_Mobile, ServiceConsts.Step_Bill_Analysis, mobileReq.Website);

                AnalysisBill(crawler, mobile);

                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                loglist.Add(appLog);
                #endregion

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

                //必要字段校验
                CrawlerCommonFun.CheckColumn(mobile, columnLog);
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "湖北电信手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "湖北电信手机账单解析异常";
                Log4netAdapter.WriteError("湖北电信手机账单解析异常", e);

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
            List<string> month = new List<string>();

            logDtl = new ApplyLogDtl("账单抓取校验");

            Url = "http://hb.189.cn/pages/selfservice/feesquery/zhangdan.jsp";
            httpItem = new HttpItem()
            {
                URL = Url,
                CookieCollection = cookies,
                Method = "get",
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "账单抓取校验zhangdan");
            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
            for (int i = 1; i <= 6; i++)
            {
                if (i == 1 || i == 6)
                {
                    if (DateTime.Now.AddMonths(-i).Month < 10)
                        month.Add(DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString7));
                    else
                        month.Add(DateTime.Now.AddMonths(-i).Year.ToString() + DateTime.Now.AddMonths(-i).Month.ToString());
                }
            }

            Url = "http://hb.189.cn/pages/selfservice/feesquery/querySixFees.action";
            postdata = string.Format("date={0}&dateend={1}", month[0], month[1]);
            httpItem = new HttpItem()
            {
                URL = Url,
                CookieCollection = cookies,
                Method = "post",
                Postdata = postdata,
                Referer = "http://hb.189.cn/pages/selfservice/feesquery/zhangdan.jsp",
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtmlLog(httpItem, appLog, "账单抓取校验querySixFees");
            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
            string monthdata = CommonFun.GetMidStr(httpResult.Html, "\":\"", "#");
            string[] months = monthdata.Split(new char[] { ',' });

            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
            logDtl.StatusCode = ServiceConsts.StatusCode_success;
            logDtl.Description = "账单抓取校验校验成功";
            appLog.LogDtlList.Add(logDtl);

            for (int i = 1; i <= months.Length; i++)
            {
                logDtl = new ApplyLogDtl(DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString7) + "月账单抓取");

                Url = "http://hb.189.cn/pages/selfservice/feesquery/newBOSSQueryCustBill.action";
                postdata = string.Format("billbeanos.citycode=027&billbeanos.btime={0}&billbeanos.accnbr={1}&skipmethod.cityname=%CE%E4%BA%BA&billbeanos.paymode=1", months[i - 1], mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://hb.189.cn/pages/selfservice/feesquery/zhangdan.jsp",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString7) + "月账单抓取");
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "bill0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString7) + "账单抓取成功";
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
            List<string> _results = new List<string>();
            MonthBill bill = null;
            DateTime date;
            for (int i = 1; i <= 6; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis, date.ToString("yyyy-MM-dd") + "月账单解析");
                try
                {
                    if (crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault() == null)
                        continue;
                    PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                    bill = new MonthBill();
                    bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);
                    _results = HtmlParser.GetResultFromParser(PhoneBillStr, "//table[1]/tr[2]/td[1]/font", "");
                    if (_results.Count > 0)
                    {
                        bill.BillCycle = DateTime.Parse(CommonFun.GetMidStr(_results[0], "", "-")).ToString(Consts.DateFormatString12);
                    }
                    _results = HtmlParser.GetResultFromParser(PhoneBillStr, "//font[@class='shuzi']", "");
                    if (_results.Count > 0)
                    {
                        bill.PlanAmt = _results[0].ToTrim("元");
                    }
                    decimal total = 0;
                    if (decimal.TryParse(CommonFun.GetMidStr(PhoneBillStr, "本期费用合计：", "元"), out total))
                    {
                        bill.TotalAmt = total.ToString();
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
        /// <param name="queryType">call 获取通话记录，data 上网详单 note 短信详单</param>
        /// <returns></returns>
        private void CrawlerDeatils(EnumMobileDeatilType type, CrawlerData crawler, string smsCode)
        {
            DateTime date = DateTime.Now;
            string Url = string.Empty;
            string title = string.Empty;
            var month = String.Empty;
            string queryType = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                queryType = "6";
            else if (type == EnumMobileDeatilType.SMS)
                queryType = "7";
            else
                queryType = "5";

            for (var i = 0; i <= 5; i++)
            {
                month = date.AddMonths(-i).ToString(Consts.DateFormatString7);
                title = month + "月详单抓取";

                logDtl = new ApplyLogDtl(title);

                Url = "http://hb.189.cn/feesquery_querylist.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Method = "post",
                    Encoding = Encoding.UTF8,
                    Postdata = String.Format("startMonth={0}&type={1}&random={2}", month + "0000", queryType, smsCode),
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, title + "校验");
                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.Description = title + "校验成功";
                appLog.LogDtlList.Add(logDtl);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                var page = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='page_c']", "inner");
                if (page.Count > 0)
                {
                    var contmes = Regex.Matches(page[0], @"(?<=共).+(?=页&nbsp)");
                    if (contmes.Count > 0)
                    {
                        var count = Regex.Matches(contmes[0].ToString(), @"\d+");
                        if (count.Count > 0)
                        {
                            var lenght = count[0].ToString().ToInt();
                            for (var j = 1; j <= lenght; j++)
                            {
                                logDtl = new ApplyLogDtl(title + "第" + j + "页");
                                Url = "http://hb.189.cn/feesquery_pageQuery.action";
                                httpItem = new HttpItem()
                                {
                                    URL = Url,
                                    CookieCollection = cookies,
                                    Method = "post",
                                    Encoding = Encoding.UTF8,
                                    Postdata = String.Format("page={0}", j),
                                    ResultCookieType = ResultCookieType.CookieCollection
                                };
                                httpResult = httpHelper.GetHtmlLog(httpItem, appLog, title + "第" + j + "页");
                                if (httpResult.StatusCode != HttpStatusCode.OK) continue;
                                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + (i + 1).ToString() + (j + 1).ToString(), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                                logDtl.Description = title + "第" + j + "页详单抓取成功";
                                appLog.LogDtlList.Add(logDtl);
                            }
                        }
                    }
                }

            }
        }

        /// <summary>
        /// 解析手机详单
        /// </summary>
        /// <param name="queryType">call 获取通话记录，data 上网详单 note 短信详单</param>
        /// <returns></returns>
        private void AnalysisDeatils(EnumMobileDeatilType type, CrawlerData crawler, Basic mobile)
        {
            List<CrawlerDtlData> PhoneCrawlerDtls = new List<CrawlerDtlData>();
            DateTime date;
            string PhoneStr = string.Empty;
            for (var i = 0; i <= 5; i++)
            {
                date = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i);
                logDtl = new ApplyLogDtl(date.ToString(Consts.DateFormatString7));
                try
                {
                    PhoneCrawlerDtls = crawler.DtlList.Where(x => x.CrawlerTitle.StartsWith(type + (i + 1).ToString())).OrderBy(x => x.CrawlerTitle).ToList<CrawlerDtlData>();
                    if (PhoneCrawlerDtls != null && PhoneCrawlerDtls.Count > 0)
                    {
                        foreach (CrawlerDtlData item in PhoneCrawlerDtls)
                        {
                            PhoneStr = System.Text.Encoding.Default.GetString(item.CrawlerTxt);
                            var tr = HtmlParser.GetResultFromParser(PhoneStr, "//tr[@class='hovergray']");
                            if (type == EnumMobileDeatilType.SMS)
                            {
                                Sms phoneSMS = new Sms();
                                for (var k = 0; k < tr.Count; k++)
                                {
                                    var StartTime = HtmlParser.GetResultFromParser(tr[k].ToString(), "//td[3]", "inner");
                                    if (StartTime.Count > 0)
                                    {
                                        phoneSMS.StartTime = DateTime.Parse(StartTime[0]).ToString(Consts.DateFormatString11);
                                    }
                                    var SubTotal = HtmlParser.GetResultFromParser(tr[k].ToString(), "//td[5]", "inner");
                                    if (SubTotal.Count > 0)
                                    {
                                        phoneSMS.SubTotal = SubTotal[0].ToDecimal().Value;
                                    }
                                    var OtherSmsPhone = HtmlParser.GetResultFromParser(tr[k].ToString(), "//td[2]", "inner");
                                    if (OtherSmsPhone.Count > 0)
                                    {
                                        phoneSMS.OtherSmsPhone = OtherSmsPhone[0];
                                    }
                                    var SmsType = HtmlParser.GetResultFromParser(tr[k].ToString(), "//td[4]", "inner");
                                    if (SmsType.Count > 0)
                                    {
                                        phoneSMS.SmsType = SmsType[0];
                                    }

                                    mobile.SmsList.Add(phoneSMS);
                                }

                            }
                            if (type == EnumMobileDeatilType.Net)
                            {
                                Call phoneCall = new Call();
                                for (var k = 0; k < tr.Count; k++)
                                {
                                    var StartTime = HtmlParser.GetResultFromParser(tr[k].ToString(), "//td[2]", "inner");
                                    if (StartTime.Count > 0)
                                    {
                                        phoneCall.StartTime = DateTime.Parse(StartTime[0]).ToString(Consts.DateFormatString11);
                                    }
                                    var SubTotal = HtmlParser.GetResultFromParser(tr[k].ToString(), "//td[10]", "inner");
                                    if (SubTotal.Count > 0)
                                    {
                                        phoneCall.SubTotal = SubTotal[0].ToDecimal().Value;
                                    }
                                    var OtherSmsPhone = HtmlParser.GetResultFromParser(tr[k].ToString(), "//td[3]", "inner");
                                    if (OtherSmsPhone.Count > 0)
                                    {
                                        phoneCall.OtherCallPhone = OtherSmsPhone[0];
                                    }

                                    var CallPlace = HtmlParser.GetResultFromParser(tr[k].ToString(), "//td[7]", "inner");
                                    if (CallPlace.Count > 0)
                                    {
                                        phoneCall.CallPlace = CallPlace[0];
                                    }

                                    var CallType = HtmlParser.GetResultFromParser(tr[k].ToString(), "//td[5]", "inner");
                                    if (CallType.Count > 0)
                                    {
                                        phoneCall.InitType = CallType[0];
                                    }
                                    var InitType = HtmlParser.GetResultFromParser(tr[k].ToString(), "//td[9]", "inner");
                                    if (InitType.Count > 0)
                                    {
                                        phoneCall.CallType = InitType[0];
                                    }

                                    var UseTime = HtmlParser.GetResultFromParser(tr[k].ToString(), "//td[4]", "inner");
                                    if (UseTime.Count > 0)
                                    {
                                        phoneCall.UseTime = UseTime[0].ToString();
                                    }
                                    mobile.CallList.Add(phoneCall);
                                }
                            }

                            if (type == EnumMobileDeatilType.Call)
                            {
                                for (var k = 0; k < tr.Count; k++)
                                {
                                    Net phoneGPRS = new Net();
                                    var StartTime = HtmlParser.GetResultFromParser(tr[k].ToString(), "//td[2]", "inner");
                                    if (StartTime.Count > 0)
                                    {
                                        phoneGPRS.StartTime = DateTime.Parse(StartTime[0]).ToString(Consts.DateFormatString11);
                                    }
                                    var UseTime = HtmlParser.GetResultFromParser(tr[k].ToString(), "//td[3]", "inner");
                                    if (UseTime.Count > 0)
                                    {
                                        phoneGPRS.UseTime = UseTime[0].ToString();
                                    }
                                    var Place = HtmlParser.GetResultFromParser(tr[k].ToString(), "//td[6]", "inner");
                                    if (Place.Count > 0)
                                    {
                                        phoneGPRS.Place = Place[0];
                                    }
                                    var SubFlow = HtmlParser.GetResultFromParser(tr[k].ToString(), "//td[4]", "inner");
                                    if (SubFlow.Count > 0)
                                    {
                                        phoneGPRS.SubFlow = (!SubFlow[0].IsEmpty() ? CommonFun.ConvertGPRS(SubFlow[0]) : 0).ToString();
                                    }
                                    var PhoneNetType = HtmlParser.GetResultFromParser(tr[k].ToString(), "//td[5]", "inner");
                                    if (PhoneNetType.Count > 0)
                                    {
                                        phoneGPRS.PhoneNetType = PhoneNetType[0];
                                    }
                                    var SubTotal = HtmlParser.GetResultFromParser(tr[k].ToString(), "//td[8]", "inner");
                                    if (SubTotal.Count > 0)
                                    {
                                        phoneGPRS.SubTotal = SubTotal[0].ToDecimal().Value;
                                    }
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

        #endregion
    }
}
