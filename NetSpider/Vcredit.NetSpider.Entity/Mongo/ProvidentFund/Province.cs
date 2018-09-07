using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Vcredit.NetSpider.Entity.Mongo.ProvidentFund
{
    [BsonIgnoreExtraElements]
    public class Province : BaseMongoEntity
    {
        /// <summary>
        /// 省名
        /// </summary>
        private string _ProvinceName;
        /// <summary>
        /// 省编号
        /// </summary>
        private string _ProvinceCode;

        /// <summary>
        /// 说明
        /// </summary>
        private string _Description;

        /// <summary>
        /// 城市
        /// </summary>
        private List<CityLevel> _CityLevel;
      
        /// <summary>
        /// 省名
        /// </summary>
        public string ProvinceName
        {
            get { return _ProvinceName; }
            set { _ProvinceName = value; }
        }
        /// <summary>
        /// 省编号
        /// </summary>
        public string ProvinceCode
        {
            get { return _ProvinceCode; }
            set { _ProvinceCode = value; }
        }
        /// <summary>
        /// 说明
        /// </summary>
        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        /// <summary>
        /// 城市
        /// </summary>
        public List<CityLevel> CityLevel
        {
            get { return _CityLevel; }
            set { _CityLevel = value; }
        }
    }
}
