using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_IS_CREDITCUE")]
    [Schema("credit")]
    public partial class CRD_IS_CREDITCUEEntity : BaseEntity
    {
        [JsonConverter(typeof(DesJsonConverter))]

        /// <summary>
        /// 住房贷款笔数
        /// </summary>
        public decimal? House_Loan_Count { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 其他贷款笔数
        /// </summary>
        public decimal? Other_Loan_Count { get; set; }

        /// <summary>
        /// 首笔贷款发放月份
        /// </summary>
        public string First_Loan_Open_Month { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 贷记卡账户数
        /// </summary>
        public decimal? Loancard_Count { get; set; }
        /// <summary>
        /// 首张贷记卡发卡月份
        /// </summary>
        public string First_Loancard_Open_Month { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 准贷记卡账户数
        /// </summary>
        public decimal? Standard_Loancard_Count { get; set; }
        /// <summary>
        /// 首张准贷记卡发卡月份
        /// </summary>
        public string First_Sl_Open_Month { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 本人声明数目
        /// </summary>
        public decimal? Announce_Count { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 异议标注数目
        /// </summary>
        public decimal? Dissent_Count { get; set; }
        /// <summary>
        /// 评分
        /// </summary>
        public string Score { get; set; }
        /// <summary>
        /// 评分月份
        /// </summary>
        public string Score_Month { get; set; }
        /// <summary>
        /// PCQS更新时间
        /// </summary>
        [Ignore]
        public DateTime? Time_Stamp { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]

        /// <summary>
        /// 
        /// </summary>
        public int? Bh { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreditcardNo { get; set; }

        /// <summary>
        /// 个人商用贷款笔数
        /// </summary>
        public decimal? PersonalLoancard_Count { get; set; }
    }
}