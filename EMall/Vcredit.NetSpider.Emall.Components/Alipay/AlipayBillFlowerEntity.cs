using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Alipay_BillFlower")]
    
    public class AlipayBillFlowerEntity
    {

        public AlipayBillFlowerEntity() { }


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
         
        private long baicID;


        /// <summary>
        /// 账户编号
        /// </summary>
        public long BaicID
        {
            get { return baicID; }
            set { baicID = value; }
        }

        private string accountName;


        /// <summary>
        /// 账户账号
        /// </summary>
        public string AccountName
        {
            get { return accountName; }
            set { accountName = value; }
        }

        private string flowerMonth;


        /// <summary>
        /// 账单月份
        /// </summary>
        public string FlowerMonth
        {
            get { return flowerMonth; }
            set { flowerMonth = value; }
        }

        private decimal? billAmount;


        /// <summary>
        /// 本期账单金额
        /// </summary>
        public decimal? BillAmount
        {
            get { return billAmount; }
            set { billAmount = value; }
        }

        private decimal? overdueInterest;


        /// <summary>
        /// 逾期利息
        /// </summary>
        public decimal? OverdueInterest
        {
            get { return overdueInterest; }
            set { overdueInterest = value; }
        }

        private decimal? amortizationAmount;


        /// <summary>
        /// 分期还款金额
        /// </summary>
        public decimal? AmortizationAmount
        {
            get { return amortizationAmount; }
            set { amortizationAmount = value; }
        }

        private decimal? hasAmount;


        /// <summary>
        /// 本期已还金额
        /// </summary>
        public decimal? HasAmount
        {
            get { return hasAmount; }
            set { hasAmount = value; }
        }
        //
        private string orderStatus;

        /// <summary>
        /// 订单状态
        /// </summary>
        public string OrderStatus
        {
            get { return orderStatus; }
            set { orderStatus = value; }
        }
        private int? orderState;

        /// <summary>
        /// 订单状态编号
        /// </summary>
        public int? OrderState
        {
            get { return orderState; }
            set { orderState = value; }
        }

        private decimal? noHasAmount;

        /// <summary>
        /// 本期未还金额
        /// </summary>
        public decimal? NoHasAmount
        {
            get { return noHasAmount; }
            set { noHasAmount = value; }
        }

        private DateTime? finalRepayDate;


        /// <summary>
        /// 最后还款日期
        /// </summary>
        public DateTime? FinalRepayDate
        {
            get { return finalRepayDate; }
            set { finalRepayDate = value; }
        }

        private DateTime createTime;


        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get { return createTime; }
            set { createTime = value; }
        }

        private DateTime? updateTime;


        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime
        {
            get { return updateTime; }
            set { updateTime = value; }
        }

        /// <summary>
        /// 花呗消费明细
        /// </summary> 
        private List<AlipayBillFlowerOrderDetailEntity> _BillFlowerOrderDetail = new List<AlipayBillFlowerOrderDetailEntity>();
        [Ignore]
        public List<AlipayBillFlowerOrderDetailEntity> BillFlowerOrderDetail
        {
            get { return _BillFlowerOrderDetail; }
            set { _BillFlowerOrderDetail = value; }

        } 
        /// <summary>
        /// 花呗消费明细
        /// </summary> 
        private List<AlipayBillFlowerRePaymentEntity> _BillFlowerRePayment = new List<AlipayBillFlowerRePaymentEntity>();
        [Ignore]
        public List<AlipayBillFlowerRePaymentEntity> BillFlowerRePayment
        {
            get { return _BillFlowerRePayment; }
            set { _BillFlowerRePayment = value; }

        }
        #endregion

    }
}
