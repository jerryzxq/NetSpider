using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SC
{
    /// <summary>
    /// 网站未提供缴费明细查询[5566],登陆密码为:1' or '1' = '1 
    /// </summary>
    public class yibin : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://cx.ybxww.cn:5566/";
        string fundCity = "sc_yibin";
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                vcRes.StatusDescription = "所选城市无需初始化";
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                vcRes.StatusCode = ServiceConsts.StatusCode_error;
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return vcRes;
        }

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = fundCity;
            string B1 = string.Empty;//公积金查询
            decimal payRate = (decimal)0.08;
            string url = string.Empty;
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
                if (string.IsNullOrWhiteSpace(fundReq.Password))
                {
                    Res.StatusDescription ="密码长度至少6位";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if ((string.IsNullOrWhiteSpace(fundReq.Username) && string.IsNullOrWhiteSpace(fundReq.Identitycard)))
                {
                    Res.StatusDescription = "请输入正确的身份证号或公积金号";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆
                url = baseUrl + "search.asp";
                httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='B1']", "value", true);
                if (results.Count > 0)
                {
                    B1 = results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                url = baseUrl + "search.asp?s=login";
                postdata = string.Format("sfz={0}&gjj={1}&pass={2}&B1={3}", fundReq.Identitycard, fundReq.Username, fundReq.Password.ToUrlEncode(Encoding.GetEncoding("gb2312")), B1.ToUrlEncode(Encoding.GetEncoding("gb2312")).Replace("%20", "+"));
                httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "Post",
                    Postdata = postdata,
                    Encoding=Encoding.GetEncoding("gb2312"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode!=HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryError;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "');</script>");
                if (!string.IsNullOrWhiteSpace(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                #region 第二步,获取基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[2]", "text");
                if (results.Count > 0)
                {
                    string temp = results[0].Replace("\r\n","#").Trim();
                    string[] arryList = temp.Split('#');
                    if (arryList.Length==11)
                    {
                        Res.Name = arryList[2];//姓名
                        Res.CompanyMonthPayAmount = arryList[3].ToTrim("元").ToDecimal(0);//公司月缴额
                        Res.PersonalMonthPayAmount = arryList[4].ToTrim("元").ToDecimal(0);//个人月缴额
                        Res.TotalAmount = arryList[6].ToTrim("元").ToDecimal(0);//账户总额
                        Res.LastProvidentFundTime = arryList[8];//最后缴费时间
                        Res.Status = arryList[9];//状态
                        Res.SalaryBase = (Res.PersonalMonthPayAmount / payRate).ToString("f2").ToDecimal(0);//基本薪资
                        if (Res.SalaryBase>0)
                        {
                            Res.CompanyMonthPayRate = (Res.CompanyMonthPayAmount / Res.SalaryBase).ToString("f2").ToDecimal(0);//公司缴费比率
                            Res.PersonalMonthPayRate = (Res.PersonalMonthPayAmount / Res.SalaryBase).ToString("f2").ToDecimal(0);//个人缴费比率
                        }
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='sfz']", "value");
                        if (results.Count>0)
                        {
                            Res.IdentityCard =results[0];//身份证
                        }
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='gjj']", "value");
                        if (results.Count > 0)
                        {
                            Res.ProvidentFundNo = results[0];//公积金账号
                        }
                    }
                }
                #endregion
                #region ===网站未提供缴费明查询===
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
    }
}
