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
using System.Xml;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.TJ
{
    public class tianjin : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.tj.hrss.gov.cn/tjsi_web/";
        string socialCity = "tj_tianjin";
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
                Url = baseUrl + "login.do?method=person";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "jcaptcha";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "login.do?method=person",
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
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";

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
            string dataObjs = string.Empty;
            string attributes = string.Empty;
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录

                Url = baseUrl + "j_unieap_security_check.do";
                postdata = String.Format("j_username={0}&j_password={1}&jcaptcha_response={2}", socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "j_unieap_security_check.do",
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
                string msg = CommonFun.GetMidStr(httpResult.Html, "var msg = \"", "\";alert(msg);");
                if (!msg.IsEmpty())
                {
                    Res.StatusDescription = msg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 校验

                Url = baseUrl + "unieap/pages/index.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "j_unieap_security_check.do",
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

                Url = baseUrl + "enterapp.do?method=begin&name=/tjsi_person&welcome=/tjsi_person/pages/index.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "j_unieap_security_check.do",
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

                #region 获取信息

                Url = baseUrl + "personquery.do?method=personCBInfo";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "tjsi_person/pages/menu/page_mid.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //基本信息
                attributes = CommonFun.GetMidStr(httpResult.Html, "<orderInfo  attrIndex=\"1\" dataType=\"STRING\" order=\"ascending\"    ></orderInfo><attributes>", "</attributes>");
                dataObjs = CommonFun.GetMidStr(httpResult.Html, "<dataObj index=\"1\" selected=\"false\" status=\"UNCHANGED\">", "</dataObj></dataObjs></dataWindow>");
                results = HtmlParser.GetResultFromParser(dataObjs, "//attribute ", "newValue");
                if (results.Count <= 0 || string.IsNullOrEmpty(results[1]))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Res.EmployeeNo = results[1].Trim();//编号
                Res.Name = results[4].Trim();//姓名
                Res.Sex = results[5].IsEmpty() ? "" : HtmlParser.GetResultFromParser(HtmlParser.GetResultFromParser(attributes, "//attribute[@index='6']", "inner")[0], "//option[@value='" + results[5] + "']", "inner")[0];//性别
                Res.BirthDate = results[7];//出生日期
                Res.Race = results[6].IsEmpty() ? "" : HtmlParser.GetResultFromParser(HtmlParser.GetResultFromParser(attributes, "//attribute[@index='7']", "inner")[0], "//option[@value='" + results[6] + "']", "inner")[0]; ;//民族
                Res.IdentityCard = results[3].Trim();//身份证号
                Res.CompanyName = results[0].Trim();//单位名称
                Res.CompanyNo = results[2];//单位编号
                Res.WorkDate = results[8].Trim();//参加工作时间
                Res.EmployeeStatus = results[10].IsEmpty() ? "" : HtmlParser.GetResultFromParser(HtmlParser.GetResultFromParser(attributes, "//attribute[@index='11']", "inner")[0], "//option[@value='" + results[10] + "']", "inner")[0]; ;//人员状态
                //账户总额、个人账户总额
                Url = baseUrl + "personquery.do?method=personEndowmentInfo";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "tjsi_person/pages/menu/page_mid.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                dataObjs = CommonFun.GetMidStr(httpResult.Html, "<dataObj index=\"1\" selected=\"false\" status=\"UNCHANGED\">", "</dataObj></dataObjs></dataWindow>");
                results = HtmlParser.GetResultFromParser(dataObjs, "//attribute ", "newValue");
                if (results.Count > 0)
                {
                    Res.InsuranceTotal = results[5].ToTrim().ToDecimal(0);//账户总额
                    Res.PersonalInsuranceTotal = results[11].Trim().ToDecimal(0);//个人账户总额
                }

                //明细
                Url = baseUrl + "personquery.do?method=personEndowmentInfo";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "tjsi_person/pages/menu/page_mid.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string dwidcode = CommonFun.GetMidStr(httpResult.Html, "dwid=\"personalQuery.v_ic01|", "\"   >");

                Url = baseUrl + "DataWindowMgrAction.do?method=resetPageSize&isPartlyRefresh=true";
                postdata = string.Format("dwid_personalQuery.v_ic01=personalQuery.v_ic01|{0}&dwName=personalQuery.v_ic01&pageSize=-1", dwidcode);
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl + "personquery.do?method=personEndowmentInfo",
                    ContentType = "",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                dataObjs = CommonFun.GetMidStr(httpResult.Html, "DATAOBJS= ", "RESPONSE_SEPARATOR");
                XmlDocument xml = new XmlDocument();
                if (dataObjs != null && dataObjs != "数据已丢失，请重新查询")
                {
                    xml.LoadXml(dataObjs);//加载xml
                    XmlNodeList xmlList = xml.GetElementsByTagName("dataObj"); //取得节点名为row的XmlNode集合
                    foreach (XmlNode xmlNode in xmlList)
                    {
                        XmlNodeList childList = xmlNode.ChildNodes; //取得row下的子节点集合
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.PayTime = DateTime.Parse(childList[1].InnerText.Trim()).ToString(Consts.DateFormatString7);
                        detailRes.SocialInsuranceTime = DateTime.Parse(childList[2].InnerText.Trim()).ToString(Consts.DateFormatString7);
                        detailRes.SocialInsuranceBase = childList[5].InnerText.Trim().ToDecimal(0);
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = childList[4].InnerText.Trim() != "1" && childList[4].InnerText.Trim() != "6" ? ServiceConsts.SocialSecurity_PaymentFlag_Adjust : ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        detailRes.PensionAmount = childList[6].InnerText.Trim().ToDecimal(0);
                        detailRes.CompanyPensionAmount = childList[15].InnerText.Trim().ToDecimal(0);
                        Res.Details.Add(detailRes);
                        PaymentMonths++;
                    }
                }

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
}
