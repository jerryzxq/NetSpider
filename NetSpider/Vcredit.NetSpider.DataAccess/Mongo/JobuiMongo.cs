using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.Mongo.JoBuiSalary;

namespace Vcredit.NetSpider.DataAccess.Mongo
{
    public class JobuiMongo
    {
        const string COLLECTIONNAME = "jobui";
        BaseMongo baseMongo = null;
        public JobuiMongo()
        {
            baseMongo = new BaseMongo("netspidercfg");
        }

        public void Save(JoBui jobui)
        {
            try
            {
                baseMongo.Insert<JoBui>(jobui, COLLECTIONNAME);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public void Update(JoBui jobui)
        {
            try
            {
                var query = new QueryDocument { { "_id", jobui.id } };
                baseMongo.Remove<JoBui>(query, COLLECTIONNAME);
                Save(jobui);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 查询全部
        /// </summary>
        /// <returns></returns>
        public List<JoBui> LoadAll()
        {
            List<JoBui> list = null;
            try
            {
                list = baseMongo.FindAll<JoBui>(COLLECTIONNAME);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return list;
        }

        public JoBui Load(string city)
        {
            JoBui province = null;
            try
            {
                var query = Query.And(Query.EQ("city", city));
                province = baseMongo.FindOne<JoBui>(query, COLLECTIONNAME);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return province;
        }

    }

}
