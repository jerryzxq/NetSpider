using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.JS
{
    public class zhenjiang : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
       
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = new HttpResult();
        HttpItem httpItem = new HttpItem();
        string baseUrl = "http://www.zjzfjj.com.cn/";
        string fundCity = "js_zhenjiang";
        #endregion
        #region 私有变量
        string Url = string.Empty;
        string _postData = string.Empty;
        List<string> _results = new List<string>();
        private int PaymentMonths = 0;
        private decimal payRate = (decimal)0.08;
        #endregion

        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            CookieCollection cookies = new CookieCollection();
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                Url = baseUrl + "jcaptcha?onlynum=true";
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

                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
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

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(ProvidentFundReq fundReq)
        {
            CookieCollection cookies = new CookieCollection();
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = fundCity;
            string loginType = string.Empty;//[1:职工账号,2:身份证]
            Regex reg = new Regex(@"[\s;\&nbsp;\,;\u5143;]*");
            string perInfoUrl = string.Empty;//个人信息链接
            string perDetailUrl = string.Empty;//缴费详情链接
            try
            {
                ////获取缓存s
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                string spcode = string.Empty;//登陆账号
                //15位或18位身份证验证
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (fundReq.LoginType == "1")
                {
                    if (regex.IsMatch(fundReq.Identitycard) == false)
                    {
                        Res.StatusDescription = "请输入有效的15位或18位身份证号";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    loginType = "2";
                    spcode = fundReq.Identitycard;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(fundReq.Username))
                    {
                        Res.StatusDescription = "职工账号不能为空";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    loginType = "1";
                    spcode = fundReq.Username;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Password))
                {
                    Res.StatusDescription = "密码不能为空！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Vercode))
                {
                    Res.StatusDescription = "您输入的验证码有误！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆
                double rdm = DateTime.Now.Subtract(DateTime.Parse("1970-1-1")).TotalMilliseconds * 10000;
                string rdmStr = string.Format("{0:F0}", rdm);
                Url = baseUrl + "searchPersonLogon.do?logon=" + rdmStr;
                _postData = string.Format("select={0}&spcode={1}&sppassword={2}&rand={3}", loginType, spcode.ToBase64(), fundReq.Password.ToBase64(), fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = _postData,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "<script> alert('", "');</script>");
                if (!string.IsNullOrWhiteSpace(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "<body>", "</body></html>");
                if (!string.IsNullOrWhiteSpace(errorMsg) && !errorMsg.Contains("<head>"))
                {
                    Res.StatusDescription = "系统没有该账号信息,请核对后再输";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (httpResult.Html.Contains("确认密码"))
                {
                    Res.StatusDescription = "首次登陆网站须修改密码后才能继续查询公积金信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = baseUrl + "searchMenuView.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = "MENUCODE=0&node=ynode-7",
                    //MENUCODE = node.attributes.dm || '0'    ynode-7(7为侧边导航栏的条数)
                    Referer = baseUrl + "searchMain.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                #endregion
                #region 第二步,查询个人基本信息
                string jsonStr = "{\"list\":" + (httpResult.Html) + "}";
                List<navigationBar> navigationBar = jsonParser.DeserializeObject<List<navigationBar>>(jsonParser.GetResultFromParser(jsonStr, "list"));
                foreach (var bar in navigationBar)
                {
                    if (bar.text.Trim().Equals("个人帐户信息"))
                    {
                        perInfoUrl = bar.url;
                    }
                    if (bar.text.Trim().Equals("个人明细查询"))
                    {
                        perDetailUrl = bar.url;
                    }
                    if (!string.IsNullOrEmpty(perInfoUrl) && !string.IsNullOrEmpty(perDetailUrl))
                    {
                        break;
                    }
                }
                if (string.IsNullOrEmpty(perInfoUrl))
                {
                    Res.StatusDescription = "个人帐户信息导航链接或描述发生改变,请核对网站是否改版";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Url = baseUrl + perInfoUrl;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    Referer = baseUrl + "searchMain.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[1]/td[1]", "text");
                if (_results.Count > 0)
                {
                    string temp = _results[0];
                    if (loginType == "2")
                    {
                        Res.IdentityCard = temp;//身份证号
                    }
                    else
                    {
                        Res.ProvidentFundNo = temp;//公积金号
                    }
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[1]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.SalaryBase = reg.Replace(_results[0], "").ToDecimal(0);//身份证号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[2]/td[1]", "text");
                if (_results.Count > 0)
                {
                    decimal temp = reg.Replace(_results[0], "").ToDecimal(0);
                    Res.PersonalMonthPayAmount = temp / 2;
                    Res.CompanyMonthPayAmount = temp / 2;
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[2]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.TotalAmount = reg.Replace(_results[0], "").ToDecimal(0);
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[3]/td[1]", "text");
                if (_results.Count > 0)
                {
                    Res.LastProvidentFundTime = reg.Replace(_results[0], "");
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju1']/tr[3]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.Status = reg.Replace(_results[0], "");
                }
                if (string.IsNullOrEmpty((Res.Name)))
                {
                    Res.Name = " ";
                }
                Res.PersonalMonthPayRate = Math.Round(Res.PersonalMonthPayAmount / Res.SalaryBase,2);
                Res.CompanyMonthPayRate = Math.Round(Res.CompanyMonthPayAmount / Res.SalaryBase, 2);
                #endregion
                #region 第三步,获取缴费明细
                //Url = baseUrl + perDetailUrl;
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "GET",

                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);

                //网站提供最近3年的记录
                int beginYear = DateTime.Now.Year;
                int endYear = beginYear - 2;

                for (int i = beginYear; i >= endYear; i--)
                {
                    Url = baseUrl + "searchGrmx.do?year=" + i;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "GET",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    List<string> results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='shuju']/tr[position()>1]", "inner");
                    foreach (var item in results)
                    {
                        ProvidentFundDetail detail = new ProvidentFundDetail();
                        List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        if (tdRow.Count != 7)
                        {
                            continue;
                        }
                        detail.PayTime = reg.Replace(tdRow[1], "").ToDateTime();//缴费年月
                        //应属年月
                        detail.Description = tdRow[3];//描述
                        if (tdRow[3].Trim().IndexOf("汇缴", StringComparison.Ordinal) > -1)
                        {
                            detail.ProvidentFundTime = reg.Replace(tdRow[2], "").Replace("-", "");
                            detail.PersonalPayAmount = reg.Replace(tdRow[4], "").ToDecimal(0) / 2;//个人缴费金额
                            detail.CompanyPayAmount = reg.Replace(tdRow[4], "").ToDecimal(0) / 2;//企业缴费金额
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                            detail.ProvidentFundBase = Math.Round(detail.PersonalPayAmount /Res.PersonalMonthPayRate,2);//缴费基数
                            PaymentMonths++;
                        }
                        else if (tdRow[3].Trim().IndexOf("还贷", StringComparison.Ordinal) > -1)
                        {
                            detail.ProvidentFundTime = reg.Replace(tdRow[2], "").Replace("-", "");
                            detail.PersonalPayAmount = reg.Replace(tdRow[5], "").ToDecimal(0);//个人缴费金额
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                        }
                        else if (tdRow[3].Trim().IndexOf("结息", StringComparison.Ordinal) > -1)
                        {
                            detail.ProvidentFundTime = reg.Replace(tdRow[2], "").Replace("-", "");
                            detail.PersonalPayAmount = reg.Replace(tdRow[4], "").ToDecimal(0);//个人缴费金额
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                        }
                        else if (tdRow[3].Trim().IndexOf("补缴", StringComparison.Ordinal) > -1)
                        {
                            detail.ProvidentFundTime = reg.Replace(tdRow[2], "").Replace("-", "");
                            detail.CompanyPayAmount = reg.Replace(tdRow[4], "").ToDecimal(0);//个人缴费金额
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                        }
                        else
                        {//（补缴，结息etc，数据不精确，只做参考用）
                            //detail.PersonalPayAmount = reg.Replace(tdRow[4], "").ToDecimal(0) + reg.Replace(tdRow[5], "").ToDecimal(0);//个人缴费金额
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
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
        /// <summary>
        /// 导航栏信息
        /// </summary>
        public class navigationBar
        {
            public string id { get; set; }
            /// <summary>
            /// 导航链接描述
            /// </summary>
            public string text { get; set; }
            /// <summary>
            /// 导航链接
            /// </summary>
            public string url { get; set; }

        }
    }
}
