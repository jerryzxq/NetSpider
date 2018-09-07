using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.JS
{
    public class changzhou : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.czgjj.com/wsfft/";
        string fundCity = "js_changzhou";
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
                Url = baseUrl + "logon.do?k=p";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "jcaptcha?onlynum=true";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "logon.do?k=p",
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
                //Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
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
            ProvidentFundLoanRes Res_Loan = new ProvidentFundLoanRes();
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
                if (fundReq.Username.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }



                #region 登陆

                //Url = string.Format("http://www.czgjj.com/wsfft/dwr/interface/RemoteService.js");

                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "get",
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                StringBuilder str = new StringBuilder();
                str.Append("{\"username\":\"");
                str.Append(fundReq.Username);
                str.Append("\",\"passwd\":\"");
                str.Append(fundReq.Password);
                str.Append("\",\"captcha\":\"");
                str.Append(fundReq.Vercode);
                str.Append("\",\"macAddress\":\"\",\"ipAddress\":\"\",\"hostName\":\"\",\"userType\":\"3@PER\"}");

                string tempDate = string.Format("{0:yyyyMMddHHmmssfff}", DateTime.Now);

                Url = baseUrl + "invoke.do";
                postdata = string.Format("FRAMEserviceName=LoginService.login&FRAMEparams={0}&FRAMEinvokeSeqno={1}&FRAMEesbSeqno={1}", str.ToString().ToUrlEncode(Encoding.GetEncoding("utf-8")), GetUUID());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    Referer = baseUrl + "logon.do?k=p",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                //httpItem.Header.Add("X-Requested-With", "XMLHttpRequest");
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string returntext = jsonParser.GetResultFromParser(httpResult.Html, "text");
                if(returntext.Contains("密码错误"))
                {
                    Res.StatusDescription = "密码错误！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (returntext.Contains("初始密码"))
                {
                    Res.StatusDescription = "初始密码不能在此登录!";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string returncode = jsonParser.GetResultFromParser(httpResult.Html, "data");
                if (returncode != "1")
                {
                    if (returncode == "2")
                    {
                        Res.StatusDescription = "密码已过期,请及时更改密码！";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    else if (returncode == "-1")
                    {
                        Res.StatusDescription = "验证码输入错误！";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    else
                    {
                        Res.StatusDescription = "登录失败，请重新登录！";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = baseUrl + "main.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "logon.do?k=p",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@url='action.do?path=person/q_zgjcmx']", "id");
                string menu_id = string.Empty;
                if (results.Count > 0)
                {
                    menu_id = results[0].Replace("m_", "");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@url='action.do?path=person/q_grdkxx']", "id");
                string menu_id_loan = string.Empty;
                if (results.Count > 0)
                {
                    menu_id_loan = results[0].Replace("m_", "");
                }
                #endregion

                #region 基本信息
                Url = baseUrl + "action.do?path=person/q_zgjbxx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "main.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string serviceName = CommonFun.GetMidStr(httpResult.Html, "ServiceEx(\"", "\"");
                string spcode = CommonFun.GetMidStr(httpResult.Html, "spcode:\"", "\"");

                Url = baseUrl + "invoke.do";
                postdata = string.Format("FRAMEserviceName={0}&FRAMEparams={1}&FRAMEinvokeSeqno={2}&FRAMEesbSeqno={2}", serviceName, ("{\"spcode\":\"" + spcode + "\"}").ToUrlEncode(), GetUUID());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "action.do?path=person/q_zgjbxx",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string json_data = jsonParser.GetResultFromParser(httpResult.Html, "data");
                List<BaseInfo> baseinfo = jsonParser.DeserializeObject<List<BaseInfo>>(json_data);
                if (baseinfo.Count > 0)
                {
                    Res.CompanyNo = baseinfo[0].sncode;
                    Res.CompanyName = baseinfo[0].snname;
                    Res.EmployeeNo = baseinfo[0].spcode;
                    Res.Name = baseinfo[0].spname;
                    Res.IdentityCard = baseinfo[0].spidno;
                    Res.Phone = baseinfo[0].sjh;
                    Res_Loan.Address = baseinfo[0].splxdz;
                    Res.PersonalMonthPayAmount = baseinfo[0].spmfact.ToDecimal(0) / 2;
                    Res.CompanyMonthPayAmount = Res.PersonalMonthPayAmount;
                    Res.TotalAmount = baseinfo[0].spmend.ToDecimal(0);
                    Res.LastProvidentFundTime = baseinfo[0].spjym;
                    Res.SalaryBase = baseinfo[0].spgz.ToDecimal(0);
                    Res.PersonalMonthPayRate = baseinfo[0].spjcbl.ToDecimal(0);
                    Res.CompanyMonthPayRate = Res.PersonalMonthPayRate;
                    Res.Status = baseinfo[0].hjztname;
                }
                #endregion

                #region 公积金明细
                Url = baseUrl + "action.do?path=person/q_zgjcmx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "main.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                Url = baseUrl + "invoke.do";
                postdata = string.Format("FRAMEserviceName={0}&FRAMEparams={1}&FRAMEinvokeSeqno={2}&FRAMEesbSeqno={2}", "210006", ("{\"menuid\":\"" + menu_id + "\",\"limit.length\":10,\"limit.enable\":true,\"limit.start\":1}").ToUrlEncode(), GetUUID());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "action.do?path=person/q_zgjcmx",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                for (int i = 0; i < 5; i++)
                {
                    postdata = string.Format("FRAMEserviceName={0}&FRAMEparams={1}&FRAMEinvokeSeqno={2}&FRAMEesbSeqno={2}", "21A102", ("{\"limit.start\":1,\"limit.length\":null,\"spcode\":\"" + spcode + "\",\"ywlx\":\"1\",\"jcnf\":\"" + DateTime.Now.AddYears(-i).Year + "\",\"limit.enable\":false}").ToUrlEncode(), GetUUID());
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Referer = baseUrl + "action.do?path=person/q_zgjcmx",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    json_data = jsonParser.GetResultFromParser(httpResult.Html, "data");
                    List<GJJDetailInfo> gjjdetailinfo = jsonParser.DeserializeObject<List<GJJDetailInfo>>(json_data);
                    if (gjjdetailinfo.Count > 0)
                    {
                        gjjdetailinfo.RemoveAt(0);
                    }
                    foreach (GJJDetailInfo gjjdetail in gjjdetailinfo)
                    {
                        ProvidentFundDetail _detail = new ProvidentFundDetail();
                        _detail.PayTime = DateTime.ParseExact(gjjdetail.qrrq, "yyyy.MM.dd", null);
                        _detail.ProvidentFundTime = gjjdetail.hjny.Replace(".", "");
                        _detail.CompanyName = gjjdetail.snname;
                        _detail.Description = gjjdetail.note;
                        if (gjjdetail.cllxname.Contains("正常汇缴"))
                        {
                            _detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            _detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            _detail.PersonalPayAmount = gjjdetail.sr.ToDecimal(0) / 2;
                            _detail.CompanyPayAmount = _detail.PersonalPayAmount;
                            if (Res.PersonalMonthPayRate != 0)
                            {
                                _detail.ProvidentFundBase = _detail.PersonalPayAmount * 100 / Res.PersonalMonthPayRate;
                            }
                        }
                        else if (gjjdetail.cllxname.Contains("支取") || gjjdetail.cllxname.Contains("提取") || gjjdetail.zc.ToDecimal(0) != 0)
                        {
                            _detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                            _detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                            _detail.PersonalPayAmount = gjjdetail.zc.ToDecimal(0);
                        }
                        else
                        {
                            _detail.PaymentFlag = gjjdetail.cllxname;
                            _detail.PaymentType = gjjdetail.cllxname;
                            _detail.PersonalPayAmount = gjjdetail.sr.ToDecimal(0);
                        }
                        Res.ProvidentFundDetailList.Add(_detail);
                    }
                }
                #endregion

                #region 贷款基本信息
                Url = baseUrl + "action.do?path=person/q_grdkxx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "main.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                Url = baseUrl + "invoke.do";
                postdata = string.Format("FRAMEserviceName={0}&FRAMEparams={1}&FRAMEinvokeSeqno={2}&FRAMEesbSeqno={2}", "21A107", ("{\"spcode\":\"" + spcode + "\"}").ToUrlEncode(), GetUUID());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "action.do?path=person/q_grdkxx",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (jsonParser.GetResultFromParser(httpResult.Html, "text").Contains("没有贷款") || jsonParser.GetArrayFromParse(httpResult.Html, "data").Count == 0)
                {
                    goto End;
                }
                string Loan_Sid = jsonParser.GetResultFromParser(results[0], "sqbh");

                Url = baseUrl + "invoke.do";
                postdata = string.Format("FRAMEserviceName={0}&FRAMEparams={1}&FRAMEinvokeSeqno={2}&FRAMEesbSeqno={2}", "21A103", ("{\"spcode\":\"" + spcode + "\",\"sqbh\":\"" + Loan_Sid + "\"}").ToUrlEncode(), GetUUID());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "action.do?path=person/q_grdkxx",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                json_data = jsonParser.GetResultFromParser(httpResult.Html, "data");
                List<LoanBaseInfo> loanbaseinfo = jsonParser.DeserializeObject<List<LoanBaseInfo>>(json_data);
                if (loanbaseinfo.Count == 0)
                {
                    goto End;
                }
                Res_Loan.Name = Res.Name;
                Res_Loan.IdentityCard = Res.IdentityCard;
                Res_Loan.Loan_Sid = loanbaseinfo[0].sqbh;
                Res_Loan.Delegate_Date = loanbaseinfo[0].sqrq;
                Res_Loan.Loan_Start_Date = loanbaseinfo[0].fdrq;
                Res_Loan.Period_Total = loanbaseinfo[0].jkqx + "月";
                Res_Loan.Status = loanbaseinfo[0].sqztname;
                Res_Loan.Current_Repay_Total = loanbaseinfo[0].yhje.ToDecimal(0);
                Res_Loan.Account_Repay = loanbaseinfo[0].xykh;
                Res_Loan.Account_Loan = loanbaseinfo[0].hkzh;
                Res_Loan.Loan_Rate = loanbaseinfo[0].jkyll;
                Res_Loan.Repay_Type = loanbaseinfo[0].hkfsname;
                Res_Loan.Loan_Balance = loanbaseinfo[0].bjye.ToDecimal(0);

                #endregion

                #region 贷款明细
                int year = DateTime.ParseExact(Res_Loan.Loan_Start_Date, "yyyy.MM.dd", null).Year;
                while (year <= DateTime.Now.Year)
                {
                    Url = baseUrl + "invoke.do";
                    postdata = string.Format("FRAMEserviceName={0}&FRAMEparams={1}&FRAMEinvokeSeqno={2}&FRAMEesbSeqno={2}", "21A104", ("{\"limit.start\":1,\"limit.length\":null,\"hknf\":\"" + year + "\",\"sqbh\":\"" + Res_Loan.Loan_Sid + "\",\"spcode\":\"" + spcode + "\",\"limit.enable\":false}").ToUrlEncode(), GetUUID());
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Referer = baseUrl + "action.do?path=person/q_grdkxx",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    json_data = jsonParser.GetResultFromParser(httpResult.Html, "data");
                    List<LoanDetailInfo> loandetailinfo = jsonParser.DeserializeObject<List<LoanDetailInfo>>(json_data);
                    if (loandetailinfo.Count > 0)
                    {
                        loandetailinfo.RemoveAt(0);
                    }

                    foreach (LoanDetailInfo loandetail in loandetailinfo)
                    {
                        ProvidentFundLoanDetail _detail_loan = new ProvidentFundLoanDetail();
                        _detail_loan.Principal = loandetail.bj.ToDecimal(0);
                        _detail_loan.Interest = loandetail.lx.ToDecimal(0);
                        _detail_loan.Interest_Penalty = loandetail.fx.ToDecimal(0);
                        _detail_loan.Base = loandetail.hj.ToDecimal(0);
                        _detail_loan.Bill_Date = loandetail.hkrq;
                        _detail_loan.Record_Date = _detail_loan.Bill_Date;
                        _detail_loan.Description = loandetail.cllxname;
                        if (!_detail_loan.Description.IsEmpty())
                        {
                            _detail_loan.Description += "，" + loandetail.note;
                        }
                        Res_Loan.ProvidentFundLoanDetailList.Add(_detail_loan);
                    }

                    year++;
                }
                #endregion

            End:
                Res.ProvidentFundLoanRes = Res_Loan;
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

        #region 私有方法
        string GetUUID()
        {
            char[] basechar = "0123456789ABCDEF".ToCharArray();

            char[] retchar = new char[32];
            retchar[12] = '4';
            Random rd = new Random();
            for (int i = 0; i < 32; i++)
            {
                if (i != 12)
                {

                    retchar[i] = basechar[rd.Next(0, basechar.Length)];
                }
                if (i == 16)
                {
                    retchar[i] = basechar[(rd.Next(0, basechar.Length) & 0x3) | 0x08];
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach (char c in retchar)
            {
                sb.Append(c);
            }

            return sb.ToString();
        }

        class BaseInfo
        {
            public string sncode { get; set; }
            public string snname { get; set; }
            public string spcode { get; set; }
            public string spname { get; set; }
            public string spidno { get; set; }
            public string sptel { get; set; }
            public string sjh { get; set; }
            public string splxdz { get; set; }
            public string spmfact { get; set; }
            public string spmend { get; set; }
            public string spjym { get; set; }
            public string spgz { get; set; }
            public string spjcbl { get; set; }
            public string hjztname { get; set; }
        }

        class GJJDetailInfo
        {
            public string cllxname { get; set; }
            public string ye { get; set; }
            public string rn { get; set; }
            public string zc { get; set; }
            public string sr { get; set; }
            public string spidsrno { get; set; }
            public string spcode { get; set; }
            public string sncode { get; set; }
            public string spname { get; set; }
            public string cllx { get; set; }
            public string lx { get; set; }
            public string id { get; set; }
            public string qrrq { get; set; }
            public string hjny { get; set; }
            public string snname { get; set; }
            public string note { get; set; }
        }

        class LoanBaseInfo
        {
            public string sqqx { get; set; }
            public string status { get; set; }
            public string xykh { get; set; }
            public string dbfsname { get; set; }
            public string hkms { get; set; }
            public string spcode { get; set; }
            public string spname { get; set; }
            public string jkje { get; set; }
            public string jkqx { get; set; }
            public string jsrq { get; set; }
            public string sqztname { get; set; }
            public string sqje { get; set; }
            public string sqbh { get; set; }
            public string hkfs { get; set; }
            public string dbfs { get; set; }
            public string hkzh { get; set; }
            public string bjye { get; set; }
            public string sqrq { get; set; }
            public string fdrq { get; set; }
            public string hkfsname { get; set; }
            public string jkyll { get; set; }
            public string note { get; set; }
            public string yhje { get; set; }
            public string htbh { get; set; }
        }

        class LoanDetailInfo
        {
            public string cllxname { get; set; }
            public string hklx { get; set; }
            public string hkzh { get; set; }
            public string ino { get; set; }
            public string rn { get; set; }
            public string bj { get; set; }
            public string fx { get; set; }
            public string bjye { get; set; }
            public string hj { get; set; }
            public string hkrq { get; set; }
            public string note { get; set; }
            public string lx { get; set; }
        }
        #endregion

    }
}
