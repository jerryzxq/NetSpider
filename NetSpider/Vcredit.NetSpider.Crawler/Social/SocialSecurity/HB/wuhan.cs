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
    public class wuhan : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "https://221.232.64.242:7022/grws/";
        string socialCity = "hb_wuhan";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "login.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    //SecurityProtocolType = SecurityProtocolType.Ssl3,
                    CerPath = "\\wh12333",
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
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                Url = baseUrl + "dologin";
                postdata = String.Format("j_username={0}&j_password={1}", socialReq.Username, socialReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    //  Allowautoredirect=false,
                    Referer = baseUrl + "login.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var errormsg = HtmlParser.GetResultFromParser(httpResult.Html, "//p[@style='font-size:12px']", "");
                if (errormsg.Count > 0)
                {
                    Res.StatusDescription = errormsg[0].Split('>')[3].Replace("\r", "").Replace("\t", "").Replace("\n", "").Replace(" ", "");
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
                Url = baseUrl + "action/MainAction?menuid=grwssb_grzlcx_grcbzlcx&ActionType=grwssb_grzlcx_grcbzlcx_q";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "action/MainAction?ActionType=Left",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);


                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='GRBH']", "value");
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];//编号
                }
                else
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='XM']", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='GMSFHM']", "value");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='XB']", "value");
                if (results.Count > 0)
                {
                    Res.Sex = results[0];//性别
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='CSRQ']", "value");
                if (results.Count > 0)
                {
                    Res.BirthDate = results[0];//出生日期
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='CJGZRQ']", "value");
                if (results.Count > 0)
                {
                    Res.WorkDate = results[0];//参加工作时间
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='DWMC']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];//单位名称
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='DWBH']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];//单位编号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='MZ']", "value");
                if (results.Count > 0)
                {
                    Res.Race = results[0];//民族
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='GRZT']", "value");
                if (results.Count > 0)
                {
                    Res.EmployeeStatus = results[0];//人员状态
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='YDRYBZ']", "value");
                if (results.Count > 0)
                {
                    Res.IsLocal = results[0] == "是" ? true : false;//是否本地
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='JZDDZ']", "value");
                if (results.Count > 0)
                {
                    Res.Address = results[0];//通讯地址
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='JZDYZBM']", "value");
                if (results.Count > 0)
                {
                    Res.ZipCode = results[0];//邮政编码
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='CBRLXSJ']", "value");
                if (results.Count > 0)
                {
                    Res.Phone = results[0];//联系电话
                }
                #endregion


                #region 第三步，查询保险账户基本信息(如果存在养老保险，则以养老保险为主，否则为医疗保险)
                Url = baseUrl + "/action/MainAction?menuid=grwssb_grzlcx_ylgrzhcx&ActionType=grwssb_grzlcx_ylgrzhcx_q";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var ylbxBaseRate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='JFJS']", "value")[0];
                if (ylbxBaseRate.ToDecimal(0) > 0)
                {
                    Res.SocialInsuranceBase = ylbxBaseRate.ToDecimal(0);  //养老保险缴费基数
                }
                else
                {
                    Url = baseUrl + "action/MainAction?menuid=grwssb_grzlcx_ylbxgrzhqkcx&ActionType=grwssb_grzlcx_ylbxgrzhqkcx_q";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='GRWS_GRCX_YLBXGRZHQK_JBXX']/tbody/tr[3]/td[4]/span", "");
                    if (results.Count > 0)
                    {
                        Res.InsuranceTotal = results[0].ToDecimal(0);  //医疗保险账户余额
                    }
                }
                #endregion

                #region 第三步，查询明细
                int rowCount = 0;
                Url = baseUrl + "action/MainAction?menuid=grwssb_grzlcx_grjfxxcx&ActionType=grwssb_grzlcx_grjfxxcx_q";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='grwssb_grsbyw_grjfxxcx_l_rowCnt']", "value");
                rowCount = results.Count > 0 ? results[0].ToInt(0) : rowCount;
                if (rowCount > 1)
                {
                    //养老保险
                    Url = baseUrl + "jsp/common/tabledata.jsp";
                    postdata = string.Format("id=grwssb_grsbyw_grjfxxcx_l&grwssb_grsbyw_grjfxxcx_l_page=0&ActionType=grwssb_grzlcx_grjfxxcx_q&filterOnNoDataRight=false&subTotalName=%25E5%25B0%258F%25E8%25AE%25A1&display=block&pageSize={0}&hasPage=true&hasTitle=true&whereCls=%2520G.grsxh%2520%253D%2520258842%2520and%2520DWJFBZ%2520in%2520('1'%252C'2')%2520and%2520XZLX%2520in%2520('10'%252C'20'%252C'30'%252C'40'%252C'50'%252C'53'%252C'54')%2520order%2520by%2520JFNY%2520desc%252Cxzlx&type=q&title=%25E4%25B8%25AA%25E4%25BA%25BA%25E7%25BC%25B4%25E8%25B4%25B9%25E4%25BF%25A1%25E6%2581%25AF%25E5%2588%2597%25E8%25A1%25A8&isQuery=true&isPageOper=true&rowCnt={1}&useAjaxPostPars=true", rowCount,rowCount);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/div/tbody/tr", "inner", true);

                    var dateFlag = "";
                    detailRes = new SocialSecurityDetailQueryRes();
                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 15)
                        {
                            continue;
                        }

                        if (dateFlag != tdRow[1].ToString())
                        {
                            if (!string.IsNullOrEmpty(detailRes.CompanyName))
                            {
                                Res.Details.Add(detailRes);
                                PaymentMonths++;
                            }
                            detailRes = new SocialSecurityDetailQueryRes();
                        }
                        dateFlag = tdRow[2];
                        detailRes.PayTime = tdRow[2];
                        if (tdRow[0].Contains("养老"))  //如果不存在养老保险则以医疗保险为主
                        {
                            detailRes.SocialInsuranceTime = tdRow[1];
                            detailRes.SocialInsuranceBase = tdRow[5].ToDecimal(0);
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = tdRow[12] == "已缴" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                            detailRes.PensionAmount = tdRow[7].ToDecimal(0);
                            detailRes.CompanyPensionAmount = tdRow[7].ToDecimal(0);
                            detailRes.EnterAccountMedicalAmount = tdRow[6].ToDecimal(0);
                            detailRes.CompanyName = tdRow[14];

                        }
                        else if (tdRow[0].Contains("基本医疗"))
                        {
                            detailRes.SocialInsuranceTime = tdRow[1];
                            detailRes.SocialInsuranceBase = tdRow[5].ToDecimal(0);
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = tdRow[12] == "已缴" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                            detailRes.MedicalAmount = tdRow[7].ToDecimal(0);
                            detailRes.CompanyMedicalAmount = tdRow[7].ToDecimal(0);
                            detailRes.EnterAccountMedicalAmount = tdRow[6].ToDecimal(0);
                            detailRes.CompanyName = tdRow[14];
                        }
                        else if (tdRow[0].Contains("失业"))
                        {
                            detailRes.UnemployAmount = tdRow[6].ToDecimal(0);
                        }
                        else if (tdRow[0].Contains("工伤"))
                        {
                            detailRes.EmploymentInjuryAmount = tdRow[6].ToDecimal(0);
                        }
                        else if (tdRow[0].Contains("生育"))
                        {
                            detailRes.MaternityAmount = tdRow[6].ToDecimal(0);
                        }
                        else if (tdRow[0].Contains("大额医疗"))
                        {
                            detailRes.IllnessMedicalAmount = tdRow[6].ToDecimal(0);
                        }

                        if (item.Equals(results[results.Count - 1]))  //当为循环最后一个时添加
                        {
                            Res.Details.Add(detailRes);
                            PaymentMonths++;
                        }
                    }

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
