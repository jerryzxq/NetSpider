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
using System.Text.RegularExpressions;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HB
{
    public class xiangyang : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "https://xygjj.fhts.bankcomm.com/hoob/";
        string fundCity = "hb_xiangyang";
        #endregion


        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            string verificationKey = string.Empty;
            Res.Token = token;
            try
            {
                //获取验证码verificationKey
                Url = baseUrl + "dwr/call/plaincall/hoobLoginAjax.generateVerificationCode.dwr";
                postdata = "callCount=1&httpSessionId=&scriptSessionId=${scriptSessionId}469&c0-scriptName=hoobLoginAjax&c0-methodName=generateVerificationCode&c0-id=0";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
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
                verificationKey = httpResult.Html.ToString().Split(',')[2].Replace("\"", "").Replace(")", "").Replace(";", "").Replace("\r\n", "");
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "LoginAction_displayVerificationCodeImage.action?isUseVerificationCode=true&verificationKey=" + verificationKey;
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
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            string totalCount = string.Empty;
            List<string> results = new List<string>();
            ProvidentFundDetail detail = null;
            int PaymentMonths = 0;
            string errorMsg = string.Empty;
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
                #region  判断用户名密码是否正确

                Url = baseUrl + "dwr/call/plaincall/hoobLoginAjax.loginCheckEx.dwr";
                postdata = string.Format("callCount=1&httpSessionId=&scriptSessionId=${2}688&c0-scriptName=hoobLoginAjax&c0-methodName=loginCheckEx&c0-id=0&c0-e1=string:personalAccount&c0-e2=string:{0}&c0-e3=string:{1}&c0-e4=number:1&c0-e5=boolean:true&c0-e6=string:&c0-e7=string:&c0-e8=string:&c0-e9=string:&c0-e10=string:&c0-param0=Object_Object:{3}&batchId=7", fundReq.Identitycard, fundReq.Password, "{scriptSessionId}", "{loginType:reference:c0-e1, loginCode:reference:c0-e2, password:reference:c0-e3, personalLoginOpt:reference:c0-e4, isValidatePWD:reference:c0-e5, certSignData:reference:c0-e6, certSubjectDN:reference:c0-e7, certIssuerDN:reference:c0-e8, certPlainText:reference:c0-e9, certSN:reference:c0-e10}");
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    //CookieCollection = cookies,
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
                //用户名或密码错误
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "NotExists|", "\"").FromUnicode();
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #endregion

                #region  第一步登录
                Url = baseUrl + "hoob/entry/LoginAction_doLogin.action";
                postdata = string.Format("loginType=personalAccount&checkCodeID=&isUseVerificationCode=true&isValidatePWD=true&certSignData=&certSubjectDN=&certIssuerDN=&certPlainText=&certSN=&cert_password=&personalLoginOpt=1&loginCode={0}&passWord={1}&verificationCode={2}", fundReq.Identitycard, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem
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
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "原因</a>：", "</td>");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 第二步 获取基本信息

                Url = baseUrl + "hoob/PersonalRegisterChangeAction_goMain.action";
                httpItem = new HttpItem
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_content'][2]//tr[2]//input", "value");
                if (results.Count > 0)
                {
                    Res.Phone = results[0];  //电话
                }
                Url = baseUrl + "hoob/PersonalBaseInfoAction_goMain.action";
                httpItem = new HttpItem
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_content']//tr[2]/td[2]/input", "value");
                if (results.Count>0)
                {
                    Res.CompanyNo = results[0];    //单位账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_content']//tr[2]/td[4]/input", "value");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];  //单位名称
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_content']//tr[3]/td[2]/input", "value");
                if (results.Count>0)
                {
                    Res.EmployeeNo = results[0];   //个人账号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_content']//tr[3]/td[4]/input", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];         //姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_content']//tr[3]/td[6]/input", "value");
                if (results.Count > 0)
                {
                    Res.Sex = results[0];          //性别
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_content']//tr[4]/td[2]/input", "value");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];          //身份证号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_content']//tr[6]/td[2]/input", "value");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = results[0].ToDecimal(0) / 100;          //个人缴存率
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_content']//tr[6]/td[4]/input", "value");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = results[0].ToDecimal(0) / 100;          //公司缴存率
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_content']//tr[7]/td[2]/input", "value");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].ToDecimal(0);          //个人缴存款
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_content']//tr[7]/td[4]/input", "value");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = results[0].ToDecimal(0);          //公司缴存款
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_content']//tr[8]/td[6]/input", "value");
                if (results.Count > 0)
                {
                    Res.SalaryBase = results[0].ToDecimal(0);       //工资基数
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_content']//tr[9]/td[2]/input", "value");
                if (results.Count > 0)
                {
                    Res.Status = results[0];       //状态
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_content']//tr[9]/td[4]/input", "value");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);       //账户总额
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_content']//tr[9]/td[6]/input", "value");
                if (results.Count > 0)
                {
                    Res.OpenTime = results[0];       //开户时间
                }

               
                #endregion

                #region 第三部 获取详细信息

                //获取查询总数
                Url = baseUrl + "hoob/PersonalDetailsAction_goMain.action";
                httpItem = new HttpItem
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_page']/tr[1]/td[1]/em[3]", "inner");
                if (results.Count > 0)
                {
                    totalCount = results[0];
                }


                //查询明细
                Url = baseUrl + "hoob/PersonalDetailsAction_goMain.action";
                postdata = string.Format("searchForm.startBusinessDate=&searchForm.endBusinessDate={0}&searchForm.pageSize={1}", DateTime.Now.ToString("yyyy-MM-dd"), totalCount);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "Post",
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_list']/tbody/tr[position()>0]", "inner");
                foreach (var result in results)
                {
                    ProvidentFundDetail providentfounddetail = new ProvidentFundDetail();
                    List<string> tdRow = HtmlParser.GetResultFromParser(result, "//td", "text", true);
                    if (tdRow.Count != 7)
                    {
                        continue;
                    }

                    providentfounddetail.Description = Regex.Replace(tdRow[2], @"[/\&nbsp;\s]", ""); ;//描述
                    //if (tdRow[3] != "&nbsp;")
                    //{
                    //    providentfounddetail.PayTime = tdRow[3].Insert(4, "-").ToDateTime();//缴费年月
                    //}

                    providentfounddetail.PayTime = tdRow[1].Split(';')[1].ToDateTime();


                    if (tdRow[2].IndexOf("转入") > -1)
                    {
                        providentfounddetail.ProvidentFundTime = Regex.Replace(tdRow[3], @"[/\&nbsp;\s]", "");
                        providentfounddetail.PersonalPayAmount = Regex.Replace(tdRow[4], @"[/\&nbsp;\s]", "").ToDecimal(0) / 2;
                        providentfounddetail.CompanyPayAmount = Regex.Replace(tdRow[4], @"[/\&nbsp;\s]", "").ToDecimal(0) / 2;
                        if (Res.PersonalMonthPayRate != 0)
                        {
                            providentfounddetail.ProvidentFundBase = Math.Round(providentfounddetail.PersonalPayAmount / Res.PersonalMonthPayRate, 2);
                        }
                        providentfounddetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                        providentfounddetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                        PaymentMonths++;
                    }
                    else if (tdRow[2].IndexOf("计息") > -1)
                    {
                        providentfounddetail.ProvidentFundTime = Regex.Replace(tdRow[3], @"[/\&nbsp;\s]", "");
                        providentfounddetail.PersonalPayAmount = Regex.Replace(tdRow[4], @"[/\&nbsp;\s]", "").ToDecimal(0);
                        providentfounddetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                        providentfounddetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                    }
                    else
                    {//（补缴，结息etc，数据不精确，只做参考用）
                        providentfounddetail.PersonalPayAmount = Regex.Replace(tdRow[4], @"[/\&nbsp;\s]", "").ToDecimal(0) + Regex.Replace(tdRow[5], @"[/\&nbsp;\s]", "").ToDecimal(0);
                        providentfounddetail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                        providentfounddetail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;//缴费类型
                    }
                    Res.ProvidentFundDetailList.Add(providentfounddetail);
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