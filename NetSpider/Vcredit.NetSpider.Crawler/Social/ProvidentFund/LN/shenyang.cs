using System;
using System.Collections.Generic;
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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.LN
{
    public class shenyang : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.sygjj.com:8080/cxxt/";
        string fundCity = "ln_shenyang";
        int PaymentMonths = 0;
        #endregion
        #region 私有变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        private Regex reg = new Regex(@"[\&nbsp;\s;\,;\%;\-;]*");
        decimal payRate = (decimal)0.08;
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "common/validateImage.action";//验证码地址
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                //添加缓存
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
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
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
                if (string.IsNullOrEmpty(fundReq.Identitycard) || string.IsNullOrEmpty(fundReq.Username))
                {
                    Res.StatusDescription = "联名卡号或身份证号不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,查询基本信息
                Url = baseUrl + "personalpaf/personalAsfQueryAction.action?idCard=" + fundReq.Identitycard + "&pafCard=" + fundReq.Username + "&identifying=" + fundReq.Vercode + "";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl + "personalpaf/personalAction.action",
                    Encoding = Encoding.GetEncoding("utf-8"),
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//center/h2[1]/font", "innertext");
                if (results.Count > 0)
                {
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    Res.StatusDescription = results[0];
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='listtable']/tr[1]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.Name = results[0].Trim();//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='listtable']/tr[1]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0].Trim();//个人账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='listtable']/tr[2]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0].Trim();//身份证号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='listtable']/tr[2]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.TotalAmount = reg.Replace(results[0], "").ToDecimal(0);//查询日余额     
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='listtable']/tr[3]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.BankCardNo = results[0].Trim();//磁卡卡号     
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='listtable']/tr[4]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.Status = results[0].Trim();//缴存状态     
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='listtable']/tr[5]/td[2]", "innertext");
                if (results.Count > 0)
                {//2015-05-01 
                    Res.LastProvidentFundTime = reg.Replace(results[0], "");//缴至年月     
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='listtable']/tr[6]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.SalaryBase = reg.Replace(results[0], "").ToDecimal(0);//缴存基数     
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='listtable']/tr[6]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = reg.Replace(results[0], "").ToDecimal(0) * 0.01M;//单位缴存比例     
                    Res.CompanyMonthPayAmount = (Res.SalaryBase * Res.CompanyMonthPayRate).ToString("f2").ToDecimal(0);//单位月缴费
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='listtable']/tr[7]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = reg.Replace(results[0], "").ToDecimal(0) * 0.01M;//个人缴存比例   
                    Res.PersonalMonthPayAmount = (Res.SalaryBase * Res.PersonalMonthPayRate).ToString("f2").ToDecimal(0);//个人月缴费
                }
                #endregion
                #region 第二步,查询缴费明细
                Url = baseUrl + "personalpaf/pafSearchAction.action?account=" + Res.ProvidentFundNo + "";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='600']/tr/td[2]/div", "inner");
                int totalPages = 0;//明细总页数
                if (results.Count > 0)
                {
                    string temp = reg.Replace(results[0], "");
                    MatchCollection matchs = new Regex(@"[0-9]{0,}[1-9]").Matches(temp);
                    if (matchs.Count >= 2)
                    {
                        string pagenu = matchs[1].Value;
                        int a = 0;
                        if (int.TryParse(pagenu, out a))
                        {
                            totalPages = a;
                            if (totalPages == 0)
                            {
                                Res.StatusDescription = "无缴费明细";
                                Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                                return Res;
                            }
                            Res = GetDetails(httpResult, totalPages);
                        }
                        else
                        {
                            Res.StatusDescription = "极可能由于页面改版,导致无法获取缴费明细页码总数";
                            Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                            return Res;
                        }
                    }
                }
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
        /// <summary>
        /// 处理缴费明细页面
        /// </summary>
        /// <param name="HttpResult">缴费明细首页数据</param>
        /// <param name="totalPages">缴费明细总页码</param>
        /// <returns>Res</returns>
        private Entity.Service.ProvidentFundQueryRes GetDetails(HttpResult HttpResult, int totalPages)
        {
            string Url = string.Empty;
            //获取第一页数据
            List<string> reResults1 = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='listtable']/tr[position()>1]", "inner");
            if (reResults1.Count > 0)
            {
                ListString(reResults1);
            }
            //页码大于1，接着从第2页查起
            if (totalPages > 1)
            {
                for (int i = 2; i <= totalPages; i++)
                {
                    Url = baseUrl + "personalpaf/pafSearchAction.action?index=next&pageno=" + i + "&msg=";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    List<string> reResults2 = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='listtable']/tr[position()>1]", "inner");
                    if (reResults2.Count > 0)
                    {
                        ListString(reResults2);
                    }
                }
            }
            return Res;
        }
        /// <summary>
        /// 保存明细列表数据
        /// </summary>
        /// <param name="reResults">抓取到的数据列表</param>
        private void ListString(List<string> reResults)
        {
            decimal perAccounting = 0;//个人占比
            decimal comAccounting = 0;//公司占比
            decimal totalRate = 0;//总缴费比率
            if (Res.PersonalMonthPayRate > 0 && Res.CompanyMonthPayRate > 0)
            {
                totalRate = Res.PersonalMonthPayRate + Res.CompanyMonthPayRate;
                perAccounting = (Res.PersonalMonthPayRate / totalRate);
                comAccounting = (Res.CompanyMonthPayRate / totalRate);
            }
            else
            {
                totalRate = (payRate) * 2;//0.16
                perAccounting = comAccounting = 0.50M;
            }
            if (reResults.Count == 0)
            {
                Res.StatusDescription = "暂无账户明细";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
            }
            else
            {
                foreach (var item in reResults)
                {
                    ProvidentFundDetail detail = new ProvidentFundDetail();
                    List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow.Count != 6)
                    {
                        continue;
                    }
                    detail.PayTime = tdRow[0].ToDateTime();//缴费年月
                    detail.Description = tdRow[2];//描述
                    if (tdRow[2].Trim() == "汇缴")
                    {
                        detail.PersonalPayAmount = reg.Replace(tdRow[1], "").ToDecimal(0) * perAccounting.ToString("f2").ToDecimal(0);//个人缴费金额
                        detail.CompanyPayAmount = reg.Replace(tdRow[1], "").ToDecimal(0) * comAccounting.ToString("f2").ToDecimal(0);//企业缴费金额
                        detail.ProvidentFundBase = (reg.Replace(tdRow[1], "").ToDecimal(0) / (totalRate)).ToString("f2").ToDecimal(0);//缴费基数
                        detail.ProvidentFundTime = new Regex(@"^[0-9]{0,6}").Match(new Regex(@"[^1-9;0-9]*").Replace(tdRow[5], "")).Value;//应属年月
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        PaymentMonths++;
                    }
                    else if (tdRow[2].Trim().Contains("支取") || tdRow[2].Trim().Contains("提取"))
                    {
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PersonalPayAmount = reg.Replace(tdRow[1], "").ToDecimal(0) * perAccounting.ToString("f2").ToDecimal(0);//个人缴费金额
                    }
                    else
                    {//（补缴，结息etc，数据不精确，只做参考用）
                        detail.PersonalPayAmount = reg.Replace(tdRow[1], "").ToDecimal(0);//个人缴费金额
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
            }
        }
    }
}
