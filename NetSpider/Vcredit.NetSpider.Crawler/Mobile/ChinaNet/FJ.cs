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

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{
    public class FJ : ChinaNet
    {
        public override VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            return null;
        }

        public override BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            return null;
        }

        public override BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {

            BaseRes Res = new BaseRes();
            MonthBill bill = new MonthBill();
            Basic mobile = new Basic();
            MobileMongo mobileMongo = new MobileMongo(appDate);

            string carrier = string.Empty;//手机号归属地
            string Url = string.Empty;
            string Uid = string.Empty;
            string PhoneCostStr = string.Empty;
            string year = string.Empty;
            string filterfield = string.Empty;
            List<string> results = new List<string>();
            DateTime first = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            string postdata = String.Empty;
            DateTime last = DateTime.Now;
            try
            {
                mobile.Token = mobileReq.Token;
                mobile.BusName = mobileReq.Name;
                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.Mobile = mobileReq.Mobile;

                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }
                //1
                Url = "http://www.189.cn/dqmh/cms/index/login_jx.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://www.189.cn/fj/",
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

                //2
                Url = "http://www.189.cn/dqmh/login.do?method=loginright";
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


                #region  积分查询
                Url = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=01420651";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    //Postdata=postdata,
                    //Method = "post",
                    CookieCollection = cookies,
                    Referer = "http://www.189.cn/fj/",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='usrr_wallet left']/tbody/tr[1]/td[1]", "");
                if(results.Count>0)
                {
                    mobile.Integral = results[0]; //积分
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                Url = "http://www.189.cn/dqmh/cms/index/login_jx.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=01420651",
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

                Url = "http://www.189.cn/dqmh/login.do?method=loginright";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=01420650",
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

                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10014&toStUrl=http://fj.189.cn/newcmsweb/commonIframe.jsp?URLPATH=/service/bill/bill.jsp&fastcode=01420650";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=01420650",
                    CookieCollection = cookies,
                    Allowautoredirect = false,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                string location =string.Empty;
                try
                {
                    location = httpResult.Header.Get("Location");
                }
                catch { }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                
                do
                {
                    Url = location;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=01420650",
                        CookieCollection = cookies,
                        Allowautoredirect = false,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    try
                    {
                        location = httpResult.Header.Get("Location");
                    }
                    catch 
                    {
                        break;
                    }
                    if (location.IsEmpty())
                        break;

                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                }
                while (location != "http://fj.189.cn/common/v2011/wait.jsp");

                #region  第二步  获取姓名
                Url = "http://fj.189.cn/common/v2011/wait.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Referer = "http://www.189.cn/dqmh/my189/initMy189home.do?fastcode=01420651",
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
                

                ///
                Url = "http://fj.189.cn/newcmsweb/commonIframe.jsp?URLPATH=/service/bill/bill.jsp&fastcode=01420651";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Referer = "http://fj.189.cn/common/v2011/wait.jsp",
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


                ///
                Url = "http://fj.189.cn/service/bill/bill.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Referer = "http://fj.189.cn/newcmsweb/commonIframe.jsp?URLPATH=/service/bill/bill.jsp&fastcode=01420651",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='biaoge2']/tr[1]/td[1]", "");
                if(results.Count>0)
                {
                    mobile.Name = results[0];//姓名
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion


                #region   五个月账单查询
                List<string> list = new List<string>();
                for (int i = 1; i <= 6; i++)
                {

                    if (DateTime.Now.AddMonths(-i).Month < 10)
                    {
                        list.Add(DateTime.Now.AddMonths(-i).Year.ToString() + "0" + DateTime.Now.AddMonths(-i).Month.ToString());
                    }
                    else
                    {
                        list.Add(DateTime.Now.AddMonths(-i).Year.ToString() + DateTime.Now.AddMonths(-i).Month.ToString());
                    }
                }
                foreach(string month in list)
                {
                    Url = "http://fj.189.cn/service/bill/custbill.jsp";
                    postdata = string.Format("ck_month={0}&citycode=593&PRODNO={1}&PRODTYPE=50",month.ToString(),mobileReq.Mobile);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method="post",
                        Postdata=postdata,
                        CookieCollection = cookies,
                        Referer = "http://fj.189.cn/service/bill/bill.jsp",
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (httpResult.StatusCode != HttpStatusCode.OK)
                    {
                        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='lszd_zl']/ul/li[3]", "");
                    if(results.Count>0)
                    {
                        bill.BillCycle = CommonFun.GetMidStr(results[0].ToString(), "</span>", "‐");
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='fyxx_bot'][6]/div[@class='fyxx_bot_a']/span/span", "");
                    if(results.Count>0)
                    {
                        bill.PlanAmt = results[0].ToTrim("\r").ToTrim("\n").ToTrim("\t").ToTrim(" ");
                    }
                    results = results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='fyxx3']/ul/li/span", "");
                    if(results.Count>0)
                    {
                        bill.TotalAmt = results[0].ToTrim("\r").ToTrim("\n").ToTrim("\t").ToTrim(" ");
                    }
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    mobile.BillList.Add(bill);
                }
                #endregion

                //保存
              
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "福建电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "福建电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("福建电信手机账单抓取异常", e);
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
            throw new NotImplementedException();
        }
    }
}
