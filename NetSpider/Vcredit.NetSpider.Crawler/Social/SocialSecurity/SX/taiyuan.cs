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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.SX
{
    public class taiyuan : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件

        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.tyyb.gov.cn/";
        string socialCity = "sx_taiyuan";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            string Url = string.Empty;
            Res.Token = token;
            try
            {
                Url = baseUrl + "job/login.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = baseUrl + "CaptchaImg";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);

                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                //Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
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
            int PaymentMonths = 0;
            string personcode = string.Empty;
            string errorMsg = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Username.IsEmpty() || socialReq.Name.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录
                Url = baseUrl + "j_spring_security_check?r=%27+Math.random()";
                postdata = String.Format("j_userid={0}&j_username={1}&j_password={2}&checkCode={3}", socialReq.Name.ToUrlEncode(), socialReq.Username, socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    //Allowautoredirect = false,
                    CookieCollection = cookies,
                    Host = "www.tyyb.gov.cn",
                    Referer = baseUrl + "job/login.jsp",
                    ContentType = "application/x-www-form-urlencoded; charset=utf-8",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var flag = jsonParser.GetResultFromParser(httpResult.Html, "success");
                if (flag != "True")
                {
                    Res.StatusDescription = jsonParser.GetResultFromParser(httpResult.Html, "msg");
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollectionEQ(cookies, httpResult.CookieCollection);

                Url = baseUrl + "loginSuccessAction.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                flag = jsonParser.GetResultFromParser(httpResult.Html, "success");
                if (flag == "False")
                {
                    var errormsg = jsonParser.GetResultFromParser(httpResult.Html, "msg");
                    Res.StatusDescription = errormsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                #endregion

                #region 获取基本信息

                Url = baseUrl + "query/queryAction.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var raceString = CommonFun.GetMidStr(httpResult.Html, "data=[", "]; option.divId=\"aac005\"");
                raceString = CommonFun.GetMidStr(raceString, "data=[", "");
                var raceList = raceString.Split('}');

                cookies = CommonFun.GetCookieCollectionEQ(cookies, httpResult.CookieCollection);//合并cookies

                //人员基本信息
                Url = baseUrl + "query/queryAction!queryBaseInfo.do";
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0",
                    URL = Url,
                    Referer = baseUrl + "query/queryAction.do",
                    Method = "POST",
                    //Host="www.tyyb.gov.cn",
                    Postdata = null,
                    ContentType = "application/x-www-form-urlencoded; charset=utf-8",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header.Add("X-Requested-With", "XMLHttpRequest");
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollectionEQ(cookies, httpResult.CookieCollection);//合并cookies

                var infoString = jsonParser.GetResultFromParser(httpResult.Html, "lists");
                infoString = jsonParser.GetResultFromParser(infoString, "companyGrid");
                infoString = jsonParser.GetResultFromParser(infoString, "list").Replace("[", "").Replace("]", "");
                var dataobj = JObject.Parse(infoString);
                Res.Name = dataobj["aac003"].ToString();  //姓名
                Res.EmployeeNo = dataobj["aac001"].ToString();  // 员工编号
                Res.IdentityCard = dataobj["aac002"].ToString();  //身份证号
                Res.Sex = dataobj["aac004"].ToString() == "1" ? "男" : "女";  //性别
                raceString = raceList.Where(e => e.Contains(dataobj["aac005"].ToString())).ToList()[0];
                Res.Race = CommonFun.GetMidStr(raceString, "\"name\":\"", "\",\"py\"");  //民族
                Res.BirthDate = dataobj["aac006"].ToString();  //出生日期
                //员工状态
                switch (dataobj["aac008"].ToString())
                {
                    case "1":
                        Res.EmployeeStatus = "在职";
                        break;
                    case "2":
                        Res.EmployeeStatus = "退休";
                        break;
                    case "3":
                        Res.EmployeeStatus = "死亡";
                        break;
                    case "4":
                        Res.EmployeeStatus = "离休";
                        break;
                    case "5":
                        Res.EmployeeStatus = "二乙";
                        break;
                }
                Res.Phone = dataobj["aae005"].ToString();  //联系电话
                Res.Address = dataobj["aac025"].ToString();  //地址

                //职工参保信息查询
                Url = baseUrl + "query/joinInsuranceAction!joinInsuranceInfo.do";
                postdata = string.Format("gridInfo%5B'personGrid_limit'%5D=10&gridInfo%5B'personGrid_start'%5D=0&gridInfo%5B'personGrid_selected'%5D=%5B%5D&gridInfo%5B'personGrid_modified'%5D=%5B%5D&gridInfo%5B'personGrid_removed'%5D=%5B%5D&gridInfo%5B'personGrid_added'%5D=%5B%5D");
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    ContentType = "application/x-www-form-urlencoded; charset=utf-8",
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0",
                    URL = Url,
                    Referer = baseUrl + "query/joinInsuranceAction.do",
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header.Add("X-Requested-With", "XMLHttpRequest");
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollectionEQ(cookies, httpResult.CookieCollection);//合并cookies
                infoString = jsonParser.GetResultFromParser(httpResult.Html, "lists");
                infoString = jsonParser.GetResultFromParser(infoString, "personGrid");
                //var list = jsonParser.GetArrayFromParse(infoString, "list");
                // dataobj = JObject.Parse(list[0]);
                //Res.CompanyName = dataobj["aab004"].ToString();  //单位名称
                // Res.CompanyNo = dataobj["aab001"].ToString();  //单位编号
                //var ylStartDate = dataobj["bccbrq"].ToString();  //参保日期
                List<SpecialPayType> specialPayType = jsonParser.DeserializeObject<List<SpecialPayType>>(jsonParser.GetResultFromParser(infoString, "list"));
                specialPayType = specialPayType.OrderByDescending(o => o.aac030).ToList();
                for (int i = 0; i < specialPayType.Count; i++)
                {
                    if (i == 0)
                    {
                        Res.CompanyName = specialPayType[i].aab004;  //单位名称
                        Res.CompanyNo = specialPayType[i].aab001;  //单位编号
                    }
                    Res.SpecialPaymentType += specialPayType[i].aae140 + "[" + specialPayType[i].aac031 + "]";
                }
                Url = baseUrl + "query/insuranceZhzeAction!insuranceZhzeInfo.do";
                postdata = string.Format("gridInfo%5B'zhzeGrid_limit'%5D=20&gridInfo%5B'zhzeGrid_start'%5D=0&gridInfo%5B'zhzeGrid_selected'%5D=%5B%5D&gridInfo%5B'zhzeGrid_modified'%5D=%5B%5D&gridInfo%5B'zhzeGrid_removed'%5D=%5B%5D&gridInfo%5B'zhzeGrid_added'%5D=%5B%5D");
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    ContentType = "application/x-www-form-urlencoded; charset=utf-8",
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0",
                    URL = Url,
                    Referer=baseUrl + "query/insuranceZhzeAction.do",
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header.Add("X-Requested-With", "XMLHttpRequest");
                httpResult = httpHelper.GetHtml(httpItem);
                infoString = jsonParser.GetResultFromParser(httpResult.Html, "lists");
                infoString = jsonParser.GetResultFromParser(infoString, "zhzeGrid");
                var list = jsonParser.GetArrayFromParse(infoString, "list");
                dataobj = JObject.Parse(list[0]);
                Res.InsuranceTotal = dataobj["akc087"].ToString().ToDecimal(0);  //账户总额

                #endregion

                #region 查询明细(暂无养老保险详细信息)  03-基本医疗保险;05-生育保险;07-大病医疗保险；04-工伤保险
                var date = DateTime.Now.AddYears(-1).ToString("yyyy");
                Url = baseUrl + "query/insuranceJfyzAction!insuranceJfyzInfo.do";
                       postdata = "gridInfo%5B'jfjeGrid_limit'%5D=5000&gridInfo%5B'jfjeGrid_start'%5D=0&gridInfo%5B'jfjeGrid_selected'%5D=%5B%5D&gridInfo%5B'jfjeGrid_modified'%5D=%5B%5D&gridInfo%5B'jfjeGrid_removed'%5D=%5B%5D&gridInfo%5B'jfjeGrid_added'%5D=%5B%5D";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "query/insuranceJfyzAction.do",
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                infoString = jsonParser.GetResultFromParser(httpResult.Html, "lists");
                infoString = jsonParser.GetResultFromParser(infoString, "jfjeGrid");
                list = jsonParser.GetArrayFromParse(infoString, "list");

                SocialSecurityDetailQueryRes detail = new SocialSecurityDetailQueryRes();
                var dateFlag = "";

                foreach (var item in list)
                {
                    var detailObj = JObject.Parse(item);
                    if (dateFlag != detailObj["aae003"].ToString())
                    {
                        if (!string.IsNullOrEmpty(detail.Name))
                        {
                            Res.Details.Add(detail);
                        }
                        detail = new SocialSecurityDetailQueryRes();
                    }
                    dateFlag = detailObj["aae003"].ToString();
                    detail.Name = Res.Name;  //姓名
                    detail.CompanyName = Res.CompanyName;  //公司名称
                    detail.IdentityCard = Res.IdentityCard;  //身份证号
                    detail.SocialInsuranceTime = detailObj["aae003"].ToString();  //所属年月
                    if (detailObj["aae140"].ToString() == "03")  //基本医疗保险
                    {

                        if (detailObj["aae143"].ToString() == "01")  //正常缴费
                        {
                            detail.SocialInsuranceBase = detailObj["aae180"].ToString().ToDecimal(0);  //社保基数
                            detail.MedicalAmount = detailObj["yab157"].ToString().ToDecimal(0);  //个人划入医保
                            detail.CompanyMedicalAmount = detailObj["aab212"].ToString().ToDecimal(0);  //单位划入医保
                            detail.EnterAccountMedicalAmount = detail.MedicalAmount + detail.CompanyMedicalAmount;//社保总共缴费金额
                            detail.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detail.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        }
                        else
                        {
                            detail.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                            detail.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                        }

                    }
                    else if (detailObj["aae140"].ToString() == "05")  //生育保险
                    {
                        detail.MaternityAmount = detailObj["yab157"].ToString().ToDecimal(0) + detailObj["aab212"].ToString().ToDecimal(0);  //生育保险
                    }
                    else if (detailObj["aae140"].ToString() == "07")  //大病救助保险
                    {
                        detail.IllnessMedicalAmount = detailObj["yab157"].ToString().ToDecimal(0) + detailObj["aab212"].ToString().ToDecimal(0);  //大病救助保险
                    }
                    else if (detailObj["aae140"].ToString() == "04")  //工伤保险
                    {
                        detail.EmploymentInjuryAmount = detailObj["yab157"].ToString().ToDecimal(0) + detailObj["aab212"].ToString().ToDecimal(0);  //工伤保险
                    }

                    if (item.Equals(list[list.Count - 1]))  //当为循环最后一个时添加
                    {
                        Res.Details.Add(detail);
                        PaymentMonths++;
                    }
                }
                #endregion
                Res.PaymentMonths = PaymentMonths;
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
        /// 特殊缴费类型
        /// </summary>
        private class SpecialPayType
        {
            /// <summary>
            /// 个人编号
            /// </summary>
            public string aac001 { get; set; }
            /// <summary>
            /// 公司编号
            /// </summary>
            public string aab001 { get; set; }
            /// <summary>
            /// 公司名称
            /// </summary>
            public string aab004 { get; set; }

            private string _aae140 = string.Empty;

            /// <summary>
            /// 险种类型
            /// </summary>
            public string aae140
            {
                get { return _aae140; }
                set
                {
                    switch (value)
                    {
                        case "03":
                            _aae140 = "基本医疗保险";
                            break;
                        case "04":
                            _aae140 = "工伤保险";
                            break;
                        case "05":
                            _aae140 = "生育保险";
                            break;
                        case "06":
                            _aae140 = "意外伤害险";
                            break;
                        case "07":
                            _aae140 = "大病医疗";
                            break;
                        case "08":
                            _aae140 = "公务员医疗";
                            break;
                        case "11":
                            _aae140 = "城镇居民医疗";
                            break;

                    }
                }
            }
            private string _aac031 = string.Empty;
            /// <summary>
            /// 人员缴费状态
            /// </summary>
            public string aac031
            {
                get { return _aac031; }
                set
                {
                    switch (value)
                    {
                        case "0":
                            _aac031 = "未参保";
                            break;
                        case "1":
                            _aac031 = "参保缴费";
                            break;
                        case "2":
                            _aac031 = "暂停缴费(中断)";
                            break;
                        case "3":
                            _aac031 = "终止缴费";
                            break;
                        case "4":
                            _aac031 = "恢复缴费";
                            break;
                    }
                }
            }
            /// <summary>
            /// 开始日期
            /// </summary>
            public string aac030 { get; set; }
        }

    }
}
