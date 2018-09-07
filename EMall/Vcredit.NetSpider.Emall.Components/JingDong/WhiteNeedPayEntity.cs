using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  Vcredit.NetSpider.Emall.Entity
{
    [Alias("JD_WhiteNeedPay")]
    
    /// <summary>
    /// 白条待还款
    /// </summary>
    public class WhiteNeedPayEntity:BaseEntity
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
        /// 应还明细
        /// </summary>
        public string ShouldPayDetail { get; set; }
        /// <summary>
        /// 应还款日
        /// </summary>
        public DateTime? RepaymentTime { get; set; }

        /// <summary>
        /// 本期应还
        /// </summary>
        public decimal? CurrentNeedPay { get; set; }

    }
}
