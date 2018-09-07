using System;

namespace Vcredit.ExternalCredit.Dto.Assure
{
	/// <summary>
	/// 数据查询DTO
	/// </summary>
	public class ApplyLimitMaintainDto : DataRequestDto
	{
		public int Id { get; set; }

		/// <summary>
		/// 申请限制次数
		/// </summary>
		public int LimitCount { get; set; }

		/// <summary>
		/// 业务代码
		/// </summary>
		public string BusType { get; set; }

		/// <summary>
		/// 业务描述
		/// </summary>
		public string BusDesc { get; set; }

		/// <summary>
		/// 源数据类型（外贸/担保）
		/// </summary>
		public int SourceType { get; set; }

		/// <summary>
		/// IP地址
		/// </summary>
		public string IpAddr { get; set; }

		/// <summary>
		/// 添加时间
		/// </summary>
		public DateTime Createtime { get; set; }

		/// <summary>
		/// 数据更新时间
		/// </summary>
		public DateTime? UpdateTime { get; set; }
	}
}
