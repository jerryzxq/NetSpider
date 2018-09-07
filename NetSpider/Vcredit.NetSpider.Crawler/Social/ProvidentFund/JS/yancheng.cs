using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
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
    public class yancheng : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.ycgjj.com:18081/";
        string fundCity = "js_yancheng";
        List<string> _results = new List<string>();
        #endregion
        #region 私有变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        string _url = string.Empty;
        string _postData = string.Empty;
        Random rdm = new Random();
        int PaymentMonths = 0;
        private decimal payRate = (decimal)0.08;
        #endregion
        /// <summary>
        /// 取验证码cookie即可,输入校验可以省略 2016年6月15日
        /// </summary>
        /// <param name="fundReq"></param>
        /// <returns></returns>
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                _url = "http://www.ycgjj.com/";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "GET",
                    Host = "www.ycgjj.com",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    vcRes.StatusDescription = ServiceConsts.ProvidentFund_InitFail;
                    vcRes.StatusCode = ServiceConsts.StatusCode_fail;
                    return vcRes;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //取验证码cookie即可,输入校验可以省略 2016年6月15日
                _url = baseUrl + "CaptchaImg";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Host = "www.ycgjj.com",
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
                //合并cookies
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //vcRes.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                ////保存验证码图片在本地
                //FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
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
            Regex reg = new Regex(@"[^\u4e00-\u9fa5]");//非汉字
            ProvidentFundDetail detail = new ProvidentFundDetail();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (string.IsNullOrEmpty(fundReq.Password) || string.IsNullOrEmpty(fundReq.Identitycard))//|| string.IsNullOrEmpty(fundReq.Vercode)
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆

                _url = baseUrl + "login.do?r=" + string.Format("{0:F16}", rdm.NextDouble());
                _postData = string.Format("username={0}&password={1}&loginType=4&vertcode={2}&bsr=firefox%2F47.0&vertype=1", fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0",
                    URL = _url,
                    Host = "www.ycgjj.com:18081",
                    Method = "Post",
                    Postdata = _postData,
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header["X-Requested-With"] = "XMLHttpRequest";
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (jsonParser.GetResultFromParser(httpResult.Html, "success").ToLower() != "true")
                {
                    Res.StatusDescription = jsonParser.GetResultFromParser(httpResult.Html, "msg");
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                _url = jsonParser.GetResultFromParser(httpResult.Html, "url");
                if (string.IsNullOrEmpty(_url))
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region 第二步,获取基本信息

                _url = baseUrl + _url;
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='pername']", "value");
                if (_results.Count == 0)
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.Name = _results[0];//姓名
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='percode']", "value");
                if (_results.Count > 0)
                {
                    Res.ProvidentFundNo = _results[0];//公积金账号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='codeno']", "value");
                if (_results.Count > 0)
                {
                    Res.IdentityCard = _results[0];//证件号码
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='phone']", "value");
                if (_results.Count > 0)
                {
                    Res.Phone = _results[0];//手机
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='bkcardname']", "value");
                if (_results.Count > 0)
                {
                    Res.Bank = _results[0];//开户银行
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='bkcard']", "value");
                if (_results.Count > 0)
                {
                    Res.BankCardNo = _results[0];//联名卡号
                }
                string jsonstr = "{" + CommonFun.GetMidStr(httpResult.Html, "var data = {", ";");
                List<YanChengBaseInfo> baseInfoList = jsonParser.DeserializeObject<List<YanChengBaseInfo>>(jsonParser.GetResultFromParser(jsonstr, "list"));
                if (baseInfoList.Count < 1)
                {
                    Res.StatusDescription = "jsonstr获取失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                YanChengBaseInfo yanChengBaseInfo = baseInfoList[0];
                Res.LastProvidentFundTime = yanChengBaseInfo.payendmnh;//缴至年月
                Res.SalaryBase = yanChengBaseInfo.bmny;//缴存基数
                Res.CompanyNo = yanChengBaseInfo.corpcode;//单位编号
                Res.CompanyName = yanChengBaseInfo.corpname;//单位名称
                Res.TotalAmount = yanChengBaseInfo.accbal;//账户余额
                Res.CompanyMonthPayAmount = yanChengBaseInfo.corpdepmny;//公司月缴额
                Res.PersonalMonthPayAmount = yanChengBaseInfo.perdepmny;//个人月缴额
                Res.CompanyMonthPayRate = yanChengBaseInfo.percorpscale.ToDecimal(0) * 0.01M;//公司月缴比率
                Res.PersonalMonthPayRate = yanChengBaseInfo.perperscale.ToDecimal(0) * 0.01M;//个人月缴比率
                Res.OpenTime = yanChengBaseInfo.regtime.Replace(".0", "");//开户时间
                Res.Status = yanChengBaseInfo.accstate;//状态
                #endregion
                #region 第三步,获取缴费明细

                List<YanChengDetails> details = new List<YanChengDetails>();
                int nowYear = DateTime.Now.Year;
                _url = baseUrl + "per/queryPerAccDetailsAction!getPerAccDetails.do";
                while (true)
                {
                    _postData = string.Format("dto%5B'corpcode'%5D={0}&dto%5B'corpcode_md5list'%5D=&dto%5B'corpcode_desc'%5D={1}&dto%5B'year'%5D={3}&gridInfo%5B'dataList_limit'%5D={2}&gridInfo%5B'dataList_start'%5D=0&", Res.CompanyNo, "", "1000", nowYear);// Res.CompanyName.ToUrlEncode()
                    httpItem = new HttpItem()
                    {
                        Accept = "application/json, text/javascript, */*; q=0.01",
                        URL = _url,
                        Method = "Post",
                        Postdata = _postData,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpItem.Header["X-Requested-With"] = "XMLHttpRequest";
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (jsonParser.GetResultFromParser(httpResult.Html, "success").ToLower() != "true")
                    {
                        break;
                    }
                    string list = jsonParser.GetResultFromMultiNode(httpResult.Html, "lists:dataList:list");
                    if (list == "[]" && nowYear != DateTime.Now.Year)
                    {
                        break;
                    }
                    details.AddRange(jsonParser.DeserializeObject<List<YanChengDetails>>(list));
                    nowYear--;
                }
                decimal perRate = decimal.Zero;
                decimal comRate = decimal.Zero;
                if (Res.PersonalMonthPayRate > 0 && Res.CompanyMonthPayRate > 0)
                {
                    perRate = Res.PersonalMonthPayRate / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate);
                    comRate = 1 - perRate;
                }
                else
                {
                    perRate = comRate = 0.5M;
                }
                foreach (YanChengDetails item in details)
                {
                    detail = new ProvidentFundDetail();
                    detail.CompanyName = item.corpname;
                    detail.Description = item.remark;
                    detail.PayTime = item.acctime.ToDateTime();
                    if (item.remark.Contains("汇缴"))
                    {
                        if (detail.PayTime != null)
                            detail.ProvidentFundTime = ((DateTime)(detail.PayTime)).ToString(Consts.DateFormatString7);
                        detail.PersonalPayAmount = item.occmny.ToDecimal(0) * perRate; //个人月缴额
                        detail.CompanyPayAmount = item.occmny.ToDecimal(0) * comRate; //公司月缴额
                        detail.ProvidentFundBase = item.occmny.ToDecimal(0) / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate);//缴费基数
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                    }
                    else if (item.remark.Contains("单位补缴"))
                    {
                        detail.CompanyPayAmount = item.occmny.ToDecimal(0); //公司月缴额
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Back;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Back;
                    }
                    else if (item.remark.Contains("补"))
                    {
                        detail.PersonalPayAmount = item.occmny.ToDecimal(0); //个人月缴额
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Back;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Back;
                    }
                    else if (item.remark.Contains("支") || item.remark.Contains("取"))
                    {
                        detail.PersonalPayAmount = item.occmny.ToDecimal(0); //个人月缴额
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                    }
                    else
                    {
                        detail.PersonalPayAmount = item.occmny.ToDecimal(0); //个人月缴额
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
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
        /// <summary>
        /// 个人基本信息
        /// </summary>
        internal class YanChengBaseInfo
        {
            /// <summary>
            /// 缴至年月
            /// </summary>
            public string payendmnh { get; set; }
            /// <summary>
            /// 缴存基数
            /// </summary>
            public decimal bmny { get; set; }
            /// <summary>
            /// 单位编号
            /// </summary>
            public string corpcode { get; set; }
            /// <summary>
            /// 单位名称
            /// </summary>
            public string corpname { get; set; }
            /// <summary>
            /// 账户余额
            /// </summary>
            public decimal accbal { get; set; }
            /// <summary>
            /// 公司月缴额
            /// </summary>
            public decimal corpdepmny { get; set; }
            /// <summary>
            /// 个人月缴额
            /// </summary>
            public decimal perdepmny { get; set; }
            /// <summary>
            /// 公司月缴比率
            /// </summary>
            public string percorpscale { get; set; }
            /// <summary>
            /// 个人月缴比率
            /// </summary>
            public string perperscale { get; set; }
            /// <summary>
            /// 开户时间
            /// </summary>
            public string regtime { get; set; }

            private string _accstate;
            /// <summary>
            /// 账户状态(01:正常,02:封存,03:托管,04:销户,09:待生效)
            /// </summary>
            public string accstate
            {
                get { return _accstate; }
                set
                {
                    switch (value)
                    {
                        case "01":
                            _accstate = "正常";
                            break;
                        case "02":
                            _accstate = "封存";
                            break;
                        case "03":
                            _accstate = "托管";
                            break;
                        case "04":
                            _accstate = "销户";
                            break;
                        case "09":
                            _accstate = "待生效";
                            break;
                        default:
                            _accstate = "错误开户销户";
                            break;
                    }
                }
            }

        }
        /// <summary>
        /// 公积金缴存明细
        /// </summary>
        internal class YanChengDetails
        {
            /// <summary>
            /// 入账时间 
            /// </summary>
            public string acctime { get; set; }
            /// <summary>
            /// 摘要
            /// </summary>
            public string remark { get; set; }
            /// <summary>
            /// 公积金账号
            /// </summary>
            public string percode { get; set; }
            /// <summary>
            ///当前余额
            /// </summary>
            public string accbal { get; set; }
            /// <summary>
            /// 公司名称
            /// </summary>
            public string corpname { get; set; }
            /// <summary>
            /// 公司编号
            /// </summary>
            public string corpcode { get; set; }
            /// <summary>
            /// 姓名
            /// </summary>
            public string pername { get; set; }
            /// <summary>
            /// 发生金额（公司+个人）
            /// </summary>
            public string occmny { get; set; }
        }
    }
}
