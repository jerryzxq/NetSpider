using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.HN
{
    /// <summary>
    /// 湖南 湘潭
    /// </summary>
    public class xiangtan : ISocialSecurityCrawler
    {
        #region properties
        private readonly IPluginHtmlParser _htmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        private const string _baseUrl = "http://www.xtrs.gov.cn/xtSS/person";
        private const string _socialCity = "hn_xiangtan";

        private readonly HttpHelper _httpHelper = new HttpHelper();
        private CookieCollection _cookies = new CookieCollection();
        private HttpResult _httpResult;
        private HttpItem _httpItem;
        #endregion

        #region 登录初始化，验证码获取
        /// <summary>
        /// 登录初始化，验证码获取
        /// </summary>
        /// <returns></returns>
        public VerCodeRes SocialSecurityInit()
        {
            var res = new VerCodeRes
            {
                Token = CommonFun.GetGuidID(),
            };
            try
            {
                // 验证码地址 Include/code.asp?time=
                var getCodeUrl = _baseUrl + "/jgylZzInfo/frame/randomPicture.action?time=" + DateTime.Now.ToString("yyyyMMddHHmmss");
                _httpItem = new HttpItem()
                {
                    URL = getCodeUrl,
                    Method = "get",
                    ResultType = ResultType.Byte,
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

                res.StatusDescription = _socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                res.StatusCode = ServiceConsts.StatusCode_success;

                SpiderCacheHelper.SetCache(res.Token, _cookies);
            }
            catch (Exception e)
            {
                res.StatusCode = ServiceConsts.StatusCode_error;
                res.StatusDescription = _socialCity + ServiceConsts.SocialSecurity_InitError;
                Log4netAdapter.WriteError(_socialCity + ServiceConsts.SocialSecurity_InitError, e);
            }
            return res;
        }
        #endregion

        #region 登录、查询等业务数据获取

        /// <summary>
        /// 登录、查询等业务数据获取
        /// </summary>
        /// <param name="socialReq"></param>
        /// <returns></returns>
        public SocialSecurityQueryRes SocialSecurityQuery(SocialSecurityReq socialReq)
        {
            var res = new SocialSecurityQueryRes
            {
                SocialSecurityCity = _socialCity,
                StatusCode = ServiceConsts.StatusCode_success,
            };
            try
            {
                // 获取cookie缓存
                if (socialReq != null && socialReq.Token != null && SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    _cookies = (CookieCollection)SpiderCacheHelper.GetCache(socialReq.Token);
                    SpiderCacheHelper.RemoveCache(socialReq.Token);
                }

                // 1. 校验参数
                this.VerifyStep(res, socialReq);

                // 2. 登录
                this.LoginStep(res, socialReq);

                // 3. 查询账号基础信息
                this.QueryUserInfoStep(res);

                // 4. 缴费详细数据获取
                this.QueryDetailStep(res);

                if (res.StatusCode == ServiceConsts.StatusCode_success)
                    res.StatusDescription = _socialCity + ServiceConsts.SocialSecurity_QuerySuccess;
            }
            catch (Exception e)
            {
                res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(_socialCity + ServiceConsts.SocialSecurity_QueryError, e);
            }
            return res;
        }

        #region 4. 缴费详细数据获取
        /// <summary>
        /// 4. 缴费详细数据获取
        /// </summary>
        /// <param name="res"></param>
        private void QueryDetailStep(SocialSecurityQueryRes res)
        {
            if (res.StatusCode != ServiceConsts.StatusCode_success)
                return;

            // 1. 机关养老保险
            this.GetPensions(res);

            // 2. 医疗保险
            this.GetHealthInsurance(res);

            // 3. 重新排序详细数据
            res.Details = res.Details.OrderByDescending(x => x.SocialInsuranceTime).ToList();
        }

        /// <summary>
        /// 医疗保险
        /// </summary>
        /// <param name="res"></param>
        private void GetHealthInsurance(SocialSecurityQueryRes res)
        {
            const string queryUrl = _baseUrl + "/ylsyPayInfo";
            // pageSize=200&pageIndex=1&PAGINATION_QUERY_nyEnd=201602&PAGINATION_QUERY_nyBegin=201501
            // 网站只提供从去年一月到当前的信息、pagesize = 200 可以满足当前查询需求
            var postdata = string.Format("pageSize=200&pageIndex=1&PAGINATION_QUERY_nyEnd={0}&PAGINATION_QUERY_nyBegin={1}", DateTime.Now.ToString("yyyyMM"), DateTime.Now.AddYears(-1).ToString("yyyy01"));
            _httpItem = new HttpItem()
            {
                URL = queryUrl,
                Method = "Post",
                Postdata = postdata,
                CookieCollection = _cookies,
                ResultCookieType = ResultCookieType.CookieCollection,
            };
            _httpItem.Header.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3");

            _httpResult = _httpHelper.GetHtml(_httpItem);
            var tableTr = _htmlParser.GetResultFromParser(_httpResult.Html, "//div[@class='mima03']/table/tr");
            if (tableTr == null || !tableTr.Any()) return;

            var healthEntities = new List<HealthInsuranceEntity>();

            for (var i = 2; i < tableTr.Count; i++)
            {
                var tds = _htmlParser.GetResultFromParser(tableTr[i], "//td");
                if (tds.Count < 8) continue;
                if (tds.Any())
                {
                    healthEntities.Add(new HealthInsuranceEntity
                    {
                        Type = tds[0],
                        CalcDate = tds[1],
                        BelongDate = tds[2],
                        PayBase = tds[3],
                        Amount = tds[4].ToDecimal(0),
                        AmountName = tds[5],
                        PayType = tds[6],
                        PayFlag = tds[7],
                        PayDate = tds[8]
                    });
                }
            }

            // 处理数据获取详细信息
            this.AnalysisData(healthEntities, res);
        }
        /// <summary>
        /// 医疗保险解析后的数据处理
        /// </summary>
        /// <param name="healthEntities"></param>
        /// <param name="res"></param>
        private void AnalysisData(List<HealthInsuranceEntity> healthEntities, SocialSecurityQueryRes res)
        {
            // 数据所有年月
            var yearMonths = healthEntities.GroupBy(x => x.BelongDate).Select(x => x.Key).OrderByDescending(x => x);
            if (!yearMonths.Any()) return;

            foreach (var belongDate in yearMonths)
            {
                if (string.IsNullOrEmpty(belongDate))
                    continue;

                var detail = res.Details.FirstOrDefault(x => !string.IsNullOrEmpty(x.SocialInsuranceTime) && x.SocialInsuranceTime.Equals(belongDate)) ?? new SocialSecurityDetailQueryRes
                {
                    IdentityCard = res.IdentityCard,
                    Name = res.Name,
                };

                var date = belongDate;
                var data = healthEntities.Where(x => x.BelongDate == date).AsQueryable();

                // 社保公司应缴
                var companyPaydata =
                    data.FirstOrDefault(x => x.Type == PayType.基本医疗保险.ToString() && x.AmountName == PayName.基本医疗统筹.ToString());
                if (companyPaydata != null)
                {
                    detail.PayTime = companyPaydata.PayDate;
                    detail.SocialInsuranceTime = companyPaydata.BelongDate;
                    detail.CompanyMedicalAmount = companyPaydata.Amount;

                    if (companyPaydata.PayFlag.Contains("已缴"))
                    {
                        detail.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    }
                    else
                    {
                        detail.PaymentType = companyPaydata.PayType;
                        detail.PaymentFlag = companyPaydata.PayFlag;
                    }

                    detail.SocialInsuranceBase = companyPaydata.PayBase.ToDecimal(0);
                }

                // 公务员补助
                var civilServantdata = data.Where(x => x.Type == PayType.公务员补助.ToString()).ToList();
                if (civilServantdata.Any())
                    detail.CivilServantMedicalAmount = civilServantdata.Sum(x => x.Amount);

                // 个人缴费
                var persondata = data.FirstOrDefault(
                        x => x.Type == PayType.基本医疗保险.ToString() && x.AmountName == PayName.基本医疗个人应缴.ToString());
                if (persondata != null)
                    detail.MedicalAmount = persondata.Amount;

                // 生育保险
                var maternitydata = data.FirstOrDefault(x => x.Type.Equals(PayType.生育保险.ToString()));
                if (maternitydata != null)
                    detail.MaternityAmount = maternitydata.Amount;

                // 大病互助 
                var illnessdata = data.FirstOrDefault(x => x.Type == PayType.大病互助.ToString());
                if (illnessdata != null)
                    detail.IllnessMedicalAmount = illnessdata.Amount;

                if (!res.Details.Contains(detail))
                    res.Details.Add(detail);
            }
        }

        /// <summary>
        /// 机关养老保险
        /// </summary>
        /// <param name="res"></param>
        private void GetPensions(SocialSecurityQueryRes res)
        {
            // 网站调整无法查询到养老保险数据
            res.SpecialPaymentType = "按湖南省社保局数据向省级集中要求，我市企业养老保险数据已移交省社保局集中管理，请登录省人社厅网站查询个人养老保险信息";
            return;
        }
        #endregion

        #region 3. 查询账号基础信息
        /// <summary>
        /// 3. 查询账号基础信息
        /// </summary>
        /// <param name="res"></param>
        private void QueryUserInfoStep(SocialSecurityQueryRes res)
        {
            if (res.StatusCode != ServiceConsts.StatusCode_success)
                return;

            #region 基础信息1
            // 醫療生育保險個人基本信息
            var infoUrl = _baseUrl + "/ylsyBasicInfo";
            _httpItem = new HttpItem()
            {
                URL = infoUrl,
                Method = "get",
                CookieCollection = _cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            _httpResult = _httpHelper.GetHtml(_httpItem);
            var dataTds = _htmlParser.GetResultFromParser(_httpResult.Html, @"//div[@class='mima03']/table[1]/tr/td", "");
            if (dataTds != null && dataTds.Any() && dataTds.Count >= 24)
            {
                res.Name = dataTds[1];
                res.IdentityCard = dataTds[3];
                res.Sex = dataTds[5];
                res.BirthDate = dataTds[7];
                res.EmployeeStatus = dataTds[9];

                res.CompanyNo = dataTds[19];
                res.CompanyName = dataTds[21];

                res.SocialSecurityCity = _socialCity;
                res.Loginname = res.IdentityCard;
            }
            #endregion

            #region 基础信息2
            // 基础信息2 失業保險基本信息

            infoUrl = _baseUrl + "/sybxBasicInfo";
            _httpItem = new HttpItem()
            {
                URL = infoUrl,
                Method = "get",
                CookieCollection = _cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            _httpResult = _httpHelper.GetHtml(_httpItem);
            dataTds = _htmlParser.GetResultFromParser(_httpResult.Html, @"//div[@class='mima03']/table/tr/td", "");
            if (dataTds == null || !dataTds.Any() || dataTds.Count < 40) return;
            if (string.IsNullOrEmpty(res.Name)) res.Name = dataTds[5];
            if (string.IsNullOrEmpty(res.IdentityCard)) res.IdentityCard = dataTds[7];
            if (string.IsNullOrEmpty(res.Sex)) res.Sex = dataTds[9];
            if (string.IsNullOrEmpty(res.BirthDate)) res.BirthDate = dataTds[13];
            if (string.IsNullOrEmpty(res.EmployeeStatus)) res.EmployeeStatus = dataTds[27];

            if (string.IsNullOrEmpty(res.CompanyNo)) res.CompanyNo = dataTds[1];
            if (string.IsNullOrEmpty(res.CompanyName)) res.CompanyName = dataTds[3];

            if (string.IsNullOrEmpty(res.SocialSecurityCity)) res.SocialSecurityCity = _socialCity;
            if (string.IsNullOrEmpty(res.Loginname)) res.Loginname = res.IdentityCard;

            res.Race = dataTds[11];
            res.WorkDate = dataTds[15];
            #endregion
        }
        #endregion

        #region 2. 登陆
        /// <summary>
        /// 2. 登陆
        /// </summary>
        /// <param name="res"></param>
        /// <param name="socialReq"></param>
        private void LoginStep(SocialSecurityQueryRes res, SocialSecurityReq socialReq)
        {
            if (res.StatusCode != ServiceConsts.StatusCode_success)
                return;

            const string loginUrl = _baseUrl + "/personLogin";
            var postData = string.Format("pcode={0}&password={1}&verifyCode={2}", socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
            _httpItem = new HttpItem()
            {
                URL = loginUrl,
                Method = "POST",
                Postdata = postData,
                CookieCollection = _cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            _httpResult = _httpHelper.GetHtml(_httpItem);

            // 1. 登陆失败返回结果中只包含 script 标签内容
            var errorMsgs = _htmlParser.GetResultFromParser(_httpResult.Html, @"//div[@class='wrong']", "innertext");

            // 登录失败
            if (_httpResult.StatusCode != HttpStatusCode.OK || errorMsgs.Count > 0)
            {
                res.StatusDescription = "登录失败，" + string.Join("，", errorMsgs);
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
        /// <param name="socialReq"></param>
        private void VerifyStep(SocialSecurityQueryRes res, SocialSecurityReq socialReq)
        {
            if (res.StatusCode != ServiceConsts.StatusCode_success)
                return;

            if (!socialReq.Identitycard.IsEmpty() && !socialReq.Password.IsEmpty() && !socialReq.Vercode.IsEmpty())
                return;

            res.StatusDescription = "身份证号码、密码、验证码不能为空";
            res.StatusCode = ServiceConsts.StatusCode_fail;
        }
        #endregion

        #endregion
    }

    #region 辅助类
    /// <summary>
    /// 险种
    /// </summary>
    internal enum PayType
    {
        公务员补助,
        基本医疗保险,
        生育保险,
        大病互助
    }

    /// <summary>
    /// 款项名称
    /// </summary>
    internal enum PayName
    {
        基本医疗个人应缴,
        基本医疗统筹,
    }

    #region 医保缴费实体
    /// <summary>
    /// 医保缴费实体
    /// </summary>
    internal class HealthInsuranceEntity
    {
        /// <summary>
        /// 险种
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 计算年月
        /// </summary>
        public string CalcDate { get; set; }

        /// <summary>
        /// 所属期间
        /// </summary>
        public string BelongDate { get; set; }

        /// <summary>
        /// 缴费基数
        /// </summary>
        public string PayBase { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 款项名称
        /// </summary>
        public string AmountName { get; set; }

        /// <summary>
        /// 款项类别
        /// </summary>
        public string PayType { get; set; }

        /// <summary>
        /// 缴费标志
        /// </summary>
        public string PayFlag { get; set; }

        /// <summary>
        /// 缴费时间
        /// </summary>
        public string PayDate { get; set; }
    }
    #endregion

    #endregion
}
