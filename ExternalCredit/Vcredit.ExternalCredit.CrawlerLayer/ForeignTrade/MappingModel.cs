using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer;

namespace Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade
{
    public class MappingModel
    {
        public static readonly Dictionary<string, PropertyInfo[]> proInfosDic = new Dictionary<string, PropertyInfo[]>()
        {
         
                { CommonData.CRD_PI_IDENTITY,typeof (CRD_PI_IDENTITYEntity).GetProperties()},
          
                { CommonData.CRD_PI_RESIDENCE,typeof(CRD_PI_RESIDENCEEntity).GetProperties()},
           
                { CommonData.CRD_PI_PROFESSNL,typeof(CRD_PI_PROFESSNLEntity).GetProperties()},
          
                { CommonData.CRD_IS_CREDITCUE,typeof(CRD_IS_CREDITCUEEntity).GetProperties()},
                    
                { CommonData.CRD_CD_OverDueBreake,typeof(CRD_CD_OverDueBreakeEntity).GetProperties()},
                
                { CommonData.CRD_IS_OVDSUMMARY, typeof(CRD_IS_OVDSUMMARYEntity).GetProperties()},
                 
                { CommonData.CRD_CD_OutstandeSummary, typeof(CRD_IS_SHAREDEBTEntity).GetProperties()},
                
                {  CommonData.CRD_CD_NoCancellLND, typeof(CRD_IS_SHAREDEBTEntity).GetProperties()},
                  
                { CommonData.CRD_CD_NoCancellSTNCARD, typeof(CRD_IS_SHAREDEBTEntity).GetProperties()},
                
                { CommonData.CRD_IS_GRTSUMMARY, typeof(CRD_IS_GRTSUMMARYEntity).GetProperties()},
                
                { CommonData.CRD_CD_ASSETDPST, typeof(CRD_CD_ASSETDPSTEntity).GetProperties()},
                
                { CommonData.CRD_CD_ASRREPAY, typeof( CRD_CD_ASRREPAYEntity).GetProperties()},
                 
                { CommonData.CRD_CD_LN,typeof(CRD_CD_LNEntity).GetProperties()},
                  
                { CommonData.CRD_CD_LND, typeof(CRD_CD_LNDEntity).GetProperties()},
         
                { CommonData.CRD_CD_STNCARD, typeof(CRD_CD_STNCARDEntity).GetProperties()},
               
                { CommonData.CRD_CD_GUARANTEE,typeof(CRD_CD_GUARANTEEEntity).GetProperties()},

                { CommonData.NewCRD_CD_GUARANTEE,typeof(CRD_CD_GUARANTEEEntity).GetProperties()},

                { CommonData.NewStnCRD_CD_GUARANTEE,typeof(CRD_CD_GUARANTEEEntity).GetProperties()},
                  
                {CommonData.CRD_PI_TAXARREAR,typeof(CRD_PI_TAXARREAREntity).GetProperties()},
                
                { CommonData.CRD_PI_CIVILJDGM,typeof(CRD_PI_CIVILJDGMEntity).GetProperties()},
                  
                { CommonData.CRD_PI_FORCEEXCTN, typeof(CRD_PI_FORCEEXCTNEntity).GetProperties()},
                 
                { CommonData.CRD_PI_ADMINPNSHM, typeof(CRD_PI_ADMINPNSHMEntity).GetProperties()},
                  
                { CommonData.CRD_PI_ACCFUND, typeof(CRD_PI_ACCFUNDEntity).GetProperties()},
                 
                { CommonData.CRD_PI_ENDINSDPT,typeof(CRD_PI_ENDINSDPTEntity).GetProperties()},
               
                { CommonData.CRD_PI_ENDINSDLR,typeof(CRD_PI_ENDINSDLREntity).GetProperties()},
                  
                { CommonData.CRD_PI_SALVATION, typeof(CRD_PI_SALVATIONEntity).GetProperties()},
              
                { CommonData.CRD_PI_COMPETENCE, typeof(CRD_PI_COMPETENCEEntity).GetProperties()},
                   
                { CommonData.CRD_PI_ADMINAWARD,  typeof(CRD_PI_ADMINAWARDEntity).GetProperties()},
                 
                { CommonData.CRD_PI_VEHICLE,   typeof(CRD_PI_VEHICLEEntity).GetProperties()},
                 
                {  CommonData.CRD_PI_TELPNT, typeof(CRD_PI_TELPNTEntity).GetProperties()},
                
                { CommonData.CRD_AN_ANCINFO, typeof(CRD_AN_ANCINFOEntity).GetProperties()},
                  
                { CommonData.CRD_AN_DSTINFO, typeof(CRD_AN_DSTINFOEntity).GetProperties()},
                 
                { CommonData.CRD_QR_RECORDDTLINFO, typeof(CRD_QR_RECORDDTLINFOEntity).GetProperties()},
              
                { CommonData.specialTradeStr,typeof(CRD_CD_LN_SPLEntity).GetProperties()},
                 
                {  CommonData.overDueStr,typeof(CRD_CD_LN_OVDEntity).GetProperties()},
                                    
                { CommonData.CRD_QR_REORDSMR,typeof(CRD_QR_REORDSMREntity).GetProperties()},

      
                  
        };

    }
}
