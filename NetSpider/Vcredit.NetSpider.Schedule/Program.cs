using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Crawler;
using Vcredit.NetSpider.Crawler.Mobile;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.Mobile;
using Vcredit.NetSpider.Processor;
using Vcredit.NetSpider.RestService.Services;
using Vcredit.NetSpider.Service;

namespace ReadCrawlerConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ISchedulerFactory sf = new StdSchedulerFactory();
            IScheduler sched = sf.GetScheduler();
            sched.Start();
        }
    }
}
