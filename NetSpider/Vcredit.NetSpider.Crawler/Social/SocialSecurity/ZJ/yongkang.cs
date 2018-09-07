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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.ZJ
{
    public class yongkang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://60.191.235.211:12333/yk12333/";
        string socialCity = "zj_yongkang";
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
            PageHash.Add(InfoType.养老保险, "1");
            PageHash.Add(InfoType.医疗保险, "3");
            PageHash.Add(InfoType.失业保险, "2");
            PageHash.Add(InfoType.工伤保险, "4");
            PageHash.Add(InfoType.生育保险, "5");
        }
        /// <summary>
        /// 获取缴费明细
        /// </summary>
        /// <param name="type">缴费类型</param>
        /// <param name="Res"></param>
        private void GetAllDetail(InfoType type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            List<YongkangDetails> details = new List<YongkangDetails>();
            string postdata = string.Empty;
            int pageNumber = 1;
            int pageCount = 1;
            int recordCount = 0;
            string endTime = DateTime.Now.ToString(Consts.DateFormatString7);
            Url = baseUrl + "rpc.do?method=doQuery";
            do
            {
                postdata = "{header:{\"code\":0,\"message\":{\"title\":\"\",\"detail\":\"\"}},body:{dataStores:{\"queryStore\":{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"queryStore\",pageNumber:" + pageNumber + ",pageSize:300,recordCount:" + recordCount + ",rowSetName:\"appwscx.v_grjfxx\",order:\"[AAE002] desc \",condition:\"[AAC001]='" + Res.EmployeeNo + "' AND [AAE002]>='196501' AND [AAE002]<='" + endTime + "' AND [AAE130]='" + PageHash[type] + "' AND [AAE140] !='33'\"}},parameters:{\"synCount\":\"true\"}}}";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Host = "60.191.235.211:12333",
                    Method = "POST",
                    Encoding = Encoding.UTF8,
                    ContentType = "multipart/form-data",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header["X-Requested-With"] = "XMLHttpRequest";
                httpItem.Header["ajaxRequest"] = "true";
                httpResult = httpHelper.GetHtml(httpItem);
                if (pageNumber == 1)
                {
                    recordCount = jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:queryStore:recordCount").ToInt(0);
                    pageCount = (int)Math.Ceiling(recordCount / 300M);
                }
                details.AddRange(jsonParser.DeserializeObject<List<YongkangDetails>>(jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:queryStore:rowSet:primary")));
                pageNumber++;
            } while (pageNumber <= pageCount);
            foreach (YongkangDetails item in details)
            {
                SocialSecurityDetailQueryRes detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == item.AAE003);
                bool isSave = false;
                if (detailRes == null)
                {
                    isSave = true;
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = item.AAC003;
                    detailRes.IdentityCard = item.AAC002;
                    detailRes.PayTime = item.AAE00A;
                    detailRes.SocialInsuranceTime = item.AAE003;
                    detailRes.PaymentFlag = item.AAE143;
                    switch (detailRes.PaymentFlag)
                    {
                        case ServiceConsts.SocialSecurity_PaymentFlag_Adjust:
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                            break;
                        case ServiceConsts.SocialSecurity_PaymentFlag_Normal:
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            break;
                        case ServiceConsts.SocialSecurity_PaymentFlag_Back:
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Back;
                            break;
                    }
                }
                switch (type)
                {
                    case InfoType.养老保险:
                        detailRes.PensionAmount += item.GRJ;
                        detailRes.CompanyPensionAmount += item.DWJ;
                        detailRes.SocialInsuranceBase = item.JFJS;
                        break;
                    case InfoType.医疗保险:
                        detailRes.MedicalAmount += item.GRJ;
                        detailRes.CompanyMedicalAmount += item.DWJ;
                        detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : item.JFJS;
                        break;
                    case InfoType.失业保险:
                        detailRes.UnemployAmount += (item.GRJ + item.DWJ);
                        break;
                    case InfoType.工伤保险:
                        detailRes.EmploymentInjuryAmount += item.DWJ;
                        break;
                    case InfoType.生育保险:
                        detailRes.MaternityAmount += item.DWJ;
                        break;
                }
                detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : item.JFJS;
                if (isSave)
                {
                    Res.Details.Add(detailRes);
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
                Url = baseUrl + "validatecode";
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
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Vercode.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                Url = baseUrl + "login.do?method=plogin";
                postdata = String.Format("username={0}&password={1}&validateCode={2}", socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
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
                string appCode = jsonParser.GetResultFromParser(httpResult.Html, "appCode");
                if (appCode != "1")
                {
                    Res.StatusDescription = jsonParser.GetResultFromParser(httpResult.Html, "message");
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                //获取员工编号
                Url = baseUrl + "appwszz/pages/wscxmodel/jbxxcx/jbxxcx.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                Res.EmployeeNo = CommonFun.GetMidStr(httpResult.Html, "var aac001 = '", "';</script>");
                if (string.IsNullOrEmpty(Res.EmployeeNo))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion
                #region 第二步， 获取基本信息

                //个人基本信息
                Url = baseUrl + "rpc.do?method=doQuery";
                postdata = "{header:{\"code\":0,\"message\":{\"title\":\"\",\"detail\":\"\"}},body:{dataStores:{\"queryStore\":{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"queryStore\",pageNumber:1,pageSize:30,recordCount:0,rowSetName:\"appwscx.ac01\",condition:\"[AAC001]='" + Res.EmployeeNo + "'\"}},parameters:{\"synCount\":\"true\"}}}";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Host = "60.191.235.211:12333",
                    Method = "POST",
                    Encoding = Encoding.UTF8,
                    ContentType = "multipart/form-data",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header["X-Requested-With"] = "XMLHttpRequest";
                httpItem.Header["ajaxRequest"] = "true";
                httpResult = httpHelper.GetHtml(httpItem);

                DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
                Res.Name = socialReq.Name;
                Res.IdentityCard = socialReq.Identitycard;
                Res.Name = CommonFun.GetMidStr(httpResult.Html, "AAC003\": \"", "\",\"AIC162");
                string resultStr = jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:queryStore:rowSet:primary");
                JArray jObj = (JArray)JsonConvert.DeserializeObject(resultStr);
                Res.Name = jObj[0]["AAC003"].ToString();
                Res.EmployeeNo = jObj[0]["AAC001"].ToString();
                Res.IdentityCard = jObj[0]["AAC002"].ToString();
                Res.BirthDate = dtStart.AddMilliseconds((long)jObj[0]["AAC006"]).ToString(Consts.DateFormatString5);
                Res.WorkDate = dtStart.AddMilliseconds((long)jObj[0]["AAC007"]).ToString(Consts.DateFormatString5);
                Res.Phone = jObj[0]["AAE005"].ToString();
                Res.Address = jObj[0]["AAE006"].ToString();
                //参加险种信息
                Url = baseUrl + "rpc.do?method=doQuery";
                postdata = "{header:{\"code\":0,\"message\":{\"title\":\"\",\"detail\":\"\"}},body:{dataStores:{\"queryStore\":{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"queryStore\",pageNumber:1,pageSize:30,recordCount:0,rowSetName:\"appwscx.ac02\",condition:\"[AAC001]='" + Res.EmployeeNo + "'\"}},parameters:{\"synCount\":\"true\"}}}";
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Host = "60.191.235.211:12333",
                    Method = "POST",
                    Encoding = Encoding.UTF8,
                    ContentType = "multipart/form-data",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header["X-Requested-With"] = "XMLHttpRequest";
                httpItem.Header["ajaxRequest"] = "true";
                httpResult = httpHelper.GetHtml(httpItem);
                resultStr = jsonParser.GetResultFromMultiNode(httpResult.Html, "body:dataStores:queryStore:rowSet:primary");
                List<YongkangSpecial> special = jsonParser.DeserializeObject<List<YongkangSpecial>>(resultStr);
                foreach (YongkangSpecial item in special)
                {
                    Res.SpecialPaymentType += item.AAE140 + ":" + item.AAC031 + ";";
                    if (item.AAE140 == "企业基本养老保险" || item.AAE140 == "事业养老保险")
                    {
                        Res.CompanyName = item.AAB004;//单位名称
                        Res.WorkDate = item.AAC131;//工作时间
                    }
                }
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
        /// 参加险种信息
        /// </summary>
        internal class YongkangSpecial
        {
            /// <summary>
            /// 首次参保时间
            /// </summary>
            public string AAC131 { get; set; }
            /// <summary>
            /// 单位名称
            /// </summary>
            public string AAB004 { get; set; }
            /// <summary>
            /// 经办时间
            /// </summary>
            public string AAE036 { get; set; }
            /// <summary>
            /// 停保时间
            /// </summary>
            public string BAC061 { get; set; }
            /// <summary>
            /// 接单时间
            /// </summary>
            public string AAC026 { get; set; }
            private string _AAC031 = string.Empty;
            /// <summary>
            /// 参保状态
            /// </summary>
            public string AAC031
            {
                get { return _AAC031; }
                set
                {
                    switch (value)
                    {
                        case "1":
                            _AAC031 = "正常参保";
                            break;
                        case "2":
                            _AAC031 = "暂停参保";
                            break;
                        case "3":
                            _AAC031 = "终止参保";
                            break;
                    }
                }
            }
            private string _AAE140 = string.Empty;
            /// <summary>
            /// 参保险种
            /// </summary>
            public string AAE140
            {
                get { return _AAE140; }
                set
                {
                    switch (value)
                    {
                        case "38":
                            _AAE140 = "城乡居民医保";
                            break;
                        case "39":
                            _AAE140 = "居民医保";
                            break;
                        case "41":
                            _AAE140 = "工伤保险";
                            break;
                        case "42":
                            _AAE140 = "建筑工伤保险";
                            break;
                        case "52":
                            _AAE140 = "生育保险";
                            break;
                        case "11":
                            _AAE140 = "企业基本养老保险";
                            break;
                        case "15":
                            _AAE140 = "事业养老保险";
                            break;
                        case "16":
                            _AAE140 = "城乡居民养老保险";
                            break;
                        case "17":
                            _AAE140 = "土地被征人员养老保险";
                            break;
                        case "19":
                            _AAE140 = "农村养老保险";
                            break;
                        case "21":
                            _AAE140 = "失业保险";
                            break;
                        case "31":
                            _AAE140 = "基本医疗保险";
                            break;
                        case "32":
                            _AAE140 = "公务员补助医疗保险";
                            break;
                        case "33":
                            _AAE140 = "重大疾病补助";
                            break;
                        case "34":
                            _AAE140 = "离休医疗保险";
                            break;
                        case "35":
                            _AAE140 = "劳模补助医疗保险";
                            break;
                        case "36":
                            _AAE140 = "二乙医疗保险";
                            break;
                        case "37":
                            _AAE140 = "住院医疗保险";
                            break;

                    }
                }
            }

        }
        /// <summary>
        /// 缴费明细
        /// </summary>
        internal class YongkangDetails
        {
            /// <summary>
            /// 个人缴纳
            /// </summary>
            public decimal GRJ { get; set; }
            /// <summary>
            /// 到账年月
            /// </summary>
            public string AAE00A { get; set; }
            /// <summary>
            /// 个人编号
            /// </summary>
            public string AAC001 { get; set; }

            /// <summary>
            /// 单位名称
            /// </summary>
            public string AAB004 { get; set; }

            /// <summary>
            /// 结算年月
            /// </summary>
            public string AAE002 { get; set; }
            /// <summary>
            /// 单位划入
            /// </summary>
            public decimal DWHR { get; set; }
            /// <summary>
            /// 缴费基数
            /// </summary>
            public decimal JFJS { get; set; }

            private string _AAE130 = string.Empty;

            /// <summary>
            /// 险种
            /// </summary>
            public string AAE130
            {
                get { return _AAE130; }
                set
                {
                    switch (value)
                    {
                        case "1":
                            _AAE130 = "养老保险";
                            break;
                        case "2":
                            _AAE130 = "失业保险";
                            break;
                        case "3":
                            _AAE130 = "医疗保险";
                            break;
                        case "4":
                            _AAE130 = "工伤保险";
                            break;
                        case "5":
                            _AAE130 = "生育保险";
                            break;
                    }
                }
            }
            /// <summary>
            /// 姓名
            /// </summary>
            public string AAC003 { get; set; }
            /// <summary>
            /// 单位缴纳
            /// </summary>
            public decimal DWJ { get; set; }

            private  string _AAE143 = string.Empty;
            /// <summary>
            /// 缴费类型
            /// </summary>
            public string AAE143
            {
                get { return _AAE143; }
                set
                {
                    if (string.IsNullOrEmpty(AAE00A))//到账年月为空
                    {
                        _AAE143 = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                    }
                    else if (int.Parse(value) < 11 & value != "09")
                    {
                        _AAE143 = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    }
                    else if (value == "11" || value == "16" || value == "21" || value == "22" || value == "26" ||
                             value == "27")
                    {
                        _AAE143 = ServiceConsts.SocialSecurity_PaymentFlag_Back;
                    }
                    else
                    {
                        _AAE143 = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                    }
                }
            }

            /// <summary>
            /// 个人划入
            /// </summary>
            public decimal GRHR { get; set; }
            /// <summary>
            /// 身份证号
            /// </summary>
            public string AAC002 { get; set; }
            /// <summary>
            /// 缴费年月
            /// </summary>
            public string AAE003 { get; set; }
            /// <summary>
            /// 险种小类
            /// </summary>
            public string AAE140 { get; set; }
        }
    }
}
