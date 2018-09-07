using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_CD_OverDueBreake")]
    [Schema("credit")]
    public partial class CRD_CD_OverDueBreakeEntity : BaseEntity
    {

        [JsonConverter(typeof(DesJsonConverter))]

        /// <summary>
        /// 
        /// </summary>
        public int? BadBebtNum { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 
        /// </summary>
        public decimal? BadBebtMoney { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 
        /// </summary>
        public int? AssetDisposalNum { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 
        /// </summary>
        public decimal? AssetDisposalBalance { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 
        /// </summary>
        public int? GuarantorCompensatoryNum { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 
        /// </summary>
        public decimal? GuarantorCompensatoryBalance { get; set; }


        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 
        /// </summary>
        [Ignore]
        public DateTime? GetTime { get; set; }
    }
}