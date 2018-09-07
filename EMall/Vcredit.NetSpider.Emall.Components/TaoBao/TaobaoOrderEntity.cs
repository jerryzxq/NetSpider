using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.TaoBao
{
    [Alias("taobao_order")]
    public class TaobaoOrderEntity
    {
        public TaobaoOrderEntity() { }

        #region Attributes

        private long id;
        /// <summary>
        /// 
        /// </summary>
        [AutoIncrement]
        public long Id
        {
            get { return id; }
            set { id = value; }
        }

        private long userId;
        /// <summary>
        /// 基础信息编号
        /// </summary>
		public long UserId
        {
            get { return userId; }
            set { userId = value; }
        }

        private string accountName = string.Empty;
        /// <summary>
        /// 用户账号
        /// </summary>
		public string AccountName
        {
            get { return accountName; }
            set { accountName = value; }
        }

        private string orderNo = string.Empty;
        /// <summary>
        /// 订单编号
        /// </summary>
		public string OrderNo
        {
            get { return orderNo; }
            set { orderNo = value; }
        }

        private string orderType = string.Empty;
        /// <summary>
        /// 订单类型
        /// </summary>
		public string OrderType
        {
            get { return orderType; }
            set { orderType = value; }
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

        private string orderStatus = string.Empty;
        /// <summary>
        /// 订单状态
        /// </summary>
		public string OrderStatus
        {
            get { return orderStatus; }
            set { orderStatus = value; }
        }

        private string payType = string.Empty;
        /// <summary>
        /// 付款方式
        /// </summary>
		public string PayType
        {
            get { return payType; }
            set { payType = value; }
        }

        private decimal? totalAmount;
        /// <summary>
        /// 商品总价
        /// </summary>
		public decimal? TotalAmount
        {
            get { return totalAmount; }
            set { totalAmount = value; }
        }

        private decimal? payAmount;
        /// <summary>
        /// 实际付款
        /// </summary>
		public decimal? PayAmount
        {
            get { return payAmount; }
            set { payAmount = value; }
        }

        private int? _goodsCount;
        /// <summary>
        /// 订单商品数量
        /// </summary>
		public int? GoodsCount
        {
            get { return _goodsCount; }
            set { _goodsCount = value; }
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

        private string address = string.Empty;
        /// <summary>
        /// 收货人地址
        /// </summary>
		public string Address
        {
            get { return address; }
            set { address = value; }
        }

        private string receiver = string.Empty;
        /// <summary>
        /// 收货人姓名
        /// </summary>
		public string Receiver
        {
            get { return receiver; }
            set { receiver = value; }
        }

        private string telephone = string.Empty;
        /// <summary>
        /// 收货人电话
        /// </summary>
		public string Telephone
        {
            get { return telephone; }
            set { telephone = value; }
        }

        private DateTime? createTime = DateTime.Now;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? CreateTime
        {
            get { return createTime; }
            set { createTime = value; }
        }

        private string createUser;
        /// <summary>
        /// 
        /// </summary>
		public string CreateUser
        {
            get { return createUser; }
            set { createUser = value; }
        }

        private DateTime? updateTime;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? UpdateTime
        {
            get { return updateTime; }
            set { updateTime = value; }
        }

        private string updateUser;
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
