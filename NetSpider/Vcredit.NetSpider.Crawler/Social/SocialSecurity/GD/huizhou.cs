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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.GD
{
    /// <summary>
    /// 类似佛山社保,目前网站:503
    /// </summary>
    public class huizhou : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://113.106.218.163:8001/grcx/";
        string socialCity = "gd_huizhou";
        #endregion

        #region 私有方法
        enum InfoType
        {
            养老保险,
            医疗保险,
            失业保险
        }

        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息
        Hashtable Months_Continuous = new Hashtable();//存放24个月份的标示
        List<string> PayTimeList = new List<string>();//存放缴费年月

        /// <summary>
        /// 将各种保险对应的页面所需信息存入PageHash
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, new string[] { "gryhcx_cbls_yl" });
            PageHash.Add(InfoType.医疗保险, new string[] { "gryhcx_cbls_yiliao" });
            PageHash.Add(InfoType.失业保险, new string[] { "gryhcx_cbls_shiye" });
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
        /// 获取某类保险的某页的信息
        /// </summary>
        /// <param name="type">保险类型</param>
        void GetAllDetail(InfoType Type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            List<string> PayTimeList = new List<string>();

            int pageIndex = 1;
            int pageCount = 0;

            do
            {
                Url = baseUrl + string.Format("action/MainAction?menuid={0}&ActionType=i_{0}&flag=true&timeout=grcx{1}", ((string[])PageHash[Type])[0], pageIndex == 1 ? "" : string.Format("&i_{0}={1}", ((string[])PageHash[Type])[0], pageIndex - 1));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                if (pageCount != 0)
                {

                }
                else
                {
                    try
                    {
                        pageCount = int.Parse(HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='list_table_pageSplitInfo']/b")[1]);
                    }
                    catch
                    { break; }
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='list_table']/div/tbody/tr");

                if (results.Count <= 0)
                {
                    if (Type == InfoType.养老保险)
                    {
                        Res.StatusDescription = "无养老保险信息";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        throw new Exception("无养老保险信息");
                    }
                    else
                        return;
                }

                foreach (string item in results)
                {
                    List<string> Detail = HtmlParser.GetResultFromParser(item, "/td");

                    if (Type == InfoType.养老保险)
                        Detail.Insert(4, "养老保险");
                    if (Type == InfoType.医疗保险)
                        if (!Detail[4].Contains("基本医疗"))
                            continue;

                    int StartYear = int.Parse(Detail[1].Split('-')[0]);
                    int StartMonth = int.Parse(Detail[1].Split('-')[1]);
                    int EndYear = int.Parse(Detail[2].Split('-')[0]);
                    int EndMonth = int.Parse(Detail[2].Split('-')[1]);

                    do
                    {
                        SocialSecurityDetailQueryRes detailRes = null;
                        string SocialInsuranceTime = StartYear.ToString() + (StartMonth < 10 ? "0" + StartMonth.ToString() : StartMonth.ToString());
                        bool NeedSave = false;
                        if (Type != InfoType.养老保险)
                        {
                            detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                        }

                        if (detailRes == null)
                        {
                            NeedSave = true;
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.Name = Res.Name;
                            detailRes.PayTime = SocialInsuranceTime;
                            detailRes.SocialInsuranceTime = SocialInsuranceTime;
                            detailRes.CompanyName = Detail[0];
                            detailRes.SocialInsuranceBase = Detail[5].ToDecimal(0);
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        }
                            switch (Type)
                            {
                                case InfoType.养老保险:
                                    detailRes.PensionAmount = Detail[6].ToDecimal(0);
                                    detailRes.CompanyPensionAmount = Detail[7].ToDecimal(0) + Detail[8].ToDecimal(0);
                                    //if (Res.SocialInsuranceBase == 0)   
                                    //    Res.SocialInsuranceBase = detailRes.SocialInsuranceBase;
                                    //try
                                    //{
                                    //    if (!PayTimeList.Contains(detailRes.PayTime))
                                    //    {
                                    //        PayTimeList.Add(detailRes.PayTime);

                                    //        if (!Months_Continuous.Contains(detailRes.PayTime))
                                    //            Months_Continuous.Add(detailRes.PayTime, 1);
                                    //    }
                                    //}
                                    //catch { }
                                    break;
                                case InfoType.医疗保险:
                                    detailRes.MedicalAmount = Detail[6].ToDecimal(0);
                                    detailRes.CompanyMedicalAmount = Detail[7].ToDecimal(0) + Detail[8].ToDecimal(0);
                                    break;
                                case InfoType.失业保险:
                                    detailRes.UnemployAmount = Detail[6].ToDecimal(0) + Detail[7].ToDecimal(0) + Detail[8].ToDecimal(0);
                                    break;
                            }

                        if (NeedSave)
                            Res.Details.Add(detailRes);

                        if (StartMonth == 12)
                        {
                            StartMonth = 1;
                            StartYear++;
                        }
                        else
                            StartMonth++;
                    }
                    while (StartYear < EndYear || StartMonth <= EndMonth);

                }

                pageIndex++;
            }
            while (pageIndex <= pageCount);

            if (Type == InfoType.养老保险)
            {
                Res.PaymentMonths_Continuous = CalMonths_Continuous();
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
            //Dictionary<string, object> dic = new Dictionary<string,object>();

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
                
                Url = baseUrl + "checkimage.jsp";
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

                //dic.Add("cookies", cookies);
                //dic.Add("key", key);
                //CacheHelper.SetCache(token, dic);
                CacheHelper.SetCache(token, cookies);

                Res.Token = token;
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
            //Dictionary<string, object> dic = new Dictionary<string, object>();
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    //dic = (Dictionary<string, object>)CacheHelper.GetCache(socialReq.Token);
                    //cookies = (CookieCollection)dic["cookies"];
                    //key = (string)dic["key"];
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Name.IsEmpty() || socialReq.CompanyName.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录
                Url = baseUrl + "login.jsp";
                postdata = string.Format("UserID={0}", socialReq.Identitycard);
                //postdata = String.Format("UserType=4&DWSXH=&GMSFHM=&h={1}&p=&resetPage=login.jsp&UserID={0}&ZSXM=&SZDW=&pd=&imagecheck=", socialReq.Identitycard, key);
                //postdata = String.Format("fromPage=grcx&rtn=N&UserID={0}&Password={1}", socialReq.Username, socialReq.Password);
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

                string Com_No = null;
                try
                {
                    string GetCompany = CommonFun.GetMidStr(httpResult.Html, "document.getElementById(\"SZDW\").options.remove(1);", "setCbdw(szdwxx);");

                    string com1 = CommonFun.GetMidStr(GetCompany, "DWSXH_C=,", "DWMC_C");
                    com1 = com1.Remove(com1.Length - 1);
                    string com2 = CommonFun.GetMidStr(GetCompany, "DWMC_C=,", "GMSFHM");
                    com2 = com2.Remove(com2.Length - 1);

                    string[] com1list = com1.Split(',');
                    string[] com2list = com2.Split(',');
                    Hashtable com_hash = new Hashtable();
                    
                    for (int n = 0; n < com1list.Length; n++)
                    {
                        com_hash.Add(com2list[n], com1list[n]);
                    }

                    if (com_hash.ContainsKey(socialReq.CompanyName))
                    {
                        Com_No = com_hash[socialReq.CompanyName].ToString();
                    }

                    if (Com_No == null)
                    {
                        if (com_hash.ContainsKey(socialReq.Name))
                            Com_No = com_hash[socialReq.CompanyName].ToString();
                        else
                        {
                            Res.StatusDescription = "单位名称错误！";
                            Res.StatusCode = ServiceConsts.StatusCode_fail;
                            return Res;
                        }
                    }
                }
                catch
                {
                    Res.StatusDescription = "登录信息错误！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                string key = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='h']", "value")[0];
                string p = RSAHelper.EncryptStringByJS_SocialSecurity_huizhou(key, socialReq.Password);
                string name = socialReq.Name.ToUrlEncode(Encoding.GetEncoding("gb2312"));
                Url = baseUrl + "action/LoginAction";
                postdata = String.Format("UserType=4&DWSXH={0}&GMSFHM={1}&UserID={1}&ZSXM={2}&SZDW={0}&imagecheck={4}&h=&p={5}&pd=&resetPage=login.jsp", Com_No, socialReq.Identitycard, name, socialReq.Password, socialReq.Vercode, p);
                //postdata = String.Format("fromPage=grcx&rtn=N&UserID={0}&Password={1}", socialReq.Username, socialReq.Password);
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 获取基本信息
                Url = baseUrl + "action/MainAction?ActionType=Left";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = baseUrl + "action/LoginAction",
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

                Url = baseUrl + "action/MainAction?menuid=gryhcx_grjbxx&ActionType=i_gryhcx_grjbxx&flag=true&timeout=grcx";
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
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='div_i_grxxcx']/table/tbody/tr/td/span");

                Res.CompanyName = results[2];
                Res.IdentityCard = results[3];
                Res.Name = results[4];
                Res.Sex = results[5];
                Res.Race = results[6];
                Res.BirthDate = results[9];
                Res.WorkDate = results[16];
                Res.EmployeeStatus = results[17];
                Res.PaymentMonths = results[22] == "" ? 0 : int.Parse(results[22]);
                Res.Phone = results[28];
                Res.Address = results[30];

                Url = baseUrl + "action/MainAction?menuid=gryhcx_ylzh&ActionType=i_gryhcx_ylzh&flag=true&timeout=grcx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='div_i_gr_ylzh']/table");
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='div_i_gr_ylzh']/table/tbody");
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='div_i_gr_ylzh']/table");
                results = HtmlParser.GetResultFromParser(results[0], "//tr/td");
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='div_i_gr_ylzh']/table//tr/td");

                Res.PersonalInsuranceTotal = results[20].ToDecimal(0);
                Res.InsuranceTotal = results[21].ToDecimal(0);
                #endregion

                #region 获取详细信息
                InitPageHash();

                foreach (InfoType info in Enum.GetValues(typeof(InfoType)))
                {
                    try
                    {
                        GetAllDetail(info, ref Res);
                    }
                    catch
                    {
                        if (info == InfoType.养老保险)
                            return Res;
                    }
                }

                #endregion

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
