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
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.GD
{
    public class shenzhen : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        //IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        //IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "";
        string socialCity = "gd_shenzhen";
        #endregion
        #region 私有变量
        enum InfoType
        {
            养老保险199208以后,
            医疗保险,
            失业保险,
            工伤保险,

        }
        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息


        /// <summary>
        ///  将各种保险对应的页面所需信息存入PageHash
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险199208以后, 1);
            PageHash.Add(InfoType.医疗保险, 2);
            PageHash.Add(InfoType.失业保险, 7);
            PageHash.Add(InfoType.工伤保险, 4);
        }
        private void GetAllDetail(InfoType type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            List<string> results = new List<string>();
            Url = string.Format("https://e.szsi.gov.cn/siservice/transUrl.jsp?url=serviceListAction.do?id={0}", PageHash[type]);
            httpItem = new HttpItem()
            {
                URL = Url,
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            string pid = CommonFun.GetMidStr(httpResult.Html, "pid=", "\"");
            Url = string.Format("https://e.szsi.gov.cn/siservice/serviceListAction.do?id={0}&pid={1}", PageHash[type], pid);
            httpItem = new HttpItem()
            {
                URL = Url,
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);

            Hashtable companyMsg = new Hashtable();
            //单位信息
            results = HtmlParser.GetResultFromParser(httpResult.Html, "//p", "");
            if (results.Count > 0)
            {
                string[] companyArr = (results[0].Remove(0, results[0].IndexOf("<br>") + 4).Replace("<br>", "$").Replace("&nbsp;&nbsp;&nbsp;&nbsp;", "#")).Split('$');
                foreach (string item in companyArr)
                {
                    string[] dic = item.Split('#');
                    if (dic.Length==2)
                    {
                        companyMsg.Add(dic[0].Trim(),dic[1]);
                    }
                }
            }
            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='two']/tbody/tr[@id='TR0']");
            foreach (string item in results)
            {
                SocialSecurityDetailQueryRes detailRes = null;
                var tdRow = HtmlParser.GetResultFromParser(item, "//td", "");
                int tdRowCount = 0;
                switch (type)
                {
                    case InfoType.养老保险199208以后:
                    case InfoType.医疗保险:
                        tdRowCount = 8;
                        break;
                    case InfoType.失业保险:
                        tdRowCount = 6;
                        break;
                    case InfoType.工伤保险:
                        tdRowCount = 5;
                        break;
                }
                if (tdRow.Count < tdRowCount) continue;
                string InsuranceTime = tdRow[1].ToTrim().ToTrim("-");
                detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == InsuranceTime);
                bool isSave = false;
                if (detailRes == null)
                {
                    isSave = true;
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    if (companyMsg.ContainsKey(tdRow[0].Trim()))
                    {
                        detailRes.CompanyName = companyMsg[tdRow[0].Trim()].ToString();
                    }
                    detailRes.PayTime = InsuranceTime;
                    detailRes.SocialInsuranceTime = InsuranceTime;
                    string beizhu = string.Empty;
                    switch (type)
                    {
                        case InfoType.医疗保险:
                            beizhu = "住院医保";
                            break;
                        default:
                            beizhu = "已到帐";
                            break;
                    }
                    detailRes.PaymentFlag = tdRow[tdRowCount - 1] == beizhu ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                    detailRes.PaymentType = tdRow[tdRowCount - 1] == beizhu ? ServiceConsts.SocialSecurity_PaymentType_Normal : ServiceConsts.SocialSecurity_PaymentType_Adjust;
                }
                switch (type)
                {
                    case InfoType.养老保险199208以后:
                        detailRes.CompanyPensionAmount += tdRow[4].ToDecimal(0);
                        detailRes.PensionAmount += tdRow[3].ToDecimal(0);
                        detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                        break;
                    case InfoType.医疗保险:
                        detailRes.CompanyMedicalAmount += tdRow[4].ToDecimal(0);
                        detailRes.MedicalAmount += tdRow[3].ToDecimal(0);
                        break;
                    case InfoType.失业保险:
                        detailRes.UnemployAmount += tdRow[3].ToDecimal(0) + tdRow[4].ToDecimal(0);
                        break;
                    case InfoType.工伤保险:
                        detailRes.EmploymentInjuryAmount += tdRow[3].ToDecimal(0);
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
                Url = "https://e.szsi.gov.cn/siservice/";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Host = "e.szsi.gov.cn",
                    SecurityProtocolType = SecurityProtocolType.Tls,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = httpResult.CookieCollection;
                string pid = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='pid']", "value")[0];

                Url = "https://e.szsi.gov.cn/siservice/PImages?pid=" + pid;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    SecurityProtocolType = SecurityProtocolType.Ssl3,
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
                //FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                //Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);

                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                //添加缓存
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("pid", pid);
                SpiderCacheHelper.SetCache(token, dics);
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
            string pid = string.Empty;
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)SpiderCacheHelper.GetCache(socialReq.Token);
                    pid = dics["pid"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    SpiderCacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                string account = string.Empty;
                bool flag = false;
                if (socialReq.LoginType == "1")
                {
                    account = socialReq.Username;
                    flag = socialReq.Username.IsEmpty();
                }
                else
                {
                    account = socialReq.Identitycard;
                    flag = socialReq.Identitycard.IsEmpty();
                }
                if (flag || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = "https://e.szsi.gov.cn/siservice/LoginAction.do";
                postdata = String.Format("Method=P&pid={3}&type=&AAC002={0}&CAC222={1}&PSINPUT={2}&dlfs={4}", account, socialReq.Password.ToBase64(), socialReq.Vercode, pid, socialReq.LoginType);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    SecurityProtocolType=SecurityProtocolType.Tls,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                if (httpResult.Html.IndexOf("请输入正确的身份证号码") != -1)
                {
                    string errorStr = CommonFun.GetMidStr(httpResult.Html, "<script language='JavaScript'>alert('", "')");

                    Res.StatusDescription = errorStr;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                pid = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='pid']", "value")[0];
                #endregion

                #region 第二步，查询基本信息
                Url = "https://e.szsi.gov.cn/siservice/serviceListAction.do?id=5&pid=" + pid;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                    Res.EmployeeNo = results[1];//个人电脑号=个人编号
                    Res.IdentityCard = results[2];//身份证
                    Res.Sex = results[3];//性别
                    Res.BirthDate = results[4].ToTrim();//出生日期
                    //Res = results[6];//社保号
                    //Res.EmployeeStatus = results[10];//状态
                    Res.CompanyName = results[12];//公司名
                    Res.SocialInsuranceBase = results[13].Replace("元", "").ToDecimal(0);//缴费基数
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Url = "https://e.szsi.gov.cn/siservice/transUrl.jsp?url=serviceListAction.do?id=6";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                pid = CommonFun.GetMidStr(httpResult.Html, "pid=", "\"");

                Url = "https://e.szsi.gov.cn/siservice/serviceListAction.do?id=6&pid=" + pid;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input", "value");
                Res.PersonalInsuranceTotal = results[5].ToDecimal(0);
                #endregion

                #region 缴费明细

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

        private static bool RemoteCertificateValidate(
               object sender, X509Certificate cert,
                 X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }
    }
}
