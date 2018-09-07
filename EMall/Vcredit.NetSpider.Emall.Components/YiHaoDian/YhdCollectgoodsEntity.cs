using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.YiHaoDian
{
	[Alias("yhd_collectgoods")]
	public class YhdCollectgoodsEntity
	{
		public YhdCollectgoodsEntity() { }

		#region Attributes

		private long id;
		/// <summary>
		/// ID
		/// </summary>
		[AutoIncrement]
		public long Id
		{
			get { return id; }
			set { id = value; }
		}

		private long userId;
		/// <summary>
		/// 基础信息编号
		/// </summary>
		public long UserId
		{
			get { return userId; }
			set { userId = value; }
		}

		private string goodsTitle;
		/// <summary>
		/// 商品标题
		/// </summary>
		public string GoodsTitle
		{
			get { return goodsTitle; }
			set { goodsTitle = value; }
		}

		private string imageUrl;
		/// <summary>
		/// 商品图片地址
		/// </summary>
		public string ImageUrl
		{
			get { return imageUrl; }
			set { imageUrl = value; }
		}

		private decimal? price;
		/// <summary>
		/// 价格
		/// </summary>
		public decimal? Price
		{
			get { return price; }
			set { price = value; }
		}

		private DateTime? createTime = DateTime.Now;
		/// <summary>
		/// 添加时间
		/// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}

		private DateTime? updateTime;
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
