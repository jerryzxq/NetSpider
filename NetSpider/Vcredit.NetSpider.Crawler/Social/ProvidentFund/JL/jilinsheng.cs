using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.JL
{
    public class jilinsheng : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.jlsgjj.cn/";
        string fundCity = "jl_jilinsheng";
        List<string> results = new List<string>();
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = "所选城市无需初始化";
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
            ProvidentFundDetail detail = null;
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (string.IsNullOrEmpty(fundReq.Identitycard) || !regex.IsMatch(fundReq.Identitycard))
                {
                    Res.StatusDescription = "身份证号不能为空或身份证号格式不正确";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrEmpty(fundReq.Password))
                {
                    Res.StatusDescription = "登录密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                string tranCode = string.Empty;
                Url = baseUrl + "ecdomain/portal/portlets/query/query.jsp?action=empquery";
                postdata = String.Format("cardno={0}&password={1}", fundReq.Identitycard, fundReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }

                if (!httpResult.Html.Contains(fundReq.Identitycard))
                {
                    Res.StatusDescription = "用户名或密码错误";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }

                #endregion
                #region 第二步，查询个人基本信息
                //在第一步登录中已经获取到基本信息

                //单位基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr/td[@class='Font9blue']", "");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];  //单位账号
                    Res.CompanyName = results[1];  //单位名称
                    Res.OpenTime = results[5];  //开户日期
                    Res.LastProvidentFundTime = results[7];  //最后缴费日期
                }

                //个人基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[4]/tr[2]/td", "");
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];  //职工编号
                    Res.Name = results[1];  //职工姓名
                    Res.IdentityCard = results[2];  //证件号码
                    Res.PersonalMonthPayAmount = results[3].ToDecimal(0) / 2;  //暂定为职工缴费，单位缴费均为缴费额的1/2
                    Res.CompanyMonthPayAmount = results[3].ToDecimal(0) / 2;  //暂定为职工缴费，单位缴费均为缴费额的1/2
                    Res.TotalAmount = results[4].ToDecimal(0);  //账户余额
                    Res.Status = results[5];  //职工状态
                }
                #endregion


                #region 第三步，缴费明细查询
                Url = baseUrl + "ecdomain/portal/portlets/query/query.jsp?" + string.Format("action=empflow&empno={0}&empname={1}&cardno={2}", Res.EmployeeNo, Res.Name.ToUrlEncode(), Res.IdentityCard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[3]/tr", "");
                foreach (var item in results)
                {
                    detail = new ProvidentFundDetail();
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow[0].Contains("日期"))
                    {
                        continue;
                    }
                    detail.PayTime = tdRow[0].ToDateTime();  //缴费日期
                    detail.Description = tdRow[2];  //描述
                    detail.CompanyName = Res.CompanyName;  //单位名称
                    if (detail.Description.Contains("汇"))
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;  //缴费标志
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;  //缴费类型
                        detail.PersonalPayAmount=tdRow[1].ToDecimal(0)/2;  //在基本信息中设定为个人缴费，单位缴费各1/2
                        detail.CompanyPayAmount = tdRow[1].ToDecimal(0) / 2;  //在基本信息中设定为个人缴费，单位缴费各1/2
                        PaymentMonths++;
                    }
                    else if (detail.Description.Contains("支取") || detail.Description.Contains("提取"))
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                        detail.PersonalPayAmount = tdRow[1].ToDecimal(0);  //支取 将缴费信息存入个人缴费字段中
                        Res.Description = "有支取，请人工校验。";  //缴费详单存在支取
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = tdRow[1].ToDecimal(0);  //其他缴费方式 将缴费信息存入个人缴费字段中
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion


                #region ********贷款信息需要合同编号**********

                #endregion
                Res.PaymentMonths = PaymentMonths;
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
