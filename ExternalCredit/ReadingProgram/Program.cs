using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;
namespace ReadingProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {       
                //3.读取本地的数据入库
                Log4netAdapter.WriteInfo("开始读取");
                OperatorLog.BatchNo = DateTime.Now.ToString("yyyyMMddhhmmss");
                AbastractFileBase ab = FileFactory.CreateFile("excel");
                ab.SaveData();

                ab = FileFactory.CreateFile("txt");
                Log4netAdapter.WriteInfo("结束读取");
                ab.SaveData();      
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("读取出现异常", ex);
            }

        }
    }
}
