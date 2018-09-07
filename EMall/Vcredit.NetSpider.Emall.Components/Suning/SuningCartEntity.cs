using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("Suning_Cart")] 
	public class SuningCartEntity
	{
		public SuningCartEntity() { }

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
      
		private string sellerName  ;
        /// <summary>
        /// �̼�����
        /// </summary>
		public string SellerName
		{
			get { return sellerName; }
			set { sellerName = value; }
		}
        private string shopUrl;

        /// <summary>
        /// ����URL
        /// </summary>
        public string ShopUrl
        {
            get { return shopUrl; }
            set { shopUrl = value; }
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
        /// ��ƷUrl
        /// </summary>
        public string GoodsUrl
        {
            get { return goodsUrl; }
            set { goodsUrl = value; }
        }

        private string imageUrl;
        /// <summary>
        /// ��ƷͼƬ
        /// </summary>

        public string ImageUrl
        {
            get { return imageUrl; }
            set { imageUrl = value; }
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
      
		private int? productCount  ;
        /// <summary>
        /// ��Ʒ����
        /// </summary>
		public int? ProductCount
		{
			get { return productCount; }
			set { productCount = value; }
		}
        private string productCountLimit;
        /// <summary>
        /// ������������
        /// </summary>
        public string ProductCountLimit
        {
            get { return productCountLimit; }
            set { productCountLimit = value; }
        }

		private decimal? totalPirce  ;
        /// <summary>
        /// �ܼ�
        /// </summary>
		public decimal? TotalPirce
		{
			get { return totalPirce; }
			set { totalPirce = value; }
		}
      
		private decimal? freight  ;
        /// <summary>
        /// �˷�
        /// </summary>
		public decimal? Freight
		{
			get { return freight; }
			set { freight = value; }
		}
        private string freeFreight;
        /// <summary>
        /// ���˷�����
        /// </summary>
        public string FreeFreight
        {
            get { return freeFreight; }
            set { freeFreight = value; }
        }
        private string productAttr;
        /// <summary>
        /// ����
        /// </summary>
        public string ProductAttr
        {
            get { return productAttr; }
            set { productAttr = value; }
        }
        private string packageName;
        /// <summary>
        /// �ײ�����
        /// </summary>
		public string PackageName
		{
			get { return packageName; }
			set { packageName = value; }
		}
      
		private decimal? packagePrice  ;
        /// <summary>
        /// �ײͼ�
        /// </summary>
		public decimal? PackagePrice
		{
			get { return packagePrice; }
			set { packagePrice = value; }
		}
      
		private int? packageCount  ;
        /// <summary>
        /// �ײ�����
        /// </summary>
		public int? PackageCount
		{
			get { return packageCount; }
			set { packageCount = value; }
		}
      
		private decimal? packageTotalPrice  ;
        /// <summary>
        /// �ײ��ܼ�
        /// </summary>
		public decimal? PackageTotalPrice
		{
			get { return packageTotalPrice; }
			set { packageTotalPrice = value; }
		}
      
		private string serivce  ;
        /// <summary>
        /// ����
        /// </summary>
		public string Serivce
		{
			get { return serivce; }
			set { serivce = value; }
		}
      
		private string realGoods  ;
        /// <summary>
        /// ��Ʒ��֤
        /// </summary>
		public string RealGoods
		{
			get { return realGoods; }
			set { realGoods = value; }
		}
      
		private string youhui  ;
        /// <summary>
        /// �Ż�
        /// </summary>
		public string Youhui
		{
			get { return youhui; }
			set { youhui = value; }
		}
      
		private string youhuiRemark  ;
        /// <summary>
        /// �Ż�����
        /// </summary>
		public string YouhuiRemark
		{
			get { return youhuiRemark; }
			set { youhuiRemark = value; }
		}
        private string shoppingType;
        /// <summary>
        /// ��������
        /// </summary>
        public string ShoppingType
        {
            get { return shoppingType; }
            set { shoppingType = value; }
        }

        private string expressType;
        /// <summary>
        /// �������
        /// </summary>
        public string ExpressType
        {
            get { return expressType; }
            set { expressType = value; }
        }
      
		private long userId  ;
        /// <summary>
        /// �ͻ����
        /// </summary>
		public long UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private string accountName  ;
        /// <summary>
        /// �ͻ�����
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}

        private DateTime createTime;
        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime CreateTime
        {
            get { return createTime; }
            set { createTime = value; }
        }

        private string shopCode;
        [Ignore]
        public string ShopCode
        {
            get { return shopCode; }
            set { shopCode = value; }
        }
        private string cmmdtyCode;
        [Ignore]
        public string CmmdtyCode
        {
            get { return cmmdtyCode; }
            set { cmmdtyCode = value; }
        }
		#endregion
	}
}
