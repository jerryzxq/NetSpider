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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.JL
{
    public class changchun : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.ccshbx.org.cn/";
        string socialCity = "jl_changchun";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "menu.jsp?ccn100=0102&fileName=employeeQueryLogin.jsp&Bid=010201&bz=";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录

                Url = baseUrl + "validate.jsp";
                postdata = String.Format("aac002={0}&employeePassword={1}&bz=1", socialReq.Identitycard, socialReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "menu.jsp?ccn100=0102&fileName=employeeQueryLogin.jsp&Bid=010201&bz=",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取基本信息

                Url = baseUrl + "employeeQuery.jsp?fileName=webQuery/employeeBaseinfoQuery.jsp&bz=1&jc10bz=1&nmbz=";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "employeeQuery.jsp?fileName=webQuery/employeeBaseinfoQuery.jsp&Bid=010201&jc10bz=1&bz=1&nmbz=",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr/td", "inner");
                if (results.Count <= 0 || string.IsNullOrEmpty(results[2]))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.EmployeeNo = results[2].Trim().Replace("&nbsp;", "");//编号
                Res.Name = results[4].Trim().Replace("&nbsp;", "");//姓名
                Res.IdentityCard = results[6].Trim().Replace("&nbsp;", "");//身份证号
                Res.BirthDate = results[8].Trim().Replace("&nbsp;", "");//出生日期
                Res.Sex = results[10].Trim().Replace("&nbsp;", "");//性别
                Res.Race = results[12].Trim().Replace("&nbsp;", "");//民族
                Res.WorkDate = results[16].Trim().Replace("&nbsp;", "");//参加工作时间
                Res.CompanyName = results[22].Trim().Replace("&nbsp;", "");//单位名称
                Res.EmployeeStatus = results[20].Trim().Replace("&nbsp;", "");//人员状态
                ////账户总额、个人账户总额
                //Url = baseUrl + "employeeQuery.jsp?fileName=webQuery/personAccountQuery.jsp&bz=1&jc10bz=1&nmbz=";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Referer = baseUrl + "employeeQuery.jsp?fileName=webQuery/employeeBaseinfoQuery.jsp&Bid=010201&jc10bz=1&bz=1&nmbz=",
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr/td/div", "inner");
                //if (results.Count > 0)
                //{
                //    Res.InsuranceTotal = results[10].ToTrim().ToDecimal(0);//账户总额
                //    Res.PersonalInsuranceTotal = results[12].Trim().ToDecimal(0);//个人账户总额
                //}
                #endregion

                #region 查询明细

                int pageIndex = 1;
                int pageCount = 0;

                do
                {
                    Url = String.Format("{0}employeeQuery.jsp?fileName=webQuery/personMonthFeeQuery.jsp&PageCount={1}&jc10bz=1&queryForword=next&select1=01&bz=1", baseUrl, pageIndex);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Referer = String.Format("{0}employeeQuery.jsp?fileName=webQuery/personMonthFeeQuery.jsp&PageCount={1}&jc10bz=1&queryForword=next&select1=01&bz=1", baseUrl, pageIndex),
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    pageCount = CommonFun.GetMidStr(httpResult.Html, "&nbsp;共", "页，当前第").ToInt(0);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr[@style='font-size:14px;']", "inner");
                    results.RemoveAt(0);
                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 7)
                            continue;

                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.PayTime = tdRow[0];
                        detailRes.SocialInsuranceTime = tdRow[0];
                        detailRes.SocialInsuranceBase = tdRow[3].ToDecimal(0);
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = tdRow[6] != "已实缴" ? ServiceConsts.SocialSecurity_PaymentFlag_Adjust : ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detailRes.PensionAmount = tdRow[4].ToDecimal(0);
                        detailRes.CompanyMedicalAmount = tdRow[5].ToDecimal(0);
                        Res.Details.Add(detailRes);
                        PaymentMonths++;
                    }
                    pageIndex++;
                }
                while (pageIndex <= pageCount);


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
