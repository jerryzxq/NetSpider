using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.Mongo.Log;

namespace Vcredit.NetSpider.DataAccess.Mongo
{
    public class MonitorLogMongo
    {
        BaseMongo baseMongo = null;
        public MonitorLogMongo()
        {
            baseMongo = new BaseMongo("netspiderlog", "CrawlerMongoDB");
        }
        public void SaveMonitorLog(MonitorLog entity)
        {
            baseMongo.Insert<MonitorLog>(entity, "mobile_monitorLog");
        }

        public List<MonitorLog> GetMonitorLog(Dictionary<string, string> dic)
        {
            List<MonitorLog> logs = null;
            try
            {
                List<IMongoQuery> queries = new List<IMongoQuery>();
                foreach (var item in dic)
                {
                    if (item.Key.Contains("Start"))
                        queries.Add(Query.GTE("CreateTime", item.Value));
                    else if (item.Key.Contains("End"))
                        queries.Add(Query.LT("CreateTime", item.Value));
                    else
                        queries.Add(Query.EQ(item.Key, item.Value));
                }
                var query = queries.Count > 0 ? Query.And(queries) : Query.EQ("1", "1");
                logs = baseMongo.Find<MonitorLog>(query, "mobile_monitorLog");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return logs;
        }
    }
}
