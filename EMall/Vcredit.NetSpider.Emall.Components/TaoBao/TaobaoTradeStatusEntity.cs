using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.TaoBao
{
	[Alias("taobao_tradestatus")]
	public class TaobaoTradeStatusEntity
	{
		public TaobaoTradeStatusEntity() { }

		#region Attributes
      
		private long id  ;
        /// <summary>
        /// 
        /// </summary>
        [AutoIncrement]
 		public long Id
		{
			get { return id; }
			set { id = value; }
		}
      
		private long userId  ;
        /// <summary>
        /// 基础信息编号
        /// </summary>
		public long UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private int tradeSucessCount  ;
        /// <summary>
        /// 交易成功笔数
        /// </summary>
		public int TradeSucessCount
		{
			get { return tradeSucessCount; }
			set { tradeSucessCount = value; }
		}
      
		private decimal tradeSuccessAmount  ;
        /// <summary>
        /// 交易成功金额
        /// </summary>
		public decimal TradeSuccessAmount
		{
			get { return tradeSuccessAmount; }
			set { tradeSuccessAmount = value; }
		}
      
		private int tradeCloseCount  ;
        /// <summary>
        /// 交易关闭笔数
        /// </summary>
		public int TradeCloseCount
		{
			get { return tradeCloseCount; }
			set { tradeCloseCount = value; }
		}
      
		private decimal tradeCloseAmount  ;
        /// <summary>
        /// 交易关闭金额
        /// </summary>
		public decimal TradeCloseAmount
		{
			get { return tradeCloseAmount; }
			set { tradeCloseAmount = value; }
		}
      
		private int refundReturnCount  ;
        /// <summary>
        /// 申请退货退款笔数
        /// </summary>
		public int RefundReturnCount
		{
			get { return refundReturnCount; }
			set { refundReturnCount = value; }
		}
      
		private decimal refundReturnAmount  ;
        /// <summary>
        /// 申请退货退款金额
        /// </summary>
		public decimal RefundReturnAmount
		{
			get { return refundReturnAmount; }
			set { refundReturnAmount = value; }
		}
      
		private int buyerPayCount  ;
        /// <summary>
        /// 买家已付款笔数
        /// </summary>
		public int BuyerPayCount
		{
			get { return buyerPayCount; }
			set { buyerPayCount = value; }
		}
      
		private decimal buyerPayAmount  ;
        /// <summary>
        /// 买家已付款金额
        /// </summary>
		public decimal BuyerPayAmount
		{
			get { return buyerPayAmount; }
			set { buyerPayAmount = value; }
		}
      
		private int sellerDeliveryCount  ;
        /// <summary>
        /// 卖家已发货笔数
        /// </summary>
		public int SellerDeliveryCount
		{
			get { return sellerDeliveryCount; }
			set { sellerDeliveryCount = value; }
		}
      
		private decimal sellerDeliveryAmount  ;
        /// <summary>
        /// 卖家已发货金额
        /// </summary>
		public decimal SellerDeliveryAmount
		{
			get { return sellerDeliveryAmount; }
			set { sellerDeliveryAmount = value; }
		}
      
		private int waitBuyerPayCount  ;
        /// <summary>
        /// 等待买家付款笔数
        /// </summary>
		public int WaitBuyerPayCount
		{
			get { return waitBuyerPayCount; }
			set { waitBuyerPayCount = value; }
		}
      
		private decimal waitBuyerPayAmount  ;
        /// <summary>
        /// 等待买家付款金额
        /// </summary>
		public decimal WaitBuyerPayAmount
		{
			get { return waitBuyerPayAmount; }
			set { waitBuyerPayAmount = value; }
		}
      
		private int notDeliveryCount  ;
        /// <summary>
        /// 未发货笔数
        /// </summary>
		public int NotDeliveryCount
		{
			get { return notDeliveryCount; }
			set { notDeliveryCount = value; }
		}
      
		private decimal notDeliveryAmount  ;
        /// <summary>
        /// 未发货金额
        /// </summary>
		public decimal NotDeliveryAmount
		{
			get { return notDeliveryAmount; }
			set { notDeliveryAmount = value; }
		}
      
		private int totalTradeCount  ;
        /// <summary>
        /// 总交易订单笔数
        /// </summary>
		public int TotalTradeCount
		{
			get { return totalTradeCount; }
			set { totalTradeCount = value; }
		}
      
		private decimal totalTradeAmount  ;
        /// <summary>
        /// 总交易订单金额
        /// </summary>
		public decimal TotalTradeAmount
		{
			get { return totalTradeAmount; }
			set { totalTradeAmount = value; }
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
      
		private string createUser  ;
        /// <summary>
        /// 
        /// </summary>
		public string CreateUser
		{
			get { return createUser; }
			set { createUser = value; }
		}
      
		private DateTime? updateTime  ;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? UpdateTime
		{
			get { return updateTime; }
			set { updateTime = value; }
		}
      
		private string updateUser  ;
        /// <summary>
        /// 
        /// </summary>
		public string UpdateUser
		{
			get { return updateUser; }
			set { updateUser = value; }
		}
		#endregion
	}
}
