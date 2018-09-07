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
    public class yancheng : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.jsychrss.gov.cn/";
        string socialCity = "js_yancheng";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            string Url = string.Empty;
            Res.Token = token;
            try
            {
                Url = baseUrl + "xxcx/csi.grzhcx.php?nm_css=10007";
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
            string personcode = string.Empty;
            string errorMsg = string.Empty;
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
                Url = baseUrl + "xxcx/csi.grzhcx.php";
                postdata = String.Format("AAC002={0}&AAC003={1}&sub=1&Query=%B2%E9+%D1%AF", socialReq.Identitycard, socialReq.Name.ToUrlEncode(Encoding.GetEncoding("GB2312")));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Encoding = Encoding.GetEncoding("GB2312"),
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

                //用户名或密码错误
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "人代码：</font><a href=\"csi.echo_grdata.php?grdm=", "\"");
                if (string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = "姓名或身份证号错误";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                personcode = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@cellpadding='5' ]/tr[1]/td/font[2]/a[2]", "inner")[0];
                #endregion

                #region 获取基本信息

                //选择养老保险查询
                Url = baseUrl + "xxcx/csi.echo_grdata.php?grdm="+personcode;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Get",
                    Encoding = Encoding.GetEncoding("GB2312"),
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


                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@cellspacing='1']/tr/td[@align='center']", "inner");
                Res.Name = HtmlParser.GetResultFromParser(results[1], "//a", "inner")[0] ;   //姓名
                Res.EmployeeNo = HtmlParser.GetResultFromParser(results[0], "//a", "inner")[0];   //个人编号
                Res.IdentityCard = results[3];   //身份证号
                Res.CompanyName = results[2];   //单位名称
                Res.Sex = results[4];   //性别
                Res.Race = results[5];  //民族
                Res.BirthDate = results[7];  //出生年月
                Res.WorkDate = results[8];   //参加工作日期
                #endregion

                #region 查询明细(暂无明细信息)
                 //results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@valign='top']/table[@height='28']/tr[position()>1]", "inner");
                 //foreach (var item in results)
                 //{
                 //    SocialSecurityDetailQueryRes detail = new SocialSecurityDetailQueryRes();
                 //    List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                 //    if (tdRow.Count != 6)
                 //    {
                 //        continue;
                 //    }

                 //    detail.PayTime = tdRow[0];
                 //    detail.SocialInsuranceTime = tdRow[1];
                 //    detail.PensionAmount = tdRow[3].ToDecimal(0);
                 //    detail.YearPaymentMonths =int.Parse(tdRow[4]);
                 //    detail.SocialInsuranceBase = tdRow[5].ToDecimal(0);
                 //    detail.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                 //    detail.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                 //    Res.Details.Add(detail);
                 //    PaymentMonths += detail.YearPaymentMonths;
                 //}
                

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
