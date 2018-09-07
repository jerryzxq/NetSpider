using Newtonsoft.Json;
using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("JD_FocusProduct")]

    public class FocusProductEntity : BaseEntity
    {
        [JsonProperty(PropertyName = "wname", NullValueHandling = NullValueHandling.Ignore)]
        public string ProductName { get; set; }
        [JsonProperty(PropertyName = "jdPrice", NullValueHandling = NullValueHandling.Ignore)]
        public decimal Price { get; set; }
        [JsonProperty(PropertyName = "wareId", NullValueHandling = NullValueHandling.Ignore)]
        public string ProductType { get; set; }

    }

}
