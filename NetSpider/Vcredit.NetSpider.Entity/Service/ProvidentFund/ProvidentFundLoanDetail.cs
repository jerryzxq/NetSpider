using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.Service
{
    public class ProvidentFundLoanDetail
    {
        /// <summary>
        /// 记账日期
        /// </summary>
        public string Record_Date { get; set; }
        /// <summary>
        /// 扣款日期
        /// </summary>
        public string Bill_Date { get; set; }
        /// <summary>
        /// 还款期数
        /// </summary>
        public string Record_Period { get; set; }
        /// <summary>
        /// 还款年月
        /// </summary>
        public string Record_Month { get; set; }
        /// <summary>
        /// 还款本金
        /// </summary>
        public decimal Principal { get; set; }
        /// <summary>
        /// 还款利息
        /// </summary>
        public decimal Interest { get; set; }
        /// <summary>
        /// 逾期本金
        /// </summary>
        public decimal Overdue_Principal { get; set; }
        /// <summary>
        /// 逾期利息
        /// </summary>
        public decimal Overdue_Interest { get; set; }
        /// <summary>
        /// 罚息
        /// </summary>
        public decimal Interest_Penalty { get; set; }
        /// <summary>
        /// 待还的罚息
        /// </summary>
        public decimal Interest_Penalty_ToPay { get; set; }
        /// <summary>
        /// 还款本息
        /// </summary>
        public decimal Base { get; set; }
        /// <summary>
        /// 贷款余额
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }
}