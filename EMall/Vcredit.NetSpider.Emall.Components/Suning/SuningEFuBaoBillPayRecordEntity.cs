using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("suning_efubaobillpayrecord")]
	public class SuningEFuBaoBillPayRecordEntity
	{
		public SuningEFuBaoBillPayRecordEntity() { }

		#region Attributes
      
		private long iD  ;
        /// <summary>
        /// 
        /// </summary>
        [AutoIncrement]
 		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private DateTime? billTime  ;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? BillTime
		{
			get { return billTime; }
			set { billTime = value; }
		}
      
		private string serialNo  ;
        /// <summary>
        /// 
        /// </summary>
		public string SerialNo
		{
			get { return serialNo; }
			set { serialNo = value; }
		}
      
		private string orderNo  ;
        /// <summary>
        /// 
        /// </summary>
		public string OrderNo
		{
			get { return orderNo; }
			set { orderNo = value; }
		}
      
		private string billType  ;
        /// <summary>
        /// 
        /// </summary>
		public string BillType
		{
			get { return billType; }
			set { billType = value; }
		}
      
		private string billName  ;
        /// <summary>
        /// 
        /// </summary>
		public string BillName
		{
			get { return billName; }
			set { billName = value; }
		}
      
		private string seller  ;
        /// <summary>
        /// 
        /// </summary>
		public string Seller
		{
			get { return seller; }
			set { seller = value; }
		}
      
		private decimal? amount  ;
        /// <summary>
        /// 
        /// </summary>
		public decimal? Amount
		{
			get { return amount; }
			set { amount = value; }
		}
      
		private decimal? balance  ;
        /// <summary>
        /// 
        /// </summary>
		public decimal? Balance
		{
			get { return balance; }
			set { balance = value; }
		}
      
		private int? tradeType  ;
        /// <summary>
        /// 
        /// </summary>
		public int? TradeType
		{
			get { return tradeType; }
			set { tradeType = value; }
		}
      
		private string payMethod  ;
        /// <summary>
        /// 
        /// </summary>
		public string PayMethod
		{
			get { return payMethod; }
			set { payMethod = value; }
		}
      
		private long? userId  ;
        /// <summary>
        /// 
        /// </summary>
		public long? UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private string accountName  ;
        /// <summary>
        /// 
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
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
