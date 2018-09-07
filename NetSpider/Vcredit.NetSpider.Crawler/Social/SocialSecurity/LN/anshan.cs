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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.LN
{
    public class anshan : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.asshbx.gov.cn/";
        string socialCity = "ln_anshan";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            string Url = string.Empty;
            Res.Token = token;
            try
            {
                Url = baseUrl + "asweb/cxlog1.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl,
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
                if (socialReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

              
            



                #region 医疗保险（鞍山医疗保险以年份为单位）

                Url = "http://www.asshbx.gov.cn/asweb/cxyil.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = "yincang=%D2%BD%C1%C6&subid=&PASSWORD=" + socialReq.Identitycard + "&Submit=%D2%BD%C1%C6",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@name='form1']/table[@width='90%']/tr/td/span", "inner");
                if (results.Count <= 0)
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0].Split('：')[1].Split('&')[0];   //单位电脑编号
                    Res.CompanyName = results[0].Split('：')[2].Split('&')[0].Split('<')[0];        //单位名称
                    Res.EmployeeNo = results[0].Split('：')[3].Split('&')[0];   //个人电脑编号
                    Res.Name = results[0].Split('：')[4].Split('&')[0];        //姓名
                }
                #endregion

                #region 查询医疗明细
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@valign='top']/table[@height='28']/tr[position()>1]", "inner");
                foreach (var item in results)
                {
                    SocialSecurityDetailQueryRes detail = new SocialSecurityDetailQueryRes();
                    List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow.Count != 8)
                    {
                        continue;
                    }

                    detail.PayTime = tdRow[0];  //缴费年
                    detail.SocialInsuranceTime = tdRow[7];  //应属年
                    detail.MedicalAmount = tdRow[2].ToDecimal(0);  //个人年度医保缴费
                    detail.CompanyMedicalAmount = tdRow[3].ToDecimal(0);  //单位年度医保缴费
                    detail.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    detail.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    Res.Details.Add(detail);
                }
                #endregion


                #region 养老保险
                Url = baseUrl + "asweb/cxyl.jsp";
                postdata = String.Format("yincang=%D1%F8%C0%CF&subid=&PASSWORD={0}&Submit=%D1%F8%C0%CF", socialReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Encoding = Encoding.GetEncoding("gb2312"),
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@valign='top']/table[@height='28']/tr[position()>1]", "inner");
                if (results.Count > 0)
                {
                    #region 查询养老明细 （2001年7月开始单位缴费部分不再按比例划入个人帐户，个人帐户只记载个人缴费部分。）
                    foreach (var item in results)
                    {
                        SocialSecurityDetailQueryRes detail = new SocialSecurityDetailQueryRes();
                        List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        if (tdRow.Count != 6)
                        {
                            continue;
                        }

                        var SocialInsuranceTime = tdRow[1];  //实际缴费年月
                        if (Res.Details.Where(e => e.PayTime == SocialInsuranceTime).Count() > 0)
                        {
                            Res.Details.Where(e => e.PayTime == SocialInsuranceTime).ToList()[0].PensionAmount = tdRow[3].ToDecimal(0);  //个人养老缴费
                            Res.Details.Where(e => e.PayTime == SocialInsuranceTime).ToList()[0].CompanyPensionAmount = tdRow[2].ToDecimal(0); //单位养老缴费
                        }
                        else
                        {
                            detail.PayTime = tdRow[0];  //缴费年月
                            detail.SocialInsuranceTime = tdRow[1];  //实际缴费年月
                            detail.PensionAmount = tdRow[3].ToDecimal(0);  //个人缴费
                            detail.CompanyPensionAmount = tdRow[2].ToDecimal(0);   //单位缴费
                            detail.YearPaymentMonths = int.Parse(tdRow[4]);  //缴费月数
                            detail.SocialInsuranceBase = tdRow[5].ToDecimal(0);  //缴费基数和
                            detail.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detail.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            Res.Details.Add(detail);
                        
                        }
                    }
                    #endregion
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
