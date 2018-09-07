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
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.FJ
{
    public class quanzhou : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.qzgjj.com/";//网址
        string fundCity = "fj_quanzhou";
        private decimal payRate =(decimal)0.08;
        #endregion

        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "PublicFundSearch.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("utf-8"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                string VIEWSTATE = string.Empty;
                string EVENTVALIDATION = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value", true);
                if (results.Count > 0)
                {
                    VIEWSTATE = results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value", true);
                if (results.Count > 0)
                {
                    EVENTVALIDATION = results[0];
                }

                Url = baseUrl + "SG_Admin/SG_Validator.aspx?num=0.3744708802551031";//验证码地址
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

                //添加缓存
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("VIEWSTATE", VIEWSTATE);
                dics.Add("EVENTVALIDATION", EVENTVALIDATION);
                CacheHelper.SetCache(token, dics);
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
            //decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
          
            try
            {
                string VIEWSTATE = string.Empty;
                string EVENTVALIDATION = string.Empty;
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(fundReq.Token);
                    VIEWSTATE = dics["VIEWSTATE"].ToString();
                    EVENTVALIDATION = dics["EVENTVALIDATION"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || fundReq.Vercode.IsEmpty())
                {
                    Res.StatusDescription = "身份证号或验证码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = baseUrl + "PublicFundSearch.aspx";
                postdata = String.Format("__VIEWSTATE={2}&__EVENTVALIDATION={3}&txtIDCard={0}&txtCheckCode={1}&btnOK=%E4%B8%8B%E4%B8%80%E6%AD%A5", fundReq.Identitycard.ToUrlEncode(), fundReq.Vercode, VIEWSTATE.ToUrlEncode(), EVENTVALIDATION.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Encoding = Encoding.GetEncoding("utf-8"),
                    Referer = Url,
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
                string errorMsg = CommonFun.GetMidStr(httpResult.Html,"defer=true>alert(\"","\");");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    return Res;
                }
                #endregion
                #region 第二步，查询个人基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='txtIDCard']", "innertext");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='txtUnitNumber']", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyLicense = results[0];// 单位账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='txtUnitName']", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];//单位名称
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='txtRealName']", "innertext");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//真实姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='txtPersonalNumber']", "innertext");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];//个人账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='txtMonthPay']", "innertext");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].ToDecimal(0) / 2;//个人月付
                    Res.CompanyMonthPayAmount = results[0].ToDecimal(0) / 2;//公司月付
                    Res.SalaryBase = Res.PersonalMonthPayAmount/payRate;//基本薪资
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='txtBalance']", "innertext");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);//账户余额
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='txtLastUpdateDate']", "innertext");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0];//最后缴交时间
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='txtBeginDate']", "innertext");
                if (results.Count > 0)
                {
                    Res.OpenTime = results[0];//开户时间
                }
                Res.PersonalMonthPayRate = payRate;
                Res.CompanyMonthPayRate = payRate;
                #endregion
                #region  ===公积金明细网站未提供===

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
