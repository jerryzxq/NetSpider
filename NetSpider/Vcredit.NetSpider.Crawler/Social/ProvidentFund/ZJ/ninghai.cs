using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.ZJ
{
    public class ninghai : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://nhgjj.ninghai.gov.cn/";
        string fundCity = "zj_ninghai";
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
                Url = baseUrl + "x_grdatacx.asp?cx_mak=A&cx=x";
                postdata = string.Format("usersfzh={0}&usermm={1}", fundReq.Identitycard, fundReq.Password);
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
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');history");
                if (!string.IsNullOrEmpty(errorMsg))
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='Table1']/tr[1]/td[1]/font", "text");
                if (results.Count>0)
                {
                    Res.Name = results[0].Replace("&nbsp;","");
                }
                Res.ProvidentFundNo = CommonFun.GetMidStr(httpResult.Html, "个人公积金账号:", "&nbsp;");
                Res.CompanyNo = CommonFun.GetMidStr(httpResult.Html, "单位账号:", "&nbsp;");
                Res.CompanyName = CommonFun.GetMidStr(httpResult.Html, "单位名称:", "</td>").Trim();
                Res.TotalAmount = CommonFun.GetMidStr(httpResult.Html, "你的当前余额是:", "元").ToDecimal(0);
                #endregion
                #region 第三步 公积金明细

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='div2']/table/tr[position()>1]", "",true);
                foreach (string s in results)
                {
                    ProvidentFundDetail detail = new ProvidentFundDetail();
                    List<string> tdRow = HtmlParser.GetResultFromParser(s, "/td", "text");
                    if (tdRow.Count < 5) continue;
                    detail.PayTime = tdRow[0].ToDateTime();
                    detail.ProvidentFundTime = tdRow[0].ToTrim("-").Substring(0,6);
                    detail.Description = tdRow[1].Trim();
                    if (tdRow[3].ToDecimal(0)>0)
                    {
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0);
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                    }
                    else if (detail.Description == "汇缴")
                    {
                        detail.PersonalPayAmount = detail.CompanyPayAmount = tdRow[2].ToDecimal(0)/2;
                        detail.ProvidentFundBase = detail.PersonalPayAmount/payRate;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                    }
                    else
                    {
                        detail.PersonalPayAmount = tdRow[2].ToDecimal(0);
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
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