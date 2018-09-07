using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.DB
{
    /// <summary>
    /// GrayNumberEntity object for NHibernate mapped table 'Gray_Number'.
    /// </summary>
    public class GrayNumberEntity
    {
        public virtual int Gray_Number_ID { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public virtual string Phone_Number { get; set; }

        /// <summary>
        /// 系数
        /// </summary>
        public virtual string Coefficient { get; set; }
    }
}
