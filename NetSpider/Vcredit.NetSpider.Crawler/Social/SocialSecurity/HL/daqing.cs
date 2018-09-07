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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.HL
{
    public class daqing : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://dq12333.gov.cn/";
        string socialCity = "hl_daqing";
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
                    URL = baseUrl,
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录
                Url = baseUrl + "gerenshebao!getlist.action";
                postdata = String.Format("code=1&pid=1&shebaohao={0}&name={1}", socialReq.Identitycard, socialReq.Name.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取基本信息



                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='threemain2']/table[@class='tab2'][1]/tr/td", "inner");
                if (results.Count <= 0)
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }



                Res.IdentityCard = results[1];//身份证号
                Res.Name = results[2];//姓名
                Res.Sex = results[3];//性别
                Res.CompanyName = results[4];//单位名称
                Res.EmployeeNo = results[0]; //员工编号
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='threemain2']/table[@class='tab2'][2]/tr/td", "inner");

                if (!string.IsNullOrEmpty(results[3]))
                {
                    Res.InsuranceTotal = results[3].ToDecimal(0); //养老账户总额
                }
                if (!string.IsNullOrEmpty(results[2]))
                {
                    Res.PaymentMonths = int.Parse(results[2]);
                }

                if (!string.IsNullOrEmpty(results[1]))
                {
                    Res.PersonalInsuranceTotal = results[1].ToDecimal(0); //个人账户总额
                }


                #endregion

                #region 查询明细
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='threemain2']/table[@class='tab2'][5]/tr[position()>1]", "inner");
                foreach (var item in results)
                {
                    SocialSecurityDetailQueryRes detail = new SocialSecurityDetailQueryRes();
                    List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow.Count != 3)
                    {
                        continue;
                    }
                    detail.PayTime = tdRow[1];
                    detail.SocialInsuranceTime = tdRow[1];
                    detail.EnterAccountMedicalAmount = tdRow[2].ToDecimal(0);
                    detail.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    detail.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    Res.Details.Add(detail);
                    PaymentMonths++;
                }


                #endregion

                Res.PaymentMonths =Res.PaymentMonths + PaymentMonths;
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
