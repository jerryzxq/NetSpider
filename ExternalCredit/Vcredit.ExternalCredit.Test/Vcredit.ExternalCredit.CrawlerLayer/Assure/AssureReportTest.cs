using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vcredit.ExternalCredit.CrawlerLayer.Assure;
using System.Web;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using System.IO;
using Vcredit.ExternalCredit.Dto.Assure;
using System.Collections.Generic;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
using System.Reflection;

namespace Vcredit.ExternalCredit.Test.Vcredit.ExternalCredit.CrawlerLayer.Assure
{
    [TestClass]
    public class AssureReportTest
    {
        AssureReport reposrtInstance = new AssureReport();

        [TestMethod]
        public void Test_AssureReport_EncodePassword()
        {
            var pwd = @"MIIEjAYJKoZIhvcNAQcCoIIEfTCCBHkCAQExDzANBglghkgBZQMEAgEFADAXBgkqhkiG9w0BBwGgCgQIcGFzczEyMzSgggNnMIIDYzCCAkugAwIBAgIQEAAAAAAAAAAAAAAQWJdGdTANBgkqhkiG9w0BAQUFADAhMQswCQYDVQQGEwJDTjESMBAGA1UEChMJQ0ZDQSBPQ0ExMB4XDTE2MTAxMDAyNDA0NloXDTE4MTAxMDAyNDA0NlowdDELMAkGA1UEBhMCY24xEjAQBgNVBAoTCUNGQ0EgT0NBMTENMAsGA1UECxMEQ1JDQzEUMBIGA1UECxMLRW50ZXJwcmlzZXMxLDAqBgNVBAMUIzA0MUA3Njk3MDg3NTUtNUB3YW5neWFueXVuQDAwMDAwMDA4MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDTk9nVeDjZjfxmGxpqiCychsOzRIHV2UD6SHYxVrNX7rIUD8KfOco19fdirVCbsfqUV6jYZWJQPj2aCne+Sh7j5QFB0RODR9CfpSppSTJnpqyA6+Xoa6ini540VFRGFDPTwRH2Pa5kUSpqemUR0t7ApfDSthei4r1z+T+hVOPE/QIDAQABo4HHMIHEMB8GA1UdIwQYMBaAFNHb6YiC5d0aj0yqAIy+fPKrG/bZMAkGA1UdEwQCMAAwNwYDVR0fBDAwLjAsoCqgKIYmaHR0cDovL2NybC5jZmNhLmNvbS5jbi9SU0EvY3JsNDMzMi5jcmwwCwYDVR0PBAQDAgTwMB0GA1UdDgQWBBRZAfzZrRAaayXQmjOUyXbSTOlzZjAxBgNVHSUEKjAoBggrBgEFBQcDAgYIKwYBBQUHAwMGCCsGAQUFBwMEBggrBgEFBQcDATANBgkqhkiG9w0BAQUFAAOCAQEAB8P3LXxvTt7U692UKJxQb4GGK4x/V8xc4uej/mA6KYIY6U9pL3+GQmN3gnche4mvl7mTicy2CvkBYJXXJVv7V2Ed6EQz/kmUOrD9Pe+HGWvP5cP3Mo2SFPxOXODwv4yvGRYaGbuxCwSTy441Dmhu5ONv/PsS0BssDILNKHxGQ0vFxmVQYx2fZfz88cYj3ohksgm5wwYrZVPRgRGLjibzcqIHaahy/s1rQiluY6we694MEjWzsf6mLo/+RYfIB7jFIY29Vt1q4jTWx4NYnXG9RPFjDZnZWaUcBwWCyyXF+WKdVv3QhABETy1WqavtTi+fG4lbx1ABWehwbiYZArnbgDGB3jCB2wIBATA1MCExCzAJBgNVBAYTAkNOMRIwEAYDVQQKEwlDRkNBIE9DQTECEBAAAAAAAAAAAAAAEFiXRnUwDQYJYIZIAWUDBAIBBQAwDQYJKoZIhvcNAQELBQAEgYAvE2QzjxFG7idikMpo7+bXmhrNk5GLYAhPz5cePcXJc5zXE0M+mz1dbxBNAovxSuDu8n2wHd2EK2cV3q46hgCZCg4CR2QKWm21KdFHKsBXBDzP5JnvxxxK6JPrRkqra5a+6CozRYabzyCchkvJLQ8IMWQ6SC8ch6f43r/nPc4PWg==";

            var newPwd = pwd.ToUrlEncode();
        }

