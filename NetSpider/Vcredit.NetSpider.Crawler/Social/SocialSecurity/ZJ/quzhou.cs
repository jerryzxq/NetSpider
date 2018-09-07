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
    public class quzhou : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://122.227.89.140:8099/qzlss/bsdt/";
        string socialCity = "zj_quzhou";
        #endregion
        #region 私有变量

        enum InfoType
        {
            养老保险,
            医疗保险,
            //失业保险,
            工伤保险,
            生育保险,
        }
        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息
        /// <summary>
        ///  将各种保险对应的页面所需信息存入PageHash
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, "grxxcx_ylgrzhxx.jsp");
            PageHash.Add(InfoType.医疗保险, "grxxcx_jbylbxjfmx.jsp");
            PageHash.Add(InfoType.工伤保险, "grxxcx_gsbxjfcx.jsp");
            PageHash.Add(InfoType.生育保险, "grxxcx_sybxjfcx.jsp");
        }
        /// <summary>
        /// 获取缴费明细
        /// </summary>
        /// <param name="type">缴费类型</param>
        /// <param name="Res"></param>
        private void GetAllDetail(InfoType type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            string postData = string.Empty;
            List<string> results = new List<string>();
            Url = baseUrl + PageHash[type];
            int i = 0;
            while (true)
            {
                postData = string.Format("AAE001={0}&Submit=%C8%B7+%B6%A8", DateTime.Now.AddYears(i).Year);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postData,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                List<string> breakPoint = HtmlParser.GetResultFromParser(httpResult.Html, "//td/table[@class='table_wsbs'][last()]/tr[position()>1]", "", true);
                if (breakPoint.Count == 0) break;
                results.AddRange(breakPoint);
                i--;
            }

            foreach (string item in results)
            {
                List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td");
                switch (type)
                {
                    case InfoType.医疗保险:
                        if (tdRow.Count < 9) continue;
                        break;
                    default:
                        if (tdRow.Count < 8) continue;
                        break;
                }
                SocialSecurityDetailQueryRes detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == tdRow[0]);
                bool isSave = false;
                if (detailRes == null)
                {
                    isSave = true;
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;
                    detailRes.SocialInsuranceTime = tdRow[0];
                    switch (type)
                    {
                        case InfoType.医疗保险:
                            detailRes.PayTime = tdRow[7];
                            detailRes.CompanyName = tdRow[8];
                            detailRes.PaymentFlag = tdRow[3] == "已到账" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                            detailRes.PaymentType = tdRow[3];
                            break;
                        default:
                            detailRes.PayTime = tdRow[6];
                            detailRes.CompanyName = tdRow[7];
                            if (string.IsNullOrEmpty(tdRow[6]) || tdRow[2].IndexOf("缴") == -1)
                            {
                                detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                                detailRes.PaymentType = tdRow[0] == DateTime.Now.ToString(Consts.DateFormatString7) ? "未交费" : tdRow[2];
                            }
                            else
                            {
                                detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                                detailRes.PaymentType = tdRow[2];
                            }
                            break;
                    }
                }
                switch (type)
                {
                    case InfoType.养老保险:
                        detailRes.PensionAmount += tdRow[5].ToDecimal(0);
                        detailRes.CompanyPensionAmount += (tdRow[4].ToDecimal(0) - tdRow[5].ToDecimal(0));
                        detailRes.SocialInsuranceBase = tdRow[3].ToDecimal(0);
                        break;
                    case InfoType.医疗保险:
                        detailRes.MedicalAmount += tdRow[6].ToDecimal(0);
                        detailRes.CompanyMedicalAmount += (tdRow[5].ToDecimal(0) - tdRow[6].ToDecimal(0));//总缴费-个人缴费
                        break;

                    case InfoType.工伤保险:
                        detailRes.EmploymentInjuryAmount += tdRow[4].ToDecimal(0);
                        break;
                    case InfoType.生育保险:
                        detailRes.MaternityAmount += tdRow[4].ToDecimal(0);
                        break;
                }
                detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : (type==InfoType.医疗保险?tdRow[4].ToDecimal(0):tdRow[3].ToDecimal(0));
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
                Url = baseUrl + "login_per.jsp";
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
                string num = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='num']", "value")[0];//验证码
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Citizencard.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                Url = baseUrl + "logincheck.jsp";
                postdata = String.Format("type=2&eac051={0}&password={1}&code={2}&num={2}&Login=%B5%C7+%C2%BC", socialReq.Citizencard, socialReq.Identitycard, num);
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
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\")");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion
                #region 第二步， 获取基本信息

                //参保人员基本信息
                Url = baseUrl + "grxxcx_cbryjbxx.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td/table[@class='table_wsbs'][1]/tr/td", "text");
                if (results.Count != 16)
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                Res.Name = socialReq.Name;
                Res.IdentityCard = socialReq.Identitycard;
                Res.EmployeeNo = results[1];
                Res.Name = results[3];
                Res.IdentityCard = results[5];
                Res.Sex = results[7].ToTrim("性");
                Res.BirthDate = results[9];
                Res.Address = results[11];
                Res.ZipCode = results[13];
                Res.Phone = results[15];
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td/table[@class='table_wsbs'][last()]/tr[position()>1]/td", "text");
                for (int i = 0; i <= results.Count - 6; i = i + 6)
                {
                    Res.SpecialPaymentType += results[i] + ":" + results[i + 3] + ";";
                    switch (results[i])
                    {
                        case "企业养老":
                        case "机关养老":
                        case "事业养老":
                            Res.EmployeeStatus = results[i + 2];
                            break;
                    }
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
