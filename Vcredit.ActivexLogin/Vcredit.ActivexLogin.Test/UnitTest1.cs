using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vcredit.ActivexLogin.App.Business;
using static Vcredit.ActivexLogin.Common.ProjectEnums;
using Vcredit.ActivexLogin.Processor;
using Vcredit.Common.Helper;
using Newtonsoft.Json;
using System.Text;
using Vcredit.ActivexLogin.FrameWork;
using Vcredit.Common.Ext;

namespace Vcredit.ActivexLogin.Test
{
    [TestClass]
    public class UnitTest1
    {
        private static HttpHelper httpHelper = new HttpHelper();

        [TestMethod]
        public void TestMethod1()
        {
            var old = @"L3TifjN09TATdTtBBB+Trt/hFuMqNFzegqx/zzHAFMEaKqwwQsebh+Rw//m+SkPsL6siQJqC1MQy9HUMAKL7uHe2551IQeLmuoZZ/rM6AXka/MHxZQQXrSJ3HN+9F5XDM6pywcZ2bsVQBI6WM6nSd+SZURBFdS6oZ3ILb5IZtMHVcBaycJLEN/S0e2/7x/uKPtVZ43eiLML2lk5oxWj6O3cbszJHoTeruHq7Vz0WXPxPlelMbp9TOThur4S6sd8tUZ3qUGBY16ekKNJuVWZdWErCiUoTbzGpAGTb1B7nccRms0EGbxFv0OEPqiEPFWROeP90/EaFam74fWsQxnb3QnUNmwf9bspsJNpERr6o42l+XXXtgahRo+d9XPvZGLIaFzRhnNBGrNoBuepm1FFNXJ4qgmLmyKGt4IgFFm1zuOuTZdZK0d5bq85lKumn1us8H1aHoJW8s0FzuIYyAZvUJ5uZTA9bU+7qZ4BOXZ4yRYohWLIlzXahSlDuHFSZq/n7Htfo6JNCV7gPVCnCEvvOgGxkCWPuiAkLVZNU05LwUtMQh1A02XJNzxaHmwusnfp7qMPB96qNjqX3CrU9ALLmoIcdRYaYU/rBg3d6HDm4FOKqfY9tqYmQHrlHjRkzWm4CbntvkaVUoxTE+lhyKgc4q833M7jkdozYfmfUJWkFoKE=";

            var newpwd = old.ToUrlEncode();
        }

        [TestMethod]
        public void AddTestData()
        {
            for (int i = 0; i < 100; i++)
            {
                DoRequest("{'SRand':'MTczNjQ3MDkyMg==','LoginType':'1','Name':'张**'}", WebSiteType.GuangZhouGjj);
                DoRequest("{'SRand':'MTczNjQ3MDkyMg==','LoginType':'2','Name':'张**'}", WebSiteType.GuangZhouGjj);

                DoRequest("{'Randnum':'72102385742474575266205500057137'}", WebSiteType.TianJinGjj);

                DoRequest("", WebSiteType.ShenZhenGjj);

                DoRequest("{'GenShell_ClientNo':'72102385742474575266205500057137'}", WebSiteType.CmbBank);
            }
        }
        private string DoRequest(string additionalParam, WebSiteType type)
        {
            var postEntity = new
            {
                Token = Guid.NewGuid().ToString("N"),
                Account = "8888888888888888",
                Password = "111111",
                AdditionalParam = additionalParam,
                SiteType = (int)type,
            };
            var httpItem = new HttpItem()
            {
                URL = "http://10.138.60.48:5400/api/ActivexLogin/SendOriginalData",
                Method = "POST",
                ContentType="application/json",
                Postdata = JsonConvert.SerializeObject(postEntity),
                PostEncoding = Encoding.UTF8,
            };
            var httpResult = httpHelper.GetHtml(httpItem);

            return httpResult.Html;
        }
    }
}
