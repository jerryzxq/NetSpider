using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_CD_GUARANTEESummery")]
    [Schema("credit")]
    public partial class CRD_CD_GUARANTEESummeryEntity : BaseEntity
	{

		 /// <summary>
		 /// 
		 /// </summary>
		public int GuaranteeNum{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public decimal GuaranteeMoney{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public decimal PrincipalBalance{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public DateTime GetTime{get; set;}
	}
}