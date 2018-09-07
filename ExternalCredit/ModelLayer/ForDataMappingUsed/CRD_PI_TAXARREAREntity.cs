using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_PI_TAXARREAR")]
    [Schema("credit")]
    public partial class CRD_PI_TAXARREAREntity : BaseEntity
	{



		 /// <summary>
		 /// 主管税务机关
		 /// </summary>
		public string Organ_Name{get; set;}
		 /// <summary>
		 /// 欠税总额
		 /// </summary>
		public decimal Tax_Arrea_Amount{get; set;}
		 /// <summary>
		 /// 欠税统计日期
		 /// </summary>
		public DateTime Tax_Arrear_Date{get; set;}


	}
}