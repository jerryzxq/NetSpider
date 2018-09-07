using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.NetSpider.Emall.AlipayBillCrawlerWinConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            AliPayBillCrawlerQueueTask task = new AliPayBillCrawlerQueueTask();
            task.GetLoginUa();
            Console.ReadKey();
           
        }
    }
}
