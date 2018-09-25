using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vcredit.ActivexLogin.Dto;
using Vcredit.ActivexLogin.Entity;
using Vcredit.ActivexLogin.FrameWork;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.Framework.Queue.Redis;
using System.IO;
using Newtonsoft.Json.Linq;
using ServiceStack;
using Vcredit.ActivexLogin.Common;
using Vcredit.NetSpider.Cache;
using Vcredit.ActivexLogin.Attributes;
using System.Reflection;

namespace Vcredit.ActivexLogin.Processor
{
    [RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.GuangZhouGjj)]
    public class GuangZhouGjjExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
    {
        public static readonly GuangZhouGjjExecutorImpl Instance = new GuangZhouGjjExecutorImpl();

        private string _redisEntityPackage;

        public GuangZhouGjjExecutorImpl() : base()
        {
            _redisEntityPackage = redisPackage + ":Entity";
        }

        /// <summary>
        /// 发送原始数据
        /// </summary>
        /// <returns></returns>
        public BaseRes SendOriginalData(ActivexLoginReq req)
        {
            var entity = new GuangZhouGjjRequestEntity();
            entity.Token = req.Token;
            entity.Account = req.Account;
            entity.Password = req.Password;

            var jObj = JObject.Parse(req.AdditionalParam);
            var jsRand = jObj.SelectToken("$..SRand");
            if (jsRand == null || jsRand.ToString().IsEmpty())
                throw new ArgumentException("AdditionalParam 参数中 SRand 不能为空");
            entity.SRand = jsRand.ToString();

            var jLoginType = jObj.SelectToken("$..LoginType");
            if (jLoginType == null || jLoginType.ToString().IsEmpty())
                throw new ArgumentException("AdditionalParam 参数中 LoginType 不能为空");
            entity.LoginType = (LoginType)jLoginType.ToString().ToInt();

            if (entity.LoginType == LoginType.CertNo)
            {
                var jName = jObj.SelectToken("$..Name");
                if (jName == null || jName.ToString().IsEmpty())
                    throw new ArgumentException("AdditionalParam 参数中 Name 不能为空");
                entity.Name = jName.ToString();
            }

            RedisHelper.SetCache(entity.Token, JsonConvert.SerializeObject(entity), _redisEntityPackage);

            RedisHelper.Enqueue(JsonConvert.SerializeObject(entity), redisQueuePackage);

            return new BaseRes { StatusDescription = "处理成功，后台正在处理加密..." };
        }

        public override BaseRes GetEncryptData(string token)
        {
            return base.GetEncryptData(token);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Init()
        {
            var tup = InitLoginPage();
            var captcha = GetCaptcha(tup.Item1);

            var data = new VerCodeRes { Token = tup.Item1, VerCodeBase64 = captcha, Result = tup.Item2 };
            return data;
        }
        private Tuple<string, string> InitLoginPage()
        {
            var httpItem = new HttpItem()
            {
                URL = "https://gzgjj.gov.cn/wsywgr/",
                Method = "get",
            };
            var httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = httpResult.Cookie;
            var doc = NSoup.NSoupClient.Parse(httpResult.Html);

            var sRand = Regex.Match(httpResult.Html, "(?<=var sRand = \").*(?=\";)").Value;
            string token = Guid.NewGuid().ToString("N");

            return new Tuple<string, string>(token, sRand);
        }
        private string GetCaptcha(string token)
        {
            var httpItem = new HttpItem()
            {
                URL = "https://gzgjj.gov.cn/wsywgr/CheckAction!createYZM.action?d=1498717038408",
                Method = "get",
                Cookie = currentCookie,
                ResultType = ResultType.Byte,
                ResultCookieType = ResultCookieType.String,

                Accept = "text/html, application/xhtml+xml, image/jxr, */*",
                Referer = "https://gzgjj.gov.cn/wsywgr/",
            };
            var httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = CookieUtil.CookieCombine(currentCookie, httpResult.Cookie);
            RedisHelper.SetCache(token, currentCookie, redisCookiesPackage);

            return CommonFun.GetVercodeBase64(httpResult.ResultByte);
        }

        /// <summary>
        /// 实际登陆
        /// </summary>
        /// <param name="token"></param>
        /// <param name="captcha"></param>
        /// <returns></returns>
        public BaseRes DoRealLogin(string token, string captcha)
        {
            var res = new BaseRes();

            var encryptStr = RedisHelper.GetCache<String>(token, redisEncryptPackage);
            if (string.IsNullOrEmpty(encryptStr))
            {
                res.Result = JsonConvert.SerializeObject(
                    new EncryptDataResultDto
                    {
                        Reason = EncryptStatus.NotFoundEncrypt,
                        ReasonDescription = "加密没有完成"
                    });
                return res;
            }

            var entity = JsonConvert.DeserializeObject<GuangZhouGjjRequestEntity>(RedisHelper.GetCache<string>(token, _redisEntityPackage));
            var encruptObj = JsonConvert.DeserializeObject<GuanZhouGjjEncryptEntity>(encryptStr);
            var postData = "";
            if (entity.LoginType == LoginType.Account)
            {
                var certno = encruptObj.certno.ToUrlEncode();
                var password = encruptObj.password.ToUrlEncode();
                postData = string.Format("radiobutton=radiobutton&certno={0}&zjh=undefined&name=&password={1}&captcha={2}",
                                            certno, password, captcha);


            }
            else
            {
                var zjh = encruptObj.zjh.ToUrlEncode();
                var password = encruptObj.password.ToUrlEncode();
                var name = entity.Name.ToUrlEncode();

                postData = string.Format("radiobutton=radiobutton&certno=undefined&zjh={0}&name={1}&password={2}&captcha={3}",
                                            zjh,
                                            name,
                                            password,
                                            captcha);
            }

            currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
            var httpItem = new HttpItem()
            {
                URL = "https://gzgjj.gov.cn/wsywgr/LoginAction!login.action",
                Method = "POST",
                Cookie = currentCookie,
                ContentType = "application/x-www-form-urlencoded",
                Accept = "text/html, application/xhtml+xml, image/jxr, */*",
                Referer = "https://gzgjj.gov.cn/wsywgr/",
                Postdata = postData,
                ResultCookieType = ResultCookieType.String,
                Allowautoredirect = false,
            };
            var httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = CookieUtil.CookieCombine(currentCookie, httpResult.Cookie);

            if (!httpResult.RedirectUrl.IsEmpty())
            {
                httpItem = new HttpItem()
                {
                    URL = httpResult.RedirectUrl,
                    Method = "get",
                    Accept = "text/html, application/xhtml+xml, image/jxr, */*",
                    Referer = "https://gzgjj.gov.cn/wsywgr/",

                    Cookie = currentCookie,

                };
                httpResult = httpHelper.GetHtml(httpItem);
            }

            if (httpResult.ResponseUri.Equals("https://gzgjj.gov.cn/wsywgr/index.jsp"))
            {
                res.Result = JsonConvert.SerializeObject(
                    new EncryptDataResultDto
                    {
                        Reason = EncryptStatus.Success,
                        ReasonDescription = "登陆成功"
                    });
            }
            else
            {
                res.Result = JsonConvert.SerializeObject(
                   new EncryptDataResultDto
                   {
                       Reason = EncryptStatus.Faild,
                       ReasonDescription = "登陆失败了"
                   });
            }

            return res;
        }

        public VerCodeRes RefreshCaptcha(string token)
        {
            throw new NotImplementedException();
        }

        private class GuanZhouGjjEncryptEntity
        {
            public string certno { get; set; }

            public string password { get; set; }

            public string zjh { get; set; }
        }
    }
}
