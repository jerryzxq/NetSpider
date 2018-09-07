using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;


namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.JS
{
    public class suzhou : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "https://gr.szgjj.gov.cn/";
        string fundCity = "js_suzhou";
        #endregion
        #region 私有变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        string Url = string.Empty;
        string postdata = string.Empty;
        decimal payRate = (decimal)0.12;
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
                Url = "https://gr.szgjj.gov.cn/retail/validateCodeServlet";
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
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;

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
            string referUrl = string.Empty;
            Regex reg = new Regex(@"[\&nbsp;\,\s]*");
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(fundReq.Token);
                    SpiderCacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.LoginType == "1")
                {
                    if (fundReq.Username.IsEmpty() || fundReq.Identitycard.IsEmpty())
                    {
                        Res.StatusDescription = "身份证号或公积金号不能为空";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                else
                {
                    if (fundReq.Password.IsEmpty())
                    {
                        Res.StatusDescription = "密码不能为空";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    if (fundReq.Identitycard.IsEmpty() && fundReq.LoginType == "2")
                    {
                        Res.StatusDescription = "身份证号不能为空";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    if (fundReq.Username.IsEmpty() && fundReq.LoginType != "2")
                    {
                        Res.StatusDescription = fundReq.LoginType == "3" ? "公积金账号不能为空" : "用户名不能为空";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                string custacno = fundReq.Username;
                string logontype = "1";
                string paperid = fundReq.Identitycard;
                if (custacno != null)
                {
                    if (custacno.IndexOf("-") != -1)
                    {
                        custacno = custacno.Substring(0, custacno.IndexOf('-'));
                    }
                    if (custacno.Length < 10)
                    {
                        custacno = "0000000000" + custacno;
                        custacno = custacno.Substring(custacno.Length - 10);
                    }
                    if (custacno.Length > 11)
                    {
                        logontype = "2";
                    }
                }
                #region 第一步，登录系统
                Url = "https://gr.szgjj.gov.cn/retail/service";
                if (fundReq.LoginType == "1")//未注册用户
                {
                    postdata = String.Format("service=com.jbsoft.i2hf.retail.services.UserLogon.unRegUserLogon&custacno={0}&paperid={1}&paperkind=A&logontype={3}&validateCode={2}", custacno, paperid, fundReq.Vercode, logontype);
                }
                else if (fundReq.LoginType == "2")//注册用户、身份证登录
                {
                    postdata = String.Format("service=com.jbsoft.i2hf.retail.services.UserLogon.beforRegUserLogon&singname={0}&login=003&pswd={1}&validateCode={2}", fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
                }
                else if (fundReq.LoginType == "3")//注册用户、公积金账号登录
                {
                    postdata = String.Format("service=com.jbsoft.i2hf.retail.services.UserLogon.beforRegUserLogon&singname={0}&login=002&pswd={1}&validateCode={2}", fundReq.Username, fundReq.Password, fundReq.Vercode);
                }
                else if (fundReq.LoginType == "4")//注册用户、用户名登录
                {
                    postdata = String.Format("service=com.jbsoft.i2hf.retail.services.UserLogon.beforRegUserLogon&singname={0}&login=001&pswd={1}&validateCode={2}", fundReq.Username.ToUrlEncode(), fundReq.Password, fundReq.Vercode);
                }
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Referer = baseUrl + "retail/internet",
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

                string errorInfo = CommonFun.GetMidStrByRegex(httpResult.Html, "错误信息</font>:", "<br />");
                if (!errorInfo.IsEmpty())
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string sid = string.Empty;
                if (fundReq.LoginType== "1")
                {
                    sid = CommonFun.GetMidStr(httpResult.Html, "window.parent.location='internet?sid=", "' + '");
                }
                else
                {
                    sid = CommonFun.GetMidStr(httpResult.Html, "internet?sid=", "&service=");
                }
                #endregion

                #region 第二步，查询个人基本信息

                Url = "https://gr.szgjj.gov.cn/retail/internet?sid=" + sid + "&service=com.jbsoft.i2hf.retail.services.UserAccService.getBaseAccountInfo&ts=" + DateTime.Now.Subtract(Convert.ToDateTime("1979-01-01")).TotalMilliseconds.ToString("f0") + "";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[1]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = reg.Replace( results[0],"");
                }
                else
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.IdentityCard = reg.Replace(results[0], "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.TotalAmount = reg.Replace(results[0], "").ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[4]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Status = reg.Replace(results[0], "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[6]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyName = reg.Replace(results[0], "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[7]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.Bank = reg.Replace(results[0], "");//开户银行
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[8]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.SalaryBase = reg.Replace(results[0], "").ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[9]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = reg.Replace(results[0], "").ToDecimal(0) / 100;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[10]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.BankCardNo = reg.Replace(results[0], "");//银行卡号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[1]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.Name = reg.Replace(results[0], "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[2]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = reg.Replace(results[0], "").ToDecimal(0) / 2;
                    Res.CompanyMonthPayAmount = Res.PersonalMonthPayAmount;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[3]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = reg.Replace(results[0], "").ToTrim("-");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[4]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.OpenTime = reg.Replace(results[0], "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[8]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = reg.Replace(results[0], "").ToDecimal(0) / 100;
                }
                #endregion

                #region 第三步，缴费明细
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = "https://gr.szgjj.gov.cn/retail/internet?sid=" + sid + "&service=com.jbsoft.i2hf.retail.services.UserAccService.getDetailAccountInfo&ts=" + DateTime.Now.Subtract(Convert.ToDateTime("1979-01-01")).TotalMilliseconds.ToString("f0") + "";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string acdateFrom = string.Empty;
                string acdateTo = string.Empty;
                int page = 0;//当前页
                int pages = 0;//总页数
                int total = 0;//明细记录条数
                referUrl = Url;
                DateTime nowDate = DateTime.Now;
                List<SuzhouDetail> details = new List<SuzhouDetail>();
                if (!string.IsNullOrWhiteSpace(Res.OpenTime))
                {
                    acdateFrom = Convert.ToDateTime(Res.OpenTime).ToString("yyyyMMdd HH:mm:ss");
                }
                else
                {
                    acdateFrom = nowDate.AddYears(-5).ToString("yyyyMMdd HH:mm:ss");
                }
                acdateTo = nowDate.ToString(Consts.DateFormatString5) + " 23:59:59";
                do
                {
                    page++;
                    postdata = string.Empty;
                    Url = "https://gr.szgjj.gov.cn/retail/internet?sid=" + sid;
                    if (page > 1)
                    {
                        postdata =
                            string.Format(
                                "service=com.jbsoft.i2hf.retail.services.UserAccService.getDetailAccountInfoJSON&acdateFrom={0}&acdateTo={1}&busidetailtype=&page={2}&pages={3}&total={4}&ts={5}",
                                acdateFrom, acdateTo, page, pages, total, DateTime.Now.Subtract(Convert.ToDateTime("1979-01-01")).TotalMilliseconds.ToString("f0"));
                    }
                    else
                    {
                        postdata = string.Format("service=com.jbsoft.i2hf.retail.services.UserAccService.getDetailAccountInfoJSON&acdateFrom={0}&acdateTo={1}&busidetailtype=&page={2}&ts={3}", acdateFrom, acdateTo, page, DateTime.Now.Subtract(Convert.ToDateTime("1979-01-01")).TotalMilliseconds.ToString("f0"));
                    }
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        Referer = referUrl,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    if (page == 1)
                    {
                        pages = jsonParser.GetResultFromMultiNode(httpResult.Html, "info:pages").ToInt(0);
                        total = jsonParser.GetResultFromMultiNode(httpResult.Html, "info:total").ToInt(0);
                    }
                    details.AddRange(jsonParser.DeserializeObject<List<SuzhouDetail>>(jsonParser.GetResultFromParser(httpResult.Html, "recoreds")));
                }
                while (page < pages);
               
                foreach (var item in details)
                {
                    detail = new ProvidentFundDetail();
                    detail.PayTime = item.acdate.ToDateTime(Consts.DateFormatString2);//支付时间
                    detail.ProvidentFundTime = string.IsNullOrEmpty(item.savemonth.ToTrim("-")) == true ? null : item.savemonth.ToTrim("-");//应属时间
                    detail.Description = item.busidetailtype;//描述
                    if(item.flag == "支出")
                    {
                        detail.PersonalPayAmount = reg.Replace(item.amt, "").ToDecimal(0);
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                    }
                    else if (item.busidetailtype.IndexOf("汇缴", StringComparison.Ordinal) > -1)
                    {
                        detail.PersonalPayAmount = reg.Replace(item.amt, "").ToDecimal(0) * perAccounting; //个人月缴额
                        detail.CompanyPayAmount = reg.Replace(item.amt, "").ToDecimal(0) * comAccounting; //公司月缴额
                        detail.ProvidentFundBase = (reg.Replace(item.amt, "").ToDecimal(0) / totalRate).ToString("f2").ToDecimal(0); //缴费基数
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        PaymentMonths++;
                    }
                    else
                    {
                        detail.PersonalPayAmount =new Regex(@"[\,\-\s]*").Replace(item.amt, "").ToDecimal(0);
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
    class SuzhouDetail
    {
        public string flag { get; set; }
        public string cancelacbooknum { get; set; }
        public string acculistid { get; set; }
        public string demandsavebal { get; set; }
        public string acid { get; set; }
        public string corpacno { get; set; }
        public string custname { get; set; }
        public string fixedsavebal { get; set; }
        public string note { get; set; }
        public string amt { get; set; }
        public string acdate { get; set; }
        public string custacno { get; set; }
        public string bankname { get; set; }
        public string busidetailtype { get; set; }
        public string acbooknum { get; set; }
        public string bal { get; set; }
        public string inte { get; set; }
        public string bankid { get; set; }
        public string savemonth { get; set; }
    }
}
