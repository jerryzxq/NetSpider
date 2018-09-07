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
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.Common.Constants;
using System.Collections;
using Vcredit.NetSpider.DataAccess.Cache;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.CQ
{
    public class chongqing : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://218.201.89.115:9001/";//http://www.cqldbz.gov.cn:9001/
        string socialCity = "cq_chongqing";
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
                Url = baseUrl + "ggfw/index1.jsp";//ggfw/index.jsp
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

                Url = baseUrl + "ggfw/validateCodeBLH_image.do";
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

                #region 第一步，校验验证码
                Url = baseUrl + "ggfw/validateCodeBLH_valid.do";
                postdata = "yzm=" + socialReq.Vercode;
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
                Url = baseUrl + "ggfw/LoginBLH_login.do";
                postdata = String.Format("sfzh={0}&password={1}&validateCode={2}", socialReq.Identitycard, socialReq.Password.ToBase64(), socialReq.Vercode);
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
                string typeCode = jsonParser.GetResultFromParser(httpResult.Html, "code");//1 職工參保，2居民參保
                switch (typeCode)
                {
                    case "1":
                        typeCode = "000";
                        break;
                    case "2":
                        typeCode = "999";
                        break;
                    default://不確定其它類型，暫定為000,可能導致獲取社保失敗
                        typeCode = "000";
                        break;
                }
                #endregion

                #region 第二步，个人基本信息

                Url = baseUrl + "ggfw/QueryBLH_main.do?code=" + typeCode;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='basicInfoTable']/tr/td");
                //if (results.Count > 0)
                //{
                //    Res.Name = results[1] == "" ? null : results[1];
                //    Res.EmployeeNo = results[3] == "" ? null : results[3];
                //    Res.Sex = results[5] == "" ? null : results[5];
                //    Res.Race = results[7] == "" ? null : results[7];
                //    Res.IdentityCard = results[9] == "" ? null : results[9];
                //    Res.CompanyNo = results[11] == "" ? null : results[11];
                //    try
                //    {
                //        Res.BirthDate = results[13] == "" ? null : Res.BirthDate = DateTime.Parse(results[13]).ToString("yyyy-MM-dd");
                //    }
                //    catch { }
                //}
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@name='s_dwmc']");
                //if(results.Count > 0)
                //    Res.CompanyName = results[0] == "" ? null : results[0];
                //姓名
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='td_xm']", "", true);
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                //个人编号
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='td_grbh']", "", true);
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];
                }
                //性别
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='td_xb']", "", true);
                if (results.Count > 0)
                {
                    Res.Sex = results[0];
                }
                //身份证号
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='td_sfzh']", "", true);
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                //出生日期
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='td_csrq']", "", true);
                if (results.Count > 0)
                {
                    try
                    {
                        Res.BirthDate = DateTime.Parse(results[0]).ToString("yyyy-MM-dd");
                    }
                    catch { }
                }
                //戶口所在地
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='td_hjszd']", "", true);
                if (results.Count > 0)
                {
                    Res.Address = results[0];
                }
                //单位编号
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='td_szdwbh']", "", true);
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];
                }
                //单位名称
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@name='s_dwmc']", "", true);
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                List<string> hrefResults = HtmlParser.GetResultFromParser(httpResult.Html, "//ul[@class='menu-item-ul']/li/a", "href");
                if (hrefResults.Count > 0)
                {
                    Url = baseUrl + hrefResults[0];
                }
                else
                {
                    Url = baseUrl + "ggfw/QueryBLH_main.do?code=011";
                }
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
                if (results.Count > 0 && Res.Name == null)
                {
                    Res.Name = results[0];
                }
                //单位编号
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@name='dwbh']", "", true);
                if (results.Count > 0 && Res.CompanyNo == null)
                {
                    Res.CompanyNo = results[0];
                }
                //个人编号
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@name='grbh']", "", true);
                if (results.Count > 0 && Res.EmployeeNo == null)
                {
                    Res.EmployeeNo = results[0];
                }
                //单位名称
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@name='dwmc']", "", true);
                if (results.Count > 0 && Res.CompanyName == null)
                {
                    Res.CompanyName = results[0];
                }
                //身份证号
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@name='sfzh']", "", true);
                if (results.Count > 0 && Res.IdentityCard == null)
                {
                    Res.IdentityCard = results[0];
                }
                //性别
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@name='xb']", "", true);
                if (results.Count > 0 && Res.Sex == null)
                {
                    Res.Sex = results[0];
                }
                //民族
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@name='mz']", "", true);
                if (results.Count > 0 && Res.Race == null)
                {
                    Res.Race = results[0];
                }
                //出生日期
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@name='csrq']", "", true);
                if (results.Count > 0 && Res.BirthDate == null)
                {
                    try
                    {
                        Res.BirthDate = DateTime.Parse(results[0]).ToString("yyyy-MM-dd");
                    }
                    catch { }
                }

                var json = CommonFun.GetMidStr(httpResult.Html, "list\":[", "]},");
                if (json != "")
                    Res.WorkDate = jsonParser.GetResultFromParser(json, "cjgzsj");
                #endregion
                #endregion


                int thisYear = DateTime.Now.Year;//当前年份
                int startYear = thisYear - 4;//开始统计年份
                int pageCount = 0;//总页数
                int currentPage = 0;//当前页数
                string data = string.Empty;//数据集
                string code = string.Empty;//返回码

                #region 第四步，养老

                if (hrefResults.Count > 0)
                {
                    Url = baseUrl + hrefResults[1];
                }
                else
                {
                    Url = baseUrl + "ggfw/QueryBLH_main.do?code=014";
                }
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                string jsonlist = CommonFun.GetMidStr(httpResult.Html, "div name=\"ZrarPageData\"  style='display:none' data='", "'></div>");

                string yanglao = jsonParser.GetResultFromParser(jsonlist, "json");
                List<string> yanglaolist = jsonParser.GetArrayFromParse(yanglao, "list");
                foreach (string Yearyanglao in yanglaolist)
                {
                    PaymentMonths += jsonParser.GetResultFromParser(Yearyanglao, "dnjfys").ToInt(0);
                }

                currentPage = jsonParser.GetResultFromMultiNode(yanglao, "page:currentPage").ToInt(0);
                pageCount = jsonParser.GetResultFromMultiNode(yanglao, "page:pageCount").ToInt(0);
                while (pageCount != currentPage)
                {
                    currentPage++;
                    Url = "http://www.cqldbz.gov.cn:9001/ggfw/QueryBLH_query.do";
                    postdata = "code=014&currentPage=" + currentPage + "&goPage=";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);


                    //jsonlist = CommonFun.GetMidStr(httpResult.Html, "div name=\"ZrarPageData\"  style='display:none' data='", "'></div>");

                    //yanglao = jsonParser.GetResultFromParser(jsonlist, "json");
                    yanglaolist = jsonParser.GetArrayFromParse(httpResult.Html, "result");
                    if (results.Count > 0)
                        foreach (string Yearyanglao in yanglaolist)
                        {
                            PaymentMonths += jsonParser.GetResultFromParser(Yearyanglao, "dnjfys").ToInt(0);
                        }

                }

                for (int i = startYear; i <= thisYear; i++)
                {
                    currentPage = 0;
                    do
                    {
                        currentPage++;
                        Url = baseUrl + "ggfw/QueryBLH_query.do";
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
                                //PaymentMonths++;
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
                //        Url = baseUrl+"ggfw/QueryBLH_query.do";
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
                //        Url = baseUrl+"ggfw/QueryBLH_query.do";
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
    internal class ChongQingDetail
    {
        public string cbd { get; set; }
        public string dwbh { get; set; }
        public string dwmc { get; set; }
        public string dyfkssq { get; set; }
        public string grbh { get; set; }
        public string grjfje { get; set; }
        public string jfbz { get; set; }
        public string jfjs { get; set; }
        public string sfzh { get; set; }
        public string xm { get; set; }
        public string xssj { get; set; }
        public string dwjfje { get; set; }
        public string fkkm { get; set; }
        public string fkssq { get; set; }
    }


}
