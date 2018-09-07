using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_CD_OutstandeSummary")]
    [Schema("credit")]
    public partial class CRD_CD_OutstandeSummaryEntity : BaseEntity
	{


		 /// <summary>
		 /// 
		 /// </summary>
		public int LoanManOrgNum{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public int LoanOrgNum{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public int LoanNum{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public decimal ContractAmount{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public decimal Balance{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public decimal RecentSixMonthRepayment{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		[Ignore]
        public DateTime GetTime{get; set;}
	}
}