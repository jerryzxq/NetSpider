using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.Service
{
	#region CRD_HD_REPORT

	/// <summary>
	/// CRD_HD_REPORT object for NHibernate mapped table 'CRD_HD_REPORT'.
	/// </summary>
	public class CRD_HD_REPORTRes:BaseRes
	{
		 /// <summary>
		 /// 
		 /// </summary>
		public  string ReportSn{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
        public string QueryTime { get; set; }
		 /// <summary>
		 /// 
		 /// </summary>
        public string ReportCreateTime { get; set; }
		 /// <summary>
		 /// 
		 /// </summary>
		public  string Name{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public  string CertType{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public  string CertNo{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public  string QueryReason{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public  string QueryFormat{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public  string QueryOrg{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public  string UserCode{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public  string QueryResultCue{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public  string MaritalState{get; set;}

        public string Loginname { get; set; }
        public int? Score { get; set; }

        //public  CRD_IS_CREDITCUE CRD_IS_CREDITCUE { get; set; }

        //private List<CRD_CD_LND> _CRD_CD_LNDList = new List<CRD_CD_LND>();
        //public  List<CRD_CD_LND> CRD_CD_LNDList
        //{
        //    get { return _CRD_CD_LNDList; }
        //    set { this._CRD_CD_LNDList = value; }
        //}

        //private List<CRD_CD_GUARANTEE> _CRD_CD_GUARANTEEList = new List<CRD_CD_GUARANTEE>();
        //public  List<CRD_CD_GUARANTEE> CRD_CD_GUARANTEEList
        //{
        //    get { return _CRD_CD_GUARANTEEList; }
        //    set { this._CRD_CD_GUARANTEEList = value; }
        //}

        //private List<CRD_CD_LN> _CRD_CD_LNList = new List<CRD_CD_LN>();
        //public  List<CRD_CD_LN> CRD_CD_LNList
        //{
        //    get { return _CRD_CD_LNList; }
        //    set { this._CRD_CD_LNList = value; }
        //}

        //private List<CRD_CD_STNCARD> _CRD_CD_STNCARDList = new List<CRD_CD_STNCARD>();
        //public  List<CRD_CD_STNCARD> CRD_CD_STNCARDList
        //{
        //    get { return _CRD_CD_STNCARDList; }
        //    set { this._CRD_CD_STNCARDList = value; }
        //}

        //private List<CRD_QR_RECORDDTL> _CRD_QR_RECORDDTLList = new List<CRD_QR_RECORDDTL>();
        //public  List<CRD_QR_RECORDDTL> CRD_QR_RECORDDTLList
        //{
        //    get { return _CRD_QR_RECORDDTLList; }
        //    set { this._CRD_QR_RECORDDTLList = value; }
        //}

        public virtual CRD_STAT_LN CRD_STAT_LN { get; set; }

        public virtual CRD_STAT_LND CRD_STAT_LND { get; set; }

        public virtual CRD_STAT_QR CRD_STAT_QR { get; set; }
	}
	#endregion
}