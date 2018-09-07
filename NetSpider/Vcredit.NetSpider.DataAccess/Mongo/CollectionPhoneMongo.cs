using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.Mongo.Seting;

namespace Vcredit.NetSpider.DataAccess.Mongo
{
    public class CollectionPhoneMongo
    {
        BaseMongo baseMongo = null;
        public CollectionPhoneMongo()
        {
            baseMongo = new BaseMongo("netspidercfg", "AnalysisMongoDB");
        }

        public void Save(CollectionPhone entity)
        {
            //保存抓取信息
            baseMongo.Insert<CollectionPhone>(entity, "mobile_collectionPhone");
        }

        public void Update(CollectionPhone entity)
        {
            try
            {
                var query = new QueryDocument { { "id", entity.id } };
                baseMongo.Remove<CollectionPhone>(query, "mobile_seting");
                Save(entity);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<CollectionPhone> Get()
        {
            List<CollectionPhone> list = null;
            try
            {
                list = baseMongo.FindAll<CollectionPhone>("mobile_collectionPhone");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return list;
        }
        
        public List<CollectionPhone> Get(Dictionary<string, string> dic)
        {
            List<CollectionPhone> list = null;
            try
            {
                List<IMongoQuery> queries = new List<IMongoQuery>();
                foreach (var item in dic)
                {
                    if (item.Key == "IsOwn")
                    {
                        queries.Add(Query.EQ(item.Key, Int32.Parse(item.Value)));
                    }
                    else
                    {
                        queries.Add(Query.EQ(item.Key, item.Value));
                    }
                }
                var query = queries.Count > 0 ? Query.And(queries) : Query.EQ("1", "1");
                list = baseMongo.Find<CollectionPhone>(query, "mobile_collectionPhone");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return list;
        }

        public List<CollectionPhone> GetByIsOwn(int isOwn)
        {
            List<CollectionPhone> list = null;
            try
            {
                var query = Query.EQ("IsOwn", isOwn);
                list = baseMongo.Find<CollectionPhone>(query, "mobile_collectionPhone");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return list;
        }

    }
}
