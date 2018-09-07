using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_PI_ADMINPNSHM")]
    [Schema("credit")]
    public partial class CRD_PI_ADMINPNSHMEntity : BaseEntity
    {



        /// <summary>
        /// 处罚机构
        /// </summary>
        public string Organ_Name { get; set; }
        /// <summary>
        /// 处罚内容
        /// </summary>
        public string Content { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 处罚金额
        /// </summary>
        public decimal? Money { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 生效日期
        /// </summary>
        public DateTime? Begin_Date { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 截止日期
        /// </summary>
        public DateTime? End_Date { get; set; }
        /// <summary>
        /// 行政复议结果
        /// </summary>
        public string Result_Dw { get; set; }


    }
}