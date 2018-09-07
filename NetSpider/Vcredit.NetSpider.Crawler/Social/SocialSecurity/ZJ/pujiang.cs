using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.ZJ
{
    /// <summary>
    /// 
    /// </summary>
    public class pujiang : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://220.191.229.142:7080/pjinsiis/";
        string socialCity = "zj_pujiang";
        #endregion
        #region 私有变量

        enum InfoType
        {
            养老保险,
            医疗保险,
            失业保险,
            工伤保险,
            生育保险,
            大病救助医疗保险,
            公务员补助医疗保险
        }
        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息
        /// <summary>
        ///  将各种保险对应的页面所需信息存入PageHash
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, "01");
            PageHash.Add(InfoType.医疗保险, "07");
            PageHash.Add(InfoType.失业保险, "06");
            PageHash.Add(InfoType.工伤保险, "04");
            PageHash.Add(InfoType.生育保险, "05");
            PageHash.Add(InfoType.大病救助医疗保险, "08");
            PageHash.Add(InfoType.公务员补助医疗保险, "09");
        }
        /// <summary>
        /// 获取缴费明细
        /// </summary>
        /// <param name="type">缴费类型</param>
        /// <param name="Res"></param>
        private void GetAllDetail(InfoType type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            List<string> results = new List<string>();
            string postData = string.Empty;
            int spage = 1;
            int pageCount = 1;
            Url = baseUrl + "person/payinfo.jsp";
            do
            {
                postData = string.Format("spage={0}&aae001={1}", spage, (string)PageHash[type]);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postData,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (spage == 1)
                {
                    pageCount = CommonFun.GetMidStr(httpResult.Html, "</font>/", "页").ToInt(0);
                }
                results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//tr[@class='table_tr_01']", "", true));
                spage++;
            } while (spage <= pageCount);
            foreach (string item in results)
            {
                var tdRow = HtmlParser.GetResultFromParser(item, "//td/div");
                if (tdRow.Count < 10) continue;
                SocialSecurityDetailQueryRes detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == tdRow[1]);
                bool isSave = false;
                if (detailRes == null)
                {
                    isSave = true;
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.IdentityCard = Res.IdentityCard;
                    detailRes.PayTime = tdRow[9];
                    detailRes.SocialInsuranceTime = tdRow[1];
                    detailRes.PaymentFlag = tdRow[8] != "未到帐" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                    detailRes.PaymentType = tdRow[8];
                }
                switch (type)
                {
                    case InfoType.养老保险:
                        detailRes.PensionAmount += tdRow[5].ToDecimal(0);
                        detailRes.CompanyPensionAmount += tdRow[4].ToDecimal(0);
                        detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                        break;
                    case InfoType.医疗保险:
                        detailRes.CompanyMedicalAmount += tdRow[4].ToDecimal(0);
                        detailRes.MedicalAmount += tdRow[5].ToDecimal(0);
                        break;
                    case InfoType.失业保险:
                        detailRes.UnemployAmount += (tdRow[4].ToDecimal(0) + tdRow[5].ToDecimal(0));
                        break;
                    case InfoType.工伤保险:
                        detailRes.EmploymentInjuryAmount += tdRow[4].ToDecimal(0);
                        break;
                    case InfoType.生育保险:
                        detailRes.MaternityAmount += tdRow[4].ToDecimal(0);
                        break;
                    case InfoType.大病救助医疗保险:
                        detailRes.IllnessMedicalAmount += tdRow[4].ToDecimal(0);
                        break;
                    case InfoType.公务员补助医疗保险:
                        detailRes.CivilServantMedicalAmount += tdRow[4].ToDecimal(0);
                        break;
                }
                detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : tdRow[2].ToDecimal(0);
                if (isSave)
                {
                    Res.Details.Add(detailRes);
                }
            }
        }
        #endregion
        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            string Url = string.Empty;
            try
            {
                Url = baseUrl + "verifyCode";
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
                    if (DateTime.Now.Hour>=17)
                    {
                        Res.StatusDescription = "该服务器已暂时关闭,请明天再试!";  
                    }
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                CacheHelper.SetCache(token, cookies);
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
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Name.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,验证

                Url = baseUrl;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                string username = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='username']", "value")[0];
                string passwd = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='passwd']", "value")[0];

                Url = baseUrl + "logonAction.do";
                postdata = String.Format("username={0}&passwd={1}&verifycode={2}&params=%7B%27username%27%3A%27{0}%27%2C%27password%27%3A%27{1}%27%2C%27scene%27%3A%27sce%3AUSER_PASS%27%7D&submitbtn.x=20&submitbtn.y=13", username, CommonFun.GetMd5Str(passwd), socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    Accept = "application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*",
                    ContentType = "application/x-www-form-urlencoded",
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
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert(\"", "\\n\");");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion
                #region 第二步,登陆

                Url = baseUrl + "radowAction.do?method=doEvent";
                #region postdata
                postdata = String.Format("pageModel=pages.onlinequery.OnlineQuery&eventNames=search.onclick&radow_parent_data=&rc=%7B%22n_aae135%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22{0}%22%7D%2C%22n_aac003%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22{1}%22%7D%2C%22n_pw%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22{2}%22%7D%2C%22search%22%3A%7B%22type%22%3A%22button%22%2C%22data%22%3A%22%22%7D%2C%22aac019%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22eac076%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22eac065%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aaa152%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aaa154%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22akc021%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac054%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac022%22%3A%7B%22type%22%3A%22numberfield%22%2C%22data%22%3A%22%22%7D%2C%22aac023%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac024%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac069%22%3A%7B%22type%22%3A%22numberfield%22%2C%22data%22%3A%22%22%7D%2C%22aac070%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac071%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac030%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aaa029%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aae135%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac003%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac004%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac006%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac005%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac007%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22eac101%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae005_1%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae005_2%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac009%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac010%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac029%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aab301%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22eab009%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22eab030%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac014%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac015%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac020%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac017%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac012%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac011%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aae147%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22eoc005%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22eoc006%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22taxno%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22qdbz%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22yhhh%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22yhzh%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae159%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae007%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac028%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aae006%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae013%22%3A%7B%22type%22%3A%22textarea%22%2C%22data%22%3A%22%22%7D%2C%22aae140s%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aae078%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22eac002%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aaa027s%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aae003beg%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aae003end%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22jiaofei%22%3A%7B%22type%22%3A%22button%22%2C%22data%22%3A%22%22%7D%2C%22grid3%22%3A%7B%22type%22%3A%22editorgrid%22%2C%22data%22%3A%22%5B%5D%22%7D%2C%22asc%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22desc%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22columns%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22groupBy%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22showGroups%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22s_grid3%22%3A%7B%22type%22%3A%22editorgrid%22%2C%22data%22%3A%22%5B%5D%22%7D%2C%22grid1%22%3A%7B%22type%22%3A%22editorgrid%22%2C%22data%22%3A%22%5B%5D%22%7D%2C%22grid111%22%3A%7B%22type%22%3A%22editorgrid%22%2C%22data%22%3A%22%5B%5D%22%7D%2C%22grid11%22%3A%7B%22type%22%3A%22editorgrid%22%2C%22data%22%3A%22%5B%5D%22%7D%2C%22ekb029%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22ekb028%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22medicalMoney%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22eac005%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae100%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22eac054%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22yh%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22grid3Data%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22s_grid3Data%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22grid1Data%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22grid111Data%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22grid11Data%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22akc021_%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22aac001%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22aab401%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22radow_parent_data%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae140s_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aae078_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22eac002_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aaa027s_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aae100_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22eac054_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac019_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22eac076_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aaa152_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aaa154_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22akc021_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac030_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aaa029_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac004_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac005_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac009_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac029_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aab301_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22eab009_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22eab030_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac014_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac015_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac020_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac017_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac012_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac011_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aae147_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22eoc005_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22qdbz_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac028_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%7D", socialReq.Identitycard, socialReq.Name.ToUrlEncode(), socialReq.Password);
                #endregion
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    ContentType = "application/x-www-form-urlencoded",
                    URL = Url,
                    Host = "220.191.229.142:7080",
                    Referer = baseUrl + "radowAction.do?method=doEvent&pageModel=pages.onlinequery.OnlineQuery",
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header["x-requested-with"] = "XMLHttpRequest";
                httpItem.Header["request-type"] = "Ajax";
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                errorMsg = jsonParser.GetResultFromParser(httpResult.Html, "detailMessage");
                if (!string.IsNullOrWhiteSpace(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion
                #region 第三步,获取基本信息

                string needMessage = CommonFun.GetMidStr(httpResult.Html, "aab401').value='';", ";\",\"mainMessage");
                Res.Name = socialReq.Name;
                Res.IdentityCard = socialReq.Identitycard;
                Res.Name = CommonFun.GetMidStr(needMessage, "aac003').value='", "';");
                //Res.Sex = CommonFun.GetMidStr(needMessage, "aac004','", "');");//1男2女9未知
                //民族
                string race = "['01','汉族'],['02','蒙古族'],['03','回族'],['04','藏族'],['05','维吾尔族'],['06','苗族'],['07','彝族'],['08','壮族'],['09','布依族'],['10','朝鲜族'],['11','满族'],['12','侗族'],['13','瑶族'],['14','白族'],['15','土家族'],['16','哈尼族'],['17','哈萨克族'],['18','傣族'],['19','黎族'],['20','傈傈族'],['21','佤族'],['22','畲族'],['23','高山族'],['24','拉祜族'],['25','水族'],['26','东乡族'],['27','纳西族'],['28','景颇族'],['29','柯尔克孜族'],['30','土族'],['31','达翰尔族'],['32','仫佬族'],['33','羌族'],['34','布朗族'],['35','撒拉族'],['36','毛南族'],['37','仡佬族'],['38','锡伯族'],['39','阿昌族'],['40','普米族'],['41','塔吉克族'],['42','怒族'],['43','乌孜别克族'],['44','俄罗斯族'],['45','鄂温克族'],['46','德昂族'],['47','保安族'],['48','裕固族'],['49','京族'],['50','塔塔尔族'],['51','独龙族'],['52','鄂伦春族'],['53','赫哲族'],['54','门巴族'],['55','珞巴族'],['56','基诺族'],['99','其他']";
                string raceNum = CommonFun.GetMidStr(needMessage, "aac005','", "');");
                int outNum;
                if (race.IndexOf(raceNum) > -1 & int.TryParse(raceNum, out outNum))
                {
                    race = race.Substring(race.IndexOf(raceNum));
                    Res.Race = CommonFun.GetMidStr(race, "','", "']");
                }
                Res.BirthDate = CommonFun.GetMidStr(needMessage, "aac006').setValue('", "');");
                Res.WorkDate = CommonFun.GetMidStr(needMessage, "aac007').setValue('", "');");
                Res.Address = CommonFun.GetMidStr(needMessage, "aae006').value='", "';");
                if (String.IsNullOrEmpty(Res.Address))
                {
                    Res.Address = CommonFun.GetMidStr(needMessage, "aac010').value='", "';");
                }
                Res.Phone = CommonFun.GetMidStr(needMessage, "eac101').value='", "';");
                if (String.IsNullOrEmpty(Res.Phone))
                {
                    Res.Phone = CommonFun.GetMidStr(needMessage, "aae005_1').value='", "';") + "-" + CommonFun.GetMidStr(needMessage, "aae005_2').value='", "';");
                }
                Res.IsSpecialWork = CommonFun.GetMidStr(httpResult.Html, "aac019','", "')") != "0";//特殊工种
                //人员状态
                string status = "['00','无医保待遇'],['11','普通企业在职(建账)'],['12','在职公务员'],['13','自谋职业在职(建账)'],['14','在职参照公务员'],['15','普通事业在职'],['16','普通企业在职(不建账)'],['17','自谋职业在职(不建账)'],['21','普通企业退休(建账)'],['22','退休公务员'],['23','自谋职业退休(建账)'],['24','退休参照公务员'],['25','普通事业退休'],['26','普通企业退休(不建账)'],['27','自谋职业退休(不建账)'],['28','企业退休参照公务员(建账)'],['31','离休人员'],['32','离休干部遗属'],['33','六级残疾军人'],['34','老红军'],['35','建国前老工人'],['41','新农医(小额)'],['42','新农医(大额)'],['50','参照退休']";
                raceNum = CommonFun.GetMidStr(httpResult.Html, "akc021','", "');");
                if (status.IndexOf(raceNum) > -1 & int.TryParse(raceNum, out outNum))
                {
                    status = status.Substring(status.IndexOf(raceNum));
                    Res.EmployeeStatus = CommonFun.GetMidStr(status, "','", "']");
                }
                //string script = jsonParser.GetResultFromParser(httpResult.Html, "elementsScript");
                //List<string> baselist = script.ToTrim("document.getElementById").ToTrim("odin.setSelectValue").Split(';').ToList<string>();
                //特殊缴费类型
                Url = baseUrl + "radowAction.do?method=doEvent";
                #region postdata
                postdata = String.Format("pageModel=pages.onlinequery.OnlineQuery&eventNames=searchInsureInfo&radow_parent_data=&rc=%7B%22n_aae135%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22{0}%22%7D%2C%22n_aac003%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22{1}%22%7D%2C%22n_pw%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22{2}%22%7D%2C%22search%22%3A%7B%22type%22%3A%22button%22%2C%22data%22%3A%22%22%7D%2C%22aac019%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%220%22%7D%2C%22eac076%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%2200%22%7D%2C%22eac065%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%220%22%7D%2C%22aaa152%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%2200%22%7D%2C%22aaa154%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22000%22%7D%2C%22akc021%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%2211%22%7D%2C%22aac054%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac022%22%3A%7B%22type%22%3A%22numberfield%22%2C%22data%22%3A%220%22%7D%2C%22aac023%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac024%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac069%22%3A%7B%22type%22%3A%22numberfield%22%2C%22data%22%3A%220%22%7D%2C%22aac070%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac071%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac030%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%220%22%7D%2C%22aaa029%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%221%22%7D%2C%22aae135%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22{0}%22%7D%2C%22aac003%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22{1}%22%7D%2C%22aac004%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%222%22%7D%2C%22aac006%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%221977-06-26%22%7D%2C%22aac005%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%2201%22%7D%2C%22aac007%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%222002-01-01%22%7D%2C%22eac101%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae005_1%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%220579%22%7D%2C%22aae005_2%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%2284112317%22%7D%2C%22aac009%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%2211%22%7D%2C%22aac010%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac029%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%220%22%7D%2C%22aab301%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22330726%22%7D%2C%22eab009%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22eab030%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac014%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac015%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac020%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac017%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac012%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac011%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aae147%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%221%22%7D%2C%22eoc005%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%222%22%7D%2C%22eoc006%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22taxno%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22qdbz%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22yhhh%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22yhzh%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae159%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae007%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac028%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%220%22%7D%2C%22aae006%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%E6%B5%A6%E6%B1%9F%22%7D%2C%22aae013%22%3A%7B%22type%22%3A%22textarea%22%2C%22data%22%3A%22%22%7D%2C%22aae140s%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aae078%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22eac002%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aaa027s%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aae003beg%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aae003end%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22jiaofei%22%3A%7B%22type%22%3A%22button%22%2C%22data%22%3A%22%22%7D%2C%22grid3%22%3A%7B%22type%22%3A%22editorgrid%22%2C%22data%22%3A%22%5B%5D%22%7D%2C%22asc%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22desc%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22columns%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22groupBy%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22showGroups%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22s_grid3%22%3A%7B%22type%22%3A%22editorgrid%22%2C%22data%22%3A%22%5B%5D%22%7D%2C%22grid1%22%3A%7B%22type%22%3A%22editorgrid%22%2C%22data%22%3A%22%5B%5D%22%7D%2C%22grid111%22%3A%7B%22type%22%3A%22editorgrid%22%2C%22data%22%3A%22%5B%5D%22%7D%2C%22grid11%22%3A%7B%22type%22%3A%22editorgrid%22%2C%22data%22%3A%22%5B%5D%22%7D%2C%22ekb029%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22ekb028%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22medicalMoney%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22eac005%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae100%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22eac054%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22yh%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22grid3Data%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22s_grid3Data%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22grid1Data%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22grid111Data%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22grid11Data%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22akc021_%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22aac001%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%2219590%22%7D%2C%22aab401%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22radow_parent_data%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae140s_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aae078_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22eac002_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aaa027s_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aae100_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22eac054_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac019_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%97%A0%22%7D%2C%22eac076_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E9%9D%9E%E7%89%B9%E6%AE%8A%E4%BA%BA%E5%91%98%22%7D%2C%22aaa152_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%97%A0%22%7D%2C%22aaa154_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%97%A0%E8%8D%A3%E8%AA%89%E7%BA%A7%E5%88%AB%22%7D%2C%22akc021_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%99%AE%E9%80%9A%E4%BC%81%E4%B8%9A%E5%9C%A8%E8%81%8C(%E5%BB%BA%E8%B4%A6)%22%7D%2C%22aac030_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E5%90%A6%22%7D%2C%22aaa029_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E5%85%AC%E6%B0%91%E8%BA%AB%E4%BB%BD%E8%AF%81%22%7D%2C%22aac004_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E5%A5%B3%E6%80%A7%22%7D%2C%22aac005_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%B1%89%E6%97%8F%22%7D%2C%22aac009_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%9C%AC%E5%9C%B0%E9%9D%9E%E5%86%9C%E4%B8%9A%E6%88%B7%E5%8F%A3%EF%BC%88%E6%9C%AC%E5%9C%B0%E5%9F%8E%E9%95%87%EF%BC%89%22%7D%2C%22aac029_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%96%B0%E4%BA%BA%22%7D%2C%22aab301_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%B5%A6%E6%B1%9F%E5%8E%BF%22%7D%2C%22eab009_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22eab030_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac014_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac015_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac020_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac017_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac012_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac011_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aae147_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%9C%AC%E7%BB%9F%E7%AD%B9%E5%9C%B0%E5%8C%BA%22%7D%2C%22eoc005_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E4%B8%8D%E5%BB%B6%E8%BF%9F%22%7D%2C%22qdbz_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac028_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E9%9D%9E%E5%86%9C%E6%B0%91%E5%B7%A5%22%7D%7D", socialReq.Identitycard, socialReq.Name.ToUrlEncode(), socialReq.Password);
                #endregion
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    ContentType = "application/x-www-form-urlencoded",
                    URL = Url,
                    Host = "220.191.229.142:7080",
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header["x-requested-with"] = "XMLHttpRequest";
                httpItem.Header["request-type"] = "Ajax";
                httpResult = httpHelper.GetHtml(httpItem);
                string specialStr = CommonFun.GetMidStr(httpResult.Html.Replace("\\", ""), "grid1','", "',true)");
                List<Pujiangspecial> specialArray = jsonParser.DeserializeObject<List<Pujiangspecial>>(specialStr);
                foreach (var item in specialArray)
                {
                    Res.SpecialPaymentType += item.aae140 + "(" + item.aac008 + "," + item.aac031 + ")" + ";";
                    if (item.aae140 == "企业养老" || item.aae140 == "事业养老")
                    {
                        Res.SocialInsuranceBase = item.aic020;
                        Res.PaymentMonths = item.aic001;
                        Res.CompanyName = item.aab004;
                    }
                }
                #endregion
                #region 第三步，查询明细

                Url = baseUrl + "radowAction.do?method=doEvent";
                #region postdata
                postdata = String.Format("pageModel=pages.onlinequery.OnlineQuery&eventNames=jiaofei.onclick&radow_parent_data=&rc=%7B%22n_aae135%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22{0}%22%7D%2C%22n_aac003%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22{1}%22%7D%2C%22n_pw%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22{2}%22%7D%2C%22search%22%3A%7B%22type%22%3A%22button%22%2C%22data%22%3A%22%22%7D%2C%22aac019%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%220%22%7D%2C%22eac076%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%2200%22%7D%2C%22eac065%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%220%22%7D%2C%22aaa152%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%2200%22%7D%2C%22aaa154%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22000%22%7D%2C%22akc021%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%2211%22%7D%2C%22aac054%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac022%22%3A%7B%22type%22%3A%22numberfield%22%2C%22data%22%3A%220%22%7D%2C%22aac023%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac024%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac069%22%3A%7B%22type%22%3A%22numberfield%22%2C%22data%22%3A%220%22%7D%2C%22aac070%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac071%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac030%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%220%22%7D%2C%22aaa029%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%221%22%7D%2C%22aae135%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22{0}%22%7D%2C%22aac003%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22{1}%22%7D%2C%22aac004%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%222%22%7D%2C%22aac006%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%221977-06-26%22%7D%2C%22aac005%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%2201%22%7D%2C%22aac007%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%222002-01-01%22%7D%2C%22eac101%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae005_1%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%220579%22%7D%2C%22aae005_2%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%2284112317%22%7D%2C%22aac009%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%2211%22%7D%2C%22aac010%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac029%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%220%22%7D%2C%22aab301%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22330726%22%7D%2C%22eab009%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22eab030%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac014%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac015%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac020%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac017%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac012%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac011%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aae147%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%221%22%7D%2C%22eoc005%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%222%22%7D%2C%22eoc006%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22taxno%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22qdbz%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22yhhh%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22yhzh%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae159%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae007%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac028%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%220%22%7D%2C%22aae006%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%E6%B5%A6%E6%B1%9F%22%7D%2C%22aae013%22%3A%7B%22type%22%3A%22textarea%22%2C%22data%22%3A%22%22%7D%2C%22aae140s%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aae078%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22eac002%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aaa027s%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aae003beg%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aae003end%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22jiaofei%22%3A%7B%22type%22%3A%22button%22%2C%22data%22%3A%22%22%7D%2C%22grid3%22%3A%7B%22type%22%3A%22editorgrid%22%2C%22data%22%3A%22%5B%5D%22%7D%2C%22asc%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22desc%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22columns%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22groupBy%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22showGroups%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22s_grid3%22%3A%7B%22type%22%3A%22editorgrid%22%2C%22data%22%3A%22%5B%5D%22%7D%2C%22grid1%22%3A%7B%22type%22%3A%22editorgrid%22%2C%22cueRowIndex%22%3A%220%22%2C%22data%22%3A%22%5B%5C%22%5C%5C%5C%22%7B%5C%5C%5C%5C%5C%5C%5C%22aaa027%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22330726%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab004%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%E5%8A%B3%E5%8A%A1%E8%BE%93%E5%87%BA%E5%8A%9E%E5%85%AC%E5%AE%A4(%E4%B8%AD%E5%9B%BD%E9%93%B6%E8%A1%8C)%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab033%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab050%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aac001%5C%5C%5C%5C%5C%5C%5C%22%3A19590%2C%5C%5C%5C%5C%5C%5C%5C%22aac008%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%221%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac031%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%223%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac033%5C%5C%5C%5C%5C%5C%5C%22%3A20020101%2C%5C%5C%5C%5C%5C%5C%5C%22aac050%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae003%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae011%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae014%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae030%5C%5C%5C%5C%5C%5C%5C%22%3A200605%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031%5C%5C%5C%5C%5C%5C%5C%22%3A201205%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae035%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222006-05-24%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac02%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac20%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea30%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea31%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae041%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae042%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae140%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2210%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae160%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae200%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae206%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae300%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaz001%5C%5C%5C%5C%5C%5C%5C%22%3A2704%2C%5C%5C%5C%5C%5C%5C%5C%22aaz157%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aaz159%5C%5C%5C%5C%5C%5C%5C%22%3A26742%2C%5C%5C%5C%5C%5C%5C%5C%22aaz289%5C%5C%5C%5C%5C%5C%5C%22%3A1002%2C%5C%5C%5C%5C%5C%5C%5C%22aaz308%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic001%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic020%5C%5C%5C%5C%5C%5C%5C%22%3A7650%2C%5C%5C%5C%5C%5C%5C%5C%22aic162%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac066%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac070%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%220%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac086%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac114%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eac115%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eaz076%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eaz132%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22ezd001%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22opseno%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%7D%5C%5C%5C%22%5C%22%2C%5C%22%5C%5C%5C%22%7B%5C%5C%5C%5C%5C%5C%5C%22aaa027%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22330726%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab004%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%E4%B8%AD%E5%9B%BD%E9%93%B6%E8%A1%8C%E8%82%A1%E4%BB%BD%E6%9C%89%E9%99%90%E5%85%AC%E5%8F%B8%E6%B5%A6%E6%B1%9F%E5%8E%BF%E6%94%AF%E8%A1%8C%EF%BC%88%E8%A1%8C%E5%91%98%EF%BC%89%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab033%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab050%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aac001%5C%5C%5C%5C%5C%5C%5C%22%3A19590%2C%5C%5C%5C%5C%5C%5C%5C%22aac008%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%221%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%221%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac031%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%221%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac033%5C%5C%5C%5C%5C%5C%5C%22%3A20011201%2C%5C%5C%5C%5C%5C%5C%5C%22aac050%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae003%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae011%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae014%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae030%5C%5C%5C%5C%5C%5C%5C%22%3A201205%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae035%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222012-05-07%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac02%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac20%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea30%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea31%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae041%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae042%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae140%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2220%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae160%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae200%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae206%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae300%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaz001%5C%5C%5C%5C%5C%5C%5C%22%3A6046%2C%5C%5C%5C%5C%5C%5C%5C%22aaz157%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aaz159%5C%5C%5C%5C%5C%5C%5C%22%3A638357%2C%5C%5C%5C%5C%5C%5C%5C%22aaz289%5C%5C%5C%5C%5C%5C%5C%22%3A2001%2C%5C%5C%5C%5C%5C%5C%5C%22aaz308%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic001%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic020%5C%5C%5C%5C%5C%5C%5C%22%3A4030%2C%5C%5C%5C%5C%5C%5C%5C%22aic162%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac066%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac070%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%220%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac086%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac114%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eac115%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eaz076%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eaz132%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22ezd001%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22opseno%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%7D%5C%5C%5C%22%5C%22%2C%5C%22%5C%5C%5C%22%7B%5C%5C%5C%5C%5C%5C%5C%22aaa027%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22330726%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab004%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%E4%B8%AD%E5%9B%BD%E9%93%B6%E8%A1%8C%E8%82%A1%E4%BB%BD%E6%9C%89%E9%99%90%E5%85%AC%E5%8F%B8%E6%B5%A6%E6%B1%9F%E5%8E%BF%E6%94%AF%E8%A1%8C%EF%BC%88%E8%A1%8C%E5%91%98%EF%BC%89%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab033%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab050%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aac001%5C%5C%5C%5C%5C%5C%5C%22%3A19590%2C%5C%5C%5C%5C%5C%5C%5C%22aac008%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%221%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%221%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac031%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%221%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac033%5C%5C%5C%5C%5C%5C%5C%22%3A20011201%2C%5C%5C%5C%5C%5C%5C%5C%22aac050%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae003%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae011%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae014%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae030%5C%5C%5C%5C%5C%5C%5C%22%3A201205%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae035%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222012-05-07%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac02%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac20%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea30%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea31%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae041%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae042%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae140%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2221%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae160%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae200%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae206%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae300%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaz001%5C%5C%5C%5C%5C%5C%5C%22%3A6046%2C%5C%5C%5C%5C%5C%5C%5C%22aaz157%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aaz159%5C%5C%5C%5C%5C%5C%5C%22%3A685812%2C%5C%5C%5C%5C%5C%5C%5C%22aaz289%5C%5C%5C%5C%5C%5C%5C%22%3A2101%2C%5C%5C%5C%5C%5C%5C%5C%22aaz308%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic001%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic020%5C%5C%5C%5C%5C%5C%5C%22%3A55%2C%5C%5C%5C%5C%5C%5C%5C%22aic162%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac066%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac070%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%220%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac086%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac114%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eac115%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eaz076%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eaz132%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22ezd001%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22opseno%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%7D%5C%5C%5C%22%5C%22%2C%5C%22%5C%5C%5C%22%7B%5C%5C%5C%5C%5C%5C%5C%22aaa027%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22330726%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab004%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%E5%8A%B3%E5%8A%A1%E8%BE%93%E5%87%BA%E5%8A%9E%E5%85%AC%E5%AE%A4(%E4%B8%AD%E5%9B%BD%E9%93%B6%E8%A1%8C)%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab033%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab050%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aac001%5C%5C%5C%5C%5C%5C%5C%22%3A19590%2C%5C%5C%5C%5C%5C%5C%5C%22aac008%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%221%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac031%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%223%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac033%5C%5C%5C%5C%5C%5C%5C%22%3A20020101%2C%5C%5C%5C%5C%5C%5C%5C%22aac050%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae003%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae011%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae014%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae030%5C%5C%5C%5C%5C%5C%5C%22%3A200605%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031%5C%5C%5C%5C%5C%5C%5C%22%3A201205%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae035%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222006-05-24%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac02%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac20%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea30%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea31%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae041%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae042%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae140%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2230%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae160%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae200%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae206%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae300%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaz001%5C%5C%5C%5C%5C%5C%5C%22%3A2704%2C%5C%5C%5C%5C%5C%5C%5C%22aaz157%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aaz159%5C%5C%5C%5C%5C%5C%5C%22%3A26741%2C%5C%5C%5C%5C%5C%5C%5C%22aaz289%5C%5C%5C%5C%5C%5C%5C%22%3A3001%2C%5C%5C%5C%5C%5C%5C%5C%22aaz308%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic001%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic020%5C%5C%5C%5C%5C%5C%5C%22%3A7650%2C%5C%5C%5C%5C%5C%5C%5C%22aic162%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac066%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac070%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%220%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac086%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac114%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eac115%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eaz076%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eaz132%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22ezd001%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22opseno%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%7D%5C%5C%5C%22%5C%22%2C%5C%22%5C%5C%5C%22%7B%5C%5C%5C%5C%5C%5C%5C%22aaa027%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22330726%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab004%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%E5%8A%B3%E5%8A%A1%E8%BE%93%E5%87%BA%E5%8A%9E%E5%85%AC%E5%AE%A4(%E4%B8%AD%E5%9B%BD%E9%93%B6%E8%A1%8C)%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab033%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab050%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aac001%5C%5C%5C%5C%5C%5C%5C%22%3A19590%2C%5C%5C%5C%5C%5C%5C%5C%22aac008%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%221%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac031%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%223%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac033%5C%5C%5C%5C%5C%5C%5C%22%3A20020101%2C%5C%5C%5C%5C%5C%5C%5C%22aac050%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae003%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae011%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae014%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae030%5C%5C%5C%5C%5C%5C%5C%22%3A200605%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031%5C%5C%5C%5C%5C%5C%5C%22%3A201205%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae035%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222006-05-24%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac02%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac20%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea30%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea31%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae041%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae042%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae140%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2240%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae160%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae200%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae206%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae300%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaz001%5C%5C%5C%5C%5C%5C%5C%22%3A2704%2C%5C%5C%5C%5C%5C%5C%5C%22aaz157%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aaz159%5C%5C%5C%5C%5C%5C%5C%22%3A26740%2C%5C%5C%5C%5C%5C%5C%5C%22aaz289%5C%5C%5C%5C%5C%5C%5C%22%3A4001%2C%5C%5C%5C%5C%5C%5C%5C%22aaz308%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic001%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic020%5C%5C%5C%5C%5C%5C%5C%22%3A7650%2C%5C%5C%5C%5C%5C%5C%5C%22aic162%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac066%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac070%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%220%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac086%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac114%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eac115%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eaz076%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eaz132%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22ezd001%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22opseno%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%7D%5C%5C%5C%22%5C%22%2C%5C%22%5C%5C%5C%22%7B%5C%5C%5C%5C%5C%5C%5C%22aaa027%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22330726%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab004%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%E4%B8%AD%E5%9B%BD%E9%93%B6%E8%A1%8C%E8%82%A1%E4%BB%BD%E6%9C%89%E9%99%90%E5%85%AC%E5%8F%B8%E6%B5%A6%E6%B1%9F%E5%8E%BF%E6%94%AF%E8%A1%8C%EF%BC%88%E8%A1%8C%E5%91%98%EF%BC%89%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab033%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab050%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aac001%5C%5C%5C%5C%5C%5C%5C%22%3A19590%2C%5C%5C%5C%5C%5C%5C%5C%22aac008%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%221%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%221%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac031%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%221%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac033%5C%5C%5C%5C%5C%5C%5C%22%3A20020101%2C%5C%5C%5C%5C%5C%5C%5C%22aac050%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae003%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae011%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae014%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae030%5C%5C%5C%5C%5C%5C%5C%22%3A201205%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae035%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222012-05-07%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac02%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac20%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea30%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea31%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae041%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae042%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae140%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2250%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae160%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae200%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae206%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae300%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaz001%5C%5C%5C%5C%5C%5C%5C%22%3A6046%2C%5C%5C%5C%5C%5C%5C%5C%22aaz157%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aaz159%5C%5C%5C%5C%5C%5C%5C%22%3A737873%2C%5C%5C%5C%5C%5C%5C%5C%22aaz289%5C%5C%5C%5C%5C%5C%5C%22%3A5001%2C%5C%5C%5C%5C%5C%5C%5C%22aaz308%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic001%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic020%5C%5C%5C%5C%5C%5C%5C%22%3A2420%2C%5C%5C%5C%5C%5C%5C%5C%22aic162%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac066%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac070%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%220%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac086%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac114%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eac115%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eaz076%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eaz132%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22ezd001%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22opseno%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%7D%5C%5C%5C%22%5C%22%5D%22%7D%2C%22grid111%22%3A%7B%22type%22%3A%22editorgrid%22%2C%22data%22%3A%22%5B%5D%22%7D%2C%22grid11%22%3A%7B%22type%22%3A%22editorgrid%22%2C%22data%22%3A%22%5B%5C%22%5C%5C%5C%22%7B%5C%5C%5C%5C%5C%5C%5C%22aaa027%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab004%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab033%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab050%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aac001%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aac008%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac031%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac033%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aac050%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2201%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae003%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae011%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22FLP%20%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae014%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae030%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae035%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2220020101%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222002-03-26%2014%3A36%3A45.0%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac02%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac20%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea30%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea31%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae041%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae042%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae140%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2210%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae160%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2210%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae200%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae206%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae300%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaz001%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aaz157%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aaz159%5C%5C%5C%5C%5C%5C%5C%22%3A26742%2C%5C%5C%5C%5C%5C%5C%5C%22aaz289%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aaz308%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic001%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic020%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic162%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac066%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac070%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac086%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac114%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eac115%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eaz076%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eaz132%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22ezd001%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22opseno%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%7D%5C%5C%5C%22%5C%22%2C%5C%22%5C%5C%5C%22%7B%5C%5C%5C%5C%5C%5C%5C%22aaa027%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab004%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab033%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab050%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aac001%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aac008%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac031%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac033%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aac050%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2204%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae003%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae011%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22HWL%20%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae014%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae030%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae035%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2220060501%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222006-05-24%2014%3A09%3A46.0%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac02%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac20%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea30%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea31%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae041%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae042%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae140%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2210%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae160%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2240%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae200%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae206%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae300%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaz001%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aaz157%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aaz159%5C%5C%5C%5C%5C%5C%5C%22%3A26742%2C%5C%5C%5C%5C%5C%5C%5C%22aaz289%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aaz308%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic001%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic020%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic162%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac066%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac070%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac086%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac114%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eac115%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eaz076%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eaz132%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22ezd001%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22opseno%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%7D%5C%5C%5C%22%5C%22%2C%5C%22%5C%5C%5C%22%7B%5C%5C%5C%5C%5C%5C%5C%22aaa027%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab004%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab033%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab050%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aac001%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aac008%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac031%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac033%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aac050%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2217%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae003%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae011%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22rzl%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae014%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae030%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae035%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2220120216%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222012-02-16%2016%3A40%3A35.0%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac02%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac20%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea30%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea31%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae041%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae042%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae140%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2210%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae160%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22B1%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae200%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae206%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae300%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaz001%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aaz157%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aaz159%5C%5C%5C%5C%5C%5C%5C%22%3A26742%2C%5C%5C%5C%5C%5C%5C%5C%22aaz289%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aaz308%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic001%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic020%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic162%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac066%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac070%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac086%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac114%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eac115%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eaz076%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eaz132%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22ezd001%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22opseno%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%7D%5C%5C%5C%22%5C%22%2C%5C%22%5C%5C%5C%22%7B%5C%5C%5C%5C%5C%5C%5C%22aaa027%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaa121_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab004%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab033%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aab050%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aac001%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aac008%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac031%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aac033%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aac050%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2205%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae003%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae011%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22wssb%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae013%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%E7%BD%91%E4%B8%8A%E7%94%B3%E6%8A%A5%E4%B8%AD%E6%96%AD%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae014%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae030%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae030_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea30%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae031_ea31%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae035%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2220120505%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%222012-05-05%2007%3A51%3A15.0%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac02%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ac20%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea30%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae036_ea31%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22aae041%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae042%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae140%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2210%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae160%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%2250%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aae200%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae206%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aae300%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22aaz001%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aaz157%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aaz159%5C%5C%5C%5C%5C%5C%5C%22%3A26742%2C%5C%5C%5C%5C%5C%5C%5C%22aaz289%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aaz308%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic001%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic020%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22aic162%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac066%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac070%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac086%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eac114%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eac115%5C%5C%5C%5C%5C%5C%5C%22%3Anull%2C%5C%5C%5C%5C%5C%5C%5C%22eaz076%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22eaz132%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22ezd001%5C%5C%5C%5C%5C%5C%5C%22%3A%5C%5C%5C%5C%5C%5C%5C%22%5C%5C%5C%5C%5C%5C%5C%22%2C%5C%5C%5C%5C%5C%5C%5C%22opseno%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac02%5C%5C%5C%5C%5C%5C%5C%22%3A0%2C%5C%5C%5C%5C%5C%5C%5C%22opseno_ac20%5C%5C%5C%5C%5C%5C%5C%22%3A0%7D%5C%5C%5C%22%5C%22%5D%22%7D%2C%22ekb029%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%220.13%22%7D%2C%22ekb028%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22141.05%22%7D%2C%22medicalMoney%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22141.18%22%7D%2C%22eac005%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%220%22%7D%2C%22aae100%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%221%22%7D%2C%22eac054%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22yh%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%E4%B8%AD%E5%9B%BD%E9%93%B6%E8%A1%8C%E9%87%91%E5%8D%8E%E5%88%86%E8%A1%8C%EF%BC%88%E6%B5%A6%E6%B1%9F%E5%8E%BF%E8%8C%83%E5%9B%B4%EF%BC%89%22%7D%2C%22grid3Data%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22s_grid3Data%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22grid1Data%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22grid111Data%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22grid11Data%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22akc021_%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%2211%22%7D%2C%22aac001%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%2219590%22%7D%2C%22aab401%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%22%7D%2C%22radow_parent_data%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae140s_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aae078_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22eac002_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aaa027s_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aae100_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%9C%89%E6%95%88%22%7D%2C%22eac054_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac019_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%97%A0%22%7D%2C%22eac076_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E9%9D%9E%E7%89%B9%E6%AE%8A%E4%BA%BA%E5%91%98%22%7D%2C%22aaa152_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%97%A0%22%7D%2C%22aaa154_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%97%A0%E8%8D%A3%E8%AA%89%E7%BA%A7%E5%88%AB%22%7D%2C%22akc021_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%99%AE%E9%80%9A%E4%BC%81%E4%B8%9A%E5%9C%A8%E8%81%8C(%E5%BB%BA%E8%B4%A6)%22%7D%2C%22aac030_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E5%90%A6%22%7D%2C%22aaa029_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E5%85%AC%E6%B0%91%E8%BA%AB%E4%BB%BD%E8%AF%81%22%7D%2C%22aac004_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E5%A5%B3%E6%80%A7%22%7D%2C%22aac005_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%B1%89%E6%97%8F%22%7D%2C%22aac009_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%9C%AC%E5%9C%B0%E9%9D%9E%E5%86%9C%E4%B8%9A%E6%88%B7%E5%8F%A3%EF%BC%88%E6%9C%AC%E5%9C%B0%E5%9F%8E%E9%95%87%EF%BC%89%22%7D%2C%22aac029_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%96%B0%E4%BA%BA%22%7D%2C%22aab301_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%B5%A6%E6%B1%9F%E5%8E%BF%22%7D%2C%22eab009_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22eab030_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac014_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac015_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac020_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac017_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac012_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac011_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aae147_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E6%9C%AC%E7%BB%9F%E7%AD%B9%E5%9C%B0%E5%8C%BA%22%7D%2C%22eoc005_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E4%B8%8D%E5%BB%B6%E8%BF%9F%22%7D%2C%22qdbz_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac028_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E9%9D%9E%E5%86%9C%E6%B0%91%E5%B7%A5%22%7D%7D", socialReq.Identitycard, socialReq.Name.ToUrlEncode(), socialReq.Password);
                #endregion
                httpItem = new HttpItem()
                {
                    Accept = "*/*",
                    ContentType = "application/x-www-form-urlencoded",
                    URL = Url,
                    Host = "220.191.229.142:7080",
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header["x-requested-with"] = "XMLHttpRequest";
                httpItem.Header["request-type"] = "Ajax";
                httpResult = httpHelper.GetHtml(httpItem);
                string detailStr = CommonFun.GetMidStr(httpResult.Html.Replace("\\", ""), "grid3','", "',true");
                List<PujiangDetais> detalList = jsonParser.DeserializeObject<List<PujiangDetais>>(detailStr);
                //[['10','企业养老'],['11','事业养老'],['13','被征地养老'],['20','基本医疗'],['21','大病'],['22','公务员补助'],['26','新型农村合作医疗'],['30','工伤'],['40','生育'],['50','失业']]
                string[] saveType = { "10", "11", "20", "50", "30", "40", "21", "22" };
                string insuranceTime = string.Empty;
                foreach (PujiangDetais socia in detalList)
                {
                    if (!saveType.Contains(socia.aae140)) continue;
                    for (int i = 0; i < socia.eac003; i++)
                    {
                        insuranceTime = DateTime.ParseExact(socia.aae003, "yyyyMM", null).AddMonths(i).ToString(Consts.DateFormatString7);
                        detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == insuranceTime);
                        bool isSave = false;
                        if (detailRes == null)
                        {
                            isSave = true;
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.Name = Res.Name;
                            detailRes.IdentityCard = Res.IdentityCard;
                            detailRes.CompanyName = socia.aab004;
                            detailRes.PayTime = socia.aae079;
                            detailRes.SocialInsuranceTime = insuranceTime;
                            detailRes.SocialInsuranceBase = socia.aic020;
                            //已到账
                            if (socia.aae078 == "1")
                            {
                                detailRes.PaymentFlag = socia.aaa115;
                                switch (detailRes.PaymentFlag)
                                {
                                    case ServiceConsts.SocialSecurity_PaymentFlag_Adjust:
                                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                                        break;
                                    case ServiceConsts.SocialSecurity_PaymentFlag_Normal:
                                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                                        break;
                                    case ServiceConsts.SocialSecurity_PaymentFlag_Back:
                                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Back;
                                        break;
                                }
                            }
                            else
                            {
                                detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                                detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Adjust;
                            }
                        }
                        switch (socia.aae140)
                        {
                            case "10":
                            case "11":
                                detailRes.PensionAmount += socia.aae022;
                                detailRes.CompanyPensionAmount += socia.aae020;
                                detailRes.SocialInsuranceBase = socia.aic020;
                                break;
                            case "20":
                                detailRes.MedicalAmount += socia.aae022;
                                detailRes.CompanyMedicalAmount += socia.aae020;
                                detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : socia.aic020;
                                break;
                            case "50":
                                detailRes.UnemployAmount += socia.aae058;
                                break;
                            case "30":
                                detailRes.EmploymentInjuryAmount += socia.aae058;
                                break;
                            case "40":
                                detailRes.MaternityAmount += socia.aae058;
                                break;
                            case "21":
                                detailRes.IllnessMedicalAmount += socia.aae058;
                                break;
                            case "22":
                                detailRes.CivilServantMedicalAmount += socia.aae058;
                                break;
                        }
                        detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : socia.aic020;
                        if (!isSave) continue;
                        Res.Details.Add(detailRes);
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
        /// <summary>
        /// 人员参保信息实体
        /// </summary>
        internal class Pujiangspecial
        {
            private string _aae140 = string.Empty;
            /// <summary>
            /// 险种
            /// </summary>
            public string aae140
            {
                get { return _aae140; }
                set
                {
                    switch (value)
                    {
                        case "10":
                            _aae140 = "企业养老";
                            break;
                        case "11":
                            _aae140 = "事业养老";
                            break;
                        case "13":
                            _aae140 = "被征地养老";
                            break;
                        case "20":
                            _aae140 = "基本医疗";
                            break;
                        case "21":
                            _aae140 = "大病";
                            break;
                        case "22":
                            _aae140 = "公务员补助";
                            break;
                        case "26":
                            _aae140 = "新型农村合作医疗";
                            break;
                        case "30":
                            _aae140 = "工伤";
                            break;
                        case "40":
                            _aae140 = "生育";
                            break;
                        case "50":
                            _aae140 = "失业";
                            break;
                    }
                }
            }

            private string _aac008 = string.Empty;
            /// <summary>
            /// 人员参保状态
            /// </summary>
            public string aac008
            {
                get { return _aac008; }
                set
                {
                    switch (value)
                    {
                        case "1":
                            _aac008 = "正常参保";
                            break;
                        default:
                            _aac008 = "终止参保";
                            break;

                    }
                }

            }

            private string _aac031 = string.Empty;
            /// <summary>
            /// 缴费状态
            /// </summary>
            public string aac031
            {
                get { return _aac031; }
                set
                {
                    switch (value)
                    {
                        case "1":
                            _aac031 = "参保缴费";
                            break;
                        default:
                            _aac031 = "停止缴费";
                            break;

                    }
                }

            }

            /// <summary>
            /// 首次参保时间
            /// </summary>
            public string aac033 { get; set; }
            /// <summary>
            /// 缴费基数
            /// </summary>
            public decimal aic020 { get; set; }
            /// <summary>
            /// 视同缴费月数
            /// </summary>
            public int aic001 { get; set; }
            /// <summary>
            /// 参保单位
            /// </summary>
            public string aab004 { get; set; }
        }
        /// <summary>
        /// 缴费明细
        /// </summary>
        internal class PujiangDetais
        {

            /// <summary>
            /// 险种类型
            /// </summary>
            public string aae140 { get; set; }
            /// <summary>
            /// 应缴年月
            /// </summary>
            public string aae003 { get; set; }
            /// <summary>
            /// 单位名称
            /// </summary>
            public string aab004 { get; set; }
            /// <summary>
            /// 到账标志[1:已到账]
            /// </summary>
            public string aae078 { get; set; }
            /// <summary>
            /// 缴费基数
            /// </summary>
            public decimal aic020 { get; set; }
            /// <summary>
            /// 缴费月数
            /// </summary>
            public int eac003 { get; set; }
            /// <summary>
            /// 总金额
            /// </summary>
            public decimal aae058 { get; set; }
            /// <summary>
            /// 单位应缴金额
            /// </summary>
            public decimal aae020 { get; set; }
            /// <summary>
            /// 个人应缴金额
            /// </summary>
            public decimal aae022 { get; set; }

            private string _aaa115 = string.Empty;

            /// <summary>
            /// 应缴类型
            /// </summary>
            public string aaa115
            {
                get { return _aaa115; }
                set
                {
                    switch (value)
                    {
                        case "1":
                            _aaa115 = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            break;
                        case "8":
                        case "21":
                        case "22":
                        case "23":
                        case "24":
                        case "89":
                            _aaa115 = ServiceConsts.SocialSecurity_PaymentFlag_Back;
                            break;
                        default:
                            _aaa115 = ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                            break;
                    }
                }
            }
            /// <summary>
            /// 到账日期
            /// </summary>
            public string aae079 { get; set; }
            /// <summary>
            /// 备注
            /// </summary>
            public string aae013 { get; set; }
        }
    }
}
