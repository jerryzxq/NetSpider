using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Ext;
using System.Collections;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using System.Web;
using System.Text.RegularExpressions;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HN
{
    /// <summary>
    /// 未提供公积金明细查询,密码为: 1' or '1' = '1(最前面的1之前有个空格)
    /// </summary>
    public class yiyang  : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.yygjj.com/";
        string fundCity = "hn_yichang";
        int PaymentMonths = 0;
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {


                //获取时间戳
                DateTime d = DateTime.Now;
                var date = d.ToString("r") + d.ToString("zzz").Replace(":", "");
                date=date.Replace(", ", " ");
                date = ReplaceString(date, 1, 2);
                Url = baseUrl + "jcaptcha?onlynum=true&radom=" + date;
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
                    vcRes.StatusDescription = ServiceConsts.StatusDescription_httpfail;

                    vcRes.StatusCode = ServiceConsts.StatusCode_fail;
                    return vcRes;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                vcRes.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                vcRes.VerCodeUrl = CommonFun.GetVercodeUrl(token);

                vcRes.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                vcRes.StatusCode = ServiceConsts.StatusCode_error;
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return vcRes;
        }

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            List<string> results = new List<string>();
            Res.ProvidentFundCity = fundCity;
            string url = string.Empty;
            string postData = string.Empty;
            decimal providentfundbase = (decimal)0;
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
                if (regex.IsMatch(fundReq.Identitycard) == false)
                {
                    Res.StatusDescription = "请输入有效身份证号";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (fundReq.Vercode.IsEmpty())
                {
                    Res.StatusDescription = "验证码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,验证登陆
                url = baseUrl + "searchPersonLogon.do?logon="+new Random().Next(1,100000000);
                postData = string.Format(@"spidno={0}&spwd={1}&sprand={2}", fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "Post",
                    Postdata = postData,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryError;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "<script> alert('", "'); window.location.href='index.jsp'</script>");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //存储cookie
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //跳转登陆页面
               
               
                #endregion
                #region 第二步,获取基本信息
                DateTime d = DateTime.Now;
                var date = d.ToString("r") + d.ToString("zzz");
                url = baseUrl + "searchGrye.do?logon=" + date;
                httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "Get",
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



                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[1]/td[1]", "inner");
                if (results.Count>0)
                {
                    Res.Name = results[0];       //职工姓名
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[1]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];       //身份证号码
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[2]/td[1]", "text");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];       //职工帐号
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];       //公司名称
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[3]/td[1]", "text");
                if (results.Count > 0)
                {
                    providentfundbase = results[0].Split('&')[0].ToDecimal(0);       //缴费基数
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = results[0].Replace("%","").ToDecimal(0)/100;       //个人缴费比率
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = results[0].Replace("%", "").ToDecimal(0)  / 100;       //公司缴费比率
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[4]/td[1]", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].Split('&')[0].ToDecimal(0)/2;       //个人缴费
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[4]/td[1]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = results[0].Split('&')[0].ToDecimal(0) / 2;       //公司月缴费
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[4]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].Split('&')[0].ToDecimal(0);       //公积金余额
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[5]/td[1]", "text");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0].Split(';')[1];       //最新汇缴年月
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[5]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Status = results[0].Split(';')[1];       //账号状态
                }

                Res.SalaryBase = Math.Round(Res.PersonalMonthPayAmount / Res.PersonalMonthPayRate,2);          //薪资


                #endregion
                #region ===第三步，获取详细信息===
                url = baseUrl + "searchGrmx.do?logon=" + d;
                httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "Get",
                    Encoding = Encoding.GetEncoding("UTF-8"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@name='select' ]/option", "text");

                foreach (var item in results)
                {
                    url = baseUrl + "searchGrmx.do?year=" + item;
                    httpItem = new HttpItem()
                    {
                        URL = url,
                        Method = "Get",
                        Encoding = Encoding.GetEncoding("UTF-8"),
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    if (httpResult.StatusCode != HttpStatusCode.OK)
                    {
                        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                        Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                        return Res;
                    }
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju']/tr[position()>1]", "inner");
                    foreach (var result in results)
                    {
                        ProvidentFundDetail providentfounddetail = new ProvidentFundDetail();
                        List<string> tdRow = HtmlParser.GetResultFromParser(result, "//td", "text", true);
                        if (tdRow.Count != 8)
                        {
                            continue;
                        }

                        providentfounddetail.Description = tdRow[3].Split(';')[1];//描述
                       // providentfounddetail.PayTime = tdRow[2].Split(';')[1].Insert(4,"-").ToDateTime();//缴费年月
                        string s = tdRow[2].Split(';')[1].Insert(4, "-");
                        providentfounddetail.PayTime = Regex.Replace(tdRow[1], @"[/\&nbsp;\s]", "").ToDateTime();
                        providentfounddetail.ProvidentFundTime = Regex.Replace(tdRow[2], @"[/\&nbsp;\s]", "");
                        //if (tdRow[5].IndexOf("汇缴", System.StringComparison.Ordinal) > -1)
                        //{
                        //    providentfounddetail.PersonalPayAmount = decimal.Truncate(tdRow[4].ToDecimal(0) * (Res.PersonalMonthPayRate / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)));//个人缴费金额
                        //    providentfounddetail.CompanyPayAmount = decimal.Truncate(tdRow[4].ToDecimal(0) * (Res.CompanyMonthPayRate / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)));//企业缴费金额
                        //    providentfounddetail.ProvidentFundBase = providentfundbase;//缴费基数
                        //    providentfounddetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                        //    providentfounddetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                        //    PaymentMonths++;
                        //}
                        string i = tdRow[3];
                        if (tdRow[3].IndexOf("个人缴存") > -1)
                        {
                            providentfounddetail.ProvidentFundTime = Regex.Replace(tdRow[2], @"[/\&nbsp;\,\s]", "");
                            providentfounddetail.PersonalPayAmount = Regex.Replace(tdRow[4], @"[/\&nbsp;\,\s]", "").ToDecimal(0);
                            providentfounddetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                            providentfounddetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                            PaymentMonths++;
                        }
                        else if (tdRow[3].IndexOf("结息") > -1)
                        {
                            providentfounddetail.PersonalPayAmount = Regex.Replace(tdRow[4], @"[/\&nbsp;\,\s]", "").ToDecimal(0);
                            providentfounddetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                            providentfounddetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                        }
                        else if (tdRow[3].IndexOf("补缴") > -1)
                        {
                            providentfounddetail.CompanyPayAmount = Regex.Replace(tdRow[4], @"[/\&nbsp;\,\s]", "").ToDecimal(0);
                            providentfounddetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                            providentfounddetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                        }
                        else if (tdRow[3].IndexOf("还贷") > -1)
                        {
                            providentfounddetail.PersonalPayAmount = Regex.Replace(tdRow[5], @"[/\&nbsp;\,\s]", "").ToDecimal(0);
                            providentfounddetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                            providentfounddetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                        }
                        else
                        {//（补缴，结息etc，数据不精确，只做参考用）
                            //providentfounddetail.PersonalPayAmount = tdRow[4].ToDecimal(0);
                            providentfounddetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                            providentfounddetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                            //providentfounddetail.PersonalPayAmount = Regex.Replace(tdRow[5], @"[/\&nbsp;\,\s]", "").ToDecimal(0) + Regex.Replace(tdRow[4], @"[/\&nbsp;\,\s]", "").ToDecimal(0);
                           
                        }
                        Res.ProvidentFundDetailList.Add(providentfounddetail);

                    }
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

        /// <summary>
        /// 格式化时间戳
        /// </summary>
        private static string ReplaceString(string str, int a, int b)
        {
            List<string> oldStr = str.Split(' ').ToList();
            List<string> newStr = str.Split(' ').ToList();
            newStr[a] = oldStr[b];
            newStr[b] = oldStr[a];
            string s1 = string.Join("%20", newStr.ToArray());
            return s1;
        } 
    }
}
