using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade;

namespace BatchSubmitProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Log4netAdapter.WriteInfo("开始批量提交");
                new BatchRequest().SubmitBatchRequest();
                Log4netAdapter.WriteInfo("结束批量提交");
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("批量提交出现异常", ex);
            }
        }
    }
}
