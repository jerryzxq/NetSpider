using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("amazon_collect")]
	public class AmazonCollectEntity
	{
		public AmazonCollectEntity() { }

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
      
		private string productName  ;
        /// <summary>
        /// 产品名称
        /// </summary>
		public string ProductName
		{
			get { return productName; }
			set { productName = value; }
		}
        private string goodsUrl;
        /// <summary>
        /// 商品地址
        /// </summary>

        public string GoodsUrl
        {
            get { return goodsUrl; }
            set { goodsUrl = value; }
        }
		private string imageUrl  ;
        /// <summary>
        /// 产品图片
        /// </summary>
		public string ImageUrl
		{
			get { return imageUrl; }
			set { imageUrl = value; }
		}
      
		private string productRemark  ;
        /// <summary>
        /// 产品标签
        /// </summary>
		public string ProductRemark
		{
			get { return productRemark; }
			set { productRemark = value; }
		}
      
		private string productBrand  ;
        /// <summary>
        /// 产品品牌
        /// </summary>
		public string ProductBrand
		{
			get { return productBrand; }
			set { productBrand = value; }
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
      
		private string promptMessage  ;
        /// <summary>
        /// 提示信息
        /// </summary>
		public string PromptMessage
		{
			get { return promptMessage; }
			set { promptMessage = value; }
		}
      
		private decimal? price  ;
        /// <summary>
        /// 产品价格
        /// </summary>
		public decimal? Price
		{
			get { return price; }
			set { price = value; }
		}
      
		private string remark  ;
        /// <summary>
        /// 备注
        /// </summary>
		public string Remark
		{
			get { return remark; }
			set { remark = value; }
		}
        private int? totalCount;
        /// <summary>
        /// 收藏夹中产品数量
        /// </summary>
        public int? TotalCount
        {
            get { return totalCount; }
            set { totalCount = value; }
        }
      
		private long? userId  ;
        /// <summary>
        /// 客户编号
        /// </summary>
		public long? UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private string accountName  ;
        /// <summary>
        /// 客户账号
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
        private DateTime? createTime = DateTime.Now;
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime? CreateTime
        {
            get { return createTime; }
            set { createTime = value; }
        }
		#endregion
	}
}
