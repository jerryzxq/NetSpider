using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_QR_RECORDDTL")]
    [Schema("credit")]
    public partial class CRD_QR_RECORDDTLEntity : BaseEntity
    {

        [JsonConverter(typeof(DesJsonConverter))]

        /// <summary>
        /// 
        /// </summary>
        public DateTime? Query_Date { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Querier { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Query_Reason { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 三个月内信用卡审批查询次数
        /// </summary>
        public int? COUNT_CARD_IN3M { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 一个月内信用卡审批查询次数
        /// </summary>
        public int? COUNT_CARD_IN1M { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 三个月内贷款审批查询次数
        /// </summary>
        public int? COUNT_loan_IN3M { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 一个月内贷款审批查询次数
        /// </summary>
        public int? COUNT_loan_IN1M { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 
        /// </summary>
        public int? Bh { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreditcardNo { get; set; }
    }
}