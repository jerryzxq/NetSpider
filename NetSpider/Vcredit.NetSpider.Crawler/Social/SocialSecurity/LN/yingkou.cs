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
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.Common.Constants;
using Vcredit.NetSpider.DataAccess.Cache;
using System.Text.RegularExpressions;
namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.LN
{
    public class yingkou : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.lnyk.lss.gov.cn:7002/";
        string socialCity = "ln_yingkou";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://www.lnyk.lss.gov.cn:7002/hso/perLogonPage.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.lnyk.lss.gov.cn:7002/hso/authcode?i=0.6547184928490167";
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

                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitError;
                Log4netAdapter.WriteError(socialCity + ServiceConsts.SocialSecurity_InitError, e);
            }
            return Res;
        }

        public SocialSecurityQueryRes SocialSecurityQuery(SocialSecurityReq socialReq)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = socialCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(socialReq.Token);
                    SpiderCacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Password.IsEmpty() || socialReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = "身份证号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                var password = CommonFun.GetMd5Str(socialReq.Password);  //密码通过md5加密
                Url = baseUrl + "hso/logon.do";
                postdata = String.Format("method=doLogonAllowRepeat&usertype=1&username={0}&password={1}&validatecode={2}", socialReq.Identitycard, password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
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
                if (!httpResult.Html.Contains("true"))
                {
                    Res.StatusDescription = "用户名或密码错误";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，个人基本信息
                //人员基本信息
                Url = baseUrl + "hso/hsoPer.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = "method=QueryPersonBaseInfo",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='sfzhm']", "value");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];  //身份证号
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='xm']", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];  //姓名
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='xbmc']", "value");
                if (results.Count > 0)
                {
                    Res.Sex = results[0];  //性别
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='csrq']", "value");
                if (results.Count > 0)
                {
                    Res.BirthDate = results[0];  //出生日期
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='jtzz']", "value");
                if (results.Count > 0)
                {
                    Res.Address = results[0];  //地址
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='mzmc']", "value");
                if (results.Count > 0)
                {
                    Res.Race = results[0];  //民族
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='lxdh']", "value");
                if (results.Count > 0)
                {
                    Res.Phone = results[0];  //联系电话
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='yzbm']", "value");
                if (results.Count > 0)
                {
                    Res.ZipCode = results[0];  //邮政编码
                }
                #endregion



                #region 第三步，养老缴费明细

                #region 养老保险 *******不存在养老保险********
                Url = baseUrl + "hso/persi.do";
                postdata = string.Format("method=queryZgYanglaozh");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='accountPay']", "");
                if (results.Count > 0)
                {

                }
                #endregion

                #region 医疗保险

                Url = baseUrl + "hso/persi.do";
                postdata = string.Format("method=queryZgMediPay");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='accountPay']/tr", "");
                if (results.Count > 0)
                {
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@name='nd']/option", "");
                    results.RemoveAt(0);
                    foreach (var item in results)
                    {
                        Url = baseUrl + "hso/persi.do";
                        postdata = string.Format("method=queryZgMediPay&_xmlString=<?xml version=\"1.0\" encoding=\"UTF-8\"?><p><s nd=\""+item+"\"/></p>");
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "Post",
                            Postdata = postdata,
                            ContentType="application/x-www-form-urlencoded;charset=UTF-8",
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='accountPay']/tr", "");
                        foreach (var item2 in results)
                        {
                            detailRes = new SocialSecurityDetailQueryRes();
                            var tdRow=HtmlParser.GetResultFromParser(item2, "//td/input", "value", true);
                            if (tdRow.Count!=7)
                            {
                                continue; 
                            }

                            detailRes.PayTime = tdRow[0];  //缴费年月
                            detailRes.CompanyName = tdRow[1];  //单位名称
                            Res.CompanyName = detailRes.CompanyName;
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            detailRes.SocialInsuranceBase = tdRow[5].ToDecimal(0);  //个人缴费基数
                            detailRes.MedicalAmount = tdRow[6].ToDecimal(0);  //个人缴费
                            detailRes.CompanyMedicalAmount = tdRow[4].ToDecimal(0);  //单位缴费
                            PaymentMonths++;
                            Res.Details.Add(detailRes);
                        }
                    }
                }
                #endregion

                #region 失业保险 *******不存在失业保险********
                Url = baseUrl + "hso/persi.do";
                postdata = string.Format("method=queryZgShiyejf");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='accountPay']", "");
                if (results.Count > 0)
                {

                }
                #endregion

                #region 工伤保险 *******不存在工伤保险********
                Url = baseUrl + "hso/persi.do";
                postdata = string.Format("method=queryZgGsjf");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='accountPay']", "");
                if (results.Count > 0)
                {

                }
                #endregion

                #region 生育保险 *******不存在生育保险********
                Url = baseUrl + "hso/persi.do";
                postdata = string.Format("method=queryZgShengyujf");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='accountPay']", "");
                if (results.Count > 0)
                {

                }

                #endregion


                #endregion
                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(socialCity + ServiceConsts.SocialSecurity_QueryError, e);
            }
            return Res;
        }
    }
}
