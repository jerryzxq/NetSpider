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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.AH
{
    public class wuhuyihuwang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.ewoho.com/";
        string socialCity = "ah_wuhuyihuwang";
        #endregion
        #region 私有变量

        #endregion
        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "socialInsurance.do";
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
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollectionEQ(cookies, httpResult.CookieCollection);
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
            string idCard = string.Empty;
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
                //校验参数 姓名可以不输入
                if (socialReq.Citizencard.IsEmpty() || socialReq.Identitycard.IsEmpty()|| socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,验证

                //Url = baseUrl + string.Format("validatePersonInfo.do?name={0}&idCard={1}&cardno={2}", socialReq.Name.ToUrlEncode(), socialReq.Identitycard, socialReq.Citizencard);
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                //    return Res;
                //}
                //if (httpResult.Html != "success")
                //{
                //    Res.StatusDescription = "用户信息输入有误";
                //    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                //    return Res;
                //}
                //cookies = CommonFun.GetCookieCollectionEQ(cookies, httpResult.CookieCollection);//合并cookies
                #endregion
                #region 第二步，登录

                Url = baseUrl + "socialInsuranceInfo.do";
                postdata = String.Format("parameter1={0}&parameter2={1}&parameter3={2}", socialReq.Name.ToUrlEncode(), socialReq.Identitycard, socialReq.Citizencard);//姓名可不输入
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='wuxian_div']/p[1]", "text");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollectionEQ(cookies, httpResult.CookieCollection);//合并cookies

                #endregion
                #region 第三步,获取基本信息

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='info_detial']/span", "text", true);
                foreach (string item in results)
                {
                    if (item.Contains("姓名："))
                    {
                        Res.Name = item.Replace("姓名：", "");
                    }
                    else if (item.Contains("个人编号："))
                    {
                        Res.EmployeeNo = item.Replace("个人编号：", "");
                    }
                    //else if (item.Contains("性别："))
                    //{
                    //    Res.Sex = item.Replace("性别：", "");
                    //}
                    else if (item.Contains("账户状态："))
                    {
                        Res.EmployeeStatus = item.Replace("账户状态：", "");
                    }
                    else if (item.Contains("单位名称："))
                    {
                        Res.CompanyName = item.Replace("单位名称：", "");
                    }
                    else if (item.Contains("201602最新到帐："))//201602最新到帐 累计缴纳：
                    {
                        Res.DeadlineYearAndMonth = item.Substring(0, 6);
                    }
                    else if (item.Contains("累计缴纳："))//累计缴纳：170月
                    {
                        Res.DeadlineYearAndMonth = item.Replace("累计缴纳：", "").Replace("月", "");
                    }
                }
                Res.IdentityCard = socialReq.Identitycard;

                #endregion
                #region 第四步,获取详细信息

                DateTime beginDt = DateTime.Now.AddMonths(-1);
                DateTime endDt = Convert.ToDateTime(beginDt.AddYears(-4).ToString("yyyy-01"));
                Url = baseUrl + "getsocialInsuranceDetail.do";
                do
                {
                    postdata = string.Format("in_year={2}&in_month={3}&cardno={0}&idCard={1}", socialReq.Citizencard.ToUpper(), socialReq.Identitycard, beginDt.Year, beginDt.Month.ToString("d2"));
                    httpItem = new HttpItem()
                    {
                        Accept = "text/plain, */*; q=0.01",
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Host = "www.ewoho.com",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpItem.Header["X-Requested-With"] = "XMLHttpRequest";
                    httpResult = httpHelper.GetHtml(httpItem);
                    try
                    {
                        List<WuhuyihuwangDetail> detailEntity = jsonParser.DeserializeObject<List<WuhuyihuwangDetail>>(httpResult.Html);
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.SocialInsuranceTime = beginDt.ToString(Consts.DateFormatString7);
                        detailRes.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detailRes.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        var yanglao = detailEntity.FindAll(o => o.INSURANCETYPE == "养老保险");
                        foreach (var item in yanglao)
                        {
                            switch (item.ITEMNAME)
                            {
                                case "个人缴纳":
                                    detailRes.PensionAmount = item.MONEY.ToDecimal(0);
                                    detailRes.SocialInsuranceBase = item.BASE_PENSION.ToDecimal(0);
                                    break;
                                case "单位缴纳":
                                    detailRes.CompanyPensionAmount = item.MONEY.ToDecimal(0);
                                    break;
                            }
                        }
                        var yiliao = detailEntity.FindAll(o => o.INSURANCETYPE == "医疗保险");
                        foreach (var item in yiliao)
                        {
                            switch (item.ITEMNAME)
                            {
                                case "个人缴纳":
                                    detailRes.MedicalAmount = item.MONEY.ToDecimal(0);
                                    break;
                                case "单位缴纳":
                                    detailRes.CompanyMedicalAmount = item.MONEY.ToDecimal(0);
                                    break;
                            }
                        }
                        var shiye = detailEntity.FindAll(o => o.INSURANCETYPE == "失业保险");
                        foreach (var item in shiye)
                        {
                            detailRes.UnemployAmount += item.MONEY.ToDecimal(0);
                        }
                        var gongshang = detailEntity.FindAll(o => o.INSURANCETYPE == "工伤保险");
                        foreach (var item in gongshang)
                        {
                            detailRes.EmploymentInjuryAmount += item.MONEY.ToDecimal(0);
                        }
                        var shengyu = detailEntity.FindAll(o => o.INSURANCETYPE == "生育保险");
                        foreach (var item in shengyu)
                        {
                            detailRes.MaternityAmount += item.MONEY.ToDecimal(0);
                        }
                        Res.Details.Add(detailRes);
                    }
                    catch (Exception)
                    {
                        throw new Exception("获取该期" + beginDt.ToString(Consts.DateFormatString7) + "信息失败");
                    }
                    beginDt = beginDt.AddMonths(-1);
                } while (beginDt >= endDt);
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

        internal class WuhuyihuwangDetail
        {
            /// <summary>
            /// 医疗保险缴费基数
            /// </summary>
            public string BASE_MEDICAL { get; set; }
            /// <summary>
            /// 生育保险缴费基数
            /// </summary>
            public string BASE_BIRTH { get; set; }
            /// <summary>
            /// 本期到账
            /// </summary>
            public string MONEY { get; set; }
            /// <summary>
            /// 失业保险缴费基数
            /// </summary>
            public string BASE_UNEMPLOYED { get; set; }
            /// <summary>
            /// 单位缴纳/个人缴纳
            /// </summary>
            public string ITEMNAME { get; set; }
            /// <summary>
            /// 保险类别
            /// </summary>
            public string INSURANCETYPE { get; set; }
            /// <summary>
            /// 工伤保险缴费基数
            /// </summary>
            public string BASE_WORK_INJURY { get; set; }
            /// <summary>
            /// 缴费基数
            /// </summary>
            public string BASE_PENSION { get; set; }
        }
    }

}