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
    public class jiashan : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.zjjiashan.lss.gov.cn:28007/sionlineman/";
        string socialCity = "zj_jiashan";
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
                Url = baseUrl;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
                    Host = "www.zjjiashan.lss.gov.cn:28007",
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.93 Safari/537.36",
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

                Url = baseUrl + "verifyCode";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    //Referer = baseUrl,
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
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);

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
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录

                Url = baseUrl + "logonAction.do";
                postdata = String.Format("username={0}&sicard={1}&passwd={2}&verifycode={3}&params=%7B%27username%27%3A%27{0}%27%2C%27sicard%27%3A%27{1}%27%2C%27password%27%3A%27{2}%27%2C%27scene%27%3A%27sce%3AUSER_PASS%3BUSERNAME%27%7D&submitbtn.x=20&submitbtn.y=14", socialReq.Identitycard, socialReq.Username, CommonFun.GetMd5Str(socialReq.Password), socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = baseUrl,
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
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//script");
                //if (results.Count == 1)
                //{
                //    string msg = CommonFun.GetMidStr(results[0], "alert(\"", "\"");
                //    if (msg != "登录成功！")
                //    {
                //        Res.StatusDescription = msg;
                //        Res.StatusCode = ServiceConsts.StatusCode_fail;
                //        return Res;
                //    }
                //}
                //else
                //{
                //    Res.StatusDescription = "登录失败！";
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 基本信息
                Url = baseUrl + "radowAction.do?method=doEvent";
                postdata = string.Format("pageModel=pages.personchange.PersonAll&eventNames=query&radow_parent_data=&rc=%7B%22aac002%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22eac001%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac003%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac004%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac006%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac005%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac009%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac010%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22eac067%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac007%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac215%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac200%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac201%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac202%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac203%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22eac101%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae005%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac204%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae006%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac205%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac206%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac207%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac208%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac209%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac008%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac210%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac211%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac212%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac213%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22grid5%22%3A%7B%22type%22%3A%22grid%22%2C%22data%22%3A%22%5B%5D%22%7D%2C%22asc%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22desc%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22columns%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22aac157_2%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22{0}%22%7D%2C%22radow_parent_data%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac004_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac005_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac009_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22eac067_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac215_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac201_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac202_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac203_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac205_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac206_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac208_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac008_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac211_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac212_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%7D", socialReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string script = jsonParser.GetResultFromParser(httpResult.Html, "elementsScript");
                List<string> baselist = script.ToTrim("document.getElementById").ToTrim("odin.setSelectValue").Split(';').ToList<string>();
                Res.IdentityCard = socialReq.Identitycard;
                foreach (string item in baselist)
                {
                    if (item.Contains("aac003"))
                    {
                        Res.Name = CommonFun.GetMidStr(item, "aac003').value='", "'");
                    }
                    else if (item.Contains("aac004"))
                    {
                        Dictionary<string, string> sex_dic = new Dictionary<string, string> { { "1", "男" }, { "2", "女" } };
                        string sex = CommonFun.GetMidStr(item, "aac004','", "'");
                        if (sex_dic.ContainsKey(sex))
                        {
                            Res.Sex = sex_dic[sex];
                        }
                    }
                    else if (item.Contains("aac006"))
                    {
                        Res.BirthDate = CommonFun.GetMidStr(item, "aac006').value='", "'");
                    }
                    else if (item.Contains("aac005"))
                    {
                        Dictionary<string, string> race_dic = new Dictionary<string, string> { { "01", "汉族" }, { "02", "蒙古族" }, { "03", "回族" }, { "04", "藏族" }, { "05", "维吾尔族" }, { "06", "苗族" }, { "07", "彝族" }, { "08", "壮族" }, { "09", "布依族" }, { "10", "朝鲜族" }, { "11", "满族" }, { "12", "侗族" }, { "13", "瑶族" }, { "14", "白族" }, { "15", "土家族" }, { "16", "哈尼族" }, { "17", "哈萨克族" }, { "18", "傣族" }, { "19", "黎族" }, { "20", "傈傈族" }, { "21", "佤族" }, { "22", "畲族" }, { "23", "高山族" }, { "24", "拉祜族" }, { "25", "水族" }, { "26", "东乡族" }, { "27", "纳西族" }, { "28", "景颇族" }, { "29", "柯尔克孜族" }, { "30", "土族" }, { "31", "达翰尔族" }, { "32", "仫佬族" }, { "33", "羌族" }, { "34", "布朗族" }, { "35", "撒拉族" }, { "36", "毛南族" }, { "37", "仡佬族" }, { "38", "锡伯族" }, { "39", "阿昌族" }, { "40", "普米族" }, { "41", "塔吉克族" }, { "42", "怒族" }, { "43", "乌孜别克族" }, { "44", "俄罗斯族" }, { "45", "鄂温克族" }, { "46", "德昂族" }, { "47", "保安族" }, { "48", "裕固族" }, { "49", "京族" }, { "50", "塔塔尔族" }, { "51", "独龙族" }, { "52", "鄂伦春族" }, { "53", "赫哲族" }, { "54", "门巴族" }, { "55", "珞巴族" }, { "56", "基诺族" } };
                        string race = CommonFun.GetMidStr(item, "'aac005','", "'");
                        if (race_dic.ContainsKey(race))
                        {
                            Res.Race = race_dic[race];
                        }
                    }
                    else if (item.Contains("eac067"))
                    {
                        Dictionary<string, string> employeestatus_dic = new Dictionary<string, string> { { "14", "公务员在职" }, { "15", "参照公务员在职" }, { "16", "保险一在职" }, { "24", "公务员退休" }, { "25", "参照公务员退休" }, { "26", "保险一退休" }, { "31", "离休" }, { "32", "二乙离休" }, { "33", "县处级退休" }, { "34", "建国前老工人" }, { "35", "建国前老工人（参公）" }, { "41", "退休免交人" }, { "42", "老知青" }, { "70", "保险二在职" }, { "71", "保险二退休" }, { "99", "居民医保" } };
                        string employeestatus = CommonFun.GetMidStr(item, "'eac067','", "'");
                        if (employeestatus_dic.ContainsKey(employeestatus))
                        {
                            Res.EmployeeStatus = employeestatus_dic[employeestatus];
                        }
                    }
                    else if (item.Contains("eac101"))
                    {
                        Res.Phone = CommonFun.GetMidStr(item, "eac101').valu ='", "'");
                    }
                    else if (item.Contains("aae006"))
                    {
                        Res.Address = CommonFun.GetMidStr(item, "aae006').value='", "'");
                    }
                }
                //参保状态
                Url = baseUrl + "radowAction.do?method=doEvent&pageModel=pages.personchange.PersonAll&eventNames=grid5.dogridquery";
                postdata = string.Format("pageModel=pages.personchange.PersonAll&eventNames=grid5.dogridquery&radow_parent_data=&rc=%7B%22aac002%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22eac001%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac003%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac004%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac006%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac005%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac009%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac010%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22eac067%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac007%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac215%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac200%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac201%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac202%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac203%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22eac101%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae005%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac204%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae006%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac205%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac206%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac207%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac208%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac209%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac008%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac210%22%3A%7B%22type%22%3A%22datefield%22%2C%22data%22%3A%22%22%7D%2C%22aac211%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac212%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22%22%7D%2C%22aac213%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22grid5%22%3A%7B%22type%22%3A%22grid%22%2C%22data%22%3A%22%22%7D%2C%22asc%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22desc%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22columns%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22aac157_2%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22{0}%22%7D%2C%22radow_parent_data%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aac004_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac005_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac009_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22eac067_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac215_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac201_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac202_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac203_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac205_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac206_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac208_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac008_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac211_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%2C%22aac212_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22%E8%AF%B7%E6%82%A8%E9%80%89%E6%8B%A9...%22%7D%7D&limit=10", socialReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                List<CanBaoInfo> canbao_list = jsonParser.DeserializeObject<List<CanBaoInfo>>(jsonParser.GetResultFromParser(httpResult.Html, "data"));
                Dictionary<int, string> canbao_dic_base = new Dictionary<int, string>();
                Dictionary<int, string> canbao_dic = new Dictionary<int, string>();
                Dictionary<int, string> canbao_dic_null = new Dictionary<int, string>();
                foreach (CanBaoInfo _canbaoinfo in canbao_list)
                {
                    if (_canbaoinfo.aaz289.Contains("养老"))
                    {
                        if (_canbaoinfo.aac031 == "在职参保缴费")
                        {
                            if (!canbao_dic.ContainsKey(0))
                            {
                                canbao_dic.Add(0, _canbaoinfo.aab004);
                                canbao_dic_base.Add(0, _canbaoinfo.aae180);
                            }
                        }
                        else
                        {
                            if (!canbao_dic_null.ContainsKey(0))
                            {
                                canbao_dic_null.Add(0, _canbaoinfo.aab004);
                            }
                        }
                    }
                    else if (_canbaoinfo.aaz289.Contains("医疗"))
                    {
                        if (_canbaoinfo.aac031 == "在职参保缴费")
                        {
                            if (!canbao_dic.ContainsKey(1))
                            {
                                canbao_dic.Add(1, _canbaoinfo.aab004);
                                canbao_dic_base.Add(1, _canbaoinfo.aae180);
                            }
                        }
                        else
                        {
                            if (!canbao_dic_null.ContainsKey(1))
                            {
                                canbao_dic_null.Add(1, _canbaoinfo.aab004);
                            }
                        }
                    }
                    else if (_canbaoinfo.aaz289.Contains("失业"))
                    {
                        if (_canbaoinfo.aac031 == "在职参保缴费")
                        {
                            if (!canbao_dic.ContainsKey(2))
                            {
                                canbao_dic.Add(2, _canbaoinfo.aab004);
                                canbao_dic_base.Add(2, _canbaoinfo.aae180);
                            }
                        }
                        else
                        {
                            if (!canbao_dic_null.ContainsKey(2))
                            {
                                canbao_dic_null.Add(2, _canbaoinfo.aab004);
                            }
                        }
                    }
                    else if (_canbaoinfo.aaz289.Contains("工伤"))
                    {
                        if (_canbaoinfo.aac031 == "在职参保缴费")
                        {
                            if (!canbao_dic.ContainsKey(3))
                            {
                                canbao_dic.Add(3, _canbaoinfo.aab004);
                                canbao_dic_base.Add(3, _canbaoinfo.aae180);
                            }
                        }
                        else
                        {
                            if (!canbao_dic_null.ContainsKey(3))
                            {
                                canbao_dic_null.Add(3, _canbaoinfo.aab004);
                            }
                        }
                    }
                    else if (_canbaoinfo.aaz289.Contains("生育"))
                    {
                        if (_canbaoinfo.aac031 == "在职参保缴费")
                        {
                            if (!canbao_dic.ContainsKey(4))
                            {
                                canbao_dic.Add(4, _canbaoinfo.aab004);
                                canbao_dic_base.Add(4, _canbaoinfo.aae180);
                            }
                        }
                        else
                        {
                            if (!canbao_dic_null.ContainsKey(4))
                            {
                                canbao_dic_null.Add(4, _canbaoinfo.aab004);
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                    Res.SpecialPaymentType += string.Format("[{0}:{1}]", _canbaoinfo.aaz289, _canbaoinfo.aac031);
                }

                for (int i = 0; i <= 4; i++)
                {
                    if (canbao_dic.ContainsKey(i))
                    {
                        Res.CompanyName = canbao_dic[i];
                        Res.SocialInsuranceBase = canbao_dic_base[i].ToDecimal(0);
                    }
                }
                if (Res.CompanyName.IsEmpty())
                {
                    for (int i = 0; i <= 4; i++)
                    {
                        if (canbao_dic_null.ContainsKey(i))
                        {
                            Res.CompanyName = canbao_dic_null[i];
                        }
                    }
                }
                #endregion

                #region 明细
                Dictionary<string, string> insurance_dic = new Dictionary<string, string> { { "10", "企业养老" }, { "11", "事业养老" }, { "20", "职工基本医疗" }, { "50", "失业" }, { "30", "工伤" }, { "40", "生育" } };

                for (int i = 0; i < insurance_dic.Count; i++)
                {
                    int pageindex = 1;
                    int totalcount = 0;

                    do
                    {
                        Url = baseUrl + "radowAction.do?method=doEvent&pageModel=pages.personchange.EmpQuery&eventNames=grid5.dogridquery";
                        postdata = string.Format("pageModel=pages.personchange.EmpQuery&eventNames=grid5.dogridquery&radow_parent_data=&rc=%7B%22aae140%22%3A%7B%22type%22%3A%22combo%22%2C%22data%22%3A%22{0}%22%7D%2C%22grid5%22%3A%7B%22type%22%3A%22grid%22%2C%22data%22%3A%22%22%7D%2C%22asc%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22desc%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22columns%22%3A%7B%22type%22%3A%22menu-item%22%2C%22data%22%3A%22%22%7D%2C%22aac157%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22{1}%22%7D%2C%22radow_parent_data%22%3A%7B%22type%22%3A%22textfield%22%2C%22data%22%3A%22%22%7D%2C%22aae140_combo%22%3A%7B%22type%22%3A%22hidden%22%2C%22data%22%3A%22{2}%22%7D%7D&start={3}&limit={4}", insurance_dic.ElementAt(i).Key, socialReq.Identitycard, insurance_dic.ElementAt(i).Value.ToUrlEncode(), (pageindex - 1) * 100, pageindex * 100 - 1);
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "POST",
                            Postdata = postdata,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        if (totalcount == 0)
                        {
                            totalcount = jsonParser.GetResultFromParser(httpResult.Html, "totalCount").ToInt(0);
                        }
                        List<DetailInfo> detail_list = jsonParser.DeserializeObject<List<DetailInfo>>(jsonParser.GetResultFromParser(httpResult.Html, "data"));
                        foreach (DetailInfo _detail in detail_list)
                        {
                            detailRes = Res.Details.Where(o => o.SocialInsuranceTime == _detail.aae003).FirstOrDefault();
                            bool NeedAdd = false;
                            if (detailRes == null)
                            {
                                detailRes = new SocialSecurityDetailQueryRes();
                                detailRes.PayTime = _detail.aae079;
                                detailRes.SocialInsuranceTime = _detail.aae003;
                                detailRes.CompanyName = _detail.aab004;
                                detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                                detailRes.PaymentFlag = _detail.aae078 == "0" ? _detail.Flag : ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                                NeedAdd = true;
                            }

                            if (i == 0 || i == 1)
                            {
                                detailRes.CompanyPensionAmount = _detail.aae020.ToDecimal(0);
                                detailRes.PensionAmount = _detail.aae022.ToDecimal(0);
                            }
                            else if (i == 2)
                            {
                                detailRes.CompanyMedicalAmount = _detail.aae020.ToDecimal(0);
                                detailRes.MedicalAmount = _detail.aae022.ToDecimal(0);
                            }
                            else if (i == 3)
                            {
                                detailRes.UnemployAmount = _detail.aae058.ToDecimal(0);
                            }
                            else if (i == 4)
                            {
                                detailRes.EmploymentInjuryAmount = _detail.aae058.ToDecimal(0);
                            }
                            else if (i == 5)
                            {
                                detailRes.MaternityAmount = _detail.aae058.ToDecimal(0);
                            }

                            if (NeedAdd)
                            {
                                Res.Details.Add(detailRes);
                            }
                        }
                        pageindex++;
                    } while (totalcount == 100);
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

        class CanBaoInfo
        {
            public string aaz289 { get; set; }//险种
            public string aac031 { get; set; }//参保状态
            public string aab004 { get; set; }//单位名称
            public string aae180 { get; set; }//缴费基数
        }

        class DetailInfo
        {
            string flag = string.Empty; 
            public string Flag { get { return flag; } }
            string AAE078 = string.Empty;
            public string aae078
            {
                get { return AAE078; }
                set
                {
                    AAE078 = value;
                    switch (value)
                    {
                        case "0":
                            flag = "未到账";
                            break;
                        case "1":
                            flag = "业务足额到账";
                            break;
                        case "9":
                            flag = "财务已到账";
                            break;
                    }
                }
            }//到账标志
            
            public string aae003 { get; set; }//应缴年月
            public string aab004 { get; set; }//单位名称
            public string aae020 { get; set; }//单位应缴
            public string aae022 { get; set; }//个人应缴金额
            public string aae079 { get; set; }//到账日期
            public string aae058 { get; set; }//应缴总额
        }
    }
}
