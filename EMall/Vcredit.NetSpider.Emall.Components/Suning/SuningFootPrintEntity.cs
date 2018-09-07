using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("Suning_FootPrint")] 
	public class SuningFootPrintEntity
	{
		public SuningFootPrintEntity() { }

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
      
		private string categoryName  ;
        /// <summary>
        /// ��������
        /// </summary>
		public string CategoryName
		{
			get { return categoryName; }
			set { categoryName = value; }
		}
        private int? categoryCount;

        /// <summary>
        /// �ķ��������
        /// </summary>
        public int? CategoryCount
        {
            get { return categoryCount; }
            set { categoryCount = value; }
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
      
		private string price  ;
        /// <summary>
        /// �۸�
        /// </summary>
		public string Price
		{
			get { return price; }
			set { price = value; }
		}
        private string snPrice;
        /// <summary>
        /// �����۸�
        /// </summary>
        public string SnPrice
        {
            get { return snPrice; }
            set { snPrice = value; }
        }
        private string visitTime;
        /// <summary>
        /// ���ʱ��
        /// </summary>
		public string VisitTime
		{
			get { return visitTime; }
			set { visitTime = value; }
		}

        private int? fvaoriteNumber;
        /// <summary>
        /// �ղ�����
        /// </summary>
        public int? FvaoriteNumber
		{
            get { return fvaoriteNumber; }
            set { fvaoriteNumber = value; }
		}
        private int? isPromotion;
        /// <summary>
        /// �Ƿ����
        /// </summary>
        public int? IsPromotion
        {
            get { return isPromotion; }
            set { isPromotion = value; }
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
      
		private DateTime createTime = DateTime.Now ;
        /// <summary>
        /// �ü�¼����ʱ��
        /// </summary>
		public DateTime CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}

        private string cmmdtyCode;

        /// <summary>
        /// ��Ʒ��¼��
        /// </summary>
        [Ignore]
        public string CmmdtyCode
        {
            get { return cmmdtyCode; }
            set { cmmdtyCode = value; }
        }
		#endregion
	}
}
