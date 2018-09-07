using System;
using System.Collections.Generic;
using System.Linq;
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
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.FJ
{
    /// <summary>
    /// 该网站（福州）只提供近2年的公积金数据
    /// </summary>
    public class fuzhou : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.fzzfgjj.com/";
        string fundCity = "fj_fuzhou";
        #endregion
        #region 私有变量
        string Url = string.Empty;
        private int PaymentMonths = 0;//连续缴费月数
        private decimal payRate = (decimal)0.08;
        decimal perAccounting = 0;//个人占比
        decimal comAccounting = 0;//公司占比
        decimal totalRate = 0;//总缴费比率
        Regex times = new Regex(@"[0-9][0-9]{0,3}");
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                vcRes.StatusDescription = fundCity + "无需初始化";
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
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
            List<string> results = new List<string>();
            string viewState = string.Empty;
            string postData = string.Empty;//post参数
            string loginType = string.Empty;//登录方式[0:身份证,1:公积金账号]
            string txtNumber = string.Empty;//登录账号
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.LoginType == "2")
                {
                    if (string.IsNullOrWhiteSpace(fundReq.Identitycard))
                    {
                        Res.StatusDescription = "请输入您的18位身份证号码";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    loginType = "0";
                    Res.IdentityCard = txtNumber = fundReq.Identitycard;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(fundReq.Username))
                    {
                        Res.StatusDescription = "请输入您的公积金账号";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    loginType = "1";
                    Res.ProvidentFundNo = txtNumber = fundReq.Username;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Password))
                {
                    Res.StatusDescription = "请输入密码";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登录

                DateTime endTime = DateTime.Now;//结束时间
                string beginTime = endTime.AddYears(-10).ToString("yyyy-01-01");//开始时间
                string hfFpid = string.Empty;
                string hfCode = string.Empty;
                #region 登陆方式页面初始化

                Url = baseUrl + "AcPerson.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                if (results.Count > 0)
                {
                    viewState = results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_InitFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //身份证登陆初始化
                if (loginType == "0")
                {
                    postData = string.Format("__EVENTTARGET=drpSearchType&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE={0}&drpZhongXin=0&drpSearchType={1}&txtNumber=&txtPassword=&txtBeginDate={2}&txtEndDate={3}&hfFpid=&hfCode=&hfpwd=", viewState.ToUrlEncode(), loginType, beginTime, endTime.ToString("yyyy-MM-dd"));
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "Post",
                        Postdata = postData,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    viewState =HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value")[0];
                }

                #endregion
                #region  登陆验证

                Url = baseUrl + "AcPerson.aspx";
                postData = string.Format("__EVENTTARGET=&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE={0}&drpZhongXin=0&drpSearchType={1}&txtNumber={2}&txtPassword={3}&btnSearch=%E6%9F%A5%E8%AF%A2&txtBeginDate={4}&txtEndDate={5}&hfFpid=&hfCode=&hfpwd=", viewState.ToUrlEncode(), loginType, txtNumber, fundReq.Password.ToUrlEncode(), beginTime, endTime.ToString("yyyy-MM-dd"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postData,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string errorMsc = CommonFun.GetMidStr(httpResult.Html, "alert('", "');");//登陆错误提示消息
                if (!string.IsNullOrEmpty(errorMsc))
                {
                    Res.StatusDescription = errorMsc;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value", true);
                if (results.Count > 0)
                {
                    viewState = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='hfFpid']", "value", true);
                if (results.Count > 0)
                {
                    hfFpid = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='hfCode']", "value", true);
                if (results.Count > 0)
                {
                    hfCode = results[0];
                }
                #endregion
                #endregion
                #region 第二步,获取基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='lblUserName']", "text", true);
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='lblCardId']", "text", true);
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证号
                }
                if (Res.PersonalMonthPayRate > 0 && Res.CompanyMonthPayRate > 0)
                {
                    totalRate = Res.PersonalMonthPayRate + Res.CompanyMonthPayRate;
                    perAccounting = (Res.PersonalMonthPayRate / totalRate);
                    comAccounting = (Res.CompanyMonthPayRate / totalRate);
                }
                else
                {
                    totalRate = (payRate) * 2;//0.16
                    perAccounting = comAccounting = 0.50M;
                }
                //对应公积金账号详情,跳转链接集【用户有多个公积金账号】
                List<string> detailHref = new List<string>();
                //身份证登陆
                if (loginType == "0")
                {
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='dlZhangHao']/tr[position()>1]/td/div", "inner", true);
                    if (results.Count > 0)
                    {
                        foreach (var item in results)
                        {
                            results = HtmlParser.GetResultFromParser(item, "//div[3]/span", "innertext", true);
                            if (results.Count > 0)
                            {
                                if (results[0].Contains("正常"))
                                {
                                    Res.Status = results[0]; //状态
                                    results = HtmlParser.GetResultFromParser(item, "//div[2]/a", "innertext", true);
                                    if (results.Count > 0)
                                    {
                                        Res.ProvidentFundNo = results[0]; //公积金账号
                                    }
                                    results = HtmlParser.GetResultFromParser(item, "//div[2]/a", "href", true);
                                    if (results.Count > 0)
                                    {
                                        detailHref.Add(CommonFun.GetMidStr(results[0], "_doPostBack('", "'").Trim());
                                    }
                                }
                            }
                        }
                        if (detailHref.Count < 1)
                        {
                            results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='dlZhangHao_ctl01_lblAccount']", "text", true);
                            if (results.Count > 0)
                            {
                                Res.ProvidentFundNo = results[0]; //公积金账号
                            }
                            results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='dlZhangHao_ctl01_lblFpaccstateDL']", "text", true);
                            if (results.Count > 0)
                            {
                                Res.Status = results[0]; //状态
                            }
                            results = HtmlParser.GetResultFromParser(httpResult.Html, "//a[@id='dlZhangHao_ctl01_lblmore']", "href", true);
                            if (results.Count > 0)
                            {
                                detailHref.Add(CommonFun.GetMidStr(results[0], "_doPostBack('", "'").Trim());
                            }
                        }
                    }
                }
                else
                {
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='lblCurrentAccount']", "text", true);
                    if (results.Count > 0)
                    {
                        Res.ProvidentFundNo = results[0]; //公积金账号
                    }
                    //缴费明细
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='gridView']/tr[position()>1]", "inner");
                    CommonProcessing(results, ref Res);
                }

                #endregion
                #region 第三步,获取公积金缴费详情

                if (loginType == "0")
                {//身份证号登陆方式详情
                    Url = baseUrl + "AcPerson.aspx";
                    foreach (var item in detailHref)
                    {
                        postData = string.Format("__EVENTTARGET={0}&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE={1}&drpZhongXin=0&drpSearchType={2}&txtNumber={3}&txtPassword=&txtBeginDate={4}&txtEndDate={5}&hfFpid={6}&hfCode={7}&hfpwd={8}", item, viewState.ToUrlEncode(Encoding.GetEncoding("utf-8")), loginType, fundReq.Identitycard, beginTime, endTime.ToString("yyyy-MM-dd"), hfFpid, hfCode, fundReq.Password.ToUrlEncode());
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "Post",
                            Postdata = postData,
                            Encoding = Encoding.GetEncoding("utf-8"),
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value", true);
                        if (results.Count > 0)
                        {
                            viewState = results[0];
                        }
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='gridView']/tr[position()>1]", "inner");
                        CommonProcessing(results, ref Res);
                    }
                }
                #endregion
                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }

        public void CommonProcessing(List<string> results, ref ProvidentFundQueryRes Res)
        {
            foreach (var itemRow in results)
            {
                List<string> tdRow = HtmlParser.GetResultFromParser(itemRow, "//td", "text", true);
                if (tdRow.Count != 6) continue;
                string fundTime = string.Empty;
                if (tdRow[1].IndexOf("汇缴", StringComparison.Ordinal) > -1 && tdRow[1].Trim() != "汇缴")
                {
                    fundTime = Regtime(tdRow[1]); //应属年月
                }
                else
                {
                    fundTime = Convert.ToDateTime(tdRow[0]).ToString(Consts.DateFormatString7); //应属年月
                }
                ProvidentFundDetail detail = Res.ProvidentFundDetailList.FirstOrDefault(o => o.ProvidentFundTime == fundTime && !string.IsNullOrEmpty(fundTime) && o.PaymentType.IndexOf("汇缴") > -1);
                bool isSave = false;
                if (detail == null)
                {
                    isSave = true;
                    detail = new ProvidentFundDetail();
                    detail.PayTime = tdRow[0].ToDateTime(); //缴费年月
                    detail.ProvidentFundTime = fundTime;
                    detail.Description = tdRow[1].Trim(); //描述
                    if (tdRow[1].Contains("汇缴"))
                    {
                        detail.PersonalPayAmount = (tdRow[2].ToDecimal(0) * perAccounting).ToString("f2").ToDecimal(0);
                        //个人缴费金额
                        detail.CompanyPayAmount = (tdRow[2].ToDecimal(0) * comAccounting).ToString("f2").ToDecimal(0);
                        //企业缴费金额
                        detail.ProvidentFundBase = (tdRow[2].ToDecimal(0) / (totalRate)).ToString("f2").ToDecimal(0);
                        //缴费基数
                        detail.PaymentType = tdRow[1].Trim();
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                    }
                    else if (tdRow[1].Contains("支取"))
                    {
                        detail.PersonalPayAmount = tdRow[2].ToDecimal(0); //个人缴费金额
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                    }
                    else
                    {//（补缴，结息etc，数据不精确，只做参考用）
                        detail.PersonalPayAmount = tdRow[2].ToDecimal(0); //个人缴费金额
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                    }
                }
                else
                {
                    if (tdRow[1].Contains("汇缴"))
                    {
                        detail.PersonalPayAmount += (tdRow[2].ToDecimal(0) * perAccounting);//个人缴费金额
                        detail.CompanyPayAmount += (tdRow[2].ToDecimal(0) * comAccounting);//企业缴费金额
                    }
                    else
                    {
                        detail.PersonalPayAmount += tdRow[2].ToDecimal(0);//个人缴费金额
                    }
                    //描述
                    detail.Description = detail.Description.IndexOf(tdRow[1].Trim()) > -1 ? detail.Description : detail.Description + ";" + tdRow[1].Trim();
                }
                if (isSave)
                {
                    Res.ProvidentFundDetailList.Add(detail);
                }
            }
        }
        /// <summary>
        /// 格式化特定时间
        /// </summary>
        /// <param name="regTime">汇缴1210;汇缴201210;</param>
        /// <returns>201210</returns>
        public string Regtime(string regTime)
        {
            string str;
            MatchCollection dates = times.Matches(regTime);
            switch (@dates.Count)
            {
                case 1:
                    string temp1 = dates[0].Value.Substring(0, 2);
                    if (int.Parse(temp1) < 50)
                    {
                        str = "20" + dates[0].Value;
                    }
                    else
                    {
                        str = "19" + dates[0].Value;
                    }
                    break;
                case 2:
                    str = dates[0].Value + dates[1].Value;
                    break;
                default:
                    str = null;
                    break;

            }
            return str;
        }
    }
}
