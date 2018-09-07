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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.HB
{
    public class zhijiang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://219.139.158.11:8082/zj/";
        string socialCity = "hb_zhijiang";
        string submit = string.Empty;
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "LogoutServlet";
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
                    Res.StatusDescription = socialCity + ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                var result = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='submit']", "value");
                if (result.Count > 0)
                {
                    submit = result[0];
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

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
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Name.IsEmpty() || socialReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = baseUrl + string.Format("ValidateUserServlet?xm={0}&sfz={1}&submit={2}", socialReq.Name.ToUrlEncode(Encoding.GetEncoding("gb2312")), socialReq.Identitycard, submit.ToUrlEncode(Encoding.GetEncoding("gb2312")));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Encoding = Encoding.GetEncoding("gbk"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var errormsg = httpResult.Html.Split('\\')[0];
                if (errormsg.Contains("不正确"))
                {
                    Res.StatusDescription = errormsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 第二步， 获取基本信息
                Url = baseUrl + string.Format("GrjbxxServlet?sfz={0}", socialReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tbody/tr/td", "");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[2];//身份证号
                    Res.CompanyNo = results[4];//单位编号
                    Res.EmployeeNo = results[6];//个人编号
                    Res.Name = results[8];//姓名
                    Res.Sex = results[10];//性别
                    Res.BirthDate = results[12];//出生日期
                    Res.Race = results[14];//民族
                    Res.EmployeeStatus = results[18];//人员状态
                    Res.Bank = results[20];//发卡银行
                }
                else
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion


                #region 第三步，查询明细（存在机关养老保险与事业养老保险区别）
                //企业养老保险
                var year = DateTime.Now.Year;
                Url = baseUrl + string.Format("SbjfjlServlet?px=A&sfz={0}&JumpPage=1&PageRow=30000&ksn=1996&ksy=1&jsn={1}&jsy=12&xzlx=qyyl&%B2%E9%D1%AF=%B2%E9%D1%AF", socialReq.Identitycard, year);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Encoding = Encoding.GetEncoding("gbk"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tbody/tr", "");
                if (results.Count <= 0)  //如果企业养老保险不存在，则查询机关养老保险
                {
                    Url = baseUrl + string.Format("SbjfjlServlet?px=A&sfz={0}&JumpPage=1&PageRow=30000&ksn=1996&ksy=1&jsn={1}&jsy=12&xzlx=jgsyyl&%B2%E9%D1%AF=%B2%E9%D1%AF", socialReq.Identitycard, year);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Encoding = Encoding.GetEncoding("gbk"),
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tbody/tr", "");
                }

                //查询失业保险
                Url = baseUrl + string.Format("SbjfjlServlet?px=A&sfz={0}&JumpPage=1&PageRow=30000&ksn=1996&ksy=1&jsn={1}&jsy=12&xzlx=sy&%B2%E9%D1%AF=%B2%E9%D1%AF", socialReq.Identitycard, year);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Encoding = Encoding.GetEncoding("gbk"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var UnemployList = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tbody/tr", "");

                //基本医疗保险
                Url = baseUrl + string.Format("SbjfjlServlet?px=A&sfz={0}&JumpPage=1&PageRow=30000&ksn=1996&ksy=1&jsn={1}&jsy=12&xzlx=jbyl&%B2%E9%D1%AF=%B2%E9%D1%AF", socialReq.Identitycard, year);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Encoding = Encoding.GetEncoding("gbk"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var MedicalList = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tbody/tr", "");

                //大额医疗保险
                Url = baseUrl + string.Format("SbjfjlServlet?px=A&sfz={0}&JumpPage=1&PageRow=30000&ksn=1996&ksy=1&jsn={1}&jsy=12&xzlx=deyl&%B2%E9%D1%AF=%B2%E9%D1%AF", socialReq.Identitycard, year);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Encoding = Encoding.GetEncoding("gbk"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var IllnessMedicalList = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tbody/tr", "");

                foreach (var item in results)
                {
                    detailRes = new SocialSecurityDetailQueryRes();
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                    if (tdRow.Count != 7 || tdRow[0].Contains("缴费"))
                    {
                        continue;
                    }
                    detailRes.PayTime = tdRow[0].Replace("\r", "").Replace("\n", "").Replace("\t", ""); //缴费年月
                    if (tdRow[6].Contains("实缴"))
                    {
                        detailRes.SocialInsuranceBase = tdRow[2].Replace("\r", "").Replace("\n", "").Replace("\t", "").ToDecimal(0);  //缴费基数
                        detailRes.PersonalTotalAmount = tdRow[3].Replace("\r", "").Replace("\n", "").Replace("\t", "").ToDecimal(0);  //缴费金额（暂定为个人缴费）
                        detailRes.PaymentFlag = tdRow[6].Replace("\r", "").Replace("\n", "").Replace("\t", "");  //缴费标志
                        detailRes.PaymentType = tdRow[5].Replace("\r", "").Replace("\n", "").Replace("\t", "");  //缴费类型
                        PaymentMonths++;
                    }
                    else
                    {
                        detailRes.SocialInsuranceBase = tdRow[1].ToDecimal(0);  //缴费基数
                        detailRes.PersonalTotalAmount = tdRow[2].ToDecimal(0);  //缴费金额（暂定为个人缴费）
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;  //缴费标志
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;  //缴费类型
                    }

                    //失业保险
                    if (UnemployList.Count > 0)
                    {
                        var UnemployString = UnemployList.Where(e => HtmlParser.GetResultFromParser(e, "//td").Count > 0 && HtmlParser.GetResultFromParser(e, "//td")[0].Contains(detailRes.PayTime));
                        if (UnemployString.Count() > 0)
                        {
                            detailRes.UnemployAmount = HtmlParser.GetResultFromParser(UnemployString.ToList()[0], "//td")[3].Replace("\r", "").Replace("\n", "").Replace("\t", "").ToDecimal(0); //失业保险缴费金额
                        }
                    }

                    //医疗保险
                    if (MedicalList.Count > 0)
                    {
                        var MedicalString = MedicalList.Where(e => HtmlParser.GetResultFromParser(e, "//td").Count > 0 && HtmlParser.GetResultFromParser(e, "//td")[0].Contains(detailRes.PayTime));
                        if (MedicalString.Count() > 0)
                        {
                            detailRes.MedicalAmount = HtmlParser.GetResultFromParser(MedicalString.ToList()[0], "//td")[3].Replace("\r", "").Replace("\n", "").Replace("\t", "").ToDecimal(0); //基本医疗保险缴费金额
                        }
                    }

                    //大额医疗保险
                    if (IllnessMedicalList.Count > 0)
                    {
                        var IllnessMedicalString = IllnessMedicalList.Where(e => HtmlParser.GetResultFromParser(e, "//td").Count > 0 && HtmlParser.GetResultFromParser(e, "//td")[0].Contains(detailRes.PayTime));
                        if (IllnessMedicalString.Count() > 0)
                        {
                            detailRes.IllnessMedicalAmount = HtmlParser.GetResultFromParser(IllnessMedicalString.ToList()[0], "//td")[3].Replace("\r", "").Replace("\n", "").Replace("\t", "").ToDecimal(0); //大额医疗保险缴费金额
                        }
                    }
                    Res.Details.Add(detailRes);
                }
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
