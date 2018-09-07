using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region CRD_HD_REPORT_HTMLEntity

	/// <summary>
	/// CRD_HD_REPORT_HTMLEntity object for NHibernate mapped table 'CRD_HD_REPORT_HTML'.
	/// </summary>
	public class CRD_HD_REPORT_HTMLEntity
	{
		public virtual int Id{get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string ReportSn{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Html{get; set;}

        /// <summary>
        /// 
        /// </summary>
        public virtual string BusType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string BusId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Loginname { get; set; }
	}
	#endregion
}