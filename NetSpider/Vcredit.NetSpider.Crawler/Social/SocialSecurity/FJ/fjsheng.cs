using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.FJ
{
    /// <summary>
    /// 失业保险无可用实例,未抓取（2016年3月24日16:52:20）
    /// </summary>
    public class fjsheng : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.fj12333.gov.cn:268/fwpt/";
        string socialCity = "fj_fjsheng";
        #endregion
        #region 私有变量
        enum InfoType
        {
            基业职工基本养老保险,
            城镇职工基本医疗保险,
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
            PageHash.Add(InfoType.基业职工基本养老保险, new string[] { "queryCorpPersionPerFund.do", "110" });
            PageHash.Add(InfoType.城镇职工基本医疗保险, new string[] { "queryCorpMedicalPerFund.do", "310" });
            PageHash.Add(InfoType.失业保险, new string[] { "queryUnemployment.do", "" });
            PageHash.Add(InfoType.工伤保险, new string[] { "queryPerFund.do", "410" });
            PageHash.Add(InfoType.生育保险, new string[] { "queryBirthPerFund.do", "510" });
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
            int currentPage = 1;
            int totalPage = 1;
            DateTime dt = DateTime.Now;
            Url = baseUrl + ((string[])PageHash[type])[0];
            //未有失业保险实例,暂时不保存
            if (type == InfoType.失业保险)
            {
                return;
            }
            string postTypeStr = type == InfoType.失业保险 ? string.Format("aae030={0}&aae031={1}&ylzps_tr_=true", dt.AddYears(-3).ToString("yyyy01"), dt.ToString(Consts.DateFormatString7)) : string.Format("aae140={0}&aae041={1}&aae042={2}&ylzps_tr_=true", ((string[])PageHash[type])[1], dt.AddYears(-3).ToString("yyyy01"), dt.ToString(Consts.DateFormatString7));
            do
            {
                postData = string.Format("{0}&ylzps_p_={1}&ylzps_mr_=100", postTypeStr, currentPage);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postData,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                if (currentPage == 1)
                {
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@align='center']/label", "text", true);
                    if (results.Count > 0)
                    {
                        if (results[0].IndexOf("不到") > -1) break;
                    }
                    results = new List<string>();
                    totalPage = (int)Math.Ceiling(CommonFun.GetMidStr(httpResult.Html, "of", ".</td>").ToDecimal(0) / 100);
                }
                //标题
                results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='ylzps']/tbody/tr[position()>1]", ""));
                currentPage++;
            } while (currentPage <= totalPage);
            string insuranceTime = string.Empty;//应属时间
            int typeCount = 0;//td行数
            foreach (string s in results)
            {
                List<string> tdRow = HtmlParser.GetResultFromParser(s, "//td", "");
                switch (type)
                {
                    case InfoType.基业职工基本养老保险:
                    case InfoType.失业保险:
                        typeCount = 8;
                        break;
                    case InfoType.城镇职工基本医疗保险:
                        typeCount = 7;
                        break;
                    case InfoType.工伤保险:
                    case InfoType.生育保险:
                        typeCount = 4;
                        break;
                }
                if (tdRow.Count < typeCount) continue;
                switch (type)
                {
                    case InfoType.基业职工基本养老保险:
                    case InfoType.城镇职工基本医疗保险:
                    case InfoType.工伤保险:
                    case InfoType.生育保险:
                        insuranceTime = tdRow[0];
                        break;
                    case InfoType.失业保险:
                        insuranceTime = tdRow[1];
                        break;
                }
                SocialSecurityDetailQueryRes detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == insuranceTime);
                bool needSave = false;
                if (detailRes == null)
                {
                    needSave = true;
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;
                    detailRes.SocialInsuranceTime = insuranceTime;
                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    switch (type)
                    {
                        case InfoType.基业职工基本养老保险:
                            detailRes.CompanyName = tdRow[1];
                            detailRes.SocialInsuranceBase = tdRow[6].ToDecimal(0);
                            break;
                        case InfoType.城镇职工基本医疗保险:
                            detailRes.SocialInsuranceBase = tdRow[5].ToDecimal(0);
                            break;
                        case InfoType.失业保险:
                            break;
                        case InfoType.工伤保险:
                        case InfoType.生育保险:
                            detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                            break;
                    }
                }
                switch (type)
                {
                    case InfoType.基业职工基本养老保险:
                        detailRes.CompanyPensionAmount += tdRow[4].ToDecimal(0);
                        detailRes.PensionAmount += tdRow[5].ToDecimal(0);
                        break;
                    case InfoType.城镇职工基本医疗保险:
                        detailRes.MedicalAmount += tdRow[4].ToDecimal(0);
                        detailRes.CompanyMedicalAmount += tdRow[3].ToDecimal(0);
                        break;
                    case InfoType.失业保险:
                        break;
                    case InfoType.工伤保险:
                        detailRes.EmploymentInjuryAmount += tdRow[1].ToDecimal(0);
                        break;
                    case InfoType.生育保险:
                        detailRes.MaternityAmount += tdRow[1].ToDecimal(0);
                        break;
                }
                if (!needSave) continue;
                Res.Details.Add(detailRes);
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
                ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
                Url = baseUrl;
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

                Url = baseUrl + "Num.jsp";
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
            string postData = string.Empty;
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Vercode.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                Url = baseUrl + "login.html";
                postData = string.Format("aac003={0}&aac002={1}&ysc002={2}&randCode={3}", socialReq.Name.ToUrlEncode(), socialReq.Identitycard, CommonFun.GetMd5Str(socialReq.Password + "{PONY}"), socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postData,
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='default']/div/label", "");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0].Trim();
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion
                #region 第二步， 获取基本信息

                Res.Name = socialReq.Name;
                Res.IdentityCard = socialReq.Identitycard;
                //个人基本信息概览
                Url = baseUrl + "queryPersonInfo.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                List<string> baseResults = HtmlParser.GetResultFromParser(httpResult.Html.Replace("<!--", "").Replace("-->", ""), "//table[@id='result']", "");
                if (baseResults.Count > 0)
                {
                    results = HtmlParser.GetResultFromParser(baseResults[0], "//tr[2]/td[4]", "text");
                    if (results.Count > 0)
                    {
                        Res.Race = results[0];
                    }
                    results = HtmlParser.GetResultFromParser(baseResults[0], "//tr[3]/td[4]", "text");
                    if (results.Count > 0)
                    {
                        Res.Address = results[0].Trim();
                    }
                    results = HtmlParser.GetResultFromParser(baseResults[0], "//tr[4]/td[4]", "text");
                    if (results.Count > 0)
                    {
                        Res.Phone = results[0];
                    }
                    if (string.IsNullOrEmpty(Res.Phone))
                    {
                        results = HtmlParser.GetResultFromParser(baseResults[0], "//tr[4]/td[2]", "text");
                        if (results.Count > 0)
                        {
                            Res.Phone = results[0];
                        }
                    }
                    if (string.IsNullOrEmpty(Res.Address))
                    {
                        results = HtmlParser.GetResultFromParser(baseResults[0], "//tr[5]/td[2]", "text");
                        if (results.Count > 0)
                        {
                            Res.Address = results[0];
                        }
                    }
                    results = HtmlParser.GetResultFromParser(baseResults[0], "//tr[5]/td[4]", "text");
                    if (results.Count > 0)
                    {
                        Res.ZipCode = results[0].Trim().Replace("&nbsp;", "");
                    }
                }
                if (baseResults.Count > 1)
                {
                    results = HtmlParser.GetResultFromParser(baseResults[1], "//tr[1]/td[2]", "text");
                    if (results.Count > 0)
                    {
                        Res.InsuranceTotal = results[0].Trim().Replace("&nbsp;", "").ToDecimal(0);
                    }
                }
                if (baseResults.Count > 3)
                {
                    results = HtmlParser.GetResultFromParser(baseResults[3], "//tr[1]/td[2]", "text");
                    if (results.Count > 0)
                    {
                        Res.EmployeeNo = results[0];
                    }
                    results = HtmlParser.GetResultFromParser(baseResults[3], "//tr[1]/td[4]", "text");
                    if (results.Count > 0)
                    {
                        Res.EmployeeStatus = results[0];
                    }
                }
                //社保卡挂失
                Url = baseUrl + "getBaseInfoBefore.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='t1']/tr[5]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='t1']/tr[6]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.Address = results[0];
                }
                //企业职工基本养老保险(无实例)

                //城镇职工基本医疗保险
                Url = baseUrl + "queryCorpMedicalInsu.do?aae140=310";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='listArea']/table/tr[2]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.WorkDate = results[0];//参保日期
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='listArea']/table/tr[2]/td[4]", "");
                if (results.Count > 0)
                {
                    Res.SocialInsuranceBase = results[0].ToDecimal(0);//年度缴费基数
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='listArea']/table/tr[3]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.EmployeeStatus = results[0];//工作状态
                }
                //企业职工基本养老保险(无实例)
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
        /// <summary>
        /// 基础链接已关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private bool ValidateServerCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}

