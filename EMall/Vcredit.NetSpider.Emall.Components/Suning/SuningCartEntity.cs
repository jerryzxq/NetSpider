using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("Suning_Cart")] 
	public class SuningCartEntity
	{
		public SuningCartEntity() { }

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
      
		private string sellerName  ;
        /// <summary>
        /// 商家名称
        /// </summary>
		public string SellerName
		{
			get { return sellerName; }
			set { sellerName = value; }
		}
        private string shopUrl;

        /// <summary>
        /// 商铺URL
        /// </summary>
        public string ShopUrl
        {
            get { return shopUrl; }
            set { shopUrl = value; }
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
        /// 产品Url
        /// </summary>
        public string GoodsUrl
        {
            get { return goodsUrl; }
            set { goodsUrl = value; }
        }

        private string imageUrl;
        /// <summary>
        /// 产品图片
        /// </summary>

        public string ImageUrl
        {
            get { return imageUrl; }
            set { imageUrl = value; }
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
      
		private int? productCount  ;
        /// <summary>
        /// 产品数量
        /// </summary>
		public int? ProductCount
		{
			get { return productCount; }
			set { productCount = value; }
		}
        private string productCountLimit;
        /// <summary>
        /// 数量限制条件
        /// </summary>
        public string ProductCountLimit
        {
            get { return productCountLimit; }
            set { productCountLimit = value; }
        }

		private decimal? totalPirce  ;
        /// <summary>
        /// 总价
        /// </summary>
		public decimal? TotalPirce
		{
			get { return totalPirce; }
			set { totalPirce = value; }
		}
      
		private decimal? freight  ;
        /// <summary>
        /// 运费
        /// </summary>
		public decimal? Freight
		{
			get { return freight; }
			set { freight = value; }
		}
        private string freeFreight;
        /// <summary>
        /// 免运费条件
        /// </summary>
        public string FreeFreight
        {
            get { return freeFreight; }
            set { freeFreight = value; }
        }
        private string productAttr;
        /// <summary>
        /// 属性
        /// </summary>
        public string ProductAttr
        {
            get { return productAttr; }
            set { productAttr = value; }
        }
        private string packageName;
        /// <summary>
        /// 套餐名称
        /// </summary>
		public string PackageName
		{
			get { return packageName; }
			set { packageName = value; }
		}
      
		private decimal? packagePrice  ;
        /// <summary>
        /// 套餐价
        /// </summary>
		public decimal? PackagePrice
		{
			get { return packagePrice; }
			set { packagePrice = value; }
		}
      
		private int? packageCount  ;
        /// <summary>
        /// 套餐数量
        /// </summary>
		public int? PackageCount
		{
			get { return packageCount; }
			set { packageCount = value; }
		}
      
		private decimal? packageTotalPrice  ;
        /// <summary>
        /// 套餐总价
        /// </summary>
		public decimal? PackageTotalPrice
		{
			get { return packageTotalPrice; }
			set { packageTotalPrice = value; }
		}
      
		private string serivce  ;
        /// <summary>
        /// 服务
        /// </summary>
		public string Serivce
		{
			get { return serivce; }
			set { serivce = value; }
		}
      
		private string realGoods  ;
        /// <summary>
        /// 产品保证
        /// </summary>
		public string RealGoods
		{
			get { return realGoods; }
			set { realGoods = value; }
		}
      
		private string youhui  ;
        /// <summary>
        /// 优惠
        /// </summary>
		public string Youhui
		{
			get { return youhui; }
			set { youhui = value; }
		}
      
		private string youhuiRemark  ;
        /// <summary>
        /// 优惠条件
        /// </summary>
		public string YouhuiRemark
		{
			get { return youhuiRemark; }
			set { youhuiRemark = value; }
		}
        private string shoppingType;
        /// <summary>
        /// 购物类型
        /// </summary>
        public string ShoppingType
        {
            get { return shoppingType; }
            set { shoppingType = value; }
        }

        private string expressType;
        /// <summary>
        /// 快递类型
        /// </summary>
        public string ExpressType
        {
            get { return expressType; }
            set { expressType = value; }
        }
      
		private long userId  ;
        /// <summary>
        /// 客户编号
        /// </summary>
		public long UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private string accountName  ;
        /// <summary>
        /// 客户名称
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}

        private DateTime createTime;
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get { return createTime; }
            set { createTime = value; }
        }

        private string shopCode;
        [Ignore]
        public string ShopCode
        {
            get { return shopCode; }
            set { shopCode = value; }
        }
        private string cmmdtyCode;
        [Ignore]
        public string CmmdtyCode
        {
            get { return cmmdtyCode; }
            set { cmmdtyCode = value; }
        }
		#endregion
	}
}
