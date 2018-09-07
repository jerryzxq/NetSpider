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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.HE
{
    class shijiazhuang_Info
    {
        public string FACTNAME { get; set; }
        public string IDCARDNO { get; set; }
        public string RACENAME { get; set; }
        public string SEX { get; set; }
        public string ADDRESS { get; set; }
        public string MOBILE { get; set; }
        public string AAZ500 { get; set; }
    }
    class shijiazhuang_Detail
    {
        public string AC43_AAE003 { get; set; }
        public string AC43_AAE140 { get; set; }
        public string AC43_AAE002 { get; set; }
        public string AC43_AAC040 { get; set; }
        public string AC43_AAE018 { get; set; }
        public string AC43_AAE020 { get; set; }
        public string AC43_AAE021 { get; set; }
        public string AC43_AAE022 { get; set; }
    }
    public class shijiazhuang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        //string baseUrl = "http://110.249.137.2:8080/eapdomain/";
        string baseUrl = "http://grsbcx.sjz12333.gov.cn/";
        string socialCity = "he_shijiazhuang";
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
                //第一步，页面初始化
                string pid = string.Empty;
                Url = baseUrl + "login.do?method=begin";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='pid']", "value");
                if (results.Count > 0)
                {
                    pid = results[0];
                }

                //第二步，获取图片验证码
                Url = baseUrl + "jcaptcha";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                //添加缓存
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("pid", pid);
                CacheHelper.SetCache(token, dics);
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
            Res.SocialSecurityCity = socialCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            SocialSecurityDetailQueryRes detail = null;
            int PaymentMonths = 0;
            try
            {
                string pid = string.Empty;
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(socialReq.Token);
                    pid = dics["pid"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
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
                postdata = String.Format("Method=P&pid={3}&j_username={0}&j_password={1}&jcaptcha_response={2}", socialReq.Identitycard, socialReq.Password, socialReq.Vercode, pid);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
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
                if (httpResult.Html.IndexOf("window.location.replace('../grlogo.jsp')") != -1)
                {
                    string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "')");
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if(httpResult.Html.IndexOf("si_LoginErrMsg=") != -1)
                {
                    string errorMsg = CommonFun.GetMidStr(httpResult.Html, "var si_LoginErrMsg=\"\";si_LoginErrMsg=\"", "\"").ToTrim("\\n") ;
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 第二步，获取基本信息

                socialReq.Citizencard = socialReq.Identitycard;
                Url = "http://110.249.137.2:8080/eapdomain/si/childmenu.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='treeheads']/tr[2]/td[2]");
                if (results.Count > 0)
                    Res.Name = results[0];

                Url = baseUrl + "ria_grid.do?method=query";
                //postdata = string.Format("{{header:{{\"code\": -100, \"message\": {{\"title\": \"\", \"detail\": \"\"}}}},body:{{dataStores:{{contentStore:{{rowSet:{{\"primary\":[],\"filter\":[],\"delete\":[]}},name:\"contentStore\",pageNumber:1,pageSize:2147483647,recordCount:0,statementName:\"si.treatment.ggfw.content\",attributes:{{\"AAC002\": [\"{0}\", \"12\"], \"AAE135\": [\"130105861030034\", \"12\"]}}}},xzStore:{{rowSet:{{\"primary\":[],\"filter\":[],\"delete\":[]}},name:\"xzStore\",pageNumber:1,pageSize:2147483647,recordCount:0,statementName:\"si.treatment.ggfw.xzxx\",attributes:{{\"AAC002\": [\"{0}\", \"12\"], \"AAE135\": [\"130105861030034\", \"12\"]}}}},userStore:{{rowSet:{{\"primary\":[],\"filter\":[],\"delete\":[]}},name:\"userStore\",pageNumber:1,pageSize:2147483647,recordCount:0,statementName:\"si.treatment.ggfw.grxx\",attributes:{{\"AAC002\": [\"{0}\", \"12\"], \"AAE135\": [\"130105861030034\", \"12\"]}}}},sbkxxStore:{{rowSet:{{\"primary\":[],\"filter\":[],\"delete\":[]}},name:\"sbkxxStore\",pageNumber:1,pageSize:2147483647,recordCount:0,statementName:\"si.treatment.ggfw.sbkxx\",attributes:{{\"AAC002\": [\"{0}\", \"12\"], \"AAE135\": [\"130105861030034\", \"12\"]}}}}}},parameters:{{\"BUSINESS_ID\": \"UCI314\", \"BUSINESS_REQUEST_ID\": \"REQ-IC-Q-098-60\", \"CUSTOMVPDPARA\": \"\", \"PAGE_ID\": \"\"}}}}}}", socialReq.Username);
                postdata = string.Format("{{header:{{\"code\": -100, \"message\": {{\"title\": \"\", \"detail\": \"\"}}}},body:{{dataStores:{{contentStore:{{rowSet:{{\"primary\":[],\"filter\":[],\"delete\":[]}},name:\"contentStore\",pageNumber:1,pageSize:2147483647,recordCount:0,statementName:\"si.treatment.ggfw.content\",attributes:{{\"AAC002\": [\"{0}\", \"12\"], \"AAE135\": [\"{0}\", \"12\"]}}}},xzStore:{{rowSet:{{\"primary\":[],\"filter\":[],\"delete\":[]}},name:\"xzStore\",pageNumber:1,pageSize:2147483647,recordCount:0,statementName:\"si.treatment.ggfw.xzxx\",attributes:{{\"AAC002\": [\"{0}\", \"12\"], \"AAE135\": [\"{0}\", \"12\"]}}}},userStore:{{rowSet:{{\"primary\":[],\"filter\":[],\"delete\":[]}},name:\"userStore\",pageNumber:1,pageSize:2147483647,recordCount:0,statementName:\"si.treatment.ggfw.grxx\",attributes:{{\"AAC002\": [\"{0}\", \"12\"], \"AAE135\": [\"{0}\", \"12\"]}}}},sbkxxStore:{{rowSet:{{\"primary\":[],\"filter\":[],\"delete\":[]}},name:\"sbkxxStore\",pageNumber:1,pageSize:2147483647,recordCount:0,statementName:\"si.treatment.ggfw.sbkxx\",attributes:{{\"AAC002\": [\"{0}\", \"12\"], \"AAE135\": [\"{0}\", \"12\"]}}}}}},parameters:{{\"BUSINESS_ID\": \"UCI314\", \"BUSINESS_REQUEST_ID\": \"REQ-IC-Q-098-60\", \"CUSTOMVPDPARA\": \"\", \"PAGE_ID\": \"\"}}}}}}", socialReq.Citizencard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    ContentType = "multipart/form-data; charset=UTF-8",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                string teamStr = jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:sbkxxStore:rowSet:primary");
                string ComStr = jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:xzStore:rowSet");
                Dictionary<string, List<string>> _status_list = new Dictionary<string, List<string>>();
                try
                {
                    List<string> AllInfo = jsonParser.GetArrayFromParse(ComStr, "primary");
                    foreach (string item in AllInfo)
                    {
                        string Type = jsonParser.GetResultFromParser(item, "AAE140_S");
                        if (Type == "企业养老保险")
                        {
                            Res.CompanyName = jsonParser.GetResultFromParser(item, "AAB004");
                            Res.CompanyNo = jsonParser.GetResultFromParser(item, "WORK");
                            Res.EmployeeStatus = jsonParser.GetResultFromParser(item, "AAC084");
                            break;
                        }
                        string date = jsonParser.GetResultFromMultiNode(item, "AAC049");
                        string status = jsonParser.GetResultFromMultiNode(item, "AAC008");
                        if (_status_list.ContainsKey(Type))
                        {
                            if (_status_list[Type][0].ToTrim("-").ToInt(0) < date.ToTrim("-").ToInt(0))
                            {
                                _status_list[Type][0] = date;
                                _status_list[Type][1] = status;
                            }
                        }
                        else
                        {
                            _status_list.Add(Type, new List<string>{date, status});
                        }
                    }
                }
                catch { }

                foreach (KeyValuePair<string, List<string>> pair in _status_list)
                {
                    Res.SpecialPaymentType += pair.Key + "：" + pair.Value[1] + "；";
                }

                var infos = jsonParser.DeserializeObject<List<shijiazhuang_Info>>(teamStr);
                if (infos.Count > 0)
                {
                    if(Res.Name.IsEmpty())
                        Res.Name = infos[0].FACTNAME;//姓名
                    //Res.Name = socialReq.Name;
                    Res.IdentityCard = infos[0].IDCARDNO;//身份证号
                    Res.Race = infos[0].RACENAME;//民族
                    Res.Sex = infos[0].SEX;//性别
                    Res.Address = infos[0].ADDRESS;//地址
                    Res.Phone = infos[0].MOBILE;//电话
                    Res.EmployeeNo = infos[0].AAZ500;//编号
                }

                Res.EmployeeStatus = CommonFun.GetMidStr(httpResult.Html, "\"AAC084\":\"", "\",\"ISTX");
                

                #endregion

                #region 第三步，查询明细
                List<shijiazhuang_Detail> details = null;
                Url = baseUrl + "ria_grid.do?method=query";
                postdata = string.Format("{{header:{{\"code\": -100, \"message\": {{\"title\": \"\", \"detail\": \"\"}}}},body:{{dataStores:{{searchStore:{{rowSet:{{\"primary\":[],\"filter\":[],\"delete\":[]}},name:\"searchStore\",pageNumber:1,pageSize:20,recordCount:0,context:{{\"BUSINESS_ID\": \"UOA017\", \"BUSINESS_REQUEST_ID\": \"REQ-OA-M-013-01\", \"CUSTOMVPDPARA\": \"\"}},statementName:\"si.treatment.ggfw.yljf\",attributes:{{\"AAC002\": [\"{0}\", \"12\"], \"AAE135\": [\"{0}\", \"12\"]}}}}}},parameters:{{\"BUSINESS_ID\": \"UOA017\", \"BUSINESS_REQUEST_ID\": \"REQ-OA-M-013-01\", \"CUSTOMVPDPARA\": \"\", \"PAGE_ID\": \"\"}}}}}}", socialReq.Citizencard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    ContentType = "multipart/form-data; charset=UTF-8",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                teamStr = jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:searchStore:rowSet:primary");
                details = jsonParser.DeserializeObject<List<shijiazhuang_Detail>>(teamStr);
               

                int recordCount = jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:searchStore:recordCount").ToInt(0);
                int totalPage = recordCount / 20 + 1;
                if (totalPage > 1)
                {
                    for (int i = 2; i <= totalPage; i++)
                    {
                        Url = baseUrl + "ria_grid.do?method=query";
                        postdata = string.Format("{{header:{{\"code\": -100, \"message\": {{\"title\": \"\", \"detail\": \"\"}}}},body:{{dataStores:{{searchStore:{{rowSet:{{\"primary\":[],\"filter\":[],\"delete\":[]}},name:\"searchStore\",pageNumber:" + i + ",pageSize:20,recordCount:" + recordCount + ",context:{{\"BUSINESS_ID\": \"UOA017\", \"BUSINESS_REQUEST_ID\": \"REQ-OA-M-013-01\", \"CUSTOMVPDPARA\": \"\"}},statementName:\"si.treatment.ggfw.yljf\",attributes:{{\"AAC002\": [\"{0}\", \"12\"], \"AAE135\": [\"{0}\", \"12\"]}}}}}},parameters:{{\"BUSINESS_ID\": \"UOA017\", \"BUSINESS_REQUEST_ID\": \"REQ-OA-M-013-01\", \"CUSTOMVPDPARA\": \"\", \"PAGE_ID\": \"\"}}}}}}", socialReq.Citizencard);
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "post",
                            Postdata = postdata,
                            ContentType = "multipart/form-data; charset=UTF-8",
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        teamStr = jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:searchStore:rowSet:primary");
                        details.AddRange(jsonParser.DeserializeObject<List<shijiazhuang_Detail>>(teamStr));

                    }
                }
                foreach (var item in details)
                {
                    detail = Res.Details.Where(o => o.SocialInsuranceTime == item.AC43_AAE003).FirstOrDefault();
                    if (detail != null)
                    {
                        if (item.AC43_AAE140 == "城镇职工基本医疗保险")
                        {
                            detail.MedicalAmount = item.AC43_AAE021.ToDecimal(0);
                            detail.CompanyMedicalAmount = item.AC43_AAE022.ToDecimal(0);
                        }
                        else if (item.AC43_AAE140 == "生育保险")
                        {
                            detail.MaternityAmount = item.AC43_AAE020.ToDecimal(0);
                        }
                    }
                    else
                    {
                        detail = new SocialSecurityDetailQueryRes();
                        detail.Name = Res.Name;
                        detail.IdentityCard = Res.IdentityCard;
                        detail.SocialInsuranceTime = item.AC43_AAE003;
                        detail.PayTime = item.AC43_AAE002;
                        detail.SocialInsuranceBase = item.AC43_AAE018.ToDecimal(0);
                        detail.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detail.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        if (item.AC43_AAE140 == "城镇职工基本医疗保险")
                        {
                            detail.MedicalAmount = item.AC43_AAE021.ToDecimal(0);
                            detail.CompanyMedicalAmount = item.AC43_AAE022.ToDecimal(0);
                        }
                        else if (item.AC43_AAE140 == "生育保险")
                        {
                            detail.MaternityAmount = item.AC43_AAE020.ToDecimal(0);
                        }
                        Res.Details.Add(detail);
                        //PaymentMonths++;
                    }
                }

                int pageNumber = 0;
                recordCount = 0;
                details = new List<shijiazhuang_Detail>();
                do
                {
                    pageNumber++;
                    Url = baseUrl + "ria_grid.do?method=query";
                    postdata = "{header:{\"code\": -100, \"message\": {\"title\": \"\", \"detail\": \"\"}},body:{dataStores:{searchStore:{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"searchStore\",pageNumber:" + pageNumber + ",pageSize:20,recordCount:" + recordCount + ",context:{\"BUSINESS_ID\": \"UOA017\", \"BUSINESS_REQUEST_ID\": \"REQ-OA-M-013-01\", \"CUSTOMVPDPARA\": \"\"},statementName:\"si.treatment.ggfw.syjf\",attributes:{\"AAC002\": [\"" + socialReq.Citizencard + "\", \"12\"], \"AAE135\": [\"" + socialReq.Citizencard + "\", \"12\"]}}},parameters:{\"BUSINESS_ID\": \"UOA017\", \"BUSINESS_REQUEST_ID\": \"REQ-OA-M-013-01\", \"CUSTOMVPDPARA\": \"\", \"PAGE_ID\": \"\"}}}";

                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        ContentType = "multipart/form-data; charset=UTF-8",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    teamStr = jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:searchStore:rowSet:primary");
                    details.AddRange(jsonParser.DeserializeObject<List<shijiazhuang_Detail>>(teamStr));

                    if(recordCount == 0)
                        recordCount = jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:searchStore:recordCount").ToInt(0);
                }
                while (pageNumber < (recordCount / 20 + 1));

                foreach(var item in details)
                {
                    detail = Res.Details.Where(o => o.SocialInsuranceTime == item.AC43_AAE003).FirstOrDefault();
                    if (detail != null)
                    {
                        detail.EmploymentInjuryAmount = item.AC43_AAE020.ToDecimal(0);
                    }
                    else
                    {
                        detail = new SocialSecurityDetailQueryRes();
                        detail.Name = Res.Name;
                        detail.SocialInsuranceTime = item.AC43_AAE003;
                        detail.PayTime = item.AC43_AAE002;
                        detail.SocialInsuranceBase = item.AC43_AAE018.ToDecimal(0);
                        detail.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detail.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detail.EmploymentInjuryAmount = item.AC43_AAE020.ToDecimal(0);
                        Res.Details.Add(detail);
                        //PaymentMonths++;
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
