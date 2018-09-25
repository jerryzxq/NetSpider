using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    /// 农业银行
    /// </summary>
    [RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.AbcBank)]
    public class AbcBankExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
    {
        public static readonly AbcBankExecutorImpl Instance = new AbcBankExecutorImpl();

        private string _redisEntityPackage;

        public AbcBankExecutorImpl() : base()
        {
            _redisEntityPackage = redisPackage + ":Entity";
        }

        public BaseRes SendOriginalData(ActivexLoginReq req)
        {
            var entity = new AbcBankRequestEntity();
            entity.Token = req.Token;
            entity.Account = req.Account;
            entity.Password = req.Password;

            if (req.AdditionalParam.IsEmpty())
                throw new ArgumentNullException("AdditionalParam 不能为空");

            var jObj = JObject.Parse(req.AdditionalParam);
            var timestamp = jObj.SelectToken("$..TimeStamp");
            if (timestamp == null || timestamp.ToString().IsEmpty())
                throw new ArgumentException("AdditionalParam 参数中 TimeStamp 不能为空");
            entity.TimeStamp = timestamp.ToString();

            var randomText = jObj.SelectToken("$..RandomText");
            if (randomText == null || randomText.ToString().IsEmpty())
                throw new ArgumentException("AdditionalParam 参数中 RandomText 不能为空");
            entity.RandomText = randomText.ToString();

            var Abc_FormId = jObj.SelectToken("$..Abc_FormId");
            if (Abc_FormId == null || Abc_FormId.ToString().IsEmpty())
                throw new ArgumentException("AdditionalParam 参数中 Abc_FormId 不能为空");
            entity.Abc_FormId = Abc_FormId.ToString();

            entity.UrlParam = string.Format("?randomText={0}&timeStamp={1}", entity.RandomText, entity.TimeStamp);

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
            var captchaStr = GetCaptcha(tup.Item1);

            var r = new
            {
                TimeStamp = tup.Item2,
                RandomText = tup.Item3,
                Abc_FormId = tup.Item4
            };
            var data = new VerCodeRes
            {
                Token = tup.Item1,
                Result = JsonConvert.SerializeObject(r),
                VerCodeBase64 = captchaStr
            };
            return data;
        }

        private string GetCaptcha(string token)
        {
            var httpItem = new HttpItem()
            {
                URL = "https://perbank.abchina.com/EbankSite/LogonImageCodeAct.do?r=0.6824938379423977",
                Method = "get",
                Cookie = currentCookie,
                ResultType = ResultType.Byte,
                ResultCookieType = ResultCookieType.String,

                Accept = "image/png, image/svg+xml, image/jxr, image/*;q=0.8, */*;q=0.5",
                Referer = "https://perbank.abchina.com/EbankSite/startup.do",
            };
            var httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
            RedisHelper.SetCache(token, currentCookie, redisCookiesPackage);
            File.WriteAllBytes("D:\\Image.png", httpResult.ResultByte);
            return CommonFun.GetVercodeBase64(httpResult.ResultByte);
        }

        private Tuple<string, string, string, string> InitLoginPage()
        {
            var httpItem = new HttpItem()
            {
                URL = "https://perbank.abchina.com/EbankSite/startup.do",
                Method = "get",
                Accept = "text/html, application/xhtml+xml, image/jxr, */*",
                Allowautoredirect = false,
                ResultCookieType = ResultCookieType.String,
            };
            var httpResult = httpHelper.GetHtml(httpItem);

            currentCookie = CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie);
            if (!httpResult.RedirectUrl.IsEmpty())
            {
                httpItem = new HttpItem()
                {
                    URL = httpResult.RedirectUrl,
                    Method = "get",
                    Accept = "text/html, application/xhtml+xml, image/jxr, */*",
                    Allowautoredirect = false,
                    ResultCookieType = ResultCookieType.String,
                    Cookie = currentCookie,
                };
                httpResult = httpHelper.GetHtml(httpItem);
            }

            currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));

            var timestamp = Regex.Match(httpResult.Html, @"(?<=var macInfo = password.GetMachineCode\('powerpass_ie', ').*(?='\);)").Value;
            var randomText = Regex.Match(httpResult.Html, @"(?<=password\.WritePassObjectOrg\(""powerpass_ie"", { ""randomText"": "").*(?="", ""softkbdType"")").Value;

            var abc_formId = Regex.Match(httpResult.Html, "(?<=<input type=\"hidden\" name=\"abc_formId\" value=\").*(?=\"/>)").Value;

            string token = Guid.NewGuid().ToString("N");
            RedisHelper.SetCache(token, currentCookie, redisCookiesPackage);
            return new Tuple<string, string, string, string>(token, timestamp, randomText, abc_formId);
        }

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
            var entity = JsonConvert.DeserializeObject<AbcBankRequestEntity>(RedisHelper.GetCache<string>(token, _redisEntityPackage));
            var encryptentity = JsonConvert.DeserializeObject<AbcBankEncryptEntity>(encryptStr);

            var _postData = string.Format("username={0}&code={1}&EntranceType={2}&abc_formId={3}&picCode={4}&MachineCode={5}&MachineInfo={6}&token={7}&plattype={8}&pwdField={9}&pwdFieldKeys={10}",
                entity.Account,
                captcha,
                encryptentity.EntranceType,
                entity.Abc_FormId,
                captcha,
                encryptentity.MachineCode.ToUrlEncode(),
                encryptentity.MachineInfo.ToUrlEncode(),
                encryptentity.token,
                encryptentity.plattype,
                encryptentity.pwdField.ToUrlEncode(),
                encryptentity.pwdFieldKeys);

            currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
            var httpItem = new HttpItem
            {
                URL = "https://perbank.abchina.com/EbankSite/upLogin.do",
                Method = "POST",
                Accept = "text/html, application/xhtml+xml, image/jxr, */*",
                ContentType = "application/x-www-form-urlencoded",
                Referer = "https://perbank.abchina.com/EbankSite/startup.do",
                Host = "perbank.abchina.com",

                PostEncoding = Encoding.UTF8,
                Postdata = _postData,
                Cookie = currentCookie,
            };
            httpItem.Header.Add("Accept-Language", "zh-Hans-CN,zh-Hans;q=0.5");
            httpItem.Header.Add("Accept-Encoding", "gzip, deflate");
            //httpItem.ProxyIp = "127.0.0.1:8888";

            var httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
            Log4netAdapter.WriteInfo(httpResult.Html);

            var doc = NSoup.NSoupClient.Parse(httpResult.Html);
            if (doc.Title.Contains("个人网上银行-用户名登录-短信校验"))
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

        private class AbcBankEncryptEntity
        {
            public string username { get; set; }
            public string EntranceType { get; set; }
            public string abc_formId { get; set; }
            public string picCode { get; set; }
            public string MachineCode { get; set; }
            public string MachineInfo { get; set; }
            public string token { get; set; }
            public string plattype { get; set; }
            public string pwdField { get; set; }
            public string pwdFieldKeys { get; set; }
        }

    }
}
