using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.Mongo.ProvidentFund;

namespace Vcredit.NetSpider.DataAccess.Mongo
{
    public class ProvinceMongo
    {
        const string COLLECTIONNAME = "housingfund_province";
        BaseMongo baseMongo = null;
        public ProvinceMongo()
        {
            baseMongo = new BaseMongo("netspider");
        }

        public void Save(Province province)
        {
            try
            {
                baseMongo.Insert<Province>(province, COLLECTIONNAME);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public void Update(Province province)
        {
            try
            {
                var query = new QueryDocument { { "_id", province.id } };
                baseMongo.Remove<Province>(query, COLLECTIONNAME);
                Save(province);
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
        public List<Province> LoadAll()
        {
            List<Province> list = null;
            try
            {
                list = baseMongo.FindAll<Province>(COLLECTIONNAME);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return list;
        }

        public Province Load(string provincecode)
        {
            Province province = null;
            try
            {
                var query = Query.And(Query.EQ("ProvinceCode", provincecode));
                province = baseMongo.FindOne<Province>(query, COLLECTIONNAME);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return province;
        }

    }
}
