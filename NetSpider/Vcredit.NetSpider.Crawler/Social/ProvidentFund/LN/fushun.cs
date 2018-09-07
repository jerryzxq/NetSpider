using System;
using System.Collections.Generic;
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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.LN
{
    /// <summary>
    /// 未提供公积金明细查询,密码为: 1' or '1' = '1(最前面的1之前有个空格)
    /// </summary>
    public class fushun : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.fushun.gov.cn/";
        string fundCity = "ln_fushun";
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
            decimal payRate = (decimal)0.08;
            string VIEWSTATE = string.Empty;
            string VIEWSTATEGENERATOR = string.Empty;
            string EVENTVALIDATION = string.Empty;
            string Button1 = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (string.IsNullOrWhiteSpace(fundReq.Identitycard) || string.IsNullOrWhiteSpace(fundReq.Password))
                {
                    Res.StatusDescription = "身份证或密码输入有误请重新输入";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //15位或18位身份证验证
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (regex.IsMatch(fundReq.Identitycard) == false)
                {
                    Res.StatusDescription = "请输入有效的15位或18位身份证号";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆

                url = baseUrl + string.Format("gjjqur/search/PersonalDetail.aspx?login={0}&password={1}", fundReq.Identitycard, fundReq.Password);
                httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "Get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
              
                #endregion
                #region 第二步,获取基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Label1']", "inner");

                if (results.Count > 0)
                {
                    if (CommonFun.ClearFlag(results[0]) == "Label")
                    {
                        Res.StatusDescription = "用户信息不存在";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    string[] stringList = results[0].Replace("<br>", "#").Split('#');
                    if (stringList.Length == 12)
                    {
                        Res.Name = stringList[0].Replace("职工姓名：", "");
                        Res.CompanyName = stringList[1].Replace("单位名称：", "");
                        Res.ProvidentFundNo = stringList[2].Replace("职工帐号：", "");
                        Res.IdentityCard = stringList[3].Replace("身份证号码:", "");
                        Res.OpenTime = stringList[4].Replace("开户日期：", "");
                        Res.PersonalMonthPayAmount = ((stringList[5].Replace("缴交额：", "").ToDecimal(0)) / 2).ToString("f2").ToDecimal(0);//个人月缴额
                        Res.CompanyMonthPayAmount = ((stringList[5].Replace("缴交额：", "").ToDecimal(0)) / 2).ToString("f2").ToDecimal(0);//公司月缴额
                        Res.PersonalMonthPayRate = Res.CompanyMonthPayRate = payRate;//缴费比率
                        Res.SalaryBase = (Res.PersonalMonthPayAmount / payRate).ToString("f2").ToDecimal(0);//缴费基数
                        Res.TotalAmount = stringList[6].Replace("余额:", "").ToDecimal(0);
                    }
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
