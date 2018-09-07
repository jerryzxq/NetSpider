using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Vcredit.NetSpider.Entity.Mongo.ProvidentFund
{
    [BsonIgnoreExtraElements]
    public class City:BaseMongoEntity
    {
        /// <summary>
        /// 城市编号
        /// </summary>
        public string CityCode { get; set; }
        /// <summary>
        /// 城市名称
        /// </summary>
        public string CityName { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        private int _IsUse = 1;
        /// <summary>
        /// 是否有效
        /// </summary>
        public int IsUse
        {
            get { return _IsUse; }
            set { _IsUse = value; }
        }

        private List<FormSetting> _FormSettings = new List<FormSetting>();
        /// <summary>
        /// 对应表单信息
        /// </summary>
        public List<FormSetting> FormSettings
        {
            get { return _FormSettings; }
            set { _FormSettings = value; }
        }
    }
}
