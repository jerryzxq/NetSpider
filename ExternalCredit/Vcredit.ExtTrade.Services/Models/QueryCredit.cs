using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExternalCredit.Dto.OrgCreditModel;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal;

namespace Vcredit.ExtTrade.Services.Models
{
    public class QueryCreditInfoReq
    {
        public decimal? ReportID { get; set; }
        public string ReportSn { get; set; }

        public string Cert_No { get; set; }

    }
    public class ContactInfo
    {
        public string Name{ get; set; }
        public string Home_Telephone_No { get; set; }
        public string Mobile { get; set; }
        public string Office_Telephone_No { get; set; }
        public List<string> AddressList { get; set; }

    }
    public class QueryCredit
    {
        BaseDao dao = new BaseDao();
        public List<string> GetCreditAddressList(decimal reportid)
        {
            List<string> list = new List<string>();
            var identity = dao.Select<CRD_PI_IDENTITYEntity>(x => x.Report_Id == reportid).FirstOrDefault();
            var residenceList = dao.Select<CRD_PI_RESIDENCEEntity>(x => x.Report_Id == reportid);
            if (identity != null)
            {
                list.Add(identity.Post_Address);
                list.Add(identity.Registered_Address);
            }
            if (residenceList.Count != 0)
            {
                list.AddRange(residenceList.Select(x => x.Address));
            }
            return list.Distinct().ToList();
        }
        public ContactInfo GetCreditAddressListByCert_No(string cert_no)
        {
            var entity= dao.Select<CRD_HD_REPORTEntity>(x => x.Cert_No == cert_no).OrderByDescending(x => x.Report_Id).FirstOrDefault();
            if (entity == null)
                return  null;
            else
            {
                ContactInfo contactInfo = new ContactInfo();
                contactInfo.Name = entity.Name;
                List<string> list = new List<string>();
                var identity = dao.Select<CRD_PI_IDENTITYEntity>(x => x.Report_Id == entity.Report_Id).FirstOrDefault();
                var residenceList = dao.Select<CRD_PI_RESIDENCEEntity>(x => x.Report_Id == entity.Report_Id);
                if (identity != null)
                {
                    list.Add(identity.Post_Address);
                    list.Add(identity.Registered_Address);
                    contactInfo.Home_Telephone_No = identity.Home_Telephone_No;
                    contactInfo.Office_Telephone_No = identity.Office_Telephone_No;
                    contactInfo.Mobile= identity.Mobile;

                }
                if (residenceList.Count != 0)
                {
                    list.AddRange(residenceList.Select(x => x.Address));
                }
                contactInfo.AddressList = list;
                return contactInfo;
            }
          
        }

        public long  QueryForceExctnNum(string cert_no, decimal? reportid)
        {
            if(reportid ==null||reportid ==0)
            {
                 var entity = dao.Select<CRD_HD_REPORTEntity>(x => x.Cert_No ==cert_no).OrderByDescending(x=>x.Report_Id).FirstOrDefault();
                 if (entity != null)
                 {
                     reportid = entity.Report_Id;
                 }
                 else //找不到相应的信息
                     return -1;
            }
            return  dao.Count<CRD_PI_FORCEEXCTNEntity>(x=>x.Report_Id ==reportid);
        }
        public string GetCreditInfo(string report_sn)
        {

            var report = dao.Select<CRD_HD_REPORTEntity>(x => x.Report_Sn == report_sn).FirstOrDefault();
            if (report == null)
                return null;
            return QueryReport(report.Report_Id, report);
        }
        public  string GetCreditInfo(decimal reportid)
        {

           var report = dao.Select<CRD_HD_REPORTEntity>(x => x.Report_Id == reportid).FirstOrDefault();
           if (report == null)
               return null;
           return QueryReport(reportid, report);

        }

