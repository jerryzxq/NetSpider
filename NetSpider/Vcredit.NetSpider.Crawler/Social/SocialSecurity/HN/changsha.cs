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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.HN
{
    public class changsha : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        //grcxFlex/index.do
        string baseUrl = "https://www.cs12333.com/";
        string socialCity = "hn_changsha";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "https://www.cshrss.gov.cn/ggfw/grsbcx/";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = socialCity + ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
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
            int PaymentMonths = 0;
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
                //获取登录账号对应信息
                Url = baseUrl + "grcxFlex/index.do";
                postdata = string.Format("leiBie=1&usName={0}",socialReq.Username);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var aac001 = HtmlParser.GetResultFromParser(httpResult.Html,"//input[@name='aac001']","value")[0];
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                //登录
                Url = baseUrl + "grcxFlex/login.do";
                postdata = string.Format("aac001={0}&pwd={1}&login.x=12&login.y=3",aac001 ,socialReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Allowautoredirect=false,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

              
                Url = baseUrl + "grcxFlex/flex-debug/grcxMain.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies


                Url = baseUrl + "xywzweb/html/fwdt/xxcx/shbxjfcx/index.shtml";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 第二步， 获取基本信息
                var day = DateTime.Now.Day + 99;
                Url = baseUrl + string.Format("hbwz/qtpage/fwdt/shbxjfcx_result.jsp?scbh={0}&sfzh={1}&dt={2}", socialReq.Password, socialReq.Identitycard, day);
                httpItem = new HttpItem()
                {
                    URL = Url,
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

                var baseInfo = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@border='1']", "")[0];
                results = HtmlParser.GetResultFromParser(baseInfo, "//tr/td", "");
                if (results.Count > 0)
                {
                    Res.Loginname = results[2];//社保账号
                    Res.Name = results[4];//姓名
                    Res.IdentityCard = results[6];//身份证号码
                    Res.Sex = results[8];//性别
                    Res.CompanyName = results[10];//公司名称
                }
                #endregion


                #region 第三步，查询明细 （养老保险无缴费明细，以医疗保险为主,且医疗保险只提供一年内缴费详单）
                var medicalDetail = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='AutoNumber3']", "")[1];
                results = HtmlParser.GetResultFromParser(medicalDetail, "//tr", "");
                foreach (var item in results)
                {
                    detailRes = new SocialSecurityDetailQueryRes();
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                    if (tdRow.Count != 5||tdRow[0].Contains("险种"))
                    {
                        continue;
                    }
                    detailRes.PayTime = tdRow[1];  //缴费年月
                    if (tdRow[4].Contains("已划入"))
                    {
                        detailRes.PensionAmount = tdRow[3].ToDecimal(0) / 2; //个人缴费
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        PaymentMonths++;
                    }
                    else
                    {
                        detailRes.PensionAmount = tdRow[3].ToDecimal(0) / 2; //个人缴费
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                    }
                    Res.Details.Add(detailRes);
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
