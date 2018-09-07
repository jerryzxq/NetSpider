using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.YiHaoDian
{
	[Alias("yhd_goods")]
	public class YhdGoodsEntity
	{
		public YhdGoodsEntity() { }

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
		/// 基础信息编号
		/// </summary>
		public long UserId
		{
			get { return userId; }
			set { userId = value; }
		}

		private string accountName  ;
		/// <summary>
		/// 用户账号
		/// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
		
		private string orderNo  ;
		/// <summary>
		/// 订单编号
		/// </summary>
		public string OrderNo
		{
			get { return orderNo; }
			set { orderNo = value; }
		}
		
		private string goodsName  ;
		/// <summary>
		/// 商品名称
		/// </summary>
		public string GoodsName
		{
			get { return goodsName; }
			set { goodsName = value; }
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

		private int? goodsCount  ;
		/// <summary>
		/// 每个商品购买数量
		/// </summary>
		public int? GoodsCount
		{
			get { return goodsCount; }
			set { goodsCount = value; }
		}

		private decimal? price  ;
		/// <summary>
		/// 商品单价
		/// </summary>
		public decimal? Price
		{
			get { return price; }
			set { price = value; }
		}

		private decimal? totalAmount;
		/// <summary>
		/// 总额
		/// </summary>
		public decimal? TotalAmount
		{
			get { return totalAmount; }
			set { totalAmount = value; }
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
