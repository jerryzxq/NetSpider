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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.ZJ
{
    public class changxing : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://218.75.53.62:8055/main/";
        string socialCity = "zj_changxing";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "login";
                httpItem = new HttpItem()
                {
                    URL = Url,
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Citizencard.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录

                Url = baseUrl + "checkLogin";
                postdata = String.Format("sfzh={0}&xm1={1}&card={2}", socialReq.Identitycard, socialReq.Name.ToUrlEncode(Encoding.GetEncoding("gbk")), socialReq.Citizencard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "login",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK && httpResult.Html != "1")
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取基本信息
                Url = baseUrl + "searchD?type=01";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "searchD",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr/td", "inner");
                if (results.Count <= 0 || string.IsNullOrEmpty(results[5].Trim().Replace("&nbsp;", "")))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Res.Name = results[1].Trim().Replace("&nbsp;", "");//姓名
                Res.Sex = results[3].Trim().Replace("&nbsp;", "");//性别
                Res.IdentityCard = results[5].Trim().Replace("&nbsp;", "");//身份证号
                Res.CompanyName = results[7].Trim().Replace("&nbsp;", "");//单位名称
                Res.Phone = HtmlParser.GetResultFromParser(results[11], "//input", "value")[0].Trim().Replace("&nbsp;", "").Replace("&nbsp", "");//电话
                Res.Address = HtmlParser.GetResultFromParser(results[13], "//input", "value")[0].Trim().Replace("&nbsp;", "").Replace("&nbsp", "");//地址

                #endregion

                #region 查询明细
                List<string> type_list = new List<string> { "11","31","21","41","51"};


                for (int i = 0; i < type_list.Count; i++)
                {
                    int pageIndex = 1;
                    int pageCount = 0;

                    do
                    {
                        Url = baseUrl + "searchD";
                        postdata = String.Format("page={0}&y1=&y2=&xz={1}&type=03", pageIndex, type_list[i]);
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "POST",
                            Postdata = postdata,
                            Referer = Url + "searchD",
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        pageCount = CommonFun.GetMidStr(httpResult.Html, ">/", "页").ToInt(0);
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[1]/tr", "inner");
                        results.RemoveRange(0, 3);
                        foreach (string item in results)
                        {
                            var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                            if (tdRow.Count != 6)
                            {
                                continue;
                            }
                            string SocialInsuranceTime = tdRow[1];
                            detailRes = Res.Details.Where(o=>o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();

                            bool NeedAdd = false;

                            if (detailRes == null)
                            {
                                detailRes = new SocialSecurityDetailQueryRes();
                                detailRes.PayTime = tdRow[5];
                                detailRes.SocialInsuranceTime = tdRow[1];
                                detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                                detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                                detailRes.PaymentFlag = tdRow[5].IsEmpty() ? ServiceConsts.SocialSecurity_PaymentFlag_Adjust : ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                                NeedAdd = true;
                            }

                            switch (i)
                            {
                                case 0:
                                    detailRes.PensionAmount = tdRow[3].ToDecimal(0);
                                    break;
                                case 1:
                                    detailRes.MedicalAmount = tdRow[3].ToDecimal(0);
                                    break;
                                case 2:
                                    detailRes.UnemployAmount = tdRow[3].ToDecimal(0);
                                    break;
                                case 3:
                                    detailRes.EmploymentInjuryAmount = tdRow[3].ToDecimal(0);
                                    break;
                                case 4:
                                    detailRes.MaternityAmount = tdRow[3].ToDecimal(0);
                                    break;
                            }

                            if (NeedAdd)
                            {
                                Res.Details.Add(detailRes);
                            }
                        }
                        pageIndex++;

                    }
                    while (pageIndex <= pageCount);
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
