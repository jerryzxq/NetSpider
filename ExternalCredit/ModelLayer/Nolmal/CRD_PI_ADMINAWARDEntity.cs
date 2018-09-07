using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Vcredit.ExternalCredit.CommonLayer.Utility;
using Newtonsoft.Json;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_PI_ADMINAWARD")]
    [Schema("credit")]
    public partial class CRD_PI_ADMINAWARDEntity : BaseEntity
    {

        /// <summary>
        /// 奖励机构
        /// </summary>
        public string Organ_Name { get; set; }
        /// <summary>
        /// 奖励内容
        /// </summary>
        public string Content { get; set; }
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


    }
}