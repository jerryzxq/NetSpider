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
    [RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.TianJinGjj)]
    public class TianJinGjjExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
    {
        public static readonly TianJinGjjExecutorImpl Instance = new TianJinGjjExecutorImpl();

        public TianJinGjjExecutorImpl():base()
        {
        }

        /// <summary>
        /// 发送原始数据
        /// </summary>
        /// <returns></returns>
        public BaseRes SendOriginalData(ActivexLoginReq req)
        {
            var entity = new TianJinGjjRequestEntity() ;
            entity.Token = req.Token;
            entity.Account = req.Account;
            entity.Password = req.Password;
            var randNumObj = JObject.Parse(req.AdditionalParam).SelectToken("$..Randnum");
            if (randNumObj == null || randNumObj.ToString().IsEmpty())
                throw new ArgumentException("AdditionalParam 参数中 Randnum 不能为空");

            entity.Randnum = randNumObj.ToString();

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

            var data = new VerCodeRes { Token = tup.Item1, VerCodeBase64 = captcha, Result = GetRandnum() };
            return data;
        }
        private Tuple<string> InitLoginPage()
        {
            var httpItem = new HttpItem()
            {
                URL = "https://cx.zfgjj.cn/dzyw-grwt/index.do",
                Method = "get",
            };
            var httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = httpResult.Cookie;

            string token = Guid.NewGuid().ToString("N");
            return new Tuple<string>(token);
        }
        private string GetCaptcha(string token)
        {
            var httpItem = new HttpItem()
            {
                URL = "https://cx.zfgjj.cn/dzyw-grwt/verify.do?0.6265528072028859",
                Method = "get",
                Cookie = currentCookie,
                ResultType = ResultType.Byte,
                ResultCookieType = ResultCookieType.String,

                Accept = "image/png, image/svg+xml, image/jxr, image/*;q=0.8, */*;q=0.5",
                Referer = "https://cx.zfgjj.cn/dzyw-grwt/index.do",
            };
            var httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = CookieUtil.CookieCombine(currentCookie, httpResult.Cookie);

            RedisHelper.SetCache(token, currentCookie, redisCookiesPackage);
            return CommonFun.GetVercodeBase64(httpResult.ResultByte);
        }
        private string GetRandnum()
        {
            var httpItem = new HttpItem()
            {
                URL = "https://cx.zfgjj.cn/dzyw-grwt/getRandnum.do?0.6265528072028859",
                Method = "get",
                Cookie = currentCookie,
                Accept = "*/*",
                Referer = "https://cx.zfgjj.cn/dzyw-grwt/index.do",
            };
            var httpResult = httpHelper.GetHtml(httpItem);

            return httpResult.Html;
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
            var entity = JsonConvert.DeserializeObject<TianjinGjjEncryptEntity>(encryptStr);

            var a = entity.a.ToUrlEncode();
            var b = entity.b.ToUrlEncode();
            var c = entity.c.ToUrlEncode();
            var d = entity.d.ToUrlEncode();
            var cxmm = entity.cxmm.ToUrlEncode();

            var sfzh = entity.sfzh;
            var yzm = captcha;

            currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
            var _postData = string.Format("a={0}&b={1}&c={2}&d={3}&sfzh={4}&cxmm={5}&yzm={6}", a, b, c, d, sfzh, cxmm, yzm);
            var httpItem = new HttpItem
            {
                URL = "https://cx.zfgjj.cn/dzyw-grwt/loginAction.do ",
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                Accept = "application/json, text/javascript, */*; q=0.01",
                Referer = "https://cx.zfgjj.cn/dzyw-grwt/index.do",
                Cookie = currentCookie,
                Postdata = _postData
            };
            //httpItem.ProxyIp = "127.0.0.1:8888";
            var httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = CookieUtil.CookieCombine(currentCookie, httpResult.Cookie);

            var result = JsonConvert.DeserializeObject<TianJinLoginResult>(httpResult.Html);

            if (result.jg.Equals("0"))
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

        private class TianjinGjjEncryptEntity
        {
            public string sfzh { get; set; }

            public string cxmm { get; set; }

            public string a { get; set; }

            public string b { get; set; }

            public string c { get; set; }

            public string d { get; set; }
        }

        private class TianJinLoginResult
        {
            public string cwms1 { get; set; }
            public string jg { get; set; }
        }

    }
}
