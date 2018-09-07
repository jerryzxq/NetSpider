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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.GD
{
    public class foshan : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://61.142.213.86/grwssb/";
        //string baseUrl = "http://61.142.213.86/grwssb/";
        string socialCity = "gd_foshan";
        #endregion
        #region 私有变量

        enum InfoType
        {
            养老保险,
            医疗保险,
            失业保险,
            工伤保险,
            生育保险,
        }
        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息
        /// <summary>
        ///  将各种保险对应的页面所需信息存入PageHash
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, new string[] { "ylbx" });
            PageHash.Add(InfoType.医疗保险, new string[] { "yilbx" });
            PageHash.Add(InfoType.失业保险, new string[] { "syebx" });
            PageHash.Add(InfoType.工伤保险, new string[] { "gsbx" });
            PageHash.Add(InfoType.生育保险, new string[] { "syubx" });
        }
        /// <summary>
        /// 获取缴费明细
        /// </summary>
        /// <param name="type">缴费类型</param>
        /// <param name="Res"></param>
        private void GetAllDetail(InfoType type, ref SocialSecurityQueryRes Res)
        {
            
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            int pageIndex = 1;
            int pageCount = 0;
            string whereCls = string.Empty;
            string typeStr = ((string[])PageHash[type])[0];
            Url = baseUrl + string.Format("action/MainAction?menuid=grcx_{0}jfcx&ActionType=grcx_{0}jfcx&flag=true", typeStr);
            httpItem = new HttpItem()
            {
                URL = Url,
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            pageCount = CommonFun.GetMidStr(httpResult.Html, "条记录&nbsp;&nbsp; <b>", "</b> 页&nbsp;&nbsp;当前是第").ToInt(0);
            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='grcx_" + typeStr + "jfcx_list']/div/tbody/tr", "inner");
            whereCls = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='table_grcx_" + typeStr + "jfcx_list']", "table_whereCls")[0];
            while (pageIndex < pageCount)
            {
                Url = baseUrl + "jsp/common/tabledata.jsp";
                postdata = string.Format("id=grcx_{2}jfcx_list&grcx_{2}jfcx_list_page={0}&ActionType=grcx_{2}jfcx&menuid=grcx_{2}jfcx&flag=true&filterOnNoDataRight=false&subTotalName=%25E5%25B0%258F%25E8%25AE%25A1&display=block&pageSize=20&hasPage=true&hasTitle=true&whereCls={1}&type=q&useAjaxPostPars=true", pageIndex, whereCls, typeStr);
                httpItem = new HttpItem()
                {
                    Accept = "text/javascript, text/html, application/xml, text/xml, */*",
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='grcx_" + typeStr + "jfcx_list']/div/tbody/tr", "inner"));
                whereCls = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='table_grcx_" + typeStr + "jfcx_list']", "table_whereCls")[0];
                pageIndex++;
            }
            //养老12行,其他11行
            int count = type == InfoType.养老保险 ? 12 : 11;
            foreach (string item in results)
            {
                SocialSecurityDetailQueryRes detailRes = null;
                var tdRow = HtmlParser.GetResultFromParser(item, "//td");
                if (tdRow.Count != count) continue;
                string[] datearr = tdRow[0].ToTrim().Split('-');
                DateTime statrdate = DateTime.ParseExact(datearr[0], "yyyyMM", null);
                DateTime enddate = DateTime.ParseExact(datearr[1], "yyyyMM", null);
                //多月缴费合并显示做拆分处理
                for (DateTime date = statrdate; date <= enddate; date = date.AddMonths(1))
                {
                    detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == date.ToString("yyyyMM"));
                    bool isSave = false;
                    if (detailRes == null)
                    {
                        isSave = true;
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.CompanyName = tdRow[1];
                        detailRes.PayTime = date.ToString("yyyyMM");
                        detailRes.SocialInsuranceTime = date.ToString("yyyyMM");
                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                        if (type == InfoType.养老保险 || type == InfoType.医疗保险)
                        {
                            detailRes.SocialInsuranceBase = tdRow[3].ToDecimal(0);
                        }
                    }
                    switch (type)
                    {
                        case InfoType.养老保险:
                            detailRes.PensionAmount += tdRow[4].ToDecimal(0);
                            detailRes.CompanyPensionAmount += tdRow[5].ToDecimal(0);
                            break;
                        case InfoType.医疗保险:
                            detailRes.CompanyMedicalAmount += tdRow[5].ToDecimal(0);
                            detailRes.MedicalAmount += tdRow[4].ToDecimal(0);
                            break;
                        case InfoType.失业保险:
                            detailRes.UnemployAmount += tdRow[5].ToDecimal(0);
                            break;
                        case InfoType.工伤保险:
                            detailRes.EmploymentInjuryAmount += tdRow[5].ToDecimal(0);
                            break;
                        case InfoType.生育保险:
                            detailRes.MaternityAmount += tdRow[5].ToDecimal(0);
                            break;
                    }
                    if (isSave)
                    {
                        Res.Details.Add(detailRes);
                    }
                }
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
                Url = "http://www.fssi.gov.cn/";
                //Url = baseUrl + "login.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，登录

                Url = baseUrl + "action/GRLoginAction";
                postdata = String.Format("UserID={0}&Password={1}", socialReq.Identitycard, socialReq.Password);
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion
                #region 第三步， 获取基本信息
                Url = baseUrl + "action/MainAction?ActionType=grcx_grjbzlcx&flag=true";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='grcx_grjbzlcx']/tbody/tr/td/span", "inner");
                if (results.Count <= 0 || string.IsNullOrEmpty(results[2]))
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Res.EmployeeNo = results[2].Trim();//编号
                Res.Name = results[3].Trim();//姓名
                Res.Sex = results[4].Trim();//性别
                Res.BirthDate = results[5];//出生日期
                Res.IdentityCard = results[1].Trim();//身份证号
                Res.CompanyName = results[6].Trim();//单位名称
                //Res.EmployeeStatus = results[11].Trim();//人员状态
                Res.PaymentMonths = results[15].Trim().ToInt(0);

                #endregion
                #region 第四步，查询明细

                InitPageHash();
                foreach (InfoType type in Enum.GetValues(typeof(InfoType)))
                {
                    try
                    {
                        GetAllDetail(type, ref Res);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
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
