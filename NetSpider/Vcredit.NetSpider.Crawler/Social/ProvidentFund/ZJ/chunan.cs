using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.ZJ
{
    public class chunan : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.cagjj.cn/";
        string fundCity = "zj_chunan";
        #endregion

        #region   私有变量
        decimal payrate = (decimal)0;
        string __EVENTVALIDATION = string.Empty;
        string __VIEWSTATE = string.Empty;
        string __VIEWSTATEGENERATOR = string.Empty;
        #endregion
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                //获取页面参数
                Url = baseUrl + "Interact/UserLogin.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@type='hidden']", "value");
                if (results.Count > 0)
                {
                    __VIEWSTATE = results[0];
                    __VIEWSTATEGENERATOR = results[1];
                    __EVENTVALIDATION = results[2];
                }

                Url = baseUrl + "VerifyCode.aspx?wn=5";
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
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
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

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(ProvidentFundReq fundReq)
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
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "身份证号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步登陆
                Url = baseUrl + "Interact/UserLogin.aspx";
                postdata = string.Format("__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__EVENTVALIDATION={2}&txt_UserName={3}&txt_Pwd={4}&txt_VerifyCode={5}&btn_Login=%E7%99%BB%E5%BD%95", __VIEWSTATE.ToUrlEncode(), __VIEWSTATEGENERATOR.ToUrlEncode(), __EVENTVALIDATION.ToUrlEncode(), fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Host = "www.cagjj.cn",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                var errormsg = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "');window");
                if (!errormsg.IsEmpty() && !errormsg.Contains("登录成功"))
                {
                    Res.StatusDescription = errormsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Url = "http://www.cagjj.cn/Interact/PersonalCenter.aspx";
                httpItem = new HttpItem
                {
                    URL = Url,
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
                #endregion


                #region  第二步 获取基本信息
                //在第一步登录中已经获得基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[1]/tr/td", "");
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[1].Split('：')[1];  //个人编号
                    Res.Name = results[2].Split('：')[1];  //姓名
                    Res.Sex = results[3].Split('：')[1];  //性别
                    Res.IdentityCard = results[4].Split('：')[1];  //身份证号
                    Res.SalaryBase = results[5].Split('：')[1].ToDecimal(0);  //缴费基数
                    Res.TotalAmount = results[10].Split('：')[1].ToDecimal(0);  //余额
                    Res.Status = results[11].Split('：')[1];  //状态
                    Res.OpenTime = results[12].Split('：')[1];  //开户日期
                }
                #endregion

                #region 第三部 获取缴费详单
                //在第一步登录种已经获得缴费详单信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='operate'][2]/tr", "");
                foreach (var item in results)
                {
                     detail = new ProvidentFundDetail();
                     var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                     if (tdRow.Count!=6||tdRow[0].Contains("序号"))
                     {
                         continue;
                     }
                     detail.PayTime = tdRow[1].ToDateTime() ;  //缴费日期
                     if (tdRow[2].Contains("托成登记"))
                     {
                         detail.PersonalPayAmount = tdRow[3].ToDecimal(0);  //暂定为个人缴费
                         detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;  //缴费标志
                         detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;  //缴费类型
                         PaymentMonths++;
                     }
                     else
                     {
                         detail.PersonalPayAmount = tdRow[3].ToDecimal(0);  //暂定为个人缴费
                         detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;  //缴费标志
                         detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;  //缴费类型
                     }
                   
                     Res.ProvidentFundDetailList.Add(detail);
                }

                #endregion

                #region 第四步 贷款信息*********账号暂无贷款信息 ***********

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
