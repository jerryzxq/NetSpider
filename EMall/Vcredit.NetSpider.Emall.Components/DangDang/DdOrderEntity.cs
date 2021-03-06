using ServiceStack.DataAnnotations;
using System;

namespace Vcredit.NetSpider.Emall.Entity.DangDang
{
	[Alias("dd_order")]
	public class DdOrderEntity
	{
		public DdOrderEntity() { }

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

		private DateTime? orderTime;
		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime? OrderTime
		{
			get { return orderTime; }
			set { orderTime = value; }
		}

		private string orderStatus;
		/// <summary>
		/// 订单状态
		/// </summary>
		public string OrderStatus
		{
			get { return orderStatus; }
			set { orderStatus = value; }
		}

		private decimal? totalAmount;
		/// <summary>
		/// 商品总价
		/// </summary>
		public decimal? TotalAmount
		{
			get { return totalAmount; }
			set { totalAmount = value; }
		}

		private string payType;
		/// <summary>
		/// 付款方式
		/// </summary>
		public string PayType
		{
			get { return payType; }
			set { payType = value; }
		}

		private decimal? payAmount;
		/// <summary>
		/// 实际付款
		/// </summary>
		public decimal? PayAmount
		{
			get { return payAmount; }
			set { payAmount = value; }
		}

		private int? goodsCount;
		/// <summary>
		/// 订单商品数量
		/// </summary>
		public int? GoodsCount
		{
			get { return goodsCount; }
			set { goodsCount = value; }
		}

		private decimal? freightage;
		/// <summary>
		/// 运费
		/// </summary>
		public decimal? Freightage
		{
			get { return freightage; }
			set { freightage = value; }
		}

		private DateTime? payTime;
		/// <summary>
		/// 付款时间
		/// </summary>
		public DateTime? PayTime
		{
			get { return payTime; }
			set { payTime = value; }
		}

		private string deliveryType;
		/// <summary>
		/// 送货方式
		/// </summary>
		public string DeliveryType
		{
			get { return deliveryType; }
			set { deliveryType = value; }
		}

		private string address;
		/// <summary>
		/// 收货人地址
		/// </summary>
		public string Address
		{
			get { return address; }
			set { address = value; }
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

		private string telephone;
		/// <summary>
		/// 收货人电话
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
