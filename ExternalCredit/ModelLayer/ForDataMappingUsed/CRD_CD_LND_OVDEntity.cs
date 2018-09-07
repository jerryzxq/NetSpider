using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_CD_LND_OVD")]
    [Schema("credit")]
    public partial class CRD_CD_LND_OVDEntity
    {
        [Ignore]
        public decimal Loancard_Overdue_Id { get; set; }
        /// <summary>
        /// 信用报告主表ID
        /// </summary>
        public decimal Report_Id { get; set; }
        /// <summary>
        /// 贷记卡ID
        /// </summary>
        [Alias("Loancard_Id")]
        public decimal Card_Id { get; set; }
        /// <summary>
        /// 逾期月份
        /// </summary>
        public string Month_Dw { get; set; }
        /// <summary>
        /// 逾期持续月数
        /// </summary>
        public decimal Last_Months { get; set; }
        /// <summary>
        /// 逾期金额
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// PCQS更新时间
        /// </summary>
        [Ignore]
        public DateTime Time_Stamp { get; set; }
        [Ignore]
        public byte[] TIMESTAMP { get; set; }

    }
}