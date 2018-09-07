using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region CRD_IS_CREDITCUEEntity

	/// <summary>
    /// 信用提示汇总段表.
	/// </summary>
	public class CRD_IS_CREDITCUEEntity
	{
		public virtual int Id{get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
		public virtual decimal ? HouseLoanCount{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual decimal ? OtherLoanCount{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual string FirstLoanOpenMonth{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual decimal ? LoancardCount{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual string FirstLoancardOpenMonth{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual decimal ? StandardLoancardCount{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual string FirstSlOpenMonth{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual decimal ? AnnounceCount{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual decimal ? DissentCount{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual string Score{get; set;}
        // /// <summary>
        // /// 
        // /// </summary>
        //public virtual string ScoreMonth{get; set;}
	}
	#endregion
}