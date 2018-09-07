using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.Service
{
    public class CreditReportQueryHis
    {
        /// <summary>
        /// 
        /// </summary>
        public int? SerialId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? QueryTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string QueryOperator { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string QueryReason { get; set; }
    }
}