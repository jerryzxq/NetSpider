using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.YiHaoDian
{
	[Alias("yhd_receiveaddress")]
	public class YhdReceiveaddressEntity
	{
		public YhdReceiveaddressEntity() { }

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

		private string area  ;
		/// <summary>
		/// ���ڵ���
		/// </summary>
		public string Area
		{
			get { return area; }
			set { area = value; }
		}

		private string address  ;
		/// <summary>
		/// ��ϸ��ַ
		/// </summary>
		public string Address
		{
			get { return address; }
			set { address = value; }
		}

		private string receiver  ;
		/// <summary>
		/// �ջ�������
		/// </summary>
		public string Receiver
		{
			get { return receiver; }
			set { receiver = value; }
		}

		private string mobile  ;
		/// <summary>
		/// �ֻ�����
		/// </summary>
		public string Mobile
		{
			get { return mobile; }
			set { mobile = value; }
		}

		private bool? isDefault  ;
		/// <summary>
		/// �Ƿ�Ĭ���ջ���ַ
		/// </summary>
		public bool? IsDefault
		{
			get { return isDefault; }
			set { isDefault = value; }
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
