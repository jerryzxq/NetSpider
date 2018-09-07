using System;
using System.Collections.Generic;
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
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HA
{
    public class zhumadian : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.zmdgjj.com/s/";
        string fundCity = "ha_zhumadian";
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
            string Url = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            int PaymentMonths = 0;
            string flag = string.Empty;//查询类型1:单位,2:个人
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || fundReq.Name.IsEmpty())
                {
                    Res.StatusDescription = "用户名或身份证号为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆

                Url = baseUrl + "index.asp?flag=&id=&name=";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='flag']", "value");
                if (results.Count > 0)
                {
                    flag = results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Url = baseUrl + string.Format("search.asp?flag={0}&id={1}&name={2}&submit.x=43&submit.y=13", flag, fundReq.Identitycard, fundReq.Name.ToUrlEncode(Encoding.GetEncoding("gb2312")));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("gb2312"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "language='javascript'>alert('", "');window.history.back(-1);").Trim();
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                #region 第二步, 基本信息,网站未提供

                Res.Name = fundReq.Name;//姓名
                Res.IdentityCard = fundReq.Identitycard;//身份证号

                #endregion
                #region 第三步,获取缴费明细
                decimal perAccounting = 0;//个人占比
                decimal comAccounting = 0;//公司占比
                decimal totalRate = 0;//总缴费比率
                Regex reg = new Regex(@"[\s;\&nbsp;\,;\%]");
                if (Res.PersonalMonthPayRate > 0 && Res.CompanyMonthPayRate > 0)
                {
                    totalRate = Res.PersonalMonthPayRate + Res.CompanyMonthPayRate;
                    perAccounting = (Res.PersonalMonthPayRate / totalRate);
                    comAccounting = (Res.CompanyMonthPayRate / totalRate);
                }
                else
                {
                    totalRate = (payRate * 100) * 2;//16
                    perAccounting = comAccounting = (decimal)0.50;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='mytable']/tbody/tr", "inner");
                foreach (string item in results)
                {
                    ProvidentFundDetail detail = new ProvidentFundDetail();
                    List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow.Count != 7)
                    {
                        continue;
                    }
                    detail.Description = tdRow[2];//描述
                    detail.PayTime = tdRow[1].ToDateTime();//缴费年月
                    detail.ProvidentFundTime = reg.Replace(tdRow[3], "");//应属年月
                    if (tdRow[1].IndexOf("汇缴", System.StringComparison.Ordinal) > -1)
                    {
                        detail.PersonalPayAmount = reg.Replace(tdRow[4], "").ToDecimal(0) * perAccounting;//个人缴费金额
                        detail.CompanyPayAmount = reg.Replace(tdRow[4], "").ToDecimal(0) * comAccounting;//企业缴费金额
                        detail.ProvidentFundBase = reg.Replace(tdRow[4], "").ToDecimal(0) / (totalRate * (decimal)0.01);//缴费基数
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                        PaymentMonths++;
                    }
                    else
                    {//（补缴，结息etc，数据不精确，只做参考用）
                        detail.PersonalPayAmount = reg.Replace(tdRow[4], "").ToDecimal(0) + reg.Replace(tdRow[5], "").ToDecimal(0);//个人缴费金额
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
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
    }
}
