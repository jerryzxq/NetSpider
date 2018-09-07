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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.JS
{
    public class zhenjiang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.hrsszj.gov.cn/ggfw/";
        string socialCity = "js_zhenjiang";
        #endregion
        enum InfoType
        {
            企业基本养老保险,
            //补充养老保险,
            职工基本医疗保险,
            大病医疗费用统筹,
            公务员补充医疗保险,
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
            PageHash.Add(InfoType.企业基本养老保险, new string[] { "110" });
            //PageHash.Add(InfoType.补充养老保险, new string[] { "190" });
            PageHash.Add(InfoType.职工基本医疗保险, new string[] { "310" });
            PageHash.Add(InfoType.大病医疗费用统筹, new string[] { "330" });
            PageHash.Add(InfoType.公务员补充医疗保险, new string[] { "320" });
            PageHash.Add(InfoType.失业保险, new string[] { "210" });
            PageHash.Add(InfoType.工伤保险, new string[] { "410" });
            PageHash.Add(InfoType.生育保险, new string[] { "510" });
        }
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
                Url = baseUrl + "login.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "workspace/createVerificationCode.action?randomcode=0.9625507328883977";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl + "login.jsp",
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
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                Url = baseUrl + "login.action";
                postdata = String.Format("macAddress={0}&ipAddress={1}&dnsName={2}&ipJson=&loginName={3}&password={4}&verificationCode={5}", "00-00-00-00-00-00", "210.22.124.10", "上海市".ToUrlEncode(), socialReq.Username.ToUrlEncode(), socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "login.jsp",
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
                string err = CommonFun.GetMidStr(httpResult.Html.ToTrim(), "<ulclass=\"errorMessage\"><li><span>", "</span></li></ul></div>");
                if (!err.IsEmpty())
                {
                    Res.StatusDescription = err;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取基本信息

                #region 校验

                Url = baseUrl + "logbusinesshis.action";
                postdata = String.Format("businesscode=0000000012");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "index.action",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                Url = baseUrl + "a1/f90010202/personInfoGet.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Referer = baseUrl + "pages/a1/f90010202/f90010202.jsp?menuId=0000000012",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = jsonParser.GetResultFromParser(httpResult.Html, "success");
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK || !bool.Parse(results))
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }

                results = jsonParser.GetResultFromParser(httpResult.Html, "result");
                if (string.IsNullOrEmpty(results))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = CommonFun.GetMidStr(results, "[", "]");
                Res.EmployeeNo = jsonParser.GetResultFromParser(results, "bac045");//编号
                Res.Name = jsonParser.GetResultFromParser(results, "aac003");//姓名
                Res.Sex = jsonParser.GetResultFromParser(results, "aac004");//性别
                Res.Sex = Res.Sex == "2" ? "女" : (Res.Sex == "1" ? "男" : (Res.Sex == "9" ? "不明" : ""));
                Res.BirthDate = DateTime.Parse(jsonParser.GetResultFromParser(results, "aac006")).ToString("yyyy-MM-dd");//出生年月
                Res.IdentityCard = jsonParser.GetResultFromParser(results, "aae135");//身份证号
                Res.Phone = jsonParser.GetResultFromParser(results, "aae321");//联系电话
                Res.Address = jsonParser.GetResultFromParser(results, "aae006");//地址
                Res.WorkDate = jsonParser.GetResultFromParser(results, "aac007").IsEmpty() ? "" : DateTime.Parse(jsonParser.GetResultFromParser(results, "aac007")).ToString("yyyy-MM-dd");//参加工作时间
                Res.ZipCode = jsonParser.GetResultFromParser(results, "aae007");//邮编
                Res.EmployeeStatus = jsonParser.GetResultFromParser(results, "aac084");//人员状态
                Res.EmployeeStatus = Res.EmployeeStatus == "10" ? "在职" : (Res.EmployeeStatus == "21" ? "退休人员" : (Res.EmployeeStatus == "22" ? "离休人员" : (Res.EmployeeStatus == "24" ? "被征地生活补助人员" : (Res.EmployeeStatus == "25" ? "失地农民养老补助人员" : (Res.EmployeeStatus == "40" ? "" : "供养亲属")))));
                if (socialReq.Name.IndexOf(Res.Name.Replace("*",""), StringComparison.Ordinal)>-1)
                {
                    Res.Name = socialReq.Name;
                }
                if (socialReq.Identitycard.IndexOf(Res.IdentityCard.Replace("*", ""), StringComparison.Ordinal) > -1)
                {
                    Res.IdentityCard = socialReq.Identitycard;
                }
                //个人总额、月数
                Url = baseUrl + "a2/f30110420/psnAccountStatePrintQuery.action";
                //http://www.hrsszj.gov.cn/ggfw/a2/f30110420/psnAccountStatePrintQuery.action
                postdata = "baz036=" + DateTime.Now.ToString("yyyyMM");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "pages/a2/f30110420/f30110420.jsp?menuId=0000000008",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = jsonParser.GetResultFromParser(httpResult.Html, "success");
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (bool.Parse(results))
                {
                    results = jsonParser.GetResultFromParser(httpResult.Html, "result");
                    results = results.Substring(1, results.Length - 2);
                    Res.PersonalInsuranceTotal = jsonParser.GetResultFromParser(results, "sumaccount").ToDecimal(0);//账户总额
                    Res.PaymentMonths = jsonParser.GetResultFromParser(results, "bac189").ToInt(0);//月数
                }

                //账户总额
                Url = baseUrl + "a2/f30110602/getPsnlAccountInfo.action";
                postdata = "aaa097=&aae140=110";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "pages/a2/f30110602/f30110602_3.jsp?menuId=0000000018",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = jsonParser.GetResultFromParser(httpResult.Html, "success");
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (bool.Parse(results))
                {
                    results = jsonParser.GetResultFromParser(httpResult.Html, "result");
                    results = results.Substring(1, results.Length - 2);
                    Res.InsuranceTotal = jsonParser.GetResultFromParser(results, "in_total").ToDecimal(0);
                }


                #endregion

                #region 查询明细

                #region 校验
                Url = baseUrl + "logbusinesshis.action";
                postdata = String.Format("businesscode=0000000014");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "index.action",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion
                InitPageHash();
                foreach (InfoType type in Enum.GetValues(typeof(InfoType)))
                {
                    try
                    {
                        GetAllDetail(type, ref Res);
                    }
                    catch
                    {
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

        private void GetAllDetail(InfoType type, ref SocialSecurityQueryRes Res)
        {
            SocialSecurityDetailQueryRes detailRes = null;
            string Url = string.Empty;
            string postdata = string.Empty;
            string resultStr = string.Empty;

            int pageIndex = 0;
            int pageCount = 0;
            DateTime endDate = DateTime.Now;
            DateTime beginDate = DateTime.Parse(endDate.AddYears(-5).ToString("yyyy-01-01"));
            List<ZhenJiangSocia> sociaresults = new List<ZhenJiangSocia>();
            do
            {
                Url = baseUrl + "a2/f30110602/queryCaptureExpendsHistory.action";
                postdata = String.Format("start={0}&limit=100&aae140={3}&aaa115=&aae078=&startAae003_html={1}&startAae003={1}&endAae003={2}&endAae003_html={2}", 100 * pageIndex, beginDate.ToString("yyyyMM"), endDate.ToString("yyyyMM"), ((string[])PageHash[type])[0]);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    //Referer = baseUrl + "pages/a2/f30110602/f30110602_2.jsp?menuId=0000000014",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                pageCount = jsonParser.GetResultFromParser(httpResult.Html, "totalPageCount").ToInt(0);
                resultStr = jsonParser.GetResultFromParser(httpResult.Html, "result");
                if (!string.IsNullOrEmpty(resultStr.Replace("[]", "")))
                    sociaresults.AddRange(jsonParser.DeserializeObject<List<ZhenJiangSocia>>(resultStr));
                pageIndex++;
            }
            while (pageIndex < pageCount);
            if (sociaresults.Count > 0 && type==InfoType.企业基本养老保险)
            {
                Res.CompanyName = sociaresults[sociaresults.Count - 1].aae044;//单位名称
            }
            foreach (ZhenJiangSocia socia in sociaresults)
            {
                bool isSave = false;
                detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == socia.aae079);
                if (detailRes == null)
                {
                    isSave = true;
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = socia.aac003;
                    detailRes.PayTime = socia.aae079;
                    detailRes.SocialInsuranceTime = socia.aae003;
                    detailRes.SocialInsuranceBase = socia.aae180.ToDecimal(0);
                    detailRes.CompanyName = socia.aae044;
                    detailRes.PaymentType = (socia.aaa115 != "正常应缴") ? ServiceConsts.SocialSecurity_PaymentType_Adjust : ServiceConsts.SocialSecurity_PaymentType_Normal;
                    detailRes.PaymentFlag = socia.aae078 != "1" ? ServiceConsts.SocialSecurity_PaymentFlag_Adjust : ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                }
                switch (type)
                {
                    case InfoType.企业基本养老保险:
                        detailRes.PensionAmount += socia.aae022.ToDecimal(0);
                        detailRes.CompanyPensionAmount += socia.aae020.ToDecimal(0);
                        break;
                    case InfoType.职工基本医疗保险:
                        detailRes.MedicalAmount += socia.aae022.ToDecimal(0);
                        detailRes.CompanyMedicalAmount += socia.aae020.ToDecimal(0);
                        break;
                    case InfoType.大病医疗费用统筹:
                        detailRes.IllnessMedicalAmount += socia.aae022.ToDecimal(0) + socia.aae020.ToDecimal(0);
                        break;
                    case InfoType.公务员补充医疗保险:
                        detailRes.CivilServantMedicalAmount += socia.aae022.ToDecimal(0) + socia.aae020.ToDecimal(0);
                        break;
                    case InfoType.失业保险:
                        detailRes.UnemployAmount += socia.aae022.ToDecimal(0) + socia.aae020.ToDecimal(0);
                        break;
                    case InfoType.工伤保险:
                        detailRes.EmploymentInjuryAmount += socia.aae020.ToDecimal(0);
                        break;
                    case InfoType.生育保险:
                        detailRes.MaternityAmount += socia.aae020.ToDecimal(0);
                        break;
                }
                if (isSave)
                {
                    Res.Details.Add(detailRes);
                }
            }
        }
    }
    #region  实体

    /// <summary>
    /// 芜湖养老保险实体
    /// </summary>
    internal class ZhenJiangSocia
    {
        public string aaa027 { get; set; }
        public string aaa041 { get; set; }
        public string aaa042 { get; set; }
        public string aaa043 { get; set; }
        private string _aaa115 = string.Empty;
        /// <summary>
        /// 应缴类型
        /// </summary>
        public string aaa115
        {
            get { return _aaa115; }
            set
            {
                switch (value)
                {
                    case "10":
                        _aaa115 = "正常应缴";
                        break;
                    case "41":
                        _aaa115 = "正常补收";
                        break;
                    default:
                        _aaa115 = value;
                        break;
                }
            }
        }
        public string aab001 { get; set; }
        /// <summary>
        /// 征收方式
        /// </summary>
        public string aab033 { get; set; }
        public string aab999 { get; set; }
        public string aac001 { get; set; }
        /// <summary>
        /// 人员姓名
        /// </summary>
        public string aac003 { get; set; }
        public string aac040 { get; set; }
        public string aac300 { get; set; }
        public string aae002 { get; set; }
        /// <summary>
        /// 所属期
        /// </summary>
        public string aae003 { get; set; }
        public string aae017 { get; set; }
        /// <summary>
        /// 单位应缴金额
        /// </summary>
        public string aae020 { get; set; }
        public string aae021 { get; set; }
        /// <summary>
        /// 个人应缴金额
        /// </summary>
        public string aae022 { get; set; }
        public string aae023 { get; set; }
        public string aae024 { get; set; }
        public string aae025 { get; set; }
        /// <summary>
        /// 单位名称
        /// </summary>
        public string aae044 { get; set; }
        /// <summary>
        /// 足额到账标志
        /// </summary>
        public string aae078 { get; set; }
        /// <summary>
        /// 足额到账年月
        /// </summary>
        public string aae079 { get; set; }
        public string aae080 { get; set; }
        public string aae081 { get; set; }
        public string aae082 { get; set; }
        public string aae083 { get; set; }
        public string aae085 { get; set; }
        /// <summary>
        /// 险种类型
        /// </summary>
        public string aae140 { get; set; }
        /// <summary>
        /// 人员缴费基数 
        /// </summary>
        public string aae180 { get; set; }
        /// <summary>
        /// 缴费月数
        /// </summary>
        public string aae202 { get; set; }
        public string aaz021 { get; set; }
        public string aaz030 { get; set; }
        public string aaz223 { get; set; }
        public string bac045 { get; set; }
        public string begin { get; set; }
        public string end { get; set; }
        public string jfxz { get; set; }
        public string jfbz_name { get; set; }
    }

    #endregion
}
