using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.Service
{
    public class CRD_STAT_LND
    {
        /// <summary>
        /// 最近九个月发放贷记卡平均金额
        /// </summary>
        public decimal? M9AVGLENDAMOUNT { get; set; }
        /// <summary>
        /// 最近九个月发放贷记卡数量
        /// </summary>
        public int? M9DELIVERCNT { get; set; }
        /// <summary>
        /// 最近九个月发放贷记卡平均金额
        /// </summary>
        public decimal? M9LENDAMOUNT { get; set; }
        /// <summary>
        /// 信用卡发生过90天以上逾期的账户数(all)
        /// </summary>
        public int? ALLCREDITDELAY90CNT { get; set; }
        /// <summary>
        /// 信用卡发生过逾期的账户数(all)
        /// </summary>
        public int? ALLCREDITDELAYCNT { get; set; }
        /// <summary>
        /// 信用卡未销户账户数(all)
        /// </summary>
        public int? ALLCREDITUNCLOSEDCNT { get; set; }
        /// <summary>
        /// 信用卡发生过逾期的账户数
        /// </summary>
        public int? CREDITDELAYCNT { get; set; }
        /// <summary>
        /// 信用卡已使用总额度
        /// </summary>
        public decimal? SUMUSEDCREDITLIMITAMOUNT { get; set; }
        /// <summary>
        /// 正常卡数量
        /// </summary>
        public int? NORMALCARDNUM { get; set; }
        /// <summary>
        /// 最早卡龄
        /// </summary>
        public int? CARDAGEMONTH { get; set; }
        /// <summary>
        /// 正常信用卡最大额度
        /// </summary>
        public decimal? CREDITLIMITAMOUNTNORMMAX { get; set; }
        /// <summary>
        /// 正常信用卡未用额度
        /// </summary>
        public decimal? NORMALCREDITBALANCE { get; set; }
        /// <summary>
        /// 正常信用卡使用率
        /// </summary>
        public decimal? NORMALUSEDRATE { get; set; }
        /// <summary>
        /// 信用卡逾期金额
        /// </summary>
        public decimal? CREDITDLQAMOUNT { get; set; }
        /// <summary>
        /// 正常信用卡最大已使用额度
        /// </summary>
        public decimal? NORMALUSEDMAX { get; set; }
        /// <summary>
        /// 正常信用卡总信用额度
        /// </summary>
        public decimal? SUMNORMALLIMITAMOUNT { get; set; }
        /// <summary>
        /// 正常卡最早卡龄
        /// </summary>
        public int? NORMALCARDAGEMONTH { get; set; }
    }
}