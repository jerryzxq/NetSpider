using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Mongo.Mobile
{
    [BsonIgnoreExtraElements]
    public class MobileSeting
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
        /// 网站描述
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
    }
}
