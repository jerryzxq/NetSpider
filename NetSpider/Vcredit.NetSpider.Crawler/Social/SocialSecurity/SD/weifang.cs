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
namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.SD
{
    public class weifang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "https://www.sdwfhrss.gov.cn:8000/rsjwz/";
        string socialCity = "sd_weifang";
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
                Url = baseUrl;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "/valid/code";
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
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
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

            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = baseUrl + "self/rzlogin";
                postdata = String.Format("username={0}&password={1}&yzm={2}&button=", socialReq.Username, socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//font[@color='red']");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies


                #endregion

                #region 第二步， 获取基本信息
                Url = baseUrl + "self/cbgk";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    //Referer = baseUrl + "self/rzlogin",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table//tbody/tr/td");
                if (results.Count > 8)
                {
                    Res.Name = CommonFun.ClearFlag(results[1]);
                    Res.EmployeeStatus = CommonFun.ClearFlag(results[3]);
                    Res.CompanyName = CommonFun.ClearFlag(results[7]);
                }
                #endregion

                #region 第三步，查询明细

                results.Clear();
                for (int i = 0; i < 5; i++)
                {
                    Url = baseUrl + "self/cblb?time=" + DateTime.Now.AddYears(-i).Year + "&cblb=101";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//tbody/tr"));
                }

                foreach (string item in results)
                {
                    List<string> detail = HtmlParser.GetResultFromParser(item, "/td");
                    if (detail.Count != 7)
                        continue;

                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.PayTime = detail[1];
                    detailRes.SocialInsuranceTime = detail[1];
                    detailRes.SocialInsuranceBase = detail[3].ToDecimal(0);
                    detailRes.CompanyPensionAmount = detail[5].ToDecimal(0);
                    detailRes.PensionAmount = detail[6].ToDecimal(0);
                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;

                    Res.Details.Add(detailRes);
                }

                results.Clear();
                for (int i = 0; i < 5; i++)
                {
                    Url = baseUrl + "self/zgyl?time=" + DateTime.Now.AddYears(-i).Year;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//tbody/tr"));
                }

                foreach (string item in results)
                {
                    List<string> detail = HtmlParser.GetResultFromParser(item, "/td");
                    if (detail.Count != 7)
                        continue;

                    string SocialInsuranceTime = detail[1];
                    detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();

                    if (detailRes == null)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.PayTime = detail[1];
                        detailRes.SocialInsuranceTime = detail[1];
                        detailRes.SocialInsuranceBase = detail[3].ToDecimal(0);
                        detailRes.CompanyMedicalAmount = detail[5].ToDecimal(0);
                        detailRes.MedicalAmount = detail[6].ToDecimal(0);
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;

                        Res.Details.Add(detailRes);
                    }
                    else
                    {
                        detailRes.CompanyMedicalAmount = detail[5].ToDecimal(0);
                        detailRes.MedicalAmount = detail[6].ToDecimal(0);
                    }
                }
                #endregion

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
