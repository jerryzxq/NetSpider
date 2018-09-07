using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Ext;
using System.Collections;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using System.Web;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.GZ
{
    /// <summary>
    /// 未提供公积金明细查询,密码为: 1' or '1' = '1(最前面的1之前有个空格)
    /// </summary>
    public class zunyi : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.zygjj.cn/";
        string fundCity = "gz_zunyi";
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                vcRes.StatusDescription = "所选城市无需初始化";
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
            List<string> results = new List<string>();
            Res.ProvidentFundCity = fundCity;
            string url = string.Empty;
            string postData = string.Empty;
            string IDNumber=string.Empty;

            decimal payRate = (decimal) 0.08;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Username.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,验证登陆
                url = baseUrl + "checkuserlogin.asp";
                postData = string.Format(@"username={0}&userpassword={1}&&B3.x=18&B3.y=14", fundReq.Username, fundReq.Password);
                httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "Post",
                    Postdata = postData,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //登陆获取身份证号
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='90%']/tr[1]/td", "inner");
                IDNumber=results[0].Split('=')[2].Replace("target","").Replace("\"","");
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryError;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryError;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //存储cookie
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
              
                #endregion
                #region 第二步,获取基本信息

                //跳转登陆页面
                url = baseUrl + "searchgjj.asp?id=" + IDNumber;
                httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "Get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[1]/tr[1]/td[2]", "inner");

                if (results.Count>0)
                {
                    Res.Name = results[0];       //姓名
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[1]/tr[1]/td[4]", "inner");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];       //身份证号
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[1]/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = results[0].Replace("元", "").ToDecimal(0) / 2;;       //公司月缴费
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[1]/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].Replace("元", "").ToDecimal(0) / 2;;       //个人月缴费
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[1]/tr[2]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0] ;       //缴至年月
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[1]/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].Replace("元", "").ToDecimal(0) ;       //余额
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[1]/tr[4]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0] ;       //单位名称
                }

                Res.SalaryBase = Res.PersonalMonthPayAmount / payRate;          //薪资


                #endregion

                #region ===网站未提供缴费明查询===
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

        /// <summary>
        /// 格式化时间戳
        /// </summary>
        private static string ReplaceString(string str, int a, int b)
        {
            List<string> oldStr = str.Split(' ').ToList();
            List<string> newStr = str.Split(' ').ToList();
            newStr[a] = oldStr[b];
            newStr[b] = oldStr[a];
            string s1 = string.Join("%20", newStr.ToArray());
            return s1;
        } 
    }
}
