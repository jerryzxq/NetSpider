using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.YiHaoDian
{
	[Alias("yhd_growthrecord")]
	public class YhdGrowthrecordEntity
	{
		public YhdGrowthrecordEntity() { }

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

		private string orderNumber;
		/// <summary>
		/// 订单号
		/// </summary>
		public string OrderNumber
		{
			get { return orderNumber; }
			set { orderNumber = value; }
		}

		private decimal? amount;
		/// <summary>
		/// 交易额
		/// </summary>
		public decimal? Amount
		{
			get { return amount; }
			set { amount = value; }
		}

		private int? growthValue  ;
		/// <summary>
		/// 获得成长值
		/// </summary>
		public int? GrowthValue
		{
			get { return growthValue; }
			set { growthValue = value; }
		}

		private int? currentValue;
		/// <summary>
		/// 当前成长值
		/// </summary>
		public int? CurrentValue
		{
			get { return currentValue; }
			set { currentValue = value; }
		}

		private DateTime? getTime  ;
		/// <summary>
		/// 获得时间
		/// </summary>
		public DateTime? GetTime
		{
			get { return getTime; }
			set { getTime = value; }
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
