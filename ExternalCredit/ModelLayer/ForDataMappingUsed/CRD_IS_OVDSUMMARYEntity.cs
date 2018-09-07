using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_IS_OVDSUMMARY")]
    [Schema("credit")]
    public partial class CRD_IS_OVDSUMMARYEntity : BaseEntity
	{

		 /// <summary>
		 /// 逾期信息类型
		 /// </summary>
		public string Type_Dw{get; set;}
		 /// <summary>
		 /// 笔数/账户数
		 /// </summary>
        public decimal Count_Dw{get; set;}
		 /// <summary>
		 /// 月份数
		 /// </summary>
		public decimal Months{get; set;}
		 /// <summary>
		 /// 单月最高逾期总额/单月最高透支总额
		 /// </summary>
		public decimal Highest_Oa_Per_Mon{get; set;}
		 /// <summary>
		 /// 最长逾期月数/最长透支月数
		 /// </summary>
		public decimal Max_Duration{get; set;}


		 /// <summary>
		 /// 
		 /// </summary>
		public int Bh{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string CreditcardNo{get; set;}
	}
}