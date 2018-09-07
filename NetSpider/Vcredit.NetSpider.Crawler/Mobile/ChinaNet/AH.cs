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
    public class AH : IMobileCrawler
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

        public VerCodeRes MobileInit(MobileReq mobileReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                //http://ah.189.cn/biz/
                Url = "http://www.189.cn/ah/";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                //第一步，初始化登录页面
                Url = "http://ah.189.cn/sso/login?returnUrl=%2Fservice%2Faccount%2Finit.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };


                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = "http://ah.189.cn/sso/freeAccountLogin";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = "ip=192.168.0.21",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //登录
                Url = "http://ah.189.cn/sso/VImage.servlet?random=0.8369153242984918";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                ////保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);

                Res.StatusDescription = "安徽电信初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "安徽电信初始化异常";
                Log4netAdapter.WriteError("安徽电信初始化异常", e);
            }
            return Res;
        }

        public BaseRes MobileLogin(MobileReq mobileReq)
        {
            BaseRes Res = new BaseRes();
            Basic mobile = new Basic();
            string Url = string.Empty;
            string postdata = string.Empty;
            Res.Token = mobileReq.Token;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                    //CacheHelper.RemoveCache(mobileReq.Token);
                }
                //校验参数
                if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = "http://ah.189.cn/sso/ValidateRandom";
                postdata = string.Format("validCode={0}", mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
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


                string SSORequestXML = string.Empty;
                Url = "http://ah.189.cn/sso/LoginServlet";
                mobileReq.Password = RSAHelper.EncryptStringByRsaJS(mobileReq.Password, "a5aeb8c636ef1fda5a7a17a2819e51e1ea6e0cceb24b95574ae026536243524f322807df2531a42139389674545f4c596db162f6e6bbb26498baab074c036777", "10001", "130");
                postdata = string.Format("ssoAuth=0&returnUrl=%2Fservice%2Faccount%2Finit.action&sysId=1003&loginType=4&accountType=10&latnId=551&loginName={0}&passType=0&passWord={1}&validCode={2}&remPwd=13&csrftoken=", mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "http://ah.189.cn/sso/login?returnUrl=%2Fservice%2Faccount%2Finit.action",
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='SSORequestXML']", "value");
                if (results.Count > 0)
                {
                    SSORequestXML = results[0];
                }

               
                Url = "http://uam.ah.ct10000.com/ffcs-uam/login";
                postdata = string.Format("SSORequestXML={0}", SSORequestXML.Replace("\n", "").ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Allowautoredirect = false,
                    Referer = "http://ah.189.cn/sso/LoginServlet",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                postdata = CommonFun.GetMidStr(httpResult.Html, "http://ah.189.cn/sso/LoginServlet?", "\">").ToTrim("amp;");
                //Url = "http://ah.189.cn/sso/LoginServlet?_v=19f8b774390a9f0cb0fa3a74f10bacc22ef822476b380c653aba75d92d56f4092e0486161eaab44607b89167554c6ce62d6a9aa0c70738e4062126023219c72a5cd3e44f3b348df6c5f9af2d6ae08bebe469e59fa797b45d48eb585dbb6d2a7876a129ea44e24da6f97b36f42780593f94fedb9b9a4d0f511175e1ee09295e5ba305def757bfca99&amp;UATicket=13-ST-750184-FRobNgLb2YnmqdaVgBiR-ahtelecom";

                Url = "http://ah.189.cn/sso/LoginServlet";
                // Url = "http://ah.189.cn/biz/service/account/init.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                    Referer = "http://ah.189.cn/sso/LoginServlet",
                    Allowautoredirect = false
                    //UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:41.0) Gecko/20100101 Firefox/41.0"
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://ah.189.cn/service/account/init.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Allowautoredirect = false,
                    Referer = "http://ah.189.cn/sso/LoginServlet",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://ah.189.cn/biz/service/account/";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://ah.189.cn/sso/LoginServlet",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://ah.189.cn/biz/service/account/init.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://ah.189.cn/biz/service/account/",
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


               
                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_SendSMS;
                CacheHelper.SetCache(Res.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "安徽电信手机登录异常";
                Log4netAdapter.WriteError("安徽电信手机登录异常", e);
            }
            return Res;
        }

        public VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            return null;
        }

        public BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            return null;
        }

        public BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {
            BaseRes Res = new BaseRes();
            Basic mobile = new Basic();
            MobileMongo mobileMongo = new MobileMongo(appDate);
            Call call = new Call();
            Net gprs = new Net();
            Sms sms = sms=new Sms();
            MonthBill bill = new  MonthBill();
            string Url = string.Empty;
            string postdata = string.Empty;
            DateTime date = DateTime.Now;
            List<string> results = new List<string>();
            List<string> infos = new List<string>();
            try
            {

                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }

               #region 获取个人基本信息
                Url = "http://ah.189.cn/biz/service/manage/showCustInfo.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://ah.189.cn/biz/service/account/init.action",
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
                
                mobile.Mobile = mobileReq.Mobile;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form/div/div/table/tbody/tr[1]/td[1]", "");
                if (results.Count > 0)
                {
                    mobile.Name = results[0].ToTrim("\t").ToTrim("\r").ToTrim("\n");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form/div/div/table/tbody/tr[5]/td[1]", "");
                if (results.Count > 0)
                {
                    mobile.Address = results[0].ToTrim("\t").ToTrim("\r").ToTrim("\n");
                }
                #endregion

                #region 第二步 PUK

                Url = "http://ah.189.cn/biz/support/puk/initPukCode.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://ah.189.cn/biz/service/account/init.action",
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

                Url = "http://ah.189.cn/biz/support/puk/pukCode.action";
                postdata = string.Format("serviceNbr={0}&serviceType=481&ofrId=481&start=0&csrftoken=", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "http://ah.189.cn/biz/support/puk/initPukCode.action",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                mobile.PUK = jsonParser.GetResultFromMultiNode(httpResult.Html, "data:PUK1");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region  第三步  获取积分
                Url = "http://ah.189.cn/service/point/point_service.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    CookieCollection = cookies,
                    Referer = "http://ah.189.cn/biz/service/account/init.action",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='fl']/table/tr[1]/td[4]", "");
                if(results.Count>0)
                {
                    mobile.Integral = results[0].ToTrim("\n").ToTrim("\t").ToTrim("\r");//当前可用积分
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region  第四步 月消费情况

                List<string> list = new List<string>();
              
                //Dictionary<string, string> list = new Dictionary<string,string>();
                Url = "http://ah.189.cn/biz/service/bill/getBillSum.action";
                postdata ="_v=709a53e9a609b906f76e90fdaad18f8bbf5659a64e852914e707a4412cdd13d09928267c87f14e1ca5d7b6e7902923f854e9254baa779d8ad759511bbf97ef29";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "http://ah.189.cn/biz/service/bill/fee.action?type=bill",
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//tr", "");
                foreach(var item in results)
                {
                    string s = "\\\"\"\">查看账单";
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                    list.Add(CommonFun.GetMidStr(tdRow[2], "_v=\"", s));
                }

                foreach (string _V in list)
                {
                    Url = string.Format("http://ah.189.cn/biz/service/bill/queryBillDetail.action?_v={0}",_V);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        Referer = "http://ah.189.cn/biz/service/bill/fee.action?type=bill",
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (httpResult.StatusCode != HttpStatusCode.OK)
                    {
                        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='mt10']/tbody/tr[2]/td[1]", "");
                    if(results.Count>0)
                    {
                        Regex regex = new Regex(@"[0-9]{4}-[0-9]{2}-[0-9]{2}");
                        string ss = regex.Match(results[0]).Value;
                        bill.BillCycle = regex.Match(results[0]).Value;
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='s2']/table[@class='titbill']/tbody/tr[1]/td[1]/ul[1]/li[1]/label[1]", "");
                    if(results.Count>0)
                    {
                        bill.PlanAmt=results[0];
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='s2']/table[@class='titbill']/tbody/tr[1]/td[1]/h5/label[@class='s1 red fr']", "");
                    if(results.Count>0)
                    {
                        bill.TotalAmt = results[0];
                    }
                    mobile.BillList.Add(bill);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                }
                #endregion


                #region  第五步  获取套餐
                Url = "http://ah.189.cn/biz/service/manage/myProducts.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://ah.189.cn/biz/service/manage/myProducts.action",
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

                Url = "http://ah.189.cn/biz/service/manage/queryattachProInfo.action";
                postdata = string.Format("serviceNbrs={0}&latnID=557&prdType=4&type=4", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata=postdata,
                    CookieCollection = cookies,
                    Referer = "http://ah.189.cn/biz/service/manage/myProducts.action",
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
                Url = "http://ah.189.cn/biz/service/manage/queryOfrByPrdInfo.action";

                postdata = string.Format("serviceNbrs={0}", mobileReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "http://ah.189.cn/biz/service/manage/myProducts.action",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                mobile.Package = jsonParser.GetResultFromParser(httpResult.Html.TrimStart('[').TrimEnd(']'), "offeringName"); 
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 上网清单
                string serviceNbr = string.Empty;
                string operListId = string.Empty;
                string fileName = string.Empty;
                string isPre = string.Empty;
                string prdType = string.Empty;
                Url = "http://ah.189.cn/biz/service/bill/initAllDetails.action?rnd=0.6775337326471547";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://ah.189.cn/biz/service/manage/myProducts.action",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='hidserviceNbr']", "value");
                if(results.Count>0)
                {
                    serviceNbr = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='internetType']", "value");
                if (results.Count > 0)
                {
                    operListId = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='hidisFileName']", "value");
                if (results.Count > 0)
                {
                    fileName = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='hidisPrepay']", "value");
                if (results.Count > 0)
                {
                    isPre = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='hidofferingProductId']", "value");
                if (results.Count > 0)
                {
                    prdType = results[0];
                }
                // 预付费
                if (isPre == "1")
                {
                    // 宽带传3
                    if (prdType == "427" || prdType == "5413")
                    {
                        operListId = "5";
                    }
                }
                else
                {// 后付费
                    // 宽带传3
                    if (prdType == "427")
                    {
                        operListId = "5";
                    }
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                for (int i = 0; i <= 6; i++)
                {
                    string expDate = string.Empty;
                    string effDate = string.Empty;
                    DateTime datetime = DateTime.Now.AddMonths(-i);
                    if(i==0)
                    {
                        effDate = datetime.AddDays(1 - datetime.Day).ToString("yyyy-MM-dd");
                        expDate = datetime.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        effDate = datetime.AddDays(1 - datetime.Day).ToString("yyyy-MM-dd");
                        expDate = datetime.AddDays(1 - datetime.Day).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd"); 
                    }
                    Url = "http://ah.189.cn/biz/service/bill/queryDetail.action";
                    int page = 1;
                    while (true)
                    {
                        postdata = "_v=" + RSAHelper.EncryptStringByRsaJS(string.Format("currentPage={5}&pageSize=10&effDate={0}&expDate={1}&serviceNbr={2}&operListId={3}&fileName={4}", effDate, expDate, serviceNbr, operListId, fileName,page.ToString()), "a5aeb8c636ef1fda5a7a17a2819e51e1ea6e0cceb24b95574ae026536243524f322807df2531a42139389674545f4c596db162f6e6bbb26498baab074c036777", "10001", "130");
                        
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "post",
                            Postdata = postdata,
                            CookieCollection = cookies,
                            Referer = "http://ah.189.cn/biz/service/bill/fee.action?type=allDetails",
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        if (httpResult.StatusCode != HttpStatusCode.OK)
                        {
                            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                            Res.StatusCode = ServiceConsts.StatusCode_fail;
                            return Res;
                        }
                        page++;
                        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tab-3']/tbody[@class='tc']/tr", "");
                        if (results.Count == 0)
                            break;
                        else
                        {
                            foreach (string item in results)
                            {
                                List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                                gprs.StartTime = tdRow[1];
                                gprs.Place = tdRow[5];
                                gprs.NetType = tdRow[4];
                                gprs.SubTotal = tdRow[6].ToDecimal(0);
                                gprs.SubFlow = tdRow[3];
                                gprs.UseTime = tdRow[2];
                               
                                //gprs.PhoneNetType = tdRow[6];
                                mobile.NetList.Add(gprs);
                            }
                        }
                    }
                }

                //var params = "currentPage=1&pageSize=10&effDate=" + effDate + "&expDate=" + expDate + "&serviceNbr=" + serviceNbr + "&operListId=" + operListID + "&fileName=" + fileName;
                #endregion

                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "安徽电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "安徽电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("安徽电信手机账单抓取异常", e);
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
    }

    internal class PUK
    {
        private string CardCode { get; set; }
        private string IMSI { get; set; }
        private string PUK1 { get; set; }
        private string PUK2 { get; set; }
     
    }
}


