using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  Vcredit.NetSpider.Emall.Entity
{
    [Alias("JD_WhiteRefunded")]
    
    /// <summary>
    /// 已退款
    /// </summary>
    public class WhiteRefundedEntity:BaseEntity
    {
        public string OrderNo { get; set; }
        /// <summary>
        /// 期数
        /// </summary>
        public string NumberOfPeriods { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>      
        public string CommodityName { get; set; }
        /// <summary>
        /// 消费金额
        /// </summary>
        public decimal ConsumptionAmount { get; set; }
        /// <summary>
        /// 消费时间
        /// </summary>
        public DateTime? ConsumingTime { get; set; }
        /// <summary>
        /// 已退金额
        /// </summary>
        public decimal? RefundedAmount { get; set; }
        /// <summary>
        /// 退款时间
        /// </summary>
        public DateTime? RefundedTime { get; set; }
    }
}
