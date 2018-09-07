using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace  Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_QR_RECORDDTLINFO")]
    [Schema("credit")]
    public partial class CRD_QR_RECORDDTLINFOEntity : BaseEntity
	{

       [JsonConverter(typeof(DesJsonConverter))]
		 /// <summary>
		 /// 查询日期
		 /// </summary>
		public DateTime? Query_Date{get; set;}
		 /// <summary>
		 /// 查询操作员
		 /// </summary>
		public string Querier{get; set;}
		 /// <summary>
		 /// 查询原因
		 /// </summary>
		public string Query_Reason{get; set;}

	}
}