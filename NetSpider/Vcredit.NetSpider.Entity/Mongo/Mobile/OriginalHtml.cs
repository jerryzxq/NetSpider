using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Vcredit.NetSpider.Entity.Mongo.Mobile
{
    [BsonIgnoreExtraElements]
    public class OriginalHtml : BaseMongoEntity
    {
        /// <summary>
        /// 类型
        /// </summary>
        public string LogType { get; set; }

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

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }

        private List<OriginalHtmlDtl> _htmlDtlList = new List<OriginalHtmlDtl>();
        /// <summary>
        /// 步骤集合
        /// </summary>
        public List<OriginalHtmlDtl> HtmlDtlList
        {
            get { return _htmlDtlList; }
            set { _htmlDtlList = value; }
        }
    }
}
