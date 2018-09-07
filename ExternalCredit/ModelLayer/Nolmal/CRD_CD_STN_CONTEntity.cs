using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace  Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_CD_STN_CONT")]
    [Schema("credit")]
    public partial class CRD_CD_STN_CONTEntity
	{
           [Ignore]
		public  decimal? Standardloan_Content_Id{get; set; }

		 /// <summary>
		 /// 准贷记卡信息段ID
		 /// </summary>
		public decimal? Standardloancard_Id{get; set;}
		 /// <summary>
		 /// 声明内容
		 /// </summary>
		public string Content{get; set;}
		 /// <summary>
		 /// 添加日期
		 /// </summary>
		public DateTime? Get_Time{get; set;}
		 /// <summary>
		 /// 说明类型
		 /// </summary>
		public string Type_Dw{get; set;}
		 /// <summary>
		 /// PCQS更新时间
		 /// </summary>
           [Ignore]
        public DateTime? Time_Stamp{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		[Ignore]
        public byte[] TIMESTAMP{get; set;}
	}
}