        private string QueryReport(decimal reportid, CRD_HD_REPORTEntity report)
        {
            var orgCon = new OrgCreditContainer
            {
                CRD_HD_Report = MapperHelper.Map<CRD_HD_REPORTEntity, CRD_HD_REPORTDto>(report),
                CRD_CD_ASRREPAY = MapperHelper.Map<List<CRD_CD_ASRREPAYEntity>, List<CRD_CD_ASRREPAYDto>>(dao.Select<CRD_CD_ASRREPAYEntity>(x => x.Report_Id == reportid)),
                CRD_CD_ASSETDPST = MapperHelper.Map<List<CRD_CD_ASSETDPSTEntity>, List<CRD_CD_ASSETDPSTDto>>(dao.Select<CRD_CD_ASSETDPSTEntity>(x => x.Report_Id == reportid)),
                CRD_CD_GUARANTEE = MapperHelper.Map<List<CRD_CD_GUARANTEEEntity>, List<CRD_CD_GUARANTEEDto>>(dao.Select<CRD_CD_GUARANTEEEntity>(x => x.Report_Id == reportid)),
                CRD_CD_LN = MapperHelper.Map<List<CRD_CD_LNEntity>, List<CRD_CD_LNDto>>(dao.Select<CRD_CD_LNEntity>(x => x.Report_Id == reportid)),
                CRD_CD_LN_OVD = MapperHelper.Map<List<CRD_CD_LN_OVDEntity>, List<CRD_CD_LN_OVDDto>>(dao.Select<CRD_CD_LN_OVDEntity>(x => x.Report_Id == reportid)),
                CRD_CD_LN_SPL = MapperHelper.Map<List<CRD_CD_LN_SPLEntity>, List<CRD_CD_LN_SPLDto>>(dao.Select<CRD_CD_LN_SPLEntity>(x => x.Report_Id == reportid)),
                CRD_CD_LND = MapperHelper.Map<List<CRD_CD_LNDEntity>, List<CRD_CD_LNDDto>>(dao.Select<CRD_CD_LNDEntity>(x => x.Report_Id == reportid)),
                CRD_CD_LND_OVD = MapperHelper.Map<List<CRD_CD_LND_OVDEntity>, List<CRD_CD_LND_OVDDto>>(dao.Select<CRD_CD_LND_OVDEntity>(x => x.Report_Id == reportid)),
                CRD_CD_LND_SPL = MapperHelper.Map<List<CRD_CD_LND_SPLEntity>, List<CRD_CD_LND_SPLDto>>(dao.Select<CRD_CD_LND_SPLEntity>(x => x.Report_Id == reportid)),
                CRD_CD_OverDueBreake = MapperHelper.Map<List<CRD_CD_OverDueBreakeEntity>, List<CRD_CD_OverDueBreakeDto>>(dao.Select<CRD_CD_OverDueBreakeEntity>(x => x.Report_Id == reportid)),
                CRD_CD_STN_OVD = MapperHelper.Map<List<CRD_CD_STN_OVDEntity>, List<CRD_CD_STN_OVDDto>>(dao.Select<CRD_CD_STN_OVDEntity>(x => x.Report_Id == reportid)),
                CRD_CD_STN_SPL = MapperHelper.Map<List<CRD_CD_STN_SPLEntity>, List<CRD_CD_STN_SPLDto>>(dao.Select<CRD_CD_STN_SPLEntity>(x => x.Report_Id == reportid)),
                CRD_CD_STNCARD = MapperHelper.Map<List<CRD_CD_STNCARDEntity>, List<CRD_CD_STNCARDDto>>(dao.Select<CRD_CD_STNCARDEntity>(x => x.Report_Id == reportid)),
                CRD_IS_CREDITCUE = MapperHelper.Map<List<CRD_IS_CREDITCUEEntity>, List<CRD_IS_CREDITCUEDto>>(dao.Select<CRD_IS_CREDITCUEEntity>(x => x.Report_Id == reportid)),
                CRD_IS_OVDSUMMARY = MapperHelper.Map<List<CRD_IS_OVDSUMMARYEntity>, List<CRD_IS_OVDSUMMARYDto>>(dao.Select<CRD_IS_OVDSUMMARYEntity>(x => x.Report_Id == reportid)),
                CRD_IS_SHAREDEBT = MapperHelper.Map<List<CRD_IS_SHAREDEBTEntity>, List<CRD_IS_SHAREDEBTDto>>(dao.Select<CRD_IS_SHAREDEBTEntity>(x => x.Report_Id == reportid)),
                CRD_PI_ACCFUND = MapperHelper.Map<List<CRD_PI_ACCFUNDEntity>, List<CRD_PI_ACCFUNDDto>>(dao.Select<CRD_PI_ACCFUNDEntity>(x => x.Report_Id == reportid)),
                CRD_PI_ENDINSDLR = MapperHelper.Map<List<CRD_PI_ENDINSDLREntity>, List<CRD_PI_ENDINSDLRDto>>(dao.Select<CRD_PI_ENDINSDLREntity>(x => x.Report_Id == reportid)),
                CRD_PI_ENDINSDPT = MapperHelper.Map<List<CRD_PI_ENDINSDPTEntity>, List<CRD_PI_ENDINSDPTDto>>(dao.Select<CRD_PI_ENDINSDPTEntity>(x => x.Report_Id == reportid)),
                CRD_PI_IDENTITY = MapperHelper.Map<CRD_PI_IDENTITYEntity, CRD_PI_IdentityDto>(dao.Select<CRD_PI_IDENTITYEntity>(x => x.Report_Id == reportid).FirstOrDefault()),
                CRD_PI_PROFESSNL = MapperHelper.Map<List<CRD_PI_PROFESSNLEntity>, List<CRD_PI_PROFESSNLDto>>(dao.Select<CRD_PI_PROFESSNLEntity>(x => x.Report_Id == reportid)),
                CRD_QR_RECORDDTLINFO = MapperHelper.Map<List<CRD_QR_RECORDDTLINFOEntity>, List<CRD_QR_RECORDDTLINFODto>>(dao.Select<CRD_QR_RECORDDTLINFOEntity>(x => x.Report_Id == reportid)),
                CRD_QR_REORDSMR = MapperHelper.Map<List<CRD_QR_REORDSMREntity>, List<CRD_QR_REORDSMRDto>>(dao.Select<CRD_QR_REORDSMREntity>(x => x.Report_Id == reportid)),
                CRD_QR_RECORDDTL = MapperHelper.Map<CRD_QR_RECORDDTLEntity, CRD_QR_RECORDDTLDto>(dao.Select<CRD_QR_RECORDDTLEntity>(x => x.Report_Id == reportid).FirstOrDefault()),
                CRD_PI_RESIDENCE = MapperHelper.Map<List<CRD_PI_RESIDENCEEntity>, List<CRD_PI_RESIDENCEDto>>(dao.Select<CRD_PI_RESIDENCEEntity>(x => x.Report_Id == reportid))
            };
            var grtSum = dao.Select<CRD_CD_GUARANTEESummeryEntity>(x => x.Report_Id == reportid).FirstOrDefault();
            if(grtSum!=null)
            {
                orgCon.CRD_IS_GRTSUMMARY = new CRD_IS_GRTSUMMARYDto 
                { Guarantee_Amount = grtSum.GuaranteeMoney, Guarantee_Count = grtSum.GuaranteeNum, Guarantee_Balance = grtSum.PrincipalBalance, Report_Id = grtSum.Report_Id };
            }
            return JsonConvert.SerializeObject(orgCon, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss", DateTimeStyles = DateTimeStyles.AllowInnerWhite });
        }
    }
}