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
    public class jinan : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.jnhrss.gov.cn/";
        string socialCity = "sd_jinan";
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
                Url = baseUrl + "query/";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "Inc/dws_GetCode.asp?type=sbcxyzm";
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (socialReq.Vercode.ToInt(-1) == -1)
                {
                    Res.StatusDescription = "验证码错误，请重新登录!";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = baseUrl + "query/index.asp";
                postdata = String.Format("psidcd={0}&password={1}&md5_password={2}&yz={3}&query.x=54&query.y=12", socialReq.Identitycard, socialReq.Password, CommonFun.GetMd5Str(socialReq.Password), socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "query/",
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
                if (httpResult.Html.Contains("window.location.href=\"index.asp\";"))
                {
                    Res.StatusDescription = CommonFun.GetMidStr(httpResult.Html, "alert(\"","\"");
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                
                #endregion

                #region 第二步， 获取基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@class='edu_menu02']");
                if (results.Count > 0)
                {
                    Res.Name = CommonFun.GetMidStr(CommonFun.ClearFlag(results[0].ToTrim()), "<!--说明部分开始-->", "，");
                }
                Url = baseUrl + "query/ylzhresult.asp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "query/menu.asp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//tr[@class='my_menu STYLE2']/td[@colspan='2']/div");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];//身份证号
                }
                Res.IdentityCard = socialReq.Identitycard;

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//tr[@class='my_menu STYLE2']/td/div");
                if (results.Count == 11)
                {
                    Res.SpecialPaymentType = string.Format("{0}年度累计缴费月数{1}个月，单位累计：{2}，个人累计：{3}",results[1],results[2],results[3],results[4]);
                }
                #endregion

                #region 第三步，查询明细

                Url = baseUrl + "query/mediresult.asp";
                postdata = string.Format("qsn={0}&qsy={1}&zzn={2}&zzy={3}", DateTime.Now.AddMonths(-60).Year, DateTime.Now.AddMonths(-60).Month, DateTime.Now.Year, DateTime.Now.Month);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = baseUrl + "query/menu.asp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//tr[@class='my_menu STYLE2']");
                
                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td/div");
                    if (tdRow.Count != 5)
                    {
                        continue;
                    }

                    if (tdRow[3] == "医保收入")
                    {
                        try
                        {
                            detailRes = new SocialSecurityDetailQueryRes();

                            detailRes.PayTime = DateTime.Parse(tdRow[1]).ToString("yyyyMM");
                            detailRes.SocialInsuranceTime = DateTime.Parse(tdRow[1]).ToString("yyyyMM");
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            detailRes.MedicalAmount = tdRow[4].ToDecimal(0);
                            detailRes.SocialInsuranceBase = detailRes.MedicalAmount / (decimal)0.02;
                            Res.Details.Add(detailRes);
                        }
                        catch { }
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
