using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Constants;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.Entity.Mongo.Log;

namespace Vcredit.NetSpider.Monitor.job
{
    /// <summary>
    /// 网站升级
    /// </summary>
    class MobileWebUpdateJob : IJob
    {
        ApplyLogMongo logMongo = new ApplyLogMongo();
        public void Execute(IJobExecutionContext context)
        {
            IList<ApplyLog> errapplyList = new List<ApplyLog>();
            string staData = DateTime.Now.AddHours(-1).ToString(Consts.DateFormatString9);
            string endData = DateTime.Now.ToString(Consts.DateFormatString9);
            errapplyList = logMongo.GetWebUpdateLog("mobile", staData, endData);

            if (errapplyList.Count > 0)
            {
                MonitorLogMongo monitorlogMongo = new MonitorLogMongo();
                var groups = from p in errapplyList
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
                        Type = "WebUpdate",
                        Description = Common.GetWebsiteName(item.Key) + "网站升级，异常条数：" + item.NumProducts 
                    });
                    CommonFun.SendMail("网站升级提醒", Common.GetWebsiteName(item.Key) + "网站升级");
                    Console.WriteLine(Common.GetWebsiteName(item.Key) + "网站升级，异常条数：" + item.NumProducts + "，时间：" + DateTime.Now);
                }
            }
            else
                Console.WriteLine("无网站升级，时间：" + DateTime.Now);
        }
    }
}
