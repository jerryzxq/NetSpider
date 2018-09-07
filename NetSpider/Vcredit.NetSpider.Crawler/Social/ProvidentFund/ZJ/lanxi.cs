using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
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
    public class lanxi : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        //IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.lxgjj.com/";
        string fundCity = "zj_lanxi";
        #endregion
        #region 私有变量
        string _url = string.Empty;
        #endregion

        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                _url = baseUrl + "CheckCode.aspx";
                httpItem = new HttpItem()
                {
                    URL = _url,
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
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
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
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = fundCity;
            int PaymentMonths = 0;
            decimal payRate = (decimal) 0.08;
            List<string> results = new List<string>();
            List<string> _years = new List<string>();//查询年度
            string postData = string.Empty;//提交数据
            string viewstate = string.Empty;
            string viewstategenerator = string.Empty;
            string eventvalidation = string.Empty;
            string button1 = string.Empty;
           
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection) CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }

                //校验参数
                if (string.IsNullOrEmpty(fundReq.Name) || string.IsNullOrEmpty(fundReq.Identitycard) || string.IsNullOrEmpty(fundReq.Password))
                {
                    Res.StatusDescription = "身份证号、公积金账号或密码为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
    
                #region 初始化
                _url = baseUrl + "WebGjjcx.aspx";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("gb2312"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                if (results.Count < 1)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                viewstate = results[0];
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATEGENERATOR']", "value");
                if (results.Count > 0)
                {
                    viewstategenerator = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                if (results.Count > 0)
                {
                    eventvalidation = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='Button1']", "value");
                if (results.Count > 0)
                {
                    button1 = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@id='DropDownList1']/option", "value");
                if (results.Count > 0)
                {
                    _years = results;
                }

                #endregion
                int once = 0;
                //按年份倒序查询信息
                foreach (var year in _years)
                {
                    //最多查询近2年信息(网站服务器太差，请求时间太长，改为只查2年记录)  2015年9月26日11:01:36  /DateTime.Now.Year -0
                    //根据需求改为3年  2015年11月27日16:47:21 /DateTime.Now.Year -1
                    if (int.Parse(year) >= (DateTime.Now.Year -1))
                    {
                        #region 第一步,登陆

                        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                        _url = baseUrl + "WebGjjcx.aspx";
                        postData = string.Format("__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__EVENTVALIDATION={2}&DropDownList1={3}&tbSfz={4}&tbSfz0={5}&tbMima={6}&tb_yanzheng={7}&Button1={8}", viewstate.ToUrlEncode(Encoding.GetEncoding("gb2312")), viewstategenerator.ToUrlEncode(Encoding.GetEncoding("gb2312")), eventvalidation.ToUrlEncode(Encoding.GetEncoding("gb2312")), year, fundReq.Identitycard, fundReq.Name.ToUrlEncode(Encoding.GetEncoding("gb2312")), fundReq.Password, fundReq.Vercode, button1.ToUrlEncode(Encoding.GetEncoding("gb2312")).Replace("%20", "+"));
                        httpItem = new HttpItem()
                        {
                            URL = _url,
                            Method = "Post",
                            Postdata = postData,
                            Timeout=50000,
                            Encoding = Encoding.GetEncoding("gb2312"),
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                       
                        string errorMsc = CommonFun.GetMidStr(httpResult.Html, "alert('", "')</script>");
                        if (once>=1)
                        {
                            if (httpResult.Html == "请求超时")
                            {
                                Res.StatusDescription = "请求超时";
                                Res.StatusCode = ServiceConsts.StatusCode_fail;
                                return Res;
                            }
                            if (!string.IsNullOrEmpty(errorMsc))
                            {
                                Res.StatusDescription = "网站自身Bug,如果查询年度早于开户年限(" + (int.Parse(year)) + ")会提示该错误:" + errorMsc + ",查询明细结束";
                                Res.PaymentMonths = PaymentMonths;
                                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                                Res.StatusCode = ServiceConsts.StatusCode_success;
                                return Res;
                            }
                        }
                        if (!string.IsNullOrEmpty(errorMsc))
                        {
                            Res.StatusDescription = errorMsc;
                            Res.StatusCode = ServiceConsts.StatusCode_fail;
                            return Res;
                        }
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='lblMessage']", "text");
                        if (results.Count>0)
                        {
                            if (!string.IsNullOrWhiteSpace(results[0]))
                            {
                                Res.StatusDescription = results[0];
                                Res.StatusCode = ServiceConsts.StatusCode_fail;
                                return Res;
                            }
                        }
                        #endregion
                        #region 第二步,获取基本信息
                        //为保证程序自身效率该信息获取一次就行
                        if (once == 0)
                        {
                            results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='lbxm']", "text");
                            if (results.Count > 0)
                            {
                                Res.Name = results[0];//姓名
                            }
                            results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='lbsfz']", "text");
                            if (results.Count > 0)
                            {
                                Res.IdentityCard = results[0];//身份证
                            }
                            results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='lbdwmc']", "text");
                            if (results.Count > 0)
                            {
                                Res.CompanyName = results[0];//单位名称
                            }
                            results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='lbdwzh']", "text");
                            if (results.Count > 0)
                            {
                                Res.CompanyNo = results[0];//单位账号
                            }
                            results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='lbgrzh']", "text");
                            if (results.Count > 0)
                            {
                                Res.ProvidentFundNo = results[0];//个人账号
                            }
                            results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='lbjje']", "text");
                            if (results.Count > 0)
                            {
                                decimal monthPay = results[0].ToDecimal(0);//月缴额
                                if (monthPay>0)
                                {
                                    Res.SalaryBase = monthPay/(payRate*2);//缴费基数
                                    Res.PersonalMonthPayAmount = Res.SalaryBase*payRate;//个人月缴额
                                    Res.CompanyMonthPayAmount = Res.SalaryBase * payRate;//公司月缴额
                                    Res.PersonalMonthPayRate = Res.CompanyMonthPayRate = payRate;//缴费比率 暂定0.08
                                }
                            }
                            results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='lbye']", "text");
                            if (results.Count > 0)
                            {
                                Res.TotalAmount = results[0].ToDecimal(0);//余额
                            }
                            results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='lbzt']", "text");
                            if (results.Count > 0)
                            {
                                Res.Status = results[0];//状态
                            }
                        }
                        once++;
                        #endregion
                        #region 第三步,查询公积金明细

                        Regex reg = new Regex(@"[0-9]{6}");//匹配6位数字
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='GridView1']/tr[position()>1]", "inner", true);
                        List<string> savingList = results;//本年度缴交情况
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='GridView2']/tr[position()>1]", "inner", true);
                        List<string> getList = results;//本年度支取情况
                        savingList.AddRange(getList);
                        foreach (string item in savingList)
                        {
                            ProvidentFundDetail detail = new ProvidentFundDetail();
                            List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                            if (tdRow.Count != 5 && tdRow.Count != 3)
                            {
                                continue;
                            }
                            if (tdRow.Count == 5)
                            {//汇缴公积金
                                detail.ProvidentFundTime = tdRow[1];//应属年月
                                detail.PayTime = tdRow[1].ToDateTime(Consts.DateFormatString7);//缴费时间
                                detail.CompanyPayAmount = tdRow[3].ToDecimal(0);//个人缴费金额
                                detail.PersonalPayAmount = tdRow[2].ToDecimal(0);//企业缴费金额
                                detail.ProvidentFundBase = (tdRow[3].ToDecimal(0) / payRate).ToString("f2").ToDecimal(0);//缴费基数
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                                detail.Description = "本年度汇缴记录";//描述
                                PaymentMonths++;
                            }
                            else
                            {//（支取公积金）
                                detail.Description = "本年度支取记录";//描述
                                detail.PayTime = tdRow[0].ToDateTime(Consts.DateFormatString5);//缴费时间
                                detail.ProvidentFundTime = reg.Match(tdRow[0]).Value;//应属年月
                                detail.PersonalPayAmount = tdRow[2].ToDecimal(0);//个人缴费金额
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费类型
                            }
                            Res.ProvidentFundDetailList.Add(detail);
                        }
                        #endregion
                    }
                }
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
