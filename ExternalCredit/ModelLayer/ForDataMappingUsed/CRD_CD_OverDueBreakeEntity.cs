using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_CD_OverDueBreake")]
    [Schema("credit")]
    public partial class CRD_CD_OverDueBreakeEntity : BaseEntity
	{



		 /// <summary>
		 /// 
		 /// </summary>
		public int BadBebtNum{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public decimal BadBebtMoney{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public int AssetDisposalNum{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public decimal AssetDisposalBalance{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public int GuarantorCompensatoryNum{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public decimal GuarantorCompensatoryBalance{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		[Ignore]
        public DateTime GetTime{get; set;}
	}
}