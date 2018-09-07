using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("Suning_Goods")]
    //[Schema("dbo")]
	public class SuningGoodsEntity
	{
	
		public SuningGoodsEntity() { }


		#region Attributes
      
		private long iD;
       
     [AutoIncrement]
        /// <summary>
        /// ���
        /// </summary>
		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private long? orderID;
       

        /// <summary>
        /// ������
        /// </summary>
		public long? OrderID
		{
			get { return orderID; }
			set { orderID = value; }
		}
      
		private string goodsName;
       

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
        /// ��Ʒ·��
        /// </summary>

        public string GoodsUrl
        {
            get { return goodsUrl; }
            set { goodsUrl = value; }
        }

		private int? goodsCount; 

        /// <summary>
        /// ��Ʒ����
        /// </summary>
		public int? GoodsCount
		{
			get { return goodsCount; }
			set { goodsCount = value; }
		}

        private string imageUrl;
        /// <summary>
        /// ��ƷͼƬ·��
        /// </summary>
        public string ImageUrl
        {
            get { return imageUrl; }
            set { imageUrl = value; }
        }
		private string goodsType;
       

        /// <summary>
        /// ��Ʒ����
        /// </summary>
		public string GoodsType
		{
			get { return goodsType; }
			set { goodsType = value; }
		}
      
		private string goodsProperty;
       

        /// <summary>
        /// ��Ʒ����
        /// </summary>
		public string GoodsProperty
		{
			get { return goodsProperty; }
			set { goodsProperty = value; }
		}
      
		private decimal? price;
       

        /// <summary>
        /// ��Ʒ����
        /// </summary>
		public decimal? Price
		{
			get { return price; }
			set { price = value; }
		}
      
		private long? userId;
       

        /// <summary>
        /// �ͻ����
        /// </summary>
		public long? UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private string accountName;
       

        /// <summary>
        /// �ͻ�����
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private DateTime? createTime;
       

        /// <summary>
        /// ����ʱ��
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
		#endregion

	}
}
