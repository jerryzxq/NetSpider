using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.Common.Constants;
using Vcredit.NetSpider.DataAccess.Cache;
namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.JS
{
    public class suzhou : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "";
        string socialCity = "js_suzhou";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://www.szsbzx.net.cn:9900/web/website/rand.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    ResultType = ResultType.Byte,
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


                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                SpiderCacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitError;
                Log4netAdapter.WriteError(socialCity + ServiceConsts.SocialSecurity_InitError, e);
            }
            return Res;
        }

        public SocialSecurityQueryRes SocialSecurityQuery(SocialSecurityReq socialReq)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = socialCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(socialReq.Token);
                    SpiderCacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Username.IsEmpty() || socialReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = "http://www.szsbzx.net.cn:9900/web/website/indexProcess?frameControlSubmitFunction=checkLogin";
                postdata = String.Format("grbh={0}&sfzh={1}&yzcode={2}", socialReq.Username, socialReq.Identitycard, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
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

                string message = jsonParser.GetResultFromParser(httpResult.Html, "errormsg");
                if (!message.IsEmpty())
                {
                    Res.StatusDescription = message;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，个人基本信息
                Url = "http://www.szsbzx.net.cn:9900/web/website/personQuery/personQueryAction.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='personNum']", "value", true);
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];//编号
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='name']", "value", true);
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='sfzNum']", "value", true);
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='danweiNum']", "value", true);
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];//性别
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='danweiSite']", "value", true);
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];//状态
                }
                #endregion

                int pageIndex = 0;
                int pageCount = 0;
                string pagelistajax = string.Empty;
                #region 第三步，养老

                do
                {
                    pageIndex++;
                    Url = "http://www.szsbzx.net.cn:9900/web/website/personQuery/personQueryAction?frameControlSubmitFunction=getPagesAjax";
                    postdata = string.Format("xz=qyylmx&pageIndex={0}&pageCount=20", pageIndex);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    pageCount = CommonFun.GetMidStr(httpResult.Html, "第" + pageIndex + "\\/", "页").ToInt(0);
                    pagelistajax = jsonParser.GetResultFromParser(httpResult.Html, "pagelistajax");

                    results = HtmlParser.GetResultFromParser(pagelistajax, "//table/tr[position()>1]");

                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 9)
                        {
                            continue;
                        }

                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        try
                        {
                            int.Parse(tdRow[7].ToTrim());
                            detailRes.PayTime = tdRow[7].ToTrim();
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        }
                        catch
                        {
                            detailRes.PaymentFlag = tdRow[7].ToTrim();
                        }
                        detailRes.SocialInsuranceTime = tdRow[0].ToTrim();
                        detailRes.SocialInsuranceBase = tdRow[4].ToDecimal(0);
                        detailRes.CompanyName = tdRow[1];
                        if (tdRow[6].ToTrim() == "正常应缴")
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        }
                        else
                        {
                            detailRes.PaymentType = tdRow[6].ToTrim();
                        }
                        

                        //养老
                        detailRes.PensionAmount = tdRow[3].ToTrim().ToDecimal(0);
                        //detailRes.CompanyPensionAmount = tdRow[4].ToTrim().ToDecimal(0);

                        Res.Details.Add(detailRes);
                        PaymentMonths++;
                    }
                }
                while (pageIndex < pageCount);

                #endregion

                #region 第四步，医疗
                pageIndex = 0;
                do
                {
                    pageIndex++;
                    Url = "http://www.szsbzx.net.cn:9900/web/website/personQuery/personQueryAction?frameControlSubmitFunction=getPagesAjax";
                    postdata = string.Format("xz=ylbx&pageIndex={0}&pageCount=20", pageIndex);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    pageCount = CommonFun.GetMidStr(httpResult.Html, "第" + pageIndex + "\\/", "页").ToInt(0);
                    pagelistajax = jsonParser.GetResultFromParser(httpResult.Html, "pagelistajax");

                    results = HtmlParser.GetResultFromParser(pagelistajax, "//table/tr[position()>1]");

                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 8)
                        {
                            continue;
                        }
                        string SocialInsuranceTime = tdRow[0].ToTrim();
                        detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                        if (detailRes == null)
                        {
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.Name = Res.Name;
                            detailRes.PayTime = tdRow[0].ToTrim();
                            detailRes.SocialInsuranceTime = tdRow[0].ToTrim();
                            detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                            detailRes.CompanyName = tdRow[1];
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            if (tdRow[7].ToTrim() == "已到账")
                                detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            else
                                detailRes.PaymentFlag = tdRow[7].ToTrim();

                            //医疗
                            detailRes.MedicalAmount = tdRow[3].ToDecimal(0);
                            detailRes.CompanyMedicalAmount = tdRow[4].ToDecimal(0);

                            Res.Details.Add(detailRes);
                            //PaymentMonths++;
                        }
                        else
                        {
                            //医疗
                            detailRes.MedicalAmount = tdRow[3].ToDecimal(0);
                            detailRes.CompanyMedicalAmount = tdRow[4].ToDecimal(0);
                        }
                    }
                }
                while (pageIndex < pageCount);

                #endregion

                #region 第五步，失业、工伤、生育
                List<string> baoxiantype = new List<string> { "shiyebx","gsbx","sybx"};
                for (int i = 0; i < 3; i++)
                {
                    pageIndex = 0;
                    pageCount = 0;
                    do
                    {
                        pageIndex++;
                        Url = "http://www.szsbzx.net.cn:9900/web/website/personQuery/personQueryAction?frameControlSubmitFunction=getPagesAjax";
                        postdata = string.Format("xz={0}&pageIndex={1}&pageCount=20", baoxiantype[i], pageIndex);
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "post",
                            Postdata = postdata,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        pageCount = CommonFun.GetMidStr(httpResult.Html, "第" + pageIndex + "\\/", "页").ToInt(0);
                        pagelistajax = jsonParser.GetResultFromParser(httpResult.Html, "pagelistajax");

                        results = HtmlParser.GetResultFromParser(pagelistajax, "//table/tr[position()>1]");

                        foreach (string item in results)
                        {
                            var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                            if (tdRow.Count != 5)
                            {
                                continue;
                            }
                            string SocialInsuranceTime = tdRow[0].ToTrim();
                            detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                            if (detailRes == null)
                            {
                                detailRes = new SocialSecurityDetailQueryRes();
                                detailRes.Name = Res.Name;
                                detailRes.PayTime = tdRow[0].ToTrim();
                                detailRes.SocialInsuranceTime = tdRow[0].ToTrim();
                                detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                                detailRes.CompanyName = tdRow[1];
                                detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                                if (tdRow[4].ToTrim() == "已到账")
                                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                                else
                                    detailRes.PaymentFlag = tdRow[4].ToTrim();

                                Res.Details.Add(detailRes);
                                //PaymentMonths++;
                            }
                        }
                    }
                    while (pageIndex < pageCount);
                }
                #endregion
                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(socialCity + ServiceConsts.SocialSecurity_QueryError, e);
            }
            return Res;
        }
    }
}
