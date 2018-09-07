using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.ZJ
{
    public class zhuji : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.zjld.gov.cn/zjlss/operation/";
        string socialCity = "zj_zhuji";
        #endregion
        #region 私有变量

        enum InfoType
        {
            养老保险,
            医疗保险,
            失业保险,
            工伤保险,
            生育保险,
            大病救助医疗保险,
            公务员补助医疗保险
        }
        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息
        /// <summary>
        ///  将各种保险对应的页面所需信息存入PageHash
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, "01");
            PageHash.Add(InfoType.医疗保险, "07");
            PageHash.Add(InfoType.失业保险, "06");
            PageHash.Add(InfoType.工伤保险, "04");
            PageHash.Add(InfoType.生育保险, "05");
            PageHash.Add(InfoType.大病救助医疗保险, "08");
            PageHash.Add(InfoType.公务员补助医疗保险, "09");
        }
        /// <summary>
        /// 获取缴费明细
        /// </summary>
        /// <param name="type">缴费类型</param>
        /// <param name="Res"></param>
        private void GetAllDetail(InfoType type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            List<string> results = new List<string>();
            string postData = string.Empty;
            int spage = 1;
            int pageCount = 1;
            Url = baseUrl + "person/payinfo.jsp";
            do
            {
                postData = string.Format("spage={0}&aae001={1}", spage, (string)PageHash[type]);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postData,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (spage == 1)
                {
                    pageCount = CommonFun.GetMidStr(httpResult.Html, "</font>/", "页").ToInt(0);
                }
                results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//tr[@class='table_tr_01']", "", true));
                spage++;
            } while (spage <= pageCount);
            foreach (string item in results)
            {
                var tdRow = HtmlParser.GetResultFromParser(item, "//td/div");
                if (tdRow.Count < 10) continue;
                SocialSecurityDetailQueryRes detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == tdRow[1]);
                bool isSave = false;
                if (detailRes == null)
                {
                    isSave = true;
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;
                    detailRes.PayTime = tdRow[9];
                    detailRes.SocialInsuranceTime = tdRow[1];
                    detailRes.PaymentFlag = tdRow[8] != "未到帐" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                    detailRes.PaymentType = tdRow[8];
                }
                switch (type)
                {
                    case InfoType.养老保险:
                        detailRes.PensionAmount += tdRow[5].ToDecimal(0);
                        detailRes.CompanyPensionAmount += tdRow[4].ToDecimal(0);
                        detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                        break;
                    case InfoType.医疗保险:
                        detailRes.CompanyMedicalAmount += tdRow[4].ToDecimal(0);
                        detailRes.MedicalAmount += tdRow[5].ToDecimal(0);
                        break;
                    case InfoType.失业保险:
                        detailRes.UnemployAmount += (tdRow[4].ToDecimal(0) + tdRow[5].ToDecimal(0));
                        break;
                    case InfoType.工伤保险:
                        detailRes.EmploymentInjuryAmount += tdRow[4].ToDecimal(0);
                        break;
                    case InfoType.生育保险:
                        detailRes.MaternityAmount += tdRow[4].ToDecimal(0);
                        break;
                    case InfoType.大病救助医疗保险:
                        detailRes.IllnessMedicalAmount += tdRow[4].ToDecimal(0);
                        break;
                    case InfoType.公务员补助医疗保险:
                        detailRes.CivilServantMedicalAmount += tdRow[4].ToDecimal(0);
                        break;
                }
                detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : tdRow[2].ToDecimal(0);
                if (isSave)
                {
                    Res.Details.Add(detailRes);
                }
            }
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
                Url = baseUrl + "login.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
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
                string num = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='num']", "value")[0];//验证码
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Dictionary<string, object> dic = new Dictionary<string, object>
                {
                    {"num",num},
                    {"cookies",cookies}
                };
                CacheHelper.SetCache(token, dic);
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
            string num = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    Dictionary<string, object> dic = (Dictionary<string, object>)CacheHelper.GetCache(socialReq.Token);
                    cookies = (CookieCollection)dic["cookies"];
                    num = (string)dic["num"];
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                Url = baseUrl + "UserLogin";
                postdata = String.Format("errors=&type=2&username={0}&password={1}&num12={2}&num={2}&image.x=23&image.y=9", socialReq.Identitycard, socialReq.Name.ToUrlEncode(Encoding.GetEncoding("GBK")), num);
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
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='errors']", "value");
                if (results.Count > 0)
                {
                    if (!string.IsNullOrEmpty(results[0]))
                    {
                        Res.StatusDescription = results[0];
                        Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                        return Res;
                    }
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion
                #region 第二步， 获取基本信息

                //个人基本信息
                Url = baseUrl + "person/personBase.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                Res.Name = socialReq.Name;
                Res.IdentityCard = socialReq.Identitycard;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_style02']/tr[2]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_style02']/tr[2]/td[4]", "");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_style02']/tr[4]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.Sex = results[0].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_style02']/tr[4]/td[4]", "");
                if (results.Count > 0)
                {
                    Res.Race = results[0].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_style02']/tr[5]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.BirthDate = results[0].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_style02']/tr[5]/td[4]", "");
                if (results.Count > 0)
                {
                    Res.EmployeeStatus = results[0].Replace("&nbsp;", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='AAE005']", "value");
                if (results.Count > 0)
                {
                    Res.Phone = results[0] != "0" ? results[0] : null;//
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='PHONE']", "value");
                if (results.Count > 0)
                {
                    Res.Phone = results[0] != "0" & string.IsNullOrEmpty(Res.Phone) ? results[0] : Res.Phone;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='AAE006']", "value");
                if (results.Count > 0)
                {
                    Res.Address = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='AAE007']", "value");
                if (results.Count > 0)
                {
                    Res.ZipCode = results[0] != "0" ? results[0] : null;
                }
                #endregion
                #region 第三步，查询明细

                InitPageHash();
                foreach (InfoType type in Enum.GetValues(typeof(InfoType)))
                {
                    try
                    {
                        GetAllDetail(type, ref Res);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
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
