using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region TransactionsEntity

	/// <summary>
	/// TransactionsEntity object for NHibernate mapped table 'transactions'.
	/// </summary>
	public class TransactionsEntity
	{
		public virtual int Id{get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? Oper_id{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
        public virtual string Bill_cycle { get; set; }
		 /// <summary>
		 /// 
		 /// </summary>
        public virtual string Update_time { get; set; }
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Cell_phone{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual double ? Plan_amt{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual double ? Total_amt{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual double ? Pay_amt{get; set;}
	}
	#endregion
}