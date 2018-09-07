using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CrawlerLayer.Assure;

namespace AssureAnalysisByConfigProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            Log4netAdapter.WriteInfo("开始单独解析=======================");

            CreditQueryImpl.AnanysisCreditByHtml();

            Log4netAdapter.WriteInfo("结束单独解析=======================");
        }
    }
}
