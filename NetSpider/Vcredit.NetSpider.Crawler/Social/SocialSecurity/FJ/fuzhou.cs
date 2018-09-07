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
using System.Collections;
using Vcredit.NetSpider.DataAccess.Cache;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.FJ
{
    public class fuzhou : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "";
        string socialCity = "fj_fuzhou";
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
                Url = "http://www.fzshbx.org/img.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
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
                Url = "http://www.fzshbx.org/sb/user/userLogin.do";
                postdata = String.Format("sbUser.username={0}&sbUser.password={1}&sbUser.yzm={2}", socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
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
                string message = CommonFun.GetMidStr(httpResult.Html, "<script type=\"text/javascript\">alert('", "');location.href");
                if (!message.IsEmpty())
                {
                    Res.StatusDescription = message;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='userRegedit']", "");
                if (results.Count > 0)
                {
                    Res.StatusDescription = "用户不存在，请注册";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，个人基本信息
                Url = "http://www.fzshbx.org/xxcx/grjbxxcx.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                message = CommonFun.GetMidStr(httpResult.Html, "<script type=\"text/javascript\">alert('", "');location.href");
                if (!message.IsEmpty())
                {
                    Res.StatusDescription = message;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='xinxi']/table/tr", "inner", true);
                Res.EmployeeNo = HtmlParser.GetResultFromParser(results[1], "//td")[1];//编号
                Res.Name = HtmlParser.GetResultFromParser(results[0], "//td")[3];//姓名
                Res.IdentityCard = HtmlParser.GetResultFromParser(results[0], "//td")[1];//身份证号
                Res.EmployeeStatus = HtmlParser.GetResultFromParser(results[1], "//td")[3];//状态
                Res.CompanyName = HtmlParser.GetResultFromParser(results[4], "//td")[1];//公司名称
                Res.Phone = HtmlParser.GetResultFromParser(results[3], "//td")[1];//电话
                Res.ZipCode = HtmlParser.GetResultFromParser(results[3], "//td")[3];//邮编
                Res.Address = HtmlParser.GetResultFromParser(results[6], "//td")[1];//地址
                //最后缴费基数查询
                Url = "http://www.fzshbx.org/xxcx/zhjfjscx.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html,"//div[@id='xinxi']/table/tr/td","");
                if (results.Count>13)
                {
                    Res.SocialInsuranceBase = results[5].ToDecimal(0);//缴费基数
                }
                #endregion

                #region 第三步，养老

                int pageIndex = 0;
                int pageCount = 0;
                do
                {
                    pageIndex++;
                    Url = "http://www.fzshbx.org/xxcx/grjfmxcx.do";
                    postdata = string.Format("ac10a.starttime=&ac10a.endtime=&temp=&page={0}&sppagetotal=10&length=10", pageIndex);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    pageCount = CommonFun.GetMidStr(httpResult.Html, "行，", " 页").ToInt(0);
                    if (Res.InsuranceTotal == 0)
                        Res.InsuranceTotal = CommonFun.GetMidStr(CommonFun.GetMidStr(httpResult.Html, "总缴费金额:", ""), "", "</td>").ToDecimal(0);
                    if (Res.PaymentMonths == 0)
                        Res.PaymentMonths = CommonFun.GetMidStr(CommonFun.GetMidStr(httpResult.Html, "总缴费月数：", ""), "", "</td>").ToInt(0);

                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='grid']/tbody/tr", "inner", true);

                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 12)
                        {
                            continue;
                        }

                        for (int i = 0; i < tdRow[5].ToInt(0); i++)
                        {
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.Name = Res.Name;
                            detailRes.PayTime = tdRow[0];
                            detailRes.SocialInsuranceTime =DateTime.ParseExact(tdRow[2],"yyyyMM",null).AddMonths(i).ToString(Consts.DateFormatString7);
                            detailRes.SocialInsuranceBase = (tdRow[6].ToDecimal(0)) / tdRow[5].ToDecimal(0);
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            detailRes.PensionAmount = (tdRow[7].ToDecimal(0)) / tdRow[5].ToDecimal(0);
                            detailRes.CompanyPensionAmount = (tdRow[9].ToDecimal(0)) / tdRow[5].ToDecimal(0);
                            detailRes.CompanyName = tdRow[11];
                            Res.Details.Add(detailRes);
                        }
                    }
                }
                while (pageIndex < pageCount);

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
