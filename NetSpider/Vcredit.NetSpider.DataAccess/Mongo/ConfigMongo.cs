using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.Mongo.Mobile;

namespace Vcredit.NetSpider.DataAccess.Mongo
{
    public class ConfigMongo
    {
        BaseMongo baseMongo = null;
        public ConfigMongo()
        {
            baseMongo = new BaseMongo("netspidercfg", "AnalysisMongoDB");
        }

        public void SaveMobileSeting(MobileSeting seting)
        {
            try
            {
                baseMongo.Insert<MobileSeting>(seting, "mobile_seting");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public void UpdateMobileSeting(MobileSeting seting)
        {
            try
            {
                var query = new QueryDocument { { "Website", seting.Website } };
                baseMongo.Remove<MobileSeting>(query, "mobile_seting");
                SaveMobileSeting(seting);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public MobileSeting GetMobileSeting(string website)
        {
            MobileSeting seting = null;
            try
            {
                var query = Query.EQ("Website", website);
                seting = baseMongo.FindOne<MobileSeting>(query, "mobile_seting");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return seting;
        }

        public List<MobileSeting> GetMobileSeting(Dictionary<string, string> dic)
        {
            List<MobileSeting> setings = null;
            try
            {
                List<IMongoQuery> queries = new List<IMongoQuery>();
                foreach (var item in dic)
                {
                    if (item.Key == "IsUse")
                    {
                        queries.Add(Query.EQ(item.Key, Int32.Parse(item.Value)));
                    }
                    else if (item.Key == "WebsiteName")
                    {
                        queries.Add(Query.Matches(item.Key, item.Value));
                    }
                    else
                    {
                        queries.Add(Query.EQ(item.Key, item.Value));
                    }
                }
                var query = queries.Count > 0 ? Query.And(queries) : Query.EQ("1", "1");
                setings = baseMongo.Find<MobileSeting>(query, "mobile_seting");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return setings;
        }
    }
}
