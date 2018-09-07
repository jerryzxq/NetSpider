using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExtTrade.ModelLayer
{
    public  partial class BaseEntity
    {
        [Ignore]
        public decimal ID{ get; set; }
        /// <summary>
        /// 信用报告主表ID
        /// </summary>
        public decimal Report_Id { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        [Ignore]
        public byte[] TIMESTAMP { get; set; }

        /// <summary>
        /// PCQS更新时间
        /// </summary>
        [Ignore]
        public DateTime Time_Stamp { get; set; }
       
    }
}
