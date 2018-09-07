using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace  Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_CD_STN_PNT")]
    [Schema("credit")]
	public partial class CRD_CD_STN_PNTEntity
	{
           [Ignore]
		public  decimal? Standardloan_Payment_Id{get; set; }

		 /// <summary>
		 /// 准贷记卡信息段ID
		 /// </summary>
		public decimal? Standardloancard_Id{get; set;}
		 /// <summary>
		 /// 还款月
		 /// </summary>
		public string Pay_Month{get; set;}
		 /// <summary>
		 /// 该月还款状态
		 /// </summary>
		public string Pay_State{get; set;}
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