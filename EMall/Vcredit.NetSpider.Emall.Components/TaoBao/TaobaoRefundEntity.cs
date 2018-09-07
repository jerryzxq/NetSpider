using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.TaoBao
{
	[Alias("taobao_refund")]
	public class TaobaoRefundEntity
	{
		public TaobaoRefundEntity() { }

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
      
		private string refundNumber  ;
        /// <summary>
        /// 退款编号
        /// </summary>
		public string RefundNumber
		{
			get { return refundNumber; }
			set { refundNumber = value; }
		}
      
		private string orderNumber  ;
        /// <summary>
        /// 订单编号
        /// </summary>
		public string OrderNumber
		{
			get { return orderNumber; }
			set { orderNumber = value; }
		}
      
		private string productName  ;
        /// <summary>
        /// 商品名称
        /// </summary>
		public string ProductName
		{
			get { return productName; }
			set { productName = value; }
		}
      
		private decimal refundAmount  ;
        /// <summary>
        /// 退款金额
        /// </summary>
		public decimal RefundAmount
		{
			get { return refundAmount; }
			set { refundAmount = value; }
		}
      
		private decimal tradeAmount  ;
        /// <summary>
        /// 交易金额
        /// </summary>
		public decimal TradeAmount
		{
			get { return tradeAmount; }
			set { tradeAmount = value; }
		}
      
		private DateTime? applyTime  ;
        /// <summary>
        /// 申请时间
        /// </summary>
		public DateTime? ApplyTime
		{
			get { return applyTime; }
			set { applyTime = value; }
		}
      
		private DateTime? outTime  ;
        /// <summary>
        /// 超时时间
        /// </summary>
		public DateTime? OutTime
		{
			get { return outTime; }
			set { outTime = value; }
		}
      
		private string status  ;
        /// <summary>
        /// 退款状态
        /// </summary>
		public string Status
		{
			get { return status; }
			set { status = value; }
		}
      
		private string reason  ;
        /// <summary>
        /// 退款原因
        /// </summary>
		public string Reason
		{
			get { return reason; }
			set { reason = value; }
		}
      
		private string detailReason  ;
        /// <summary>
        /// 详细原因
        /// </summary>
		public string DetailReason
		{
			get { return detailReason; }
			set { detailReason = value; }
		}
      
		private string refundType  ;
        /// <summary>
        /// 退款类型
        /// </summary>
		public string RefundType
		{
			get { return refundType; }
			set { refundType = value; }
		}
      
		private bool? isInterpose  ;
        /// <summary>
        /// 小二介入
        /// </summary>
		public bool? IsInterpose
		{
			get { return isInterpose; }
			set { isInterpose = value; }
		}
      
		private string paidStatus  ;
        /// <summary>
        /// 垫付状态
        /// </summary>
		public string PaidStatus
		{
			get { return paidStatus; }
			set { paidStatus = value; }
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
