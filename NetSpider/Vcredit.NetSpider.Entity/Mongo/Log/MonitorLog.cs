using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Mongo.Log
{
    [BsonIgnoreExtraElements]
    public  class MonitorLog: BaseMongoEntity
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
        /// 总条数
        /// </summary>
        public int TotalRow { get; set; }

        /// <summary>
        /// 相对条数
        /// </summary>
        public int RelativeRow { get; set; }

        /// <summary>
        /// 比率
        /// </summary>
        public double Ratio { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }

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
