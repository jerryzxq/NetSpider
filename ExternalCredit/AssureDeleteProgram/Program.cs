using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CrawlerLayer.Assure;

namespace AssureDeleteProgram
{
    class Program
    {
        /// <summary>
        /// 担保上报
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Log4netAdapter.WriteInfo("开始删除====================================");

            new AssureDelete().StartDelete();

            Log4netAdapter.WriteInfo("结束删除====================================");
        }
    }
}
