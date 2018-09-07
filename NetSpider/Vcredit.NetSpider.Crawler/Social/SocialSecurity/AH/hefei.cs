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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.AH
{
    public class hefei : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://60.173.202.219/wssb/";
        string socialCity = "ah_hefei";
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
                Url = baseUrl + "grlogo.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "imagecoder";
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
            SocialSecurityDetailQueryRes detail = null;
            Res.SocialSecurityCity = socialCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                Url = baseUrl + "admin/grpass.jsp";
                postdata = String.Format("xingm={0}&AtAction=Logon&sfz={1}&sbh={2}&verify={3}", socialReq.Name.ToUrlEncode(Encoding.GetEncoding("gb2312")), socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "grlogo.jsp",
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
                if (httpResult.Html.IndexOf("window.location.replace('../grlogo.jsp')") != -1)
                {
                    string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "')");
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 获取基本信息
                Url = baseUrl + "admin/000001/Grwxcb.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    Referer = baseUrl + "admin/grcx.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);


                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='user_id']", "value");
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];//编号
                }
                else
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='user_logname']", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='user_password']", "value");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='xingbie']", "value");
                if (results.Count > 0)
                {
                    Res.Sex = results[0];//性别
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/table[2]/tr[2]/td[2]/table/tr[3]/td[4]/input", "value");
                if (results.Count > 0)
                {
                    Res.WorkDate = results[0];//参加工作时间
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='user_yljs']", "value");
                if (results.Count > 0)
                {
                    Res.SocialInsuranceBase = results[0].Substring(0, results[0].Length - 1).ToDecimal(0);//缴费基数
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/table[2]/tr[2]/td[2]/table/tr[9]/td[4]/input", "value");
                if (results.Count > 0)
                {
                    Res.EmployeeStatus = results[0];//工作状态
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='user_logname']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[2];//单位名称
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='user_logname']", "value");
                if (results.Count > 0)
                {
                    Res.PaymentMonths = results[4].Substring(0, results[4].Length - 2).ToInt(0);//养老视同缴费年限
                }
                //养老保险参保状态
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='user_yl']", "value");
                if (results.Count > 0)
                {
                    Res.SpecialPaymentType = "养老保险参保状态:"+results[0]+";";
                }
                //失业参保状态
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='user_sy']", "value");
                if (results.Count > 0)
                {
                    Res.SpecialPaymentType += "失业参保状态:" + results[0] + ";"; 
                }
                //医疗参保状态
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='user_yy']", "value");
                if (results.Count > 0)
                {
                    Res.SpecialPaymentType += "医疗参保状态:" + results[0] + ";"; 
                }
                //工伤参保状态
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='user_gs']", "value");
                if (results.Count > 0)
                {
                    Res.SpecialPaymentType += "工伤参保状态:" + results[0] + ";"; 
                }
                //生育参保状态
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='user_syu']", "value");
                if (results.Count > 0)
                {
                    Res.SpecialPaymentType += "生育参保状态:" + results[0] + " "; 
                }
                #endregion

                #region 明细

                DateTime date = DateTime.Now;
                List<WuHuSocia> sociaresults = new List<WuHuSocia>();
                do
                {
                    Url = String.Format("{0}admin/000001/SearchGrzmmxSubmit.jsp?user_id=1&user_name={1}", baseUrl, date.ToString("yyyy"));
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                    Url = baseUrl + "admin/000001/Grzmmx.jsp";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr[@style='CURSOR: hand']", "inner");
                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 10)
                        {
                            continue;
                        }

                        detail = new SocialSecurityDetailQueryRes();
                        detail.PayTime = tdRow[1];
                        detail.SocialInsuranceTime = tdRow[1];
                        detail.SocialInsuranceBase = tdRow[4].ToDecimal(0);
                        detail.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detail.PaymentFlag = tdRow[9] != "已到帐" ? ServiceConsts.SocialSecurity_PaymentFlag_Adjust : ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detail.PensionAmount = tdRow[7].ToDecimal(0);
                        detail.CompanyPensionAmount = tdRow[6].ToDecimal(0);
                        detail.CompanyName = CommonFun.GetMidStr(item, "title='", "'");
                        Res.Details.Add(detail);
                    }
                    date = date.AddYears(-1);


                }
                while (date > DateTime.Now.AddYears(-5));

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
