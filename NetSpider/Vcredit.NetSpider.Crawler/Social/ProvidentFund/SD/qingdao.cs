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
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SD
{
    public class qingdao : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://219.147.7.52:89/";
        string fundCity = "sd_qingdao";
        #endregion
        #region 私有变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        string Url = string.Empty;
        string postdata = string.Empty;
        decimal payRate = (decimal)0.07;
        List<string> results = new List<string>();
        ProvidentFundDetail detail = null;
        int PaymentMonths = 0;
        #endregion
        public VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "Controller/Image.aspx";
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

                SpiderCacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return Res;
        }

        public ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = fundCity;
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(fundReq.Token);
                    SpiderCacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if ((fundReq.Identitycard.IsEmpty() || fundReq.Password.IsEmpty()) && fundReq.LoginType == "1")
                {
                    Res.StatusDescription = "身份证号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if ((fundReq.Username.IsEmpty() || fundReq.Password.IsEmpty()) && fundReq.LoginType == "2")
                {
                    Res.StatusDescription = "用户名或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录青岛公积金
                Url = baseUrl + "Controller/login.ashx";
                if (fundReq.LoginType == "1")
                    postdata = String.Format("name={0}&password={1}&yzm={2}&logintype=0&usertype=10&dn=&signdata=&1=y", fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
                else
                    postdata = string.Format("name={0}&password={1}&yzm={2}&logintype=0&usertype=20&dn=&signdata=", fundReq.Username, fundReq.Password, fundReq.Vercode);
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
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (!httpResult.Html.Contains("登录成功"))
                {
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    Res.StatusDescription = CommonFun.GetMidStr(httpResult.Html, "{\"msg\":\"", "\",\"success\":false}");
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，查询工作单位基本信息
                Url = baseUrl + "Controller/GR/gjcx/dwjbxx.ashx";
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
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                Res.CompanyOpenTime = jsonParser.GetResultFromParser(httpResult.Html, "clrq");//成立日期
                Res.CompanyName = jsonParser.GetResultFromParser(httpResult.Html, "hm");//单位名称
                Res.CompanyAddress = jsonParser.GetResultFromParser(httpResult.Html, "dz");//单位地址
                Res.CompanyNo = jsonParser.GetResultFromParser(httpResult.Html, "khh");//单位编号
                Res.CompanyDistrict = jsonParser.GetResultFromParser(httpResult.Html, "szqs");//所在市区
                Res.CompanyLicense = jsonParser.GetResultFromParser(httpResult.Html, "yyzz");//营业执照编号
                #endregion

                #region 第三步，查询个人基本信息
                Url = baseUrl + "Controller/GR/gjcx/gjjzlcx.ashx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                Res.Name = jsonParser.GetResultFromParser(httpResult.Html, "hm");//职工姓名
                if (Res.Name.IsEmpty())
                {
                    Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Res.EmployeeNo = jsonParser.GetResultFromParser(httpResult.Html, "khh");//职工编号
                Res.Status = jsonParser.GetResultFromParser(httpResult.Html, "zt");//账户状态
                Res.IdentityCard = jsonParser.GetResultFromParser(httpResult.Html, "sfz");//身份证
                Res.Phone = jsonParser.GetResultFromParser(httpResult.Html, "sjhm");//手机号码
                if (jsonParser.GetResultFromParser(httpResult.Html, "khrq") != "1900-01-01")
                    Res.OpenTime = jsonParser.GetResultFromParser(httpResult.Html, "khrq");//开户日期
                Res.BankCardOpenTime = jsonParser.GetResultFromParser(httpResult.Html, "djrq");//登记日期
                Res.Bank = jsonParser.GetResultFromParser(httpResult.Html, "hb");//发卡行
                Res.BankCardNo = jsonParser.GetResultFromParser(httpResult.Html, "kh");//联名卡号
                Res.CompanyMonthPayRate = jsonParser.GetResultFromParser(httpResult.Html, "dwjcbl").ToDecimal(0) * 0.01M;//单位比例
                Res.CompanyMonthPayAmount = jsonParser.GetResultFromParser(httpResult.Html, "dwyhjje").ToDecimal(0);//单位月汇缴额
                Res.PersonalMonthPayRate = jsonParser.GetResultFromParser(httpResult.Html, "grjcbl").ToDecimal(0) * 0.01M;//个人比例
                Res.PersonalMonthPayAmount = jsonParser.GetResultFromParser(httpResult.Html, "gryhjje").ToDecimal(0);//单位月汇缴额
                Res.SalaryBase = jsonParser.GetResultFromParser(httpResult.Html, "gze").ToDecimal(0);//月工资额
                Res.TotalAmount = jsonParser.GetResultFromParser(httpResult.Html, "zhye").ToDecimal(0);//账户余额
                //Res.MonthPayAmount = Res.PersonalMonthPayAmount + Res.CompanyMonthPayAmount;

                #endregion

                #region 第四步，查询支取信息
                Url = baseUrl + "Controller/GR/gjcx/gjjmx.ashx?dt=" + CommonFun.GetTimeStamp() + "&page=1&rows=1&sort=mxbc&order=desc";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string rows2 = jsonParser.GetResultFromParser(httpResult.Html, "total");

                Url = baseUrl + "Controller/GR/gjcx/gjjmx.ashx?dt=" + CommonFun.GetTimeStamp() + "&page=1&rows=" + rows2 + "&sort=mxbc&order=desc";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string JsonStr = CommonFun.GetMidStrByRegex(httpResult.Html, "\"rows\":", "]") + "]";
                var detailListNew = jsonParser.DeserializeObject<List<QingDaoDetailNew>>(JsonStr);

                ProvidentFundDetail detail = null;
                foreach (var detailItem in detailListNew)
                {
                    if (detailItem.zymzh == "一般支取")
                    {
                        detail = new ProvidentFundDetail();
                        detail.PersonalPayAmount = detailItem.fse.ToDecimal(0);//个人月缴额
                        detail.Description = detailItem.zymzh;//描述
                        detail.PayTime = detailItem.jyrq.ToDateTime();//缴费年月
                        detail.ProvidentFundTime = detailItem.ssny;//应属年月
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                        Res.ProvidentFundDetailList.Add(detail);
                    }
                }
                #endregion

                #region 第五步，查询缴费明细
                Url = baseUrl + "Controller/GR/gjcx/gjjmx.ashx?transDateBegin=2005-01-01&transDateEnd=2014-12-31&page=1&rows=30&sort=mxbc&order=desc";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                Url = baseUrl + "Controller/GR/gjcx/gjcx.ashx";
                string nowdate = DateTime.Now.ToString("yyyy-MM-dd");
                string rows = "1";
                postdata = String.Format("m=grjcmx&start=2005-01-01&end={0}&page=1&rows={1}&sort=csrq&order=desc", nowdate, rows);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                rows = jsonParser.GetResultFromParser(httpResult.Html, "total");

                postdata = String.Format("m=grjcmx&start=2005-01-01&end={0}&page=1&rows={1}&sort=csrq&order=desc", nowdate, rows);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                JsonStr = CommonFun.GetMidStrByRegex(httpResult.Html, "\"rows\":", "]") + "]";
                var detailList = jsonParser.DeserializeObject<List<QingDaoDetail>>(JsonStr);


                int PaymentMonths = 0;
                foreach (var detailItem in detailList)
                {
                    detail = new ProvidentFundDetail();
                    detail.CompanyName = detailItem.hm;//单位名称
                    detail.CompanyPayAmount = detailItem.dwje.ToDecimal(0);//单位月缴额
                    detail.PersonalPayAmount = detailItem.grje.ToDecimal(0);//个人月缴额
                    //detail.PaymentFlag = detailItem.ztname;
                    //detail.PaymentType = detailItem.jjyyname;
                    detail.Description = detailItem.jjyyname;//描述
                    detail.PayTime = detailItem.csrq.ToDateTime();//缴费年月
                    detail.ProvidentFundTime = detailItem.ssny;//应属年月
                    if (detailItem.jjyyname == "正常汇缴")
                    {
                        if (Res.PersonalMonthPayRate > 0)
                        {
                            detail.ProvidentFundBase =
                                (detail.PersonalPayAmount / (Res.PersonalMonthPayRate)).ToString("f2").ToDecimal(0);//缴费基数
                        }
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        PaymentMonths++;
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion

                #region 第六步， 查询贷款基本信息
                //获取合同编号
                Url = baseUrl + "Controller/GR/dkcx/dkhtxx.ashx?dt=" + CommonFun.GetTimeStamp() + "&sort=hth&order=desc";
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
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                JsonStr = CommonFun.GetMidStrByRegex(httpResult.Html, "\"rows\":", "]").Replace("[", "");
                var hth = jsonParser.GetResultFromParser(JsonStr, "hth");//合同号
                if (string.IsNullOrWhiteSpace(hth))
                {
                    goto End;
                }
                ProvidentFundLoanRes Res_Loan = new ProvidentFundLoanRes();
                Url = baseUrl + "Controller/GR/dkcx/dkxx_htxx.ashx?hth=" + hth + "&cxlx=ysf";
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
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                JsonStr = CommonFun.GetMidStrByRegex(httpResult.Html, "\"rows\":", "]").Replace("[", "");
                Res_Loan.Name = jsonParser.GetResultFromParser(JsonStr, "xm");//姓名
                Res_Loan.IdentityCard = jsonParser.GetResultFromParser(JsonStr, "sfz");//身份证号
                Res_Loan.ProvidentFundNo = Res.BankCardNo;//暂用联名卡号
                Res_Loan.Phone = jsonParser.GetResultFromParser(JsonStr, "dh2");//手机号
                Res_Loan.Address = jsonParser.GetResultFromParser(JsonStr, "dwdz");//暂用公司地址
                Res_Loan.Con_No = jsonParser.GetResultFromParser(JsonStr, "hth");//合同号
                Res_Loan.Account_Loan = jsonParser.GetResultFromParser(JsonStr, "yhzh");//贷款账号
                Res_Loan.Account_Repay = jsonParser.GetResultFromParser(JsonStr, "hkzh");//还款账号
                Res_Loan.Loan_Credit = jsonParser.GetResultFromParser(JsonStr, "jkje").ToDecimal(0);//贷款金额
                Res_Loan.Period_Total = jsonParser.GetResultFromParser(JsonStr, "jkqx");//还款期数(总期数)
                Res_Loan.Loan_Rate = jsonParser.GetResultFromParser(JsonStr, "ll") + "%";//贷款利率
                Res_Loan.Repay_Type = jsonParser.GetResultFromParser(JsonStr, "hkfs");//还款方式
                Res_Loan.Current_Repay_Date = jsonParser.GetResultFromParser(JsonStr, "qsrq");//还款日期
                Res_Loan.Bank_Delegate = jsonParser.GetResultFromParser(JsonStr, "wtdkyh");//委托银行
                Res_Loan.Bank_Opening = jsonParser.GetResultFromParser(JsonStr, "yhdh");//开户银行
                Res_Loan.Bank_Repay = jsonParser.GetResultFromParser(JsonStr, "jsstyh");//还款银行
                Res_Loan.House_Purchase_Address = jsonParser.GetResultFromParser(JsonStr, "dz");//房贷地址
                Res_Loan.House_Type = jsonParser.GetResultFromParser(JsonStr, "fwlx");//房屋类型
                #endregion
                #region 第七部，查询贷款还款明细
                //还款息数等基本信息
                Url = baseUrl + "Controller/GR/dkcx/hkmx_wlhkmx_head.ashx?dt=" + CommonFun.GetTimeStamp() + "&hth=" + hth + "";
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
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                var dw_cre0070_hkxx = jsonParser.GetResultFromParser(httpResult.Html, "dw_cre0070_hkxx");
                var dw_cre0070_hkxx_row = jsonParser.GetResultFromParser(dw_cre0070_hkxx, "dw_cre0070_hkxx_row");
                Res_Loan.Principal_Payed = jsonParser.GetResultFromParser(dw_cre0070_hkxx_row, "yhbj").ToDecimal(0);  //已还本金
                Res_Loan.Interest_Payed = jsonParser.GetResultFromParser(dw_cre0070_hkxx_row, "yhlx").ToDecimal(0);  //已还利息
                Res_Loan.Period_Payed = jsonParser.GetResultFromParser(dw_cre0070_hkxx_row, "yhqs").ToInt(0);  //已还期数
                Res_Loan.Overdue_Principal = jsonParser.GetResultFromParser(dw_cre0070_hkxx_row, "yqbj").ToDecimal(0);  //逾期本金（网站）
                Res_Loan.Overdue_Interest = jsonParser.GetResultFromParser(dw_cre0070_hkxx_row, "yqlx").ToDecimal(0);  //逾期利息（网站）
                Res_Loan.Interest_Penalty = jsonParser.GetResultFromParser(dw_cre0070_hkxx_row, "yqfx").ToDecimal(0);  //罚息（网站）
                Res_Loan.Overdue_Period = jsonParser.GetResultFromParser(dw_cre0070_hkxx_row, "dqyqqs").ToInt(0);  //逾期期数
                Res_Loan.Current_Repay_Date = jsonParser.GetResultFromParser(dw_cre0070_hkxx_row, "hkr");  //当期还款日期

                //还款详细信息
                Url = baseUrl + "Controller/GR/dkcx/dkxx_hkmx.ashx?hth=" + hth + "&dt=" + CommonFun.GetTimeStamp() + "&sort=jzrq&order=desc";
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
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                var list = jsonParser.GetArrayFromParse(httpResult.Html, "rows");
                foreach (var item in list)
                {
                    ProvidentFundLoanDetail Res_LoanDetail = new ProvidentFundLoanDetail();
                    Res_LoanDetail.Record_Date = jsonParser.GetResultFromParser(item, "hkrq");  //记账日期
                    Res_LoanDetail.Description = jsonParser.GetResultFromParser(item, "mxlxcn");  //描述
                    if (Res_LoanDetail.Description.Contains("正常扣款"))
                    {
                        Res_LoanDetail.Record_Month = jsonParser.GetResultFromParser(item, "hkrq"); //还款年月
                        Res_LoanDetail.Principal = jsonParser.GetResultFromParser(item, "qhbj").ToDecimal(0); //还款本金
                        Res_LoanDetail.Interest = jsonParser.GetResultFromParser(item, "qhlx").ToDecimal(0); //还款利息
                    }
                    else if (Res_LoanDetail.Description.Contains("正常转逾"))
                    {
                        Res_LoanDetail.Overdue_Principal = jsonParser.GetResultFromParser(item, "yqbj").ToDecimal(0); //逾期本金
                        Res_LoanDetail.Overdue_Interest = jsonParser.GetResultFromParser(item, "yqlx").ToDecimal(0); //逾期利息
                    }
                    else if (Res_LoanDetail.Description.Contains("逾期还款"))
                    {
                        Res_LoanDetail.Overdue_Principal = jsonParser.GetResultFromParser(item, "yqbj").ToDecimal(0); //逾期本金
                        Res_LoanDetail.Overdue_Interest = jsonParser.GetResultFromParser(item, "yqlx").ToDecimal(0); //逾期利息
                    }
                    else if (Res_LoanDetail.Description.Contains("逾期罚息"))
                    {
                        Res_LoanDetail.Interest_Penalty = jsonParser.GetResultFromParser(item, "yqlx").ToDecimal(0); //罚息
                    }
                    Res_LoanDetail.Base = jsonParser.GetResultFromParser(item, "hkbxhj").ToDecimal(0); //还款本息
                    Res_LoanDetail.Balance = jsonParser.GetResultFromParser(item, "sybj").ToDecimal(0); //贷款余额

                    Res_Loan.ProvidentFundLoanDetailList.Add(Res_LoanDetail);
                }

                #endregion
                Res.ProvidentFundLoanRes = Res_Loan;
            End:
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
    internal class QingDaoDetail
    {
        public string csrq { get; set; }
        public string czfs { get; set; }
        public string czfsname { get; set; }
        public string djhm { get; set; }
        public string dwje { get; set; }
        public string grje { get; set; }
        public string grzh { get; set; }
        public string hm { get; set; }
        public string jjyy { get; set; }
        public string jjyyname { get; set; }
        public string jslx { get; set; }
        public string ssny { get; set; }
        public string zt { get; set; }
        public string ztname { get; set; }
    }

    internal class QingDaoDetailNew
    {
        public string fse { get; set; }
        public string jylxzh { get; set; }
        public string jyrq { get; set; }
        public string mxbc { get; set; }
        public string ssny { get; set; }
        public string ye { get; set; }
        public string zymzh { get; set; }
    }
}
