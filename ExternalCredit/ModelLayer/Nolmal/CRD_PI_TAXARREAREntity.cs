using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace  Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_PI_TAXARREAR")]
    [Schema("credit")]
    public partial class CRD_PI_TAXARREAREntity : BaseEntity
	{



		 /// <summary>
		 /// 主管税务机关
		 /// </summary>
		public string Organ_Name{get; set;}
          [JsonConverter(typeof(DesJsonConverter))]
		 /// <summary>
		 /// 欠税总额
		 /// </summary>
		public decimal? Tax_Arrea_Amount{get; set;}
          [JsonConverter(typeof(DesJsonConverter))]
		 /// <summary>
		 /// 欠税统计日期
		 /// </summary>
		public DateTime? Tax_Arrear_Date{get; set;}


	}
}