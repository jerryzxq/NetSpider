using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("suning_efubaotrade")]
	public class SuningEFuBaoTradeEntity
	{
		public SuningEFuBaoTradeEntity() { }

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
      
		private DateTime? tradeTime  ;
        /// <summary>
        /// 交易时间
        /// </summary>
		public DateTime? TradeTime
		{
			get { return tradeTime; }
			set { tradeTime = value; }
		}
      
		private string orderNo  ;
        /// <summary>
        /// 订单编号
        /// </summary>
		public string OrderNo
		{
			get { return orderNo; }
			set { orderNo = value; }
		}
      
		private string tradeNo  ;
        /// <summary>
        /// 交易号
        /// </summary>
		public string TradeNo
		{
			get { return tradeNo; }
			set { tradeNo = value; }
		}
      
		private string tradeType  ;
        /// <summary>
        /// 交易类型
        /// </summary>
		public string TradeType
		{
			get { return tradeType; }
			set { tradeType = value; }
		}
      
		private string tradeName  ;
        /// <summary>
        /// 交易名称
        /// </summary>
		public string TradeName
		{
			get { return tradeName; }
			set { tradeName = value; }
		}
      
		private string trader  ;
        /// <summary>
        /// 对方
        /// </summary>
		public string Trader
		{
			get { return trader; }
			set { trader = value; }
		}
      
		private decimal? tradeAmount  ;
        /// <summary>
        /// 交易金额
        /// </summary>
		public decimal? TradeAmount
		{
			get { return tradeAmount; }
			set { tradeAmount = value; }
		}
      
		private string tradeStatus  ;
        /// <summary>
        /// 交易状态
        /// </summary>
		public string TradeStatus
		{
			get { return tradeStatus; }
			set { tradeStatus = value; }
		}
      
		private string paymentMethod  ;
        /// <summary>
        /// 交易方式
        /// </summary>
		public string PaymentMethod
		{
			get { return paymentMethod; }
			set { paymentMethod = value; }
		}
      
		private string paymentDesc  ;
        /// <summary>
        /// 交易描述
        /// </summary>
		public string PaymentDesc
		{
			get { return paymentDesc; }
			set { paymentDesc = value; }
		}
      
		private decimal? orderAmount  ;
        /// <summary>
        /// 订单金额
        /// </summary>
		public decimal? OrderAmount
		{
			get { return orderAmount; }
			set { orderAmount = value; }
		}
      
		private decimal? privilege  ;
        /// <summary>
        /// 优惠
        /// </summary>
		public decimal? Privilege
		{
			get { return privilege; }
			set { privilege = value; }
		}
      
		private decimal? payableAmount  ;
        /// <summary>
        /// 实际支付
        /// </summary>
		public decimal? PayableAmount
		{
			get { return payableAmount; }
			set { payableAmount = value; }
		}
      
		private string finalTradeStatus  ;
        /// <summary>
        /// 最终交易状态
        /// </summary>
		public string FinalTradeStatus
		{
			get { return finalTradeStatus; }
			set { finalTradeStatus = value; }
		}
      
		private string refundNo  ;
        /// <summary>
        /// 退款编号
        /// </summary>
		public string RefundNo
		{
			get { return refundNo; }
			set { refundNo = value; }
		}
      
		private DateTime? refundTime  ;
        /// <summary>
        /// 退款时间
        /// </summary>
		public DateTime? RefundTime
		{
			get { return refundTime; }
			set { refundTime = value; }
		}
      
		private decimal? refundAmount  ;
        /// <summary>
        /// 退款金额
        /// </summary>
		public decimal? RefundAmount
		{
			get { return refundAmount; }
			set { refundAmount = value; }
		}
      
		private string refundMethod  ;
        /// <summary>
        /// 退款方式
        /// </summary>
		public string RefundMethod
		{
			get { return refundMethod; }
			set { refundMethod = value; }
		}
      
		private string refundDesc  ;
        /// <summary>
        /// 退款描述
        /// </summary>
		public string RefundDesc
		{
			get { return refundDesc; }
			set { refundDesc = value; }
		}
      
		private string refundRemark  ;
        /// <summary>
        /// 退款备注
        /// </summary>
		public string RefundRemark
		{
			get { return refundRemark; }
			set { refundRemark = value; }
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
      
		private DateTime? createTime = DateTime.Now ;
        /// <summary>
        /// 创建时间
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}

        private List<SuningEFuBaoTradeProcessEntity> _TradeProcess = new List<SuningEFuBaoTradeProcessEntity>();
        [Ignore]
        public List<SuningEFuBaoTradeProcessEntity> TradeProcess
        {
            get { return _TradeProcess; }
            set { _TradeProcess = value; }
        }

		#endregion
	}
}
