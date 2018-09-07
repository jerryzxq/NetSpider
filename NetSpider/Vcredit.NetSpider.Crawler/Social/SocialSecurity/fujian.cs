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
using System.Security.Cryptography;

namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity
{
    public class fujian : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://yzzx2.fjshldbx.com.cn/";
        string socialCity = "fujian";//福州
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
                string md5_random_salt = string.Empty;
                string lt = string.Empty;
                Url = baseUrl + "login?service=http%3A%2F%2F117.27.136.133%3A8081%2Flogincenter%2Fpersonal%2F&serverid=22";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='md5_random_salt']", "value");
                if (results.Count > 0)
                {
                    md5_random_salt = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='lt']", "value");
                if (results.Count > 0)
                {
                    lt = results[0];
                }

                Url = baseUrl + "siusercenter/captcha.htm";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                //添加缓存
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("lt", lt);
                dics.Add("md5_random_salt", md5_random_salt);
                CacheHelper.SetCache(token, dics);
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
            Res.SocialSecurityCity = socialCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            ProvidentFundDetail detail = null;
            int PaymentMonths = 0;
            try
            {
                string md5_random_salt = string.Empty;
                string lt = string.Empty;
                //获取缓存
                if (CacheHelper.GetCache(socialReq.Token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(socialReq.Token);
                    md5_random_salt = dics["md5_random_salt"].ToString();
                    lt = dics["lt"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    CacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Username.IsEmpty() || socialReq.Password.IsEmpty() || socialReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                Url = baseUrl + "siusercenter/login?service=http%3A%2F%2F117.27.136.133%3A8081%2Flogincenter%2Fpersonal%2F&serverid=22";
                postdata = String.Format("username={0}&md5_random_salt={1}&ylzpassword={2}&password={3}&j_captcha_response={4}&lt={5}&_eventId=submit", socialReq.Username, md5_random_salt, CreateMD5Hash(socialReq.Password), CreateMD5Hash(CreateMD5Hash(socialReq.Password + socialReq.Username)+md5_random_salt), socialReq.Vercode, lt);
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
                if (httpResult.Html.IndexOf("window.location.replace('../grlogo.jsp')") != -1)
                {
                    string errorMsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "')");
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 获取基本信息
                Url = baseUrl + "admin/000001/Grwxcb.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    Referer = baseUrl + "admin/grcx.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='user_id']", "value");
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];//编号
                }
                else
                {
                    Res.StatusDescription = "无社保信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='user_logname']", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='user_password']", "value");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='xingbie']", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];//性别
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/table[2]/tr[2]/td[2]/table/tr[3]/td[4]/input", "value");
                if (results.Count > 0)
                {
                    Res.WorkDate = results[0];//参加工作时间
                }
                
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='user_yljs']", "value");
                if (results.Count > 0)
                {
                    Res.SocialInsuranceBase = results[0].ToDecimal(0);//缴费基数
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/table[2]/tr[2]/td[2]/table/tr[9]/td[4]/input", "value");
                if (results.Count > 0)
                {
                    Res.EmployeeStatus = results[0];//工作状态
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


        #region 私有方法
        private  string CreateMD5Hash(string input)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
        #endregion
    }
}
