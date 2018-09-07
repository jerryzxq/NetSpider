using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Alipay_BillOrderDetail")]
    
    public class AlipayBillOrderDetailEntity
    {

        public AlipayBillOrderDetailEntity() { }


        #region Attributes

        private long iD;

        [AutoIncrement]
        /// <summary>
        /// 主键
        /// </summary>
        public long ID
        {
            get { return iD; }
            set { iD = value; }
        }

        private long? orderID;


        /// <summary>
        /// 订单编号
        /// </summary>
        public long? OrderID
        {
            get { return orderID; }
            set { orderID = value; }
        }

        private string goodsName;


        /// <summary>
        /// 商品
        /// </summary>
        public string GoodsName
        {
            get { return goodsName; }
            set { goodsName = value; }
        }

        private string goodProperty;


        /// <summary>
        /// 商品属性
        /// </summary>
        public string GoodProperty
        {
            get { return goodProperty; }
            set { goodProperty = value; }
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



        private string service;
        /// <summary>
        /// 服务
        /// </summary>
        public string Service
        {
            get { return service; }
            set { service = value; }
        }

        private decimal? price;


        /// <summary>
        /// 单价(元)
        /// </summary>
        public decimal? Price
        {
            get { return price; }
            set { price = value; }
        }

        private int? goodCount;


        /// <summary>
        /// 数量
        /// </summary>
        public int? GoodCount 
        {
            get { return goodCount; }
            set { goodCount = value; }
        }

        private string discount;


        /// <summary>
        /// 优惠
        /// </summary>
        public string Discount
        {
            get { return discount; }
            set { discount = value; }
        }

     
        private DateTime? createTime;


        /// <summary>
        /// 创建时间
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
        #endregion

    }
}
