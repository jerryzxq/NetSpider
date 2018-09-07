using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region CreditReportEntity

	/// <summary>
	/// CreditReportEntity object for NHibernate mapped table 'CreditReport'.
	/// </summary>
	public class CreditReportEntity
	{
        public virtual string ReportId { get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? ParentId{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual DateTime ? ReportTime{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual DateTime ? QueryTime{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Name{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Gender{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string CertType{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string CertNo{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string MaritalState{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual DateTime ? CreateTime{get; set;}

        private List<CreditReportSummaryEntity> _CreditReportSummaryList = new List<CreditReportSummaryEntity>();
        public virtual List<CreditReportSummaryEntity> CreditReportSummaryList
        {
            get { return _CreditReportSummaryList; }
            set { this._CreditReportSummaryList = value; }
        }
        private List<CreditCardEntity> _CreditCardList = new List<CreditCardEntity>();
        public virtual List<CreditCardEntity> CreditCardList
        {
            get { return _CreditCardList; }
            set { this._CreditCardList = value; }
        }
        private List<CreditReportQueryHisEntity> _CreditReportQueryHisList = new List<CreditReportQueryHisEntity>();
        public virtual List<CreditReportQueryHisEntity> CreditReportQueryHisList
        {
            get { return _CreditReportQueryHisList; }
            set { this._CreditReportQueryHisList = value; }
        }
        public virtual CreditReportHtmlEntity CreditReportHtml { get; set; }
	}
	#endregion
}