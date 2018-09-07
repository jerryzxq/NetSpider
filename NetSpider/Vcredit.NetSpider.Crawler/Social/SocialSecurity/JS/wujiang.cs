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

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.JS
{
    public class wujiang : ISocialSecurityCrawler
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
        string socialCity = "js_wujiang";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            try
            {
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
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
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Username.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                Url = "http://www.wjrs.gov.cn:8080/wscx/index.jsp";
                postdata = String.Format("akc020={0}&aac002={1}&submit=%E6%9F%A5%E8%AF%A2", socialReq.Username, socialReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Encoding = Encoding.UTF8
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (httpResult.Html.Contains("找不到卡号"))
                {
                    Res.StatusDescription = "找不到卡号" + CommonFun.GetMidStr(httpResult.Html, "找不到卡号", "");
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #endregion

                #region 获取基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='datagrid2']//td");
                if (results.Count == 10)
                {
                    Res.EmployeeNo = results[0];
                    Res.CompanyName = results[2];
                    Res.Name = results[3];
                    Res.IdentityCard = results[4];
                    Res.BirthDate = results[5];
                    Res.Sex = results[6];
                    Res.EmployeeStatus = results[7];
                }
                #endregion

                #region 查询明细
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='datagrid4']//tr");
                foreach (string item in results)
                {
                    List<string> _detail = HtmlParser.GetResultFromParser(item, "/td");
                    if (_detail.Count == 6)
                    {
                        try
                        {
                            int starttime = int.Parse(_detail[2]);
                            int endtime = int.Parse(_detail[3]);
                            decimal SocialInsuranceBase = _detail[4].ToDecimal(0) == 0 ? 0 :_detail[5].ToDecimal(0)/_detail[4].ToDecimal(0);
                            do
                            {
                                detailRes = new SocialSecurityDetailQueryRes();
                                detailRes.Name = Res.Name;
                                detailRes.IdentityCard = Res.IdentityCard;
                                detailRes.SocialInsuranceTime = endtime.ToString();
                                detailRes.PayTime = detailRes.SocialInsuranceTime;
                                detailRes.SocialInsuranceBase = SocialInsuranceBase;
                                detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                                detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                                Res.Details.Add(detailRes);

                                endtime--;
                            }
                            while (starttime <= endtime);
                        }
                        catch { }
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
