using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.Framework.Server.Dto
{
    public class BillSummaryRes
    {
        /// <summary>
        /// 电商额度 
        /// </summary>
        public decimal E_commercelimit { get; set; }

        /// <summary>
        /// 电商额度是否当前逾期   
        /// </summary>
        public string E_commercelimit_Isoverdue { get; set; }

        /// <summary>
        /// 近3个月电商额度是否有使用记录 
        /// </summary>
        public string E_commercelimit_In3_use { get; set; }
    }
}
