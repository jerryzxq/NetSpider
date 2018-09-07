using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_CD_STN_OVD")]
    [Schema("credit")]
    public partial class CRD_CD_STN_OVDEntity
    {
                [JsonConverter(typeof(DesJsonConverter))]
        [Ignore]
        /// <summary>
        /// 编号
        /// </summary>
        public int? Number { get; set; }
        [Ignore]
        public decimal Standardloan_Overdue_Id { get; set; }
        /// <summary>
        /// 信用报告主表ID
        /// </summary>
        public decimal Report_Id { get; set; }
        /// <summary>
        /// 准贷记卡信息段ID
        /// </summary>
        [Alias("Standardloancard_Id")]
        public decimal Card_Id { get; set; }
        /// <summary>
        /// 透支月份
        /// </summary>
        public string Month_Dw { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 透支持续月数
        /// </summary>
        public decimal? Last_Months { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 透支金额
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// PCQS更新时间
        /// </summary>
        [Ignore]
        public DateTime? Time_Stamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Ignore]
        public byte[] TIMESTAMP { get; set; }
    }
}