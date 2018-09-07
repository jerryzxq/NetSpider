using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Constants;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Mongo.Log;

namespace Vcredit.NetSpider.Monitor.job
{
    /// <summary>
    /// 程序异常
    /// </summary>
    class MobileProExcJob : IJob
    {
        ApplyLogMongo logMongo = new ApplyLogMongo();
        public void Execute(IJobExecutionContext context)
        {
            IList<ApplyLog> applyList = new List<ApplyLog>();
            string staData = DateTime.Now.AddHours(-1).ToString(Consts.DateFormatString9);
            string endData = DateTime.Now.ToString(Consts.DateFormatString9);
            applyList = logMongo.GetProExcLog("mobile", staData, endData);

            if (applyList.Count > 0)
            {
                MonitorLogMongo monitorlogMongo = new MonitorLogMongo();
                var groups = from p in applyList
                             group p by p.Website into g
                             orderby g.Key ascending
                             select new
                             {
                                 g.Key,
                                 NumProducts = g.Count()
                             };
                //出现异常处理方式
                foreach (var item in groups)
                {
                    monitorlogMongo.SaveMonitorLog(new MonitorLog()
                    {
                        Website = item.Key,
                        WebsiteName = Common.GetWebsiteName(item.Key),
                        TotalRow = item.NumProducts,
                        RelativeRow = item.NumProducts,
                        Type = "ProExc",
                        Description = Common.GetWebsiteName(item.Key) + "程序异常，异常条数：" + item.NumProducts
                    });
                    CommonFun.SendMail("程序异常提醒", Common.GetWebsiteName(item.Key) + "程序异常");
                    Console.WriteLine(Common.GetWebsiteName(item.Key) + "程序异常，异常条数：" + item.NumProducts + "，时间：" + DateTime.Now);
                }
            }
            else
                Console.WriteLine("程序无异常，时间：" + DateTime.Now);

        }
    }
}
