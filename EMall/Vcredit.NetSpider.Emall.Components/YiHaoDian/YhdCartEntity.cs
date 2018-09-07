using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.YiHaoDian
{
	[Alias("yhd_cart")]
	public class YhdCartEntity
	{
		public YhdCartEntity() { }

		#region Attributes

		private long id  ;
		/// <summary>
		/// ID
		/// </summary>
		[AutoIncrement]
 		public long Id
		{
			get { return id; }
			set { id = value; }
		}

		private long userId  ;
		/// <summary>
		/// 用户Id
		/// </summary>
		public long UserId
		{
			get { return userId; }
			set { userId = value; }
		}

		private string shopUrl  ;
		/// <summary>
		/// 店铺地址
		/// </summary>
		public string ShopUrl
		{
			get { return shopUrl; }
			set { shopUrl = value; }
		}

		private string productTitle  ;
		/// <summary>
		/// 商品标题
		/// </summary>
		public string ProductTitle
		{
			get { return productTitle; }
			set { productTitle = value; }
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
		/// 商品图片地址
		/// </summary>
		public string ImageUrl
		{
			get { return imageUrl; }
			set { imageUrl = value; }
		}

		private decimal? price  ;
		/// <summary>
		/// 单价
		/// </summary>
		public decimal? Price
		{
			get { return price; }
			set { price = value; }
		}

		private int? count  ;
		/// <summary>
		/// 数量
		/// </summary>
		public int? Count
		{
			get { return count; }
			set { count = value; }
		}

		private decimal? totalAmount  ;
		/// <summary>
		/// 总额
		/// </summary>
		public decimal? TotalAmount
		{
			get { return totalAmount; }
			set { totalAmount = value; }
		}

		private string weight;
		/// <summary>
		/// 重量
		/// </summary>
		public string Weight
		{
			get { return weight; }
			set { weight = value; }
		}

		private bool isValid  ;
		/// <summary>
		/// 是否有效
		/// </summary>
		public bool IsValid
		{
			get { return isValid; }
			set { isValid = value; }
		}

		private DateTime? createTime = DateTime.Now ;
		/// <summary>
		/// 添加时间
		/// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}

		private DateTime? updateTime  ;
		/// <summary>
		/// 更新时间
		/// </summary>
		public DateTime? UpdateTime
		{
			get { return updateTime; }
			set { updateTime = value; }
		}
		#endregion
	}
}
