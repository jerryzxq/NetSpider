using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.Common.Constants;
namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SD
{
    class yantai : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.ytgjj.com/";//网址
        string fundCity = "sd_yantai";
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "image.jsp";//验证码地址
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
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                CacheHelper.SetCache(token, dics);
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
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(fundReq.Token);
                    cookies = (CookieCollection)dics["cookie"];
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || fundReq.Username.IsEmpty())
                {
                    Res.StatusDescription = "身份证和公积金账户均为必填！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //15位或18位身份证验证
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (!regex.IsMatch(fundReq.Identitycard))
                {
                    Res.StatusDescription = "身份证输入有误！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (fundReq.Username.Length != 14)
                {
                    Res.StatusDescription = "公积金账户输入有误！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (fundReq.Vercode.IsEmpty())
                {
                    Res.StatusDescription = "请输入验证码！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步登陆
                Url = baseUrl + "post_gjj.jsp";
                postdata = String.Format("IDcard={0}&acount={1}&trand={2}&imageField.x=34&imageField.y=11", fundReq.Identitycard, fundReq.Username, fundReq.Vercode);
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
                string errorInfo = CommonFun.GetMidStr(httpResult.Html, "<script>alert(", "');</script>");
                if (!errorInfo.IsEmpty())
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                errorInfo = CommonFun.GetMidStr(httpResult.Html, "<script language='javascript'>alert('", "');window.close();</script>");
                if (!errorInfo.IsEmpty())
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                #region 第二部，查询公积金信息
                Url = "http://218.56.40.227/gjjsearch.jsp";
                postdata = String.Format("card={0}&count={1}&source=gov", fundReq.Identitycard, fundReq.Username);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='Content']/table[2]/tr[1]/td", "innertext");
                if (results.Count == 1)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='Content']/div[1]", "innertext");
                if (results.Count > 0)
                {
                    Res.Status = results[0].Replace("账户状态：", "");//状态
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='Content']/table[2]/tr[1]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];//单位名称
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='Content']/table[2]/tr[1]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];//单位账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='Content']/table[2]/tr[2]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='Content']/table[2]/tr[2]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];//公积金账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='Content']/table[2]/tr[3]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='Content']/table[2]/tr[4]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);//账户余额
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='Content']/table[2]/tr[5]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.SalaryBase = results[0].ToDecimal(0);//缴存基数
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='Content']/table[2]/tr[5]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0];//缴至月份
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='Content']/table[2]/tr[6]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = (results[0].Replace("%", "").ToDecimal(0)) * 0.01M;//单位缴存比例
                    Res.CompanyMonthPayAmount = (Res.SalaryBase * Res.CompanyMonthPayRate).ToString("f2").ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='Content']/table[2]/tr[6]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = (results[0].Replace("%", "").ToDecimal(0)) * 0.01M;//个人缴存比例
                    Res.PersonalMonthPayAmount = (Res.SalaryBase * Res.PersonalMonthPayRate).ToString("f2").ToDecimal(0);
                }
                #endregion
                #region 第三步，公积金缴费记录
                Url = "http://218.56.40.227/gjjhshow.jsp?card=" + fundReq.Identitycard + "&count=" + fundReq.Username + "";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='Content']/table[2]/tr[position()>1]", "inner");
                if (results.Count == 0)
                {
                    Res.StatusDescription = "暂无账户明细";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                else
                {
                    foreach (var item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        if (tdRow.Count != 6)
                        {
                            continue;
                        }
                        detail = new ProvidentFundDetail();
                        detail.PayTime = tdRow[1].ToDateTime();//缴费年月
                        detail.Description = tdRow[2];//描述
                        if (tdRow[2].Trim() == "正常缴交")
                        {

                            detail.ProvidentFundTime = tdRow[3];//应属年月
                            detail.PersonalPayAmount = tdRow[4].ToDecimal(0) / 2;//个人缴费金额
                            detail.CompanyPayAmount = tdRow[4].ToDecimal(0) / 2;//企业缴费金额
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            decimal realPayRate = Res.PersonalMonthPayRate > 0
                                ? Res.PersonalMonthPayRate
                                : payRate;
                            detail.ProvidentFundBase = (detail.PersonalPayAmount / realPayRate).ToString("f2").ToDecimal(0);//缴费基数
                            PaymentMonths++;
                        }
                        else
                        {
                            detail.PersonalPayAmount = tdRow[4].ToDecimal(0);//个人缴费金额
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        }
                        Res.ProvidentFundDetailList.Add(detail);
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
    }
}
