using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Crawler;
using Vcredit.NetSpider.Crawler.Edu;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.Chsi;
using Vcredit.NetSpider.Entity.Service.Mobile;
using Vcredit.NetSpider.Processor;
using Vcredit.NetSpider.Service;

namespace TestConsoleApp
{
    public class TEST
    {
        [TestFixtureSetUp]
        //这个属性用来标记为整个test fixture初始化资源方法一次的方法.       
        public void stepInstall()
        {

        }
        [SetUp]
        public void Init()
        {
            ISummary summarySer = NetSpiderFactoryManager.GetSummaryService();
            //summarySer.UpdateRealNameAuth("320683199111061551", "18817302037");

            var q1 = summarySer.GetByIdentityNoAndMobile("320124198905190214", "18817302037");
            //TestChsi_Info();
            //string str = "[{\"Name\":\"张志博\",\"IdentityCard\":\"410923198207131718\",\"Sex\":\"男\",\"BirthDate\":\"1982-07-13T00:00:00\",\"Race\":\"\",\"Phone\":null,\"ExamineeNo\":\"02410813100946\",\"StudentNo\":\"\",\"University\":\"河南大学\",\"College\":\"\",\"Department\":\"\",\"MajorName\":\"通信工程(网络技术)\",\"Class\":\"\",\"Degree\":\"本科\",\"Schoolinglength\":\"\",\"EducationType\":\"普通\",\"LearningMode\":\"普通全日制\",\"EnrollmentDate\":\"2002-09-01T00:00:00\",\"SchoolState\":\"已离校(毕业)\",\"LeavingDate\":\"2006-07-01T00:00:00\",\"CertificateNo\":\"104751200605003182\",\"GraduateState\":\"毕业\",\"UniversityLocation\":\"河南省\",\"GraduatePhoto\":\"/9j/4AAQSkZJRgABAQEBLAEsAAD/2wBDAAIBAQEBAQIBAQECAgICAgQDAgICAgUEBAMEBgUGBgYFBgYGBwkIBgcJBwYGCAsICQoKCgoKBggLDAsKDAkKCgr/2wBDAQICAgICAgUDAwUKBwYHCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgr//gN2AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA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\",\"EnrollPhoto\":null}]";
            //var q = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Chsi_InfoEntity>>(str);
        }
        #region 学信网
        private void TestChsi_Info()
        {
            IChsi_Info service = NetSpiderFactoryManager.GetChsi_InfoService();
            IChsiCrawler crawler = CrawlerManager.GetChsiCrawler();
            var res = crawler.Query_Init();
            LoginReq req = new LoginReq();
            req.Username = "13524909205";
            req.Password = "082713";
            req.Token = res.Token;
            var query = crawler.Query_GetInfo(req);
            var entitys = JsonConvert.DeserializeObject<List<Chsi_InfoEntity>>(query.Result);
            service.SaveAll(entitys);
        }
        private void TestChsi_Register()
        {
            IChsi_Info service = NetSpiderFactoryManager.GetChsi_InfoService();
            IChsiCrawler crawler = CrawlerManager.GetChsiCrawler();
            ChsiRegisterReq regReq = new ChsiRegisterReq();
            regReq.Mobile = "13524909203";
            VerCodeRes codeRes = crawler.Register_Init(regReq);
            regReq.Name = "test";
            regReq.Smscode = "082713";
            regReq.Vercode = "082713";
            regReq.Password = "082713";
            regReq.Password1 = "082713";
            regReq.Credentialtype = "QIT";
            regReq.Identitycard = "082713";
            regReq.Email = "082713@126.com";
            regReq.Pwdreq1 = "1";
            regReq.Pwdreq2 = "1";
            regReq.Pwdreq3 = "1";
            regReq.Pwdanswer1 = "1";
            regReq.Pwdanswer2 = "1";
            regReq.Pwdanswer3 = "1";
            regReq.Token = codeRes.Token;

            BaseRes res = crawler.Register_Step1(regReq);
            res = crawler.Register_Step2(regReq);

        }
        private void TestChsi_ForgetPwd()
        {
            IChsi_Info service = NetSpiderFactoryManager.GetChsi_InfoService();
            IChsiCrawler crawler = CrawlerManager.GetChsiCrawler();
            ChsiForgetReq regReq = new ChsiForgetReq();
            VerCodeRes codeRes = crawler.ForgetPwd_Step1();
            regReq.Username = "13524909205";
            codeRes = crawler.ForgetPwd_Step2(regReq);
            regReq.Name = "test";
            regReq.Smscode = "082713";
            regReq.Vercode = "082713";
            regReq.Password = "082713";
            regReq.Password1 = "082713";
            regReq.Credentialtype = "QIT";
            regReq.Identitycard = "082713";
            regReq.Email = "082713@126.com";
            regReq.Pwdreq1 = "1";
            regReq.Pwdreq2 = "1";
            regReq.Pwdreq3 = "1";
            regReq.Pwdanswer1 = "1";
            regReq.Pwdanswer2 = "1";
            regReq.Pwdanswer3 = "1";
            regReq.Token = codeRes.Token;

            BaseRes res = crawler.Register_Step1(regReq);
            res = crawler.Register_Step2(regReq);

        }

        #endregion
        [Test]
        public void Test()
        {
        }
    }
}
