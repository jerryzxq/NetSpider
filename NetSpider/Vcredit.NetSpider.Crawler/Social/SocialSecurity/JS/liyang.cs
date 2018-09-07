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
    public class liyang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.lyhrss.gov.cn:7001/";
        string socialCity = "js_liyang";
        #endregion

        #region 私有变量
        private class PersonInfo
        {
            public string aac004 { get; set; }
            public string aab069 { get; set; }
            public string aab001 { get; set; }
            public string aac003 { get; set; }
            public string aac006 { get; set; }
            public string aac001 { get; set; }
            public string aac002 { get; set; }
        }

        private class DetailInfo
        {
            public string aae003 { get; set; }
            public string aac003 { get; set; }
            public string aae022 { get; set; }
            public string ROWNUM_ { get; set; }
            public string aae020 { get; set; }
            public string aae180 { get; set; }
            public string aac001 { get; set; }
        }
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "lyld/index.jsp";
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
            int PaymentMonths = 0;
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

                #region 第一步，登录

                Url = baseUrl + "lyld/LoginAction?act=Login";
                postdata = String.Format("u={0}&p={1}", socialReq.Username, socialReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "lyld/index.jsp",
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
                if (httpResult.Html.StartsWith("<SCRIPT"))
                {
                    Res.StatusDescription = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\\");
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取基本信息
                Url = baseUrl + "lywsbs/lywsbs/rygl/ryxxcx.action";
                postdata = string.Format("aac001={0}&cid=&pid=&pcode=", socialReq.Username);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                PersonInfo _personinfo = jsonParser.DeserializeObject<PersonInfo>(httpResult.Html);
                Res.Name = _personinfo.aac003;
                Res.IdentityCard = _personinfo.aac002;
                Res.CompanyName = _personinfo.aab069;
                Res.CompanyNo = _personinfo.aab001;
                Res.Sex = _personinfo.aac004 == "1" ? "男" : "女";
                Res.EmployeeNo = _personinfo.aac001;
                Res.BirthDate = _personinfo.aac006;

                #endregion

                #region 查询明细

                int pageIndex = 1;
                int pageCount = 0;

                do
                {
                    Url = baseUrl + "lywsbs/framework/pageQuery.action";
                    postdata = string.Format("aac001={0}&sid=cn.les.lywsbs.rygl.mapper.RyglMapper.selGryljfjl&page={1}&pageSize=20", socialReq.Username, pageIndex);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (pageCount == 0)
                    {
                        pageCount = jsonParser.GetResultFromParser(httpResult.Html, "total").ToInt(0) / 20 + 1;
                    }
                    results = jsonParser.GetArrayFromParse(httpResult.Html, "rows");

                    foreach (string item in results)
                    {
                        DetailInfo _detail = jsonParser.DeserializeObject<DetailInfo>(item);

                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;
                        detailRes.PayTime = DateTime.Parse(_detail.aae003).ToString("yyyyMM");
                        detailRes.SocialInsuranceTime = detailRes.PayTime;
                        detailRes.SocialInsuranceBase = _detail.aae180.ToDecimal(0);
                        detailRes.CompanyPensionAmount = _detail.aae020.ToDecimal(0);
                        detailRes.PensionAmount = _detail.aae022.ToDecimal(0);
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        Res.Details.Add(detailRes);
                    }
                    pageIndex++;

                }
                while (pageIndex <= pageCount);

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
