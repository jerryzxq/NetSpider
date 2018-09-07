using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
    #region CRD_STAT_LNDEntity

    /// <summary>
    /// CRD_STAT_LNDEntity object for NHibernate mapped table 'CRD_STAT_LND'.
    /// </summary>
    public class CRD_STAT_LNDEntity
    {
        public virtual int Id { get; set; }

        /// <summary>
        /// 报告ID
        /// </summary>
        public virtual int? ReportId { get; set; }
        /// <summary>
        /// 最近九个月发放贷记卡平均金额
        /// </summary>
        public virtual decimal? M9AVGLENDAMOUNT { get; set; }
        /// <summary>
        /// 最近九个月发放贷记卡数量
        /// </summary>
        public virtual int? M9DELIVERCNT { get; set; }
        /// <summary>
        /// 最近九个月发放贷记卡平均金额
        /// </summary>
        public virtual decimal? M9LENDAMOUNT { get; set; }
        /// <summary>
        /// 信用卡发生过90天以上逾期的账户数(all)
        /// </summary>
        public virtual int? ALLCREDITDELAY90CNT { get; set; }
        /// <summary>
        /// 信用卡发生过逾期的账户数(all)
        /// </summary>
        public virtual int? ALLCREDITDELAYCNT { get; set; }
        /// <summary>
        /// 信用卡未销户账户数(all)
        /// </summary>
        public virtual int? ALLCREDITUNCLOSEDCNT { get; set; }
        /// <summary>
        /// 信用卡发生过逾期的账户数
        /// </summary>
        public virtual int? CREDITDELAYCNT { get; set; }
        /// <summary>
        /// 信用卡已使用总额度
        /// </summary>
        public virtual decimal? SUMUSEDCREDITLIMITAMOUNT { get; set; }
        /// <summary>
        /// 正常卡数量
        /// </summary>
        public virtual int? NORMALCARDNUM { get; set; }
        /// <summary>
        /// 最早卡龄
        /// </summary>
        public virtual int? CARDAGEMONTH { get; set; }
        /// <summary>
        /// 正常信用卡最大额度
        /// </summary>
        public virtual decimal? CREDITLIMITAMOUNTNORMMAX { get; set; }
        /// <summary>
        /// 正常信用卡未用额度
        /// </summary>
        public virtual decimal? NORMALCREDITBALANCE { get; set; }
        /// <summary>
        /// 正常信用卡使用率
        /// </summary>
        public virtual decimal? NORMALUSEDRATE { get; set; }
        /// <summary>
        /// 信用卡逾期金额
        /// </summary>
        public virtual decimal? CREDITDLQAMOUNT { get; set; }
        /// <summary>
        /// 正常信用卡最大已使用额度
        /// </summary>
        public virtual decimal? NORMALUSEDMAX { get; set; }
        /// <summary>
        /// 正常信用卡总信用额度
        /// </summary>
        public virtual decimal? SUMNORMALLIMITAMOUNT { get; set; }
        /// <summary>
        /// 正常卡最早卡龄
        /// </summary>
        public virtual int? NORMALCARDAGEMONTH { get; set; }

        //新增
        /// <summary>
        /// 贷记卡5年逾期的月数
        /// </summary>
        public virtual int?  ALL_CREDIT_DELAY_MONTH { get; set; }
        /// <summary>
        /// 贷记卡呆账数
        /// </summary>
        public virtual int? loand_Badrecord { get; set; }
        /// <summary>
        /// 准贷记卡呆账数
        /// </summary>
        public virtual int? stncard_Badrecord { get; set; }
        /// <summary>
        /// 准贷记卡使用额度
        /// </summary>
        public virtual decimal? StnCard_UseCreditLimit { get; set; }
        /// <summary>
        /// 贷记卡最大逾期次数占比
        /// </summary>
        public virtual decimal? lnd_max_overdue_percent { get; set; }
        /// <summary>
        /// 最早未销户卡龄
        /// </summary>
        public virtual int CRD_AGE_UNCLS_OLDEST { get; set; }
        /// <summary>
        /// 正常状态最大授信额度卡的卡龄
        /// </summary>
        public virtual int lnd_max_normal_Age { get; set; }
        /// <summary>
        /// 过去12个月开户的贷款账户最大单笔贷款余额过去12个月开户平均额度之比
        /// </summary>
        public virtual decimal L12M_LOANACT_USED_MAX_R { get; set; }
        /// <summary>
        /// 当前正常的信用卡账户最大负债额与透支余额之比的均值
        /// </summary>
        public virtual decimal NORM_CDT_BAL_USED_PCT_AVG { get; set; }

        /// <summary>
        /// 为他人担保，信用卡数
        /// </summary>
        public virtual int? ALL_CREDIT_FOROTHERS_CNT { get; set; }
        /// <summary>
        /// 信用卡账户数
        /// </summary>
        public virtual int? ALL_CREDIT_CNT { get; set; }
        /// <summary>
        /// 五年内最大逾期次数
        /// </summary>
        public virtual int? DLQ_5YR_CNT_MAX { get; set; }
        /// <summary>
        /// 最近正常贷记卡授信额度
        /// </summary>
        public virtual decimal? RCNT_CDT_LMT { get; set; }

        /// <summary>
        /// 最近未销户贷记卡额度
        /// </summary>
        public virtual decimal? UNCLS_RCNT_CDT_LMT { get; set; }
        /// <summary>
        /// 最早未销户贷记卡额度
        /// </summary>
        public virtual decimal? UNCLS_OLD_CDT_LMT { get; set; }
    }
    #endregion
}