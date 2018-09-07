using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Crawler.Social;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.LN
{
    public class anshan : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string _url = string.Empty;
        string _postData = string.Empty;
        List<string> _results = new List<string>();
        private int PaymentMonths = 0;
        #endregion
        #region 私有变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        string viewState = string.Empty;
        string eventValidation = string.Empty;
        string baseUrl = " http://www.asgjj.com.cn/";
        string fundCity = "ln_anshan";
        private Regex regSpace = new Regex(@"[/\&nbsp;\s;\t]*");//去除空格
        private Regex reg = new Regex(@"[^0-9.1-9.]*");//去除非数字
        private decimal payRate = (decimal)0.08;
        decimal perAccounting = 0;//个人占比
        decimal comAccounting = 0;//公司占比
        decimal totalRate = 0;//总缴费比率
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                vcRes.StatusDescription = "所选城市无需初始化";
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                vcRes.StatusCode = ServiceConsts.StatusCode_error;
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return vcRes;
        }

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            Res.ProvidentFundCity = fundCity;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (string.IsNullOrEmpty(fundReq.Username) || string.IsNullOrEmpty(fundReq.Password))
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆
                _url = baseUrl + "index/index.aspx";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "GET",
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value");
                if (_results.Count > 0)
                {
                    viewState = _results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value");
                if (_results.Count > 0)
                {
                    eventValidation = _results[0];
                }
                _url = baseUrl + "index/index.aspx";
                _postData = string.Format("__VIEWSTATE={0}&__EVENTVALIDATION={1}&TextBox1=&username={2}&userpwd={3}&ImageButton1.x=15&ImageButton1.y=49&DropDownList1=&DropDownList2=", viewState.ToUrlEncode(), eventValidation.ToUrlEncode(), fundReq.Username.ToUrlEncode(), fundReq.Password.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Postdata = _postData,
                    Method = "Post",
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "<script language=javascript>alert('", "')</script>").Trim();
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #region 第二步,获取基本信息
                _url = baseUrl + "fwdd/grxx.aspx";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Referer = baseUrl + "index/index.aspx",
                    Method = "GET",
                    Encoding = Encoding.GetEncoding("utf-8"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='dwzh']", "text");
                if (_results.Count > 0)
                {
                    Res.CompanyNo = _results[0];//单位账号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='grzh']", "text");
                if (_results.Count > 0)
                {
                    Res.ProvidentFundNo = _results[0];//个人账号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='dwmc']", "text");
                if (_results.Count > 0)
                {
                    Res.CompanyName = _results[0];//所属单位
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='xm']", "text");
                if (_results.Count > 0)
                {
                    Res.Name = _results[0];//姓名
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='sfhm']", "text");
                if (_results.Count > 0)
                {
                    Res.IdentityCard = _results[0];//身份号码
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='zt']", "text");
                if (_results.Count > 0)
                {
                    Res.Status = _results[0];//当前帐户状况
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='ye']", "text");
                if (_results.Count > 0)
                {
                    Res.TotalAmount = _results[0].ToDecimal(0);//帐户余额
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='grnnjy']", "text");
                if (_results.Count > 0)
                {
                    Res.LastProvidentFundTime = _results[0];//缴至年月
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='dwjl']", "text");
                if (_results.Count > 0)
                {
                    Res.CompanyMonthPayRate = _results[0].ToDecimal(0) * 0.01M;//单位月缴存比例
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='grjl']", "text");
                if (_results.Count > 0)
                {
                    Res.PersonalMonthPayRate = _results[0].ToDecimal(0) * 0.01M;//个人月缴存比例
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='gzze']", "text");
                if (_results.Count > 0)
                {
                    Res.SalaryBase = _results[0].ToDecimal(0);//月缴存基数
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='dwypje']", "text");
                if (_results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = _results[0].ToDecimal(0);//单位月缴存额
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='gryjje']", "text");
                if (_results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = _results[0].ToDecimal(0);//个人月缴存额
                }


                _url = "http://www.asshbx.gov.cn/asweb/cxlog2.jsp";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "Post",
                    Postdata = "yincang=%D2%BD%C1%C6&subid=&PASSWORD=" + fundReq.Password + "&Submit=%D2%BD%C1%C6",
                    Encoding = Encoding.GetEncoding("utf-8"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #endregion

                #region 第三步,公积金缴费明细
                if (Res.PersonalMonthPayRate > 0 && Res.CompanyMonthPayRate > 0)
                {
                    totalRate = Res.PersonalMonthPayRate + Res.CompanyMonthPayRate;
                    perAccounting = (Res.PersonalMonthPayRate / totalRate);
                    comAccounting = (Res.CompanyMonthPayRate / totalRate);
                }
                else
                {
                    totalRate = (payRate) * 2;//0.16
                    perAccounting = comAccounting = (decimal)0.50;
                }
                //今年
                _url = baseUrl + "fwdd/grls.aspx";
                Res = GetHttpResult(_url);
                //
                //上年
                _url = baseUrl + "fwdd/sngrls.aspx";
                Res = GetHttpResult(_url);
                //
                #endregion

                #region 第四步，获取贷款基本信息
                _url = baseUrl + "fwdd/zfbtxx.aspx";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "GET",
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                var contactNo = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@span='hth']", "");
                if (contactNo.Count > 0)
                {
                    //存在贷款基本信息
                }
                #endregion
                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return Res;
        }
        /// <summary>
        /// 获取明细页面
        /// </summary>
        /// <param name="url">明细请求url(今年,去年)</param>
        /// <returns>Res</returns>
        private NetSpider.Entity.Service.ProvidentFundQueryRes GetHttpResult(string url)
        {
            int pages = 0;//总页码
            httpItem = new HttpItem()
            {
                URL = _url,
                Method = "get",
                Encoding = Encoding.UTF8,
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            Res = DealWithHttpResults(httpResult);
            _results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='lblCurrentPage']", "text");
            if (_results.Count > 0)
            {
                pages = _results[0].Substring(_results[0].IndexOf("/") + 1, 1).ToInt(0);
            }
            else
            {
                pages = 0;
            }
            if (pages > 1)
            {
                for (int i = 2; i <= pages; i++)
                {
                    _url = url + "?Page=" + i;
                    httpItem = new HttpItem()
                    {
                        URL = _url,
                        Method = "get",
                        Encoding = Encoding.UTF8,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    Res = DealWithHttpResults(httpResult);
                }
            }
            return Res;
        }
        /// <summary>
        /// 处理明细页面明细
        /// </summary>
        /// <param name="httpResult">http参数返回类</param>
        /// <returns>Res</returns>
        private NetSpider.Entity.Service.ProvidentFundQueryRes DealWithHttpResults(HttpResult httpResult)
        {
            List<string> results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='DataList1']/tr[position()>1]", "inner");
            foreach (string item in results)
            {
                ProvidentFundDetail detail = new ProvidentFundDetail();
                List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td/div", "text", true);
                if (tdRow.Count != 7)
                {
                    continue;
                }
                detail.Description = tdRow[3];//描述
                detail.CompanyName = regSpace.Replace(tdRow[2], "");//公司名称
                detail.PayTime = Convert.ToDateTime(regSpace.Replace(tdRow[0], ""));//缴费年月
                DateTime dt;
                if (DateTime.TryParse(tdRow[0], out dt))
                {
                    detail.ProvidentFundTime = dt.ToString("yyyyMM");//应属年月
                }
                if (tdRow[3].IndexOf("汇缴", System.StringComparison.Ordinal) > -1)
                {
                    detail.PersonalPayAmount = (reg.Replace(tdRow[4], "").ToDecimal(0) * perAccounting).ToString("f2").ToDecimal(0);//个人缴费金额
                    detail.CompanyPayAmount = (reg.Replace(tdRow[4], "").ToDecimal(0) * comAccounting).ToString("f2").ToDecimal(0);//企业缴费金额
                    detail.ProvidentFundBase = (reg.Replace(tdRow[4], "").ToDecimal(0) / totalRate).ToString("f2").ToDecimal(0);//基本薪资
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                    PaymentMonths++;
                }
                else if (tdRow[3].IndexOf("支取", System.StringComparison.Ordinal) > -1)
                {
                    detail.PersonalPayAmount = (reg.Replace(tdRow[4], "").ToDecimal(0) * perAccounting).ToString("f2").ToDecimal(0);//个人缴费金额
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                    Res.Description = "有支取，请人工校验。";
                }
                else
                {//（补缴，结息etc，数据不精确，只做参考用）
                    detail.PersonalPayAmount = reg.Replace(tdRow[4], "").ToDecimal(0);//个人缴费金额
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                }
                Res.ProvidentFundDetailList.Add(detail);
            }
            return Res;
        }
    }
}
