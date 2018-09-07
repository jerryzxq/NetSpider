using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.Entity.Mongo.Mobile
{
    [BsonIgnoreExtraElements]
    public class CrawlerData : BaseMongoEntity
    {
        /// <summary>
        /// 会话令牌
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 身份证号
        /// </summary>
        public string IdentityCard { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 用户类型
        /// </summary>
        public string UserType { get; set; }

        private string _CrawlerDate = DateTime.Now.ToString(Consts.DateFormatString11);
        /// <summary>
        /// 抓取时间
        /// </summary>
        public string CrawlerDate
        {
            get { return _CrawlerDate; }
            set { _CrawlerDate = value; }
        }

        /// <summary>
        /// 数据名称
        /// </summary>
        public List<CrawlerDtlData> DtlList = new List<CrawlerDtlData>();


    }
}
