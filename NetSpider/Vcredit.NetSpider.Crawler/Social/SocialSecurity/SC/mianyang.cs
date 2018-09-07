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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.SC
{
    public class mianyang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://rsjapp.my.gov.cn/mycx/";
        string socialCity = "sc_mianyang";
        #endregion

        #region 私有方法
        /// <summary>
        /// 保险类型
        /// </summary>
        enum InfoType
        {
            个人基本信息,
            养老月缴费明细,
            医疗缴费记录,
            账户划入明细
        }

        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息
        Hashtable Months_Continuous = new Hashtable();//存放24个月份的标示
        Hashtable YearPaymentMonths = new Hashtable();
        SortedList<int, decimal> PayList = new SortedList<int,decimal>();//存放PayTime对应的缴费基数

        /// <summary>
        /// 将各种保险对应的页面所需信息存入PageHash
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.个人基本信息, new string[] { "Smyp01001" });
            PageHash.Add(InfoType.养老月缴费明细, new string[] { "Smyi02005" });
            PageHash.Add(InfoType.医疗缴费记录, new string[] { "Smyk01001" });
            PageHash.Add(InfoType.账户划入明细, new string[] { "Smyk01003" });
        }


        /// <summary>
        /// 通过页面返回的json源获取list中的信息及页数和数据总数
        /// </summary>
        /// <param name="jsonString">json源</param>
        /// <param name="PageCount">页数</param>
        /// <param name="TotalCount">数据总数</param>
        /// <returns></returns>
        List<string> GetListFromJson(string jsonString, ref int PageCount, ref int TotalCount)
        {
            List<string> list = new List<string>();

            try
            {
                if (PageCount == 0)
                {
                    string fieldData = jsonParser.GetResultFromParser(httpResult.Html, "fieldData");
                    PageCount = int.Parse(jsonParser.GetResultFromParser(fieldData, "pageCount"));
                    TotalCount = int.Parse(jsonParser.GetResultFromParser(fieldData, "totalCount"));
                }

                string lists = jsonParser.GetResultFromParser(httpResult.Html, "lists");
                string domainlists = jsonParser.GetResultFromParser(lists, "domainList");
                list = jsonParser.GetArrayFromParse(domainlists, "list");
            }
            catch { }

            return list;
        }

        /// <summary>
        /// 通过页面返回的json源获取list中的信息
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        List<string> GetListFromJson(string jsonString)
        {
            List<string> list = new List<string>();

            try
            {
                string lists = jsonParser.GetResultFromParser(httpResult.Html, "lists");
                string domainlists = jsonParser.GetResultFromParser(lists, "domainList");
                list = jsonParser.GetArrayFromParse(domainlists, "list");
            }
            catch { }

            return list;
        }

        /// <summary>
        /// 计算24个月连续缴费月数
        /// </summary>
        /// <returns>24个月连续缴费月数</returns>
        int CalMonths_Continuous()
        {
            int Months = 0;

            for (int i = -1; i > -25; i--)
            {
                DateTime Year_Month = DateTime.Now.AddMonths(i);
                if (Months_Continuous.Contains((Year_Month.Year * 100 + Year_Month.Month).ToString()))
                    Months++;
                else
                    break;
            }

            return Months;
        }

        /// <summary>
        /// 获取最近一个月的缴费基数
        /// </summary>
        /// <returns>最近一个月的缴费基数</returns>
        decimal CalInsuranceBase()
        {
            decimal InsuranceBase = 0;

            InsuranceBase = PayList[PayList.Keys.Max()];

            return InsuranceBase;
        }

        /// <summary>
        /// 获取某类保险的某页的信息
        /// </summary>
        /// <param name="type">保险类型</param>
        void GetAllDetail(InfoType Type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            //List<string> PayTimeList = new List<string>();
            int PayMonths = 0;

            int pageIndex = 1;
            int pageCount = 0;
            int totlaCount = 0;

            Url = baseUrl + string.Format("queryAction.do?functionId={0}", ((string[])PageHash[Type])[0]);
            httpItem = new HttpItem()
            {
                URL = Url,
                Method = "get",
                Referer = baseUrl + "indexAction.do",
                ResultType = ResultType.Byte,
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            if (httpResult.StatusCode != HttpStatusCode.OK)
            {
                Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                Res.StatusCode = ServiceConsts.StatusCode_fail;
            }
            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

            //if (Type == InfoType.养老月缴费明细)
            //    FillMonths_Continuous();

            do
            {
                Url = baseUrl + "queryAction!query.do";
                postdata = string.Format("dto['pageNo']={0}&dto['funNo']={1}&dto['counts']={2}", pageIndex, ((string[])PageHash[Type])[0], pageIndex == 1 ? "" : totlaCount.ToString());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + string.Format("queryAction.do?functionId={0}", ((string[])PageHash[Type])[0]),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                results = GetListFromJson(httpResult.Html, ref pageCount, ref totlaCount);

                if (Type == InfoType.个人基本信息)
                {
                    if (results.Count <= 0)
                    {
                        Res.StatusDescription = "无社保信息";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        throw new Exception("无社保信息");
                    }
                    Res.EmployeeNo = jsonParser.GetResultFromParser(results[0], "aac001");//员工编号
                    Res.IdentityCard = jsonParser.GetResultFromParser(results[0], "aac002");//员工编号
                    Res.Name = jsonParser.GetResultFromParser(results[0], "aac003");//姓名
                    Res.BirthDate = jsonParser.GetResultFromParser(results[0], "aac006");//出生日期

                    return;
                }

                if (pageCount == 0)
                {
                    //Res.StatusDescription = "无" + Type.ToString();
                    //Res.StatusCode = ServiceConsts.StatusCode_fail;
                    //throw new Exception("无" + Type.ToString());
                    return;
                }

                foreach (string Detail in results)
                {
                    if (Detail != "")
                    {
                        if (jsonParser.GetResultFromParser(Detail, "grsj").StartsWith("-"))
                            continue;
                        string SocialInsuranceTime = jsonParser.GetResultFromParser(Detail, "aae002");
                        SocialSecurityDetailQueryRes detailRes = null;
                        bool NeedSave = false;
                        if (Type != InfoType.养老月缴费明细)
                        {
                            detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                        }

                        if (detailRes == null)
                        {
                            NeedSave = true;
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.Name = Res.Name;
                            detailRes.PayTime = SocialInsuranceTime;//缴费时间
                            //detailRes.PayTime = jsonParser.GetResultFromParser(Detail, "aae002");
                            detailRes.SocialInsuranceTime = SocialInsuranceTime;//实缴时间
                            detailRes.CompanyName = jsonParser.GetResultFromParser(Detail, "aae044");//单位名称
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;//缴费类型
                            detailRes.PaymentFlag = (jsonParser.GetResultFromParser(Detail, "yj").ToDecimal(0) == jsonParser.GetResultFromParser(Detail, "sj").ToDecimal(0)) ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : "未缴费";//缴费标志
                        }
                        switch (Type)
                        {
                            case InfoType.养老月缴费明细:
                                detailRes.PensionAmount = jsonParser.GetResultFromParser(Detail, "grsj").ToDecimal(0);
                                detailRes.SocialInsuranceBase = (decimal)(double.Parse(jsonParser.GetResultFromParser(Detail, "grsj")) / 0.08);
                                detailRes.CompanyPensionAmount = jsonParser.GetResultFromParser(Detail, "dwsj").ToDecimal(0);
                                try
                                {
                                    string PayYear = detailRes.PayTime.Remove(4);
                                    if (YearPaymentMonths.Contains(PayYear))
                                        detailRes.YearPaymentMonths = int.Parse(YearPaymentMonths[PayYear].ToString());
                                }
                                catch { }
                                int PayTime = 0;
                                try
                                {
                                    PayTime = int.Parse(detailRes.PayTime);
                                    if (!PayList.ContainsKey(PayTime))
                                    {
                                        if (detailRes.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal)
                                            PayMonths++;
                                        PayList.Add(PayTime, detailRes.SocialInsuranceBase);

                                        //if (!Months_Continuous.Contains(detailRes.PayTime))
                                        //    Months_Continuous.Add(detailRes.PayTime, 1);
                                    }
                                    else
                                    {
                                        PayList[PayTime] += detailRes.SocialInsuranceBase;
                                    }
                                }
                                catch { }
                                break;
                            case InfoType.医疗缴费记录:
                                detailRes.SocialInsuranceBase = (decimal)(double.Parse(jsonParser.GetResultFromParser(Detail, "grsj")) / 0.02);
                                detailRes.MedicalAmount = jsonParser.GetResultFromParser(Detail, "grsj").ToDecimal(0);
                                detailRes.CompanyMedicalAmount = jsonParser.GetResultFromParser(Detail, "dwsj").ToDecimal(0);
                                break;
                            case InfoType.账户划入明细:
                                detailRes.EnterAccountMedicalAmount += jsonParser.GetResultFromParser(Detail, "aae065").ToDecimal(0);
                                break;

                        }
                        if (NeedSave)
                            Res.Details.Add(detailRes);
                    }
                }

                pageIndex++;
            }
            while (pageIndex <= pageCount);

            if (Type == InfoType.养老月缴费明细)
            {
                Res.PaymentMonths = PayMonths;
                //Res.PaymentMonths_Continuous = CalMonths_Continuous();
                Res.SocialInsuranceBase = CalInsuranceBase();
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
                Url = baseUrl + "login.jsp";
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

                Url = baseUrl + "CaptchaImg";
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
            //SocialSecurityDetailQueryRes detailRes = null;
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
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录
                Url = baseUrl + "commonIndexAction!checkUserType.do";
                postdata = String.Format("dto['yae041']={0}", socialReq.Username);
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                Url = baseUrl + "j_spring_security_check?r=%27+Math.random()";
                long tm = (DateTime.Now.Ticks - 621355968000000000)/10000;
                postdata = String.Format("j_username={0}&j_password={1}&orgId=&checkCode={2}&tm={3}", socialReq.Username, socialReq.Password, socialReq.Vercode, tm);
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

                string success = jsonParser.GetResultFromParser(httpResult.Html, "success");
                string StatusDescription = jsonParser.GetResultFromParser(httpResult.Html, "msg");

                if (jsonParser.GetResultFromParser(httpResult.Html, "success").Trim().ToLower() == "false")
                {
                    Res.StatusDescription = jsonParser.GetResultFromParser(httpResult.Html, "msg");
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                //此步操作会返回302；

                Url = baseUrl + "loginSuccessAction.do";
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

                Url = baseUrl + "indexAction.do";
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

                #endregion

                #region 获取每年月缴费数
                Url = baseUrl + "queryAction.do?functionId=Smyp01003";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl + "indexAction.do",
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

                Url = baseUrl + "queryAction!query.do";
                postdata = "dto['pageNo']=1}&dto['funNo']=Smyp01003&dto['counts']=";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "queryAction.do?functionId=Smyp01003",
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

                results = GetListFromJson(httpResult.Html);

                if (results.Count <= 0)
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    throw new Exception("无社保信息");
                }

                foreach (string item in results)
                {
                    YearPaymentMonths.Add(jsonParser.GetResultFromParser(item, "aae001"), jsonParser.GetResultFromParser(item, "aae094"));
                }
                #endregion


                #region 获取个人信息及明细
                InitPageHash();

                GetAllDetail(InfoType.个人基本信息, ref Res);//先获取个人基本信息，填充姓名和身份证

                foreach (InfoType info in Enum.GetValues(typeof(InfoType)))
                {
                    try
                    {
                        if(info != InfoType.个人基本信息)
                            GetAllDetail(info, ref Res);
                    }
                    catch
                    {
                        return Res;
                    }
                }

                #endregion
                //计算缴费基数
                SocialSecurityDetailQueryRes last_detail = Res.Details.OrderByDescending(o => o.SocialInsuranceTime).Where(p=>p.PensionAmount != 0 || p.CompanyPensionAmount != 0).FirstOrDefault();
                if (last_detail != null)
                {
                    Res.SocialInsuranceBase = last_detail.SocialInsuranceBase;
                }
                else
                {
                    last_detail = Res.Details.OrderByDescending(o => o.SocialInsuranceTime).Where(p => p.MedicalAmount != 0 || p.CompanyMedicalAmount != 0).FirstOrDefault();
                    if (last_detail != null)
                    {
                        if (last_detail.SocialInsuranceBase > 4347)
                        {
                            Res.SocialInsuranceBase = last_detail.SocialInsuranceBase;
                        }
                        else
                        {
                            Res.SocialInsuranceBase = 1791;
                        }
                    }
                }

                //提示缴费计算
                Res.SpecialPaymentType = "医保基数小于或等于4347元时，不认可；养老保底基数1791";

                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success ;
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
