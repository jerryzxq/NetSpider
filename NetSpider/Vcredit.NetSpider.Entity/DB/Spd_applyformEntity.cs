using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
    #region Spd_applyformEntity

    /// <summary>
    /// Spd_applyformEntity object for NHibernate mapped table 'spd_applyform'.
    /// </summary>
    public class Spd_applyformEntity
    {
        public virtual int Id { get; set; }

        /// <summary>
        /// 申请单ID
        /// </summary>
        public virtual int? ApplyId { get; set; }
        /// <summary>
        /// 表单name
        /// </summary>
        public virtual string Form_name { get; set; }
        /// <summary>
        /// 表单name
        /// </summary>
        public virtual string Form_value { get; set; }
        private DateTime _CreateTime = DateTime.Now;
        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }
    }
    #endregion
}