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
    public class haining : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://60.190.143.123:8080/synet/";
        string socialCity = "zj_haining";
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

                #region 登录

                Url = baseUrl + "login_check.jsp";
                postdata = String.Format("ccc001={0}&Submit=%D3%C3%BB%A7%B5%C7%C2%BC&ccc004={1}", socialReq.Identitycard, socialReq.Password);
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
                string msg = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\"");
                if (msg != "登录成功！")
                {
                    Res.StatusDescription = msg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取基本信息

                Url = baseUrl + "person/person_message.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "login_check.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@bgcolor='#9db5d4']/tr/td");
                if (results.Count != 20)
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                Res.Name = results[1].Trim().Replace("&nbsp", "");//姓名
                Res.IdentityCard = results[3].Trim().Replace("&nbsp", "");//身份证号
                Res.Sex = results[5].Trim().Replace("&nbsp", "");//性别
                Res.Race = results[7].Trim().Replace("&nbsp", "");//民族
                Res.BirthDate = results[9].Trim().Replace("&nbsp", "");//出生日期
                Res.WorkDate = results[11].Trim().Replace("&nbsp", "");//参加工作时间
                Res.CompanyName = results[15].Trim().Replace("&nbsp", "");//单位名称
                Res.EmployeeStatus = results[17].Trim().Replace("&nbsp", "");//人员状态
                Res.IsSpecialWork = !results[19].Contains("非特殊工种");

                #endregion

                #region 查询明细

                int pageIndex = 1;
                int pageCount = 0;

                do
                {
                    Url = String.Format("{0}person/person_zhengjiao.jsp?names=&newsPageNo={1}", baseUrl, pageIndex);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Referer = baseUrl + "person/person_message.jsp",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if(pageCount == 0)
                    {
                        pageCount = CommonFun.GetMidStr(httpResult.Html, "页 共", "页").ToInt(0);
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@style='border: 1px #C5C5C5 solid;']/tr");
                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "td");
                        if (tdRow[0] == "险种类别" || tdRow.Count != 8)
                        {
                            continue;
                        }
                        string SocialInsuranceTime = tdRow[1];
                        detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                        bool NeedAdd = false;
                        bool NeedChangeFlag = false;//根据险种优先级判断是否需要更改标示
                        if (detailRes == null)
                        {
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.PayTime = tdRow[3].Replace("-", "");
                            detailRes.SocialInsuranceTime = tdRow[1];
                            detailRes.SocialInsuranceBase = tdRow[4].ToDecimal(0);
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            NeedAdd = true;
                        }
                        if (tdRow[0] == "企业养老")
                        {
                            detailRes.PensionAmount = tdRow[5].ToDecimal(0);
                            detailRes.CompanyPensionAmount = tdRow[6].ToDecimal(0);
                            NeedChangeFlag = true;
                        }
                        else if (tdRow[0] == "职工基本医疗")
                        {
                            detailRes.MedicalAmount = tdRow[5].ToDecimal(0);
                            detailRes.CompanyMedicalAmount = tdRow[6].ToDecimal(0);
                            if (detailRes.PensionAmount == 0 && detailRes.CompanyPensionAmount == 0)//是否已记录养老数据
                            {
                                NeedChangeFlag = true;
                            }
                        }
                        else if (tdRow[0] == "失业")
                        {
                            detailRes.UnemployAmount = tdRow[5].ToDecimal(0) + tdRow[6].ToDecimal(0);
                            if (detailRes.PensionAmount == 0 && detailRes.CompanyPensionAmount == 0 && detailRes.MedicalAmount == 0 && detailRes.CompanyMedicalAmount == 0)//是否已记录养老、医疗数据
                            {
                                NeedChangeFlag = true;
                            }
                        }
                        else if (tdRow[0] == "工伤")
                        {
                            detailRes.IllnessMedicalAmount = tdRow[5].ToDecimal(0) + tdRow[6].ToDecimal(0);
                            if (detailRes.PensionAmount == 0 && detailRes.CompanyPensionAmount == 0 && detailRes.MedicalAmount == 0 && detailRes.CompanyMedicalAmount == 0 && detailRes.UnemployAmount == 0)//是否已记录养老、医疗、失业数据
                            {
                                NeedChangeFlag = true;
                            }
                        }
                        else if (tdRow[0] == "生育")
                        {
                            detailRes.MaternityAmount = tdRow[5].ToDecimal(0) + tdRow[6].ToDecimal(0);
                            if (detailRes.PensionAmount == 0 && detailRes.CompanyPensionAmount == 0 && detailRes.MedicalAmount == 0 && detailRes.CompanyMedicalAmount == 0 && detailRes.UnemployAmount == 0 && detailRes.EmploymentInjuryAmount == 0)//是否已记录养老、医疗、失业、工伤数据
                            {
                                NeedChangeFlag = true;
                            }
                        }
                        else
                        {
                            continue;
                        }
                        if (NeedChangeFlag)
                        {
                            detailRes.PaymentFlag = tdRow[2] == "到帐" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : tdRow[2];
                        }
                        if (NeedAdd)
                        {
                            Res.Details.Add(detailRes);
                        }
                    }
                    pageIndex++;
                }
                while (pageIndex <= pageCount);

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
