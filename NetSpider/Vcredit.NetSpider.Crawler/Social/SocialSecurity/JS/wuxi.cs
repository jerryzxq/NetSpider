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
namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.JS
{
    public class wuxi : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "";
        string socialCity = "js_wuxi";
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
                Url = "http://ggfw.wxhrss.gov.cn/";
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

                Url = "http://ggfw.wxhrss.gov.cn/captcha.svl?d=" + CommonFun.GetTimeStamp();
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    //Referer = "http://ggfw.wxhrss.gov.cn/",
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
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                SpiderCacheHelper.SetCache(token, cookies);
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
            decimal rate = (decimal)0.08;
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(socialReq.Token);
                    SpiderCacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                Url = "http://ggfw.wxhrss.gov.cn/personloginvalidate.html";
                postdata = String.Format("account={0}&password={1}&type=1&captcha={2}", socialReq.Username, socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
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

                string message = CommonFun.GetMidStr(httpResult.Html, "['", "']");
                if (message != "success")
                {
                    if (message == "wrongaccount")
                    {
                        Res.StatusDescription = "用户名不存在";
                    }
                    if (message == "wrongpass")
                    {
                        Res.StatusDescription = "密码和用户名不匹配";
                    }
                    if (message == "captchawrong")
                    {
                        Res.StatusDescription = "验证码错误";
                    }
                    if (message == "captchaexpire")
                    {
                        Res.StatusDescription = "验证码过期";
                    }
                    if (message == "isLocked")
                    {
                        Res.StatusDescription = "该账号已被冻结";
                    }
                    if (message == "noPersonId")
                    {
                        Res.StatusDescription = "该账号已经终止参保，不允许登录";
                    }
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，个人基本信息

                #region 个人信息
                Url = "http://ggfw.wxhrss.gov.cn/person/personBaseInfo.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    //Host = "ggfw.wxhrss.gov.cn",
                    //Referer = "http://ggfw.wxhrss.gov.cn/personwork.html",
                    //Referer = "http://ggfw.wxhrss.gov.cn/",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[2]/li[1]", "text", true);
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];//编号
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[2]/li[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[2]/li[3]", "text", true);
                if (results.Count > 0)
                {
                    Res.BirthDate = results[0];//出生日期
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[2]/li[4]", "text", true);
                if (results.Count > 0)
                {
                    Res.Sex = results[0];//性别
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[2]/li[6]", "text", true);
                if (results.Count > 0)
                {
                    Res.EmployeeStatus = results[0];//状态
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[2]/li[9]", "text", true);
                if (results.Count > 0)
                {
                    Res.WorkDate = results[0];//工作日期
                }



                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[4]/li[1]", "text", true);
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[4]/li[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.Race = results[0];//民族
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[4]/li[19]", "text", true);
                if (results.Count > 0)
                {
                    Res.Phone = results[0];//手机号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[6]/li[1]", "text", true);
                if (results.Count > 0)
                {
                    Res.Address = results[0];//居住地
                }
                #endregion
                #endregion

                #region 参保状态
                Url = "http://ggfw.wxhrss.gov.cn/person/personCBInfo.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    //Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='listinfo']/dl");
                List<string> InsuranceType = new List<string>();
                List<string> Status = new List<string>();
                List<string> Company = new List<string>();
                foreach (var item in results)
                {
                    string item_type = HtmlParser.GetResultFromParser(item, "/dt")[0];
                    if (item_type == "险种类型")
                    {
                        InsuranceType = HtmlParser.GetResultFromParser(item, "/dd");
                    }
                    if (item_type == "参保状态")
                    {
                        Status = HtmlParser.GetResultFromParser(item, "/dd");
                    }
                    if (item_type == "单位名称")
                    {
                        Company = HtmlParser.GetResultFromParser(item, "/dd");
                    }
                }
                Dictionary<int, string> companynamedic = new Dictionary<int, string>();
                for (int i = 0; i < Status.Count; i++)
                {
                    if (Status[i].Contains("正常参保"))
                    {
                        if (Res.SpecialPaymentType.IsEmpty())
                        {
                            Res.SpecialPaymentType = "正常参保（" + CommonFun.ClearFlag(InsuranceType[i]);
                        }
                        else
                        {
                            Res.SpecialPaymentType += ";" + CommonFun.ClearFlag(InsuranceType[i]);
                        }

                        switch (CommonFun.ClearFlag(InsuranceType[i]))
                        {
                            case "企业基本养老保险":
                                if (!companynamedic.ContainsKey(1))
                                    companynamedic.Add(1, Company[i]);
                                break;
                            case "基本医疗保险":
                                if (!companynamedic.ContainsKey(2))
                                    companynamedic.Add(2, Company[i]);
                                break;
                            case "失业保险":
                                if (!companynamedic.ContainsKey(3))
                                    companynamedic.Add(3, Company[i]);
                                break;
                            case "工伤保险":
                                if (!companynamedic.ContainsKey(4))
                                    companynamedic.Add(4, Company[i]);
                                break;
                            case "生育保险":
                                if (!companynamedic.ContainsKey(5))
                                    companynamedic.Add(5, Company[i]);
                                break;
                        }
                    }
                }
                for (int i = 1; i < 6; i++)
                {
                    if (companynamedic.ContainsKey(i))
                    {
                        Res.CompanyName = companynamedic[i];
                        break;
                    }
                }
                if (Res.SpecialPaymentType.IsEmpty())
                    Res.SpecialPaymentType = "暂停参保";
                else
                    Res.SpecialPaymentType += "）";
                #endregion

                #region 第三步，查询明细
                int pagecount = 1;
                int pageno = 1;
                //string refer = string.Empty;
                List<List<List<string>>> AllDetails = new List<List<List<string>>>();
                do
                {
                    if (pageno == 1)
                    {
                        Url = "http://ggfw.wxhrss.gov.cn/person/monthFeeInfo.html";
                    }
                    else
                    {
                        Url = "http://ggfw.wxhrss.gov.cn/person/monthFeeInfo.html?pagerMethod=&pageNo=" + pageno + "&currentPage=";
                    }

                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        //Referer = refer,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    //refer = Url;
                    if (pagecount == 1)
                    {
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='page_num']/ul/li");
                        if (results.Count == 3)
                        {
                            pagecount = CommonFun.GetMidStr(results[1], "总", "页").ToInt(1);
                        }
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='listinfo']/dl");
                    if (results.Count == 11)
                    {
                        List<List<string>> Details = new List<List<string>>();
                        for (int i = 0; i < 11; i++)
                        {
                            Details.Add(HtmlParser.GetResultFromParser(results[i], "//dd"));
                        }
                        AllDetails.Add(Details);
                    }
                    pageno++;
                    //System.Threading.Thread.Sleep(1000);
                    //httpItem = new HttpItem()
                    //{
                    //    URL = "http://ggfw.wxhrss.gov.cn/checkolduser.html?aa=" + CommonFun.GetTimeStamp(),
                    //    Method = "post",
                    //    CookieCollection = cookies,
                    //    ResultCookieType = ResultCookieType.CookieCollection
                    //};
                    //httpResult = httpHelper.GetHtml(httpItem);
                    //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    //System.Threading.Thread.Sleep(1000);
                    //httpItem = new HttpItem()
                    //{
                    //    URL = "http://ggfw.wxhrss.gov.cn/checklogin.html",
                    //    Method = "post",
                    //    CookieCollection = cookies,
                    //    ResultCookieType = ResultCookieType.CookieCollection
                    //};
                    //  httpResult = httpHelper.GetHtml(httpItem);
                    //  cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    //  System.Threading.Thread.Sleep(1000);

                    //  httpItem = new HttpItem()
                    //  {
                    //      URL = "http://ggfw.wxhrss.gov.cn/personshortcut.html",
                    //      Method = "post",
                    //      CookieCollection = cookies,
                    //      ResultCookieType = ResultCookieType.CookieCollection
                    //  };
                    //  httpResult = httpHelper.GetHtml(httpItem);
                    //  cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                    //  System.Threading.Thread.Sleep(1000);
                }
                while (pageno <= pagecount);

                foreach (List<List<string>> Details in AllDetails)
                {
                    for (int i = 0; i < Details[0].Count; i++)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;
                        detailRes.CompanyName = Details[2][i];
                        detailRes.SocialInsuranceTime = Details[3][i];
                        detailRes.PayTime = detailRes.SocialInsuranceTime;
                        detailRes.SocialInsuranceBase = Details[4][i].ToDecimal(0);
                        detailRes.PensionAmount = Details[5][i].ToDecimal(0);
                        detailRes.MedicalAmount = Details[6][i].ToDecimal(0);
                        detailRes.UnemployAmount = Details[7][i].ToDecimal(0);
                        detailRes.EmploymentInjuryAmount = Details[8][i].ToDecimal(0);
                        detailRes.MaternityAmount = Details[9][i].ToDecimal(0);
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        Res.Details.Add(detailRes);
                    }
                }
                #endregion

                //Res.PaymentMonths = PaymentMonths;
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
