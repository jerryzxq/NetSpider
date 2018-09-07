using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.JX
{
    public class jiujiang : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        #endregion
        #region 私有变量

        string url = string.Empty;
        private string baseUrl = "http://www.jjzfgjj.cn/";
        string fundCity = "jx_jiujiang";
        int PaymentMonths = 0;
        decimal perAccounting;//个人占比
        decimal comAccounting;//公司占比
        decimal totalRate;//总缴费比率
        #endregion
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            Res.Token = CommonFun.GetGuidID();
            try
            {
                url = baseUrl + "ValidateCode.aspx";
                httpItem = new HttpItem()
                {
                    Accept = "image/png,image/*;q=0.8,*/*;q=0.5",
                    URL = url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(Res.Token, httpResult.ResultByte);
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = fundCity + "公积金初始化完成";
                CacheHelper.SetCache(Res.Token, cookies);
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
            Res.Token = fundReq.Token;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            try
            {
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (string.IsNullOrWhiteSpace(fundReq.Identitycard) || string.IsNullOrEmpty(fundReq.Password) || string.IsNullOrEmpty(fundReq.Vercode))
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步,登陆

                //页面初始化
                url = baseUrl + "caxun.aspx";
                httpItem = new HttpItem()
                {
                    URL = url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string VIEWSTATE = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='__VIEWSTATE']", "value")[0];
                string VIEWSTATEGENERATOR = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='__VIEWSTATEGENERATOR']", "value")[0];
                string EVENTVALIDATION = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='__EVENTVALIDATION']", "value")[0];
                //登陆
                postdata = string.Format("__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__EVENTVALIDATION={2}&UserCard={3}&Password={4}&CaptchaText={5}&Button1=", VIEWSTATE.ToUrlEncode(), VIEWSTATEGENERATOR.ToUrlEncode(), EVENTVALIDATION.ToUrlEncode(), fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    return Res;
                }
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "');</script></form>");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region 第二步,公积金基本信息

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='msgERR']/table/tr[1]/td[2]", "");
                if (results.Count>0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='msgERR']/table/tr[2]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='msgERR']/table/tr[3]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='msgERR']/table/tr[4]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='msgERR']/table/tr[5]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].ToDecimal(0);
                    Res.PersonalMonthPayRate = payRate;
                    Res.SalaryBase = Res.PersonalMonthPayAmount/payRate;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='msgERR']/table/tr[6]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='msgERR']/table/tr[7]/td[2]", "");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);
                }
                #endregion

                #region 第三步,公积金缴费明细（功能暂未开放2016年3月23日17:45:12）

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
