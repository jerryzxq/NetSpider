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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.FJ
{
    public class xiamen : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://222.76.242.141:8888/";
        string fundCity = "fj_xiamen";
        #endregion
        #region 私有变量
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
                Url = "http://222.76.242.141:8888/codeImage.shtml";
                httpItem = new HttpItem()
                {
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
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
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
                Url = "http://222.76.242.141:8888/login.shtml";
                postdata = string.Format("username={0}&password={1}&securityCode2={2}&securityCode={2}", fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='err_area']", "text", true);
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                #region 第二步，查询个人基本信息

                Url = "http://222.76.242.141:8888/queryZgzh.shtml";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.Html.Trim() == "{}")
                {
                    Res.StatusDescription = "没有查询到您的公积金缴存信息，谢谢！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string name = jsonParser.GetResultFromMultiNode(httpResult.Html, "acVwPerson:custName");
                if (name.IsEmpty())
                {
                    Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //姓名
                Res.Name = fundReq.Name;
                string[] namearr = name.Split('*');
                foreach (string s in namearr)
                {
                    if (string.IsNullOrEmpty(s)) continue;
                    if (fundReq.Name.IndexOf(s) == -1)
                    {
                        Res.Name = name;
                        break;
                    }
                }
                Res.Bank = jsonParser.GetResultFromMultiNode(httpResult.Html, "acVwPerson:aaa103");
                Res.ProvidentFundNo = jsonParser.GetResultFromMultiNode(httpResult.Html, "acVwPerson:custAcct");
                Res.Status = jsonParser.GetResultFromMultiNode(httpResult.Html, "acVwPerson:acctStatus") == "0" ? "正常" : "非正常";
                Res.OpenTime = jsonParser.GetResultFromMultiNode(httpResult.Html, "acVwPerson:openDate");
                Res.IdentityCard = jsonParser.GetResultFromMultiNode(httpResult.Html, "acVwPerson:idNo");
                Res.TotalAmount = jsonParser.GetResultFromMultiNode(httpResult.Html, "acVwPerson:bal").ToDecimal(0);
                Res.CompanyName = jsonParser.GetResultFromMultiNode(httpResult.Html, "acVwPerson:compName");
                //size:1,单账号   2：多账号
                string size = jsonParser.GetResultFromMultiNode(httpResult.Html, "size");
                string custAcct = jsonParser.GetResultFromMultiNode(httpResult.Html, "acVwPerson:custAcct");
                //个人中心
                Url = "http://222.76.242.141:8888/grzx.php";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='oldPhone']", "value");
                if (results.Count > 0)
                {
                    Res.Phone = results[0];
                }
                //贷款能力计算
                if (size == "1")
                {
                    Url = "http://222.76.242.141:8888/sfsyCalculator.shtml";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    JObject jobject = (JObject)JsonConvert.DeserializeObject(jsonParser.GetResultFromMultiNode(httpResult.Html, "Person"));
                    Res.LastProvidentFundTime = DateTime.ParseExact(jobject["lastUseDate"].ToString(), "yyyyMMdd", null).ToString(Consts.DateFormatString7);
                    Res.SalaryBase = jobject["lastYearAvgAmt"].ToString().ToDecimal(0);
                    Res.OpenTime = jobject["openDate"].ToString();
                    Res.CompanyMonthPayAmount = jobject["compPayAmt"].ToString().ToDecimal(0);
                    Res.Name = jobject["custName"].ToString();
                    Res.CompanyNo = jobject["compCode"].ToString();
                    Res.PersonalMonthPayAmount = jobject["custPayAmt"].ToString().ToDecimal(0);
                    Res.PersonalMonthPayRate = (Res.SalaryBase > 0 && Res.PersonalMonthPayAmount > 0) ? Res.PersonalMonthPayAmount / Res.SalaryBase : payRate;
                    Res.CompanyMonthPayRate = (Res.SalaryBase > 0 && Res.CompanyMonthPayAmount > 0) ? Res.CompanyMonthPayAmount / Res.SalaryBase : payRate;
                }
                decimal percent = decimal.Zero;
                if (Res.PersonalMonthPayRate > 0 & Res.CompanyMonthPayRate > 0)
                {
                    percent = Res.PersonalMonthPayRate / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate);
                    payRate = Res.PersonalMonthPayRate;
                }
                else
                {
                    percent = 0.5M;
                }
                #endregion
                string startTime = "20000101";
                string endTime = DateTime.Now.ToString(Consts.DateFormatString5);
                #region 第三步，缴费明细
                Url = "http://222.76.242.141:8888/queryGrzhxxJson.shtml";
                postdata = string.Format("custAcct={0}&startDate={1}&endDate={2}", custAcct, startTime, endTime);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                List<XiamenDetail> details = jsonParser.DeserializeObject<List<XiamenDetail>>(jsonParser.GetResultFromParser(httpResult.Html, "list"));
                foreach (var item in details)
                {
                    string fundTime = item.centDealDate.Substring(0, 6);
                    ProvidentFundDetail detail = Res.ProvidentFundDetailList.FirstOrDefault(o => o.ProvidentFundTime == fundTime & o.PaymentType == item.centSumy);
                    bool needSave = false;
                    if (detail == null)
                    {
                        needSave = true;
                        detail = new ProvidentFundDetail();
                        detail.PayTime = item.centDealDate.ToDateTime(Consts.DateFormatString5);
                        detail.ProvidentFundTime = fundTime;
                        detail.Description = item.centSumy;
                        switch (item.centSumy)
                        {
                            case "汇缴":
                            case "缴交公积金":
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                                detail.PaymentType = item.centSumy;
                                detail.PersonalPayAmount = item.creditAmt.ToDecimal(0) * percent; //金额
                                detail.CompanyPayAmount = item.creditAmt.ToDecimal(0)*(1-percent); //金额
                                detail.ProvidentFundBase = (detail.PersonalPayAmount / payRate); //缴费基数
                                break;
                            case "账户结息":
                                detail.PersonalPayAmount = item.creditAmt.ToDecimal(0);
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                                break;
                            default:
                                if (item.centSumy.IndexOf("支取", StringComparison.Ordinal) > -1)
                                {
                                    detail.PersonalPayAmount = item.creditAmt.ToDecimal(0);
                                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                                }
                                else
                                {
                                    detail.PersonalPayAmount = item.debitAmt.ToDecimal(0);
                                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (item.centSumy)
                        {
                            case "汇缴":
                            case "缴交公积金":
                                detail.PersonalPayAmount += (item.creditAmt.ToDecimal(0) / 2);//金额
                                detail.CompanyPayAmount += detail.PersonalPayAmount;//金额
                                break;
                        }
                        //描述
                        detail.Description = detail.Description.IndexOf(item.centSumy) > -1 ? detail.Description : detail.Description + ";" + item.centSumy;
                    }
                    if (needSave)
                    {
                        Res.ProvidentFundDetailList.Add(detail);
                    }
                }
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
    internal class XiamenDetail
    {
        /// <summary>
        /// 状态
        /// </summary>
        public string centSumy { get; set; }
        /// <summary>
        /// 交易日期
        /// </summary>
        public string centDealDate { get; set; }
        /// <summary>
        ///余额
        /// </summary>
        public string bal { get; set; }
        /// <summary>
        /// 缴费金额
        /// </summary>
        public string creditAmt { get; set; }
        /// <summary>
        /// 其他情况发生金额
        /// </summary>
        public string debitAmt { get; set; }
    }
}
