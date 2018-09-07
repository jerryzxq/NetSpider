using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
namespace  Vcredit.NetSpider.Emall.Entity 
{
 
    public class BaseEntity
    {
        [JsonIgnore]
        [Ignore]
        public DateTime CreateTime { get; set; }
        [AutoIncrement]
        [PrimaryKey]
        [JsonIgnore]
        public int ID  { get; set; }
        [JsonIgnore]
        public int UserId { get; set; }
    }
}
