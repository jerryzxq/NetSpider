using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace  Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_CD_ASRREPAY")]
    [Schema("credit")]
    public partial class CRD_CD_ASRREPAYEntity : BaseEntity
    {


        /// <summary>
        /// 代偿机构
        /// </summary>
        public string Organ_Name { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 最近一次代偿日期
        /// </summary>
        public DateTime? Latest_Assurer_Repay_Date { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 累计代偿金额
        /// </summary>
        public decimal? Money { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 最近一次还款日期
        /// </summary>
        public DateTime? Latest_Repay_Date { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 余额
        /// </summary>
        public decimal? Balance { get; set; }


    }
}