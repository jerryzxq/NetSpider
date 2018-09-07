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
    /// 雅安市未提供公积金明细查询
    /// </summary>
    public class yaan : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.yaan.gov.cn/";
        string fundCity = "sc_yaan";
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
            Res.ProvidentFundCity = fundCity;
            List<string> results = new List<string>();
            string url = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                //15位或18位身份证验证
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (!regex.IsMatch(fundReq.Identitycard))
                {
                    Res.StatusDescription ="请输入有效的身份证号";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆

                url = baseUrl + string.Format("htm/searchback.htm?sfz={0}&action=gjj", fundReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "GET",
                    Referer = baseUrl + "htm/serviceindex.htm",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='cxlb']/table/tr[1]/td[4]", "text");
                if (httpResult.Html.Contains("没有查询到相关信息"))
                {
                    Res.StatusDescription = "没有查询到相关信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                #region 第二步,获取基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='cxlb']/table/tr[1]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];//公积金账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='cxlb']/table/tr[1]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='cxlb']/table/tr[2]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0];//最后缴费时间
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='cxlb']/table/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0].Trim();//身份证号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='cxlb']/table/tr[5]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);//账户总额
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='cxlb']/table/tr[5]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.Status = results[0];//状态
                }
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
    }
}
