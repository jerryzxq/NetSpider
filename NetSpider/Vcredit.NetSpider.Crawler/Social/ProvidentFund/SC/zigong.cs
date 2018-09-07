using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SC
{
    public class zigong : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://222.208.126.82/OBS/";
        string fundCity = "sc_zigong";
        #endregion
        #region 私有变量

        string Url = string.Empty;
        string postdata = string.Empty;
        decimal payRate = (decimal)0.07;
        List<string> results = new List<string>();
        ProvidentFundDetail detail = null;
        int PaymentMonths = 0;
        #endregion
        public VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "Forms/System/Login.aspx";
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
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string viewstate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='__VIEWSTATE']", "value")[0];
                string eventvalidation = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='__EVENTVALIDATION']", "value")[0];
                Url = baseUrl + "Forms/System/Sys_ValidatorImage.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                Dictionary<string, object> dic = new Dictionary<string, object>
                {
                    {"viewstate", viewstate},
                    {"eventvalidation", eventvalidation},
                    {"cookies", cookies}
                };

                SpiderCacheHelper.SetCache(token, dic);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return Res;
        }

        public ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = fundCity;
            Res.Token = fundReq.Token;
            string viewstate = string.Empty;
            string eventvalidation = string.Empty;
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(fundReq.Token) != null)
                {
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    dic = (Dictionary<string, object>)SpiderCacheHelper.GetCache(fundReq.Token);
                    viewstate = dic["viewstate"].ToString();
                    eventvalidation = dic["eventvalidation"].ToString();
                    cookies = (CookieCollection)dic["cookies"];
                    SpiderCacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录系统
                //校验

                Url = baseUrl + "WebServices/SysWebService.asmx/checkUserLogin";
                postdata = "[{\"Key\":\"UserName\",\"Value\":\"" + fundReq.Identitycard + "\"},{\"Key\":\"UserPasswords\",\"Value\":\"" + fundReq.Password + "\"},{\"Key\":\"Vailtext\",\"Value\":\"" + fundReq.Vercode + "\"},{\"Key\":\"UserType\",\"Value\":\"2\"},{\"Key\":\"SmsVailCode\",\"Value\":\"\"},{\"Key\":\"returnMsg\",\"Value\":\"\"}]";
                postdata = "p_jsons=" + postdata.ToUrlEncode().ToUrlEncode();
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "222.208.126.82",
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "returnMsg\",\"Value\":\"", "\"},").Trim();
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //登陆
                Url = baseUrl + "Forms/System/Login.aspx";
                postdata = String.Format("__VIEWSTATE={0}&__EVENTVALIDATION={1}&Type=2&TxtUseName={2}&TxtPwd={3}&txtVailtext={4}&txtSmsVailCode=&imgCmdLog.x=0&imgCmdLog.y=0&hfUserType=2&hfLoginEtpsBySMS=True&hfLoginIndvBySMS=False&hfLoginDevBySMS=True&hfSMSWaitingTime=180&hfSMSValidTime=180&hfShyTime=&hfVailCode=&hfUseEtpsCode=0813", viewstate.ToUrlEncode(), eventvalidation.ToUrlEncode(), fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    Allowautoredirect = false,
                    URL = Url,
                    Referer = baseUrl + "Login.aspx",
                    Host = "222.208.126.82",
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.Found)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = httpResult.RedirectUrl;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "222.208.126.82",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，查询个人基本信息
                Url = baseUrl + "Forms/Indv/Indv_InfoDetials.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string baseMsg = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='ctl00_CPBody_hfIndvInfo']", "value", true)[0].Replace("&quot;", "");
                string[] baseMsgToChar = CommonFun.GetMidStr(baseMsg, "InterestMoney],[", "]").Split(',');
                if (baseMsgToChar.Length == 15)
                {
                    Res.CompanyNo = baseMsgToChar[1];
                    Res.CompanyName = baseMsgToChar[2];
                    Res.ProvidentFundNo = baseMsgToChar[3];
                    Res.Name = baseMsgToChar[4];
                    Res.IdentityCard = baseMsgToChar[5];
                    Res.SalaryBase = baseMsgToChar[6].ToDecimal(0);
                    Res.TotalAmount = baseMsgToChar[7].ToDecimal(0);
                    Res.Status = baseMsgToChar[9];
                    Res.OpenTime = DateTime.ParseExact(baseMsgToChar[10].Replace(@"\", ""), "yyyy/MM/dd", CultureInfo.InvariantCulture).ToString(Consts.DateFormatString2);
                    Res.LastProvidentFundTime = DateTime.ParseExact(baseMsgToChar[13].Replace(@"\", ""), "yyyy/MM/dd", CultureInfo.InvariantCulture).ToString(Consts.DateFormatString2);
                    decimal monthPay = baseMsgToChar[11].ToDecimal(0);
                    if (monthPay > 0 && Res.SalaryBase > 0)
                    {
                        Res.PersonalMonthPayRate = Res.CompanyMonthPayRate = Math.Round((monthPay / Res.SalaryBase) / 2, 2);
                        Res.PersonalMonthPayAmount = Res.CompanyMonthPayAmount = Math.Round(monthPay / 2, 2);
                    }
                }
                #endregion

                //查询单位名称列表信息
                Url = baseUrl + "Forms/Indv/Indv_GatherList.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string EtpsIDListStr = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='ctl00_CPBody_hfDirList']", "value", true)[0].Replace("&quot;", "");
                EtpsIDListStr = CommonFun.GetMidStr(EtpsIDListStr, "Value:[", "]}]").Replace("],[", "]&[");
                string[] EtpsIDList = EtpsIDListStr.Split('&');


                #region 第三步，查询缴费明细

                if (Res.PersonalMonthPayRate>=0&& Res.CompanyMonthPayRate>=0)
                {
                    payRate = Res.PersonalMonthPayRate;
                }
                string[] detailResult = null;
                int nowYear = DateTime.Now.Year;
                int openYear = !string.IsNullOrEmpty(Res.OpenTime) ? Convert.ToDateTime(Res.OpenTime).Year : nowYear-5;
                foreach (string company in EtpsIDList)
                {
                    string[] companyChar = company.TrimStart('[').TrimEnd(']').Split(',');
                    if (companyChar.Length != 2)
                    {
                        continue;
                    }
                   
                    do
                    {
                       
                        Url = baseUrl + "WebServices/IndvWebService.asmx/obs_Indv_GetGatherList";
                        postdata = "[{\"Key\":\"EtpsID\",\"Value\":\"" + companyChar[0] + "\"},{\"Key\":\"Year\",\"Value\":\""+nowYear+"\"},{\"Key\":\"Number\",\"Value\":1},{\"Key\":\"Size\",\"Value\":\"50\"}]";
                        postdata = "p_json=" + postdata.ToUrlEncode();
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "POST",
                            Postdata = postdata,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                       
                        //查询日期早于开户日期,得到当前年份数据
                        if (!httpResult.Html.Contains(nowYear + "-0") && nowYear!=DateTime.Now.Year)
                        {
                            nowYear--;
                            continue;
                        }
                        detailResult = CommonFun.GetMidStr(httpResult.Html, "YuE\"],[", "]}]").Replace("],[", "&").TrimStart('[').TrimEnd(']').Split('&');
                        foreach (var item in detailResult)
                        {
                            string[] tdRow = item.Replace("\",\"", "&").Replace("\"", "").ToTrim().Split('&');
                            if (tdRow.Length<8)
                            {
                                continue;
                            }
                            detail = new ProvidentFundDetail();
                            detail.CompanyName = companyChar[1];
                            if (tdRow[4].IndexOf("职工缴交") > -1)
                            {
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                                detail.Description = tdRow[4];
                                detail.PayTime = tdRow[3].ToDateTime(Consts.DateFormatString2);
                                detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[4], "交", "月至").Replace("年", "");
                                detail.PersonalPayAmount = tdRow[5].ToDecimal(0);
                                detail.ProvidentFundBase = Math.Round(detail.PersonalPayAmount / payRate,2);
                            }
                            else if (tdRow[4].IndexOf("单位缴交") > -1)
                            {
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                                detail.Description = tdRow[4];
                                detail.PayTime = tdRow[3].ToDateTime(Consts.DateFormatString2);
                                detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[4], "交", "月至").Replace("年", "");
                                detail.CompanyPayAmount = tdRow[5].ToDecimal(0);
                                detail.ProvidentFundBase = Math.Round(detail.PersonalPayAmount / payRate, 2);
                            }
                            else if (tdRow[4].StartsWith("补缴"))
                            {
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Back;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Back;
                                detail.Description = tdRow[4];
                                detail.PayTime = tdRow[3].ToDateTime(Consts.DateFormatString2);
                                detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[4], "交", "月至").Replace("年", "");
                                if (!detail.ProvidentFundTime.IsEmpty())
                                {
                                    detail.PersonalPayAmount = tdRow[5].ToDecimal(0) / 2;
                                    detail.CompanyPayAmount = detail.PersonalPayAmount;
                                    detail.ProvidentFundBase = Math.Round(detail.PersonalPayAmount / payRate, 2);
                                }
                            }
                            else if (tdRow[4].IndexOf("*") > -1)
                            {
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                                detail.Description = tdRow[3];
                                detail.PersonalPayAmount = tdRow[5].ToDecimal(0);
                            }
                            else
                            {
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                                detail.Description = tdRow[4];
                                detail.PayTime = tdRow[3].ToDateTime(Consts.DateFormatString2);
                                detail.PersonalPayAmount = tdRow[5].ToDecimal(0);
                            }
                            Res.ProvidentFundDetailList.Add(detail);
                        }
                        nowYear--;
                    } while (nowYear >= openYear);
                }
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
    }
}
