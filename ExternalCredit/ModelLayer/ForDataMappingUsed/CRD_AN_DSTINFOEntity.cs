using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_AN_DSTINFO")]
    [Schema("credit")]
    public partial class CRD_AN_DSTINFOEntity:BaseEntity
	{

		 /// <summary>
		 /// 标注内容
		 /// </summary>
		public string Content{get; set;}
		 /// <summary>
		 /// 添加日期
		 /// </summary>
		public DateTime Get_Time{get; set;}


	}
}