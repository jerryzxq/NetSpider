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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SD
{
    public class zibo : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.zbzfgjj.com/search/";//网址
        string fundCity = "sd_zibo";
        #endregion

        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "AccumulationFund.aspx";//http://www.zbzfgjj.com/search/AccumulationFund.aspx
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("utf-8"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                string VIEWSTATE = string.Empty;
                string EVENTVALIDATION = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value", true);
                if (results.Count > 0)
                {
                    VIEWSTATE = results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Url = baseUrl + "ValidateCode.aspx";//验证码地址http://www.zbzfgjj.com/search/ValidateCode.aspx
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("utf-8"),
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
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("VIEWSTATE", VIEWSTATE);
                CacheHelper.SetCache(token, dics);
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
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            ProvidentFundDetail detail = null;
            int PaymentMonths = 0;
            try
            {
                string VIEWSTATE = string.Empty;
                string EVENTVALIDATION = string.Empty;
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(fundReq.Token);
                    VIEWSTATE = dics["VIEWSTATE"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (StringExtension.IsEmpty(fundReq.Username) || StringExtension.IsEmpty(fundReq.Identitycard))
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 查询信息
                Url = baseUrl + "AccumulationFund.aspx";
                postdata = String.Format("__VIEWSTATE={3}&t1={0}&t2={1}&t3={2}", fundReq.Username, fundReq.Identitycard, fundReq.Vercode, StringExtension.ToUrlEncode(VIEWSTATE));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
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
                string errorInfo = CommonFun.GetMidStr(httpResult.Html, "<script>alert(", "');</script>");
                if (!errorInfo.IsEmpty())
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[1]/form/div[2]/div[1]/div/div[1]/div[3]", "innertext");
                if (results.Count > 0)
                {
                    if (!results[0].Contains("缴至年月"))
                    {
                        Res.StatusDescription =CommonFun.ClearFlag(results[0]);
                        Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                        return Res;
                    }
                }
                else
                {
                    Res.StatusDescription = "网站个人基本信息页面发生改动";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                string handleStr = CommonFun.ClearFlag(results[0]).Replace("缴至年月：", " ").Replace("姓名：", " ").Replace("余额：", " ").Trim();
                string[] array = handleStr.Split(' ');
                if (array.Length==3)
                {
                    Res.LastProvidentFundTime=array[0];//缴至年月
                    Res.Name = array[1];//姓名
                    Res.TotalAmount = array[2].ToDecimal(0);//余额
                    Res.IdentityCard = fundReq.Identitycard;
                    Res.ProvidentFundNo = fundReq.Username;
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
