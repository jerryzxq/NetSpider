using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.DataAnnotations;
namespace  Vcredit.NetSpider.Emall.Entity
{
    [Alias("JD_AttentionData")]
    
    public class  AttentionDataEntity:BaseEntity //关注数据
    {

        /// <summary>
        /// 商品类型
        /// </summary>
      
        public string ProductType { get; set; }

        /// <summary>
        /// 商品金额
        /// </summary>
        public decimal ProductCost { get; set; }

        /// <summary>
        /// 商品类型个数
        /// </summary>
        public int ProductTypeCount { get; set; }

        /// <summary>
        /// 店铺
        /// </summary>
   
        public string ShopName { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
     
        public string Brand { get; set; }

        /// <summary>
        /// 活动
        /// </summary>
        
        public string Activity { get; set; }


    }
}
