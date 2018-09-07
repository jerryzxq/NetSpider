using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Mongo.Log
{
    [BsonIgnoreExtraElements]
    public class SetingLog : BaseMongoEntity
    {
        /// <summary>
        /// 采集网站
        /// </summary>
        public string Website { get; set; }

        /// <summary>
        /// 网站名称
        /// </summary>
        public string WebsiteName { get; set; }

        /// <summary>
        /// ip
        /// </summary>
        public string IP { get; set; }

        private int _IsUse = 1;
        /// <summary>
        /// 是否有效
        /// </summary>
        public int IsUse
        {
            get { return _IsUse; }
            set { _IsUse = value; }
        }

        /// <summary>
        /// 描述
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
