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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.JS
{
    public class changzhou : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://222.185.254.75:8080/wsbs/";
        string socialCity = "js_changzhou";
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
                Url = baseUrl + "login.do?method=begin";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
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
            string idCard = string.Empty;
            string results = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                Url = baseUrl + "j_unieap_security_check.do";
                postdata = String.Format("j_username=&j_password=&j_username2={0}&j_password2={1}&radio=on", MultiKeyDES.EncryptDES(socialReq.Identitycard, "wsbs", "cz", "cz"), MultiKeyDES.EncryptDES(socialReq.Password, "wsbs", "cz", "cz"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "login.do?method=begin",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                string err = CommonFun.GetMidStr(CommonFun.GetMidStr(httpResult.Html, "var loginErrorMsg = '", ""), "", "'");
                if (!err.IsEmpty())
                {
                    Res.StatusDescription = err;
                    Res.StatusCode = ServiceConsts.StatusCode_error;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取基本信息
                Url = baseUrl + "appwsbs/perbasicque.do?method=begin";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                Res.EmployeeNo = CommonFun.GetMidStr(httpResult.Html, "var aac001 = '", "'");
                if (Res.EmployeeNo.IsEmpty())
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = baseUrl + "ria_grid.do?method=paging";
                postdata = "{\"header\": {\"code\": -100, \"message\": {\"title\": \"\", \"detail\": \"\"}}, \"body\": {\"parameters\": {}, \"dataStores\": {\"gridDataStore\": {\"rowSet\": {\"primary\": [], \"delete\": [], \"filter\": []}, \"name\": \"gridDataStore\", \"pageNumber\": 1, \"pageSize\": 10, \"recordCount\": 0, \"rowSetName\": \"entinfo.pc01\", \"condition\": \"[PC01_AAC001]='" + Res.EmployeeNo + "'\"}}}}";
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    ContentType = "multipart / form - data",
                    Referer = baseUrl + "appwsbs/perbasicque.do?method=begin",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                try
                {
                    results = jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:gridDataStore:rowSet:primary");
                    results = CommonFun.GetMidStr(results, "[", "]");
                    if (string.IsNullOrEmpty(results))
                    {
                        Res.StatusDescription = "无社保信息";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                catch
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.Name = jsonParser.GetResultFromParser(results, "PC01_AAC003");//姓名
                Res.Sex = jsonParser.GetResultFromParser(results, "PC01_AAC004");//性别
                Res.IdentityCard = jsonParser.GetResultFromParser(results, "PC01_AAC002");//身份证号
                Res.CompanyName = jsonParser.GetResultFromParser(results, "PC01_AAB004");//公司名称
                Res.EmployeeStatus = jsonParser.GetResultFromParser(results, "PC01_RYZT");//人员状态
                //个人账户总额，缴费月数
                Url = baseUrl + "ria_grid.do?method=paging";
                postdata = "{\"header\": {\"code\": -100, \"message\": {\"title\": \"\", \"detail\": \"\"}}, \"body\": {\"parameters\": {}, \"dataStores\": {\"grqyqdDataStore\": {\"rowSet\": {\"primary\": [], \"delete\": [], \"filter\": []}, \"name\": \"grqyqdDataStore\", \"pageNumber\": 1, \"pageSize\": 10, \"recordCount\": 0, \"rowSetName\": \"si.v_qyd\", \"condition\": \"[V_QYD_AAC001]='" + Res.EmployeeNo + "'\"}}}}";
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    ContentType = "multipart / form - data",
                    Referer = baseUrl + "appwsbs/pages/si/pages/ggyw/grqyqd.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                try
                {
                    results = jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:grqyqdDataStore:rowSet:primary");
                    JArray detailList = JsonConvert.DeserializeObject(results) as JArray;
                    if (detailList != null && detailList.Count > 0)
                    {
                        Res.CompanyNo = detailList[0]["V_QYD_AAB001"].ToString();//公司编号
                        Res.PersonalInsuranceTotal = detailList[0]["V_QYD_FY_1120"].ToString().ToDecimal(0);//个人账户总额
                        Res.PaymentMonths = detailList[0]["V_QYD_FY_1121"].ToString().ToInt(0); //缴费月数
                    }
                }
                catch { }
                //参保信息
                Url = baseUrl + "ria_grid.do?method=paging";
                postdata = "{\"header\": {\"code\": -100, \"message\": {\"title\": \"\", \"detail\": \"\"}}, \"body\": {\"parameters\": {}, \"dataStores\": {\"gridDataStore\": {\"rowSet\": {\"primary\": [], \"delete\": [], \"filter\": []}, \"name\": \"gridDataStore\", \"pageNumber\": 1, \"pageSize\": 10, \"recordCount\": 0, \"rowSetName\": \"perinfo.pc02\", \"condition\": \"[PC02_AAC001]='" + Res.EmployeeNo + "'\"}}}}";
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    ContentType = "multipart / form - data",
                    Referer = baseUrl + "appwsbs/pages/si/pages/ggyw/grqyqd.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                try
                {
                    results = jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:gridDataStore:rowSet");
                    List<string> canbaolist = jsonParser.GetArrayFromParse(results, "primary");
                    Dictionary<string, int> canbao_status_dic = new Dictionary<string, int>();
                    Dictionary<string, int> canbao_type_dic = new Dictionary<string, int>();
                    int reflect = 0;
                    foreach (string item in canbaolist)
                    {
                        string canbao_type = jsonParser.GetResultFromParser(item, "PC02_AAE140");
                        string canbao_status = jsonParser.GetResultFromParser(item, "PC02_AAC031");
                        if (!canbao_type_dic.ContainsKey(canbao_type))
                        {
                            if (!canbao_status_dic.ContainsKey(canbao_status))
                            {
                                canbao_status_dic.Add(canbao_status, reflect);
                                reflect++;
                            }
                            canbao_type_dic.Add(canbao_type, canbao_status_dic[canbao_status]);

                        }
                    }
                    foreach (KeyValuePair<string, int> status_pair in canbao_status_dic)
                    {
                        string canbao_str = status_pair.Key + "：[";
                        foreach (string canbao_type in canbao_type_dic.Where(o => o.Value == status_pair.Value).Select(p => p.Key))
                        {
                            canbao_str += canbao_type + "，";
                        }
                        if (canbao_str.EndsWith("，"))
                        {
                            canbao_str = canbao_str.Remove(canbao_str.Length - 1);
                        }
                        canbao_str += "]";
                        Res.SpecialPaymentType += canbao_str;
                    }
                }
                catch { }
                #endregion

                #region 查询明细
                List<string> detail_type = new List<string> { "浼佷笟鍩烘湰", "鍩烘湰鍖荤枟", "澶变笟淇濋櫓", "宸ヤ激淇濋櫓", "鐢熻偛淇濋櫓" };//养老，医疗，失业，工伤，生育

                for (int i = 0; i < detail_type.Count; i++)
                {
                    results = string.Empty;
                    Url = baseUrl + "ria_grid.do?method=paging";
                    postdata = "{\"header\": {\"code\": -100, \"message\": {\"title\": \"\", \"detail\": \"\"}}, \"body\": {\"parameters\": {}, \"dataStores\": {\"gridDataStore\": {\"rowSet\": {\"primary\": [], \"delete\": [], \"filter\": []}, \"name\": \"gridDataStore\", \"pageNumber\": 1, \"pageSize\": 10000, \"recordCount\": 0, \"rowSetName\": \"perinfo.pc20\", \"condition\": \"[PC20_AAC001]='" + Res.EmployeeNo + "' and [PC20_AAE140] like '%" + detail_type[i] + "%'\"}}}}";
                    httpItem = new HttpItem()
                    {
                        Accept = "*/*",
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        ContentType = "multipart / form - data",
                        Referer = baseUrl + "appwsbs/perpayque.do?method=begin",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    List<changzhouSocial> sociaresults = new List<changzhouSocial>();
                    try
                    {
                        results = jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:gridDataStore:rowSet:primary");
                        sociaresults = jsonParser.DeserializeObject<List<changzhouSocial>>(results);
                    }
                    catch { continue; }
                    if (sociaresults == null)
                    {
                        continue;
                    }
                    foreach (changzhouSocial social in sociaresults)
                    {
                        string SocialInsuranceTime = social.PC20_AAE003;
                        detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                        bool NeedAdd = false;
                        if (detailRes == null)
                        {
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.PayTime = social.PC20_AAE002;
                            detailRes.SocialInsuranceTime = social.PC20_AAE003;
                            detailRes.SocialInsuranceBase = social.PC20_AAC1501.ToDecimal(0);
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            detailRes.CompanyName = social.PC20_AAB004;

                            NeedAdd = true;
                        }

                        switch(i)
                        {
                            case 0:
                                detailRes.CompanyPensionAmount = social.PC20_AAC1241.ToDecimal(0);
                                detailRes.PensionAmount = social.PC20_AAC1242.ToDecimal(0);
                                break;
                            case 1:
                                detailRes.CompanyMedicalAmount = social.PC20_AAC1241.ToDecimal(0);
                                detailRes.MedicalAmount = social.PC20_AAC1242.ToDecimal(0);
                                break;
                            case 2:
                                detailRes.UnemployAmount = social.PC20_AAC124.ToDecimal(0);
                                break;
                            case 3:
                                detailRes.EmploymentInjuryAmount = social.PC20_AAC124.ToDecimal(0);
                                break;
                            case 4:
                                detailRes.MaternityAmount = social.PC20_AAC124.ToDecimal(0);
                                break;
                        }

                        if (NeedAdd)
                        {
                            Res.Details.Add(detailRes);
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

    #region  实体

    /// <summary>
    /// 芜湖养老保险实体
    /// </summary>
    internal class changzhouSocial
    {
        public string PC20_AAB001 { get; set; }
        public string PC20_AAC001 { get; set; }
        public string PC20_AAC002 { get; set; }
        public string PC20_AAC003 { get; set; }
        public string PC20_AAE140 { get; set; }
        public string PC20_AAB004 { get; set; }
        public string PC20_AAC1501 { get; set; }
        public string PC20_AAC1502 { get; set; }
        public string PC20_AAE003 { get; set; }
        public string PC20_AAE002 { get; set; }
        public string PC20_AAC1241 { get; set; }
        public string PC20_AAC1242 { get; set; }
        public string PC20_AAC1243 { get; set; }
        public string PC20_AAC124 { get; set; }
    }

    #endregion
}
