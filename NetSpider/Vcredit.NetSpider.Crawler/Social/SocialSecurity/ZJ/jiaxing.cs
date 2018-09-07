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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.ZJ
{
    public class jiaxing : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.jxsmk.com/";
        string socialCity = "zj_jiaxing";
        #endregion

        #region 私有方法
        /// <summary>
        /// 保险类型
        /// </summary>
        enum InfoType
        {
            职工基本养老保险,
            基本医疗保险,
            失业保险,
            工伤保险,
            生育保险
        }

        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息
        Hashtable Months_Continuous = new Hashtable();//存放24个月份的标示
        List<string> PayTimeList = new List<string>();//存放缴费年月

        /// <summary>
        /// 将各种保险对应的页面所需信息存入PageHash
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.职工基本养老保险, new string[] { "10" });
            PageHash.Add(InfoType.基本医疗保险, new string[] { "20" });
            PageHash.Add(InfoType.失业保险, new string[] { "50" });
            PageHash.Add(InfoType.工伤保险, new string[] { "30" });
            PageHash.Add(InfoType.生育保险, new string[] { "40" });

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
            int PayMonths = 0;

            int pageIndex = 1;
            int pageCount = 0;

            do
            {
                Url = baseUrl + "CitizenUser/SB03";
                postdata = string.Format("XZLB={0}&JHJD=&PageNo={1}", ((string[])PageHash[Type])[0] ,pageIndex);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "CitizenUser/SB03",
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

                if (pageCount == 0)
                    pageCount = int.Parse(jsonParser.GetResultFromParser(httpResult.Html, "TotalPages"));

                if (pageCount == 0)
                    break;

                results = jsonParser.GetArrayFromParse(httpResult.Html, "Records");
                if (results.Count <= 0)
                {
                    return;
                }

                foreach (string Detail in results)
                {
                    try
                    {
                        string SocialInsuranceTime = jsonParser.GetResultFromParser(Detail, "YJNY");
                        string paymenttype = jsonParser.GetResultFromParser(Detail, "YJLXStr");
                        string paymentflag = jsonParser.GetResultFromParser(Detail, "DZBZStr");
                        SocialSecurityDetailQueryRes detailRes = null;
                        bool NeedSave = false;
                        if (Type != InfoType.职工基本养老保险)
                        {
                            if (paymenttype == "正常应缴")
                                detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime && o.PaymentType == ServiceConsts.SocialSecurity_PaymentType_Normal).FirstOrDefault();
                            else
                                detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime && o.PaymentType != ServiceConsts.SocialSecurity_PaymentType_Normal).FirstOrDefault();
                        }

                        if (detailRes == null)
                        {
                            NeedSave = true;
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.Name = Res.Name;
                            detailRes.PayTime = SocialInsuranceTime;
                            detailRes.SocialInsuranceTime = SocialInsuranceTime;
                            detailRes.SocialInsuranceBase = jsonParser.GetResultFromParser(Detail, "JFJS").ToDecimal(0);
                            detailRes.CompanyName = jsonParser.GetResultFromParser(Detail, "DWMC");
                            if (paymenttype == "正常应缴" && paymentflag == "已到账")
                            {
                                detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal; //缴费类型
                                detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal; //缴费标志
                            }
                            else
                            {
                                detailRes.PaymentType = paymenttype; //缴费类型
                                detailRes.PaymentFlag = paymentflag; //缴费标志
                            }
                            //detailRes.PaymentType = paymenttype == "正常应缴" ? ServiceConsts.SocialSecurity_PaymentType_Normal : paymenttype;//缴费类型
                            //detailRes.PaymentFlag = paymentflag == "已到账" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : paymentflag;//缴费标志
                        }
                        switch (Type)
                        {
                            case InfoType.职工基本养老保险:
                                detailRes.PensionAmount = jsonParser.GetResultFromParser(Detail, "GRJF").ToDecimal(0);
                                detailRes.CompanyPensionAmount = jsonParser.GetResultFromParser(Detail, "DWJF").ToDecimal(0);
                                //PayMonths++;
                                try
                                {
                                    if (!PayTimeList.Contains(detailRes.PayTime))
                                    {
                                        PayMonths++;
                                        PayTimeList.Add(detailRes.PayTime);

                                        //if (!Months_Continuous.Contains(detailRes.PayTime))
                                        //    Months_Continuous.Add(detailRes.PayTime, 1);
                                    }
                                }
                                catch { }
                                break;
                            case InfoType.基本医疗保险:
                                detailRes.MedicalAmount = jsonParser.GetResultFromParser(Detail, "GRJF").ToDecimal(0);
                                detailRes.CompanyMedicalAmount = jsonParser.GetResultFromParser(Detail, "DWJF").ToDecimal(0);
                                break;
                            case InfoType.失业保险:
                                detailRes.UnemployAmount = jsonParser.GetResultFromParser(Detail, "YJZE").ToDecimal(0);
                                break;
                            case InfoType.工伤保险:
                                detailRes.EmploymentInjuryAmount = jsonParser.GetResultFromParser(Detail, "YJZE").ToDecimal(0);
                                break;
                            case InfoType.生育保险:
                                detailRes.MaternityAmount = jsonParser.GetResultFromParser(Detail, "YJZE").ToDecimal(0);
                                break;
                        }

                        if (NeedSave)
                            Res.Details.Add(detailRes);
                    }
                    catch { }

                }

                pageIndex++;
            }
            while (pageIndex <= pageCount);

            if(Type == InfoType.职工基本养老保险)
            {
                Res.PaymentMonths = PayMonths;
                //Res.PaymentMonths_Continuous = CalMonths_Continuous();
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
                Url = baseUrl + "CitizenUser/Login";
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

                Url = baseUrl + "Validate/GetVCode";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl + "CitizenUser/Login",
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
            //int PaymentMonths = 0;
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

                #region 登录

                Url = baseUrl + "CitizenUser/Login";
                postdata = String.Format("cardno={0}&password={1}&vcode={2}", socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    //Referer = baseUrl + "CitizenUser/Login",
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
                if (jsonParser.GetResultFromParser(httpResult.Html, "type").ToLower() == "error")
                {
                    Res.StatusDescription = CommonFun.GetMidStr(jsonParser.GetResultFromParser(httpResult.Html, "lstMsg"), "[\"", "\"]");
                    Res.StatusCode = ServiceConsts.StatusCode_error;
                    return Res;
                }

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                Url = baseUrl + "CitizenUser";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl + "CitizenUser/Login",
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

                #region 获取基本信息
                Url = baseUrl + "CitizenUser/BaseInfo";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Encoding = Encoding.UTF8,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='myinfo']/table/tr/td[@align='left']", "inner");
                if (results.Count <= 0)
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                Res.Name = results[2].Trim().Replace("&nbsp;", "");//姓名
                Res.BirthDate = results[3].Trim().Replace("&nbsp;", "");//出生日期
                Res.Race = results[4].Trim().Replace("&nbsp;", "");//民族
                Res.EmployeeNo = results[0].Trim().Replace("&nbsp;", "");//编号
                Res.Sex = results[5].Trim().Replace("&nbsp;", "");//性别
                Res.IdentityCard = results[1].Trim().Replace("&nbsp;", "");//身份证号
                Res.CompanyName = results[7].Trim().Replace("&nbsp;", "");//单位名称
                Res.Address = results[8].Trim().Replace("&nbsp;", "");//家庭地址
                Res.Phone = CommonFun.GetMidStr(results[6].Trim(), ";", "<a h");//手机号码

                #endregion


                #region 获取详细信息
                #region 获取个人账户总额
                //Url = baseUrl + "CitizenUser/SB04";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "get",
                //    Referer = baseUrl + "CitizenUser/BaseInfo",
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                //Url = baseUrl + "CitizenUser/SB04";
                //postdata = "PageNo=1";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "POST",
                //    Postdata = postdata,
                //    CookieCollection = cookies,
                //    Encoding = Encoding.UTF8,
                //    //Accept = "application/json, text/javascript, */*; q=0.01",
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                ////请求失败后返回
                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                //    return Res;
                //}
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                //results = jsonParser.GetArrayFromParse(httpResult.Html, "Records");
                //Res.InsuranceTotal = jsonParser.GetResultFromParser(results[0], "GRZHZE").ToDecimal(0);//个人缴费总额为上年度的，所以不取值
                #endregion

                #region 获取缴费基数
                Url = baseUrl + "CitizenUser/SB02";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl + "CitizenUser/SB04",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                Url = baseUrl + "CitizenUser/SB02";
                postdata = "XZLB=10&JHJD=&PageNo=1";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Encoding = Encoding.UTF8,
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
                string ttt = jsonParser.GetResultFromParser(httpResult.Html, "Records");
                if (!string.IsNullOrWhiteSpace(jsonParser.GetResultFromParser(httpResult.Html, "Records")))
                {
                    results = jsonParser.GetArrayFromParse(httpResult.Html, "Records");
                    foreach (string Detail in results)
                    {
                        if (Detail != "")
                        {
                            if (Detail.Contains("企业养老"))
                                Res.SocialInsuranceBase = jsonParser.GetResultFromParser(Detail, "JFJS").ToDecimal(0);//养老缴费基数
                        }

                    }
                }
                #endregion

                
                #region 获取缴费详细信息
                
                Url = baseUrl + "CitizenUser/SB03";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl + "CitizenUser/SB04",
                    //Referer = baseUrl + "CitizenUser/BaseInfo",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                InitPageHash();

                foreach (InfoType info in Enum.GetValues(typeof(InfoType)))
                {
                    try
                    {
                        GetAllDetail(info, ref Res);
                    }
                    catch
                    {
                        return Res;
                    }
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

    }
}
