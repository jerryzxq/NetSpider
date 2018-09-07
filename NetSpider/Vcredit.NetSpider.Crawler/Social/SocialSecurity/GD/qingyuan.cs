using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.GD
{
    public class qingyuan : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://api.qysi.gov.cn/qysiapi/";
        string socialCity = "gd_qingyuan";

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
        /// <summary>
        ///  将各种保险对应的页面所需信息存入PageHash
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, new string[] { "yanglao" });
            PageHash.Add(InfoType.医疗保险, new string[] { "yiliao" });
            PageHash.Add(InfoType.失业保险, new string[] { "shiye" });
            PageHash.Add(InfoType.工伤保险, new string[] { "gongshang" });
            PageHash.Add(InfoType.生育保险, new string[] { "shengyu" });
        }
        private void GetAllDetail(InfoType type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            List<string> results = new List<string>();
            int pages = 1;
            int currentPage = 1;
            do
            {
                Url = baseUrl + string.Format("{0}_jiaofei.php?page={1}", ((string[])PageHash[type])[0], currentPage);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (pages == 1)
                {
                    pages = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='pages']", "value")[0].ToInt(0);
                }
                results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_text']/tr[position()>1]", "", true));
                currentPage++;
            } while (currentPage <= pages);
            foreach (string item in results)
            {
                SocialSecurityDetailQueryRes detailRes = null;
                var tdRow = HtmlParser.GetResultFromParser(item, "//th", "");
                int tdRowCount = 0;
                switch (type)
                {
                    case InfoType.养老保险:
                    case InfoType.医疗保险:
                        tdRowCount = 9;
                        break;
                    case InfoType.失业保险:
                        tdRowCount = 8;
                        break;
                    case InfoType.工伤保险:
                    case InfoType.生育保险:
                        tdRowCount = 6;
                        break;
                }
                if (tdRow.Count < tdRowCount) continue;
                if (tdRow[1] == "补充医疗") continue;
                detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == tdRow[2]);
                bool isSave = false;
                if (detailRes == null)
                {
                    isSave = true;
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.CompanyName = tdRow[0];
                    detailRes.PayTime = tdRow[2];
                    detailRes.SocialInsuranceTime = tdRow[2];
                    detailRes.PaymentFlag = tdRow[tdRowCount - 1] == "已实缴" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                    detailRes.PaymentType = tdRow[tdRowCount - 1] == "已实缴" ? ServiceConsts.SocialSecurity_PaymentType_Normal : ServiceConsts.SocialSecurity_PaymentType_Adjust;
                }
                switch (type)
                {
                    case InfoType.养老保险:
                        detailRes.CompanyPensionAmount += tdRow[6].ToDecimal(0) + tdRow[7].ToDecimal(0);
                        detailRes.PensionAmount += tdRow[4].ToDecimal(0);
                        detailRes.SocialInsuranceBase = tdRow[3].ToDecimal(0);
                        break;
                    case InfoType.医疗保险:
                        detailRes.CompanyMedicalAmount += tdRow[6].ToDecimal(0) + tdRow[7].ToDecimal(0);
                        detailRes.MedicalAmount += tdRow[4].ToDecimal(0);
                        break;
                    case InfoType.失业保险:
                        detailRes.UnemployAmount += tdRow[4].ToDecimal(0) + tdRow[6].ToDecimal(0);
                        break;
                    case InfoType.工伤保险:
                        detailRes.EmploymentInjuryAmount += tdRow[4].ToDecimal(0);
                        break;
                    case InfoType.生育保险:
                        detailRes.MaternityAmount += tdRow[4].ToDecimal(0);
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
                Url = baseUrl + "index.html";
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
                if (socialReq.Password.IsEmpty() || socialReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                Url = baseUrl + "gerenxinxi.php";
                postdata = String.Format("AAC001={0}", socialReq.Identitycard);
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
                String errorMsg = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "');location.href");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #region 账号选择

                //个人缴费状态
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//li[@class='state']/span[@class='text']", "text");
                if (results.Count > 0)
                {
                    Res.EmployeeStatus = results[0].Trim();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//ul[@class='diqu']/li[@class='text']/a", "href");
                //身份证对应唯一一个人
                if (results.Count == 0)
                {
                    //验证密码
                    string aac001 = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='AAC001']", "value")[0];
                    Url = baseUrl + "checka.php";
                    postdata = String.Format("AAC001={0}&pass={1}", aac001, socialReq.Password);
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
                    errorMsg = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "');");
                }
                else
                {
                    //身份证号对应多个人,根据输入密码是否正确进行账号选择判断 441802199001102042 QY2042
                    foreach (string selectLinks in results)
                    {
                        //选择账号
                        Url = baseUrl + selectLinks;
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
                        string aac001 = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='AAC001']", "value")[0];
                        //个人缴费状态
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//li[@class='state']/span[@class='text']", "text");
                        if (results.Count > 0)
                        {
                            Res.EmployeeStatus = results[0].Trim();
                        }
                        //验证密码
                        Url = baseUrl + "checka.php";
                        postdata = String.Format("AAC001={0}&pass={1}", aac001, socialReq.Password);
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
                        errorMsg = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "');");
                        if (string.IsNullOrEmpty(errorMsg))
                        {
                            break;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion
                #endregion
                #region 第二步 获取基本信息

                Url = baseUrl + "gerenxx.php";
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                results = HtmlParser.GetResultFromParser(httpResult.Html.Trim(), "//ul[@class='qysed_right_ce5_ul']/li", "text", true);
                if (results.Count > 0)
                {
                    Res.IdentityCard = CommonFun.GetMidStr(results[0], "公民身份号码：", "姓名");
                    Res.Name = CommonFun.GetMidStr(results[0], "姓名：", "性别");
                    Res.Sex = CommonFun.GetMidStr(results[0], "性别：", "出生日期");
                    Res.BirthDate = CommonFun.GetMidStr(results[0], "出生日期：", "社会保障卡号");
                    Res.EmployeeNo = CommonFun.GetMidStr(results[0], "社会保障卡号：", "社会保障卡状态");
                    //Res.IdentityCard = CommonFun.GetMidStr(results[0], "医保帐号：", "开户银行");
                    Res.Bank = CommonFun.GetMidStr(results[0], "开户银行：", "移动电话");
                    Res.Phone = CommonFun.GetMidStr(results[0], "移动电话：", "联系地址");
                    Res.Address = CommonFun.GetMidStr(results[0], "联系地址：", "");
                }
                //参保险种信息
                Url = baseUrl + "canbao_xianzhong.php";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_text']/tr[position()>1]", "", true);
                foreach (string s in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(s, "//th", "text");
                    if (tdRow.Count < 6) continue;
                    if (tdRow[0].IndexOf("养老保险") > -1)
                    {
                        Res.PaymentMonths = tdRow[3].ToInt(0);
                    }
                    if (tdRow[0].Trim() != "补充医疗")
                    {
                        Res.SpecialPaymentType += tdRow[0].Trim() + ":" + tdRow[1] + "；";
                    }
                    Res.InsuranceTotal += tdRow[5].ToDecimal(0) + tdRow[6].ToDecimal(0);
                }
                #endregion
                #region 第三步 缴费详细

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
