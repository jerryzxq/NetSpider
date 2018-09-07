using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.YiHaoDian
{
	[Alias("yhd_collectshops")]
	public class YhdCollectshopsEntity
	{
		public YhdCollectshopsEntity() { }

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

		private decimal? descriptScore;
		/// <summary>
		/// 描述相符评分
		/// </summary>
		public decimal? DescriptScore
		{
			get { return descriptScore; }
			set { descriptScore = value; }
		}

		private decimal? attitudeScore;
		/// <summary>
		/// 服务态度评分
		/// </summary>
		public decimal? AttitudeScore
		{
			get { return attitudeScore; }
			set { attitudeScore = value; }
		}

		private decimal? logisticsScore;
		/// <summary>
		/// 发货速度评分
		/// </summary>
		public decimal? LogisticsScore
		{
			get { return logisticsScore; }
			set { logisticsScore = value; }
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
