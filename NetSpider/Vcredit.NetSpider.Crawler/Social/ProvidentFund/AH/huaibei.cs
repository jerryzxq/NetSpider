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
using System.Text.RegularExpressions;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.AH
{
    public class huaibei : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.hbzfgjj.cn/";
        string fundCity = "ah_huaibei";
        List<string> results = new List<string>();
        #endregion

        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {

                Url = baseUrl + "utility/createCaptcha/?r=2.0870163873769343";
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
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


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

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            ProvidentFundDetail detail = null;
            int PaymentMonths = 0;
            string errorMsg = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "身份证号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步
                string c = string.Empty;
                string m = string.Empty;
                string account = string.Empty;
                Url = baseUrl+"gjjcx/";
               // cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                postdata = string.Format("account={0}&password={1}&vcode={2}",fundReq.Identitycard,fundReq.Password,fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata=postdata,
                    Encoding = Encoding.GetEncoding("utf-8"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                errorMsg = jsonParser.GetResultFromMultiNode(httpResult.Html, "msg");
                if (!string.IsNullOrEmpty(errorMsg) && errorMsg!="登录成功")
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                c = CommonFun.GetMidStr(httpResult.Html, "?c=", "&m");
                m = CommonFun.GetMidStr(httpResult.Html, "&m=", "&account");
                account = CommonFun.GetMidStr(httpResult.Html, "&account=", "\"");
                #endregion

                #region 第二步  没有公积金明细
               
                Url = baseUrl + string.Format("index.php?c={0}&m={1}&account={2}",c,m,account);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("utf-8"),
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
             
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table [@class='is-xxgkinfo']/tr[2]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table [@class='is-xxgkinfo']/tr[3]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.BankCardNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table [@class='is-xxgkinfo']/tr[4]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                     results = HtmlParser.GetResultFromParser(httpResult.Html, "//table [@class='is-xxgkinfo']/tr[5]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table [@class='is-xxgkinfo']/tr[6]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = decimal.Parse(results[0]);

                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table [@class='is-xxgkinfo']/tr[7]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = decimal.Parse(results[0]);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table [@class='is-xxgkinfo']/tr[11]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.Status = results[0];
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table [@class='is-xxgkinfo']/tr[9]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.TotalAmount = decimal.Parse(results[0]);
                }
                //SalaryBase
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table [@class='is-xxgkinfo']/tr[12]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.SalaryBase = decimal.Parse(results[0]);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table [@class='is-xxgkinfo']/tr[13]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0];
                }
                Res.PaymentMonths = PaymentMonths;
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
