using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("JD_ShoppingCart")]
    
    public  class ShoppingCartEntity:BaseEntity
    {
        /// <summary>
        /// 商品名称
        /// </summary>
        public string GoodsName { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 商品数量
        /// </summary>
        
        public int GoodsCount { get; set; }
        /// <summary>
        /// 商品类型
        /// </summary>
        public string  GoodsType { get; set; }

        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImageUrl { get; set; }
        /// <summary>
        /// 产品链接地址
        /// </summary>
        public string GoodsUrl { get; set; }

    }
}
