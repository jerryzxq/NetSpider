using System;
using System.Collections.Generic;
using Vcredit.ExtTrade.CommonLayer;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExternalCredit.Dto.OrgCreditModel
{

    public partial class CRD_CD_LNDto : BaseDto
    {

        public decimal Loan_Id { get; set; }

        /// <summary>
        /// 基本信息提示
        /// </summary>
        public string Cue { get; set; }
        /// <summary>
        /// 账户状态
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 贷款机构
        /// </summary>
        public string Finance_Org { get; set; }
        /// <summary>
        /// 业务号
        /// </summary>
        public string Account_Dw { get; set; }
        /// <summary>
        /// 贷款种类细分
        /// </summary>
        public string Type_Dw { get; set; }
        /// <summary>
        /// 币种
        /// </summary>
        public string Currency { get; set; }
      
        /// <summary>
        /// 发放日期
        /// </summary>
        public DateTime? Open_Date { get; set; }
      
        /// <summary>
        /// 到期日期
        /// </summary>
        public DateTime? End_Date { get; set; }
      
        /// <summary>
        /// 合同金额
        /// </summary>
        public decimal? Credit_Limit_Amount { get; set; }
        /// <summary>
        /// 担保方式
        /// </summary>
        public string Guarantee_Type { get; set; }
        /// <summary>
        /// 还款频率
        /// </summary>
        public string Payment_Rating { get; set; }
        /// <summary>
        /// 还款期数
        /// </summary>
        public string Payment_Cyc { get; set; }
      
        /// <summary>
        /// 状态截止日
        /// </summary>
        public DateTime? State_End_Date { get; set; }
        /// <summary>
        /// 状态截止月/转出月
        /// </summary>
        public string State_End_Month { get; set; }
        /// <summary>
        /// 五级分类
        /// </summary>
        public string Class5_State { get; set; }
      
        /// <summary>
        /// 本金余额
        /// </summary>
        public decimal? Balance { get; set; }
      
        /// <summary>
        /// 剩余还款期数
        /// </summary>
        public decimal? Remain_Payment_Cyc { get; set; }
      
        /// <summary>
        /// 本月应还款
        /// </summary>
        public decimal? Scheduled_Payment_Amount { get; set; }
      
        /// <summary>
        /// 应还款日
        /// </summary>
        public DateTime? Scheduled_Payment_Date { get; set; }
      
        /// <summary>
        /// 本月实还款
        /// </summary>
        public decimal? Actual_Payment_Amount { get; set; }
      
        /// <summary>
        /// 最近一次还款日期
        /// </summary>
        public DateTime? Recent_Pay_Date { get; set; }
      
        /// <summary>
        /// 当前逾期期数
        /// </summary>
        public decimal? Curr_Overdue_Cyc { get; set; }
      
        /// <summary>
        /// 当前逾期金额
        /// </summary>
        public decimal? Curr_Overdue_Amount { get; set; }
      
        /// <summary>
        /// 逾期31-60天未归还贷款本金
        /// </summary>
        public decimal? Overdue31_To60_Amount { get; set; }
      
        /// <summary>
        /// 逾期61-90天未归还贷款本金
        /// </summary>
        public decimal? Overdue61_To90_Amount { get; set; }
      
        /// <summary>
        /// 逾期91-180天未归还贷款本金
        /// </summary>
        public decimal? Overdue91_To180_Amount { get; set; }
      
        /// <summary>
        /// 逾期180天以上未归还贷款本金
        /// </summary>
        public decimal? Overdue_Over180_Amount { get; set; }

        /// <summary>
        /// 24个月还款状态
        /// </summary>
        public string Payment_State { get; set; }

      
     


        /// <summary>
        /// 
        /// </summary>
        public DateTime? GetTime { get; set; }

    }
}