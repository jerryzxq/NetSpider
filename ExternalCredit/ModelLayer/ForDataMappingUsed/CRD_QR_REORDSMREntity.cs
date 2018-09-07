using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
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
		 /// <summary>
		 /// 统计数量
		 /// </summary>
		public decimal Sum_Dw{get; set;}


	}
}