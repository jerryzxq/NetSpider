using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_CD_LN_OVD")]
    [Schema("credit")]
    public partial class CRD_CD_LN_OVDEntity
    {
        [Ignore]
        public decimal Loan_Overdue_Id { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        [Ignore]
        /// <summary>
        /// 编号
        /// </summary>
        public int? Number { get; set; }
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
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 持续月数
        /// </summary>
        public decimal? Last_Months { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 金额
        /// </summary>
        public decimal? Amount { get; set; }
        /// <summary>
        /// PCQS更新时间
        /// </summary>
        [Ignore]
        public DateTime? Time_Stamp { get; set; }
        [Ignore]
        public byte[] TIMESTAMP { get; set; }
    }
}