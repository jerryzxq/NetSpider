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
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SD
{
    /// <summary>
    /// 网站未提供公积金明细查询[5566]
    /// </summary>
    public class liaocheng : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = new HttpResult();
        HttpItem httpItem = new HttpItem();
        string baseUrl = "http://search.lcgjj.com.cn/";
        string fundCity = "sd_liaocheng";
        #endregion
        #region 私有变量
        string Url = string.Empty;
        private int PaymentMonths = 0;//连续缴费月数
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                Url = baseUrl + "000/validatecode.asp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl,
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                vcRes.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                vcRes.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                //添加缓存
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
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = fundCity;
            Regex reg = new Regex(@"[\&nbsp;\s]");
            List<string> results = new List<string>();
            string postData = string.Empty;//post参数
            try
            {    //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (string.IsNullOrWhiteSpace(fundReq.Identitycard))
                {
                    Res.StatusDescription = "身份证号不能为空!";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Name))
                {
                    Res.StatusDescription = "姓名不能为空!";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Password))
                {
                    Res.StatusDescription = "密码不能为空!";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一,登陆

                Url = baseUrl + "person_Result.asp";
                postData = string.Format("person_cardnum={0}&person_Name={1}&person_PassWord={2}&validatecode={3}&I1.x=48&I1.y=10", fundReq.Identitycard, fundReq.Name.ToUrlEncode(Encoding.GetEncoding("gb2312")), fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postData,
                    Method = "Post",
                    Referer = baseUrl,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "language=JavaScript>alert('", "')");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                #region 第二步,获取基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='table1']/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.IdentityCard = reg.Replace(results[0], "");//身份证号码
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='table1']/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Name = reg.Replace(results[0], "");//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='table1']/tr[9]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = reg.Replace(results[0], "");//公积金账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='table1']/tr[10]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyNo = reg.Replace(results[0], "");//单位代码
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='table1']/tr[11]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyName = reg.Replace(results[0], "");//单位名称
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='table1']/tr[12]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.OpenTime = new Regex(@"[0-9]{4}-[1-9]{1}-[0-9]{2}").Match(results[0]).Value;//开户日期
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='table1']/tr[13]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.SalaryBase = reg.Replace(results[0], "").ToDecimal(0);//计缴公积金工资基数
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='table1']/tr[14]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = (Res.SalaryBase * reg.Replace(results[0], "").ToDecimal(0) * (decimal)0.01).ToString("f2").ToDecimal(0);
                    Res.PersonalMonthPayRate = reg.Replace(results[0], "").ToDecimal(0)*0.01M;//个人缴存比例
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='table1']/tr[15]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = reg.Replace(results[0], "").ToDecimal(0) - Res.PersonalMonthPayAmount;
                    if (Res.SalaryBase > 0)
                    {
                        Res.CompanyMonthPayRate = (Res.CompanyMonthPayAmount / (Res.SalaryBase)).ToString("f2").ToDecimal(0);//单位缴存比例
                    }
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='table1']/tr[16]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = new Regex(@"[\u4e00-\u9fa5;\s;\&nbsp;]{0,}").Replace(results[0], "");//缴至年月
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='table1']/tr[17]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.TotalAmount = reg.Replace(results[0], "").ToDecimal(0);//缴存余额
                }
                #endregion

                #region ===网站未提供缴费明细查询===
                //5566
                #endregion
                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_QuerySuccess;
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
    }
}
