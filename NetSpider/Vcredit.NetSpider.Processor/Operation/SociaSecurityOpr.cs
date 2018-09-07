using System.Net;
using System.Text;
using System.Web;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.Common.Constants;
using Vcredit.NetSpider.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vcredit.NetSpider.Processor.Operation.JsonModel.SociaSecurity;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Vcredit.NetSpider.Processor.Operation
{
    internal class SociaSecurityOpr
    {
        #region 变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        #endregion

        #region 成都
        /// <summary>
        /// 成都登录初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Chengdu_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {

                Url = "http://www.cdhrss.gov.cn/images/image.jsp";
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
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.Number);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "成都社保查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "成都社保查询初始化异常";
                Log4netAdapter.WriteError("成都社保查询初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 成都社保登录、查询
        /// </summary>
        /// <param name="username">登录名</param>
        /// <param name="password">密码</param>
        /// <param name="token">会话token</param>
        /// <param name="vercode">验证码</param>
        /// <returns></returns>
        public SocialSecurityQueryRes Chengdu_GetSocialSecurity(string username, string password, string token, string vercode)
        {

            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            Res.SocialSecurityCity = "成都";
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = "http://www.cdhrss.gov.cn/LoginSIAction.do";
                postdata = String.Format("siusername={0}&sipassword={1}&randCode={2}", username, password, vercode);
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.Html.IndexOf("图形验证码输入错误。") != -1)
                {
                    //goto Lable_Start;
                    Res.StatusDescription = "图形验证码输入错误。";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (httpResult.Html.IndexOf("社保编码或者查询密码错误。") != -1)
                {
                    Res.StatusDescription = "社保编码或者查询密码错误。";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = "http://www.cdhrss.gov.cn/QueryListAction.do?title=%25E4%25B8%25AA%25E4%25BA%25BA%25E5%259F%25BA%25E6%259C%25AC%25E4%25BF%25A1%25E6%2581%25AF&bizID=BC.000.000.001";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                Res.Name = CommonFun.GetMidStrByRegex(httpResult.Html, "姓名</nobr></td><td bgcolor=\"#ffffff\"><nobr>", "</nobr>");//姓名
                Res.WorkDate = CommonFun.GetMidStrByRegex(httpResult.Html, "参工时间</nobr></td><td bgcolor=\"#ffffff\"><nobr>", "</nobr>");//参加工作日期
                Res.Sex = CommonFun.GetMidStrByRegex(httpResult.Html, "性别</nobr></td><td bgcolor=\"#ffffff\"><nobr>", "</nobr>");//性别
                Res.BirthDate = CommonFun.GetMidStrByRegex(httpResult.Html, "出生日期</nobr></td><td bgcolor=\"#ffffff\"><nobr>", "</nobr>");//出生日期
                Res.IdentityCard = CommonFun.GetMidStrByRegex(httpResult.Html, "身份证</nobr></td><td bgcolor=\"#ffffff\"><nobr>", "</nobr>");//身份证
                if (Res.Name.IsEmpty())
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                int thisyear = DateTime.Now.Year;


                List<string> result1 = new List<string>();
                List<string> result2 = new List<string>();
                List<string> result3 = new List<string>();
                //养老金月缴费
                for (int i = 0; i <= 3; i++)
                {
                    Url = "http://www.cdhrss.gov.cn/QueryListAction.do";
                    postdata = String.Format("p_year={0}&x=23&y=14&bizID=BC.002.000.001", thisyear - i);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (httpResult.Html.IndexOf("养老保险状态已经中止，暂不能查询明细") != -1)
                    {
                        break;
                    }
                    result1.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[position()>1]", ""));
                }
                //医保月缴费
                for (int i = 0; i <= 3; i++)
                {
                    Url = "http://www.cdhrss.gov.cn/QueryListAction.do";
                    postdata = String.Format("p_year={0}&x=23&y=14&bizID=BC.001.000.001", thisyear - i);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    result2.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[position()>1]", ""));
                }
                //失业保险月缴费
                for (int i = 0; i <= 3; i++)
                {
                    Url = "http://www.cdhrss.gov.cn/QueryListAction.do";
                    postdata = String.Format("p_year={0}&x=23&y=14&bizID=BC.012.000.001", thisyear - i);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    result3.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[position()>1]", ""));
                }


                SocialSecurityDetailQueryRes detailRes = null;
                List<string> tdRows1 = new List<string>();
                List<string> tdRows2 = new List<string>();
                List<string> tdRows3 = new List<string>();
                for (int i = 0; i < result1.Count; i++)
                {
                    detailRes = new SocialSecurityDetailQueryRes();
                    tdRows1 = HtmlParser.GetResultFromParser(result1[i], "//td/nobr", "");
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;

                    detailRes.PayTime = tdRows1[7];
                    detailRes.SocialInsuranceTime = tdRows1[1];
                    detailRes.SocialInsuranceBase = tdRows1[2].ToDecimal(0);

                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;

                    //养老
                    detailRes.PensionAmount = tdRows1[4].ToDecimal(0);
                    detailRes.CompanyPensionAmount = tdRows1[3].ToDecimal(0);

                    foreach (string strItem in result2)
                    {
                        if (strItem.Contains(tdRows1[1]))
                        {
                            tdRows2 = HtmlParser.GetResultFromParser(strItem, "//td/nobr", "");
                            //医疗
                            detailRes.MedicalAmount = tdRows2[4].ToDecimal(0);
                            detailRes.CompanyMedicalAmount = tdRows2[3].ToDecimal(0);
                            break;
                        }
                    }
                    foreach (string strItem in result3)
                    {
                        if (strItem.Contains(tdRows1[1]))
                        {
                            tdRows3 = HtmlParser.GetResultFromParser(strItem, "//td/nobr", "");
                            //失业
                            detailRes.UnemployAmount = tdRows3[3].ToDecimal(0);
                            break;
                        }
                    }


                    Res.Details.Add(detailRes);
                }
                Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("成都社保查询异常", e);
            }
            return Res;
        }

        ///// <summary>
        ///// 成都登录初始化
        ///// </summary>
        ///// <returns></returns>
        //public VerCodeRes Chengdu_Init()
        //{

        //    VerCodeRes Res = new VerCodeRes();
        //    string Url = string.Empty;
        //    string postdata = string.Empty;
        //    List<string> results = new List<string>();
        //    string token = CommonFun.GetGuidID();
        //    Res.Token = token;
        //    try
        //    {
        //        Url = "https://gr.cdhrss.gov.cn:442/cdwsjb/login.jsp";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "get",
        //            CookieCollection = cookies,
        //            SecurityProtocolType = SecurityProtocolType.Ssl3,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        Url = "https://gr.cdhrss.gov.cn:442/cdwsjb/CaptchaImg";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "get",
        //            ResultType = ResultType.Byte,
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
        //        Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
        //        //保存验证码图片在本地
        //        FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
        //        Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
        //        Res.StatusCode = ServiceConsts.StatusCode_success;


        //        Res.StatusDescription = "成都社保查询已初始化";
        //        Res.StatusCode = ServiceConsts.StatusCode_success;

        //        CacheHelper.SetCache(token, cookies);
        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Res.StatusDescription = "成都社保查询初始化异常";
        //        Log4netAdapter.WriteError("成都社保查询初始化异常", e);
        //    }
        //    return Res;
        //}
        ///// <summary>
        ///// 成都社保登录、查询
        ///// </summary>
        ///// <param name="username">登录名</param>
        ///// <param name="password">密码</param>
        ///// <param name="token">会话token</param>
        ///// <param name="vercode">验证码</param>
        ///// <returns></returns>
        //public SocialSecurityQueryRes Chengdu_GetSocialSecurity(string username, string password, string token, string vercode)
        //{

        //    SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
        //    Res.SocialSecurityCity = "成都";
        //    string Url = string.Empty;
        //    string postdata = string.Empty;
        //    List<string> results = new List<string>();
        //    try
        //    {
        //        //获取缓存
        //        if (CacheHelper.GetCache(token) != null)
        //        {
        //            cookies = (CookieCollection)CacheHelper.GetCache(token);
        //            CacheHelper.RemoveCache(token);
        //        }
        //        //校验参数
        //        if (username.IsEmpty() || password.IsEmpty())
        //        {
        //            Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }

        //        Url = "https://gr.cdhrss.gov.cn:442/cdwsjb/j_spring_security_check";
        //        postdata = String.Format("j_username={0}&j_password={1}&checkCode={2}", username, password, vercode);
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "POST",
        //            Postdata = postdata,
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        //请求失败后返回
        //        if (httpResult.StatusCode != HttpStatusCode.OK)
        //        {
        //            Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
        //            Res.StatusCode = ServiceConsts.StatusCode_httpfail;
        //            return Res;
        //        }
        //        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

        //        string msg = jsonParser.GetResultFromParser(httpResult.Html, "msg");
        //        if (!msg.IsEmpty())
        //        {
        //            //goto Lable_Start;
        //            Res.StatusDescription = msg;
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }

        //        Url = "https://gr.cdhrss.gov.cn:442/cdwsjb/runqian/reportJsp/showReport.jsp?raq=gt0200.raq";
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "get",
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='report1_sub_1_1']/tr[1]/td[2]", "text");
        //        if (results.Count == 0)
        //        {
        //            Res.StatusDescription = "无社保信息";
        //            Res.StatusCode = ServiceConsts.StatusCode_fail;
        //            return Res;
        //        }
        //        else
        //        {
        //            Res.EmployeeNo = results[0];
        //        }
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='report1_sub_1_1']/tr[1]/td[4]", "text");
        //        if (results.Count > 0)
        //        {
        //            Res.Name = results[0];
        //        }
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='report1_sub_1_1']/tr[1]/td[6]", "text");
        //        if (results.Count > 0)
        //        {
        //            Res.IdentityCard = results[0];
        //        }

        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='report1_sub_1_1']/tr[2]/td[2]", "text");
        //        if (results.Count > 0)
        //        {
        //            Res.Sex = results[0];
        //        }
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='report1_sub_1_1']/tr[2]/td[4]", "text");
        //        if (results.Count > 0)
        //        {
        //            Res.Race = results[0];
        //        }
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='report1_sub_1_1']/tr[2]/td[6]", "text");
        //        if (results.Count > 0)
        //        {
        //            Res.BirthDate = results[0];
        //        }
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='report1_sub_1_1']/tr[3]/td[2]", "text");
        //        if (results.Count > 0)
        //        {
        //            Res.WorkDate = results[0];
        //        }
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='report1_sub_1_1']/tr[3]/td[4]", "text");
        //        if (results.Count > 0)
        //        {
        //            Res.EmployeeStatus = results[0];
        //        }
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='report1_sub_1_1']/tr[5]/td[6]", "text");
        //        if (results.Count > 0)
        //        {
        //            Res.Phone = results[0];
        //        }
        //        string prm = CommonFun.GetMidStrByRegex(httpResult.Html, "gt0200_jf01.raq&", "',");

        //        Url = "https://gr.cdhrss.gov.cn:442/cdwsjb/runqian/reportJsp/showReport.jsp?raq=gt0200_jf01.raq&" + prm;
        //        httpItem = new HttpItem()
        //        {
        //            URL = Url,
        //            Method = "get",
        //            CookieCollection = cookies,
        //            ResultCookieType = ResultCookieType.CookieCollection
        //        };
        //        httpResult = httpHelper.GetHtml(httpItem);
        //        string prm_yab139 = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='prm_yab139']", "value")[0];
        //        string prm_aac001 = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='prm_aac001']", "value")[0];
        //        string reportParamsId = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='reportParamsId']", "value")[0];
        //        int report1_currPage = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='report1_currPage']", "value")[0].ToInt(1);
        //        string report1_cachedId = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='report1_cachedId']", "value")[0];
        //        int totalPage = CommonFun.GetMidStrByRegex(httpResult.Html, "report1_getTotalPage\\(\\) {return ", ";").ToInt(1);
        //        results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='report1']/tr[position()>2]");


        //        while (report1_currPage < totalPage)
        //        {
        //            report1_currPage++;
        //            Url = "https://gr.cdhrss.gov.cn:442/cdwsjb/runqian/reportJsp/showReport.jsp";
        //            postdata = String.Format("raq=gt0200_jf01.raq&prm_yab139={0}&prm_aac001={1}&reportParamsId={2}&report1_currPage={3}&report1_currPage={4}", prm_yab139, prm_aac001, reportParamsId, report1_currPage, report1_cachedId);
        //            httpItem = new HttpItem()
        //            {
        //                URL = Url,
        //                Method = "POST",
        //                Postdata = postdata,
        //                CookieCollection = cookies,
        //                ResultCookieType = ResultCookieType.CookieCollection
        //            };
        //            httpResult = httpHelper.GetHtml(httpItem);
        //            results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='report1']/tr[position()>2]"));
        //        }


        //        SocialSecurityDetailQueryRes detailRes = null;
        //        foreach (string item in results)
        //        {
        //            var tdRows = HtmlParser.GetResultFromParser(item, "//td", "text");
        //            if (tdRows.Count != 10)
        //            {
        //                continue;
        //            }
        //            detailRes = new SocialSecurityDetailQueryRes();
        //            detailRes.Name = Res.Name;
        //            detailRes.IdentityCard = Res.IdentityCard;
        //            detailRes.CompanyName = tdRows[2];

        //            detailRes.PayTime = tdRows[9];
        //            detailRes.SocialInsuranceTime = tdRows[1];
        //            detailRes.SocialInsuranceBase = tdRows[3].ToDecimal(0);

        //            if (tdRows[6] == "正常缴费记录")
        //            {
        //                detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
        //                detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
        //            }
        //            else
        //            {
        //                detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
        //                detailRes.PaymentType = tdRows[6];
        //            }
        //            //养老
        //            detailRes.PensionAmount = tdRows[5].ToDecimal(0);
        //            detailRes.CompanyPensionAmount = tdRows[4].ToDecimal(0);
        //            Res.Details.Add(detailRes);
        //        }

        //        Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QuerySuccess;
        //        Res.StatusCode = ServiceConsts.StatusCode_success;

        //    }
        //    catch (Exception e)
        //    {
        //        Res.StatusCode = ServiceConsts.StatusCode_error;
        //        Log4netAdapter.WriteError("成都社保查询异常", e);
        //    }
        //    return Res;
        //}
        #endregion

        #region 上海
        /// <summary>
        /// 青岛社保初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Shanghai_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {

                Url = "http://www.12333sh.gov.cn/sbsjb/wzb/Bmblist.jsp";
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
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "青岛社保查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "青岛社保查询初始化异常";
                Log4netAdapter.WriteError("青岛社保查询初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 上海社保登录、查询
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public SocialSecurityQueryRes Shanghai_GetSocialSecurity(string username, string password, string token, string vercode)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = "上海";
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(username) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(username);
                    CacheHelper.RemoveCache(username);
                }
                //校验参数
                if (username.IsEmpty() || username.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录上海社保
                Url = "http://www.12333sh.gov.cn/sbsjb/wzb/dologin.jsp";
                postdata = String.Format("userid={0}&userpw={1}&userjym={2}", username, password, vercode);
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
                        Res.StatusDescription = results[0];
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
                Url = "http://www.12333sh.gov.cn/sbsjb/wzb/sbsjbcx.jsp";
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
                Res.IdentityCard = username;//身份证

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//xml[@id='dataisxxb_sum1']/xxblist/jsjs/xm");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
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


                results = HtmlParser.GetResultFromParser(httpResult.Html, "//xml[@id='dataisxxb_sum2']/xxblist/jsjs");
                List<string> results1 = new List<string>();
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
                    results1 = HtmlParser.GetResultFromParser(trItem, "//jsjs3");
                    if (results1.Count > 0)
                    {
                        detailRes.SocialInsuranceBase = results1[0].ToDecimal(0);
                    }
                    results1 = HtmlParser.GetResultFromParser(trItem, "//jsjs4");
                    if (results1.Count > 0)
                    {
                        detailRes.PensionAmount = results1[0].ToDecimal(0);
                    }
                    results1 = HtmlParser.GetResultFromParser(trItem, "//jsjs6");
                    if (results1.Count > 0)
                    {
                        detailRes.MedicalAmount = results1[0].ToDecimal(0);
                    }
                    results1 = HtmlParser.GetResultFromParser(trItem, "//jsjs8");
                    if (results1.Count > 0)
                    {
                        detailRes.UnemployAmount = results1[0].ToDecimal(0);
                    }

                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;

                    Res.Details.Add(detailRes);
                    PaymentMonths++;
                }
                #endregion

                Res.PaymentMonths = PaymentMonths;

                Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("成都社保查询异常", e);
            }
            return Res;
        }
        #endregion

        #region 青岛
        /// <summary>
        /// 青岛社保初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Qingdao_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {

                Url = "http://221.215.38.136/grcx/common/checkcode.do";
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
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.Number);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "青岛社保查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "青岛社保查询初始化异常";
                Log4netAdapter.WriteError("青岛社保查询初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 青岛社保登录、查询
        /// </summary>
        /// <param name="username">登录名</param>
        /// <param name="password">密码</param>
        /// <param name="token">会话token</param>
        /// <param name="vercode">验证码</param>
        /// <returns></returns>
        public SocialSecurityQueryRes Qingdao_GetSocialSecurity(string username, string password, string token, string vercode)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            Res.SocialSecurityCity = "青岛";
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                string passwordMD5 = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(password, "MD5"); ;
                Url = "http://221.215.38.136/grcx/work/login.do?method=login";
                postdata = String.Format("method=login&domainId=1&groupid=-95&loginName={0}&loginName18=&password={1}&checkCode={2}", username, passwordMD5.ToLower(), vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Encoding = Encoding.UTF8,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@class='text3']");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，查询基本信息
                Url = "http://221.215.38.136/grcx/work/m01/f1101/personQuery.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input", "value");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];
                    Res.CompanyType = results[1];
                    Res.CompanyStatus = results[2];
                    Res.District = results[3];
                    Res.CompanyName = results[4];
                    Res.EmployeeNo = results[5];
                    Res.Name = results[6];
                    Res.IdentityCard = results[7];
                    Res.Sex = results[8];
                    Res.WorkDate = results[9];
                    Res.BirthDate = results[10];
                    Res.EmployeeStatus = results[11];
                    Res.Race = results[12];
                    Res.IsSpecialWork = results[13] == "否" ? false : true;
                    Res.RetireType = results[14];
                    Res.PensionLevel = results[15];
                    Res.HealthType = results[16];
                    Res.SpecialPaymentType = results[17];
                    Res.Bank = results[18];
                    Res.BankAddress = results[19];
                    if (!String.IsNullOrEmpty(results[20]))
                    {
                        Res.PaymentMonths = results[20].ToInt(0);
                    }
                    if (!String.IsNullOrEmpty(results[21]))
                    {
                        Res.OldPaymentMonths = results[21].ToInt(0);
                    }
                    Res.Phone = results[22];
                    Res.ZipCode = results[23];
                    Res.Address = results[24];
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion

                #region 第三步，查询总页数
                Url = "http://221.215.38.136/grcx/work/m01/f1203/oldQuery.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                #endregion

                #region 第四步，查询养老、医保、失业等缴费明细
                int page = CommonFun.GetMidStr(httpResult.Html, "共1/", "页").ToInt(1);
                int currpage = 1;

                List<string> result1 = new List<string>();
                List<string> result2 = new List<string>();
                List<string> result3 = new List<string>();
                do
                {
                    //养老金
                    Url = "http://221.215.38.136/grcx/work/m01/f1203/oldQuery.action?page_oldQuery=" + currpage;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    result1.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table/tbody/tr"));

                    //医保
                    Url = "http://221.215.38.136/grcx/work/m01/f1204/medicalQuery.action?page_medicalQuery=" + currpage;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    result2.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table/tbody/tr"));

                    //失业保险
                    Url = "http://221.215.38.136/grcx/work/m01/f1205/unemployQuery.action?page_unemployQuery=" + currpage;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    result3.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table/tbody/tr"));
                    currpage++;
                }
                while (page >= currpage);

                #endregion

                #region 整理养老保险、医疗保险、失业保险数据
                SocialSecurityDetailQueryRes detailRes = null;

                for (int i = 0; i < result1.Count; i++)
                {
                    var tdRow1 = HtmlParser.GetResultFromParser(result1[i], "//td");

                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;

                    detailRes.PayTime = tdRow1[1];
                    detailRes.SocialInsuranceTime = tdRow1[2];
                    detailRes.SocialInsuranceBase = tdRow1[5].ToDecimal(0);
                    if (tdRow1[4] == "正常应缴")
                    {
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    }
                    else
                    {
                        detailRes.PaymentType = tdRow1[4];
                        detailRes.PaymentFlag = tdRow1[9];
                    }

                    //养老
                    detailRes.PensionAmount = tdRow1[6].ToDecimal(0);
                    detailRes.CompanyPensionAmount = tdRow1[7].ToDecimal(0);
                    detailRes.NationPensionAmount = tdRow1[8].ToDecimal(0);


                    Res.Details.Add(detailRes);
                }

                for (int i = 0; i < result2.Count; i++)
                {
                    var tdRow2 = HtmlParser.GetResultFromParser(result2[i], "//td");
                    if (tdRow2.Count != 12)
                    {
                        continue;
                    }

                    detailRes = Res.Details.Where(o => o.SocialInsuranceTime == tdRow2[2]).FirstOrDefault();
                    if (detailRes != null)
                    {
                        //医疗
                        detailRes.MedicalAmount = tdRow2[6].ToDecimal(0);
                        detailRes.CompanyMedicalAmount = tdRow2[7].ToDecimal(0);
                        detailRes.EnterAccountMedicalAmount = tdRow2[8].ToDecimal(0);
                        detailRes.IllnessMedicalAmount = tdRow2[9].ToDecimal(0);
                    }
                    else
                    {
                        detailRes.PayTime = tdRow2[1];
                        detailRes.SocialInsuranceTime = tdRow2[2];
                        detailRes.SocialInsuranceBase = tdRow2[5].ToDecimal(0);

                        if (tdRow2[4] == "正常应缴")
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        }
                        else
                        {
                            detailRes.PaymentType = tdRow2[4];
                            detailRes.PaymentFlag = tdRow2[11];
                        }

                        //医疗
                        detailRes.MedicalAmount = tdRow2[6].ToDecimal(0);
                        detailRes.CompanyMedicalAmount = tdRow2[7].ToDecimal(0);
                        detailRes.EnterAccountMedicalAmount = tdRow2[8].ToDecimal(0);
                        detailRes.IllnessMedicalAmount = tdRow2[9].ToDecimal(0);

                        Res.Details.Add(detailRes);
                    }
                }

                for (int i = 0; i < result3.Count; i++)
                {
                    var tdRow3 = HtmlParser.GetResultFromParser(result3[i], "//td");
                    if (result3.Count != 10)
                    {
                        continue;
                    }

                    detailRes = Res.Details.Where(o => o.SocialInsuranceTime == tdRow3[2]).FirstOrDefault();
                    if (detailRes != null)
                    {
                        //失业
                        detailRes.UnemployAmount = tdRow3[6].ToDecimal(0);
                    }
                    else
                    {
                        detailRes.PayTime = tdRow3[1];
                        detailRes.SocialInsuranceTime = tdRow3[2];
                        detailRes.SocialInsuranceBase = tdRow3[5].ToDecimal(0);

                        if (tdRow3[4] == "正常应缴")
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        }
                        else
                        {
                            detailRes.PaymentType = tdRow3[4];
                            detailRes.PaymentFlag = tdRow3[9];
                        }

                        detailRes.PaymentType = tdRow3[4];
                        detailRes.SocialInsuranceBase = tdRow3[5].ToDecimal(0);
                        detailRes.PaymentFlag = tdRow3[11];

                        //失业
                        detailRes.UnemployAmount = tdRow3[6].ToDecimal(0);

                        Res.Details.Add(detailRes);
                    }
                }

                #endregion

                Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("青岛社保查询异常", e);
            }
            return Res;
        }
        #endregion

        #region 深圳
        /// <summary>
        /// 深圳社保初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Shenzhen_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "https://e.szsi.gov.cn/siservice/";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    SecurityProtocolType = SecurityProtocolType.Ssl3,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = httpResult.CookieCollection;
                string pid = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='pid']", "value")[0];

                Url = "https://e.szsi.gov.cn/siservice/PImages?pid=" + pid;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    SecurityProtocolType = SecurityProtocolType.Ssl3,
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
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.Number);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "深圳社保查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                //添加缓存
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("pid", pid);
                CacheHelper.SetCache(token, dics);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "深圳社保查询初始化异常";
                Log4netAdapter.WriteError("深圳社保查询初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 深圳社保登录、查询
        /// </summary>
        /// <param name="username">登录名</param>
        /// <param name="password">密码</param>
        /// <param name="token">会话token</param>
        /// <param name="vercode">验证码</param>
        /// <returns></returns>
        public SocialSecurityQueryRes Shenzhen_GetSocialSecurity(string username, string password, string token, string vercode)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            Res.SocialSecurityCity = "深圳";
            string Url = string.Empty;
            string postdata = string.Empty;
            string pid = string.Empty;
            int PaymentMonths = 0;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(token);
                    pid = dics["pid"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    CacheHelper.RemoveCache(token);
                }
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = "https://e.szsi.gov.cn/siservice/LoginAction.do";
                postdata = String.Format("Method=P&pid={3}&type=&AAC002={0}&CAC222={1}&PSINPUT={2}", username, password.ToBase64(), vercode, pid);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                if (httpResult.Html.IndexOf("请输入正确的身份证号码") != -1)
                {
                    string errorStr = CommonFun.GetMidStr(httpResult.Html, "<script language='JavaScript'>alert('", "')");

                    Res.StatusDescription = errorStr;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                pid = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='pid']", "value")[0];
                #endregion

                #region 第二步，查询基本信息
                Url = "https://e.szsi.gov.cn/siservice/serviceListAction.do?id=5&pid=" + pid;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                    //Res.EmployeeNo = results[1];
                    Res.IdentityCard = results[2];//身份证
                    Res.Sex = results[3];//性别
                    Res.BirthDate = results[4].ToTrim();//出生日期
                    Res.EmployeeNo = results[5];//社保号
                    Res.EmployeeStatus = results[10];//状态
                    Res.CompanyName = results[12];//公司名
                    Res.SocialInsuranceBase = results[13].Replace("元", "").ToDecimal(0);//缴费基数
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion

                SocialSecurityDetailQueryRes detailRes = null;

                #region 第三步，92年8月以后养老保险缴交明细
                Url = "https://e.szsi.gov.cn/siservice/transUrl.jsp?url=serviceListAction.do?id=1";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                pid = CommonFun.GetMidStr(httpResult.Html, "pid=", "\"");

                Url = "https://e.szsi.gov.cn/siservice/serviceListAction.do?id=1&pid=" + pid;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='two']/tbody/tr");

                for (int i = 0; i < results.Count; i++)
                {
                    var tdRow = HtmlParser.GetResultFromParser(results[i], "//td");
                    if (tdRow.Count != 8)
                    {
                        continue;
                    }
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;

                    detailRes.CompanyName = tdRow[0].ToTrim();
                    detailRes.PayTime = tdRow[1].ToTrim();
                    detailRes.SocialInsuranceTime = tdRow[1].ToTrim().ToTrim("-");
                    detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                    if (tdRow[7] == "已到帐")
                    {
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        PaymentMonths++;
                    }
                    else
                    {
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                        detailRes.PaymentFlag = tdRow[5];
                    }
                    //养老
                    detailRes.PensionAmount = tdRow[3].ToTrim().ToDecimal(0);
                    detailRes.CompanyPensionAmount = tdRow[4].ToTrim().ToDecimal(0);

                    Res.Details.Add(detailRes);
                }

                #endregion

                #region 第四步，医保
                Url = "https://e.szsi.gov.cn/siservice/transUrl.jsp?url=serviceListAction.do?id=2";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                pid = CommonFun.GetMidStr(httpResult.Html, "pid=", "\"");

                Url = "https://e.szsi.gov.cn/siservice/serviceListAction.do?id=2&pid=" + pid;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='two']/tbody/tr");

                for (int i = 0; i < results.Count; i++)
                {
                    var tdRow = HtmlParser.GetResultFromParser(results[i], "//td");
                    if (tdRow.Count != 8)
                    {
                        continue;
                    }
                    string SocialInsuranceTime = tdRow[1].ToTrim().ToTrim("-");
                    detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                    if (detailRes == null)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;

                        detailRes.CompanyName = tdRow[0].ToTrim();
                        detailRes.PayTime = tdRow[1].ToTrim();
                        detailRes.SocialInsuranceTime = SocialInsuranceTime;
                        detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                        if (tdRow[7].Contains("医保"))
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        }
                        else
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                            detailRes.PaymentFlag = tdRow[7];
                        }

                        //医疗
                        detailRes.MedicalAmount = tdRow[3].ToDecimal(0);
                        detailRes.CompanyMedicalAmount = tdRow[4].ToDecimal(0);

                        Res.Details.Add(detailRes);
                        PaymentMonths++;
                    }
                    else
                    {
                        //医疗
                        detailRes.MedicalAmount = tdRow[3].ToDecimal(0);
                        detailRes.CompanyMedicalAmount = tdRow[4].ToDecimal(0);
                    }

                }
                #endregion

                #region 第五步，失业
                Url = "https://e.szsi.gov.cn/siservice/transUrl.jsp?url=serviceListAction.do?id=7";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                pid = CommonFun.GetMidStr(httpResult.Html, "pid=", "\"");

                Url = "https://e.szsi.gov.cn/siservice/serviceListAction.do?id=7&pid=" + pid;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='two']/tbody/tr");

                for (int i = 0; i < results.Count; i++)
                {
                    var tdRow = HtmlParser.GetResultFromParser(results[i], "//td");
                    if (tdRow.Count != 6)
                    {
                        continue;
                    }
                    string SocialInsuranceTime = tdRow[1].ToTrim().ToTrim("-");
                    detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                    if (detailRes == null)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;

                        detailRes.CompanyName = tdRow[0].ToTrim();
                        detailRes.PayTime = tdRow[1].ToTrim();
                        detailRes.SocialInsuranceTime = SocialInsuranceTime;
                        detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                        if (tdRow[7] == "已到账")
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        }
                        else
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                            detailRes.PaymentFlag = tdRow[7];
                        }
                        //失业
                        detailRes.UnemployAmount = tdRow[3].ToDecimal(0);
                        Res.Details.Add(detailRes);
                        PaymentMonths++;
                    }
                    else
                    {
                        //失业
                        detailRes.UnemployAmount = tdRow[3].ToDecimal(0);
                    }

                }
                #endregion
                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("深圳社保查询异常", e);
            }
            return Res;
        }
        #endregion

        #region 北京
        /// <summary>
        /// 北京社保初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Beijing_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {

                Url = "http://www.bjrbj.gov.cn/csibiz/indinfo/validationCodeServlet.do";
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
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.Number);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "北京社保查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "北京社保查询初始化异常";
                Log4netAdapter.WriteError("北京社保查询初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 北京社保登录、查询
        /// </summary>
        /// <param name="username">登录名</param>
        /// <param name="password">密码</param>
        /// <param name="token">会话token</param>
        /// <param name="vercode">验证码</param>
        /// <returns></returns>
        public SocialSecurityQueryRes Beijing_GetSocialSecurity(string username, string password, string token, string vercode)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = "北京";
            string Url = string.Empty;
            string postdata = string.Empty;
            int PaymentMonths = 0;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = "http://www.bjrbj.gov.cn/csibiz/indinfo/login_check";
                postdata = String.Format("type=1&flag=3&j_username={0}&j_password={1}&safecode={2}&x=37&y=18", username, password, vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Expect100Continue = false,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//font[@color='red']", "text", true);
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，查询基本信息
                Url = "http://www.bjrbj.gov.cn/csibiz/indinfo/search/ind/indNewInfoSearchAction";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = "1=1",
                    CookieCollection = cookies,
                    Expect100Continue = false,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[position() >1]/td", "text", true);
                if (results.Count > 0)
                {
                    Res.Name = results[1];//姓名
                    Res.IdentityCard = results[3];//身份证
                    Res.Sex = results[5];//性别
                    Res.BirthDate = results[7];//出生日期
                    Res.Race = results[9];//民族
                    Res.WorkDate = results[15];//参加工作日期
                    Res.Address = results[25];//居住地址
                    Res.ZipCode = results[27];//邮编
                    Res.Phone = results[41];//手机
                    Res.SocialInsuranceBase = results[43].ToDecimal(0);//缴费基数
                    Res.EmployeeStatus = results[51];//
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion

                int thisYear = DateTime.Now.Year;
                int startYear = thisYear - 4;
                #region 第三步，养老
                results.Clear();
                for (int i = startYear; i <= thisYear; i++)
                {
                    Url = "http://www.bjrbj.gov.cn/csibiz/indinfo/search/ind/indPaySearchAction!oldage?searchYear=" + i;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = "1=1",
                        CookieCollection = cookies,
                        Expect100Continue = false,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[position() >4]", "", true));
                }
                for (int i = 0; i < results.Count; i++)
                {
                    var tdRow = HtmlParser.GetResultFromParser(results[i], "//td");
                    if (tdRow.Count != 12 || tdRow[1] == "-")
                    {
                        continue;
                    }

                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;

                    detailRes.PayTime = tdRow[1];
                    detailRes.SocialInsuranceTime = tdRow[1].ToTrim("-").Substring(0, 6);
                    detailRes.SocialInsuranceBase = tdRow[3].ToDecimal(0);
                    detailRes.CompanyName = tdRow[8].ToTrim();

                    if (tdRow[11] == "月报计帐")
                    {
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        PaymentMonths++;
                    }
                    else
                    {
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                        detailRes.PaymentFlag = tdRow[11];
                    }
                    //养老
                    detailRes.PensionAmount = tdRow[5].ToTrim().ToDecimal(0);
                    detailRes.CompanyPensionAmount = tdRow[4].ToTrim().ToDecimal(0);

                    Res.Details.Add(detailRes);
                }
                #endregion

                #region 第四步，失业
                results.Clear();
                for (int i = startYear; i <= thisYear; i++)
                {
                    Url = "http://www.bjrbj.gov.cn/csibiz/indinfo/search/ind/indPaySearchAction!unemployment?searchYear=" + i;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = "1=1",
                        CookieCollection = cookies,
                        Expect100Continue = false,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[position() >3]", "", true));
                }
                for (int i = 0; i < results.Count; i++)
                {
                    var tdRow = HtmlParser.GetResultFromParser(results[i], "//td");
                    if (tdRow.Count != 6 || tdRow[1] == "-")
                    {
                        continue;
                    }
                    string SocialInsuranceTime = tdRow[0].ToTrim("-").Substring(0, 6);
                    detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                    if (detailRes == null)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;

                        detailRes.PayTime = tdRow[0];
                        detailRes.SocialInsuranceTime = SocialInsuranceTime;
                        detailRes.SocialInsuranceBase = tdRow[3].ToDecimal(0);

                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;

                        //失业
                        detailRes.UnemployAmount = tdRow[5].ToTrim().ToDecimal(0);

                        Res.Details.Add(detailRes);
                        PaymentMonths++;
                    }
                    else
                    {
                        //失业
                        detailRes.UnemployAmount = tdRow[5].ToTrim().ToDecimal(0);
                    }
                }
                #endregion

                #region 第五步，医保
                results.Clear();
                for (int i = startYear; i <= thisYear; i++)
                {
                    Url = "http://www.bjrbj.gov.cn/csibiz/indinfo/search/ind/yiliaoUserPayAction!yiliaoPaySearch?date=" + i;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = "1=1",
                        CookieCollection = cookies,
                        Expect100Continue = false,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr[position() >3]", "", true));
                }
                for (int i = 0; i < results.Count; i++)
                {
                    var tdRow = HtmlParser.GetResultFromParser(results[i], "//td");
                    if (tdRow.Count != 10 || tdRow[1] == "-")
                    {
                        continue;
                    }
                    string SocialInsuranceTime = tdRow[0].ToTrim("-");
                    detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                    if (detailRes == null)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;

                        detailRes.PayTime = tdRow[0];
                        detailRes.SocialInsuranceTime = SocialInsuranceTime;
                        detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                        detailRes.CompanyName = tdRow[8].ToTrim();

                        if (tdRow[1] == "正常缴纳")
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        }
                        else
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                            detailRes.PaymentFlag = tdRow[11];
                        }

                        //医疗
                        detailRes.MedicalAmount = tdRow[4].ToDecimal(0);
                        detailRes.CompanyMedicalAmount = tdRow[3].ToDecimal(0);

                        Res.Details.Add(detailRes);
                        PaymentMonths++;
                    }
                    else
                    {
                        //医疗
                        detailRes.MedicalAmount = tdRow[4].ToDecimal(0);
                        detailRes.CompanyMedicalAmount = tdRow[3].ToDecimal(0);
                    }
                }
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("北京社保查询异常", e);
            }
            return Res;
        }
        #endregion

        #region 南京
        /// <summary>
        /// 南京社保初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Nanjing_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://wsbs.njhrss.gov.cn/NJLD/index.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://wsbs.njhrss.gov.cn/NJLD/Images";
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
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.Number);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "南京社保查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "南京社保查询初始化异常";
                Log4netAdapter.WriteError("南京社保查询初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 南京社保登录、查询
        /// </summary>
        /// <param name="username">登录名</param>
        /// <param name="password">密码</param>
        /// <param name="token">会话token</param>
        /// <param name="vercode">验证码</param>
        /// <returns></returns>
        public SocialSecurityQueryRes Nanjing_GetSocialSecurity(string username, string password, string token, string vercode)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = "南京";
            string Url = string.Empty;
            string postdata = string.Empty;
            int PaymentMonths = 0;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = "http://wsbs.njhrss.gov.cn/NJLD/LoginAction?act=CompanyLoginPerson";
                postdata = String.Format("u={0}&p={1}&lx=1&key={2}&dl=", username, password, vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                if (httpResult.Html.IndexOf("/NJLD/company/system/lesmain.jsp") == -1)
                {
                    string errorStr = CommonFun.GetMidStr(httpResult.Html, "<SCRIPT LANGUAGE=\"JavaScript\">alert(\"", "\")");
                    Res.StatusDescription = errorStr;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 基本信息
                Url = "http://wsbs.njhrss.gov.cn/NJLD/company/system/lesmainload.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table1']/tr/td", "", true);
                if (results.Count > 8)
                {
                    Res.EmployeeNo = results[1];
                    Res.Name = results[3];
                    Res.IdentityCard = results[5];
                    Res.CompanyName = results[7];
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion

                #region 第二步，养老
                Url = "http://wsbs.njhrss.gov.cn/NJLD/ZjGrJf?act=perform";
                postdata = "xz=1&hide=null&Submit=%E6%9F%A5%E8%AF%A2";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table1']/tr", "", true);
                for (int i = 0; i < results.Count; i++)
                {
                    var tdRow = HtmlParser.GetResultFromParser(results[i], "//td");
                    if (tdRow.Count != 8 || tdRow[0] != "养老")
                    {
                        continue;
                    }

                    detailRes = new SocialSecurityDetailQueryRes();

                    detailRes.PayTime = tdRow[2];
                    detailRes.SocialInsuranceTime = tdRow[3].ToTrim("-").Substring(0, 6);
                    detailRes.SocialInsuranceBase = tdRow[1].ToDecimal(0);

                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    //养老
                    detailRes.PensionAmount = tdRow[5].ToTrim().ToDecimal(0);
                    detailRes.CompanyPensionAmount = tdRow[4].ToTrim().ToDecimal(0);

                    Res.Details.Add(detailRes);
                    PaymentMonths++;
                }
                #endregion

                #region 第三步，医疗
                Url = "http://wsbs.njhrss.gov.cn/NJLD/ZjGrJf?act=perform";
                postdata = "xz=5&hide=null&Submit=%E6%9F%A5%E8%AF%A2";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table1']/tr", "", true);
                for (int i = 0; i < results.Count; i++)
                {
                    var tdRow = HtmlParser.GetResultFromParser(results[i], "//td");
                    if (tdRow.Count != 8 || tdRow[0] != "医疗")
                    {
                        continue;
                    }

                    string SocialInsuranceTime = tdRow[3].ToTrim("-").Substring(0, 6);
                    detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                    if (detailRes == null)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();

                        detailRes.PayTime = tdRow[2];
                        detailRes.SocialInsuranceTime = SocialInsuranceTime;
                        detailRes.SocialInsuranceBase = tdRow[1].ToDecimal(0);

                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;

                        //医疗
                        detailRes.MedicalAmount = tdRow[5].ToDecimal(0);
                        detailRes.CompanyMedicalAmount = tdRow[4].ToDecimal(0);

                        Res.Details.Add(detailRes);
                        PaymentMonths++;
                    }
                    else
                    {
                        //医疗
                        detailRes.MedicalAmount = tdRow[5].ToDecimal(0);
                        detailRes.CompanyMedicalAmount = tdRow[4].ToDecimal(0);
                    }
                }
                #endregion

                #region 第四步，失业
                Url = "http://wsbs.njhrss.gov.cn/NJLD/ZjGrJf?act=perform";
                postdata = "xz=4&hide=null&Submit=%E6%9F%A5%E8%AF%A2";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table1']/tr", "", true);
                for (int i = 0; i < results.Count; i++)
                {
                    var tdRow = HtmlParser.GetResultFromParser(results[i], "//td");
                    if (tdRow.Count != 8 || tdRow[0] != "失业")
                    {
                        continue;
                    }

                    string SocialInsuranceTime = tdRow[3].ToTrim("-").Substring(0, 6); ;
                    detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                    if (detailRes == null)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();

                        detailRes.PayTime = tdRow[2];
                        detailRes.SocialInsuranceTime = SocialInsuranceTime;
                        detailRes.SocialInsuranceBase = tdRow[1].ToDecimal(0);

                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;

                        //失业
                        detailRes.UnemployAmount = tdRow[5].ToTrim().ToDecimal(0);

                        Res.Details.Add(detailRes);
                        PaymentMonths++;
                    }
                    else
                    {
                        //失业
                        detailRes.UnemployAmount = tdRow[5].ToTrim().ToDecimal(0);
                    }
                }
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("南京社保查询异常", e);
            }
            return Res;
        }
        #endregion

        #region 广州
        /// <summary>
        /// 广州社保初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Guangzhou_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://gzlss.hrssgz.gov.cn/cas/login";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    SecurityProtocolType = SecurityProtocolType.Ssl3,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = httpResult.CookieCollection;
                string lt = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='lt']", "value")[0];

                Url = "http://gzlss.hrssgz.gov.cn/cas/captcha.jpg";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    SecurityProtocolType = SecurityProtocolType.Ssl3,
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
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.NumberAndUpper);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "广州社保查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                //添加缓存
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("lt", lt);
                CacheHelper.SetCache(token, dics);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "广州社保查询初始化异常";
                Log4netAdapter.WriteError("广州社保查询初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 广州社保登录、查询
        /// </summary>
        /// <param name="username">登录名</param>
        /// <param name="password">密码</param>
        /// <param name="token">会话token</param>
        /// <param name="vercode">验证码</param>
        /// <returns></returns>
        public SocialSecurityQueryRes Guangzhou_GetSocialSecurity(string username, string password, string token, string vercode)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            Res.SocialSecurityCity = "广州";
            string Url = string.Empty;
            string postdata = string.Empty;
            string lt = string.Empty;
            int PaymentMonths = 0;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(token);
                    lt = dics["lt"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    CacheHelper.RemoveCache(token);
                }
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = "http://gzlss.hrssgz.gov.cn/cas/login";
                postdata = String.Format("username={0}&password={1}&yzm={2}&usertype=2&lt={3}&_eventId=submit", username, password, vercode, lt);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header.Add("Accept-Language", "zh-cn,zh;");
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='*.errors']", "");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 其他步骤
                Url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/tomain/main.xhtml";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/tomain/main.xhtml";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/message/msg/refreshMsg.xhtml";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，查询基本信息
                Url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/getPersonInfoSearch.xhtml?querylog=true&businessocde=291QB-GRJCXX&visitterminal=PC-MENU";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                string aac001 = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='aac001']", "value")[0];
                //string csrftoken = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='csrftoken']", "value")[0];

                //Url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/realGetPersonInfoSearch.xhtml?querylog=true&businessocde=291QB-GRJCXX&visitterminal=PC";
                //postdata = "csrftoken=" + csrftoken + "&pd=&type=1";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "post",
                //    Postdata = postdata,
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);

                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[1]/tr/td", "text", true);

                //if (results.Count > 0)
                //{
                //    Res.IdentityCard = results[1];//身份证
                //    Res.Sex = results[3];//性别
                //    Res.BirthDate = results[5].ToTrim();//出生日期
                //    Res.EmployeeStatus = results[7];//状态
                //    Res.ZipCode = results[15];//状态
                //    Res.IsSpecialWork = results[19] == "是" ? true : false;
                //    Res.Address = results[25];//地址
                //    Res.Race = results[29];//民族
                //}
                //else
                //{
                //    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                #endregion

                SocialSecurityDetailQueryRes detailRes = null;

                #region 第三步，缴费历史明细表
                Url = string.Format("http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/anonview/viewPersonPayHistoryInfo.xhtml?aac001={0}&xzType=1&startStr=&endStr=&querylog=true&businessocde=291QB-GRJFLS&visitterminal=PC", aac001);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr");

                //个人信息
                List<string> infos = HtmlParser.GetResultFromParser(results[0] + results[1], "//span", "text", true);
                if (infos.Count > 0)
                {
                    Res.EmployeeNo = infos[0].ToTrim("&nbsp;").ToTrim("个人编号：");
                    Res.Name = infos[1].ToTrim("&nbsp;").ToTrim("姓名：");
                    Res.IdentityCard = infos[2].ToTrim("&nbsp;").ToTrim("证件号码：");
                    Res.CompanyName = infos[4].ToTrim("&nbsp;").ToTrim("现在单位名称:");
                }


                DateTime start = new DateTime();
                int monthCount = 0;
                for (int i = 5; i < results.Count - 4; i++)
                {
                    var tdRow = HtmlParser.GetResultFromParser(results[i], "//td");
                    if (tdRow.Count != 13 || tdRow[0] == "&nbsp;")
                    {
                        continue;
                    }

                    start = (DateTime)tdRow[0].ToDateTime(Consts.DateFormatString7);
                    monthCount = tdRow[2].ToInt(1);
                    for (int j = 0; j < monthCount; j++)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;

                        detailRes.CompanyName = tdRow[11].ToTrim();
                        detailRes.PayTime = start.ToString(Consts.DateFormatString2);
                        detailRes.SocialInsuranceTime = start.ToString(Consts.DateFormatString7);
                        detailRes.SocialInsuranceBase = tdRow[3].ToDecimal(0);
                        if (tdRow[12] == "正常")
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            PaymentMonths++;
                        }
                        else
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                            detailRes.PaymentFlag = tdRow[5];
                        }
                        //养老
                        detailRes.CompanyPensionAmount = tdRow[4].ToTrim().ToDecimal(0) / monthCount;
                        detailRes.PensionAmount = tdRow[5].ToTrim().ToDecimal(0) / monthCount;
                        //失业
                        detailRes.UnemployAmount = tdRow[7].ToTrim().ToDecimal(0) / monthCount;

                        Res.Details.Add(detailRes);

                        start = start.AddMonths(1);
                    }
                }
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("深圳社保查询异常", e);
            }
            return Res;
        }
        #endregion

        #region 重庆
        /// <summary>
        /// 重庆社保初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Chongqing_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://www.cqldbz.gov.cn:9001/ggfw/index.jsp";
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

                Url = "http://www.cqldbz.gov.cn:9001/ggfw/validateCodeBLH_image.do";
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


                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.Number);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "重庆社保查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "重庆社保查询初始化异常";
                Log4netAdapter.WriteError("重庆社保查询初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 重庆社保登录、查询
        /// </summary>
        /// <param name="username">登录名</param>
        /// <param name="password">密码</param>
        /// <param name="token">会话token</param>
        /// <param name="vercode">验证码</param>
        /// <returns></returns>
        public SocialSecurityQueryRes Chongqing_GetSocialSecurity(string username, string password, string token, string vercode)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = "重庆";
            string Url = string.Empty;
            string postdata = string.Empty;
            int PaymentMonths = 0;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，校验验证码
                Url = "http://www.cqldbz.gov.cn:8003/ggfw/validateCodeBLH_valid.do";
                postdata = "yzm=" + vercode;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string message = jsonParser.GetResultFromParser(httpResult.Html, "message");
                if (message != "验证成功!")
                {
                    Res.StatusDescription = message;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion

                #region 第一步，登录
                Url = "http://www.cqldbz.gov.cn:9001/ggfw/LoginBLH_login.do";
                postdata = String.Format("sfzh={0}&password={1}&validateCode={2}", username, password.ToBase64(), vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                message = jsonParser.GetResultFromParser(httpResult.Html, "message");
                if (message != "操作成功!")
                {
                    Res.StatusDescription = message;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，个人基本信息
                Url = "http://www.cqldbz.gov.cn:8003/ggfw/QueryBLH_main.do?code=000";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                Url = "http://www.cqldbz.gov.cn:8003/ggfw/QueryBLH_main.do?code=011";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                #region 解析基本信息数据
                //姓名
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@name='xm']", "", true);
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //单位编号
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@name='dwbh']", "", true);
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];
                }
                //个人编号
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@name='grbh']", "", true);
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];
                }
                //单位名称
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@name='dwmc']", "", true);
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                //身份证号
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@name='sfzh']", "", true);
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                //性别
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@name='xb']", "", true);
                if (results.Count > 0)
                {
                    Res.Sex = results[0];
                }
                //民族
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@name='mz']", "", true);
                if (results.Count > 0)
                {
                    Res.Race = results[0];
                }
                //出生日期
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@name='csrq']", "", true);
                if (results.Count > 0)
                {
                    Res.BirthDate = results[0];
                }
                #endregion
                #endregion


                int thisYear = DateTime.Now.Year;//当前年份
                int startYear = thisYear - 2;//开始统计年份
                int pageCount = 0;//总页数
                int currentPage = 0;//当前页数
                string data = string.Empty;//数据集
                string code = string.Empty;//返回码

                #region 第四步，养老

                for (int i = startYear; i <= thisYear; i++)
                {
                    currentPage = 0;
                    do
                    {
                        currentPage++;
                        Url = "http://www.cqldbz.gov.cn:8003/ggfw/QueryBLH_query.do";
                        postdata = "code=015&currentPage=" + currentPage + "&goPage=&year=" + i;
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "post",
                            Postdata = postdata,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);

                        code = jsonParser.GetResultFromMultiNode(httpResult.Html, "code");
                        if (code != "1")
                        {
                            break;
                        }
                        pageCount = jsonParser.GetResultFromMultiNode(httpResult.Html, "page:pageCount").ToInt(0);
                        currentPage = jsonParser.GetResultFromMultiNode(httpResult.Html, "page:currentPage").ToInt(0);
                        data = jsonParser.GetResultFromMultiNode(httpResult.Html, "result");

                        var details = jsonParser.DeserializeObject<List<ChongQingDetail>>(data);

                        foreach (var item in details)
                        {
                            detailRes = new SocialSecurityDetailQueryRes();

                            detailRes.PayTime = item.xssj;
                            detailRes.SocialInsuranceTime = item.dyfkssq;
                            detailRes.SocialInsuranceBase = item.jfjs.ToInt(0);
                            detailRes.CompanyName = item.dwmc;
                            detailRes.IdentityCard = item.sfzh;
                            detailRes.Name = item.xm;

                            if (item.jfbz == "已实缴")
                            {
                                detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                                detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                                PaymentMonths++;
                            }
                            else
                            {
                                detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                                detailRes.PaymentFlag = item.jfbz;
                            }
                            //养老
                            detailRes.PensionAmount = item.grjfje.ToDecimal(0);
                            detailRes.CompanyPensionAmount = item.dwjfje.ToDecimal(0);

                            Res.Details.Add(detailRes);
                        }

                    }
                    while (pageCount != currentPage);

                }
                #endregion

                #region 第五步，医疗

                //for (int i = startYear; i <= thisYear; i++)
                //{
                //    currentPage = 0;
                //    do
                //    {
                //        currentPage++;
                //        Url = "http://www.cqldbz.gov.cn:8003/ggfw/QueryBLH_query.do";
                //        postdata = "code=023&currentPage=" + currentPage + "&goPage=&year=" + i;
                //        httpItem = new HttpItem()
                //        {
                //            URL = Url,
                //            Method = "post",
                //            Postdata = postdata,
                //            CookieCollection = cookies,
                //            ResultCookieType = ResultCookieType.CookieCollection
                //        };
                //        httpResult = httpHelper.GetHtml(httpItem);

                //        code = jsonParser.GetResultFromMultiNode(httpResult.Html, "code");
                //        if (code != "1")
                //        {
                //            break;
                //        }
                //        pageCount = jsonParser.GetResultFromMultiNode(httpResult.Html, "page:pageCount").ToInt(0);
                //        currentPage = jsonParser.GetResultFromMultiNode(httpResult.Html, "page:currentPage").ToInt(0);
                //        data = jsonParser.GetResultFromMultiNode(httpResult.Html, "result");

                //        var details = jsonParser.DeserializeObject<List<ChongQingDetail>>(data);

                //        foreach (var item in details)
                //        {
                //            if (item.fkkm != "基本医疗保险")
                //            {
                //                continue;
                //            }
                //            detailRes = Res.Details.Where(o => o.SocialInsuranceTime == item.fkssq).FirstOrDefault();
                //            if (detailRes != null)
                //            {
                //                //医疗
                //                detailRes.MedicalAmount = item.grjfje.ToDecimal(0);
                //                detailRes.CompanyMedicalAmount = item.dwjfje.ToDecimal(0);
                //            }
                //            else
                //            {
                //                detailRes.PayTime = item.xssj;
                //                detailRes.SocialInsuranceTime = item.dyfkssq;
                //                detailRes.SocialInsuranceBase = item.jfjs.ToInt(0);
                //                detailRes.CompanyName = item.dwmc;
                //                detailRes.IdentityCard = item.sfzh;
                //                detailRes.Name = item.xm;

                //                if (item.jfbz == "已实缴")
                //                {
                //                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                //                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                //                    PaymentMonths++;
                //                }
                //                else
                //                {
                //                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                //                    detailRes.PaymentFlag = item.jfbz;
                //                }
                //                //医疗
                //                detailRes.MedicalAmount = item.grjfje.ToDecimal(0);
                //                detailRes.CompanyMedicalAmount = item.dwjfje.ToDecimal(0);

                //                Res.Details.Add(detailRes);
                //            }
                //        }

                //    }
                //    while (pageCount == currentPage);

                //}
                #endregion

                #region 第六步，失业

                //for (int i = startYear; i <= thisYear; i++)
                //{
                //    currentPage = 0;
                //    do
                //    {
                //        currentPage++;
                //        Url = "http://www.cqldbz.gov.cn:8003/ggfw/QueryBLH_query.do";
                //        postdata = "code=043&currentPage=" + currentPage + "&goPage=&year=" + i;
                //        httpItem = new HttpItem()
                //        {
                //            URL = Url,
                //            Method = "post",
                //            Postdata = postdata,
                //            CookieCollection = cookies,
                //            ResultCookieType = ResultCookieType.CookieCollection
                //        };
                //        httpResult = httpHelper.GetHtml(httpItem);
                //        code = jsonParser.GetResultFromMultiNode(httpResult.Html, "code");
                //        if (code != "1")
                //        {
                //            break;
                //        }
                //        pageCount = jsonParser.GetResultFromMultiNode(httpResult.Html, "page:pageCount").ToInt(0);
                //        currentPage = jsonParser.GetResultFromMultiNode(httpResult.Html, "page:currentPage").ToInt(0);
                //        data = jsonParser.GetResultFromMultiNode(httpResult.Html, "result");

                //        var details = jsonParser.DeserializeObject<List<ChongQingDetail>>(data);

                //        foreach (var item in details)
                //        {
                //            detailRes = Res.Details.Where(o => o.SocialInsuranceTime == item.fkssq).FirstOrDefault();
                //            if (detailRes != null)
                //            {
                //                //失业
                //                detailRes.UnemployAmount = item.grjfje.ToDecimal(0);
                //            }
                //            else
                //            {
                //                detailRes.PayTime = item.xssj;
                //                detailRes.SocialInsuranceTime = item.dyfkssq;
                //                detailRes.SocialInsuranceBase = item.jfjs.ToInt(0);
                //                detailRes.CompanyName = item.dwmc;
                //                detailRes.IdentityCard = item.sfzh;

                //                if (item.jfbz == "足额缴费")
                //                {
                //                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                //                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                //                    PaymentMonths++;
                //                }
                //                else
                //                {
                //                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                //                    detailRes.PaymentFlag = item.jfbz;
                //                }
                //                //失业
                //                detailRes.UnemployAmount = item.grjfje.ToDecimal(0);

                //                Res.Details.Add(detailRes);
                //            }
                //        }

                //    }
                //    while (pageCount == currentPage);

                //}
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("重庆社保查询异常", e);
            }
            return Res;
        }
        #endregion

        #region 厦门
        /// <summary>
        /// 厦门社保初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Xiamen_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "https://app.xmhrss.gov.cn/wcm/servlet/FirstServlet";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    SecurityProtocolType = SecurityProtocolType.Tls,
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

                Url = "https://app.xmhrss.gov.cn/wcm/servlet/VCodeServlet";
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


                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.Number);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "厦门社保查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "厦门社保查询初始化异常";
                Log4netAdapter.WriteError("厦门社保查询初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 厦门社保登录、查询
        /// </summary>
        /// <param name="username">登录名</param>
        /// <param name="password">密码</param>
        /// <param name="token">会话token</param>
        /// <param name="vercode">验证码</param>
        /// <returns></returns>
        public SocialSecurityQueryRes Xiamen_GetSocialSecurity(string username, string password, string token, string vercode)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = "厦门";
            string Url = string.Empty;
            string postdata = string.Empty;
            int PaymentMonths = 0;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，校验验证码
                Url = "https://app.xmhrss.gov.cn/wcm/ChangeYzm?self=loginIndex&vcode=" + vercode;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = "2=2",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string message = jsonParser.GetResultFromParser(httpResult.Html, "msg");
                if (message != "校验码成功")
                {
                    Res.StatusDescription = message;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion

                #region 第二步，登录
                Url = "https://app.xmhrss.gov.cn/wcm/servlet/WebLoginServlet?self=loginIndex";
                postdata = String.Format("id0000={0}&userpwd={1}&vcode={2}", username, password, vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK || httpResult.Html.Contains("社会保障号或密码错误"))
                {
                    Res.StatusDescription = "社会保障号或密码错误";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第三步，个人基本信息
                Url = "https://app.xmhrss.gov.cn/wcm/servlet/PersonalServlet";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html[1]/body[1]/table[1]/tr[1]/td[2]/table[1]/tr[1]/td[4]/div[5]/table[1]/tr/td[@align='left']", "text", true);

                if (results.Count > 0)
                {
                    Res.Name = results[0].ToTrim("&nbsp;");//姓名
                    Res.IdentityCard = results[1].ToTrim("&nbsp;");//身份证
                    Res.EmployeeNo = results[2].ToTrim("&nbsp;");//社会保障卡卡号
                    Res.CompanyName = results[4].ToTrim("&nbsp;");//单位名称
                    Res.CompanyNo = results[5].ToTrim("&nbsp;");//单位编号
                    Res.CompanyType = results[6].ToTrim("&nbsp;");
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion


                int currentPage = 0;//当前页数
                string starttime = "200001";//查询开始月份
                string endtime = DateTime.Now.ToString(Consts.DateFormatString7);//查询开始月份

                #region 第四步，养老

                Url = "https://app.xmhrss.gov.cn/wcm/servlet/PaymentInfoServlet";
                postdata = string.Format("qsnyue={0}&jznyue={1}&xzdm00=2&zmlx00=", starttime, endtime);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (!httpResult.Html.Contains("对不起，没有你需要查找的信息"))
                {
                    currentPage = 1;
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tableData']/tr[position()>1]", "");
                    do
                    {
                        currentPage++;
                        Url = "https://app.xmhrss.gov.cn/wcm/servlet/PaymentInfoServlet?page=" + currentPage;
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "post",
                            Postdata = postdata,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tableData']/tr[position()>1]", ""));

                    }
                    while (httpResult.Html.Contains("对不起，没有你需要查找的信息"));
                }

                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                    if (tdRow.Count != 10)
                    {
                        continue;
                    }

                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;

                    detailRes.PayTime = tdRow[3].ToTrim("&nbsp;");
                    detailRes.SocialInsuranceTime = tdRow[3].ToTrim("&nbsp;");
                    detailRes.SocialInsuranceBase = tdRow[8].ToTrim("&nbsp;").ToDecimal(0);
                    if (tdRow[9].ToTrim("&nbsp;") == "已缴费")
                    {
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        PaymentMonths++;
                    }
                    else
                    {
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                        detailRes.PaymentFlag = tdRow[9].ToTrim("&nbsp;");
                    }
                    //养老
                    detailRes.PensionAmount = tdRow[7].ToTrim("&nbsp;").ToDecimal(0);
                    detailRes.CompanyPensionAmount = tdRow[6].ToTrim("&nbsp;").ToDecimal(0) - detailRes.PensionAmount;

                    Res.Details.Add(detailRes);
                }

                #endregion

                #region 第五步，医疗

                Url = "https://app.xmhrss.gov.cn/wcm/servlet/PaymentInfoServlet";
                postdata = string.Format("qsnyue={0}&jznyue={1}&xzdm00=1&zmlx00=", starttime, endtime);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (!httpResult.Html.Contains("对不起，没有你需要查找的信息"))
                {
                    currentPage = 1;
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tableData']/tr[position()>1]", "");
                    do
                    {
                        currentPage++;
                        Url = "https://app.xmhrss.gov.cn/wcm/servlet/PaymentInfoServlet?page=" + currentPage;
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "post",
                            Postdata = postdata,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tableData']/tr[position()>1]", ""));
                    }
                    while (httpResult.Html.Contains("对不起，没有你需要查找的信息"));
                }

                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                    if (tdRow.Count != 10)
                    {
                        continue;
                    }
                    string SocialInsuranceTime = tdRow[3].ToTrim("&nbsp;");
                    detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                    if (detailRes != null)
                    {
                        decimal SocialInsuranceBase = tdRow[8].ToTrim("&nbsp;").ToDecimal(0);
                        if (SocialInsuranceBase > detailRes.SocialInsuranceBase)
                        {
                            detailRes.SocialInsuranceBase = SocialInsuranceBase;
                        }
                        //医疗
                        detailRes.MedicalAmount = tdRow[7].ToTrim("&nbsp;").ToDecimal(0);
                        detailRes.CompanyMedicalAmount = tdRow[6].ToTrim("&nbsp;").ToDecimal(0) - detailRes.PensionAmount;
                    }
                    else
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;

                        detailRes.PayTime = tdRow[3].ToTrim("&nbsp;");
                        detailRes.SocialInsuranceTime = tdRow[3].ToTrim("&nbsp;");
                        detailRes.SocialInsuranceBase = tdRow[8].ToTrim("&nbsp;").ToDecimal(0);
                        if (tdRow[9].ToTrim("&nbsp;") == "已缴费")
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            PaymentMonths++;
                        }
                        else
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                            detailRes.PaymentFlag = tdRow[9].ToTrim("&nbsp;");
                        }
                        //医疗
                        detailRes.MedicalAmount = tdRow[7].ToTrim("&nbsp;").ToDecimal(0);
                        detailRes.CompanyMedicalAmount = tdRow[6].ToTrim("&nbsp;").ToDecimal(0) - detailRes.PensionAmount;

                        Res.Details.Add(detailRes);
                    }
                }

                #endregion

                #region 第六步，失业

                Url = "https://app.xmhrss.gov.cn/wcm/servlet/PaymentInfoServlet";
                postdata = string.Format("qsnyue={0}&jznyue={1}&xzdm00=4&zmlx00=", starttime, endtime);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (!httpResult.Html.Contains("对不起，没有你需要查找的信息"))
                {
                    currentPage = 1;
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tableData']/tr[position()>1]", "");
                    do
                    {
                        currentPage++;
                        Url = "https://app.xmhrss.gov.cn/wcm/servlet/PaymentInfoServlet?page=" + currentPage;
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "post",
                            Postdata = postdata,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='tableData']/tr[position()>1]", ""));
                    }
                    while (httpResult.Html.Contains("对不起，没有你需要查找的信息"));
                }

                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                    if (tdRow.Count != 10)
                    {
                        continue;
                    }
                    string SocialInsuranceTime = tdRow[3].ToTrim("&nbsp;");
                    detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                    if (detailRes != null)
                    {
                        //失业
                        detailRes.UnemployAmount = tdRow[6].ToTrim("&nbsp;").ToDecimal(0);
                    }
                    else
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;

                        detailRes.PayTime = tdRow[3].ToTrim("&nbsp;");
                        detailRes.SocialInsuranceTime = tdRow[3].ToTrim("&nbsp;");
                        detailRes.SocialInsuranceBase = tdRow[8].ToTrim("&nbsp;").ToDecimal(0);
                        if (tdRow[9].ToTrim("&nbsp;") == "已缴费")
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            PaymentMonths++;
                        }
                        else
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                            detailRes.PaymentFlag = tdRow[9].ToTrim("&nbsp;");
                        }
                        //失业
                        detailRes.UnemployAmount = tdRow[6].ToTrim("&nbsp;").ToDecimal(0);

                        Res.Details.Add(detailRes);
                    }
                }

                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("厦门社保查询异常", e);
            }
            return Res;
        }
        #endregion

        #region 宁波
        /// <summary>
        /// 宁波社保登录、查询
        /// </summary>
        /// <param name="username">登录名</param>
        /// <param name="password">密码</param>
        /// <param name="token">会话token</param>
        /// <param name="vercode">验证码</param>
        /// <returns></returns>
        public SocialSecurityQueryRes Ningbo_GetSocialSecurity(string username, string password)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = "宁波";
            string Url = string.Empty;
            string postdata = string.Empty;
            string responseString = string.Empty;
            byte[] responseData;
            int PaymentMonths = 0;
            List<string> results = new List<string>();
            HttpClient httpClient = new HttpClient();

            try
            {
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                System.Net.ServicePointManager.Expect100Continue = false;

                #region 第一步，登录
                Url = "http://www.nbhrss.gov.cn/sbxm/MainServlet?cmd=enterpriseloginbyjquery";
                postdata = String.Format("iscode={0}&password={1}", username, password);
                httpClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                responseData = httpClient.UploadData(Url, "POST", Encoding.UTF8.GetBytes(postdata));//得到返回字符流  
                responseString = Encoding.UTF8.GetString(responseData);//解码  

                if (responseString == "1")
                {
                    Res.StatusDescription = "账号或密码错误";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion

                #region 第三步，个人基本信息
                Url = "http://ybxm.nbhrss.gov.cn/outweb-xssb/xssb/outwebgrybxx/queryYbxx.action";
                postdata = String.Format("sfzh={0}&text=", username);
                httpClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                httpClient.Headers.Add("Referer", "http://ybxm.nbhrss.gov.cn/outweb-xssb/ybxx.jsp?sfzh=" + username);
                responseData = httpClient.UploadData(Url, "POST", Encoding.UTF8.GetBytes(postdata));//得到返回字符流  
                responseString = Encoding.UTF8.GetString(responseData);//解码 
                results = HtmlParser.GetResultFromParser(responseString, "/html/body/table/tr/td/table/tr/td/div/div/table[3]/tr[1]/td[4]", "text", true);

                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证号
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(responseString, "/html/body/table/tr/td/table/tr/td/div/div/table[3]/tr[1]/td[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                #endregion

                #region 第四步，养老

                Url = "http://www.nbhrss.gov.cn/sbxm/sebao/lss01.jsp?sig=0";
                responseData = httpClient.DownloadData(Url);//得到返回字符流  
                responseString = Encoding.UTF8.GetString(responseData);//解码 

                Res.Payment_State = CommonFun.GetMidStr(responseString, "当前参保状态：", "</td>");

                results = HtmlParser.GetResultFromParser(responseString, "/html/body/table/tr/td/table/tr[1]/td/div[1]/table/tr/td/table[5]/tr/td/table[2]/tr[position()>1]", "inner", true);

                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                    if (tdRow.Count != 6)
                    {
                        continue;
                    }

                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;

                    detailRes.PayTime = tdRow[0];
                    detailRes.SocialInsuranceTime = tdRow[0];
                    detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    PaymentMonths++;


                    //养老
                    detailRes.PensionAmount = tdRow[5].ToDecimal(0);

                    Res.Details.Add(detailRes);
                }

                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(Res.SocialSecurityCity + "社保查询异常", e);
            }
            return Res;
        }
        #endregion

        #region 杭州

        /// <summary>
        /// 杭州社保初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Hangzhou_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            string responseString = string.Empty;
            byte[] responseData;
            HttpClient httpClient = new HttpClient();
            try
            {
                Url = "http://wsbs.zjhz.hrss.gov.cn/captcha.svl"; 
                responseData = httpClient.DownloadData(Url);//得到返回字符流  

                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, responseData);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "杭州社保查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, httpClient);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "杭州社保查询初始化异常";
                Log4netAdapter.WriteError("杭州社保查询初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 杭州社保登录、查询
        /// </summary>
        /// <param name="username">登录名</param>
        /// <param name="password">密码</param>
        /// <param name="token">会话token</param>
        /// <param name="vercode">验证码</param>
        /// <returns></returns>
        public SocialSecurityQueryRes Hangzhou_GetSocialSecurity(string username, string password, string token, string vercode)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = "杭州";
            string Url = string.Empty;
            string postdata = string.Empty;
            string responseString = string.Empty;
            byte[] responseData;
            int PaymentMonths = 0;
            List<string> results = new List<string>();
            HttpClient httpClient = new HttpClient();

            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    httpClient = (HttpClient)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录



                Url = "http://wsbs.zjhz.hrss.gov.cn/loginvalidate.html?logintype=2&captcha=" + vercode;
                postdata = String.Format("type=01&persontype=01&account={0}%40hz.cn&password={1}&captcha1={2}", username, password, vercode);
                httpClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                responseData = httpClient.UploadData(Url, "POST", Encoding.UTF8.GetBytes(postdata));//得到返回字符流  
                responseString = Encoding.UTF8.GetString(responseData);//解码  

                responseString = CommonFun.GetMidStr(responseString, "['", "']");
                if (responseString != "success")
                {
                    Res.StatusDescription = "账号或密码错误";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion

                #region 第二步，个人基本信息
                Url = "http://wsbs.zjhz.hrss.gov.cn/person/personInfo/index.html";
                responseData = httpClient.DownloadData(Url);//得到返回字符流  
                responseString = Encoding.UTF8.GetString(responseData);//解码 
                results = HtmlParser.GetResultFromParser(responseString, "/html/body/table/tr/td/table[2]/tr/td/table/tr[1]/td[2]", "text", true);

                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(responseString, "/html/body/table/tr/td/table[2]/tr/td/table/tr[1]/td[4]", "text", true);
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证号
                }
                results = HtmlParser.GetResultFromParser(responseString, "/html/body/table/tr/td/table[2]/tr/td/table/tr[2]/td[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];//个人编号
                }
                results = HtmlParser.GetResultFromParser(responseString, "/html/body/table/tr/td/table[2]/tr/td/table/tr[2]/td[4]", "text", true);
                if (results.Count > 0)
                {
                    Res.Sex = results[0];//性别
                }
                results = HtmlParser.GetResultFromParser(responseString, "/html/body/table/tr/td/table[2]/tr/td/table/tr[3]/td[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.Race = results[0];//民族
                }
                results = HtmlParser.GetResultFromParser(responseString, "/html/body/table/tr/td/table[2]/tr/td/table/tr[3]/td[4]", "text", true);
                if (results.Count > 0)
                {
                    Res.BirthDate = results[0];//出生日期

                }
                #endregion

                #region 第三步，养老

                int pageNo = 1;
                bool isFinish = false;
                int payMonth = 0;
                string payYear = string.Empty;
                decimal insuranceBase = 0;
                do
                {
                    Url = "http://wsbs.zjhz.hrss.gov.cn/person/ylgrzhQuery/index.html";
                    postdata = String.Format("pageNo={0}&message=", pageNo);
                    httpClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    responseData = httpClient.UploadData(Url, "POST", Encoding.UTF8.GetBytes(postdata));//得到返回字符流  
                    responseString = Encoding.UTF8.GetString(responseData);//解码  

                    results = HtmlParser.GetResultFromParser(responseString, "//table[@class='grid']/tr[position()>4]", "inner", true);

                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                        if (tdRow.Count != 21 || tdRow[0] == "&nbsp;")
                        {
                            isFinish = true;
                            continue;
                        }
                        if (tdRow[3] == "0&nbsp;")
                        {
                            continue;
                        }
                        payYear = tdRow[0].ToTrim("&nbsp;");
                        payMonth = tdRow[3].ToTrim("&nbsp;").ToInt(0);
                        insuranceBase = tdRow[2].ToTrim("&nbsp;").ToDecimal(0) / payMonth;
                        PaymentMonths += payMonth;
                        for (int i = 1; i <= payMonth; i++)
                        {
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.Name = Res.Name;
                            detailRes.IdentityCard = Res.IdentityCard;

                            if (i < 10)
                            {
                                detailRes.PayTime = payYear + "0" + i;
                                detailRes.SocialInsuranceTime = payYear + "0" + i;
                            }
                            else
                            {
                                detailRes.PayTime = payYear + i;
                                detailRes.SocialInsuranceTime = payYear + i;
                            }

                            detailRes.SocialInsuranceBase = insuranceBase;
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;

                            //养老
                            detailRes.PensionAmount = tdRow[5].ToTrim("&nbsp;").ToDecimal(0);
                            detailRes.CompanyPensionAmount = tdRow[6].ToTrim("&nbsp;").ToDecimal(0);

                            Res.Details.Add(detailRes);
                        }
                    }
                    pageNo++;
                }
                while (!isFinish);

                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(Res.SocialSecurityCity + "社保查询异常", e);
            }
            return Res;
        }
        #endregion

        #region 无锡
        /// <summary>
        /// 无锡社保初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Wuxi_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://ggfw.wxhrss.gov.cn/captcha.svl";
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


                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.Character);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "无锡社保查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "无锡社保查询初始化异常";
                Log4netAdapter.WriteError("无锡社保查询初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 无锡社保登录、查询
        /// </summary>
        /// <param name="username">登录名</param>
        /// <param name="password">密码</param>
        /// <param name="token">会话token</param>
        /// <param name="vercode">验证码</param>
        /// <returns></returns>
        public SocialSecurityQueryRes Wuxi_GetSocialSecurity(string username, string password, string token, string vercode)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = "无锡";
            string Url = string.Empty;
            string postdata = string.Empty;
            int PaymentMonths = 0;
            List<string> results = new List<string>();
            decimal rate = (decimal)0.08;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第二步，登录
                Url = "http://ggfw.wxhrss.gov.cn/personloginvalidate.html";
                postdata = String.Format("account={0}&password={1}&type=1&captcha={2}", username, password, vercode);
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
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                string message = CommonFun.GetMidStr(httpResult.Html, "['", "']");
                if (message != "success")
                {
                    if (message == "wrongaccount")
                    {
                        Res.StatusDescription = "用户名不存在";
                    }
                    if (message == "wrongpass")
                    {
                        Res.StatusDescription = "密码和用户名不匹配";
                    }
                    if (message == "captchawrong")
                    {
                        Res.StatusDescription = "验证码错误";
                    }
                    if (message == "captchaexpire")
                    {
                        Res.StatusDescription = "验证码过期";
                    }
                    if (message == "isLocked")
                    {
                        Res.StatusDescription = "该账号已被冻结";
                    }
                    if (message == "noPersonId")
                    {
                        Res.StatusDescription = "该账号已经终止参保，不允许登录";
                    }
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第三步，个人基本信息
                Url = "http://ggfw.wxhrss.gov.cn/person/personBaseInfo.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[2]/li[1]", "text", true);
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];//编号
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[2]/li[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[2]/li[3]", "text", true);
                if (results.Count > 0)
                {
                    Res.BirthDate = results[0];//出生日期
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[2]/li[4]", "text", true);
                if (results.Count > 0)
                {
                    Res.Sex = results[0];//性别
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[2]/li[6]", "text", true);
                if (results.Count > 0)
                {
                    Res.EmployeeStatus = results[0];//状态
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[2]/li[9]", "text", true);
                if (results.Count > 0)
                {
                    Res.WorkDate = results[0];//工作日期
                }



                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[4]/li[1]", "text", true);
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[4]/li[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.Race = results[0];//民族
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[4]/li[19]", "text", true);
                if (results.Count > 0)
                {
                    Res.Phone = results[0];//手机号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/div[3]/div/div/ul[6]/li[1]", "text", true);
                if (results.Count > 0)
                {
                    Res.Address = results[0];//居住地
                }
                #endregion

                #region 第四步，养老

                Url = "http://ggfw.wxhrss.gov.cn/person/personMedCountInfo.html";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/form/div/div[1]/div/dl[2]/dd", "");
                var results1 = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[3]/div[2]/ul[2]/form/div/div[1]/div/dl[4]/dd", "");

                string thisyear = DateTime.Now.Year.ToString();
                int month = 0;
                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i] == thisyear)
                    {
                        month = DateTime.Now.Month;
                    }
                    else
                    {
                        month = 12;
                    }

                    for (int m = 1; m <= month; m++)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;
                        if (m < 10)
                        {
                            detailRes.PayTime = results[i] + "0" + m;
                            detailRes.SocialInsuranceTime = results[i] + "0" + m;
                        }
                        else
                        {
                            detailRes.PayTime = results[i] + m;
                            detailRes.SocialInsuranceTime = results[i] + m;
                        }
                        detailRes.CompanyMedicalAmount = results1[i].ToDecimal(0) / month;
                        detailRes.SocialInsuranceBase = detailRes.CompanyMedicalAmount / rate;
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        Res.Details.Add(detailRes);
                    }
                }

                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("厦门社保查询异常", e);
            }
            return Res;
        }
        #endregion

        #region 苏州
        /// <summary>
        /// 苏州社保初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Suzhou_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://www.szsbzx.net.cn:9900/web/website/rand.action";
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


                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.Number);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "苏州社保查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "苏州社保查询初始化异常";
                Log4netAdapter.WriteError("苏州社保查询初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 苏州社保登录、查询
        /// </summary>
        /// <param name="username">登录名</param>
        /// <param name="password">密码</param>
        /// <param name="token">会话token</param>
        /// <param name="vercode">验证码</param>
        /// <returns></returns>
        public SocialSecurityQueryRes Suzhou_GetSocialSecurity(string username, string password, string token, string vercode)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = "苏州";
            string Url = string.Empty;
            string postdata = string.Empty;
            int PaymentMonths = 0;
            List<string> results = new List<string>();
            decimal rate = (decimal)0.08;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = "http://www.szsbzx.net.cn:9900/web/website/indexProcess?frameControlSubmitFunction=checkLogin";
                postdata = String.Format("grbh={0}&sfzh={1}&yzcode={2}", username, password, vercode);
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
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                string message = jsonParser.GetResultFromParser(httpResult.Html, "errormsg");
                if (!message.IsEmpty())
                {
                    Res.StatusDescription = message;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，个人基本信息
                Url = "http://www.szsbzx.net.cn:9900/web/website/personQuery/personQueryAction.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='personNum']", "value", true);
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];//编号
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='name']", "value", true);
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='sfzNum']", "value", true);
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='danweiNum']", "value", true);
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];//性别
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='danweiSite']", "value", true);
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];//状态
                }
                #endregion

                int pageIndex = 0;
                int pageCount = 0;
                string pagelistajax = string.Empty;
                #region 第三步，养老

                do
                {
                    pageIndex++;
                    Url = "http://www.szsbzx.net.cn:9900/web/website/personQuery/personQueryAction?frameControlSubmitFunction=getPagesAjax";
                    postdata = string.Format("xz=qyylmx&pageIndex={0}&pageCount=20", pageIndex, pageCount);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    pageCount = CommonFun.GetMidStr(httpResult.Html, "第" + pageIndex + "\\/", "页").ToInt(0);
                    pagelistajax = jsonParser.GetResultFromParser(httpResult.Html, "pagelistajax");

                    results = HtmlParser.GetResultFromParser(pagelistajax, "//table/tr[position()>1]");

                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 9)
                        {
                            continue;
                        }

                        detailRes = new SocialSecurityDetailQueryRes();

                        detailRes.PayTime = tdRow[7].ToTrim();
                        detailRes.SocialInsuranceTime = tdRow[0].ToTrim();
                        detailRes.SocialInsuranceBase = tdRow[4].ToDecimal(0);
                        detailRes.CompanyName = tdRow[1];
                        if (tdRow[6].ToTrim() == "正常应缴")
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        }
                        else
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                        }

                        //养老
                        detailRes.PensionAmount = tdRow[3].ToTrim().ToDecimal(0);
                        //detailRes.CompanyPensionAmount = tdRow[4].ToTrim().ToDecimal(0);

                        Res.Details.Add(detailRes);
                        PaymentMonths++;
                    }
                }
                while (pageIndex < pageCount);

                #endregion

                #region 第四步，医疗
                pageIndex = 0;
                do
                {
                    pageIndex++;
                    Url = "http://www.szsbzx.net.cn:9900/web/website/personQuery/personQueryAction?frameControlSubmitFunction=getPagesAjax";
                    postdata = string.Format("xz=ylbx&pageIndex={0}&pageCount=20", pageIndex, pageCount);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    pageCount = CommonFun.GetMidStr(httpResult.Html, "第" + pageIndex + "\\/", "页").ToInt(0);
                    pagelistajax = jsonParser.GetResultFromParser(httpResult.Html, "pagelistajax");

                    results = HtmlParser.GetResultFromParser(pagelistajax, "//table/tr[position()>1]");

                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 9)
                        {
                            continue;
                        }
                        string SocialInsuranceTime = tdRow[0];
                        detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                        if (detailRes == null)
                        {
                            detailRes = new SocialSecurityDetailQueryRes();

                            detailRes.PayTime = tdRow[0].ToTrim();
                            detailRes.SocialInsuranceTime = tdRow[0].ToTrim();
                            detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                            detailRes.CompanyName = tdRow[1];
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;

                            //医疗
                            detailRes.MedicalAmount = tdRow[3].ToDecimal(0);
                            detailRes.CompanyMedicalAmount = tdRow[4].ToDecimal(0);

                            Res.Details.Add(detailRes);
                            PaymentMonths++;
                        }
                        else
                        {
                            //医疗
                            detailRes.MedicalAmount = tdRow[4].ToDecimal(0);
                            detailRes.CompanyMedicalAmount = tdRow[3].ToDecimal(0);
                        }
                    }
                }
                while (pageIndex < pageCount);

                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("厦门社保查询异常", e);
            }
            return Res;
        }
        #endregion

        #region 大连

        /// <summary>
        /// 大连社保登录、查询
        /// </summary>
        /// <param name="username">登录名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public SocialSecurityQueryRes Dalian_GetSocialSecurity(string username, string password)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = "大连";
            string Url = string.Empty;
            string postdata = string.Empty;
            int PaymentMonths = 0;
            List<string> results = new List<string>();
            decimal rate = (decimal)0.08;
            try
            {
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                postdata = String.Format("code={0}&IdCard={1}&type=0&P_Type=0", username, password);
                Url = "http://www.dl12333.gov.cn/_layouts/LssbwebSite/A04/A0402/List_QY_Tab.aspx?" + postdata;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
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

                string message = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "');window.open");
                if (!message.IsEmpty())
                {
                    Res.StatusDescription = message;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，个人基本信息
                postdata = string.Format("AACOO1={0}&AAC002={1}", username, password);
                Url = "http://www.dl12333.gov.cn/_layouts/LssbwebSite/A04/A0402/A040253/List.aspx?" + postdata;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//label[@id='AAC001']", "text", true);
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];//编号
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//label[@id='AAC003']", "text", true);
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//label[@id='AAC002']", "text", true);
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//label[@id='CKC090']", "text", true);
                if (results.Count > 0)
                {
                    Res.EmployeeStatus = results[0];//账户状态
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//label[@id='AAB004']", "text", true);
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];//公司名称
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//label[@id='AKC087']", "text", true);
                if (results.Count > 0)
                {
                    Res.PersonalInsuranceTotal = results[0].ToDecimal(0);//个人帐号余额
                }
                #endregion

                #region 第三步，养老

                postdata = string.Format("AACOO1={0}&AAC002={1}", username, password);
                Url = "http://www.dl12333.gov.cn/_layouts/LssbwebSite/A04/A0402/A040272/List_JF.aspx?" + postdata;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                int pageIndex = 0;
                int pageCount = CommonFun.GetMidStr(httpResult.Html, "页 共 ", " 页").ToInt(0);
                string pagelistajax = string.Empty;
                do
                {
                    string viewState = string.Empty;
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value", true);
                    if (results.Count > 0)
                    {
                        viewState = results[0];
                    }
                    pageIndex++;
                    postdata = string.Format("AACOO1={0}&AAC002={1}", username, password);
                    Url = "http://www.dl12333.gov.cn/_layouts/LssbwebSite/A04/A0402/A040272/List_JF.aspx?" + postdata;

                    postdata = string.Format("__VIEWSTATE={0}&__EVENTTARGET={1}&__EVENTARGUMENT={2}", viewState.ToUrlEncode(), "A040272_JF_pager", pageIndex);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='lssb_sub_Table_01']/tr[position()>2]", "inner", true);

                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 11)
                        {
                            continue;
                        }

                        detailRes = new SocialSecurityDetailQueryRes();

                        detailRes.PayTime = tdRow[10].ToTrim("&nbsp;");
                        detailRes.SocialInsuranceTime = tdRow[0].ToTrim("&nbsp;") + tdRow[1].ToTrim("&nbsp;");
                        detailRes.SocialInsuranceBase = tdRow[3].ToTrim("&nbsp;").ToDecimal(0);
                        if (tdRow[7].ToTrim("&nbsp;") == "正常缴费")
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            PaymentMonths++;
                        }
                        else
                        {
                            detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                            detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                        }
                        detailRes.PensionAmount = tdRow[6].ToTrim("&nbsp;").ToDecimal(0);

                        Res.Details.Add(detailRes);
                    }
                }
                while (pageIndex < pageCount);

                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("大连社保查询异常", e);
            }
            return Res;
        }
        #endregion

        #region 福州
        /// <summary>
        /// 福州社保初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Fuzhou_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://www.fzshbx.org/img.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
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

                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.Number);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "福州社保查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "苏州社保查询初始化异常";
                Log4netAdapter.WriteError("苏州社保查询初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 福州社保登录、查询
        /// </summary>
        /// <param name="username">登录名</param>
        /// <param name="password">密码</param>
        /// <param name="token">会话token</param>
        /// <param name="vercode">验证码</param>
        /// <returns></returns>
        public SocialSecurityQueryRes Fuzhou_GetSocialSecurity(string username, string password, string token, string vercode)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = "福州";
            string Url = string.Empty;
            string postdata = string.Empty;
            int PaymentMonths = 0;
            List<string> results = new List<string>();
            decimal rate = (decimal)0.08;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = "http://www.fzshbx.org/sb/user/userLogin.do";
                postdata = String.Format("sbUser.username={0}&sbUser.password={1}&sbUser.yzm={2}", username, password, vercode);
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
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string message = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "');location.href");
                if (!message.IsEmpty())
                {
                    Res.StatusDescription = message;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，个人基本信息
                Url = "http://www.fzshbx.org/xxcx/grjbxxcx.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='xinxi']/table/tr", "inner", true);
                Res.EmployeeNo = HtmlParser.GetResultFromParser(results[1], "//td")[1];//编号
                Res.Name = HtmlParser.GetResultFromParser(results[0], "//td")[3];//姓名
                Res.IdentityCard = HtmlParser.GetResultFromParser(results[0], "//td")[1];//身份证号
                Res.EmployeeStatus = HtmlParser.GetResultFromParser(results[1], "//td")[3];//状态
                Res.CompanyName = HtmlParser.GetResultFromParser(results[4], "//td")[1];//公司名称
                Res.Phone = HtmlParser.GetResultFromParser(results[3], "//td")[1];//电话
                #endregion

                #region 第三步，养老

                int pageIndex = 0;
                int pageCount = 0;
                string pagelistajax = string.Empty;
                do
                {
                    pageIndex++;
                    Url = "http://www.fzshbx.org/xxcx/grjfmxcx.do";
                    postdata = string.Format("ac10a.starttime=&ac10a.endtime=&temp=&page={0}&sppagetotal=10&length=10", pageIndex);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    pageCount = CommonFun.GetMidStr(httpResult.Html, "行，", " 页").ToInt(0);

                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='grid']/tbody/tr", "inner", true);

                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                        if (tdRow.Count != 12)
                        {
                            continue;
                        }

                        detailRes = new SocialSecurityDetailQueryRes();

                        detailRes.PayTime = tdRow[0];
                        detailRes.SocialInsuranceTime = tdRow[1];
                        detailRes.SocialInsuranceBase = tdRow[6].ToDecimal(0);
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detailRes.PensionAmount = tdRow[7].ToDecimal(0);
                        PaymentMonths++;

                        Res.Details.Add(detailRes);
                        PaymentMonths++;
                    }
                }
                while (pageIndex < pageCount);

                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("福州社保查询异常", e);
            }
            return Res;
        }
        #endregion

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
