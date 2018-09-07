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
using Vcredit.Common.Ext;

namespace Vcredit.NetSpider.Monitor.job
{
    /// <summary>
    /// 网站不稳定
    /// </summary>
    class MobileWebExcJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            ApplyLogMongo logMongo = new ApplyLogMongo();
            MonitorLogMongo monitorlogMongo = new MonitorLogMongo();
            IList<ApplyLog> applyList = new List<ApplyLog>();
            IList<ApplyLog> errapplyList = new List<ApplyLog>();
            string staData = DateTime.Now.AddHours(-1).ToString(Consts.DateFormatString9);
            string endData = DateTime.Now.ToString(Consts.DateFormatString9);
            errapplyList = logMongo.GetWebExcLog("mobile", staData, endData);
            applyList = logMongo.GetHttpLog("mobile", staData, endData);

            if (applyList.Count > 0)
            {
                //运行商请求总数数
                var groups = from p in applyList
                             where !p.Website.IsEmpty()
                             group p by p.Website into g
                             orderby g.Key ascending
                             select new
                             {
                                 g.Key,
                                 NumProducts = g.Count()
                             };
                //请求异常数
                var errgroups = from p in errapplyList
                                where !p.Website.IsEmpty()
                                group p by p.Website into g
                                orderby g.Key ascending
                                select new
                                {
                                    g.Key,
                                    NumProducts = g.Count()
                                };
                //解析成功率
                foreach (var item in groups)
                {
                    var erritem = errgroups.Where(x => x.Key == item.Key).FirstOrDefault();
                    int errRow = erritem == null ? 0 : erritem.NumProducts;
                    double errRatio = Double.Parse(errRow.ToString()) / Double.Parse(item.NumProducts.ToString());
                    if (errRatio > 0.2)
                    {
                        CommonFun.SendMail("网站不稳定提醒", Common.GetWebsiteName(item.Key) + "网站不稳定");
                        Console.WriteLine(Common.GetWebsiteName(item.Key) + "网站不稳定，监控条数：" + item.NumProducts + "，异常条数：" + errRow + "，时间：" + DateTime.Now);
                    }
                    else
                    {
                        Console.WriteLine(Common.GetWebsiteName(item.Key) + "网站未达到不稳定条件，监控条数：" + item.NumProducts + "，异常条数：" + errRow + "，时间：" + DateTime.Now);
                    }
                    monitorlogMongo.SaveMonitorLog(new MonitorLog()
                    {
                        Website = item.Key,
                        WebsiteName = Common.GetWebsiteName(item.Key),
                        TotalRow = item.NumProducts,
                        RelativeRow = errRow,
                        Ratio = errRatio,
                        Type = "WebExcRatio",
                        Description = Common.GetWebsiteName(item.Key) + "网站不稳定率：" + errRatio
                    });
                }
            }
        }
    }
}
