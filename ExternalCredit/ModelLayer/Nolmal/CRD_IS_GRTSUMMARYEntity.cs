using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_IS_GRTSUMMARY")]
    [Schema("credit")]
    public partial class CRD_IS_GRTSUMMARYEntity : BaseEntity
    {
        [JsonConverter(typeof(DesJsonConverter))]

        /// <summary>
        /// 担保笔数
        /// </summary>
        public decimal? Guarantee_Count { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 担保金额
        /// </summary>
        public decimal? Guarantee_Amount { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 担保本金余额
        /// </summary>
        public decimal? Guarantee_Balance { get; set; }


    }
}