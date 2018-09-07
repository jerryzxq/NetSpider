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
    public class shengzhou : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.szldj.gov.cn/wsbs/";
        string socialCity = "zj_shengzhou";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "work/login.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "pages/szyth/wsbs/delegate.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = baseUrl + "common/checkcode.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "work/login.do",
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
            int PaymentMonths = 0;
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

                Url = baseUrl + "work/login.do";
                postdata = String.Format("method=login&domainId=1&loginName={0}&password={1}&checkCode={2}&logintype=1", socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Encoding = Encoding.UTF8,
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@class='left_users_contact']/table/tr[2]/td", "");
                if (results.Count > 0)
                {
                    Res.Name = results[0].ToTrim("名　称:");
                }
                if (string.IsNullOrEmpty(Res.Name))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion
                #region 获取基本信息
                // 个人基本信息查询
                Url = baseUrl + "work/m11/f110006.do?method=viewbaseinfo";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html.ToTrim("&nbsp;"), "//table[@class='main_table']/tr/td[@align='left']", "inner", true);
                if (results.Count <= 0 || string.IsNullOrEmpty(results[0]))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.Name = results[1].Trim();//姓名
                Res.BirthDate = results[2].Trim();//出生日期
                Res.Sex = results[3].Trim();//性别
                Res.IdentityCard = results[0].Trim();//身份证号
                Res.Race = results[6];//民族
                Res.Address = results[11].Trim();//通讯地址
                Res.Phone = results[9].Trim().Replace("&nbsp;", "");//联系电话
                Res.ZipCode = results[12].Trim().Replace("&nbsp;", "");//邮政编码
                // 社会保险参保证明
                Url = baseUrl + "work/m11/f110038.do?method=viewshbxcbzminfo";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html.ToTrim("&nbsp;"), "//table[@class='main_table']/tr[5]/td[2]", "", true);
                if (results.Count > 0)
                {
                    Res.WorkDate = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html.ToTrim("&nbsp;"), "//table[@class='main_table']/tr[6]/td[4]", "", true);
                if (results.Count > 0)
                {
                    Res.EmployeeStatus = results[0];
                }
                #endregion
                #region 查询明细
                int pageIndex = 1;
                int pageCount = 1;
                results = new List<string>();
                List<string> removeList;
                do
                {
                    Url = baseUrl + string.Format("work/m11/f110006.do?method=viewjfinfo&page_psjf={0}&page_active=psjf", pageIndex);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (pageIndex == 1)
                    {
                        pageCount = CommonFun.GetMidStr(httpResult.Html, "共1/", "页").ToInt(0);
                    }
                    removeList = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='main_table']/tbody/tr", "");
                    removeList.RemoveAt(removeList.Count - 1);
                    results.AddRange(removeList);
                    pageIndex++;
                } while (pageIndex <= pageCount);
                string insuranceTime;
                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow.Count < 9) continue;
                    if (string.IsNullOrEmpty(Res.CompanyName))
                    {
                        Res.CompanyName = tdRow[0];
                        List<string> companyNo = HtmlParser.GetResultFromParser(item, "//td/a", "title");
                        if (companyNo.Count > 0)
                        {
                            Res.CompanyNo = companyNo[0].ToTrim("单位编号：");
                        }
                    }
                    insuranceTime = tdRow[1];
                    detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == insuranceTime);
                    bool isSave = false;
                    if (detailRes == null)
                    {
                        isSave = true;
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.IdentityCard = Res.IdentityCard;
                        detailRes.Name = Res.Name;
                        detailRes.CompanyName = tdRow[0];
                        detailRes.SocialInsuranceTime = insuranceTime;
                        detailRes.PayTime = tdRow[8];
                        detailRes.PaymentFlag = tdRow[7] == "已到帐" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                        detailRes.PaymentType = tdRow[7];
                    }
                    switch (tdRow[2])
                    {
                        case "企业基本养老保险":
                            detailRes.PensionAmount += tdRow[6].ToDecimal(0);
                            detailRes.CompanyPensionAmount += tdRow[5].ToDecimal(0);
                            detailRes.SocialInsuranceBase = tdRow[4].ToDecimal(0);
                            break;
                        case "企业基本医疗保险":
                            detailRes.CompanyMedicalAmount += tdRow[5].ToDecimal(0);
                            detailRes.MedicalAmount += tdRow[6].ToDecimal(0);
                            break;
                        case "企业失业保险":
                            detailRes.UnemployAmount += (tdRow[5].ToDecimal(0) + tdRow[6].ToDecimal(0));
                            break;
                        case "企业工伤保险":
                            detailRes.EmploymentInjuryAmount += tdRow[5].ToDecimal(0);
                            break;
                        case "企业生育保险":
                            detailRes.MaternityAmount += tdRow[5].ToDecimal(0);
                            break;
                        default:
                            if (tdRow[2].IndexOf("大病", StringComparison.Ordinal) > -1)
                            {
                                detailRes.IllnessMedicalAmount += tdRow[5].ToDecimal(0);
                            }
                            else if (tdRow[2].IndexOf("公务员", StringComparison.Ordinal) > -1)
                            {
                                detailRes.CivilServantMedicalAmount += tdRow[5].ToDecimal(0);
                            }
                            break;
                    }
                    detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : tdRow[4].ToDecimal(0);
                    if (!isSave) continue;
                    Res.Details.Add(detailRes);
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
