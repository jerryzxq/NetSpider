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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.HB
{
    public class hubeisheng : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://59.175.218.195:8888/hbsi/";
        string socialCity = "hb_hubeisheng";
        string reqCode = string.Empty;
        string login = string.Empty;
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "login.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
                var result = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@type='hidden']", "value");
                if (result.Count > 0)
                {
                    reqCode = result[0];
                }

                result = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='login']", "value");

                if (result.Count > 0)
                {
                    login = result[0];
                }

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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Name.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = baseUrl + "login.do";
                postdata = string.Format("reqCode={0}&dto%28aac003%29={1}&dto%28aac002%29={2}&dto%28yae096%29={3}&login={4}", "login", socialReq.Name.ToUrlEncode(Encoding.GetEncoding("GB2312")), socialReq.Identitycard, socialReq.Password, login.ToUrlEncode(Encoding.GetEncoding("GB2312")));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Allowautoredirect = false,
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var errormsg = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "');</script>");
                if (!errormsg.IsEmpty())
                {
                    Res.StatusDescription = errormsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 第二步， 获取基本信息
                Url = baseUrl + "dzzw/internet/personBaseInfo.do";
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


                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dto(aac001)']", "value");
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];//个人编号
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dto(aac003)']", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dto(aac002)']", "value");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dto(aac004)']", "value");
                if (results.Count > 0)
                {
                    Res.Sex = results[0];//性别
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dto(aac005)']", "value");
                if (results.Count > 0)
                {
                    Res.Race = results[0];//民族
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dto(aac008)']", "value");
                if (results.Count > 0)
                {
                    Res.EmployeeStatus = results[0];//职工状态
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dto(aab001)']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];//单位编号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dto(aab004)']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];//单位名称
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dto(aac006)']", "value");
                if (results.Count > 0)
                {
                    Res.BirthDate = results[0];//出生日期
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dto(aac007)']", "value");
                if (results.Count > 0)
                {
                    Res.WorkDate = results[0];//参加工作时间
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dto(aae005)']", "value");
                if (results.Count > 0)
                {
                    Res.Phone = results[0];//联系电话
                }

                #endregion


                #region 第三步，查询明细 （只有养老保险）
                //获取养老保险详单页数
                Url = baseUrl + "dzzw/internet/ic01Query.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                int pagecount = 0;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@class='TREELABEL']/select/option", "");
                if (results.Count > 0)
                {
                    pagecount = results.Count;
                }

                //查询养老保险
                for (int i = 1; i <= pagecount; i++)
                {
                    Url = baseUrl + "dzzw/internet/ic01Query.do";

                    //reqCode=jumpPage&dto%28startaae002%29=&calmois=0&dto%28endaae002%29=&dto%28page%29=1
                    postdata = string.Format("reqCode=jumpPage&dto%28startaae002%29=&calmois=0&dto%28endaae002%29=&dto%28page%29={0}", i);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "Post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//tr[@height='25']", "");
                    foreach (var item in results)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 8)
                        {
                            continue;
                        }
                        detailRes.PayTime = tdRow[0];  //缴费年月
                        detailRes.SocialInsuranceTime = tdRow[7]; //到账日期
                        if (tdRow[2].Contains("正常应缴"))
                        {
                            detailRes.PensionAmount = tdRow[3].ToDecimal(0) / 2; //个人缴费
                            detailRes.CompanyPensionAmount = tdRow[4].ToDecimal(0) / 2; //公司缴费
                            detailRes.PaymentType = tdRow[2];
                            detailRes.PaymentFlag = tdRow[6];
                            PaymentMonths++;
                        }
                        else
                        {
                            detailRes.PensionAmount = tdRow[3].ToDecimal(0) / 2; //个人缴费
                            detailRes.CompanyPensionAmount = tdRow[4].ToDecimal(0) / 2; //公司缴费
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                        }
                        Res.Details.Add(detailRes);
                    }
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
