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
using System.Text.RegularExpressions;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{
    public class ZJ : IMobileCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        #endregion
        public VerCodeRes MobileInit(MobileReq mobileReq = null)
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                //第一步，初始化登录页面
                Url = "http://zj.189.cn/wt_uac/auth.html?app=wt&login_goto_url=nb%2Fzhuanti%2Fsdhd%2F&module=null&auth=uam_login_auth&template=uam_login_page";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultCookieType = ResultCookieType.CookieCollection,
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //第二步，获取验证码
                Url = "http://zj.189.cn/wt_uac/UserCCServlet?type=2&method=loginimage&dt=71536";
                httpItem = new HttpItem()
                {
                    Accept = "image/png,image/*;q=0.8,*/*;q=0.5",
                    URL = Url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ContentType = "text/html",
                    Referer = "http://zj.189.cn/wt_uac/auth.html?app=wt&login_goto_url=index/&module=null&auth=uam_login_auth&template=uam_login_page",
                    ResultCookieType = ResultCookieType.CookieCollection,
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0"
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);

                Res.StatusDescription = "浙江电信初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;
                CacheHelper.SetCache(token, cookies);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "浙江电信初始化异常";
                Log4netAdapter.WriteError("浙江电信初始化异常", e);
            }
            return Res;
        }

        public BaseRes MobileLogin(MobileReq mobileReq)
        {
            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                //校验参数
                if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                //第三步
                Url = "http://zj.189.cn/wt_uac/accounttype!gettype.ajax";
                postdata = string.Format("authBean.module=null&authBean.channel=wt&authBean.client_ip=210.22.124.10&authBean.app_name=wt&authBean.call_back_url=&authBean.auth_name=uam_login_auth&nexturl=nb%2Fzhuanti%2Fsdhd%2F&type=&area_id=&authSign=&phoneVerify=&verifySign=&account={0}&password={1}&validcode={2}&pr_type=&pr_area=&isSaveCookie=0", mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                var authSign = Regex.Matches(httpResult.Html, @"(?<=success\:).*(?=\:\[\{)");
                if (authSign.Count == 0)
                {
                    Res.StatusDescription = "登录失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //第三步
                Url = "http://zj.189.cn/wt_uac/ajaxauth!login.ajax";
                postdata = string.Format("authBean.module=null&authBean.channel=wt&authBean.client_ip=210.22.124.10&authBean.app_name=wt&authBean.call_back_url=&authBean.auth_name=uam_login_auth&nexturl=nb%2Fzhuanti%2Fsdhd%2F&type=18&area_id=576&authSign={0}&phoneVerify=&verifySign=&account={1}&password={2}&validcode={3}&pr_type=&pr_area=&isSaveCookie=0", authSign[0], mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                var result = JsonConvert.DeserializeObject(httpResult.Html);
                JObject js = result as JObject;
                if (js == null)
                {
                    Res.StatusDescription = "登录失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                var extraInfo = js["extraInfo"];
                if (extraInfo == null)
                {
                    Res.StatusDescription = "登录失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = "http://uam.zj.ct10000.com/portal/LoginAuth";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = "SSORequestXML=" + extraInfo.ToString(),
                    Referer = "http://zj.189.cn/wt_uac/auth.html?app=wt&login_goto_url=nb%2Fzhuanti%2Fsdhd%2F&module=null&auth=uam_login_auth&template=uam_login_page",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0"
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                var urlReslut = Regex.Matches(httpResult.Html, "(?<=location.href\\s\\=\\s\").*(?=\"\\;)");
                if (urlReslut.Count == 0)
                {
                    Res.StatusDescription = "登录失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                httpItem = new HttpItem()
                {
                    URL = urlReslut[0].ToString(),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                    Referer = "http://zj.189.cn/wt_uac/auth.html?app=wt&login_goto_url=nb%2Fzhuanti%2Fsdhd%2F&module=null&auth=uam_login_auth&template=uam_login_page"
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                httpItem = new HttpItem()
                {
                    URL = "http://zj.189.cn/nb/zhuanti/sdhd/",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                    Referer = urlReslut[0].ToString()
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);




                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_SendSMS;
                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "浙江电信登录异常";
                Log4netAdapter.WriteError("浙江电信登录异常", e);
            }
            return Res;
            

        }

        public VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            throw new NotImplementedException();
        }

        public BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            throw new NotImplementedException();
        }

        public BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {
            BaseRes Res = new BaseRes();
            Basic mobile = new Basic();
            MobileMongo mobileMongo = new MobileMongo(appDate);
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                //校验参数
                if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                httpItem = new HttpItem()
                {
                    URL = String.Format("http://zj.189.cn/shouji/{0}/", mobileReq.Mobile),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,

                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                httpItem = new HttpItem()
                {
                    URL = String.Format("http://zj.189.cn/shouji/{0}/bangzhu/ziliaoxg/", mobileReq.Mobile),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                var country = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='clientDomain.country']", "value");
                var city = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='clientDomain.city']", "value");
                var zone = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@name='clientDomain.county']/option", "value");
                var streetAddress = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='clientDomain.streetAddress']", "value");
                var email = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='clientDomain.email']", "value");
                mobile.Token = mobileReq.Token;
                mobile.BusName = mobileReq.Name;
                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.Mobile = mobileReq.Mobile;
                mobile.Address = (country.Count > 0 ? country[0] : "") + (city.Count > 0 ? city[0] : "") + (zone.Count > 0 ? zone[0] : "") + (streetAddress.Count > 0 ? streetAddress[0] : "");
                mobile.Email = email.Count > 0 ? email[0] : "";

                #region 账单
                ///账单
                GetDeatilsBill(mobile);
                #endregion
                //  mobileMongo.SaveBasic(mobile);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "江苏电信登录异常";
                Log4netAdapter.WriteError("江苏电信登录异常", e);
            }
            return Res;
        }

        /// <summary>
        /// 解析抓取的原始数据
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <param name="crawlerDate"></param>
        /// <returns></returns>
        public BaseRes MobileAnalysis(MobileReq mobileReq, DateTime appDate)
        {
            throw new NotImplementedException();
        }


        public void GetDeatilsBill(Basic mobile)
        {
            string Url = string.Empty;
            var startDate = String.Empty;
            var endDate = String.Empty;
            var month = String.Empty;
            var cycle = String.Empty;
            string postdata = String.Empty;
            DateTime now;
            DateTime endnow;
            var cn = String.Empty;

            for (var i = 0; i <= 5; i++)
            {

                MonthBill bill = new MonthBill();
                now = DateTime.Now.AddMonths(-i);
                startDate = new DateTime(now.Year, now.Month, 1).ToString(Consts.DateFormatString5);
                endnow = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddDays(-1);
                endDate = endnow.ToString(Consts.DateFormatString5);
                month = now.ToString(Consts.DateFormatString7);
                cycle = startDate;

                
                ///积分查询
                Url = String.Format("http://zj.189.cn/zjpr/bill/getBillDetail.htm?pr_billDomain.bill_month={0}&pr_billDomain.product_id=9599519&pr_billDomain.query_type=1&pr_billDomain.bill_type=0&flag=htzd", month);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };

                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                var table = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_fixed']");

                if (table.Count == 0)
                    continue;
                var tr = HtmlParser.GetResultFromParser(table[0], "//tr");
                if (tr.Count > 0)
                {
                    var Package = HtmlParser.GetResultFromParser(tr[1], "//td[1]");
                    if (Package.Count > 0)
                        mobile.Package = Package[0];
                   
                    var PlanAmt = HtmlParser.GetResultFromParser(tr[4], "//td[2]");
                    if (PlanAmt.Count > 0)
                        bill.PlanAmt = PlanAmt[0];
                }

                var total = HtmlParser.GetResultFromParser(table[0], "//table[@class='table_fixed']/tr[1]/td[1]");
                if (total.Count > 0)
                {
                    var totoal = Regex.Matches(total[0], @"\d+(.)?.*\d");
                    if (totoal.Count > 0)
                    {
                        bill.TotalAmt = totoal[0].ToString();
                    }
                }
                bill.BillCycle = new DateTime(now.Year, now.Month, 1).ToString(Consts.DateFormatString12);

                mobile.BillList.Add(bill);
            }
        }
    }
}
