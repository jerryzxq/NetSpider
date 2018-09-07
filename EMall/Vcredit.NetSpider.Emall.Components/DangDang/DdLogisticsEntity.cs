using ServiceStack.DataAnnotations;
using System;

namespace Vcredit.NetSpider.Emall.Entity.DangDang
{
	[Alias("dd_logistics")]
	public class DdLogisticsEntity
	{
		public DdLogisticsEntity() { }

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

		private string accountName;
		/// <summary>
		/// 用户账号
		/// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}

		private string orderNo;
		/// <summary>
		/// 订单编号
		/// </summary>
		public string OrderNo
		{
			get { return orderNo; }
			set { orderNo = value; }
		}

		private string logisticsNo;
		/// <summary>
		/// 货运单号
		/// </summary>
		public string LogisticsNo
		{
			get { return logisticsNo; }
			set { logisticsNo = value; }
		}

		private string logisticsType;
		/// <summary>
		/// 发货方式
		/// </summary>
		public string LogisticsType
		{
			get { return logisticsType; }
			set { logisticsType = value; }
		}

		private string logisticsCompany;
		/// <summary>
		/// 承运人(公司)
		/// </summary>
		public string LogisticsCompany
		{
			get { return logisticsCompany; }
			set { logisticsCompany = value; }
		}

		private string address;
		/// <summary>
		/// 订单收货地址
		/// </summary>
		public string Address
		{
			get { return address; }
			set { address = value; }
		}

		private string receiver;
		/// <summary>
		/// 收货人
		/// </summary>
		public string Receiver
		{
			get { return receiver; }
			set { receiver = value; }
		}

		private string telephone;
		/// <summary>
		/// 电话
		/// </summary>
		public string Telephone
		{
			get { return telephone; }
			set { telephone = value; }
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
