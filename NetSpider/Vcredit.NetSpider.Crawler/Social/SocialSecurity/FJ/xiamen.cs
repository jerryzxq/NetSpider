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
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Vcredit.NetSpider.DataAccess.Cache;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.FJ
{
    public class xiamen : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "https://app.xmhrss.gov.cn/";
        string socialCity = "fj_xiamen";
        #endregion
        private static bool RemoteCertificateValidate(
            object sender, X509Certificate cert,
              X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }
        #region 私有方法

        /// <summary>
        /// 获取缴费明细
        /// </summary>
        /// <param name="Res"></param>
        private void GetAllDetail(ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            string postData = string.Empty;
            List<string> results = new List<string>();
            //首页数据
            Url = "https://app.xmhrss.gov.cn/UCenter/sbjfxxcx.xhtml";
            httpItem = new HttpItem()
            {
                URL = Url,
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            cookies = CommonFun.GetCookieCollection(httpResult.CookieCollection, cookies);
            string[] temppage = CommonFun.GetMidStr(httpResult.Html, "showPages(", ")").Split(',');
            int totalPage = 0;
            if (temppage.Count() == 6)
            {
                totalPage = temppage[1].ToInt(0);
            }
            results = HtmlParser.GetResultFromParser(CommonFun.ClearFlag(httpResult.Html), "//table[@class='tab5']/tr");
            //剩余页码
            for (int i = 2; i <= totalPage; i++)
            {
                Url = "https://app.xmhrss.gov.cn/UCenter/sbjfxxcx.xhtml?pageNo=" + i;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results.AddRange(HtmlParser.GetResultFromParser(CommonFun.ClearFlag(httpResult.Html), "//table[@class='tab5']/tr"));
            }
            string insuranceTime;//应属年月
            foreach (string s in results)
            {
                List<string> tdRow = HtmlParser.GetResultFromParser(s, "//td", "text");
                if (tdRow.Count != 9) continue;
                insuranceTime = tdRow[3];
                SocialSecurityDetailQueryRes detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == insuranceTime);
                bool needSave = false;
                if (detailRes == null)
                {
                    needSave = true;
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;
                    detailRes.SocialInsuranceTime = insuranceTime;
                    detailRes.PayTime = insuranceTime;
                    if (tdRow[8] == "已缴费")
                    {
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    }
                    else if (tdRow[8].Contains("补"))
                    {
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Back;
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Back;
                    }
                    else
                    {
                        detailRes.PaymentFlag = tdRow[8];
                        if (tdRow[2] == "正常参保")
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        }
                        else
                        {
                            detailRes.PaymentType = tdRow[2];
                        }
                    }
                    detailRes.SocialInsuranceBase = tdRow[7].ToDecimal(0);
                }
                switch (tdRow[1])
                {
                    case "养老保险":
                        detailRes.PensionAmount += (decimal)0.08 * (tdRow[7].ToDecimal(0));
                        detailRes.CompanyPensionAmount += (tdRow[5].ToDecimal(0) - (decimal)0.08 * (tdRow[7].ToDecimal(0)));
                        break;
                    case "医疗保险":
                        detailRes.CompanyMedicalAmount += (decimal)0.02 * (tdRow[8].ToDecimal(0));
                        detailRes.MedicalAmount += (tdRow[5].ToDecimal(0) - (decimal)0.02 * (tdRow[7].ToDecimal(0)));
                        break;
                    case "失业保险":
                        detailRes.UnemployAmount += tdRow[5].ToDecimal(0);
                        break;
                    case "工伤保险":
                        detailRes.EmploymentInjuryAmount += tdRow[5].ToDecimal(0);
                        break;
                    case "生育保险":
                        detailRes.MaternityAmount += tdRow[5].ToDecimal(0);
                        break;
                    case "公务员补助":
                        detailRes.CivilServantMedicalAmount += tdRow[5].ToDecimal(0);
                        break;
                    case "商业保险":
                        switch (tdRow[0])
                        {
                            case "养老待遇保险":
                                detailRes.PensionAmount += tdRow[5].ToDecimal(0);
                                break;
                            case "基本医疗保险":
                                detailRes.CompanyMedicalAmount += tdRow[5].ToDecimal(0);
                                break;
                            case "失业待遇保险":
                                detailRes.UnemployAmount += tdRow[5].ToDecimal(0);
                                break;
                            case "工伤待遇保险":
                                detailRes.EmploymentInjuryAmount += tdRow[5].ToDecimal(0);
                                break;
                            case "生育待遇保险":
                                detailRes.MaternityAmount += tdRow[5].ToDecimal(0);
                                break;
                        }
                        break;
                }
                if (!needSave) continue;
                Res.Details.Add(detailRes);

            }
        }
        #endregion
        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "login.xhtml";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    SecurityProtocolType = SecurityProtocolType.Tls,
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

                Url = baseUrl + "vcode.xhtml";
                httpItem = new HttpItem()
                {
                    URL = Url,
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
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = socialCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            int PaymentMonths = 0;
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
                ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
                #region 第一步，登录
                Url = baseUrl + "login_dowith.xhtml";
                postdata = String.Format("id0000={0}&userpwd={1}&validateCode={2}&date={3}", socialReq.Username, socialReq.Password, socialReq.Vercode, CommonFun.GetTimeStamp());
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
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (jsonParser.GetResultFromParser(httpResult.Html, "result").ToLower() != "true")
                {
                    Res.StatusDescription = jsonParser.GetResultFromParser(httpResult.Html, "msg");
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region 第二步，个人基本信息
                Url = baseUrl + "UCenter/index_grjbxx.xhtml";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='con_r pad_t20']/table/tr/td");

                if (results.Count > 17)
                {
                    Res.Name = results[1].ToTrim("<br>");//姓名
                    Res.SpecialPaymentType = CommonFun.GetMidStr(results[7].ToTrim(),"", "&nbsp");
                    Res.CompanyNo = results[11].ToTrim();//单位编号
                    Res.CompanyType = results[13].ToTrim();//单位状态
                    Res.EmployeeStatus = results[17].ToTrim();//人员状态
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //参保情况
                Url = baseUrl + "UCenter/grcbqk.xhtml";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='con_r pad_t20']/table/tr");
                if (results.Count > 0)
                {
                    foreach (string item in results)
                    {
                        if (item.Contains("养老缴费工资"))
                        {
                            Res.SocialInsuranceBase = HtmlParser.GetResultFromParser(item, "/td")[1].ToDecimal(0);
                            break;
                        }
                    }
                }
                #endregion
                #region 第四步，查询缴费明细
                try
                {
                    GetAllDetail(ref Res);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
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
    }

}
