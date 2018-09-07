using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_IS_FBKSUMMARY")]
    [Schema("credit")]
    public partial class CRD_IS_FBKSUMMARYEntity : BaseEntity
	{


		 /// <summary>
		 /// 违约信息类型
		 /// </summary>
		public string Type_Dw{get; set;}
		 /// <summary>
		 /// 笔数
		 /// </summary>
		public decimal Count_Dw{get; set;}
		 /// <summary>
		 /// 余额
		 /// </summary>
		public decimal Balance{get; set;}


	}
}