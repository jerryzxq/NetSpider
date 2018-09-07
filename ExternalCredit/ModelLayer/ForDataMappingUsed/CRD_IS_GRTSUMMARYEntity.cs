using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_IS_GRTSUMMARY")]
    [Schema("credit")]
    public partial class CRD_IS_GRTSUMMARYEntity : BaseEntity
	{


		 /// <summary>
		 /// 担保笔数
		 /// </summary>
		public decimal Guarantee_Count{get; set;}
		 /// <summary>
		 /// 担保金额
		 /// </summary>
		public decimal Guarantee_Amount{get; set;}
		 /// <summary>
		 /// 担保本金余额
		 /// </summary>
		public decimal Guarantee_Balance{get; set;}


	}
}