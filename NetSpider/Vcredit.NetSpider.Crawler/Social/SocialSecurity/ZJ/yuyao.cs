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
    public class yuyao : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://183.136.195.143:8080/sionline/";
        string socialCity = "zj_yuyao";

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
        /// <summary>
        ///  将各种保险对应的页面所需信息存入PageHash
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, new string[] { "yanglao" });
            PageHash.Add(InfoType.医疗保险, new string[] { "yiliao" });
            PageHash.Add(InfoType.失业保险, new string[] { "shiye" });
            PageHash.Add(InfoType.工伤保险, new string[] { "gongshang" });
            PageHash.Add(InfoType.生育保险, new string[] { "shengyu" });
        }
        private void GetAllDetail(InfoType type, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            List<string> results = new List<string>();
            int pages = 1;
            int currentPage = 1;
            do
            {
                Url = baseUrl + string.Format("{0}_jiaofei.php?page={1}", ((string[])PageHash[type])[0], currentPage);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (pages == 1)
                {
                    pages = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='pages']", "value")[0].ToInt(0);
                }
                results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table_text']/tr[position()>1]", "", true));
                currentPage++;
            } while (currentPage <= pages);
            foreach (string item in results)
            {
                SocialSecurityDetailQueryRes detailRes = null;
                var tdRow = HtmlParser.GetResultFromParser(item, "//th", "");
                int tdRowCount = 0;
                switch (type)
                {
                    case InfoType.养老保险:
                    case InfoType.医疗保险:
                        tdRowCount = 9;
                        break;
                    case InfoType.失业保险:
                        tdRowCount = 8;
                        break;
                    case InfoType.工伤保险:
                    case InfoType.生育保险:
                        tdRowCount = 6;
                        break;
                }
                if (tdRow.Count < tdRowCount) continue;
                if (tdRow[1] == "补充医疗") continue;
                detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == tdRow[2]);
                bool isSave = false;
                if (detailRes == null)
                {
                    isSave = true;
                    detailRes = new SocialSecurityDetailQueryRes();
                    detailRes.Name = Res.Name;
                    detailRes.CompanyName = tdRow[0];
                    detailRes.PayTime = tdRow[2];
                    detailRes.SocialInsuranceTime = tdRow[2];
                    detailRes.PaymentFlag = tdRow[tdRowCount - 1] == "已实缴" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                    detailRes.PaymentType = tdRow[tdRowCount - 1] == "已实缴" ? ServiceConsts.SocialSecurity_PaymentType_Normal : ServiceConsts.SocialSecurity_PaymentType_Adjust;
                }
                switch (type)
                {
                    case InfoType.养老保险:
                        detailRes.CompanyPensionAmount += tdRow[6].ToDecimal(0) + tdRow[7].ToDecimal(0);
                        detailRes.PensionAmount += tdRow[4].ToDecimal(0);
                        detailRes.SocialInsuranceBase = tdRow[3].ToDecimal(0);
                        break;
                    case InfoType.医疗保险:
                        detailRes.CompanyMedicalAmount += tdRow[6].ToDecimal(0) + tdRow[7].ToDecimal(0);
                        detailRes.MedicalAmount += tdRow[4].ToDecimal(0);
                        break;
                    case InfoType.失业保险:
                        detailRes.UnemployAmount += tdRow[4].ToDecimal(0) + tdRow[6].ToDecimal(0);
                        break;
                    case InfoType.工伤保险:
                        detailRes.EmploymentInjuryAmount += tdRow[4].ToDecimal(0);
                        break;
                    case InfoType.生育保险:
                        detailRes.MaternityAmount += tdRow[4].ToDecimal(0);
                        break;
                }
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
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
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
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
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
                if (socialReq.Password.IsEmpty() || socialReq.Identitycard.IsEmpty() || socialReq.Name.IsEmpty() || socialReq.Vercode.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步，校验

                Url = baseUrl + "register/RegisterServlet";
                string inParams = "{\"psname\":\"" + socialReq.Name + "\", \"iscode\":\"" + socialReq.Identitycard + "\", \"verifycode\":\"" + socialReq.Vercode + "\"}";
                postdata = String.Format("inParams={0}", inParams.ToUrlEncode(Encoding.GetEncoding("gbk")));
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
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (httpResult.Html.Trim().ToLower() != "true")
                {
                    Res.StatusDescription = httpResult.Html.Trim().ToTrim("0");
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion
                #region 第二步,登陆

                Url = baseUrl + "logonAction.do";
                postdata = String.Format("username={0}&psname={1}&passwd={2}&verifycode={3}&macAddr=", socialReq.Identitycard, socialReq.Name.ToUrlEncode(Encoding.GetEncoding("gbk")), CommonFun.GetMd5Str(socialReq.Password), socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Encoding = Encoding.GetEncoding("gb2312"),
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion
                #region 第三步,获取基本信息

                //查询菜单
                Dictionary<string, string[]> dicMenu = new Dictionary<string, string[]>();
                Url = baseUrl + "LeftMenu.jsp";
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
                dicMenu.Add("人员基本信息查询", CommonFun.GetMidStr(httpResult.Html, "人员基本信息查询','", "')").Replace("','/", "$").Split('$'));
                //dicMenu.Add("人员缴费信息查询", CommonFun.GetMidStr(httpResult.Html, "人员缴费信息查询','", "')").Replace("','/", "$").Split('$'));
                //人员基本信息查询

                Res.Name = socialReq.Name;
                Res.IdentityCard = socialReq.Identitycard;

                Url = baseUrl + dicMenu["人员基本信息查询"][1];
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
                string currentAaz001 = CommonFun.GetMidStr(httpResult.Html, "&currentAaz001=", "\";src");
                string currentAab023 = CommonFun.GetMidStr(httpResult.Html, "&currentAab023=", "\";src");
                //string functionid = dicMenu["人员基本信息查询"][0];

                string unicodeName = string.Empty;
                string[] charName = Res.Name.ToUnicode().Replace("\\u", "[").TrimStart('[').Split('[');
                foreach (string s in charName)
                {
                    unicodeName += "[" + s + "]";
                }
                Url = baseUrl + string.Format("ReportServer?reportlet=/wssb/gzcx/psbaseinfo.cpt&method=wssb.gzcx.Psbaseinfo&fr_filename=[4eba][5458][57fa][672c][4fe1][606f][67e5][8be2]{0}&currentAab301={1}&currentAaa027=&currentLoginName={1}&currentAaz001={2}&currentAab023={3}&currentAaf015=&psseno_nonem=4634239&iscode_a={1}&psname_a={4}&1=@_WPcbvFuwN1Y=_@a.iscode@_xXS9SHcqhWs=_@'{1}@_SWDwiDCBke8=_@@_rHmXV9CvNJA=_@@_WPcbvFuwN1Y=_@a.psname@_xXS9SHcqhWs=_@'{4}@_SWDwiDCBke8=_@@_rHmXV9CvNJA=_@", DateTime.Now.ToString(Consts.DateFormatString5), socialReq.Identitycard, currentAaz001, currentAab023, unicodeName);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = "__rtype__=page",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string currentSessionID = CommonFun.GetMidStr(httpResult.Html, "currentSessionID = '", "';");
                Url = baseUrl + "ReportServer";
                postdata = String.Format("op=page_content&sessionID={0}&pn=0", currentSessionID);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Encoding = Encoding.GetEncoding("gbk"),
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header["x-requested-with"] = "XMLHttpRequest";
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='x-table']//tr[position()>4]/td//td", "text");
                for (int i = 0; i <= results.Count - 4; i = i + 4)
                {
                    if (results[i] == "养老保险")
                    {
                        Res.SocialInsuranceBase = results[i + 1].ToDecimal(0);
                        Res.CompanyName = results[i + 3];
                    }
                    Res.SpecialPaymentType += results[i] + ":" + results[i + 2] + ";";
                }
                #endregion
                #region 第四步 缴费详细

                #region querySQL

                Url = baseUrl + "pages/commAction.do?method=wssb.gzcx.Psjfinfo";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string functionid = CommonFun.GetMidStr(httpResult.Html, "functionid\":\"", "\",");
                string parent = CommonFun.GetMidStr(httpResult.Html, "parent\":\"", "\",");
                //1
                Url = baseUrl + "pages/comm/commAction.do?method=doAction";
                postdata = String.Format("divForStrust(div_1)=%5B%7B%22acym_a%22%3A%22%22%2C%22acym_a_s%22%3A%22%22%2C%22pdcode_a%22%3A%22%22%2C%22pdcode_a_combo%22%3A%22%22%7D%5D&divForStrust(div_2)=%5B%5D&divForStrust(commParams)=%5B%7B%22currentEvent%22%3A%22init%22%2C%22currentOpseno%22%3Anull%2C%22currentLoginName%22%3A%22{0}%22%7D%5D&divForStrust(MDParams)=%5B%7B%22active%22%3A%22%22%2C%22auflag%22%3A%220%22%2C%22baseLocation%22%3A%22%2Fpages%2FcommAction.do%3Fmethod%3Dwssb.gzcx.Psjfinfo%22%2C%22bdyy%22%3A%22%22%2C%22cprate%22%3A%22%22%2C%22description%22%3A%22%E4%BA%BA%E5%91%98%E7%BC%B4%E8%B4%B9%E4%BF%A1%E6%81%AF%E6%9F%A5%E8%AF%A2%22%2C%22firstopen%22%3A%220%22%2C%22focanclose%22%3A%22%22%2C%22functioncode%22%3A%22%22%2C%22functiondesc%22%3A%22%22%2C%22functionid%22%3A%22{1}%22%2C%22location%22%3A%22%2Fpages%2FcommAction.do%3Fmethod%3Dwssb.gzcx.Psjfinfo%22%2C%22log%22%3A%220%22%2C%22method%22%3A%22wssb.gzcx.Psjfinfo%22%2C%22opctrl%22%3A%22000%22%2C%22orderno%22%3A1%2C%22owner%22%3A%22gwf%22%2C%22param1%22%3A%22%22%2C%22param2%22%3A%22%22%2C%22param3%22%3A%22%22%2C%22param4%22%3A%22%22%2C%22parent%22%3A%22{2}%22%2C%22proc%22%3A%22P0%22%2C%22prsource%22%3A%22%22%2C%22publicflag%22%3A%220%22%2C%22rbflag%22%3A%221%22%2C%22repid%22%3A%22%2Fwssb%2Fgzcx%2Fpsjfinfo.cpt%22%2C%22rpflag%22%3A%220%22%2C%22sysid%22%3A%221%22%2C%22title%22%3A%22%E4%BA%BA%E5%91%98%E7%BC%B4%E8%B4%B9%E4%BF%A1%E6%81%AF%E6%9F%A5%E8%AF%A2%22%2C%22type%22%3A%220%22%2C%22uptype%22%3A%22%22%2C%22vsclass%22%3A%22%22%2C%22ywlx%22%3A%22%22%2C%22zyxx%22%3A%22%22%7D%5D&divForStrust(types_div_1)=%5B%7B%22acym_a%22%3A%22number%22%2C%22acym_a_s%22%3A%22number%22%2C%22pdcode_a%22%3A%22string%22%2C%22pdcode_a_combo%22%3A%22string%22%7D%5D&divForStrust(labels_div_1)=%5B%7B%22acym_a%22%3A%22%E7%BB%93%E7%AE%97%E5%B9%B4%E6%9C%88%22%2C%22acym_a_s%22%3A%22%E5%88%B0%22%2C%22pdcode_a%22%3A%22%E9%99%A9%E7%A7%8D%22%2C%22pdcode_a_combo%22%3A%22%22%7D%5D&divForStrust(REDH_div_1)=%5B%7B%22acym_a%22%3A%22E%22%2C%22acym_a_s%22%3A%22E%22%2C%22pdcode_a%22%3A%22E%22%7D%5D&divForStrust(types_div_2)=%5B%7B%22ysym%22%3A%22string%22%2C%22pdcode%22%3A%22string%22%2C%22ftflg%22%3A%22string%22%2C%22acym%22%3A%22string%22%2C%22rewage%22%3A%22string%22%2C%22arfd%22%3A%22string%22%2C%22arcpfd%22%3A%22string%22%2C%22arpsfd%22%3A%22string%22%2C%22cpname%22%3A%22string%22%7D%5D&divForStrust(labels_div_2)=%5B%7B%22ysym%22%3A%22%E5%BA%94%E7%BC%B4%E5%B9%B4%E6%9C%88%22%2C%22pdcode%22%3A%22%E9%99%A9%E7%A7%8D%22%2C%22ftflg%22%3A%22%E5%88%B0%E8%B4%A6%E6%A0%87%E8%AE%B0%22%2C%22acym%22%3A%22%E7%BB%93%E7%AE%97%E5%B9%B4%E6%9C%88%22%2C%22rewage%22%3A%22%E7%BC%B4%E8%B4%B9%E5%9F%BA%E6%95%B0%E6%80%BB%E9%A2%9D%22%2C%22arfd%22%3A%22%E5%BA%94%E7%BC%B4%E6%80%BB%E9%87%91%E9%A2%9D%22%2C%22arcpfd%22%3A%22%E5%8D%95%E4%BD%8D%E5%BA%94%E7%BC%B4%22%2C%22arpsfd%22%3A%22%E4%B8%AA%E4%BA%BA%E5%BA%94%E7%BC%B4%22%2C%22cpname%22%3A%22%E5%8D%95%E4%BD%8D%E5%90%8D%E7%A7%B0%22%7D%5D&divForStrust(REDH_div_2)=%5B%7B%22ysym%22%3A%22D%22%2C%22pdcode%22%3A%22D%22%2C%22ftflg%22%3A%22D%22%2C%22acym%22%3A%22D%22%2C%22rewage%22%3A%22D%22%2C%22arfd%22%3A%22D%22%2C%22arcpfd%22%3A%22D%22%2C%22arpsfd%22%3A%22D%22%2C%22cpname%22%3A%22D%22%7D%5D&divForStrust(divpagetypes)=%5B%7B%22div_1%22%3A%222%22%2C%22div_2%22%3A%221%22%7D%5D", Res.IdentityCard, functionid, parent);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Encoding = Encoding.GetEncoding("utf-8"),
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header["x-requested-with"] = "XMLHttpRequest";
                httpResult = httpHelper.GetHtml(httpItem);
                string querySQL = CommonFun.GetMidStr(httpResult.Html, "div_2\\\":\\\"", "\\\"}]").ToUrlEncode(Encoding.GetEncoding("GBK"));
                //2
                Url = baseUrl + "pages/comm/commAction.do?method=doAction";
                postdata = String.Format("divForStrust(div_1)=%5B%7B%22acym_a%22%3A{0}%2C%22acym_a_s%22%3A{1}%2C%22pdcode_a%22%3A%22%22%2C%22pdcode_a_combo%22%3A%22%22%7D%5D&divForStrust(div_2)=%5B%5D&divForStrust(commParams)=%5B%7B%22currentEvent%22%3A%22query%22%2C%22currentOpseno%22%3Anull%2C%22currentLoginName%22%3A%22{2}%22%7D%5D&divForStrust(MDParams)=%5B%7B%22active%22%3A%22%22%2C%22auflag%22%3A%220%22%2C%22baseLocation%22%3A%22%2Fpages%2FcommAction.do%3Fmethod%3Dwssb.gzcx.Psjfinfo%22%2C%22bdyy%22%3A%22%22%2C%22cprate%22%3A%22%22%2C%22description%22%3A%22%E4%BA%BA%E5%91%98%E7%BC%B4%E8%B4%B9%E4%BF%A1%E6%81%AF%E6%9F%A5%E8%AF%A2%22%2C%22firstopen%22%3A%220%22%2C%22focanclose%22%3A%22%22%2C%22functioncode%22%3A%22%22%2C%22functiondesc%22%3A%22%22%2C%22functionid%22%3A%22{3}%22%2C%22location%22%3A%22%2Fpages%2FcommAction.do%3Fmethod%3Dwssb.gzcx.Psjfinfo%22%2C%22log%22%3A%220%22%2C%22method%22%3A%22wssb.gzcx.Psjfinfo%22%2C%22opctrl%22%3A%22000%22%2C%22orderno%22%3A1%2C%22owner%22%3A%22gwf%22%2C%22param1%22%3A%22%22%2C%22param2%22%3A%22%22%2C%22param3%22%3A%22%22%2C%22param4%22%3A%22%22%2C%22parent%22%3A%22{4}%22%2C%22proc%22%3A%22P0%22%2C%22prsource%22%3A%22%22%2C%22publicflag%22%3A%220%22%2C%22rbflag%22%3A%221%22%2C%22repid%22%3A%22%2Fwssb%2Fgzcx%2Fpsjfinfo.cpt%22%2C%22rpflag%22%3A%220%22%2C%22sysid%22%3A%221%22%2C%22title%22%3A%22%E4%BA%BA%E5%91%98%E7%BC%B4%E8%B4%B9%E4%BF%A1%E6%81%AF%E6%9F%A5%E8%AF%A2%22%2C%22type%22%3A%220%22%2C%22uptype%22%3A%22%22%2C%22vsclass%22%3A%22%22%2C%22ywlx%22%3A%22%22%2C%22zyxx%22%3A%22%22%7D%5D&divForStrust(divsql)=%5B%7B%22div_1%22%3A%22%40_qMLdfYvZDtE%3D_%40%22%2C%22div_2%22%3A%22{5}%22%7D%5D&divForStrust(types_div_1)=%5B%7B%22acym_a%22%3A%22number%22%2C%22acym_a_s%22%3A%22number%22%2C%22pdcode_a%22%3A%22string%22%2C%22pdcode_a_combo%22%3A%22string%22%7D%5D&divForStrust(labels_div_1)=%5B%7B%22acym_a%22%3A%22%E7%BB%93%E7%AE%97%E5%B9%B4%E6%9C%88%22%2C%22acym_a_s%22%3A%22%E5%88%B0%22%2C%22pdcode_a%22%3A%22%E9%99%A9%E7%A7%8D%22%2C%22pdcode_a_combo%22%3A%22%22%7D%5D&divForStrust(REDH_div_1)=%5B%7B%22acym_a%22%3A%22E%22%2C%22acym_a_s%22%3A%22E%22%2C%22pdcode_a%22%3A%22E%22%7D%5D&divForStrust(types_div_2)=%5B%7B%22ysym%22%3A%22string%22%2C%22pdcode%22%3A%22string%22%2C%22ftflg%22%3A%22string%22%2C%22acym%22%3A%22string%22%2C%22rewage%22%3A%22string%22%2C%22arfd%22%3A%22string%22%2C%22arcpfd%22%3A%22string%22%2C%22arpsfd%22%3A%22string%22%2C%22cpname%22%3A%22string%22%7D%5D&divForStrust(labels_div_2)=%5B%7B%22ysym%22%3A%22%E5%BA%94%E7%BC%B4%E5%B9%B4%E6%9C%88%22%2C%22pdcode%22%3A%22%E9%99%A9%E7%A7%8D%22%2C%22ftflg%22%3A%22%E5%88%B0%E8%B4%A6%E6%A0%87%E8%AE%B0%22%2C%22acym%22%3A%22%E7%BB%93%E7%AE%97%E5%B9%B4%E6%9C%88%22%2C%22rewage%22%3A%22%E7%BC%B4%E8%B4%B9%E5%9F%BA%E6%95%B0%E6%80%BB%E9%A2%9D%22%2C%22arfd%22%3A%22%E5%BA%94%E7%BC%B4%E6%80%BB%E9%87%91%E9%A2%9D%22%2C%22arcpfd%22%3A%22%E5%8D%95%E4%BD%8D%E5%BA%94%E7%BC%B4%22%2C%22arpsfd%22%3A%22%E4%B8%AA%E4%BA%BA%E5%BA%94%E7%BC%B4%22%2C%22cpname%22%3A%22%E5%8D%95%E4%BD%8D%E5%90%8D%E7%A7%B0%22%7D%5D&divForStrust(REDH_div_2)=%5B%7B%22ysym%22%3A%22D%22%2C%22pdcode%22%3A%22D%22%2C%22ftflg%22%3A%22D%22%2C%22acym%22%3A%22D%22%2C%22rewage%22%3A%22D%22%2C%22arfd%22%3A%22D%22%2C%22arcpfd%22%3A%22D%22%2C%22arpsfd%22%3A%22D%22%2C%22cpname%22%3A%22D%22%7D%5D&divForStrust(divpagetypes)=%5B%7B%22div_1%22%3A%222%22%2C%22div_2%22%3A%221%22%7D%5D", DateTime.Now.AddYears(-10).ToString("yyyy01"), DateTime.Now.ToString(Consts.DateFormatString7), Res.IdentityCard, functionid, parent, querySQL);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Encoding = Encoding.GetEncoding("utf-8"),
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header["x-requested-with"] = "XMLHttpRequest";
                httpResult = httpHelper.GetHtml(httpItem);
                querySQL = CommonFun.GetMidStr(httpResult.Html, "querySQL\\\":\\\"", "\\\",\\\"sqlType").ToUrlEncode(Encoding.GetEncoding("GBK"));
                #endregion
                int pageCount = 0;
                int pageIndex = 0;
                List<Yuyaodetails> details = new List<Yuyaodetails>();
                Url = baseUrl + "common/pageQueryAction.do?method=query";
                do
                {
                    postdata = String.Format("start={1}&limit=100&div=div_2&querySQL={0}&sqlType=SQL&rowstatus=U&limit=12", querySQL, pageIndex * 100);
                    httpItem = new HttpItem()
                    {
                        Accept = "*/*",
                        ContentType = "application/x-www-form-urlencoded",
                        URL = Url,
                        Host = "183.136.195.143:8080",
                        Method = "POST",
                        Postdata = postdata,
                        UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/7.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.3; .NET4.0C; .NET4.0E)",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpItem.Header["x-requested-with"] = "XMLHttpRequest";
                    httpItem.Header["request-type"] = "Ajax";
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (pageIndex == 0)
                    {
                        pageCount = (int)Math.Ceiling(jsonParser.GetResultFromParser(httpResult.Html, "totalCount").ToDecimal(0) / 100);
                    }
                    details.AddRange(jsonParser.DeserializeObject<List<Yuyaodetails>>(jsonParser.GetResultFromParser(httpResult.Html, "data")));
                    pageIndex++;
                } while (pageIndex < pageCount);
                details = details.OrderByDescending(o => o.pdcode == "养老保险").ToList();
                foreach (Yuyaodetails item in details)
                {
                    if (item.pdcode != "养老保险" || item.pdcode != "基本医疗") continue;
                    detailRes = Res.Details.FirstOrDefault(o => o.SocialInsuranceTime == item.ysym);
                    bool needSave = false;
                    if (detailRes == null)
                    {
                        needSave = true;
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = Res.Name;
                        detailRes.IdentityCard = Res.IdentityCard;
                        detailRes.CompanyName = item.cpname;
                        detailRes.PayTime = item.acym;
                        detailRes.SocialInsuranceBase = item.rewage;
                        detailRes.PaymentFlag = item.ftflg == "1" ? ServiceConsts.SocialSecurity_PaymentFlag_Normal : ServiceConsts.SocialSecurity_PaymentFlag_Adjust;
                        detailRes.PaymentType = item.ftflg == "1" ? "足额到" : ServiceConsts.SocialSecurity_PaymentType_Adjust;
                    }
                    switch (item.pdcode)
                    {
                        case "养老保险":
                            detailRes.PensionAmount += item.arpsfd;
                            detailRes.CompanyPensionAmount += item.arcpfd;
                            break;
                        case "基本医疗":
                            detailRes.MedicalAmount += item.arpsfd;
                            detailRes.CompanyMedicalAmount += item.arcpfd;
                            break;
                    }
                    detailRes.SocialInsuranceBase = detailRes.SocialInsuranceBase > 0 ? detailRes.SocialInsuranceBase : item.rewage;
                    if (!needSave) continue;
                    Res.Details.Add(detailRes);
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
        /// 明细实体
        /// </summary>
        internal class Yuyaodetails
        {
            /// <summary>
            /// 应缴年月
            /// </summary>
            public string ysym { get; set; }
            /// <summary>
            /// 结算年月
            /// </summary>
            public string acym { get; set; }

            private string _pdcode = string.Empty;
            /// <summary>
            /// 保险类型
            /// </summary>
            public string pdcode
            {
                get { return _pdcode; }
                set
                {
                    switch (value)
                    {
                        case "01":
                            _pdcode = "养老保险";
                            break;
                        case "02":
                            _pdcode = "基本医疗";
                            break;
                    }
                }

            }
            /// <summary>
            /// 到账标记
            /// </summary>
            public string ftflg { get; set; }
            /// <summary>
            /// 缴费基数
            /// </summary>
            public decimal rewage { get; set; }
            /// <summary>
            /// 总缴费
            /// </summary>
            public decimal arfd { get; set; }
            /// <summary>
            /// 单位缴费
            /// </summary>
            public decimal arcpfd { get; set; }
            /// <summary>
            /// 个人缴费
            /// </summary>
            public decimal arpsfd { get; set; }
            /// <summary>
            /// 公司名称
            /// </summary>
            public string cpname { get; set; }
        }
    }
}
