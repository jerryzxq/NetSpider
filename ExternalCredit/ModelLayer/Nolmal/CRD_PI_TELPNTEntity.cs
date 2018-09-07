using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_PI_TELPNT")]
    [Schema("credit")]
    public partial class CRD_PI_TELPNTEntity : BaseEntity
    {


        /// <summary>
        /// 电信运营商
        /// </summary>
        public string Organ_Name { get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public string Type_Dw { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 业务开通日期
        /// </summary>
        public DateTime? Register_Date { get; set; }
        /// <summary>
        /// 当前缴费状态
        /// </summary>
        public string State { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 当前欠费金额
        /// </summary>
        public decimal? Arrear_Money { get; set; }
        /// <summary>
        /// 当前欠费月数
        /// </summary>
        public string Arrear_Months { get; set; }
        /// <summary>
        /// 最近24个月缴费状态
        /// </summary>
        public string Status24 { get; set; }
        /// <summary>
        /// 记帐年月
        /// </summary>
        public string Get_Time { get; set; }


    }
}