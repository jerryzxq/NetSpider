using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using System.Text.RegularExpressions;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HL
{
    /// <summary>
    /// 未提供缴费明细[5566]
    /// </summary>
    public class haerbin : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "https://fund.hrbgjj.gov.cn:8443/fund/";
        string fundCity = "hl_haerbin";
        #endregion
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
                Url = baseUrl + "webSearchInfoAction.do?method=process&dispatch=genetateValidatecode";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return Res;
        }
        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                //15位或18位身份证验证
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (regex.IsMatch(fundReq.Identitycard) == false)
                {
                    Res.StatusDescription = "请输入有效的15位或18位身份证号";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (fundReq.Username.Length != 12)
                {
                    Res.StatusDescription = "公积金不能为空或不足12位";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (fundReq.Password.Length != 6)
                {
                    Res.StatusDescription = "密码不能为空或不足6位";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆

                Url = baseUrl + "webSearchInfoAction.do?method=process";
                postdata = string.Format("dispatch=fund_search&return_message=&id_card={0}&id_account={1}&searchpwd={2}&validcode={3}", fundReq.Identitycard.Trim(), fundReq.Username.Trim(), fundReq.Password, fundReq.Vercode);//[dispatch=fund_search]为脚本赋值
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    Encoding = Encoding.GetEncoding("gb2312"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='return_message']", "value");
                if (results.Count > 0)
                {
                    if (!string.IsNullOrEmpty(results[0].Trim()))
                    {
                        Res.StatusDescription = results[0];
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
               
                if (httpResult.Html.Contains("<%@ page contentType=\"text/html; charset=GBK\" language=\"java\" import=\"java.sql.*\" errorPage=\"\" %>"))
                {
                    Res.StatusDescription = " 如果您此次输入的15位（或18位）身份证号码没有显示您的个人信息，请输入18位（或15位）身份证号码重新查询。若果仍不能登陆，请到公积金中心咨询。 ";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='table36']", "inner");//[id=36有2个]
                if (results.Count != 2)
                {
                    Res.StatusDescription = "网站页面信息变动";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                #region 第二步,获取基本信息

                Regex reg = new Regex(@"[\&nbsp;\%\,\s]*");
                string reResults = results[1];
                results = HtmlParser.GetResultFromParser(reResults, "//tr[2]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.Name = reg.Replace(results[0], "");//姓 名
                }
                results = HtmlParser.GetResultFromParser(reResults, "//tr[2]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyName = reg.Replace(results[0], "");//单位名称
                }
                results = HtmlParser.GetResultFromParser(reResults, "//tr[3]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = reg.Replace(results[0], "");//个人账号
                }
                results = HtmlParser.GetResultFromParser(reResults, "//tr[3]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyNo = reg.Replace(results[0], "");//单位公积金账号
                }
                results = HtmlParser.GetResultFromParser(reResults, "//tr[4]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.IdentityCard = reg.Replace(results[0], "");//身份证号
                }
                results = HtmlParser.GetResultFromParser(reResults, "//tr[4]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.OpenTime = reg.Replace(results[0], "");//开户日期
                }
                results = HtmlParser.GetResultFromParser(reResults, "//tr[5]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.SalaryBase = reg.Replace(results[0], "").ToDecimal(0);//缴存基数
                }
                results = HtmlParser.GetResultFromParser(reResults, "//tr[5]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = reg.Replace(results[0], "").ToDecimal(0) * 0.01M;//个人缴存比例
                    //Res.PersonalMonthPayAmount = Res.SalaryBase * (Res.PersonalMonthPayRate);//个人月缴额
                }
                results = HtmlParser.GetResultFromParser(reResults, "//tr[6]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = reg.Replace(results[0], "").ToDecimal(0) * 0.01M;//单位缴存比例
                    //Res.CompanyMonthPayAmount = Res.SalaryBase * (Res.CompanyMonthPayRate);//单位月缴额
                }
                results = HtmlParser.GetResultFromParser(reResults, "//tr[6]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayAmount =
                        Res.PersonalMonthPayAmount = reg.Replace(results[0], "").ToDecimal(0) / 2;
                }
                results = HtmlParser.GetResultFromParser(reResults, "//tr[7]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = reg.Replace(results[0], "");//最后汇缴年月

                }
                results = HtmlParser.GetResultFromParser(reResults, "//tr[8]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.Status = reg.Replace(results[0], "");//状 态
                }
                results = HtmlParser.GetResultFromParser(reResults, "//tr[9]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.TotalAmount = reg.Replace(results[0], "").ToDecimal(0);//账户余额
                }
                #endregion
                #region === 网站未提供缴费明细查询 ===
                //5566
                #endregion
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
        /// <summary>
        /// 解决：基础连接已经关闭: 未能为SSL/TLS 安全通道建立信任关系
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
