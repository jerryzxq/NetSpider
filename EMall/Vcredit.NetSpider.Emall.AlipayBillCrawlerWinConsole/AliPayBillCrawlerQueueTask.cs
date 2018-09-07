using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.Framework.Queue.Redis;
using Vcredit.NetSpider.Cache;
using Vcredit.NetSpider.Emall.Crawler;
using Vcredit.NetSpider.Emall.Dto;
using Vcredit.NetSpider.Emall.Entity;
using Vcredit.NetSpider.Emall.Framework;

namespace Vcredit.NetSpider.Emall.AlipayBillCrawlerWinConsole
{
    public class AliPayBillCrawlerQueueTask
    {
        public void GetLoginUa()
        {

            //RedisQueue.Receive<AlipayBaicEntity>((x) =>
            //{
            //    BillCrawler(x);

            //}, "AliPayBillCrawlerQueue");

            Crawler:
            try
            {
                    System.Threading.Thread.Sleep(10);
                    var result = Vcredit.NetSpider.Cache.WorkQueue.DequeueByList<AliPayBillBaic>("Queue:AliPayBillCrawlerQueue").FirstOrDefault();
                    if (result != null)
                    {
                        BillCrawler(new AlipayBaicEntity()
                        {
                            ID = result.ID,
                            Token = result.Token,
                            UserID = result.UserID

                        });
                    }
                }
                catch (Exception ex) {
                    Log4netAdapter.WriteError("AliPayBillCrawlerQueueTask控制台：",ex);
                }
            goto Crawler;
        }


        void BillCrawler(AlipayBaicEntity baic)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Reset();
                    sw.Start();
                    Console.WriteLine(string.Format("时间：{0},Token：{1}开始账单抓取", DateTime.Now.ToString(), baic.Token));
                    var dyMode = (Dictionary<string, string>)RedisHelper.GetCache(baic.Token + "_dy");
                    var cookies = (CookieCollection)RedisHelper.GetCache(baic.Token);
                    if (cookies != null)
                    {
                        if (RedisHelper.GetCache(baic.Token + "_cookie") != null)
                        {
                            var cookieString = RedisHelper.GetCache<string>(baic.Token + "_cookie");
                            new BillCrawler(cookieString, baic.Token).DoBill(dyMode, ref cookies, baic);
                        }
                        else
                        {
                            new BillCrawler(baic.Token).DoBill(dyMode, ref cookies, baic);
                        }
                    }
                    Console.WriteLine(string.Format("时间：{0},Token：{1}抓取账单完成", DateTime.Now.ToString(), baic.Token + ",计算时间：" + sw.Elapsed.ToString()));
                    sw.Stop();
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError("AliPayBillCrawlerQueueTask：" + baic.Token, ex);
                    Console.WriteLine(string.Format("时间：{0},Token：{1}账单抓取异常==》{2}", DateTime.Now.ToString(), baic.Token, ex.Message));

                }
            }).ContinueWith(e => e.Dispose());
        }
    }
}
