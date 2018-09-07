using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Mongo.Seting
{
    [BsonIgnoreExtraElements]
    public class CollectionPhone : BaseMongoEntity
    {
        /// <summary>
        /// 公司
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 是否本公司
        /// </summary>
        public int IsOwn { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Description { get; set; }

        private string _CreateTime = DateTime.Now.ToString();
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }
    }
}
