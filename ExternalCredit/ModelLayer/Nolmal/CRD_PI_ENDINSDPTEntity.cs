using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_PI_ENDINSDPT")]
    [Schema("credit")]
    public partial class CRD_PI_ENDINSDPTEntity : BaseEntity
    {



        /// <summary>
        /// 参保地
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// 参保日期
        /// </summary>
        public string Register_Date { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 累计缴费月数
        /// </summary>
        public decimal? Month_Duration { get; set; }
        /// <summary>
        /// 参加工作月份
        /// </summary>
        public string Work_Date { get; set; }
        /// <summary>
        /// 缴费状态
        /// </summary>
        public string State { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 个人缴费基数
        /// </summary>
        public decimal? Own_Basic_Money { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 本月缴费金额
        /// </summary>
        public decimal? Money { get; set; }
        /// <summary>
        /// 缴费单位
        /// </summary>
        public string Organ_Name { get; set; }
        /// <summary>
        /// 中断或终止缴费原因
        /// </summary>
        public string Pause_Reason { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 信息更新日期
        /// </summary>
        public DateTime? Get_Time { get; set; }


    }
}