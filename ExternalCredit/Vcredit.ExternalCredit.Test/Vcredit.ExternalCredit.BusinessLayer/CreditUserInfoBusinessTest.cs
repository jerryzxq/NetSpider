using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vcredit.ExternalCredit.CrawlerLayer.Assure;
using System.Web;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using System.IO;
using Vcredit.ExternalCredit.Dto.Assure;
using System.Collections.Generic;
using Vcredit.ExtTrade.BusinessLayer;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.Test.Vcredit.ExternalCredit.BusinessLayer
{
    [TestClass]
    public class CreditUserInfoBusinessTest
    {
        private CRD_CD_CreditUserInfoBusiness biz = new CRD_CD_CreditUserInfoBusiness();

        [TestMethod]
        public void CreditUserInfoBusinessTest_GetNeedDownload()
        {
            // Arrange
            var perCount = 3000;

            // Act
            var result = biz.GetNeedDownload(perCount);

            // Assert
            Assert.IsTrue(result != null);
        }

        [TestMethod]
        public void CreditUserInfoBusinessTest_GetConnectionFailedUsersByJobConfig()
        {
            // Arrange
            var perCount = 50;
            var jobIndex = 0;
            var jobCount = 2;

            // Act
            var result = biz.GetConnectionFailedUsersByJobConfig(perCount, jobCount, jobIndex);

            // Assert
            Assert.IsTrue(result != null);
        }

        [TestMethod]
        public void ReadExcelFileTest()
        {
            var filePath = @"E:\02_tfs\ExternalCredit\Develop\ExternalCredit-170309\AssureAnalysisProgram\Template\征信报告与数据库对应表格.xlsx";
            for (int i = 0; i < 200; i++)
            {
                new TaskFactory().StartNew(() =>
                {
                    var file = File.ReadAllText(filePath);

                    Console.WriteLine(file);
                });

            }
        }
    }
}
