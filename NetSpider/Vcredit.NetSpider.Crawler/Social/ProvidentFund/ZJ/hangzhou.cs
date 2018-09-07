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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.ZJ
{
    public class hangzhou : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.hzgjj.gov.cn:8080/";
        string fundCity = "zj_hangzhou";
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
                Url = "http://www.hzgjj.gov.cn:8080/WebAccounts/pages/per/perLogin.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@name='userLoginForm']", "action");
                if (results.Count == 0)
                {
                    Res.StatusDescription = "杭州公积金页面初始化失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }


                Url = "http://www.hzgjj.gov.cn:8080/WebAccounts/codeMaker?t=";//Thu%20Sep%2010%202015%2017:12:51%20GMT+0800
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
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(fundReq.Token);
                    SpiderCacheHelper.RemoveCache(fundReq.Token);
                }
                string user_type = fundReq.LoginType;
                //校验参数
                if (fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (user_type == "1" && string.IsNullOrWhiteSpace(fundReq.Username))
                {
                    Res.StatusDescription = "客户号不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (user_type == "3" && string.IsNullOrWhiteSpace(fundReq.Username))
                {
                    Res.StatusDescription = "用户名不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录系统

                Url = baseUrl + string.Format("WebAccounts/userLogin.do?cust_no={0}&password={1}&validate_code={2}&cust_type={3}&user_type={4}", fundReq.Username, fundReq.Password, fundReq.Vercode, "2", user_type);
                postdata = string.Format("cust_no={0}", fundReq.Username);
                int index = 0;
                do
                {
                    index++;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Encoding = Encoding.GetEncoding("utf-8"),
                        Postdata = postdata,
                        Referer = baseUrl + "WebAccounts/pages/per/login.jsp",
                        CookieCollection = cookies,
                        Timeout = 6000,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (index > 3)
                    {
                        break;
                    }
                } while (httpResult.Html == "操作超时" || httpResult.Html == "The operation has timed out");
                if (httpResult.Html != "1")
                {
                    switch (httpResult.Html)
                    {
                        case "2":
                            Res.StatusDescription = "验证码不正确,请重新录入";
                            break;
                        case "-1":
                            Res.StatusDescription = "输入登录名或密码不正确,请重新录入";
                            break;
                        case "3":
                            Res.StatusDescription = "用户被停用，若有疑问请咨询公积金管理中心";
                            break;
                        case "5":
                            Res.StatusDescription = "您市民邮箱的个人信息与中心不符";
                            break;
                        case "6":
                            Res.StatusDescription = "市民邮箱接口错误";
                            break;
                        case "7":
                            Res.StatusDescription = "输入的市民邮箱或密码错误,不能登录";
                            break;
                        case "8":
                            Res.StatusDescription = "您市民邮箱的个人姓名不正确";
                            break;
                        case "9":
                            Res.StatusDescription = "您市民邮箱的个人身份证号码不正确";
                            break;
                        case "10":
                            Res.StatusDescription = "该用户在业务系统中存在重复注册的情况，请选择其他方式登录";
                            break;
                        case "11":
                            Res.StatusDescription = "尊敬的客户，您的用户名有重复，请以公积金个人客户号登录，并修改用户名，谢谢！";
                            break;
                        case "操作超时":
                            Res.StatusDescription = "请求超时,请稍后再试";
                            break;
                        default://The operation has timed out
                            Res.StatusDescription = "操作超时,请稍后再试";
                            break;
                    }
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }


                Url = "http://www.hzgjj.gov.cn:8080/WebAccounts/userLogin.do";
                postdata = String.Format("cust_type=2&flag=1&user_type_2={3}&user_type={3}&cust_no={0}&password={1}&validate_code={2}", fundReq.Username, fundReq.Password, fundReq.Vercode, user_type);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Encoding = Encoding.GetEncoding("GBK"),
                    Postdata = postdata,
                    Referer = baseUrl + "WebAccounts/pages/per/login.jsp",
                    CookieCollection = cookies,
                    Timeout = 5000,
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
                Res.Name = CommonFun.GetMidStr(httpResult.Html, "欢迎您，", "（客户号");
                #endregion

                #region 第二步，查询个人基本信息

                Url = "http://www.hzgjj.gov.cn:8080/WebAccounts/perComInfo.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='BStyle_TB']/tr[position()>2]", "inner");
                if (results.Count == 0)
                {
                    Res.StatusDescription = "无缴费信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (results.Count == 1)
                {//单账号
                    var Rows = HtmlParser.GetResultFromParser(results[0], "//td", "text", true);
                    if (Rows.Count > 8)
                    {
                        Res.ProvidentFundNo = Rows[1];//公积金账号
                        Res.CompanyName = Rows[3];//公司名称
                        Res.CompanyMonthPayAmount = Rows[4].ToDecimal(0);
                        Res.PersonalMonthPayAmount = Rows[5].ToDecimal(0);
                        Res.Status = Rows[7];//状态
                    }
                }
                else
                {//多账号
                    List<string> fundNoList = new List<string>();//资金性质为:住房公积金
                    bool flag = false;//是否有正常状态账号，默认否（false）
                    //取资金性质为:住房公积金,状态为：正常
                    foreach (var item in results)
                    {
                        var Rows = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        if (Rows.Count > 8)
                        {
                            if (Rows[2] == "住房公积金")
                            {
                                fundNoList.Add(item);
                                if (Rows[7] == "正常")
                                {
                                    flag = true;
                                    Res.ProvidentFundNo = Rows[1];//公积金账号
                                    Res.CompanyName = Rows[3];//公司名称
                                    Res.CompanyMonthPayAmount = Rows[4].ToDecimal(0);
                                    Res.PersonalMonthPayAmount = Rows[5].ToDecimal(0);
                                    Res.Status = Rows[7];//状态
                                }
                            }
                        }
                    }
                    //无正常状态账号,则选取资金性质为:住房公积金,列表最后一条
                    if (flag == false)
                    {
                        var Rows = HtmlParser.GetResultFromParser(fundNoList[fundNoList.Count - 1], "//td", "text", true);
                        if (Rows.Count > 8)
                        {
                            Res.ProvidentFundNo = Rows[1];//公积金账号
                            Res.CompanyName = Rows[3];//公司名称
                            Res.CompanyMonthPayAmount = Rows[4].ToDecimal(0);
                            Res.PersonalMonthPayAmount = Rows[5].ToDecimal(0);
                            Res.Status = Rows[7];//状态
                        }
                    }
                }

                #endregion
                #region 第二步，查询个人基本信息

                Url = "http://www.hzgjj.gov.cn:8080/WebAccounts/perComInfo.do?flag=1";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/table[1]/tr[position()>1]", "inner");
                foreach (var item in results)
                {
                    //匹配到要查看的公积金账号
                    if (item.Contains(Res.ProvidentFundNo))
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "/td[4]/div", "text");
                        if (tdRow.Count > 0)
                        {
                            Res.TotalAmount = tdRow[0].ToDecimal(0);//账户总额
                        }
                        Res.IdentityCard = CommonFun.GetMidStr(httpResult.Html, "cert_code=", "&");//身份证号
                    }
                }

                #endregion
                #region 第三步，缴费明细
                results.Clear();
                bool isFinish = false;
                int year = DateTime.Now.Year;
                Url = "http://www.hzgjj.gov.cn:8080/WebAccounts/perBillDetial.do";
                do
                {
                    postdata = string.Format("check_ym={0}&button1=+%B2%E9%D1%AF+&acct_no={1}&cacct_no=&cert_code={2}&fund_type={3}&cname=&flag=0&begin_date={0}0101&end_date={0}1231",year,Res.ProvidentFundNo,Res.IdentityCard,"10");
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata=postdata,
                        Encoding=Encoding.GetEncoding("gbk"),
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (httpResult.StatusCode != HttpStatusCode.OK || httpResult.Html.Contains("无个人账务信息"))
                    {
                        isFinish = true;
                    }
                    else
                    {
                        results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='BStyle_TB']/tr[position()>2]", "inner"));
                    }
                    year--;
                }
                while (!isFinish);
                Regex regex = new Regex(@"[^\d]*");
                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow.Count != 6)
                    {
                        continue;
                    }
                    detail = new ProvidentFundDetail();
                    detail.Description = tdRow[2];
                    detail.PayTime = tdRow[2].ToDateTime(Consts.DateFormatString7);
                    if (tdRow[2].IndexOf("汇缴", StringComparison.Ordinal) == 0)
                    {
                        detail.ProvidentFundTime = regex.Replace(tdRow[2],"");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = detail.CompanyPayAmount = Math.Round(tdRow[3].ToDecimal(0)/2,2);//金额
                        detail.ProvidentFundBase = Math.Round(detail.PersonalPayAmount / payRate,2);//缴费基数
                        PaymentMonths++;
                    }
                    else if (tdRow[2] == "补缴")
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0);//金额
                    }
                    else if (tdRow[2] == "提取")
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = tdRow[4].ToDecimal(0);//金额
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = Math.Abs(tdRow[3].ToDecimal(0) - tdRow[4].ToDecimal(0));//金额
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion

                #region 退出
                //Url = "http://www.hzgjj.gov.cn:8080/WebAccounts/logout.do?subsysId=02";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "get",
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                cookies = null;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = e.Message;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
        /// <summary>
        /// 基础链接已关闭
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
