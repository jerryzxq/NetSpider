using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Emall.Entity.TaoBao.DomainEntity
{
    /// <summary>
    /// 订单详情
    /// </summary>
    public class OrderDomainModel
    {
        private string orderStatus;
        /// <summary>
        /// 订单交易状态
        /// </summary>
        public string OrderStatus
        {
            get { return orderStatus; }
            set { orderStatus = value; }
        }


        private string _orderNo = string.Empty;
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo
        {
            get { return _orderNo; }
            set { _orderNo = value; }
        }

        private string alipayNumber = string.Empty;
        /// <summary>
        /// 支付宝交易号
        /// </summary>
        public string AliPayNumber
        {
            get { return alipayNumber; }
            set { alipayNumber = value; }
        }

        private int _goodsCount;
        /// <summary>
        /// 订单商品数量
        /// </summary>
        public int GoodsCount
        {
            get { return _goodsCount; }
            set { _goodsCount = value; }
        }

        private DateTime? orderTime;
        /// <summary>
        /// 创建时间
        /// </summary>
		public DateTime? OrderTime
        {
            get { return orderTime; }
            set { orderTime = value; }
        }

        private DateTime? shipmentTime;
        /// <summary>
        /// 发货时间
        /// </summary>
		public DateTime? ShipmentTime
        {
            get { return shipmentTime; }
            set { shipmentTime = value; }
        }

        private DateTime? payTime;
        /// <summary>
        /// 付款时间
        /// </summary>
		public DateTime? PayTime
        {
            get { return payTime; }
            set { payTime = value; }
        }

        private DateTime? dealTime;
        /// <summary>
        /// 成交时间
        /// </summary>
		public DateTime? DealTime
        {
            get { return dealTime; }
            set { dealTime = value; }
        }

        private DateTime? confirmTime;
        /// <summary>
        /// 确认时间
        /// </summary>
		public DateTime? ConfirmTime
        {
            get { return confirmTime; }
            set { confirmTime = value; }
        }

        private decimal? _totalAmount;
        /// <summary>
        /// 商品总价
        /// </summary>
        public decimal? TotalAmount
        {
            get { return _totalAmount; }
            set { _totalAmount = value; }
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

        private decimal? _payAmount;
        /// <summary>
        /// 实际付款
        /// </summary>
        public decimal? PayAmount
        {
            get { return _payAmount; }
            set { _payAmount = value; }
        }

        private string _address = string.Empty;
        /// <summary>
        /// 地址
        /// </summary>
        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        private List<GoodsDomainModel> products = new List<GoodsDomainModel>();
        /// <summary>
        /// 商品集合
        /// </summary>
        public List<GoodsDomainModel> Products
        {
            get { return products; }
            set { products = value; }
        }

        private List<LogisticsDomainModel> logisticses = new List<LogisticsDomainModel>();
        /// <summary>
        /// 物流集合
        /// </summary>
        public List<LogisticsDomainModel> Logisticses
        {
            get { return logisticses; }
            set { logisticses = value; }
        }
    }

    /// <summary>
    /// 商品
    /// </summary>
    public class GoodsDomainModel
    {
        private string _orderNo = string.Empty;
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo
        {
            get { return _orderNo; }
            set { _orderNo = value; }
        }

        private string _goodsName = string.Empty;
        /// <summary>
        /// 商品品牌
        /// </summary>
        public string GoodsName
        {
            get { return _goodsName; }
            set { _goodsName = value; }
        }

        private string _goodsProperty = string.Empty;
        /// <summary>
        /// 商品属性
        /// </summary>
        public string GoodsProperty
        {
            get { return _goodsProperty; }
            set { _goodsProperty = value; }
        }

        private int? _goodsCount;
        /// <summary>
        /// 每个商品购买数量
        /// </summary>
        public int? GoodsCount
        {
            get { return _goodsCount; }
            set { _goodsCount = value; }
        }

        private decimal? price;
        /// <summary>
        /// 商品单价
        /// </summary>
		public decimal? Price
        {
            get { return price; }
            set { price = value; }
        }

        private string _status = string.Empty;
        /// <summary>
        /// 状态
        /// </summary>
		public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        private string _service = string.Empty;
        /// <summary>
        /// 服务
        /// </summary>
		public string Service
        {
            get { return _service; }
            set { _service = value; }
        }

        private string _discount = string.Empty;
        /// <summary>
        /// 优惠
        /// </summary>
		public string Discount
        {
            get { return _discount; }
            set { _discount = value; }
        }
    }

    /// <summary>
    /// 物流
    /// </summary>
    public class LogisticsDomainModel
    {
        private string orderNo = string.Empty;
        /// <summary>
        /// 订单编号
        /// </summary>
		public string OrderNo
        {
            get { return orderNo; }
            set { orderNo = value; }
        }

        private string _logisticsNo = string.Empty;
        /// <summary>
        /// 货运单号
        /// </summary>
        public string LogisticsNo
        {
            get { return _logisticsNo; }
            set { _logisticsNo = value; }
        }

        private string _logisticsType = string.Empty;
        /// <summary>
        /// 发货方式
        /// </summary>
		public string LogisticsType
        {
            get { return _logisticsType; }
            set { _logisticsType = value; }
        }

        private string _logisticsPhone = string.Empty;
        /// <summary>
        /// 承运人电话
        /// </summary>
		public string LogisticsPhone
        {
            get { return _logisticsPhone; }
            set { _logisticsPhone = value; }
        }

        private string _logisticsCompany = string.Empty;
        /// <summary>
        /// 承运人(公司)
        /// </summary>
		public string LogisticsCompany
        {
            get { return _logisticsCompany; }
            set { _logisticsCompany = value; }
        }

        private string _address = string.Empty;
        /// <summary>
        /// 地址
        /// </summary>
        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        private string _receiver = string.Empty;
        /// <summary>
        /// 姓名
        /// </summary>
        public string Receiver
        {
            get { return _receiver; }
            set { _receiver = value; }
        }

        private string _telephone = string.Empty;
        /// <summary>
        /// 电话
        /// </summary>
        public string Telephone
        {
            get { return _telephone; }
            set { _telephone = value; }
        }
    }
}
