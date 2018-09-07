using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.YiHaoDian
{
	[Alias("yhd_collectgoods")]
	public class YhdCollectgoodsEntity
	{
		public YhdCollectgoodsEntity() { }

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

		private string goodsTitle;
		/// <summary>
		/// ��Ʒ����
		/// </summary>
		public string GoodsTitle
		{
			get { return goodsTitle; }
			set { goodsTitle = value; }
		}

		private string imageUrl;
		/// <summary>
		/// ��ƷͼƬ��ַ
		/// </summary>
		public string ImageUrl
		{
			get { return imageUrl; }
			set { imageUrl = value; }
		}

		private decimal? price;
		/// <summary>
		/// �۸�
		/// </summary>
		public decimal? Price
		{
			get { return price; }
			set { price = value; }
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
