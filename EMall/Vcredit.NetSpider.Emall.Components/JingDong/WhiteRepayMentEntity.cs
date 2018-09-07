using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  Vcredit.NetSpider.Emall.Entity
{
    [Alias("JD_WhiteRepayMent")]
    
    /// <summary>
    /// 已还款
    /// </summary>
    public class WhiteRepayMentEntity:BaseEntity
    {
        /// <summary>
        /// 还款单号
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 分期服务费
        /// </summary>
        public decimal StageServiceFee { get; set; }
        /// <summary>
        /// 违约金
        /// </summary>      
        public decimal PayDamageAmount { get; set; }
        /// <summary>
        /// 还款金额
        /// </summary>
        public decimal RepayAmount { get; set; }
        /// <summary>
        /// 还款时间
        /// </summary>
        public DateTime? RepayTime { get; set; }
        /// <summary>
        /// 还款本金
        /// </summary>
        public decimal  RepaymentOfPrincipal { get; set; }
        /// <summary>
        /// 还款状态
        /// </summary>
        public string  RepayStatus{ get; set; }

        /// <summary>
        /// 支付方式
        /// </summary>
        public string PaymentMethod{ get; set; }
    }
}
