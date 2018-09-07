using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace  Vcredit.ExtTrade.ModelLayer.Nolmal
{
    /// <summary>
    /// 晋城银行数据
    /// </summary>
    public class CreditContainer
    {
        private CRD_HD_REPORTEntity report = new CRD_HD_REPORTEntity();
        /// <summary>
        /// 征信报告主表
        /// </summary>
        public CRD_HD_REPORTEntity Report
        {
            get { return report; }
            set { report = value; }
        }
        private CRD_PI_IDENTITYEntity identity = new CRD_PI_IDENTITYEntity();
        /// <summary>
        /// 省份信息
        /// </summary>
        public CRD_PI_IDENTITYEntity Identity
        {
            get { return identity; }
            set { identity = value; }
        }
        private List<CRD_PI_RESIDENCEEntity> residenceList = new List<CRD_PI_RESIDENCEEntity>();
        /// <summary>
        /// 居住信息
        /// </summary>
        public List<CRD_PI_RESIDENCEEntity> ResidenceList
        {
            get { return residenceList; }
            set { residenceList = value; }
        }
        private List<CRD_PI_PROFESSNLEntity> professionList = new List<CRD_PI_PROFESSNLEntity>();
        /// <summary>
        /// 职业信息
        /// </summary>
        public List<CRD_PI_PROFESSNLEntity> ProfessionList
        {
            get { return professionList; }
            set { professionList = value; }
        }
        private CRD_CD_NoCancellLNDEntity noCancellLND = new CRD_CD_NoCancellLNDEntity();
        /// <summary>
        /// 未销户贷记卡信息汇总
        /// </summary>
        public CRD_CD_NoCancellLNDEntity NoCancellLND
        {
            get { return noCancellLND; }
            set { noCancellLND = value; }
        }
        private CRD_CD_NoCancellSTNCARDEntity noCancellSTNCARD = new CRD_CD_NoCancellSTNCARDEntity();
        /// <summary>
        /// 未销户准贷记卡信息汇总
        /// </summary>
        public CRD_CD_NoCancellSTNCARDEntity NoCancellSTNCARD
        {
            get { return noCancellSTNCARD; }
            set { noCancellSTNCARD = value; }
        }
        private CRD_CD_OutstandeSummaryEntity outStandeSummary = new CRD_CD_OutstandeSummaryEntity();
        /// <summary>
        /// 未结清贷款信息
        /// </summary>
        public CRD_CD_OutstandeSummaryEntity OutStandeSummary
        {
            get { return outStandeSummary; }
            set { outStandeSummary = value; }
        }
        private List<CRD_CD_LNEntity> lnList = new List<CRD_CD_LNEntity>();
        /// <summary>
        /// 贷款信息
        /// </summary>
        public List<CRD_CD_LNEntity> LnList
        {
            get { return lnList; }
            set { lnList = value; }
        }
        private List<CRD_CD_LNDEntity> lndiLst = new List<CRD_CD_LNDEntity>();
        /// <summary>
        /// 贷记卡信息
        /// </summary>
        public List<CRD_CD_LNDEntity> LndiLst
        {
            get { return lndiLst; }
            set { lndiLst = value; }
        }
        private List<CRD_CD_STNCARDEntity> stnCardList = new List<CRD_CD_STNCARDEntity>();
        /// <summary>
        /// 准贷记卡信息
        /// </summary>
        public List<CRD_CD_STNCARDEntity> StnCardList
        {
            get { return stnCardList; }
            set { stnCardList = value; }
        }
        private List<CRD_CD_GUARANTEEEntity> guaranteeList = new List<CRD_CD_GUARANTEEEntity>();
        /// <summary>
        /// 对外担保信息
        /// </summary>
        public List<CRD_CD_GUARANTEEEntity> GuaranteeList
        {
            get { return guaranteeList; }
            set { guaranteeList = value; }
        }
        private List<CRD_PI_ACCFUNDEntity> accfundList = new List<CRD_PI_ACCFUNDEntity>();
        /// <summary>
        /// 住房公积金缴存记录
        /// </summary>
        public List<CRD_PI_ACCFUNDEntity> AccfundList
        {
            get { return accfundList; }
            set { accfundList = value; }
        }
        private List<CRD_PI_ENDINSDPTEntity> endInsdptList = new List<CRD_PI_ENDINSDPTEntity>();
        /// <summary>
        /// 养老保险缴存记录
        /// </summary>
        public List<CRD_PI_ENDINSDPTEntity> EndInsdptList
        {
            get { return endInsdptList; }
            set { endInsdptList = value; }
        }
        private List<CRD_PI_ENDINSDLREntity> endinsdlrList = new List<CRD_PI_ENDINSDLREntity>();
        /// <summary>
        /// 养老保险发放记录
        /// </summary>
        public List<CRD_PI_ENDINSDLREntity> EndinsdlrList
        {
            get { return endinsdlrList; }
            set { endinsdlrList = value; }
        }
        private List<CRD_PI_SALVATIONEntity> salvationList = new List<CRD_PI_SALVATIONEntity>();
        /// <summary>
        /// 低保救助记录
        /// </summary>
        public List<CRD_PI_SALVATIONEntity> SalvationList
        {
            get { return salvationList; }
            set { salvationList = value; }
        }
        private List<CRD_QR_RECORDDTLINFOEntity> recorddtlinfoList = new List<CRD_QR_RECORDDTLINFOEntity>();
        /// <summary>
        /// 信贷审批查询记录明细
        /// </summary>
        public List<CRD_QR_RECORDDTLINFOEntity> RecorddtlinfoList
        {
            get { return recorddtlinfoList; }
            set { recorddtlinfoList = value; }
        }


    }
}
