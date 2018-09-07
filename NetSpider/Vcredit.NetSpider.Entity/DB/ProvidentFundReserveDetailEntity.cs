using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
    #region ProvidentFundReserveDetailEntity
    /// <summary>
    /// ProvidentFundReserveDetailEntity object for NHibernate mapped table 'ProvidentFundReserveDetail'.
    /// </summary>
    public class ProvidentFundReserveDetailEntity
    {
        public virtual long Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual long? ProvidentFundId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual long? ProvidentFundReserveId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string PaymentType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string PaymentFlag { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual DateTime? PayTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string ProvidentFundTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual decimal? ProvidentFundBase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual decimal? PersonalPayAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual decimal? CompanyPayAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string CompanyName { get; set; }
        public virtual string Description { get; set; }
    }
    #endregion
}
