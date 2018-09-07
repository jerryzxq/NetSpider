using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_QR_RECORDDTLINFO")]
    [Schema("credit")]
    public partial class CRD_QR_RECORDDTLINFOEntity : BaseEntity
	{



		 /// <summary>
		 /// 查询日期
		 /// </summary>
		public DateTime Query_Date{get; set;}
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