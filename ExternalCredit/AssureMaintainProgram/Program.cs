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
        /// 担保剩余本金维护
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("开始维护（正常在保余额维护）====================================");

            new AssureMaintain().StartMaintain();

            Console.WriteLine("开始维护（正常在保余额维护）====================================");
        }
    }
}
