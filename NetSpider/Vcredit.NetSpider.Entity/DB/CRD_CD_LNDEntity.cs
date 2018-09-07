using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region CRD_CD_LNDEntity

	/// <summary>
	/// CRD_CD_LNDEntity object for NHibernate mapped table 'CRD_CD_LND'.
	/// </summary>
	public class CRD_CD_LNDEntity
	{
		public virtual int Id{get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ReportId{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Cue{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string State{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string FinanceOrg{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string AccountDw{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Currency{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual DateTime ? OpenDate{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual decimal ? CreditLimitAmount{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual string GuaranteeType{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual DateTime ? StateEndDate{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual string StateEndMonth{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual decimal ? ShareCreditLimitAmount{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual decimal ? UsedCreditLimitAmount{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual decimal ? Latest6MonthUsedAvgAmount{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual decimal ? UsedHighestAmount{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual DateTime ? ScheduledPaymentDate{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual decimal ? ScheduledPaymentAmount{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual decimal ? ActualPaymentAmount{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual DateTime ? RecentPayDate{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual decimal ? CurrOverdueCyc{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual decimal ? CurrOverdueAmount{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual decimal ? Overdue31To60Amount{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual decimal ? Overdue61To90Amount{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual decimal ? Overdue91To180Amount{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual decimal ? OverdueOver180Amount{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual string PaymentStateBeginMonth{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual string PaymentStateEndMonth{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual string PaymentState{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual string OverdueRecordBeginMonth{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual string OverdueRecordEndMonth{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? OverdueOver90Cyc{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? OverdueCyc{get; set;}
	}
	#endregion
}