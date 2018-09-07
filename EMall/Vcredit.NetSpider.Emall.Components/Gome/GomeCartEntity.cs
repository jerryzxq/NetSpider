using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.Gome
{
	[Alias("gome_cart")]
	public class GomeCartEntity
	{
		public GomeCartEntity() { }

		#region Attributes
      
		private long id  ;
        /// <summary>
        /// 
        /// </summary>
        [AutoIncrement]
 		public long Id
		{
			get { return id; }
			set { id = value; }
		}
      
		private long userId  ;
        /// <summary>
        /// 
        /// </summary>
		public long UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private string shopName  ;
        /// <summary>
        /// 
        /// </summary>
		public string ShopName
		{
			get { return shopName; }
			set { shopName = value; }
		}
      
		private string shopUrl  ;
        /// <summary>
        /// 
        /// </summary>
		public string ShopUrl
		{
			get { return shopUrl; }
			set { shopUrl = value; }
		}
      
		private string goodsName  ;
        /// <summary>
        /// 
        /// </summary>
		public string GoodsName
		{
			get { return goodsName; }
			set { goodsName = value; }
		}

        private string goodsUrl;
        /// <summary>
        /// 
        /// </summary>
		public string GoodsUrl
        {
            get { return goodsUrl; }
            set { goodsUrl = value; }
        }

        private string imageUrl;
        /// <summary>
        /// 
        /// </summary>
		public string ImageUrl
        {
            get { return imageUrl; }
            set { imageUrl = value; }
        }

        private decimal? price  ;
        /// <summary>
        /// 
        /// </summary>
		public decimal? Price
		{
			get { return price; }
			set { price = value; }
		}
      
		private int? count  ;
        /// <summary>
        /// 
        /// </summary>
		public int? Count
		{
			get { return count; }
			set { count = value; }
		}
      
		private decimal? totalAmount  ;
        /// <summary>
        /// 
        /// </summary>
		public decimal? TotalAmount
		{
			get { return totalAmount; }
			set { totalAmount = value; }
		}
      
		private bool isValid  ;
        /// <summary>
        /// 
        /// </summary>
		public bool IsValid
		{
			get { return isValid; }
			set { isValid = value; }
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
      
		private DateTime? updateTime  ;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? UpdateTime
		{
			get { return updateTime; }
			set { updateTime = value; }
		}
		#endregion
	}
}
