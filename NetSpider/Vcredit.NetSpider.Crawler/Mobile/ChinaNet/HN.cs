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
    public class HN : ChinaNet
    {

        public override VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            VerCodeRes Res = new VerCodeRes();
            Res.Token = mobileReq.Token;
            cookies = new CookieCollection();
            string Url = string.Empty;
            string postdata = string.Empty;
            #region 私有成员变量
            string userLoginDate = string.Empty;
            string lastLoginDate = string.Empty;
            string areaCode = string.Empty;
            string areaId = string.Empty;
            string superId = string.Empty;
            string userName = string.Empty;
            string userAddress = string.Empty;
            string userLoginType = string.Empty;
            string productId = string.Empty;
            string productAreaCode = string.Empty;
            string productCollection = string.Empty;
            string prodString = string.Empty;
            string points = string.Empty;
            string userDetailInfo = string.Empty;
            string userAccountInfo = string.Empty;
            string userPackageInfo = string.Empty;
            string pwdType = string.Empty;
            string partyId = string.Empty;
            string registerId = string.Empty;
            string indentCode = string.Empty;
            string indentNbrType = string.Empty;
            string zipCode = string.Empty;
            string contactPerson = string.Empty;
            string email = string.Empty;
            string contactPhone = string.Empty;
            string registerUserKeyID = string.Empty;
            string telProdId = string.Empty;
            string UATicket = string.Empty;
            string userType = string.Empty;
            string indentInfos = string.Empty;
            string grade = string.Empty;
            string vipIdentifierId = string.Empty;
            string loginTime = string.Empty;
            string userIP = string.Empty;
            string currentNumber = string.Empty;
            string managerFlag = string.Empty;
            string rUrl = string.Empty;

            #endregion
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                Url = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=10000280";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postdata,
                    CookieCollection = cookies,

                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/dqmh/login.do?method=loginright";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postdata,
                    CookieCollection = cookies,

                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //获取查看状态
                Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
                postdata = string.Format("fastcode=10000280");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,

                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                //获取查询参数
                Url = "http://hn.189.cn/grouplogin?rUrl=http://hn.189.cn/webportal-wt/hnselfservice/billquery/bill-query!queryBill.action&fastcode=10000280";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);



                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10019&toStUrl=http://hn.189.cn/webportal-wt/hnselfservice/billquery/bill-query!queryBill.action?_z=1&fastcode=10000280";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                var results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@type='hidden']", "value");
                if (results.Count > 0)
                {
                    userLoginDate = results[0];
                    lastLoginDate = results[1];
                    areaCode = results[2];
                    areaId = results[3];
                    superId = results[4];
                    userName = results[5];
                    userAddress = results[6];
                    userLoginType = results[7];
                    productId = results[8];
                    productAreaCode = results[9];
                    productCollection = results[10];
                    prodString = results[11];
                    points = results[12];
                    userDetailInfo = results[13];
                    userAccountInfo = results[14];
                    userPackageInfo = results[15];
                    pwdType = results[16];
                    partyId = results[17];
                    registerId = results[18];
                    indentCode = results[19];
                    indentNbrType = results[20];
                    zipCode = results[21];
                    contactPerson = results[22];
                    email = results[23];
                    contactPhone = results[24];
                    registerUserKeyID = results[25];
                    telProdId = results[26];
                    UATicket = results[27];
                    userType = results[28];
                    indentInfos = results[29];
                    grade = results[30];
                    vipIdentifierId = results[31];
                    loginTime = results[32];
                    userIP = results[33];
                    currentNumber = results[34];
                    managerFlag = results[35];
                    rUrl = results[36];
                }

                Url = "http://www.189.cn/dqmh/isUpFour.do?method=queryDataplanByPhoneNumber";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://hn.189.cn/webportal-wt/hnselfservice/billquery/bill-query!queryBill.action?_z=1&fastcode=10000280";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://hn.189.cn/webportal-wt/login!getUserInfoToSession.do";
                postdata = string.Format(@"userInfo.userLoginDate={0}&userInfo.lastLoginDate={1}&userInfo.areaCode={2}&userInfo.areaId={3}&userInfo
                .superId={4}&userInfo.userName={5}&userInfo.userAddress={6}&userInfo.userLoginType={7}
                &userInfo.productId={8}&userInfo.productAreaCode={9}&userInfo.productCollection={10}&userInfo.prodString
                ={11}&userInfo.points={12}&userInfo.userDetailInfo={13}&userInfo.userAccountInfo
                ={14}&userInfo.userPackageInfo={15}&userInfo.pwdType={16}&userInfo.partyId={17}&userInfo.registerId={18}&userInfo
                .indentCode={19}&userInfo.indentNbrType={20}&userInfo.zipCode={21}&userInfo.contactPerson={22}&userInfo.email={23}&userInfo
                .contactPhone={24}&userInfo.registerUserKeyID={25}&userInfo.telProdId={26}&userInfo.UATicket={27}&userInfo.userType={28}
                &userInfo.indentInfos={29}&userInfo.grade={30}&userInfo.vipIdentifierId={31}&userInfo.loginTime={32}&userInfo.userIP
                ={33}&userInfo.currentNumber={34}&userInfo.managerFlag={35}&rUrl={36}", userLoginDate.ToUrlEncode(), lastLoginDate.ToUrlEncode(), areaCode.ToUrlEncode(), areaId.ToUrlEncode(), superId.ToUrlEncode(), userName.ToUrlEncode(),
 userAddress.ToUrlEncode(), userLoginType.ToUrlEncode(), productId.ToUrlEncode(), productAreaCode.ToUrlEncode(), productCollection.ToUrlEncode(), prodString.ToUrlEncode(),
 points.ToUrlEncode(), userDetailInfo.ToUrlEncode(), userAccountInfo.ToUrlEncode(), userPackageInfo.ToUrlEncode(), pwdType.ToUrlEncode(), partyId.ToUrlEncode(), registerId.ToUrlEncode(),
 indentCode.ToUrlEncode(), indentNbrType.ToUrlEncode(), zipCode.ToUrlEncode(), contactPerson.ToUrlEncode(), email.ToUrlEncode(), contactPhone.ToUrlEncode(),
 registerUserKeyID.ToUrlEncode(), telProdId.ToUrlEncode(), UATicket.ToUrlEncode(), userType.ToUrlEncode(), indentInfos.ToUrlEncode(), grade.ToUrlEncode(),
  vipIdentifierId.ToUrlEncode(), loginTime.ToUrlEncode(), userIP.ToUrlEncode(), currentNumber.ToUrlEncode(), managerFlag.ToUrlEncode(), rUrl.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,

                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                if (httpResult.Html == "本次请求并未返回任何数据")
                {
                    Res.StatusDescription = "本次请求并未返回任何数据";
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
                Res.StatusDescription = "湖南电信手机验证码发送异常";
                Log4netAdapter.WriteError("湖南电信手机验证码发送异常", e);
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
            cookies = new CookieCollection();
            string Url = string.Empty;
            string postdata = string.Empty;
            DateTime date = DateTime.Now;

            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10019&toStUrl=http://hn.189.cn/webportal-wt/hnselfservice/billquery/bill-query!queryBill.action?_z=1&fastcode=10000280";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = string.Format("http://hn.189.cn/webportal-wt/hnselfservice/billquery/bill-query!queryBill.action?tabIndex=2&queryMonth={1}&patitype=2&valicode={0}&accNbr=", mobileReq.Smscode, date.ToString("yyyy-MM"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Get",
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

                if (httpResult.Html.Contains("请输入正确的随机码"))
                {
                    Res.StatusDescription = "请输入正确的随机码";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.StatusDescription = "湖南电信手机验证码验证成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Query;

                CacheHelper.SetCache(mobileReq.Token, cookies);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "湖南电信手机验证码验证异常";
                Log4netAdapter.WriteError("湖南电信手机验证码验证异常", e);
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
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                #region 基本信息
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Crawler);

                #region 用户基本信息

                Url = "http://hn.189.cn/grouplogin?rUrl=http://hn.189.cn/webportal-wt/hnselfservice/businessquery/business-query!userBusinessList.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=10000284",
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
                Url = "http://hn.189.cn/webportal-wt/hnselfservice/businessquery/business-query!userBusinessList.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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

                Url = "http://hn.189.cn/hnselfservice/customerinfomanager/customer-info!queryCustInfo.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "baseInfor", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                #endregion

                #region 套餐
                Url = "http://hn.189.cn/grouplogin?rUrl=http://hn.189.cn/webportal-wt/hnselfservice/businessquery/business-query!userBusinessList.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=10000284",
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
                Url = "http://hn.189.cn/webportal-wt/hnselfservice/businessquery/business-query!userBusinessList.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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

                Url = "http://hn.189.cn/webportal-wt/hnselfservice/billquery/bill-query!queryPackageUse.action";

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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

                #region 积分
                Url = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=10000290";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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

                #region 详单查询

                #region 话费详单
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

                #region 上网详单
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Crawler);

                //CrawlerDeatils(EnumMobileDeatilType.Net, mobileReq, crawler);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单抓取成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion


                #endregion

                //保存
                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "湖南电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusDescription = "湖南电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("湖南电信手机账单抓取异常", e);

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
                results = HtmlParser.GetResultFromParser(result, "//table[@class='sersel_table sersel_table_right'][2]/tr[1]/td[1]", "");
                if (results.Count != 0)
                {
                    mobile.Name = results[0].Split('：')[1];//姓名
                }

                results = HtmlParser.GetResultFromParser(result, "//table[@class='sersel_table sersel_table_right'][2]/tr[2]/td[2]", "");
                if (results.Count != 0)
                {
                    mobile.Idtype = results[0].Split('：')[1];//证件类型
                }

                results = HtmlParser.GetResultFromParser(result, "//table[@class='sersel_table sersel_table_right'][2]/tr[3]/td[1]", "");
                if (results.Count != 0)
                {
                    mobile.Idcard = results[0].Split('：')[1].Replace("\r", "").Replace("\t", "").Replace("\n", "").Replace(" ", "");//证件号
                }

                results = HtmlParser.GetResultFromParser(result, "//table[@class='sersel_table sersel_table_right'][2]/tr[3]/td[2]", "");
                if (results.Count != 0)
                {
                    mobile.Address = results[0].Split('：')[1].Replace("\r", "").Replace("\t", "").Replace("\n", "").Replace(" ", "");//地址
                }

                results = HtmlParser.GetResultFromParser(result, "//table[@class='sersel_table sersel_table_right'][2]/tr[7]/td[1]", "");
                if (results.Count != 0)
                {
                    mobile.PUK = results[0].Split(':')[1];//PUK码
                }
                mobile.Mobile = mobileReq.Mobile;
                #endregion

                #region 套餐
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "packageInfor").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//table[@class='taoc_table']/tr[2]/td[2]", "");
                if (results.Count > 0)
                {
                    mobile.Package = results[0];
                }
                #endregion

                #region 积分
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "integralInfor").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//table[@class='usrr_wallet left']/tbody/tr[1]/td[1]", "");
                if (results.Count > 0)
                {
                    mobile.Integral = results[0];
                }
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

                AnalysisDeatils(EnumMobileDeatilType.SMS, crawler, mobile);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单解析成功";
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

                #endregion

                //保存
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "湖南电信手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "湖南电信手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("湖南电信手机账单解析异常", e);

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

            Url = "http://hn.189.cn/webportal-wt/hnselfservice/billquery/bill-query!queryBillDetail.action";
            httpItem = new HttpItem()
            {
                URL = Url,
                Method = "get",
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

            for (int i = 0; i <= 5; i++)
            {
                DateTime date = DateTime.Now;
                Url = string.Format("http://hn.189.cn/webportal-wt/hnselfservice/billquery/bill-query!queryUserBillDetail.action?chargeType=&queryMonth={0}&productId={1}", date.AddMonths(-i).ToString("yyyy-MM"), mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
            List<string> infos = new List<string>();
            List<string> results = new List<string>();
            MonthBill bill = null;
            for (var i = 0; i <= 5; i++)
            {
                bill = new MonthBill();
                PhoneBillStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == ("bill0" + (i + 1))).FirstOrDefault().CrawlerTxt);
                bill = new MonthBill();
                int cout = 0;
                results = HtmlParser.GetResultFromParser(PhoneBillStr, "//table[@id='Table4'][1]/tr/td[@valign='top']", "");
                var s = HtmlParser.GetResultFromParser(PhoneBillStr, "//table[@id='Table4'][1]/tr/td[@valign='top']", "");
                cout = HtmlParser.GetResultFromParser(s[1], "//table/tr", "").Count;
                if (results.Count > 0)
                {
                    bill.BillCycle = DateTime.Parse(crawler.CrawlerDate).AddMonths(-i).ToString(Consts.DateFormatString12);
                    bill.PlanAmt = HtmlParser.GetResultFromParser(results[1], "//tr[2]/td[2]", "")[0].Replace("\r\n", "").Replace(" ", "");
                    bill.TotalAmt = HtmlParser.GetResultFromParser(results[1], "//tr[" + (cout) + "]/td[2]", "")[0].Replace("\r\n", "").Replace(" ", "");
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

            var startDate = String.Empty;
            var endDate = String.Empty;
            var postdata = string.Empty;
            string queryType = string.Empty;

            if (type == EnumMobileDeatilType.Call)
                queryType = "2";
            else if (type == EnumMobileDeatilType.SMS)
                queryType = "12";
            else
                queryType = "9";

            for (int i = 0; i <= 5; i++)
            {
                DateTime date = DateTime.Now;
                date = date.AddMonths(-i);

                Url = string.Format("http://hn.189.cn/webportal-wt/hnselfservice/billquery/bill-query!queryBill.action?tabIndex=2&queryMonth={0}&patitype={1}&valicode=", date.ToString("yyyy-MM"), queryType); ;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                int pageCount = 0;
                var result = HtmlParser.GetResultFromParser(httpResult.Html, "//li[@style='float:right;']", "inner");
                if (result.Count != 0)
                {
                    pageCount = Regex.Replace(result[0].Split('：')[2], @"[^\d.\d]", "").ToInt(0);
                }
                for (int j = 1; j <= pageCount; j++)
                {

                    Url = string.Format("http://hn.189.cn/webportal-wt/hnselfservice/billquery/bill-query!queryBill.action?tabIndex=2&queryMonth={0}&patitype={1}&pageNo={2}&valicode=&accNbr=", date.ToString("yyyy-MM"), queryType, j);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = type + "0" + (i + 1) + j, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
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
                var s = crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1) + "1"));
                if (s.Count() > 0)
                {
                    PhoneCostStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1) + "1")).FirstOrDefault().CrawlerTxt);
                    int pageCount = 0;
                    var result = HtmlParser.GetResultFromParser(PhoneCostStr, "//li[@style='float:right;']", "inner");
                    if (result.Count != 0)
                    {
                        pageCount = Regex.Replace(result[0].Split('：')[2], @"[^\d.\d]", "").ToInt(0);
                    }
                    for (int j = 1; j <= pageCount; j++)
                    {
                        PhoneCostStr = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == (type + "0" + (i + 1) + j)).FirstOrDefault().CrawlerTxt);
                        var infos = HtmlParser.GetResultFromParser(PhoneCostStr, "//table[@class='pas_table taocan_table']/tr", "inner");
                        //定义年月
                        var yearandmonth = string.Empty;
                        foreach (var item in infos)
                        {
                            //短信详单
                            if (type == EnumMobileDeatilType.SMS)
                            {
                                List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                                if (tdRow.Count != 6 && tdRow.Count != 0)
                                {
                                    yearandmonth = tdRow[0];
                                    continue;
                                }
                                Sms sms = new Sms();
                                if (tdRow.Count > 0)
                                {
                                    if (yearandmonth != "")
                                    {
                                        sms.StartTime = DateTime.Parse(yearandmonth + " " + tdRow[1]).ToString(Consts.DateFormatString11);  //起始时间
                                    }
                                    else
                                    {
                                        sms.StartTime = DateTime.Parse(tdRow[1]).ToString(Consts.DateFormatString11);
                                    }
                                    sms.InitType = tdRow[5];  //通信方式
                                    sms.OtherSmsPhone = tdRow[3];  //对方号码
                                    sms.SubTotal = tdRow[4].ToDecimal(0);  //总费用
                                    mobile.SmsList.Add(sms);
                                }
                            }
                            else if (type == EnumMobileDeatilType.Net)    //上网详单
                            {
                                List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                                if (tdRow.Count != 8)
                                {
                                    continue;
                                }
                                Net gprs = new Net();
                                if (tdRow.Count > 0)
                                {
                                    gprs.StartTime = DateTime.Parse(tdRow[1]).ToString(Consts.DateFormatString11);  //开始时间
                                    gprs.Place = tdRow[5];  //通信地点
                                    gprs.NetType = tdRow[4];  //网络类型
                                    gprs.SubTotal = tdRow[6].ToDecimal(0);  //单次费用
                                    gprs.SubFlow = tdRow[3];  //单次流量
                                    gprs.UseTime = tdRow[2];  //上网时长
                                    gprs.PhoneNetType = tdRow[7]; //上网方式
                                    mobile.NetList.Add(gprs);
                                }


                            }
                            else      //通话详单
                            {
                                List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                                Call call = new Call();
                                if (tdRow.Count != 8)
                                {
                                    if (tdRow.Count != 0)
                                    {
                                        yearandmonth = tdRow[0];
                                    }
                                    continue;
                                }
                                if (tdRow.Count > 0)
                                {
                                    if (yearandmonth != "")
                                    {
                                        call.StartTime = DateTime.Parse(yearandmonth + " " + tdRow[1]).ToString(Consts.DateFormatString11);  //起始时间
                                    }
                                    else
                                    {
                                        call.StartTime = DateTime.Parse(tdRow[1]).ToString(Consts.DateFormatString11);
                                    }

                                    var totalSecond = 0;
                                    var usetime = tdRow[4].ToString();
                                    if (!string.IsNullOrEmpty(usetime))
                                    {
                                        totalSecond = CommonFun.ConvertDate(usetime);
                                    }
                                    call.CallPlace = tdRow[5];  //通话地点
                                    call.OtherCallPhone = tdRow[3];  //对方号码
                                    call.UseTime = totalSecond.ToString();  //通话时长
                                    call.CallType = tdRow[7];  //通话类型
                                    call.InitType = tdRow[2];  //呼叫类型
                                    call.SubTotal = tdRow[6].ToDecimal(0);  //通话费用
                                    mobile.CallList.Add(call);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 校验是否需要验证码
        /// </summary>
        /// <returns></returns>
        private bool CheckNeedVerify(MobileReq mobileReq)
        {
            string Url = string.Empty;
            string flag = string.Empty;
            string postdata = string.Empty;
            try
            {
                Url = "http://login.189.cn/login/ajax";
                postdata = string.Format("m=loadlogincaptcha&Account={0}&UType=201&ProvinceID=06&AreaCode=&CityNo=", mobileReq.Mobile);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "http://login.189.cn/login",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                JObject jsonObj = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                int FailTimes = jsonObj["FailTimes"].ToString().ToInt(0);
                int LockTimes = jsonObj["LockTimes"].ToString().ToInt(0);
                if (FailTimes >= 3)
                {
                    return true;
                }

            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("湖北电信校验异常", e);
            }
            return false;
        }

        #endregion

    }
}
