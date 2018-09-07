using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CommonLayer.helper;
using Vcredit.ExternalCredit.Dto;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.CommonLayer.helper;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure.Handlers
{
    public class GetCookieHandler : BaseCreditQueryHandler
    {
        #region Properties

        private HttpHelper _httpHelper = new HttpHelper();
        private HttpItem _httpItem;
        private HttpResult _httpResult;

        /// <summary>
        /// redisPackage
        /// </summary>
        private string _redisPackage
        {
            get
            {
                var str = ConfigurationManager.AppSettings["redisPackage"];
                if (str.IsEmpty())
                    throw new ArgumentException("redisPackage");
                return str;
            }
        }

        /// <summary>
        /// Token标识
        /// </summary>
        private string _cookieToken
        {
            get
            {
                var str = ConfigurationManager.AppSettings["redisCookieToken"];
                if (str.IsEmpty())
                    throw new ArgumentException("redisCookieToken");
                return str;
            }
        }

        /// <summary>
        /// 登陆名
        /// </summary>
        private string _loginName
        {
            get
            {
                var str = ConfigurationManager.AppSettings["loginName"];
                if (str.IsEmpty())
                    throw new ArgumentException("loginName");
                return str;
            }
        }

        /// <summary>
        /// 密码
        /// </summary>
        private string _password
        {
            get
            {
                var str = ConfigurationManager.AppSettings["password"];
                if (str.IsEmpty())
                    throw new ArgumentException("password");
                return str;
            }
        }

        private string _currentCookie = string.Empty;

        #endregion

        protected override HandleResponse Handle(HandleRequest request)
        {
            var res = new HandleResponse();

            res.DoNextHandler = this.GetCookies();
            if (res.DoNextHandler)
                res.Data = _currentCookie;
            else
            {
                res.Description = "登陆失败了！";
                res.ErrorReason = HandlerErrorReason.CookieError;
            }

            return res;
        }

        protected override void SetNextRequest(HandleRequest request, HandleResponse res)
        {
            request.CurrentCookies = res.Data as string;
        }

        /// <summary>
        /// 获取登陆Cookie
        /// </summary>
        /// <returns></returns>
        private bool GetCookies()
        {
            try
            {
                _currentCookie = RedisHelper.GetCache<string>(_cookieToken, _redisPackage);
                if (!string.IsNullOrEmpty(_currentCookie))
                {
                    // 判断redis缓存cookie 是否有效
                    var url = "https://msi.pbccrc.org.cn/sfcp/crrecord/personqueryresult/list?_dc=1469001441580&start=0&limit=20&itype=2&sname=&scardtype=&scardno=&istate=&iblock=1&page=1";

                    _httpItem = new HttpItem
                    {
                        URL = url,
                        Cookie = _currentCookie,
                    };
                    _httpResult = _httpHelper.GetHtml(_httpItem);

                    JObject jobj = null;
                    try
                    {
                        var html = _httpResult.Html;

                        Log4netAdapter.WriteInfo(string.Format("cookie 校验返回结果：{0}", html));
                        jobj = JObject.Parse(html);
                    }
                    catch (Exception ex)
                    {
                        Log4netAdapter.WriteError(string.Format("cookie 校验失败了！"), ex);
                        return false;
                    }

                    if (jobj.SelectToken("$..errors") != null || (jobj.SelectToken("$..success").ToString().ToLower() == "false"))
                    {
                        Log4netAdapter.WriteInfo(string.Format("cookie 已失效正在尝试重新登陆！"));
                        TryLogin();
                    }

                    // todo cookie 有效重新添加到redis 
                }
                else
                {
                    TryLogin();
                }
                if (string.IsNullOrEmpty(_currentCookie))
                {
                    Console.WriteLine("登陆失败了，暂时无法查询数据，请检查登陆接口是否正常");
                    Log4netAdapter.WriteInfo("登陆失败了，暂时无法查询数据，请检查登陆接口是否正常");
                    return false;
                }
                else
                {
                    Console.WriteLine("cookies 获取成功");
                    Log4netAdapter.WriteInfo("cookies 获取成功");
                    return true;
                }
            }
            catch (Exception ex)
            {
                var msg = "登陆失败了";
                Console.WriteLine(msg);
                Log4netAdapter.WriteError(msg, ex);
                return false;
            }
        }

        /// <summary>
        /// 登陆
        /// </summary>
        /// <returns></returns>
        private ApiResultDto<AssureLoginResultDto> LoginMethod()
        {
            var apiResult = new ApiResultDto<AssureLoginResultDto>();
            // 1. 获取初始化cookie
            var url = "https://msi.pbccrc.org.cn/html/login.html";
            _httpItem = new HttpItem
            {
                URL = url,
                Method = "GET",
            };
            _httpResult = _httpHelper.GetHtml(_httpItem);
            _currentCookie = CookieHelper.ConvertResponseCookieToRequestCookie(_httpResult.Cookie);
            //Log4netAdapter.WriteInfo(string.Format("初始化cookie：{0}", cookie));

            // 2. 获取验证码，并且合并cookie
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

            this.LoginRequest(svcode, _currentCookie, apiResult);
            return (apiResult);
        }

        /// <summary>
        /// 登陆请求
        /// </summary>
        /// <param name="svcode"></param>
        /// <param name="cookie"></param>
        /// <param name="apiResult"></param>
        /// <returns></returns>
        private void LoginRequest(string svcode, string cookie, ApiResultDto<AssureLoginResultDto> apiResult)
        {
            var message = string.Empty;
            // 登陆
            var postdata = string.Format(@"susername={0}&spassword={1}&svcode={2}&ukeyid=", _loginName, _password, svcode);

            var url = "https://msi.pbccrc.org.cn/login";
            _httpItem = new HttpItem
            {
                URL = url,
                Method = "POST",
                Postdata = postdata,
                Cookie = cookie,
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
                    apiResult.Result = new AssureLoginResultDto { Token = _cookieToken, Cookies = cookie };
                    message = "恭喜你，登陆成功啦！！！";

                    RedisHelper.SetCache(_cookieToken, cookie, _redisPackage, -1);
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
                Cookie = _currentCookie,
            };

            var vcode = string.Empty;
            _httpResult = _httpHelper.GetHtml(_httpItem);
            _currentCookie = CookieHelper.CookieCombine(_currentCookie, CookieHelper.ConvertResponseCookieToRequestCookie(_httpResult.Cookie));
            if (_httpResult.ResultByte != null && _httpResult.ResultByte.Length > 0)
            {
                //// 保存验证码
                //var rootPath = string.Empty;
                //try
                //{
                //    rootPath = HttpRuntime.AppDomainAppPath;
                //}
                //catch (Exception)
                //{
                //    rootPath = Environment.CurrentDirectory;
                //}
                //var imagePath = Path.Combine(rootPath, "VcodeImage");
                //if (!Directory.Exists(imagePath))
                //    Directory.CreateDirectory(imagePath);
                //var name = Guid.NewGuid().ToString("N") + ".jpg";
                //var fullPath = Path.Combine(imagePath, name);
                //File.WriteAllBytes(fullPath, httpResult.ResultByte);

                // 解析验证码
                vcode = AnalysisVCode.GetVerifycode(_httpResult.ResultByte);
            }

            return vcode;
        }

        /// <summary>
        /// 如果登陆失败，重试登陆
        /// </summary>
        private ApiResultDto<AssureLoginResultDto> TryLogin()
        {
            int tryCount = 0;
            ApiResultDto<AssureLoginResultDto> result = null;
            while (true)
            {
                // 最多重试5次
                tryCount++;
                if (tryCount > ConfigData.tryLoginCount)
                {
                    Log4netAdapter.WriteInfo(string.Format("{0}次登陆尝试没有成功，请检查登陆是否存在问题！", ConfigData.tryLoginCount));
                    break;
                }

                result = this.LoginMethod();
                _currentCookie = (result.IsSuccess && result.Result != null && !string.IsNullOrEmpty(result.Result.Cookies))
                        ? result.Result.Cookies : string.Empty;

                if (!string.IsNullOrEmpty(_currentCookie))
                {
                    Log4netAdapter.WriteInfo(string.Format("登陆成功"));
                    break;
                }
                Log4netAdapter.WriteInfo(string.Format("登陆没有成功，Message：{0}，尝试第{1}次登陆", result.StatusDescription, tryCount));
            }
            return result;
        }
    }
}
