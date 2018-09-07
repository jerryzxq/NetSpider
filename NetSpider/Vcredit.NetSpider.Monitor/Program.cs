using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.NetSpider.Monitor
{
    class Program
    {
        static void Main(string[] args)
        {
            //Vcredit.NetSpider.Monitor.job.MobileAnalysisSucceedRatioJob a = new job.MobileAnalysisSucceedRatioJob();
            //a.Execute();

            ISchedulerFactory sf = new StdSchedulerFactory();
            IScheduler sched = sf.GetScheduler();
            sched.Start();
        }
    }
}
