using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vcredit.ExternalCredit.CrawlerLayer.Assure;

namespace AssureReportedAfterProgram
{
    class Program
    {
        /// <summary>
        /// 担保后上报
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("开始担保后上报 AssureReportedAfter====================================");

            new AssureReport().AssureReportedAfter();

            Console.WriteLine("结束担保后上报 AssureReportedAfter====================================");
            Thread.Sleep(2000);
        }
    }
}
