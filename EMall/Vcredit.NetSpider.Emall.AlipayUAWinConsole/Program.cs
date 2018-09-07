using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.NetSpider.Emall.AlipayUAWinConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            AliWinServiceUaTask task = new AliWinServiceUaTask();
            task.GetLoginUa();
            Console.ReadKey();
        }
    }
}
