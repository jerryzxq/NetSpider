using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Crawler.Edu;
using Vcredit.NetSpider.Entity.Service;

namespace Vcredit.NetSpider.Processor.Impl
{
    public class ChsiExecutor:IChsiExecutor
    {
        IChsiCrawler crawler = Crawler.CrawlerManager.GetChsiCrawler();
        public VerCodeRes Query_Init()
        {
            return crawler.Query_Init();
        }

        public BaseRes Query_GetInfo(LoginReq login)
        {
            return crawler.Query_GetInfo(login);
        }

        public VerCodeRes Register_Init(Entity.Service.Chsi.ChsiRegisterReq registerReq)
        {
            return crawler.Register_Init(registerReq);
        }

        public BaseRes Register_Step1(Entity.Service.Chsi.ChsiRegisterReq registerReq)
        {
            return crawler.Register_Step1(registerReq);
        }

        public BaseRes Register_Step2(Entity.Service.Chsi.ChsiRegisterReq registerReq)
        {
            return crawler.Register_Step2(registerReq);
        }


        public BaseRes ForgetUsername(Entity.Service.Chsi.ChsiForgetReq forgetReq)
        {
            return crawler.ForgetUsername(forgetReq);
        }

        public VerCodeRes ForgetPwd_Step1()
        {
            return crawler.ForgetPwd_Step1();
        }

        public VerCodeRes ForgetPwd_Step2(Entity.Service.Chsi.ChsiForgetReq forgetReq)
        {
            return crawler.ForgetPwd_Step2(forgetReq);
        }

        public BaseRes ForgetPwd_Step3(Entity.Service.Chsi.ChsiForgetReq forgetReq)
        {
            return crawler.ForgetPwd_Step3(forgetReq);
        }

        public BaseRes ForgetPwd_Step4(Entity.Service.Chsi.ChsiForgetReq forgetReq)
        {
            return crawler.ForgetPwd_Step4(forgetReq);
        }

    }
}
