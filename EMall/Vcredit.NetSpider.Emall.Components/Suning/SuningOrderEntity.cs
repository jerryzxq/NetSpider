using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("Suning_Order")]
    //[Schema("dbo")]
	public class SuningOrderEntity
	{
	
		public SuningOrderEntity() { }


		#region Attributes
      
		private long iD;
       
     [AutoIncrement]
        /// <summary>
        /// 编号
        /// </summary>
		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private string orderNo;
       

        /// <summary>
        /// 订单号
        /// </summary>
		public string OrderNo
		{
			get { return orderNo; }
			set { orderNo = value; }
		}
      
		private DateTime? orderTime;
       

        /// <summary>
        /// 订单时间
        /// </summary>
		public DateTime? OrderTime
		{
			get { return orderTime; }
			set { orderTime = value; }
		}
        private string seller;

        /// <summary>
        /// 对方
        /// </summary>
        public string Seller
        {
            get { return seller; }
            set { seller = value; }
        }
		private string orderType;
       

        /// <summary>
        /// 订单类别
        /// </summary>
		public string OrderType
		{
			get { return orderType; }
			set { orderType = value; }
		}
      
		private string orderStatus;
       

        /// <summary>
        /// 订单状态
        /// </summary>
		public string OrderStatus
		{
			get { return orderStatus; }
			set { orderStatus = value; }
		}
      
		private string payType;
       

        /// <summary>
        /// 支付方式
        /// </summary>
		public string PayType
		{
			get { return payType; }
			set { payType = value; }
		}
      
		private decimal? totalAmount;
       

        /// <summary>
        /// 订单总额
        /// </summary>
		public decimal? TotalAmount
		{
			get { return totalAmount; }
			set { totalAmount = value; }
		}
      
		private decimal? payAmount;
       

        /// <summary>
        /// 实付总额
        /// </summary>
		public decimal? PayAmount
		{
			get { return payAmount; }
			set { payAmount = value; }
		}
      
		private string receiver;
       

        /// <summary>
        /// 收货人姓名
        /// </summary>
		public string Receiver
		{
			get { return receiver; }
			set { receiver = value; }
		}
      
		private string telephone;
       

        /// <summary>
        /// 收货人电话
        /// </summary>
		public string Telephone
		{
			get { return telephone; }
			set { telephone = value; }
		}
      
		private string adress;
       

        /// <summary>
        /// 收货人地址
        /// </summary>
		public string Adress
		{
			get { return adress; }
			set { adress = value; }
		}
      
		private string invoiceHead;
       

        /// <summary>
        /// 发票抬头
        /// </summary>
		public string InvoiceHead
		{
			get { return invoiceHead; }
			set { invoiceHead = value; }
		}
      
		private string invoiceContent;
       

        /// <summary>
        /// 发票内容
        /// </summary>
		public string InvoiceContent
		{
			get { return invoiceContent; }
			set { invoiceContent = value; }
		}
      
		private decimal? freight;
       

        /// <summary>
        /// 运费
        /// </summary>
		public decimal? Freight
		{
			get { return freight; }
			set { freight = value; }
		}
      
		private string logistics;
       

        /// <summary>
        /// 物流
        /// </summary>
		public string Logistics
		{
			get { return logistics; }
			set { logistics = value; }
		}
      
		private string receivingMode;
       

        /// <summary>
        /// 收货方式
        /// </summary>
		public string ReceivingMode
		{
			get { return receivingMode; }
			set { receivingMode = value; }
		} 
		private string coupon;
       

        /// <summary>
        /// 卡券/优惠
        /// </summary>
		public string Coupon
		{
			get { return coupon; }
			set { coupon = value; }
		}

        private decimal? goodsAmount;
        /// <summary>
        /// 合并订单产品总金额
        /// </summary>
        public decimal? GoodsAmount
        {
            get { return goodsAmount; }
            set { goodsAmount = value; }
        } 
		private long? userId;
       

        /// <summary>
        /// 用户编号
        /// </summary>
		public long? UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private string accountName;
       

        /// <summary>
        /// 用户账号
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private DateTime? createTime;
       

        /// <summary>
        /// 创建时间
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}

        /// <summary>
        /// 订单产品
        /// </summary> 
        private List<SuningGoodsEntity> _OrderGoods = new List<SuningGoodsEntity>();
        [Ignore]
        public List<SuningGoodsEntity> OrderGoods
        {
            get { return _OrderGoods; }
            set { _OrderGoods = value; }

        }

        /// <summary>
        /// 订单物流
        /// </summary> 
        private List<SuningLogisticsDetailEntity> _OrderLogistics = new List<SuningLogisticsDetailEntity>();
        [Ignore]
        public List<SuningLogisticsDetailEntity> OrderLogistics
        {
            get { return _OrderLogistics; }
            set { _OrderLogistics = value; }

        } 
		#endregion

	}
}
