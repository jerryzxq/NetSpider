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
using Newtonsoft.Json.Linq;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.HE
{
    public class handan : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.handanshebao.com.cn:7080/";
        string socialCity = "he_handan";
        #endregion

        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            string Url = string.Empty;
            Res.Token = token;
            try
            {
                Url = baseUrl + "ggfwweb/captchaimg?tm=1456378170167";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);

                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
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
            string personcode = string.Empty;
            string errorMsg = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(socialReq.Token);
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 登录

                Url = baseUrl + "ggfwweb/app/login";
                postdata = String.Format("j_username={0}&j_password={1}&j_captcha={2}", socialReq.Username, socialReq.Password, socialReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Encoding = Encoding.GetEncoding("GB2312"),
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var msg = jsonParser.GetResultFromParser(httpResult.Html, "appcode");
                if (msg != "0000")
                {
                    Res.StatusDescription = "姓名或身份证号错误";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                Url = baseUrl + "ggfwweb/app/curuser?_=1456382869937";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Get",
                    ContentType = "application/json; charset=utf-8",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var list = jsonParser.GetArrayFromParse(httpResult.Html, "userbussList");
                var grbh = jsonParser.GetResultFromParser(list[0], "grbh");  //个人编号
                #endregion

                #region 获取基本信息
                //获取codeList
                Url = baseUrl + "ggfwweb/app/getCodeLists";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var RaceList = jsonParser.GetArrayFromParse(httpResult.Html, "AAC005");

                //人员基本信息
                Url = baseUrl + "ggfwweb/app/ylxxlist/getYlRyjbxx?grbh=" + grbh;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Get",
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

                var dataobj = JObject.Parse(httpResult.Html);
                Res.Name = dataobj["xm"].ToString();  //姓名
                Res.Loginname = dataobj["grbh"].ToString();  // 账号
                Res.IdentityCard = dataobj["sfzh"].ToString();  //身份证号
                Res.Sex = dataobj["xb"].ToString() == "1" ? "男" : "女";  //性别
                var mz = RaceList.Where(e => jsonParser.GetResultFromParser(e, "aaa102") == dataobj["mz"].ToString());
                Res.Race = jsonParser.GetResultFromParser(mz.ToList()[0], "aaa103");  //民族
                Res.BirthDate = dataobj["csrq"].ToString();  //出生日期
                Res.EmployeeStatus = dataobj["rylb"].ToString() == "0" ? "未退休" : "离退休";  //员工状态
                Res.WorkDate = dataobj["gzrq"].ToString(); //参加工作日期


                //人员参保基本信息
                Url = baseUrl + "ggfwweb/app/ylxxlist/getYlRycbxx?grbh=" + grbh;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Get",
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
                dataobj = JObject.Parse(httpResult.Html);
                Res.CompanyNo = dataobj["dwbh"].ToString();  //单位编号
                Res.CompanyName = dataobj["dwmc"].ToString();  //单位名称
                Res.Address = dataobj["dz"].ToString();  //地址
                Res.ZipCode = dataobj["yzbm"].ToString();  //邮政编号
                Res.PaymentMonths = int.Parse(dataobj["stjfys"].ToString()); //视同缴费月数
                var ylStartDate = dataobj["bccbrq"].ToString();  //参保日期

                //获取生育保险参保日期
                Url = baseUrl + "ggfwweb/app/sypayment/getSyuRycbxx?grbh=" + grbh;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                dataobj = JObject.Parse(httpResult.Html);
                var syStartDate = dataobj["bccbrq"].ToString();  //参保日期
                #endregion

                #region 查询明细(暂无养老保险详细信息)
                var date = DateTime.Now.AddYears(-1).ToString("yyyy");
                Url = string.Format(baseUrl + "ggfwweb/app/ylxxlist/ylpersoncount?aac001={0}&aae001=" + date + "&pageno=1&pagesize=20", grbh);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                dataobj = JObject.Parse(httpResult.Html);
                var ds = jsonParser.GetResultFromParser(httpResult.Html, "content");
                Res.InsuranceTotal = jsonParser.GetResultFromParser(jsonParser.GetResultFromParser(httpResult.Html, "content").Replace("[", "").Replace("]", ""), "bkc010").ToString().ToDecimal(0);       //账号余额

                //医疗保险缴费详单
                Url = string.Format(baseUrl + "ggfwweb/app/ylxxlist/ylpaymentdetail?aac001={0}&aae041={1}&aae042={2}&_search=false&nd=1456389811811&pagesize=5000&pageno=1&sidx=&sord=asc", grbh, DateTime.Parse(ylStartDate).ToString("yyyyMM"), DateTime.Now.ToString("yyyyMM"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var ylDetailList = jsonParser.GetArrayFromParse(httpResult.Html, "content");

                //生育保险缴费详单
                Url = string.Format(baseUrl + "ggfwweb/app/sypayment/sypaymentdetail?aac001={0}&aae041={1}&aae042={2}&_search=false&nd=1456389811811&pagesize=5000&pageno=1&sidx=&sord=asc", grbh, DateTime.Parse(ylStartDate).ToString("yyyyMM"), DateTime.Now.ToString("yyyyMM"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var syDetailList = jsonParser.GetArrayFromParse(httpResult.Html, "content");


                foreach (var item in ylDetailList)
                {
                    SocialSecurityDetailQueryRes detail = new SocialSecurityDetailQueryRes();
                    var detailObj = JObject.Parse(item);

                    detail.Name = Res.Name;  //姓名
                    detail.CompanyName = Res.CompanyName;  //公司名称
                    detail.IdentityCard = Res.IdentityCard;  //身份证号
                    detail.SocialInsuranceTime = detailObj["aae003"].ToString();  //所属年月
                    detail.SocialInsuranceBase = detailObj["aae018"].ToString().ToDecimal(0);  //社保基数
                    detail.EnterAccountMedicalAmount = detailObj["aae020"].ToString().ToDecimal(0);  //社保总共缴费金额
                    detail.MedicalAmount = detailObj["aae021"].ToString().ToDecimal(0);  //个人划入医保
                    detail.CompanyMedicalAmount = detailObj["aae022"].ToString().ToDecimal(0);  //单位划入医保
                    var maternityAmountInfo=syDetailList.Where(e => jsonParser.GetResultFromParser(e, "aae002").ToString() == detailObj["aae003"].ToString());
                    detail.MaternityAmount = jsonParser.GetResultFromParser(maternityAmountInfo.ToList()[0], "aae020").ToString().ToDecimal(0);  //生育保险缴费金额;
                    detail.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                    detail.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                    Res.Details.Add(detail);
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
