using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_CD_GUARANTEESummery")]
    [Schema("credit")]
    public partial class CRD_CD_GUARANTEESummeryEntity : BaseEntity
    {
        [JsonConverter(typeof(DesJsonConverter))]

        /// <summary>
        /// 
        /// </summary>
        public int? GuaranteeNum { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 
        /// </summary>
        public decimal? GuaranteeMoney { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 
        /// </summary>
        public decimal? PrincipalBalance { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 
        /// </summary>
        public DateTime? GetTime { get; set; }
    }
}