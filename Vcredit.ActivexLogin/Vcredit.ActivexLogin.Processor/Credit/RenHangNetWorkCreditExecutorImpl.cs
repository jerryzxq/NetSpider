using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text.RegularExpressions;
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.Dto;
using Vcredit.ActivexLogin.Entity;
using Vcredit.ActivexLogin.Entity.Bank;
using Vcredit.ActivexLogin.Entity.Credit;
using Vcredit.ActivexLogin.FrameWork;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;

namespace Vcredit.ActivexLogin.Processor.Credit
{
    [RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.RenHangNetWorkCredit)]
    public class RenHangNetWorkCreditExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
    {
        public static readonly RenHangNetWorkCreditExecutorImpl Instance = new RenHangNetWorkCreditExecutorImpl();

        private string _redisEntityPackage;

        public RenHangNetWorkCreditExecutorImpl() : base()
        {
            _redisEntityPackage = redisPackage + ":Entity";
        }

        public BaseRes SendOriginalData(ActivexLoginReq req)
        {
            var entity = new RenHangNetWorkCreditRequestEntity();
            entity.Token = req.Token;
            entity.Password = req.Password;

            var jObj = JObject.Parse(req.AdditionalParam);
            var jsRand = jObj.SelectToken("$..RandomKey");
            if (jsRand == null || jsRand.ToString().IsEmpty())
                throw new ArgumentException("AdditionalParam 参数中 RandomKey 不能为空");
            entity.RandomKey = jsRand.ToString();

            RedisHelper.Enqueue(JsonConvert.SerializeObject(entity), redisQueuePackage);
            return new BaseRes { StatusDescription = "处理成功，后台正在处理加密..." };
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Init()
        {
            var tup = InitLoginPage();
            var token = tup.Item1;
            var randomKey = this.GetRandomKey(token);
            var captchaStr = GetCaptcha(token);
            var data = new VerCodeRes
            {
                Token = token,
                VerCodeBase64 = captchaStr,
                Result = randomKey,
            };

            RedisHelper.SetCache(token, tup.Item2, _redisEntityPackage);
            return data;
        }

        private string GetRandomKey(string token)
        {
            Random rd = new Random();
            var httpItem = new HttpItem()
            {
                URL = "https://ipcrs.pbccrc.org.cn/userReg.do?method=getSrandNum&num=" + rd.NextDouble(),
                Method = "post",
                Cookie = currentCookie,
                ResultCookieType = ResultCookieType.String,

                UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 10.0; WOW64; Trident/7.0; .NET4.0C; .NET4.0E; .NET CLR 2.0.50727; .NET CLR 3.0.30729; .NET CLR 3.5.30729)",
                Accept = "text/plain, */*; q=0.01",
                Referer = "https://ipcrs.pbccrc.org.cn/page/login/loginreg.jsp",
                Host = "ipcrs.pbccrc.org.cn",
                KeepAlive = true,
            };
            httpItem.Header.Add("Accept-Language", "zh-Hans-CN,zh-Hans;q=0.5");
            httpItem.Header.Add("Accept-Encoding", "gzip, deflate");

            httpItem.Header.Add("X-Requested-With", "XMLHttpRequest");
            httpItem.Header.Add("Cache-Control", "no-cache");
            //httpItem.ProxyIp = "127.0.0.1:8888";

            var httpResult = httpHelper.GetHtml(httpItem);

            currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
            RedisHelper.SetCache(token, currentCookie, redisCookiesPackage);

            return httpResult.Html;
        }

        private Tuple<string, string> InitLoginPage()
        {
            var httpItem = new HttpItem()
            {
                URL = "https://ipcrs.pbccrc.org.cn/page/login/loginreg.jsp ",
                Method = "get",
                Accept = "text/html, application/xhtml+xml, image/jxr, */*",
                UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
                Host = "ipcrs.pbccrc.org.cn",
                Allowautoredirect = false,
                ResultCookieType = ResultCookieType.String,
            };
            httpItem.Header.Add("Accept-Language", "zh-Hans-CN,zh-Hans;q=0.5");
            httpItem.Header.Add("Accept-Encoding", "gzip, deflate");

            //httpItem.ProxyIp = "127.0.0.1:8888";

            var httpResult = httpHelper.GetHtml(httpItem);
            string token = Guid.NewGuid().ToString("N");
            currentCookie = CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie);
            var doc = NSoup.NSoupClient.Parse(httpResult.Html);

            var date = "";
            var dateEle = doc.GetElementsByAttributeValue("name", "date").First;
            if (dateEle != null)
            {
                date = dateEle.Attr("value");
            }
            return new Tuple<string, string>(token, date);
        }

        private string GetCaptcha(string token)
        {
            Random rd = new Random();
            var httpItem = new HttpItem()
            {
                URL = "https://ipcrs.pbccrc.org.cn/imgrc.do?a=" + rd.NextDouble(),
                Method = "get",
                Cookie = currentCookie,
                ResultType = ResultType.Byte,
                ResultCookieType = ResultCookieType.String,

                UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
                Accept = "*/*",
                Referer = "https://ipcrs.pbccrc.org.cn/page/login/loginreg.jsp",
                Host = "ipcrs.pbccrc.org.cn",
            };
            httpItem.Header.Add("Accept-Language", "zh-Hans-CN,zh-Hans;q=0.5");
            httpItem.Header.Add("Accept-Encoding", "gzip, deflate");
            //httpItem.ProxyIp = "127.0.0.1:8888";

            var httpResult = httpHelper.GetHtml(httpItem);

            currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
            RedisHelper.SetCache(token, currentCookie, redisCookiesPackage);

#if DEBUG
            File.WriteAllBytes(@"D:\captcha.png", httpResult.ResultByte);
#endif

            var base64Str = CommonFun.GetVercodeBase64(httpResult.ResultByte);
            return base64Str;
        }

        public BaseRes DoRealLogin(string token, string captcha)
        {
            var res = new BaseRes() { Token = token };
            currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
            var encryptPwd = RedisHelper.GetCache<string>(token, redisEncryptPackage);

            if (string.IsNullOrEmpty(encryptPwd))
            {
                res.Result = JsonConvert.SerializeObject(
                    new EncryptDataResultDto
                    {
                        Reason = EncryptStatus.NotFoundEncrypt,
                        ReasonDescription = "加密没有完成"
                    });
                return res;
            }

            var date = RedisHelper.GetCache<string>(token, _redisEntityPackage);
            var loginName = "devilleo";

            var _postData = string.Format("method=login&date={0}&loginname={1}&password={2}&_%40IMGRC%40_={3}",
                 date, loginName, encryptPwd.ToUrlDecode(), captcha);

            var httpItem = new HttpItem
            {
                URL = "https://ipcrs.pbccrc.org.cn/login.do",
                Method = "POST",
                Accept = "text/html, application/xhtml+xml, image/jxr, */*",
                ContentType = "application/x-www-form-urlencoded",
                Referer = "https://ipcrs.pbccrc.org.cn/page/login/loginreg.jsp",
                UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
                Host = "ipcrs.pbccrc.org.cn",
                KeepAlive = true,

                Postdata = _postData,
                Cookie = currentCookie,
            };
            httpItem.Header.Add("Accept-Language", "zh-Hans-CN,zh-Hans;q=0.5");
            httpItem.Header.Add("Accept-Encoding", "gzip, deflate");
            httpItem.Header.Add("Cache-Control", "no-cache");

            //httpItem.ProxyIp = "127.0.0.1:8888";

            var httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
            Log4netAdapter.WriteInfo(httpResult.Html);
            res.Result = httpResult.Html;
            return res;
        }

        public VerCodeRes RefreshCaptcha(string token)
        {
            var captchaStr = GetCaptcha(token);

            var data = new VerCodeRes
            {
                Token = token,
                VerCodeBase64 = captchaStr
            };
            return data;
        }
    }
}
