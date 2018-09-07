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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.YN
{
    /// <summary>
    /// 532401197010270027李燕萍
    /// </summary>
    public class yuxi : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.yxgjj.com/";
        string fundCity = "yn_yuxi";
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
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

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            ProvidentFundLoanRes Res_load = new ProvidentFundLoanRes();
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            ProvidentFundDetail detail = null;
            try
            {
                //校验参数
                if (string.IsNullOrEmpty(fundReq.Name) || string.IsNullOrEmpty(fundReq.Identitycard))
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆

                Url = baseUrl + "find_detail_x.php";
                postdata = string.Format("xm={0}&id_card_input={1}&serch_type=ye&add_select=1", fundReq.Name.ToUrlEncode(Encoding.GetEncoding("gbk")), fundReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = httpResult.StatusDescription;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr[2]/td[@background='images/sanji_2.jpg']", "", true);
                if (results.Count > 0)
                {
                    if (results[0].EndsWith("错误"))
                    {
                        Res.StatusDescription = results[0];
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region 第二步,公积金基本信息

                Res.Name = fundReq.Name;
                Res.IdentityCard = fundReq.Identitycard;
                Res.Name = CommonFun.GetMidStr(httpResult.Html, "姓名:", "<br>");
                if (Res.Name != fundReq.Name)
                {
                    Res.StatusDescription = "无公积金信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.CompanyName = CommonFun.GetMidStr(httpResult.Html, "单位:", "<br>");
                Res.PersonalMonthPayAmount = Res.CompanyMonthPayAmount = CommonFun.GetMidStr(httpResult.Html, "月缴交额:", "元").ToDecimal(0) / 2;
                Res.TotalAmount = CommonFun.GetMidStr(httpResult.Html, "帐户余额:", "元").ToDecimal(0);
                Res.BankCardNo = CommonFun.GetMidStr(httpResult.Html, "银行账号:", "（");
                Res.EmployeeNo = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='tsgrzh']", "value")[0];
                //明细(最近3年)
                List<string> saveList = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='9size']/tr[position()>2]", "", true);
                #endregion
                #region 第三步,公积金明细

                //3年后剩余7年明细(服务器不稳定,请求至多2次,再不行拉倒)
                //Url = baseUrl + "find_detail_x.php";
                postdata = string.Format("s_year={3}&o_year={4}&Submit=%CC%E1%BD%BB&tsgrzh={0}&xm={1}&id_card_input={2}&serch_again=tsmx", Res.EmployeeNo, fundReq.Name.ToUrlEncode(Encoding.GetEncoding("gbk")), fundReq.Identitycard, DateTime.Now.Year - 10, DateTime.Now.Year - 2);
                for (int i = 0; i < 2; i++)
                {
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "Post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='9size']/tr[position()>2]", "", true);
                    if (results.Count > 0) break;
                }
                saveList.AddRange(results);
                foreach (string item in saveList)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                    if (tdRow.Count < 5) continue;
                    detail = new ProvidentFundDetail();
                    detail.PayTime = tdRow[0].ToDateTime();//缴费年月
                    if (tdRow[3].ToDecimal(0) > 0)
                    {
                        detail.Description = tdRow[1];
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0);
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                    }
                    else if (tdRow[1].IndexOf("汇缴", StringComparison.Ordinal) > -1 || tdRow[1].IndexOf("缴存", StringComparison.Ordinal) > -1)
                    {
                        detail.Description = tdRow[1];
                        if (tdRow[1].IndexOf("汇缴", StringComparison.Ordinal) > -1)
                        {
                            detail.ProvidentFundTime = tdRow[1].Substring(2, 6);//应属年月 
                        }
                        else if (tdRow[1].IndexOf("缴存", StringComparison.Ordinal) > -1)
                        {
                            detail.ProvidentFundTime = DateTime.ParseExact(tdRow[1].ToTrim().Trim(), "缴存yyyy年M月公积金", null).ToString(Consts.DateFormatString7);
                        }
                        detail.PersonalPayAmount = detail.CompanyPayAmount = tdRow[2].ToDecimal(0) / 2;
                        detail.ProvidentFundBase = detail.PersonalPayAmount / payRate;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                    }
                    else
                    {
                        detail.Description = tdRow[1] + tdRow[0];
                        detail.PersonalPayAmount = tdRow[2].ToDecimal(0);
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion
                #region 第四步,贷款基本信息

                //Url = baseUrl + "find_detail_x.php";
                postdata = string.Format("xm={0}&id_card_input={1}&serch_type=dk", fundReq.Name.ToUrlEncode(Encoding.GetEncoding("gbk")), fundReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                Res_load.Loan_Credit = CommonFun.GetMidStr(httpResult.Html, "借款金额:", "元").ToDecimal(0);//贷款金额
                if (Res_load.Loan_Credit == 0)
                {
                    Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                    return Res;
                }
                Res_load.IdentityCard = Res.IdentityCard;
                Res_load.Name = Res.Name;
                Res_load.Loan_Start_Date = CommonFun.GetMidStr(httpResult.Html, "贷款日期:", "<br>");//贷款开始日期
                Res_load.Loan_End_Date = CommonFun.GetMidStr(httpResult.Html, "到期日期:", "<br>");//贷款结束日期
                Res_load.Period_Total = CommonFun.GetMidStr(httpResult.Html, "贷款期限:", "个月");//贷款总期数
                Res_load.Loan_Balance = CommonFun.GetMidStr(httpResult.Html, "贷款余额:", "元").ToDecimal(0);//贷款余额

                #endregion
                Res.ProvidentFundLoanRes = Res_load;
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
