using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_PI_SALVATION")]
    [Schema("credit")]
    public partial class CRD_PI_SALVATIONEntity : BaseEntity
    {



        /// <summary>
        /// 人员类别
        /// </summary>
        public string Personnel_Type { get; set; }
        /// <summary>
        /// 所在地
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// 工作单位
        /// </summary>
        public string Organ_Name { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 家庭月收入
        /// </summary>
        public decimal? Money { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 申请日期
        /// </summary>
        public DateTime? Register_Date { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 批准日期
        /// </summary>
        public DateTime? Pass_Date { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 信息更新日期
        /// </summary>
        public DateTime? Get_Time { get; set; }


    }
}