using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExternalCredit.CrawlerLayer.ShanghaiLoan;

namespace QueryCredit
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Log4netAdapter.WriteInfo("开始获取征信报告");
                ShangHaiCredit credit = new ShangHaiCredit();
                credit.PostReqMeg();
                Log4netAdapter.WriteInfo("获取征信报告结束");
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("上海小贷调用查询接口出现问题", ex); 
            }
        }
    }
}
