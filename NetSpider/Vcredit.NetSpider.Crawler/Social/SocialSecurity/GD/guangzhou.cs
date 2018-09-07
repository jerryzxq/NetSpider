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
using System.Collections;
using Vcredit.NetSpider.DataAccess.Cache;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.GD
{
    public class guangzhou : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "";
        string socialCity = "gd_guangzhou";
        #endregion
        #region 私有变量

        enum InfoType
        {
            医疗保险,
            大病补助
        }
        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息

        private void GetDetails(InfoType type, List<string> results, ref SocialSecurityQueryRes Res)
        {
            SocialSecurityDetailQueryRes detailRes = null;
            foreach (string s in results)
            {
                var tdRow = HtmlParser.GetResultFromParser(s, "//td", "text");
                if (tdRow.Count < 9) continue;
                DateTime beginTime = DateTime.ParseExact(tdRow[1], "yyyyMM", null);
                int months = int.Parse(tdRow[3]);
                //一次多月缴费拆分处理
                for (int i = 0; i < months; i++)
                {
                    DateTime payTime = beginTime.AddMonths(i);
                    string insuranceTime = payTime.ToString("yyyyMM");
                    detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == insuranceTime);
                    bool isSave = false;
                    if (detailRes == null)
                    {
                        isSave = true;
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;
                        detailRes.CompanyName = tdRow[0] == ((List<string>)PageHash["companyNoNow"])[0] ? ((List<string>)PageHash["companyNoNow"])[1] : "";
                        detailRes.PayTime = payTime.ToString(Consts.DateFormatString2);
                        detailRes.SocialInsuranceTime = insuranceTime;
                        detailRes.SocialInsuranceBase = tdRow[8].ToDecimal(0);
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    }
                    
                    switch (type)
                    {
                        case InfoType.医疗保险:
                            detailRes.CompanyMedicalAmount += CommonFun.GetMidStr(tdRow[6], "getValue(", ")").ToDecimal(0) / months;
                            detailRes.MedicalAmount += CommonFun.GetMidStr(tdRow[7], "getValue(", ")").ToDecimal(0) / months;
                            break;
                        case InfoType.大病补助:
                            detailRes.IllnessMedicalAmount +=
                                (CommonFun.GetMidStr(tdRow[6], "getValue(", ")").ToDecimal(0) +
                                 CommonFun.GetMidStr(tdRow[7], "getValue(", ")").ToDecimal(0)) / months;
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
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://gzlss.hrssgz.gov.cn/cas/login";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    SecurityProtocolType = SecurityProtocolType.Ssl3,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = httpResult.CookieCollection;
                string lt = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='lt']", "value")[0];
                string modulus = CommonFun.GetMidStr(httpResult.Html, "var modulus=\"", "\";var exponent");

                Url = "http://gzlss.hrssgz.gov.cn/cas/captcha.jpg";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    SecurityProtocolType = SecurityProtocolType.Ssl3,
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
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);

                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                //添加缓存
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("lt", lt);
                dics.Add("modulus", modulus);
                SpiderCacheHelper.SetCache(token, dics);
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
            string lt = string.Empty;
            string modulus = string.Empty;
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)SpiderCacheHelper.GetCache(socialReq.Token);
                    lt = dics["lt"].ToString();
                    modulus = dics["modulus"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    SpiderCacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //校验参数
                if (modulus.IsEmpty())
                {
                    Res.StatusDescription = "请重新初始化";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = "http://gzlss.hrssgz.gov.cn:7001/cas/login?service=http%3A%2F%2Fgzlss.hrssgz.gov.cn%3A7001%2Fgzlss_web%2Fbusiness%2Fauthentication%2Flogin.xhtml";//http://gzlss.hrssgz.gov.cn/cas/login(原链接)
                Url = "http://gzlss.hrssgz.gov.cn/cas/login";
                //try
                //{
                //    while (modulus.StartsWith("0"))
                //    {
                //        modulus = modulus.Substring(1);
                //    }
                //    string a = Common.Utility.RSAUtil.EnCryptByKey(socialReq.Username, modulus);
                //    string b = Common.Utility.RSAUtil.EnCryptByKey(socialReq.Password, modulus);
                //}
                //catch { }
                //string a = Common.Utility.RSAUtil.EnCryptByKey("431021199408109039", @"008f94320c2e518393b16b1c9ac70a30f405b4ac1ffc1005a859c927573be8a0c6df96928ea5970a424a3d7ebb2b0dab14ab5b8233faefc5f16b7bd00285a5b96f5552fb55f0d741d0ede29f2a85791930c216214691fc2dbe56d3707ebce5efc85a482f09eaa971da67d3562ed9d4152ef5f9802498357b07d0b2b9f3a9e75a0b");
                //while (modulus.StartsWith("0"))
                //{
                //    modulus = modulus.Substring(1);
                //}
                //postdata = String.Format("username={0}&password={1}&yzm={2}&usertype=2&lt={3}&_eventId=submit", RSAEncrypt(socialReq.Username, modulus), RSAEncrypt(socialReq.Password, modulus), socialReq.Vercode, lt);

                postdata = String.Format("username={0}&password={1}&yzm={2}&usertype=2&lt={3}&_eventId=submit", RSAHelper.EncryptStringByRsaJS(socialReq.Username, modulus, "010001", "130"), RSAHelper.EncryptStringByRsaJS(socialReq.Password, modulus, "010001", "130"), socialReq.Vercode, lt);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",

                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header.Add("Accept-Language", "zh-cn,zh;");
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='*.errors']", "");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 其他步骤
                Url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/tomain/main.xhtml";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/tomain/main.xhtml";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/message/msg/refreshMsg.xhtml";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，查询基本信息
                Url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/getPersonInfoSearch.xhtml?querylog=true&businessocde=291QB-GRJCXX&visitterminal=PC-MENU";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                string aac001 = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='aac001']", "value")[0];
                string csrftoken = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='csrftoken']", "value")[0];

                Url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/realGetPersonInfoSearch.xhtml?querylog=true&businessocde=291QB-GRJCXX&visitterminal=PC";
                postdata = "csrftoken=" + csrftoken + "&pd=&type=1";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[1]/tr/td", "text", true);

                if (results.Count > 0)
                {
                    Res.IdentityCard = results[1];//身份证
                    Res.Sex = results[3].ToTrim("性");//性别
                    Res.BirthDate = results[5].ToTrim();//出生日期
                    Res.EmployeeStatus = results[7];//状态
                    Res.ZipCode = results[15];//状态
                    Res.IsSpecialWork = results[19] == "是" ? true : false;
                    Res.Address = results[25];//地址
                    Res.Race = results[29];//民族
                    Res.Phone = results[31];
                }
                #endregion

                #region 第三步，缴费历史明细表
                #region 养老,失业,工伤,生育
                Url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/getPersonPayHistoryInfoByPage.xhtml?querylog=true&businessocde=SBGRJFLSCX&visitterminal=PC";
                //Url = string.Format("http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/anonview/viewPersonPayHistoryInfo.xhtml?aac001={0}&xzType=1&startStr=&endStr=&querylog=true&businessocde=291QB-GRJFLS&visitterminal=PC", aac001);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);



                long time = (DateTime.Now.Ticks - 621355968000000000) / 10000;

                Url = string.Format("http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/statistics/logquery/getPCPreBusinessCount.xhtml?businessCode=291QB-GRJFLS&_={0}", time);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/getPersonPayHistoryInfoByPage.xhtml?querylog=true&businessocde=SBGRJFLSCX&visitterminal=PC",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                Url = string.Format("http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/viewPage/viewPersonPayHistoryInfo.xhtml?aac001={0}&xzType=1&startStr=&endStr=&querylog=true&businessocde=291QB-GRJFLS&visitterminal=PC", aac001);
                //Url = string.Format("http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/anonview/viewPersonPayHistoryInfo.xhtml?aac001={0}&xzType=1&startStr=&endStr=&querylog=true&businessocde=291QB-GRJFLS&visitterminal=PC", aac001);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/getPersonPayHistoryInfoByPage.xhtml?querylog=true&businessocde=SBGRJFLSCX&visitterminal=PC",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //个人信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr");
                List<string> infos = HtmlParser.GetResultFromParser(results[0] + results[1], "//span", "text", true);
                if (infos.Count > 0)
                {
                    Res.EmployeeNo = infos[0].ToTrim("&nbsp;").ToTrim("个人编号：");
                    Res.Name = infos[1].ToTrim("&nbsp;").ToTrim("姓名：");
                    //Res.IdentityCard = infos[2].ToTrim("&nbsp;").ToTrim("证件号码：");
                    Res.CompanyName = infos[4].ToTrim("&nbsp;").ToTrim("现在单位名称:");
                }

                DateTime start = new DateTime();
                int monthCount = 0;
                for (int i = 5; i < results.Count - 3; i++)
                {
                    var tdRow = HtmlParser.GetResultFromParser(results[i], "//td");
                    if (tdRow[0].Contains("分险种月数统计"))
                        Res.PaymentMonths = tdRow[1].ToInt(0);

                    if (tdRow.Count != 13 || tdRow[0] == "&nbsp;")
                    {
                        continue;
                    }
                    DateTime? dateTime = tdRow[0].ToDateTime(Consts.DateFormatString7);
                    if (dateTime != null)
                        start = (DateTime)dateTime;
                    monthCount = tdRow[2].ToInt(1);
                    for (int j = 0; j < monthCount; j++)
                    {
                        string insuranceTime = start.AddMonths(j).ToString(Consts.DateFormatString7);
                        detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == insuranceTime);
                        bool isSave = false;
                        if (detailRes == null)
                        {
                            isSave = true;
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.Name = Res.Name;
                            detailRes.IdentityCard = Res.IdentityCard;
                            detailRes.CompanyName = tdRow[11].ToTrim();
                            detailRes.PayTime = start.AddMonths(j).ToString(Consts.DateFormatString2);
                            detailRes.SocialInsuranceTime = insuranceTime;
                            detailRes.SocialInsuranceBase = tdRow[3].ToDecimal(0);
                        }
                        detailRes.PaymentType = tdRow[12] == "正常" ? ServiceConsts.SocialSecurity_PaymentType_Normal : tdRow[12];
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        //保存养老保险缴费基数
                        if (tdRow[4].ToDecimal(0) > 0)
                        {
                            detailRes.SocialInsuranceBase = tdRow[3].ToDecimal(0);
                        }
                        //养老
                        detailRes.CompanyPensionAmount += tdRow[4].ToTrim().ToDecimal(0) / monthCount;
                        detailRes.PensionAmount += tdRow[5].ToTrim().ToDecimal(0) / monthCount;
                        //失业
                        detailRes.UnemployAmount += (tdRow[6].ToTrim().ToDecimal(0) + tdRow[7].ToTrim().ToDecimal(0)) / monthCount;
                        //工伤
                        detailRes.EmploymentInjuryAmount += tdRow[8].ToTrim().ToDecimal(0) / monthCount;
                        //生育
                        detailRes.MaternityAmount += tdRow[9].ToTrim().ToDecimal(0) / monthCount;
                        if (isSave)
                        {
                            Res.Details.Add(detailRes);
                        }
                    }
                }
                #endregion
                #region 职工医疗,大病补助

                Url = string.Format("http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/getHealthcarePersonPayHistorySumup.xhtml?query=1&querylog=true&businessocde=291QB_YBGRJFLSCX&visitterminal=PC&aac001={0}&startStr=195001&endStr={1}", aac001, DateTime.Now.ToString("yyyyMM"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //现在工作单位编号
                string companyNoNow = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tableDataList']/tr[3]/td[2]", "text", true)[0];
                //现在工作单位名称
                string companyNameNow = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tableDataList']/tr[3]/td[4]", "text", true)[0];
                if (Res.CompanyName == companyNameNow)
                {
                    Res.CompanyNo = companyNoNow;
                }
                //当前工作单位信息
                PageHash.Add("companyNoNow", new List<string>()
                {
                    companyNoNow,companyNameNow
                
                });
                //医疗保险
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tableDataList']/tr[@temp='城职职工基本医疗保险']", "", true);
                GetDetails(InfoType.医疗保险, results, ref Res);
                //大病补助
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tableDataList']/tr[@temp='重大疾病医疗补助']", "", true);
                GetDetails(InfoType.大病补助, results, ref Res);
                #endregion
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
