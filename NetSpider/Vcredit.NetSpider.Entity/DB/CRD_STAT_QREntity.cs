using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region CRD_STAT_QREntity

	/// <summary>
	/// CRD_STAT_QREntity object for NHibernate mapped table 'CRD_STAT_QR'.
	/// </summary>
	public class CRD_STAT_QREntity
	{
		public virtual int Id{get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? ReportId{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? M3ALLCNTTOTAL{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? M3CREDITCNT{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
        public virtual int ? M3LOANCNT{get; set;}
        /// <summary>
        /// 过去3个月信用卡审批查询次数
        /// </summary>
        public virtual int L3M_ACCT_QRY_NUM { get; set; }
        /// <summary>
        /// 过去3个月贷款审批查询次数
        /// </summary>
        public virtual int L3M_LN_QRY_NUM { get; set; }

        /// <summary>
        /// 非银行机构的贷款审批查询次数
        /// </summary>
        public virtual int NON_BNK_LN_QRY_CNT { get; set; }
        /// <summary>
        /// [征信]3个月内非银机构查询次数
        /// </summary>
        public virtual int COUNT_Nonbank_IN3M { get; set; }
	}
	#endregion
}