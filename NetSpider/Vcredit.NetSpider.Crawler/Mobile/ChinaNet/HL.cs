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
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net.NetworkInformation;
using System.Web.UI.WebControls;
using System.Threading;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{
    public class HL : ChinaNet
    {

        public override VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            VerCodeRes Res = new VerCodeRes();
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

                Url = "http://hl.189.cn/service/dynamicValidate.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    Method = "POST",
                    Postdata = string.Format("dynnum=&imgnum={0}&requesttype=send&oper=/cqd/detailQueryCondition.do", mobileReq.Vercode),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                var result = JsonConvert.DeserializeObject(httpResult.Html);
                {
                    JObject js = result as JObject;
                    if (js != null)
                    {
                        var msg = js["msg"].ToString();
                        if (msg == "图片验证码输入错误!")
                        {
                            Res.StatusDescription = "图片验证码输入错误!";
                            Res.StatusCode = ServiceConsts.StatusCode_error;
                            return Res;
                        }
                    }
                }

                //{"go":"false","msg":"图片验证码输入错误!","url":""}
                //{"go":"false","msg":"已发送动态验证码到手机,请查收!","url":""}

                Res.StatusDescription = "已发送动态验证码到手机,请查收!";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;

                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "黑龙江电信手机验证码发送异常";
                Log4netAdapter.WriteError("黑龙江电信手机验证码发送异常", e);
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
                Url = "http://hl.189.cn/service/dynamicValidate.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    Method = "POST",
                    Postdata = string.Format("dynnum=&imgnum={0}&requesttype=end&oper=/cqd/detailQueryCondition.do", mobileReq.Vercode),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                var result = JsonConvert.DeserializeObject(httpResult.Html);
                {
                    JObject js = result as JObject;
                    if (js != null)
                    {
                        var msg = js["msg"].ToString();
                        if (msg == "动态验证码输入错误!")
                        {
                            Res.StatusDescription = "动态验证码输入错误!";
                            Res.StatusCode = ServiceConsts.StatusCode_error;
                            return Res;
                        }
                    }
                }

                Res.StatusDescription = "动态验证码验证成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Query;
                CacheHelper.RemoveCache(Res.Token + "verCode");

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "黑龙江电信手机验证码验证异常";
                Log4netAdapter.WriteError("黑龙江电信手机验证码验证异常", e);
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

        public override BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            ApplyLogDtl logDtl = new ApplyLogDtl("");
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            CrawlerData crawler = new CrawlerData() { Token = mobileReq.Token, IdentityCard = mobileReq.IdentityCard, Name = mobileReq.Name, Mobile = mobileReq.Mobile, UserType = "1" };
            string Url = string.Empty;
            string postdata = string.Empty;
            DateTime date = DateTime.Now;
            List<JObject> results = new List<JObject>();
            List<string> infos = new List<string>();
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                #region 用户基本信息

                Url = "http://www.189.cn/dqmh/userCenter/userInfo.do?method=editUserInfo_new&fastcode=";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                ///积分
                httpItem = new HttpItem()
                {
                    URL = "http://hl.189.cn/service/pointQuery.do?method=init&opFlag=init",
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "integralInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                /// 套餐
                httpItem = new HttpItem()
                {
                    URL = "http://hl.189.cn/service/show_crm_device_init.do?frameFlag=compages&canAdd2Tool=canAdd2Tool",
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "packageInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                //姓名
                httpItem = new HttpItem()
                {
                    URL = "http://hl.189.cn/service/crm_cust_info_show.do?funcName=custSupport&canAdd2Tool=canAdd2Tool",
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "nameInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });


                httpItem = new HttpItem()
                {
                    URL = "http://hl.189.cn/service/billDateChoiceNew.do?method=doSearch&selectDate=201509",
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://hl.189.cn/service/cqd/detailQueryCondition.do?menuid=4&subNo=";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                var accountNum = String.Empty;
                var account = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='accountNum']", "value");
                if (account.Count > 0)
                    accountNum = account[0].ToUrlEncode();


                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "基本信息抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion


                #region 账单
                CrawlerBill(crawler, mobileReq);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 通话
                GetcCallDetail(crawler, mobileReq, accountNum);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单抓取成功";
                appLog.LogDtlList.Add(logDtl);

                #endregion

                #region 短信
                GetSmsDetail(crawler, mobileReq, accountNum);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 上网
                GetNetDetail(crawler, mobileReq, accountNum);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "黑龙江电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusDescription = "黑龙江电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("黑龙江电信手机账单抓取异常", e);
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
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "baseInfor").FirstOrDefault().CrawlerTxt);
                var realName = HtmlParser.GetResultFromParser(result, "//input[@name='realName']", "value");
                if (realName.Count > 0)
                    mobile.Name = realName[0];

                var certificateNumber = HtmlParser.GetResultFromParser(result, "//input[@name='certificateNumber']", "value");
                if (certificateNumber.Count > 0)
                    mobile.Idcard = certificateNumber[0];


                var Idcardtype = HtmlParser.GetResultFromParser(result, "//select[@name='certificateType']/option[@selected='selected']", "text");
                if (Idcardtype.Count > 0)
                    mobile.Idtype = Idcardtype[0];

                var address = String.Empty;


                var Address = HtmlParser.GetResultFromParser(result, "//textarea[@id='address']");
                if (Address.Count > 0)
                    address += Address[0];
                mobile.Address = address;

                //积分
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                var integral = HtmlParser.GetResultFromParser(result, "//table/tr[2]/td[2]", "text");
                if (integral.Count > 0)
                {
                    mobile.Integral = integral[0].ToString();
                }

                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault().CrawlerTxt);
                var Package = HtmlParser.GetResultFromParser(result, "//table/tr[2]/td[2]");
                var PackageBrand = HtmlParser.GetResultFromParser(result, "//table/tr[2]/td[3]");
                if (Package.Count > 0)
                {
                    mobile.Package = Package[0].ToString();
                }
                if (PackageBrand.Count > 0)
                {
                    mobile.PackageBrand = PackageBrand[0].ToString();
                }
                //姓名
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "nameInfor").FirstOrDefault().CrawlerTxt);
                var name = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr[2]/td[2]");
                if (name.Count > 0)
                {
                    mobile.Name = name[0].ToString().Replace("&nbsp;", "");
                }
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "个人信息解析成功";
                appLog.LogDtlList.Add(logDtl);

                #endregion

                #region 账单
                AnalysisBill(crawler, mobile);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 通话
                AnalysisCall(crawler, mobile);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion

                #region 上网
                AnalysisNet(crawler, mobile);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "网络详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 短信
                AnalysisSMS(crawler, mobile);
                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单解析成功";
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
                Res.StatusDescription = "黑龙江电信手机账单解析异常";
                Log4netAdapter.WriteError("黑龙江电信手机账单解析异常", e);
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
            for (var i = 1; i <= 6; i++)
            {
                month = DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString7);
                Url = "http://hl.189.cn/service/billDateChoiceNew.do?method=doSearch&selectDate=" + month;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
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

            for (var i = 1; i <= 6; i++)
            {
                var date = DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString8);
                var billhtml = crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault();
                if (billhtml == null) continue;
                PhoneBillStr = System.Text.Encoding.Default.GetString(billhtml.CrawlerTxt);
                bill = new MonthBill();

                var billtable = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr").Where(e => e.Contains("基本月租费")).ToList();
                if (billtable.Count() > 0)
                {
                    var PlanAmt = 0m;
                    for (var j = 0; j < billtable.Count(); j++)
                    {
                        var tr = HtmlParser.GetResultFromParser(billtable[j], "//tr");

                        var td = tr.Where(e => e.Contains("基本月租费")).FirstOrDefault();
                        if (td != null)
                        {
                            var aonmt = HtmlParser.GetResultFromParser(td, "//td[2]");
                            if (aonmt.Count > 0)
                            {
                                var RePlanAmt = Regex.Matches(aonmt[0].ToString(), @"\d+(.)?.*\d");
                                if (RePlanAmt.Count > 0)
                                {
                                    PlanAmt += RePlanAmt[0].ToString().ToDecimal().Value;
                                }
                            }
                        }
                    }

                    bill.PlanAmt = PlanAmt.ToString();

                }
                var TotalAmt = Regex.Matches(httpResult.Html, @"(?<=本期费用合计\：).*\d");
                if (TotalAmt.Count > 0)
                {
                    bill.TotalAmt = TotalAmt[0].ToString();
                }
                bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);
                mobile.BillList.Add(bill);

            }
        }

        /// <summary>
        /// 通话
        /// </summary>
        /// <param name="crawler"></param>
        /// <param name="mobileReq"></param>
        public void GetcCallDetail(CrawlerData crawler, MobileReq mobileReq, string accountNum)
        {
            string Url = string.Empty;
            var month = String.Empty;
            string postdata = String.Empty;


            for (var i = 1; i <= 6; i++)
            {

                month = DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString7);
                //  <li class="" value="1">长途详单</li>
                //<li class="" value="0">漫游详单</li>
                //<li class="" value="2">市话详单</li>
                //<li class="" value="3">信息台详单</li>
                //<li value="4" class="">上网详单</li>
                //<li value="5" class="">短信详单</li>
                //<li value="7">预付费充值详单</li>
                //<li value="8">预付费扣费详单</li>
                //<li value="9">增值/彩信详单</li>
                Url = "http://web.vcredit.com/HL.html";
                //第一步，初始化登录页面

                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);


                Url = String.Format("http://hl.189.cn/service/cqd/queryDetailList.do?isMobile=0&seledType=0&queryType=&pageSize=10&pageNo=1&flag=&pflag=&accountNum={1}&callType=3&selectType=1&detailType=2&selectedDate={0}&method=queryCQDMain", month, accountNum);

                List<string> cookieslist = new List<string>();
                foreach (var item in cookies)
                {
                    cookieslist.Add(item.ToString());
                }


                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    Referer = "http://hl.189.cn/service/cqd/detailQueryCondition.do?menuid=4&subNo=",
                    ContentType = "multipart/x-mixed-replace;boundary=ThisRandomString",
                    Expect100Continue = false,
                    Timeout = 60000,
                    KeepAlive = false,
                    Allowautoredirect = false,
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0",

                    //Encoding = Encoding.GetEncoding("gb2312"),
                    ProtocolVersion = HttpVersion.Version10,
                    //ContentLength = 0
                };
                httpItem.Header.Add("Expires", "Thu, 01 Jan 1970 00:00:00 GMT");
                // httpItem.Header.Add("Date", "Mon, 09 Nov 2015 06:31:55 GMT");
                httpItem.Header.Add("Cache-Control", "no-cache");
                httpItem.Header.Add("Keep-Alive", "timeout=100, max=500");
                httpItem.Header.Add("Pragma", "no-cache");
                httpItem.Header.Add("Accept-Encoding", "gzip, deflate");

                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "Call_0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                Log4netAdapter.WriteInfo("===>>>>通话:" + httpResult.Html);

                Url = "http://hl.189.cn/service/cqd/queryDetailList.do?isMobile=0&seledType=1&queryType=&pageSize=10&pageNo=1&flag=&pflag=&accountNum={1}&callType=3&selectType=1&detailType=2&selectedDate={0}&method=queryCQDMain";
                httpItem = new HttpItem()
                {
                    URL = String.Format(Url, month, accountNum),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "Call_1" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                Url = "http://hl.189.cn/service/cqd/queryDetailList.do?isMobile=0&seledType=2&queryType=&pageSize=10&pageNo=1&flag=&pflag=&accountNum={1}&callType=3&selectType=1&detailType=2&selectedDate={0}&method=queryCQDMain";
                httpItem = new HttpItem()
                {
                    URL = String.Format(Url, month, accountNum),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "Call_2" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

            }
        }

        #endregion



        /// <summary>
        /// 上网
        /// </summary>
        /// <param name="crawler"></param>
        /// <param name="mobileReq"></param>
        public void GetNetDetail(CrawlerData crawler, MobileReq mobileReq, string accountNum)
        {
            string Url = string.Empty;
            var month = String.Empty;
            string postdata = String.Empty;
            for (var i = 1; i <= 6; i++)
            {
                month = DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString7);

                Url = "http://hl.189.cn/service/cqd/queryFlowDetailList.do?isMobile=0&seledType=4&queryType=&pageSize=10&pageNo=1&flag=&pflag=&accountNum={1}&callType=3&selectType=1&detailType=4&selectedDate={0}&method=queryCQDMain";
                httpItem = new HttpItem()
                {
                    URL = String.Format(Url, month, accountNum),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "Net0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
            }
        }

        /// <summary>
        /// 短信
        /// </summary>
        /// <param name="crawler"></param>
        /// <param name="mobileReq"></param>
        public void GetSmsDetail(CrawlerData crawler, MobileReq mobileReq, string accountNum)
        {

            string Url = string.Empty;
            var month = String.Empty;
            string postdata = String.Empty;
            for (var i = 1; i <= 6; i++)
            {

                Url = "http://hl.189.cn/service/cqd/queryDetailList.do?isMobile=0&seledType=5&queryType=&pageSize=10&pageNo=1&flag=&pflag=&accountNum={1}&callType=3&selectType=1&detailType=2&selectedDate={0}&method=queryCQDMain";
                httpItem = new HttpItem()
                {
                    URL = String.Format(Url, month, accountNum),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "SMS0" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                Url = "http://hl.189.cn/service/cqd/queryDetailList.do?isMobile=0&seledType=5&queryType=&pageSize=10&pageNo=1&flag=&pflag=&accountNum={1}&callType=3&selectType=1&detailType=2&selectedDate={0}&method=queryCQDMain";
                httpItem = new HttpItem()
                {
                    URL = String.Format(Url, month, accountNum),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "SMS9" + (i + 1), CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

            }
        }

        /// <summary>
        /// 解析通话清单
        /// </summary>
        /// <param name="crawler"></param>
        /// <param name=ServiceConsts.SpiderType_Mobile></param>
        public void AnalysisCall(CrawlerData crawler, Basic mobile)
        {
            string PhoneBillStr = string.Empty;
            Call call = null;


            var billhtml = crawler.DtlList.Where(x => x.CrawlerTitle.Contains("Call")).ToList();
            if (billhtml == null || billhtml.Count() == 0) return;
            billhtml.ForEach(e =>
                {
                    PhoneBillStr = System.Text.Encoding.Default.GetString(e.CrawlerTxt);
                    var tr = HtmlParser.GetResultFromParser(PhoneBillStr, "//table[@class='sort-table']/tr");
                    for (var j = 0; j < tr.Count; j++)
                    {
                        call = new Call();
                        var OtherCallPhone = HtmlParser.GetResultFromParser(tr[j], "//td[4]");
                        if (OtherCallPhone.Count > 0)
                            call.OtherCallPhone = OtherCallPhone[0];
                        var StartTime = HtmlParser.GetResultFromParser(tr[j], "//td[5]");
                        if (StartTime.Count > 0)
                            call.StartTime = DateTime.Parse(StartTime[0]).ToString(Consts.DateFormatString11);
                        var UseTime = HtmlParser.GetResultFromParser(tr[j], "//td[6]");
                        if (UseTime.Count > 0)
                            call.UseTime = UseTime[0];
                        var SubTotal = HtmlParser.GetResultFromParser(tr[j], "//td[7]");
                        if (SubTotal.Count > 0)
                            call.SubTotal = SubTotal[0].ToDecimal().Value;
                        var CallType = HtmlParser.GetResultFromParser(tr[j], "//td[8]");
                        if (SubTotal.Count > 0)
                            call.CallType = CallType[0];
                        mobile.CallList.Add(call);
                    }
                });
        }

        public void AnalysisNet(CrawlerData crawler, Basic mobile)
        {
            string PhoneBillStr = string.Empty;
            Net net = null;


            var billhtml = crawler.DtlList.Where(x => x.CrawlerTitle.Contains("Net")).ToList();
            if (billhtml == null || billhtml.Count() == 0) return;
            billhtml.ForEach(e =>
                {
                    PhoneBillStr = System.Text.Encoding.Default.GetString(e.CrawlerTxt);
                    var tr = HtmlParser.GetResultFromParser(PhoneBillStr, "//table/tr");
                    for (var j = 5; j < tr.Count - 1; j++)
                    {
                        net = new Net();
                        var StartTime = HtmlParser.GetResultFromParser(tr[j], "//td[1]");
                        if (StartTime.Count > 0)
                            net.StartTime = DateTime.Parse(StartTime[0]).ToString(Consts.DateFormatString11);
                        var Place = HtmlParser.GetResultFromParser(tr[j], "//td[6]");
                        if (Place.Count > 0)
                            net.Place = Place[0];
                        var NetType = HtmlParser.GetResultFromParser(tr[j], "//td[5]");
                        if (NetType.Count > 0)
                            net.NetType = NetType[0];
                        var UseTime = HtmlParser.GetResultFromParser(tr[j], "//td[3]");
                        if (UseTime.Count > 0)
                            net.UseTime = UseTime[0];
                        var SubTotal = HtmlParser.GetResultFromParser(tr[j], "//td[8]");
                        if (SubTotal.Count > 0)
                            net.SubTotal = SubTotal[0].ToDecimal().Value;
                        mobile.NetList.Add(net);
                    }
                });

        }
        public void AnalysisSMS(CrawlerData crawler, Basic mobile)
        {

            string PhoneBillStr = string.Empty;
            Sms sms = null;
            var billhtml = crawler.DtlList.Where(x => x.CrawlerTitle.Contains("SMS")).ToList();
            if (billhtml == null || billhtml.Count() == 0) return;
            billhtml.ForEach(e =>
            {
                PhoneBillStr = System.Text.Encoding.Default.GetString(e.CrawlerTxt);
                var tr = HtmlParser.GetResultFromParser(PhoneBillStr, "//table[@class='sort-table']/tr");
                for (var j = 0; j < tr.Count; j++)
                {
                    sms = new Sms();
                    var OtherCallPhone = HtmlParser.GetResultFromParser(tr[j], "//td[3]");
                    if (OtherCallPhone.Count > 0)
                        sms.OtherSmsPhone = OtherCallPhone[0];
                    var StartTime = HtmlParser.GetResultFromParser(tr[j], "//td[4]");
                    if (StartTime.Count > 0)
                        sms.StartTime = DateTime.Parse(StartTime[0]).ToString(Consts.DateFormatString11);
                    var SubTotal = HtmlParser.GetResultFromParser(tr[j], "//td[6]");
                    if (SubTotal.Count > 0)
                        sms.SubTotal = SubTotal[0].ToDecimal().Value;
                    var CallType = HtmlParser.GetResultFromParser(tr[j], "//td[7]");
                    if (SubTotal.Count > 0)
                        sms.SmsType = CallType[0];
                    mobile.SmsList.Add(sms);
                }
            });
        }

        /// <summary>
        /// Requests the specified URL.
        /// </summary>
        /// <param name=”url”>The URL.</param>
        /// <param name=”cookies”>The cookies.</param>
        /// <returns></returns>
        public string Request(string url, List<string> cookies)
        {
            //var lCookieValues = cookies.Select(cookie => cookie.(cookie.IndexOf(";", StringComparison.Ordinal))).ToArray();
            var strCookieValues = String.Join(";", cookies);
            var address = new Uri(url);
            var strHttpRequest = "GET" + address.PathAndQuery + "HTTP/1.1\r\n";
            strHttpRequest += "Accept: application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, */*\r\n";
            strHttpRequest += "Accept-Language: en-US\r\n";
            strHttpRequest += "User-Agent: Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E)\r\n";
            strHttpRequest += "Cookie:" + strCookieValues + "\r\n";
            strHttpRequest += "Connection: Keep-Alive\r\n";
            strHttpRequest += "Host:" + address.Host + "\r\n\r\n";

            return SendWebRequest(address.Host, strHttpRequest, address.Scheme == Uri.UriSchemeHttps);
        }


        /// <summary>
        /// Requests the specified URL.
        /// </summary>
        /// <param name=”url”>The URL.</param>
        /// <param name=”postdata”>The post data.</param>
        /// <param name=”cookies”>The cookies.</param>
        /// <returns></returns>
        public string Request(string url, string postdata, List<string> cookies)
        {
            //var lCookieValues = cookies.Select(cookie => cookie.Left(cookie.IndexOf(";", StringComparison.Ordinal))).ToArray();
            var strCookieValues = String.Join(";", cookies);
            var address = new Uri(url);
            var strHttpRequest = "POST" + address.PathAndQuery + "HTTP/1.1\r\n";
            strHttpRequest += "Accept: application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, */*\r\n";
            strHttpRequest += "Accept-Language: en-US\r\n";
            strHttpRequest += "User-Agent: Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E)\r\n";
            strHttpRequest += "Cookie:" + strCookieValues + "\r\n";
            strHttpRequest += "Connection: Keep-Alive\r\n";
            strHttpRequest += "Content-Type: application/x-www-form-urlencoded\r\n";
            strHttpRequest += "Content-Length:" + postdata.Length + "\r\n";
            strHttpRequest += "Host:" + address.Host + "\r\n\r\n";
            strHttpRequest += postdata + "\r\n";
            return SendWebRequest(address.Host, strHttpRequest, address.Scheme == Uri.UriSchemeHttps);
        }
        // The following method is invoked by the RemoteCertificateValidationDelegate. 
        private static bool ValidateServerCertificate(
        object sender,
        X509Certificate certificate,
        X509Chain chain,
        SslPolicyErrors sslPolicyErrors)
        {
            return true; // Accept all certs
        }
        private static string SendWebRequest(string url, string request, bool isHttps)
        {
            using (var tc = new TcpClient())
            {
                tc.Connect(url, isHttps ? 443 : 80);
                using (var ns = tc.GetStream())
                {
                    if (isHttps)
                    {
                        // Secure HTTP
                        using (var ssl = new SslStream(ns, false, ValidateServerCertificate, null))
                        {
                            ssl.AuthenticateAsClient(url, null, SslProtocols.Tls, false);
                            using (var sw = new System.IO.StreamWriter(ssl))
                            {
                                using (var sr = new System.IO.StreamReader(ssl))
                                {
                                    sw.Write(request);
                                    sw.Flush();
                                    return sr.ReadToEnd();
                                }
                            }
                        }
                    }
                    // Normal HTTP
                    using (var sw = new System.IO.StreamWriter(ns))
                    {
                        using (var sr = new System.IO.StreamReader(ns))
                        {
                            sw.Write(request);
                            sw.Flush();
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

}
