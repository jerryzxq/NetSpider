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
    public class wujin : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string Url = "http://www.wjhrss.gov.cn/others/sycx/";
        string socialCity = "js_wujin";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = Url,
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
            string postdata = string.Empty;
            int PaymentMonths = 0;
            List<string> results = new List<string>();
            decimal rate_yanglao = (decimal)8/(decimal)28; //养老个人：养老实缴 8%/28%
            decimal rate_yiliao = (decimal)2/(decimal)10;//医保个人：（医保实缴 - 5） 2%/10% ；医保个人缴费为缴费基数 * 2% + 5；通过实缴计算位： （实缴-5） * rate + 5
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                postdata = String.Format("idcord={0}&pwd={1}&button.x=34&button.y=14&action=loginpost", socialReq.Username.IsEmpty() ? socialReq.Identitycard : socialReq.Username, socialReq.Password);
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
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/script");
                if (results.Count > 0 && results[0].Contains("alert"))
                {
                    Res.StatusDescription = CommonFun.GetMidStr(results[0], "alert('", "'"); ;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取信息

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div [@id='midcontent']/table[5]/tr/td", "inner");
                results = HtmlParser.GetResultFromParser(results[1], "//table/tr/td", "inner");
                if (results.Count != 18 || string.IsNullOrEmpty(results[9]))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.IdentityCard = results[9].Trim();//身份证号
                Res.Name = results[10].Trim();//姓名
                Res.Sex = results[11].Trim();//性别
                Res.SpecialPaymentType = "医保卡状态：" + results[14].ToTrim();

                for (int i = 1; i < 6; i++)
                {
                    //排列读取数据优先级（养老：1>医疗：2>失业：4>工伤：5>生育：3）
                    int j = i;
                    if (i == 5)
                    {
                        j = 3;
                    }
                    else if (i > 2)
                    {
                        j = i + 1;
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table [@id='f2_" + j + "']/tr", "inner");
                    results.RemoveAt(0);
                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 6)
                            continue;

                        string SocialInsuranceTime = tdRow[2];
                        detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                        bool NeedAddNew = false;
                        if (detailRes == null)
                        {
                            detailRes = new SocialSecurityDetailQueryRes();
                            NeedAddNew = true;
                        }

                        switch (i)
                        {
                            case 1://养老
                                detailRes.PensionAmount = tdRow[5].ToDecimal(0) * (decimal)8 / (decimal)28;
                                detailRes.CompanyPensionAmount = tdRow[5].ToDecimal(0) - detailRes.PensionAmount;
                                break;
                            case 2://医疗
                                detailRes.MedicalAmount = tdRow[5].ToDecimal(0) == 0 ? 0 : rate_yiliao * (tdRow[5].ToDecimal(0) - 5) + 5;
                                detailRes.CompanyMedicalAmount = tdRow[5].ToDecimal(0) - detailRes.MedicalAmount;
                                break;
                            case 3://失业
                                detailRes.UnemployAmount = tdRow[5].ToDecimal(0);
                                break;
                            case 4://工伤
                                detailRes.EmploymentInjuryAmount = tdRow[5].ToDecimal(0);
                                break;
                            case 5://生育
                                detailRes.MaternityAmount = tdRow[5].ToDecimal(0);
                                break;
                        }
                        
                        //if (Res.CompanyName.IsEmpty())
                        //    Res.CompanyName = tdRow[0].Trim();//公司名称

                        if (NeedAddNew)
                        {
                            detailRes.CompanyName = tdRow[0].Trim();
                            detailRes.PayTime = tdRow[1];
                            detailRes.SocialInsuranceTime = tdRow[2];
                            detailRes.SocialInsuranceBase = tdRow[3].ToDecimal(0);
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = tdRow[1] != "" && (tdRow[4] == tdRow[5]) ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                            
                            Res.Details.Add(detailRes);
                        }
                    }
                }


                #endregion

                //Res.PaymentMonths = PaymentMonths;
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
