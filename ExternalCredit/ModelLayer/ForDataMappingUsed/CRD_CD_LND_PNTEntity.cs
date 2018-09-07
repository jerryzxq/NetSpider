using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_CD_LND_PNT")]
    [Schema("credit")]
	public partial class CRD_CD_LND_PNTEntity
	{
           [Ignore]
		public  decimal Loancard_Payment_Id{get; set; }

		 /// <summary>
		 /// 贷记卡ID
		 /// </summary>
		public decimal Loancard_Id{get; set;}
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
        public DateTime Time_Stamp{get; set;}
        [Ignore]
        public byte[] TIMESTAMP { get; set; }
	}
}