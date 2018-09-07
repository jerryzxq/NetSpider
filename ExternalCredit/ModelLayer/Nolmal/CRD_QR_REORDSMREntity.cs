using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Vcredit.ExternalCredit.CommonLayer.Utility;
using Newtonsoft.Json;

namespace  Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_QR_REORDSMR")]
    [Schema("credit")]
    public partial class CRD_QR_REORDSMREntity : BaseEntity
	{


		 /// <summary>
		 /// 统计类型
		 /// </summary>
		public string Type_Id{get; set;}
		 /// <summary>
		 /// 统计原因
		 /// </summary>
		public string Reason{get; set;}
        [JsonConverter(typeof(DesJsonConverter))]
		 /// <summary>
		 /// 统计数量
		 /// </summary>
		public decimal? Sum_Dw{get; set;}


	}
}