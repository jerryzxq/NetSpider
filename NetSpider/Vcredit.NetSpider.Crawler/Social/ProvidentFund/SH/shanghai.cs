using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using System.Security.Cryptography.X509Certificates;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SH
{
    public class shanghai : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "https://persons.shgjj.com/";
        //string baseUrl = "https://222.66.120.15/";
        string fundCity = "sh_shanghai";
        #endregion

        #region 私有变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        string Url = string.Empty;
        string postdata = string.Empty;
        decimal payRate = (decimal)0.07;
        decimal payReserveRate = (decimal)0.07;
        List<string> results = new List<string>();

        string ClearDiv(string input)
        {
            string output = CommonFun.ClearFlag(input).ToTrim("&nbsp;").ToTrim();
            if (input.StartsWith("<div"))
            {
                List<string> temp = HtmlParser.GetResultFromParser(input, "div");
                if (temp.Count > 0)
                {
                    output = CommonFun.ClearFlag(temp[0]).ToTrim("&nbsp;").ToTrim();
                }
                else
                {
                    output = string.Empty;
                }
            }
            return output;
        }

        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
                httpItem = new HttpItem()
                {
                    URL = baseUrl,
                    Host = "persons.shgjj.com",
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode == HttpStatusCode.BadGateway)
                {
                    if(httpResult.Html.Contains("temporarily unavailable"))
                    Res.StatusDescription = "上海公积金官网维护中，请稍后再试！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = baseUrl + "SsoLogin?url=" + baseUrl + "MainServlet?ID=1";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Host = "persons.shgjj.com",
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
                Url = baseUrl + "VerifyImageServlet";//http://persons.shgjj.com/VerifyImageServlet
                httpItem = new HttpItem()
                {
                    Accept = "image/png,image/*;q=0.8,*/*;q=0.5",
                    URL = Url,
                    Method = "get",
                    Host = "persons.shgjj.com",
                    UserAgent="Mozilla/5.0 (Windows NT 6.1; WOW64; rv:42.0) Gecko/20100101 Firefox/42.0",
                    Referer = baseUrl + "SsoLogin?url=" + baseUrl + "MainServlet?ID=1",
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
            ProvidentFundReserveRes Res_Reserve = new ProvidentFundReserveRes();
            ProvidentFundLoanRes Res_Loan = new ProvidentFundLoanRes();
            Res.ProvidentFundCity = fundCity;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(fundReq.Token);
                    SpiderCacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Username.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录
                Url = baseUrl + "SsoLogin";
                string passwordMD5 = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(fundReq.Password, "MD5"); ;
                postdata = String.Format("username={0}&password={1}&imagecode={2}&password_md5={3}&ID=0", fundReq.Username, fundReq.Password, fundReq.Vercode, passwordMD5);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "SsoLogin?url=" + baseUrl + "MainServlet?ID=1",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='para1']", "value");//解析html
                if (results.Count == 0)
                {
                    //goto Lable_Start;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//font[@color='#CC0000']");
                    if (results.Count > 0)
                    {
                        Res.StatusDescription = results[0];
                    }
                    else
                    {
                        Res.StatusDescription = "登录失败";
                    }
                    return Res;
                }
                #endregion

                #region 第二步，进入公积金SSO页面，初始化页面（主要是为获取cookie和下一步URL）

                Url = "http://bbs.shgjj.com/sso/sso.php?url=" + baseUrl + "MainServlet?ID=1";
                postdata = String.Format("para1={0}&para2={1}", fundReq.Username, passwordMD5);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 第三步，进入公积金SSO导航页
                Url = HtmlParser.GetResultFromParser(httpResult.Html, "//script[1]", "src")[0]; 
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 普通公积金
                #region 第四步，进入公积金首页 解析html，并对解析后的数据进行整理
                Url = baseUrl + "MainServlet?ID=1";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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

                Res.Name = CommonFun.GetMidStrByRegex(httpResult.Html, "姓 名</div></td><td width=\"751\">", "<strong>");//姓名

                if (Res.Name.IsEmpty())
                {
                    Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.IdentityCard = fundReq.Identitycard;
                Res.OpenTime = (DateTime.ParseExact(CommonFun.GetMidStrByRegex(httpResult.Html, "开户日期</div></td><td>", "</td>"), Consts.DateFormatString4, null)).ToString("yyyy-MM-dd");//开户日期
                Res.CompanyName = CommonFun.ClearFlag(CommonFun.GetMidStrByRegex(httpResult.Html, "所属单位</div></td><td>", "</td>")).ToTrim();//所属单位
                Res.ProvidentFundNo = CommonFun.ClearFlag(CommonFun.GetMidStrByRegex(httpResult.Html, "公积金账号</div></td><td>", "</td>"));//公积金账号
                Res.TotalAmount = CommonFun.GetMidStrByRegex(httpResult.Html, "账户余额</div></td><td>", "</td>").Replace("元", "").ToDecimal(0);//账户余额
                Res.Status = CommonFun.GetMidStrByRegex(httpResult.Html, "当前账户状态</div></td><td>", "<input").Replace(" ", "");//当前账户状态
                Res.PersonalMonthPayAmount = CommonFun.GetMidStrByRegex(httpResult.Html, "月缴存额</div></td><td>", "</td>").Replace("元", "").ToDecimal(0) / 2;//月缴存额
                Res.LastProvidentFundTime = CommonFun.GetMidStrByRegex(httpResult.Html, "末次缴存年月</div></td><td>", "</td>").Replace("年", "").Replace("月", "");//月缴存额

                Res.CompanyMonthPayAmount = Res.PersonalMonthPayAmount;
                Res.SalaryBase = (Res.CompanyMonthPayAmount / payRate).ToString("f2").ToDecimal(0);

                Url = baseUrl + "MainServlet?ID=10";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='mobile']", "value");
                if (results.Count > 0)
                {
                    Res.Phone = results[0];
                }
                #endregion

                #region 第五步，进入公积金缴费详情页，解析html，并对解析后的数据进行整理
                Url = baseUrl + "MainServlet?ID=11";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table']/tr[position() >2]", "");
                ProvidentFundDetail detail = null;

                int PaymentMonths = 0;
                string FirstYearMonth = DateTime.Now.ToString("yyyyMM");
                bool need_description = false;
                foreach (string strItem in results)
                {
                    var strDetail = HtmlParser.GetResultFromParser(strItem, "//div", "");
                    if (strDetail.Count < 5)
                    {
                        continue;
                    }
                    detail = new ProvidentFundDetail();
                    detail.CompanyName = strDetail[1];//缴费单位
                    detail.PayTime = DateTime.ParseExact(strDetail[0], Consts.DateFormatString4, null);//发生日期
                    if (strDetail[3].IndexOf("汇缴") != -1)//正常汇缴
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.ProvidentFundTime = strDetail[3].Substring(2, 4) + strDetail[3].Substring(7, 2);
                        detail.PersonalPayAmount = strDetail[2].ToDecimal(0) / 2;//金额
                        detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                        detail.ProvidentFundBase = (detail.PersonalPayAmount / payRate).ToString("f2").ToDecimal(0);//缴费基数
                        detail.Description = strDetail[3];
                        PaymentMonths++;
                    }
                    else if (strDetail[3].Contains("支取"))//支取
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                        detail.Description = strDetail[4].Replace("&nbsp;", "");
                        detail.PersonalPayAmount = strDetail[2].ToDecimal(0);//金额
                    }
                    else if (strDetail[3].IndexOf("font") != -1)//调整
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.Description = HtmlParser.GetResultFromParser(strItem, "//font", "")[0];
                        detail.PersonalPayAmount = strDetail[2].ToDecimal(0);//金额
                    }
                    else//其它
                    {
                        detail.PaymentFlag = strDetail[3];
                        detail.PaymentType = strDetail[3];
                        detail.PersonalPayAmount = strDetail[2].ToDecimal(0);//金额
                        detail.Description = strDetail[3];
                    }

                    if (strDetail[3].Contains("补缴"))//有补缴情况需提示
                    {
                        need_description = true;
                    }

                    if (!detail.ProvidentFundTime.IsEmpty() && detail.ProvidentFundTime.ToInt(0) < FirstYearMonth.ToInt(0))
                    {
                        FirstYearMonth = detail.ProvidentFundTime;
                    }
                    else if (((DateTime)detail.PayTime).ToString("yyyyMM").ToInt(0) < FirstYearMonth.ToInt(0))
                    {
                        FirstYearMonth =((DateTime)detail.PayTime).ToString("yyyyMM");
                    }

                    Res.ProvidentFundDetailList.Add(detail);
                }
                if (need_description)
                {
                    Res.Description = "有补缴，请人工校验";
                }
                #endregion

                #region 第六步，获取历年住房公积金
                int year = FirstYearMonth.Remove(4).ToInt(0);
                int month = FirstYearMonth.Substring(4).ToInt(0);
                if (month > 7)
                {
                    year++;
                }

                List<string> tempresults = new List<string>();
                while (year > DateTime.Now.Year - 5)
                {
                    year--;
                    Url = baseUrl + "MainServlet?ID=2";
                    postdata = "time=" + year.ToString();
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
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
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='persontable']//tr/td");

                    if (results.Count != 53)
                        continue;

                    for (int i = 1; i < 13; i++)
                    {
                        string payday = string.Empty;
                        int showyear = year;
                        if (i < 7)
                        {
                            showyear++;
                            tempresults = HtmlParser.GetResultFromParser(results[33 + i].Replace("&nbsp;", ""), "/div");
                        }
                        else
                        {
                            tempresults = HtmlParser.GetResultFromParser(results[15 + i].Replace("&nbsp;", ""), "/div");
                        }
                        if (tempresults.Count > 0)
                            payday = tempresults[0];
                        if (payday.IsEmpty() || DateTime.ParseExact(showyear.ToString() + "年" + payday, Consts.DateFormatString4, null).ToString("yyyyMM").ToInt(0) >= FirstYearMonth.ToInt(0))
                            continue;

                        detail = new ProvidentFundDetail();
                        detail.CompanyName = results[4];
                        detail.PayTime = DateTime.ParseExact(showyear.ToString() + "年" + payday, Consts.DateFormatString4, null);
                        detail.ProvidentFundTime = (showyear * 100 + i).ToString();
                        detail.PersonalPayAmount = results[14].ToDecimal(0) / 2; ;
                        detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                        detail.ProvidentFundBase = (detail.PersonalPayAmount / payRate).ToString("f2").ToDecimal(0);
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        Res.ProvidentFundDetailList.Add(detail);
                        PaymentMonths++;
                    }
                }

                #endregion
                #endregion

                #region 补充公积金
                #region 第七步，进入公积金首页 解析html，并对解析后的数据进行整理
                Url = baseUrl + "MainServlet?ID=3";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                string Name = CommonFun.GetMidStrByRegex(httpResult.Html, "姓 名</div></td><td width=\"751\">", "<strong>");//姓名
                int Reserver_PaymentMonths = 0;

                if (!Name.IsEmpty())//名字不为空则有补充公积金
                {

                    Res_Reserve.OpenTime = (DateTime.ParseExact(CommonFun.GetMidStrByRegex(httpResult.Html, "开户日期</div></td><td>", "</td>"), Consts.DateFormatString4, null)).ToString("yyyy-MM-dd");//开户日期
                    Res_Reserve.CompanyName = CommonFun.ClearFlag(CommonFun.GetMidStrByRegex(httpResult.Html, "所属单位</div></td><td>", "</td>")).ToTrim();//所属单位
                    Res_Reserve.ProvidentFundNo = CommonFun.ClearFlag(CommonFun.GetMidStrByRegex(httpResult.Html, "公积金账号</div></td><td>", "</td>"));//公积金账号
                    Res_Reserve.TotalAmount = CommonFun.GetMidStrByRegex(httpResult.Html, "账户余额</div></td><td>", "</td>").Replace("元", "").ToDecimal(0);//账户余额
                    Res_Reserve.Status = CommonFun.GetMidStrByRegex(httpResult.Html, "当前账户状态</div></td><td>", "<input").Replace(" ", "");//当前账户状态
                    Res_Reserve.PersonalMonthPayAmount = CommonFun.GetMidStrByRegex(httpResult.Html, "月缴存额</div></td><td>", "</td>").Replace("元", "").ToDecimal(0) / 2;//月缴存额
                    Res_Reserve.LastProvidentFundTime = CommonFun.GetMidStrByRegex(httpResult.Html, "末次缴存年月</div></td><td>", "</td>").Replace("年", "").Replace("月", "");//月缴存额

                    Res_Reserve.CompanyMonthPayAmount = Res_Reserve.PersonalMonthPayAmount;
                #endregion

                    #region 第八步，进入公积金缴费详情页，解析html，并对解析后的数据进行整理
                    Url = baseUrl + "MainServlet?ID=31";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
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

                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table']/tr[position() >2]", "");
                    detail = null;

                    FirstYearMonth = DateTime.Now.ToString("yyyyMM");
                    need_description = false;
                    foreach (string strItem in results)
                    {
                        var strDetail = HtmlParser.GetResultFromParser(strItem, "//div", "");
                        if (strDetail.Count < 5)
                        {
                            continue;
                        }
                        detail = new ProvidentFundDetail();
                        detail.CompanyName = strDetail[1];//缴费单位
                        detail.PayTime = DateTime.ParseExact(strDetail[0], Consts.DateFormatString4, null);//发生日期
                        if (strDetail[3].IndexOf("汇缴") != -1)//正常汇缴
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            detail.ProvidentFundTime = strDetail[3].Substring(2, 4) + strDetail[3].Substring(7, 2);
                            detail.PersonalPayAmount = strDetail[2].ToDecimal(0) / 2;//金额
                            detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                            detail.ProvidentFundBase =(detail.PersonalPayAmount / payReserveRate).ToString("f2").ToDecimal(0);//缴费基数
                            detail.Description = strDetail[3];
                            Reserver_PaymentMonths++;
                        }
                        else if (strDetail[3].Contains("支取"))//支取
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                            detail.Description = strDetail[4].Replace("&nbsp;", "");
                            detail.PersonalPayAmount = strDetail[2].ToDecimal(0);//金额
                        }
                        else if (strDetail[3].IndexOf("font") != -1)//调整
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.Description = HtmlParser.GetResultFromParser(strItem, "//font", "")[0];
                            detail.PersonalPayAmount = strDetail[2].ToDecimal(0);//金额
                        }
                        else//其它
                        {
                            detail.PaymentFlag = strDetail[3];
                            detail.PaymentType = strDetail[3];
                            detail.PersonalPayAmount = strDetail[2].ToDecimal(0);//金额
                            detail.Description = strDetail[3];
                        }

                        if (strDetail[3].Contains("补缴"))//有补缴情况需提示
                        {
                            need_description = true;
                        }

                        if (!detail.ProvidentFundTime.IsEmpty() && detail.ProvidentFundTime.ToInt(0) < FirstYearMonth.ToInt(0))
                        {
                            FirstYearMonth = detail.ProvidentFundTime;
                        }
                        else if (((DateTime)detail.PayTime).ToString("yyyyMM").ToInt(0) < FirstYearMonth.ToInt(0))
                        {
                            FirstYearMonth = ((DateTime)detail.PayTime).ToString("yyyyMM");
                        }

                        Res_Reserve.ProvidentReserveFundDetailList.Add(detail);
                    }
                    if (need_description)
                    {
                        Res_Reserve.Description = "有补缴，请人工校验";
                    }
                    #endregion

                    #region 第九步，获取历年补充公积金
                    year = FirstYearMonth.Remove(4).ToInt(0);
                    month = FirstYearMonth.Substring(4).ToInt(0);
                    if (month > 7)
                    {
                        year++;
                    }

                    tempresults = new List<string>();
                    while (year > DateTime.Now.Year - 5)
                    {
                        year--;
                        Url = baseUrl + "MainServlet?ID=4";
                        postdata = "time=" + year.ToString();
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "POST",
                            Postdata = postdata,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);

                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='persontable']//tr/td");

                        if (results.Count != 53)
                            continue;

                        for (int i = 1; i < 13; i++)
                        {
                            string payday = string.Empty;
                            int showyear = year;
                            if (i < 7)
                            {
                                showyear++;
                                tempresults = HtmlParser.GetResultFromParser(results[33 + i].Replace("&nbsp;", ""), "/div");
                            }
                            else
                            {
                                tempresults = HtmlParser.GetResultFromParser(results[15 + i].Replace("&nbsp;", ""), "/div");
                            }
                            if (tempresults.Count > 0)
                                payday = tempresults[0];
                            if (payday.IsEmpty() || DateTime.ParseExact(showyear.ToString() + "年" + payday, Consts.DateFormatString4, null).ToString("yyyyMM").ToInt(0) >= FirstYearMonth.ToInt(0))
                                continue;

                            detail = new ProvidentFundDetail();
                            detail.CompanyName = results[4];
                            detail.PayTime = DateTime.ParseExact(showyear.ToString() + "年" + payday, Consts.DateFormatString4, null);
                            detail.ProvidentFundTime = (showyear * 100 + i).ToString();
                            detail.PersonalPayAmount = results[14].ToDecimal(0) / 2; ;
                            detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                            detail.ProvidentFundBase = (detail.PersonalPayAmount / payReserveRate).ToString("f2").ToDecimal(0);//缴费基数
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            Res_Reserve.ProvidentReserveFundDetailList.Add(detail);
                            Reserver_PaymentMonths++;
                        }
                    }
                }
                #endregion

                #endregion

                #region 贷款
                #region 第十步，获取贷款基本信息
                Url = baseUrl + "MainServlet?ID=5";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table']");
                string _DetailUrl = string.Empty;
                foreach (string item in results)
                {
                    List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td");
                    if (tdRow.Count == 17 && tdRow[0].Contains("贷款账户号"))
                    {
                        tdRow = HtmlParser.GetResultFromParser(item, "//td/div");
                        if (tdRow.Count == 17)
                        {
                            Res_Loan.Account_Loan = CommonFun.GetMidStr(tdRow[1], "", "<a").ToTrim();
                            Res_Loan.Status = tdRow[3];
                            Res_Loan.Loan_Opening_Date = tdRow[5];
                            Res_Loan.Period_Total = tdRow[7];
                            Res_Loan.Repay_Type = tdRow[9];
                            Res_Loan.Loan_Credit = tdRow[11].Replace("元", "").ToTrim().ToDecimal(0);
                            Res_Loan.Loan_Balance = tdRow[13].Replace("元", "").ToTrim().ToDecimal(0);
                            Res_Loan.LatestRepayTime = tdRow[15];

                            List<string> urllist = new List<string>();
                            urllist = HtmlParser.GetResultFromParser(item, "//a", "href");
                            if (urllist.Count > 0)
                            {
                                _DetailUrl = baseUrl + urllist[0];
                            }
                        }
                    }
                    else if (tdRow.Count == 16 && tdRow[0].Contains("委托书编号"))
                    {
                        Res_Loan.Loan_Sid = ClearDiv(tdRow[1]);
                        Res_Loan.Loan_Start_Date = ClearDiv(tdRow[3]);
                        Res_Loan.Bank_Delegate = ClearDiv(tdRow[5]);
                        Res_Loan.Withdrawal_Type = ClearDiv(tdRow[7]);
                        Res_Loan.Loan_Type = ClearDiv(tdRow[9]);
                        Res_Loan.ProvidentFundNo = ClearDiv(tdRow[11]);
                        Res_Loan.Account_CommercialLoan = ClearDiv(tdRow[13]); 
                    }
                    else if (tdRow.Count > 0 && ClearDiv(tdRow[0]) == "还款顺序" && tdRow.Count % 4 == 0)
                    {
                        List<List<string>> JoinInfoList = new List<List<string>>();
                        for (int i = 1; i < (tdRow.Count / 4); i++)
                        {
                            List<string> JoinInfo = new List<string>();
                            for (int j = 0; j < 4; j++)
                            {
                                JoinInfo.Add(ClearDiv(tdRow[i * 4 + j]));
                            }
                            JoinInfoList.Add(JoinInfo);
                        }
                        for (int i = 0; i < JoinInfoList.Count; i++)
                        {
                            if (JoinInfoList[i][2] == "本人")
                            {
                                continue;
                            }
                            else
                            {
                                Res_Loan.JoinPerson += string.Format("[姓名：{0}，关系：{1}，签约日期：{2}]", JoinInfoList[i][1], JoinInfoList[i][2], JoinInfoList[i][3]);
                                if (JoinInfoList[i][2] == "配偶")
                                {
                                    Res_Loan.Couple_Name = JoinInfoList[i][1];
                                }
                            }
                        }
                    }
                }
                #endregion

                #region 第十一步，获取贷款明细
                if (!_DetailUrl.IsEmpty())
                {
                    Url = _DetailUrl;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table']/tr");
                    bool has_overdue = false;
                    bool has_penalty = false;
                    if (results.Count > 2)
                    {
                        results.RemoveRange(0, 2);

                        foreach (string item in results)
                        {
                            List<string> _detail_loan = HtmlParser.GetResultFromParser(item, "/td/div");
                            if (_detail_loan.Count != 5)
                                continue;

                            string Record_Month = _detail_loan[1].Replace("年", "").Replace("月", "").Replace("&nbsp;","");

                            ProvidentFundLoanDetail _ProvidentFundLoanDetail = Res_Loan.ProvidentFundLoanDetailList.Where(o => o.Record_Month == Record_Month && !o.Record_Month.IsEmpty()).FirstOrDefault();

                            bool NeedAdd = false;
                            if (_ProvidentFundLoanDetail == null)
                            {
                                _ProvidentFundLoanDetail = new ProvidentFundLoanDetail();
                                _ProvidentFundLoanDetail.Record_Date = _detail_loan[0];
                                _ProvidentFundLoanDetail.Record_Month = Record_Month;
                                NeedAdd = true;
                            }

                            if (_detail_loan[4].Contains("正常"))
                            {
                                if (Res_Loan.Current_Repay_Date.IsEmpty())
                                {
                                    Res_Loan.Current_Repay_Date = CommonFun.GetMidStr(_detail_loan[0], "月", "日");//还款日
                                }
                            }

                            if (_detail_loan[4].Contains("逾期"))
                            {
                                _ProvidentFundLoanDetail.Overdue_Principal += _detail_loan[2].ToDecimal(0);
                                _ProvidentFundLoanDetail.Overdue_Interest += _detail_loan[3].ToDecimal(0);
                                has_overdue = true;
                            }
                            else if (_detail_loan[4].Contains("罚息"))
                            {
                                _ProvidentFundLoanDetail.Interest_Penalty += _detail_loan[2].ToDecimal(0) + _detail_loan[3].ToDecimal(0);
                                has_penalty = true;
                            }
                            else
                            {
                                _ProvidentFundLoanDetail.Principal += _detail_loan[2].ToDecimal(0);
                                _ProvidentFundLoanDetail.Interest += _detail_loan[3].ToDecimal(0);
                            }

                            _ProvidentFundLoanDetail.Base += _detail_loan[2].ToDecimal(0) + _detail_loan[3].ToDecimal(0);

                            if (!_ProvidentFundLoanDetail.Description.IsEmpty() && !_detail_loan[4].IsEmpty())
                            {
                                _ProvidentFundLoanDetail.Description += ";";
                            }
                            _ProvidentFundLoanDetail.Description += _detail_loan[4];

                            if (NeedAdd)
                            {
                                Res_Loan.ProvidentFundLoanDetailList.Add(_ProvidentFundLoanDetail);
                            }
                        }
                    }
                    if (has_overdue || has_penalty)
                    {
                        Res_Loan.Description = (has_overdue ? "有逾期，" : "") + (has_penalty ? "有罚息，" : "") + "请人工校验";
                    }
                }
                #endregion
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res_Reserve.PaymentMonths = Reserver_PaymentMonths;
                Res.ProvidentFundReserveRes = Res_Reserve;
                Res.ProvidentFundLoanRes = Res_Loan;
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
        /// <summary>
        /// 解决：基础连接已经关闭: 未能为SSL/TLS 安全通道建立信任关系
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private bool ValidateServerCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }

}
