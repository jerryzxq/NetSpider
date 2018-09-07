using System;
using System.Collections;
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
namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.JS
{
    public class kunshan : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.kssbzx.net.cn/webPages/";
        string socialCity = "js_kunshan";
        #endregion

        #region 私有变量
        enum InfoType
        {
            养老保险,
            医疗保险,
            失业保险,
            工伤保险,
            生育保险        
        }

        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息

        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, new string[] { "yljf", "" , "8" });
            PageHash.Add(InfoType.医疗保险, new string[] { "ybjf", "yb", "6" });
            PageHash.Add(InfoType.失业保险, new string[] { "ybjf", "shiye", "7" });
            PageHash.Add(InfoType.工伤保险, new string[] { "ybjf", "gs", "7" });
            PageHash.Add(InfoType.生育保险, new string[] { "ybjf", "shengyu", "7" });
        }

        void GetAllDetail(string SSID, InfoType Type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            SocialSecurityDetailQueryRes detailRes = new SocialSecurityDetailQueryRes();

            Url = baseUrl + ((string[])PageHash[Type])[0] + ".aspx?pageIndex=1&id=" + SSID + (Type == InfoType.养老保险 ? "" : "&type=" + ((string[])PageHash[Type])[1]);
            httpItem = new HttpItem()
            {
                URL = Url,
                Encoding = Encoding.UTF8,
                Referer = baseUrl + "grjb.aspx",
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='ctl00_ContentPlaceHolder1_showInfo']//tr");

            for (int i = 0; i < results.Count; i++)
            {
                var tdRow = HtmlParser.GetResultFromParser(results[i], "//td");
                if (tdRow.Count != ((string[])PageHash[Type])[2].ToInt(0))
                {
                    continue;
                }
                if (Type == InfoType.养老保险 && tdRow[1] != "企业养老")
                {
                    continue;
                }
                if (tdRow[0] == "缴费年月")
                {
                    continue;
                }

                string SocialInsuranceTime = tdRow[0];
                detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                bool NeedAddNew = false;
                if (detailRes == null)
                {
                    detailRes = new SocialSecurityDetailQueryRes();
                    NeedAddNew = true;
                }
                if (tdRow[3].IndexOf('<') > 0)
                {
                    tdRow[3] = tdRow[3].Substring(0, tdRow[3].IndexOf('<'));
                }
                switch (Type)
                {
                    case InfoType.养老保险:
                        detailRes.CompanyPensionAmount = tdRow[3].ToTrim().ToDecimal(0);
                        detailRes.PensionAmount = tdRow[4].ToTrim().ToDecimal(0);
                        break;
                    case InfoType.医疗保险:
                        detailRes.CompanyMedicalAmount = tdRow[3].ToTrim().ToDecimal(0);
                        detailRes.MedicalAmount = tdRow[4].ToTrim().ToDecimal(0);
                        break;
                    case InfoType.失业保险:
                        detailRes.UnemployAmount = tdRow[3].ToTrim().ToDecimal(0) + tdRow[4].ToTrim().ToDecimal(0);
                        break;
                    case InfoType.生育保险:
                        detailRes.MaternityAmount = tdRow[3].ToTrim().ToDecimal(0) + tdRow[4].ToTrim().ToDecimal(0);
                        break;
                    case InfoType.工伤保险:
                        detailRes.EmploymentInjuryAmount = tdRow[3].ToTrim().ToDecimal(0) + tdRow[4].ToTrim().ToDecimal(0);
                        break;
                }
                if (NeedAddNew)
                {
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;
                    detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                    detailRes.PayTime = Type == InfoType.养老保险 ? tdRow[6] : SocialInsuranceTime;
                    detailRes.SocialInsuranceTime = SocialInsuranceTime;

                    if (Type == InfoType.医疗保险)
                    {
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    }
                    else
                    {
                        detailRes.CompanyName = tdRow[5];
                        detailRes.PaymentType = tdRow[tdRow.Count - 1] == "正常结算" ? ServiceConsts.SocialSecurity_PaymentType_Normal : ServiceConsts.SocialSecurity_PaymentType_Adjust;
                        detailRes.PaymentFlag = tdRow[tdRow.Count - 1] == "正常结算" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : tdRow[tdRow.Count - 1];
                    }
                    Res.Details.Add(detailRes);
                }
            }
        }
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            Dictionary<string, object> _dic = new Dictionary<string, object>();
            try
            {
                Url = baseUrl + "grxxcxdl.aspx";
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
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                
                string __VIEWSTATE = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                if (results.Count > 0)
                {
                    __VIEWSTATE = results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.SocialSecurity_InitFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string __EVENTVALIDATION = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                if (results.Count > 0)
                {
                    __EVENTVALIDATION = results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.SocialSecurity_InitFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                _dic.Add("cookie", cookies);
                _dic.Add("__VIEWSTATE", __VIEWSTATE);
                _dic.Add("__EVENTVALIDATION", __EVENTVALIDATION);

                SpiderCacheHelper.SetCache(token, _dic);
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
            string __VIEWSTATE = string.Empty;
            string __EVENTVALIDATION = string.Empty;
            List<string> results = new List<string>();
            Dictionary<string, object> _dic = new Dictionary<string, object>();
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    _dic = (Dictionary<string, object>)SpiderCacheHelper.GetCache(socialReq.Token);
                    cookies = (CookieCollection)_dic["cookie"];
                    __VIEWSTATE = _dic["__VIEWSTATE"].ToString();
                    __EVENTVALIDATION = _dic["__EVENTVALIDATION"].ToString();
                    SpiderCacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Username.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                Url = baseUrl + "grxxcxdl.aspx";
                postdata = String.Format("__VIEWSTATE={0}&__EVENTVALIDATION={1}&txtSocial={2}&txtIdCard={3}&btn=%E7%99%BB+%E5%BD%95", __VIEWSTATE.ToUrlEncode(), __EVENTVALIDATION.ToUrlEncode(), socialReq.Username, socialReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Encoding = Encoding.UTF8,
                    Postdata = postdata,
                    Referer = baseUrl + "grxxcxdl.aspx",
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
                string msg = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "'");
                if (!msg.IsEmpty() && !msg.StartsWith(" PUBLIC"))
                {
                    Res.StatusDescription = msg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，个人基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='ctl00_ContentPlaceHolder1_showInfo']//td");
                Res.IdentityCard = socialReq.Identitycard;
                if (results.Count == 17)
                {
                    Res.Name = results[2];
                    Res.CompanyName = results[6];
                    Res.BirthDate = results[8];
                    Res.WorkDate = results[10];
                    Res.EmployeeStatus = results[12];
                }

                #endregion

                #region 第三步，查询明细
                InitPageHash();
                foreach (InfoType type in Enum.GetValues(typeof(InfoType)))
                {
                    GetAllDetail(socialReq.Username, type, ref Res);
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
