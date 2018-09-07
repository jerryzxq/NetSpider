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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.FJ
{
    public class zhangzhou : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string _lt = string.Empty;
        string _md5_random_salt = string.Empty;
        string baseUrl = "http://zzsb.fzxhit.com:80/logincenter/personal/";
        string baseLoginUrl = "http://yzzx2.fjshldbx.com.cn/siusercenter/";
        string socialCity = "fj_zhangzhou";
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
                Url = baseLoginUrl + "login?service=" + baseUrl + "&serverid=22";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _lt = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='lt']", "value")[0];
                _md5_random_salt = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='md5_random_salt']", "value")[0];
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseLoginUrl + "captcha.htm";
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

                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("cookies", cookies);
                dic.Add("_lt", _lt);
                dic.Add("_md5_random_salt", _md5_random_salt);
                CacheHelper.SetCache(token, dic);
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
            Dictionary<string, object> dic = null;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    dic = (Dictionary<string, object>)CacheHelper.GetCache(socialReq.Token);
                    cookies = (CookieCollection)dic["cookies"];
                    _lt = (string)dic["_lt"];
                    _md5_random_salt = (string)dic["_md5_random_salt"];
                }
                //校验参数
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                System.Threading.Thread.Sleep(1000);
                Url = baseLoginUrl + "login?service=" + baseUrl.ToUrlEncode() + "&serverid=22";
                postdata = String.Format("username={0}&md5_random_salt={1}&ylzpassword={2}&password={3}&j_captcha_response={4}&lt={5}&_eventId=submit", socialReq.Username, _md5_random_salt, CommonFun.GetMd5Str(socialReq.Password), CommonFun.GetMd5Str(CommonFun.GetMd5Str(socialReq.Password + socialReq.Username) + _md5_random_salt), socialReq.Vercode, _lt);
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseLoginUrl + "login?service=" + baseUrl + "&serverid=22",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='status']");
                if (results.Count > 0 && !string.IsNullOrWhiteSpace(results[0]))
                {
                    Res.StatusDescription =results[0].Trim();
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                _lt = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='lt']", "value")[0];
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies status

                #endregion

                #region 第二步，登录校验，进入系统

                Url = baseLoginUrl + "login?service=" + baseUrl + "&serverid=22";
                postdata = String.Format("lt={0}&_eventId=continue&hasMobilePhoneNo=1&hasChangePassword=1", _lt);
                httpItem = new HttpItem()
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseLoginUrl + "login?service=" + baseUrl + "&serverid=22",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies


                #endregion

                #region 第三步， 获取基本信息
                Url = baseUrl + "search/viewYLPersonInfo.do?_=1436495935847";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "main/main.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);


                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='aac002']", "value");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证号
                }
                else
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='aac003']", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='aac004_view']", "value");
                if (results.Count > 0)
                {
                    Res.Sex = results[0];//性别
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='aac006']", "value");
                if (results.Count > 0)
                {
                    Res.BirthDate = results[0];//出生日期
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='aac007']", "value");
                if (results.Count > 0)
                {
                    Res.WorkDate = results[0];//参加工作时间
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='aab004']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];//单位名称
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='aab001']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];//单位编号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='aac005_view']", "value");
                if (results.Count > 0)
                {
                    Res.Race = results[0];//民族
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='aac008_view']", "value");
                if (results.Count > 0)
                {
                    Res.EmployeeStatus = results[0];//人员状态
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='aae006']", "value");
                if (results.Count > 0)
                {
                    Res.Address = results[0];//通讯地址
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='aae007']", "value");
                if (results.Count > 0)
                {
                    Res.ZipCode = results[0];//邮政编码
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='aae005']", "value");
                if (results.Count > 0)
                {
                    Res.Phone = results[0];//联系电话
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='aab033_view']", "value");
                if (results.Count > 0)
                {
                    Res.SpecialPaymentType = results[0];//缴费方式
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='biaa05_view']", "value");
                if (results.Count > 0)
                {
                    Res.District = results[0];//地区
                }

                //个人账户总额、月数
                Url = baseUrl + "search/viewYLPersonalCbjfpz.do?_=1442216363079";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "main/main.do#zhcx_sb_gssyxxcx",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                Res.PersonalInsuranceTotal = CommonFun.GetMidStr(CommonFun.GetMidStr(httpResult.Html, "本地参保期间个人账户存储额</td><td colspan=\"7\" class=\"xl32\" style=\"border-left:none\">", ""), "", "</td>").ToDecimal(0);
                Res.PaymentMonths = CommonFun.GetMidStr(CommonFun.GetMidStr(httpResult.Html, "本地实际缴费月数</td><td colspan=\"3\" class=\"xl32\" style=\"border-left:none\">", ""), "", "</td>").ToInt(0);

                #endregion

                #region 第四步，查询明细
                DateTime endDate = DateTime.Now;
                DateTime beginDate = DateTime.Parse(endDate.AddYears(-5).ToString("yyyy-01-01"));

                Url = baseUrl + "search/ylPersonPaymentList.do?s_isSearch=0&_=1436509274136";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "main/main.do",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies


                Url = baseUrl + "search/ylPersonPaymentList.do";
                postdata = string.Format("pageNum=1&numPerPage=100&orderField=&s_biea60_begin={0}&s_biea60_end={1}&s_aae143=&s_orderby=baeah2&s_isSearch=1", beginDate.ToString("yyyy-MM"), endDate.ToString("yyyy-MM"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Timeout = 1000000,
                    ReadWriteTimeout = 300000,
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table [@class='table']/tbody/tr[@target='k_baeah2']", "inner");

                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                    if (tdRow.Count != 13)
                    {
                        continue;
                    }
                    if (string.IsNullOrEmpty(Res.EmployeeNo))
                        Res.EmployeeNo = tdRow[0];//编号

                    for (int i = 0; i < tdRow[8].ToInt(0); i++)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();

                        detailRes.PayTime = tdRow[4];
                        detailRes.SocialInsuranceTime = (tdRow[6].ToInt(0) + i).ToString();
                        detailRes.SocialInsuranceBase = (tdRow[9].ToDecimal(0)) / tdRow[8].ToDecimal(0);
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = tdRow[5] != "正常应缴记录" ? ServiceConsts.SocialSecurity_PaymentFlag_Adjust : ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detailRes.PensionAmount = (tdRow[11].ToDecimal(0)) / tdRow[8].ToDecimal(0);
                        detailRes.CompanyPensionAmount = (tdRow[10].ToDecimal(0)) / tdRow[8].ToDecimal(0);
                        detailRes.CompanyName = tdRow[1];

                        Res.Details.Add(detailRes);
                    }
                }

                #endregion

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
