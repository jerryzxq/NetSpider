
using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;

namespace Vcredit.ExternalCredit.CrawlerLayer.CreditVariable
{
    #region CRD_STAT_LNDEntity

    public class CRD_STAT_LNDEntity
    {
        /// <summary>
        /// 报告ID
        /// </summary>
        public int? ReportId { get; set; }

        /// <summary>
        /// 最近九个月发放贷记卡平均金额()
        /// </summary>
        public decimal? M9AVGLENDAMOUNT { get; set; }

        /// <summary>
        /// 最近九个月发放贷记卡数量()
        /// </summary>
        public int? M9DELIVERCNT { get; set; }

        /// <summary>
        /// 最近九个月发放贷记卡平均金额()
        /// </summary>
        public decimal? M9LENDAMOUNT { get; set; }

        /// <summary>
        /// 信用卡发生过90天以上逾期的账户数(all)()
        /// </summary>
        public int? ALLCREDITDELAY90CNT { get; set; }

        /// <summary>
        /// 信用卡发生过逾期的账户数(all)()
        /// </summary>
        public int? ALLCREDITDELAYCNT { get; set; }

        /// <summary>
        /// 信用卡未销户账户数(all)()
        /// </summary>
        public int? ALLCREDITUNCLOSEDCNT { get; set; }

        /// <summary>
        /// 信用卡发生过逾期的账户数()
        /// </summary>
        public int? CREDITDELAYCNT { get; set; }

        /// <summary>
        /// 信用卡已使用总额度()
        /// </summary>
        public decimal? SUMUSEDCREDITLIMITAMOUNT { get; set; }
    
        /// <summary>
        /// 正常卡数量()
        /// </summary>
        public int? NORMALCARDNUM { get; set; }

        /// <summary>
        /// 最早卡龄()
        /// </summary>
        public int? CARDAGEMONTH { get; set; }

        /// <summary>
        /// 正常信用卡最大额度()
        /// </summary>
        public decimal? CREDITLIMITAMOUNTNORMMAX { get; set; }

        /// <summary>
        /// 正常信用卡未用额度()
        /// </summary>
        public decimal? NORMALCREDITBALANCE { get; set; }

        /// <summary>
        /// 正常信用卡使用率()
        /// </summary>
        public decimal? NORMALUSEDRATE { get; set; }

        /// <summary>
        /// 信用卡逾期金额()
        /// </summary>
        public decimal? CREDITDLQAMOUNT { get; set; }

        /// <summary>
        /// 正常信用卡最大已使用额度()
        /// </summary>
        public decimal? NORMALUSEDMAX { get; set; }

        /// <summary>
        /// 正常信用卡总信用额度()
        /// </summary>
        public decimal? SUMNORMALLIMITAMOUNT { get; set; }

        /// <summary>
        /// 正常卡最早卡龄
        /// </summary>
        public int? NORMALCARDAGEMONTH { get; set; }

        //[Alias("ALL_CREDIT_DELAY_MONTH")]
        //新增
        /// <summary>
        /// 贷记卡5年逾期的月数
        /// </summary>
        public int? ALL_CREDIT_DELAY_MONTH { get; set; }

        /// <summary>
        /// 贷记卡呆账数
        /// </summary>
        public int? loand_Badrecord { get; set; }

        /// <summary>
        /// 准贷记卡呆账数
        /// </summary>
        public int? stncard_Badrecord { get; set; }

        /// <summary>
        /// 准贷记卡使用额度
        /// </summary>
        public decimal? StnCard_UseCreditLimit { get; set; }

        /// <summary>
        /// 贷记卡最大逾期次数占比
        /// </summary>
        public decimal? lnd_max_overdue_percent { get; set; }
 
        /// <summary>
        /// 最早未销户卡龄
        /// </summary>
        public int CRD_AGE_UNCLS_OLDEST { get; set; }

        /// <summary>
        /// 正常状态最大授信额度卡的卡龄
        /// </summary>
        public int lnd_max_normal_Age { get; set; }

        /// <summary>
        /// 过去12个月开户的贷款账户最大单笔贷款余额过去12个月开户平均额度之比
        /// </summary>
        public decimal L12M_LOANACT_USED_MAX_R { get; set; }

        /// <summary>
        /// 当前正常的信用卡账户最大负债额与透支余额之比的均值
        /// </summary>
        public decimal NORM_CDT_BAL_USED_PCT_AVG { get; set; }
        /// <summary>
        /// 为他人担保，信用卡数
        /// </summary>
        public int? ALL_CREDIT_FOROTHERS_CNT { get; set; }
        /// <summary>
        /// 信用卡账户数
        /// </summary>
        public int? ALL_CREDIT_CNT { get; set; }

        /// <summary>
        /// 五年内最大逾期次数
        /// </summary>
        public double? DLQ_5YR_CNT_MAX { get; set; }

        /// <summary>
        /// 最近正常贷记卡授信额度
        /// </summary>
        public decimal? RCNT_CDT_LMT { get; set; }

        /// <summary>
        /// 最近未销户贷记卡额度
        /// </summary>
        public decimal? UNCLS_RCNT_CDT_LMT { get; set; }

        /// <summary>
        /// 最早未销户贷记卡额度
        /// </summary>
        public decimal? UNCLS_OLD_CDT_LMT { get; set; }

        //=============豆豆钱新增6个变量=============

        /// <summary>
        /// 五年内平均逾期次数
        /// </summary>
        public decimal? DLQ_5YR_CNT_AVG { get; set; }

        /// <summary>
        /// 当前正常的信用卡账户最高额度
        /// </summary>
        public decimal? NORM_CDT_LMT_MAX { get; set; }

        /// <summary>
        /// 正常卡已使用总额度
        /// </summary>
        public decimal? NORM_USED_SUM { get; set; }

        /// <summary>
        /// 正常非共享信用卡已使用总额度
        /// </summary>
        public decimal? NORM_CDT_SUM { get; set; }

        /// <summary>
        /// 当前正常的非共享信用卡账户开户数
        /// </summary>
        public decimal? NOW_NORM_UNI_ACCT_NUM { get; set; }

        /// <summary>
        /// [征信]信用卡最早卡龄
        /// </summary>
        public int CREDIT_CARD_AGE_OLDEST { get; set; }

        //===========================================

        //=================账户数量==================

        ///// <summary>
        ///// 未销户最早最近授信额度差
        ///// </summary>
        //public  decimal? UNCLS_RCNT_OLD_CDT_LMT_RNG { get; set; }

        [Ignore]
        public virtual int CardNum { get; set; }
        /// <summary>
        /// [征信]近24个月发放信用卡次数
        /// </summary>
        public int? M24_DELIVER_CNT{ get; set; }
        /// <summary>
        /// [征信]信用卡24个月最大正常还款次数
        /// </summary>
        public int? CREDIT_NORMAL_PAY_MAX { get; set; }
        /// <summary>
        /// [征信]正常信用卡平均额度
        /// </summary>
        public decimal? AVG_NORMAL_LIMIT_AMOUNT{ get; set; }
        /// <summary>
        /// [征信]近24个月发放信用卡总额度
        /// </summary>
        public decimal? M24_LEND_AMOUNT { get; set; }

        
    }
    #endregion
}