        [TestMethod]
        public void Test_StringToUrlEncode()
        {
            var s = null as string;

            var str = "中国对外经济贸易信托有限公司";

            str = str.ToUrlEncode();


            str = "%25E6%259D%25AD%25E5%25B7%259E%25E9%2593%25B6%25E8%25A1%258C%25E8%2582%25A1%25E4%25BB%25BD%25E6%259C%2589%25E9%2599%2590%25E5%2585%25AC%25E5%258F%25B8";

            str = str.ToUrlDecode();
        }


        [TestMethod]
        public void Test_AssureReport_Reported()
        {
            var rp = new AssureReport();

            rp.StartReported();
        }

        //[TestMethod]
        //public void Test_AssureReport_Login()
        //{
        //    var rp = new AssureReport();

        //    var result = rp.Login();
        //}

        [TestMethod]
        public void Test_GetEntities()
        {
            var txtes = File.ReadLines(@"E:\share\担保上报.txt");

            var list = new List<AssureReportedParamDto>();
            foreach (var txt in txtes)
            {
                var sp = txt.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                if (sp.Length < 9)
                    continue;

                var _paramDto = new AssureReportedParamDto
                {
                    GuaranteeLetterCode = sp[0],
                    GuaranteeContractCode = sp[1],
                    WarranteeName = sp[2],
                    WarranteeCertNo = sp[3],
                    GuaranteeStartDate = Convert.ToDateTime(sp[4]),
                    GuaranteeStopDate = Convert.ToDateTime(sp[5]),
                    GuaranteeSum = Convert.ToDecimal(sp[6]),
                    Rate = Convert.ToDecimal(sp[7]),
                    InkeepBalance = Convert.ToDecimal(sp[8]),
                };

                list.Add(_paramDto);
            }
        }

        [TestMethod]
        public void Test_SearchCredit()
        {
            var entity = new CRD_CD_AssureReportedInfoEntity()
            {
                CreditorName = "杭州银行股份有限公司",
            };
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = reposrtInstance.GetType();
            FieldInfo field = type.GetField("_currentCookies", flag);
            field.SetValue(reposrtInstance, "JSESSIONID=a!1100347494;BIGipServerpool_xwqy=jrivdIZKoPXbEkAJhDwsLENSvVT0llsfYk8RKP6KfNoEkamgpYwq1n8ch+hTSLo63+ql9lx19onNbS8=");

            field = type.GetField("_currentEntity", flag);
            field.SetValue(reposrtInstance, entity);

            MethodInfo m = type.GetMethod("SearchCredit", flag);
            m.Invoke(reposrtInstance, null);

            field = type.GetField("financeCode", flag);
            var financeCode = field.GetValue(reposrtInstance) as string;
            Assert.IsTrue(field.GetValue(reposrtInstance) != null);
        }

        [TestMethod]
        public void Test_DoStepOne()
        {
            var entity = new CRD_CD_AssureReportedInfoEntity()
            {
                GuaranteeLetterCode = "12345678",
                WarranteeCertNo = "1233"
            };
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = reposrtInstance.GetType();

            var field = type.GetField("_currentEntity", flag);
            field.SetValue(reposrtInstance, entity);

            MethodInfo m = type.GetMethod("DoStepOne", flag);
            var r = (bool)m.Invoke(reposrtInstance, null);

            Assert.IsFalse(r);

        }

    }
}
