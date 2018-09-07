using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("amazon_order")]
	public class AmazonOrderEntity
	{
		public AmazonOrderEntity() { }

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
      
		private string orderNo  ;
        /// <summary>
        /// 订单号
        /// </summary>
		public string OrderNo
		{
			get { return orderNo; }
			set { orderNo = value; }
		}
      
		private DateTime? orderTime  ;
        /// <summary>
        /// 下单时间
        /// </summary>
		public DateTime? OrderTime
		{
			get { return orderTime; }
			set { orderTime = value; }
		}
      
		private string orderStatus  ;
        /// <summary>
        /// 订单状态
        /// </summary>
		public string OrderStatus
		{
			get { return orderStatus; }
			set { orderStatus = value; }
		}
      
		private string orderStatusDesc  ;
        /// <summary>
        /// 订单状态描述
        /// </summary>
		public string OrderStatusDesc
		{
			get { return orderStatusDesc; }
			set { orderStatusDesc = value; }
		}
      
		private decimal? totalAmount  ;
        /// <summary>
        /// 商品总额
        /// </summary>
		public decimal? TotalAmount
		{
			get { return totalAmount; }
			set { totalAmount = value; }
		}
      
		private decimal? freightAge  ;
        /// <summary>
        /// 运费
        /// </summary>
		public decimal? FreightAge
		{
			get { return freightAge; }
			set { freightAge = value; }
		}
      
		private decimal? orderAmount  ;
        /// <summary>
        /// 订单总额
        /// </summary>
		public decimal? OrderAmount
		{
			get { return orderAmount; }
			set { orderAmount = value; }
		}
      
		private decimal? promotions  ;
        /// <summary>
        /// 优惠
        /// </summary>
		public decimal? Promotions
		{
			get { return promotions; }
			set { promotions = value; }
		}
      
		private decimal? payAmount  ;
        /// <summary>
        /// 支付总额
        /// </summary>
		public decimal? PayAmount
		{
			get { return payAmount; }
			set { payAmount = value; }
		}
      
		private string receiver  ;
        /// <summary>
        /// 收货人
        /// </summary>
		public string Receiver
		{
			get { return receiver; }
			set { receiver = value; }
		}
      
		private string province  ;
        /// <summary>
        /// 省
        /// </summary>
		public string Province
		{
			get { return province; }
			set { province = value; }
		}
      
		private string city  ;
        /// <summary>
        /// 市
        /// </summary>
		public string City
		{
			get { return city; }
			set { city = value; }
		}
      
		private string area  ;
        /// <summary>
        /// 区
        /// </summary>
		public string Area
		{
			get { return area; }
			set { area = value; }
		}
      
		private string zip  ;
        /// <summary>
        /// 邮编
        /// </summary>
		public string Zip
		{
			get { return zip; }
			set { zip = value; }
		}
      
		private string adress  ;
        /// <summary>
        /// 地址
        /// </summary>
		public string Adress
		{
			get { return adress; }
			set { adress = value; }
		}
        private string mobile;

        public string Mobile
        {
            get { return mobile; }
            set { mobile = value; }
        }

		private string payMethod  ;
        /// <summary>
        /// 支付方式
        /// </summary>
		public string PayMethod
		{
			get { return payMethod; }
			set { payMethod = value; }
		}
      
		private string deliverMethod  ;
        /// <summary>
        /// 配送方式
        /// </summary>
		public string DeliverMethod
		{
			get { return deliverMethod; }
			set { deliverMethod = value; }
		}
      
		private string deliverTime  ;
        /// <summary>
        /// 配送时间
        /// </summary>
		public string DeliverTime
		{
			get { return deliverTime; }
			set { deliverTime = value; }
		}
      
		private string deliverLike  ;
        /// <summary>
        /// 配送喜好
        /// </summary>
		public string DeliverLike
		{
			get { return deliverLike; }
			set { deliverLike = value; }
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
        /// 客户账号名
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private DateTime? createTime = DateTime.Now ;
        /// <summary>
        /// 时间
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}

        private List<AmazonGoodsEntity> _GoodsList = new List<AmazonGoodsEntity>();
        [Ignore]
        public List<AmazonGoodsEntity> GoodsList
        {
            get { return _GoodsList; }
            set { _GoodsList = value; }
        }
		#endregion
	}
}
