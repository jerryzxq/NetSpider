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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.AH
{
    public class hefei : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://220.178.98.86/hfgjj/";
        string fundCity = "ah_hefei";
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

                Url = baseUrl + "code.jsp";
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
                string ErrorMsg = string.Empty;

                #region 第一步，登录

                Url = baseUrl + "jsp/web/public/search/grloginAct.jsp";
                //postdata = String.Format("__VIEWSTATE=%2FwEPDwUIMTMwMjQ1MzAPZBYCAgUPZBYCAgEPDxYCHgRUZXh0BUrmgLvorr%2Fpl67kurrmlbDvvJombmJzcDsmbmJzcDsmbmJzcDsmbmJzcDs8Zm9udCBjb2xvcj0ncmVkJz4yNTM5MjQ2PC9mb250PmRkZJI9a7zmwgLbhTXDE482HsFQ2RGl&__EVENTVALIDATION=%2FwEWBALmv%2BIOAufF57IKApjZhZcGAt2SmY8BQrydkVehn%2BD%2B6%2B%2FCee0HULm318Q%3D&txtIDCard={0}&txtCheckCode={1}&btnOK=%E4%B8%8B%E4%B8%80%E6%AD%A5", fundReq.Username, fundReq.Password, fundReq.Vercode);
                postdata = string.Format("lb=b&hm={0}&mm={1}&yzm={2}&imageField.x=44&imageField.y=11", fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                ErrorMsg = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "');</script>");
                if (!string.IsNullOrEmpty(ErrorMsg))
                {
                    Res.StatusDescription = ErrorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                int multiType = 0;
                if (httpResult.Html.IndexOf("<script>window.location='grSelect.jsp'</script>") != -1)
                {
                    multiType = 1;
                }
                else if (httpResult.Html.IndexOf("<script>window.location='grCenter.jsp") != -1)
                {
                    multiType = 2;
                }
                else
                {
                    string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');");
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                CookieCollection cookies12 = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 第二步，多个单位时,选择缴费记录时间最近的一个单位

                if (multiType == 1)
                {
                    Url = baseUrl + "jsp/web/public/search/grSelect.jsp";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies12,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='massage']/table/tr[position()>2]/td[1]/a", "href");

                    if (results.Count == 0)
                    {
                        Res.StatusDescription = "无公积金信息";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    CookieCollection cookies1 = CommonFun.GetCookieCollection(cookies12, httpResult.CookieCollection);
                    Dictionary<DateTime, string> dicDetails = new Dictionary<DateTime, string>();
                    List<string> detailResults = new List<string>();
                    foreach (string link in results)
                    {
                        Url = baseUrl + "jsp/web/public/search/" + link;
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "get",
                            CookieCollection = cookies1,
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
                        CookieCollection cookiesSelect = CommonFun.GetCookieCollection(cookies1, httpResult.CookieCollection);
                        Url = baseUrl + "jsp/web/public/search/grCenter.jsp";
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "get",
                            CookieCollection = cookiesSelect,
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
                        cookiesSelect = CommonFun.GetCookieCollection(cookiesSelect, httpResult.CookieCollection);

                        detailResults = ProvidentFundDetail(ref Res, cookiesSelect, "//div[@id='L13']/table/tr/td[3]");
                        if (Res.StatusCode == ServiceConsts.StatusCode_httpfail)
                        {
                            return Res;
                        }
                        if (detailResults.Count != 0)
                        {
                            detailResults.Sort();
                            dicDetails.Add(Convert.ToDateTime(detailResults[detailResults.Count - 1]), link);
                        }

                    }
                    var value = dicDetails.OrderByDescending(o => o.Key).ToDictionary(o => o.Key, p => p.Value).FirstOrDefault().Value;
                    Url = baseUrl + "jsp/web/public/search/" + value;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies1,
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
                    cookies = CommonFun.GetCookieCollection(cookies1, httpResult.CookieCollection);
                }
                if (multiType == 1)
                {
                    cookies12 = cookies;
                }
                Url = baseUrl + "jsp/web/public/search/grCenter.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies12,
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
                cookies = CommonFun.GetCookieCollection(cookies12, httpResult.CookieCollection);
                #endregion

                #region 第三步，获取个人基本信息

                Url = baseUrl + "jsp/web/public/search/grCenter.jsp";
                postdata = "url=2&dkzh=";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/center/table/tr[2]/td[3]/table/tr[2]/td/table[1]/tr[1]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Name = CommonFun.GetMidStrByRegex(httpResult.Html, "姓 名</div></td><td width=\"751\">", "<strong>");//姓名
                    Res.Name = results[0];
                }
                else
                {
                    Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/center/table/tr[2]/td[3]/table/tr[2]/td/table[1]/tr[1]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/center/table/tr[2]/td[3]/table/tr[2]/td/table[1]/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0].ToTrim("nbs").Replace("&p;", "");//公积金账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/center/table/tr[2]/td[3]/table/tr[2]/td/table[1]/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0].ToTrim();//身份证号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/center/table/tr[2]/td[3]/table/tr[2]/td/table[1]/tr[4]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.SalaryBase = results[0].ToDecimal(0);//缴费基数/工资
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/center/table/tr[2]/td[3]/table/tr[2]/td/table[1]/tr[5]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].ToDecimal(0);//个人月缴费
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/center/table/tr[2]/td[3]/table/tr[2]/td/table[1]/tr[6]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);//账户余额
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/center/table/tr[2]/td[3]/table/tr[2]/td/table[1]/tr[7]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Bank = results[0];//银行
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/center/table/tr[2]/td[3]/table/tr[2]/td/table[1]/tr[1]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];//单位名称
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/center/table/tr[2]/td[3]/table/tr[2]/td/table[1]/tr[4]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = results[0].ToTrim("%").ToDecimal(0) / 100;//个人缴费比率
                    Res.CompanyMonthPayRate = results[0].ToTrim("%").ToDecimal(0) / 100;//公司缴费比率
                }


                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/center/table/tr[2]/td[3]/table/tr[2]/td/table[1]/tr[5]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = results[0].ToDecimal(0);//公司月缴费
                    Res.PersonalMonthPayAmount = Res.CompanyMonthPayAmount;//单位月缴费
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/center/table/tr[2]/td[3]/table/tr[2]/td/table[1]/tr[6]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.Status = results[0];//状态
                }
                #endregion

                #region 第四步，获取缴费明细

                results = ProvidentFundDetail(ref Res, cookies, "//div[@id='L13']/table/tr");
                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                    if (tdRow.Count != 6)
                    {
                        continue;
                    }
                    detail = new ProvidentFundDetail();
                    detail.PayTime = tdRow[2].ToDateTime();
                    if (tdRow[1].IndexOf("汇缴") != -1)
                    {
                        detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[1], "汇缴", "公积金");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0) / 2;//金额
                        detail.CompanyPayAmount = tdRow[3].ToDecimal(0) / 2;//金额
                        detail.ProvidentFundBase = (detail.PersonalPayAmount / Res.PersonalMonthPayRate);//缴费基数
                        PaymentMonths++;
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.Description = tdRow[1];
                        detail.PersonalPayAmount = 0;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
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
        private List<string> ProvidentFundDetail(ref ProvidentFundQueryRes res, CookieCollection cookies, string selectPath)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            ProvidentFundDetail detail = null;
            #region 获取缴费明细
            //本年
            Url = baseUrl + "jsp/web/public/search/grCenter.jsp?rnd=1440556247625";
            postdata = "url=3&dkzh=";
            httpItem = new HttpItem()
            {
                URL = Url,
                Method = "post",
                Postdata = postdata,
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            if (httpResult.StatusCode != HttpStatusCode.OK)
            {
                res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                res.StatusCode = ServiceConsts.StatusCode_httpfail;
            }
            //results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='L13']/table/tr", "");
            results = HtmlParser.GetResultFromParser(httpResult.Html, selectPath, "");
            cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

            System.Threading.Thread.Sleep(1000);
            //往年
            Url = baseUrl + "jsp/web/public/search/grCenter.jsp?rnd=1440556247625";
            postdata = "url=3_1&dkzh=";
            httpItem = new HttpItem()
            {
                URL = Url,
                Method = "post",
                Postdata = postdata,
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            if (httpResult.StatusCode != HttpStatusCode.OK)
            {
                res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                res.StatusCode = ServiceConsts.StatusCode_httpfail;

            }
            results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, selectPath, ""));
            #endregion
            return results;
        }
    }
}
