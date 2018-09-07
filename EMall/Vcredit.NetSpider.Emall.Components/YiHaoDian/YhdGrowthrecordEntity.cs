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
		/// ������Ϣ���
		/// </summary>
		public long UserId
		{
			get { return userId; }
			set { userId = value; }
		}

		private string orderNumber;
		/// <summary>
		/// ������
		/// </summary>
		public string OrderNumber
		{
			get { return orderNumber; }
			set { orderNumber = value; }
		}

		private decimal? amount;
		/// <summary>
		/// ���׶�
		/// </summary>
		public decimal? Amount
		{
			get { return amount; }
			set { amount = value; }
		}

		private int? growthValue  ;
		/// <summary>
		/// ��óɳ�ֵ
		/// </summary>
		public int? GrowthValue
		{
			get { return growthValue; }
			set { growthValue = value; }
		}

		private int? currentValue;
		/// <summary>
		/// ��ǰ�ɳ�ֵ
		/// </summary>
		public int? CurrentValue
		{
			get { return currentValue; }
			set { currentValue = value; }
		}

		private DateTime? getTime  ;
		/// <summary>
		/// ���ʱ��
		/// </summary>
		public DateTime? GetTime
		{
			get { return getTime; }
			set { getTime = value; }
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
