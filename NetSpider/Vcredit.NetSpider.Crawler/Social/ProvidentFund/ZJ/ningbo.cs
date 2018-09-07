using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.ZJ
{
    public class ningbo : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.nbgjj.com/";
        string fundCity = "zj_ningbo";
        #endregion
        #region 私有变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        string Url = string.Empty;
        string postdata = string.Empty;
        decimal payRate = (decimal)0.08;
        List<string> results = new List<string>();
        ProvidentFundDetail detail = null;
        int PaymentMonths = 0;
        #endregion
        public VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://www.nbgjj.com/perlogin.jhtml";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "www.nbgjj.com",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.nbgjj.com/website/trans/ValidateImg";
                httpItem = new HttpItem()
                {
                    Accept = "image/png,image/*;q=0.8,*/*;q=0.5",
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

                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                //添加缓存
                SpiderCacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return Res;
        }

        public ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = fundCity;
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(fundReq.Token);
                    SpiderCacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "身份证号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录系统
                Url = "http://www.nbgjj.com/GJJQuery";
                postdata = String.Format("tranCode=142501&task=&accnum=&certinum={0}&pwd={1}&verify={2}", fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = "http://www.nbgjj.com/perlogin.jhtml",
                    Method = "post",
                    Postdata = postdata,
                    Encoding = Encoding.GetEncoding("gbk"),
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
                if (!httpResult.Html.Contains("success"))
                {
                    JObject JsonStr = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                    Res.StatusDescription = JsonStr["msg"].ToString();
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                #endregion

                #region 第二步，查询个人基本信息

                string accnum = cookies["gjjaccnum"].Value;
                Url = "http://www.nbgjj.com/GJJQuery";
                postdata = string.Format("tranCode=142503&task=&accnum={0}", accnum);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    Encoding = Encoding.GetEncoding("gbk"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                JObject jsonBaseInfo = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                Res.Name = jsonBaseInfo["accname"].ToString();
                Res.Status = jsonBaseInfo["freeuse1"].ToString();
                Res.CompanyName = jsonBaseInfo["unitaccname"].ToString();
                Res.OpenTime = ((DateTime)jsonBaseInfo["begdate"]).ToString("yyyy-MM-dd");
                Res.SalaryBase = jsonBaseInfo["basenum"].ToString().ToDecimal(0);
                Res.TotalAmount = jsonBaseInfo["amt"].ToString().ToDecimal(0);
                Res.IdentityCard = jsonBaseInfo["certinum"].ToString();
                Res.PersonalMonthPayAmount = Res.CompanyMonthPayAmount = jsonBaseInfo["monpaysum"].ToString().ToDecimal(0) / 2;
                Res.PersonalMonthPayRate =
                    Res.CompanyMonthPayRate = Math.Round(((jsonBaseInfo["monpaysum"].ToString().ToDecimal(0) / Res.SalaryBase) / 2), 2);
                Res.ProvidentFundNo = accnum;

                #endregion

                #region 第三步，缴费明细(提供近3年记录)

                DateTime dtNow = DateTime.Now;
                payRate = Res.PersonalMonthPayRate > 0 ? Res.PersonalMonthPayRate : payRate;
                Url = "http://www.nbgjj.com/GJJQuery";
                postdata = string.Format("tranCode=142504&task=ftp&accnum={0}&begdate={1}&enddate={2}&indiacctype=1", Res.ProvidentFundNo, dtNow.AddYears(-2).ToString("yyyy-MM-dd"), dtNow.ToString("yyyy-MM-dd"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    Encoding = Encoding.GetEncoding("gbk"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                JArray arrList = (JArray)JsonConvert.DeserializeObject(httpResult.Html);
                for (int i = arrList.Count; i > 0; i--)
                {
                    JToken items = arrList[i - 1];
                    detail = new ProvidentFundDetail();
                    detail.PayTime = (DateTime)items["trandate"];
                    detail.CompanyName = items["unitaccname"].ToString();
                    detail.Description = items["ywtype"].ToString().Trim();
                    if (detail.Description == "汇缴")
                    {
                        detail.ProvidentFundTime = ((DateTime)items["trandate"]).ToString("yyyyMM");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = items["amt"].ToString().ToDecimal(0) / 2; //金额
                        detail.CompanyPayAmount = detail.PersonalPayAmount; //金额
                        detail.ProvidentFundBase = detail.PersonalPayAmount / payRate;//缴费基数
                        PaymentMonths++;
                    }
                    else if (detail.Description.IndexOf("取")>-1)
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                        detail.PersonalPayAmount = items["amt"].ToString().ToDecimal(0);//金额
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = items["amt"].ToString().ToDecimal(0);//金额
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion
                Res.PaymentMonths = PaymentMonths;
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
