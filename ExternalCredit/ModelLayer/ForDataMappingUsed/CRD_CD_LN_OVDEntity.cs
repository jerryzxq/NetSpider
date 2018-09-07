using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_CD_LN_OVD")]
    [Schema("credit")]
    public partial class CRD_CD_LN_OVDEntity
    {
        [Ignore]
        public decimal Loan_Overdue_Id { get; set; }
        /// <summary>
        /// 信用报告主表ID
        /// </summary>
        public decimal Report_Id { get; set; }
        /// <summary>
        /// 贷款ID
        /// </summary>
        [Alias("Loan_Id")]
        public decimal Card_Id { get; set; }
        /// <summary>
        /// 月份
        /// </summary>
        public string Month_Dw { get; set; }
        /// <summary>
        /// 持续月数
        /// </summary>
        public decimal Last_Months { get; set; }
        /// <summary>
        /// 金额
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