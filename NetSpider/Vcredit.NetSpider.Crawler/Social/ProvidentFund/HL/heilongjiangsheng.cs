using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HL
{
    public class heilongjiangsheng : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.hljszgjj.com/";
        string fundCity = "hl_heilongjiangsheng";
        List<string> results = new List<string>();
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
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
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            ProvidentFundDetail detail = null;
            int PaymentMonths = 0;

            string zgxm = string.Empty;
            string zgzh = string.Empty;
            string dwbm = string.Empty;
            decimal providentFundBase = 0;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (string.IsNullOrEmpty(fundReq.Identitycard) || !regex.IsMatch(fundReq.Identitycard))
                {
                    Res.StatusDescription = "身份证号不能为空或身份证号格式不正确";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrEmpty(fundReq.Password))
                {
                    Res.StatusDescription = "登录密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = "http://wscx.hljszgjj.com/zfbzgl/zfbzsq/login_hidden.jsp";
                postdata = String.Format("zgzh1=&cxydmc=%B5%B1%C7%B0%C4%EA%B6%C8&xuanze=0&sfzh={0}&password={1}", fundReq.Identitycard, fundReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var errormsg = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\");");
                if (!errormsg.IsEmpty())
                {
                    Res.StatusDescription = errormsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgxm']", "value");
                if (results.Count > 0)
                {
                    zgxm = results[0];
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='zgzh']", "value");
                if (results.Count > 0)
                {
                    zgzh = results[0];
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='dwbm']", "value");
                if (results.Count > 0)
                {
                    dwbm = results[0];
                }

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                #endregion


                #region 第二步，查询个人基本信息
                Url = "http://wscx.hljszgjj.com/zfbzgl/zfbzsq/main_menu.jsp";
                postdata = string.Format("zgzh={0}&sfzh={1}&zgxm={2}&dwbm={3}&cxyd=%B5%B1%C7%B0%C4%EA%B6%C8&pass=null&dbname=null", zgzh, fundReq.Identitycard, zgxm.ToUrlEncode(), dwbm);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr/td/font", "");
                if (results.Count > 0)
                {
                    Res.Name = results[0].Split(';')[1];  //姓名
                    Res.CompanyName = results[1].Split(';')[1];  //单位名称
                    Res.IdentityCard = results[2].Split(';')[1];  //身份证号
                    Res.EmployeeNo = results[3].Split(';')[1];  //职工账号
                    Res.OpenTime = results[4].Split(';')[1];  //开户日期
                    Res.Status = results[5].Split(';')[1];  //状态
                    providentFundBase = results[6].Split(';')[1].ToDecimal(0);  //缴费基数
                    Res.PersonalMonthPayRate = results[7].Split(';')[1].Split('%')[0].ToDecimal(0);  //个人缴费比例
                    Res.CompanyMonthPayRate = results[9].Split(';')[1].Split('%')[0].ToDecimal(0);  //公司缴费比例
                    Res.PersonalMonthPayAmount = (Res.PersonalMonthPayRate / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)) * results[8].Split(';')[1].ToDecimal(0);  //通过个人缴费比例和公司缴费比例计算缴费额
                    Res.CompanyMonthPayAmount = (Res.CompanyMonthPayRate / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)) * results[8].Split(';')[1].ToDecimal(0);  //通过个人缴费比例和公司缴费比例计算缴费额
                    Res.TotalAmount = results[16].Split(';')[1].ToDecimal(0);  //余额
                }
                #endregion


                #region 第三步，缴费明细查询
                //首先查询总的年度
                Url = "http://wscx.hljszgjj.com/zfbzgl/gjjmxcx/gjjmx_cx.jsp";
                postdata = string.Format("sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd=%B5%B1%C7%B0%C4%EA%B6%C8", fundReq.Identitycard, zgxm.ToUrlEncode(), zgzh, dwbm);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@name='cxydtwo']/option", "");

                var year = "当前年度";
                //循环月度，查询缴费详细
                foreach (var item in results)
                {
                    Url = "http://wscx.hljszgjj.com/zfbzgl/zfbzsq/gjjmx_cxtwo.jsp";
                    if (year.Contains("当前年度"))
                    {
                        year = year.ToUrlEncode(Encoding.GetEncoding("gbk"));
                    }
                    if (item.Contains("当前年度"))
                    {
                        postdata = string.Format("cxydtwo={0}&cxydtwo={5}&cxyd=%B5%B1%C7%B0%C4%EA%B6%C8&zgzh={1}&sfzh={2}&zgxm={3}&dwbm={4}", item.ToUrlEncode(Encoding.GetEncoding("gbk")), zgzh, fundReq.Identitycard, zgxm.ToUrlEncode(), dwbm, year);
                    }
                    else
                    {
                        postdata = string.Format("cxydtwo={0}&cxydtwo={5}&cxyd=%B5%B1%C7%B0%C4%EA%B6%C8&zgzh={1}&sfzh={2}&zgxm={3}&dwbm={4}", item, zgzh, fundReq.Identitycard, zgxm.ToUrlEncode(), dwbm, year);
                    }
                    year = item;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='1']/tr", "");
                    foreach (var item2 in results)
                    {
                        var tdRow=HtmlParser.GetResultFromParser(item2, "//td", "text", true);
                        detail = new ProvidentFundDetail();
                        if (tdRow[0].Contains("日期"))
                        {
                            continue;
                        }
                        detail.PayTime = tdRow[0].ToDateTime();  //缴费日期
                        detail.Description = tdRow[1];  //描述
                        detail.CompanyName = Res.CompanyName;  //单位名称
                         if (detail.Description.Contains("汇"))
                         {
                             detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;  //缴费标志
                             detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;  //缴费类型
                             detail.PersonalPayAmount = tdRow[3].ToDecimal(0) * (Res.PersonalMonthPayRate / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate));  //根据个人缴费，单位缴费比例计算缴费金额
                             detail.CompanyPayAmount = tdRow[3].ToDecimal(0) * (Res.CompanyMonthPayRate / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate));  //根据个人缴费，单位缴费比例计算缴费金额
                             PaymentMonths++;
                         }
                         else if (detail.Description.Contains("支取") || detail.Description.Contains("提取"))
                         {
                             detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                             detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                             detail.PersonalPayAmount = tdRow[2].ToDecimal(0);  //支取 将缴费信息存入个人缴费字段中
                             Res.Description = "有支取，请人工校验。";  //缴费详单存在支取
                         }
                         else
                         {
                             detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                             detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                             detail.PersonalPayAmount = tdRow[3].ToDecimal(0);  //其他缴费方式 将缴费信息存入个人缴费字段中
                         }
                         Res.ProvidentFundDetailList.Add(detail);
                    }
                }

                #endregion


                #region ********所给账号没有贷款信息**********
                Url = "http://wscx.hljszgjj.com/zfbzgl/dkxxcx/dkxx_cx.jsp";
                postdata = string.Format("sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd=%B5%B1%C7%B0%C4%EA%B6%C8",fundReq.Identitycard,zgxm.ToUrlEncode(),zgzh,dwbm);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var msg = HtmlParser.GetResultFromParser(httpResult.Html, "//legend[@id='title']", "");
                if (msg[0].Contains("没有"))
                {
                    ///暂无贷款信息
                }

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
