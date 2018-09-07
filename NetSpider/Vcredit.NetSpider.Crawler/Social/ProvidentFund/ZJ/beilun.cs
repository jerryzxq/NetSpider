using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.ZJ
{
    public class beilun : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://gjj.bl.gov.cn/";
        string fundCity = "zj_beilun";
        #endregion
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Res.StatusDescription = fundCity + "无需初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;
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
            //ProvidentFundLoanRes ResLoad = new ProvidentFundLoanRes();//贷款
            //ProvidentFundReserveRes ReserveRes = new ProvidentFundReserveRes();//补充公积金
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            try
            {
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步登陆
                Url = baseUrl + "check_grdl.asp";
                postdata = string.Format("usersfz={0}&usergrmm={1}&submit=", fundReq.Identitycard, fundReq.Password);
                httpItem = new HttpItem
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
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');location");
                if (errorMsg != "登录成功!")
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region  第二步 公积金基本信息

                Res.Name = fundReq.Name;
                Res.IdentityCard = fundReq.Identitycard;
                Url = baseUrl + "grcx/gr_IndexTop.asp";
                httpItem = new HttpItem
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[1]/tr[1]/td[2]/font", "");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                Res.ProvidentFundNo = CommonFun.GetMidStr(httpResult.Html, "id=01&gr=", "\"");

                #endregion
                #region 第三步 公积金明细

                payRate = Res.PersonalMonthPayRate > 0 ? Res.PersonalMonthPayRate : payRate;
                Url = baseUrl + "grcx/grcx.asp?id=01&gr=" + Res.ProvidentFundNo;
                httpItem = new HttpItem
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='table_right']", "text");
                for (int i = 0; i <= results.Count - 6; i = i + 6)
                {
                    ProvidentFundDetail detail = new ProvidentFundDetail();
                    if (i==0)
                    {
                        Res.TotalAmount = results[5].ToDecimal(0);
                    }
                    detail.PayTime = DateTime.ParseExact(results[i + 4], "yyyyMMdd", null);
                    detail.ProvidentFundTime = results[i + 4].Substring(0, 6);
                    detail.Description = results[i + 2];
                    if (detail.Description == "汇缴")
                    {
                        detail.PersonalPayAmount = detail.CompanyPayAmount = results[i + 3].ToDecimal(0) / 2;
                        detail.ProvidentFundBase = detail.PersonalPayAmount / payRate;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                    }
                    else if (detail.Description.IndexOf("取") > -1)
                    {
                        detail.PersonalPayAmount = results[i + 3].ToDecimal(0);
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                    }
                    else
                    {
                        detail.PersonalPayAmount = results[i + 3].ToDecimal(0);
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                ProvidentFundDetail dd = Res.ProvidentFundDetailList.OrderByDescending(o => o.PayTime).FirstOrDefault(o => o.PaymentFlag == ServiceConsts.ProvidentFund_PaymentFlag_Normal);
                if (dd != null)
                {
                    Res.LastProvidentFundTime = dd.ProvidentFundTime;
                }
                #endregion
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
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