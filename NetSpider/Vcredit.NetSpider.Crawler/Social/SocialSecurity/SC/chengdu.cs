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
using Vcredit.NetSpider.DataAccess.Cache;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.SC
{
    public class chengdu : ISocialSecurityCrawler
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
        string socialCity = "sc_chengdu";
        #endregion

        #region 私有方法
        enum InfoType
        {
            养老保险,
            医疗保险,
            失业保险
        }

        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息

        /// <summary>
        /// 将各种保险对应的页面所需信息存入PageHash
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, new string[] { "3" });
            PageHash.Add(InfoType.医疗保险, new string[] { "5" });
            PageHash.Add(InfoType.失业保险, new string[] { "8" });
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

            Url = "http://insurance.cdhrss.gov.cn/QueryInsuranceInfo.do?flag=" + ((string[])PageHash[Type])[0];
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
                throw new Exception("Http Fail");
            }
            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr");

            if (results.Count <= 2)
            {
                return;
                //if (Type == InfoType.养老保险)
                //{
                //    Res.StatusDescription = "无养老保险信息";
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    throw new Exception("无养老保险信息");
                //}
                //else
                //return;
            }

            foreach (string item in results)
            {
                List<string> Detail = HtmlParser.GetResultFromParser(item, "td");
                if (Detail.Count != 10)
                    continue;

                string EnterAccountMedicalAmount = string.Empty;
                if (Type != InfoType.医疗保险)
                    Detail.RemoveAt(0);
                else
                {
                    EnterAccountMedicalAmount = Detail[5].ToTrim();
                    Detail.RemoveAt(5);
                }

                SocialSecurityDetailQueryRes detailRes = null;
                string SocialInsuranceTime = Detail[0].ToTrim();
                //string PaymentType = Detail[5].ToTrim() == "正常缴费记录" ? ServiceConsts.SocialSecurity_PaymentType_Normal : Detail[5].ToTrim();
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
                    detailRes.PayTime = Detail[8].ToTrim();
                    detailRes.SocialInsuranceTime = SocialInsuranceTime;
                    detailRes.CompanyName = Detail[1].ToTrim();
                    try
                    {
                        detailRes.SocialInsuranceBase = Detail[2].ToTrim().Substring(1).ToDecimal(0);
                    }
                    catch { }
                    detailRes.PaymentType = Detail[5].ToTrim() == "正常缴费记录" ? ServiceConsts.SocialSecurity_PaymentType_Normal : Detail[5].ToTrim();
                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    //detailRes.PaymentFlag = PaymentType == ServiceConsts.SocialSecurity_PaymentType_Normal ? ServiceConsts.ProvidentFund_PaymentFlag_Normal : ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                }
                switch (Type)
                {
                    case InfoType.养老保险:
                        detailRes.PensionAmount = Detail[4].ToTrim().Substring(1).ToDecimal(0);
                        detailRes.CompanyPensionAmount = Detail[3].ToTrim().Substring(1).ToDecimal(0);
                        break;
                    case InfoType.医疗保险:
                        detailRes.MedicalAmount = Detail[4].ToTrim().Substring(1).ToDecimal(0);
                        detailRes.CompanyMedicalAmount = Detail[3].ToTrim().Substring(1).ToDecimal(0);
                        detailRes.EnterAccountMedicalAmount = EnterAccountMedicalAmount.Substring(1).ToDecimal(0);
                        break;
                    case InfoType.失业保险:
                        detailRes.UnemployAmount = Detail[3].ToTrim().Substring(1).ToDecimal(0) + Detail[4].ToTrim().Substring(1).ToDecimal(0);
                        break;
                }

                if (NeedSave)
                    Res.Details.Add(detailRes);
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
                Url = "http://insurance.cdhrss.gov.cn/index.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
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

                //Url = "http://www.cdhrss.gov.cn/images/image.jsp";
                //Url = "http://insurance.cdhrss.gov.cn/images/image.jsp?";
                Url = "http://insurance.cdhrss.gov.cn/captcha.do";
                httpItem = new HttpItem()
                {
                    Accept = "image/png,image/*;q=0.8,*/*;q=0.5",
                    URL = Url,
                    Referer = "http://insurance.cdhrss.gov.cn/index.jsp",
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
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(socialReq.Token);
                    SpiderCacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                //Url = "http://insurance.cdhrss.gov.cn/LoginSIAction.do";
                Url = "http://insurance.cdhrss.gov.cn/loginAction.do";
                postdata = String.Format("siusername={0}&sipassword={1}&randCode={2}", socialReq.Username, socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "http://www.cdhrss.gov.cn/",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (httpResult.Html.IndexOf("图形验证码输入错误。") != -1)
                {
                    //goto Lable_Start;
                    Res.StatusDescription = "图形验证码输入错误。";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (httpResult.Html.IndexOf("社保编码或者查询密码错误。") != -1)
                {
                    Res.StatusDescription = "社保编码或者查询密码错误。";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://insurance.cdhrss.gov.cn/QueryInsuranceInfo.do?flag=1";
                //Url = "http://insurance/QueryListAction.do?title=%25E4%25B8%25AA%25E4%25BA%25BA%25E5%259F%25BA%25E6%259C%25AC%25E4%25BF%25A1%25E6%2581%25AF&bizID=BC.000.000.001";
                httpItem = new HttpItem()
                {
                    URL = Url,
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//tr/th", "text", true);
                List<string> results_add = HtmlParser.GetResultFromParser(httpResult.Html, "//tr/td");

                if (results.Count > 0 && results.Count == results_add.Count)
                {
                    for (int i = 0; i < results.Count; i++)
                    {
                        switch (results[i].Replace("：", ":").Replace("&nbsp;", ""))
                        {
                            case "个人编码：":
                                Res.EmployeeNo = results_add[i];
                                break;
                            case "姓名:":
                                Res.Name = results_add[i];
                                break;
                            case "身份证号码:":
                                Res.IdentityCard = results_add[i];
                                break;
                            case "性别:":
                                Res.Sex = results_add[i];
                                break;
                            case "出生日期:":
                                Res.BirthDate = results_add[i];
                                break;
                            case "参工时间:":
                                Res.WorkDate = results_add[i];
                                break;
                            case "人员状态:":
                                Res.EmployeeStatus = results_add[i];
                                break;
                            case "移动电话:":
                                Res.Phone = results_add[i];
                                break;
                        }
                    }
                }
                if (Res.IdentityCard.Length > 0 && Res.BirthDate.Length > 0)
                {
                    var xxx = Res.IdentityCard.Substring(Res.IdentityCard.Length - 3, 3);
                    if (socialReq.Identitycard.IndexOf(Res.IdentityCard.Substring(0, 3)) > -1 && socialReq.Identitycard.IndexOf(Res.IdentityCard.Substring(Res.IdentityCard.Length - 3, 3)) > -1 && socialReq.Identitycard.IndexOf(Res.BirthDate.Replace("-", "")) > -1)
                    {
                        Res.IdentityCard = socialReq.Identitycard;
                    }
                }

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
