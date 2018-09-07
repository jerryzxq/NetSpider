using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using System.Text.RegularExpressions;
using System.Xml;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.SC
{
    /// <summary>
    /// 四川 内江数据
    /// </summary>
    public class neijiang : ISocialSecurityCrawler
    {
        #region property
        private readonly IPluginHtmlParser _htmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        private const string _baseUrl = "http://www.scnj.hrss.gov.cn/";
        private const string _socialCity = "sc_neijiang";

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
                var getCodeUrl = _baseUrl + "Include/code.asp?time=" + DateTime.Now.ToString("yyyyMMddHHmmss");
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
        /// 登录查询等业务操作
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
                var loginSuccessPageInfo = string.Empty;
                this.LoginStep(res, socialReq, ref loginSuccessPageInfo);

                // 3. 查询账号基础信息
                this.QueryUserInfoStep(res, socialReq, loginSuccessPageInfo);

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

            res.Details = new List<SocialSecurityDetailQueryRes>();

            // 查询年度，这里查询最近5个年度的缴费记录
            var years = new List<int>();
            for (var i = 0; i < 5; i++)
                years.Add(DateTime.Now.Year - i);
            //years.AddRange(new[] { DateTime.Now.Year, DateTime.Now.AddYears(-1).Year, DateTime.Now.AddYears(-2).Year });

            // 1. 养老缴费
            this.GetPensions(res, years);

            // 2. 医疗保险信息
            this.GetHealthInsurance(res);

            // 3. 失业缴费信息
            this.GetUnemployment(res, years);

            // 4. 工伤缴费数据
            this.GetEmploymentInjury(res, years);

            // 5. 生育缴费信息
            this.GetMaternity(res, years);

            // 数据重新排序
            res.Details = res.Details.OrderByDescending(x => x.SocialInsuranceTime).ToList();
        }

        /// <summary>
        /// 5. 生育缴费信息
        /// </summary>
        /// <param name="res"></param>
        /// <param name="years"></param>
        private void GetMaternity(SocialSecurityQueryRes res, List<int> years)
        {
            const string queryUrl = _baseUrl + "cxx_list.asp?id=8";
            var entities = new List<PaymentEntity>();
            foreach (var year in years)
            {
                var postdata = "bt=1&aae002=" + year;
                _httpItem = new HttpItem()
                {
                    URL = queryUrl,
                    Method = "POST",
                    CookieCollection = _cookies,
                    Postdata = postdata,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
                var root = this.AnalysisPageInfo(_httpResult.Html, 2);
                if (root == null) continue;

                var recordcount = root.SelectSingleNode("/result/output/recordcount").InnerText.ToInt(0);
                if (recordcount <= 0) continue;

                var dataNodes = root.SelectNodes("/result/output/sqldata/row");
                if (dataNodes == null || dataNodes.Count <= 0) continue;

                for (var i = 1; i <= dataNodes.Count; i++)
                {
                    entities.Add(new PaymentEntity
                    {
                        BelongDate = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/aae002").InnerText,
                        Type = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/aae143").InnerText,
                        Status = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/aae115").InnerText,
                        BaseAmount = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/amc020").InnerText.ToDecimal(0),
                        PayAmount = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/amc027").InnerText.ToDecimal(0),
                    });
                }
            }

            // 数据处理
            var yearMonths = entities.GroupBy(x => x.BelongDate).Select(x => x.Key);
            foreach (var yearMonth in yearMonths)
            {
                var detail = res.Details.FirstOrDefault(x => !string.IsNullOrEmpty(x.SocialInsuranceTime) && x.SocialInsuranceTime.Equals(yearMonth)) ?? new SocialSecurityDetailQueryRes
                {
                    Name = res.Name,
                    IdentityCard = res.IdentityCard,
                    SocialInsuranceTime = yearMonth,
                };
                var datas = entities.Where(x => x.BelongDate.Equals(yearMonth)).AsQueryable();
                if (datas.Any())
                {
                    detail.SocialInsuranceBase = detail.SocialInsuranceBase == 0 ? datas.Sum(x => x.BaseAmount) : detail.SocialInsuranceBase;
                    detail.MaternityAmount = datas.Sum(x => x.PayAmount);
                }

                if (res.Details.Contains(detail)) continue;
                // 新增
                var onedata = datas.FirstOrDefault();
                if (onedata != null)
                {
                    if (onedata.Status.Contains("已实缴"))
                    {
                        detail.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    }
                    else
                    {
                        detail.PaymentType = onedata.Type;
                        detail.PaymentFlag = onedata.Status;
                    }
                }
                res.Details.Add(detail);
            }
        }

        /// <summary>
        /// 4. 工伤缴费数据
        /// </summary>
        /// <param name="res"></param>
        /// <param name="years"></param>
        private void GetEmploymentInjury(SocialSecurityQueryRes res, List<int> years)
        {
            const string queryUrl = _baseUrl + "cxx_list.asp?id=6";
            var entities = new List<PaymentEntity>();
            foreach (var year in years)
            {
                var postdata = "bt=1&aae002=" + year;
                _httpItem = new HttpItem()
                {
                    URL = queryUrl,
                    Method = "POST",
                    CookieCollection = _cookies,
                    Postdata = postdata,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
                var root = this.AnalysisPageInfo(_httpResult.Html, 2);
                if (root == null) continue;

                var recordcount = root.SelectSingleNode("/result/output/recordcount").InnerText.ToInt(0);
                if (recordcount <= 0) continue;

                var dataNodes = root.SelectNodes("/result/output/sqldata/row");
                if (dataNodes == null || dataNodes.Count <= 0) continue;

                for (var i = 1; i <= dataNodes.Count; i++)
                {
                    entities.Add(new PaymentEntity
                    {
                        BelongDate = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/aae002").InnerText,
                        Type = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/aae143").InnerText,
                        Status = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/aae115").InnerText,
                        BaseAmount = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/alc020").InnerText.ToDecimal(0),
                        PayAmount = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/alc027").InnerText.ToDecimal(0),
                    });
                }
            }

            // 数据处理
            var yearMonths = entities.GroupBy(x => x.BelongDate).Select(x => x.Key);
            foreach (var yearMonth in yearMonths)
            {
                var detail = res.Details.FirstOrDefault(x => !string.IsNullOrEmpty(x.SocialInsuranceTime) && x.SocialInsuranceTime.Equals(yearMonth)) ?? new SocialSecurityDetailQueryRes
                {
                    Name = res.Name,
                    IdentityCard = res.IdentityCard,
                    SocialInsuranceTime = yearMonth,
                };

                var datas = entities.Where(x => x.BelongDate.Equals(yearMonth)).AsQueryable();
                if (datas.Any())
                {
                    detail.SocialInsuranceBase = detail.SocialInsuranceBase == 0 ? datas.Sum(x => x.BaseAmount) : detail.SocialInsuranceBase;
                    detail.EmploymentInjuryAmount = datas.Sum(x => x.PayAmount);
                }

                if (res.Details.Contains(detail)) continue;
                // 新增
                var onedata = datas.FirstOrDefault();
                if (onedata != null)
                {
                    if (onedata.Status.Contains("已实缴"))
                    {
                        detail.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    }
                    else
                    {
                        detail.PaymentType = onedata.Type;
                        detail.PaymentFlag = onedata.Status;
                    }
                }
                res.Details.Add(detail);
            }
        }

        /// <summary>
        /// 失业缴费信息
        /// </summary>
        /// <param name="res"></param>
        /// <param name="years"></param>
        private void GetUnemployment(SocialSecurityQueryRes res, List<int> years)
        {
            const string queryUrl = _baseUrl + "cxx_list.asp?id=7";
            foreach (var year in years)
            {
                var postdata = "bt=1&aae002=" + year;
                _httpItem = new HttpItem()
                {
                    URL = queryUrl,
                    Method = "POST",
                    CookieCollection = _cookies,
                    Postdata = postdata,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
                var root = this.AnalysisPageInfo(_httpResult.Html, 2);
                if (root == null) continue;

                var recordcount = root.SelectSingleNode("/result/output/recordcount").InnerText.ToInt(0);
                if (recordcount <= 0) continue;

                var dataNodes = root.SelectNodes("/result/output/sqldata/row");
                if (dataNodes == null || dataNodes.Count <= 0) continue;

                for (var i = 1; i <= dataNodes.Count; i++)
                {
                    var type = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/aae143").InnerText;
                    if (!type.Contains("正常应缴"))
                        continue;

                    var belongDate = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/aae002").InnerText;
                    var detail = res.Details.FirstOrDefault(x => !string.IsNullOrEmpty(x.SocialInsuranceTime) && x.SocialInsuranceTime.Equals(belongDate)) ?? new SocialSecurityDetailQueryRes
                    {
                        Name = res.Name,
                        IdentityCard = res.IdentityCard,
                        SocialInsuranceTime = belongDate,
                    };

                    var socialInsuranceBase =
                        root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/ajc020").InnerText.ToDecimal(0);
                    detail.SocialInsuranceBase = detail.SocialInsuranceBase == 0 ? socialInsuranceBase : detail.SocialInsuranceBase;

                    var personPay = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/grjf").InnerText.ToDecimal(0); ;
                    var companyPay = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/dwjf").InnerText.ToDecimal(0); ;
                    detail.UnemployAmount = personPay + companyPay;

                    if (res.Details.Contains(detail)) continue;
                    // 新增
                    var status = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/aae115").InnerText;
                    if (status.Contains("已实缴"))
                    {
                        detail.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    }
                    else
                    {
                        detail.PaymentFlag = status;
                        detail.PaymentType = type;
                    }
                    res.Details.Add(detail);
                }
            }
        }

        /// <summary>
        /// 医疗保险信息
        /// </summary>
        /// <param name="res"></param>
        private void GetHealthInsurance(SocialSecurityQueryRes res)
        {
            const string queryUrl = _baseUrl + "cx_list.asp?id=10";
            _httpItem = new HttpItem()
            {
                URL = queryUrl,
                Method = "GET",
                CookieCollection = _cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            _httpResult = _httpHelper.GetHtml(_httpItem);
            var root = this.AnalysisPageInfo(_httpResult.Html, 3);
            if (root == null) return;

            var recordcount = root.SelectSingleNode("/result/output/recordcount").InnerText.ToInt(0);
            if (recordcount <= 0) return;

            var dataNodes = root.SelectNodes("/result/output/sqldata/row");
            if (dataNodes == null || dataNodes.Count <= 0) return;

            for (var i = 1; i <= dataNodes.Count; i++)
            {
                // 这里获取的是年度数据
                var yearPaymentMonths =
                    root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/zhys").InnerText.ToInt(0);

                // 数据平均到每个月
                for (var month = 1; month <= yearPaymentMonths; month++)
                {
                    var belongDate = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/aae002").InnerText
                        + (month < 10 ? "0" + month : month.ToString());

                    var detail = res.Details.FirstOrDefault(x => !string.IsNullOrEmpty(x.SocialInsuranceTime) && x.SocialInsuranceTime.Equals(belongDate)) ?? new SocialSecurityDetailQueryRes
                    {
                        Name = res.Name,
                        IdentityCard = res.IdentityCard,
                        PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal,
                        PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal,
                        SocialInsuranceTime = belongDate,
                    };

                    // 数据平均到每个月
                    var socialInsuranceBase =
                        Math.Round(root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/jfjs").InnerText.ToDecimal(0) /
                        yearPaymentMonths, 2);
                    detail.SocialInsuranceBase = detail.SocialInsuranceBase == 0 ? socialInsuranceBase : detail.SocialInsuranceBase;

                    //detail.YearPaymentMonths = yearPaymentMonths;
                    detail.MedicalAmount = Math.Round(root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/grjn").InnerText.ToDecimal(0) / yearPaymentMonths, 2);
                    detail.CompanyMedicalAmount = Math.Round(root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/dwjn_jb").InnerText.ToDecimal(0) / yearPaymentMonths, 2);
                    detail.CivilServantMedicalAmount = Math.Round(root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/dwjn_gwy").InnerText.ToDecimal(0) / yearPaymentMonths, 2);

                    if (!res.Details.Contains(detail))
                        res.Details.Add(detail);
                }
            }
        }

        /// <summary>
        /// 养老缴费信息
        /// </summary>
        /// <param name="res"></param>
        /// <param name="years"></param>
        private void GetPensions(SocialSecurityQueryRes res, IEnumerable<int> years)
        {
            const string queryUrl = _baseUrl + "cxx_list.asp?id=5";
            // years = new List<int>(new[] { 2013 });
            foreach (var year in years)
            {
                var postdata = "bt=1&aae002=" + year;
                _httpItem = new HttpItem()
                {
                    URL = queryUrl,
                    Method = "POST",
                    CookieCollection = _cookies,
                    Postdata = postdata,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
                var root = this.AnalysisPageInfo(_httpResult.Html, 2);
                if (root == null) continue;

                var recordcount = root.SelectSingleNode("/result/output/recordcount").InnerText.ToInt(0);
                if (recordcount <= 0) continue;

                var dataNodes = root.SelectNodes("/result/output/sqldata/row");
                if (dataNodes == null || dataNodes.Count <= 0) continue;

                for (var i = 1; i <= dataNodes.Count; i++)
                {
                    var detail = new SocialSecurityDetailQueryRes();
                    detail.Name = res.Name;
                    detail.IdentityCard = res.IdentityCard;
                    detail.SocialInsuranceTime = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/aae002").InnerText;

                    var flag = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/aae114").InnerText;
                    if (flag.Contains("已实缴"))
                    {
                        detail.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    }
                    else
                    {
                        detail.PaymentFlag = flag;
                        detail.PaymentType = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/aae143").InnerText;
                    }

                    detail.PensionAmount = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/hzf").InnerText.ToDecimal(0);
                    detail.CompanyPensionAmount = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/htc").InnerText.ToDecimal(0);

                    detail.SocialInsuranceBase = root.SelectSingleNode("/result/output/sqldata/row[" + i + "]/aic020").InnerText.ToDecimal(0);
                    //detail.EnterAccountMedicalAmount = detail.PensionAmount + detail.CompanyPensionAmount;

                    res.Details.Add(detail);
                }
            }
        }

        #endregion

        #region 3. 人员基础信息获取
        /// <summary>
        /// 3. 人员基础信息获取
        /// </summary>
        /// <param name="res"></param>
        /// <param name="socialReq"></param>
        /// <param name="loginSuccessPageInfo"></param>
        private void QueryUserInfoStep(SocialSecurityQueryRes res, SocialSecurityReq socialReq, string loginSuccessPageInfo)
        {
            if (res.StatusCode != ServiceConsts.StatusCode_success)
                return;

            // 1. 登陆成功页面 基本信息解析
            var root = this.AnalysisPageInfo(loginSuccessPageInfo, 3);
            if (root != null)
            {
                res.EmployeeNo = root.SelectSingleNode("/result/output/sqldata/row/aac001").InnerText;
                res.Name = root.SelectSingleNode("/result/output/sqldata/row/aac003").InnerText;
                res.Sex = root.SelectSingleNode("/result/output/sqldata/row/aac004").InnerText;
                res.BirthDate = root.SelectSingleNode("/result/output/sqldata/row/aac006").InnerText;

                res.SocialSecurityCity = _socialCity;
                res.Loginname = socialReq.Identitycard;
                res.IdentityCard = socialReq.Identitycard;
            }

            // 2. 个人参保状态页面 数据获取
            var infoUrl = _baseUrl + "cx_list.asp?id=2";
            _httpItem = new HttpItem()
            {
                URL = infoUrl,
                Method = "get",
                CookieCollection = _cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            _httpResult = _httpHelper.GetHtml(_httpItem);
            root = this.AnalysisPageInfo(_httpResult.Html, 3);
            if (root != null)
            {
                res.EmployeeStatus = root.SelectSingleNode("/result/output/sqldata/row[1]/aac008").InnerText;
                res.WorkDate = root.SelectSingleNode("/result/output/sqldata/row[1]/aac007").InnerText;

                // Payment_State 通过计算获得
                //res.Payment_State = root.SelectSingleNode("/result/output/sqldata/row[1]/aac031").InnerText; 

                res.CompanyName = root.SelectSingleNode("/result/output/sqldata/row[1]/aab004").InnerText;
            }

            // 3. 社保卡信息获取
            infoUrl = _baseUrl + "cxx_list.asp?id=80";
            _httpItem = new HttpItem()
            {
                URL = infoUrl,
                Method = "get",
                CookieCollection = _cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            _httpResult = _httpHelper.GetHtml(_httpItem);
            root = this.AnalysisPageInfo(_httpResult.Html, 2);
            if (root != null)
            {
                res.Bank = root.SelectSingleNode("/result/output/sqldata/row/aae008").InnerText;
            }
        }

        /// <summary>
        /// 获取页面数据xml
        /// </summary>
        /// <param name="htmlInfo"></param>
        /// <param name="scriptIndex"></param>
        /// <returns></returns>
        private XmlElement AnalysisPageInfo(string htmlInfo, int scriptIndex)
        {
            XmlElement root = null;
            var scriptdata = _htmlParser.GetResultFromParser(htmlInfo, "//script[" + scriptIndex + "]", "");
            if (scriptdata == null || !scriptdata.Any()) return null;

            var xmlstr = Regex.Match(scriptdata[0], @"var xmlstr = '[\s\S]*';").Value.Replace("var xmlstr = '", "").Replace("';", "");
            var doc = new XmlDocument();
            doc.LoadXml(xmlstr);
            root = doc.DocumentElement;
            return root;
        }
        #endregion

        #region 2. 登陆
        /// <summary>
        /// 2. 登陆
        /// </summary>
        /// <param name="res"></param>
        /// <param name="socialReq"></param>
        /// <param name="loginSuccessPageInfo"></param>
        private void LoginStep(SocialSecurityQueryRes res, SocialSecurityReq socialReq, ref string loginSuccessPageInfo)
        {
            if (res.StatusCode != ServiceConsts.StatusCode_success)
                return;

            const string loginUrl = _baseUrl + "/cx_Ckin.asp";
            var postData = string.Format("sfz={0}&passw={1}&code={2}", socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
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
            var errorResult = new List<string>();
            var math = Regex.Match(_httpResult.Html, @"^<script [\s\S]*</script>$");
            if (!math.Value.IsEmpty())
            {
                errorResult.Add(Regex.Match(_httpResult.Html, @"alert\([\s\S]*!'\)").Value.Replace("'", "").Replace("alert(", "").Replace(")", ""));
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

                // 存储登陆成功后页面数据
                loginSuccessPageInfo = _httpResult.Html;
            }
        }
        #endregion

        #region 1. 校验参数
        /// <summary>
        /// 校验参数
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
    /// 缴费实体
    /// </summary>
    internal class PaymentEntity
    {
        /// <summary>
        /// 日期
        /// </summary>
        public string BelongDate { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 缴费基数
        /// </summary>
        public decimal BaseAmount { get; set; }

        /// <summary>
        /// 缴费金额
        /// </summary>
        public decimal PayAmount { get; set; }

    }
    #endregion
}
