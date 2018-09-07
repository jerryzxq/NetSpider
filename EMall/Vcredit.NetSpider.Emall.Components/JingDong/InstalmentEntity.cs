using Newtonsoft.Json;
using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.NetSpider.Emall.Entity 
{
   [Alias("jd_instalment")]
    public   class InstalmentEntity:BaseEntity
    {
        /// <summary>
        /// 期数
        /// </summary>
        public int? PeriodNum { get; set; }
        /// <summary>
        /// 应还款日期
        /// </summary>
        public DateTime? PayDate { get; set; }
        /// <summary>
        /// 本期应还
        /// </summary>
        public decimal? CurrentPayAmount { get; set; }
        /// <summary>
        /// 还款状态
        /// </summary>
        public string  PayState { get; set; }
        /// <summary>
        /// 本金
        /// </summary>
        public decimal? CapitalAmount { get; set; }
        /// <summary>
        /// 分期服务费
        /// </summary>
        public decimal? ServiceFee { get; set; }
        /// <summary>
        /// 违约金
        /// </summary>
        public decimal? DamageFee { get; set; }
        /// <summary>
        /// 已还款金额
        /// </summary>
        public decimal? HavePayAmount { get; set; }
        /// <summary>
        /// 是否当前还款期数
        /// </summary>
        public bool?  IsCurrentPeriod { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public string  OrderNo { get; set; }
       /// <summary>
       /// 退款金额
       /// </summary>
        public decimal? RefundedAmount{ get; set; } 

    }
}
