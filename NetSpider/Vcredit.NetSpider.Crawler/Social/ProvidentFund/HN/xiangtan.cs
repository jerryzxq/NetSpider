using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HN
{
    /// <summary>
    /// 湖南 湘潭 公积金
    /// </summary>
    public class xiangtan : IProvidentFundCrawler
    {
        #region properties
        private readonly IPluginHtmlParser _htmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件

        private const string BaseUrl = "http://110.53.148.39:9001/";
        private const string City = "hn_xiangtan";

        private CookieCollection _cookies = new CookieCollection();
        private readonly HttpHelper _httpHelper = new HttpHelper();
        private HttpResult _httpResult = null;
        private HttpItem _httpItem = null;
        #endregion

        #region 验证码数据初始化
        /// <summary>
        /// 验证码数据初始化
        /// </summary>
        /// <param name="fundReq"></param>
        /// <returns></returns>
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq = null)
        {
            var res = new VerCodeRes
            {
                Token = CommonFun.GetGuidID(),
                StatusCode = ServiceConsts.StatusCode_success
            };
            var verCodeUrl = BaseUrl + "jcaptcha?onlynum=true&time=" + DateTime.Now.ToString("yyyyMMddHHmmss");
            try
            {
                _httpItem = new HttpItem
                {
                    URL = verCodeUrl,
                    Method = "GET",
                    ResultType = ResultType.Byte,
                    CookieCollection = _cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
                if (_httpResult.StatusCode != HttpStatusCode.OK)
                {
                    res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    res.StatusCode = ServiceConsts.StatusCode_fail;

                    return res;
                }

                _cookies = CommonFun.GetCookieCollection(_cookies, _httpResult.CookieCollection);
                res.VerCodeBase64 = CommonFun.GetVercodeBase64(_httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(res.Token, _httpResult.ResultByte);
                res.VerCodeUrl = CommonFun.GetVercodeUrl(res.Token);

                res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                res.StatusCode = ServiceConsts.StatusCode_success;
                res.StatusDescription = City + ServiceConsts.ProvidentFund_InitSuccess;
                CacheHelper.SetCache(res.Token, _cookies);
            }
            catch (Exception e)
            {
                res.StatusCode = ServiceConsts.StatusCode_error;
                res.StatusDescription = City + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(City + ServiceConsts.ProvidentFund_InitError, e);
            }
            return res;
        }
        #endregion

        #region 登陆 查询 数据获取
        public ProvidentFundQueryRes ProvidentFundQuery(ProvidentFundReq fundReq)
        {
            var res = new ProvidentFundQueryRes
            {
                StatusCode = ServiceConsts.StatusCode_success
            };
            try
            {
                // 1. 缓存获取
                this.GetCatch(fundReq);

                // 2. 校验参数
                this.VerifyStep(res, fundReq);

                // 3. 登陆
                this.LoginStep(res, fundReq);

                // 4. 查询账号基础信息
                this.QueryUserInfoStep(res, fundReq);

                // 5. 查询缴费记录
                this.QueryPaymentRecords(res);

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

        #region 5. 查询缴费记录
        /// <summary>
        /// 5. 查询缴费记录
        /// </summary>
        /// <param name="res"></param>
        private void QueryPaymentRecords(ProvidentFundQueryRes res)
        {
            if (res.StatusCode != ServiceConsts.StatusCode_success)
                return;

            // 默认当前年度的数据
            var queryUrl = BaseUrl + "searchGrmx.do";
            _httpItem = new HttpItem
            {
                URL = queryUrl,
                Method = "GET",
                CookieCollection = _cookies,
                ResultCookieType = ResultCookieType.CookieCollection,
            };
            _httpResult = _httpHelper.GetHtml(_httpItem);

            var queryTrs = new List<string>();
            var years = new List<string>();
            if (_httpResult.StatusCode == HttpStatusCode.OK)
            {
                // 获取可查询年度
                years = _htmlParser.GetResultFromParser(_httpResult.Html, "//select[@class='sel']/option", "value");
                // 数据较多查询速度较慢，取最近5个年度
                years = years.OrderByDescending(x => x).Take(5).ToList();
                // 移除当前年度
                var currentyear = DateTime.Now.Year.ToString();
                if (years.Any())
                {
                    currentyear = years[0];
                    years.Remove(currentyear);
                }

                this.GetHtmlTrs(_httpResult.Html, queryTrs);
                this.GetYearPagingData(currentyear, queryTrs, _httpResult.Html);
            }

            // 其他年度的数据
            foreach (var year in years)
            {
                queryUrl = BaseUrl + "searchGrmx.do" + string.Format("?year={0}&pageFlag={1}", year, 0);
                _httpItem = new HttpItem
                {
                    URL = queryUrl,
                    Method = "GET",
                    CookieCollection = _cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);

                this.GetHtmlTrs(_httpResult.Html, queryTrs);
                this.GetYearPagingData(year, queryTrs, _httpResult.Html);
            }

            this.AnalysisData(queryTrs, res);
        }

        /// <summary>
        /// 对获取所有tr数据处理
        /// </summary>
        /// <param name="queryTrs"></param>
        /// <param name="res"></param>
        private void AnalysisData(List<string> queryTrs, ProvidentFundQueryRes res)
        {
            if (queryTrs == null || !queryTrs.Any()) return;

            var entities = (from textTr in queryTrs
                            select _htmlParser.GetResultFromParser(textTr, "//td")
                                into tds
                                where tds != null && tds.Any()
                                select new XiangTanFundQueryEntity
                                {
                                    EntryDate = tds[1].Replace("&nbsp;", "").Replace("-", ""),
                                    BelongDate = tds[2].Replace("&nbsp;", "").Replace("-", ""),
                                    Remark = tds[3].Replace("&nbsp;", ""),
                                    InComeAmount = tds[4].Replace("&nbsp;", "").ToDecimal(0),
                                    PayAmount = tds[5].Replace("&nbsp;", "").ToDecimal(0),
                                    Balances = tds[6].Replace("&nbsp;", "").ToDecimal(0)
                                }).ToList();

            if (!entities.Any()) return;

            // 所有月份
            var yearMonths = entities.GroupBy(x => x.BelongDate).Select(x => x.Key).OrderByDescending(x => x);
            if (!yearMonths.Any()) return;

            foreach (var yearMonth in yearMonths)
            {
                if (string.IsNullOrEmpty(yearMonth))
                    continue;

                // 当前年月数据
                var month = yearMonth;
                var data = entities.Where(x => x.BelongDate.Equals(month)).AsQueryable();
                if (!data.Any()) continue;

                var detail = new ProvidentFundDetail();
                // 没有缴费标志，默认为：正常
                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                detail.ProvidentFundTime = month;
                // 职工 缴费
                detail.PersonalPayAmount = data.Where(x => x.Remark.Contains(PayType.职工.ToString())).Sum(x => x.InComeAmount);
                var personPay = data.Where(x => x.Remark.Contains(PayType.职工.ToString()));
                if (personPay.Any())
                {
                    foreach (var entity in personPay)
                        detail.Description += string.Format("({0}：{1}，公积金余额:{2}) &nbsp;\r\n"
                            , entity.Remark
                            , entity.InComeAmount
                            , entity.Balances);
                }

                // 单位 缴费 
                detail.CompanyPayAmount = data.Where(x => x.Remark.Contains(PayType.单位.ToString())).Sum(x => x.InComeAmount);
                var companyPay = data.Where(x => x.Remark.Contains(PayType.单位.ToString()));
                if (companyPay.Any())
                {
                    foreach (var entity in companyPay)
                        detail.Description += string.Format("({0}：{1}，公积金余额:{2}) &nbsp;\r\n"
                            , entity.Remark
                            , entity.InComeAmount
                            , entity.Balances);
                }

                // 描述 
                var desces = data.Where(x => !x.Remark.Contains(PayType.单位.ToString()) && !x.Remark.Contains(PayType.职工.ToString()));
                if (desces.Any())
                {
                    foreach (var desc in desces)
                        detail.Description += string.Format("({0}：{1}，公积金余额:{2}) &nbsp;\r\n"
                            , desc.Remark
                            , desc.InComeAmount + desc.PayAmount
                            , desc.Balances);
                }

                res.ProvidentFundDetailList.Add(detail);
            }
        }

        /// <summary>
        /// 获取当年分页数据
        /// </summary>
        /// <param name="year"></param>
        /// <param name="queryTrs"></param>
        /// <param name="html"></param>
        private void GetYearPagingData(string year, List<string> queryTrs, string html)
        {
            // 判断是否多页
            var pages = _htmlParser.GetResultFromParser(html, "//div[@class='fenye']/font[1]");
            if (pages == null || !pages.Any()) return;

            for (var pageFlag = 1; pageFlag < pages[0].ToInt(0); pageFlag++)
            {
                var queryUrl = BaseUrl + "searchGrmx.do" + string.Format("?year={0}&pageFlag={1}", year, pageFlag);
                _httpItem = new HttpItem
                {
                    URL = queryUrl,
                    Method = "GET",
                    CookieCollection = _cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);

                this.GetHtmlTrs(_httpResult.Html, queryTrs);
            }
        }

        /// <summary>
        /// 获取html 可用tr内容
        /// </summary>
        /// <param name="html"></param>
        /// <param name="queryTrs"></param>
        private void GetHtmlTrs(string html, List<string> queryTrs)
        {
            // 数据tr
            var trs = _htmlParser.GetResultFromParser(html, "//table[@class='shuju']/tr");
            if (trs == null || !trs.Any()) return;

            trs.RemoveAt(0);
            queryTrs.AddRange(trs);
        }
        #endregion

        #region 4. 查询账号基础信息
        /// <summary>
        /// 4. 查询账号基础信息
        /// </summary>
        /// <param name="res"></param>
        /// <param name="fundReq"></param>
        private void QueryUserInfoStep(ProvidentFundQueryRes res, ProvidentFundReq fundReq)
        {
            if (res.StatusCode != ServiceConsts.StatusCode_success)
                return;

            var infoUrl = BaseUrl + string.Format("searchGrye.do?logon={0}", DateTime.Now.ToUniversalTime());
            _httpItem = new HttpItem
            {
                URL = infoUrl,
                Method = "GET",
                CookieCollection = _cookies,
                ResultCookieType = ResultCookieType.CookieCollection,
            };
            _httpResult = _httpHelper.GetHtml(_httpItem);
            if (_httpResult.StatusCode != HttpStatusCode.OK) return;

            var dataTds = _htmlParser.GetResultFromParser(_httpResult.Html, "//table[@class='shuju1']/tr/td");
            if (dataTds == null || !dataTds.Any()) return;

            res.ProvidentFundNo = fundReq.FundAccount;
            res.IdentityCard = fundReq.Identitycard;

            res.Name = dataTds[0];
            res.Loginname = dataTds[2];
            res.SalaryBase = dataTds[3].Replace("&nbsp;元", "").ToDecimal(0);
            res.TotalAmount = dataTds[5].Replace("&nbsp;元", "").ToDecimal(0);
            res.Status = dataTds[7];

            res.ProvidentFundCity = City;
        }
        #endregion

        #region 3. 登陆
        /// <summary>
        /// 3. 登陆
        /// </summary>
        /// <param name="res"></param>
        /// <param name="fundReq"></param>
        private void LoginStep(ProvidentFundQueryRes res, ProvidentFundReq fundReq)
        {
            if (res.StatusCode != ServiceConsts.StatusCode_success)
                return;

            var loginUrl = string.Format(BaseUrl + "searchPersonLogon.do?logon={0}"
                , Math.Round(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds) + new Random().Next(1, 10) * 10000);

            var postdata = string.Format("select=2&spidno={0}&spcardno={1}&sppassword={2}&rand={3}"
                , fundReq.Identitycard
                , fundReq.FundAccount
                , fundReq.Password
                , fundReq.Vercode);

            _httpItem = new HttpItem()
            {
                URL = loginUrl,
                Method = "POST",
                CookieCollection = _cookies,
                ResultCookieType = ResultCookieType.CookieCollection,
                Postdata = postdata
            };
            _httpResult = _httpHelper.GetHtml(_httpItem);

            // 判断是否登陆成功
            var errormgs = string.Empty;
            var title = _htmlParser.GetResultFromParser(_httpResult.Html, "//title[1]").FirstOrDefault();

            if (title == "公积金查询系统登陆")
            {
                var scriptText = _htmlParser.GetResultFromParser(_httpResult.Html, "//script[3]").FirstOrDefault();
                if (scriptText != null)
                {
                    var matches = Regex.Matches(scriptText, @"alert\([\s\S]*\)");
                    errormgs = matches.Count > 0 ? matches[0].Value.Replace("alert('", "").Replace("')", "") : string.Empty;
                }
            }

            // 登录失败
            if (_httpResult.StatusCode != HttpStatusCode.OK || !string.IsNullOrEmpty(errormgs))
            {
                res.StatusDescription = "登录失败，" + errormgs;
                res.StatusCode = ServiceConsts.StatusCode_fail;
            }
            // 登录成功
            else if (title == "职工查询")
            {
                // 设置请求cookie
                _cookies = CommonFun.GetCookieCollection(_cookies, _httpResult.CookieCollection);
            }
        }
        #endregion

        #region 2. 校验参数
        /// <summary>
        /// 2. 校验参数
        /// </summary>
        /// <param name="res"></param>
        /// <param name="fundReq"></param>
        private void VerifyStep(ProvidentFundQueryRes res, ProvidentFundReq fundReq)
        {
            if (res.StatusCode != ServiceConsts.StatusCode_success)
                return;

            if (!fundReq.Identitycard.IsEmpty() && !fundReq.FundAccount.IsEmpty() && !fundReq.Password.IsEmpty() && !fundReq.Vercode.IsEmpty())
                return;

            res.StatusDescription = "身份证号码、账号、密码、验证码不能为空";
            res.StatusCode = ServiceConsts.StatusCode_fail;
        }
        #endregion

        #region 1. 缓存获取
        /// <summary>
        /// 1. 缓存获取
        /// </summary>
        /// <param name="fundReq"></param>
        private void GetCatch(ProvidentFundReq fundReq)
        {
            if (string.IsNullOrEmpty(fundReq.Token) || SpiderCacheHelper.GetCache(fundReq.Token) == null) return;

            _cookies = (CookieCollection)SpiderCacheHelper.GetCache(fundReq.Token);
            SpiderCacheHelper.RemoveCache(fundReq.Token);
        }
        #endregion

        #endregion
    }

    #region 辅助类
    /// <summary>
    /// 类型
    /// </summary>
    internal enum PayType
    {
        职工,
        单位
    }

    internal class XiangTanFundQueryEntity
    {
        /// <summary>
        /// 入账日期
        /// </summary>
        public string EntryDate { get; set; }

        /// <summary>
        /// 汇缴年月
        /// </summary>
        public string BelongDate { get; set; }

        /// <summary>
        /// 摘要
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 收入
        /// </summary>
        public decimal InComeAmount { get; set; }

        /// <summary>
        /// 支出
        /// </summary>
        public decimal PayAmount { get; set; }

        /// <summary>
        /// 余额
        /// </summary>
        public decimal Balances { get; set; }

    }
    #endregion
}
