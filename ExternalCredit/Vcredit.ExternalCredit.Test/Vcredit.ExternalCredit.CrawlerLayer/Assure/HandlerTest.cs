using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vcredit.ExternalCredit.CrawlerLayer.Assure.Handlers;
using Vcredit.ExternalCredit.CommonLayer.helper;

namespace Vcredit.ExternalCredit.Test.Vcredit.ExternalCredit.CrawlerLayer.Assure
{
    [TestClass]
    public class HandlerTest
    {
        [TestMethod]
        public void HandlerTest_NeedCommitUserInfoHandler()
        {
            // Arrange

            // Act
            var result = new NeedCommitUserInfoHandler().HandleRequest(new HandleRequest());

            // Assert
            Assert.IsTrue(result.Data != null);
        }

        [TestMethod]
        public void HandlerTest_GetCookieHandler()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
  ((sender, certificate, chain, sslPolicyErrors) => true);

            // act
            new GetCookieHandler().HandleRequest(new HandleRequest());
        }

        [TestMethod]
        public void HandlerTest_RedisTest()
        {
            var _redisPackage = "assure";
            var key = "0987654321";

            RedisHelper.SetCache(key, "valuevaluevaluevalue", _redisPackage);

            var _currentCookie = RedisHelper.GetCache<string>(key, _redisPackage);
        }


    }
}
