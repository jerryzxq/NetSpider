using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.ExternalCredit.Dto.OrgCreditModel
{
    public class OrgCreditContainer
    {
        /// <summary>
        /// 主表
        /// </summary>
        public CRD_HD_REPORTDto CRD_HD_Report { get; set; }

        private CRD_PI_IdentityDto identity;
        /// <summary>
        /// 身份信息
        /// </summary>
        public CRD_PI_IdentityDto CRD_PI_IDENTITY

        {
            get { return identity; }
            set { identity = value; }
        }
     
        private List<CRD_PI_PROFESSNLDto> professionList ;
        /// <summary>
        /// 职业信息
        /// </summary>
        public List<CRD_PI_PROFESSNLDto> CRD_PI_PROFESSNL
        {
            get { return professionList; }
            set { professionList = value; }
        }
        private List<CRD_PI_RESIDENCEDto> rESIDENCEList;
        /// <summary>
        ///居住信息
        /// </summary>
        public List<CRD_PI_RESIDENCEDto> CRD_PI_RESIDENCE
        {
            get { return rESIDENCEList; }
            set { rESIDENCEList = value; }
        }
        private List<CRD_IS_CREDITCUEDto> cRD_IS_CREDITCUE;
        /// <summary>
        /// 信用提示
        /// </summary>
        public List<CRD_IS_CREDITCUEDto> CRD_IS_CREDITCUE
        {
            get { return cRD_IS_CREDITCUE; }
            set { cRD_IS_CREDITCUE = value; }
        }

        private List<CRD_CD_OverDueBreakeDto> cRD_CD_OverDueBreakeDto;
        /// <summary>
        /// 逾期及违约信息概要
        /// </summary>
        public List<CRD_CD_OverDueBreakeDto> CRD_CD_OverDueBreake
        {
            get { return cRD_CD_OverDueBreakeDto; }
            set { cRD_CD_OverDueBreakeDto = value; }
        }
     

        private List<CRD_IS_OVDSUMMARYDto> cRD_IS_OVDSUMMARY;
        /// <summary>
        /// 逾期(透支)信息汇总
        /// </summary>
        public List<CRD_IS_OVDSUMMARYDto> CRD_IS_OVDSUMMARY
        {
            get { return cRD_IS_OVDSUMMARY; }
            set { cRD_IS_OVDSUMMARY = value; }
        }


        private List<CRD_IS_SHAREDEBTDto> cRD_IS_SHAREDEBT;
        /// <summary>
        /// 未结清贷款,贷记卡，准贷记卡信息汇总
        /// </summary>
        public List<CRD_IS_SHAREDEBTDto> CRD_IS_SHAREDEBT
        {
            get { return cRD_IS_SHAREDEBT; }
            set { cRD_IS_SHAREDEBT = value; }
        }

      

        private List<CRD_CD_ASSETDPSTDto> cRD_CD_ASSETDPST;
        /// <summary>
        /// 资产处置信息
        /// </summary>
        public List<CRD_CD_ASSETDPSTDto> CRD_CD_ASSETDPST
        {
            get { return cRD_CD_ASSETDPST; }
            set { cRD_CD_ASSETDPST = value; }
        }


        private List<CRD_CD_ASRREPAYDto> cRD_CD_ASRREPAY;
        /// <summary>
        /// 保证人代偿信息
        /// </summary>
        public List<CRD_CD_ASRREPAYDto> CRD_CD_ASRREPAY
        {
            get { return cRD_CD_ASRREPAY; }
            set { cRD_CD_ASRREPAY = value; }
        }


        private List<CRD_CD_LNDto> lnList ;
        /// <summary>
        /// 贷款信息
        /// </summary>
        public List<CRD_CD_LNDto> CRD_CD_LN
        {
            get { return lnList; }
            set { lnList = value; }
        }
        private List<CRD_CD_LNDDto> lndiLst ;
        /// <summary>
        /// 贷记卡信息
        /// </summary>
        public List<CRD_CD_LNDDto> CRD_CD_LND
        {
            get { return lndiLst; }
            set { lndiLst = value; }
        }
        private List<CRD_CD_STNCARDDto> stnCardList ;
        /// <summary>
        /// 准贷记卡信息
        /// </summary>
        public List<CRD_CD_STNCARDDto> CRD_CD_STNCARD
        {
            get { return stnCardList; }
            set { stnCardList = value; }
        }
        private List<CRD_CD_GUARANTEEDto> guaranteeList ;
        /// <summary>
        /// 对外担保信息
        /// </summary>
        public List<CRD_CD_GUARANTEEDto> CRD_CD_GUARANTEE
        {
            get { return guaranteeList; }
            set { guaranteeList = value; }
        }



        private List<CRD_PI_ACCFUNDDto> accfundList ;
        /// <summary>
        /// 住房公积金缴存记录
        /// </summary>
        public List<CRD_PI_ACCFUNDDto> CRD_PI_ACCFUND
        {
            get { return accfundList; }
            set { accfundList = value; }
        }
        private List<CRD_PI_ENDINSDPTDto> endInsdptList;
        /// <summary>
        /// 养老保险缴存记录
        /// </summary>
        public List<CRD_PI_ENDINSDPTDto> CRD_PI_ENDINSDPT
        {
            get { return endInsdptList; }
            set { endInsdptList = value; }
        }
        private List<CRD_PI_ENDINSDLRDto> endinsdlrList;
        /// <summary>
        /// 养老保险发放记录
        /// </summary>
        public List<CRD_PI_ENDINSDLRDto> CRD_PI_ENDINSDLR
        {
            get { return endinsdlrList; }
            set { endinsdlrList = value; }
        }
  
        private List<CRD_QR_RECORDDTLINFODto> recorddtlinfoList;
        /// <summary>
        /// 信贷审批查询记录明细
        /// </summary>
        public List<CRD_QR_RECORDDTLINFODto> CRD_QR_RECORDDTLINFO
        {
            get { return recorddtlinfoList; }
            set { recorddtlinfoList = value; }
        }

        private List<CRD_QR_REORDSMRDto> cRD_QR_REORDSMR;
        /// <summary>
        /// 查询记录汇总
        /// </summary>
        public List<CRD_QR_REORDSMRDto> CRD_QR_REORDSMR
        {
            get { return cRD_QR_REORDSMR; }
            set { cRD_QR_REORDSMR = value; }
        }

        private List<CRD_CD_LN_SPLDto> cRD_CD_LN_SPL;
        /// <summary>
        /// 贷款特殊交易
        /// </summary>
        public List<CRD_CD_LN_SPLDto> CRD_CD_LN_SPL
        {
            get { return cRD_CD_LN_SPL; }
            set { cRD_CD_LN_SPL = value; }
        }

        private List<CRD_CD_LN_OVDDto> cRD_CD_LN_OVD;
        /// <summary>
        /// 贷款逾期/透支记录
        /// </summary>
        public List<CRD_CD_LN_OVDDto> CRD_CD_LN_OVD
        {
            get { return cRD_CD_LN_OVD; }
            set { cRD_CD_LN_OVD = value; }
        }


        private List<CRD_CD_LND_SPLDto> cRD_CD_LND_SPL;
        /// <summary>
        /// 贷记卡特殊交易
        /// </summary>
        public List<CRD_CD_LND_SPLDto> CRD_CD_LND_SPL
        {
            get { return cRD_CD_LND_SPL; }
            set { cRD_CD_LND_SPL = value; }
        }
        private List<CRD_CD_LND_OVDDto> cRD_CD_LND_OVD;
        /// <summary>
        /// 贷记卡逾期/透支记录
        /// </summary>
        public List<CRD_CD_LND_OVDDto> CRD_CD_LND_OVD
        {
            get { return cRD_CD_LND_OVD; }
            set { cRD_CD_LND_OVD = value; }
        }

        private List<CRD_CD_STN_SPLDto> cRD_CD_STN_SPL;
        /// <summary>
        /// 准贷记卡特殊交易
        /// </summary>
        public List<CRD_CD_STN_SPLDto> CRD_CD_STN_SPL
        {
            get { return cRD_CD_STN_SPL; }
            set { cRD_CD_STN_SPL = value; }
        }

        private List<CRD_CD_STN_OVDDto> cRD_CD_STN_OVD;
        /// <summary>
        /// 准贷记卡逾期/透支记录
        /// </summary>
        public List<CRD_CD_STN_OVDDto> CRD_CD_STN_OVD
        {
            get { return cRD_CD_STN_OVD; }
            set { cRD_CD_STN_OVD = value; }
        }
        public CRD_QR_RECORDDTLDto CRD_QR_RECORDDTL { get; set; }
        /// <summary>
        /// 担保信息汇总
        /// </summary>
        public CRD_IS_GRTSUMMARYDto CRD_IS_GRTSUMMARY { get; set; }

    }
}
