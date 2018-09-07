using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region CreditReportSummaryEntity

	/// <summary>
	/// CreditReportSummaryEntity object for NHibernate mapped table 'CreditReportSummary'.
	/// </summary>
	public class CreditReportSummaryEntity
	{
		public virtual int Id{get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string ReportId{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string SummaryType{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? CreditCardCount{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? HouseLoanCount{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? OtherLoanCount{get; set;}
	}
	#endregion
}