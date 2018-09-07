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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.AH
{
    public class anqing : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://220.179.13.107:7001/webeps/";
        string socialCity = "ah_anqing";
        #endregion
        #region 私有变量

        enum InfoType
        {
            养老保险,
            医疗保险,
            失业保险,
            工伤保险,
            生育保险,
            大额救助,
            公务员补助
        }
        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息
        /// <summary>
        ///  将各种保险对应的页面所需信息存入PageHash
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, new string[] { "110" });
            PageHash.Add(InfoType.医疗保险, new string[] { "310" });
            PageHash.Add(InfoType.失业保险, new string[] { "210" });
            PageHash.Add(InfoType.工伤保险, new string[] { "410" });
            PageHash.Add(InfoType.生育保险, new string[] { "510" });
            PageHash.Add(InfoType.大额救助, new string[] { "330" });
            PageHash.Add(InfoType.公务员补助, new string[] { "320" });
        }
        private void GetAllDetail(InfoType type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            int pageIndex = 1;
            int pageCount = 0;
            Url = baseUrl + "getMvac07.do";
            do
            {
                postdata = string.Format("pageNumber={0}&pageSize=200&mv_ac07.aae140={1}&mv_ac07.aae002=&mv_ac07.aae003=&mv_ac07.aae004=", pageIndex, ((string[])PageHash[type])[0]);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (pageIndex == 1)
                {
                    pageCount = CommonFun.GetMidStr(CommonFun.GetMidStr(httpResult.Html, "当前第", "页"), "/", "").ToInt(0);
                }
                results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", "").Replace("\t", ""), "//table[@class='tableInput']/tr[position()>1]", "inner", true));
                pageIndex++;
            }
            while (pageIndex <= pageCount);

            foreach (string item in results)
            {
                SocialSecurityDetailQueryRes detailRes = null;
                var tdRow = HtmlParser.GetResultFromParser(item, "//td", "");
                if (tdRow.Count < 14) continue;
                detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == tdRow[3] && !string.IsNullOrEmpty(tdRow[3]));
                bool isSave = false;
                if (detailRes == null)
                {
                    isSave = true;
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.PayTime = tdRow[2];
                    detailRes.SocialInsuranceTime = tdRow[3];
                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    detailRes.PaymentFlag = tdRow[12].Trim() != "已到款" ? tdRow[12] : ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                }
                switch (type)
                {
                    case InfoType.养老保险:
                        detailRes.CompanyPensionAmount += tdRow[7].ToDecimal(0);
                        detailRes.PensionAmount += tdRow[9].ToDecimal(0);
                        detailRes.SocialInsuranceBase = tdRow[5].ToDecimal(0);
                        break;
                    case InfoType.医疗保险:
                        detailRes.CompanyMedicalAmount += tdRow[7].ToDecimal(0);
                        detailRes.MedicalAmount += tdRow[9].ToDecimal(0);
                        detailRes.SocialInsuranceBase = tdRow[5].ToDecimal(0);
                        break;
                    case InfoType.失业保险:
                        detailRes.UnemployAmount += tdRow[7].ToDecimal(0);
                        break;
                    case InfoType.工伤保险:
                        detailRes.EmploymentInjuryAmount += tdRow[7].ToDecimal(0);
                        break;
                    case InfoType.生育保险:
                        detailRes.MaternityAmount += tdRow[7].ToDecimal(0);
                        break;
                    case InfoType.大额救助:
                        detailRes.IllnessMedicalAmount += tdRow[7].ToDecimal(0);
                        break;
                    case InfoType.公务员补助:
                        detailRes.CivilServantMedicalAmount += tdRow[7].ToDecimal(0);
                        break;
                }
                if (isSave)
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
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "logon/logon.jsp?inputlb=001";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "code.jsp";
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
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;

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
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                Url = baseUrl + "logonAction.do";
                postdata = String.Format("userlb=null&userid={0}&passwd={1}&checkcode={2}&cmdok=+%B5%C7+%C2%BD+", socialReq.Username, socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "logon/logon.jsp",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (HtmlParser.GetResultFromParser(httpResult.Html, "//body").Count > 0)
                {
                    Url = baseUrl + "logon/logon.jsp";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Referer = baseUrl + "/logonAction.do",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    Res.StatusDescription = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\\n\")"); ;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                //合并cookies
                #endregion

                #region 获取基本信息
                Url = baseUrl + "sbyw/jbbuiness/grmain.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "logonAction.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tableInput']/tr/td", "inner");
                if (results.Count > 0)
                    results.RemoveAt(0);
                if (results.Count <= 0 && results[1].IsEmpty())
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Res.EmployeeNo = results[1].Trim().Replace("&nbsp;", "");//编号
                Res.Name = results[5].Trim().Replace("&nbsp;", "");//姓名
                Res.BirthDate = results[11].Trim().Replace("&nbsp;", "");//出生日期
                Res.Race = results[9].Replace("&nbsp;", "").Replace("\r", "").Replace("\n", "").Replace("\t", "").Trim();//民族
                Res.Sex = results[7].Replace("&nbsp;", "").Replace("\r", "").Replace("\n", "").Replace("\t", "").Trim();//性别
                Res.IdentityCard = results[3].Trim().Replace("&nbsp;", "");//身份证号
                Res.WorkDate = results[13].Trim().Replace("&nbsp;", "");//参加工作时间
                Res.EmployeeStatus = results[15].Replace("&nbsp;", "").Replace("\r", "").Replace("\n", "").Replace("\t", "").Trim();//工作状态
                Res.Phone = results[31].Trim().Replace("&nbsp;", "");//工作状态
                //基数
                Url = baseUrl + "sbyw/getGrcbxx.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "sbyw/getUserInfo.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tableInput']/tr/td", "inner");
                Res.SocialInsuranceBase = results[14].Trim().Replace("&nbsp;", "").ToDecimal(0);
                //账户总额
                Url = baseUrl + "sbyw/aczhTotle.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "sbyw/getAczhGBaaz002.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tableInput']/tr/td", "inner");
                if (results.Count > 10)
                    Res.InsuranceTotal = results[11].Trim().Replace("&nbsp;", "").ToDecimal(0);

                #endregion

                #region 查询明细

                InitPageHash();
                foreach (InfoType type in Enum.GetValues(typeof(InfoType)))
                {
                    try
                    {
                        GetAllDetail(type, ref Res);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
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
