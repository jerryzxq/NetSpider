using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("amazon_goods")]
	public class AmazonGoodsEntity
	{
		public AmazonGoodsEntity() { }

		#region Attributes
      
		private long iD  ;
        /// <summary>
        /// ���
        /// </summary>
        [AutoIncrement]
 		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private long? orderID  ;
        /// <summary>
        /// ������
        /// </summary>
		public long? OrderID
		{
			get { return orderID; }
			set { orderID = value; }
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
        /// ��Ʒ·��
        /// </summary>

        public string GoodsUrl
        {
            get { return goodsUrl; }
            set { goodsUrl = value; }
        }
        private string goodsMark;
        /// <summary>
        /// ��Ʒ��ǩ
        /// </summary>
        public string GoodsMark
        {
            get { return goodsMark; }
            set { goodsMark = value; }
        }
        private int? goodsCount;

        //��Ʒ����
        public int? GoodsCount
        {
            get { return goodsCount; }
            set { goodsCount = value; }
        }
		private string goodsImageUrl  ;
        /// <summary>
        /// ͼƬURL
        /// </summary>
		public string GoodsImageUrl
		{
			get { return goodsImageUrl; }
			set { goodsImageUrl = value; }
		}
      
		private string sellerName  ;
        /// <summary>
        /// ����
        /// </summary>
		public string SellerName
		{
			get { return sellerName; }
			set { sellerName = value; }
		}
      
		private string sellerUrl  ;
        /// <summary>
        /// ��������
        /// </summary>
		public string SellerUrl
		{
			get { return sellerUrl; }
			set { sellerUrl = value; }
		}
      
		private string goodsDesc  ;
        /// <summary>
        /// ��Ʒ����
        /// </summary>
		public string GoodsDesc
		{
			get { return goodsDesc; }
			set { goodsDesc = value; }
		}
      
		private decimal? price  ;
        /// <summary>
        /// �۸�
        /// </summary>
		public decimal? Price
		{
			get { return price; }
			set { price = value; }
		} 
		private DateTime? createTime = DateTime.Now ;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
		#endregion
	}
}
