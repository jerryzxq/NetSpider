using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.SD
{
    public class yantai : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.yt12333.com/ssc/";
        string socialCity = "sd_yantai";
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

        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, new string[] { "client/payhistory.do", "aged" });
            PageHash.Add(InfoType.医疗保险, new string[] { "client/payhistory_medi.do", "" });
            PageHash.Add(InfoType.失业保险, new string[] { "client/payhistory.do", "lost" });
            PageHash.Add(InfoType.生育保险, new string[] { "client/payhistory.do", "birth" });
            PageHash.Add(InfoType.工伤保险, new string[] { "client/payhistory.do", "harm" });
        }

        void GetAllDetail(InfoType Type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            SocialSecurityDetailQueryRes detailRes = new SocialSecurityDetailQueryRes();

            int year = DateTime.Now.Year;
            int minyear = year - 5;

            while (year > minyear)
            {
                Url = baseUrl + ((string[])PageHash[Type])[0];
                postdata = string.Format("type={0}&year={1}", ((string[])PageHash[Type])[1], year);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table [@class='datatable']/tr[@bgcolor='#FFFFFF']", "inner");
                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                    int count = Type == InfoType.医疗保险 ? 10 : 11;
                    if (tdRow.Count != count)
                    {
                        continue;
                    }

                    detailRes = Res.Details.Where(o => o.SocialInsuranceTime == tdRow[2]).FirstOrDefault();
                    bool NeedAddNew = false;
                    if (detailRes == null)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        NeedAddNew = true;
                    }
                    switch (Type)
                    {
                        case InfoType.养老保险:
                            detailRes.PensionAmount += tdRow[7].ToDecimal(0);
                            detailRes.CompanyPensionAmount += tdRow[5].ToDecimal(0);
                            break;
                        case InfoType.医疗保险:
                            detailRes.MedicalAmount += tdRow[7].ToDecimal(0);
                            detailRes.CompanyMedicalAmount += tdRow[5].ToDecimal(0);
                            break;
                        case InfoType.失业保险:
                            detailRes.UnemployAmount += tdRow[7].ToDecimal(0) + tdRow[5].ToDecimal(0);
                            break;
                        case InfoType.生育保险:
                            detailRes.MaternityAmount += tdRow[7].ToDecimal(0) + tdRow[5].ToDecimal(0);
                            break;
                        case InfoType.工伤保险:
                            detailRes.EmploymentInjuryAmount += tdRow[7].ToDecimal(0) + tdRow[5].ToDecimal(0);
                            break;
                    }
                    if (detailRes.SocialInsuranceBase == 0 && tdRow[4].ToDecimal(0)!= 0)
                        detailRes.SocialInsuranceBase = tdRow[4].ToDecimal(0);
                    if (NeedAddNew)
                    {
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;
                        detailRes.PayTime = tdRow[1];
                        detailRes.SocialInsuranceTime = tdRow[2];
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        if(Type == InfoType.医疗保险)
                            detailRes.CompanyName = tdRow[9];
                        else
                            detailRes.CompanyName = tdRow[10];
                        Res.Details.Add(detailRes);
                    }
                }
                year--;

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
                Url = baseUrl + "index.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "random/rand.do";
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                Url = baseUrl + "client/login.do";
                postdata = String.Format("random={0}&card_num={1}&pwd={2}&lb=1", socialReq.Vercode, socialReq.Identitycard, socialReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    //Referer = baseUrl + "index.jsp",
                    Referer ="http://www.yt12333.com/ssc/",
                    Encoding=Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
               // string success = jsonParser.GetResultFromParser(httpResult.Html, "success");
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK )
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                JObject JsonStr = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                if (JsonStr["success"].ToString().Trim().ToLower()!="true")
                {
                    Res.StatusDescription = JsonStr["msg"].ToString();
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 第二步， 获取基本信息
                Url = baseUrl + "client/personInfo.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "client/main.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);


                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='datatable']/tr/td/input", "value");
                if (results.Count <= 0 || string.IsNullOrEmpty(results[0]))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Res.EmployeeNo = results[0];//编号
                Res.Name = results[1];//姓名
                Res.BirthDate = results[3];//出生日期
                Res.IdentityCard = results[8];//身份证号
                Res.Sex = results[2];//性别
                Res.CompanyNo = results[4];//单位编号
                Res.CompanyName = results[5];//单位名称
                Res.EmployeeStatus = results[6];//人员状态
                Res.Address = results[10];//通讯地址
                Res.Phone = results[11];//联系电话
                //>缴费情况统计
                Url = baseUrl + "client/payhistory.do?type=aged";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "client/left.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string payMonthStr = CommonFun.GetMidStr(httpResult.Html, "<td>共缴费", ",共中断");
                Res.PaymentMonths = CommonFun.GetMidStr(payMonthStr, "", "年").ToInt(0) * 12 + CommonFun.GetMidStr(payMonthStr, "年", "月").ToInt(0);
               //发卡银行
                Url = baseUrl + "client/cardInfo.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "client/left.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='datatable']//input[@name='FKYH']", "value");
                if (results.Count>0)
                {
                    Res.Bank = results[0];
                }
               
                #endregion


                #region 第三步,获取详细信息
                InitPageHash();
                foreach (InfoType type in Enum.GetValues(typeof(InfoType)))
                {
                    GetAllDetail(type, ref Res);
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
