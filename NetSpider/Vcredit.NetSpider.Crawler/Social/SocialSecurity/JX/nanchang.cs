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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.JX
{
    public class nanchang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://hrss.nc.gov.cn/";
        string socialCity = "jx_nanchang";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "login.jsp?returnUrl=http://hrss.nc.gov.cn/insure/query.jsp&message=true&location=http%3A%2F%2Fhrss.nc.gov.cn%2Finsure%2Fquery.jsp&res_system=%2Fskin%2Fgrand&locale=zh_CN";
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
                #region 登录

                Url = baseUrl + "login.jsp";
                // postdata = String.Format("username={0}&password={1}&returnUrl=%2Finsure%2Fquery.jsp", socialReq.Username.ToUrlEncode(), socialReq.Password.ToUrlEncode());
                postdata = String.Format("username={0}&password={1}&passwordstrength=3&x=23&y=14&returnUrl=http%3A%2F%2Fhrss.nc.gov.cn%2Finsure%2Fquery.jsp", socialReq.Username.ToUrlEncode(), socialReq.Password.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Allowautoredirect = false,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.Html.IndexOf("changepwd") > -1 || httpResult.RedirectUrl.IndexOf("changepwd") > -1)
                {
                    Res.StatusDescription = "您的密码过于简单,请修改登陆密码后重试！";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                //请求失败后返回
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "AlertError);alert(\"", "\");");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (httpResult.StatusCode != HttpStatusCode.Found)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                Url = httpResult.RedirectUrl;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "hrss.nc.gov.cn",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='chnl_list fr']/iframe", "src");
                if (results.Count < 1)
                {
                    Res.StatusDescription = "获取基本信息失败";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (string.IsNullOrEmpty(results[0]))
                {
                    Res.StatusDescription = "获取基本信息失败";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                #endregion

                #region 获取基本信息

                Url = results[0];
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode == HttpStatusCode.Forbidden)
                {
                    Res.StatusDescription = "服务器内部出错,请稍后再试！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='ftable']/tr/td[@class='fcontent']", "inner");
                if (results.Count <= 0 || string.IsNullOrEmpty(results[0]))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.Name = socialReq.Name;
                Res.IdentityCard = socialReq.Identitycard;
                Res.Loginname = socialReq.Username;
                //Res.EmployeeNo = results[0].Trim().Replace("&nbsp;", "");//编号
                //Res.Name = results[3].Trim().Replace("&nbsp;", "");//姓名
                //Res.IdentityCard = results[2].Trim().Replace("&nbsp;", "");//身份证号
                //Res.CompanyNo = results[11].Trim().Replace("&nbsp;", "");//单位编号
                //Res.CompanyName = results[12].Trim().Replace("&nbsp;", "");//单位名称
                //Res.EmployeeStatus = results[14].Trim().Replace("&nbsp;", "");//人员状态
                //// Res.Payment_State = results[13].Trim().Replace("&nbsp;", "");//缴费状态
                //Res.PaymentMonths = results[17].Trim().Replace("&nbsp;", "").ToInt(0);//养老保险缴费月数
                //Res.SocialInsuranceBase = results[15].Trim().Replace("&nbsp;", "").ToDecimal(0);//养老缴费基数
                //Res.PersonalInsuranceTotal = results[16].Trim().Replace("&nbsp;", "").ToDecimal(0);//个人账户总额(养老)
                //Res.InsuranceTotal = results[10].Trim().Replace("&nbsp;", "").ToDecimal(0) + results[16].Trim().Replace("&nbsp;", "").ToDecimal(0);//账户总额（医疗+养老）
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='ftable']/tr/td", "", true);
                for (int i = 0; i < results.Count - 1; i++)
                {
                    if (results[i].Contains("个人编号"))
                    {
                        Res.EmployeeNo = results[i + 1].Trim().Replace("&nbsp;", "");//编号
                    }
                    else if (results[i].Contains("身份证号"))
                    {
                        Res.IdentityCard = results[i + 1].Trim().Replace("&nbsp;", "");//身份证号
                    }
                    else if (results[i].Contains("姓名"))
                    {
                        Res.Name = results[i + 1].Trim().Replace("&nbsp;", "");//姓名
                    }
                    else if (results[i].Contains("养老保险单位编号"))
                    {
                        Res.CompanyNo = results[i + 1].Trim().Replace("&nbsp;", "");//单位编号
                    }
                    else if (results[i].Contains("养老保险单位名称"))
                    {
                        Res.CompanyName = results[i + 1].Trim().Replace("&nbsp;", "");//单位名称
                    }
                    else if (results[i].Contains("人员状态"))
                    {
                        Res.EmployeeStatus = results[i + 1].Trim().Replace("&nbsp;", "");//人员状态
                    }
                    else if (results[i].Contains("养老保险参保状态"))
                    {
                        Res.Payment_State = results[i + 1].Trim().Replace("&nbsp;", "");//缴费状态
                    }
                    else if (results[i].Contains("养老保险缴费基数"))
                    {
                        Res.SocialInsuranceBase = results[i + 1].Trim().Replace("&nbsp;", "").ToDecimal(0);//养老缴费基数
                    }
                    else if (results[i].Contains("养老保险账户金额") || results[i].Contains("医疗保险账户余额"))
                    {
                        Res.InsuranceTotal += results[i + 1].Trim().Replace("&nbsp;", "").ToDecimal(0);//账户总额（医疗+养老）
                    }
                    else if (results[i].Contains("养老保险缴费月数"))
                    {
                        Res.PaymentMonths = results[i + 1].Trim().Replace("&nbsp;", "").ToInt(0);//养老保险缴费月数
                    }
                }
                #endregion

                #region 查询明细
                #region 养老保险

                List<string> result1 = new List<string>();
                int pageIndex = 1;
                int pageCount;
                string detailsUrl = HtmlParser.GetResultFromParser(httpResult.Html, "//a", "href")[0];
                detailsUrl = "http://218.204.132.6:9090" + detailsUrl;
                do
                {
                    Url = String.Format("{0}&pageNo={1}", detailsUrl, pageIndex);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    pageCount = CommonFun.GetMidStr(CommonFun.GetMidStr(httpResult.Html, "条记录", "页 &nbsp;"), "/", "").ToInt(0);
                    result1.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='ftable2']/tr[position()>1]", "inner"));
                    pageIndex++;
                }
                while (pageIndex <= pageCount);

                #endregion
                #region 医疗保险

                List<string> result2 = new List<string>();
                pageIndex = 1;
                detailsUrl = "http://218.204.132.6:9090" + HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='chnl_list_t g_t_c']/ul/li[2]/a", "href")[0]; ;
                do
                {
                    Url = String.Format("{0}&pageNo={1}", detailsUrl, pageIndex);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    pageCount = CommonFun.GetMidStr(CommonFun.GetMidStr(httpResult.Html, "条记录", "页 &nbsp;"), "/", "").ToInt(0);
                    result2.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='ftable2']/tr[position()>1]", "inner"));
                    pageIndex++;
                }
                while (pageIndex <= pageCount);

                #endregion
                #region 整理养老、医疗保险

                foreach (string item in result1)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                    if (tdRow.Count != 8)
                        continue;

                    if (tdRow[2] == "正常应缴" && tdRow[3] == "已实缴")
                        detailRes = Res.Details.Where(o => o.SocialInsuranceTime == tdRow[1] && o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal).FirstOrDefault();
                    else
                        detailRes = Res.Details.Where(o => o.SocialInsuranceTime == tdRow[1] && o.PaymentFlag == tdRow[2]).FirstOrDefault();
                    if (detailRes == null)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;

                        detailRes.SocialInsuranceTime = tdRow[1];
                        detailRes.SocialInsuranceBase = tdRow[4].ToDecimal(0);
                        if (tdRow[2] == "正常应缴" && tdRow[3] == "已实缴")
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        }
                        else
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                            detailRes.PaymentFlag = tdRow[2];
                        }
                    }

                    detailRes.PensionAmount += tdRow[6].ToDecimal(0);
                    detailRes.CompanyPensionAmount += tdRow[5].ToDecimal(0);

                    Res.Details.Add(detailRes);
                }

                foreach (string item in result2)
                {
                    var tdRow2 = HtmlParser.GetResultFromParser(item, "//td");
                    if (tdRow2.Count != 8)
                        continue;

                    if (tdRow2[2] == "正常应缴" && tdRow2[3] == "已实缴")
                        detailRes = Res.Details.Where(o => o.SocialInsuranceTime == tdRow2[1] && o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal).FirstOrDefault();
                    else
                        detailRes = Res.Details.Where(o => o.SocialInsuranceTime == tdRow2[1] && o.PaymentFlag == tdRow2[2]).FirstOrDefault();

                    if (detailRes != null)
                    {
                        //医疗
                        detailRes.MedicalAmount = tdRow2[6].ToDecimal(0);
                        detailRes.CompanyMedicalAmount = tdRow2[5].ToDecimal(0);
                        detailRes.EnterAccountMedicalAmount = tdRow2[7].ToDecimal(0);
                    }
                    else
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.SocialInsuranceTime = tdRow2[1];
                        detailRes.SocialInsuranceBase = tdRow2[4].ToDecimal(0);
                        if (tdRow2[2] == "正常应缴" && tdRow2[3] == "已实缴")
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        }
                        else
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                            detailRes.PaymentFlag = tdRow2[2];
                        }
                        //医疗
                        detailRes.MedicalAmount = tdRow2[6].ToDecimal(0);
                        detailRes.CompanyMedicalAmount = tdRow2[5].ToDecimal(0);
                        detailRes.EnterAccountMedicalAmount = tdRow2[7].ToDecimal(0);
                        Res.Details.Add(detailRes);
                    }

                }
                #endregion
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
