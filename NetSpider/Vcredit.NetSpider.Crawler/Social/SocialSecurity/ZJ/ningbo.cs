using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.ZJ
{
    public class ningbo : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://rzxt.nbhrss.gov.cn/nbsbk-rzxt/";
        string socialCity = "zj_ningbo";
        #endregion
        #region 私有变量

        enum InfoType
        {
            养老保险,
            //医疗保险,
            失业保险,
            工伤保险,
            生育保险
        }
        Hashtable PageHash = new Hashtable();
        /// <summary>
        /// 养老保险,查询年份信息
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, "CX1501");
            //PageHash.Add(InfoType.医疗保险, "CX2501");
            PageHash.Add(InfoType.失业保险, "CX3501");
            PageHash.Add(InfoType.工伤保险, "CX4501");
            PageHash.Add(InfoType.生育保险, "CX5501");
        }

        /// <summary>
        /// 获取缴费明细
        /// </summary>
        /// <param name="type">该年请求信息</param>
        /// <param name="access_token"></param>
        /// <param name="ljjfys">累计缴费月数</param>
        /// <param name="Res"></param>
        private void GetAllDetail(InfoType type, string access_token, ref int ljjfys, ref SocialSecurityQueryRes Res)
        {
            try
            {
                string Url = string.Empty;
                Url = baseUrl + string.Format("open-api/commapi?api={0}&access_token={1}", (string)PageHash[type], access_token);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (jsonParser.GetResultFromMultiNode(httpResult.Html, "result") == "{}")
                {
                    return;
                }
                if (type == InfoType.养老保险)
                {
                    Res.EmployeeStatus = jsonParser.GetResultFromMultiNode(httpResult.Html, "result:gk:cbzt");
                }
                Res.SpecialPaymentType += type.ToString() + ":" + jsonParser.GetResultFromMultiNode(httpResult.Html, "result:gk:cbzt") + ";";
            }
            catch (Exception)
            {
                throw new Exception(jsonParser.GetResultFromParser(httpResult.Html, "msg"));
            }
            List<Details> details = jsonParser.DeserializeObject<List<Details>>(jsonParser.GetResultFromMultiNode(httpResult.Html, "result:mx"));
            string insuranceTime;//应属年月
            string payTime;//应属年月
            foreach (Details item in details)
            {
                DateTime dt = DateTime.ParseExact(item.sbny, "yyyyMM", null);
                payTime = InfoType.养老保险 == type ? item.dzny : item.jzny;
                for (int i = 0; i < item.jfys; i++)
                {
                    insuranceTime = dt.AddMonths(i).ToString(Consts.DateFormatString7);
                    SocialSecurityDetailQueryRes detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == insuranceTime);
                    bool needSave = false;
                    if (detailRes == null)
                    {
                        if (type == InfoType.养老保险 & payTime != "托收途中")
                        {
                            ljjfys++;
                        }
                        needSave = true;
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;
                        detailRes.PayTime = payTime != "托收途中" ? payTime : string.Empty;
                        detailRes.SocialInsuranceTime = insuranceTime;
                        detailRes.PaymentFlag = payTime != "托收途中" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                        detailRes.PaymentType = payTime != "托收途中" ? ServiceConsts.SocialSecurity_PaymentType_Normal : payTime;
                    }
                    switch (type)
                    {
                        case InfoType.养老保险:
                            detailRes.PensionAmount += item.grjn / item.jfys;
                            detailRes.CompanyPensionAmount += item.dwjn / item.jfys;
                            break;
                        //case InfoType.医疗保险:
                        //    detailRes.MedicalAmount += item.grjn / item.jfys;
                        //    detailRes.CompanyMedicalAmount += item.dwjn / item.jfys;
                        //    break;
                        case InfoType.失业保险:
                            detailRes.UnemployAmount += item.zje / item.jfys; ;
                            break;
                        case InfoType.工伤保险:
                            detailRes.EmploymentInjuryAmount += item.zje / item.jfys;
                            break;
                        case InfoType.生育保险:
                            detailRes.MaternityAmount += item.zje / item.jfys;
                            break;
                    }
                    detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : item.jfjs;
                    if (!needSave) continue;
                    Res.Details.Add(detailRes);
                }
            }
        }
        #endregion
        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Res.StatusDescription = socialCity + "无需初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;
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
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                Url = baseUrl + "rzxt/nbhrssLogin.action";
                //http://rzxt.nbhrss.gov.cn/nbsbk-rzxt/rzxt/nbhrssLogin.action?id=420115198512259812&password=831027&_=1466754282867
                postdata = String.Format("id={0}&password={1}", socialReq.Identitycard, socialReq.Password);
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
                string errorMsg = jsonParser.GetResultFromParser(httpResult.Html, "msg");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg == "账号不存在" ? "账号不存在,请尝试升级账号后重试" : errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                string access_token = jsonParser.GetResultFromMultiNode(httpResult.Html, "result:access_token");
                //string client_no = jsonParser.GetResultFromMultiNode(httpResult.Html, "result:client_no");
                #endregion
                #region 第二步， 获取基本信息,当年养老明细

                Res.IdentityCard = jsonParser.GetResultFromMultiNode(httpResult.Html, "result:sfz");
                Res.Name = jsonParser.GetResultFromMultiNode(httpResult.Html, "result:xm");
                Res.EmployeeNo = jsonParser.GetResultFromMultiNode(httpResult.Html, "result:sbkh");
                //个人中心
                Url = baseUrl + "rzxt/hasSj.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (jsonParser.GetResultFromMultiNode(httpResult.Html, "result") == "0")
                {
                    Res.Phone = jsonParser.GetResultFromMultiNode(httpResult.Html, "sj");
                }
                //完善资料
                Url = baseUrl + "web/pages/rzxt/user.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='lxdz']", "value");
                if (results.Count > 0)
                {
                    Res.Address = results[0];
                }
                #endregion
                #region 第三步，查询养老明细
                //当前明细
                int ljjfys = 0;//累计缴费月数
                InitPageHash();
                foreach (InfoType type in Enum.GetValues(typeof(InfoType)))
                {
                    try
                    {
                        GetAllDetail(type, access_token, ref ljjfys, ref  Res);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                }
                #region 医保缴费状态
                Url = baseUrl + string.Format("open-api/commapi?api={0}&access_token={1}", "CX2501", access_token);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                try
                {
                    if (jsonParser.GetResultFromMultiNode(httpResult.Html, "result") != "{}")
                    {
                        Res.SpecialPaymentType += "医疗保险" + ":" + CommonFun.GetMidStr(httpResult.Html, "\\\"dqdyzt\\\":\\\"", "\\\",") + ";";
                    }
                }
                catch (Exception)
                {
                    Res.SpecialPaymentType = "";
                    goto Going;
                }


                #endregion
                #region 养老历史明细

            Going: int j = 0;
                while (true)
                {
                    Url = baseUrl + string.Format("open-api/commapi?api=CX1502&queryYear={0}&access_token={1}", DateTime.Now.AddYears(-2 - j).Year, access_token);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (jsonParser.GetResultFromMultiNode(httpResult.Html, "result") == "{}")
                    {
                        break;
                    }
                    //缴费基数
                    decimal salaryBase = jsonParser.GetResultFromMultiNode(httpResult.Html, "result:dnjfjszh").ToDecimal(0) / jsonParser.GetResultFromMultiNode(httpResult.Html, "result:dnjfys").ToInt(0);
                    //累计缴费月数
                    ljjfys += jsonParser.GetResultFromMultiNode(httpResult.Html, "result:ljjfys").ToInt(0);
                    Res.PaymentMonths = ljjfys;
                    //缴费明细
                    List<OldDetails> oldDetails = jsonParser.DeserializeObject<List<OldDetails>>(jsonParser.GetResultFromMultiNode(httpResult.Html, "result:dnjfdy"));
                    //是否缴费标识（1:缴费）
                    string[] payMonthSign = new string[] { };
                    foreach (OldDetails old in oldDetails)
                    {
                        if (CommonFun.GetMidStr(old.ftnotes, "积数:", "缴费").Trim() == "0") continue;
                        payMonthSign = CommonFun.GetMidStr(old.ftnotes, "缴费:", "").Trim().Replace("*", "-").Split('-');
                        break;
                    }
                    string[] jfyf = new string[] { "05", "06", "07", "08", "09", "10", "11", "12", "01", "02", "03", "04" };//年月(页面写死)
                    string payTime = string.Empty;
                    for (int i = 0; i < payMonthSign.Length; i++)
                    {
                        if (payMonthSign[i] != "1") continue;
                        if (i < 8)
                        {
                            payTime = DateTime.Now.AddYears(-2 - j).Year + jfyf[i];
                        }
                        else
                        {
                            payTime = (DateTime.Now.AddYears(-2 - j).Year + 1) + jfyf[i];
                        }
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;
                        detailRes.SocialInsuranceTime = detailRes.PayTime = payTime;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.SocialInsuranceBase = salaryBase;
                        Res.Details.Add(detailRes);
                    }
                    if (j == 1)
                    {
                        break;
                    }
                    j++;
                }
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
        #region 实体
        /// <summary>
        /// 当年明细
        /// </summary>
        internal class Details
        {
            public string date { get; set; }
            /// <summary>
            /// 个人部分
            /// </summary>
            public decimal grjn { get; set; }
            /// <summary>
            /// 单位缴费
            /// </summary>
            public decimal dwjn { get; set; }
            /// <summary>
            /// 总缴额
            /// </summary>
            public decimal zje { get; set; }
            /// <summary>
            /// 申报年月
            /// </summary>
            public string sbny { get; set; }
            /// <summary>
            /// 到账年月
            /// </summary>
            public string dzny { get; set; }
            /// <summary>
            /// 记账年月
            /// </summary>
            public string jzny { get; set; }
            /// <summary>
            /// 缴费基数
            /// </summary>
            public decimal jfjs { get; set; }
            /// <summary>
            /// 缴费月数
            /// </summary>
            public int jfys { get; set; }

        }
        /// <summary>
        /// 养老历史明细
        /// </summary>
        internal class OldDetails
        {
            private string _dbbz;
            public string dbbz
            {
                get { return _dbbz; }
                set
                {
                    switch (value)
                    {
                        case "0":
                            _dbbz = "基本";
                            break;
                        case "1":
                            _dbbz = "低标准";
                            break;
                        case "3":
                            _dbbz = "外来务工";
                            break;
                    }
                }
            }
            public string ftnotes { get; set; }
        }
        #endregion
    }
}
