using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  Vcredit.NetSpider.Emall.Entity
{
    [Alias("JD_JDWhiteBar")]
    
    public class JDWhiteBarEntity : BaseEntity
    {
        /// <summary>
        /// 账号号
        /// </summary>
        public string AccountName { get; set; }
        /// <summary>
        /// 信用等级
        /// </summary>
        public string CreditLevel { get; set; }
        /// <summary>
        /// 是否开通自动还款
        /// </summary>
        public bool OpenAutomaticRepayment { get; set; }

        /// <summary>
        /// 授信总额度
        /// </summary>
        public decimal TotalCreditNum { get; set; }
        /// <summary>
        /// 当前可分期金额
        /// </summary>
        public decimal CurrentPayment { get; set; }
        /// <summary>
        /// 已用总额度
        /// </summary>
        public decimal HaveUsedNum { get; set; }
        /// <summary>
        /// 消费类型
        /// </summary>
        public string ConsumptionType { get; set; }
    }
}
