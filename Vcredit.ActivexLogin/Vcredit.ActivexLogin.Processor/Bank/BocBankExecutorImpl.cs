using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.Dto;
using Vcredit.ActivexLogin.Entity.Bank;
using Vcredit.ActivexLogin.FrameWork;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;

namespace Vcredit.ActivexLogin.Processor.Bank
{
    /// <summary>
    /// 中国银行
    /// </summary>
    [RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.BocBank)]
    public class BocBankExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
    {
        public static readonly BocBankExecutorImpl Instance = new BocBankExecutorImpl();

        private string _redisEntityPackage;

        public BocBankExecutorImpl() : base()
        {
            _redisEntityPackage = redisPackage + ":Entity";
        }

        public BaseRes SendOriginalData(ActivexLoginReq req)
        {
            var entity = new BocBankRequestEntity();
            entity.Token = req.Token;
            entity.Account = req.Account;
            entity.Password = req.Password;

            if (req.AdditionalParam.IsEmpty())
                throw new ArgumentNullException("AdditionalParam 不能为空");

            var jObj = JObject.Parse(req.AdditionalParam);
            var randomKey_S = jObj.SelectToken("$..RandomKey_S");
            if (randomKey_S == null || randomKey_S.ToString().IsEmpty())
                throw new ArgumentException("AdditionalParam 参数中 RandomKey_S 不能为空");
            entity.RandomKey_S = randomKey_S.ToString();

            var conversationId = jObj.SelectToken("$..ConversationId");
            if (conversationId == null || conversationId.ToString().IsEmpty())
                throw new ArgumentException("AdditionalParam 参数中 ConversationId 不能为空");
            entity.ConversationId = conversationId.ToString();

            entity.UrlParam = string.Format("?RandomKey_S={0}", entity.RandomKey_S);

            RedisHelper.SetCache(entity.Token, JsonConvert.SerializeObject(entity), _redisEntityPackage);
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
            var r = this.InitParam();
            var captchaStr = GetCaptcha(tup.Item1);

            var data = new VerCodeRes
            {
                Token = tup.Item1,
                Result = JsonConvert.SerializeObject(r),
                VerCodeBase64 = captchaStr
            };
            return data;
        }

        private object InitParam()
        {
            var httpItem = new HttpItem()
            {
                URL = "https://ebsnew.boc.cn/BII/PsnGetUserProfile.do?_locale=zh_CN",
                Method = "POST",
                Accept = "*/*",
                Referer = "https://ebsnew.boc.cn/boc15/login.html",
                Host = "ebsnew.boc.cn",

                Cookie = currentCookie,
                Postdata = "{'header':{'local':'zh_CN','agent':'WEB15','bfw-ctrl':'json','version':'','device':'','platform':'','plugins':'','page':'','ext':''},'request':[{'id':9,'method':'PsnAccBocnetCreateConversationPre','conversationId':null,'params':null}]}"
                .Replace("'", "\""),

                ContentType = "text/json",
                KeepAlive = true,
            };
            httpItem.Header.Add("Pragma", "no-cache");
            httpItem.Header.Add("Cache-Control", "no-cache");
            httpItem.Header.Add("bfw-ctrl", "json");
            httpItem.Header.Add("X-id", "10");
            httpItem.Header.Add("Origin", "https://ebsnew.boc.cn");
            httpItem.Header.Add("X-Requested-With", "XMLHttpRequest");

            var httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
            var jsonObj = JObject.Parse(httpResult.Html);
            var conversationId = jsonObj.SelectToken("$..response[0].result").ToString();

            httpItem = new HttpItem()
            {
                URL = "https://ebsnew.boc.cn/BII/PsnGetUserProfile.do?_locale=zh_CN",
                Method = "POST",
                Accept = "*/*",
                Referer = "https://ebsnew.boc.cn/boc15/login.html",
                Host = "ebsnew.boc.cn",

                Cookie = currentCookie,
                Postdata = "{'header':{'local':'zh_CN','agent':'WEB15','bfw-ctrl':'json','version':'','device':'','platform':'','plugins':'','page':'','ext':''},'request':[{'id':11,'method':'PsnAccBocnetGetRandomPre','conversationId':'@@conversationId','params':null}]}"
                .Replace("@@conversationId", conversationId)
                .Replace("'", "\""),

                ContentType = "text/json",
                KeepAlive = true,
            };
            httpItem.Header.Add("Pragma", "no-cache");
            httpItem.Header.Add("Cache-Control", "no-cache");
            httpItem.Header.Add("bfw-ctrl", "json");
            httpItem.Header.Add("X-id", "12");
            httpItem.Header.Add("Origin", "https://ebsnew.boc.cn");
            httpItem.Header.Add("X-Requested-With", "XMLHttpRequest");

            httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
            jsonObj = JObject.Parse(httpResult.Html);
            var RandomKey_S = jsonObj.SelectToken("$..response[0].result").ToString();

            return new { conversationId = conversationId, RandomKey_S = Convert.ToBase64String(Encoding.Default.GetBytes(RandomKey_S)) };
        }

        private string GetCaptcha(string token)
        {
            var httpItem = new HttpItem()
            {
                URL = "https://ebsnew.boc.cn/BII/ImageValidation/validation1502069912899.gif",
                Method = "get",

                Cookie = currentCookie,
                ResultType = ResultType.Byte,
                ResultCookieType = ResultCookieType.String,

                Accept = "image/webp,image/apng,image/*,*/*;q=0.8",
                Referer = "https://ebsnew.boc.cn/boc15/login.html?locale=zh_CN",
            };
            var httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
            RedisHelper.SetCache(token, currentCookie, redisCookiesPackage);
            File.WriteAllBytes("D:\\Image.png", httpResult.ResultByte);
            return CommonFun.GetVercodeBase64(httpResult.ResultByte);
        }

        private Tuple<string> InitLoginPage()
        {
            var httpItem = new HttpItem()
            {
                URL = "https://ebsnew.boc.cn/boc15/login.html?locale=zh_CN",
                Method = "get",
                Accept = "text/html, application/xhtml+xml, image/jxr, */*",
                Allowautoredirect = false,
                ResultCookieType = ResultCookieType.String,
            };
            var httpResult = httpHelper.GetHtml(httpItem);

            currentCookie = CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie);

            //var timestamp = Regex.Match(httpResult.Html, @"(?<=var macInfo = password.GetMachineCode\('powerpass_ie', ').*(?='\);)").Value;

            string token = Guid.NewGuid().ToString("N");
            RedisHelper.SetCache(token, currentCookie, redisCookiesPackage);
            return new Tuple<string>(token);
        }

        public BaseRes DoRealLogin(string token, string captcha)
        {
            var res = new BaseRes() { Token = token };

            var encryptStr = RedisHelper.GetCache<string>(token, redisEncryptPackage);
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
            var entity = JsonConvert.DeserializeObject<BocBankRequestEntity>(RedisHelper.GetCache<string>(token, _redisEntityPackage));
            var encryptentity = JsonConvert.DeserializeObject<BocBankEncryptEntity>(encryptStr);

            encryptentity.loginName = entity.Account;
            encryptentity.validationChar = captcha;

            currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);

            var httpItem = new HttpItem()
            {
                URL = "https://ebsnew.boc.cn/BII/PsnGetUserProfile.do?_locale=zh_CN",
                Method = "POST",
                Accept = "*/*",
                Referer = "https://ebsnew.boc.cn/boc15/login.html",
                Host = "ebsnew.boc.cn",

                Cookie = currentCookie,
                Postdata = "{'header':{'local':'zh_CN','agent':'WEB15','bfw-ctrl':'json','version':'','device':'','platform':'','plugins':'','page':'','ext':'','cipherType':'0'},'request':[{'id':16,'method':'PsnAccBocnetLogin','conversationId':'@@conversationId','params':@@params}]}"
                .Replace("@@conversationId", entity.ConversationId)
                .Replace("@@params", JsonConvert.SerializeObject(encryptentity))
                .Replace("'", "\""),

                ContentType = "text/json",
                KeepAlive = true,
            };
            httpItem.Header.Add("Pragma", "no-cache");
            httpItem.Header.Add("Cache-Control", "no-cache");
            httpItem.Header.Add("bfw-ctrl", "json");
            httpItem.Header.Add("X-id", "17");
            httpItem.Header.Add("Origin", "https://ebsnew.boc.cn");
            httpItem.Header.Add("X-Requested-With", "XMLHttpRequest");

            //httpItem.ProxyIp = "127.0.0.1:8888";

            var httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
            Log4netAdapter.WriteInfo(httpResult.Html);

            var jsonObj = JObject.Parse(httpResult.Html);
            var status = jsonObj.SelectToken("$..response[0].status").ToString();
            if (status.Equals("01"))
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
            var captchaStr = GetCaptcha(token);

            var data = new VerCodeRes
            {
                Token = token,
                VerCodeBase64 = captchaStr,
            };
            return data;
        }

        private class BocBankEncryptEntity
        {
            public string loginName { get; set; }
            public string validationChar { get; set; }
            public string activ { get; set; }
            public string state { get; set; }
            public string atmPassword { get; set; }
            public string atmPassword_RC { get; set; }
            public string phoneBankPassword { get; set; }
            public string phoneBankPassword_RC { get; set; }
        }
    }
}
