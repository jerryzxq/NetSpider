using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.LN
{
    public class fushun : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.lnfssi.gov.cn/";
        string socialCity = "ln_fushun";
        private string cookieStr = string.Empty;
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            string Url = string.Empty;
            Res.Token = token;
            try
            {
                Url = baseUrl + "fssi//login.do?method=begin&type=emp";
                httpItem = new HttpItem()
                {
                    URL = baseUrl,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("UTF-8"),
                    Referer = baseUrl,
                    //CookieCollection = cookies,
                    //ResultCookieType = ResultCookieType.CookieCollection
                    Cookie = cookieStr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                // cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                //CacheHelper.SetCache(token, cookies);
                CacheHelper.SetCache(token, cookieStr);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitError;
                Log4netAdapter.WriteError(socialCity + ServiceConsts.SocialSecurity_InitError, e);
            }
            return Res;
        }

        public SocialSecurityQueryRes SocialSecurityQuery(SocialSecurityReq socialReq)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = socialCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookieStr = (string)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录
                Url = baseUrl + "fssi/login.do";
                postdata = String.Format("method=fsLogin&type=emp&IDType=aac002&j_username={0}&j_username1={1}&j_password={2}", socialReq.Identitycard, socialReq.Identitycard, socialReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Encoding = Encoding.GetEncoding("GB2312"),
                    Postdata = postdata,
                    //CookieCollection = cookies,
                    //ResultCookieType = ResultCookieType.CookieCollection
                    Cookie = cookieStr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string errorflag = string.Empty;
                string errormsg = string.Empty;
                errorflag = HtmlParser.GetResultFromParser(httpResult.Html, "//title", "inner")[0];
                errormsg = CommonFun.GetMidStr(httpResult.Html, "errmsg=\"", "\"");
                if (!string.IsNullOrEmpty(errorflag) && errorflag == "错误信息")
                {
                    Res.StatusDescription = errormsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }

                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                #endregion

                #region 获取基本信息

                //选择养老保险查询
                Url = baseUrl + "fssi/query.do?method=gr_yanglao_main";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Get",
                    Encoding = Encoding.GetEncoding("GB2312"),
                    Cookie = cookieStr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);

                //养老保险个人账户信息
                Url = baseUrl + "fssi/query.do?method=yanglao_zh";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Get",
                    Encoding = Encoding.GetEncoding("GB2312"),
                    Cookie = cookieStr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);


                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='90%']/tr/td", "inner");
                Res.Name = HtmlParser.GetResultFromParser(results[0], "//strong/font", "inner")[0].Split('：')[1];   //姓名
                Res.EmployeeNo = HtmlParser.GetResultFromParser(results[1], "//strong/font", "inner")[0].Split('：')[1];   //个人编号
                Res.IdentityCard = HtmlParser.GetResultFromParser(results[2], "//strong/font", "inner")[0].Split('：')[1];   //身份证号
                Res.CompanyNo = HtmlParser.GetResultFromParser(results[3], "//strong/font", "inner")[0].Split('：')[1];   //单位编号


                Url = "http://www.lnfssi.gov.cn/fssi/query.do?method=unitinfoquery&type=" + Res.CompanyNo;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Get",
                    Encoding = Encoding.GetEncoding("GB2312"),
                    Cookie = cookieStr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                //单位状态
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='42']", "value");
                if (results.Count >= 2)
                {
                    var companystatus = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='42']", "value")[1];
                    if (!string.IsNullOrEmpty(companystatus))
                    {
                        if (companystatus == "1")
                        {
                            Res.CompanyStatus = "登记在册";
                        }
                        else if (companystatus == "2")
                        {
                            Res.CompanyStatus = "破产";
                        }
                        else if (companystatus == "3")
                        {
                            Res.CompanyStatus = "注销";
                        }
                        else
                        {
                            Res.CompanyStatus = "分立";
                        }
                    }

                }

                Url = "http://www.lnfssi.gov.cn/fssi/query.do?method=person_canbao";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Get",
                    Encoding = Encoding.GetEncoding("GB2312"),
                    Cookie = cookieStr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                //公司名称
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='10']", "value");
                if (results.Count >= 2)
                {
                    var companyName = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='10']", "value")[1];
                    if (!string.IsNullOrEmpty(companyName))
                    {
                        Res.CompanyName = companyName;
                    }
                }
                //工作日期
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='8']", "value");
                if (results.Count >= 2)
                {
                    var wokedate = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='8']", "value")[1];
                    if (!string.IsNullOrEmpty(wokedate))
                    {
                        Res.WorkDate = wokedate;
                    }
                }
                //出生日期
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='7']", "value");
                if (results.Count >= 2)
                {
                    var birthdate = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='7']", "value")[1];
                    if (!string.IsNullOrEmpty(birthdate))
                    {
                        Res.BirthDate = birthdate;
                    }
                }

                //人员状态
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='9']", "value");
                if (results.Count >= 2)
                {
                    var employeestatus = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='9']", "value")[1];
                    if (!string.IsNullOrEmpty(employeestatus))
                    {
                        if (employeestatus == "1")
                        {
                            Res.EmployeeStatus = "在职参保";
                        }
                        else if (employeestatus == "2")
                        {
                            Res.EmployeeStatus = "离退休";
                        }
                        else if (employeestatus == "3")
                        {
                            Res.EmployeeStatus = "享受一次性待遇的退休";
                        }
                        else if (employeestatus == "4")
                        {
                            Res.EmployeeStatus = "终止保险关系";
                        }
                        else
                        {
                            Res.EmployeeStatus = "已注销";
                        }
                    }
                }

                //性别
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='5']", "value");
                if (results.Count >= 2)
                {
                    var sex = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='5']", "value")[1];
                    if (!string.IsNullOrEmpty(sex))
                    {
                        if (sex == "1")
                        {
                            Res.Sex = "男";
                        }
                        else
                        {
                            Res.Sex = "女";
                        }
                    }
                }

                #region 公司类型
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='11']", "value");
                if (results.Count >= 2)
                {
                    var companyType = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='11']", "value")[1];
                    if (!string.IsNullOrEmpty(companyType))
                    {
                        switch (companyType)
                        {
                            case "00":
                                Res.CompanyType = "个体虚拟单位";
                                break;
                            case "01":
                                Res.CompanyType = "个体4050虚拟单位";
                                break;
                            case "02":
                                Res.CompanyType = "下岗4050虚拟单位";
                                break;
                            case "10":
                                Res.CompanyType = "企业";
                                break;
                            case "20":
                                Res.CompanyType = "事业单位";
                                break;
                            case "21":
                                Res.CompanyType = "全额拨款事业单位";
                                break;
                            case "22":
                                Res.CompanyType = "差额拨款事业单位";
                                break;
                            case "23":
                                Res.CompanyType = "自收自支事业单位";
                                break;
                            case "24":
                                Res.CompanyType = "其他事业单位";
                                break;
                            case "30":
                                Res.CompanyType = "机关";
                                break;
                            case "40":
                                Res.CompanyType = "社会团体";
                                break;
                            case "50":
                                Res.CompanyType = "民办非企业单位";
                                break;
                            case "60":
                                Res.CompanyType = "城镇个体工商户";
                                break;
                            case "71":
                                Res.CompanyType = "社区";
                                break;
                            case "72":
                                Res.CompanyType = "学校";
                                break;
                            case "80":
                                Res.CompanyType = "农民";
                                break;
                            case "90":
                                Res.CompanyType = "其他";
                                break;
                        }

                    }
                }
                #endregion

                Url = "http://www.lnfssi.gov.cn/fssi/query.do?method=yanglao_zh";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Get",
                    Encoding = Encoding.GetEncoding("GB2312"),
                    Cookie = cookieStr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                //缴费月数
                int nowMonthCount = 0;  //养老本年缴费月数 
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='8']", "value");
                if (results.Count > 2)
                {
                    var count = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='8']", "value")[1];
                    if (!string.IsNullOrEmpty(count))
                    {
                        nowMonthCount = int.Parse(count);
                    }
                }
                int lastMonthCount = 0;  //截止上年末累计缴费月数 
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='7']", "value");
                if (results.Count > 2)
                {
                    var count = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='7']", "value")[1];
                    if (!string.IsNullOrEmpty(count))
                    {
                        lastMonthCount = int.Parse(count);
                    }
                }

                int addMonthCount = 0;  //本年补缴历年缴费月数
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='9']", "value");
                if (results.Count > 2)
                {
                    var count = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='9']", "value")[1];
                    if (!string.IsNullOrEmpty(count))
                    {
                        addMonthCount = int.Parse(count);
                    }
                }

                Res.PaymentMonths = nowMonthCount + lastMonthCount + addMonthCount;  //养老保险视同缴费月数


                //账户总额
                decimal nowInterest = 0; //本年利息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='10']", "value");
                if (results.Count > 2)
                {
                    var count = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='10']", "value")[1];
                    if (!string.IsNullOrEmpty(count))
                    {
                        nowInterest = count.ToDecimal(0);
                    }
                }

                decimal nowAmonth = 0; //本年帐户总金额
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='11']", "value");
                if (results.Count > 2)
                {
                    var count = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='11']", "value")[1];
                    if (!string.IsNullOrEmpty(count))
                    {
                        nowAmonth = count.ToDecimal(0);
                    }
                }

                decimal lastyearAmonth = 0; //上年结转
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='12']", "value");
                if (results.Count > 2)
                {
                    var count = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='12']", "value")[1];
                    if (!string.IsNullOrEmpty(count))
                    {
                        lastyearAmonth = count.ToDecimal(0);
                    }
                }
                Res.InsuranceTotal = nowInterest + nowAmonth + lastyearAmonth;  //账户总额

                Url = "http://www.lnfssi.gov.cn/fssi/query.do?method=examine";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Get",
                    Encoding = Encoding.GetEncoding("GBK"),
                    Cookie = cookieStr,
                    ResultCookieType = ResultCookieType.String
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                //个人账户总额
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='1']", "value");
                if (results.Count > 2)
                {
                    var count = HtmlParser.GetResultFromParser(httpResult.Html, "//attribute[@index='1']", "value")[1];
                    if (!string.IsNullOrEmpty(count))
                    {
                        Res.PersonalInsuranceTotal = count.ToDecimal(0);
                    }
                }
                string dwid_list_ic01_sum = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='dwid_list.ic01_sum']", "value");
                if (results.Count > 0)
                {
                    dwid_list_ic01_sum = results[0];
                }
                string dwUpdateXml_list_ic01_sum = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='dwUpdateXml_list.ic01_sum']", "value");
                if (results.Count > 0)
                {
                    dwUpdateXml_list_ic01_sum = results[0];//
                }
                string dwQueryXml_list_ic01_sum = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='dwQueryXml_list.ic01_sum']", "value");
                if (results.Count > 0)
                {
                    dwQueryXml_list_ic01_sum = results[0];//
                }
                string dwid_list_ic01_jf = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='dwid_list.ic01_jf']", "value");
                if (results.Count > 0)
                {
                    dwid_list_ic01_jf = results[0];
                }
                string dwUpdateXml_list_ic01_jf = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='dwUpdateXml_list.ic01_jf']", "value");
                if (results.Count > 0)
                {
                    dwUpdateXml_list_ic01_jf = results[0];//
                }
                string dwQueryXml_list_ic01_jf = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='dwQueryXml_list.ic01_jf']", "value");
                if (results.Count > 0)
                {
                    dwQueryXml_list_ic01_jf = results[0];//
                }
                string dwid_list_examine = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='dwid_list.examine']", "value");
                if (results.Count > 0)
                {
                    dwid_list_examine = results[0];
                }
                string dwQueryXml_list_examine = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//xml[@id='dwXml_list.examine']/datawindow/filters", "inner");
                if (results.Count > 0)
                {
                    dwQueryXml_list_examine = "<filters logicalmean=\"并且\" pattern=\"$1$ AND $2$ AND $3$ AND $4$\">" + results[0] + "</filters>";
                }
                #endregion
                #region 查询明细(获取不到,在IE模式下切换到兼容视图)
               // 案例：姓名：丛德华 身份证：210623680405382 密码：405382
                Url = "http://www.lnfssi.gov.cn/fssi/DataWindowMgrAction.do?method=query&isPartlyRefresh=true";
                StringBuilder str = new StringBuilder();
                str.Append("method=&FORWARD_NAME_IN_PARTLY_REFRESH=&dwNames=");
                str.Append("&editer_list.ic01_sum_3=278&editer_list.ic01_sum_4=278&editer_list.ic01_sum_1=53368.68&editer_list.ic01_sum_2=7873.50");
                str.Append("&dwid_list.ic01_sum=");
                str.Append(dwid_list_ic01_sum);
                str.Append("&dwUpdateXml_list.ic01_sum=");
                str.Append(dwUpdateXml_list_ic01_sum);
                str.Append("&dwQueryXml_list.ic01_sum=");
                str.Append(dwQueryXml_list_ic01_sum);
                str.Append("&checkbox_list.ic01_jf=checked&=1&turn_pages_list.ic01_jf=1");
                str.Append("&dwid_list.ic01_jf=");
                str.Append(dwid_list_ic01_jf);
                str.Append("&dwUpdateXml_list.ic01_jf=");
                str.Append(dwUpdateXml_list_ic01_jf);
                str.Append("&dwQueryXml_list.ic01_jf=");
                str.Append(dwQueryXml_list_ic01_jf);
                str.Append("&turn_pages_list.examine=1");
                str.Append("&dwid_list.examine=");
                str.Append(dwid_list_examine);
                str.Append("&dwUpdateXml_list.examine=");
                str.Append("&dwQueryXml_list.examine=");
                str.Append(dwQueryXml_list_examine.Replace("\n", ""));//FileOperateHelper.ReadFile(@"C:\Users\lihailin\Desktop\lalala.txt")
                str.Append("&&dwName=list.examine");
                postdata = str.ToString();
                httpItem = new HttpItem()
                           {
                               Accept = "*/*",
                               URL = Url,
                               Method = "Post",
                               Referer = "http://www.lnfssi.gov.cn/fssi/query.do?method=examine",
                               Postdata = postdata,
                               UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/7.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.3; .NET4.0C; .NET4.0E)",
                               Host = "www.lnfssi.gov.cn",
                               Cookie = cookieStr,
                               ResultCookieType = ResultCookieType.String
                           };
                httpResult = httpHelper.GetHtml(httpItem);
                cookieStr = CommonFun.GetCookieStringNew(cookieStr, httpResult.Cookie);
                #endregion
                //Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(socialCity + ServiceConsts.SocialSecurity_QueryError, e);
            }
            return Res;
        }

    }
}
