using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_CD_ASRREPAY")]
    [Schema("credit")]
    public partial class CRD_CD_ASRREPAYEntity : BaseEntity
	{


		 /// <summary>
		 /// 代偿机构
		 /// </summary>
		public string Organ_Name{get; set;}
		 /// <summary>
		 /// 最近一次代偿日期
		 /// </summary>
		public DateTime Latest_Assurer_Repay_Date{get; set;}
		 /// <summary>
		 /// 累计代偿金额
		 /// </summary>
		public decimal Money{get; set;}
		 /// <summary>
		 /// 最近一次还款日期
		 /// </summary>
		public DateTime Latest_Repay_Date{get; set;}
		 /// <summary>
		 /// 余额
		 /// </summary>
		public decimal Balance{get; set;}


	}
}