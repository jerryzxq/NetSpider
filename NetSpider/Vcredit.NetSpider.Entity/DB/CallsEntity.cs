using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
    #region CallsEntity

    /// <summary>
    /// CallsEntity object for NHibernate mapped table 'calls'.
    /// </summary>
    public class CallsEntity
    {
        public virtual long Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual int? Oper_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Update_time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Cell_phone { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Other_cell_phone { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Place { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Start_time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Use_time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Call_type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Init_type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual double? Subtotal { get; set; }
        public virtual OperationLogEntity OperationLog { get; set; }
    }
    #endregion
}