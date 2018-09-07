using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Alipay_BillOrder")]
    
	public class AlipayBillOrderEntity
	{
	
		public AlipayBillOrderEntity() { }


		#region Attributes
      
		private long iD;
       
     [AutoIncrement]
        /// <summary>
        /// 
        /// </summary>
		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private long? billID;
       

        /// <summary>
        /// 
        /// </summary>
		public long? BillID
		{
			get { return billID; }
			set { billID = value; }
		}
      
		private string orderNO;
       

        /// <summary>
        /// 订单编号
        /// </summary>
		public string OrderNO
		{
			get { return orderNO; }
			set { orderNO = value; }
		}
      
		private DateTime? closingTime;
       

        /// <summary>
        /// 成交时间
        /// </summary>
		public DateTime? ClosingTime
		{
			get { return closingTime; }
			set { closingTime = value; }
		}
      
		private DateTime? paymentTime;

        private DateTime? shipmentTime;
        /// <summary>
        /// 发货时间
        /// </summary>
        public DateTime? ShipmentTime
        {
            get { return shipmentTime; }
            set { shipmentTime = value; }
        }

        /// <summary>
        /// 付款时间
        /// </summary>
		public DateTime? PaymentTime
		{
			get { return paymentTime; }
			set { paymentTime = value; }
		}
      
		private DateTime? confirmationTime;
       

        /// <summary>
        /// 确认时间
        /// </summary>
		public DateTime? ConfirmationTime
		{
			get { return confirmationTime; }
			set { confirmationTime = value; }
		}
      
		private DateTime? deliveryTime;
       

        /// <summary>
        /// 发货时间
        /// </summary>
		public DateTime? DeliveryTime
		{
			get { return deliveryTime; }
			set { deliveryTime = value; }
		}
      
		private decimal? realPayment;
       

        /// <summary>
        /// 实付款
        /// </summary>
		public decimal? RealPayment
		{
			get { return realPayment; }
			set { realPayment = value; }
		}
      
		private decimal? freightage;
       

        /// <summary>
        /// 运费
        /// </summary>
		public decimal? Freightage
		{
			get { return freightage; }
			set { freightage = value; }
		}

        private decimal? totalAmount;
       

        /// <summary>
        /// 
        /// </summary>
        public decimal? TotalAmount 
		{
            get { return totalAmount; }
            set { totalAmount = value; }
		}

        private string orderStatus;
       

        /// <summary>
        /// 状态
        /// </summary>
        public string OrderStatus
		{
            get { return orderStatus; }
            set { orderStatus = value; }
		}
      
		private DateTime? createTime;
       

        /// <summary>
        /// 
        /// </summary>
		public DateTime? CreateTime
		{
            get
            {

                if (createTime == null)
                {
                    return DateTime.Now;
                }
                else
                {
                    return createTime;
                }
            }
			set { createTime = value; }
		}

        public List<AlipayBillOrderDetailEntity> _BillOrderDetail = new List<AlipayBillOrderDetailEntity>();

        [Ignore]
        public List<AlipayBillOrderDetailEntity> BillOrderDetail
        {
            get { return _BillOrderDetail; }
            set { _BillOrderDetail = value; }

        }

        public List<AlipayBillOrderLogisticsEntity> _BillOrderLogistics = new List<AlipayBillOrderLogisticsEntity>();

        [Ignore]
        public List<AlipayBillOrderLogisticsEntity> BillOrderLogistics
        {
            get { return _BillOrderLogistics; }
            set { _BillOrderLogistics = value; }

        }
		#endregion

	}
}
