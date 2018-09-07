using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Vcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_CD_STNCARD")]
    [Schema("credit")]
    public partial class CRD_CD_STNCARDEntity : BaseEntity
	{
        public  CRD_CD_STNCARDEntity()
        {
            GetTime = CommonData.defaultTime;
            QCR_Issued_Time = CommonData.defaultTime;
            State_End_Date = CommonData.defaultTime;
            Open_Date = CommonData.defaultTime;
            Scheduled_Payment_Date = CommonData.defaultTime;
            Recent_Pay_Date = CommonData.defaultTime;
        }

        public string FinanceType { get; set; }
        private List<CRD_CD_STN_OVDEntity> stnoverList = new List<CRD_CD_STN_OVDEntity>();
        [Ignore]
        /// <summary>
        /// 准贷记卡逾期
        /// </summary>
        public List<CRD_CD_STN_OVDEntity> StnoverList
        {
            get { return stnoverList; }
            set { stnoverList = value; }
        }
        private List<CRD_CD_STN_SPLEntity> stnSpLList = new List<CRD_CD_STN_SPLEntity>();
        [Ignore]
        /// <summary>
        /// 准贷记卡特殊交易
        /// </summary>
        public List<CRD_CD_STN_SPLEntity> StnSPLList
        {
            get { return stnSpLList; }
            set { stnSpLList = value; }
        }
		 /// <summary>
		 /// 基本信息提示
		 /// </summary>
		public string Cue{get; set;}
		 /// <summary>
		 /// 账户状态
		 /// </summary>
		public string State{get; set;}
		 /// <summary>
		 /// 贷记卡授信情况_发卡机构
		 /// </summary>
		public string Finance_Org{get; set;}
		 /// <summary>
		 /// 贷记卡授信情况_业务号
		 /// </summary>
		public string Account_Dw{get; set;}
		 /// <summary>
		 /// 贷记卡授信情况_币种
		 /// </summary>
		public string Currency{get; set;}
		 /// <summary>
		 /// 贷记卡授信情况_发卡日期
		 /// </summary>
		public DateTime Open_Date{get; set;}
		 /// <summary>
		 /// 贷记卡授信情况_授信额度
		 /// </summary>
		public decimal Credit_Limit_Amount{get; set;}
		 /// <summary>
		 /// 贷记卡授信情况_担保方式
		 /// </summary>
		public string Guarantee_Type{get; set;}
		 /// <summary>
		 /// 使用/还款情况_状态截止日
		 /// </summary>
        public DateTime State_End_Date{get; set;}
		 /// <summary>
		 /// 使用/还款情况_状态截止月
		 /// </summary>
		public string State_End_Month{get; set;}
		 /// <summary>
		 /// 使用/还款情况_共享额度
		 /// </summary>
		public decimal Share_Credit_Limit_Amount{get; set;}
		 /// <summary>
		 /// 使用/还款情况_ /透支余额
		 /// </summary>
		public decimal Used_Credit_Limit_Amount{get; set;}
		 /// <summary>
		 /// 使用/还款情况_ /最近6个月平均透支余额
		 /// </summary>
		public decimal Latest6_Month_Used_Avg_Amount{get; set;}
		 /// <summary>
		 /// 使用/还款情况_ /最大透支余额
		 /// </summary>
		public decimal Used_Highest_Amount{get; set;}
		 /// <summary>
		 /// 使用/还款情况_账单日
		 /// </summary>
		public DateTime Scheduled_Payment_Date{get; set;}
		 /// <summary>
		 /// 使用/还款情况_本月应还款
		 /// </summary>
		public decimal Scheduled_Payment_Amount{get; set;}
		 /// <summary>
		 /// 使用/还款情况_本月实还款
		 /// </summary>
		public decimal Actual_Payment_Amount{get; set;}
		 /// <summary>
		 /// 使用/还款情况_最近一次还款日期
		 /// </summary>
		public DateTime Recent_Pay_Date{get; set;}
		 /// <summary>
		 /// 当前逾期期数
		 /// </summary>
		public decimal Curr_Overdue_Cyc{get; set;}
		 /// <summary>
		 /// 当前逾期金额
		 /// </summary>
		public decimal Curr_Overdue_Amount{get; set;}
		 /// <summary>
		 /// 逾期31-60天未归还贷款本金
		 /// </summary>
		public decimal Overdue31_To60_Amount{get; set;}
		 /// <summary>
		 /// 逾期61-90天未归还贷款本金
		 /// </summary>
		public decimal Overdue61_To90_Amount{get; set;}
		 /// <summary>
		 /// 逾期91-180天未归还贷款本金
		 /// </summary>
		public decimal Overdue91_To180_Amount{get; set;}
		 /// <summary>
		 /// 逾期180天以上未归还贷款本金
		 /// </summary>
		public decimal OVERDUE_OVER180_AMOUNT{get; set;}
		 /// <summary>
		 /// 最近还款状态起始月
		 /// </summary>
		public string Payment_State_Begin_Month{get; set;}
		 /// <summary>
		 /// 最近还款状态截止月
		 /// </summary>
		public string Payment_State_End_Month{get; set;}
		 /// <summary>
		 /// 24个月还款状态
		 /// </summary>
		public string Payment_State{get; set;}
		 /// <summary>
		 /// 最近5年内的逾期记录起始月
		 /// </summary>
		public string Overdue_Record_Begin_Month{get; set;}
		 /// <summary>
		 /// 最近5年内的逾期记录截止月
		 /// </summary>
		public string Overdue_Record_End_Month{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
        public DateTime QCR_Issued_Time{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string QCR_Issued_Agency{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string QCR_curType{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public decimal QCR_Credit_Line{get; set;}

		 /// <summary>
		 /// 
		 /// </summary>
		public int Bh{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string CreditcardNo{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string CarDType{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string  CancellationDate{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public decimal AccountBalance{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public DateTime GetTime{get; set;}
	}
}