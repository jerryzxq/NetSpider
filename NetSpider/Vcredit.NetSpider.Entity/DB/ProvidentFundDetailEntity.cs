using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region ProvidentFundDetailEntity

	/// <summary>
	/// ProvidentFundDetailEntity object for NHibernate mapped table 'ProvidentFundDetail'.
	/// </summary>
	public class ProvidentFundDetailEntity
	{
		public virtual int Id{get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? ProvidentFundId{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string PaymentType{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string PaymentFlag{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual DateTime ? PayTime{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string ProvidentFundTime{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual decimal ? ProvidentFundBase{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual decimal ? PersonalPayAmount{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual decimal ? CompanyPayAmount{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string CompanyName{get; set;}
        public virtual string Description { get; set; }
	}
	#endregion
}