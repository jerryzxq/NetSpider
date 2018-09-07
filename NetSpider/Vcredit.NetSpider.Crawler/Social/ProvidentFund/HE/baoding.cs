using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
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
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HE
{
    public class baoding : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.bdgjj.gov.cn/";
        string fundCity = "he_baoding";
        int PaymentMonths = 0;
        List<string> results = new List<string>();
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        #endregion
        #region 私有变量
        decimal payRate = (decimal)0.08;
        #endregion
        /// <summary>
        /// 解析保存验证码
        /// </summary>
        /// <returns></returns>
        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "wt-web/captcha?" + new Random().Next();
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
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                //添加缓存
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
        /// <summary>
        /// 抓取页面数据
        /// </summary>
        /// <param name="fundReq"></param>
        /// <returns></returns>
        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            ProvidentFundDetail detail = null;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (string.IsNullOrEmpty(fundReq.Username) || string.IsNullOrEmpty(fundReq.Password) || string.IsNullOrEmpty(fundReq.Identitycard))
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆
                //首先获取页面信息
                Url = baseUrl + "wt-web/login";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string modulus = string.Empty;
                string exponent = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@type='hidden']", "value");
                if (results.Count > 0)
                {
                    modulus = results[0];
                    exponent = results[1];
                }


                Url = baseUrl + "wt-web/login";
                //密码 RSA加密
                var password = RSAHelper.EncryptStringByRsaJS(fundReq.Password, modulus, exponent);
                postdata = string.Format("username={0}&password={1}&captcha={2}&modulus={3}&exponent={4}", fundReq.Username, password, fundReq.Vercode, modulus, exponent);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Allowautoredirect = false,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (!httpResult.Html.IsEmpty())
                {
                    var errormsg = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='error']/i", "");
                    if (errormsg.Count > 0)
                    {
                        Res.StatusDescription = errormsg[0].Replace("<br>", "");
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                cookies = CommonFun.GetCookieCollectionEQ(cookies, httpResult.CookieCollection);

                Url = baseUrl + "wt-web/home";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollectionEQ(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步,获取个人基本信息
                Url = baseUrl + "wt-web/person/jbxx?_=1459304975437";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (!httpResult.Html.StartsWith("{") && !httpResult.Html.EndsWith("}"))
                {
                    Res.StatusDescription = "公积金网站服务器内部错误";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                var flag = jsonParser.GetResultFromParser(httpResult.Html, "success");
                if (flag.ToLower() == "true")
                {
                    var dataString = jsonParser.GetResultFromParser(httpResult.Html, "results").TrimStart('[').TrimEnd(']');
                    var data = JObject.Parse(dataString);
                    if (data.Count > 0)
                    {
                        Res.CompanyNo = data["a003"].ToString();  //单位编号
                        Res.CompanyName = data["a004"].ToString();  //单位名称
                        Res.EmployeeNo = data["a001"].ToString();  //员工编号
                        Res.Name = data["a002"].ToString();  //姓名
                        Res.Status = data["a070"].ToString();  //状态
                        Res.IdentityCard = data["a008"].ToString();  //身份证号
                        Res.Phone = data["yddh"].ToString();  //移动电话
                        Res.Bank = data["khyh"].ToString();  //开户银行
                        Res.BankCardNo = data["yhkh"].ToString();  //银行卡号
                        Res.OpenTime = data["a013"].ToString();  //开户日期
                    }
                }

                Url = baseUrl + "wt-web/person/jczqxx?_=1459304975438";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                flag = jsonParser.GetResultFromParser(httpResult.Html, "success");
                if (flag == "True")
                {
                    var dataString = jsonParser.GetResultFromParser(httpResult.Html, "data").TrimStart('[').TrimEnd(']');
                    var data = JObject.Parse(dataString);
                    if (data.Count > 0)
                    {
                        Res.PersonalMonthPayRate = CommonFun.GetMidStr(data["a097"].ToString(), "个人比例", "").ToDecimal(0);  //个人缴费比例
                        Res.CompanyMonthPayRate = CommonFun.GetMidStr(data["a097"].ToString(), "单位比例", " ").ToDecimal(0);  //单位缴费比例
                        Res.PersonalMonthPayAmount = data["a034"].ToString().ToDecimal(0);  //个人缴费金额
                        Res.CompanyMonthPayAmount = data["a035"].ToString().ToDecimal(0);  //单位缴费金额
                        Res.SalaryBase = data["a011"].ToString().ToDecimal(0);  //工资
                        Res.TotalAmount = data["a044"].ToString().ToDecimal(0);  //余额
                    }
                }

                #endregion
                #region 第三步,获取缴费明细   *********缴存详细为PDF文件，暂时无法读取**********
                Url = baseUrl + "wt-web/person/jcmx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ContentType = "application/pdf",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
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

    }
}
