using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vcredit.ExternalCredit.CrawlerLayer.Assure;

namespace AssureReportedProgram
{
    class Program
    {
        /// <summary>
        /// 担保上报
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("开始担保上报====================================");

            new AssureReport().StartReported();

            Console.WriteLine("结束担保上报====================================");
        }
    }
}
