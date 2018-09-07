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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.AH
{
    public class wuhu : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://60.169.170.18:82/";
        string socialCity = "ah_wuhu";

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
        /// <summary>
        ///  将各种保险对应的页面所需信息存入PageHash
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, new string[] { "oldster/insure/pay?_" });
            PageHash.Add(InfoType.医疗保险, new string[] { "pay/medical?_" });
            PageHash.Add(InfoType.失业保险, new string[] { "pay/losejob?_" });
            PageHash.Add(InfoType.工伤保险, new string[] { "pay/injury?_" });
            PageHash.Add(InfoType.生育保险, new string[] { "pay/birth?_" });
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
                Url = baseUrl + "index.shtml";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "sso/code/imgcode/width/0/height/0/length/4?_=" + GetTimeStamp();
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl + "index.shtml",
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
                byte[] resultByte = Convert.FromBase64String(httpResult.Html.Replace("data:image/gif;base64,", ""));
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCodeBase64 = httpResult.Html.Replace("data:image/gif;base64,", "").Trim();
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, resultByte);
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
            string idCard = string.Empty;
            string results = string.Empty;
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
                if (socialReq.Citizencard.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                Url = baseUrl + "sso/account/ahsblogin";
                postdata = String.Format("login_name={0}&pwd={1}&code={2}&loginType=1", socialReq.Citizencard, CommonFun.GetEncodeBase64(Encoding.UTF8, socialReq.Password), socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "index.shtml",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = jsonParser.GetResultFromParser(httpResult.Html, "statusCode");
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (results != "200")
                {
                    Res.StatusDescription = jsonParser.GetResultFromParser(httpResult.Html, "message");
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 第二步 校验

                Url = baseUrl + "sso/account/ahsblogin";
                postdata = String.Format("login_name={0}&pwd={1}&code={2}&forced_entry=1&loginType=1", socialReq.Citizencard, CommonFun.GetEncodeBase64(Encoding.UTF8, socialReq.Password), socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "index.shtml",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = jsonParser.GetResultFromParser(httpResult.Html, "statusCode");
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK || results != "200")
                {
                    Res.StatusDescription = jsonParser.GetResultFromParser(httpResult.Html, "message");
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = jsonParser.GetResultFromParser(httpResult.Html, "result");
                if (!string.IsNullOrEmpty(results))
                    idCard = jsonParser.GetResultFromParser(results, "idcard");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 第三步 获取基本信息
                Url = String.Format("{0}ahsb/personal/basic?idCard={1}&callback=callback&_={2}", baseUrl, idCard, GetTimeStamp());
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    URL = Url,
                    Method = "GET",
                    Referer = baseUrl + "index.shtml",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //string resultstr = CommonFun.GetMidStr(httpResult.Html, "callback(", ")");
                string resultstr = httpResult.Html.Replace("callback(", "").TrimEnd(')');
                results = jsonParser.GetResultFromParser(resultstr, "statusCode");
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK || results != "200")
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }

                results = jsonParser.GetResultFromParser(resultstr, "result");
                if (string.IsNullOrEmpty(results))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = CommonFun.GetMidStr(results, "[", "]");
                Res.EmployeeNo = jsonParser.GetResultFromParser(results, "GRBH");//编号
                Res.Name = jsonParser.GetResultFromParser(results, "XM");//姓名
                Res.IdentityCard = jsonParser.GetResultFromParser(results, "SFZH");//身份证号
                Res.Sex = jsonParser.GetResultFromParser(results, "xb_name");//性别
                Res.Race = jsonParser.GetResultFromParser(results, "mz_name");//民族
                Res.Phone = jsonParser.GetResultFromParser(results, "LXDH");//联系电话
                Res.WorkDate = jsonParser.GetResultFromParser(results, "CJGZSJ");//参加工作时间
                Res.EmployeeStatus = jsonParser.GetResultFromParser(results, "ryzt_name");//工作状态
                Res.CompanyName = jsonParser.GetResultFromParser(results, "DWMC");//单位名称
                //Res.Payment_State = jsonParser.GetResultFromParser(results, "cbzt_name");

                #endregion
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
                #region 第四步,养老保险

                //int total = 100;
                //List<WuHuSocia> sociaresults = new List<WuHuSocia>();

                //do
                //{
                //    Url = String.Format("{0}ahsb/personal/oldster/insure/pay?_={1}&idCard={2}&pagesize={3}&pageno=1", baseUrl, GetTimeStamp(), idCard, total);
                //    httpItem = new HttpItem()
                //    {
                //        Accept = "application/json, text/javascript, */*; q=0.01",
                //        ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                //        URL = Url,
                //        Method = "GET",
                //        Referer = baseUrl + "index.shtml",
                //        CookieCollection = cookies,
                //        ResultCookieType = ResultCookieType.CookieCollection
                //    };
                //    httpResult = httpHelper.GetHtml(httpItem);
                //    results = jsonParser.GetResultFromParser(httpResult.Html, "statusCode");
                //    if (httpResult.StatusCode != HttpStatusCode.OK || results != "200")
                //    {
                //        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //        return Res;
                //    }
                //    if (jsonParser.GetResultFromParser(httpResult.Html, "total").ToInt(0) > total)
                //        total = jsonParser.GetResultFromParser(httpResult.Html, "total").ToInt(0);
                //    else
                //        total = 0;
                //}
                //while (total > 100);

                //results = jsonParser.GetResultFromParser(httpResult.Html, "result");
                //if (!string.IsNullOrEmpty(results))
                //    sociaresults = jsonParser.DeserializeObject<List<WuHuSocia>>(results);

                //sociaresults = sociaresults.OrderByDescending(x => x.TZNY).ToList();
                //for (int i = 0; i < sociaresults.Count; i++)
                //{

                //    detailRes = new SocialSecurityDetailQueryRes();
                //    detailRes.Name = Res.Name;
                //    detailRes.PayTime = sociaresults[i].FKSSQ;
                //    detailRes.SocialInsuranceTime = sociaresults[i].TZNY;
                //    detailRes.SocialInsuranceBase = sociaresults[i].JFJS.ToDecimal(0);
                //    if (sociaresults[i].jfbz_name == "已实缴")
                //    {
                //        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                //        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                //    }
                //    else
                //    {
                //        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                //        detailRes.PaymentType = sociaresults[i].jfbz_name;
                //    }
                //    detailRes.CompanyName = sociaresults[i].DWMC;
                //    if (sociaresults[i].TZNY == sociaresults[i + 1].TZNY)
                //    {
                //        if (sociaresults[i].kx_name == "个人缴纳")
                //        {
                //            detailRes.PensionAmount = sociaresults[i].BQYJ.ToDecimal(0);
                //            detailRes.CompanyPensionAmount = sociaresults[i + 1].BQYJ.ToDecimal(0);
                //        }
                //        if (sociaresults[i].kx_name == "单位缴纳")
                //        {
                //            detailRes.PensionAmount = sociaresults[i + 1].BQYJ.ToDecimal(0);
                //            detailRes.CompanyPensionAmount = sociaresults[i].BQYJ.ToDecimal(0);
                //        }
                //        i++;
                //    }
                //    else
                //    {
                //        if (sociaresults[i].kx_name == "个人缴纳")
                //        {
                //            detailRes.PensionAmount = sociaresults[i].BQYJ.ToDecimal(0);
                //        }
                //        if (sociaresults[i].kx_name == "单位缴纳")
                //        {
                //            detailRes.CompanyPensionAmount = sociaresults[i].BQYJ.ToDecimal(0);
                //        }
                //    }
                //    if (detailRes.PaymentFlag != "欠费")
                //        PaymentMonths++;
                //    Res.Details.Add(detailRes);
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
        private void GetAllDetail(InfoType type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            string results = string.Empty;
            int total = 2000;//大概有50年明细
            List<WuHuSocia> sociaresults = new List<WuHuSocia>();
            Url = String.Format("{0}ahsb/personal/{4}={1}&idCard={2}&pagesize={3}&pageno=1", baseUrl, GetTimeStamp(), Res.IdentityCard, total, ((string[])PageHash[type])[0]);
            httpItem = new HttpItem()
            {
                Accept = "application/json, text/javascript, */*; q=0.01",
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                URL = Url,
                Referer = baseUrl + "index.shtml",
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            results = jsonParser.GetResultFromParser(httpResult.Html, "statusCode");
            if (httpResult.StatusCode != HttpStatusCode.OK || results != "200")
            {
                Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                return;
            }
            results = jsonParser.GetResultFromParser(httpResult.Html, "result");
            if (!string.IsNullOrEmpty(results))
                sociaresults = jsonParser.DeserializeObject<List<WuHuSocia>>(results).OrderByDescending(x => x.TZNY).ToList();

            for (int i = 0; i < sociaresults.Count; i++)
            {
                SocialSecurityDetailQueryRes detailRes = null;
                bool NeedSave = false;
                detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == sociaresults[i].TZNY);
                if (detailRes == null)
                {
                    NeedSave = true;
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.PayTime = sociaresults[i].FKSSQ;
                    detailRes.SocialInsuranceTime = sociaresults[i].TZNY;
                    detailRes.SocialInsuranceBase = sociaresults[i].JFJS.ToDecimal(0);
                    detailRes.CompanyName = sociaresults[i].DWMC;
                    if (sociaresults[i].jfbz_name == "已实缴")
                    {
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    }
                    else
                    {
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                        detailRes.PaymentType = sociaresults[i].jfbz_name;
                    }
                    switch (type)
                    {
                        case InfoType.养老保险:
                            if (sociaresults[i].TZNY == sociaresults[i + 1].TZNY)
                            {
                                if (sociaresults[i].kx_name != sociaresults[i + 1].kx_name)
                                {
                                    switch (sociaresults[i].kx_name)
                                    {
                                        case "个人缴纳":
                                            detailRes.PensionAmount = sociaresults[i].BQYJ.ToDecimal(0);
                                            detailRes.CompanyPensionAmount = sociaresults[i + 1].BQYJ.ToDecimal(0);
                                            break;
                                        case "单位缴纳":
                                            detailRes.PensionAmount = sociaresults[i + 1].BQYJ.ToDecimal(0);
                                            detailRes.CompanyPensionAmount = sociaresults[i].BQYJ.ToDecimal(0);
                                            break;
                                    }
                                }
                                else
                                {
                                    switch (sociaresults[i].kx_name)
                                    {
                                        case "个人缴纳":
                                            detailRes.MedicalAmount = sociaresults[i].BQYJ.ToDecimal(0) + sociaresults[i + 1].BQYJ.ToDecimal(0);
                                            break;
                                        case "单位缴纳":
                                            detailRes.CompanyMedicalAmount = sociaresults[i].BQYJ.ToDecimal(0) + sociaresults[i + 1].BQYJ.ToDecimal(0);
                                            break;
                                    }
                                }
                                i++;
                            }
                            else
                            {
                                switch (sociaresults[i].kx_name)
                                {
                                    case "个人缴纳":
                                        detailRes.PensionAmount = sociaresults[i].BQYJ.ToDecimal(0);
                                        break;
                                    case "单位缴纳":
                                        detailRes.CompanyPensionAmount = sociaresults[i].BQYJ.ToDecimal(0);
                                        break;
                                }
                            }
                            break;
                        case InfoType.医疗保险:
                            if (sociaresults[i].TZNY == sociaresults[i + 1].TZNY)
                            {
                                switch (sociaresults[i].xzlx_name)
                                {
                                    case "基本医疗保险":
                                        if (sociaresults[i].kx_name != sociaresults[i + 1].kx_name)
                                        {
                                            switch (sociaresults[i].kx_name)
                                            {
                                                case "个人缴纳":
                                                    detailRes.MedicalAmount = sociaresults[i].BQYJ.ToDecimal(0);
                                                    detailRes.CompanyMedicalAmount = sociaresults[i + 1].BQYJ.ToDecimal(0);
                                                    break;
                                                case "单位缴纳":
                                                    detailRes.CompanyMedicalAmount = sociaresults[i].BQYJ.ToDecimal(0);
                                                    detailRes.MedicalAmount = sociaresults[i + 1].BQYJ.ToDecimal(0);
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            switch (sociaresults[i].kx_name)
                                            {
                                                case "个人缴纳":
                                                    detailRes.MedicalAmount = sociaresults[i].BQYJ.ToDecimal(0) + sociaresults[i + 1].BQYJ.ToDecimal(0);
                                                    break;
                                                case "单位缴纳":
                                                    detailRes.CompanyMedicalAmount = sociaresults[i].BQYJ.ToDecimal(0) + sociaresults[i + 1].BQYJ.ToDecimal(0);
                                                    break;
                                            }
                                        }
                                        break;
                                    case "大病救助":
                                        detailRes.IllnessMedicalAmount = sociaresults[i].BQYJ.ToDecimal(0) + sociaresults[i + 1].BQYJ.ToDecimal(0);
                                        break;
                                }
                                i++;
                            }
                            else
                            {
                                switch (sociaresults[i].xzlx_name)
                                {
                                    case "基本医疗保险":
                                        switch (sociaresults[i].kx_name)
                                        {
                                            case "个人缴纳":
                                                detailRes.PensionAmount = sociaresults[i].BQYJ.ToDecimal(0);
                                                break;
                                            case "单位缴纳":
                                                detailRes.CompanyPensionAmount = sociaresults[i].BQYJ.ToDecimal(0);
                                                break;
                                        }
                                        break;
                                    case "大病救助":
                                        detailRes.IllnessMedicalAmount = sociaresults[i].BQYJ.ToDecimal(0);
                                        break;
                                }
                            }
                            break;
                        case InfoType.失业保险:
                            if (sociaresults[i].TZNY == sociaresults[i + 1].TZNY)
                            {
                                detailRes.UnemployAmount = sociaresults[i].BQYJ.ToDecimal(0) + sociaresults[i + 1].BQYJ.ToDecimal(0);
                                i++;
                            }
                            else
                            {
                                detailRes.UnemployAmount = sociaresults[i].BQYJ.ToDecimal(0);
                            }
                            break;
                        case InfoType.工伤保险:
                            detailRes.EmploymentInjuryAmount = sociaresults[i].BQYJ.ToDecimal(0);
                            break;
                        case InfoType.生育保险:
                            detailRes.MaternityAmount = sociaresults[i].BQYJ.ToDecimal(0);
                            break;
                    }
                }
                else
                {
                    switch (type)
                    {
                        case InfoType.养老保险:
                            switch (sociaresults[i].kx_name)
                            {
                                case "个人缴纳":
                                    detailRes.PensionAmount += sociaresults[i].BQYJ.ToDecimal(0);
                                    break;
                                case "单位缴纳":
                                    detailRes.CompanyPensionAmount += sociaresults[i].BQYJ.ToDecimal(0);
                                    break;
                            }
                            break;
                        case InfoType.医疗保险:
                            switch (sociaresults[i].kx_name)
                            {
                                case "个人缴纳":
                                    detailRes.MedicalAmount += sociaresults[i].BQYJ.ToDecimal(0);
                                    break;
                                case "单位缴纳":
                                    detailRes.CompanyMedicalAmount += sociaresults[i].BQYJ.ToDecimal(0);
                                    break;
                            }
                            break;
                        case InfoType.失业保险:
                            detailRes.UnemployAmount = +sociaresults[i].BQYJ.ToDecimal(0);
                            break;
                        case InfoType.工伤保险:
                            detailRes.EmploymentInjuryAmount += sociaresults[i].BQYJ.ToDecimal(0);
                            break;
                        case InfoType.生育保险:
                            detailRes.MaternityAmount += sociaresults[i].BQYJ.ToDecimal(0);
                            break;
                    }
                }

                if (detailRes.PaymentType != "欠费" && NeedSave)
                {
                    Res.Details.Add(detailRes);
                }
            }
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        private static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 01, 01, 00, 00, 00, 0000);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }

    }

    #region  实体

    /// <summary>
    /// 芜湖养老保险实体
    /// </summary>
    internal class WuHuSocia
    {
        /// <summary>
        /// 本期应缴
        /// </summary>
        public string BQYJ { get; set; }
        /// <summary>
        /// 款项
        /// </summary>
        public string kx_name { get; set; }
        /// <summary>
        /// 单位名称
        /// </summary>
        public string DWMC { get; set; }
        public string HZBZ { get; set; }
        public string KX { get; set; }
        /// <summary>
        /// 险种类型
        /// </summary>
        public string xzlx_name { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public string RN_RN { get; set; }
        /// <summary>
        /// 单位编号
        /// </summary>
        public string DWBH { get; set; }
        public string XZLX { get; set; }
        /// <summary>
        /// 个人编号
        /// </summary>
        public string GRBH { get; set; }
        /// <summary>
        /// 划入个人帐户金额
        /// </summary>
        public string HRJE { get; set; }
        /// <summary>
        /// 费款所属期
        /// </summary>
        public string FKSSQ { get; set; }
        /// <summary>
        /// 台帐年月
        /// </summary>
        public string TZNY { get; set; }
        /// <summary>
        /// 划帐日期
        /// </summary>
        public string HZRQ { get; set; }
        /// <summary>
        /// 缴费基数
        /// </summary>
        public string JFJS { get; set; }
        /// <summary>
        /// 划帐标志
        /// </summary>
        public string hzbz_name { get; set; }
        /// <summary>
        /// 所属城市
        /// </summary>
        public string tcm_name { get; set; }
        public string JFBZ { get; set; }
        /// <summary>
        /// 所属城市区号
        /// </summary>
        public string TCM { get; set; }
        /// <summary>
        /// 缴费标志
        /// </summary>
        public string jfbz_name { get; set; }
    }

    #endregion
}
