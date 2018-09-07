using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_PI_RESIDENCE")]
    [Schema("credit")]
    public partial class CRD_PI_RESIDENCEEntity : BaseEntity
	{
     
		 /// <summary>
		 /// 居住地址
		 /// </summary>
		public string Address{get; set;}
		 /// <summary>
		 /// 居住状况
		 /// </summary>
		public string Residence_Type{get; set;}
		 /// <summary>
		 /// 信息更新日期
		 /// </summary>
		public DateTime Get_Time{get; set;}


	}
}