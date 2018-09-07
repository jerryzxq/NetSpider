using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.NetSpider.PluginManager;


namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.GD
{
    public class shenzhencomputer : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "https://e.szsi.gov.cn/siservice/";
        string socialCity = "gd_shenzhencomputer";
        #endregion
        #region 私有变量
        /// <summary>
        /// 快速排序
        /// </summary>
        /// <param name="nums">list数组</param>
        /// <param name="left">0</param>
        /// <param name="right">list长度</param>
        static void QuickSort(ref List<int> nums, int left, int right)
        {
            if (left < right)
            {
                int i = left;
                int j = right - 1;
                int middle = nums[(left + right) / 2];
                while (true)
                {
                    while (i < right && nums[i] > middle) { i++; };
                    while (j > 0 && nums[j] < middle) { j--; };
                    if (i == j) break;
                    nums[i] = nums[i] + nums[j];
                    nums[j] = nums[i] - nums[j];
                    nums[i] = nums[i] - nums[j];
                    if (nums[i] == nums[j]) j--;
                }
                QuickSort(ref nums, left, i);
                QuickSort(ref nums, i + 1, right);
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
                Url = baseUrl + "search.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
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
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Password.IsEmpty() || socialReq.Username.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                Url = baseUrl + "search.jsp";
                postdata = String.Format("flag=1&pdcode={0}&pdserl={1}", socialReq.Username, socialReq.Password);
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
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "JavaScript'>alert('", "')</script>");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion
                #region 第二步， 获取基本信息

                Url = baseUrl + "frame/printShow.jsp";
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
                string formatBase = (CommonFun.GetMidStr(httpResult.Html, "flashvars =", ",xmldata") + "}").Replace("{", "{\"").Replace(":", "\":").Replace(",", ",\"");
                JObject baseObj = (JObject)JsonConvert.DeserializeObject(formatBase);
                Res.Name = baseObj["labname"].ToString();
                Res.Sex = baseObj["labsex"].ToString();
                Res.IdentityCard = baseObj["labidentityid"].ToString();
                Res.EmployeeNo = baseObj["labpcid"].ToString();
                Res.PaymentMonths = baseObj["labprovide"].ToString().ToInt(0);
                List<int> nums = new List<int> { baseObj["labprovide"].ToString().ToInt(0), baseObj["labmedical"].ToString().ToInt(0), baseObj["labunemployed"].ToString().ToInt(0), baseObj["labinjury"].ToString().ToInt(0) };
                QuickSort(ref nums, 0, 4);
                Res.OldPaymentMonths = nums[0] > Res.PaymentMonths ? nums[0] - Res.PaymentMonths : 0;

                //公司字典
                string company = baseObj["labremark"].ToString().Replace("&#xd;", "");
                MatchCollection maches = new Regex(@"[0-9]\d*").Matches(company);
                Dictionary<string, string> dic = new Dictionary<string, string>();
                if (maches.Count > 0)
                {
                    for (int i = 0; i < maches.Count; i++)
                    {
                        string endStr = string.Empty;
                        if ((i + 1) < maches.Count)
                        {
                            endStr = maches[i + 1].ToString();
                        }
                        dic.Add(maches[i].ToString(), CommonFun.GetMidStr(company, maches[i].ToString(), endStr).Trim());
                    }
                }
                #endregion
                #region 第三步，查询明细

                string detailStr = CommonFun.GetMidStr(httpResult.Html, "xmldata:\"", "\"};var").Trim();
                //detailStr = detailStr.Replace("<dataset>", "[").Replace("'/></dataset>", "\"}]").Replace("<terminateNode", "{\"").Replace("'/>", "\"},").Replace("='", "\":\"").Replace("'", "\",\"");
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(detailStr);
                XmlNode xmlnode = xmldoc.SelectSingleNode("dataset");
                if (xmlnode != null)
                {
                    XmlNodeList xmlnodelist = xmlnode.ChildNodes;
                    foreach (XmlNode singlenode in xmlnodelist)
                    {
                        XmlElement xmlel = (XmlElement)singlenode;
                        //缴费明细表中带"*"标示为补缴,空行为断缴
                        if (string.IsNullOrEmpty(xmlel.GetAttribute("provide_unitNO"))) continue;

                        DateTime dt = DateTime.ParseExact(xmlel.GetAttribute("year") + xmlel.GetAttribute("month"), "yyyyM", null);
                        string insuranceTime = dt.ToString(Consts.DateFormatString7);
                        bool isSave = false;
                        detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == insuranceTime);
                        if (detailRes == null)
                        {
                            isSave = true;
                            detailRes = new SocialSecurityDetailQueryRes
                            {
                                Name = Res.Name,
                                IdentityCard = Res.IdentityCard,
                                PayTime = dt.ToString(Consts.DateFormatString2),
                                SocialInsuranceTime = insuranceTime,
                                CompanyName = dic[xmlel.GetAttribute("provide_unitNO")],
                                PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal,
                                //未见有补缴案例,无法判断该标志,暂时全部写为正常
                                PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal
                            };
                        }
                        detailRes.SocialInsuranceBase = xmlel.GetAttribute("provide_salary").ToDecimal(0);
                        //养老
                        detailRes.PensionAmount += xmlel.GetAttribute("provide_personal").ToDecimal(0);
                        detailRes.CompanyPensionAmount += xmlel.GetAttribute("provide_unit").ToDecimal(0);
                        //医疗
                        detailRes.MedicalAmount += xmlel.GetAttribute("medical_personal").ToDecimal(0);
                        detailRes.CompanyMedicalAmount += xmlel.GetAttribute("medical_unit").ToDecimal(0);
                        //工伤
                        detailRes.EmploymentInjuryAmount += xmlel.GetAttribute("injury_unit").ToDecimal(0);
                        if (isSave)
                        {
                            Res.Details.Add(detailRes);
                        }
                    }
                }
                //公司名称,编号
                SocialSecurityDetailQueryRes detailRecently = Res.Details.OrderByDescending(o => o.SocialInsuranceTime).FirstOrDefault();
                if (detailRecently != null)
                {
                    Res.CompanyName = detailRecently.CompanyName;
                    var value = dic.FirstOrDefault(o => o.Value == detailRecently.CompanyName).Key;
                    Res.CompanyNo = value;
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

