using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spring.Context;
using Spring.Context.Support;

namespace Vcredit.NetSpider.Service
{
    public static class NetSpiderFactoryManager
    {
        static IApplicationContext ctx = ContextRegistry.GetContext();
        public static ICRD_ACCOUNT GetCRDACCOUNTService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.CRD_ACCOUNT") as ICRD_ACCOUNT;
        }
        public static ICRD_CD_GUARANTEE GetCRDCDGUARANTEEService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.CRD_CD_GUARANTEE") as ICRD_CD_GUARANTEE;
        }
        public static ICRD_CD_LN GetCRDCDLNService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.CRD_CD_LN") as ICRD_CD_LN;
        }
        public static ICRD_CD_LND GetCRDCDLNDService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.CRD_CD_LND") as ICRD_CD_LND;
        }
        public static ICRD_CD_STNCARD GetCRDCDSTNCARDService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.CRD_CD_STNCARD") as ICRD_CD_STNCARD;
        }
        public static ICRD_HD_REPORT GetCRDHDREPORTService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.CRD_HD_REPORT") as ICRD_HD_REPORT;
        }
        public static ICRD_HD_REPORT_HTML GetCRDHDREPORTHTMLService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.CRD_HD_REPORT_HTML") as ICRD_HD_REPORT_HTML;
        }
        public static ICRD_IS_CREDITCUE GetCRDISCREDITCUEService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.CRD_IS_CREDITCUE") as ICRD_IS_CREDITCUE;
        }
        public static ICRD_QR_RECORDDTL GetCRDQRRECORDDTLService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.CRD_QR_RECORDDTL") as ICRD_QR_RECORDDTL;
        }

        public static ICRD_PI_FORCEEXCTN GetCRDPIFORCEEXCTNService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.CRD_PI_FORCEEXCTN") as ICRD_PI_FORCEEXCTN;
        }
        public static ICRD_PI_TAXARREAR GetCRDPITAXARREARService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.CRD_PI_TAXARREAR") as ICRD_PI_TAXARREAR;
        }

        public static ICRD_PI_TELPNT GetCRDPITELPNTService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.CRD_PI_TELPNT") as ICRD_PI_TELPNT;
        }

        public static ICRD_PI_CIVILJDGM GetCRDPICIVILJDGMService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.CRD_PI_CIVILJDGM") as ICRD_PI_CIVILJDGM;
        }

        public static ICRD_CD_ASRREPAY GetCRDCDASRREPAYService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.CRD_CD_ASRREPAY") as ICRD_CD_ASRREPAY;
        }

        public static IProvidentFund GetProvidentFundService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.ProvidentFund") as IProvidentFund;
        }
        public static IProvidentFundDetail GetProvidentFundDetailService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.ProvidentFundDetail") as IProvidentFundDetail;
        }
        //====================20160317 周正恺=======================
        public static IProvidentFundReserve GetProvidentFundReserveService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.ProvidentFundReserve") as IProvidentFundReserve;
        }
        public static IProvidentFundReserveDetail GetProvidentFundReserveDetailService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.ProvidentFundReserveDetail") as IProvidentFundReserveDetail;
        }
        public static IProvidentFundLoan GetProvidentFundLoanService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.ProvidentFundLoan") as IProvidentFundLoan;
        }
        public static IProvidentFundLoanDetail GetProvidentFundLoanDetailService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.ProvidentFundLoanDetail") as IProvidentFundLoanDetail;
        }
        //===========================================================
        public static ICRD_STAT_LN GetCRDSTATLNService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.CRD_STAT_LN") as ICRD_STAT_LN;
        }
        public static ICRD_STAT_LND GetCRDSTATLNDService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.CRD_STAT_LND") as ICRD_STAT_LND;
        }
        public static ICRD_STAT_QR GetCRDSTATQRService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.CRD_STAT_QR") as ICRD_STAT_QR;
        }
        public static ICRD_KbaQuestion GetCRDKbaQuestionService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.CRD_KbaQuestion") as ICRD_KbaQuestion;
        }
        public static ISocialSecurity GetSocialSecurityService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.SocialSecurity") as ISocialSecurity;
        }
        public static ISocialSecurityDetail GetSocialSecurityDetailService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.SocialSecurityDetail") as ISocialSecurityDetail;
        }
        public static IBasic GetBasicService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.Basic") as IBasic;
        }
        public static ICalls GetCallsService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.Calls") as ICalls;
        }
        public static INets GetNetsService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.Nets") as INets;
        }
        public static IOperationLog GetOperationLogService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.OperationLog") as IOperationLog;
        }
        public static ISmses GetSmsService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.Smses") as ISmses;
        }
        public static ISummary GetSummaryService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.Summary") as ISummary;
        }
        public static ITransactions GetTransactionsService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.Transactions") as ITransactions;
        }
        public static IChsi_Info GetChsi_InfoService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.Chsi_Info") as IChsi_Info;
        }
        //public static ISpd_Login GetSpd_LoginService()
        //{
        //    return ctx.GetObject("Vcredit.NetSpider.Service.Spd_Login") as ISpd_Login;
        //}
        public static ISpd_apply GetSpd_applyService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.Spd_apply") as ISpd_apply;
        }
        public static ISpd_applyform GetSpd_applyformService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.Spd_applyform") as ISpd_applyform;
        }
        public static IDangernumber GetDangernumberService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.Dangernumber") as IDangernumber;
        }
        public static IGrayNumber GetGrayNumberService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.GrayNumber") as IGrayNumber;
        }
        public static IMobile_Number GetMobile_NumberService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.Mobile_Number") as IMobile_Number;
        }

        public static IVariable_mobile_summary GetVariable_mobile_summaryService()
        {
            return ctx.GetObject("Vcredit.NetSpider.Service.Variable_mobile_summary") as IVariable_mobile_summary;
        }
    }
}