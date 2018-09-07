using ServiceStack.DataAnnotations;
using System;

namespace Vcredit.NetSpider.Emall.Entity.DangDang
{
	[Alias("dd_collectshops")]
	public class DdCollectshopsEntity
	{
		public DdCollectshopsEntity() { }

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

		private string shopTitle;
		/// <summary>
		/// 店铺标题
		/// </summary>
		public string ShopTitle
		{
			get { return shopTitle; }
			set { shopTitle = value; }
		}

		private string shopUrl;
		/// <summary>
		/// 店铺地址
		/// </summary>
		public string ShopUrl
		{
			get { return shopUrl; }
			set { shopUrl = value; }
		}

		private int? storeLevel;
		/// <summary>
		/// 店铺等级
		/// </summary>
		public int? StoreLevel
		{
			get { return storeLevel; }
			set { storeLevel = value; }
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
