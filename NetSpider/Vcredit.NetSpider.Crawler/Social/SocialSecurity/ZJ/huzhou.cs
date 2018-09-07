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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.ZJ
{
    public class huzhou : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://61.153.183.131:8090/huzhounet/";
        string socialCity = "zj_huzhou";
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
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "ImageCode";
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
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
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
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Name.IsEmpty() || socialReq.Vercode.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录
                Url = baseUrl + "checkLoginpersonal";
                postdata = String.Format("uname={0}&truename={1}&password={2}&uaertype=1&imgCode={3}", socialReq.Identitycard, socialReq.Name.ToUrlEncode(), socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "login",
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
                try
                {
                    string msg = jsonParser.GetResultFromParser(httpResult.Html, "errMsg");
                    if (jsonParser.GetResultFromParser(httpResult.Html, "type") != "1")
                    {
                        Res.StatusDescription = msg;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                catch
                {
                    Res.StatusDescription = "登录失败";
                    Res.StatusCode = ServiceConsts.StatusCode_error;
                    return Res;
                }

                Url = baseUrl + "personal/home";
                postdata = String.Format("uname={0}&truename={1}&password={2}", socialReq.Identitycard, socialReq.Name.ToUrlEncode(Encoding.GetEncoding("gbk")), socialReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "login",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 获取基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='gridxx']/tr/td");
                if (results.Count == 13)
                {
                    Res.EmployeeNo = results[0];
                    Res.Name = results[1];
                    Res.IdentityCard = results[3];
                    Res.Sex = results[4];
                    Res.BirthDate = results[5];
                    Res.CompanyNo = results[6];
                    Res.CompanyName = results[7];
                    Res.Payment_State = results[9];
                    Res.Phone = results[11];
                    if (string.IsNullOrWhiteSpace(Res.Phone))
                    {
                        Res.Phone = results[10];
                    }
                    Res.Address = results[12];
                    if (results[1]==socialReq.Name&&results[2].Contains("身份证"))
                    {
                        Res.IdentityCard = socialReq.Identitycard;
                    }
                }
                #endregion

                #region 参保状态
                Url = baseUrl + "personal/InsuranceListFind";
                //postdata = "title_Name=&active=&pagedata.pageSize=0&pagedata.totalRows=0&pagedata.pageSum=0&pagedata.currentPage=0&pagedata.pageStart=0&pagedata.pageEnd=0&pagedata.beginning=1&pagedata.prev=0&pagedata.next=0&pagedata.end=0";
                postdata = "active=1&pagedata.pageSize=0&pagedata.totalRows=0&pagedata.pageSum=0&pagedata.currentPage=0&pagedata.pageStart=0&pagedata.pageEnd=0&pagedata.beginning=1&pagedata.prev=1&pagedata.next=1&pagedata.end=1&currentPage=1";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "personal/home",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                List<CanBaoStatus> canbao_list = jsonParser.DeserializeObject<List<CanBaoStatus>>(jsonParser.GetResultFromParser(httpResult.Html, "data"));
                foreach (CanBaoStatus canbaoinfo in canbao_list)
                {
                    if (canbaoinfo.aae140.Contains("基本养老") || canbaoinfo.aae140.Contains("基本医疗") || canbaoinfo.aae140.Contains("失业保险") || canbaoinfo.aae140.Contains("工伤保险") || canbaoinfo.aae140.Contains("生育保险"))
                    {
                        Res.SpecialPaymentType += canbaoinfo.aae140 + ":[" + canbaoinfo.aac008 + "、" + canbaoinfo.aac031 + "]";
                    }
                }
                #endregion

                #region 查询明细
                List<string> type_list = new List<string> { "企业职工基本养老保险", "城镇职工基本医疗保险", "失业保险", "工伤保险", "生育保险" };
                for (int i = 0; i < type_list.Count; i++)
                {
                    int pageno = 0;
                    int pagecount = 0;
                    do
                    {
                        Url = baseUrl + "personal/PaymentDetailsFind";
                        postdata = string.Format("title_Name=&active=2&ace021={0}&ace023a=&ace023b=&pagedata.pageSize=20&pagedata.totalRows=0&pagedata.pageSum=0&pagedata.currentPage={1}&pagedata.pageStart=0&pagedata.pageEnd=0&pagedata.beginning=1&pagedata.prev=0&pagedata.next=0&pagedata.end=&currentPage={2}", type_list[i].ToUrlEncode(), pageno, pageno + 1);
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "POST",
                            Postdata = postdata,
                            Referer = Url,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        try
                        {
                            if (pagecount == 0)
                            {
                                pagecount = jsonParser.GetResultFromMultiNode(httpResult.Html, "page:pageSum").ToInt(0);
                            }
                            List<DetailInfo> detail_list = jsonParser.DeserializeObject<List<DetailInfo>>(jsonParser.GetResultFromParser(httpResult.Html, "data"));
                            foreach (DetailInfo _detail in detail_list)
                            {
                                string SocialInsuranceTime = _detail.ace023;
                                detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime).FirstOrDefault();
                                bool NeedAdd = false;
                                if (detailRes == null)
                                {
                                    detailRes = new SocialSecurityDetailQueryRes();
                                    detailRes.Name = Res.Name;
                                    detailRes.IdentityCard = Res.IdentityCard;
                                    detailRes.CompanyName = _detail.ace020;
                                    detailRes.SocialInsuranceBase = _detail.ace022.ToDecimal(0);
                                    detailRes.PayTime = _detail.ace026;
                                    detailRes.SocialInsuranceTime = _detail.ace023;
                                    detailRes.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                                    detailRes.PaymentFlag = _detail.ace027 == "已到账" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : _detail.ace027;

                                    NeedAdd = true;
                                }
                                else
                                {
                                    if (_detail.ace027 == "已到账")
                                    {
                                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                                    }
                                }
                                switch (i)
                                {
                                    case 0:
                                        detailRes.PensionAmount += _detail.ace203.ToDecimal(0);
                                        detailRes.CompanyPensionAmount += _detail.ace202.ToDecimal(0);
                                        break;
                                    case 1:
                                        detailRes.MedicalAmount += _detail.ace203.ToDecimal(0);
                                        detailRes.CompanyMedicalAmount += _detail.ace202.ToDecimal(0);
                                        break;
                                    case 2:
                                        detailRes.UnemployAmount += _detail.ace025.ToDecimal(0);
                                        break;
                                    case 3:
                                        detailRes.EmploymentInjuryAmount += _detail.ace025.ToDecimal(0);
                                        break;
                                    case 4:
                                        detailRes.MaternityAmount += _detail.ace025.ToDecimal(0);
                                        break;
                                }

                                if (NeedAdd)
                                {
                                    Res.Details.Add(detailRes);
                                }
                            }
                        }
                        catch { }
                        pageno++;
                    } while (pageno < pagecount);
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

        class CanBaoStatus
        {
            public string aae140 { get; set; }//险种
            public string aac008 { get; set; }//参保情况
            public string aac031 { get; set; }//缴费情况
        }

        class DetailInfo
        {
            public string ace203 { get; set; }//个人应缴
            public string ace202 { get; set; }//单位应缴
            public string ace025 { get; set; }//缴费金额
            public string ace026 { get; set; }//到账年月
            public string ace027 { get; set; }//到账标识
            public string ace022 { get; set; }//缴费基数
            public string ace023 { get; set; }//缴费年月
            public string ace020 { get; set; }//单位名称
        }

    }
}
