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
    public class haiyan : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        String cookiestr = string.Empty;
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.hyshbx.gov.cn/";
        string socialCity = "zj_haiyan";
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
                Url = baseUrl + "errorlogin.jspx?returnUrl=/sungov/biz_sbcxMain.jspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "www.hyshbx.gov.cn",
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookiestr = CommonFun.GetCookieStringNew(cookiestr, httpResult.Cookie);

                Url = baseUrl + "captcha.svl";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "errorlogin.jspx?returnUrl=/sungov/biz_sbcxMain.jspx",
                    ResultType = ResultType.Byte,
                    Host = "www.hyshbx.gov.cn",
                    Cookie = cookiestr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookiestr = CommonFun.GetCookieStringNew(cookiestr, httpResult.Cookie);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookiestr);
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
                    cookiestr = CacheHelper.GetCache(socialReq.Token).ToString();
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Citizencard.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 校验验证码
                Url = baseUrl + "sungov/findpwd_captha.do";
                postdata = String.Format("captcha={0}", socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "errorlogin.jspx?returnUrl=/sungov/biz_sbcxMain.jspx",
                    Host = "www.hyshbx.gov.cn",
                    Cookie = cookiestr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (httpResult.Html == "false")
                {
                    Res.StatusDescription = "验证码错误！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookiestr = CommonFun.GetCookieStringNew(cookiestr, httpResult.Cookie);
                #endregion

                #region 登录

                Url = baseUrl + "sungov/login.do";
                postdata = String.Format("returnUrl=%2Fsungov%2Fbiz_sbcxMain.jspx&logintype=2&password={1}&username={0}", socialReq.Username, socialReq.Password.ToBase64());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "errorlogin.jspx?returnUrl=/sungov/biz_sbcxMain.jspx",
                    Cookie = cookiestr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//script");
                if (results.Count > 1)
                {
                    string msg = CommonFun.GetMidStr(results.LastOrDefault(), "alert(\"", "\"");
                    if (!msg.IsEmpty())
                    {
                        Res.StatusDescription = msg;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                cookiestr = CommonFun.GetCookieStringNew(cookiestr, httpResult.Cookie);

                //Url = httpResult.RedirectUrl;
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Referer = baseUrl + "errorlogin.jspx?returnUrl=/sungov/biz_sbcxMain.jspx",
                //    Cookie = cookiestr,
                //    ResultCookieType = ResultCookieType.String
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                //    return Res;
                //}
                cookiestr = CommonFun.GetCookieStringNew(cookiestr, httpResult.Cookie);
                #endregion

                #region 获取基本信息

                Url = baseUrl + "sungov/biz_sbcxMainSearch.jspx";
                postdata = String.Format("numType=AAC162&cardNum={0}", socialReq.Citizencard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = httpResult.ResponseUri,
                    Cookie = cookiestr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tab05']/tr/td", "inner");
                if (results.Count <= 0 || string.IsNullOrEmpty(results[5]))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //cookiestr = CommonFun.GetCookieStringNew(cookiestr, httpResult.Cookie);

                Res.EmployeeNo = results[5].Trim();//编号
                Res.Name = results[1].Trim();//姓名
                Res.BirthDate = results[9].Trim();//出生日期
                Res.Sex = results[7].Trim();//性别
                Res.IdentityCard = results[3].Trim();//身份证号
                Res.Race = results[17];//民族
                Res.CompanyName = results[11].Trim();//单位名称
                Res.EmployeeStatus = results[21].Trim();//人员状态
                Res.WorkDate = results[15].Trim();//参加工作时间

                #endregion

                #region 参保状态
                Url = baseUrl + "insuranceInfoQueryP.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Cookie = cookiestr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tab05']/tr");
                foreach (string item in results)
                {
                    List<string> tdrow = HtmlParser.GetResultFromParser(item, "td");
                    if (tdrow.Count == 3)
                    {
                        Res.SpecialPaymentType += string.Format("[{0}:{1}]", tdrow[0], tdrow[2]);
                    }
                }
                #endregion

                #region 查询明细

                List<string> baoxiantype = new List<string> { "1", "4" };//养老、医疗
                for (int i = 0; i < baoxiantype.Count; i++)
                {
                    int pageIndex = 1;
                    int pageCount = 0;
                    do
                    {
                        Url = String.Format("{0}paymentInfoQueryP.do?pageNo={1}&insurance={2}", baseUrl, pageIndex, baoxiantype[i]);
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            //Referer = baseUrl + "paymentInfoQueryP.do?insurance=1",
                            Cookie = cookiestr,
                            ResultCookieType = ResultCookieType.String
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        pageCount = CommonFun.GetMidStr(CommonFun.GetMidStr(httpResult.Html, "页次：<span>", "</span>页</td>"), "/", "").ToInt(0);
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tab05']/tr", "inner");
                        results.RemoveAt(0);
                        foreach (string item in results)
                        {
                            var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                            if (tdRow.Count != 6)
                            {
                                continue;
                            }
                            string SocialInsuranceTime = tdRow[1];
                            detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                            bool NeedAdd = false;
                            if (detailRes == null)
                            {
                                detailRes = new SocialSecurityDetailQueryRes();
                                detailRes.PayTime = tdRow[1];
                                detailRes.SocialInsuranceTime = tdRow[1];
                                detailRes.SocialInsuranceBase = tdRow[4].ToDecimal(0);
                                detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                                detailRes.PaymentFlag = tdRow[5] != "已实缴" ? tdRow[5] : ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                                NeedAdd = true;
                            }
                            switch (i)
                            {
                                case 0:
                                    detailRes.PensionAmount = tdRow[3].ToDecimal(0);
                                    detailRes.CompanyPensionAmount = tdRow[2].ToDecimal(0);
                                    break;
                                case 1:
                                    detailRes.MedicalAmount = tdRow[3].ToDecimal(0);
                                    detailRes.CompanyMedicalAmount = tdRow[2].ToDecimal(0);
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
