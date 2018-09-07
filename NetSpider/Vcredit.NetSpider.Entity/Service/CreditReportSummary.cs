using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.Service
{
    public class CreditReportSummary
    {
        /// <summary>
        /// 
        /// </summary>
        public string SummaryType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? CreditCardCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? HouseLoanCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? OtherLoanCount { get; set; }
    }
}