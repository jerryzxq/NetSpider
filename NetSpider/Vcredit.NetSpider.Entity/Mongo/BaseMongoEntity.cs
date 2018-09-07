using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Vcredit.NetSpider.Entity
{
    public class BaseMongoEntity
    {
        private string _id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        [BsonRepresentation(BsonType.ObjectId)]
        public string id
        {
            get { return _id; }
            set { _id = value; }
        }
    }
}
