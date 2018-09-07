using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    public class NewForeignContainer
    {

        private CRD_HD_REPORTEntity report = new CRD_HD_REPORTEntity() { SourceType = (byte)Vcredit.ExternalCredit.CommonLayer.SysEnums.SourceType.Trade };
        /// <summary>
        /// 征信报告主表
        /// </summary>
        public CRD_HD_REPORTEntity CRD_HD_REPORT
        {
            get { return report; }
            set { report = value; }
        }
        private CRD_PI_IDENTITYEntity identity;
        /// <summary>
        /// 身份信息
        /// </summary>
        public CRD_PI_IDENTITYEntity CRD_PI_IDENTITY
        {
            get { return identity; }
            set { identity = value; }
        }
        private List<CRD_PI_RESIDENCEEntity> residenceList;
        /// <summary>
        /// 居住信息
        /// </summary>
        public List<CRD_PI_RESIDENCEEntity> CRD_PI_RESIDENCE
        {
            get { return residenceList; }
            set { residenceList = value; }
        }
        private List<CRD_PI_PROFESSNLEntity> professionList ;
        /// <summary>
        /// 职业信息
        /// </summary>
        public List<CRD_PI_PROFESSNLEntity> CRD_PI_PROFESSNL
        {
            get { return professionList; }
            set { professionList = value; }
        }
        private List<CRD_IS_CREDITCUEEntity> cRD_IS_CREDITCUE;
        /// <summary>
        /// 信用提示
        /// </summary>
        public List<CRD_IS_CREDITCUEEntity> CRD_IS_CREDITCUE
        {
            get { return cRD_IS_CREDITCUE; }
            set { cRD_IS_CREDITCUE = value; }
        }

        private List<CRD_CD_OverDueBreakeEntity> cRD_CD_OverDueBreakeEntity;
        /// <summary>
        /// 逾期及违约信息概要
        /// </summary>
        public List<CRD_CD_OverDueBreakeEntity> CRD_CD_OverDueBreake
        {
            get { return cRD_CD_OverDueBreakeEntity; }
            set { cRD_CD_OverDueBreakeEntity = value; }
        }
     

        private List<CRD_IS_OVDSUMMARYEntity> cRD_IS_OVDSUMMARY;
        /// <summary>
        /// 逾期(透支)信息汇总
        /// </summary>
        public List<CRD_IS_OVDSUMMARYEntity> CRD_IS_OVDSUMMARY
        {
            get { return cRD_IS_OVDSUMMARY; }
            set { cRD_IS_OVDSUMMARY = value; }
        }


        private List<CRD_IS_SHAREDEBTEntity> cRD_IS_SHAREDEBT;
        /// <summary>
        /// 未结清贷款,贷记卡，准贷记卡信息汇总
        /// </summary>
        public List<CRD_IS_SHAREDEBTEntity> CRD_IS_SHAREDEBT
        {
            get { return cRD_IS_SHAREDEBT; }
            set { cRD_IS_SHAREDEBT = value; }
        }

        private CRD_CD_NoCancellSTNCARDEntity noCancellSTNCARD = new CRD_CD_NoCancellSTNCARDEntity();
       

        private List<CRD_CD_GUARANTEESummeryEntity> cRD_CD_GUARANTEESummery;
        /// <summary>
        /// 对外担保信息汇总
        /// </summary>
        public List<CRD_CD_GUARANTEESummeryEntity> CRD_CD_GUARANTEESummery
        {
            get { return cRD_CD_GUARANTEESummery; }
            set { cRD_CD_GUARANTEESummery = value; }
        }

        private List<CRD_CD_ASSETDPSTEntity> cRD_CD_ASSETDPST;
        /// <summary>
        /// 资产处置信息
        /// </summary>
        public List<CRD_CD_ASSETDPSTEntity> CRD_CD_ASSETDPST
        {
            get { return cRD_CD_ASSETDPST; }
            set { cRD_CD_ASSETDPST = value; }
        }


        private List<CRD_CD_ASRREPAYEntity> cRD_CD_ASRREPAY;
        /// <summary>
        /// 保证人代偿信息
        /// </summary>
        public List<CRD_CD_ASRREPAYEntity> CRD_CD_ASRREPAY
        {
            get { return cRD_CD_ASRREPAY; }
            set { cRD_CD_ASRREPAY = value; }
        }


        private List<CRD_CD_LNEntity> lnList ;
        /// <summary>
        /// 贷款信息
        /// </summary>
        public List<CRD_CD_LNEntity> CRD_CD_LN
        {
            get { return lnList; }
            set { lnList = value; }
        }
        private List<CRD_CD_LNDEntity> lndiLst ;
        /// <summary>
        /// 贷记卡信息
        /// </summary>
        public List<CRD_CD_LNDEntity> CRD_CD_LND
        {
            get { return lndiLst; }
            set { lndiLst = value; }
        }
        private List<CRD_CD_STNCARDEntity> stnCardList ;
        /// <summary>
        /// 准贷记卡信息
        /// </summary>
        public List<CRD_CD_STNCARDEntity> CRD_CD_STNCARD
        {
            get { return stnCardList; }
            set { stnCardList = value; }
        }
        private List<CRD_CD_GUARANTEEEntity> guaranteeList ;
        /// <summary>
        /// 对外担保信息
        /// </summary>
        public List<CRD_CD_GUARANTEEEntity> CRD_CD_GUARANTEE
        {
            get { return guaranteeList; }
            set { guaranteeList = value; }
        }

        private List<CRD_PI_TAXARREAREntity> cRD_PI_TAXARREAR;
        /// <summary>
        /// 欠税记录
        /// </summary>
        public List<CRD_PI_TAXARREAREntity> CRD_PI_TAXARREAR
        {
            get { return cRD_PI_TAXARREAR; }
            set { cRD_PI_TAXARREAR = value; }
        }

        private List<CRD_PI_CIVILJDGMEntity> cRD_PI_CIVILJDGM;
        /// <summary>
        /// 民事判决记录
        /// </summary>
        public List<CRD_PI_CIVILJDGMEntity> CRD_PI_CIVILJDGM
        {
            get { return cRD_PI_CIVILJDGM; }
            set { cRD_PI_CIVILJDGM = value; }
        }

        private List<CRD_PI_FORCEEXCTNEntity> cRD_PI_FORCEEXCTN;
        /// <summary>
        /// 强制执行记录
        /// </summary>
        public List<CRD_PI_FORCEEXCTNEntity> CRD_PI_FORCEEXCTN
        {
            get { return cRD_PI_FORCEEXCTN; }
            set { cRD_PI_FORCEEXCTN = value; }
        }



        private List<CRD_PI_ADMINPNSHMEntity> cRD_PI_ADMINPNSHM;
        /// <summary>
        /// 行政处罚记录
        /// </summary>
        public List<CRD_PI_ADMINPNSHMEntity> CRD_PI_ADMINPNSHM
        {
            get { return cRD_PI_ADMINPNSHM; }
            set { cRD_PI_ADMINPNSHM = value; }
        }

        private List<CRD_PI_ACCFUNDEntity> accfundList ;
        /// <summary>
        /// 住房公积金缴存记录
        /// </summary>
        public List<CRD_PI_ACCFUNDEntity> CRD_PI_ACCFUND
        {
            get { return accfundList; }
            set { accfundList = value; }
        }
        private List<CRD_PI_ENDINSDPTEntity> endInsdptList;
        /// <summary>
        /// 养老保险缴存记录
        /// </summary>
        public List<CRD_PI_ENDINSDPTEntity> CRD_PI_ENDINSDPT
        {
            get { return endInsdptList; }
            set { endInsdptList = value; }
        }
        private List<CRD_PI_ENDINSDLREntity> endinsdlrList;
        /// <summary>
        /// 养老保险发放记录
        /// </summary>
        public List<CRD_PI_ENDINSDLREntity> CRD_PI_ENDINSDLR
        {
            get { return endinsdlrList; }
            set { endinsdlrList = value; }
        }
        private List<CRD_PI_SALVATIONEntity> salvationList ;
        /// <summary>
        /// 低保救助记录
        /// </summary>
        public List<CRD_PI_SALVATIONEntity> CRD_PI_SALVATION
        {
            get { return salvationList; }
            set { salvationList = value; }
        }
        private List<CRD_QR_RECORDDTLINFOEntity> recorddtlinfoList;
        /// <summary>
        /// 信贷审批查询记录明细
        /// </summary>
        public List<CRD_QR_RECORDDTLINFOEntity> CRD_QR_RECORDDTLINFO
        {
            get { return recorddtlinfoList; }
            set { recorddtlinfoList = value; }
        }


        private List<CRD_PI_COMPETENCEEntity> cRD_PI_COMPETENCE;
        /// <summary>
        /// 执业资格记录
        /// </summary>
        public List<CRD_PI_COMPETENCEEntity> CRD_PI_COMPETENCE
        {
            get { return cRD_PI_COMPETENCE; }
            set { cRD_PI_COMPETENCE = value; }
        }

        private List<CRD_PI_ADMINAWARDEntity> cRD_PI_ADMINAWARD;
        /// <summary>
        /// 行政奖励记录
        /// </summary>
        public List<CRD_PI_ADMINAWARDEntity> CRD_PI_ADMINAWARD
        {
            get { return cRD_PI_ADMINAWARD; }
            set { cRD_PI_ADMINAWARD = value; }
        }


        private List<CRD_PI_VEHICLEEntity> cRD_PI_VEHICLE;
        /// <summary>
        /// 车辆交易和抵押记录
        /// </summary>
        public List<CRD_PI_VEHICLEEntity> CRD_PI_VEHICLE
        {
            get { return cRD_PI_VEHICLE; }
            set { cRD_PI_VEHICLE = value; }
        }


        private List<CRD_PI_TELPNTEntity> cRD_PI_TELPNT;
        /// <summary>
        /// 电信缴费记录
        /// </summary>
        public List<CRD_PI_TELPNTEntity> CRD_PI_TELPNT
        {
            get { return cRD_PI_TELPNT; }
            set { cRD_PI_TELPNT = value; }
        }


        private List<CRD_AN_ANCINFOEntity> cRD_AN_ANCINFO;
        /// <summary>
        /// 本人声明
        /// </summary>
        public List<CRD_AN_ANCINFOEntity> CRD_AN_ANCINFO
        {
            get { return cRD_AN_ANCINFO; }
            set { cRD_AN_ANCINFO = value; }
        }

        private List<CRD_AN_DSTINFOEntity> cRD_AN_DSTINFO;
        /// <summary>
        /// 异议标注
        /// </summary>
        public List<CRD_AN_DSTINFOEntity> CRD_AN_DSTINFO
        {
            get { return cRD_AN_DSTINFO; }
            set { cRD_AN_DSTINFO = value; }
        }

        private List<CRD_QR_REORDSMREntity> cRD_QR_REORDSMR;
        /// <summary>
        /// 查询记录汇总
        /// </summary>
        public List<CRD_QR_REORDSMREntity> CRD_QR_REORDSMR
        {
            get { return cRD_QR_REORDSMR; }
            set { cRD_QR_REORDSMR = value; }
        }

        private List<CRD_CD_LN_SPLEntity> cRD_CD_LN_SPL;
        /// <summary>
        /// 贷款特殊交易
        /// </summary>
        public List<CRD_CD_LN_SPLEntity> CRD_CD_LN_SPL
        {
            get { return cRD_CD_LN_SPL; }
            set { cRD_CD_LN_SPL = value; }
        }

        private List<CRD_CD_LN_OVDEntity> cRD_CD_LN_OVD;
        /// <summary>
        /// 贷款逾期/透支记录
        /// </summary>
        public List<CRD_CD_LN_OVDEntity> CRD_CD_LN_OVD
        {
            get { return cRD_CD_LN_OVD; }
            set { cRD_CD_LN_OVD = value; }
        }


        private List<CRD_CD_LND_SPLEntity> cRD_CD_LND_SPL;
        /// <summary>
        /// 贷记卡特殊交易
        /// </summary>
        public List<CRD_CD_LND_SPLEntity> CRD_CD_LND_SPL
        {
            get { return cRD_CD_LND_SPL; }
            set { cRD_CD_LND_SPL = value; }
        }
        private List<CRD_CD_LND_OVDEntity> cRD_CD_LND_OVD;
        /// <summary>
        /// 贷记卡逾期/透支记录
        /// </summary>
        public List<CRD_CD_LND_OVDEntity> CRD_CD_LND_OVD
        {
            get { return cRD_CD_LND_OVD; }
            set { cRD_CD_LND_OVD = value; }
        }

        private List<CRD_CD_STN_SPLEntity> cRD_CD_STN_SPL;
        /// <summary>
        /// 准贷记卡特殊交易
        /// </summary>
        public List<CRD_CD_STN_SPLEntity> CRD_CD_STN_SPL
        {
            get { return cRD_CD_STN_SPL; }
            set { cRD_CD_STN_SPL = value; }
        }

        private List<CRD_CD_STN_OVDEntity> cRD_CD_STN_OVD;
        /// <summary>
        /// 准贷记卡逾期/透支记录
        /// </summary>
        public List<CRD_CD_STN_OVDEntity> CRD_CD_STN_OVD
        {
            get { return cRD_CD_STN_OVD; }
            set { cRD_CD_STN_OVD = value; }
        }
    }
}
