using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_CD_NoCancellSTNCARD")]
    [Schema("credit")]
    public partial class CRD_CD_NoCancellSTNCARDEntity : BaseEntity
    {

        [JsonConverter(typeof(DesJsonConverter))]

        /// <summary>
        /// 
        /// </summary>
        public int? FinanceMan_OrgNum { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 
        /// </summary>
        public int? Finance_OrgNum { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 
        /// </summary>
        public int? AccountNum { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 
        /// </summary>
        public decimal? TotalCreditAmount { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 
        /// </summary>
        public decimal? MaxCreditAmount { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 
        /// </summary>
        public decimal? MinimumCreditAmount { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 
        /// </summary>
        public decimal? OverDueNum { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 
        /// </summary>
        public decimal? AverageRecent6Months { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 
        /// </summary>
        [Ignore]
        public DateTime? GetTime { get; set; }
    }
}