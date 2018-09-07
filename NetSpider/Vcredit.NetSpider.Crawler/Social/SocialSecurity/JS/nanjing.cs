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
using Vcredit.NetSpider.DataAccess.Cache;
namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.JS
{
    public class nanjing : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "";
        string socialCity = "js_nanjing";
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
            PageHash.Add(InfoType.养老保险, new string[] { "1", "养老" });
            PageHash.Add(InfoType.医疗保险, new string[] { "5", "医疗" });
            PageHash.Add(InfoType.失业保险, new string[] { "4", "失业" });
            PageHash.Add(InfoType.工伤保险, new string[] { "2", "工伤" });
            PageHash.Add(InfoType.生育保险, new string[] { "3", "生育" });
            
        }

        void GetAllDetail(InfoType Type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            SocialSecurityDetailQueryRes detailRes = new SocialSecurityDetailQueryRes();

            Url = "http://wsbs.njhrss.gov.cn/NJLD/ZjGrJf?act=perform";
            postdata = "xz=" + ((string[])PageHash[Type])[0] +"&hide=null&Submit=%E6%9F%A5%E8%AF%A2";
            httpItem = new HttpItem()
            {
                URL = Url,
                Method = "post",
                Postdata = postdata,
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table1']/tr", "", true);
            List<string> UpdateYear = new List<string>();
            //System.Collections.Hashtable PayTimeHash= new System.Collections.Hashtable();
            for (int i = 0; i < results.Count; i++)
            {
                var tdRow = HtmlParser.GetResultFromParser(results[i], "//td");
                if (tdRow.Count != 8 || tdRow[0] != ((string[])PageHash[Type])[1])
                {
                    continue;
                }

                string SocialInsuranceTime = tdRow[3].ToTrim("-").Substring(0, 6);
                detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                bool NeedAddNew = false;
                if (detailRes == null)
                {
                    detailRes = new SocialSecurityDetailQueryRes();
                    NeedAddNew = true;
                    UpdateYear.Add(SocialInsuranceTime);
                }
                switch (Type)
                {
                    case InfoType.养老保险:
                        detailRes.CompanyPensionAmount += tdRow[4].ToTrim().ToDecimal(0);
                        detailRes.PensionAmount += tdRow[5].ToTrim().ToDecimal(0);
                        break;
                    case InfoType.医疗保险:
                        detailRes.CompanyMedicalAmount += tdRow[4].ToTrim().ToDecimal(0);
                        detailRes.MedicalAmount += tdRow[5].ToTrim().ToDecimal(0);
                        break;
                    case InfoType.失业保险:
                        detailRes.UnemployAmount += tdRow[4].ToTrim().ToDecimal(0) + tdRow[5].ToTrim().ToDecimal(0);
                        break;
                    case InfoType.生育保险:
                        detailRes.MaternityAmount += tdRow[4].ToTrim().ToDecimal(0) + tdRow[5].ToTrim().ToDecimal(0);
                        break;
                    case InfoType.工伤保险:
                        detailRes.EmploymentInjuryAmount += tdRow[4].ToTrim().ToDecimal(0) + tdRow[5].ToTrim().ToDecimal(0);
                        break;
                }    
                if(UpdateYear.Contains(SocialInsuranceTime))
                {
                    detailRes.SocialInsuranceBase += tdRow[1].ToDecimal(0);
                }
                if (NeedAddNew)
                {
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;
                    detailRes.PayTime = tdRow[2];
                    detailRes.SocialInsuranceTime = SocialInsuranceTime;
                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
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
            Dictionary<string, CookieCollection> dic = new Dictionary<string, CookieCollection>();
            try
            {
                //Url = "http://wsbs.njhrss.gov.cn/NJLD/index.jsp";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "get",
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //dic.Add("cookie_old", cookies);
                //cookies = new CookieCollection();

                Url = "http://wsbs.njhrss.gov.cn/NJLD/web/cbzm/yzpt.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //dic.Add("cookie_new", cookies);

                Url = "http://wsbs.njhrss.gov.cn/NJLD/Images";
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
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);

                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                SpiderCacheHelper.SetCache(token, cookies);
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
            Res.SocialSecurityCity = socialCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            SocialSecurityDetailQueryRes detailRes = new SocialSecurityDetailQueryRes();
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(socialReq.Token);
                    SpiderCacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.LoginType == "1" && (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty()))
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                else if ((socialReq.LoginType == "2" || socialReq.LoginType == "3") && (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty()))
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 缴费证明验证码流程
                if (socialReq.LoginType == "3")//验证码
                {
                    Url = "http://wsbs.njhrss.gov.cn/NJLD/CbzmAction?act=grYz";
                    postdata = String.Format("sjm={0}&yzm={1}&dl=", socialReq.Password, socialReq.Vercode);

                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Referer = "http://wsbs.njhrss.gov.cn/NJLD/web/cbzm/yzpt.jsp",
                        Host = "wsbs.njhrss.gov.cn",
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
                    if (httpResult.Html.StartsWith("<script>alert"))
                    {
                        string errorStr = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "')");
                        Res.StatusDescription = errorStr;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tab1']/tr/td");
                    if (results.Count == 7)
                    {
                        Res.Name = results[0].Replace("姓名:", "").ToTrim();
                        Res.IdentityCard = results[1].Replace("身份证号:", "").ToTrim();
                        Res.EmployeeStatus = results[4].Replace("参保状态：", "").ToTrim();
                        Res.CompanyName = results[5].Replace("单位名称:", "").ToTrim().ToTrim("-");
                    }

                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@colspan='5']");
                    if (results.Count == 2)
                    {
                        Res.PaymentMonths = results[1].ToInt(0);
                    }

                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tb2']/tr");
                    foreach (string item in results)
                    {
                        List<string> detail = HtmlParser.GetResultFromParser(item, "td");
                        if (detail.Count != 20)
                            continue;

                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;
                        detailRes.CompanyName = detail[19];
                        detailRes.PayTime = detail[0].Replace("/", "");
                        detailRes.SocialInsuranceTime = detailRes.PayTime;
                        detailRes.CompanyPensionAmount = detail[3].ToDecimal(0);
                        detailRes.PensionAmount = detail[4].ToDecimal(0);
                        detailRes.CompanyMedicalAmount = detail[7].ToDecimal(0);
                        detailRes.MedicalAmount = detail[8].ToDecimal(0);
                        detailRes.UnemployAmount = detail[11].ToDecimal(0) + detail[12].ToDecimal(0);
                        detailRes.EmploymentInjuryAmount = detail[15].ToDecimal(0);
                        detailRes.MaternityAmount = detail[18].ToDecimal(0);
                        detailRes.SocialInsuranceBase = detail[2].ToDecimal(0);
                        if (item.Contains("√"))
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        else
                            detailRes.PaymentFlag = "未到账";
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;

                        Res.Details.Add(detailRes);
                    }
                }
                #endregion

                #region 第一步，登录
                else
                {
                    Url = "http://wsbs.njhrss.gov.cn/NJLD/LoginAction?act=CompanyLoginPerson";
                    if (socialReq.LoginType == "1")//社会保障卡号
                    {
                        if (socialReq.Username.IsEmpty())
                        {
                            Res.StatusDescription = ServiceConsts.RequiredEmpty;
                            Res.StatusCode = ServiceConsts.StatusCode_fail;
                            return Res;
                        }
                        postdata = String.Format("u={0}&p={1}&lx=1&key={2}&dl=", socialReq.Username, socialReq.Password, socialReq.Vercode);
                    }
                    else if (socialReq.LoginType == "2")//身份证号
                    {
                        Url = "http://wsbs.njhrss.gov.cn/NJLD/LoginAction?act=PersonLogin";
                        postdata = String.Format("u={0}&p={1}&key={2}&dl=", socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
                    }
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
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    if (httpResult.Html.IndexOf("/NJLD/company/system/lesmain.jsp") == -1)
                    {
                        string errorStr = CommonFun.GetMidStr(httpResult.Html, "<SCRIPT LANGUAGE=\"JavaScript\">alert(\"", "\")");
                        Res.StatusDescription = errorStr;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }

                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                    #region 基本信息
                    Url = "http://wsbs.njhrss.gov.cn/NJLD/company/system/lesmainload.jsp";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table1']/tr/td", "", true);
                    if (results.Count > 8)
                    {
                        Res.EmployeeNo = results[1];
                        Res.Name = results[3];
                        Res.IdentityCard = results[5];
                        Res.CompanyName = results[7];
                        Res.EmployeeStatus = results[9];
                    }
                    else
                    {
                        Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    #endregion

                    #region 详细信息
                    InitPageHash();
                    foreach (InfoType type in Enum.GetValues(typeof(InfoType)))
                    {
                        GetAllDetail(type, ref Res);
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
}
