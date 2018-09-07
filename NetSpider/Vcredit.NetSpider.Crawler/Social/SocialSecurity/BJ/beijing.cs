using System;
using System.Collections;
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
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.Common.Constants;
using Vcredit.NetSpider.DataAccess.Cache;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.BJ
{
    public class beijing : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "";
        string socialCity = "bj_beijing";
        #endregion

        #region 私有变量
        enum InfoType
        {
            养老保险,
            医疗保险,
            失业保险,
            工伤保险,
            生育保险
        }

        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息

        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, new string[] { "oldage" });
            PageHash.Add(InfoType.医疗保险, new string[] { "medicalcare" });
            PageHash.Add(InfoType.失业保险, new string[] { "unemployment" });
            PageHash.Add(InfoType.生育保险, new string[] { "maternity" });
            PageHash.Add(InfoType.工伤保险, new string[] { "injuries" });
        }

        void GetAllDetail(InfoType Type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            SocialSecurityDetailQueryRes detailRes = new SocialSecurityDetailQueryRes();

            int thisYear = DateTime.Now.Year;
            int startYear = thisYear - 4;
            for (int i = startYear; i <= thisYear; i++)
            {
                Url = string.Format("http://www.bjrbj.gov.cn/csibiz/indinfo/search/ind/indPaySearchAction!{0}?searchYear={1}&time={2}",((string[])PageHash[Type])[0], i, CommonFun.GetTimeStamp());
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
                results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[1]/tr[position() >2]", "", true));
            }
            string LastTime = string.Empty;
            for (int i = 0; i < results.Count; i++)
            {
                var tdRow = HtmlParser.GetResultFromParser(results[i], "//td");
                switch (Type)
                {
                    case InfoType.养老保险:
                        if (tdRow.Count != 7 || tdRow[1] == "-")
                        {
                            continue;
                        }
                        break;
                    case InfoType.医疗保险:
                        if ((tdRow.Count != 7 && tdRow.Count != 6) || tdRow[1] == "-")
                        {
                            continue;
                        }
                        break;
                    case InfoType.失业保险:
                        if (tdRow.Count != 4|| tdRow[1] == "-")
                        {
                            continue;
                        }
                        break;
                    case InfoType.生育保险:
                        if (tdRow.Count != 3 || tdRow[1] == "-")
                        {
                            continue;
                        }
                        break;
                    case InfoType.工伤保险:
                        if (tdRow.Count != 3 || tdRow[1] == "-")
                        {
                            continue;
                        }
                        break;
                }
                
                

                string SocialInsuranceTime = string.Empty;


                if (Type == InfoType.医疗保险)
                {
                    if (tdRow.Count == 6)
                    {
                        if (LastTime.IsEmpty())
                            continue;

                        SocialInsuranceTime = LastTime;
                        tdRow.Insert(0, SocialInsuranceTime);
                    }
                    else
                    {
                        SocialInsuranceTime = tdRow[0].ToTrim("-");
                        LastTime = SocialInsuranceTime;
                    }
                }
                else
                {
                    SocialInsuranceTime = tdRow[0].ToTrim("-").Substring(0, 6);
                }

                detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();

                bool NeedAddNew = false;
                if (detailRes == null)
                {
                    detailRes = new SocialSecurityDetailQueryRes();
                    NeedAddNew = true;
                }
                switch (Type)
                {
                    case InfoType.养老保险:
                        detailRes.PensionAmount = tdRow[4].ToTrim().ToDecimal(0);
                        detailRes.CompanyPensionAmount = tdRow[3].ToTrim().ToDecimal(0);
                        break;
                    case InfoType.医疗保险:
                        detailRes.MedicalAmount += tdRow[4].ToDecimal(0);
                        detailRes.CompanyMedicalAmount += tdRow[3].ToDecimal(0);
                        break;
                    case InfoType.失业保险:
                        detailRes.UnemployAmount = tdRow[2].ToTrim().ToDecimal(0) + tdRow[3].ToTrim().ToDecimal(0);
                        break;
                    case InfoType.生育保险:
                        detailRes.MaternityAmount = tdRow[2].ToTrim().ToDecimal(0);
                        break;
                    case InfoType.工伤保险:
                        detailRes.EmploymentInjuryAmount = tdRow[2].ToTrim().ToDecimal(0);
                        break;
                }

                if (NeedAddNew)
                {
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;

                    detailRes.PayTime = tdRow[0].ToTrim("-").Substring(0, 6);
                    detailRes.SocialInsuranceTime = SocialInsuranceTime;
                    detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                    if (tdRow.Count > 5)
                    {
                        detailRes.CompanyName = tdRow[5].ToTrim();
                    }

                    if (Type == InfoType.养老保险 || Type == InfoType.医疗保险)
                    {
                        detailRes.PaymentType = tdRow[1] == "正常缴纳" || tdRow[1] == "正常缴费" ? ServiceConsts.SocialSecurity_PaymentType_Normal : tdRow[1];
                    }
                    else
                    {
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    }
                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;

                    Res.Details.Add(detailRes);
                }
            }
        }
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://www.bjrbj.gov.cn/csibiz/indinfo/login.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (httpResult.Html.Contains("在此期间系统暂停服务"))
                {
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@colspan='2']");
                    if (results.Count > 0)
                    {
                        Res.StatusDescription = CommonFun.ClearFlag(results[0].ToTrim().ToTrim("<br/>").ToTrim("<br>"));
                    }
                    else
                    {
                        Res.StatusDescription = "系统暂停服务";
                    }
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

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
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);

                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                SpiderCacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitError;
                Log4netAdapter.WriteError(socialCity + ServiceConsts.SocialSecurity_InitError, e);
            }
            return Res;
        }

        public SocialSecurityQueryRes SocialSecurityQuery(SocialSecurityReq socialReq)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = socialCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(socialReq.Token);
                    SpiderCacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = "http://www.bjrbj.gov.cn/csibiz/indinfo/login_check";
                postdata = String.Format("type=1&flag=3&j_username={0}&j_password={1}&safecode={2}&x=37&y=18", socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
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
                Res.CompanyName = CommonFun.GetMidStr(httpResult.Html, "单位名称：", "组织机构代码：").ToTrim("&nbsp;").ToTrim();
                Res.CompanyNo = CommonFun.GetMidStr(httpResult.Html, "组织机构代码：", "社会保险登记证编号：").ToTrim("&nbsp;").ToTrim();

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[position() >1]/td", "text", true);
                if (results.Count > 0)
                {
                    Res.Name = results[1];//姓名
                    Res.IdentityCard = results[3];//身份证
                    Res.Sex = results[5];//性别
                    Res.BirthDate = results[7];//出生日期
                    Res.Race = results[9];//民族
                    Res.District = results[11];//地区
                    Res.WorkDate = results[15];//参加工作日期
                    Res.Address = results[25];//居住地址
                    Res.ZipCode = results[27];//邮编
                    Res.Phone = results[41] == "" ? results[39] : results[41];//手机
                    //Res.SocialInsuranceBase = results[43].ToDecimal(0);//缴费基数
                    Res.EmployeeStatus = results[51];//
                    Res.RetireType = results[53];
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion

                
                #region 第三步,获取详细信息
                InitPageHash();
                foreach (InfoType type in Enum.GetValues(typeof(InfoType)))
                {
                    GetAllDetail(type, ref Res);
                }
                #endregion
                //int thisYear = DateTime.Now.Year;
                //int startYear = thisYear - 4;
                //#region 第三步，养老
                //results.Clear();
                //for (int i = startYear; i <= thisYear; i++)
                //{
                //    Url = string.Format("http://www.bjrbj.gov.cn/csibiz/indinfo/search/ind/indPaySearchAction!oldage?searchYear={0}&time={1}" , i, CommonFun.GetTimeStamp());
                //    httpItem = new HttpItem()
                //    {
                //        URL = Url,
                //        Method = "post",
                //        Postdata = "1=1",
                //        CookieCollection = cookies,
                //        Expect100Continue = false,
                //        ResultCookieType = ResultCookieType.CookieCollection
                //    };
                //    httpResult = httpHelper.GetHtml(httpItem);
                //    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[1]/tr[position() >2]", "", true));
                //}
                //for (int i = 0; i < results.Count; i++)
                //{
                //    var tdRow = HtmlParser.GetResultFromParser(results[i], "//td");
                //    if (tdRow.Count != 7 || tdRow[1] == "-")
                //    {
                //        continue;
                //    }

                //    detailRes = new SocialSecurityDetailQueryRes();
                //    detailRes.Name = Res.Name;
                //    detailRes.IdentityCard = Res.IdentityCard;

                //    detailRes.PayTime = tdRow[0].ToTrim("-").Substring(0, 6);
                //    detailRes.SocialInsuranceTime = tdRow[0].ToTrim("-").Substring(0, 6);
                //    detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                //    detailRes.CompanyName = tdRow[5].ToTrim();

                //    detailRes.PaymentType = tdRow[1] == "正常缴费" ? ServiceConsts.SocialSecurity_PaymentType_Normal : tdRow[1];
                //    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;

                //    PaymentMonths++;
                //    //养老
                //    detailRes.PensionAmount = tdRow[4].ToTrim().ToDecimal(0);
                //    detailRes.CompanyPensionAmount = tdRow[3].ToTrim().ToDecimal(0);

                //    Res.Details.Add(detailRes);
                //}
                //#endregion

                //#region 第四步，失业
                //results.Clear();
                //for (int i = startYear; i <= thisYear; i++)
                //{
                //    Url = string.Format("http://www.bjrbj.gov.cn/csibiz/indinfo/search/ind/indPaySearchAction!unemployment?searchYear={0}&time={1}", i, CommonFun.GetTimeStamp());
                //    httpItem = new HttpItem()
                //    {
                //        URL = Url,
                //        Method = "post",
                //        Postdata = "1=1",
                //        CookieCollection = cookies,
                //        Expect100Continue = false,
                //        ResultCookieType = ResultCookieType.CookieCollection
                //    };
                //    httpResult = httpHelper.GetHtml(httpItem);
                //    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[1]/tr[position() >2]", "", true));
                //}
                //for (int i = 0; i < results.Count; i++)
                //{
                //    var tdRow = HtmlParser.GetResultFromParser(results[i], "//td");
                //    if (tdRow.Count != 6 || tdRow[1] == "-")
                //    {
                //        continue;
                //    }
                //    string SocialInsuranceTime = tdRow[0].ToTrim("-").Substring(0, 6);
                //    detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                //    if (detailRes == null)
                //    {
                //        detailRes = new SocialSecurityDetailQueryRes();
                //        detailRes.Name = Res.Name;
                //        detailRes.IdentityCard = Res.IdentityCard;

                //        detailRes.PayTime = SocialInsuranceTime;
                //        detailRes.SocialInsuranceTime = SocialInsuranceTime;
                //        detailRes.SocialInsuranceBase = tdRow[1].ToDecimal(0);

                //        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                //        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;

                //        //失业
                //        detailRes.UnemployAmount = tdRow[2].ToTrim().ToDecimal(0) + tdRow[3].ToTrim().ToDecimal(0);

                //        Res.Details.Add(detailRes);
                //        PaymentMonths++;
                //    }
                //    else
                //    {
                //        //失业
                //        detailRes.UnemployAmount = tdRow[2].ToTrim().ToDecimal(0) + tdRow[3].ToTrim().ToDecimal(0);
                //    }
                //}
                //#endregion

                //#region 第五步，医保
                //results.Clear();
                //for (int i = startYear; i <= thisYear; i++)
                //{
                //    Url = string.Format("http://www.bjrbj.gov.cn/csibiz/indinfo/search/ind/indPaySearchAction!medicalcare?searchYear={0}&time={1}", i, CommonFun.GetTimeStamp());
                //    httpItem = new HttpItem()
                //    {
                //        URL = Url,
                //        Method = "post",
                //        Postdata = "1=1",
                //        CookieCollection = cookies,
                //        Expect100Continue = false,
                //        ResultCookieType = ResultCookieType.CookieCollection
                //    };
                //    httpResult = httpHelper.GetHtml(httpItem);
                //    results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr[position() >2]", "", true));
                //}
                //string LastTime = string.Empty;
                //for (int i = 0; i < results.Count; i++)
                //{
                //    var tdRow = HtmlParser.GetResultFromParser(results[i], "//td");
                //    if ((tdRow.Count != 7 && tdRow.Count != 6) || tdRow[1] == "-" )
                //    {
                //        continue;
                //    }

                //    string SocialInsuranceTime = string.Empty;
                //    if (tdRow.Count == 6)
                //    {
                //        if (LastTime.IsEmpty())
                //            continue;

                //        SocialInsuranceTime = LastTime;
                //        tdRow.Insert(0, SocialInsuranceTime);
                //    }
                //    else
                //    {
                //        SocialInsuranceTime = tdRow[0].ToTrim("-");
                //        LastTime = SocialInsuranceTime;
                //    }
                //    detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                //    if (detailRes == null)
                //    {
                //        detailRes = new SocialSecurityDetailQueryRes();
                //        detailRes.Name = Res.Name;
                //        detailRes.IdentityCard = Res.IdentityCard;

                //        detailRes.PayTime = tdRow[0].ToTrim("-").Substring(0, 6);
                //        detailRes.SocialInsuranceTime = tdRow[0].ToTrim("-").Substring(0, 6);
                //        detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                //        detailRes.CompanyName = tdRow[5].ToTrim();

                //        detailRes.PaymentType = tdRow[1] == "正常缴费" ? ServiceConsts.SocialSecurity_PaymentType_Normal : tdRow[1];
                //        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;

                //        //医疗
                //        detailRes.MedicalAmount = tdRow[4].ToDecimal(0);
                //        detailRes.CompanyMedicalAmount = tdRow[3].ToDecimal(0);

                //        Res.Details.Add(detailRes);
                //        PaymentMonths++;
                //    }
                //    else
                //    {
                //        //医疗
                //        detailRes.MedicalAmount += tdRow[4].ToDecimal(0);
                //        detailRes.CompanyMedicalAmount += tdRow[3].ToDecimal(0);
                //    }
                //}
                //#endregion

                //Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(socialCity + ServiceConsts.SocialSecurity_QueryError, e);
            }
            return Res;
        }
    }

}
