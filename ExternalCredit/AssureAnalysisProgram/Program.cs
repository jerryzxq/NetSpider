using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExternalCredit.CrawlerLayer.Assure;

namespace AssureAnalysisProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            var crawler = new CreditQueryImpl();
            // 获取报告
            crawler.AnanysisDownloadCreditV2();

            //CreditQueryImpl.AnanysisCreditByHtml();
        }
    }
}
