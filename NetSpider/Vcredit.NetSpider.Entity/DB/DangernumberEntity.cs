using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
    #region DangernumberEntity

    /// <summary>
    /// DangernumberEntity object for NHibernate mapped table 'dangernumber'.
    /// </summary>
    public class DangernumberEntity
    {
        public virtual int DangerNumId { get; set; }
        /// <summary>
        /// 公司
        /// </summary>
        public virtual string CompanyName { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public virtual string Phone { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public virtual string City { get; set; }

        /// <summary>
        /// 是否本公司
        /// </summary>
        public virtual Boolean IsOwn { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// IP
        /// </summary>
        public virtual string IPAddr { get; set; }

    }
    #endregion
}
