using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region CreditReportHtmlEntity

	/// <summary>
	/// CreditReportHtmlEntity object for NHibernate mapped table 'CreditReportHtml'.
	/// </summary>
	public class CreditReportHtmlEntity
	{
        public virtual string ReportId { get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Html{get; set;}
	}
	#endregion
}