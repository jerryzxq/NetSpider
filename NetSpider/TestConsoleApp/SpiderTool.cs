using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using LumiSoft.Net.IMAP;
using LumiSoft.Net.IMAP.Client;
using LumiSoft.Net.Mail;
using LumiSoft.Net.Mime;
using LumiSoft.Net.MIME;
using LumiSoft.Net.Mime.vCard;
using LumiSoft.Net.POP3.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using S22.Imap;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Crawler;
using Vcredit.NetSpider.Crawler.Edu;
using Vcredit.NetSpider.Crawler.Social;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.Chsi;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.Processor;
using Vcredit.NetSpider.Service;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity.Service.Mobile;
using Vcredit.NetSpider.Crawler.Mobile;
using Vcredit.NetSpider.Entity;
using Aspose.Cells;

namespace TestConsoleApp
{
    public class SpiderTool
    {
        static IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();
        static IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();
        static IPluginSecurityCode secodeParser = PluginServiceManager.GetSecurityCodeParserPlugin();
        static HttpHelper http = new HttpHelper();
        static CookieCollection cookies = new CookieCollection();


        //static IExecutor executor = ExecutorManager.GetExecutor();
        #region MyRegion
        // 创建Key
        public static string GenerateKey()
        {
            DESCryptoServiceProvider desCrypto = (DESCryptoServiceProvider)DESCryptoServiceProvider.Create();
            return ASCIIEncoding.ASCII.GetString(desCrypto.Key);
        }
        // 加密字符串
        public static string EncryptString(string sInputString, string sKey)
        {
            byte[] data = Encoding.UTF8.GetBytes(sInputString);
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            ICryptoTransform desencrypt = DES.CreateEncryptor();
            byte[] result = desencrypt.TransformFinalBlock(data, 0, data.Length);
            return BitConverter.ToString(result);
        }
        // 解密字符串
        public static string DecryptString(string sInputString, string sKey)
        {
            string[] sInput = sInputString.Split("-".ToCharArray());
            byte[] data = new byte[sInput.Length];
            for (int i = 0; i < sInput.Length; i++)
            {
                data[i] = byte.Parse(sInput[i], NumberStyles.HexNumber);
            }
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            ICryptoTransform desencrypt = DES.CreateDecryptor();
            byte[] result = desencrypt.TransformFinalBlock(data, 0, data.Length);
            return Encoding.UTF8.GetString(result);
        }
        #endregion

        #region crawler

        #region 学信网
        public static void TestChsi_Info()
        {
            IChsi_Info service = NetSpiderFactoryManager.GetChsi_InfoService();
            IChsiCrawler crawler = CrawlerManager.GetChsiCrawler();
            var res = crawler.Query_Init();
            LoginReq req = new LoginReq();
            req.Username = "13524909205";
            req.Password = "082713";
            req.Token = res.Token;
            var query = crawler.Query_GetInfo(req);
            Chsi_InfoEntity entity = JsonConvert.DeserializeObject<Chsi_InfoEntity>(JsonConvert.SerializeObject(query));
            service.Save(entity);
        }
        public static void TestChsi_Register()
        {
            IChsi_Info service = NetSpiderFactoryManager.GetChsi_InfoService();
            IChsiCrawler crawler = CrawlerManager.GetChsiCrawler();
            ChsiRegisterReq regReq = new ChsiRegisterReq();
            regReq.Mobile = "13524909205";
            VerCodeRes codeRes = crawler.Register_Init(regReq);
            regReq.Name = "张志博";
            regReq.Password = "082713";
            regReq.Password1 = "082713";
            regReq.Credentialtype = "SFZ";
            regReq.Identitycard = "410923198207131718";
            regReq.Email = "raul7.zhang@163.com";
            regReq.Pwdreq1 = "1";
            regReq.Pwdreq2 = "5";
            regReq.Pwdreq3 = "9";
            regReq.Pwdanswer1 = "郭翠粉";
            regReq.Pwdanswer2 = "邱相生";
            regReq.Pwdanswer3 = "刘慧";
            regReq.Token = codeRes.Token;

            Console.Write("请输入验证码：");
            regReq.Vercode = Console.ReadLine();
            BaseRes res = crawler.Register_Step1(regReq);
            Console.Write("请输入短信验证码：");
            regReq.Smscode = Console.ReadLine();
            res = crawler.Register_Step2(regReq);
        }
        public static void TestChsi_ForgetPwd()
        {
            IChsi_Info service = NetSpiderFactoryManager.GetChsi_InfoService();
            IChsiCrawler crawler = CrawlerManager.GetChsiCrawler();
            ChsiForgetReq regReq = new ChsiForgetReq();
            VerCodeRes codeRes = crawler.ForgetPwd_Step1();
            regReq.Username = "13524909205";
            Console.Write("请输入验证码：");
            regReq.Vercode = Console.ReadLine();
            regReq.Token = codeRes.Token;
            codeRes = crawler.ForgetPwd_Step2(regReq);
            regReq.Name = "张志博";
            regReq.Mobile = "13524909205";
            regReq.Identitycard = "410923198207131718";
            Console.Write("请输入验证码：");
            regReq.Vercode = Console.ReadLine();

            BaseRes baseRes = crawler.ForgetPwd_Step3(regReq);
            regReq.Password = "082713";
            regReq.Password1 = "082713";
            Console.Write("请输入短信验证码：");
            regReq.Smscode = Console.ReadLine();

            baseRes = crawler.ForgetPwd_Step4(regReq);

        }

