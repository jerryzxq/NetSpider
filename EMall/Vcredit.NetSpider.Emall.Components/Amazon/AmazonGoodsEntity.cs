using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("amazon_goods")]
	public class AmazonGoodsEntity
	{
		public AmazonGoodsEntity() { }

		#region Attributes
      
		private long iD  ;
        /// <summary>
        /// 编号
        /// </summary>
        [AutoIncrement]
 		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private long? orderID  ;
        /// <summary>
        /// 订单号
        /// </summary>
		public long? OrderID
		{
			get { return orderID; }
			set { orderID = value; }
		}
      
		private string goodsName  ;
        /// <summary>
        /// 产品名称
        /// </summary>
		public string GoodsName
		{
			get { return goodsName; }
			set { goodsName = value; }
		}
        private string goodsUrl;
        /// <summary>
        /// 产品路径
        /// </summary>

        public string GoodsUrl
        {
            get { return goodsUrl; }
            set { goodsUrl = value; }
        }
        private string goodsMark;
        /// <summary>
        /// 产品标签
        /// </summary>
        public string GoodsMark
        {
            get { return goodsMark; }
            set { goodsMark = value; }
        }
        private int? goodsCount;

        //产品数量
        public int? GoodsCount
        {
            get { return goodsCount; }
            set { goodsCount = value; }
        }
		private string goodsImageUrl  ;
        /// <summary>
        /// 图片URL
        /// </summary>
		public string GoodsImageUrl
		{
			get { return goodsImageUrl; }
			set { goodsImageUrl = value; }
		}
      
		private string sellerName  ;
        /// <summary>
        /// 卖家
        /// </summary>
		public string SellerName
		{
			get { return sellerName; }
			set { sellerName = value; }
		}
      
		private string sellerUrl  ;
        /// <summary>
        /// 卖家商铺
        /// </summary>
		public string SellerUrl
		{
			get { return sellerUrl; }
			set { sellerUrl = value; }
		}
      
		private string goodsDesc  ;
        /// <summary>
        /// 商品详情
        /// </summary>
		public string GoodsDesc
		{
			get { return goodsDesc; }
			set { goodsDesc = value; }
		}
      
		private decimal? price  ;
        /// <summary>
        /// 价格
        /// </summary>
		public decimal? Price
		{
			get { return price; }
			set { price = value; }
		} 
		private DateTime? createTime = DateTime.Now ;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
		#endregion
	}
}
