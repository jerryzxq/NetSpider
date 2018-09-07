using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SC
{
    /// <summary>
    /// 网站未提供公积金明细查询;没有有效账号进行信息测试
    /// </summary>
    public class guangyuan : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = new HttpResult();
        HttpItem httpItem = new HttpItem();
        string baseUrl = "http://218.200.202.157:10000/";
        string fundCity = "sc_guangyuan";
        #endregion
        #region 私有变量
        string Url = string.Empty;
        private ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        private List<string> _results = new List<string>();
        string _postData = string.Empty;//post参数
        //private int PaymentMonths = 0;//连续缴费月数
        decimal payRate = (decimal)0.08;
        private Regex _reg = new Regex(@"[\&nbsp;\%;\s;\,;\u5143;]");
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            string flag = string.Empty;
            string fundAccount = string.Empty;
            string account = string.Empty;
            string lk = string.Empty;
            try
            {
                Url = baseUrl + "ap/fund/jsp/sel/fundWebSel.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='flag']", "value", true);
                if (_results.Count > 0)
                {
                    flag = _results[0];
                }
                else
                {
                    vcRes.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    vcRes.StatusCode = ServiceConsts.StatusCode_fail;
                    return vcRes;
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='fundAccount']", "value", true);
                if (_results.Count > 0)
                {
                    fundAccount = _results[0];
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='account']", "value", true);
                if (_results.Count > 0)
                {
                    account = _results[0];
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='lk']", "value", true);
                if (_results.Count > 0)
                {
                    lk = _results[0];
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = baseUrl + "common/validImage.jsp";
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
                    vcRes.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    vcRes.StatusCode = ServiceConsts.StatusCode_fail;
                    return vcRes;
                }
                vcRes.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);

                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                vcRes.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                //添加缓存
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("flag", flag);
                dics.Add("fundAccount", fundAccount);
                dics.Add("account", account);
                dics.Add("lk", lk);
                CacheHelper.SetCache(token, dics);
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
            string flag = string.Empty;
            string fundAccount = string.Empty;
            string account = string.Empty;
            string lk = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(fundReq.Token);
                    flag = dics["flag"].ToString();
                    fundAccount = dics["fundAccount"].ToString();
                    account = dics["account"].ToString();
                    lk = dics["lk"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (StringExtension.IsEmpty(fundReq.Name) || StringExtension.IsEmpty(fundReq.Identitycard) || StringExtension.IsEmpty(fundReq.Password))
                {
                    Res.StatusDescription = "姓名、身份证或密码为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步,登陆
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = baseUrl + "web";
                _postData = String.Format("selType={0}&fundName={1}&flag={2}&identification={3}&fundAccount={4}&account={5}&flag={6}&fundPwd={7}&validcode={8}&lk={9}", "1", fundReq.Name.ToUrlEncode(), flag, fundReq.Identitycard, fundAccount, account, flag, fundReq.Password, fundReq.Vercode, lk);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Referer = baseUrl + "ap/fund/jsp/sel/fundWebSel.jsp",
                    Postdata = _postData,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                #endregion
                #region 第二步,获取基本信息
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@border='1']/tbody/tr[2]/td[2]", "innertext");
                if (_results.Count > 0)
                {
                    Res.Name = _reg.Replace(_results[0], "");//姓名
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@border='1']/tbody/tr[3]/td[2]", "innertext");
                if (_results.Count > 0)
                {
                    Res.ProvidentFundNo = _reg.Replace(_results[0], "");//公积金账号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@border='1']/tbody/tr[4]/td[2]", "innertext");
                if (_results.Count > 0)
                {
                    Res.IdentityCard = _reg.Replace(_results[0], "");//身份证号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@border='1']/tbody/tr[5]/td[2]", "innertext");
                if (_results.Count > 0)
                {
                    Res.CompanyName = _reg.Replace(_results[0], "");//单位名称
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@border='1']/tbody/tr[6]/td[2]", "innertext");
                if (_results.Count > 0)
                {
                    Res.OpenTime = _reg.Replace(_results[0], "");//开户日期
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@border='1']/tbody/tr[11]/td[2]", "innertext");
                if (_results.Count > 0)
                {
                    Res.TotalAmount = _reg.Replace(_results[0], "").ToDecimal(0);//公积金余额
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@border='1']/tbody/tr[13]/td[2]", "innertext");
                if (_results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = _reg.Replace(_results[0], "").ToDecimal(0);//单位缴存
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@border='1']/tbody/tr[14]/td[2]", "innertext");
                if (_results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = _reg.Replace(_results[0], "").ToDecimal(0);//个人缴存
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@border='1']/tbody/tr[15]/td[2]", "innertext");
                if (_results.Count > 0)
                {
                    Res.LastProvidentFundTime = _reg.Replace(_results[0], "");//缴至月度
                }
                #endregion
                #region ===网站未提供公积金明细查询===

                #endregion

                //Res.PaymentMonths = PaymentMonths;
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
    }
}
