using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region CreditAccountEntity

	/// <summary>
	/// CreditAccountEntity object for NHibernate mapped table 'CreditAccount'.
	/// </summary>
	public class CreditAccountEntity
	{
		public virtual int Id{get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string UserName{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Password{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string TradeCode{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual DateTime ? CreateTime{get; set;}
	}
	#endregion
}