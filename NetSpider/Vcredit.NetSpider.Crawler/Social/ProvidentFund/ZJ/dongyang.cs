using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.ZJ
{
    public class dongyang : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://61.175.190.122:8080/";
        string fundCity = "zj_dongyang";
        #endregion
        #region 私有变量
        Regex times = new Regex(@"[0-9][0-9]{0,2}");
        Regex Nregnum = new Regex(@"[^0-9]*");//去除非数字
        string realTime = string.Empty;//应属年月
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        List<string> _results = new List<string>();
        int PaymentMonths = 0;
        private decimal payRate = (decimal)0.08;
        decimal perAccounting = 0;//个人占比
        decimal comAccounting = 0;//公司占比
        decimal totalRate = 0;//总缴费比率
        #endregion
        public VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                vcRes.StatusDescription = "所选城市无需初始化";
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
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || fundReq.Username.IsEmpty())
                {
                    Res.StatusDescription = "身份证号或公积金账号不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆

                Url = baseUrl + "check.php";
                postdata = string.Format("cardid={0}&bankid={1}&submit.x=41&submit.y=28", fundReq.Identitycard, fundReq.Username);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\");").Trim();
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                #region 第二步,获取基本信息
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='rightbox']/table[@class='gridtable']/tr[1]/td[1]", "innertext");
                if (_results.Count > 0)
                {
                    Res.Name = _results[0]; //姓名
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='rightbox']/table[@class='gridtable']/tr[1]/td[2]", "innertext");
                if (_results.Count > 0)
                {
                    Res.IdentityCard = _results[0];//身份证号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='rightbox']/table[@class='gridtable']/tr[2]/td[1]", "innertext");
                if (_results.Count > 0)
                {
                    Res.ProvidentFundNo = _results[0]; //公积金账号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='rightbox']/table[@class='gridtable']/tr[2]/td[2]", "innertext");
                if (_results.Count > 0)
                {
                    Res.CompanyNo = _results[0];//单位帐号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='rightbox']/table[@class='gridtable']/tr[3]/td[1]", "innertext");
                if (_results.Count > 0)
                {
                    Res.CompanyName = _results[0]; //单位名称
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='rightbox']/table[@class='gridtable']/tr[4]/td[1]", "innertext");
                if (_results.Count > 0)
                {
                    Res.OpenTime = Regtime(_results[0], "yyyy-MM-dd"); //开户日期
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='rightbox']/table[@class='gridtable']/tr[4]/td[2]", "innertext");
                if (_results.Count > 0)
                {
                    Res.Status = _results[0];//当前状态
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='rightbox']/table[@class='gridtable']/tr[5]/td[1]", "innertext");
                if (_results.Count > 0)
                {
                    Res.SalaryBase = _results[0].ToDecimal(0); //月缴基数
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='rightbox']/table[@class='gridtable']/tr[6]/td[1]", "innertext");
                if (_results.Count > 0)
                {
                    MatchCollection matchs = new Regex(@"[0-9.][0-9.]{0,3}").Matches(_results[0]);
                    if (matchs.Count == 2)
                    {
                        Res.PersonalMonthPayRate = matchs[0].Value.ToDecimal(0);//个人缴费比率
                        Res.CompanyMonthPayRate = matchs[1].Value.ToDecimal(0);//公司缴费比例
                        if (Res.SalaryBase > 0)
                        {
                            Res.PersonalMonthPayAmount = (matchs[0].Value.ToDecimal(0) * Res.SalaryBase).ToString("f2").ToDecimal(0);//个人月缴额
                            Res.CompanyMonthPayAmount = (matchs[1].Value.ToDecimal(0) * Res.SalaryBase).ToString("f2").ToDecimal(0);//公司月缴额
                        }
                    }
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='rightbox']/table[@class='gridtable']/tr[9]/td[2]", "innertext");
                if (_results.Count > 0)
                {
                    Res.TotalAmount = _results[0].ToDecimal(0); //余额
                }
                #endregion
                #region 第三步,缴费明细
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = baseUrl + "index_list.php";
                //2015
                HttpResult tempHttpResult;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    Encoding = Encoding.GetEncoding("gbk"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                tempHttpResult = httpHelper.GetHtml(httpItem);
                errorMsg = CommonFun.GetMidStr(tempHttpResult.Html, "alert(\"", "\");").Trim();
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                //Res = DealDetails(httpResult);
                _results = HtmlParser.GetResultFromParser(tempHttpResult.Html, "//div[@id='rightbox']/table[@class='gridtable']/a", "href");
                // 2014 2013 年
                cookies = CommonFun.GetCookieCollection(cookies, tempHttpResult.CookieCollection);
                foreach (var link in _results)
                {
                    Url = baseUrl + link;
                    if (string.IsNullOrEmpty(link) || (link.Contains("#") && !link.Contains(".")))
                    {
                        continue;
                    }
                    //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "GET",
                        Encoding = Encoding.GetEncoding("gbk"),
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\");").Trim();
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        Res.StatusDescription = errorMsg;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    Res = DealDetails(httpResult);
                }
                //2015
                Res = DealDetails(tempHttpResult);
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
        /// 处理缴费明细
        /// </summary>
        /// <param name="httpresult">httpresult</param>
        /// <returns>Res</returns>
        public NetSpider.Entity.Service.ProvidentFundQueryRes DealDetails(HttpResult httpresult)
        {
            _results = HtmlParser.GetResultFromParser(httpresult.Html, "//div[@id='rightbox']/table[2]/tr[position()>1]", "inner");
            foreach (string items in _results)
            {
                ProvidentFundDetail providentFundDetail = new ProvidentFundDetail();
                var tdRow = HtmlParser.GetResultFromParser(items, "//td", "text", true);
                if (tdRow.Count != 5)
                {
                    continue;
                }
                if (tdRow[1].IndexOf("汇缴", StringComparison.Ordinal) > -1)
                {
                    providentFundDetail.Description = tdRow[1];//描述
                    realTime = Nregnum.Replace(tdRow[1], "");
                    if (realTime.Length != 6)
                    {
                        realTime = Regtime(tdRow[0], "yyyyMM");
                    }
                    providentFundDetail.ProvidentFundTime = realTime;//应属年月
                    providentFundDetail.PayTime = Regtime(tdRow[0], "yyyy-MM-dd").ToDateTime();//缴费年月
                    providentFundDetail.PersonalPayAmount = (tdRow[3].ToDecimal(0) * perAccounting);//个人缴费金额
                    providentFundDetail.CompanyPayAmount = (tdRow[3].ToDecimal(0) * comAccounting);//企业缴费金额
                    providentFundDetail.ProvidentFundBase = (tdRow[3].ToDecimal(0) / totalRate);//缴费基数
                    providentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                    providentFundDetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                    PaymentMonths++;
                }
                else if (tdRow[0].IndexOf("本年", StringComparison.Ordinal) > -1)
                {
                    providentFundDetail.Description = tdRow[0] + Benniantime(tdRow[1]);//描述
                    providentFundDetail.PersonalPayAmount = Math.Abs(tdRow[2].ToDecimal(0) - tdRow[3].ToDecimal(0));//个人缴费金额
                    providentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                    providentFundDetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                }
                else if (tdRow[2].ToDecimal(0) > 0)
                {
                    providentFundDetail.Description = tdRow[1] + Regtime(tdRow[0], "yyyy-MM-dd");//描述
                    providentFundDetail.PersonalPayAmount = tdRow[2].ToDecimal(0);//个人缴费金额
                    providentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;//缴费标志
                    providentFundDetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;//缴费类型
                }
                else
                {//（补缴，结息etc，数据不精确，只做参考用）
                    providentFundDetail.Description = tdRow[1];//描述
                    providentFundDetail.PersonalPayAmount = Math.Abs(tdRow[2].ToDecimal(0) - tdRow[3].ToDecimal(0));//个人缴费金额
                    providentFundDetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                    providentFundDetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                }
                Res.ProvidentFundDetailList.Add(providentFundDetail);
            }
            return Res;
        }
        /// <summary>
        /// 格式化特定时间
        /// </summary>
        /// <param name="regTime">30-6月 -95</param>
        /// <param name="type">返回类型[yyyy-MM-dd,yyyyMM]</param>
        /// <returns>1995-06-30 或199506</returns>
        public string Regtime(string regTime, string type)
        {
            string str = string.Empty;
            string year = string.Empty;
            string month = string.Empty;
            string day = string.Empty;
            MatchCollection dates = times.Matches(regTime);
            if (dates.Count == 3)
            {
                if (int.Parse(dates[2].Value) < 50)
                {
                    year = "20" + dates[2].Value;
                }
                else
                {
                    year = "19" + dates[2].Value;
                }
                month = dates[1].Value.Length == 2 ? dates[1].Value : ("0" + dates[1].Value);
                day = dates[0].Value.Length == 2 ? dates[0].Value : ("0" + dates[0].Value);
                if (type == "yyyy-MM-dd")
                {
                    str = year + "-" + month + "-" + day;
                }
                else
                {
                    str = year + month;
                }
            }
            else
            {
                return null;
            }
            return str;
        }

        /// <summary>
        /// 本年合计 本年利息时间格式化
        /// </summary>
        /// <param name="timeinterval">01-7月-14/30-6月-15</param>
        /// <returns>(2014-07-01到2015-06-30)</returns>
        public string Benniantime(string timeinterval)
        {
            string str = string.Empty;
            MatchCollection matchs = new Regex(@"[0-9]{1,2}-[0-9]{1,2}[\u6708]-[0-9]{1,2}").Matches(timeinterval);
            if (matchs.Count == 2)
            {
                str = "(" + Regtime(matchs[0].Value, "yyyy-MM-dd") + "到" + Regtime(matchs[1].Value, "yyyy-MM-dd") + ")";
            }
            else
            {
                str = "";
            }
            return str;
        }
    }
}
