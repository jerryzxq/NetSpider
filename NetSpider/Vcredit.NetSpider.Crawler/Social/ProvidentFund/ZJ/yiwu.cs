using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.ZJ
{

    public class yiwu : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://122.226.76.37/";
        string fundCity = "zj_yiwu";
        #endregion
        #region 私有变量
        string _url = string.Empty;
        private List<string> _results = new List<string>();
        private ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        string _postData = string.Empty;
        private int PaymentMonths = 0;
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                _url = baseUrl + "Validate?" + DateTime.Now.Subtract(DateTime.Parse("1970-01-01")).Milliseconds;
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "get",
                    Referer = baseUrl + "account/Logon",
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
                vcRes.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                vcRes.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                //合并缓存
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //添加缓存
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
            Res.ProvidentFundCity = fundCity;
            decimal payRate = (decimal)0.08;
            decimal perAccounting = 0;//个人占比
            decimal comAccounting = 0;//公司占比
            decimal totalRate = 0;//总缴费比率
            Regex reg = new Regex(@"[\&nbsp;\s;\,;\￥;\%;]*");
            Regex Reg = new Regex(@"[^0-9.1-9.]*");//去除非数字
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (string.IsNullOrEmpty(fundReq.Password) || string.IsNullOrEmpty(fundReq.Identitycard))
                {
                    Res.StatusDescription = "身份证号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆
                _url = baseUrl + "Account/Logon";
                _postData = string.Format("UserName={0}&Password={1}&Racha={2}", fundReq.Identitycard, fundReq.Password.ToUrlEncode(), fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "Post",
                    Postdata = _postData,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@class='field-validation-error']", "text");
                string errorMsg = string.Empty;

                if (_results.Count > 0)
                {
                    errorMsg = _results[0];
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        Res.StatusDescription = errorMsg;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }

                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='validation-summary-errors']/span", "text");
                if (_results.Count > 0)
                {
                    errorMsg = _results[0];
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        Res.StatusDescription = errorMsg;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='right']/h3", "text");
                if (_results.Count > 0)
                {
                    errorMsg = _results[0];
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        Res.StatusDescription = errorMsg;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region 第二步,获取基本信息
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //_url = baseUrl + "psl";
                //httpItem = new HttpItem()
                //{
                //    URL = _url,
                //    Method = "GET",
                //    Referer = baseUrl + "account/Logon",
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='right']/table/tbody/tr[1]/td[1]", "text");
                if (_results.Count > 0)
                {
                    Res.Name = reg.Replace((_results[0]), "");//姓名
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='right']/table/tbody/tr[1]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.ProvidentFundNo = reg.Replace((_results[0]), "");//公积金账号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='right']/table/tbody/tr[2]/td[1]", "text");
                if (_results.Count > 0)
                {
                    Res.IdentityCard = reg.Replace((_results[0]), "");//身份证号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='right']/table/tbody/tr[2]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.Phone = reg.Replace((_results[0]), "");//手机号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='right']/table/tbody/tr[3]/td[1]", "text");
                if (_results.Count > 0)
                {
                    Res.OpenTime = reg.Replace((_results[0]), "");//开户日期
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='right']/table/tbody/tr[3]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.Status = reg.Replace((_results[0]), "");//状态
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='right']/table/tbody/tr[3]/td[3]", "text");
                if (_results.Count > 0)
                {
                    Res.BankCardNo = reg.Replace((_results[0]), "");//银行账号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='right']/table/tbody/tr[5]/td[1]", "text");
                if (_results.Count > 0)
                {
                    Res.CompanyNo = reg.Replace((_results[0]), "");//单位帐号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='right']/table/tbody/tr[5]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.CompanyName = reg.Replace((_results[0]), "");//单位名称
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='right']/table/tbody/tr[6]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.TotalAmount = reg.Replace((_results[0]), "").ToDecimal(0);//当前余额
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='right']/table/tbody/tr[7]/td[1]", "text");
                if (_results.Count > 0)
                {
                    Res.SalaryBase = reg.Replace((_results[0]), "").ToDecimal(0);//月缴基数
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='right']/table/tbody/tr[7]/td[3]", "text");
                if (_results.Count > 0)
                {
                    Res.PersonalMonthPayRate = reg.Replace((_results[0]), "").ToDecimal(0) * 0.01M;//个人缴存比率
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='right']/table/tbody/tr[8]/td[3]", "text");
                if (_results.Count > 0)
                {
                    Res.CompanyMonthPayRate = reg.Replace((_results[0]), "").ToDecimal(0) * 0.01M;//单位缴存比率
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='right']/table/tbody/tr[9]/td[1]", "text");
                if (_results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = reg.Replace((_results[0]), "").ToDecimal(0);//单位月缴额
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='right']/table/tbody/tr[9]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = reg.Replace((_results[0]), "").ToDecimal(0);//个人月缴额
                }
                #endregion
                #region 第三步,查询公积金明细

                if (Res.PersonalMonthPayRate > 0 && Res.CompanyMonthPayRate > 0)
                {
                    totalRate = Res.PersonalMonthPayRate + Res.CompanyMonthPayRate;
                    perAccounting = (Res.PersonalMonthPayRate / totalRate);
                    comAccounting = (Res.CompanyMonthPayRate / totalRate);
                }
                else
                {
                    totalRate = (payRate) * 2;//0.16
                    perAccounting = comAccounting = (decimal)0.50;
                }
                int nowYear = DateTime.Now.Year - 1;
                _url = baseUrl + "psl/detail";
                for (int i = nowYear; i >= nowYear - 5; i--)
                {
                    _postData = string.Format("year={0}", i);
                    httpItem = new HttpItem()
                    {
                        URL = _url,
                        Method = "Post",
                        Postdata = _postData,
                        Referer = baseUrl + "psl/detail",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='content']/table[@class='grid']/tbody/tr", "", true);
                    foreach (string item in _results)
                    {
                        ProvidentFundDetail detail = new ProvidentFundDetail();
                        List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        if (tdRow.Count != 5) continue;
                        detail.PayTime = tdRow[0].ToDateTime(Consts.DateFormatString2);//缴费年月
                        detail.Description = tdRow[1];//描述
                        detail.ProvidentFundTime = new Regex(@"[0-9 1-9]{0,6}").Match(Reg.Replace(tdRow[0], "")).Value;//应属年月
                        if (tdRow[2].ToDecimal(0) > 0)
                        {
                            detail.PersonalPayAmount = reg.Replace(tdRow[3], "").ToDecimal(0);//支取金额
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;//缴费类型
                        }
                        else if (tdRow[1].IndexOf("汇缴", StringComparison.Ordinal) > -1)
                        {
                            detail.PersonalPayAmount = (reg.Replace(tdRow[3], "").ToDecimal(0) * perAccounting);//个人缴费金额
                            detail.CompanyPayAmount = (reg.Replace(tdRow[3], "").ToDecimal(0) * comAccounting);//企业缴费金额
                            detail.ProvidentFundBase = (reg.Replace(tdRow[3], "").ToDecimal(0) / totalRate);//缴费基数
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                            PaymentMonths++;
                        }
                        else
                        {
                            detail.PersonalPayAmount = Math.Abs(reg.Replace(tdRow[2], "").ToDecimal(0) - reg.Replace(tdRow[3], "").ToDecimal(0));//个人缴费金额
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费类型
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
