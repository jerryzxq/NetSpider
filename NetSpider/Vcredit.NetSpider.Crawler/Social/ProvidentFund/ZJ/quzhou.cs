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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.ZJ
{
    public class quzhou : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://221.12.139.170/hoob-quzhou/";
        string fundCity = "zj_quzhou";
        int PaymentMonths = 0;
        private decimal payRate = 0.08M;
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            string Url = string.Empty;
            string postData = string.Empty;
            try
            {
                Url = baseUrl + "EntryAction_goPersonal.action";
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

                Url = baseUrl + "dwr/call/plaincall/hoobLoginAjax.generateVerificationCode.dwr";
                StringBuilder strb = new StringBuilder();
                strb.Append("callCount=1\n");
                strb.Append("page=/hoob-quzhou/EntryAction_goPersonal.action\n");
                strb.Append("httpSessionId=\n");
                strb.Append("scriptSessionId=${scriptSessionId}313\n");
                strb.Append("c0-scriptName=hoobLoginAjax\n");
                strb.Append("c0-methodName=generateVerificationCode\n");
                strb.Append("c0-id=0\n");
                strb.Append("batchId=2");
                postData = strb.ToString();
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    ContentType = "text/plain",
                    URL = Url,
                    Method = "POST",
                    Postdata = postData,
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
                string verificationKey = CommonFun.GetMidStr(httpResult.Html, ",\"", "\");");
                Url = baseUrl + "entry/LoginAction_displayVerificationCodeImage.action?isUseVerificationCode=true&verificationKey=" + verificationKey;
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
            ProvidentFundReserveRes ReserveRes = new ProvidentFundReserveRes();//补充公积金
            ProvidentFundDetail detail = null;
            ProvidentFundDetail detailRes = null;
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string eventvalidation = string.Empty;
            string viewstategenerator = string.Empty;
            string viewstate = string.Empty;
            try
            {
                //校验参数 
                if (fundReq.Identitycard.IsEmpty() || fundReq.Password.IsEmpty() || fundReq.Vercode.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                string JSESSIONID = string.Empty;
                Cookie cookie = cookies["JSESSIONID"];
                if (cookie != null)
                {
                    JSESSIONID = cookie.Value;
                }
                #region 第一步,登录

                Url = baseUrl + "entry/LoginAction_doLogin.action;jsessionid=" + JSESSIONID;
                postdata = string.Format("loginType=personalAccount&checkCodeID=&isUseVerificationCode=true&isValidatePWD=true&certSignData=&certSubjectDN=&certIssuerDN=&certPlainText=&certSN=&personalLoginOpt=1&loginCode={0}&passWord={1}&verificationCode={2}", fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = baseUrl + "PersonalRegisterChangeAction_goMain.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@class='input_text_ro']", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                else
                {
                    Res.StatusDescription = "无公积金信息";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }

                #endregion
                #region 第二步，公积金,补充公积金基本信息

                Url = baseUrl + "PersonalBaseInfoAction_goMain.action";
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_content']/tbody/tr[position()>1]/td/input", "value");
                if (results.Count != 23)
                {
                    Res.StatusDescription = "无公积金信息";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                //公积金基本信息
                Res.IdentityCard = fundReq.Identitycard;
                Res.Name = fundReq.Name;
                Res.CompanyNo = results[0];
                Res.CompanyName = results[1];
                Res.ProvidentFundNo = results[2];
                Res.Name = results[3];
                Res.Sex = results[4];
                Res.IdentityCard = results[5];
                Res.Phone = results[7];
                Res.PersonalMonthPayRate = results[9].ToDecimal(0) * 0.01M;
                Res.CompanyMonthPayRate = results[10].ToDecimal(0) * 0.01M;
                Res.PersonalMonthPayAmount = results[11].ToDecimal(0);
                Res.CompanyMonthPayAmount = results[12].ToDecimal(0);
                Res.SalaryBase = results[15].ToDecimal(0);
                Res.Status = results[16];
                Res.OpenTime = results[18];
                Res.TotalAmount = results[22].ToDecimal(0);
                Res.PersonalMonthPayRate = Res.CompanyMonthPayRate = Res.PersonalMonthPayRate > payRate ? Res.PersonalMonthPayRate : payRate;
                //补充公积金基本信息
                int accountType = 1;//明细类型：0:全部,1:公积金,3:公积金补贴,5:住房补贴
                if (results[20].ToDecimal(0) > 0 || results[21].ToDecimal(0) > 0)
                {
                    accountType = 0;
                    ReserveRes.CompanyNo = Res.CompanyNo;
                    ReserveRes.CompanyName = Res.CompanyName;
                    ReserveRes.OpenTime = Res.OpenTime;
                    ReserveRes.ProvidentFundNo = Res.ProvidentFundNo;
                }
                //公积金补贴余额
                if (results[20].ToDecimal(0) > 0)
                {
                    ReserveRes.Status = results[17];
                    ReserveRes.TotalAmount = results[20].ToDecimal(0);
                }
                //住房补贴
                if (results[21].ToDecimal(0) > 0)
                {
                    ReserveRes.Status = Res.Status;
                    ReserveRes.TotalAmount = results[21].ToDecimal(0);
                }
                #endregion
                #region 公积金,补充公积金明细

                int currentPage = 1;
                int totalPage = 1;
                results = new List<string>();
                Url = baseUrl + "PersonalDetailAction_goMain.action";
                do
                {
                    postdata = string.Format("toGoPage_={0}&searchForm.orderPropertyName=&searchForm.jsonWhere=%7B%22accountType%22%3A{3}%2C%22businessName%22%3A%22%22%2C%22businessNo%22%3A%22%22%2C%22corpAccountNo%22%3A%22%22%2C%22endBusinessDate%22%3Anull%2C%22endValueDate%22%3Anull%2C%22firstOrderProp%22%3A%22ASC%7CvalueDate%2CASC%7CbusinessNo%22%2C%22needPage%22%3Atrue%2C%22pageSize%22%3A{1}%2C%22personalAccountNo%22%3A%22{2}%22%2C%22projectionType%22%3A%5B%5D%2C%22propertyCondition%22%3A%5B%5D%2C%22startBusinessDate%22%3Anull%2C%22startValueDate%22%3Anull%7D&searchForm.page={0}", currentPage, 100, Res.ProvidentFundNo, accountType);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (currentPage == 1)
                    {
                        totalPage = CommonFun.GetMidStr(httpResult.Html, "共<em>", "</em>页").ToInt(0);
                    }
                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html.Replace("&nbsp;", ""), "//form[@id='listForm']/table/tbody/tr", "", true));
                    if (results.Count > 2)
                    {
                        results.RemoveRange(results.Count - 2, 2);
                    }
                    currentPage++;
                } while (currentPage <= totalPage);
                foreach (string s in results)
                {
                    List<string> tdRow = HtmlParser.GetResultFromParser(s, "//td", "");
                    if (tdRow.Count < 7) continue;
                    detail = new ProvidentFundDetail();//公积金明细
                    detailRes = new ProvidentFundDetail();//补充公积金明细
                    detail.Description = detailRes.Description = tdRow[1];
                    detail.PayTime = detailRes.PayTime = tdRow[2].ToDateTime();
                    if (tdRow[1].IndexOf("托收缴存公积金") > -1 & tdRow[1].Length == 19)
                    {
                        detail.ProvidentFundTime = detailRes.ProvidentFundTime = tdRow[1].Substring(7, 6);
                    }
                    else
                    {
                        detail.ProvidentFundTime = detailRes.ProvidentFundTime = DateTime.Parse(tdRow[2]).ToString(Consts.DateFormatString7);
                    }
                    if (tdRow[4].ToDecimal(0) > 0)
                    {
                        detail.PaymentFlag = detailRes.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = detailRes.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                    }
                    else if (tdRow[1].IndexOf("缴存") > -1)
                    {
                        detail.PaymentFlag = detailRes.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = detailRes.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                    }
                    else
                    {
                        detail.PaymentFlag = detailRes.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = detailRes.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    }
                    switch (tdRow[6])
                    {
                        case "公积金":
                            if (tdRow[1].IndexOf("期初数据") > -1)
                            {
                                detail.PersonalPayAmount = detail.CompanyPayAmount = tdRow[3].ToDecimal(0) / 2;
                            }
                            else if (detail.PaymentFlag == ServiceConsts.ProvidentFund_PaymentFlag_Normal)
                            {
                                detail.PersonalPayAmount = detail.CompanyPayAmount = tdRow[3].ToDecimal(0) / 2;
                                detail.ProvidentFundBase = detail.PersonalPayAmount / Res.PersonalMonthPayRate;
                            }
                            else
                            {
                                detail.PersonalPayAmount = tdRow[3].ToDecimal(0);
                            }
                            Res.ProvidentFundDetailList.Add(detail);
                            break;
                        case "公积金补贴":
                            if (tdRow[1].IndexOf("期初数据") > -1)
                            {
                                detailRes.PersonalPayAmount = detailRes.CompanyPayAmount = tdRow[3].ToDecimal(0) / 2;
                            }
                            else if (detail.PaymentFlag == ServiceConsts.ProvidentFund_PaymentFlag_Normal)
                            {
                                detailRes.PersonalPayAmount = detailRes.CompanyPayAmount = tdRow[3].ToDecimal(0) / 2;
                                detailRes.ProvidentFundBase = detailRes.PersonalPayAmount / Res.PersonalMonthPayRate;
                            }
                            else
                            {
                                detailRes.PersonalPayAmount = tdRow[3].ToDecimal(0);
                            }
                            ReserveRes.ProvidentReserveFundDetailList.Add(detailRes);
                            break;
                        case "住房补贴":
                            detailRes.CompanyPayAmount = tdRow[3].ToDecimal(0);
                            ReserveRes.ProvidentReserveFundDetailList.Add(detailRes);
                            break;
                    }
                }
                #endregion
                ProvidentFundDetail haveReserve = ReserveRes.ProvidentReserveFundDetailList.FirstOrDefault(o => o.CompanyPayAmount > 0);
                if (haveReserve != null)
                {
                    Res.ProvidentFundReserveRes = ReserveRes;
                }
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

