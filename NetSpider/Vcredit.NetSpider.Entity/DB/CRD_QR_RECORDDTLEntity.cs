using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region CRD_QR_RECORDDTLEntity

	/// <summary>
	/// CRD_QR_RECORDDTLEntity object for NHibernate mapped table 'CRD_QR_RECORDDTL'.
	/// </summary>
	public class CRD_QR_RECORDDTLEntity
	{
		public virtual int Id{get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ReportId{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual DateTime ? QueryDate{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Querier{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string QueryReason{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? COUNTCARDIN3M{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? COUNTCARDIN1M{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? COUNTloanIN3M{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? COUNTloanIN1M{get; set;}
	}
	#endregion
}