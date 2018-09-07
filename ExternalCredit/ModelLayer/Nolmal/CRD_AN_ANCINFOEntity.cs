using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace  Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_AN_ANCINFO")]
    [Schema("credit")]
    public partial class CRD_AN_ANCINFOEntity : BaseEntity
	{
		 /// <summary>
		 /// 声明内容
		 /// </summary>
		public string Content{get; set;}
        [JsonConverter(typeof(DesJsonConverter))]
		 /// <summary>
		 /// 添加日期
		 /// </summary>
		public DateTime? Get_Time{get; set;}

	}
}