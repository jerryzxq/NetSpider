using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.GD
{
    public class zhongshan : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://wsbs.zszfgjj.gov.cn/nbp/";
        string fundCity = "gd_zhongshan";
        int PaymentMonths = 0;
        private decimal payRate = 0.08M;
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            string Url = string.Empty;
            try
            {
                Url = baseUrl + "index.jsp?flg=1";
                httpItem = new HttpItem()
                {
                    URL = Url,
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

                Url = baseUrl + "vericode.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
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
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                //添加缓存
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
            ProvidentFundDetail detailRes = null;
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数 
                if (fundReq.Username.IsEmpty() || fundReq.Password.IsEmpty() || fundReq.Vercode.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 登录

                Url = baseUrl + "per.login";
                //1:联名卡号 2:公积金账号登陆
                postdata = fundReq.LoginType == "1" ? string.Format("rad=on&cardno={0}&hi_value=1&perpwd={1}&vericode={2}", fundReq.Username, fundReq.Password, fundReq.Vercode) : string.Format("rad=on&accnum={0}&hi_value=0&perpwd={1}&vericode={2}", fundReq.Username, fundReq.Password, fundReq.Vercode);
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
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "JavaScript'>alert('", "')</script");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='WTLoginError']/ul/li[1]", "text");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion
                #region 获取基本信息

                Url = baseUrl + "init.summer?_PROCID=60020009";
                httpItem = new HttpItem()
                {
                    URL = Url,
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
                Res.IdentityCard = fundReq.Identitycard;
                Res.Name = fundReq.Name;
                Res.ProvidentFundNo = fundReq.Username;
                Res.Phone = fundReq.Mobile;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='accnum']", "value");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='unitaccnum']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='unitaccname']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='accname']", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='certinum']", "value");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='handset']", "value");
                if (results.Count > 0)
                {
                    Res.Phone = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='bankname']", "value");
                if (results.Count > 0)
                {
                    Res.Bank = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@id='indiaccstate']", "");
                if (results.Count > 0)
                {
                    Res.Status = CommonFun.GetMidStr(results[0], "selected=\"selected\">", "</option>");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='opnaccdate']", "value");
                if (results.Count > 0)
                {
                    Res.OpenTime = results[0].Replace("1899-12-31", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='lpaym']", "value");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='bal']", "value");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='indiprop']", "value");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = results[0].ToDecimal(0) * 0.01M;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='unitprop']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = results[0].ToDecimal(0) * 0.01M;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='unitpayamt']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='indipayamt']", "value");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='cardno']", "value");
                if (results.Count > 0)
                {
                    Res.BankCardNo = results[0];
                }
                Res.PersonalMonthPayRate = Res.PersonalMonthPayRate > 0 ? Res.PersonalMonthPayRate : payRate;
                Res.CompanyMonthPayRate = Res.CompanyMonthPayRate > 0 ? Res.CompanyMonthPayRate : payRate;
                #endregion
                #region 查询明细

                Url = baseUrl + "init.summer?_PROCID=60020007";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string DATAlISTGHOST = HtmlParser.GetResultFromParser(httpResult.Html, "//textarea[@name='DATAlISTGHOST']", "")[0].Replace("\n", "");
                string _DATAPOOL_ = HtmlParser.GetResultFromParser(httpResult.Html, "//textarea[@name='_DATAPOOL_']", "")[0].Replace("\n", "");
                string _LOGIP = CommonFun.GetMidStr(httpResult.Html, "'_LOGIP': '", "',");
                string _IS = CommonFun.GetMidStr(httpResult.Html, "'_IS': '", "',");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                DateTime dt = DateTime.Now;
                string begdate = dt.AddYears(-10).ToString("yyyy-01-01");//查询10年纪录
                string enddate = dt.ToString(Consts.DateFormatString2);
                Url = baseUrl + "command.summer?uuid=1457946653679";
                postdata = string.Format("%24page=%2Fydpx%2F60020007%2Fdppage4003.ydpx&_ACCNUM={0}&_OPERID={0}&_PAGEID=step1&_IS={6}&_LOGIP={1}&_ACCNAME={2}&isSamePer=false&_PROCID=60020007&_SENDOPERID={0}&_DEPUTYIDCARDNUM={3}&_TRACEDATE={4}&_SENDTIME={4}+17%3A08%3A20&_SENDDATE={4}&_BRANCHKIND=0&CURRENT_SYSTEM_DATE={4}&_TYPE=init&_ISCROP=0&_PORCNAME=%E4%B8%AA%E4%BA%BA%E6%98%8E%E7%BB%86%E8%B4%A6%E6%9F%A5%E8%AF%A2&_WITHKEY=0&accnum={0}&accname={2}&begdate={5}&enddate={4}&summarycode=1", Res.ProvidentFundNo, _LOGIP, Res.Name.ToUrlEncode(), Res.IdentityCard, enddate, begdate, _IS);
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Referer = baseUrl + "init.summer?_PROCID=60020007",
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header["X-Requested-With"] = "XMLHttpRequest";
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "dynamictable?uuid=1457946657221";
                int currentPage = 0;
                int totalPage = 0;
                do
                {
                    postdata = string.Format("dynamicTable_id=datalist&dynamicTable_currentPage={6}&dynamicTable_pageSize=100&dynamicTable_nextPage={7}&dynamicTable_page=%2Fydpx%2F60020007%2Fdppage4003.ydpx&dynamicTable_paging=true&dynamicTable_configSqlCheck=0&errorFilter=1%3D1&accnum={2}&accname={3}&begdate={4}&enddate={5}&summarycode=1&_APPLY=0&_CHANNEL=1&_PROCID=60020007&DATAlISTGHOST={0}&_DATAPOOL_={1}", DATAlISTGHOST.ToUrlEncode(), _DATAPOOL_.ToUrlEncode(), Res.ProvidentFundNo, Res.Name.ToUrlEncode(), begdate, enddate, currentPage, (currentPage + 1));
                    httpItem = new HttpItem()
                    {
                        Accept = "application/json, text/javascript, */*; q=0.01",
                        URL = Url,
                        Referer = baseUrl + "init.summer?_PROCID=60020007",
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpItem.Header["X-Requested-With"] = "XMLHttpRequest";
                    httpResult = httpHelper.GetHtml(httpItem);

                    totalPage = jsonParser.GetResultFromMultiNode(httpResult.Html, "data:pageCount").ToInt(0);
                    if (totalPage == 0) break; 
                     
                    List<zhongshanDetail> details = jsonParser.DeserializeObject<List<zhongshanDetail>>(System.Text.RegularExpressions.Regex.Unescape(jsonParser.GetResultFromMultiNode(httpResult.Html, "data:data")));
                    foreach (zhongshanDetail item in details)
                    {
                        detailRes = new ProvidentFundDetail();
                        detailRes.Description = item.accname2;
                        detailRes.PayTime = item.transdate.ToDateTime(Consts.DateFormatString2);
                        detailRes.ProvidentFundTime = Convert.ToDateTime(item.transdate).ToString(Consts.DateFormatString7);
                        detailRes.CompanyName = item.unitaccname;
                        if (@item.accname2 == "汇缴")
                        {
                            detailRes.ProvidentFundBase = item.amt1.ToDecimal(0) / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate);
                            detailRes.PersonalPayAmount = detailRes.ProvidentFundBase * Res.PersonalMonthPayRate;
                            detailRes.CompanyPayAmount = detailRes.ProvidentFundBase * Res.CompanyMonthPayRate;
                            detailRes.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detailRes.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        }
                        else if (@item.accname2.IndexOf("提取", StringComparison.Ordinal) > -1)
                        {
                            detailRes.PersonalPayAmount = item.amt1.ToDecimal(0);
                            detailRes.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                            detailRes.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                        }
                        else
                        {
                            detailRes.PersonalPayAmount = item.amt1.ToDecimal(0);
                            detailRes.PaymentFlag = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detailRes.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        }
                        Res.ProvidentFundDetailList.Add(detailRes);
                    }
                    currentPage++;
                } while (currentPage < totalPage);



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

        #region 明细实体

        internal class zhongshanDetail
        {
            /// <summary>
            /// 姓名
            /// </summary>
            public string accname1 { get; set; }
            /// <summary>
            /// 摘要
            /// </summary>
            public string accname2 { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string agentop { get; set; }
            /// <summary>
            /// 单位账号
            /// </summary>
            public string unitaccnum1 { get; set; }
            /// <summary>
            /// 单位名称
            /// </summary>
            public string unitaccname { get; set; }
            /// <summary>
            /// 发生额(月缴存额)
            /// </summary>
            public string amt1 { get; set; }
            /// <summary>
            /// 余额
            /// </summary>
            public string amt2 { get; set; }
            /// <summary>
            /// 个人账号
            /// </summary>
            public string accnum1 { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string dpbusitype { get; set; }
            /// <summary>
            /// 经办机构
            /// </summary>
            public string addr { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string instance { get; set; }
            /// <summary>
            /// 交易日期
            /// </summary>
            public string transdate { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string freeuse4 { get; set; }
        }
        #endregion
    }
}
