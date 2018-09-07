using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Vcredit.NetSpider.Entity.Mongo.SocialSecurity;
namespace Vcredit.NetSpider.DataAccess.Mongo
{
    public class SheBaoProvinceMongo
    {
        const string COLLECTIONNAME = "socialsecurity_province";
        BaseMongo baseMongo = null;
        public SheBaoProvinceMongo()
        {
            baseMongo = new BaseMongo("netspider");
        }

        public void Save(SheBaoProvince province)
        {
            try
            {
                baseMongo.Insert<SheBaoProvince>(province, COLLECTIONNAME);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public void Update(SheBaoProvince province)
        {
            try
            {
                var query = new QueryDocument { { "_id", province.id } };
                baseMongo.Remove<SheBaoProvince>(query, COLLECTIONNAME);
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
        public List<SheBaoProvince> LoadAll()
        {
            List<SheBaoProvince> list = null;
            try
            {
                list = baseMongo.FindAll<SheBaoProvince>(COLLECTIONNAME);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return list;
        }

        public SheBaoProvince Load(string provincecode)
        {
            SheBaoProvince province = null;
            try
            {
                var query = Query.And(Query.EQ("ProvinceCode", provincecode));
                province = baseMongo.FindOne<SheBaoProvince>(query, COLLECTIONNAME);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return province;
        }
    }
}
