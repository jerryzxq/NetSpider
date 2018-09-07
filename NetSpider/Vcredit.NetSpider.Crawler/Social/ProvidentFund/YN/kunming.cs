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
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.YN
{
    public class kunming : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://zfgjj.km.gov.cn/website/";
        string fundCity = "yn_kunming";
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            string Url = string.Empty;
            try
            {
                Url = baseUrl+"trans/queryPer_login.jsp?txt=1";
                httpItem = new HttpItem
                {
                    URL=Url,
                    CookieCollection=cookies,
                    ResultCookieType=ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode!=HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies,httpResult.CookieCollection);

                Url = baseUrl + "trans/InmageCreat.jsp";
                httpItem = new HttpItem
                {
                    URL = Url,
                    ResultType=ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess; ;
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
            List<string> results = new List<string>();
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
                if (string.IsNullOrEmpty(fundReq.Identitycard) || fundReq.Username.IsEmpty())
                {
                    Res.StatusDescription = "身份证号或公积金账号不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //15位或18位身份证验证
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (!regex.IsMatch(fundReq.Identitycard))
                {
                    Res.StatusDescription = "请输入有效的身份证号";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步登陆

                //登陆
                Url = baseUrl + "website.do?className=TRA020001";
                postdata = string.Format("txt=1&accnum={2}&certinum={1}&verify={0}&Submit=%CC%E1%BD%BB",fundReq.Vercode,fundReq.Identitycard,fundReq.Username);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Encoding=Encoding.GetEncoding("GBK"),
                    Method="Post",
                    Postdata=postdata,
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
                //错误提示
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert(", "'); history.back()");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription =Regex.Unescape(errorMsg);
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html,"//div[@id='main']","text");
                if (results.Count>0)
                {
                    if (results[0].Contains("查询失败"))
                    {
                        string temp = new Regex(@"[\u67e5\u8be2\u5931\u8d25\:\s]*").Replace(results[0], "");
                        Res.StatusDescription = new Regex(@"[^\u4e00-\u9fa5]").Replace(temp,"");
                        Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                        return Res;
                    }
                }
                #endregion
                #region 第二步，获取个人信息

                Res.Name = fundReq.Name;
                Res.Phone = fundReq.Mobile;
                Res.IdentityCard = fundReq.Identitycard;
                Res.ProvidentFundNo = fundReq.Username;
                Url = baseUrl + "trans/gjjquery.do";
                postdata = string.Format("accnum={0}&ceridnum={1}&className=TRA020101",fundReq.Username,fundReq.Identitycard);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='main']/table/tr[1]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0].Trim();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='main']/table/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Name = results[0].Trim();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='main']/table/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.OpenTime = results[0].Trim();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='main']/table/tr[4]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0].Trim();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='main']/table/tr[5]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.SalaryBase = results[0].Trim().ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='main']/table/tr[6]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = results[0].Trim().ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='main']/table/tr[7]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = results[0].Trim().ToDecimal(0);
                }
                if (Res.SalaryBase > 0)
                {
                    Res.CompanyMonthPayAmount = Math.Round(Res.SalaryBase * Res.CompanyMonthPayRate,2);
                    Res.PersonalMonthPayAmount = Math.Round(Res.SalaryBase * Res.PersonalMonthPayRate,2);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='main']/table/tr[9]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].Trim().ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='main']/table/tr[11]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Status = results[0].Trim();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='main']/table/tr[12]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0].Trim();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='main']/table/tr[13]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0].Trim();
                }
                #endregion
                #region 第三步，历史账户明细

                decimal perAccounting;//个人占比
                decimal comAccounting;//公司占比
                decimal totalRate;//总缴费比率
                if (Res.PersonalMonthPayRate > 0 && Res.CompanyMonthPayRate > 0)
                {
                    totalRate = Res.PersonalMonthPayRate + Res.CompanyMonthPayRate;
                    perAccounting = (Res.PersonalMonthPayRate / totalRate);
                    comAccounting = (Res.CompanyMonthPayRate / totalRate);
                }
                else
                {
                    totalRate = payRate * 2;//0.16
                    perAccounting = comAccounting = 0.50M;
                }
                results = new List<string>();
                Url = baseUrl + "trans/gjjquery.do";
                int pages = 1;
                int pageNO = 1;
                do
                {
                    postdata = string.Format("enddate={4}&className=TRA020102&begdate={3}&ceridnum={2}&accnum={1}&pageNo={0}",pageNO,fundReq.Username,fundReq.Identitycard,DateTime.Now.AddYears(-4).ToString("yyyy-01-01"),DateTime.Now.ToString("yyyy-MM-dd"));
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Method = "Post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    if (pageNO==1)
                    {
                        pages = CommonFun.GetMidStr(httpResult.Html.Replace("&nbsp;", ""), "onclick=\"subMit(-1)\">共", "页").Trim().ToInt(0);
                    }
                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html,"//div[@id='main']/table/tr[position()>1]","inner"));
                    pageNO++;
                } while (pageNO <= pages);
                foreach (var items in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(items, "//td", "text");
                    if (tdRow.Count < 11)
                    {
                        continue;
                    }
                    detail = new ProvidentFundDetail();
                    detail.Description = tdRow[10].Trim();
                    detail.PayTime = tdRow[0].Trim().ToDateTime();
                    detail.CompanyName = tdRow[1].Trim();
                    if (tdRow[3].Trim() == "汇缴")
                    {
                        detail.PersonalPayAmount = Math.Round(tdRow[5].ToDecimal(0) * perAccounting, 2); //个人月缴额
                        detail.CompanyPayAmount = Math.Round(tdRow[5].ToDecimal(0) * comAccounting, 2); //公司月缴额
                        detail.ProvidentFundBase = Math.Round(tdRow[5].ToDecimal(0) / totalRate, 2); //缴费基数
                        detail.ProvidentFundTime =Convert.ToDateTime(tdRow[8].Trim()).ToString("yyyyMM"); //应属年月
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        PaymentMonths++;
                    }
                    else
                    {//（补缴，结息etc，数据不精确，只做参考用）
                        detail.PersonalPayAmount = tdRow[5].ToDecimal(0); //个人缴费
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
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
