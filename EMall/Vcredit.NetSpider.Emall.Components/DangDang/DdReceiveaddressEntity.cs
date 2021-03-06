using ServiceStack.DataAnnotations;
using System;

namespace Vcredit.NetSpider.Emall.Entity.DangDang
{
	[Alias("dd_receiveaddress")]
	public class DdReceiveaddressEntity
	{
		public DdReceiveaddressEntity() { }

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

		private string receiver;
		/// <summary>
		/// 收货人姓名
		/// </summary>
		public string Receiver
		{
			get { return receiver; }
			set { receiver = value; }
		}

		private string area;
		/// <summary>
		/// 所在地区
		/// </summary>
		public string Area
		{
			get { return area; }
			set { area = value; }
		}

		private string address;
		/// <summary>
		/// 详细地址
		/// </summary>
		public string Address
		{
			get { return address; }
			set { address = value; }
		}

		private string postcode;
		/// <summary>
		/// 邮政编码
		/// </summary>
		public string Postcode
		{
			get { return postcode; }
			set { postcode = value; }
		}

		private string mobile;
		/// <summary>
		/// 手机号码
		/// </summary>
		public string Mobile
		{
			get { return mobile; }
			set { mobile = value; }
		}

		private string telephone;
		/// <summary>
		/// 固定电话
		/// </summary>
		public string Telephone
		{
			get { return telephone; }
			set { telephone = value; }
		}

		private bool? isDefault;
		/// <summary>
		/// 是否默认收货地址
		/// </summary>
		public bool? IsDefault
		{
			get { return isDefault; }
			set { isDefault = value; }
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
