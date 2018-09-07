using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region NetsEntity

	/// <summary>
	/// NetsEntity object for NHibernate mapped table 'nets'.
	/// </summary>
	public class NetsEntity
	{
		public virtual long Id{get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? Oper_id{get; set;}
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
		public virtual string Place{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Net_type{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Start_time{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? Use_time{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? Subflow{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual double ? Subtotal{get; set;}
	}
	#endregion
}