using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.ZJ
{
    public class jinhua : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://wsbs.jhgjj.gov.cn/";
        string fundCity = "zj_jinhua";
        #endregion
        #region 私有变量
        string _url = string.Empty;
        private List<string> _results = new List<string>();
        private ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        string _postData = string.Empty;
        private int PaymentMonths = 0;
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {

            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            Random rdm = new Random();
            try
            {
                _url = baseUrl + "SystemWeb/gif.aspx?" + rdm.NextDouble();
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "get",
                    Referer = baseUrl + "PubWeb/default.aspx",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    vcRes.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    vcRes.StatusCode = ServiceConsts.StatusCode_fail;
                    return vcRes;
                }
                vcRes.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                vcRes.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                //合并缓存
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //保存缓存
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
            decimal payRate = (decimal)0.08;
            decimal perAccounting = 0;//个人占比
            decimal comAccounting = 0;//公司占比
            decimal totalRate = 0;//总缴费比率
            string act = string.Empty;
            int index = 0;
            List<string> comName = new List<string>();//公司名称
            List<string> comNo = new List<string>();//公司编号
            string comNametemp = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (string.IsNullOrEmpty(fundReq.Password) || string.IsNullOrEmpty(fundReq.Identitycard) || string.IsNullOrEmpty(fundReq.Name))
                {
                    Res.StatusDescription = "姓名、身份证号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆
                //陈乐  330721199311202425 165228
                //初始化页面开始
                _url = baseUrl + "PubWeb/default.aspx";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Host = "wsbs.jhgjj.gov.cn",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='act']", "value");
                if (_results.Count > 0)
                {
                    act = _results[0];
                }
                else
                {
                    Res.StatusDescription = "页面初始化失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //初始化页面结束
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                _url = baseUrl + "PubWeb/LoginAction.ashx";
                _postData = string.Format("tbxXM={0}&tbxUserName={1}&tbxPass={2}&tbxNZM={3}&act={4}", fundReq.Name.ToUrlEncode(), fundReq.Identitycard, fundReq.Password.ToUrlEncode(), fundReq.Vercode, act);
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "Post",
                    Postdata = _postData,
                    Encoding = Encoding.GetEncoding("utf-8"),
                    Host = "wsbs.jhgjj.gov.cn",
                    Referer = baseUrl + "PubWeb/default.aspx",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_InitFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (!string.IsNullOrWhiteSpace(httpResult.Html))
                {
                    Res.StatusDescription = httpResult.Html;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                #region 第二步,获取基本信息
                _url = baseUrl + "PubWeb/GR/GRZH.aspx";
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "GET",
                    Referer = baseUrl + "PubWeb/default.aspx",
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (!string.IsNullOrEmpty(fundReq.Identitycard))
                {
                    Res.IdentityCard = fundReq.Identitycard;//身份证号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='grid']/tr[2]/td[1]", "text");
                if (_results.Count > 0)
                {
                    Res.CompanyName = _results[0];//公司名称
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='grid']/tr[2]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.Name = _results[0];//姓名
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='grid']/tr[2]/td[3]", "text");
                if (_results.Count > 0)
                {
                    Res.TotalAmount = _results[0].ToDecimal(0);//账户余额
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='grid']/tr[2]/td[4]", "text");
                if (_results.Count > 0)
                {
                    Res.LastProvidentFundTime = _results[0];//缴止年月
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='grid']/tr[2]/td[5]", "text");
                if (_results.Count > 0)
                {
                    Res.Status = _results[0];//当前状态
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='grid']/tr[2]/td[6]/a", "href");
                if (_results.Count > 0)
                {
                    Res.CompanyNo = Regex.Replace(_results[0], @"[^0-9][\s]*", "");//公司编号
                }
                #endregion
                #region 第三步,获取缴费明细

                if (Res.PersonalMonthPayRate > 0 && Res.CompanyMonthPayRate > 0)
                {
                    totalRate = Res.PersonalMonthPayRate + Res.CompanyMonthPayRate;
                    perAccounting = (Res.PersonalMonthPayRate / totalRate);
                    comAccounting = (Res.CompanyMonthPayRate / totalRate);
                }
                else
                {
                    totalRate = (payRate) * 2;//0.16
                    perAccounting = comAccounting = 0.50M;
                }
                _url = baseUrl + "PubWeb/GR/GRZHMX.aspx?grzh=" + Res.CompanyNo;
                httpItem = new HttpItem()
                {
                    URL = _url,
                    Method = "GET",
                    Referer = baseUrl + "PubWeb/GR/GRZH.aspx",
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //公司编号列表
                comNo = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@id='ddlGRZH']/option", "value");
                //公司名称列表
                comName = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@id='ddlGRZH']/option", "text");
                DateTime now = DateTime.Now;//当前时间
                string endTime = now.ToString("yyyy-MM-dd");//结束时间
                string beginTime = now.AddYears(-3).ToString("yyyy-MM-dd");//开始时间
                foreach (string companyNo in comNo)
                {
                    if (comNo.Count == comName.Count)
                    {
                        comNametemp = comName[index];//公司名称
                        index++;
                    }
                    _url = baseUrl + "PubWeb/GR/GRZHMX_List.aspx";
                    _postData = string.Format("ddlGRZH={0}&tbxStart={1}&tbxEnd={2}", companyNo, beginTime, endTime);
                    httpItem = new HttpItem()
                    {
                        URL = _url,
                        Method = "Post",
                        Postdata = _postData,
                        Referer = baseUrl + "PubWeb/GR/GRZHMX.aspx?grzh=" + companyNo,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='grid']/tr[position()>1]", "inner", true);
                    foreach (string item in _results)
                    {
                        ProvidentFundDetail detail = new ProvidentFundDetail();
                        List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        if (tdRow.Count != 5)
                        {
                            continue;
                        }
                        detail.CompanyName = comNametemp;//公司名称
                        
                        detail.PayTime = tdRow[0].ToDateTime(Consts.DateFormatString2);//缴费年月
                        if (tdRow[1].IndexOf("取", StringComparison.Ordinal) > -1)
                        {
                            detail.Description = tdRow[1] + "(所属年月/凭证号:"+tdRow[2]+")";//描述
                            detail.PersonalPayAmount = tdRow[3].ToDecimal(0);//个人缴费金额
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;//缴费类型
                        }
                        else if (tdRow[1].IndexOf("一般入帐", StringComparison.Ordinal) > -1)
                        {
                            detail.Description = tdRow[1];//描述
                            detail.ProvidentFundTime = tdRow[2];//应属年月
                            detail.PersonalPayAmount = (tdRow[3].ToDecimal(0) * perAccounting).ToString("f2").ToDecimal(0);//个人缴费金额
                            detail.CompanyPayAmount = (tdRow[3].ToDecimal(0) * comAccounting).ToString("f2").ToDecimal(0);//企业缴费金额
                            detail.ProvidentFundBase = (tdRow[3].ToDecimal(0) / totalRate).ToString("f2").ToDecimal(0);//缴费基数
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                            PaymentMonths++;
                        }
                        else
                        {
                            detail.Description = tdRow[1] + "(所属年月/凭证号:" + tdRow[2] + ")";//描述
                            detail.ProvidentFundTime = tdRow[2];//应属年月
                            detail.PersonalPayAmount = tdRow[3].ToDecimal(0);//个人缴费金额
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                        }
                        Res.ProvidentFundDetailList.Add(detail);
                    }
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
    }
}
