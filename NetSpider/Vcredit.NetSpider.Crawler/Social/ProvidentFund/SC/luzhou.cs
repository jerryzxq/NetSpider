using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using System.Text.RegularExpressions;
using System.Web;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SC
{
    /// <summary>
    /// 四川--泸州--公积金
    /// </summary>
    public class luzhou : IProvidentFundCrawler
    {
        #region property
        private readonly IPluginHtmlParser _htmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件

        private const string BaseUrl = "http://182.130.246.152:7001/";
        private const string City = "sc_luzhou";

        private CookieCollection _cookies = new CookieCollection();
        private readonly HttpHelper _httpHelper = new HttpHelper();
        private HttpResult _httpResult = null;
        private HttpItem _httpItem = null;
        #endregion

        #region 获取验证码数据
        /// <summary>
        /// 获取验证码数据，本例没有验证码，返回null;
        /// </summary>
        /// <param name="fundReq"></param>
        /// <returns></returns>
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            var res = new VerCodeRes();
            res.StatusCode = ServiceConsts.StatusCode_success;
            return res;
        }
        #endregion

        #region 登录、查询等业务数据获取
        /// <summary>
        /// 登录、查询等业务数据获取
        /// </summary>
        /// <param name="fundReq"></param>
        /// <returns></returns>
        public ProvidentFundQueryRes ProvidentFundQuery(ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes res = new ProvidentFundQueryRes
            {
                StatusCode = ServiceConsts.StatusCode_success
            };
            try
            {
                //获取缓存
                if (string.IsNullOrEmpty(fundReq.Token) && SpiderCacheHelper.GetCache(fundReq.Token) != null)
                {
                    _cookies = (CookieCollection)SpiderCacheHelper.GetCache(fundReq.Token);
                    SpiderCacheHelper.RemoveCache(fundReq.Token);
                }

                // 1. 校验参数
                this.VerifyStep(res, fundReq);

                // 2. 登陆
                var queryParam = string.Empty;
                this.LoginStep(res, fundReq, ref queryParam);

                // 3. 查询账号基础信息
                this.QueryUserInfoStep(res, fundReq, queryParam);

                // 4. 查询缴费记录
                this.QueryPaymentRecords(res, queryParam);

                if (res.StatusCode == ServiceConsts.StatusCode_success)
                    res.StatusDescription = City + ServiceConsts.ProvidentFund_QuerySuccess;
            }
            catch (Exception e)
            {
                res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(City + ServiceConsts.ProvidentFund_QueryError, e);
            }

            return res;
        }

        #region 4. 查询缴费记录

        /// <summary>
        /// 4. 查询缴费记录
        /// </summary>
        /// <param name="res"></param>
        /// <param name="queryParam"></param>
        private void QueryPaymentRecords(ProvidentFundQueryRes res, string queryParam)
        {
            if (res.StatusCode != ServiceConsts.StatusCode_success)
                return;

            var queryPaymentRecordsUrl = BaseUrl + "zfbzgl/gjjmxcx/gjjmx_cx.jsp";
            _httpItem = new HttpItem()
            {
                URL = queryPaymentRecordsUrl,
                Method = "POST",
                CookieCollection = _cookies,
                ResultCookieType = ResultCookieType.CookieCollection,
                Postdata = queryParam + "&flag=1&ys=1", // flag 变量未知默认值为1，ys代表页数，首先抓取第一页数据
                ContentType = "application/x-www-form-urlencoded; charset=gbk",
            };
            _httpResult = _httpHelper.GetHtml(_httpItem);
            // 根据页面内容获取总页数
            var totalCount = 1;
            var counts = _htmlParser.GetResultFromParser(_httpResult.Html, "//input[@name='intPageCount']", "value");
            if (counts != null && counts.Any())
                totalCount = counts[0].ToInt(1);

            // 第一页数据
            this.AnalysisForFundDetail(res);
            // 从第二页逐页获取
            for (var ys = 2; ys <= totalCount; ys++)
            {
                _httpItem = new HttpItem()
                {
                    URL = queryPaymentRecordsUrl,
                    Method = "POST",
                    CookieCollection = _cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                    Postdata = queryParam + "&flag=1&ys=" + ys,
                    ContentType = "application/x-www-form-urlencoded; charset=gbk",
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
                this.AnalysisForFundDetail(res);
            }

            // 详细数据重新排序
            res.ProvidentFundDetailList =
                res.ProvidentFundDetailList.OrderByDescending(x => x.ProvidentFundTime).ToList();
        }
        /// <summary>
        /// 解析HTML 获取缴费数据集合
        /// </summary>
        /// <param name="res"></param>
        private void AnalysisForFundDetail(ProvidentFundQueryRes res)
        {
            var resulTrData = _htmlParser.GetResultFromParser(_httpResult.Html, "//html/body/table[2]/tr/td[2]/table[4]/tr/td[1]/table/tr", "");
            for (var i = 1; i < resulTrData.Count; i++)
            {
                var tdValues = _htmlParser.GetResultFromParser(resulTrData[i], "//td", "");
                if (tdValues == null || !tdValues.Any()) continue;
                var date = tdValues[0];
                if (date.IsEmpty()) continue;

                var detail = new ProvidentFundDetail();
                detail.PayTime = date.ToDateTime();
                detail.ProvidentFundTime = this.GetProvidentFundTime(tdValues[1]); // 日期格式为：201503

                if (tdValues[1].Contains("汇缴"))
                {
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                }
                else
                {
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                }

                detail.Description = "缴费总金额：" + tdValues[3];

                res.ProvidentFundDetailList.Add(detail);
            }
        }
        /// <summary>
        /// 应属年月
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetProvidentFundTime(string value)
        {
            MatchCollection matches = Regex.Matches(value, @"[0-9]{4}年[0-9]{2}月");
            var result = (matches.Count > 0 ? matches[0].Value : string.Empty).Replace("年", "").Replace("月", "");

            return result;
        }
        #endregion

        #region 3. 获取用户信息
        /// <summary>
        /// 3. 获取用户信息
        /// </summary>
        /// <param name="res"></param>
        /// <param name="fundReq"></param>
        /// <param name="queryParam"></param>
        private void QueryUserInfoStep(ProvidentFundQueryRes res, ProvidentFundReq fundReq, string queryParam)
        {
            if (res.StatusCode != ServiceConsts.StatusCode_success)
                return;

            var queryInfoUrl = BaseUrl + "zfbzgl/gjjxxcx/gjjxx_cx.jsp?" + queryParam;
            _httpItem = new HttpItem()
            {
                URL = queryInfoUrl,
                Method = "get",
                CookieCollection = _cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            _httpResult = _httpHelper.GetHtml(_httpItem);
            // 返回页面表单数据集
            var resulFontData = _htmlParser.GetResultFromParser(_httpResult.Html
                , "//html/body/table[2]/tr/td[2]/table[4]/tr/td/table[1]/tr/td/table[1]/tr/td/font", "");
            if (resulFontData == null || !resulFontData.Any()) return;
            
            res.IdentityCard = fundReq.Identitycard;
            res.EmployeeNo = fundReq.FundAccount;

            res.Name = resulFontData[0].Replace("&nbsp;", "");
            res.CompanyName = resulFontData[4].Replace("&nbsp;", "");
            res.CompanyDistrict = resulFontData[5].Replace("&nbsp;", "");
            res.OpenTime = resulFontData[6].Replace("&nbsp;", "");
            res.Status = resulFontData[7].Replace("&nbsp;", "");
            res.SalaryBase = resulFontData[8].Replace("&nbsp;", "").ToDecimal(0);

            res.PersonalMonthPayRate = this.GetMonthPayRate(resulFontData[9].Replace("&nbsp;", ""), 0);
            res.CompanyMonthPayRate = this.GetMonthPayRate(resulFontData[9].Replace("&nbsp;", ""), 1);

            res.PersonalMonthPayAmount = this.GetMonthPayAmount(resulFontData[10].Replace("&nbsp;", "").ToDecimal(0), res.PersonalMonthPayRate, res.PersonalMonthPayRate + res.CompanyMonthPayRate);
            res.CompanyMonthPayAmount = this.GetMonthPayAmount(resulFontData[10].Replace("&nbsp;", "").ToDecimal(0), res.CompanyMonthPayRate, res.PersonalMonthPayRate + res.CompanyMonthPayRate);

            res.TotalAmount = resulFontData[18].Replace("&nbsp;", "").ToDecimal(0);

            res.ProvidentFundCity = City;
        }

        /// <summary>
        /// 计算缴费金额
        /// </summary>
        /// <param name="totalMonthPayAmount"></param>
        /// <param name="rate"></param>
        /// <param name="totalRate"></param>
        /// <returns></returns>
        private decimal GetMonthPayAmount(decimal totalMonthPayAmount, decimal rate, decimal totalRate)
        {
            return decimal.Round(totalMonthPayAmount * rate / totalRate, 2);
        }

        /// <summary>
        /// 计算缴费比例
        /// </summary>
        /// <param name="payRate"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private decimal GetMonthPayRate(string payRate, int index)
        {
            decimal result = 0;
            string[] payRates = payRate.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (payRates.Length > index)
            {
                result = payRates[index].Replace("%", "").ToDecimal(0) / 100;
            }
            return result;
        }
        #endregion

        #region 2. 登陆
        /// <summary>
        /// 2. 登陆
        /// </summary>
        /// <param name="res"></param>
        /// <param name="fundReq"></param>
        /// <param name="queryParam"></param>
        private void LoginStep(ProvidentFundQueryRes res, ProvidentFundReq fundReq, ref string queryParam)
        {
            if (res.StatusCode != ServiceConsts.StatusCode_success)
                return;

            var loginUrl = string.Format(BaseUrl + "zfbzgl/zfbzsq/login_hidden.jsp?password={0}&sfzh={1}&cxyd=当前年度&zgzh={2}"
                     , fundReq.Password
                     , fundReq.Identitycard
                     , fundReq.FundAccount);

            _httpItem = new HttpItem()
            {
                URL = loginUrl,
                Method = "get",
                CookieCollection = _cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            _httpResult = _httpHelper.GetHtml(_httpItem);
            // 根据返回html 可通过script标签内容判断是否登陆成功
            var loginResults = _htmlParser.GetResultFromParser(_httpResult.Html, "//script", "");

            var errorResult = new List<string>();
            // 返回结构可能有多个script标签
            foreach (var scriptText in loginResults.Where(scriptText => !scriptText.IsEmpty()))
            {
                // 登录失败，获取错误信息
                if (scriptText.Contains("login.jsp"))
                {
                    var matches = Regex.Matches(scriptText, @"alert[\s\S]*\)");
                    errorResult.AddRange(from Match match in matches select match.Value.Replace("alert(\"", "").Replace("\")", ""));
                }
                // 登录成功
                else if (scriptText.Contains("main_menu.jsp"))
                {
                    // 获取用户姓名，需要用户姓名作为请求参数
                    var userName = _htmlParser.GetResultFromParser(_httpResult.Html, "//input[@name='zgxm']", "value");
                    // 该系统登录之后，设置页面请求的请求参数
                    var dwbm = _htmlParser.GetResultFromParser(_httpResult.Html, "//input[@name='dwbm']", "value");
                    queryParam = string.Format("sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd={4}"
                        , fundReq.Identitycard
                        , CommonFun.ConvertToGbk(userName.Any() ? userName[0] : string.Empty)  // 中文GBK格式
                        , fundReq.FundAccount
                        , dwbm.Any() ? dwbm[0] : string.Empty
                        , CommonFun.ConvertToGbk("当前年度")); // 中文GBK格式

                }
            }

            // 登录失败
            if (_httpResult.StatusCode != HttpStatusCode.OK || errorResult.Count > 0)
            {
                res.StatusDescription = "登录失败，" + string.Join("，", errorResult);
                res.StatusCode = ServiceConsts.StatusCode_fail;
            }
            // 登录成功
            else
            {
                // 设置请求cookie
                _cookies = CommonFun.GetCookieCollection(_cookies, _httpResult.CookieCollection);
            }
        }
        #endregion

        #region 1. 校验参数
        /// <summary>
        /// 1. 校验参数
        /// </summary>
        /// <param name="res"></param>
        /// <param name="fundReq"></param>
        private void VerifyStep(ProvidentFundQueryRes res, ProvidentFundReq fundReq)
        {
            if (res.StatusCode != ServiceConsts.StatusCode_success)
                return;

            if (fundReq.Identitycard.IsEmpty() || fundReq.FundAccount.IsEmpty() || fundReq.Password.IsEmpty())
            {
                res.StatusDescription = "身份证号码、账号、密码不能为空";
                res.StatusCode = ServiceConsts.StatusCode_fail;
            }
        }
        #endregion

        #endregion
    }
}
