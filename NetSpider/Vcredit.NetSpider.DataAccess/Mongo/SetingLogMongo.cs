using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.Mongo.Log;

namespace Vcredit.NetSpider.DataAccess.Mongo
{
    public class SetingLogMongo
    {
        BaseMongo baseMongo = null;
        public SetingLogMongo()
        {
            baseMongo = new BaseMongo("netspiderlog", "CrawlerMongoDB");
        }
        public void SaveSetingLog(SetingLog entity)
        {
            baseMongo.Insert<SetingLog>(entity, "mobile_setingLog");
        }

        public List<SetingLog> GetLogSeting(Dictionary<string, string> dic)
        {
            List<SetingLog> setings = null;
            try
            {
                List<IMongoQuery> queries = new List<IMongoQuery>();
                foreach (var item in dic)
                {
                    if (item.Key == "CreateTime")
                    {
                        queries.Add(Query.GT(item.Key, DateTime.Parse(item.Value).ToString()));
                        queries.Add(Query.LT(item.Key, DateTime.Parse(item.Value).AddDays(1).ToString()));
                    }
                    else
                    {
                        queries.Add(Query.EQ(item.Key, item.Value));
                    }
                }
                var query = queries.Count > 0 ? Query.And(queries) : Query.EQ("1", "1");
                setings = baseMongo.Find<SetingLog>(query, "mobile_setingLog");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return setings;
        }
    }
}
