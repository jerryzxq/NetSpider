using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_IS_OVDSUMMARY")]
    [Schema("credit")]
    public partial class CRD_IS_OVDSUMMARYEntity : BaseEntity
    {

        /// <summary>
        /// 逾期信息类型
        /// </summary>
        public string Type_Dw { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 笔数/账户数
        /// </summary>
        public decimal? Count_Dw { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 月份数
        /// </summary>
        public decimal? Months { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 单月最高逾期总额/单月最高透支总额
        /// </summary>
        public decimal? Highest_Oa_Per_Mon { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 最长逾期月数/最长透支月数
        /// </summary>
        public decimal? Max_Duration { get; set; }
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