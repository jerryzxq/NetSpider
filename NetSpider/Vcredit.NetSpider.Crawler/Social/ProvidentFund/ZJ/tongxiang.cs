using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
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
    /// <summary>
    /// 5566
    /// </summary>
    public class tongxiang : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string _url = string.Empty;
        string _postData = string.Empty;
        List<string> _results = new List<string>();
        private int PaymentMonths = 0;
        #endregion
        #region 私有变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        int startYear;//开始年份
        int beginYear;//开始年份
        int beginMonth;//开始年份
        int endYear = DateTime.Now.Year;//结束年份
        int endMonth = DateTime.Now.Month;//结束月份
        int totalPages = 0;//总页数
        string baseUrl = "http://www.txgjj.com/";
        string fundCity = "zj_tongxiang";
        string viewState = string.Empty;
        Regex reg = new Regex(@"[\&nbsp;\%\,\s]*");
        Regex times = new Regex(@"[0-9][0-9]{0,4}");
        private decimal payRate = (decimal)0.08;
        decimal perAccounting;//个人占比
        decimal comAccounting;//公司占比
        decimal totalRate;//总缴费比率
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = "所选城市无需初始化";
                CacheHelper.SetCache(token, cookies);
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
            Res.ProvidentFundCity = fundCity;
            string leftBtnLogin = string.Empty;
            string selectId = string.Empty;
            string txtKey = string.Empty;
            try
            {
                //校验参数
                if (string.IsNullOrWhiteSpace(fundReq.Username))
                {
                    Res.StatusDescription = "请输入用户名";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Password))
                {
                    Res.StatusDescription = "请输入密码";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 初始化页面

                _url = baseUrl + "Default.aspx";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "GET",
                    Encoding = Encoding.GetEncoding("gb2312"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='__VIEWSTATE']", "value");
                if (_results.Count > 0)
                {
                    viewState = _results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='left:btnLogin']", "value");
                if (_results.Count > 0)
                {
                    leftBtnLogin = _results[0];
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@id='selectid']/option", "value");
                if (_results.Count > 0)
                {
                    selectId = _results[0];
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='txtKey']", "value");
                if (_results.Count > 0)
                {
                    txtKey = _results[0];
                }
               
                #endregion
                #region 第一步,登陆

                _url = baseUrl + "Default.aspx";
                _postData = string.Format("__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE={0}&left%3AtxtUserName={1}&left%3AtxtPassword={2}&left%3AbtnLogin={3}&left%3AdrpFriend=&selectid={4}&txtKey={5}", viewState.ToUrlEncode(Encoding.GetEncoding("gb2312")), fundReq.Username, fundReq.Password, leftBtnLogin.ToUrlEncode(Encoding.GetEncoding("gb2312")), selectId, txtKey.ToUrlEncode(Encoding.GetEncoding("gb2312")));
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "Post",
                    Referer = baseUrl + "Show1Ad.aspx",
                    Postdata = _postData,
                    Encoding = Encoding.GetEncoding("gb2312"),
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
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "')</script>").Trim();
                if (!string.IsNullOrWhiteSpace(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                #region 第二步,获取基本信息

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                _url = baseUrl + "Member/Personal/PInfo.aspx";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "GET",
                    Encoding = Encoding.GetEncoding("gb2312"),
                    Referer = baseUrl + "Member/Member.aspx",
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
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@width='650']/table[@bgcolor='#4e9ed9']/tr[1]/td[2]", "text");
                Res.Name = _results.Count > 0 ? reg.Replace(_results[0], "") : null;//姓名
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@width='650']/table[@bgcolor='#4e9ed9']/tr[1]/td[4]", "text");
                Res.ProvidentFundNo = _results.Count > 0 ? reg.Replace(_results[0], "") : null;//个人公积金帐号
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@width='650']/table[@bgcolor='#4e9ed9']/tr[2]/td[2]", "text");
                Res.CompanyName = _results.Count > 0 ? reg.Replace(_results[0], "") : null;//单位名称
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@width='650']/table[@bgcolor='#4e9ed9']/tr[2]/td[4]", "text");
                Res.CompanyNo = _results.Count > 0 ? reg.Replace(_results[0], "") : null;//单位公积金帐号
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@width='650']/table[@bgcolor='#4e9ed9']/tr[3]/td[2]", "text");
                Res.Status = _results.Count > 0 ? reg.Replace(_results[0], "") : null;//状态标志
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@width='650']/table[@bgcolor='#4e9ed9']/tr[3]/td[4]", "text");
                Res.SalaryBase = _results.Count > 0 ? reg.Replace(_results[0], "").ToDecimal(0) : decimal.Zero;//缴存工资基数
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@width='650']/table[@bgcolor='#4e9ed9']/tr[4]/td[2]", "text");
                Res.CompanyMonthPayRate = _results.Count > 0 ? reg.Replace(_results[0], "").ToDecimal(0) * 0.01M : decimal.Zero;//单位交缴率
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@width='650']/table[@bgcolor='#4e9ed9']/tr[4]/td[4]", "text");
                Res.CompanyMonthPayAmount = _results.Count > 0 ? reg.Replace(_results[0], "").ToDecimal(0) : decimal.Zero;//单位缴交额
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@width='650']/table[@bgcolor='#4e9ed9']/tr[5]/td[2]", "text");
                Res.PersonalMonthPayRate = _results.Count > 0 ? reg.Replace(_results[0], "").ToDecimal(0) * 0.01M : decimal.Zero;//个人交缴率
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@width='650']/table[@bgcolor='#4e9ed9']/tr[5]/td[4]", "text");
                Res.PersonalMonthPayAmount = _results.Count > 0 ? reg.Replace(_results[0], "").ToDecimal(0) : decimal.Zero;//个人缴交额
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@width='650']/table[@bgcolor='#4e9ed9']/tr[7]/td[2]", "text");
                Res.IdentityCard = _results.Count > 0 ? reg.Replace(_results[0], "") : null;//身份证号码
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@width='650']/table[@bgcolor='#4e9ed9']/tr[9]/td[2]", "text");
                Res.TotalAmount = _results.Count > 0 ? reg.Replace(_results[0], "").ToDecimal(0) : decimal.Zero;//当前余额
                #endregion
                #region 第三步,公积金缴费明细

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
                #region 目前明细版本

                //最近一年明细
                _url = baseUrl + "Member/Personal/PPayDetail.aspx";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Host = "www.txgjj.com",
                    Referer = baseUrl+"Member/Member.aspx",
                    Encoding = Encoding.GetEncoding("gb2312"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                List<string> results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='dgd']/tr[position()>1]", "", true);
                viewState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='__VIEWSTATE']", "value")[0];
                //剩余年份
                List<string> yearList = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@id='ddlYear']/option[position()>1]", "value");
                foreach (string year in yearList)
                {
                   // _url = baseUrl + "Member/Personal/PPayDetail.aspx";
                    _postData = string.Format("__VIEWSTATE={0}&ddlYear={1}&btnSearch=%B2%E9+%D1%AF", viewState.ToUrlEncode(Encoding.GetEncoding("gb2312")), year);
                    httpItem = new HttpItem()
                    {
                        URL = _url,
                        Method="POST",
                        Postdata=_postData,
                        Host = "www.txgjj.com",
                        Referer = baseUrl + "Member/Personal/PPayDetail.aspx",
                        Encoding = Encoding.GetEncoding("gb2312"),
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    viewState = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='__VIEWSTATE']", "value")[0];
                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='dgd']/tr[position()>1]", "", true));
                }
                //保存明细
                foreach (string s in results)
                {
                    ProvidentFundDetail detail = new ProvidentFundDetail();
                    List<string> tdRow = HtmlParser.GetResultFromParser(s, "//td");
                    if (tdRow.Count != 4) continue;
                    if (tdRow[1].IndexOf("已缴", StringComparison.Ordinal) > -1)
                    {
                        detail.Description = tdRow[1];//描述
                        detail.PayTime = tdRow[2].ToDateTime();//缴费年月
                        detail.ProvidentFundTime = DateTime.ParseExact(tdRow[2].Substring(0, 4) + tdRow[0],"yyyyM",null).ToString(Consts.DateFormatString7);
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0) * perAccounting;//个人缴费金额
                        detail.CompanyPayAmount = tdRow[3].ToDecimal(0) * comAccounting;//企业缴费金额
                        detail.ProvidentFundBase = tdRow[3].ToDecimal(0) / totalRate;//基本薪资
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        PaymentMonths++;
                    }
                    else
                    {
                        detail.Description = tdRow[1]+"(月份:"+tdRow[0]+")";//描述
                        detail.PayTime = tdRow[2].ToDateTime();//缴费年月
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0) ;//个人缴费金额
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion
                #region 2016年4月13日之前明细查询版本

                //_url = baseUrl + "Member/Personal/PBillDetail.aspx";
                //httpItem = new HttpItem()
                //{
                //    Referer = baseUrl + "Member/Personal/PInfo.aspx",
                //    URL = _url,
                //    Method = "get",
                //    Encoding = Encoding.GetEncoding("gb2312"),
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //_results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='__VIEWSTATE']", "value");
                //if (_results.Count > 0)
                //{
                //    viewState = _results[0];
                //}
                //endYear = DateTime.Now.Year;
                //endMonth = DateTime.Now.Month;
                //_results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@id='ddlBeginYear']/option", "value");
                //if (_results.Count > 0)
                //{
                //    int temp1 = int.Parse(_results[0]); //【年份递增】2014
                //    int temp2 = (endYear - 5); //2015-5
                //    startYear = temp1 >= temp2 ? temp1 : temp2;
                //}
                //else
                //{
                //    startYear = endYear - 5;
                //}
                //HttpPost(_url, _postData, 2);
                //_results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='__VIEWSTATE']", "value");
                //if (_results.Count > 0)
                //{
                //    viewState = _results[0];
                //}
                //_results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='pager_lblPageCount']", "text");
                //if (_results.Count > 0)
                //{
                //    totalPages = CommonFun.ClearFlag(_results[0]).ToInt(0);
                //}
                //if (totalPages > 1)
                //{
                //    HttpPost(_url, _postData, totalPages);
                //}
                //处理剩余日期结束
                #endregion
                #endregion
                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return Res;
        }
        /// <summary>
        /// 页面剩余页码获取
        /// </summary>
        /// <param name="url">链接地址</param>
        /// <param name="_postData">数据字符串</param>
        /// <param name="totalPages">总页数</param>
        private void HttpPost(string url, string _postData, int totalPages)
        {
            for (int i = 2; i <= totalPages; i++)
            {
                _postData = string.Format("__EVENTTARGET=pager%3AbtnNext&__EVENTARGUMENT=&__VIEWSTATE={0}&ddlBeginYear={1}&ddlBeginMonth={2}&ddlEndYear={3}&ddlEndMonth={4}", viewState.ToUrlEncode(Encoding.GetEncoding("gb2312")), startYear, "01", endYear, "0" + endMonth);
                httpItem = new HttpItem()
                {
                    URL = url,
                    Referer = url,
                    Method = "Post",
                    Postdata = _postData,
                    Encoding = Encoding.GetEncoding("gb2312"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='__VIEWSTATE']", "value");
                if (_results.Count > 0)
                {
                    viewState = _results[0];
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                DealWithHttpResults(httpResult);
            }
        }
        /// <summary>
        /// 处理请求页面
        /// </summary>
        /// <param name="httpResult">http返回类</param>
        /// <returns>Res</returns>
        private NetSpider.Entity.Service.ProvidentFundQueryRes DealWithHttpResults(HttpResult httpResult)
        {
            Regex regM = new Regex(@"[/\&nbsp;\,;\u4e00-\u9fa5;\s]*");
            List<string> results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='dgd']/tr[position()>1]", "inner");
            foreach (string item in results)
            {
                ProvidentFundDetail detail = new ProvidentFundDetail();
                List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                if (tdRow.Count != 7)
                {
                    continue;
                }
                detail.Description = tdRow[3];//描述
                detail.PayTime = tdRow[2].ToDateTime();//缴费年月
                if (tdRow[3].IndexOf("汇缴", StringComparison.Ordinal) > -1)
                {
                    detail.ProvidentFundTime = FormatTime(tdRow[3], "yyyyMM");//应属年月
                    detail.PersonalPayAmount = (regM.Replace(tdRow[4], "").ToDecimal(0) * perAccounting).ToString("f2").ToDecimal(0);//个人缴费金额
                    detail.CompanyPayAmount = (regM.Replace(tdRow[4], "").ToDecimal(0) * comAccounting).ToString("f2").ToDecimal(0);//企业缴费金额
                    detail.ProvidentFundBase = (regM.Replace(tdRow[4], "").ToDecimal(0) / totalRate).ToString("f2").ToDecimal(0);//基本薪资
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                    PaymentMonths++;
                }
                else if (tdRow[3].IndexOf("余额", StringComparison.Ordinal) > -1)
                {
                    detail.PersonalPayAmount = regM.Replace(tdRow[6], "").ToDecimal(0);//个人缴费金额
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                }
                else
                {//（补缴，结息etc，数据不精确，只做参考用）
                    detail.PersonalPayAmount = Math.Abs(regM.Replace(tdRow[4], "").ToDecimal(0) - regM.Replace(tdRow[5], "").ToDecimal(0));//个人缴费金额
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                }
                Res.ProvidentFundDetailList.Add(detail);
            }
            return Res;
        }
        /// <summary>
        /// 格式化特定时间
        /// </summary>
        /// <param name="strtime">托收汇缴(2014年8月)</param>
        /// <param name="type">返回类型[yyyyMM]</param>
        /// <returns>201408</returns>
        public string FormatTime(string strtime, string type)
        {
            string str = string.Empty;
            string year = string.Empty;
            string month = string.Empty;
            //string day = string.Empty;
            MatchCollection dates = times.Matches(strtime);
            if (dates.Count == 2)
            {
                year = dates[0].Value;
                month = dates[1].Value.Length == 2 ? dates[1].Value : ("0" + dates[1].Value);
                switch (type)
                {
                    case "yyyyMM":
                        str = year + month;
                        break;
                    default:
                        str = "";
                        break;
                }
            }
            else
            {
                return null;
            }
            return str;
        }
    }
}
