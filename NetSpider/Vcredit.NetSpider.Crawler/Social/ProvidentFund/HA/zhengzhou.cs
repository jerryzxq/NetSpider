using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HA
{
    public class zhengzhou : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.zzgjj.com/";
        string fundCity = "ha_zhengzhou";
        #endregion
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = "所选城市无需初始化";
                //CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return Res;
        }
        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            Regex reg = new Regex(@"[\&nbsp;\,;\u5143;\s]*");
            try
            {
                //获取缓存
                //if (CacheHelper.GetCache(fundReq.Token) != null)
                //{
                //    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                //    CacheHelper.RemoveCache(fundReq.Token);
                //}
                //校验参数
                string txtNumber;//登陆账号
                string loginType;//登录方式 1:账号，2:身份证号
                if (fundReq.LoginType=="2")
                {
                    if (string.IsNullOrWhiteSpace(fundReq.Identitycard) || string.IsNullOrWhiteSpace(fundReq.Name))
                    {
                        Res.StatusDescription = "请输入您的姓名或身份账号";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    loginType = "2";
                    Res.IdentityCard =txtNumber= fundReq.Identitycard;//身份账号
                }
               
                else
                {
                    if (string.IsNullOrWhiteSpace(fundReq.Username) || string.IsNullOrWhiteSpace(fundReq.Name) )
                    {
                        Res.StatusDescription = "请输入您的姓名或公积金账号";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    loginType = "1";
                    txtNumber = fundReq.Username;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Password))
                {
                    Res.StatusDescription = "请输入您的密码";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆
                Url = baseUrl + "user/login.asp";//http://www.zzgjj.com/user/login.asp
                postdata = string.Format("selectlb={4}&username={0}&radename={1}&mm={2}&submit322={3}", txtNumber, fundReq.Name.ToUrlEncode(Encoding.GetEncoding("GB2312")), fundReq.Password.ToUrlEncode(), "确认".ToUrlEncode(Encoding.GetEncoding("gb2312")), loginType);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Encoding=Encoding.GetEncoding("gb2312"),
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "language=JavaScript>alert('", "');javascript:this.history.go(-1)").Trim();
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                #region 第二步,获取基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='96%']/tr/td/table[@width='98%']/tr[1]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.Name = results[0]; //姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='96%']/tr/td/table[@width='98%']/tr[1]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];//个人编号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='96%']/tr/td/table[@width='98%']/tr[2]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.SalaryBase=reg.Replace(results[0],"").ToDecimal(0)/(payRate*2);//缴费基数
                    Res.PersonalMonthPayRate = payRate;
                    Res.CompanyMonthPayRate = payRate;
                    Res.CompanyMonthPayAmount = reg.Replace(results[0], "").ToDecimal(0) / 2;//公司月缴额
                    Res.PersonalMonthPayAmount = reg.Replace(results[0], "").ToDecimal(0) / 2;//个人月缴费
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='96%']/tr/td/table[@width='98%']/tr[2]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.Status = results[0];//账户状态
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='96%']/tr/td/table[@width='98%']/tr[4]/td[2]", "innertext");
                if (results.Count > 0)
                {
                    Res.TotalAmount = reg.Replace(results[0], "").ToDecimal(0);//总 余 额 
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='96%']/tr/td/table[@width='98%']/tr[5]/td[40]", "innertext");
                if (results.Count > 0 && !string.IsNullOrWhiteSpace(results[0]))
                {
                    Res.LastProvidentFundTime = Convert.ToDateTime(results[0]).ToString("yyyyMM");//缴至月份
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='96%']/tr/td/table[@width='98%']/tr[5]/td[4]", "innertext");
                if (results.Count > 0)
                {
                    Res.OpenTime = results[0];//开户日期
                }
                #endregion
                #region 缴费明细，网站未提供
                //5566
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
    }
}
