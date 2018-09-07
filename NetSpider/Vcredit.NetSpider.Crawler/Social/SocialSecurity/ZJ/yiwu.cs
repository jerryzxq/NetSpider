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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.ZJ
{
    public class yiwu : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://122.226.66.211:8098/sionlineman/";
        string socialCity = "zj_yiwu";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "verifyCode";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl,
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
            string jsonStr = string.Empty;
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Vercode.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录

                Url = baseUrl + "logonAction.do";
                postdata = String.Format("username={0}&passwd={1}&verifycode={2}&params=%7B%27username%27%3A%27{0}%27%2C%27password%27%3A%27{1}%27%2C%27scene%27%3A%27sce%3AUSER_PASS%3BUSERNAME%27%7D&submitbtn.x=37&submitbtn.y=10", socialReq.Identitycard, CommonFun.GetMd5Str(socialReq.Password), socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl,
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
                List<string> list = HtmlParser.GetResultFromParser(httpResult.Html, "//body/p", "", true);
                if (list.Count > 0)
                {
                    Res.StatusDescription = list[0];
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取基本信息

                jsonStr = "{\"aac003\":{\"type\":\"textfield\",\"data\":\"\"},\"aae135\":{\"type\":\"textfield\",\"data\":\"\"},\"aac009\":{\"type\":\"combo\",\"data\":\"\"},\"aac007\":{\"type\":\"textfield\",\"data\":\"\"},\"tel\":{\"type\":\"textfield\",\"data\":\"\"},\"aae005\":{\"type\":\"textfield\",\"data\":\"\"},\"aac010\":{\"type\":\"textfield\",\"data\":\"\"},\"aae006\":{\"type\":\"textfield\",\"data\":\"\"},\"eac001\":{\"type\":\"textfield\",\"data\":\"\"},\"aac006\":{\"type\":\"textfield\",\"data\":\"\"},\"grid5\":{\"type\":\"grid\",\"data\":\"[]\"},\"asc\":{\"type\":\"menu-item\",\"data\":\"\"},\"desc\":{\"type\":\"menu-item\",\"data\":\"\"},\"columns\":{\"type\":\"menu-item\",\"data\":\"\"},\"aac157_2\":{\"type\":\"hidden\",\"data\":\"" + socialReq.Identitycard + "\"},\"radow_parent_data\":{\"type\":\"textfield\",\"data\":\"\"},\"aac009_combo\":{\"type\":\"hidden\",\"data\":\"请您选择...\"}}";
                Url = baseUrl + "radowAction.do?method=doEvent";
                postdata = String.Format("pageModel=pages.personchange.PersonAll&eventNames=query&radow_parent_data=&rc={0}", jsonStr.ToUrlEncode(Encoding.GetEncoding("utf-8")));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "radowAction.do?method=doEvent&pageModel=pages.personchange.PersonAll",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = jsonParser.GetResultFromParser(httpResult.Html, "messageCode");
                if (httpResult.StatusCode != HttpStatusCode.OK || results != "0")
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = jsonParser.GetResultFromParser(httpResult.Html, "elementsScript");

                Res.EmployeeNo = CommonFun.GetMidStr(results, "getElementById('eac001').value='", "';document.getElementById('aac003').value");//编号
                Res.Name = CommonFun.GetMidStr(results, "getElementById('aac003').value='", "';document.getElementById('aae135').value");//姓名
                Res.IdentityCard = CommonFun.GetMidStr(results, "getElementById('aae135').value='", "';odin.setSelectValue('aac009',");//身份证号
                Res.BirthDate = CommonFun.GetMidStr(results, "getElementById('aac006').value='", "';document.getElementById('aac010').value");//出生日期
                if (results.IndexOf("aac007") > -1)
                {
                    Res.WorkDate = CommonFun.GetMidStr(results, "getElementById('aac007').value='", "';document.getElementById('aae006').value");//参加工作时间
                }
                Res.Address = CommonFun.GetMidStr(results, "getElementById('aae006').value='", "';document.getElementById('tel').value");//通讯地址
                Res.Phone = CommonFun.GetMidStr(results, "getElementById('tel').value='", "").Replace("';", "");//联系电话

                Url = baseUrl + "radowAction.do?method=doEvent&pageModel=pages.personchange.PersonAll&eventNames=grid5.dogridquery";
                postdata = String.Format("pageModel=pages.personchange.PersonAll&eventNames=grid5.dogridquery&radow_parent_data=&rc=%7B%22aac003%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae135%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac009%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac007%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22tel%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae005%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac010%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae006%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22eac001%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac006%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22grid5%22%3A%7B%22type%22%3A%22grid%22%2C%22data%22%3A%22%22%7D%2C%22asc%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22desc%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22columns%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22aac157_2%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22{0}%22%7D%2C%22radow_parent_data%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac009_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%7D&limit=10", Res.IdentityCard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "radowAction.do?method=doEvent&pageModel=pages.personchange.PersonAll",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult.Header["x-requested-with"] = "XMLHttpRequest";
                httpResult.Header["request-type"] = "Ajax";
                httpResult = httpHelper.GetHtml(httpItem);
                List<YiWubase> baseList = jsonParser.DeserializeObject<List<YiWubase>>(jsonParser.GetResultFromParser(httpResult.Html, "data"));
                foreach (YiWubase yiWubase in baseList)
                {
                    Res.SpecialPaymentType += yiWubase.aae140 + ":" + yiWubase.aac031 + ";";
                    if (yiWubase.aae140 == "企业养老")
                    {
                        Res.SocialInsuranceBase = yiWubase.aae180;
                        Res.CompanyName = yiWubase.aab004;
                    }
                }

                #endregion

                #region 查询明细

                string[] saveType = { "企业养老", "基本医疗", "失业", "工伤", "生育" };//明细保存类型
                int totalCount = 0;
                int pageIndex = 0;
                List<YiWuSocia> sociaresults = new List<YiWuSocia>();
                Url = baseUrl + "radowAction.do?method=doEvent&pageModel=pages.personchange.Person_jf&eventNames=grid5.dogridquery";
                do
                {
                    postdata = string.Format("start={0}&limit=200&pageModel=pages.personchange.Person_jf&eventNames=grid5.dogridquery&radow_parent_data=&rc=%7B%22eac001%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22{1}%22%7D%2C%22aac003%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22{2}%22%7D%2C%22aac157%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22{3}%22%7D%2C%22aae140%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22startTime%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22endTime%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22grid5%22%3A%7B%22type%22%3A%22grid%22%2C%22data%22%3A%22%22%7D%2C%22asc%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22desc%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22columns%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22aac157_2%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22{3}%22%7D%2C%22radow_parent_data%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae140_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%7D", pageIndex * 200, Res.EmployeeNo, Res.Name.ToUrlEncode(), Res.IdentityCard);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Referer = baseUrl + "radowAction.do?method=doEvent&pageModel=pages.personchange.Person_jf",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = jsonParser.GetResultFromParser(httpResult.Html, "data");
                    if (totalCount == 0)
                    {
                        totalCount = (int)Math.Ceiling(jsonParser.GetResultFromParser(httpResult.Html, "totalCount").ToDecimal(0) / 200);
                    }
                    sociaresults.AddRange(jsonParser.DeserializeObject<List<YiWuSocia>>(results));
                    pageIndex++;
                } while (pageIndex < totalCount);

                string insuranceTime;
                foreach (YiWuSocia socia in sociaresults)
                {
                    if (!saveType.Contains(socia.aae140)) continue;
                    for (int i = 0; i < socia.eac003; i++)
                    {
                        insuranceTime = DateTime.ParseExact(socia.aae0021, "yyyyMM", null).AddMonths(i).ToString(Consts.DateFormatString7);
                        detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == insuranceTime);
                        bool isSave = false;
                        if (detailRes == null)
                        {
                            isSave = true;
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.Name = socia.aac003;
                            detailRes.IdentityCard = socia.aae135;
                            detailRes.CompanyName = socia.aae044;
                            detailRes.PayTime = socia.aae079;
                            detailRes.SocialInsuranceTime = insuranceTime;
                            detailRes.SocialInsuranceBase = socia.aae180;
                            detailRes.PaymentType = socia.aaa115 != "正常应缴" ? ServiceConsts.SocialSecurity_PaymentType_Adjust : ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = socia.aae078 != "已到账" ? ServiceConsts.SocialSecurity_PaymentFlag_Adjust : ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        }
                        switch (socia.aae140)
                        {
                            case "企业养老":
                                detailRes.PensionAmount += socia.aae022;
                                detailRes.CompanyPensionAmount += socia.aae020;
                                detailRes.SocialInsuranceBase = socia.aae180;
                                break;
                            case "基本医疗":
                                detailRes.PensionAmount += socia.aae022;
                                detailRes.CompanyPensionAmount += socia.aae020;
                                detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : socia.aae180;
                                break;
                            case "失业":
                                detailRes.UnemployAmount += socia.aae022 + socia.aae020;
                                break;
                            case "工伤":
                                detailRes.EmploymentInjuryAmount += socia.aae020;
                                break;
                            case "生育":
                                detailRes.MaternityAmount += socia.aae020;
                                break;
                        }
                        detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : socia.aae180;
                        if (!isSave) continue;
                        Res.Details.Add(detailRes);
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
    /// 特殊缴费类型
    /// </summary>
    internal class YiWubase
    {
        public string aaz289 { get; set; }
        public string aac033 { get; set; }
        public string aac031 { get; set; }
        public string aae140 { get; set; }
        public decimal aae180 { get; set; }
        public string aab004 { get; set; }
    }
    /// <summary>
    /// 明细实体
    /// </summary>
    internal class YiWuSocia
    {
        public string aae079 { get; set; }
        public string aae078 { get; set; }
        public string aac003 { get; set; }
        public string aae140 { get; set; }
        /// <summary>
        /// 缴费基数
        /// </summary>
        public decimal aae180 { get; set; }
        public string aae0021 { get; set; }
        public string eac001 { get; set; }
        /// <summary>
        /// 单位缴费
        /// </summary>
        public decimal aae020 { get; set; }
        public string aae003 { get; set; }
        public string aaa115 { get; set; }
        public string aae058 { get; set; }
        public string aae135 { get; set; }
        /// <summary>
        /// 缴费月数
        /// </summary>
        public int eac003 { get; set; }
        public string aae002 { get; set; }
        /// <summary>
        /// 个人缴费
        /// </summary>
        public decimal aae022 { get; set; }
        public string aae044 { get; set; }
    }

    #endregion
}
