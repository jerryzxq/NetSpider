using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.YiHaoDian
{
	[Alias("yhd_goods")]
	public class YhdGoodsEntity
	{
		public YhdGoodsEntity() { }

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
		
		private string goodsName  ;
		/// <summary>
		/// ��Ʒ����
		/// </summary>
		public string GoodsName
		{
			get { return goodsName; }
			set { goodsName = value; }
		}

		private string goodsUrl;
		/// <summary>
		/// ��Ʒ��ַ
		/// </summary>
		public string GoodsUrl
		{
			get { return goodsUrl; }
			set { goodsUrl = value; }
		}

		private string imageUrl  ;
		/// <summary>
		/// ��ƷͼƬ��ַ
		/// </summary>
		public string ImageUrl
		{
			get { return imageUrl; }
			set { imageUrl = value; }
		}

		private int? goodsCount  ;
		/// <summary>
		/// ÿ����Ʒ��������
		/// </summary>
		public int? GoodsCount
		{
			get { return goodsCount; }
			set { goodsCount = value; }
		}

		private decimal? price  ;
		/// <summary>
		/// ��Ʒ����
		/// </summary>
		public decimal? Price
		{
			get { return price; }
			set { price = value; }
		}

		private decimal? totalAmount;
		/// <summary>
		/// �ܶ�
		/// </summary>
		public decimal? TotalAmount
		{
			get { return totalAmount; }
			set { totalAmount = value; }
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
