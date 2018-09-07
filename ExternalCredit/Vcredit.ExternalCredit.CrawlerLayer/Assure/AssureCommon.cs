using Newtonsoft.Json;
using NSoup;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CommonLayer.helper;
using Vcredit.ExternalCredit.Dto;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.CommonLayer.helper;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure
{
    public class AssureCommon
    {
        public AssureCommon()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
   ((sender, certificate, chain, sslPolicyErrors) => true);
        }

        private HttpHelper _httpHelper = new HttpHelper();
        private HttpItem _httpItem;
        private HttpResult _httpResult;

        /// <summary>
        /// 当前登陆cookies
        /// </summary>
        private string _cookies = string.Empty;

        private static readonly string _CookieToken = "A78A3D9F734F44E3BA26013C9C82D5BB";
        /// <summary>
        /// 登陆名
        /// </summary>
        private string _loginName
        {
            get
            {
                return ConfigurationManager.AppSettings["loginName"];
            }
        }

        /// <summary>
        /// 密码
        /// </summary>
        private string _password
        {
            get
            {
                return ConfigurationManager.AppSettings["password"];
            }
        }

        /// <summary>
        /// 获取登陆Cookie
        /// </summary>
        /// <returns></returns>
        public string GetCookie()
        {
            _cookies = RedisHelper.GetCache<string>(_CookieToken, "AssureReport");
            if (!string.IsNullOrEmpty(_cookies))
            {
                // 判断redis缓存_cookies 是否有效
                var url = "https://msi.pbccrc.org.cn/start?r=705";

                _httpItem = new HttpItem
                {
                    URL = url,
                    Cookie = _cookies,
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
                var doc = NSoupClient.Parse(_httpResult.Html);
                if (doc.GetElementById("susername") != null &&
                    doc.GetElementById("spassword") != null)
                {
                    Log4netAdapter.WriteInfo(string.Format("cookies 已失效正在尝试重新登陆！"));
                    TryLogin();
                }

                // todo _cookies 有效重新添加到redis 
            }
            else
            {
                TryLogin();
            }
            if (string.IsNullOrEmpty(_cookies))
                Log4netAdapter.WriteInfo(string.Format("无法获取 cookies"));

            return _cookies;
        }

        /// <summary>
        /// 尝试登陆
        /// </summary>
        private ApiResultDto<AssureLoginResultDto> TryLogin()
        {
            int tryCount = 1;
            ApiResultDto<AssureLoginResultDto> result = null;
            while (true)
            {
                result = this.LoginMethod();
                _cookies = (result.IsSuccess && result.Result != null && !string.IsNullOrEmpty(result.Result.Cookies))
                        ? result.Result.Cookies : string.Empty;

                if (!string.IsNullOrEmpty(_cookies))
                {
                    Log4netAdapter.WriteInfo(string.Format("登陆成功"));
                    break;
                }

                // 最多重试5次
                tryCount++;
                if (tryCount > ConfigData.tryLoginCount)
                {
                    Log4netAdapter.WriteInfo(string.Format("{0}次登陆尝试没有成功，请检查登陆是否存在问题！", ConfigData.tryLoginCount));
                    break;
                }
                Log4netAdapter.WriteInfo(string.Format("登陆没有成功，Message：{0}，尝试第{1}次登陆", result.StatusDescription, tryCount));
            }
            return result;
        }

        /// <summary>
        /// 登陆
        /// </summary>
        /// <returns></returns>
        private ApiResultDto<AssureLoginResultDto> LoginMethod()
        {
            var apiResult = new ApiResultDto<AssureLoginResultDto>();
            // 1. 获取初始化_cookies
            var url = "https://msi.pbccrc.org.cn/html/login.html";
            _httpItem = new HttpItem
            {
                URL = url,
                Method = "GET",
            };
            _httpResult = _httpHelper.GetHtml(_httpItem);
            _cookies = CookieHelper.ConvertResponseCookieToRequestCookie(_httpResult.Cookie);
            //Log4netAdapter.WriteInfo(string.Format("初始化_cookies：{0}", _cookies));

            // 2. 获取验证码，并且合并_cookies
            var svcode = string.Empty;

            // 四位验证码
            var tryCount = 1;
            while (svcode.Length != 4)
            {
                if (tryCount > 3)
                {
                    apiResult.StatusDescription = "3次尝试解析验证码失败了！";
                    Log4netAdapter.WriteInfo(apiResult.StatusDescription);
                    return (apiResult);
                }

                svcode = this.GetVerifyCode();
                tryCount++;
            }

            this.LoginRequest(svcode, _cookies, apiResult);
            return (apiResult);
        }
        /// <summary>
        /// 登陆请求
        /// </summary>
        /// <param name="svcode"></param>
        /// <param name="_cookies"></param>
        /// <param name="apiResult"></param>
        /// <returns></returns>
        private void LoginRequest(string svcode, string _cookies, ApiResultDto<AssureLoginResultDto> apiResult)
        {
            var message = string.Empty;
            // 登陆
            var userName = this._loginName;
            var password = this._password;

            var postdata = string.Format(@"susername={0}&spassword={1}&svcode={2}&ukeyid=", userName, password, svcode);

            var url = "https://msi.pbccrc.org.cn/login";
            _httpItem = new HttpItem
            {
                URL = url,
                Method = "POST",
                Postdata = postdata,
                Cookie = _cookies,
            };
            _httpResult = _httpHelper.GetHtml(_httpItem);
            var rst = JsonConvert.DeserializeObject<AssureLoginResult>(_httpResult.Html);

            //// TODO 测试不登陆
            //var rst = new loginResult();
            if (rst.success)
            {
                if (rst.errors == "1")
                {
                    message = ("机构未递交网签授权协议，请尽快申请。");
                }
                else if (rst.errors == "2")
                {
                    //TODO 需要获取证书格式
                }
                else if (rst.errors == "3")
                {
                    message = ("超过授权时间，请对用户重新进行授权。");
                }
                else if (rst.errors == "4")
                {
                    message = ("机构尚未进行网签，请授权用户及时进行操作。");
                }
                else if (rst.errors == "5")
                {
                    // 登陆成功
                    apiResult.StatusCode = StatusCode.Success;
                    apiResult.Result = new AssureLoginResultDto { Token = _CookieToken, Cookies = _cookies };
                    message = "恭喜你，登陆成功啦！！！";

                    RedisHelper.SetCache(_CookieToken, _cookies, "AssureReport", 240);
                }
            }
            else
            {
                var errors = rst.errors;
                message = (errors) + "，请重试！！！";
            }
            apiResult.StatusDescription = message;
        }
        /// <summary>
        /// 解析获取验证码
        /// </summary>
        /// <returns></returns>
        private string GetVerifyCode()
        {
            _httpItem = new HttpItem
            {
                URL = "https://msi.pbccrc.org.cn/verificationcode?ver=" + DateTime.Now.ToString("yyyy-MM-ddHH:mm:ss"),
                ResultType = ResultType.Byte,
                Cookie = _cookies,
            };

            var vcode = string.Empty;
            _httpResult = _httpHelper.GetHtml(_httpItem);
            _cookies = CookieHelper.CookieCombine(_cookies, CookieHelper.ConvertResponseCookieToRequestCookie(_httpResult.Cookie));
            if (_httpResult.ResultByte != null && _httpResult.ResultByte.Length > 0)
            {
                // 解析验证码
                vcode = AnalysisVCode.GetVerifycode(_httpResult.ResultByte);
            }

            return vcode;
        }

    }
}
