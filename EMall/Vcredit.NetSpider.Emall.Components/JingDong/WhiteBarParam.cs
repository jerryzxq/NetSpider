using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.NetSpider.Emall.Entity.JingDong
{
    public class WhiteBarParam
    {
        /// <summary>
        /// 是否有逾期
        /// </summary>
        public bool IsOverDue   { get; set; }
        /// <summary>
        /// 3个月内是否有使用记录
        /// </summary>
        public bool HaveRecordWithin3Months { get; set; }
    }
}
