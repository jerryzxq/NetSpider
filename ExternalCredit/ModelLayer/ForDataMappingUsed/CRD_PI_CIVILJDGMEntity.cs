using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_PI_CIVILJDGM")]
    [Schema("credit")]
    public partial class CRD_PI_CIVILJDGMEntity : BaseEntity
	{
	

		 /// <summary>
		 /// 立案法院
		 /// </summary>
		public string Court{get; set;}
		 /// <summary>
		 /// 案由
		 /// </summary>
		public string Case_Reason{get; set;}
		 /// <summary>
		 /// 立案日期
		 /// </summary>
		public DateTime Register_Date{get; set;}
		 /// <summary>
		 /// 结案方式
		 /// </summary>
		public string Closed_Type{get; set;}
		 /// <summary>
		 /// 判决/调解结果
		 /// </summary>
		public string Case_Result{get; set;}
		 /// <summary>
		 /// 判决/调解生效日期
		 /// </summary>
		public DateTime Case_Validate_Date{get; set;}
		 /// <summary>
		 /// 诉讼标的
		 /// </summary>
		public string Suit_Object{get; set;}
		 /// <summary>
		 /// 诉讼标的金额
		 /// </summary>
		public decimal Suit_Object_Money{get; set;}


	}
}