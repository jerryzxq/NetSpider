using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region OperationLogEntity

	/// <summary>
	/// OperationLogEntity object for NHibernate mapped table 'OperationLog'.
	/// </summary>
	public class OperationLogEntity
	{
		public virtual int Id{get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
        //public virtual int Bid{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string IdentityNo{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Mobile{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Name{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int Status{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual DateTime SendTime{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual DateTime ? ReceiveTime{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string ReceiveFilePath{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? ReceiveFailCount{get; set;}
        public virtual string City { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string BusType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string BusId { get; set; }
        /// <summary>
        ///来源 
        /// </summary>
        public virtual string Source { get; set; }

        public virtual BasicEntity Basic { get; set; }

        public virtual IList<NetsEntity> Nets { get; set; }
        public virtual IList<SmsesEntity> Smses { get; set; }
        public virtual IList<TransactionsEntity> Transactions { get; set; }
        public virtual IList<CallsEntity> Calls { get; set; }
	}
	#endregion
}