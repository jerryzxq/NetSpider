using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CrawlerLayer.Assure;

namespace AssureCommitQueryProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            var crawler = new CreditQueryImpl();
            // 提交查询
            crawler.CommitToQueryV2();
        }
    }
}
