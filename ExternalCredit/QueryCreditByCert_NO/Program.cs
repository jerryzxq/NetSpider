using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade;

namespace QueryCreditByCert_NO
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {
                Log4netAdapter.WriteInfo("开始下载");
                new ForeignTradeQuery().GetForeignCredit();
                Log4netAdapter.WriteInfo("结束下载");
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("根据身份证号和批次号下载出现异常", ex);
            }
        }
    }
}
