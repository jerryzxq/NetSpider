using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.Service
{
    public class CRD_CD_LND
    {
        /// <summary>
        /// 
        /// </summary>
        public string Cue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FinanceOrg { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AccountDw { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? OpenDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? CreditLimitAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string GuaranteeType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? StateEndDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string StateEndMonth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? ShareCreditLimitAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? UsedCreditLimitAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? Latest6MonthUsedAvgAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? UsedHighestAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? ScheduledPaymentDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? ScheduledPaymentAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? ActualPaymentAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? RecentPayDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? CurrOverdueCyc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? CurrOverdueAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? Overdue31To60Amount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? Overdue61To90Amount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? Overdue91To180Amount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? OverdueOver180Amount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PaymentStateBeginMonth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PaymentStateEndMonth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PaymentState { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OverdueRecordBeginMonth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OverdueRecordEndMonth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? OverdueOver90Cyc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? OverdueCyc { get; set; }
    }
}