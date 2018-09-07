using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vcredit.ExternalCredit.Services;
using Newtonsoft.Json;
using System.IO;
using Vcredit.ExternalCredit.Services.Requests;
using System.Collections.Generic;
using Vcredit.ExternalCredit.Services.Responses;
using Vcredit.ExternalCredit.Services.Impl;
using Vcredit.Common.Ext;

namespace Vcredit.ExternalCredit.Test.Vcredit.ExternalCredit.Services
{
    [TestClass]
    public class ComplianceServiceTest
    {
        private ComplianceService _service = new ComplianceServiceImpl();

        [TestMethod]
        public void ComplianceServiceTest_IsSignatured()
        {
            // Arrange
            var txts = File.ReadAllLines(@"E:\02_tfs\ExternalCredit\Develop\ExternalCredit-1230\Vcredit.ExternalCredit.Test\Vcredit.ExternalCredit.Services\test_signatured_data.txt");

            var requests = new List<IsSignaturedRequest>();
            foreach (var txt in txts)
            {
                var request = new IsSignaturedRequest();
                var colums = txt.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (colums.Length < 3)
                    continue;

                request.ApplyID = colums[0].ToInt(0);
                request.IdentityNo = colums[1];
                request.Name = colums[2];

                requests.Add(request);
            }

            // act
            var responsese = new List<IsSignaturedResponse>();
            foreach (var re in requests)
            {
                var response = _service.IsSignatured(re);

                responsese.Add(response);
            }

            // assert
            Assert.IsTrue(responsese.Count == 5);
        }

        [TestMethod]
        public void ComplianceServiceTest_CallBack()
        {
            // Arrange
            var req = new CallBackRequest { ApplyID = 12 };

            // act
            var response = _service.CallBack(req);

            // assert
            Assert.IsTrue(response != null);
        }
    }
}
