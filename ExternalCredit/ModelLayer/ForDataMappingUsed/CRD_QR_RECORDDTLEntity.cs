using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_QR_RECORDDTL")]
    [Schema("credit")]
    public partial class CRD_QR_RECORDDTLEntity : BaseEntity
	{



		 /// <summary>
		 /// 
		 /// </summary>
		public DateTime? Query_Date{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string Querier{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string Query_Reason{get; set;}

		 /// <summary>
		 /// 三个月内信用卡审批查询次数
		 /// </summary>
		public int COUNT_CARD_IN3M{get; set;}
		 /// <summary>
		 /// 一个月内信用卡审批查询次数
		 /// </summary>
		public int COUNT_CARD_IN1M{get; set;}
		 /// <summary>
		 /// 三个月内贷款审批查询次数
		 /// </summary>
		public int COUNT_loan_IN3M{get; set;}
		 /// <summary>
		 /// 一个月内贷款审批查询次数
		 /// </summary>
		public int COUNT_loan_IN1M{get; set;}

		 /// <summary>
		 /// 
		 /// </summary>
		public int? Bh{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string CreditcardNo{get; set;}
	}
}