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
using Vcredit.ActivexLogin.Common;
using Vcredit.NetSpider.Cache;
using Vcredit.ActivexLogin.Attributes;
using System.Reflection;

namespace Vcredit.ActivexLogin.Processor
{
    [RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.WuHanGjj)]
    public class WuHanGjjExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
    {
        public static readonly WuHanGjjExecutorImpl Instance = new WuHanGjjExecutorImpl();

        public WuHanGjjExecutorImpl() : base()
        {
        }

        /// <summary>
        /// 发送原始数据
        /// </summary>
        /// <returns></returns>
        public BaseRes SendOriginalData(ActivexLoginReq req)
        {
            var entity = new WuHanGjjRequestEntity();
            entity.Token = req.Token;
            entity.Account = req.Account;
            entity.Password = req.Password;

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

            var data = new VerCodeRes { Token = tup.Item1, VerCodeBase64 = captcha };
            return data;
        }
        private Tuple<string> InitLoginPage()
        {
            var httpItem = new HttpItem()
            {
                URL = "https://whgjj.hkbchina.com/portal/pc/login.html",
                Method = "get",
                Accept = "text/html, application/xhtml+xml, image/jxr, */*",
            };
            var httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie);

            string token = Guid.NewGuid().ToString("N");
            return new Tuple<string>(token);
        }
        private string GetCaptcha(string token)
        {
            var httpItem = new HttpItem()
            {
                URL = "https://whgjj.hkbchina.com/portal/GenTokenImg.do",
                Method = "get",
                Cookie = currentCookie,
                ResultType = ResultType.Byte,
                ResultCookieType = ResultCookieType.String,

                Accept = "image/png, image/svg+xml, image/jxr, image/*;q=0.8, */*;q=0.5",
                Referer = "https://whgjj.hkbchina.com/portal/pc/login.html",
            };
            var httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));

            RedisHelper.SetCache(token, currentCookie, redisCookiesPackage);
            return CommonFun.GetVercodeBase64(httpResult.ResultByte);
        }

        /// <summary>
        /// 登陆方法
        /// </summary>
        /// <param name="token"></param>
        /// <param name="captcha"></param>
        /// <returns></returns>
        public BaseRes DoRealLogin(string token, string captcha)
        {
            var res = new BaseRes() { Token = token };

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
            var entity = JsonConvert.DeserializeObject<WuHanGjjEncryptEntity>(encryptStr);

            var postDataEntity = new
            {
                _ChannelId = "PMBS",
                _locale = "zh_CN",
                BankId = "9999",
                LoginType = "F",
                robust = "0",

                _vTokenName = captcha,
                LoginId = entity.LoginId,
                LoginPassword = entity.LoginPasswordObj,
            };

            currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
            var _postData = JsonConvert.SerializeObject(postDataEntity);

            var httpItem = new HttpItem
            {
                URL = "https://whgjj.hkbchina.com/portal/login.do",
                Method = "POST",
                Accept = "application/json, text/plain, */*",
                ContentType = "application/json;charset=utf-8",
                Referer = "https://whgjj.hkbchina.com/portal/pc/login.html",
                PostEncoding = Encoding.UTF8,
                Postdata = _postData,
                Cookie = currentCookie,
            };
            httpItem.Header.Add("$Referer", "htmls/LoginContent/loginContent.html");

            httpItem.ProxyIp = "127.0.0.1:8888";
            var httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
            Log4netAdapter.WriteInfo(httpResult.Html);

            var jsonObj = JObject.Parse(httpResult.Html);
            if (jsonObj.SelectToken("$.._RejCode").ToString() == "000000")
            {
                res.Result = JsonConvert.SerializeObject(
                    new EncryptDataResultDto
                    {
                        Reason = EncryptStatus.Success,
                        ReasonDescription = "登陆成功",
                        Data = httpResult.Html + "  Cookie is ==> " + currentCookie
                    });
            }
            else
            {
                res.Result = JsonConvert.SerializeObject(
                   new EncryptDataResultDto
                   {
                       Reason = EncryptStatus.Faild,
                       ReasonDescription = "登陆失败了",
                       Data = httpResult.Html
                   });
            }

            return res;
        }

        public VerCodeRes RefreshCaptcha(string token)
        {
            throw new NotImplementedException();
        }

        private class WuHanGjjEncryptEntity
        {
            public string LoginPasswordObj { get; set; }

            public string LoginId { get; set; }
        }

    }
}
