using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Crawler.Social.ProvidentFund.JS;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.NM
{
    public class huhehaote : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.hhhtgjj.com.cn/";
        string fundCity = "nm_huhehaote";
        List<string> _results = new List<string>();
        #endregion
        #region 私有变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        string _url = string.Empty;
        string _postData = string.Empty;
        int PaymentMonths = 0;
        private decimal payRate = (decimal)0.08;
        decimal perAccounting = 0;//个人占比
        decimal comAccounting = 0;//公司占比
        decimal totalRate = 0;//总缴费比率
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                _url = baseUrl + "website/trans/ValidateImg";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "get",
                    Referer = baseUrl + "login.jsp",
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
                vcRes.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                vcRes.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
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
            string loginType = string.Empty;//登录方式 0:公积金账号，1:身份证号
            string errorMsg = string.Empty;
            string userNum = string.Empty;//登陆账号
            string tranCode = string.Empty;//tranCode:'111124'(脚本自定义)
            string Flag = string.Empty;//Flag:'1'(脚本自定义)
            string task = string.Empty;//task:''(脚本自定义)
            Regex reg = new Regex(@"[\&nbsp;\s;\%;\,;\，;/\t;]*");
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
                    //15位或18位身份证验证
                    Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                    if (regex.IsMatch(fundReq.Identitycard) == false)
                    {
                        Res.StatusDescription = "请输入有效的15位或18位身份证号";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    loginType = "1";
                    userNum = fundReq.Identitycard;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(fundReq.Username))
                    {
                        Res.StatusDescription = "公积金账号不能为空";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    loginType = "0";
                    userNum = fundReq.Username;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Password))
                {
                    Res.StatusDescription = "请输入密码";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Vercode))
                {
                    Res.StatusDescription = "请输入验证码";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆

                _url = baseUrl + "grdl.htm";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                tranCode = CommonFun.GetMidStr(httpResult.Html, "tranCode:'", "'");
                if (string.IsNullOrEmpty(tranCode))
                {
                    Res.StatusDescription = "登陆页页面脚本已改变,请核对网站信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                _url = baseUrl + "PerSendServlet";
                _postData = string.Format("tranCode={0}&task=&CI_Act={1}&CI_UserNum={2}&CI_Password={3}&verify={4}", tranCode, loginType, userNum, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "Post",
                    Postdata = _postData,
                    Referer = baseUrl + "grdl.htm",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "{\"msg\":\"", "[]\",\"success\":\"0\"}");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = Regex.Unescape(errorMsg);
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "{\"msg\":\"", "\",\"success\":\"0\"}");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = Regex.Unescape(errorMsg);
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "{\"msg\":\"", "\"}");
                if (!string.IsNullOrEmpty(errorMsg) && !errorMsg.Contains("ss\":\"1"))
                {
                    Res.StatusDescription = Regex.Unescape(errorMsg);//ASCII码转汉字
                    // errorMsg = HttpUtility.UrlDecode("\u9a8c\u8bc1\u7801\u4e0d\u6b63\u786e");
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #endregion
                #region 第二步,获取基本信息
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string certinum;
                if (cookies["certinum"] != null)
                {
                    certinum = Res.IdentityCard = cookies["certinum"].Value; //身份证号
                }
                else
                {
                    Res.StatusDescription = "请检查浏览器是否禁用cookie";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                _url = baseUrl + "querymenulist.htm";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("gbk"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                tranCode = CommonFun.GetMidStr(httpResult.Html, "tranCode:'", "'");
                task = CommonFun.GetMidStr(httpResult.Html, "task   : \"", "\",");
                Flag = CommonFun.GetMidStr(httpResult.Html, "Flag   : \"", "\",");
                if (string.IsNullOrEmpty(tranCode) || string.IsNullOrEmpty(Flag))
                {
                    Res.StatusDescription = "基本信息页面来源页面脚本已改变,请核对网站信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                _url = baseUrl + "PerSendServlet";
                _postData = string.Format("tranCode={0}&task={1}&Flag={2}&UserNum={3}", tranCode, task, Flag, certinum);
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "Post",
                    Postdata = _postData,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (!httpResult.Html.StartsWith("{") && !httpResult.Html.EndsWith("}"))
                {
                    Res.StatusDescription = Regex.Unescape(httpResult.Html);
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "{\"error\":\"", "\",\"msg\":true");
                if (!string.IsNullOrWhiteSpace(errorMsg))
                {
                    Res.StatusDescription = Regex.Unescape(errorMsg);
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                List<HuhehaoteBaseInfo> BaseInfo = jsonParser.DeserializeObject<List<HuhehaoteBaseInfo>>(Regex.Unescape("[" + httpResult.Html) + "]");
                if (BaseInfo.Count > 0)
                {
                    Res.Name = BaseInfo[0].accname;//姓名
                    Res.ProvidentFundNo = BaseInfo[0].accnum;//公积金账号
                    Res.CompanyName = BaseInfo[0].unitaccname;//单位名称
                    Res.CompanyNo = BaseInfo[0].unitaccnum;//单位账号
                    Res.SalaryBase = reg.Replace(BaseInfo[0].basenum, "").ToDecimal(0);//缴存基数
                    Res.Status = BaseInfo[0].indiaccstate;//账号状态
                    Res.CompanyMonthPayRate = reg.Replace(BaseInfo[0].unitprop, "").ToDecimal(0) * 0.01M;//单位缴存比率
                    Res.PersonalMonthPayRate = reg.Replace(BaseInfo[0].indiprop, "").ToDecimal(0) * 0.01M;//个人缴存比率
                    if (Res.SalaryBase > 0 && Res.CompanyMonthPayRate > 0)
                    {
                        Res.CompanyMonthPayAmount = (Res.SalaryBase * Res.CompanyMonthPayRate).ToString("f2").ToDecimal(0); //公司月缴额
                    }
                    else
                    {
                        Res.CompanyMonthPayAmount = (reg.Replace(BaseInfo[0].monpaysum, "").ToDecimal(0) / 2).ToString("f2").ToDecimal(0);
                    }
                    if (Res.SalaryBase > 0 && Res.PersonalMonthPayRate > 0)
                    {
                        Res.PersonalMonthPayAmount = (Res.SalaryBase * Res.PersonalMonthPayRate).ToString("f2").ToDecimal(0); //个人月缴额
                    }
                    else
                    {
                        Res.PersonalMonthPayAmount = (reg.Replace(BaseInfo[0].monpaysum, "").ToDecimal(0) / 2).ToString("f2").ToDecimal(0);
                    }
                    Res.TotalAmount = reg.Replace(BaseInfo[0].lastbal, "").ToDecimal(0);//账户总额
                    Res.LastProvidentFundTime = BaseInfo[0].lastpaydate;//最后缴费时间
                    if (Res.CompanyMonthPayRate > 0 && Res.PersonalMonthPayRate > 0)
                    {
                        totalRate = Res.CompanyMonthPayRate + Res.PersonalMonthPayRate;//总缴费比率（公司+个人）
                        perAccounting = (Res.PersonalMonthPayRate / totalRate);
                        comAccounting = (Res.CompanyMonthPayRate / totalRate);
                    }
                    else
                    {
                        totalRate = (payRate) * 2;//0.16
                        perAccounting = comAccounting = (decimal)0.50;
                    }
                }
                #endregion
                #region 第三步,获取缴费明细

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                _url = baseUrl + "grmx.htm";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("gbk"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                tranCode = CommonFun.GetMidStr(httpResult.Html, "'tranCode':'", "',");
                task = CommonFun.GetMidStr(httpResult.Html, "'task'    : \"", "\",");
                if (string.IsNullOrEmpty(tranCode) || string.IsNullOrEmpty(task))
                {
                    Res.StatusDescription = "缴费明细页面来源页面脚本已改变,请核对网站信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                _url = baseUrl + "PerSendServlet";
                _postData = string.Format("tranCode={0}&task={1}&AccNum={2}&CertiNum={3}", tranCode, task, Res.ProvidentFundNo, Res.IdentityCard);
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "Post",
                    Postdata = _postData,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (!httpResult.Html.StartsWith("[") && !httpResult.Html.EndsWith("]"))
                {
                    Res.StatusDescription = Regex.Unescape(httpResult.Html);
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                List<HuhehaoteDetails> detailList = jsonParser.DeserializeObject<List<HuhehaoteDetails>>(Regex.Unescape(httpResult.Html));
                foreach (var item in detailList)
                {
                    ProvidentFundDetail detail = new ProvidentFundDetail();
                    detail.Description = item.zy.Trim();//描述
                    if (!item.jyrq.Contains("1899"))
                    {
                        detail.PayTime = item.jyrq.ToDateTime();//缴费年月
                    }
                    if (!item.zzyf.Contains("1899"))
                    {
                        detail.ProvidentFundTime = new Regex(@"[0-9-;1-9]{5,7}").Match(item.zzyf).Value.ToTrim("-");
                    }
                    if (item.zy.IndexOf("汇缴", System.StringComparison.Ordinal) > -1)
                    {
                        detail.CompanyPayAmount = (reg.Replace(item.fse, "").ToDecimal(0) * comAccounting).ToString("f2").ToDecimal(0);//单位缴费
                        detail.PersonalPayAmount = (reg.Replace(item.fse, "").ToDecimal(0) * perAccounting).ToString("f2").ToDecimal(0);//个人缴费
                        detail.ProvidentFundBase = (reg.Replace(item.fse, "").ToDecimal(0) / totalRate).ToString("f2").ToDecimal(0);//缴费基数
                        detail.CompanyName = reg.Replace(item.dwmc, "");//单位名称
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        PaymentMonths++;
                    }
                    else
                    {//（补缴，结息etc，数据不精确，只做参考用）
                        detail.PersonalPayAmount = reg.Replace(item.fse, "").ToDecimal(0);//个人缴费
                        detail.CompanyName = reg.Replace(item.dwmc, "");//单位名称
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
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
        /// 用户基本信息
        /// </summary>
        public class HuhehaoteBaseInfo
        {
            /// <summary>
            /// 单位名称
            /// </summary>
            public string unitaccname { get; set; }
            /// <summary>
            /// 当前余额
            /// </summary>
            public string lastbal { get; set; }
            /// <summary>
            /// 个人缴存比例
            /// </summary>
            public string indiprop { get; set; }
            /// <summary>
            /// 姓名
            /// </summary>
            public string accname { get; set; }
            /// <summary>
            /// 账号状态
            /// </summary>
            public string indiaccstate { get; set; }
            /// <summary>
            /// 单位账号
            /// </summary>
            public string unitaccnum { get; set; }
            /// <summary>
            /// 月应缴额(元)
            /// </summary>
            public string monpaysum { get; set; }
            /// <summary>
            /// 最后汇缴月
            /// </summary>
            public string lastpaydate { get; set; }
            /// <summary>
            /// 缴存基数
            /// </summary>
            public string basenum { get; set; }
            /// <summary>
            /// 单位缴存比例
            /// </summary>
            public string unitprop { get; set; }
            /// <summary>
            /// 公积金账号
            /// </summary>
            public string accnum { get; set; }

        }
        /// <summary>
        /// 缴费明细信息
        /// </summary>
        public class HuhehaoteDetails
        {
            /// <summary>
            /// 单位名称
            /// </summary>
            public string dwmc { get; set; }
            /// <summary>
            /// 余额(元)
            /// </summary>
            public string ye { get; set; }
            /// <summary>
            /// 摘要
            /// </summary>
            public string zy { get; set; }
            /// <summary>
            /// 交易日期
            /// </summary>
            public string jyrq { get; set; }
            /// <summary>
            /// 起始月份
            /// </summary>
            public string ksyf { get; set; }
            /// <summary>
            /// 中止月份
            /// </summary>
            public string zzyf { get; set; }
            /// <summary>
            /// 发生额(元)
            /// </summary>
            public string fse { get; set; }
            /// <summary>
            /// 单位账号
            /// </summary>
            public string dwzh { get; set; }
        }
    }
}
