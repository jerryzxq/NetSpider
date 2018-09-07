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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.SH
{
    public class shanghailaodong : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.12333sh.gov.cn/";
        string socialCity = "sh_shanghailaodong";
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
                //Url = "http://www.12333sh.gov.cn/ztg/htcx.jsp";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                //    return Res;
                //}
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "ztg/Bmblist.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    //Accept = "Accept:image/webp,image/*,*/*;q=0.8",
                    ResultType = ResultType.Byte,
                    //Referer = "http://www.12333sh.gov.cn/ztg/htcx.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
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
            //int PaymentMonths = 0;
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

                #region 登录上海劳动备案
                if (socialReq.LoginType == "1")
                {
                    Url = baseUrl + "ztg/htcx.jsp?act=checkperson";
                    postdata = "radiobutton=htcx.jsp&zjhm={0}&xm={1}&sj_mima1={2}";
                }
                if (socialReq.LoginType == "2")
                {
                    Url = baseUrl + "ztg/htcx_outlabor.jsp?act=checkperson";
                    postdata = "radiobutton=htcx_outlabor.jsp&zjhm={0}&xm={1}&sj_mima1={2}";
                }
                postdata = string.Format(postdata, socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Allowautoredirect = false,
                    CookieCollection = cookies,
                    Host = "www.12333sh.gov.cn",
                    Referer = "http://www.12333sh.gov.cn/ztg/htcx.jsp",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode == HttpStatusCode.Found)
                {
                    Res.StatusDescription = "验证码错误！";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@class='unnamed2']");
                if (results.Count > 1)
                {
                    results = HtmlParser.GetResultFromParser(results[1], "//div");
                    if (results.Count > 0)
                    {
                        if (results[0].StartsWith("<"))
                            Res.StatusDescription = results[0].Substring(results[0].IndexOf('>') + 1, results[0].Length - results[0].IndexOf('>') - 1);
                        else
                            Res.StatusDescription = results[0];
                    }
                    else
                    {
                        Res.StatusDescription = "无信息！";
                    }
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 查询劳动备案信息
                Res.Name = socialReq.Name;
                Res.IdentityCard = socialReq.Identitycard;

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@checked]", "value");
                if (results.Count > 1)
                {
                    switch (results[1])
                    {
                        case "1":
                            Res.EmployeeStatus = "未签";
                            break;
                        case "2":
                            Res.EmployeeStatus = "初签";
                            break;
                        case "3":
                            Res.EmployeeStatus = "续签";
                            break;
                    }
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr");
                List<string> tempresults = new List<string>();
                foreach (string item in results)
                {
                    tempresults = HtmlParser.GetResultFromParser(item, "//td");
                    if (tempresults.Count == 2)
                    {
                        if (tempresults[0].Contains("本次劳动合同起止日期"))
                        {
                            Res.EmployeeStatus += "(本次劳动合同起止日期:" + CommonFun.ClearFlag(tempresults[1]).Replace("&nbsp;", "") + ")";
                            continue;
                        }
                        if (tempresults[0].Contains("所属单位"))
                        {
                            Res.CompanyName = CommonFun.ClearFlag(tempresults[1]).Replace("&nbsp;", "");
                            continue;
                        }
                        if (tempresults[0].Contains("当前工作起止日期"))
                        {
                            Res.WorkDate = CommonFun.ClearFlag(tempresults[1]).Replace("&nbsp;", "");
                            continue;
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
