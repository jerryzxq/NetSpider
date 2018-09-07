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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.SD
{
    public class qingdao : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.12333sh.gov.cn/sbsjb/";
        string socialCity = "sd_qingdao";
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
                Url = "http://221.215.38.136/grcx/common/checkcode.do";
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                string passwordMD5 = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(socialReq.Password, "MD5"); ;
                Url = "http://221.215.38.136/grcx/work/login.do?method=login";
                postdata = String.Format("method=login&domainId=1&groupid=-95&loginName={0}&loginName18=&password={1}&checkCode={2}", socialReq.Identitycard, passwordMD5.ToLower(), socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@class='text3']");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0].Replace("用户名", "身份证号");
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，查询基本信息
         
                Url = "http://221.215.38.136/grcx/work/m01/f1101/personQuery.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input", "value");
                if (results.Count >17)
                {
                    Res.CompanyNo = results[0];
                    Res.District = results[1];
                    Res.CompanyName = results[2];
                    Res.EmployeeNo = results[3];
                    Res.Name = results[4];
                    Res.IdentityCard = results[5];
                    Res.Sex = results[6];
                    Res.WorkDate = results[7];
                    Res.BirthDate = results[8];
                    Res.EmployeeStatus = results[9];
                    Res.Race = results[10];
                    Res.IsSpecialWork = results[11] == "否" ? false : true;
                    Res.SpecialPaymentType = results[14];
                    Res.Bank = results[15];
                    Res.BankAddress = results[16];
                 
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = "http://221.215.38.136/grcx/work/m01/f1121/show.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input", "value");
                if (results.Count==16)
                {
                    Res.Phone = results[12];
                    Res.ZipCode = results[13];
                    Res.Address = results[14];
                }
                //参保状态
                Url = "http://221.215.38.136/grcx/work/m01/f1102/insuranceQuery.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tbody/tr");
                Dictionary<string, List<string>> _status_list = new Dictionary<string, List<string>>();
                foreach (string item in results)
                {
                    List<string> tdrow = HtmlParser.GetResultFromParser(item, "td");
                    if(tdrow.Count == 4)
                    {
                        if (_status_list.ContainsKey(tdrow[3]))
                        {
                            if (!_status_list[tdrow[3]].Contains(tdrow[2]))
                            {
                                _status_list[tdrow[3]].Add(tdrow[2]);
                            }
                        }
                        else
                        {
                            _status_list.Add(tdrow[3], new List<string> { tdrow[2] });
                        }
                    }
                }
                foreach (KeyValuePair<string, List<string>> pair in _status_list)
                {
                    Res.SpecialPaymentType += pair.Key + "：[";
                    for (int i = 0; i < pair.Value.Count; i++)
                    {
                        Res.SpecialPaymentType += pair.Value[i] + (i == (pair.Value.Count - 1) ? "" : "，");
                    }
                    Res.SpecialPaymentType += "]";
                }
                #endregion

                #region 第三步，查询总页数
                Url = "http://221.215.38.136/grcx/work/m01/f1203/oldQuery.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                #endregion

                #region 第四步，查询养老、医保、失业等缴费明细
                int page = CommonFun.GetMidStr(httpResult.Html, "共1/", "页").ToInt(1);
                int currpage = 1;

                List<string> result1 = new List<string>();
                List<string> result2 = new List<string>();
                List<string> result3 = new List<string>();
                bool NeedGetTotal = true;
                do
                {
                    //养老金
                    Url = "http://221.215.38.136/grcx/work/m01/f1203/oldQuery.action?page_oldQuery=" + currpage;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    result1.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table/tbody/tr"));
                    if (NeedGetTotal)
                    {
                        NeedGetTotal = false;
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr");
                        Res.PersonalInsuranceTotal = CommonFun.GetMidStr(results[0], "个人缴费合计:", "单位划入账户合计:").ToTrim("&nbsp;").ToDecimal(0);
                        Res.InsuranceTotal = Res.PersonalInsuranceTotal + CommonFun.GetMidStr(results[0], "单位划入账户合计:", "社平划入合计:").ToTrim("&nbsp;").ToDecimal(0) + CommonFun.GetMidStr(results[0], "社平划入合计:", "</td>").ToTrim("&nbsp;").ToDecimal(0);
                    }
                    //医保
                    Url = "http://221.215.38.136/grcx/work/m01/f1204/medicalQuery.action?page_medicalQuery=" + currpage;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    result2.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table/tbody/tr"));

                    //失业保险
                    Url = "http://221.215.38.136/grcx/work/m01/f1205/unemployQuery.action?page_unemployQuery=" + currpage;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    result3.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table/tbody/tr"));
                    currpage++;
                }
                while (page >= currpage);

                #endregion

                #region 整理养老保险、医疗保险、失业保险数据
                //System.Collections.Hashtable timehash = new System.Collections.Hashtable();
                try
                {
                    for (int i = 0; i < result1.Count; i++)
                    {
                        var tdRow1 = HtmlParser.GetResultFromParser(result1[i], "//td");

                        if (tdRow1.Count != 10)
                        {
                            continue;
                        }

                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;

                        detailRes.PayTime = tdRow1[1];
                        detailRes.SocialInsuranceTime = tdRow1[2];
                        detailRes.CompanyName = tdRow1[3];
                        detailRes.SocialInsuranceBase = tdRow1[5].ToDecimal(0);
                        if (tdRow1[4] == "正常应缴")
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            //detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        }
                        else
                        {
                            detailRes.PaymentType = tdRow1[4];
                            //detailRes.PaymentFlag = tdRow1[9];
                        }
                        if (tdRow1[9] == "已实缴")
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        else
                            detailRes.PaymentFlag = tdRow1[9];

                        //养老
                        detailRes.PensionAmount = tdRow1[6].ToDecimal(0);
                        detailRes.CompanyPensionAmount = tdRow1[7].ToDecimal(0);
                        detailRes.NationPensionAmount = tdRow1[8].ToDecimal(0);


                        Res.Details.Add(detailRes);

                        //try
                        //{
                        //    timehash.Add(tdRow1[2], 1);
                        //}
                        //catch { }
                    }
                }
                catch { };

                try
                {
                    for (int i = 0; i < result2.Count; i++)
                    {
                        var tdRow2 = HtmlParser.GetResultFromParser(result2[i], "//td");
                        if (tdRow2.Count != 12)
                        {
                            continue;
                        }

                        if (tdRow2[4] == "正常应缴")
                            detailRes = Res.Details.Where(o => o.SocialInsuranceTime == tdRow2[2] && o.PaymentType == ServiceConsts.SocialSecurity_PaymentType_Normal).FirstOrDefault();
                        else
                            detailRes = Res.Details.Where(o => o.SocialInsuranceTime == tdRow2[2] && o.PaymentType == tdRow2[4]).FirstOrDefault();
                        if (detailRes != null)
                        {
                            //医疗
                            detailRes.MedicalAmount += tdRow2[6].ToDecimal(0);
                            detailRes.CompanyMedicalAmount += tdRow2[7].ToDecimal(0);
                            detailRes.EnterAccountMedicalAmount += tdRow2[9].ToDecimal(0);
                            detailRes.IllnessMedicalAmount += tdRow2[8].ToDecimal(0);
                        }
                        else
                        {
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.PayTime = tdRow2[1];
                            detailRes.SocialInsuranceTime = tdRow2[2];
                            detailRes.CompanyName = tdRow2[3];
                            detailRes.SocialInsuranceBase = tdRow2[5].ToDecimal(0);

                            if (tdRow2[4] == "正常应缴")
                            {
                                detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                                //detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            }
                            else
                            {
                                detailRes.PaymentType = tdRow2[4];
                                //detailRes.PaymentFlag = tdRow2[11];
                            }
                            if (tdRow2[11] == "已实缴")
                                detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            else
                                detailRes.PaymentFlag = tdRow2[11];

                            //医疗
                            detailRes.MedicalAmount = tdRow2[6].ToDecimal(0);
                            detailRes.CompanyMedicalAmount = tdRow2[7].ToDecimal(0);
                            detailRes.IllnessMedicalAmount = tdRow2[8].ToDecimal(0);
                            detailRes.EnterAccountMedicalAmount = tdRow2[9].ToDecimal(0);
                            

                            Res.Details.Add(detailRes);
                        }
                    }
                }
                catch { }

                try
                {
                    for (int i = 0; i < result3.Count; i++)
                    {
                        var tdRow3 = HtmlParser.GetResultFromParser(result3[i], "//td");
                        if (result3.Count != 8)
                        {
                            continue;
                        }

                        if (tdRow3[4] == "正常应缴")
                            detailRes = Res.Details.Where(o => o.SocialInsuranceTime == tdRow3[3] && o.PaymentType == ServiceConsts.SocialSecurity_PaymentType_Normal).FirstOrDefault();
                        else
                            detailRes = Res.Details.Where(o => o.SocialInsuranceTime == tdRow3[3] && o.PaymentType == tdRow3[4]).FirstOrDefault();

                        if (detailRes != null)
                        {
                            //失业
                            detailRes.UnemployAmount += tdRow3[6].ToDecimal(0);
                        }
                        else
                        {
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.CompanyName = tdRow3[1];
                            detailRes.PayTime = tdRow3[2];
                            detailRes.SocialInsuranceTime = tdRow3[3];
                            detailRes.SocialInsuranceBase = tdRow3[5].ToDecimal(0);

                            if (tdRow3[4] == "正常应缴")
                            {
                                detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                                //detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            }
                            else
                            {
                                detailRes.PaymentType = tdRow3[4];
                                //detailRes.PaymentFlag = tdRow3[9];
                            }
                            if (tdRow3[7] == "已实缴")
                                detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            else
                                detailRes.PaymentFlag = tdRow3[7];

                            // detailRes.PaymentType = tdRow3[4];
                            //detailRes.SocialInsuranceBase = tdRow3[5].ToDecimal(0);
                            //detailRes.PaymentFlag = tdRow3[11];

                            //失业
                            detailRes.UnemployAmount = tdRow3[6].ToDecimal(0);

                            Res.Details.Add(detailRes);
                        }
                    }
                }
                catch { }

                #endregion

                //if(Res.PaymentMonths != 0)
                //    Res.PaymentMonths = timehash.Count;
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
