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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.SH
{
    public class shanghai : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.12333sh.gov.cn/sbsjb/";
        string socialCity = "sh_shanghai";
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
                Url = baseUrl + "wzb/Bmblist.jsp";
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
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
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
            int oldestyear = 0;//需统计的历史年份，如果历史年份的缴费月数不等于12，则为0；
            //Dictionary<int, int> year_continuemonth = new Dictionary<int, int>();//需统计的年份--年缴费月数
            //int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(socialReq.Token);
                    SpiderCacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录上海社保
                Url =baseUrl+ "wzb/dologin.jsp";
                postdata = String.Format("userid={0}&userpw={1}&userjym={2}", socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
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

                if (!httpResult.Html.Contains("登陆成功"))
                {
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/table/tr/td/span");
                    if (results.Count > 0)
                    {
                        //Res.StatusDescription = results[0];
                        Res.StatusDescription = "登录失败，请检查身份证号和密码";
                    }
                    else
                    {
                        Res.StatusDescription = "无信息";
                    }
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 查询社保信息
                Url = baseUrl + "wzb/qydcx.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = "",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//xml[@id='dataisxxb_sum5']/xxblist/jsjs/jsjs1");
                if (results.Count > 0)
                {
                    Res.SpecialPaymentType = results[0];
                }
                string last2yearrecord = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//xml[@id='dataisxxb_sum5']/xxblist/jsjs/jsjs32");
                if (results.Count > 0)
                {
                    last2yearrecord = results[0];
                    
                    
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//xml[@id='dataisxxb_sum5']/xxblist/jsjs/jsjs11");
                    if (results.Count > 0)
                    {
                        if (results[0].ToInt(0) == 12)
                        {
                            oldestyear = last2yearrecord.Substring(0, 4).ToInt(0);
                        }
                        last2yearrecord += ("缴费月数 : " + results[0].ToInt(0).ToString() + "个月");
                        Res.SpecialPaymentType += last2yearrecord;
                    }
                }

                Url = baseUrl + "wzb/psnl_regxg.jsp"; ;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = "",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//lxdh");
                if (results.Count > 0)
                {
                    Res.Phone = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//lxdz");
                if (results.Count > 0)
                {
                    Res.Address = results[0];
                }

                Url = baseUrl + "wzb/sbsjbcx.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = "",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                #endregion

                #region 解析html，并对解析后的数据进行整理
                //Res.IdentityCard = socialReq.Username;//身份证

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//xml[@id='dataisxxb_sum1']/xxblist/jsjs/xm");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//xml[@id='dataisxxb_sum1']/xxblist/jsjs/zjhm");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证
                }
                else
                {
                    Res.IdentityCard = socialReq.Identitycard;
                }


                results = HtmlParser.GetResultFromParser(httpResult.Html, "//xml[@id='dataisxxb_sum1']/xxblist/jsjs/jsjs1");
                if (results.Count > 0)
                {
                    Res.WorkingAge = results[0];//连续工龄
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//xml[@id='dataisxxb_sum4']/xxblist/jsjs/jsjs1");
                if (results.Count > 0)
                {
                    Res.DeadlineYearAndMonth = GetYearAndMonth(results[0]);//截止年月
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//xml[@id='dataisxxb_sum4']/xxblist/jsjs/jsjs2");
                if (results.Count > 0)
                {
                    Res.PaymentMonths = results[0].ToInt(0);//累计缴费月数
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//xml[@id='dataisxxb_sum4']/xxblist/jsjs/jsjs3");
                if (results.Count > 0)
                {
                    Res.InsuranceTotal = results[0].ToDecimal(0);//养老金本息总额
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//xml[@id='dataisxxb_sum4']/xxblist/jsjs/jsjs4");
                if (results.Count > 0)
                {
                    Res.PersonalInsuranceTotal = results[0].ToDecimal(0);//养老金总额个人部分
                }

                List<string> results1 = new List<string>();
                Dictionary<string, List<decimal>> paytimedic = new Dictionary<string, List<decimal>>();
                

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//xml[@id='dataisxxb_sum2']/xxblist/jsjs");
                foreach (string trItem in results)
                {
                    string paytime = string.Empty;
                    decimal SocialInsuranceBase = 0;
                    decimal PensionAmount = 0;
                    decimal MedicalAmount = 0;
                    decimal UnemployAmount = 0;
                    List<decimal> amountlist = new List<decimal>();
                    results1 = HtmlParser.GetResultFromParser(trItem, "//jsjs1");
                    if (results1.Count > 0)
                    {
                        paytime = results1[0];
                    }
                    results1 = HtmlParser.GetResultFromParser(trItem, "//jsjs3");
                    if (results1.Count > 0)
                    {
                        SocialInsuranceBase = results1[0].ToDecimal(0);
                    }
                    results1 = HtmlParser.GetResultFromParser(trItem, "//jsjs4");
                    if (results1.Count > 0)
                    {
                        PensionAmount = results1[0].ToDecimal(0);
                    }
                    results1 = HtmlParser.GetResultFromParser(trItem, "//jsjs6");
                    if (results1.Count > 0)
                    {
                        MedicalAmount = results1[0].ToDecimal(0);
                    }
                    results1 = HtmlParser.GetResultFromParser(trItem, "//jsjs8");
                    if (results1.Count > 0)
                    {
                        UnemployAmount = results1[0].ToDecimal(0);
                    }
                    amountlist.Add(SocialInsuranceBase);
                    amountlist.Add(PensionAmount);
                    amountlist.Add(MedicalAmount);
                    amountlist.Add(UnemployAmount);
                    paytimedic.Add(paytime, amountlist);
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//xml[@id='dataisxxb_sum3']/xxblist/jsjs");
                bool isContinue = true;
                List<int> shijiao_year = new List<int>();
                List<SocialSecurityDetailQueryRes> tempProvidentFundDetail = new List<SocialSecurityDetailQueryRes>();
                foreach (string trItem in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(trItem, "//td");
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;

                    results1 = HtmlParser.GetResultFromParser(trItem, "//jsjs1");
                    if (results1.Count > 0)
                    {
                        detailRes.PayTime = results1[0];
                        detailRes.SocialInsuranceTime = detailRes.PayTime;
                    }
                    else
                        continue;
                    int paytime_year = detailRes.PayTime.Substring(0, 4).ToInt(0);
                    int paytime_month = detailRes.PayTime.Substring(4).ToInt(0);
                    if (paytime_year != 0 && !shijiao_year.Contains(paytime_year))
                    {
                        shijiao_year.Add(paytime_year);
                    }
                    results1 = HtmlParser.GetResultFromParser(trItem, "//jsjs2");
                    if (results1.Count > 0)
                    {
                        detailRes.YearPaymentMonths = results1[0].ToInt(0);
                        if (detailRes.YearPaymentMonths != paytime_month)
                        {
                            isContinue = false;
                        }
                    }
                    if (paytimedic.ContainsKey(detailRes.PayTime))
                    {
                        List<decimal> amountlist = paytimedic[detailRes.PayTime];
                        detailRes.SocialInsuranceBase = amountlist[0];
                        detailRes.PensionAmount = amountlist[1];
                        detailRes.MedicalAmount = amountlist[2];
                        detailRes.UnemployAmount = amountlist[3];
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;

                        Res.Details.Add(detailRes);
                    }

                    tempProvidentFundDetail.Add(detailRes);

                    if(detailRes.SocialInsuranceBase != 0 && (detailRes.SocialInsuranceTime.ToInt(0) > Res.DeadlineYearAndMonth.ToInt(0)) && Res.DeadlineYearAndMonth.ToInt(0) != 0)
                    {
                        Res.PaymentMonths++;
                    }
                }
                #endregion

                #region 上海统计规则
                detailRes = tempProvidentFundDetail.OrderByDescending(o => o.SocialInsuranceTime).FirstOrDefault();//查询最后一条记录
                bool NeedCalState = false;
                if (detailRes == null)
                {
                    Res.PaymentMonths_Continuous = 0;
                }
                else
                {
                    if (detailRes.SocialInsuranceBase == 0)//如果最后一条记录没有
                    {
                        Res.PaymentMonths_Continuous = -1;//则标记连续缴费为-1，统计时算为0
                        Res.SocialInsuranceBase = -1;//则标记缴费基数为-1，统计时算为0
                    }
                    if (isContinue)//如果实际缴费中每个月都为连续的
                    {
                        NeedCalState = true;
                        if (shijiao_year.Count == 2)//如果实际缴费中有两年的记录
                        {
                            if (shijiao_year.Contains(oldestyear))//如果这两年中包含了历史年份
                            {
                                Res.PaymentMonths_Continuous = detailRes.YearPaymentMonths + 12;//则为最新一年的年缴费月数+12
                            }
                            else if (oldestyear != 0)//如果这两年中不包含历史年份，且历史年份不为0（即年缴费月数为12）
                            {
                                Res.PaymentMonths_Continuous = detailRes.YearPaymentMonths + 24;//则为最新一年的年缴费月数+24
                            }
                            else//如果这两年中不包含历史年份，且历史年份为0（即年缴费月数为0）
                            {
                                Res.PaymentMonths_Continuous = detailRes.YearPaymentMonths + 12;//则为最新一年的年缴费月数+12
                            }
                        }
                        else//如果实际缴费中有一年的记录
                        {
                            if (oldestyear != 0)//如果历史年份不为0（即年缴费月数为12）
                            {
                                Res.PaymentMonths_Continuous = detailRes.YearPaymentMonths + 12;//则为最新一年的年缴费月数+12
                            }
                            else//如果历史年份为0（即年缴费月数为0）
                            {
                                Res.PaymentMonths_Continuous = detailRes.YearPaymentMonths;//则为最新一年的年缴费月数
                            }
                        }
                    }
                    else//如果实际缴费中有断保的
                    {
                        Res.PaymentMonths_Continuous = detailRes.YearPaymentMonths;//则为最新一年的年缴费月数
                        Res.Description = "有断缴、补缴，请人工校验";
                    }
                }
                if (Res.PaymentMonths_Continuous > 25)
                {
                    Res.PaymentMonths_Continuous = 25;
                }
                string Payment_State = string.Empty;
                if (NeedCalState)
                {
                    for (int i = 0; i <= 23; i++)
                    {
                        string SocialInsuranceTime = DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString7);

                        SocialSecurityDetailQueryRes query = new SocialSecurityDetailQueryRes();
                        query = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime && o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal && (o.PensionAmount != 0 || o.CompanyPensionAmount != 0)).FirstOrDefault();
                            
                        if (query != null)
                        {
                            break;//缴费
                        }
                        else
                        {
                            Payment_State += "/ ";//未缴费
                        }
                    }
                    for (int i = Payment_State.ToTrim().Length; i < 24; i++)
                    {
                        Payment_State += "N ";
                    }
                }
                #endregion

                Res.Payment_State = Payment_State;
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

        #region 私有方法
        private string GetYearAndMonth(string inputStr)
        {
            string year = inputStr.Substring(0, 4);
            string month = inputStr.Replace("年", "").Replace("月", "");
            month = month.Substring(4, month.Length - 4);
            if (month.Length == 1)
            {
                month = "0" + month;
            }
            return year + month;
        }
        #endregion
    }

}
