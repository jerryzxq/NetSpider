using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.YiHaoDian
{
	[Alias("yhd_logistics")]
	public class YhdLogisticsEntity
	{
		public YhdLogisticsEntity() { }

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
		/// ������Ϣ���
		/// </summary>
		public long UserId
		{
			get { return userId; }
			set { userId = value; }
		}

		private string accountName  ;
		/// <summary>
		/// �û��˺�
		/// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}

		private string orderNo  ;
		/// <summary>
		/// �������
		/// </summary>
		public string OrderNo
		{
			get { return orderNo; }
			set { orderNo = value; }
		}

		private string logisticsNo  ;
		/// <summary>
		/// ���˵���
		/// </summary>
		public string LogisticsNo
		{
			get { return logisticsNo; }
			set { logisticsNo = value; }
		}

		private string logisticsType  ;
		/// <summary>
		/// ������ʽ
		/// </summary>
		public string LogisticsType
		{
			get { return logisticsType; }
			set { logisticsType = value; }
		}

		private string logisticsCompany  ;
		/// <summary>
		/// ������(��˾)
		/// </summary>
		public string LogisticsCompany
		{
			get { return logisticsCompany; }
			set { logisticsCompany = value; }
		}

		private string address  ;
		/// <summary>
		/// �����ջ���ַ
		/// </summary>
		public string Address
		{
			get { return address; }
			set { address = value; }
		}

		private string receiver  ;
		/// <summary>
		/// �ջ���
		/// </summary>
		public string Receiver
		{
			get { return receiver; }
			set { receiver = value; }
		}

		private string telephone  ;
		/// <summary>
		/// �绰
		/// </summary>
		public string Telephone
		{
			get { return telephone; }
			set { telephone = value; }
		}

		private DateTime? createTime = DateTime.Now ;
		/// <summary>
		/// ���ʱ��
		/// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}

		private DateTime? updateTime  ;
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
