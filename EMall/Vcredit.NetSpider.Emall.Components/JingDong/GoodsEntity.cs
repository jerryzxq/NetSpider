using Newtonsoft.Json;
using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("JD_Goods")]

    /// <summary>
    /// 商品类
    /// </summary>
    public class GoodsEntity : BaseEntity
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }
        [JsonProperty(PropertyName = "wname", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// 商品名称
        /// </summary>
        public string GoodsName { get; set; }
        [JsonProperty(PropertyName = "jdPrice", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 商品数量
        /// </summary>
        [JsonProperty(PropertyName = "num", NullValueHandling = NullValueHandling.Ignore)]
        public int GoodsCount { get; set; }
        [JsonProperty(PropertyName = "catalogyName", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// 商品类型
        /// </summary>
        public string GoodsType { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string AccountName { get; set; }
        [JsonProperty(PropertyName = "imageurl", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImageUrl { get; set; }
       [JsonProperty(PropertyName = "wareId", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// 产品链接地址
        /// </summary>
        public string  GoodsUrl { get; set; }
    }
}
