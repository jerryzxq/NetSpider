using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.ZJ
{
    public class fenghua : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://61.164.72.147:8000/web_acct/";
        string socialCity = "zj_fenghua";
        #endregion
        #region 私有变量

        Hashtable PageHash = new Hashtable();
        /// <summary>
        /// 养老保险,查询年份信息
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(DateTime.Now.AddYears(-2).Year, "employee");
            PageHash.Add(DateTime.Now.AddYears(-3).Year, "employee_before");
        }

        /// <summary>
        /// 获取缴费明细
        /// </summary>
        /// <param name="thisYear">该年请求信息</param>
        /// <param name="totalPayMonths">总缴费月数</param>
        /// <param name="Res"></param>
        private void GetAllDetail(DictionaryEntry thisYear, ref int totalPayMonths, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            List<string> results = new List<string>();
            Url = baseUrl + thisYear.Value;
            httpItem = new HttpItem()
            {
                URL = Url,
                Encoding = Encoding.UTF8,
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@align='center']/table[3]/tr/td", "text", true);
            if (results.Count != 8) return;
            decimal salaryBase = results[2].ToTrim("当年缴费基数总和：").ToDecimal(0) / results[0].ToTrim("当年缴费月数：").ToInt(0);//缴费基数
            if ((string)thisYear.Value == "employee")
            {
                Res.PaymentMonths = results[4].ToTrim("累计缴费月数：").ToInt(0) + totalPayMonths;//总缴费月数
            }
            results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@align='center']/table[4]/tr[1]/td[position()>1]", "text", true);//支付月份
            //支付月份一次缴费月数
            List<string> payMonths = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@align='center']/table[4]/tr[2]/td[position()>1]", "", true);
            int year = (int)thisYear.Key;
            for (int j = 0; j < results.Count; j++)
            {
                if (j > 0)
                {
                    if (results[j].ToInt(0) < results[j - 1].ToInt(0))
                    {
                        year = year + 1;//年份+1
                    }
                }
                DateTime dtPayTime = DateTime.ParseExact(year + results[j], "yyyyM", null);//支付时间
                for (int i = 0; i < payMonths[j].ToInt(0); i++)
                {
                    SocialSecurityDetailQueryRes detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;
                    detailRes.PayTime = dtPayTime.ToString(Consts.DateFormatString7);
                    detailRes.SocialInsuranceTime = dtPayTime.AddMonths(i).ToString(Consts.DateFormatString7);
                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    detailRes.SocialInsuranceBase = salaryBase;
                    Res.Details.Add(detailRes);
                }
            }
        }
        #endregion
        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Res.StatusDescription = socialCity + "无需初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;
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
                //校验参数
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                Url = baseUrl + "login";
                postdata = String.Format("uname={0}&pwd={1}", socialReq.Username, socialReq.Password);
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
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@style='color:red']", "text", true);
                if (results.Count > 0)
                {
                    if (!string.IsNullOrEmpty(results[0]))
                    {
                        Res.StatusDescription = results[0];
                        Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                        return Res;
                    }
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion
                #region 第二步， 获取基本信息

                //个人基本信息
                Url = baseUrl + "indi_acct";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                Res.Name = socialReq.Name;
                Res.IdentityCard = socialReq.Identitycard;
                List<string> resultsBase = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@class='td_text']", "");
                if (resultsBase.Count != 7)
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                //养老保险界面
                results = HtmlParser.GetResultFromParser(resultsBase[0], "/table/tr[2]/td/table/tr/td", "text", true);
                if (results.Count == 20)
                {
                    Res.IdentityCard = results[1];
                    Res.Name = results[3];
                    Res.Sex = results[5];
                    Res.EmployeeStatus = results[11];
                    Res.SocialInsuranceBase = results[13].ToDecimal(0);
                    Res.CompanyName = results[19];
                }
                //医疗保险界面
                results = HtmlParser.GetResultFromParser(resultsBase[2], "/table/tr[2]/td/table/tr/td", "text", true);
                if (results.Count == 24)
                {
                    Res.EmployeeNo = results[7];//医保卡号
                }
                //失业保险界面
                results = HtmlParser.GetResultFromParser(resultsBase[3], "/table/tr[2]/td/table/tr/td", "text", true);
                if (results.Count == 16)
                {
                    if (Res.CompanyName == results[1])
                    {
                        Res.WorkDate = results[5];//参保日期
                    }
                }
                #endregion
                #region 第三步，查询养老明细

                int totalPayMonths = 0;//总缴费月数
                //当前年度养老保险
                results = HtmlParser.GetResultFromParser(resultsBase[1], "/table/tr[2]/td/table/tr[position()>1]", "", true);
                foreach (string s in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(s, "//td", "text");
                    if (tdRow.Count < 6) continue;
                    int payMonths = tdRow[5].ToInt(0);
                    totalPayMonths += payMonths;
                    DateTime dt = DateTime.ParseExact(tdRow[0], "yyyyMM", null);
                    string insuranceTime;
                    for (int i = 0; i < payMonths; i++)
                    {
                        insuranceTime = dt.AddMonths(i).ToString(Consts.DateFormatString7);
                        detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == insuranceTime);
                        bool needSave = false;
                        if (detailRes == null)
                        {
                            needSave = true;
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.Name = Res.Name;
                            detailRes.IdentityCard = Res.IdentityCard;
                            detailRes.PayTime = tdRow[4];
                            detailRes.SocialInsuranceTime = insuranceTime;
                            detailRes.SocialInsuranceBase = tdRow[1].ToDecimal(0);
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        }
                        detailRes.PensionAmount += tdRow[3].ToDecimal(0) / payMonths;
                        detailRes.CompanyPensionAmount += (tdRow[2].ToDecimal(0) - tdRow[3].ToDecimal(0)) / payMonths;
                        if (!needSave) continue;
                        Res.Details.Add(detailRes);
                    }
                }
                //历史明细
                InitPageHash();
                foreach (DictionaryEntry page in PageHash)
                {
                    try
                    {
                        GetAllDetail(page, ref totalPayMonths, ref  Res);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
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
