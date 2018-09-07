using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_CD_GUARANTEE")]
    [Schema("credit")]
    public partial class CRD_CD_GUARANTEEEntity : BaseEntity
	{



		 /// <summary>
		 /// 担保贷款发放机构
		 /// </summary>
		public string Organ_Name{get; set;}
		 /// <summary>
		 /// 担保贷款合同金额
		 /// </summary>
		public decimal Contract_Money{get; set;}
		 /// <summary>
		 /// 担保贷款发放日期
		 /// </summary>
		public DateTime Begin_Date{get; set;}
		 /// <summary>
		 /// 担保贷款到期日期
		 /// </summary>
		public DateTime End_Date{get; set;}
		 /// <summary>
		 /// 担保金额
		 /// </summary>
		public decimal Guarantee_Money{get; set;}
		 /// <summary>
		 /// 担保贷款本金余额
		 /// </summary>
		public decimal Guarantee_Balance{get; set;}
		 /// <summary>
		 /// 担保贷款五级分类
		 /// </summary>
		public string Class5_State{get; set;}
		 /// <summary>
		 /// 结算日期
		 /// </summary>
		public DateTime Billing_Date{get; set;}


	}
}