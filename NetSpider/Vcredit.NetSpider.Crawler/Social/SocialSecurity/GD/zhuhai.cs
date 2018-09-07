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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.GD
{
    public class zhuhai : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "https://www.zhldj.gov.cn/zhrsClient/";
        string socialCity = "gd_zhuhai";
        #endregion
        #region 私有变量

        enum InfoType
        {
            养老保险,
            医疗保险,
            失业保险,
            工伤保险,
            生育保险,
        }
        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息
        /// <summary>
        ///  将各种保险对应的页面所需信息存入PageHash
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, "1");
            PageHash.Add(InfoType.医疗保险, "3");
            PageHash.Add(InfoType.失业保险, "2");
            PageHash.Add(InfoType.工伤保险, "4");
            PageHash.Add(InfoType.生育保险, "5");
        }
        /// <summary>
        /// 获取缴费明细
        /// </summary>
        /// <param name="type">缴费类型</param>
        /// <param name="Res"></param>
        private void GetAllDetail(InfoType type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            string postData = string.Empty;
            List<string> results = new List<string>();
            List<string> titles = new List<string>();
            List<string> contents = new List<string>();
            int currentPage = 1;
            int totalPage = 1;
            Url = baseUrl + "social.do";
            do
            {
                postData = string.Format("doMethod=listFee&insured_kind={0}&curr_page={1}", PageHash[type], currentPage);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postData,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (currentPage == 1)
                {
                    totalPage = CommonFun.GetMidStr(httpResult.Html, "第<b>1/", "</b>页").ToInt(0);
                }
                //标题
                titles.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='yanglao']/fieldset[position()>1]/legend", ""));
                //内容
                contents.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='yanglao']/fieldset[position()>1]/div/ul", ""));
                currentPage++;
            } while (currentPage < totalPage);
            string insuranceTime = string.Empty;//应属时间
            string flag = string.Empty;//标志
            string company = string.Empty;//公司
            for (int i = 0; i < titles.Count; i++)
            {
                results = HtmlParser.GetResultFromParser(titles[i], "//label", "");
                if (results.Count > 0)
                {
                    insuranceTime = results[0];
                }
                results = HtmlParser.GetResultFromParser(contents[i], "//li/span", "");
                if (string.IsNullOrEmpty(insuranceTime) || results.Count < 6) continue;

                flag = HtmlParser.GetResultFromParser(titles[i], "//span", "")[0];
                company = HtmlParser.GetResultFromParser(titles[i], "//a", "")[0].Trim();
                SocialSecurityDetailQueryRes detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == insuranceTime);
                bool isSave = false;
                if (detailRes == null)
                {
                    isSave = true;
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;
                    detailRes.SocialInsuranceTime = insuranceTime;
                    detailRes.CompanyName = company;
                    detailRes.PayTime = results[4];
                    detailRes.PaymentFlag = flag == "已到帐" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : flag;
                    detailRes.PaymentType = results[5] == "正常缴" ? ServiceConsts.SocialSecurity_PaymentType_Normal : results[5];
                }
                switch (type)
                {
                    case InfoType.养老保险:
                        detailRes.PensionAmount += results[1].ToDecimal(0);
                        detailRes.CompanyPensionAmount += results[2].ToDecimal(0);
                        detailRes.SocialInsuranceBase = results[0].ToDecimal(0);
                        break;
                    case InfoType.医疗保险:
                        detailRes.MedicalAmount += results[1].ToDecimal(0);
                        detailRes.CompanyMedicalAmount += results[2].ToDecimal(0);
                        break;
                    case InfoType.失业保险:
                        detailRes.UnemployAmount += results[1].ToDecimal(0)+results[2].ToDecimal(0);;
                        break;
                    case InfoType.工伤保险:
                        detailRes.EmploymentInjuryAmount += results[2].ToDecimal(0);
                        break;
                    case InfoType.生育保险:
                        detailRes.MaternityAmount += results[2].ToDecimal(0);
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
                ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

                Url = baseUrl + "login.jsp";
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

                Url = baseUrl + "imageCode.jsp?user_type=1&isreg=true&ss=0.16026324772299572";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

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
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                Url = String.Format("{0}login.do?password={1}&id_card={2}&verifycode={3}&user_type=1&rand=0.07780734739310735", baseUrl, socialReq.Password, socialReq.Identitycard, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
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
                string errorMsg = string.Empty;
                if (httpResult.Html != "1")
                {
                    switch (httpResult.Html)
                    {
                        case "-1":
                            errorMsg = "数据库连接失败！";
                            break;
                        case "0":
                        case "2":
                            errorMsg = "身份证号码或者登录密码输入不正确！";
                            break;
                        case "3":
                            errorMsg = "验证码不正确！";
                            break;
                        case "4":
                            errorMsg = "调用接口错误！";
                            break;
                        default:
                            errorMsg = "登陆失败！";
                            break;
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
                #region 第二步， 获取基本信息

                Url = baseUrl + "social.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='id_card']", "value");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证号
                }
                else
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='infoview1']/ul/li[2]/input", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='infoview1']/ul/li[4]/input", "value");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];//公司名称
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='infoview1']/ul/li[5]/input", "value");
                if (results.Count > 0)
                {
                    Res.SpecialPaymentType = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='infoview1']/ul/li[6]/input", "value");
                if (results.Count > 0)
                {
                    Res.SocialInsuranceBase = results[0].ToDecimal(0);
                }
                //
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='infoview2']/ul/li[1]/input", "value");
                if (results.Count > 0)
                {
                    Res.PaymentMonths = results[0].ToInt(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='infoview2']/ul/li[2]/input", "value");
                if (results.Count > 0)
                {
                    Res.PersonalInsuranceTotal = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='infoview2']/ul/li[6]/input", "value");
                if (results.Count > 0)
                {
                    Res.EmployeeStatus = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='infoview2']/ul/li[8]/input", "value");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='infoview2']/ul/li[10]/input", "value");
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];
                }
                #endregion
                #region 第三步，查询明细

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
        /// <summary>
        /// 基础链接已关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private bool ValidateServerCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
