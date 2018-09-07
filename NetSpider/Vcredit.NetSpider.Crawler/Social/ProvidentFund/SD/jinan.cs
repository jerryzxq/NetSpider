using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.Common.Constants;
namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SD
{
    public class jinan : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://123.233.117.50:801/jnwt/";
        string fundCity = "sd_jinan";
        #endregion
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "vericode.jsp";//验证码地址
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
        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(ProvidentFundReq fundReq)
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
                //15位或18位身份证验证
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (regex.IsMatch(fundReq.Identitycard) == false)
                {
                    Res.StatusDescription = "输入正确的身份证号";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Password))
                {
                    Res.StatusDescription = "请输入密码";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Vercode))
                {
                    Res.StatusDescription = "请输入验证码";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                Url = baseUrl + "per.login";
                postdata = String.Format("certinum={0}&perpwd={1}&vericode={2}", fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                string errorInfo = CommonFun.GetMidStr(httpResult.Html, "<script>alert(", "');</script>");
                if (!errorInfo.IsEmpty())
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='frameContentShow']/div[@class='WTLoginError']/ul/li[1]", "text");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region 第二步，查询个人基本信息
                Url = baseUrl + "init.summer?_PROCID=60020009";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='UnitAccName']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];//单位名称
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='UnitAccNum']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];//单位账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='AccNum']", "value");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];//公积金账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='AccName']", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//职工姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='CertiNum']", "value");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//证件号码
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='CardNo']", "value");
                if (results.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(results[0]))
                    {
                        Res.BankCardNo = results[0];//银行卡号
                    }
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@id='PerAccState']/option[@selected='selected']", "innertext");
                if (results.Count > 0)
                {
                    Res.Status = results[0];//状态
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='AgentUnitNo']", "value");
                if (results.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(results[0]))
                    {
                        Res.CompanyLicense = results[0];//公司登记号
                    }
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='OpenDate']", "value");
                if (results.Count > 0)
                {
                    Res.OpenTime = results[0];//开户日期
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='LastPayDate']", "value");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0];//最后汇缴月
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='UnitProp']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = results[0].ToDecimal(0) * 0.01M;//单位缴费比率
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='IndiProp']", "value");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = results[0].ToDecimal(0) * 0.01M;//个人缴费比率
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='MonPaySum']", "value");
                decimal totalRate = 0; //总比率
                decimal perRate = 0; //个人缴费占比
                decimal comRate = 0; //公司缴费占比
                if (results.Count > 0)
                {
                    if (Res.CompanyMonthPayRate > 0 && Res.PersonalMonthPayRate > 0)
                    {
                        totalRate = Res.CompanyMonthPayRate + Res.PersonalMonthPayRate;
                        perRate = Res.PersonalMonthPayRate / totalRate;
                        comRate = Res.CompanyMonthPayRate / totalRate;
                        Res.SalaryBase = (results[0].ToDecimal(0) * perRate) / (Res.PersonalMonthPayRate);
                        //基本薪资
                    }
                    else
                    {
                        perRate = comRate = (decimal)0.5;
                    }
                    Res.CompanyMonthPayAmount = results[0].ToDecimal(0) * comRate; //单位月缴费
                    Res.PersonalMonthPayAmount = results[0].ToDecimal(0) * perRate; //个人月缴费
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='BaseNumber']", "value");
                if (results.Count > 0)
                {
                    Res.SalaryBase = results[0].ToDecimal(0);//基本薪资 
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='Balance']", "value");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);//账户总额 
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='AccBankName']", "value");
                if (results.Count > 0)
                {
                    Res.Bank = results[0];//开户银行
                }
                #endregion
                #region 第三步, 查询个人明细

                Url = baseUrl + "init.summer?_PROCID=60020007";
                string DATAlISTGHOST = string.Empty;
                string _DATAPOOL_ = string.Empty;
                string _APPLY = string.Empty;
                string _CHANNEL = string.Empty;
                string _PROCID = string.Empty;
                string _LoginType = string.Empty;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    Encoding = Encoding.GetEncoding("gbk"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                errorInfo = CommonFun.GetMidStr(httpResult.Html, "alert('", "');");
                if (!string.IsNullOrWhiteSpace(errorInfo) && !httpResult.Html.Contains("html"))
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//textarea[@name='DATAlISTGHOST']", "innertext");
                if (results.Count > 0)
                {
                    DATAlISTGHOST = results[0].ToUrlEncode();
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//textarea[@name='_DATAPOOL_']", "innertext");
                if (results.Count > 0)
                {
                    _DATAPOOL_ = results[0].ToUrlEncode();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='_APPLY']", "value");
                if (results.Count > 0)
                {
                    _APPLY = results[0].ToUrlEncode();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='_CHANNEL']", "value");
                if (results.Count > 0)
                {
                    _CHANNEL = results[0].ToUrlEncode();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='_PROCID']", "value");
                if (results.Count > 0)
                {
                    _PROCID = results[0].ToUrlEncode();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='_LoginType']", "value");
                if (results.Count > 0)
                {
                    _LoginType = results[0].ToUrlEncode();
                }
                //_DATAPOOL_ = "rO0ABXNyABZjb20ueWR5ZC5wb29sLkRhdGFQb29sp4pd0OzirDkCAAZMAAdTWVNEQVRFdAASTGph%0AdmEvbGFuZy9TdHJpbmc7TAAGU1lTREFZcQB%2BAAFMAAhTWVNNT05USHEAfgABTAAHU1lTVElNRXEA%0AfgABTAAHU1lTV0VFS3EAfgABTAAHU1lTWUVBUnEAfgABeHIAEWphdmEudXRpbC5IYXNoTWFwBQfa%0AwcMWYNEDAAJGAApsb2FkRmFjdG9ySQAJdGhyZXNob2xkeHA%2FQAAAAAAAGHcIAAAAIAAAABR0AAdf%0AQUNDTlVNdAAMMjAyMDcwMTY5NjE1dAALX1VOSVRBQ0NOVU1wdAAHX1BBR0VJRHQABXN0ZXAxdAAD%0AX0lTc3IADmphdmEubGFuZy5Mb25nO4vkkMyPI98CAAFKAAV2YWx1ZXhyABBqYXZhLmxhbmcuTnVt%0AYmVyhqyVHQuU4IsCAAB4cP%2F%2F%2F%2F%2F%2F9nY7dAAMX1VOSVRBQ0NOQU1FcHQABl9MT0dJUHQAETIwMTUw%0ANjI5MTQ1MDE1ODUydAAIX0FDQ05BTUV0AAnotbXmoYLmnIt0AAlpc1NhbWVQZXJ0AAVmYWxzZXQA%0AB19QUk9DSUR0AAg2MDAyMDAxMHQAC19TRU5ET1BFUklEdAAMMjAyMDcwMTY5NjE1dAAQX0RFUFVU%0AWUlEQ0FSRE5VTXQAEjM3MDEwNTE5ODEwNjE4NTYxMXQACV9TRU5EVElNRXQACjIwMTUtMDYtMjl0%0AAAtfQlJBTkNIS0lORHQAATB0AAlfU0VORERBVEV0AAoyMDE1LTA2LTI5dAATQ1VSUkVOVF9TWVNU%0ARU1fREFURXEAfgAfdAAFX1RZUEV0AARpbml0dAAHX0lTQ1JPUHQAATF0AAlfUE9SQ05BTUV0ABvl%0AvoDlubTkuKrkurrmmI7nu4botKbmn6Xor6J0AAdfVVNCS0VZcHQACF9XSVRIS0VZcQB%2BAB14dAAI%0AQFN5c0RhdGV0AAdAU3lzRGF5dAAJQFN5c01vbnRodAAIQFN5c1RpbWV0AAhAU3lzV2Vla3QACEBT%0AeXNZZWFy";
                //DATAlISTGHOST = "rO0ABXNyABNqYXZhLnV0aWwuQXJyYXlMaXN0eIHSHZnHYZ0DAAFJAARzaXpleHAAAAABdwQAAAAK%0Ac3IAJWNvbS55ZHlkLm5icC5lbmdpbmUucHViLkRhdGFMaXN0R2hvc3RCsjhA3j2pwwIAA0wAAmRz%0AdAASTGphdmEvbGFuZy9TdHJpbmc7TAAEbmFtZXEAfgADTAADc3FscQB%2BAAN4cHQAEHdvcmtmbG93%0ALmNmZy54bWx0AAhkYXRhbGlzdHQA93NlbGVjdCB0cmFuc2RhdGUsIG9wZXIsIHNleCwgYW10MSwg%0AYW10MiwoY2FzZSBiZWdpbmRhdGVjIHdoZW4gJzE4OTkxMicgdGhlbiAnJyBlbHNlIGJlZ2luZGF0%0AZWMgZW5kKSBiZWdpbmRhdGVjLCAoY2FzZSBlbmRkYXRlYyB3aGVuICcxODk5MTInIHRoZW4gJycg%0AZWxzZSBlbmRkYXRlYyBlbmQpIGVuZGRhdGVjLCBpbnN0YW5jZW51bSBmcm9tIGRwMDc3IHdoZXJl%0AIGluc3RhbmNlbnVtID0gLTYyNTA5MyBvcmRlciBieSB0cmFuc2RhdGV4";

                Url = baseUrl + string.Format("command.summer?uuid={0}", DateTime.Now.Subtract(DateTime.Parse("1970-1-1")).TotalMilliseconds.ToString("f0"));
                postdata = string.Format("%24page=%2Fydpx%2F60020007%2F602007_01.ydpx&_ACCNUM={0}&_PAGEID=step1&_IS={1}&_LOGIP={2}&_ACCNAME={3}&isSamePer=false&_PROCID=60020007&_SENDOPERID={0}&_DEPUTYIDCARDNUM={4}&_SENDTIME={5}&_BRANCHKIND=0&_SENDDATE={5}&CURRENT_SYSTEM_DATE={5}&_TYPE=init&_ISCROP=1&_PORCNAME=%E5%BD%93%E5%B9%B4%E4%B8%AA%E4%BA%BA%E6%98%8E%E7%BB%86%E8%B4%A6%E6%9F%A5%E8%AF%A2&_WITHKEY=0&BegDate={6}-01-01&EndDate={5}", Res.ProvidentFundNo, CommonFun.GetMidStr(httpResult.Html, "'_IS': '", "',"), DateTime.Now.ToString("yyyyMMddHHmmssfff"), Res.Name.ToUrlEncode(), Res.IdentityCard, DateTime.Now.ToString("yyyy-MM-dd"), (DateTime.Now.Year - 1));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                List<jinan_Detail> details = new List<jinan_Detail>();
                string EndDate = DateTime.Now.ToString("yyyy-MM-dd");
                int CurrentPage = 0;//当前页
                int PageNum = 0;//页数
                do
                {
                    CurrentPage++;
                    Url = baseUrl + string.Format("dynamictable?uuid={0}", DateTime.Now.Subtract(DateTime.Parse("1970-1-1")).TotalMilliseconds.ToString("f0"));
                    postdata = string.Format("dynamicTable_id=datalist&dynamicTable_currentPage={0}&dynamicTable_pageSize=10&dynamicTable_nextPage={1}&dynamicTable_page=%2Fydpx%2F60020007%2F602007_01.ydpx&dynamicTable_paging=true&dynamicTable_configSqlCheck=0&errorFilter=1%3D1&BegDate=2014-01-01&EndDate={2}&_APPLY={3}&_CHANNEL={4}&_PROCID={5}&_LoginType={6}&DATAlISTGHOST={7}&_DATAPOOL_={8}", CurrentPage - 1, CurrentPage, EndDate, _APPLY, _CHANNEL, _PROCID, _LoginType, DATAlISTGHOST, _DATAPOOL_);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    PageNum = jsonParser.GetResultFromMultiNode(httpResult.Html, "data:pageCount").ToInt(0);
                    details.AddRange(jsonParser.DeserializeObject<List<jinan_Detail>>(jsonParser.GetResultFromMultiNode(httpResult.Html, "data:data")));
                } while (CurrentPage < PageNum);
                foreach (var item in details)
                {

                    detail = new ProvidentFundDetail();
                    detail.PayTime = item.transdate.ToDateTime(Consts.DateFormatString2);//缴费年月
                    switch (item.oper.Trim())
                    {

                        case "1015":
                            detail.Description = "汇缴";
                            PaymentMonths++;
                            break;
                        case "1018":
                            detail.Description = "不定额补缴";
                            break;
                        case "2024":
                            detail.Description = "住房提取";
                            break;
                        case "2037":
                            detail.Description = "年度结息";
                            break;
                    }
                    if (detail.Description == "汇缴")
                    {
                        if (comRate > 0 && perRate > 0)
                        {
                            detail.PersonalPayAmount = item.amt1.ToDecimal(0) * perRate;//个人月缴额
                            detail.CompanyPayAmount = item.amt1.ToDecimal(0) * comRate;//公司月缴额
                            detail.ProvidentFundBase = (item.amt1.ToDecimal(0) / (totalRate)).ToString("f2").ToDecimal(0);//缴费基数
                        }
                        else
                        {
                            detail.PersonalPayAmount = item.amt1.ToDecimal(0) / 2;//个人月缴额
                            detail.CompanyPayAmount = item.amt1.ToDecimal(0) / 2;//公司月缴额
                            detail.ProvidentFundBase = (detail.PersonalPayAmount / payRate).ToString("f2").ToDecimal(0);//缴费基数
                        }
                        detail.ProvidentFundTime = item.enddatec;//应属年月
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                    }
                    else
                    {
                        detail.PersonalPayAmount = item.amt1.ToDecimal(0);//个人月缴额
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                #endregion
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }

    }
    /// <summary>
    /// 济南缴费明细数据字段
    /// </summary>
    class jinan_Detail
    {
        /// <summary>
        /// 表隐藏列数据（暂时用不到）
        /// </summary>
        public string instancenum { get; set; }
        /// <summary>
        /// 缴费标志 贷(+)
        /// </summary>
        public string sex { get; set; }
        /// <summary>
        /// 缴费年月
        /// </summary>
        public string transdate { get; set; }
        /// <summary>
        /// 缴费金额
        /// </summary>
        public string amt1 { get; set; }
        /// <summary>
        /// 缴交终止年月
        /// </summary>
        public string enddatec { get; set; }
        /// <summary>
        /// 账户余额
        /// </summary>
        public string amt2 { get; set; }
        /// <summary>
        /// 缴交起始年月
        /// </summary>
        public string begindatec { get; set; }
        /// <summary>
        /// 描述（1015:汇缴,1018:不定额补缴,2037:年度结息）
        /// </summary>
        public string oper { get; set; }
    }
}
