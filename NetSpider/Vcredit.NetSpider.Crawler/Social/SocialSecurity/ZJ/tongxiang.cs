using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.ZJ
{
    /// <summary>
    /// 
    /// </summary>
    public class tongxiang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.txlss.gov.cn/txlss/wsbsdt/";
        string socialCity = "zj_tongxiang";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Res.StatusDescription = socialCity + "无需初始化";
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
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                Url = baseUrl + "logincheck.jsp";
                postdata = String.Format("type=2&AAE135={0}&password={1}", socialReq.Identitycard, socialReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
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
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\");");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 第二步，基本信息

                Url = baseUrl + "view_grjcxx.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_wsbsdt']/tr[position()>1]/td", "");
                if (results.Count != 10)
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.IdentityCard = socialReq.Identitycard;
                Res.Name = socialReq.Name;
                Res.IdentityCard = results[1];//身份证号
                Res.EmployeeNo = results[3];//编号
                Res.Name = results[5];//姓名
                Res.CompanyNo = results[7];
                Res.CompanyName = results[9];//单位名称

                //个人参保信息
                Url = baseUrl + "view_cbxx.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_wsbsdt']/tr[position()>2]/td", "", true);
                for (int i = 0; i <= results.Count - 5; i = i + 5)
                {
                    Res.SpecialPaymentType += results[i + 1] + ":" + results[i + 4] + ";";
                    if (results[i] == "企业养老")
                    {
                        Res.SocialInsuranceBase = results[i + 2].ToDecimal(0);
                    }
                }
                #endregion

                #region 第三步，查询明细

                Url = baseUrl + "view_ylbxgrzh.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string year = CommonFun.GetMidStr(httpResult.Html, "帐户年度:", "</td>");//帐户年度
                if (year == DateTime.Now.Year.ToString())
                {
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_wsbsdt']/tr[@class='text_tr_09'][17]/td[3]");
                    if (results.Count > 0)
                    {
                        Res.InsuranceTotal = results[0].ToDecimal(0);//账户总额
                    }
                }
                string paySignStr = CommonFun.GetMidStr(httpResult.Html, "缴费：", "</td>").Trim();//缴费月数标记
                int payMonth = paySignStr.Where((t, i) => paySignStr.Substring(i, 1) == "☆").Count();//该年缴费月数
                //本年个人缴费
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_wsbsdt']/tr[@class='text_tr_09'][13]/td[2]");
                decimal insuranceBase = CommonFun.GetMidStr(httpResult.Html, "缴费基数：", "(").ToDecimal(0);//该年缴费基数
                decimal payRate = (results[0].ToDecimal(0) / payMonth) / insuranceBase;  //缴费比率
                for (int i = 0; i < paySignStr.Length; i++)
                {
                    if (paySignStr.Substring(i, 1) == "☆")
                    {
                        detailRes = new SocialSecurityDetailQueryRes
                        {
                            IdentityCard = Res.IdentityCard,
                            Name = Res.Name,
                            PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal,
                            PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal,
                            SocialInsuranceTime = year + string.Format("{0:00}", i + 1),
                            SocialInsuranceBase = insuranceBase
                        };
                        detailRes.PensionAmount = detailRes.CompanyPensionAmount = insuranceBase*payRate;
                        Res.Details.Add(detailRes);
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
