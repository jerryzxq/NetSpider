using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_AN_ANCINFO")]
    [Schema("credit")]
    public partial class CRD_AN_ANCINFOEntity : BaseEntity
	{
		 /// <summary>
		 /// 声明内容
		 /// </summary>
		public string Content{get; set;}
		 /// <summary>
		 /// 添加日期
		 /// </summary>
		public DateTime Get_Time{get; set;}

	}
}