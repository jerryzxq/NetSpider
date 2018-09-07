using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
    #region ProvidentFundLoanDetailEntity
    /// <summary>
    /// ProvidentFundLoanDetailEntity object for NHibernate mapped table 'ProvidentFundLoanDetail'.
    /// </summary>
    public class ProvidentFundLoanDetailEntity
    {
        public virtual long Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual long? ProvidentFundId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual long? ProvidentFundLoanId { get; set; }
        /// <summary>
        /// 记账日期
        /// </summary>
        public virtual string Record_Date { get; set; }
        /// <summary>
        /// 扣款日期
        /// </summary>
        public virtual string Bill_Date { get; set; }
        /// <summary>
        /// 还款期数
        /// </summary>
        public virtual string Record_Period { get; set; }
        /// <summary>
        /// 还款年月
        /// </summary>
        public virtual string Record_Month { get; set; }
        /// <summary>
        /// 还款本金
        /// </summary>
        public virtual decimal? Principal { get; set; }
        /// <summary>
        /// 还款利息
        /// </summary>
        public virtual decimal? Interest { get; set; }
        /// <summary>
        /// 逾期本金
        /// </summary>
        public virtual decimal? Overdue_Principal { get; set; }
        /// <summary>
        /// 逾期利息
        /// </summary>
        public virtual decimal? Overdue_Interest { get; set; }
        /// <summary>
        /// 罚息
        /// </summary>
        public virtual decimal? Interest_Penalty { get; set; }
        /// <summary>
        /// 待还的罚息
        /// </summary>
        public virtual decimal? Interest_Penalty_ToPay { get; set; }
        /// <summary>
        /// 还款本息
        /// </summary>
        public virtual decimal? Base { get; set; }
        /// <summary>
        /// 贷款余额
        /// </summary>
        public virtual decimal? Balance { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public virtual string Description { get; set; }
    }
    #endregion
}
