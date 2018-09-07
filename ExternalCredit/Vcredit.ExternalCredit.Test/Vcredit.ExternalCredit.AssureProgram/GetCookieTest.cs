using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vcredit.ExternalCredit.CommonLayer.helper;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace Vcredit.ExternalCredit.Test.Vcredit.ExternalCredit.AssureProgram
{
    [TestClass]
    public class GetCookieTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var _cookieToken = "63E7B05E1FAE4613A10F3E06B5712902";

            var package = "Assure";
            RedisHelper.SetCache(_cookieToken, "2wsxWWWWW", package, -20);

            var _currentCookie = RedisHelper.GetCache<string>(_cookieToken, package);


            string CookieToken = "DB21E553A5834E66AAB4EC32434101DD";
            var cookie = RedisHelper.GetCache<string>(CookieToken, "");

            var name = "雷超江";
            name = name.ToUrlEncode();

            //var postData = string.Format("scardtype={0}&scardno={1}&sname={2}&spersonreason={3}",
            //                                       "789456123",
            //                                       "",
            //                                       user.Name,
            //                                       user.QueryReason).ToUrlEncode();

        }
       
    }

}
