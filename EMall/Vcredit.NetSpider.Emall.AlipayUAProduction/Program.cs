using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.NetSpider.Emall.Dto;
using Vcredit.NetSpider.Emall.Framework;

namespace Vcredit.NetSpider.Emall.AlipayUAProduction
{
    class Program
    {
        static void Main(string[] args)
        {
            ProductionUA result = null;
            while (true)
            {
                result = ProductionUATask.SetAliPayUA();
                if (result != null)
                    Console.WriteLine(string.Format("时间：{0},UA：{1}", DateTime.Now.ToString(), result.Ua));
            }
        }
    }
}
