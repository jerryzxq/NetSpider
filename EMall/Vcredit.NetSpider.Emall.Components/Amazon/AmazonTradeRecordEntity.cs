using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("amazon_traderecord")]
	public class AmazonTradeRecordEntity
	{
		public AmazonTradeRecordEntity() { }

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
      
		private DateTime? tradeTime  ;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? TradeTime
		{
			get { return tradeTime; }
			set { tradeTime = value; }
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
      
		private decimal? tradeAmount  ;
        /// <summary>
        /// 
        /// </summary>
		public decimal? TradeAmount
		{
			get { return tradeAmount; }
			set { tradeAmount = value; }
		}
      
		private string tradeType  ;
        /// <summary>
        /// 
        /// </summary>
		public string TradeType
		{
			get { return tradeType; }
			set { tradeType = value; }
		}
      
		private int? billState  ;
        /// <summary>
        /// 
        /// </summary>
		public int? BillState
		{
			get { return billState; }
			set { billState = value; }
		}
      
		private string paymentMethod  ;
        /// <summary>
        /// 
        /// </summary>
		public string PaymentMethod
		{
			get { return paymentMethod; }
			set { paymentMethod = value; }
		}
      
		private string tradeStatus  ;
        /// <summary>
        /// 
        /// </summary>
		public string TradeStatus
		{
			get { return tradeStatus; }
			set { tradeStatus = value; }
		}
      
		private string tradeDesc  ;
        /// <summary>
        /// 
        /// </summary>
		public string TradeDesc
		{
			get { return tradeDesc; }
			set { tradeDesc = value; }
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
