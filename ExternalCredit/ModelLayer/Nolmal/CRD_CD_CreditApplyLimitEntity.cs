using System;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
	/// <summary>
	/// 申请次数限制实体
	/// </summary>
	[Alias("CRD_CD_CreditApplyLimit")]
	[Schema("credit")]
	public partial class CRD_CD_CreditApplyLimitEntity
	{
		[AutoIncrement]
		[PrimaryKey]
		public int Id { get; set; }

		/// <summary>
		/// 申请限制次数
		/// </summary>
		public int LimitCount { get; set; }

		/// <summary>
		/// 业务类型
		/// </summary>
		public string BusType { get; set; }

		/// <summary>
		/// 业务描述
		/// </summary>
		public string BusDesc { get; set; }

		/// <summary>
		/// 外贸/担保
		/// </summary>
		public int SourceType { get; set; }

		/// <summary>
		/// IP地址
		/// </summary>
		public string IpAddr { get; set; }

		private DateTime _createtime = DateTime.Now;
		/// <summary>
		/// 添加时间
		/// </summary>
		public DateTime CreateTime
		{
			get { return _createtime; }
			set { _createtime = value; }
		}

		/// <summary>
		/// 数据更新时间
		/// </summary>
		public DateTime? UpdateTime { get; set; }
	}
}