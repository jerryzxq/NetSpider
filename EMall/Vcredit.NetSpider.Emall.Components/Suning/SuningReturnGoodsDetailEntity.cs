using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Suning_ReturnGoodsDetail")] 
    public class SuningReturnGoodsDetailEntity
    {
        public SuningReturnGoodsDetailEntity() { }

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

        private long returnID;
        /// <summary>
        /// 退货订单编号
        /// </summary>
        public long ReturnID
        {
            get { return returnID; }
            set { returnID = value; }
        }
        private string serviceType;
        /// <summary>
        /// 服务类型
        /// </summary>
        public string ServiceType
        {
            get { return serviceType; }
            set { serviceType = value; }
        }
        private int? serviceTypeValueCount;
        /// <summary>
        /// 服务类型值数量
        /// </summary>
        public int? ServiceTypeValueCount
        {
            get { return serviceTypeValueCount; }
            set { serviceTypeValueCount = value; }
        }
        private string refund;
        /// <summary>
        /// 退款方式
        /// </summary>
        public string Refund
        {
            get { return refund; }
            set { refund = value; }
        }
        private string serviceTypeReason;
        /// <summary>
        /// 服务类型值原因
        /// </summary>
        public string ServiceTypeReason
        {
            get { return serviceTypeReason; }
            set { serviceTypeReason = value; }
        }
        private string totalPayment;
        /// <summary>
        /// 支付总额
        /// </summary>
        public string TotalPayment
        {
            get { return totalPayment; }
            set { totalPayment = value; }
        }
        private decimal? transactionAmount;
        /// <summary>
        /// 交易金额
        /// </summary>
        public decimal? TransactionAmount
        {
            get { return transactionAmount; }
            set { transactionAmount = value; }
        }
        private decimal? refundAmount;
        /// <summary>
        /// 退款金额
        /// </summary>
        public decimal? RefundAmount
        {
            get { return refundAmount; }
            set { refundAmount = value; }
        }
        private string description;
        /// <summary>
        /// 详细描述
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        private string returnStatus;
        /// <summary>
        /// 退款状态
        /// </summary>
        public string ReturnStatus
        {
            get { return returnStatus; }
            set { returnStatus = value; }
        }
        private string returnStatusDesc;
        /// <summary>
        /// 退款状态描述
        /// </summary>
        public string ReturnStatusDesc
        {
            get { return returnStatusDesc; }
            set { returnStatusDesc = value; }
        }

		 
		private DateTime createTime = DateTime.Now ;
        /// <summary>
        /// 该记录创建时间
        /// </summary>
		public DateTime CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
         
		#endregion
    }
}
