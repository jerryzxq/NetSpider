using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("Suning_ProductCollect")] 
	public class SuningProductCollectEntity
	{
		public SuningProductCollectEntity() { }

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
      
		private string catagoryName  ;
        /// <summary>
        /// 分类名称
        /// </summary>
		public string CatagoryName
		{
			get { return catagoryName; }
			set { catagoryName = value; }
		}
      
		private int? catagoryCount  ;
        /// <summary>
        /// 收藏该分类数量
        /// </summary>
		public int? CatagoryCount
		{
			get { return catagoryCount; }
			set { catagoryCount = value; }
		}
      
		private string dealer  ;
        /// <summary>
        /// 商家名称
        /// </summary>
		public string Dealer
		{
			get { return dealer; }
			set { dealer = value; }
		}
      
		private string shopUrl  ;
        /// <summary>
        /// 商家店铺路径
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
      
		private string priceType  ;
        /// <summary>
        /// 价格类型
        /// </summary>
		public string PriceType
		{
			get { return priceType; }
			set { priceType = value; }
		}
      
		private string price  ;
        /// <summary>
        /// 收藏商品价格
        /// </summary>
		public string Price
		{
			get { return price; }
			set { price = value; }
		}
      
		private string productState  ;
        /// <summary>
        /// 收藏产品类型
        /// </summary>
		public string ProductState
		{
            get { return productState; }
            set { productState = value; }
		}
      
		private DateTime? collectDateTime  ;
        /// <summary>
        /// 收藏时间
        /// </summary>
		public DateTime? CollectDateTime
		{
			get { return collectDateTime; }
			set { collectDateTime = value; }
		}
      
		private int? fvaoriteNumber  ;
        /// <summary>
        /// 收藏人数
        /// </summary>
		public int? FvaoriteNumber
		{
			get { return fvaoriteNumber; }
			set { fvaoriteNumber = value; }
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
        /// 客户账号账号名
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private DateTime? createTime = DateTime.Now ;
        /// <summary>
        /// 创建时间
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
        private string rmmd;

        /// <summary>
        /// 商铺编码
        /// </summary>
        [Ignore]
        public string Rmmd
        {
            get { return rmmd; }
            set { rmmd = value; }
        } 
        private string shopCode;

        /// <summary>
        /// 商铺编码
        /// </summary>
        [Ignore]
        public string ShopCode
        {
            get { return shopCode; }
            set { shopCode = value; }
        }

        private string partNumberandshopIdStr;
        /// <summary>
        /// prod+Rmmd+ShopCode
        /// </summary>
        [Ignore]
        public string PartNumberandshopIdStr
        {
            get { return partNumberandshopIdStr; }
            set { partNumberandshopIdStr = value; }
        }
		#endregion
	}
}
