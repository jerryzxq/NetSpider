using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.YiHaoDian
{
	[Alias("yhd_collectshops")]
	public class YhdCollectshopsEntity
	{
		public YhdCollectshopsEntity() { }

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

		private string shopTitle;
		/// <summary>
		/// ���̱���
		/// </summary>
		public string ShopTitle
		{
			get { return shopTitle; }
			set { shopTitle = value; }
		}

		private string shopUrl;
		/// <summary>
		/// ���̵�ַ
		/// </summary>
		public string ShopUrl
		{
			get { return shopUrl; }
			set { shopUrl = value; }
		}

		private decimal? descriptScore;
		/// <summary>
		/// �����������
		/// </summary>
		public decimal? DescriptScore
		{
			get { return descriptScore; }
			set { descriptScore = value; }
		}

		private decimal? attitudeScore;
		/// <summary>
		/// ����̬������
		/// </summary>
		public decimal? AttitudeScore
		{
			get { return attitudeScore; }
			set { attitudeScore = value; }
		}

		private decimal? logisticsScore;
		/// <summary>
		/// �����ٶ�����
		/// </summary>
		public decimal? LogisticsScore
		{
			get { return logisticsScore; }
			set { logisticsScore = value; }
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
