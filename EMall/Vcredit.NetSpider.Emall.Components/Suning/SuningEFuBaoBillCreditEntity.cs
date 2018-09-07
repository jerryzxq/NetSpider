using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("suning_efubaobillcredit")]
	public class SuningEFuBaoBillCreditEntity
	{
		public SuningEFuBaoBillCreditEntity() { }

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
      
		private DateTime? tradeTime  ;
        /// <summary>
        /// 交易时间
        /// </summary>
		public DateTime? TradeTime
		{
			get { return tradeTime; }
			set { tradeTime = value; }
		}
        private string loanNo;
        /// <summary>
        /// 贷款编号
        /// </summary>
        public string LoanNo
        {
            get { return loanNo; }
            set { loanNo = value; }
        }

        private decimal? costAmount;
        /// <summary>
        /// 花费金额
        /// </summary>
        public decimal? CostAmount
        {
            get { return costAmount; }
            set { costAmount = value; }
        }
		private string tradeContent  ;
        /// <summary>
        /// 交易摘要
        /// </summary>
		public string TradeContent
		{
			get { return tradeContent; }
			set { tradeContent = value; }
		}
      
		private string billStatus  ;
        /// <summary>
        /// 账单状态
        /// </summary>
		public string BillStatus
		{
			get { return billStatus; }
			set { billStatus = value; }
		}
        private int? billState;
        /// <summary>
        /// 账单状态编号
        /// </summary>
        public int? BillState
        {
            get { return billState; }
            set { billState = value; }
        }
        private decimal? refundAmount;

        public decimal? RefundAmount
        {
            get { return refundAmount; }
            set { refundAmount = value; }
        }
		private DateTime? repaymentDate  ;
        /// <summary>
        /// 还款日期
        /// </summary>
		public DateTime? RepaymentDate
		{
			get { return repaymentDate; }
			set { repaymentDate = value; }
		}
      
		private decimal? shouldRepayAmount  ;
        /// <summary>
        /// 应还金额
        /// </summary>
		public decimal? ShouldRepayAmount
		{
			get { return shouldRepayAmount; }
			set { shouldRepayAmount = value; }
		}
      
		private decimal? capitalAmount  ;
        /// <summary>
        /// 本金
        /// </summary>
		public decimal? CapitalAmount
		{
			get { return capitalAmount; }
			set { capitalAmount = value; }
		}
      
		private decimal? feesAmount  ;
        /// <summary>
        /// 分期手续费
        /// </summary>
		public decimal? FeesAmount
		{
			get { return feesAmount; }
			set { feesAmount = value; }
		}
      
		private string laveDays  ;
        /// <summary>
        /// 剩余还款时间
        /// </summary>
		public string LaveDays
		{
			get { return laveDays; }
			set { laveDays = value; }
		}
        private decimal? interestAmount;
        /// <summary>
        /// 手续费
        /// </summary>
        public decimal? InterestAmount
        {
            get { return interestAmount; }
            set { interestAmount = value; }
        }
        private int? usedDayNum;
        /// <summary>
        /// 天数
        /// </summary>
        public int? UsedDayNum
        {
            get { return usedDayNum; }
            set { usedDayNum = value; }
        }
        private string inteRate;
        /// <summary>
        /// 利率
        /// </summary>
        public string InteRate
        {
            get { return inteRate; }
            set { inteRate = value; }
        }
        private decimal overdueCharges;
        /// <summary>
        /// 逾期费用
        /// </summary>
        public decimal OverdueCharges
        {
            get { return overdueCharges; }
            set { overdueCharges = value; }
        }
        private int? overdueState;
        /// <summary>
        /// 利息状态
        /// </summary>
        public int? OverdueState
        {
            get { return overdueState; }
            set { overdueState = value; }
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
        /// 客户账号
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private DateTime? createTime = DateTime.Now ;
        /// <summary>
        /// 创建时间
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
		#endregion
	}
}
