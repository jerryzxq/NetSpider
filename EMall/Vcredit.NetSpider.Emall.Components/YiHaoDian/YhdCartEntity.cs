using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.YiHaoDian
{
	[Alias("yhd_cart")]
	public class YhdCartEntity
	{
		public YhdCartEntity() { }

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
		/// �û�Id
		/// </summary>
		public long UserId
		{
			get { return userId; }
			set { userId = value; }
		}

		private string shopUrl  ;
		/// <summary>
		/// ���̵�ַ
		/// </summary>
		public string ShopUrl
		{
			get { return shopUrl; }
			set { shopUrl = value; }
		}

		private string productTitle  ;
		/// <summary>
		/// ��Ʒ����
		/// </summary>
		public string ProductTitle
		{
			get { return productTitle; }
			set { productTitle = value; }
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

		private decimal? price  ;
		/// <summary>
		/// ����
		/// </summary>
		public decimal? Price
		{
			get { return price; }
			set { price = value; }
		}

		private int? count  ;
		/// <summary>
		/// ����
		/// </summary>
		public int? Count
		{
			get { return count; }
			set { count = value; }
		}

		private decimal? totalAmount  ;
		/// <summary>
		/// �ܶ�
		/// </summary>
		public decimal? TotalAmount
		{
			get { return totalAmount; }
			set { totalAmount = value; }
		}

		private string weight;
		/// <summary>
		/// ����
		/// </summary>
		public string Weight
		{
			get { return weight; }
			set { weight = value; }
		}

		private bool isValid  ;
		/// <summary>
		/// �Ƿ���Ч
		/// </summary>
		public bool IsValid
		{
			get { return isValid; }
			set { isValid = value; }
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
