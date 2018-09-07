using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using System.Linq;
using System.Text;
using Vcredit.Common.Ext;
using Newtonsoft.Json;
namespace Vcredit.NetSpider.Emall.Entity
{
   

    [Alias("JD_Order")]
    public class OrderEntity : BaseEntity  //订单详情
    {
        public OrderEntity()
        {
            IsRetreatReturn = false;
            Logistics = new LogisticsEntity();
        }
        [Ignore]
        /// <summary>
        /// 物流信息
        /// </summary>
        public LogisticsEntity Logistics { get; set; }

        private List<GoodsEntity> goodsList = new List<GoodsEntity>();
        /// <summary>
        /// 该订单下的商品集合
        /// </summary>

        [Ignore]

        public List<GoodsEntity> GoodsList
        {
            get { return goodsList; }
            set { goodsList = value; }
        }
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 订单时间
        /// </summary>

        public DateTime? OrderTime { get; set; }

        /// <summary>
        /// 商家类别
        /// </summary>

        public string OrderType { get; set; }

        /// <summary>
        /// 收货人
        /// </summary>
        public string Receiver { get; set; }

        /// <summary>
        /// 收货地址
        /// </summary>
        public string ReceiveAddress { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>

        public string Telephone { get; set; }

        /// <summary>
        /// 支付类型
        /// </summary>

        public string PayType { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 真实付款金额
        /// </summary>
        public decimal? PayAmount { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public string OrderStatus { get; set; }



        /// <summary>
        /// 发票类型
        /// </summary>

        public string InvoiceType { get; set; }

        /// <summary>
        /// 发票抬头
        /// </summary>
        public string InvoiceHead { get; set; }


        /// <summary>
        /// 发票内容
        /// </summary>

        public string InvoiceDetail { get; set; }
        /// <summary>
        /// 是否返修退换过
        /// </summary>

        public bool IsRetreatReturn { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>

        public string AccountName { get; set; }
        [Ignore]
        public string  carrier { get; set; }
        [Ignore]
        public string carrierMobile { get; set; }
        [Ignore]
        /// <summary>
        /// 送货方式
        /// </summary>
        public string deliveryMode { get; set; }


    }
}
