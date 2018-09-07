using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("Suning_ProductCollect")] 
	public class SuningProductCollectEntity
	{
		public SuningProductCollectEntity() { }

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
      
		private string catagoryName  ;
        /// <summary>
        /// ��������
        /// </summary>
		public string CatagoryName
		{
			get { return catagoryName; }
			set { catagoryName = value; }
		}
      
		private int? catagoryCount  ;
        /// <summary>
        /// �ղظ÷�������
        /// </summary>
		public int? CatagoryCount
		{
			get { return catagoryCount; }
			set { catagoryCount = value; }
		}
      
		private string dealer  ;
        /// <summary>
        /// �̼�����
        /// </summary>
		public string Dealer
		{
			get { return dealer; }
			set { dealer = value; }
		}
      
		private string shopUrl  ;
        /// <summary>
        /// �̼ҵ���·��
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
      
		private string priceType  ;
        /// <summary>
        /// �۸�����
        /// </summary>
		public string PriceType
		{
			get { return priceType; }
			set { priceType = value; }
		}
      
		private string price  ;
        /// <summary>
        /// �ղ���Ʒ�۸�
        /// </summary>
		public string Price
		{
			get { return price; }
			set { price = value; }
		}
      
		private string productState  ;
        /// <summary>
        /// �ղز�Ʒ����
        /// </summary>
		public string ProductState
		{
            get { return productState; }
            set { productState = value; }
		}
      
		private DateTime? collectDateTime  ;
        /// <summary>
        /// �ղ�ʱ��
        /// </summary>
		public DateTime? CollectDateTime
		{
			get { return collectDateTime; }
			set { collectDateTime = value; }
		}
      
		private int? fvaoriteNumber  ;
        /// <summary>
        /// �ղ�����
        /// </summary>
		public int? FvaoriteNumber
		{
			get { return fvaoriteNumber; }
			set { fvaoriteNumber = value; }
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
        /// �ͻ��˺��˺���
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private DateTime? createTime = DateTime.Now ;
        /// <summary>
        /// ����ʱ��
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
        private string rmmd;

        /// <summary>
        /// ���̱���
        /// </summary>
        [Ignore]
        public string Rmmd
        {
            get { return rmmd; }
            set { rmmd = value; }
        } 
        private string shopCode;

        /// <summary>
        /// ���̱���
        /// </summary>
        [Ignore]
        public string ShopCode
        {
            get { return shopCode; }
            set { shopCode = value; }
        }

        private string partNumberandshopIdStr;
        /// <summary>
        /// prod+Rmmd+ShopCode
        /// </summary>
        [Ignore]
        public string PartNumberandshopIdStr
        {
            get { return partNumberandshopIdStr; }
            set { partNumberandshopIdStr = value; }
        }
		#endregion
	}
}
