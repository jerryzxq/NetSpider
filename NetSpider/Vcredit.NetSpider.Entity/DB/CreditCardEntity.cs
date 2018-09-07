using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region CreditCardEntity

	/// <summary>
	/// CreditCardEntity object for NHibernate mapped table 'CreditCard'.
	/// </summary>
	public class CreditCardEntity
	{
        public virtual int LoancardId { get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string ReportId{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Cue{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual decimal ? HighestAmount{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual decimal ? UsedAmount{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual DateTime ? OpenTime{get; set;}
	}
	#endregion
}