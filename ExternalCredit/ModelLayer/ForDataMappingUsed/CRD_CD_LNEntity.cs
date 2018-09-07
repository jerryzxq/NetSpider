using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Vcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_CD_LN")]
    [Schema("credit")]
    public partial class CRD_CD_LNEntity : BaseEntity
	{
        public CRD_CD_LNEntity()
        {
            GetTime =CommonData.defaultTime;
            Issued_Time = CommonData.defaultTime;
            State_End_Date = CommonData.defaultTime;
            Open_Date = CommonData.defaultTime;
            End_Date = CommonData.defaultTime;
            Scheduled_Payment_Date = CommonData.defaultTime;
            Recent_Pay_Date = CommonData.defaultTime;

        }
        private List<CRD_CD_LN_OVDEntity> lnoverList = new List<CRD_CD_LN_OVDEntity>();

        [Ignore]
        /// <summary>
        /// 贷款逾期
        /// </summary>
        public List<CRD_CD_LN_OVDEntity> LnoverList
        {
            get { return lnoverList; }
            set { lnoverList = value; }
        }

        private List<CRD_CD_LN_SPLEntity> lnSPLList = new List<CRD_CD_LN_SPLEntity>();
        [Ignore]
        /// <summary>
        /// 特殊交易
        /// </summary>
        public List<CRD_CD_LN_SPLEntity> LnSPLList
        {
            get { return lnSPLList; }
            set { lnSPLList = value; }
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
		 /// 贷款机构
		 /// </summary>
		public string Finance_Org{get; set;}
		 /// <summary>
		 /// 业务号
		 /// </summary>
		public string Account_Dw{get; set;}
		 /// <summary>
		 /// 贷款种类细分
		 /// </summary>
		public string Type_Dw{get; set;}
		 /// <summary>
		 /// 币种
		 /// </summary>
		public string Currency{get; set;}
		 /// <summary>
		 /// 发放日期
		 /// </summary>
		public DateTime Open_Date{get; set;}
		 /// <summary>
		 /// 到期日期
		 /// </summary>
		public DateTime End_Date{get; set;}
		 /// <summary>
		 /// 合同金额
		 /// </summary>
		public decimal Credit_Limit_Amount{get; set;}
		 /// <summary>
		 /// 担保方式
		 /// </summary>
		public string Guarantee_Type{get; set;}
		 /// <summary>
		 /// 还款频率
		 /// </summary>
		public string Payment_Rating{get; set;}
		 /// <summary>
		 /// 还款期数
		 /// </summary>
		public string Payment_Cyc{get; set;}
		 /// <summary>
		 /// 状态截止日
		 /// </summary>
        public DateTime State_End_Date{get; set;}
		 /// <summary>
		 /// 状态截止月/转出月
		 /// </summary>
		public string State_End_Month{get; set;}
		 /// <summary>
		 /// 五级分类
		 /// </summary>
		public string Class5_State{get; set;}
		 /// <summary>
		 /// 本金余额
		 /// </summary>
		public decimal Balance{get; set;}
		 /// <summary>
		 /// 剩余还款期数
		 /// </summary>
		public decimal Remain_Payment_Cyc{get; set;}
		 /// <summary>
		 /// 本月应还款
		 /// </summary>
		public decimal Scheduled_Payment_Amount{get; set;}
		 /// <summary>
		 /// 应还款日
		 /// </summary>
		public DateTime Scheduled_Payment_Date{get; set;}
		 /// <summary>
		 /// 本月实还款
		 /// </summary>
		public decimal Actual_Payment_Amount{get; set;}
		 /// <summary>
		 /// 最近一次还款日期
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
		public decimal Overdue_Over180_Amount{get; set;}
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
        public DateTime Issued_Time{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string Issued_Agency{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public decimal Loan_Amount{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string Loan_Kind{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string Guar_Kind{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public int Loan_Period{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string Repay_Mothed{get; set;}

		 /// <summary>
		 /// 
		 /// </summary>
		public int Bh{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D24{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D23{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D22{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D21{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D20{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D19{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D18{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D17{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D16{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D15{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D14{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D13{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D12{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D11{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D10{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D9{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D8{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D7{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D6{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D5{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D4{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D3{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D2{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string D1{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string CreditcardNo{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string  SettlementDate{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public decimal LoadBalance{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public DateTime GetTime{get; set;}
        /// <summary>
        /// 银行类型
        /// </summary>
        public string FinanceType { get; set; }
	}
}