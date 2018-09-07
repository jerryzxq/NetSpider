
using System;
using System.Collections.Generic;

namespace Vcredit.ExternalCredit.CrawlerLayer.CreditVariable
{
    #region CRD_STAT_LNEntity


    public class CRD_STAT_LNEntity
    {
        /// <summary>
        /// 报告ID
        /// </summary>
        public int? ReportId { get; set; }
        /// <summary>
        /// 住房贷款发生过90天以上逾期的账户数(all)()
        /// </summary>
        public int? ALLLOANHOUSEDELAY90CNT { get; set; }
        /// <summary>
        /// 住房贷款发生过逾期的账户数(all)()
        /// </summary>
        public int? ALLLOANHOUSEDELAYCNT { get; set; }
        /// <summary>
        /// 住房贷款未销户账户数(all)()
        /// </summary>
        public int? ALLLOANHOUSEUNCLOSEDCNT { get; set; }
        /// <summary>
        /// 其他贷款发生过90天以上逾期的账户数(all)()
        /// </summary>
        public int? ALLLOANOTHERDELAY90CNT { get; set; }
        /// <summary>
        /// 其他贷款发生过逾期的账户数(all)()
        /// </summary>
        public int? ALLLOANOTHERDELAYCNT { get; set; }
        /// <summary>
        /// 其他贷款未销户账户数(all)()
        /// </summary>
        public int? ALLLOANOTHERUNCLOSEDCNT { get; set; }
        /// <summary>
        /// 贷款逾期90天以上账户数()
        /// </summary>
        public int? LOANDELAY90CNT { get; set; }
        /// <summary>
        /// 最早贷龄()
        /// </summary>
        public int? LOANAGEMONTH { get; set; }
        /// <summary>
        /// 贷款逾期金额()
        /// </summary>
        public decimal? LOANDLQAMOUNT { get; set; }
        /// <summary>
        /// 贷款每月还款本金金额()
        /// </summary>
        public decimal? LOANPMTMONTHLY { get; set; }
        /// <summary>
        /// 其他贷款账户数(all)()
        /// </summary>
        public int? ALLLOANOTHERCNT { get; set; }

        /// <summary>
        /// 贷款本金总额()
        /// </summary>
        public decimal? SUMLOANLIMITAMOUNT { get; set; }
        /// <summary>
        /// 贷款总余额()
        /// </summary>
        public decimal? SUMLOANBALANCE { get; set; }
        /// <summary>
        /// 房贷每月还款本金金额()
        /// </summary>
        public decimal? LOAN_HOUSE_DLQ_AMOUNT { get; set; }
        /// <summary>
        /// 正常状态最早贷龄()
        /// </summary>
        public int? NORMALLOANAGEMONTH { get; set; }
        /// <summary>
        /// 个人住房公积金贷款额
        /// </summary>
        public decimal? ln_housing_fund_amount { get; set; }
        /// <summary>
        /// 个人住房商铺贷款额
        /// </summary>
        public decimal? ln_shopfront_amount { get; set; }
        /// <summary>
        /// 个人住房按揭贷款额
        /// </summary>
        public decimal? ln_housing_mortgage_amount { get; set; }
        //新增
        /// <summary>
        /// 贷款5年逾期的月数 
        /// </summary>
        public int? ALL_LOAN_DELAY_MONTH { get; set; }
        /// <summary>
        /// 最近6个月平均应还款
        /// </summary>
        public decimal? ln_Latest_6M_Used_Avg_Amount { get; set; }
        /// <summary>
        /// 状态为正常的贷款笔数  
        /// </summary>
        public int? ln_normal_count { get; set; }
        /// <summary>
        /// 过去24个月内非共享信用卡账户开户数占当前正常账户比例
        /// </summary>
        public decimal L24M_OPE_NORM_ACCT_PCT { get; set; }

        /// <summary>
        /// 为他人担保贷款，购房贷款
        /// </summary>
        public int? ALL_LOAN_HOUSE_FOROTHERS_CNT { get; set; }
        /// <summary>
        /// 为他人担保贷款，其他贷款
        /// </summary>
        public int? ALL_LOAN_OTHER_FOROTHERS_CNT { get; set; }

        /// <summary>
        /// 住房贷款账户数
        /// </summary>
        public int? ALL_LOAN_HOUSE_CNT { get; set; }
        /// <summary>
        /// 银行信贷涉及的账户数
        /// </summary>
        public int? ACCT_NUM { get; set; }
        /// <summary>
        /// 过去贷款结清数
        /// </summary>
        public int? LST_LOAN_CLS_CNT { get; set; }
        /// <summary>
        /// 过去贷款开户数
        /// </summary>
        public int? LST_LOAN_OPE_CNT { get; set; }

        //20160524 需求新增
        /// <summary>
        /// 月按揭还款总额
        /// </summary>
        public decimal? Monthly_Mortgage_Payment_Total { get; set; }
        /// <summary>
        /// 最大月按揭还款额
        /// </summary>
        public decimal? Monthly_Mortgage_Payment_Max { get; set; }
        /// <summary>
        /// 商用房月按揭还款总额
        /// </summary>
        public decimal? Monthly_Commercial_Mortgage_Payment_Total { get; set; }
        /// <summary>
        /// 最大商用房月按揭还款额
        /// </summary>
        public decimal? Monthly_Commercial_Mortgage_Payment_Max { get; set; }
        /// <summary>
        /// 其他贷款额
        /// </summary>
        public decimal? ln_other_amount { get; set; }
        /// <summary>
        /// 保证人代偿总额
        /// </summary>
        public decimal assurerrepay_amount { get; set; }
        /// <summary>
        /// 其他贷款月按揭还款总额
        /// </summary>
        public virtual decimal? Monthly_Other_Mortgage_Payment_Total { get; set; }
        //20160617 需求新增
        /// <summary>
        /// 最大公积金月按揭还款额
        /// </summary>
        public virtual decimal? Max_AccumFund_Mon_Mort_Repay { get; set; }
        /// <summary>
        /// 公积金月按揭还款总额
        /// </summary>
        public virtual decimal? Max_AccumFund_All_Mort_Repay { get; set; }

        ///// <summary>
        ///// 过去结清贷款与开户贷款数比
        ///// </summary>
        //public decimal? LST_LOAN_CLS_CHU_OPE { get; set; }

    }
    #endregion
}