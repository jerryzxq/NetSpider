using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.SC
{
    /// <summary>
    /// 目前网站只有医疗明细可查,其它明细功能不可用
    /// </summary>
    public class scsheng : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://119.6.84.89:7001/scwssb/";//http://www.sc.hrss.gov.cn:7001/scwssb/
        private string host = "119.6.84.89:7001";
        string socialCity = "sc_scsheng";
        #endregion

        enum InfoType
        {
            //职工基本养老保险,
            职工基本医疗保险,
            //失业保险,
            //工伤保险,
            //生育保险
        }
        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息
        /// <summary>
        ///  将各种保险对应的页面所需信息存入PageHash
        /// </summary>
        void InitPageHash()
        {
            //PageHash.Add(InfoType.职工基本养老保险, new string[] { "110" });
            PageHash.Add(InfoType.职工基本医疗保险, new string[] { "310" });
            //PageHash.Add(InfoType.失业保险, new string[] { "210" });
            //PageHash.Add(InfoType.工伤保险, new string[] { "410" });
            //PageHash.Add(InfoType.生育保险, new string[] { "510" });
        }
        public VerCodeRes SocialSecurityInit()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "login.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Host = host,
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36",
                    KeepAlive = true,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header["Cache-Control"] = "max-age=0";

                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollectionEQ(cookies, httpResult.CookieCollection);

                Url = baseUrl + "CaptchaImg";
                httpItem = new HttpItem()
                {
                    URL = Url,
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
                cookies = CommonFun.GetCookieCollectionEQ(cookies, httpResult.CookieCollection);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
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
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(socialReq.Token);
                    SpiderCacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                //登陆
                Url = baseUrl + "j_spring_security_check?r=%27+Math.random()";
                postdata = String.Format("j_username={0}&j_password={1}&orgId=undefined&checkCode={2}&bz=0&tm={3}", socialReq.Username, socialReq.Password, socialReq.Vercode, CommonFun.GetTimeStamp());
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",

                    URL = Url,
                    Method = "POST",
                    Host = host,
                    Postdata = postdata,
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header["X-Requested-With"] = "XMLHttpRequest";
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    return Res;
                }
                string errorMsg = jsonParser.GetResultFromParser(httpResult.Html, "msg");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    Res.StatusDescription = errorMsg;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollectionEQ(cookies, httpResult.CookieCollection);
                Cookie addCookie = new Cookie("j_username", socialReq.Username, "/", "www.sc.hrss.gov.cn");
                cookies.Add(addCookie);
                Url = baseUrl + "indexAction.do";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
                    URL = Url,
                    Host = host,
                    Referer = "http://www.sc.hrss.gov.cn:7001/scwssb/login.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36"
                };
                httpItem.Header["Accept-Encoding"] = "gzip, deflate, sdch";
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollectionEQ(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，个人基本信息
                Url = baseUrl + "g03Action.do?___businessId=01520101";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='AAC003']", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='AAC002']", "value");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                string sex = CommonFun.GetMidStr(httpResult.Html, "AAC004']\"; option.value=\"", "\";");
                switch (sex)
                {
                    case "1":
                        Res.Sex = "男";
                        break;
                    case "2":
                        Res.Sex = "女";
                        break;
                    default:
                        Res.Sex = "无";
                        break;
                }
                string raceNum = CommonFun.GetMidStr(httpResult.Html, "AAC005']\"; option.value=\"", "\";");
                string raceArray = CommonFun.GetMidStr(httpResult.Html, "data=", "; option.divId=\"AAC005\"");
                JArray jsonArry = (JArray)JsonConvert.DeserializeObject(raceArray);
                foreach (JToken item in jsonArry)
                {
                    if (item["id"].ToString() == raceNum)
                    {
                        Res.Race = item["name"].ToString();
                        break;
                    }
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='AAC006']", "value");
                if (results.Count > 0)
                {
                    Res.BirthDate = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='AAC007']", "value");
                if (results.Count > 0)
                {
                    Res.WorkDate = results[0];
                }
                Url = baseUrl + "g40Action.do?___businessId=01520104";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string datastr = CommonFun.GetMidStr(httpResult.Html, "list\":[", "]};");
                JObject objJson = (JObject)JsonConvert.DeserializeObject(datastr);
                string EmployeeStatus = objJson["AAC008"].ToString();
                switch (@EmployeeStatus)
                {
                    case "0":
                        Res.EmployeeStatus = "未参保";
                        break;
                    case "1":
                        Res.EmployeeStatus = "正常参保";
                        break;
                    default://4
                        Res.EmployeeStatus = "终止参保";
                        break;
                }
                Res.CompanyName = objJson["AAB044"].ToString();
                #endregion

                InitPageHash();
                foreach (InfoType info in Enum.GetValues(typeof(InfoType)))
                {
                    try
                    {
                        GetAllDetail(info, ref Res);
                    }
                    catch
                    {
                        if (info == InfoType.职工基本医疗保险)
                            return Res;
                    }
                }
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

        private void GetAllDetail(InfoType type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            Url = baseUrl + "g04Action!queryData.do";
            postdata = String.Format("dto%5B'aae140'%5D={0}&&", ((string[])PageHash[type])[0]);
            httpItem = new HttpItem()
            {

                URL = Url,
                Method = "POST",
                Host = host,
                Postdata = postdata,
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);

            List<Details> details = jsonParser.DeserializeObject<List<Details>>(jsonParser.GetResultFromMultiNode(httpResult.Html, "lists:cbxx_grid:list"));
            foreach (var item in details)
            {
                SocialSecurityDetailQueryRes detailRes = null;
                bool NeedSave = false;
                //if (type != InfoType.职工基本养老保险)
                //{
                //    detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == item.AAE002);
                //}
                if (detailRes == null)
                {
                    NeedSave = true;
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    if (!string.IsNullOrEmpty(item.AAE079.Trim()))
                    {
                        detailRes.PayTime = DateTime.ParseExact(item.AAE079.Trim().Replace(" -", "-"), "dd-M月-yy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                    }
                    detailRes.SocialInsuranceTime = item.AAE002;
                    detailRes.CompanyName = item.AAB044;
                    detailRes.SocialInsuranceBase = item.AAC040.ToDecimal(0);
                    detailRes.PaymentType = item.AAE078 == "已足额到账" ? ServiceConsts.SocialSecurity_PaymentType_Normal : item.AAE078;
                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                }
                switch (type)
                {
                    //case InfoType.职工基本养老保险:
                    //    //detailRes.PensionAmount = Detail[4].ToTrim().Substring(1).ToDecimal(0);
                    //    //detailRes.CompanyPensionAmount = Detail[3].ToTrim().Substring(1).ToDecimal(0);
                    //    break;
                    case InfoType.职工基本医疗保险:
                        detailRes.MedicalAmount = item.AAE070.ToDecimal(0);
                        decimal payRate = (detailRes.SocialInsuranceBase > 0 && detailRes.MedicalAmount > 0) ? detailRes.MedicalAmount / detailRes.SocialInsuranceBase : 0.08M;
                        detailRes.CompanyMedicalAmount = Math.Round(detailRes.SocialInsuranceBase * payRate, 2);
                        detailRes.EnterAccountMedicalAmount = detailRes.MedicalAmount + detailRes.CompanyMedicalAmount;
                        break;
                    //case InfoType.失业保险:
                    //    //detailRes.UnemployAmount = Detail[3].ToTrim().Substring(1).ToDecimal(0) + Detail[4].ToTrim().Substring(1).ToDecimal(0);
                    //    break;
                    //case InfoType.工伤保险:
                    //    break;
                    //case InfoType.生育保险:
                    //    break;
                }
                if (NeedSave)
                    Res.Details.Add(detailRes);
            }

        }
        internal class Details
        {
            /// <summary>
            /// 单位名称
            /// </summary>
            public string AAB044 { get; set; }

            private string _AAE140 = string.Empty;
            /// <summary>
            /// 险种
            /// </summary>
            public string AAE140
            {
                get { return _AAE140; }
                set
                {
                    switch (value)
                    {
                        case "110":
                            _AAE140 = "职工基本养老保险";
                            break;
                        case "120":
                            _AAE140 = "机关事业养老保险";
                            break;
                        case "310":
                            _AAE140 = "医疗保险";
                            break;
                        case "311":
                            _AAE140 = "公务员医疗保险";
                            break;
                        case "312":
                            _AAE140 = "住院补充医疗保险";
                            break;
                        case "313":
                            _AAE140 = "门诊补充医疗保险";
                            break;
                        case "210":
                            _AAE140 = "失业保险";
                            break;
                        case "410":
                            _AAE140 = "工伤保险";
                            break;
                        case "510":
                            _AAE140 = "生育保险";
                            break;
                        default:
                            _AAE140 = "";
                            break;
                    }
                }
            }
            /// <summary>
            /// 费用所属期号
            /// </summary>
            public string AAE002 { get; set; }
            /// <summary>
            /// 月缴费工资基数
            /// </summary>
            public string AAC040 { get; set; }
            /// <summary>
            /// 个人月缴费额
            /// </summary>
            public string AAE070 { get; set; }

            private string _AAE078 = string.Empty;
            /// <summary>
            /// 到账标志
            /// </summary>
            public string AAE078
            {
                get
                {
                    return _AAE078;
                }
                set
                {
                    switch (value)
                    {
                        case "0":
                            _AAE078 = "未足额到账";
                            break;
                        case "1":
                            _AAE078 = "已足额到账";
                            break;
                        default:
                            _AAE078 = "";
                            break;
                    }
                }
            }
            /// <summary>
            /// 到账时间
            /// </summary>
            public string AAE079 { get; set; }
        }
    }
}