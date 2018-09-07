using System.Net;
using System.Text;
using System.Web;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.Common.Constants;
using Vcredit.NetSpider.Entity;
using System;
using Vcredit.NetSpider.Processor.Operation.JsonModel.ProvidentFund;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Vcredit.NetSpider.Processor.Operation
{
    internal class VcreditCertifyOpr
    {
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;

        public VerCodeRes TaobaoCertify(string username, string password, string vercode)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string ua = "174UW5TcyMNYQwiAiwTR3tCf0J%2FQnhEcUpkMmQ%3D%7CUm5Ockt3QnhNdkN6Qn9Dey0%3D%7CU2xMHDJ%2BH2QJZwBxX39Rb1Z4WHYqSy1BJlgiDFoM%7CVGhXd1llXGBVb1phVG1VaFRsW2ZEfkV6T3pEe0N7QndLck5wSmQy%7CVWldfS0QMA80DCwQKAgmSzYSeEU2FncGfxw5D3sWOG44%7CVmNDbUMV%7CV2NDbUMV%7CWGRYeCgGZhtmH2VScVI2UT5fORtmD2gCawwuRSJHZAFsCWMOdVYyVTpbPR99HWAFYVMpUDUFOBonBTgaJwVoEHQPSg0nSixQKRl3HnoAbUNjTWM1Yw%3D%3D%7CWWdHFyMXNwoqFikSLAw5DTUVKRYjHj4CPwA5GSUaLxIyBjMKXAo%3D%7CWmZYeCgGWjtdMVYoUn5EakpkOHs%2FEy8UNAkpFDQLNQ0jdSM%3D%7CW2daelQEMA42Dy94VmpTb1NpVW1YYFtmU2kcIQM7DzEKMQk2CzcDOAI8CTEFUnxcYDYYTg%3D%3D%7CXGZGFjgWNgsrECgUIHYg%7CXWREFDplPngsUCpHPFozVjtvU31daF1pSXZIfV1kXWZcZDJk%7CXmZGFjhnPHouUihFPlgxVDltUX9fDzsCPR0iGydxUWxMYkxsVW1RbFIEUg%3D%3D%7CX2VFFTsVNQkpECgUKBJEEg%3D%3D%7CQHpaCiR7IGYyTjRZIkQtSCVxTWNDf19mXmJdZjBm%7CQXtbCyV6IWczTzVYI0UsSSRwTGJCf19mU2dTbjhu%7CQnhYCCYIKBU1DDgFOA1bDQ%3D%3D%7CQ3tbCyV6IWczTzVYI0UsSSRwTGJCEiYeJAQ7BTFnR3padFp6QnpFeEUTRQ%3D%3D%7CRH5eDiAOLhIyCjINMQlfCQ%3D%3D%7CRX9fDyF%2BJWM3SzFcJ0EoTSB0SGZGelpiWmVabzlv%7CRn9fDyF%2BJWM3SzFcJ0EoTSB0SGZGfEh1VWpUaEhwSHFEey17%7CR3xcDCJ9JmA0SDJfJEIrTiN3S2VFcUxsUXFKc0x5RBJE%7CSHNTAy1yKW87Rz1QK00kQSx4RGpKfkNjXn5FfEVwTBpM%7CSXJSAixzKG46RjxRKkwlQC15RWtLcEtrVnZNdE50TRtN%7CSnJSAiwCInJIdUlpV2xRBycaOhQ6GiEbJR8lcyU%3D%7CS3FRAS9wK205RT9SKU8mQy56RmhIdVVuVGpeYzVj%7CTHVIdVVoSHdXa1JuTnBIclJqXn5EfFxgXGVFeVllXmNDd0pqVGFBeFhkW3tFfV1jWnpEelpkW3tBYV9iQn1JaUhwUGxVdUl3V2tUdElpVmg%2B";
            try
            {
                if (CacheHelper.GetCache("vcert_taobao_" + username) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache("vcert_taobao_" + username);
                }
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (vercode.IsEmpty() || cookies.Count == 0)
                {
                    Url = "https://login.taobao.com/member/request_nick_check.do?_input_charset=utf-8";
                    postdata = String.Format("username={0}&ua=", username, ua);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                    string needcode = jsonParser.GetResultFromParser(httpResult.Html, "needcode");
                    if (needcode.ToLower() == "true")
                    {
                        CacheHelper.SetCache("vcert_taobao_" + username, cookies);
                        Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                        Res.VerCodeUrl = jsonParser.GetResultFromParser(httpResult.Html, "url");
                        Res.StatusCode = ServiceConsts.StatusCode_needvercode;
                        return Res;
                    }
                }

                Url = "https://login.taobao.com/member/login.jhtml";
                postdata = string.Format("ua={3}&TPL_username={0}&TPL_password={1}&TPL_checkcode={2}&loginsite=0&newlogin=1&TPL_redirect_url=http%3A%2F%2Flz.taobao.com%2Flogin%2F%3F&from=lzdp&fc=default&style=minisimple&css_style=&tid=XOR_1_000000000000000000000000000000_63583322450974020A07040D&support=000001&CtrlVersion=1%2C0%2C0%2C7&loginType=4&minititle=&minipara=0%2C0%2C0&umto=NaN&pstrong=&llnick=&sign=&need_sign=&isIgnore=&full_redirect=&popid=&callback=1&guf=¬_duplite_str=&need_user_id=&poy=&gvfdcname=10&gvfdcre=687474703A2F2F6C7A2E74616F62616F2E636F6D2F6C6F67696E2F3F7374617475733D30&from_encoding=&sub=true&allp=&oslanguage=&sr=1376*774&osVer=windows%7C6.1&naviVer=ie%7C9", HttpUtility.UrlEncode(username), HttpUtility.UrlEncode(password), vercode, ua);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    Allowautoredirect = true,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string state = jsonParser.GetResultFromParser(httpResult.Html, "state");

                if (state.ToLower() == "true")
                {
                    Log4netAdapter.WriteInfo(String.Format("淘宝账号登录认证成功,用户名：{0},密码：{1}", username, password));
                    Res.StatusDescription = "淘宝认证成功！";
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                }
                else
                {
                    Log4netAdapter.WriteInfo(String.Format("淘宝账号登录认证成功,用户名：{0},密码：{1}", username, password));
                    Res.StatusDescription = "淘宝认证失败！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                }
                CacheHelper.RemoveCache("vcert_taobao_" + username);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("淘宝账号登录认证异常", e);
            }
            return Res;
        }
        public VerCodeRes JingDongCertify(string username, string password, string vercode)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string uuid = string.Empty;
            string param = string.Empty;
            try
            {
                if (CacheHelper.GetCache("vcert_jd_" + username) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache("vcert_jd_" + username);
                    cookies = (CookieCollection)dics["cookies"];
                    uuid = (string)dics["uuid"];
                    param = (string)dics["param"];
                }
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (vercode.IsEmpty()|| cookies.Count == 0)
                {
                    Url = "https://passport.jd.com/new/login.aspx";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "GET",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    uuid = CommonFun.GetMidStr(httpResult.Html, "uuid\" value=\"", "\"");
                    param = CommonFun.GetMidStr(httpResult.Html, "<span class=\"clr\"></span><input type=\"hidden\" name=\"", "\" />").Replace("\" value=\"", "=");

                    httpItem = new HttpItem()
                    {
                        URL = "https://passport.jd.com/uc/showAuthCode",
                        Method = "post",
                        Postdata = "loginName=" + username,
                        Cookie = httpResult.Cookie
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    if (httpResult.Html.Contains("true"))
                    {
                        Dictionary<string, object> dics = new Dictionary<string, object>();
                        dics.Add("cookies", cookies);
                        dics.Add("uuid", uuid);
                        dics.Add("param", param);
                        CacheHelper.SetCache("vcert_jd_" + username, dics);
                        Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                        Url = "https://authcode.jd.com/verify/image?a=1&acid=" + uuid + "&uid=" + uuid;
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "get",
                            ResultType = ResultType.Byte,
                            Referer = "https://passport.jd.com/uc/login"
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        if (httpResult.StatusCode != HttpStatusCode.OK)
                        {
                            Res.StatusDescription = "获取验证码时网络异常";
                            Res.StatusCode = ServiceConsts.StatusCode_fail;
                            return Res;
                        }
                        cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                        //保存验证码图片在本地
                        FileOperateHelper.WriteVerCodeImage("vcert_jd_" + username, httpResult.ResultByte);
                        Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                        Res.VerCodeUrl = AppSettings.localUrl + "/vercodeimg.aspx?vercode=" + "vcert_jd_" + username;
                        Res.StatusCode = ServiceConsts.StatusCode_needvercode;
                        return Res;
                    }
                }

                Url = "https://passport.jd.com/uc/loginService?uuid=" + uuid;
                postdata = string.Format("uuid={0}&loginname={1}&nloginpwd={2}&loginpwd={2}&machineNet=&machineCpu=&machineDisk=&authcode={3}&{4}", uuid, username, password, vercode, param);

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                if (httpResult.Html == "({\"success\":\"http://www.jd.com\"})")
                {
                    Log4netAdapter.WriteInfo(String.Format("京东账号登录认证成功,用户名：{0},密码：{1}", username, password));
                    Res.StatusDescription = "京东认证成功！";
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                }
                else if(httpResult.Html.Contains("emptyAuthcode"))
                {
                    Res.StatusCode = ServiceConsts.StatusCode_needvercode;
                    Res.StatusDescription = "验证码不正确";
                    Log4netAdapter.WriteInfo(String.Format("京东账号登录认证验证码不正确,用户名：{0},密码：{1}", username, password));
                }
                else 
                {
                    Res.StatusDescription = "登录失败";
                    Log4netAdapter.WriteInfo(String.Format("京东账号登录认证失败,用户名：{0},密码：{1}，错误信息：{2}", username, password, httpResult.Html));
                }
                CacheHelper.RemoveCache("vcert_jd_" + username);
                return Res;
            }
            catch (Exception e)
            {
                Res.StatusDescription = e.Message;
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("京东账号登录认证异常", e);
            }
            return Res;
        }
        public BaseRes ChsiCertify(string username, string password)
        {
            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string lt = string.Empty;
            string param = string.Empty;
            try
            {
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Url = "https://account.chsi.com.cn/passport/login";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = "获取验证码时网络异常";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                lt = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='lt']", "value")[0];

                Url = "https://account.chsi.com.cn/passport/login";
                postdata = string.Format("username={0}&password={1}&lt={2}&_eventId=submit&submit=%E7%99%BB%C2%A0%C2%A0%E5%BD%95", username, password, lt);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                if (httpResult.Html.Contains("确定要退出吗？"))
                {
                    Log4netAdapter.WriteInfo(String.Format("学信网账号登录认证成功,用户名：{0},密码：{1}", username, password));

                    Res.StatusDescription = "学信网认证成功！";
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                }
                else
                {
                    Log4netAdapter.WriteInfo(String.Format("学信网账号登录认证失败,用户名：{0},密码：{1}", username, password));
                }
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("学信网账号登录认证异常", e);
            }
            return Res;
        }
        public BaseRes MailCertify(string username, string password)
        {
            IPluginEmail emailPlug = PluginServiceManager.GetEmailPlugin();
            BaseRes Res = new BaseRes();
            try
            {
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string server = username.Split('@')[1];


                //if (LoginMail("pop." + server, username, password))
                if (emailPlug.AuthenticateByPop3("pop." + server,username,password))
                {
                    Log4netAdapter.WriteInfo(String.Format("邮箱账号登录认证成功,用户名：{0},密码：{1}", username, password));

                    Res.StatusDescription = "邮箱认证成功！";
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                }
                else
                {
                    Log4netAdapter.WriteInfo(String.Format("邮箱账号登录认证失败,用户名：{0},密码：{1}", username, password));

                    Res.StatusDescription = "邮箱认证失败！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                }
                return Res;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("邮箱账号认证异常", e);
            }
            return Res;
        }

        #region 私有方法
        /// <summary>
        /// unicode转中文（符合js规则的）
        /// </summary>
        /// <returns></returns>
        public static string unicode_js_1(string str)
        {
            string outStr = "";
            Regex reg = new Regex(@"(?i)\\u([0-9a-f]{4})");
            outStr = reg.Replace(str, delegate(Match m1)
            {
                return ((char)Convert.ToInt32(m1.Groups[1].Value, 16)).ToString();
            });
            return outStr;
        }
        #endregion
    }
}
