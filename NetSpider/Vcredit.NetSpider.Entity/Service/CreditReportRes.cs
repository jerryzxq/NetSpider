using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.Service
{
    public class CreditReportRes : BaseRes
    {
        public string ReportId { get; set; }

        /// <summary>
        /// 
        /// </summary>		 
        public DateTime? ReportTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? QueryTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CertType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CertNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MaritalState { get; set; }
        private List<CreditReportSummary> _CreditReportSummaryList = new List<CreditReportSummary>();
        public List<CreditReportSummary> CreditReportSummaryList 
        {
            get { return _CreditReportSummaryList; }
            set { this._CreditReportSummaryList = value; }
        }
        private List<CreditCard> _CreditCardList = new List<CreditCard>();
        public List<CreditCard> CreditCardList
        {
            get { return _CreditCardList; }
            set { this._CreditCardList = value; }
        }
        private List<CreditReportQueryHis> _CreditReportQueryHisList = new List<CreditReportQueryHis>();
        public List<CreditReportQueryHis> CreditReportQueryHisList
        {
            get { return _CreditReportQueryHisList; }
            set { this._CreditReportQueryHisList = value; }
        }
    }
}