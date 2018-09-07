using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("amazon_collect")]
	public class AmazonCollectEntity
	{
		public AmazonCollectEntity() { }

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
      
		private string productName  ;
        /// <summary>
        /// ��Ʒ����
        /// </summary>
		public string ProductName
		{
			get { return productName; }
			set { productName = value; }
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
        /// ��ƷͼƬ
        /// </summary>
		public string ImageUrl
		{
			get { return imageUrl; }
			set { imageUrl = value; }
		}
      
		private string productRemark  ;
        /// <summary>
        /// ��Ʒ��ǩ
        /// </summary>
		public string ProductRemark
		{
			get { return productRemark; }
			set { productRemark = value; }
		}
      
		private string productBrand  ;
        /// <summary>
        /// ��ƷƷ��
        /// </summary>
		public string ProductBrand
		{
			get { return productBrand; }
			set { productBrand = value; }
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
      
		private string promptMessage  ;
        /// <summary>
        /// ��ʾ��Ϣ
        /// </summary>
		public string PromptMessage
		{
			get { return promptMessage; }
			set { promptMessage = value; }
		}
      
		private decimal? price  ;
        /// <summary>
        /// ��Ʒ�۸�
        /// </summary>
		public decimal? Price
		{
			get { return price; }
			set { price = value; }
		}
      
		private string remark  ;
        /// <summary>
        /// ��ע
        /// </summary>
		public string Remark
		{
			get { return remark; }
			set { remark = value; }
		}
        private int? totalCount;
        /// <summary>
        /// �ղؼ��в�Ʒ����
        /// </summary>
        public int? TotalCount
        {
            get { return totalCount; }
            set { totalCount = value; }
        }
      
		private long? userId  ;
        /// <summary>
        /// �ͻ����
        /// </summary>
		public long? UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private string accountName  ;
        /// <summary>
        /// �ͻ��˺�
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
        private DateTime? createTime = DateTime.Now;
        /// <summary>
        /// ��������
        /// </summary>
        public DateTime? CreateTime
        {
            get { return createTime; }
            set { createTime = value; }
        }
		#endregion
	}
}
