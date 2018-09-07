using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SN
{
    //分页数据不够
    public class baoji : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = new HttpResult();
        HttpItem httpItem = new HttpItem();
        string baseUrl = "http://61.134.23.147:8080/";
        string fundCity = "sn_baoji";
        #endregion

        #region 私有变量
        string Url = string.Empty;
        private ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        private List<string> results = new List<string>();
        private int PaymentMonths = 0;//连续缴费月数
        #endregion


        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                vcRes.StatusDescription = fundCity + "无需初始化";
                ////添加缓存
                //CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                vcRes.StatusCode = ServiceConsts.StatusCode_error;
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return vcRes;
        }

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            ProvidentFundDetail detail = null;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数

                //15位或18位身份证验证
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (!regex.IsMatch(fundReq.Identitycard))
                {
                    Res.StatusDescription = "请输入15位或18位身份证号";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "密码不能为空！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string zgzh = string.Empty;
                string sfzh = string.Empty;
                string zgxm = string.Empty;
                string dwbm = string.Empty;
                string cxyd = string.Empty;
                string zgzt = string.Empty;
                #region  第一步 登陆
                // http://61.134.23.147:8080/wscx/zfbzgl/zfbzsq/login_hidden.jsp?pass=111111&zh=610321198704140035
                Url = baseUrl + string.Format("wscx/zfbzgl/zfbzsq/login_hidden.jsp?pass={0}&zh={1}", fundReq.Password, fundReq.Identitycard);
                httpItem = new HttpItem
                {
                    URL = Url,
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
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\");");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgzh']", "value");
                if (results.Count > 0)
                {
                    zgzh = results[0];
                }
                if (String.IsNullOrWhiteSpace(zgzh))
                {
                    Res.StatusDescription = "无公积金信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='sfzh']", "value");
                if (results.Count > 0)
                {
                    sfzh = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgxm']", "value");
                if (results.Count > 0)
                {
                    zgxm = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dwbm']", "value");
                if (results.Count > 0)
                {
                    dwbm = results[0];
                }
                #endregion

                #region 第二步 查询基本信息

                Url = baseUrl + "wscx/zfbzgl/" + CommonFun.GetMidStr(httpResult.Html, "MainFrame.location.href =\"..", "\";</script>");
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("gbk"),
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
                results = HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", "").Replace("元", ""), "//table[@class='1']//font", "");
                if (results.Count > 21)
                {
                    Res.Name = results[0];
                    Res.BankCardNo = results[1];
                    Res.IdentityCard = results[2];
                    Res.ProvidentFundNo = results[3];
                    Res.CompanyName = results[4];
                    Res.OpenTime = results[6];
                    Res.Status = results[7];
                    Res.SalaryBase = results[8].ToDecimal(0);
                    string[] rateArray = results[9].Replace("%", "").Split('/');
                    if (rateArray.Length == 2)
                    {
                        Res.PersonalMonthPayRate = rateArray[0].ToDecimal(0) / 100;
                        Res.CompanyMonthPayRate = rateArray[1].ToDecimal(0) / 100;
                    }
                    else
                    {
                        Res.PersonalMonthPayRate = Res.CompanyMonthPayRate = payRate;
                    }
                    Res.CompanyMonthPayAmount = results[12].ToDecimal(0);
                    Res.PersonalMonthPayAmount = results[14].ToDecimal(0);
                    Res.TotalAmount = results[19].ToDecimal(0);
                    Res.LastProvidentFundTime = results[20];
                }
                if (fundReq.Identitycard.Substring(0, 14) == Res.IdentityCard.Substring(0, 14) && fundReq.Name.Substring(0, 1) == Res.Name.Substring(0, 1))
                {
                    Res.Name = fundReq.Name;
                    Res.IdentityCard = fundReq.Identitycard;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='cxyd']", "value");
                if (results.Count > 0)
                {
                    cxyd = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgzt']", "value");
                if (results.Count > 0)
                {
                    zgzt = results[0];
                }
                #endregion

                #region 第三步  获取明细

               
                decimal perAcount = Res.PersonalMonthPayRate > 0 && Res.CompanyMonthPayRate > 0
                    ? Res.PersonalMonthPayRate / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate) : 0.5M;//个人缴费比率/(个人缴费比率+公司缴费比率)
                Url = baseUrl + "wscx/zfbzgl/gjjmxcx/gjjmx_cx.jsp";
                postdata = string.Format("sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd={4}&zgzt={5}", sfzh, Res.Name, zgzh, dwbm, cxyd, zgzt);
                httpItem = new HttpItem
                {
                    URL = Url,
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
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']//tr[position()>1]", "");
                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                    if (tdRow.Count != 6) continue;
                    detail = new ProvidentFundDetail();
                    if (!string.IsNullOrWhiteSpace(tdRow[0]))
                    {
                        detail.PayTime = tdRow[0].ToDateTime();
                    }
                    detail.Description = tdRow[5];

                    if (tdRow[5].IndexOf("汇缴") != -1)
                    {
                        detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[5], "汇缴", "公积金");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = tdRow[2].ToDecimal(0) * perAcount;//金额
                        detail.CompanyPayAmount = tdRow[2].ToDecimal(0) * (1 - perAcount);//金额
                        detail.ProvidentFundBase = (detail.PersonalPayAmount / perAcount);//缴费基数
                        PaymentMonths++;
                    }
                    else if (tdRow[5].IndexOf("合计") != -1 || tdRow[5].IndexOf("提取") != -1)
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                        detail.PersonalPayAmount = tdRow[2].ToDecimal(0);//金额
                    }
                    else if (tdRow[5].IndexOf("补缴") != -1)
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Back;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Back;
                        detail.CompanyPayAmount = tdRow[2].ToDecimal(0);//金额
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = tdRow[1].ToDecimal(0) + tdRow[2].ToDecimal(0);
                    }
                    Res.ProvidentFundDetailList.Add(detail);
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
