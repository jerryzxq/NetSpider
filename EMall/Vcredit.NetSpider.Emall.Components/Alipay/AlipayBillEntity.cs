using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Alipay_Bill")]
    
    public class AlipayBillEntity
    {

        public AlipayBillEntity() { }


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

        private long? baicID;


        /// <summary>
        /// 账户编号
        /// </summary>
        public long? BaicID
        {
            get { return baicID; }
            set { baicID = value; }
        }

        private DateTime? billTime;


        /// <summary>
        /// 订单时间
        /// </summary>
        public DateTime? BillTime
        {
            get { return billTime; }
            set { billTime = value; }
        }


        private string accountName;
        /// <summary>
        /// 用户名
        /// </summary>
        public string AccountName
        {
            get { return accountName; }
            set { accountName = value; }
        }

        private string title;


        /// <summary>
        /// 名称
        /// </summary>
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        private string eachOtherName;


        /// <summary>
        /// 对方名称
        /// </summary>
        public string EachOtherName
        {
            get { return eachOtherName; }
            set { eachOtherName = value; }
        }

        private string billNO;


        /// <summary>
        /// 流水号 
        /// </summary>
        public string BillNO
        {
            get { return billNO; }
            set { billNO = value; }
        }

        private decimal? amount;


        /// <summary>
        /// 金额
        /// </summary>
        public decimal? Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        private string state;


        /// <summary>
        /// 状态 
        /// </summary>
        public string State
        {
            get { return state; }
            set { state = value; }
        }

        private string tradeType;


        /// <summary>
        /// 分类
        /// </summary>
        public string TradeType
        {
            get { return tradeType; }
            set { tradeType = value; }
        }
        private string tradeName;


        /// <summary>
        /// 分类名
        /// </summary>
        public string TradeName
        {
            get { return tradeName; }
            set { tradeName = value; }
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
   
        /// <summary>
        /// 支付明细
        /// </summary>
        private List<AlipayBillPaymentEntity> _BillPayment = new List<AlipayBillPaymentEntity>();
        [Ignore]
        public List<AlipayBillPaymentEntity> BillPayment
        {
            get { return _BillPayment; }
            set { _BillPayment = value; }

        }
        /// <summary>
        /// 账单支付记录
        /// </summary>

        [Ignore]
        public AlipayBillExpenditureEntity BillExpenditure
        {
            get;
            set;

        }
        /// <summary>
        /// 账单信用卡还款
        /// </summary>

        [Ignore]
        public AlipayBillCreditPaymentEntity BillCreditPayment
        {
            get;
            set;

        }
        [Ignore]
        public AlipayBillTransferEntity BillTransfer
        {
            get;
            set;

        }
        [Ignore]
        public AlipayBillRefundsEntity BillRefunds
        {
            get;
            set;

        }
        [Ignore]
        public AlipayBillOrderEntity BillOrder
        {
            get;
            set;

        }
        #endregion

    }
}
