using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.JS
{
    public class gaoyou : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://58.220.237.168:9981/";
        string socialCity = "js_gaoyou";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "58.220.237.168:9981",
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
                cookies = CommonFun.GetCookieCollectionEQ(cookies, httpResult.CookieCollection);
                Url = baseUrl + "checkimage.aspx";
                httpItem = new HttpItem()
                {
                    Accept = "image/png,image/*;q=0.8,*/*;q=0.5",
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                cookies = CommonFun.GetCookieCollectionEQ(cookies, httpResult.CookieCollection);

                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                SpiderCacheHelper.SetCache(token, cookies);
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
                if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(socialReq.Token);
                    SpiderCacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Citizencard.IsEmpty() || socialReq.Name.IsEmpty() || socialReq.Vercode.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                //登陆
                Url = baseUrl + "FAMgr/LoginHandler.aspx";
                string data = "{\"GRBM\":\"" + socialReq.Citizencard + "\",\"XM\":\"" + socialReq.Name + "\",\"ValidateCode\":\"" + socialReq.Vercode + "\"}";
                postdata = String.Format("method=doLogin&data={0}", data.ToUrlEncode());
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
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    return Res;
                }
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "message\":\"", "\",");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    Res.StatusDescription = errorMsg;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollectionEQ(cookies, httpResult.CookieCollection);

                #endregion
                #region 第二步，个人基本信息

                Url = baseUrl + "FAMgr/LoginHandler.aspx?method=GetPersonInfo";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//ul/li", "text");
                if (results.Count == 3)
                {
                    Res.EmployeeNo = results[0].Replace("社保号:", "");
                    Res.Name = results[1].Replace("姓名:", "");
                    Res.CompanyName = results[2].Replace("所在单位:", "");
                }
                Res.IdentityCard = socialReq.Identitycard;
                cookies = CommonFun.GetCookieCollectionEQ(cookies, httpResult.CookieCollection);
                #endregion
                #region 查询详细信息
                Url = baseUrl + "FAMgr/gysbzzjfxxHandler.aspx?method=Search&data=%7B%22startdate%22:%22%22,%22endate%22:%22%22%7D";

                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                List<gaoyouDetails> details = jsonParser.DeserializeObject<List<gaoyouDetails>>(jsonParser.GetResultFromParser(httpResult.Html, "data")).OrderByDescending(o => o.ND).ToList();
                foreach (var item in details)
                {
                   

                    DateTime saveDt = DateTime.ParseExact(item.ND + item.YS, "yyyyM", null);
                    for (int i = 0; i < saveDt.Month; i++)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.SocialInsuranceBase = item.CHARGE.ToDecimal(0) / item.YS.ToInt(0);
                        detailRes.PensionAmount = detailRes.SocialInsuranceBase * (item.BL.ToInt(0) * 0.01M);
                        detailRes.CompanyPensionAmount = detailRes.SocialInsuranceBase - detailRes.PensionAmount;

                        detailRes.SocialInsuranceTime = saveDt.AddMonths(-i).ToString(Consts.DateFormatString7);
                        Res.Details.Add(detailRes);
                    }
                }
                #endregion
                //Res.PaymentMonths = PaymentMonths;
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
        internal class gaoyouDetails
        {
            /// <summary>
            /// 年份
            /// </summary>
            public string ND { get; set; }
            /// <summary>
            /// 个人编号
            /// </summary>
            public string GRBM { get; set; }
            /// <summary>
            /// 姓名
            /// </summary>
            public string XM { get; set; }
            /// <summary>
            /// 缴费基数*该年缴费总月数
            /// </summary>
            public string CHARGE { get; set; }
            /// <summary>
            /// 该年缴费总月数
            /// </summary>
            public string YS { get; set; }
            /// <summary>
            /// 总缴费月数
            /// </summary>
            public string YS1 { get; set; }
            /// <summary>
            /// 截止年月201510
            /// </summary>
            public string NY { get; set; }
            /// <summary>
            /// 个人缴费比例
            /// </summary>
            public string BL { get; set; }
        }
    }
}