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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.JS
{
    public class danyang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://218.3.90.9:8080/dyquery/";
        string socialCity = "js_danyang";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "loginzz.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录

                Url = baseUrl + "login.do?method=loginzz";
                postdata = String.Format("aac001={0}&aac002={1}&password={2}&x=27&y=21", "", socialReq.Identitycard, socialReq.Password);//个人编号可省去
                for (int i = 0; i < 2; i++)
                {
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Referer = baseUrl + "loginzz.jsp",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (httpResult.StatusCode == HttpStatusCode.OK)
                    {
                        break;
                    }
                }
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = "连接超时,请重新登陆";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 基本信息

                Url = baseUrl + "service.do?method=canbao";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "login.do?method=loginzz",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form/table/tr/td/input ", "value");
                if (results.Count <= 0 || string.IsNullOrEmpty(results[0]))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.EmployeeNo = results[0].Trim();//编号
                Res.Name = results[2].Trim();//姓名
                Res.IdentityCard = results[3].Trim();//身份证号
                Res.CompanyName = results[1].Trim();//单位名称
                Res.WorkDate = results[4].Trim();//参加工作时间
                Res.Address = results[5];//户口所在地
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='content']/table/tr[position()>1]", "", true);
                foreach (var item in results)
                {
                    var tdRowBase = HtmlParser.GetResultFromParser(item, "//td", "text");
                    if (tdRowBase.Count < 4)
                    {
                        continue;
                    }
                    switch (tdRowBase[1])
                    {
                        case "基本养老保险":
                            Res.EmployeeStatus = tdRowBase[3];//参保状态
                            break;
                    }
                    if (!string.IsNullOrEmpty(Res.EmployeeStatus))
                    {
                        break;
                    }
                }

                #endregion
                #region 明细信息
                //明细
                Url = baseUrl + "service.do?method=jiaofei2";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "login.do?method=loginzz",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='content']/center/table/tr[position()>2]", "inner", true);
                var tdRow = HtmlParser.GetResultFromParser(results[0], "//td");
                for (int i = 0; i < (tdRow.Count / 11); i++)
                {
                    bool isSave = false;
                    detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == tdRow[i * 11 + 1].Trim());
                    if (detailRes == null)
                    {
                        isSave = true;
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.PayTime = tdRow[i * 11 + 1].Trim();
                        detailRes.SocialInsuranceTime = tdRow[i * 11 + 1].Trim();
                        detailRes.SocialInsuranceBase = tdRow[i * 11 + 2].Trim().ToDecimal(0);
                    }
                    detailRes.PensionAmount += tdRow[i * 11 + 4].Trim().ToDecimal(0);
                    detailRes.UnemployAmount += tdRow[i * 11 + 6].Trim().ToDecimal(0);
                    detailRes.EmploymentInjuryAmount += tdRow[i * 11 + 8].Trim().ToDecimal(0);
                    detailRes.MaternityAmount += tdRow[i * 11 + 10].Trim().ToDecimal(0);
                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    if (!isSave) continue;
                    Res.Details.Add(detailRes);
                    PaymentMonths++;
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
