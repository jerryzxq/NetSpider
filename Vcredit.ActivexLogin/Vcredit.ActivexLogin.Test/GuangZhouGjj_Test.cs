using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vcredit.Common.Helper;
using Newtonsoft.Json.Linq;
using Vcredit.Common.Ext;
using System.IO;
using Newtonsoft.Json;
using Vcredit.Framework.Queue.Redis;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Vcredit.ActivexLogin.FrameWork;
using System.Text;

namespace Vcredit.ActivexLogin.Test
{
    [TestClass]
    public class GuangZhouGjj_Test
    {
        private static readonly HttpHelper _helper = new HttpHelper();
        private string currentCookie = "";

        [TestMethod]
        public void Test_Login()
        {
            Init();

            GetCaptcha();

            DoLogin();
        }

        private void Init()
        {
            var httpItem = new HttpItem()
            {
                URL = "https://gzgjj.gov.cn/wsywgr/",
                Method = "get",
            };
            var httpResult = _helper.GetHtml(httpItem);
            currentCookie = httpResult.Cookie;
            var doc = NSoup.NSoupClient.Parse(httpResult.Html);

            var sRand = Regex.Match(httpResult.Html, "(?<=var sRand = \").*(?=\";)").Value;

            string queueName = "GuangZhouGjj_Queue";
            var list = new List<dynamic>();
            var msg = new { Token = Guid.NewGuid().ToString("N"), Account = "44028119841228071300", Password = "080808", SRand = sRand };
            list.Add(JsonConvert.SerializeObject(msg));
            RedisQueue.Send<dynamic>(list, queueName);
        }

        private void GetCaptcha()
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
            var httpResult = _helper.GetHtml(httpItem);
            currentCookie = CookieUtil.CookieCombine(currentCookie, httpResult.Cookie);
            File.WriteAllBytes("d:\\captcha.png", httpResult.ResultByte);
        }

        private void DoLogin()
        {
            var certno = "Am+ZFrovS0WygSKdsGd+756cAGHrGSh7NGxCXCj89+LA0HnGSiKSsGHRGx/OEJBEb4OFRSMxL9voJRP36sChOTtFk4AdsLn0qnTQS4vHwGA8eZqYJ+DS4nwR8AOeIzMg1PTqDIfTniih3MWkUebEPgsRSQp77H5GVnYu4jfaN6EOAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAC9bcjaQX4GfKDkNz8d/dHLYRVJJQeiL/8=".ToUrlEncode();
            var password = "AsYfKNMNf3TrJgeII1I/2l9JHSkUhw1t4LSkyXy0ufalRCuAWf9Rp8e0X0lEmdwvceMsHB8HqD4XwSNQoIdYzeFw/niQwgar59G2CetBHtE8esSzSi38LieCCD08xq37ATRxGE5X+t0iqOuLACpwhcYiFlQMYXNW59qM9iggFys8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAi/+3Rbacv6A==".ToUrlEncode();
            var captcha = "";

            var postData = string.Format("radiobutton=radiobutton&certno={0}&zjh=undefined&name=&password={1}&captcha={2}",
                                        certno, password, captcha);
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
            var httpResult = _helper.GetHtml(httpItem);
            currentCookie = CookieUtil.CookieCombine(currentCookie, httpResult.Cookie);
           
            httpItem = new HttpItem()
            {
                URL = httpResult.RedirectUrl,
                Method = "get",
                Cookie = currentCookie,

                Accept = "text/html, application/xhtml+xml, image/jxr, */*",
                Referer = "https://gzgjj.gov.cn/wsywgr/",

            };
            httpResult = _helper.GetHtml(httpItem);

        }

        [TestMethod]
        public void Test()
        {
            //var jsonStr = "{\"Reason\":1,\"ReasonDescription\":\"加密成功\",\"Data\":\"{  \\\"cxmm\\\": \\\"cxmm\\\",  \\\"a\\\": \\\"a\\\",  \\\"b\\\": \\\"b\\\",  \\\"c\\\": \\\"c\\\",  \\\"d\\\": \\\"d\\\",  \\\"sfzh\\\": \\\"12011219751123001X\\\"}\"}";
            //var jObj = JObject.Parse(jsonStr);

            var postData = new
            {
                Account= "string",
                Password = "string",
                AdditionalParam = "{'SRand':'ODE0NTUyNTkyNQ==', 'LoginType':'2', 'Name':'苏涛'}",
                Token = "2222222222222222222222222",
                SiteType = 1001,
            };

            var httpItem = new HttpItem()
            {
                URL = "http://10.138.60.48:5400/api/ActivexLogin/SendOriginalData",
                Method = "POST",
                Accept= "application/json",
                ContentType = "application/json",
                Postdata = JsonConvert.SerializeObject(postData),
                PostEncoding = Encoding.UTF8
            };
            var httpResult = _helper.GetHtml(httpItem);
        }

    }
}
