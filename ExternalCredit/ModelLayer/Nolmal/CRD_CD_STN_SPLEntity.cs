using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_CD_STN_SPL")]
    [Schema("credit")]
    public partial class CRD_CD_STN_SPLEntity
    {
                [JsonConverter(typeof(DesJsonConverter))]
        [Ignore]
        /// <summary>
        /// 编号
        /// </summary>
        public int? Number { get; set; }
        [Ignore]
        public decimal Standardloan_Special_Id { get; set; }
        /// <summary>
        /// 信用报告主表ID
        /// </summary>
        public decimal Report_Id { get; set; }
        /// <summary>
        /// 准贷记卡信息段ID
        /// </summary>
        [Alias("Standardloancard_Id")]
        public decimal Card_Id { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string Type_Dw { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 发生日期
        /// </summary>
        public DateTime? Get_Time { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 变更月数
        /// </summary>
        public decimal? Changing_Months { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 发生金额
        /// </summary>
        public decimal? Changing_Amount { get; set; }
        /// <summary>
        /// 明细记录
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// PCQS更新时间
        /// </summary>
        [Ignore]
        public DateTime? Time_Stamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Ignore]
        public byte[] TIMESTAMP { get; set; }
    }
}