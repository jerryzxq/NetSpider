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
		/// ������Ϣ���
		/// </summary>
		public long UserId
		{
			get { return userId; }
			set { userId = value; }
		}

		private string accountName;
		/// <summary>
		/// �û��˺�
		/// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}

		private string orderNo;
		/// <summary>
		/// �������
		/// </summary>
		public string OrderNo
		{
			get { return orderNo; }
			set { orderNo = value; }
		}

		private DateTime? orderTime;
		/// <summary>
		/// ����ʱ��
		/// </summary>
		public DateTime? OrderTime
		{
			get { return orderTime; }
			set { orderTime = value; }
		}

		private string orderStatus;
		/// <summary>
		/// ����״̬
		/// </summary>
		public string OrderStatus
		{
			get { return orderStatus; }
			set { orderStatus = value; }
		}

		private decimal? totalAmount;
		/// <summary>
		/// ��Ʒ�ܼ�
		/// </summary>
		public decimal? TotalAmount
		{
			get { return totalAmount; }
			set { totalAmount = value; }
		}

		private string payType;
		/// <summary>
		/// ���ʽ
		/// </summary>
		public string PayType
		{
			get { return payType; }
			set { payType = value; }
		}

		private decimal? payAmount;
		/// <summary>
		/// ʵ�ʸ���
		/// </summary>
		public decimal? PayAmount
		{
			get { return payAmount; }
			set { payAmount = value; }
		}

		private int? goodsCount;
		/// <summary>
		/// ������Ʒ����
		/// </summary>
		public int? GoodsCount
		{
			get { return goodsCount; }
			set { goodsCount = value; }
		}

		private decimal? freightage;
		/// <summary>
		/// �˷�
		/// </summary>
		public decimal? Freightage
		{
			get { return freightage; }
			set { freightage = value; }
		}

		private DateTime? payTime;
		/// <summary>
		/// ����ʱ��
		/// </summary>
		public DateTime? PayTime
		{
			get { return payTime; }
			set { payTime = value; }
		}

		private string deliveryType;
		/// <summary>
		/// �ͻ���ʽ
		/// </summary>
		public string DeliveryType
		{
			get { return deliveryType; }
			set { deliveryType = value; }
		}

		private string address;
		/// <summary>
		/// �ջ��˵�ַ
		/// </summary>
		public string Address
		{
			get { return address; }
			set { address = value; }
		}

		private string receiver;
		/// <summary>
		/// �ջ�������
		/// </summary>
		public string Receiver
		{
			get { return receiver; }
			set { receiver = value; }
		}

		private string telephone;
		/// <summary>
		/// �ջ��˵绰
		/// </summary>
		public string Telephone
		{
			get { return telephone; }
			set { telephone = value; }
		}

		private DateTime? createTime = DateTime.Now;
		/// <summary>
		/// ���ʱ��
		/// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}

		private DateTime? updateTime;
		/// <summary>
		/// ����ʱ��
		/// </summary>
		public DateTime? UpdateTime
		{
			get { return updateTime; }
			set { updateTime = value; }
		}
		#endregion
	}
}
