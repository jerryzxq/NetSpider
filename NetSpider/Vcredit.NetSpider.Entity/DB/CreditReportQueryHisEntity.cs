using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region CreditReportQueryHisEntity

	/// <summary>
	/// CreditReportQueryHisEntity object for NHibernate mapped table 'CreditReportQueryHis'.
	/// </summary>
	public class CreditReportQueryHisEntity
	{
		public virtual int Id{get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string ReportId{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? SerialId{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual DateTime ? QueryTime{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string QueryOperator{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string QueryReason{get; set;}
	}
	#endregion
}