using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Alipay_BillPayment")]
    
    public class AlipayBillPaymentEntity
    {

        public AlipayBillPaymentEntity() { }


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

        private string billNO;


        /// <summary>
        /// 流水号
        /// </summary>
        public string BillNO
        {
            get { return billNO; }
            set { billNO = value; }
        }

        private long? billID;


        /// <summary>
        /// 账单编号
        /// </summary>
        public long? BillID
        {
            get { return billID; }
            set { billID = value; }
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

        private string payMethod;


        /// <summary>
        /// 支付方式
        /// </summary>
        public string PayMethod
        {
            get { return payMethod; }
            set { payMethod = value; }
        }

        private decimal? amount;


        /// <summary>
        /// 支付金额
        /// </summary>
        public decimal? Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        private string instructions;


        /// <summary>
        /// 支付说明
        /// </summary>
        public string Instructions
        {
            get { return instructions; }
            set { instructions = value; }
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
