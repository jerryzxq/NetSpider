using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_IS_SHAREDEBT")]
    [Schema("credit")]
    public partial class CRD_IS_SHAREDEBTEntity : BaseEntity
	{


		 /// <summary>
		 /// 授信及负债类型
		 /// </summary>
		public string Type_Dw{get; set;}
		 /// <summary>
		 /// 贷款法人机构数/发卡法人机构数
		 /// </summary>
		public decimal Finance_Corp_Count{get; set;}
		 /// <summary>
		 /// 贷款机构数/发卡机构数
		 /// </summary>
		public decimal Finance_Org_Count{get; set;}
		 /// <summary>
		 /// 笔数/账户数
		 /// </summary>
		public decimal Account_Count{get; set;}
		 /// <summary>
		 /// 合同金额/授信总额
		 /// </summary>
		public decimal Credit_Limit{get; set;}
		 /// <summary>
		 /// 单家行最高授信额度
		 /// </summary>
		public decimal Max_Credit_Limit_Per_Org{get; set;}
		 /// <summary>
		 /// 单家行最低授信额度
		 /// </summary>
		public decimal Min_Credit_Limit_Per_Org{get; set;}
		 /// <summary>
		 /// 余额
		 /// </summary>
		public decimal Balance{get; set;}
		 /// <summary>
		 /// 已用额度/透支余额
		 /// </summary>
		public decimal Used_Credit_Limit{get; set;}
		 /// <summary>
		 /// 最近6个月平均应还款/最近6个月平均使用额度/最近6个月平均透支余额
		 /// </summary>
		public decimal Latest_6M_Used_Avg_Amount{get; set;}


		 /// <summary>
		 /// 
		 /// </summary>
		public int Bh{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string CreditcardNo{get; set;}
	}
}