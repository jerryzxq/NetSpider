using System.Text;
using System.Threading.Tasks;
using Quartz;
using System.IO;
using System.Linq;
using Vcredit.NetSpider.Processor;
using Vcredit.NetSpider.Service;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Entity.Service.Mobile;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using System;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.Common.Constants;
using System.Collections.Generic;
using Vcredit.NetSpider.Entity.Mongo.Log;
using Vcredit.Common.Utility;

namespace Vcredit.NetSpider.Monitor.job
{
    /// <summary>
    /// 网站解析成功比
    /// </summary>
    class MobileAnalysisSucceedRatioJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
            MonitorLogMongo monitorlogMongo = new MonitorLogMongo();
            IList<Spd_applyEntity> applyList = new List<Spd_applyEntity>();
            DateTime date = DateTime.Now;
            applyList = applyService.GetApplyListByTimeQuantums(date.AddHours(-2), date);

            if (applyList.Count > 0)
            {
                //解析成功数
                var AnalySuccGroups = from p in applyList
                                      where p.Crawl_status == ServiceConsts.CrawlerStatusCode_AnalysisSuccess
                                      group p by p.Website into g
                                      orderby g.Key ascending
                                      select new
                                      {
                                          g.Key,
                                          NumProducts = g.Count()
                                      };

                //抓取成功数
                var CrawlerSuccGroups = from p in applyList
                                        where new List<int>() { ServiceConsts.CrawlerStatusCode_AnalysisSuccess, ServiceConsts.CrawlerStatusCode_AnalysisFail, ServiceConsts.CrawlerStatusCode_AnalysisFail_NoCalls }.Contains(p.Crawl_status)
                                        group p by p.Website into g
                                        orderby g.Key ascending
                                        select new
                                        {
                                            g.Key,
                                            NumProducts = g.Count()
                                        };

                //抓取数
                var CrawlerGroups = from p in applyList
                                    where new List<int>() { ServiceConsts.CrawlerStatusCode_AnalysisSuccess, ServiceConsts.CrawlerStatusCode_AnalysisFail, ServiceConsts.CrawlerStatusCode_AnalysisFail_NoCalls, ServiceConsts.CrawlerStatusCode_CrawlerFail }.Contains(p.Crawl_status)
                                    group p by p.Website into g
                                    orderby g.Key ascending
                                    select new
                                    {
                                        g.Key,
                                        NumProducts = g.Count()
                                    };
                //申请总数
                var groups = from p in applyList
                             where !new List<int>() { ServiceConsts.CrawlerStatusCode_PasswordError, ServiceConsts.CrawlerStatusCode_PasswordEasy, ServiceConsts.CrawlerStatusCode_AccountLock, ServiceConsts.CrawlerStatusCode_SystemUpdate, ServiceConsts.CrawlerStatusCode_SystemBusy, ServiceConsts.CrawlerStatusCode_VercodeError, ServiceConsts.CrawlerStatusCode_SmsCodeInvalidation, ServiceConsts.CrawlerStatusCode_AccountProtect }.Contains(p.Crawl_status)
                             group p by p.Website into g
                             orderby g.Key ascending
                             select new
                             {
                                 g.Key,
                                 NumProducts = g.Count()
                             };

                //解析成功率
                foreach (var itemcrasucc in CrawlerSuccGroups)
                {
                    var itemanaly = AnalySuccGroups.Where(x => x.Key == itemcrasucc.Key).FirstOrDefault();
                    int analySuccRow = itemanaly == null ? 0 : itemanaly.NumProducts;
                    double analySuccRatio = Double.Parse(analySuccRow.ToString()) / Double.Parse(itemcrasucc.NumProducts.ToString());
                    monitorlogMongo.SaveMonitorLog(new MonitorLog()
                    {
                        Website = itemcrasucc.Key,
                        WebsiteName = Common.GetWebsiteName(itemcrasucc.Key),
                        TotalRow = itemcrasucc.NumProducts,
                        RelativeRow = analySuccRow,
                        Ratio = analySuccRatio,
                        Type = "AnalySuccRatio",
                        Description = Common.GetWebsiteName(itemcrasucc.Key) + "解析成功率：" + analySuccRatio
                    });
                    Console.WriteLine(Common.GetWebsiteName(itemcrasucc.Key) + "，抓取成功条数：" + itemcrasucc.NumProducts + "，解析成功条数：" + analySuccRow + "，解析成功率：" + analySuccRatio + "，时间：" + DateTime.Now);
                    if (analySuccRatio < 0.5 && itemcrasucc.NumProducts > 3)
                    {
                        CommonFun.SendMail("解析率异常提醒", Common.GetWebsiteName(itemcrasucc.Key) + "解析率：" + analySuccRatio);
                    }

                }
                //抓取成功率
                foreach (var itemcra in CrawlerGroups)
                {
                    var itemcraSucc = CrawlerSuccGroups.Where(x => x.Key == itemcra.Key).FirstOrDefault();
                    int craSuccRow = itemcraSucc == null ? 0 : itemcraSucc.NumProducts;
                    double crawlerSuccRatio = Double.Parse(craSuccRow.ToString()) / Double.Parse(itemcra.NumProducts.ToString());
                    monitorlogMongo.SaveMonitorLog(new MonitorLog()
                    {
                        Website = itemcra.Key,
                        WebsiteName = Common.GetWebsiteName(itemcra.Key),
                        TotalRow = itemcra.NumProducts,
                        RelativeRow = craSuccRow,
                        Ratio = crawlerSuccRatio,
                        Type = "CrawlerSuccRatio",
                        Description = Common.GetWebsiteName(itemcra.Key) + "抓取成功率：" + crawlerSuccRatio
                    });
                    Console.WriteLine(Common.GetWebsiteName(itemcra.Key) + "，抓取条数：" + itemcra.NumProducts + "，抓取成功条数：" + craSuccRow + "，抓取成功率：" + crawlerSuccRatio + "，时间：" + DateTime.Now);
                }
                //抓取率
                foreach (var item in groups)
                {
                    var itemcra = CrawlerGroups.Where(x => x.Key == item.Key).FirstOrDefault();
                    int craRow = itemcra == null ? 0 : itemcra.NumProducts;
                    double crawlerRatio = Double.Parse(craRow.ToString()) / Double.Parse(item.NumProducts.ToString());
                    if (crawlerRatio == 0 && item.NumProducts > 3)
                    {
                        monitorlogMongo.SaveMonitorLog(new MonitorLog()
                        {
                            Website = item.Key,
                            WebsiteName = Common.GetWebsiteName(item.Key),
                            TotalRow = item.NumProducts,
                            RelativeRow = craRow,
                            Ratio = crawlerRatio,
                            Type = "CrawlerRatio",
                            Description = Common.GetWebsiteName(item.Key) + "抓取率：" + crawlerRatio
                        });
                        CommonFun.SendMail("抓取异常提醒", Common.GetWebsiteName(item.Key) + "抓取异常");
                        Console.WriteLine(Common.GetWebsiteName(item.Key) + "抓取异常，申请条数：" + item.NumProducts + "，抓取条数：" + craRow + "，抓取率：" + crawlerRatio + "，时间：" + DateTime.Now);
                    }
                }
            }
        }
    }
}
