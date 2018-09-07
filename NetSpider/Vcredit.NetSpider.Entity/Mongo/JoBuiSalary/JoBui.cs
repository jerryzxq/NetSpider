using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Mongo.JoBuiSalary
{
    [BsonIgnoreExtraElements]
    public class JoBui : BaseMongoEntity
    {
        public string citySpell { get; set; }
        public string city { get; set; }
        public string sampleSize { get; set; }
        public decimal salary { get; set; }
        public string dateTime { set; get; }
    }
}
