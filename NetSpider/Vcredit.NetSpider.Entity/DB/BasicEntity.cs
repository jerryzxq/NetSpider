using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region BasicEntity

	/// <summary>
	/// BasicEntity object for NHibernate mapped table 'basic'.
	/// </summary>
	public class BasicEntity
	{
		public virtual int Id{get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? Oper_id{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Cell_phone{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Real_name{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Reg_time{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Idcard{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
        public virtual string Update_time { get; set; }
        public virtual OperationLogEntity OperationLog { get; set; }
	}
	#endregion
}