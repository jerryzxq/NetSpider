using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region CRD_CD_GUARANTEEEntity

	/// <summary>
	/// CRD_CD_GUARANTEEEntity object for NHibernate mapped table 'CRD_CD_GUARANTEE'.
	/// </summary>
	public class CRD_CD_GUARANTEEEntity
	{
		public virtual int Id{get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ReportId{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string OrganName{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual decimal ? ContractMoney{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual DateTime ? BeginDate{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual DateTime ? EndDate{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual decimal ? GuaranteeMoney{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual decimal ? GuaranteeBalance{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Class5State{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual DateTime ? BillingDate{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Name{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string CertType{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string CertNo{get; set;}
	}
	#endregion
}