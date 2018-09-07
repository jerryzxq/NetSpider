using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade;
using Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;
namespace UpLoadFileProgram
{


    class Program
    {
        static void Main(string[] args)
        {
            try
            {
             
                Log4netAdapter.WriteInfo("开始上传");
                //读取文件和上传文件
                UpLoadFile file = new UpLoadFile();
                file.UpLoad();
                Log4netAdapter.WriteInfo("结束上传");

            }
            catch (Exception  ex)
            {
                Log4netAdapter.WriteError("上传出现异常", ex);
            }
        }
    }
}
