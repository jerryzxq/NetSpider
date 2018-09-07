using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
    #region CRD_HD_REPORTEntity

    /// <summary>
    /// CRD_HD_REPORTEntity object for NHibernate mapped table 'CRD_HD_REPORT'.
    /// </summary>
    public class CRD_HD_REPORTEntity
    {
        public virtual int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual DateTime? RecordDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual int? AcountId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string ReportSn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual DateTime? QueryTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual DateTime? ReportCreateTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string CertType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string CertNo { get; set; }
        ///// <summary>
        ///// 
        ///// </summary>
        //public virtual string QueryReason { get; set; }
        ///// <summary>
        ///// 
        ///// </summary>
        //public virtual string QueryFormat { get; set; }
        ///// <summary>
        ///// 
        ///// </summary>
        //public virtual string QueryOrg { get; set; }
        ///// <summary>
        ///// 
        ///// </summary>
        //public virtual string UserCode { get; set; }
        ///// <summary>
        ///// 
        ///// </summary>
        //public virtual string QueryResultCue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// 
        public virtual string MaritalState { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual int Score { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Loginname { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Password { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //public virtual int Bid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string BusType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string BusIdentityCard { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string BusId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsCreditBlank { get; set; }

        private DateTime _CreateTime = DateTime.Now;
        public virtual DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }
        public virtual CRD_HD_REPORT_HTMLEntity CRD_HD_REPORT_HTML { get; set; }
        public virtual CRD_IS_CREDITCUEEntity CRD_IS_CREDITCUE { get; set; }

        private List<CRD_CD_LNDEntity> _CRD_CD_LNDList = new List<CRD_CD_LNDEntity>();
        public virtual List<CRD_CD_LNDEntity> CRD_CD_LNDList
        {
            get { return _CRD_CD_LNDList; }
            set { this._CRD_CD_LNDList = value; }
        }

        private List<CRD_CD_GUARANTEEEntity> _CRD_CD_GUARANTEEList = new List<CRD_CD_GUARANTEEEntity>();
        public virtual List<CRD_CD_GUARANTEEEntity> CRD_CD_GUARANTEEList
        {
            get { return _CRD_CD_GUARANTEEList; }
            set { this._CRD_CD_GUARANTEEList = value; }
        }

        private List<CRD_CD_LNEntity> _CRD_CD_LNList = new List<CRD_CD_LNEntity>();
        public virtual List<CRD_CD_LNEntity> CRD_CD_LNList
        {
            get { return _CRD_CD_LNList; }
            set { this._CRD_CD_LNList = value; }
        }

        private List<CRD_CD_STNCARDEntity> _CRD_CD_STNCARDList = new List<CRD_CD_STNCARDEntity>();
        public virtual List<CRD_CD_STNCARDEntity> CRD_CD_STNCARDList
        {
            get { return _CRD_CD_STNCARDList; }
            set { this._CRD_CD_STNCARDList = value; }
        }

        private List<CRD_QR_RECORDDTLEntity> _CRD_QR_RECORDDTLList = new List<CRD_QR_RECORDDTLEntity>();
        public virtual List<CRD_QR_RECORDDTLEntity> CRD_QR_RECORDDTLList
        {
            get { return _CRD_QR_RECORDDTLList; }
            set { this._CRD_QR_RECORDDTLList = value; }
        }

        public virtual CRD_STAT_LNEntity CRD_STAT_LN { get; set; }

        public virtual CRD_STAT_LNDEntity CRD_STAT_LND { get; set; }

        public virtual CRD_STAT_QREntity CRD_STAT_QR { get; set; }

        private List<CRD_PI_FORCEEXCTNEntity> _CRD_PI_FORCEEXCTNELList = new List<CRD_PI_FORCEEXCTNEntity>();
        public virtual List<CRD_PI_FORCEEXCTNEntity> CRD_PI_FORCEEXCTNEList
        {
            get { return _CRD_PI_FORCEEXCTNELList; }
            set { this._CRD_PI_FORCEEXCTNELList = value; }
        }

        private List<CRD_CD_ASRREPAYEntity> _CRD_CD_ASRREPAYList = new List<CRD_CD_ASRREPAYEntity>();
        public virtual List<CRD_CD_ASRREPAYEntity> CRD_CD_ASRREPAYList
        {
            get { return _CRD_CD_ASRREPAYList; }
            set { this._CRD_CD_ASRREPAYList = value; }
        }
        private List<CRD_PI_TAXARREAREntity> _CRD_PI_TAXARREARList = new List<CRD_PI_TAXARREAREntity>();
        public virtual List<CRD_PI_TAXARREAREntity> CRD_PI_TAXARREARList
        {
            get { return _CRD_PI_TAXARREARList; }
            set { this._CRD_PI_TAXARREARList = value; }
        }
        private List<CRD_PI_TELPNTEntity> _CRD_PI_TELPNTEntity = new List<CRD_PI_TELPNTEntity>();
        public virtual List<CRD_PI_TELPNTEntity> CRD_PI_TELPNTEntityList
        {

            get { return _CRD_PI_TELPNTEntity; }
            set { this._CRD_PI_TELPNTEntity = value; }
        }
        private List<CRD_PI_CIVILJDGMEntity> _CRD_PI_CIVILJDGMEntity = new List<CRD_PI_CIVILJDGMEntity>();
        public virtual List<CRD_PI_CIVILJDGMEntity> CRD_PI_CIVILJDGMEntityList
        {

            get { return _CRD_PI_CIVILJDGMEntity; }
            set { this._CRD_PI_CIVILJDGMEntity = value; }
        }
        private List<CRD_PI_ADMINPNSHMEntity> _CRD_PI_ADMINPNSHMEntity = new List<CRD_PI_ADMINPNSHMEntity>();
        public virtual List<CRD_PI_ADMINPNSHMEntity> CRD_PI_ADMINPNSHMEntityList
        {

            get { return _CRD_PI_ADMINPNSHMEntity; }
            set { this._CRD_PI_ADMINPNSHMEntity = value; }
        }
        private PublicInformationSummary _PubInfoSummary = new PublicInformationSummary();
        public virtual PublicInformationSummary PubInfoSummary
        {
            get { return _PubInfoSummary; }
            set { this._PubInfoSummary = value; }
        }
    }

    /// <summary>
    /// 公共信息统计
    /// </summary>
    public class PublicInformationSummary
    {
        /// <summary>
        /// 行政处罚记录段
        /// </summary>
        public int AdminpnshmCount { get; set; }
        /// <summary>
        /// 民事判决记录段
        /// </summary>
        public int CiviljdgmCount { get; set; }

        /// <summary>
        /// 强制执行记录段
        /// </summary>
        public int ForceexctnCount { get; set; }

        /// <summary>
        /// 欠税记录段
        /// </summary>
        public int TaxarrearCount { get; set; }
        /// <summary>
        /// 电信缴费记录段
        /// </summary>
        public int TelpntCount { get; set; }

    }
    #endregion
}