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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.ZJ
{
    public class dongyang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://app1.dyrls.gov.cn:8095/sionlineman/";
        string socialCity = "zj_dongyang";
        #endregion
        #region 私有变量

        //类型查询优先级数组
        private readonly List<string> SortType = new List<string>() { "企业养老保险", "基本医疗保险", "失业保险", "工伤保险", "生育保险", "大病补助医疗保险", "公务员补助医疗保险" };
        Dictionary<string, string> PageDic = new Dictionary<string, string>();//参加的险种类型
        /// <summary>
        /// 查询缴费明细
        /// </summary>
        /// <param name="type">明细类型</param>
        /// <param name="year">查询年份</param>
        /// <param name="Res"></param>
        private void GetAllDetail(KeyValuePair<string, string> type, int year, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            List<string> results = new List<string>();
            List<string> saveResults = new List<string>();
            while (true)
            {
                Url = baseUrl + string.Format("pages/socialinsurance/person_detail_wx.jsp?psseno={0}&qqx={1}&bbf={2}&name={3}", Res.EmployeeNo, type.Value, year, type.Key);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@border='1']/tr[position()>1]", "",true);
                if (results.Count == 0) break;
                saveResults.AddRange(results);
                year--;
            }
            foreach (string item in saveResults)
            {
                var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text");
                if (tdRow.Count != 7) continue;
                SocialSecurityDetailQueryRes detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == tdRow[0]);
                bool needSave = false;
                if (detailRes == null)
                {
                    needSave = true;
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;
                    detailRes.CompanyName = tdRow[1].Trim();
                    detailRes.PayTime = detailRes.SocialInsuranceTime = tdRow[0].Trim();
                    detailRes.SocialInsuranceBase = tdRow[2].Trim().ToDecimal(0);
                    detailRes.CompanyPensionAmount = tdRow[3].Trim().ToDecimal(0);
                    detailRes.PensionAmount = tdRow[4].Trim().ToDecimal(0);
                    detailRes.PaymentFlag = tdRow[6].Trim() == "已到帐" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : ServiceConsts.SocialSecurity_PaymentType_Adjust;
                    detailRes.PaymentType = tdRow[6].Trim();
                }
                switch (type.Key)
                {
                    case "企业养老保险":
                        detailRes.PensionAmount += tdRow[4].ToDecimal(0);
                        detailRes.CompanyPensionAmount += tdRow[3].ToDecimal(0);
                        break;
                    case "基本医疗保险":
                        detailRes.MedicalAmount += tdRow[4].ToDecimal(0);
                        detailRes.CompanyMedicalAmount += tdRow[3].ToDecimal(0);
                        break;
                    case "失业保险":
                        detailRes.UnemployAmount += (tdRow[4].ToDecimal(0) + tdRow[3].ToDecimal(0));
                        break;
                    case "工伤保险":
                        detailRes.EmploymentInjuryAmount += tdRow[3].ToDecimal(0);
                        break;
                    case "生育保险":
                        detailRes.MaternityAmount += tdRow[3].ToDecimal(0);
                        break;
                    case "大病补助医疗保险":
                        detailRes.IllnessMedicalAmount += tdRow[3].ToDecimal(0);
                        break;
                    case "公务员补助医疗保险":
                        detailRes.CivilServantMedicalAmount += tdRow[3].ToDecimal(0);
                        break;
                }
                detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : tdRow[2].Trim().ToDecimal(0);
                if (!needSave) continue;
                Res.Details.Add(detailRes);
            }
        }
        #endregion
        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "verifyCode";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl,
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
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                CacheHelper.SetCache(token, cookies);
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
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Vercode.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登录

                Url = baseUrl + "logonAction.do";
                postdata = String.Format("username={0}&name={1}&passwd={2}&verifycode={3}&submitbtn.x=26&submitbtn.y=9&params={{'username':'{0}','password':'{2}','name':'{1}','scene':'sce:USER_PASS;USERNAME;NAME'}}",
                    socialReq.Identitycard, socialReq.Name, CommonFun.GetMd5Str(socialReq.Password), socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl,
                    CookieCollection = cookies,
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                Url = baseUrl + "Main.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "logonAction.do",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
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

                #endregion

                #region 第二步,获取基本信息

                Url = baseUrl + "common/commQueryAction.do?method=query";
                postdata = string.Format("sqlType=SQL&querySQL=select count (1)  a from ac01 a ,  ac02 b where 1=1 and a.eac019 in ('0','1') and a.aac008 not  in ('03','04') and a.aac001=b.aac001 and b.aae140 not in ('17','10') and a.aac002='{0}'", socialReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    Referer = baseUrl + "Main.jsp",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
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

                Url = baseUrl + "radowAction.do?method=doEvent&pageModel=pages.socialinsurance.socialinsurance01";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Referer = baseUrl + "Main.jsp",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@cellspacing='1']/tr/td", "inner");
                Res.IdentityCard = results[3].Trim().Replace("&nbsp;", "");//身份证号
                Res.Name = results[5].Trim().Replace("&nbsp;", "");//姓名
                Res.BirthDate = results[7].Trim().Replace("&nbsp;", "");//出生日期
                Res.Sex = results[9].Trim().Replace("&nbsp;", "");//性别
                Res.CompanyName = results[13].Trim().Replace("&nbsp;", "");//单位名称
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@style='border:1px #9db5d4 solid; border-collapse:collapse;']/tr[position()>2]/td", "text", true);
                List<string> joinTypes = new List<string>();
                for (int i = 0; i <= results.Count - 6; i = i + 6)
                {
                    Res.SpecialPaymentType += results[i] + ":" + results[i + 5] + ";";
                    joinTypes.Add(results[i]);
                    if (results[i] == "企业养老保险")
                    {
                        Res.SocialInsuranceBase = results[i + 4].Trim().Replace("&nbsp;", "").ToDecimal(0);
                    }
                }

                //排序参加险种类型
                foreach (string type in SortType)
                {
                    foreach (string join in joinTypes)
                    {
                        if (type != @join) continue;
                        switch (type)
                        {
                            case "企业养老保险":
                                PageDic.Add(type, "01");
                                break;
                            case "基本医疗保险":
                                PageDic.Add(type, "07");
                                break;
                            case "失业保险":
                                PageDic.Add(type, "06");
                                break;
                            case "工伤保险":
                                PageDic.Add(type, "04");
                                break;
                            case "生育保险":
                                PageDic.Add(type, "05");
                                break;
                            case "大病补助医疗保险":
                                PageDic.Add(type, "08");
                                break;
                            case "公务员补助医疗保险":
                                PageDic.Add(type, "09");
                                break;
                        }
                    }
                }
                //个人年度缴费查询
                Url = baseUrl + "radowAction.do?method=doEvent&pageModel=pages.socialinsurance.socialinsurance02";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                Res.EmployeeNo = CommonFun.GetMidStr(httpResult.Html, "psseno=", "&qqx");
                int beginYear = CommonFun.GetMidStr(httpResult.Html, "&bbf=", "&name=").ToInt(0);//最近年份
                #endregion

                #region 获取详细信息

                try
                {
                    foreach (KeyValuePair<string, string>  type in PageDic)
                    {
                        GetAllDetail(type, beginYear, ref Res);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
                #endregion
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
