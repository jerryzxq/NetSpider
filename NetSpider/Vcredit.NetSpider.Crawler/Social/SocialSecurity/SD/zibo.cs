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
namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.SD
{
    public class zibo : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://sdzb.hrss.gov.cn:8001/";
        string socialCity = "sd_zibo";
        #endregion
        #region 私有方法
        enum InfoType
        {
            医疗保险,
            养老保险,
            失业保险,
            工伤保险
        }

        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息
        /// <summary>
        /// 明细基数按医疗明细的计算
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.医疗保险, new string[] { "siMedi.do", "queryMediPayHis", "medipayhis", "6" });
            PageHash.Add(InfoType.养老保险, new string[] { "siAd.do", "queryAgedPayHis", "agedpayhis", "5" });
            PageHash.Add(InfoType.失业保险, new string[] { "siLost.do", "queryLostPayHis", "lostpayhis", "5" });
            PageHash.Add(InfoType.工伤保险, new string[] { "siHarm.do", "queryHarmPayHis", "harmpayhis", "4" });
        }

        void GetAllDetail(InfoType Type, string __usersession_uuid, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            SocialSecurityDetailQueryRes detailRes = new SocialSecurityDetailQueryRes();

            Url = baseUrl + ((string[])PageHash[Type])[0];
            postdata = string.Format("method={0}&_random=0.3627017554988906&__usersession_uuid={1}", ((string[])PageHash[Type])[1], __usersession_uuid);
            httpItem = new HttpItem()
            {
                URL = Url,
                Method = "post",
                Postdata = postdata,
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@name='" + ((string[])PageHash[Type])[2] + "']/tr", "");
            //System.Collections.Hashtable PayTimeHash= new System.Collections.Hashtable();
            for (int i = 0; i < results.Count; i++)
            {
                var tdRow = HtmlParser.GetResultFromParser(results[i], "td/input", "value");
                if (tdRow.Count != ((string[])PageHash[Type])[3].ToInt(0))
                {
                    continue;
                }
                if (Type == InfoType.医疗保险 && tdRow[0] != "职工医疗")
                {
                    continue;
                }
                string SocialInsuranceTime = tdRow[1].ToTrim("-");
                detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                bool NeedAdd = false;
                if (detailRes == null)
                {
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;
                    detailRes.PayTime = SocialInsuranceTime;
                    detailRes.SocialInsuranceTime = SocialInsuranceTime;
                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    NeedAdd = true;
                }
                switch (Type)
                {
                    case InfoType.医疗保险:
                        detailRes.CompanyMedicalAmount += tdRow[4].ToTrim().ToDecimal(0);
                        detailRes.MedicalAmount += tdRow[5].ToTrim().ToDecimal(0);
                        detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                        break;
                    case InfoType.养老保险:
                        detailRes.CompanyPensionAmount += tdRow[3].ToTrim().ToDecimal(0);
                        detailRes.PensionAmount += tdRow[4].ToTrim().ToDecimal(0);
                        detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : tdRow[2].ToDecimal(0);
                        break;
                    case InfoType.失业保险:
                        detailRes.UnemployAmount += tdRow[3].ToTrim().ToDecimal(0) + tdRow[4].ToTrim().ToDecimal(0);
                        break;
                    case InfoType.工伤保险:
                        detailRes.EmploymentInjuryAmount += tdRow[3].ToTrim().ToDecimal(0);
                        break;
                }
                if (NeedAdd)
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
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "authcode?i=0.5515986692243237";
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = baseUrl + "logon.do";
                postdata = String.Format("method=writeMM2Temp&_xmlString=<?xml version=\"1.0\" encoding=\"UTF-8\"?><p><s tempmm=\"{0}\"/></p>&_random=0.6570567348162227", socialReq.Password);
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
                if (httpResult.StatusCode != HttpStatusCode.OK && bool.Parse(httpResult.Html))
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                Url = baseUrl + "logon.do";
                postdata = String.Format("method=doLogon&_xmlString=<?xml version=\"1.0\" encoding=\"UTF-8\"?><p><s userid=\"{0}\"/><s usermm=\"{1}\"/><s authcode=\"{2}\"/></p>&_random=0.29480932264256365", socialReq.Identitycard, CommonFun.GetMd5Str(socialReq.Password), socialReq.Vercode);
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
                if (httpResult.StatusCode != HttpStatusCode.OK && bool.Parse(httpResult.Html))
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                if (!httpResult.Html.Contains("__usersession_uuid"))
                {
                    Res.StatusDescription = httpResult.Html;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                string __usersession_uuid = jsonParser.GetResultFromParser(httpResult.Html, "__usersession_uuid");
                Url = baseUrl + "logon.do";
                postdata = String.Format("method=checkPwd&_xmlString=&_random=0.08888123655460256&__usersession_uuid={0}", __usersession_uuid);
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
                if (httpResult.StatusCode != HttpStatusCode.OK && bool.Parse(httpResult.Html))
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 第二步， 获取基本信息
                Url = baseUrl + "hspUser.do";
                postdata = String.Format("method=fwdQueryPerInfo&_random=0.5112008709021018&__usersession_uuid={0}", __usersession_uuid);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='sfzhm']", "value");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证号
                }
                else
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='xm']", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='xbmc']", "value");
                if (results.Count > 0)
                {
                    Res.Sex = results[0];//性别
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='csrq']", "value");
                if (results.Count > 0)
                {
                    Res.BirthDate = results[0];//出生日期
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='mzmc']", "value");
                if (results.Count > 0)
                {
                    Res.Race = results[0];//民族
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='jtzz']", "value");
                if (results.Count > 0)
                {
                    Res.Address = results[0];//通讯地址
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='yzbm']", "value");
                if (results.Count > 0)
                {
                    Res.ZipCode = results[0];//邮政编码
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='lxdh']", "value");
                if (results.Count > 0)
                {
                    Res.Phone = results[0];//联系电话
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='dwmc']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];//单位名称
                }

                //账户总额、养老基数
                Url = baseUrl + "systemOSP.do";
                postdata = "method=returnMain&_random=0.9152969575798503&__usersession_uuid=" + __usersession_uuid;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='mainTotalblock']/div/font");
                if (results.Count == 3)
                {
                    Res.InsuranceTotal = results[0].ToDecimal(0);
                    Res.SocialInsuranceBase = results[1].ToDecimal(0);
                }
                #endregion

                #region 第三步，查询明细
                //当年明细
                InitPageHash();
                foreach (InfoType type in Enum.GetValues(typeof(InfoType)))
                {
                    GetAllDetail(type, __usersession_uuid, ref Res);
                }
                //医保历年明细
                Url = baseUrl + "scQuery.do";
                postdata = string.Format("method=queryGrzhszxq&_xmlString=<?xml version=\"1.0\" encoding=\"UTF-8\"?><p><s qsny=\"{0}\"/><s zzny=\"{1}\"/></p>&_random=0.6786379366769493&__usersession_uuid={2}", DateTime.Now.AddYears(-2).ToString("yyyy01"), DateTime.Now.ToString("yyyyMM"), __usersession_uuid);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='defaultTableClass']/tr");
                foreach (string item in results)
                {
                    List<string> tdrow = HtmlParser.GetResultFromParser(item, "td/input", "value");
                    if (tdrow.Count != 5) continue;
                    if (tdrow[3] != "收入：职工医疗" || !tdrow[4].StartsWith("缴费年月：")) continue;
                    string SocialInsuranceTime = tdrow[4].Replace("缴费年月：", "");
                    detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                    bool NeedAdd = false;
                    if (detailRes == null)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.SocialInsuranceTime = SocialInsuranceTime;
                        detailRes.PayTime = tdrow[0];
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        NeedAdd = true;
                    }
                    if (detailRes.MedicalAmount == 0)
                    {
                        detailRes.MedicalAmount += tdrow[1].ToDecimal(0);
                    }
                    if (NeedAdd)
                    {
                        Res.Details.Add(detailRes);
                    }
                }
                #endregion

                #region 计算
                //计算最近24个月的缴费情况
                string SocialTime = string.Empty;
                string Payment_State = string.Empty;
                int PaymentMonths_Continuous = 0;
                int maxmonth = 24;//20160316修改，最小抓25个月
                for (int i = 0; i <= maxmonth; i++)//20160314修改，若最近2个月缴费情况不为正常则，提前2个月开始计算24个月的缴费情况
                {
                    SocialTime = DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString7);

                    SocialSecurityDetailQueryRes query = new SocialSecurityDetailQueryRes();
                    query = Res.Details.Where(o => o.SocialInsuranceTime == SocialTime && o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal && (o.MedicalAmount != 0 || o.CompanyMedicalAmount != 0)).FirstOrDefault();
                    if (query != null)
                    {
                        Payment_State += "N ";//缴费
                    }
                    else
                    {
                        if (i == 0)
                        {
                            maxmonth = 25;//如果最近一个月不正常则往前推1个月
                        }
                        else if (i == 1)
                        {
                            if (Payment_State != "N ")
                            {
                                maxmonth = 26;//如果最近两个月不正常则往前推2个月
                            }
                            else
                            {
                                Payment_State += "/ ";//未缴费
                            }
                        }
                        else
                        {
                            Payment_State += "/ ";//未缴费
                        }
                    }
                }
                //最近24个月连续缴费情况
                var Continuous = Payment_State.ToTrim().Split('/');
                foreach (string item in Continuous)
                {
                    if (!item.IsEmpty())
                    {
                        PaymentMonths_Continuous = item.Split('N').Count() - 1;
                        break;
                    }
                }
                Res.Payment_State = Payment_State;
                Res.PaymentMonths_Continuous = PaymentMonths_Continuous;
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
