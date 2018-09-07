using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vcredit.ExternalCredit.CrawlerLayer.Assure;

namespace Vcredit.ExternalCredit.AssureReportedProgram
{
    class Program
    {
        /// <summary>
        /// 担保剩余本金维护
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("开始维护（代偿）====================================");
            new AssureMaintain().StartMaintainWithBalanceTransfer();
            Console.WriteLine("开始维护（代偿）====================================");
            Thread.Sleep(2000);
        }
    }
}
