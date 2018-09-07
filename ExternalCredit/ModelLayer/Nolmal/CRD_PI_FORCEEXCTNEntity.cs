using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Vcredit.ExternalCredit.CommonLayer.Utility;
using Newtonsoft.Json;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_PI_FORCEEXCTN")]
    [Schema("credit")]
    public partial class CRD_PI_FORCEEXCTNEntity : BaseEntity
    {


        /// <summary>
        /// 执行法院
        /// </summary>
        public string Court { get; set; }
        /// <summary>
        /// 执行案由
        /// </summary>
        public string Case_Reason { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 立案日期
        /// </summary>
        public DateTime? Register_Date { get; set; }
        /// <summary>
        /// 结案方式
        /// </summary>
        public string Closed_Type { get; set; }
        /// <summary>
        /// 案件状态
        /// </summary>
        public string Case_State { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 结案日期
        /// </summary>
        public DateTime? Closed_Date { get; set; }
        /// <summary>
        /// 申请执行标的
        /// </summary>
        public string Enforce_Object { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 申请执行标的金额
        /// </summary>
        public decimal? Enforce_Object_Money { get; set; }
        /// <summary>
        /// 已执行标的
        /// </summary>
        public string Already_Enforce_Object { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 已执行标的金额
        /// </summary>
        public decimal? Already_Enforce_Object_Money { get; set; }


    }
}