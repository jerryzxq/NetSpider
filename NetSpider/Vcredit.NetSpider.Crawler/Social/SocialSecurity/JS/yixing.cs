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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.JS
{
    public class yixing : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://218.90.170.222:7061/yxwsbs/";
        string socialCity = "js_yixing";
        #endregion

        #region 私有变量

        private class DetailInfo
        {
            public string T_WSBS_AC43_AAE180;
            public string T_WSBS_AC43_AAE002;
            public string T_WSBS_AC43_AAE003;
            public string T_WSBS_AC43_AAE140_11D;
            public string T_WSBS_AC43_AAE140_11G;
            public string T_WSBS_AC43_AAE140_21D;
            public string T_WSBS_AC43_AAE140_21G;
            public string T_WSBS_AC43_AAE140_31D;
            public string T_WSBS_AC43_AAE140_31G;
            public string T_WSBS_AC43_AAE140_41D;
            public string T_WSBS_AC43_AAE140_51D;
            public string T_WSBS_AB01_AAB004;
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
                Url = baseUrl + "login.do?method=begin";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
                if (socialReq.Username.IsEmpty() || socialReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = baseUrl + "j_unieap_security_check.do";
                postdata = string.Format("j_username={0}&j_password={1}&j_logintype=0&com_username=&com_password=&per_username={0}&per_password={1}", socialReq.Identitycard, socialReq.Username);
                httpItem = new HttpItem
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='errorDiv']");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，基本信息
                Url = baseUrl + "SiBusinessDelegateAction.do?method=submit&BUSINESS_REQUEST_ID=REQ-QS-A-001-01";
                postdata = "{header:{\"code\":0,\"message\":{\"title\":\"\",\"detail\":\"\"}},body:{dataStores:{\"basicInfoDs\":{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"basicInfoDs\",pageNumber:1,pageSize:0,recordCount:0,rowSetName:\"si.t_wsbs_ac01_ab01\",condition:\"t_wsbs_ac01.aac001 = '" + socialReq.Username + "'\"}},parameters:{}}}";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    ContentType = "multipart/form-data; charset=UTF-8",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                string basicinfo = jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:basicInfoDs:rowSet:primary");
                basicinfo = basicinfo.Remove(basicinfo.Length - 1, 1).Remove(0, 1);

                Res.Sex = jsonParser.GetResultFromParser(basicinfo, "T_WSBS_AC01_AAC004") == "1" ? "男" : "女";
                Res.CompanyName = jsonParser.GetResultFromParser(basicinfo, "T_WSBS_AB01_AAB004");
                Res.EmployeeNo = jsonParser.GetResultFromParser(basicinfo, "T_WSBS_AC01_AAC001");
                Res.IdentityCard = jsonParser.GetResultFromParser(basicinfo, "T_WSBS_AC01_AAC002");
                Res.Name = jsonParser.GetResultFromParser(basicinfo, "T_WSBS_AC01_AAC003");
                Res.SocialInsuranceBase = jsonParser.GetResultFromParser(basicinfo, "T_WSBS_AC01_AAE180").ToDecimal(0);

                Url = baseUrl + "SiBusinessDelegateAction.do?method=submit&BUSINESS_REQUEST_ID=REQ-QS-A-001-01";
                postdata = "{header:{\"code\":0,\"message\":{\"title\":\"\",\"detail\":\"\"}},body:{dataStores:{\"canbaoInfoDs\":{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"canbaoInfoDs\",pageNumber:1,pageSize:15,recordCount:0,rowSetName:\"si.t_wsbs_ac02_ab01\",condition:\"t_wsbs_ac02.aac001 = '" + socialReq.Username + "'\"}},parameters:{}}}";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    ContentType = "multipart/form-data; charset=UTF-8",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string canbaoinfo = jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:canbaoInfoDs:rowSet");
                results = jsonParser.GetArrayFromParse(canbaoinfo, "primary");
                Dictionary<string, List<string>> _canbao_info_dic = new Dictionary<string, List<string>>();
                Dictionary<string, string> _canbao_status = new Dictionary<string, string>();
                _canbao_status.Add("0", "未参保");
                _canbao_status.Add("1", "参保缴费");
                _canbao_status.Add("2", "暂停参保");
                _canbao_status.Add("3", "终止参保");
                Dictionary<string, string> _canbao_type = new Dictionary<string, string>();
                _canbao_type.Add("11", "企业基本养老保险");
                _canbao_type.Add("12", "机关事业养老保险");
                _canbao_type.Add("140", "农村养老保险（老农保）");
                _canbao_type.Add("15", "失地农民养老保险");
                _canbao_type.Add("150", "城乡居民养老保险(新农保)");
                _canbao_type.Add("21", "失业保险");
                _canbao_type.Add("31", "基本医疗保险");
                _canbao_type.Add("32", "大病医疗保险");
                _canbao_type.Add("33", "公务员医疗保险");
                _canbao_type.Add("35", "离休医疗保险");
                _canbao_type.Add("36", "荣军医疗保险");
                _canbao_type.Add("37", "补充医疗保险");
                _canbao_type.Add("38", "特殊病人补助");
                _canbao_type.Add("39", "居民医疗保险");
                _canbao_type.Add("41", "工伤保险");
                _canbao_type.Add("51", "生育保险");
                foreach (string item in results)
                {
                    try
                    {
                        string status = _canbao_status[jsonParser.GetResultFromParser(item, "T_WSBS_AC02_AAC008")];
                        string typename = _canbao_type[jsonParser.GetResultFromParser(item, "T_WSBS_AC02_AAE140")];
                        if (_canbao_info_dic.ContainsKey(status))
                        {
                            _canbao_info_dic[status].Add(typename);
                        }
                        else
                        {
                            _canbao_info_dic.Add(status, new List<string>() { typename });
                        }
                    }
                    catch { }
                }
                foreach(string key in _canbao_info_dic.Keys)
                {
                    List<string> alltype = _canbao_info_dic[key];
                    string type_str = alltype[0];
                    for (int i = 1; i < alltype.Count; i++)
                    {
                        type_str += "、" + alltype[i];
                    }
                    Res.SpecialPaymentType += key + "(" + type_str + ")";
                }
                #endregion

                #region 第三步，详细信息
                int pageNumber = 0;
                int recordCount = 0;
                results.Clear();
                do
                {
                    pageNumber++;
                    Url = baseUrl + "SiBusinessDelegateAction.do?method=submit&BUSINESS_REQUEST_ID=REQ-QS-A-001-01";
                    postdata = "{\"header\":{\"code\":0,\"message\":{\"title\":\"\",\"detail\":\"\"}},\"body\":{\"dataStores\":{\"jiaofeiInfoDs\":{\"rowSet\":{\"primary\":[],\"filter\":[],\"delete\":[]},\"name\":\"jiaofeiInfoDs\",\"pageNumber\":" + pageNumber + ",\"pageSize\":100,\"recordCount\":" + recordCount + ",\"rowSetName\":\"si.t_wsbs_ac43_ab01\",\"condition\":\"t_wsbs_ac43.aac001 = '" + socialReq.Username + "'\"}},\"parameters\":{}}}";
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        ContentType = "multipart/form-data; charset=UTF-8",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    if (recordCount == 0)
                        recordCount = jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:jiaofeiInfoDs:recordCount").ToInt(0);
                    string detailinfo = jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:jiaofeiInfoDs:rowSet");
                    results.AddRange(jsonParser.GetArrayFromParse(detailinfo, "primary"));
                }
                while (pageNumber < (recordCount / 100 + 1));

                foreach (string item in results)
                {
                    DetailInfo detail = jsonParser.DeserializeObject<DetailInfo>(item);
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;
                    detailRes.SocialInsuranceBase = detail.T_WSBS_AC43_AAE180.ToDecimal(0);
                    detailRes.PayTime = detail.T_WSBS_AC43_AAE002;
                    detailRes.SocialInsuranceTime = detail.T_WSBS_AC43_AAE003;
                    detailRes.CompanyPensionAmount = detail.T_WSBS_AC43_AAE140_11D.ToDecimal(0);
                    detailRes.PensionAmount = detail.T_WSBS_AC43_AAE140_11G.ToDecimal(0);
                    detailRes.CompanyMedicalAmount = detail.T_WSBS_AC43_AAE140_31D.ToDecimal(0);
                    detailRes.MedicalAmount = detail.T_WSBS_AC43_AAE140_31G.ToDecimal(0);
                    detailRes.UnemployAmount = detail.T_WSBS_AC43_AAE140_21D.ToDecimal(0) + detail.T_WSBS_AC43_AAE140_21G.ToDecimal(0);
                    detailRes.EmploymentInjuryAmount = detail.T_WSBS_AC43_AAE140_41D.ToDecimal(0);
                    detailRes.MaternityAmount = detail.T_WSBS_AC43_AAE140_51D.ToDecimal(0);
                    detailRes.CompanyName = detail.T_WSBS_AB01_AAB004;
                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    Res.Details.Add(detailRes);
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
