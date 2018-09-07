using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_CD_LND_CONT")]
    [Schema("credit")]
	public partial class CRD_CD_LND_CONTEntity
	{
           [Ignore]
		public  decimal Loancard_Content_Id{get; set; }

		 /// <summary>
		 /// 贷记卡ID
		 /// </summary>
		public decimal Loancard_Id{get; set;}
		 /// <summary>
		 /// 声明内容
		 /// </summary>
		public string Content{get; set;}
		 /// <summary>
		 /// 添加日期
		 /// </summary>
		public DateTime Get_Time{get; set;}
		 /// <summary>
		 /// 说明类型
		 /// </summary>
		public string Type_Dw{get; set;}
		 /// <summary>
		 /// PCQS更新时间
		 /// </summary>
           [Ignore]
        public DateTime Time_Stamp{get; set;}
        [Ignore]
        public byte[] TIMESTAMP { get; set; }

	}
}