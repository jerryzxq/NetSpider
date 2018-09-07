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
        /// ���
        /// </summary>
        [AutoIncrement]
 		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private DateTime? tradeTime  ;
        /// <summary>
        /// ����ʱ��
        /// </summary>
		public DateTime? TradeTime
		{
			get { return tradeTime; }
			set { tradeTime = value; }
		}
        private string loanNo;
        /// <summary>
        /// ������
        /// </summary>
        public string LoanNo
        {
            get { return loanNo; }
            set { loanNo = value; }
        }

        private decimal? costAmount;
        /// <summary>
        /// ���ѽ��
        /// </summary>
        public decimal? CostAmount
        {
            get { return costAmount; }
            set { costAmount = value; }
        }
		private string tradeContent  ;
        /// <summary>
        /// ����ժҪ
        /// </summary>
		public string TradeContent
		{
			get { return tradeContent; }
			set { tradeContent = value; }
		}
      
		private string billStatus  ;
        /// <summary>
        /// �˵�״̬
        /// </summary>
		public string BillStatus
		{
			get { return billStatus; }
			set { billStatus = value; }
		}
        private int? billState;
        /// <summary>
        /// �˵�״̬���
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
        /// ��������
        /// </summary>
		public DateTime? RepaymentDate
		{
			get { return repaymentDate; }
			set { repaymentDate = value; }
		}
      
		private decimal? shouldRepayAmount  ;
        /// <summary>
        /// Ӧ�����
        /// </summary>
		public decimal? ShouldRepayAmount
		{
			get { return shouldRepayAmount; }
			set { shouldRepayAmount = value; }
		}
      
		private decimal? capitalAmount  ;
        /// <summary>
        /// ����
        /// </summary>
		public decimal? CapitalAmount
		{
			get { return capitalAmount; }
			set { capitalAmount = value; }
		}
      
		private decimal? feesAmount  ;
        /// <summary>
        /// ����������
        /// </summary>
		public decimal? FeesAmount
		{
			get { return feesAmount; }
			set { feesAmount = value; }
		}
      
		private string laveDays  ;
        /// <summary>
        /// ʣ�໹��ʱ��
        /// </summary>
		public string LaveDays
		{
			get { return laveDays; }
			set { laveDays = value; }
		}
        private decimal? interestAmount;
        /// <summary>
        /// ������
        /// </summary>
        public decimal? InterestAmount
        {
            get { return interestAmount; }
            set { interestAmount = value; }
        }
        private int? usedDayNum;
        /// <summary>
        /// ����
        /// </summary>
        public int? UsedDayNum
        {
            get { return usedDayNum; }
            set { usedDayNum = value; }
        }
        private string inteRate;
        /// <summary>
        /// ����
        /// </summary>
        public string InteRate
        {
            get { return inteRate; }
            set { inteRate = value; }
        }
        private decimal overdueCharges;
        /// <summary>
        /// ���ڷ���
        /// </summary>
        public decimal OverdueCharges
        {
            get { return overdueCharges; }
            set { overdueCharges = value; }
        }
        private int? overdueState;
        /// <summary>
        /// ��Ϣ״̬
        /// </summary>
        public int? OverdueState
        {
            get { return overdueState; }
            set { overdueState = value; }
        }
		private long? userId  ;
        /// <summary>
        /// �ͻ����
        /// </summary>
		public long? UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private string accountName  ;
        /// <summary>
        /// �ͻ��˺�
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private DateTime? createTime = DateTime.Now ;
        /// <summary>
        /// ����ʱ��
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
		#endregion
	}
}
