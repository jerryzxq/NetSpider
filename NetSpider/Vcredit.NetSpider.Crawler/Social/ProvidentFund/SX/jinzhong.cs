using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.Common.Constants;
using System.Text.RegularExpressions;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SX
{
    public class jinzhong : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.sxjzgjj.com/";
        string fundCity = "sx_jinzhong";
        #endregion
        #region 私有变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        string cxyd = string.Empty;
        string cxydmc = string.Empty;
        string dbname = string.Empty;
        string _item = string.Empty;
        string EmployeeNo = string.Empty;
        string IdentityCard = string.Empty;
        string Name = string.Empty;
        string Url = string.Empty;
        string postdata = string.Empty;
        List<string> results = new List<string>();
        int PaymentMonths = 0;
        private decimal payRate = (decimal)0.08;
        private Regex reg = new Regex(@"[\s;\t;\&nbsp;\,;\u5143]*");
        decimal perAccounting;//个人占比
        decimal comAccounting;//公司占比
        decimal totalRate;//总缴费比率
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            Res.Token = token;
            try
            {
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = "所选城市无需初始化";
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

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = "身份证号不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //15位或18位身份证验证
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (regex.IsMatch(fundReq.Identitycard) == false)
                {
                    Res.StatusDescription = "请输入正确的15位或18位身份证号码";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆(包含基本信息和详细信息)

                //初始化
                Url = baseUrl + "search/search_geren.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var list = HtmlParser.GetResultFromParser(httpResult.Html, "//input", "value");

                var __EVENTTARGET = list[0];
                var __EVENTARGUMENT = list[1];
                var __VIEWSTATE = list[2];
                var __EVENTVALIDATION = list[4];

                //登录
                Url = baseUrl + "search/search_geren.aspx";
                postdata = string.Format("__EVENTTARGET={0}&__EVENTARGUMENT={1}&__VIEWSTATE={2}&txtSfz={3}&Button1=%E6%9F%A5%E8%AF%A2&__EVENTVALIDATION={4}", __EVENTTARGET, __EVENTARGUMENT, __VIEWSTATE.ToUrlEncode(), fundReq.Identitycard, __EVENTVALIDATION.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                //基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span", "inner");
                if (results.Count > 0)
                {
                    Res.Name = results[1];  //姓名
                    Res.IdentityCard = results[2] + "***";  //身份证号
                    Res.TotalAmount = results[3].ToDecimal(0);  //账户余额

                }
                else
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                //详细信息
                list = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='gvGeRen']/tr", "inner");
                foreach (var item in list)
                {
                    ProvidentFundDetail detail = new ProvidentFundDetail();
                    List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow.Count != 4)
                    {
                        continue;
                    }
                    detail.Description = tdRow[2];//描述
                    detail.PayTime = tdRow[1].ToDateTime();//缴费年月
                    detail.CompanyName = tdRow[0];//单位名称
                    detail.ProvidentFundTime = Convert.ToDateTime(tdRow[1]).ToString("yyyyMM");//应属年月
                    if (tdRow[3].IndexOf("汇缴", System.StringComparison.Ordinal) > -1)  //唯一账户查询不到缴费信息
                    {
                        //detail.PersonalPayAmount = tdRow[1].ToDecimal(0) * perAccounting.ToString("f2").ToDecimal(0);//个人缴费金额
                        //detail.CompanyPayAmount = tdRow[1].ToDecimal(0) * comAccounting.ToString("f2").ToDecimal(0);//企业缴费金额
                        //detail.ProvidentFundBase = (tdRow[1].ToDecimal(0) / (totalRate)).ToString("f2").ToDecimal(0);//缴费基数
                        //detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                        //detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                        //PaymentMonths++;
                    }
                    else
                    {//（补缴，结息etc，数据不精确，只做参考用）
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0);//个人缴费金额
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

        /// <summary>
        /// 页面请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="postdata">提交数据</param>
        /// <param name="method">请求方法</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="cookies">cookies</param>
        /// <returns>Http返回参数</returns>
        private Vcredit.Common.Helper.HttpResult HttpResult(string url, string postdata, string method, string encoding, CookieCollection cookies)
        {
            httpItem = new HttpItem()
            {
                URL = url,
                Method = method,
                Postdata = postdata,
                Encoding = Encoding.GetEncoding(encoding),
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            return httpResult;
        }
    }
}
