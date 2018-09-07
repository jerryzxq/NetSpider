using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.Service
{
    public class CRD_STAT_LN
    {
        /// <summary>
        /// 住房贷款发生过90天以上逾期的账户数(all)
        /// </summary>
        public int? ALLLOANHOUSEDELAY90CNT { get; set; }
        /// <summary>
        /// 住房贷款发生过逾期的账户数(all)
        /// </summary>
        public int? ALLLOANHOUSEDELAYCNT { get; set; }
        /// <summary>
        /// 其他贷款发生过90天以上逾期的账户数(all)
        /// </summary>
        public int? ALLLOANOTHERDELAY90CNT { get; set; }
        /// <summary>
        /// 其他贷款发生过逾期的账户数(all)
        /// </summary>
        public int? ALLLOANOTHERDELAYCNT { get; set; }
        /// <summary>
        /// 其他贷款未销户账户数(all)
        /// </summary>
        public int? ALLLOANOTHERUNCLOSEDCNT { get; set; }
        /// <summary>
        /// 贷款逾期90天以上账户数
        /// </summary>
        public int? LOANDELAY90CNT { get; set; }
        /// <summary>
        /// 最早贷龄
        /// </summary>
        public int? LOANAGEMONTH { get; set; }
        /// <summary>
        /// 贷款逾期金额
        /// </summary>
        public decimal? LOANDLQAMOUNT { get; set; }
        /// <summary>
        /// 贷款每月还款本金金额
        /// </summary>
        public decimal? LOANPMTMONTHLY { get; set; }
        /// <summary>
        /// 其他贷款账户数(all)
        /// </summary>
        public int? ALLLOANOTHERCNT { get; set; }
        /// <summary>
        /// 贷款本金总额
        /// </summary>
        public decimal? SUMLOANLIMITAMOUNT { get; set; }
        /// <summary>
        /// 贷款总余额
        /// </summary>
        public decimal? SUMLOANBALANCE { get; set; }
        /// <summary>
        /// 房贷每月还款本金金额
        /// </summary>
        public decimal? LOAN_HOUSE_DLQ_AMOUNT { get; set; }
        /// <summary>
        /// 正常状态最早贷龄
        /// </summary>
        public int? NORMALLOANAGEMONTH { get; set; }
    }
}