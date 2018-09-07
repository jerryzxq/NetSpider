using Newtonsoft.Json;
using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("JD_FocusShop")]

    public class FocusShopEntity : BaseEntity
    {
        [JsonProperty(PropertyName = "shopName", NullValueHandling = NullValueHandling.Ignore)]
        public string ShopName { get; set; }
        [JsonProperty(PropertyName = "followCount", NullValueHandling = NullValueHandling.Ignore)]
        public int? Hot { get; set; }

        [JsonProperty(PropertyName = "score", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Evaluate { get; set; }
        public DateTime? FocusTime { get; set; }
    }
}

