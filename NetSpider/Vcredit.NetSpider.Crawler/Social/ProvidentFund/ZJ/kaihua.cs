using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
    public class kaihua : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.khgjj.com/";
        string fundCity = "zj_kaihua";
        #endregion
        #region 私有变量

        Regex regex = new Regex(@"[0-9][0-9]*");
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
            ProvidentFundLoanRes ResLoad = new ProvidentFundLoanRes();//贷款
            ProvidentFundReserveRes ReserveRes = new ProvidentFundReserveRes();//补充公积金
            ProvidentFundDetail detail = null;//公积金明细
            ProvidentFundDetail detailReserve = null;//补充公积金明细
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
                Url = baseUrl + "logincheck.php";
                postdata = string.Format("username={0}&password={1}&submit=%E6%9F%A5%E8%AF%A2", fundReq.Identitycard, fundReq.Password);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Encoding = Encoding.UTF8,
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
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "<big>", "</big>");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                #region  第二步  公积金,补充公积金基本信息
                //见详细
                #endregion
                #region 第三步 公积金,补充公积金明细

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='main']/table/tr[position()>1]", "", true);
                int index = 0;
                //建立在首条数据有年份,且年月递减的基础上
                foreach (string s in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(s, "//td", "text");
                    if (tdRow.Count < 11) continue;
                    MatchCollection matchs = regex.Matches(tdRow[0]);
                    //公积金,补充公积金基本信息
                    if (tdRow[3] == "正常" & index == 0)
                    {
                        Res.Name = tdRow[2];
                        Res.IdentityCard = tdRow[5];
                        Res.PersonalMonthPayAmount = Res.CompanyMonthPayAmount = tdRow[8].ToDecimal(0) / 2;
                        //Res.PersonalMonthPayRate = Res.CompanyMonthPayAmount = payRate;
                        //Res.SalaryBase = Res.PersonalMonthPayAmount/Res.PersonalMonthPayRate;
                        Res.TotalAmount = tdRow[10].ToDecimal(0);
                        Res.CompanyName = ReserveRes.CompanyName = tdRow[1];
                        Res.Status = ReserveRes.Status = tdRow[3];
                        Res.ProvidentFundNo = ReserveRes.ProvidentFundNo = tdRow[4];
                        ReserveRes.CompanyMonthPayAmount = tdRow[9].ToDecimal(0);
                        ReserveRes.TotalAmount = tdRow[6].ToDecimal(0);
                        index++;
                    }
                    detail = new ProvidentFundDetail();
                    detailReserve = new ProvidentFundDetail();
                    detail.Description = detailReserve.Description = tdRow[0];
                    detail.CompanyName = detailReserve.CompanyName = tdRow[1];
                    if (tdRow[3] == "正常")
                    {
                        detail.PaymentFlag = detailReserve.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = detailReserve.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                    }
                    else
                    {
                        detail.PaymentFlag = detailReserve.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = detailReserve.PaymentType = tdRow[3];
                    }
                    switch (matchs.Count)
                    {
                        case 3:
                            detail.PayTime = detailReserve.PayTime = Convert.ToDateTime(tdRow[0]);
                            detail.ProvidentFundTime = detailReserve.ProvidentFundTime = Convert.ToDateTime(tdRow[0]).ToString(Consts.DateFormatString7);
                            break;
                        case 2:
                            DateTime maybeTime = DateTime.ParseExact(tdRow[0], "M月d日", null);
                            ProvidentFundDetail detailTime = Res.ProvidentFundDetailList.LastOrDefault();
                            if (detailTime != null && maybeTime > detailTime.PayTime)
                            {
                                maybeTime = maybeTime.AddYears(-1);
                            }
                            detail.PayTime = detailReserve.PayTime = maybeTime;
                            detail.ProvidentFundTime = detailReserve.ProvidentFundTime = maybeTime.ToString(Consts.DateFormatString7);
                            break;
                    }
                    detail.PersonalPayAmount = detail.CompanyPayAmount = tdRow[8].ToDecimal(0) / 2;
                    detail.ProvidentFundBase = detail.PersonalPayAmount / payRate;
                    detailReserve.CompanyPayAmount = tdRow[9].ToDecimal(0);
                    Res.ProvidentFundDetailList.Add(detail);
                    ReserveRes.ProvidentReserveFundDetailList.Add(detailReserve);
                }
                #endregion
                //有补充公积金缴费记录
                ProvidentFundDetail isReserve = ReserveRes.ProvidentReserveFundDetailList.FirstOrDefault(o => o.CompanyPayAmount > 0);
                if (isReserve != null)
                {
                    Res.ProvidentFundReserveRes = ReserveRes;
                }
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
