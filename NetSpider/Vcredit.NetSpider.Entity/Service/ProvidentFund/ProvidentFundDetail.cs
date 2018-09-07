using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.Service
{
    public class ProvidentFundDetail
    {
        /// <summary>
        /// 缴费类型
        /// </summary>
        public string PaymentType { get; set; }
        /// <summary>
        /// 缴费标志
        /// </summary>
        public string PaymentFlag { get; set; }
        /// <summary>
        /// 缴费年月
        /// </summary>
        public DateTime? PayTime { get; set; }
        /// <summary>
        /// 应属年月
        /// </summary>
        public string ProvidentFundTime { get; set; }
        /// <summary>
        /// 缴费基数
        /// </summary>
        public decimal ProvidentFundBase { get; set; }
        /// <summary>
        /// 个人缴费
        /// </summary>
        public decimal PersonalPayAmount { get; set; }
        /// <summary>
        /// 单位缴费
        /// </summary>
        public decimal CompanyPayAmount { get; set; }
        /// <summary>
        /// 单位名称
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }
}