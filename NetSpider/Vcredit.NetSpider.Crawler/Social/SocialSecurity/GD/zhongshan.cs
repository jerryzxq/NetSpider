using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.GD
{
    public class zhongshan : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.gdzs.si.gov.cn/";
        string socialCity = "gd_zhongshan";
        #endregion
        #region 私有变量

        enum InfoType
        {
            养老保险,
            医疗保险,
            失业保险,
            工伤保险,
            生育保险,
        }
        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息
        /// <summary>
        ///  将各种保险对应的页面所需信息存入PageHash
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, "B2_02_01");
            PageHash.Add(InfoType.医疗保险, "B2_06_01");
            PageHash.Add(InfoType.失业保险, "B2_04_01");
            PageHash.Add(InfoType.工伤保险, "B2_03_01");
            PageHash.Add(InfoType.生育保险, "B2_05_01");
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

            Url = baseUrl + "myprofile/index.action?appcode=" + PageHash[type];
            httpItem = new HttpItem()
            {
                URL = Url,
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tbody/tr", "");
            foreach (string item in results)
            {
                var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                if (tdRow.Count < 8) continue;
                DateTime begindate = DateTime.ParseExact(tdRow[0], "yyyyMM", null);
                int payMonths = tdRow[2].ToInt(0);
                //多月缴费合并显示做拆分处理
                for (int i = 0; i < payMonths; i++)
                {
                    string insuranceTime = begindate.AddMonths(i).ToString(Consts.DateFormatString7);
                    SocialSecurityDetailQueryRes detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == insuranceTime);
                    bool isSave = false;
                    if (detailRes == null)
                    {
                        isSave = true;
                        detailRes = new SocialSecurityDetailQueryRes();
                       
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;
                        detailRes.PayTime = detailRes.SocialInsuranceTime = insuranceTime;
                        detailRes.CompanyName = tdRow[4];
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    }
                    switch (type)
                    {
                        case InfoType.养老保险:
                            detailRes.PensionAmount += tdRow[7].ToDecimal(0) / payMonths;
                            detailRes.CompanyPensionAmount += tdRow[6].ToDecimal(0) / payMonths;
                            detailRes.SocialInsuranceBase = tdRow[5].ToDecimal(0);
                           
                            break;
                        case InfoType.医疗保险:
                            detailRes.CompanyMedicalAmount += tdRow[7].ToDecimal(0) / payMonths;
                            detailRes.MedicalAmount += tdRow[6].ToDecimal(0) / payMonths;
                            break;
                        case InfoType.失业保险:
                            detailRes.UnemployAmount += (tdRow[6].ToDecimal(0) + tdRow[7].ToDecimal(0)) / payMonths;
                            break;
                        case InfoType.工伤保险:
                            detailRes.EmploymentInjuryAmount += tdRow[6].ToDecimal(0) / payMonths;
                            break;
                        case InfoType.生育保险:
                            detailRes.MaternityAmount += tdRow[6].ToDecimal(0) / payMonths;
                            break;
                    }
                    if (isSave)
                    {
                        Res.Details.Add(detailRes);
                    }
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
                Url = baseUrl + "main/myprofile/index.action?keyword=A1_01_01";
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
                Url = baseUrl + "validateCodeServlet3";
                httpItem = new HttpItem()
                {
                    Accept = "image/png,image/*;q=0.8,*/*;q=0.5",
                    URL = Url,
                    Host = "www.gdzs.si.gov.cn",
                    Referer = baseUrl + "main/myprofile/index.action?keyword=A1_01_01",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
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
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                //type:1普通用户,2:认证用户
                bool renzhengPw=false;
                if (socialReq.LoginType=="2")
                {
                    if (string.IsNullOrEmpty(socialReq.Password))
                    {
                        renzhengPw = true;
                    }
                }
                if (socialReq.Identitycard.IsEmpty() || socialReq.Citizencard.IsEmpty() || socialReq.Vercode.IsEmpty() || renzhengPw)
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                Url = baseUrl + "ajax/login.action";
                postdata = String.Format("cardid={0}&taxid=&sbkid={1}&password={3}&code={2}&type={4}", socialReq.Identitycard, socialReq.Citizencard, socialReq.Vercode, socialReq.Password, socialReq.LoginType);
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
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
                JObject jobJson = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                if (jobJson["ret"].ToString() != "1")
                {
                    Res.StatusDescription = jobJson["prompt"].ToString();
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion
                #region 第二步， 获取基本信息

                //医疗待遇查询
                Url = baseUrl + "myprofile/index.action?appcode=A1_05_01";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='tablediv normal_table']/table/thead/tr[1]/th[1]", "");
                if (results.Count > 0)
                {
                    Res.Name = results[0].Replace("姓名：", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='tablediv normal_table']/table/thead/tr[1]/th[2]", "");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0].Replace("身份证号码：", "").Trim();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='tablediv normal_table']/table/thead/tr[2]/th[1]", "");
                if (results.Count > 0)
                {
                    string identityCard = results[0].Replace("社保卡号：", "").Trim();
                    if (!String.IsNullOrEmpty(identityCard))
                    {
                        Res.IdentityCard = identityCard;
                    }
                }
                //养老缴费情况
                Url = baseUrl + "myprofile/index.action?appcode=C1_02_01";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tbody/tr[last()]/td", "");
                if (results.Count == 7)
                {
                    Res.PaymentMonths = results[1].ToInt(0);
                    Res.PersonalInsuranceTotal = results[5].ToDecimal(0);
                    Res.InsuranceTotal = results[3].ToDecimal(0) + Res.PersonalInsuranceTotal;
                }
                Res.EmployeeNo = socialReq.Citizencard;
                #endregion
                #region 第三步，查询明细
                //普通版
                if (socialReq.LoginType == "1")
                {
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tbody/tr", "");
                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "");
                        if (tdRow.Count < 7) continue;
                        if (tdRow[0] == "历年合计") continue;
                        DateTime beginTime = DateTime.ParseExact(tdRow[0], "yyyyMM", null);
                        int payMonths = int.Parse(tdRow[2]);
                        for (int i = 0; i < payMonths; i++)
                        {
                            DateTime payTime = beginTime.AddMonths(i);
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.Name = Res.Name;
                            detailRes.IdentityCard = Res.IdentityCard;
                            detailRes.PayTime = beginTime.AddMonths(i).ToString(Consts.DateFormatString12);
                            detailRes.SocialInsuranceTime = payTime.ToString(Consts.DateFormatString7);
                            detailRes.SocialInsuranceBase = tdRow[3].ToDecimal(0);
                            detailRes.CompanyPensionAmount = tdRow[4].ToDecimal(0) / payMonths;
                            detailRes.PensionAmount = tdRow[6].ToDecimal(0);
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            Res.Details.Add(detailRes);
                        }
                    }
                }
                //认证版
                if (socialReq.LoginType == "2")
                {
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

