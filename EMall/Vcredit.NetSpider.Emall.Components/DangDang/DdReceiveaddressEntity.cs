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
		/// ������Ϣ���
		/// </summary>
		public long UserId
		{
			get { return userId; }
			set { userId = value; }
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

		private string area;
		/// <summary>
		/// ���ڵ���
		/// </summary>
		public string Area
		{
			get { return area; }
			set { area = value; }
		}

		private string address;
		/// <summary>
		/// ��ϸ��ַ
		/// </summary>
		public string Address
		{
			get { return address; }
			set { address = value; }
		}

		private string postcode;
		/// <summary>
		/// ��������
		/// </summary>
		public string Postcode
		{
			get { return postcode; }
			set { postcode = value; }
		}

		private string mobile;
		/// <summary>
		/// �ֻ�����
		/// </summary>
		public string Mobile
		{
			get { return mobile; }
			set { mobile = value; }
		}

		private string telephone;
		/// <summary>
		/// �̶��绰
		/// </summary>
		public string Telephone
		{
			get { return telephone; }
			set { telephone = value; }
		}

		private bool? isDefault;
		/// <summary>
		/// �Ƿ�Ĭ���ջ���ַ
		/// </summary>
		public bool? IsDefault
		{
			get { return isDefault; }
			set { isDefault = value; }
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
