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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.JS
{
    public class taizhou : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://58.222.185.50:9009/";
        string socialCity = "js_taizhou";
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
                Url = baseUrl + "captcha.svl?d=1458801343023";
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
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = baseUrl + "cas/loginvalidate.html";
                postdata = String.Format("account={0}&password={1}&oldhref=&a=person&type=1&captcha={2}", socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                if (httpResult.Html.Contains("captchawrong"))
                {
                    Res.StatusDescription = "验证码错误";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (httpResult.Html.Contains("wrongpass"))
                {
                    Res.StatusDescription = "用户名或密码错误";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                #endregion

                #region 获取基本信息
                Url = baseUrl + "person/personMessage.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//ul[@style='width:220px']/li/input", "value");
                if (results.Count <= 0 || string.IsNullOrEmpty(results[0]))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.EmployeeNo = results[0];  //个人编号
                Res.Name = results[1];//姓名
                Res.BirthDate = results[2];//出生日期
                Res.Sex = results[3].Trim();//性别
                Res.EmployeeStatus = results[5]; //个人状态
                Res.CompanyName = results[7]; //**通讯地址暂设为公司名称**
                Res.IdentityCard = results[8].Trim();//身份证号
                Res.Race = results[9];//民族
                Res.Phone = results[10];  //联系电话
                Res.Address = results[11].Trim();//通讯地址
                #endregion

                #region 查询明细
                //以养老保险为主,从养老保险缴费初始月份开始统计


                //得到养老保险初始月份
                var startDate = string.Empty;
                Url = baseUrl + "person/monthAccountYLao.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//dl[@style='width: 8%'][1]/dd", "");
                if (results.Count > 0)
                {
                    startDate = results[0];
                }
                do
                {
                    if (!startDate.Contains("/") && !startDate.Contains("-"))
                    {
                        startDate = startDate.Insert(4, "-");
                    }

                    Url = baseUrl + "person/gerenjiaofei_result.html";
                    postdata = string.Format("pageNo=1&inputvalue={0}", DateTime.Parse(startDate).ToString("yyyyMM"));
                    startDate = DateTime.Parse(startDate).AddMonths(1).ToString();
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "Post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='listinfo']/dl[1]/dd", "");
                    if (results.Count <= 0 || results[0].IsEmpty())
                    {
                        continue;
                    }
                    else
                    {
                        var payTimeList = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='listinfo']/dl[1]/dd", "");  //缴费年月list
                        var socialInsuranceTimeList = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='listinfo']/dl[2]/dd", "");   //应属年月list
                        var typeList = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='listinfo']/dl[3]/dd", "");   //险种list
                        var socialInsuranceBaseList = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='listinfo']/dl[5]/dd", "");   //缴费基数list
                        var pensionAmountList = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='listinfo']/dl[6]/dd", "");   //个人缴费list
                        var companyPensionAmountList = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='listinfo']/dl[7]/dd", "");   //公司缴费list
                        var dateflag = "";
                        detailRes = new SocialSecurityDetailQueryRes();
                        for (int i = 0; i < payTimeList.Count; i++)
                        {
                            if (dateflag != socialInsuranceTimeList[i].ToString())
                            {
                                if (!string.IsNullOrEmpty(detailRes.Name))
                                {
                                    Res.Details.Add(detailRes);
                                    PaymentMonths++;
                                }
                                detailRes = new SocialSecurityDetailQueryRes();
                            }
                            dateflag = socialInsuranceTimeList[i];
                            detailRes.PayTime = payTimeList[i];  //缴费日期
                            detailRes.SocialInsuranceTime = socialInsuranceTimeList[i];  //应属日期
                            detailRes.Name = Res.Name;  //姓名
                            detailRes.IdentityCard = Res.IdentityCard;  //身份证号
                            if (typeList[i].Contains("养老保险"))  //养老保险
                            {
                                detailRes.SocialInsuranceBase = socialInsuranceBaseList[i].ToDecimal(0);  //缴费基数
                                detailRes.PensionAmount = pensionAmountList[i].ToDecimal(0);  //个人缴费
                                detailRes.CompanyPensionAmount = companyPensionAmountList[i].ToDecimal(0);  //公司缴费
                                detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                                detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            }
                            else if (typeList[i].Contains("医疗保险"))  //医疗保险
                            {
                                detailRes.MedicalAmount = pensionAmountList[i].ToDecimal(0);  //个人缴费
                                detailRes.CompanyMedicalAmount = companyPensionAmountList[i].ToDecimal(0);  //公司缴费
                            }
                            else if (typeList[i].Contains("工伤保险"))  //工伤保险
                            {
                                detailRes.EmploymentInjuryAmount = pensionAmountList[i].ToDecimal(0) + companyPensionAmountList[i].ToDecimal(0);  //工伤保险
                            }
                            else if (typeList[i].Contains("生育保险"))  //生育保险
                            {
                                detailRes.MaternityAmount = pensionAmountList[i].ToDecimal(0) + companyPensionAmountList[i].ToDecimal(0);  //生育保险
                            }
                            else if (typeList[i].Contains("大病医疗"))  //大病医疗
                            {
                                detailRes.IllnessMedicalAmount = pensionAmountList[i].ToDecimal(0) + companyPensionAmountList[i].ToDecimal(0);  //大病医疗
                            }

                            if (i == payTimeList.Count - 1)  //当为循环最后一个时添加
                            {
                                Res.Details.Add(detailRes);
                                PaymentMonths++;
                            }
                        }
                    }

                } while (DateTime.Parse(startDate) <= DateTime.Now);

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
