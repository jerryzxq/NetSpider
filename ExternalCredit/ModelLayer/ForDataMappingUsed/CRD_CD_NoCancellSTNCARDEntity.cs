using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_CD_NoCancellSTNCARD")]
    [Schema("credit")]
	public partial class CRD_CD_NoCancellSTNCARDEntity:BaseEntity
	{



		 /// <summary>
		 /// 
		 /// </summary>
		public int FinanceMan_OrgNum{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public int Finance_OrgNum{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public int AccountNum{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public decimal TotalCreditAmount{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public decimal MaxCreditAmount{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public decimal MinimumCreditAmount{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public decimal OverDueNum{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public decimal AverageRecent6Months{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		[Ignore]
        public DateTime GetTime{get; set;}
	}
}