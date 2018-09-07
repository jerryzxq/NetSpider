using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using System.Linq;
using System.Text;
using Vcredit.Common.Ext;
using Newtonsoft.Json;
namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("JD_BrowseHistorie")]

    public class BrowseHistoryEntity : BaseEntity //浏览记录
    {
        [Ignore]
        public string BrowseDateTime { get; set; }

        /// <summary>
        /// 浏览时间
        /// </summary>
        [Alias("BrowseDateTime")]
        public DateTime? BrowseDateTimeReal { get { return BrowseDateTime.ToDateTime(); } }

        /// <summary>
        /// 商品名称
        /// </summary>
        [JsonProperty(PropertyName = "wname", NullValueHandling = NullValueHandling.Ignore)]

        public string ProductName { get; set; }

        /// <summary>
        /// 商品类型
        /// </summary>

        public string ProductType { get; set; }
        [JsonProperty(PropertyName = "jdPrice", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// 商品金额
        /// </summary>
        public decimal ProductCost { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string AccountName { get; set; }




    }
}
