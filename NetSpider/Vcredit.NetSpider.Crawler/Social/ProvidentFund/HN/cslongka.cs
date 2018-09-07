using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Web.UI;
using Vcredit.Common;
using Vcredit.Common.Ext;
using System.Collections;
using System.Text.RegularExpressions;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using System.Web;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HN
{
    /// <summary>
    /// 未提供公积金明细查询,密码为: 1' or '1' = '1(最前面的1之前有个空格)
    /// </summary>
    public class cslongka : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.csgjj.com.cn/";
        string fundCity = "hn_cslongka";
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
                date = date.Replace(", ", " ");
                date = ReplaceString(date, 1, 2);

                Url = baseUrl + "jcaptcha?onlynum=true&date=Mon%20Feb%2029%202016%2017:04:14%20GMT+0800";
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
            ProvidentFundDetail detail = null;
            string postData = string.Empty;
            decimal payRate = (decimal)0.08;
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


                if (fundReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = "请输入身份证号！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (fundReq.Username.IsEmpty())
                {
                    Res.StatusDescription = "请输入金龙卡卡号！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (fundReq.Vercode.IsEmpty())
                {
                    Res.StatusDescription = "请输入验证码！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,验证登陆
                url = baseUrl + "/searchGrmx.do?logon=1456734512000";
                postData = string.Format(@"spidno=%E8%AF%B7%E8%BE%93%E5%85%A5%E8%BA%AB%E4%BB%BD%E8%AF%81%E5%8F%B7&spname=%E8%AF%B7%E8%BE%93%E5%85
                                          %A5%E5%A7%93%E5%90%8D&alertspwd=%E8%AF%B7%E8%BE%93%E5%85%A5%E5%AF%86%E7%A0%81&spwd=123456&rand1=%E8%AF
                                          %B7%E8%BE%93%E5%85%A5%E9%AA%8C%E8%AF%81%E7%A0%81&flag=2&spidno2={0}&spcard={1}
                                          &rand2={2}&flag=2&sfzhm=%E8%AF%B7%E8%BE%93%E5%85%A5%E8%BA%AB%E4%BB%BD%E8%AF%81%E5%8F%B7&yhxm=%E8%AF
                                          %B7%E8%BE%93%E5%85%A5%E5%A7%93%E5%90%8D&rand5=%E8%AF%B7%E8%BE%93%E5%85%A5%E9%AA%8C%E8%AF%81%E7%A0%81
                                          &flag=5&spidno3=%E8%AF%B7%E8%BE%93%E5%85%A5%E8%BA%AB%E4%BB%BD%E8%AF%81%E5%8F%B7&xykh=%E8%AF%B7%E8%BE
                                          %93%E5%85%A5%E6%89%A3%E6%AC%BE%E8%B4%A6%E5%8F%B7&rand4=%E8%AF%B7%E8%BE%93%E5%85%A5%E9%AA%8C%E8%AF%81
                                          %E7%A0%81&flag=4", fundReq.Identitycard, fundReq.Username, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "Post",
                    Postdata = postData,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var msg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');window");
                if (!string.IsNullOrEmpty(msg) && httpResult.Html.Contains("alert"))
                {
                    Res.StatusDescription = msg;
                    Res.StatusCode = ServiceConsts.StatusCode_error;
                    return Res;
                }
                msg = HtmlParser.GetResultFromParser(httpResult.Html, "//title", "")[0].ToString();
                if (msg.Contains("错误"))
                {
                    Res.StatusDescription = "验证码输入错误";
                    Res.StatusCode = ServiceConsts.StatusCode_error;
                    return Res;
                }
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryError;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //存储cookie
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                int pagecount = int.Parse(HtmlParser.GetResultFromParser(httpResult.Html, "//font[@color='red']")[0]);
                #endregion

                #region 第二步,获取基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@border='0']/tr/td", "inner");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0].Split('>')[2].Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");       //单位账号
                    Res.EmployeeNo = results[1].Split('>')[2].Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");       //个人账号
                    Res.Status = results[2].Split('>')[2].Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");       //账户状态
                    Res.CompanyName = results[3].Split('>')[2].Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");       //单位名称
                    Res.Name = results[4].Split('>')[2].Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");       //姓名
                }

                #endregion
                #region ===第三步，获取详细信息(无法获取页面信息)===
                for (int i = 0; i < pagecount; i++)
                {
                    url = baseUrl + "searchGrmx.do?pageFlag=" + i;
                    httpItem = new HttpItem()
                    {
                        URL = url,
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

                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@cellpadding='0']/tr", "inner");
                    foreach (var item in results)
                    {
                        detail = new ProvidentFundDetail();
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        if (tdRow[0].Contains("序号"))
                        {
                            continue;
                        }
                        detail.PayTime = tdRow[1].ToDateTime();
                        detail.Description = tdRow[2];
                        if (item[2].ToString().Contains("汇缴"))
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            detail.CompanyPayAmount = item[3].ToString().ToDecimal(0)/2;//金额
                            detail.PersonalPayAmount = item[3].ToString().ToDecimal(0) / 2;//金额
                            PaymentMonths++;
                        }
                        else if (item[2].ToString().Contains("提取"))
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                            detail.PersonalPayAmount = item[3].ToString().ToDecimal(0);//金额
                            Res.Description = "有支取，请人工校验。";
                        }
                        else if (item[2].ToString().Contains("结息"))
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.PersonalPayAmount = item[3].ToString().ToDecimal(0);//金额
                        }
                        else if (item[2].ToString().Contains("补缴"))
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.CompanyPayAmount = item[3].ToString().ToDecimal(0);//金额

                        }
                        else if (item[2].ToString().Contains("职工并户"))
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.PersonalPayAmount = item[3].ToString().ToDecimal(0);//金额

                        }
                        else if (item[2].ToString().Contains("转入") || item[2].ToString().Contains("转出"))
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.PersonalPayAmount = item[3].ToString().ToDecimal(0);//金额
                        }
                        else
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        }
                        Res.ProvidentFundDetailList.Add(detail);
                    }

                }

                #endregion


                #region  第四步，获取贷款基本信息 ******所用测试账号暂无贷款信息******

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
