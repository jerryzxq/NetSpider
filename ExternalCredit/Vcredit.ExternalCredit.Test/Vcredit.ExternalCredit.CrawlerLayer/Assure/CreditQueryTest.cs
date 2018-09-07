using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vcredit.ExternalCredit.CrawlerLayer.Assure;
using System.Web;
using Microsoft.QualityTools.Testing.Fakes;
using Vcredit.Common.Utility;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using Vcredit.ExternalCredit.CrawlerLayer.Assure.Handlers;
using Vcredit.ExtTrade.ModelLayer.Common;
using System.Collections.Generic;

namespace Vcredit.ExternalCredit.Test.Vcredit.ExternalCredit.CrawlerLayer.Assure
{
    [TestClass]
    public class CreditQueryTest
    {
        private CreditQueryImpl crawler = new CreditQueryImpl();

        [TestMethod]
        public void Test_TimeStamp_Convert()
        {
            var ms = 1473840124830;

            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(ms).AddHours((DateTime.Now - DateTime.UtcNow).TotalHours);
        }

        [TestMethod]
        public void CreditQueryTest_NeedCommitUserInfoHandler()
        {
            var data = new NeedCommitUserInfoHandler().HandleRequest(new HandleRequest()).Data;

            Assert.IsTrue(data != null);
        }

        [TestMethod]
        public void CreditQueryTest_TimeLimitHandler()
        {
            var data = new TimeLimitHandler().HandleRequest(new HandleRequest()).DoNextHandler;

            Assert.IsTrue(data);
        }


        [TestMethod]
        public void CreditQueryTest_Calendar()
        {
            var str = "9:00:00,21:59:59";

            var arr = str.Split(new char[] { ',' });

            var d1 = Convert.ToDateTime(arr[0]);
            var d2 = Convert.ToDateTime(arr[1]);

            var now = DateTime.Now;
            Assert.IsTrue(d1.Equals(new DateTime(now.Year, now.Month, now.Day, 9, 0, 0)));
            Assert.IsTrue(d2.Equals(new DateTime(now.Year, now.Month, now.Day, 21, 59, 59)));
        }
    }
}
