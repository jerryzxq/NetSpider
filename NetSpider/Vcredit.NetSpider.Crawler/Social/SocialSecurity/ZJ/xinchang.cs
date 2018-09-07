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
    public class xinchang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://122.224.17.5:8800/insur_wd/";
        string socialCity = "zj_xinchang";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "login.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
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

                Url = baseUrl + "index.do";
                postdata = String.Format("flag=0&userCode={0}&userName={1}&password={2}&yzm={3}&submit1=", socialReq.Identitycard, socialReq.Name.ToUrlEncode(), socialReq.Password, "");
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
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');history.go(-1)");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 获取基本信息

                Url = baseUrl + "InsurPersion.do?StatType=PerBaseInfoQuery";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "common/LeftMenu.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='biaogeb']/tr/td[@align='center']/div[@align='center']", "text");
                if (results.Count <= 0 || string.IsNullOrEmpty(results[6]))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.EmployeeNo = results[6].Trim();//编号
                Res.Name = results[3].Trim();//姓名
                Res.BirthDate = results[1].Trim();//出生日期
                Res.ZipCode = results[8].Trim();//邮编
                Res.IdentityCard = results[9].Trim();//身份证号
                Res.Sex = results[12].Trim();//性别
                Res.CompanyName = results[0].Trim();//单位名称
                Res.EmployeeStatus = results[14].Trim();//人员状态
                Res.Race = results[15].Trim();//民族
                Res.Address = results[5].Trim();//通讯地址
                Res.Phone = results[2].Trim();//联系电话
                Res.WorkDate = results[4].Trim();//参加工作时间
                //个人参保信息
                Url = baseUrl + "InsurPersion.do?StatType=PerInsurInfo";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "common/LeftMenu.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='biaogea'][1]/tr[position()>1]/td/div", "text", true);
                for (int i = 0; i <= results.Count - 6; i = i + 6)
                {
                    Res.SpecialPaymentType += results[i] + ":" + results[i + 5] + ";";
                    if (results[i] == "养老保险")
                    {
                        Res.PaymentMonths = results[i + 4].ToInt(0);
                    }
                }
                //职工个人养老账户信息
                Url = baseUrl + "InsurPersion.do?StatType=PensionInfo";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "common/LeftMenu.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='biaogeb']/tr[3]/td[@align='center'][3]/div", "text", true);
                if (results.Count > 0)
                {
                    Res.PersonalInsuranceTotal = results[0].ToDecimal(0);
                }
                if (Res.PersonalInsuranceTotal > 0)
                {
                    //职工个人医疗账户信息
                    Url = baseUrl + "InsurPersion.do?StatType=MedicalAccount";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        //Referer = baseUrl + "common/LeftMenu.do",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='biaogeb']/tr[2]/td[@align='center'][3]/div", "text", true);
                    if (results.Count > 0)
                    {
                        Res.InsuranceTotal = results[0].ToDecimal(0) + Res.PersonalInsuranceTotal;
                    }
                }
                #endregion

                #region 查询明细

                int currentPage = 1;
                int pageCount = 1;
                results = new List<string>();
                do
                {
                    Url = baseUrl + string.Format("InsurList.do?StatType=PaymentinformationInfo&currentPage={0}&insr_detail_name=", currentPage);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Referer = baseUrl + "common/LeftMenu.do",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (currentPage == 1)
                    {
                        pageCount = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='pageCount']", "value")[0].ToInt(0);
                    }
                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='biaogea']/tr[position()>1]", "", true));
                    currentPage++;
                } while (currentPage <= pageCount);
                foreach (string s in results)
                {
                    List<string> tdRow = HtmlParser.GetResultFromParser(s, "/td/div");
                    if (tdRow.Count < 6) continue;
                    if (string.IsNullOrEmpty(tdRow[4]) || tdRow[4]=="0") continue;
                    detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == tdRow[0]);
                    bool isSave = false;
                    if (detailRes == null)
                    {
                        isSave = true;
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.IdentityCard = Res.IdentityCard;
                        detailRes.Name = Res.Name;
                        detailRes.PayTime = tdRow[6];
                        detailRes.SocialInsuranceTime = tdRow[0];
                        detailRes.PaymentFlag = tdRow[5] != "未缴" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                        detailRes.PaymentType = tdRow[5];
                    }
                    switch (tdRow[1])
                    {
                        case "养老保险":
                            if (tdRow[2].IndexOf("个人", StringComparison.Ordinal) > -1)
                            {
                                detailRes.PensionAmount += tdRow[4].ToDecimal(0);
                            }
                            else if (tdRow[2].IndexOf("单位", StringComparison.Ordinal) > -1 || tdRow[2].IndexOf("统筹", StringComparison.Ordinal) > -1)
                            {
                                detailRes.CompanyPensionAmount += tdRow[4].ToDecimal(0);
                            }
                            detailRes.SocialInsuranceBase = tdRow[3].ToDecimal(0);
                            break;
                        case "基本医疗保险":
                            if (tdRow[2].IndexOf("个人", StringComparison.Ordinal) > -1)
                            {
                                detailRes.MedicalAmount += tdRow[4].ToDecimal(0);
                            }
                            else if (tdRow[2].IndexOf("单位", StringComparison.Ordinal) > -1 || tdRow[2].IndexOf("统筹", StringComparison.Ordinal) > -1)
                            {
                                detailRes.CompanyMedicalAmount += tdRow[4].ToDecimal(0);
                            }
                            break;
                        case "失业保险":
                            detailRes.UnemployAmount += tdRow[4].ToDecimal(0);
                            break;
                        case "工伤保险":
                            detailRes.EmploymentInjuryAmount += tdRow[4].ToDecimal(0);
                            break;
                        case "生育保险":
                            detailRes.MaternityAmount += tdRow[4].ToDecimal(0);
                            break;
                        case "大病医疗":
                            detailRes.IllnessMedicalAmount += tdRow[4].ToDecimal(0);
                            break;
                        case "公务员补助":
                            detailRes.CivilServantMedicalAmount += tdRow[4].ToDecimal(0);
                            break;
                    }
                    detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : tdRow[2].ToDecimal(0);
                    if (!isSave) continue;
                    Res.Details.Add(detailRes);
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
