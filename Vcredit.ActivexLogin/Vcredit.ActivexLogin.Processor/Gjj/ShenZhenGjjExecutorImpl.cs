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
    [RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.ShenZhenGjj)]
    public class ShenZhenGjjExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
    {
        public static readonly ShenZhenGjjExecutorImpl Instance = new ShenZhenGjjExecutorImpl();

        public ShenZhenGjjExecutorImpl() : base()
        {
        }

        /// <summary>
        /// 发送原始数据
        /// </summary>
        /// <returns></returns>
        public BaseRes SendOriginalData(ActivexLoginReq req)
        {
            var entity = new ShenZhenGjjRequestEntity();
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
                URL = "https://nbp.szzfgjj.com/newui/login.jsp?transcode=pri",
                Method = "get",
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
                URL = "https://nbp.szzfgjj.com/nbp/ranCode.jsp?tab=card&t=0.07672147006178115",
                Method = "get",
                Cookie = currentCookie,
                ResultType = ResultType.Byte,
                ResultCookieType = ResultCookieType.String,

                Accept = "*/*",
                Referer = "https://nbp.szzfgjj.com/newui/login.jsp?transcode=pri",
            };
            var httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));

            RedisHelper.SetCache(token, currentCookie, redisCookiesPackage);
            return CommonFun.GetVercodeBase64(httpResult.ResultByte);
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
            var entity = JsonConvert.DeserializeObject<ShenZhenGjjEncryptEntity>(encryptStr);

            var CardNo = entity.CardNo;
            var identifyCode = captcha;
            var sSignTxt = "";
            var task = "pri";
            var transcode = "card";
            var ssoLogin = "";
            var QryPwd = entity.QryPwd;

            currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
            var _postData = string.Format("CardNo={0}&identifyCode={1}&sSignTxt={2}&task={3}&transcode={4}&ssoLogin={5}&QryPwd={6}&x=159&y=20",
                CardNo, identifyCode, sSignTxt, task, transcode, ssoLogin, QryPwd);
            var httpItem = new HttpItem
            {
                URL = "https://nbp.szzfgjj.com/Login",
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded",
                Accept = "image/gif, image/jpeg, image/pjpeg, application/x-ms-application, application/xaml+xml, application/x-ms-xbap, */*",
                Referer = "https://nbp.szzfgjj.com/newui/login.jsp?transcode=pri",
                Cookie = currentCookie,
                Postdata = _postData
            };
            //httpItem.ProxyIp = "127.0.0.1:8888";
            var httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));

            if (httpResult.Html.Contains("location.href='/nbp/ydp/mainPri_new.jsp'"))
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

        private class ShenZhenGjjEncryptEntity
        {
            public string CardNo { get; set; }

            public string QryPwd { get; set; }
        }
    }

}
