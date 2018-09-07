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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.BJ
{
    public class beijing : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.bjgjj.gov.cn/";
        string fundCity = "bj_beijing";
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
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "wsyw/servlet/PicCheckCode1";
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
                //校验参数
                if (fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //校验参数
                if (fundReq.LoginType == "1" && fundReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = "身份证号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //校验参数
                if (fundReq.LoginType != "1" && fundReq.Username.IsEmpty())
                {
                    Res.StatusDescription = "联名卡号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                string Key1 = "pdcss123";
                string Key2 = "css11q1a";
                string Key3 = "co1qacq11";
                #region 第一步，初始化登录页面
                Url = baseUrl + "wsyw/wscx/gjjcx-login.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                #endregion

                #region 第二步，获取动态变量lk

                Url = baseUrl + "wsyw/wscx/asdwqnasmdnams.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    CookieCollection = cookies,
                    Referer = baseUrl + "wsyw/wscx/gjjcx-login.jsp",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                #endregion

                #region 第三步，登录系统
                string lk = CommonFun.ClearFlag(httpResult.Html);
                lk = lk.Substring(4, lk.Length - 4);


                Url = baseUrl + "wsyw/wscx/gjjcx-choice.jsp";
                if (fundReq.LoginType == "1")
                {
                    postdata = String.Format("lb=1&bh={0}&mm={1}&gjjcxjjmyhpppp={2}&lk={3}", MultiKeyDES.EncryptDES(fundReq.Identitycard, Key1, Key2, Key3), MultiKeyDES.EncryptDES(fundReq.Password, Key1, Key2, Key3), fundReq.Vercode, lk);
                }
                else
                {
                    postdata = String.Format("lb={4}&bh={0}&mm={1}&gjjcxjjmyhpppp={2}&lk={3}", MultiKeyDES.EncryptDES(fundReq.Username, Key1, Key2, Key3), MultiKeyDES.EncryptDES(fundReq.Password, Key1, Key2, Key3), fundReq.Vercode, lk, fundReq.LoginType);
                }
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@class='cOrange']", "text");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@class='title-zcfgmc']/div/table/tr[last()]/td[2]", "");

                if (httpResult.StatusCode != HttpStatusCode.OK || results.Count == 0)
                {
                    Res.StatusDescription = "登录失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (fundReq.LoginType=="5")
                {
                    Res.BankCardNo = fundReq.Username;//联名卡号登陆方式
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = CommonFun.GetMidStr(results[0], "window.open(\"", "\",");

                if (Url.IsEmpty())
                {
                    Res.StatusDescription = "密码错误或无公积金信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion

                #region 第四步，查询个人基本信息
               
                Url = baseUrl + "wsyw/wscx/" + Url;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                //分析个人基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[3]/td[1]/table[1]/tr[1]/td[1]/div[1]/table[1]/tr[1]/td[1]/div[1]/div[2]/table[1]/tr/td", "text", true);
                if (results.Count > 0)
                {
                    Res.Name = HttpUtility.HtmlDecode(results[1]);
                    Res.ProvidentFundNo = results[3];
                    Res.IdentityCard = results[7];
                    Res.CompanyNo = results[9];
                    Res.CompanyName = HttpUtility.HtmlDecode(results[11]);
                    Res.TotalAmount = results[17].Replace("元", "").ToDecimal(0);
                    Res.Status = HttpUtility.HtmlDecode(results[19]);
                }
                else
                {
                    Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.PersonalMonthPayRate = Res.CompanyMonthPayRate = payRate;//0.12
                //今年缴费明细
                List<string> detailResults = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[3]/td[1]/table[1]/tr[1]/td[1]/div[1]/table[1]/tr[1]/td[1]/div[3]/div[1]/table[1]/tr[position()>1]", "inner", true);

                Url = CommonFun.GetMidStr(httpResult.Html, "gjj_cxls.jsp?", "&#");
                #endregion

                #region 第五步，缴费明细
                Url = baseUrl + "wsyw/wscx/gjj_cxls.jsp?" + Url + "缴存";
               // Url = "http://www.bjgjj.gov.cn/wsyw/wscx/gjj_cx.jsp?nicam=enpyenI1enk3eXI2cmM1OHo3cnI2MDAA&hskwe=R0pKd2NjNXo2MnphNTc1&vnv=JiMyNDM1MjsmIzI5NzM0OwAA&lx=1#";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[1]/tr[1]/td[1]/table[1]/tr[1]/td[1]/table[2]/tr[1]/td[1]/table[2]/tr[1]/td[1]/form[1]/div[1]/table[1]/tr[position()>1]", "");

                //历史缴费信息
                foreach (string item in results)
                {
                    var strDetail = HtmlParser.GetResultFromParser(item.Replace("&nbsp;", ""), "//td", "text", true);
                    if (strDetail.Count == 6)
                    {
                        detail = new ProvidentFundDetail();

                        detail.PayTime = DateTime.ParseExact(strDetail[0], "yyyyMMdd", null);
                        detail.ProvidentFundTime = strDetail[1];
                        detail.Description = HttpUtility.HtmlDecode(strDetail[2]);//描述
                        if (strDetail[2].IndexOf("&#27719;&#32564;") != -1)
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            detail.PersonalPayAmount = strDetail[3].ToDecimal(0) / 2;//金额
                            detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                            detail.ProvidentFundBase = (detail.PersonalPayAmount / payRate).ToString("f2").ToDecimal(0);//缴费基数
                            PaymentMonths++;
                        }
                        else if (HttpUtility.HtmlDecode(strDetail[2]).Contains("提取"))
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                            detail.PaymentType = HttpUtility.HtmlDecode(strDetail[2]);
                            detail.PersonalPayAmount = strDetail[3].ToDecimal(0);
                        }
                        else
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.PersonalPayAmount = strDetail[3].ToDecimal(0);
                        }
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                //今年缴费信息
                foreach (string item in detailResults)
                {
                    var strDetail = HtmlParser.GetResultFromParser(item.Replace("&nbsp;", ""), "//td", "inner", true);
                    if (strDetail.Count == 6)
                    {
                        //查询月份是否已统计过
                        var query = Res.ProvidentFundDetailList.Where(o => o.ProvidentFundTime == strDetail[1]).FirstOrDefault();
                        if (query != null)
                        {
                            continue;
                        }
                        detail = new ProvidentFundDetail();

                        detail.PayTime = DateTime.ParseExact(strDetail[0], "yyyyMMdd", null);
                        detail.ProvidentFundTime = strDetail[1];
                        detail.Description = HttpUtility.HtmlDecode(strDetail[2]);//描述
                        if (strDetail[2].IndexOf("&#27719;&#32564;") != -1)
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            detail.PersonalPayAmount = strDetail[3].ToDecimal(0) / 2;//金额
                            detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                            detail.ProvidentFundBase = (detail.PersonalPayAmount / payRate).ToString("f2").ToDecimal(0);//缴费基数
                            PaymentMonths++;
                        }
                        else if (HttpUtility.HtmlDecode(strDetail[2]).Contains("提取"))
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                            detail.PaymentType = HttpUtility.HtmlDecode(strDetail[2]);
                            detail.PersonalPayAmount = strDetail[3].ToDecimal(0);
                        }
                        else
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                           
                            detail.PersonalPayAmount = strDetail[3].ToDecimal(0);
                        }
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
