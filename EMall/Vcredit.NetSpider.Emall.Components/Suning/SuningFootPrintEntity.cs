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
        /// 编号
        /// </summary>
        [AutoIncrement]
 		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private string categoryName  ;
        /// <summary>
        /// 分类名称
        /// </summary>
		public string CategoryName
		{
			get { return categoryName; }
			set { categoryName = value; }
		}
        private int? categoryCount;

        /// <summary>
        /// 改分类的数量
        /// </summary>
        public int? CategoryCount
        {
            get { return categoryCount; }
            set { categoryCount = value; }
        } 
		private string productName  ;
        /// <summary>
        /// 产品名称
        /// </summary>
		public string ProductName
		{
			get { return productName; }
			set { productName = value; }
		}
      
		private string price  ;
        /// <summary>
        /// 价格
        /// </summary>
		public string Price
		{
			get { return price; }
			set { price = value; }
		}
        private string snPrice;
        /// <summary>
        /// 苏宁价格
        /// </summary>
        public string SnPrice
        {
            get { return snPrice; }
            set { snPrice = value; }
        }
        private string visitTime;
        /// <summary>
        /// 浏览时间
        /// </summary>
		public string VisitTime
		{
			get { return visitTime; }
			set { visitTime = value; }
		}

        private int? fvaoriteNumber;
        /// <summary>
        /// 收藏人数
        /// </summary>
        public int? FvaoriteNumber
		{
            get { return fvaoriteNumber; }
            set { fvaoriteNumber = value; }
		}
        private int? isPromotion;
        /// <summary>
        /// 是否促销
        /// </summary>
        public int? IsPromotion
        {
            get { return isPromotion; }
            set { isPromotion = value; }
        }
		private long? userId  ;
        /// <summary>
        /// 客户编号
        /// </summary>
		public long? UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private string accountName  ;
        /// <summary>
        /// 客户账号
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private DateTime createTime = DateTime.Now ;
        /// <summary>
        /// 该记录创建时间
        /// </summary>
		public DateTime CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}

        private string cmmdtyCode;

        /// <summary>
        /// 产品记录码
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
