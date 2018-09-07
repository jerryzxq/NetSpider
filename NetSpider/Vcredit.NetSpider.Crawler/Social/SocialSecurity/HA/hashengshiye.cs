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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.HA
{
    public class hashengshiye : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://hnylbx.com/siq/";
        string socialCity = "ha_hashengshiye";//河南省事业单位
        #endregion
        #region 私有变量
        Dictionary<string, string> dicArrayBase = new Dictionary<string, string>();
        #endregion
        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;

            try
            {
                Url = baseUrl + "index.jsp";
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
                string zoneCode = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='zoneCode']", "value")[0];

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Dictionary<string, object> dic = new Dictionary<string, object>()
                {
                    {"zoneCode",zoneCode},
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
            int PaymentMonths = 0;
            string zoneCode = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    Dictionary<string, object> dic = (Dictionary<string, object>)CacheHelper.GetCache(socialReq.Token);
                    cookies = (CookieCollection)dic["cookies"];
                    zoneCode = dic["zoneCode"].ToString();
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

                Url = baseUrl + "web/loginWeb.action";
                postdata = String.Format("user.userCode={0}&user.psw={1}&user.zoneCode={2}&user.userType=3", socialReq.Username, socialReq.Password, zoneCode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header["X-Requested-With"] = "XMLHttpRequest";
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (!httpResult.Html.Contains("success"))
                {
                    Res.StatusDescription = httpResult.Html.Replace("\"", "");
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);



                #endregion
                #region 第二步,获取基本信息

                //基本信息
                Url = baseUrl + "web/skipPage_loginSucc.action";
                httpItem = new HttpItem()
                {
                    //Allowautoredirect=false,
                    URL = Url,
                    Host = "hnylbx.com",
                    CookieCollection = cookies,
                    Referer = "http://hnylbx.com/siq/index.jsp",
                    ResultCookieType = ResultCookieType.CookieCollection,
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='proofTable']/div[1]/table//tr/tr[2]/td", "text", true);
                if (results.Count == 3)
                {
                    Res.CompanyNo = results[0].Replace("单位编号:", "");
                    Res.CompanyName = results[1].Replace("单位名称:", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='proofTable']/div[1]/table//tr/tr[3]//table/tr/td", "", true);
                for (int i = 0; i < results.Count; i++)
                {
                    switch (@results[i])
                    {
                        case "姓名":
                            Res.Name = results[i + 1];
                            break;
                        case "性别":
                            Res.Sex = results[i + 1];
                            break;
                        case "民族":
                            Res.Race = results[i + 1];
                            break;
                        case "出生日期":
                            Res.BirthDate = results[i + 1];
                            break;
                        case "个人编号":
                            Res.EmployeeNo = results[i + 1];
                            break;
                        case "身份证号码":
                            Res.IdentityCard = results[i + 1];
                            break;
                        case "参加工作时间":
                            Res.WorkDate = results[i + 1];
                            break;
                        case "参保状态":
                            Res.EmployeeStatus = results[i + 1];
                            break;
                        case "账户月数":
                            Res.PaymentMonths = results[i + 1].ToInt(0);
                            break;
                        case "账户本息合计":
                            Res.PersonalInsuranceTotal = results[i + 1].ToDecimal(0);
                            break;
                    }
                }
                Url = baseUrl + "web/account_queryPerAccount.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "hnylbx.com",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                };
                httpResult = httpHelper.GetHtml(httpItem);
                JObject jBase = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                Res.Phone = jBase["phone"].ToString();
                #endregion
                #region 第三步,获取详细信息

                //个人历年缴费基数  
                Url = baseUrl + "web/staff_queryYpwilist.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "hnylbx.com",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html.Replace("<\\/td>", "</td>").ToTrim("年"), "//tr/td", "");

                int times = results.Count / 18;
                for (int i = 0; i < times; i++)
                {
                    for (int j = i * 18; j < i * 18 + 9; j++)
                    {
                        if (!string.IsNullOrEmpty(results[j]))
                        {
                            dicArrayBase.Add(results[j], results[j + 9]);
                        }
                    }
                }
                //详细信息
                Url = baseUrl + "web/staff_queryPpiblist.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "hnylbx.com",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                List<hashengshiyeDetails> details = jsonParser.DeserializeObject<List<hashengshiyeDetails>>(httpResult.Html); ;
                foreach (hashengshiyeDetails item in details)
                {
                    if (string.IsNullOrEmpty(item.theYear))
                    {
                        continue;
                    }
                    GetDetails(item.payMonthOne, item.theYear, "01", ref Res);
                    GetDetails(item.payMonthTwo, item.theYear, "02", ref Res);
                    GetDetails(item.payMonthThree, item.theYear, "03", ref Res);
                    GetDetails(item.payMonthFour, item.theYear, "04", ref Res);
                    GetDetails(item.payMonthFive, item.theYear, "05", ref Res);
                    GetDetails(item.payMonthSix, item.theYear, "06", ref Res);
                    GetDetails(item.payMonthSeven, item.theYear, "07", ref Res);
                    GetDetails(item.payMonthEight, item.theYear, "08", ref Res);
                    GetDetails(item.payMonthNine, item.theYear, "09", ref Res);
                    GetDetails(item.payMonthTen, item.theYear, "10", ref Res);
                    GetDetails(item.payMonthEleven, item.theYear, "11", ref Res);
                    GetDetails(item.payMonthTwelve, item.theYear, "12", ref Res);
                }
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

        private void GetDetails(string payMonthState, string theYear, string month, ref SocialSecurityQueryRes Res)
        {
            if (string.IsNullOrEmpty(payMonthState))
            {
                return;
            }
            SocialSecurityDetailQueryRes detailRes = new SocialSecurityDetailQueryRes();
            detailRes.Name = Res.Name;
            if (dicArrayBase.ContainsKey(theYear))
            {
                detailRes.SocialInsuranceBase = dicArrayBase[theYear].ToDecimal(0);
            }
            detailRes.SocialInsuranceTime = theYear + month;
            switch (@payMonthState)
            {
                case "正常缴费":
                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    break;
                case "调整":
                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                    break;
            }
            Res.Details.Add(detailRes);
        }
        private static string GetState(string month)
        {
            switch (@month)
            {
                case "●":
                case "▲":
                case "■":
                    return ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                case "△":
                    return ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                default:
                    return "";
            }
        }
        internal class hashengshiyeDetails
        {
            private string _payMonthOne = string.Empty;
            public string payMonthOne
            {
                get { return _payMonthOne; }
                set
                {
                    _payMonthOne = GetState(value);
                }
            }
            private string _payMonthTwo = string.Empty;
            public string payMonthTwo
            {
                get { return _payMonthTwo; }
                set
                {
                    _payMonthTwo = GetState(value);
                }
            }
            private string _payMonthThree = string.Empty;
            public string payMonthThree
            {
                get { return _payMonthThree; }
                set
                {
                    _payMonthThree = GetState(value);
                }
            }
            private string _payMonthFour = string.Empty;
            public string payMonthFour
            {
                get { return _payMonthFour; }
                set
                {
                    _payMonthFour = GetState(value);
                }
            }
            private string _payMonthFive = string.Empty;
            public string payMonthFive
            {
                get { return _payMonthFive; }
                set
                {
                    _payMonthFive = GetState(value);
                }
            }
            private string _payMonthSix = string.Empty;
            public string payMonthSix
            {
                get { return _payMonthSix; }
                set
                {
                    _payMonthSix = GetState(value);
                }
            }
            private string _payMonthSeven = string.Empty;
            public string payMonthSeven
            {
                get { return _payMonthSeven; }
                set
                {
                    _payMonthSeven = GetState(value);
                }
            }
            private string _payMonthEight = string.Empty;
            public string payMonthEight
            {
                get { return _payMonthEight; }
                set
                {
                    _payMonthEight = GetState(value);
                }
            }
            private string _payMonthNine = string.Empty;
            public string payMonthNine
            {
                get { return _payMonthNine; }
                set
                {
                    _payMonthNine = GetState(value);
                }
            }

            private string _payMonthTen = string.Empty;
            public string payMonthTen
            {
                get { return _payMonthTen; }
                set
                {
                    _payMonthTen = GetState(value);
                }
            }
            private string _payMonthEleven = string.Empty;
            public string payMonthEleven
            {
                get { return _payMonthEleven; }
                set
                {
                    _payMonthEleven = GetState(value);
                }
            }
            private string _payMonthTwelve = string.Empty;
            public string payMonthTwelve
            {
                get { return _payMonthTwelve; }
                set
                {
                    _payMonthTwelve = GetState(value);
                }
            }
            public string personalNo { get; set; }
            public string theYear { get; set; }
        }
    }
}
