using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.JS
{
    public class xuzhou : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        //IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        //IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://cx.xzgjj.gov.cn/wscxxuzhou/";
        string fundCity = "js_xuzhou";
        List<string> _results = new List<string>();
        decimal payRate = (decimal)0.08;
        int PaymentMonths = 0;
        #endregion
        #region 私有变量
        string cxydmc = string.Empty;//查询年度
        string _url = string.Empty;
        private string sfzh = string.Empty;
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                #region 初始化

                _url = baseUrl;//+ "zfbzgl/zfbzsq/login.jsp?zgzh=&sfzh=";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "get",
                    Host="cx.xzgjj.gov.cn",
                    Encoding = Encoding.GetEncoding("GBK"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies,httpResult.CookieCollection);
                _url = baseUrl + "zfbzgl/zfbzsq/index.jsp";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "get",
                    Host = "cx.xzgjj.gov.cn",
                    Referer=baseUrl,
                    Encoding = Encoding.GetEncoding("GBK"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                _url = baseUrl+ "zfbzgl/zfbzsq/login.jsp?zgzh=&sfzh=";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "get",
                    Host = "cx.xzgjj.gov.cn",
                    Referer = baseUrl + "zfbzgl/zfbzsq/index.jsp",
                    Encoding = Encoding.GetEncoding("GBK"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='cxydmc']", "value");
                if (_results.Count < 1)
                {
                    vcRes.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    vcRes.StatusCode = ServiceConsts.StatusCode_fail;
                    return vcRes;
                }
                cxydmc = _results[0];
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                //添加缓存
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("cxydmc", cxydmc);
                CacheHelper.SetCache(token, dics);
                #endregion
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
            string postData = string.Empty;//提交数据
            string zgxm = string.Empty;//姓名
            string dwbm = string.Empty;//单位编号
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(fundReq.Token);
                    cxydmc = dics["cxydmc"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                //15位或18位身份证验证
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (regex.IsMatch(fundReq.Identitycard) == false)
                {
                    Res.StatusDescription = "请输入有效的18位身份证号";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrEmpty(fundReq.Username))
                {
                    Res.StatusDescription = "公积金账号不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,校验登陆

                _url = baseUrl + "zfbzgl/zfbzsq/login_hidden.jsp";
                postData = string.Format("cxyd={0}&zgzh={1}&sfzh={2}", cxydmc.ToUrlEncode(Encoding.GetEncoding("GBK")), fundReq.Username, fundReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "POST",
                    Postdata = postData,
                    KeepAlive = false,
                    Encoding = Encoding.GetEncoding("GBK"),
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
                string errorMsc = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\");");
                if (!string.IsNullOrEmpty(errorMsc))
                {
                    Res.StatusDescription = errorMsc;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgxm']", "value");
                if (_results.Count > 0)
                {
                    zgxm = _results[0];
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dwbm']", "value");
                if (_results.Count > 0)
                {
                    dwbm = _results[0];
                }
                if (string.IsNullOrEmpty(zgxm) || string.IsNullOrEmpty(dwbm))
                {
                    Res.StatusDescription = "账号信息不存在";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='sfzh']", "value");
                if (_results.Count > 0)
                {
                    sfzh = !string.IsNullOrEmpty(_results[0]) ? _results[0] : fundReq.Identitycard;
                }
                #endregion
                #region 第二步,获取基本信息

                _url = baseUrl + string.Format("zfbzgl/zfbzsq/main_menu.jsp?zgzh={0}&sfzh={1}&zgxm={2}&dwbm={3}&cxyd={4}", fundReq.Username, sfzh, zgxm.ToUrlEncode(Encoding.GetEncoding("GBK")), dwbm, cxydmc.ToUrlEncode(Encoding.GetEncoding("GBK")));
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("GBK"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class=1]/tr[1]/td[2]", "text");
                Regex reg = new Regex(@"[\&nbsp;\;\s]");
                if (_results.Count > 0)
                {
                    Res.Name = reg.Replace(_results[0], "");//职工姓名
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class=1]/tr[2]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.IdentityCard = reg.Replace(_results[0], "");//身份证号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class=1]/tr[2]/td[4]", "text");
                if (_results.Count > 0)
                {
                    Res.ProvidentFundNo = reg.Replace(_results[0], "");//职工账号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class=1]/tr[3]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.CompanyName = reg.Replace(_results[0], "");//所属单位
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class=1]/tr[4]/td[4]", "text");
                if (_results.Count > 0)
                {
                    Res.OpenTime = reg.Replace(_results[0], "");//开户日期
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class=1]/tr[5]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.Status = reg.Replace(_results[0], "");//当前状态
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class=1]/tr[6]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = reg.Replace(_results[0], "").ToDecimal(0);//单位月缴金额
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class=1]/tr[6]/td[4]", "text");
                if (_results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = reg.Replace(_results[0], "").ToDecimal(0);//个人月缴金额
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class=1]/tr[9]/td[4]", "text");
                if (_results.Count > 0)
                {
                    Res.TotalAmount = reg.Replace(_results[0], "").ToDecimal(0);//余额
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class=1]/tr[5]/td[4]", "text");
                if (_results.Count > 0)
                {
                    Res.LastProvidentFundTime = reg.Replace(_results[0], "");//最后缴费时间
                }
                Res.ProvidentFundNo = string.IsNullOrEmpty(Res.ProvidentFundNo) ? fundReq.Username : Res.ProvidentFundNo;
                Res.PersonalMonthPayRate = Res.CompanyMonthPayRate = payRate;
                #endregion
                #region  公积金缴费明细查询
         
                _url =baseUrl+ string.Format("zfbzgl/gjjmxcx/gjjmx_cx.jsp?sfzh={0}&zgxm={2}&zgzh={1}&dwbm={3}&cxyd{4}", Res.IdentityCard, fundReq.Username, zgxm.ToUrlEncode(Encoding.GetEncoding("GBK")), dwbm, cxydmc.ToUrlEncode(Encoding.GetEncoding("GBK")));
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("GBK"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                string intRowCount = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='intRowCount']", "value")[0];
                _url = baseUrl + "zfbzgl/gjjmxcx/gjjmx_cx.jsp";
                postData = string.Format("startRow=0&endRow={4}&sfzh={0}&zgzh={1}&zgxm={2}&dwbm={3}&flag=1", Res.IdentityCard, fundReq.Username, zgxm.ToUrlEncode(Encoding.GetEncoding("GBK")), dwbm, intRowCount);
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "POST",
                    Postdata = postData,
                    Encoding = Encoding.GetEncoding("GBK"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr[position()>1]", "inner");
                foreach (var items in _results)
                {
                    ProvidentFundDetail detail = new ProvidentFundDetail();
                    var tdRow = HtmlParser.GetResultFromParser(items, "//td", "text");
                    if (tdRow.Count < 5)
                    {
                        continue;
                    }
                    detail.Description = tdRow[1];//描述
                    if (tdRow[1].Contains("汇缴"))
                    {
                        detail.PayTime = tdRow[0].ToDateTime();
                        //detail.ProvidentFundTime = Convert.ToDateTime((tdRow[1].ToTrim("汇缴").ToTrim("公积金")))
                        //    .ToString("yyyyMM");
                        detail.ProvidentFundTime = Convert.ToDateTime(CommonFun.GetMidStr(tdRow[1], "汇缴", "公积金"))
                            .ToString("yyyyMM");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = Math.Round(tdRow[3].ToDecimal(0) / 2, 2);//金额
                        detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                        detail.ProvidentFundBase = Math.Round((detail.PersonalPayAmount / payRate), 2);//缴费基数
                        PaymentMonths++;
                    }
                    else if (tdRow[1].Contains("本年合计"))
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0); //金额
                    }
                    else if (tdRow[1].Contains("取"))
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                        detail.PersonalPayAmount = tdRow[2].ToDecimal(0); //金额
                        Res.Description = "有支取，请人工校验";
                    }
                    else
                    {
                        detail.PayTime = tdRow[0].ToDateTime();
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0); //金额
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
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
    }
}