        #endregion

        #region 社保
        #endregion

        #region 公积金
        public static void GetGJJ()
        {
            string province = string.Empty;//查询省份
            string city = string.Empty;//查询城市
            string username = string.Empty;//用户名
            string IDNumber = string.Empty;//证件号码
            string password = string.Empty;//密码
            string identitycard = string.Empty;
            string name = string.Empty;
            province = "HB";
            city = "xiangyang";
            //  IDNumber = "34020419890504232X";
            identitycard = "420620196402010505";//22062519890513073X
            username = "3305265862343";
            name = "马锐";
            password = "010505";//000000  LA76810
            IProvidentFundCrawler crawlerService = CrawlerManager.GetProvidentFundCrawler(province, city);
            VerCodeRes codeRes = crawlerService.ProvidentFundInit();
            ProvidentFundReq fundReq = new ProvidentFundReq()
            {
                Token = codeRes.Token,
                Password = password,
                Username = username,
                Name = name,
                Identitycard = identitycard
            };
            Console.Write("请输入验证码：");
            fundReq.Vercode = Console.ReadLine();
            ProvidentFundQueryRes Res = crawlerService.ProvidentFundQuery(fundReq);
            Console.WriteLine(jsonParser.SerializeObject(Res));
        }
        #endregion

        #region 手机

        public static void GetMobile()
        {
            string region = string.Empty;
            string mobile = string.Empty;
            string password = string.Empty;

            mobile = "18173116585";
            password = "266173";
            region = "BJ";
            //mobile = "15360669026";
            //password = "456123";
            mobile = "18173116585";
            password = "312230";

            MobileReq fundReq = new MobileReq()
            {
                Mobile = mobile,
                Password = password,
                IdentityCard = "123456789",
                Name = "zhangsan"
            };
            IMobileCrawler crawlerService = CrawlerManager.GetMobileCrawler(EnumMobileCompany.ChinaMobile, region);
            //VerCodeRes codeRes = crawlerService.MobileInit(fundReq);
            fundReq.Token = "81d27ceb4b60471ca970ab7ff9646f7d";

            //Console.Write("请输入验证码：");
            ////fundReq.Vercode = Console.ReadLine();
            //BaseRes Res = crawlerService.MobileLogin(fundReq);
            ////Res = crawlerService.MobileSendSms(fundReq);
            ////Console.Write("请输入短信验证码：");
            ////fundReq.Smscode = Console.ReadLine();
            ////Res = crawlerService.MobileCheckSms(fundReq);
            //Res = crawlerService.MobileCrawler(fundReq);
            //BaseRes Res = crawlerService.MobileAnalysis(fundReq);
            //var s = "";
        }

        public static void ExportMobileNum()
        {
            string filepath = "e://mobile/mobile.xlsx";
            Workbook workbook = new Workbook(filepath);
            Worksheet sheet = workbook.Worksheets[0];
            int count = sheet.Cells.Rows.Count;
            IMobileCrawler crawlerService = CrawlerManager.GetMobileCrawler(EnumMobileCompany.ChinaUnicom, "unicom");

            for (int i = 1; i < count; i++)
            {
                if (sheet.Cells[i, 9].Value.ToString() != "中国联通手机账单解析异常")
                    continue;
                MobileReq fundReq1 = new MobileReq()
                {
                    Name = sheet.Cells[i, 2].Value.ToString(),
                    IdentityCard = sheet.Cells[i, 3].Value.ToString(),
                    Mobile = sheet.Cells[i, 4].Value.ToString(),
                    Password = sheet.Cells[i, 5].Value.ToString(),
                    Token = sheet.Cells[i, 1].Value.ToString()
                };
                //BaseRes Res = crawlerService.MobileAnalysis(fundReq1);
                //sheet.Cells[i, 8].PutValue(Res.StatusCode);
                //sheet.Cells[i, 9].PutValue(Res.StatusDescription);
                //workbook.Save(filepath);
                //Console.WriteLine("身份证号：" + fundReq1.IdentityCard + "    " + Res.StatusDescription);


                //if (sheet.Cells[i, 8].Value != null)
                //    continue;

                //MobileReq fundReq = new MobileReq()
                //{
                //    Name = sheet.Cells[i, 2].Value.ToString(),
                //    IdentityCard = sheet.Cells[i, 3].Value.ToString(),
                //    Mobile = sheet.Cells[i, 4].Value.ToString(),
                //    Password = sheet.Cells[i, 5].Value.ToString(),
                //    Token = sheet.Cells[i, 1].Value.ToString()
                //};
                //BaseRes Res = crawlerService.MobileLogin(fundReq);
                //if (Res.StatusCode == ServiceConsts.StatusCode_success)
                //{
                //    Res = crawlerService.MobileCrawler(fundReq);
                //}
                //sheet.Cells[i, 8].PutValue(Res.StatusCode);
                //sheet.Cells[i, 9].PutValue(Res.StatusDescription);
                //workbook.Save(filepath);
                //Console.WriteLine("身份证号：" + fundReq.IdentityCard + "    " + Res.StatusDescription);
            }

        }

        #endregion

        #endregion
    }


}
