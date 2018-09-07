using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Vcredit.NetSpider.Entity.Mongo.Faceverify
{
    [BsonIgnoreExtraElements]
    public class FaceCompare : BaseMongoEntity
    {
        public string Name { get; set; }
        public string IdentityCard { get; set; }
        public decimal BaiduSimilarity { get; set; }
        public decimal MinshiSimilarity { get; set; }
    }
}
