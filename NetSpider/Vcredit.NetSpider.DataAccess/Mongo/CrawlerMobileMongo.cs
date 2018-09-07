using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver.Builders;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.Common.Ext;
using Vcredit.Common.Constants;
using System.Text.RegularExpressions;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using MongoDB.Driver;
using Vcredit.NetSpider.Entity;

namespace Vcredit.NetSpider.DataAccess.Mongo
{
    public class CrawlerMobileMongo
    {
        BaseMongo baseMongo = null;
        string dbName = "netcrawer_";
        public CrawlerMobileMongo(DateTime date)
        {
            string serverName = dbName + date.ToString(Consts.DateFormatString7);
            ////测试
            //if (date < DateTime.Parse("2016-03-30 18:00:00"))
            //    serverName = "netcrawer";
            //生产
            if (date < DateTime.Parse("2016-04-11 14:25:00"))
                serverName = "netcrawer";
            baseMongo = new BaseMongo(serverName, "CrawlerMongoDB");
        }

        public void SaveCrawler(CrawlerData crawler)
        {
            //保存抓取信息
            try
            {
                baseMongo.Insert<CrawlerData>(crawler, "mobile_crawler");
            }
            catch (Exception e)
            {
                if (e.Message.Contains("is larger than MaxDocumentSize"))
                {
                    crawler.DtlList = crawler.DtlList.Where(x => !x.CrawlerTitle.Contains(EnumMobileDeatilType.Net.ToString())).ToList();
                    baseMongo.Insert<CrawlerData>(crawler, "mobile_crawler");
                }
                else
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public void DropCrawler()
        {
            string serverName = string.Empty;
            DateTime date = DateTime.Now.AddMonths(-3);
            for (int i = 1; i <= 3; i++)
            {
                serverName = dbName + date.AddMonths(i).ToString(Consts.DateFormatString7);
            }
            if (!serverName.IsEmpty())
                baseMongo.Drop(serverName);

        }

        public CrawlerData GetCrawler(string Token, string mobile, DateTime date)
        {
            CrawlerData crawler = null;
            try
            {
                var query = Query.EQ("1", "1");
                if (!Token.IsEmpty())
                    query = Query.And(Query.EQ("Token", Token));
                else
                    query = Query.And(Query.EQ("CrawlerDate", date), Query.EQ("Mobile", mobile));
                crawler = baseMongo.FindOne<CrawlerData>(query, "mobile_crawler");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return crawler;
        }
    }
}
