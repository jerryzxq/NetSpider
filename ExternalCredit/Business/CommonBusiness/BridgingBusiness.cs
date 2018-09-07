using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.OrmLite;
using Vcredit.ExtTrade.ModelLayer;
using Vcredit.ExtTrade.CommonLayer;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Converters;
using System.IO;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExtTrade.ModelLayer.Common;

namespace Vcredit.ExtTrade.BusinessLayer.CommonBusiness
{
    /// <summary>
    /// 跨表业务处理
    /// </summary>
    public  class BridgingBusiness
    {
        BaseDao dao = new BaseDao();
        OperatorLog opeLog = new OperatorLog(FileType.ExcelFile);
        CRD_CD_CreditUserInfoBusiness credit = new CRD_CD_CreditUserInfoBusiness();
        public BridgingBusiness() { }
     
        public BridgingBusiness(OperatorLog opeLog)
        {
            this.opeLog = opeLog;
        }
       

        #region 获取征信报告
        public string GetCreditInfo(string cert_no)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            var report = GetReportID(cert_no);
            if (report == null)
            {
                throw new Exception("身份证号为：" + cert_no + "的征信报告不存在");
            }
            decimal reportid = report.Report_Id;
            dic.Add("CRD_HD_REPORT", report);
            var identity = dao.Single<CRD_PI_IDENTITYEntity>(x => x.Report_Id == reportid);
            dic.Add("CRD_PI_IDENTITY", identity);
            AddData<CRD_PI_RESIDENCEEntity>(dic, "CRD_PI_RESIDENCE", dao.Select<CRD_PI_RESIDENCEEntity>(x => x.Report_Id == reportid));//居住信息
            AddData<CRD_PI_PROFESSNLEntity>(dic, "CRD_PI_PROFESSNL", dao.Select<CRD_PI_PROFESSNLEntity>(x => x.Report_Id == reportid));//职业信息
            AddData<CRD_IS_CREDITCUEEntity>(dic, "CRD_IS_CREDITCUE", dao.Select<CRD_IS_CREDITCUEEntity>(x => x.Report_Id == reportid));//信用提示
            AddData<CRD_CD_OverDueBreakeEntity>(dic, "CRD_CD_OverDueBreake", dao.Select<CRD_CD_OverDueBreakeEntity>(x => x.Report_Id == reportid));//逾期及违约信息概要
            AddData<CRD_IS_OVDSUMMARYEntity>(dic, "CRD_IS_OVDSUMMARY", dao.Select<CRD_IS_OVDSUMMARYEntity>(x => x.Report_Id == reportid)); //逾期(透支)信息汇总
            AddData<CRD_IS_SHAREDEBTEntity>(dic, "CRD_IS_SHAREDEBT", dao.Select<CRD_IS_SHAREDEBTEntity>(x => x.Report_Id == reportid));//未销户/未结清贷款/贷记卡/准贷记卡信息汇总
            AddData<CRD_CD_GUARANTEESummeryEntity>(dic, "CRD_CD_GUARANTEESummery", dao.Select<CRD_CD_GUARANTEESummeryEntity>(x => x.Report_Id == reportid));//对外担保信息汇总
            AddData<CRD_CD_ASSETDPSTEntity>(dic, "CRD_CD_ASSETDPST", dao.Select<CRD_CD_ASSETDPSTEntity>(x => x.Report_Id == reportid));//资产处置信息
            AddData<CRD_CD_ASRREPAYEntity>(dic, "CRD_CD_ASRREPAY", dao.Select<CRD_CD_ASRREPAYEntity>(x => x.Report_Id == reportid));//保证人代偿信息
            AddData<CRD_CD_LNEntity>(dic, "CRD_CD_LN", dao.Select<CRD_CD_LNEntity>(x => x.Report_Id == reportid));//贷款
            AddData<CRD_CD_LNDEntity>(dic, "CRD_CD_LND", dao.Select<CRD_CD_LNDEntity>(x => x.Report_Id == reportid));//贷记卡
            AddData<CRD_CD_STNCARDEntity>(dic, "CRD_CD_STNCARD", dao.Select<CRD_CD_STNCARDEntity>(x => x.Report_Id == reportid));//准贷记卡
            AddData<CRD_CD_GUARANTEEEntity>(dic, "CRD_CD_GUARANTEE", dao.Select<CRD_CD_GUARANTEEEntity>(x => x.Report_Id == reportid));//对外担保信息
            AddData<CRD_PI_TAXARREAREntity>(dic, "CRD_PI_TAXARREAR", dao.Select<CRD_PI_TAXARREAREntity>(x => x.Report_Id == reportid));//欠税记录
            AddData<CRD_PI_CIVILJDGMEntity>(dic, "CRD_PI_CIVILJDGM", dao.Select<CRD_PI_CIVILJDGMEntity>(x => x.Report_Id == reportid));//民事判决记录
            AddData<CRD_PI_FORCEEXCTNEntity>(dic, "CRD_PI_FORCEEXCTN", dao.Select<CRD_PI_FORCEEXCTNEntity>(x => x.Report_Id == reportid));//强制执行记录
            AddData<CRD_PI_ADMINPNSHMEntity>(dic, "CRD_PI_ADMINPNSHM", dao.Select<CRD_PI_ADMINPNSHMEntity>(x => x.Report_Id == reportid));//行政处罚记录
            AddData<CRD_PI_ACCFUNDEntity>(dic, "CRD_PI_ACCFUND", dao.Select<CRD_PI_ACCFUNDEntity>(x => x.Report_Id == reportid));//住房公积金参缴记录
            AddData<CRD_PI_ENDINSDPTEntity>(dic, "CRD_PI_ENDINSDPT", dao.Select<CRD_PI_ENDINSDPTEntity>(x => x.Report_Id == reportid)); //养老保险金缴存记录
            AddData<CRD_PI_ENDINSDLREntity>(dic, "CRD_PI_ENDINSDLR", dao.Select<CRD_PI_ENDINSDLREntity>(x => x.Report_Id == reportid));//养老保险金发放记录
            AddData<CRD_PI_SALVATIONEntity>(dic, "CRD_PI_SALVATION", dao.Select<CRD_PI_SALVATIONEntity>(x => x.Report_Id == reportid));//低保救助记录
            AddData<CRD_PI_COMPETENCEEntity>(dic, "CRD_PI_COMPETENCE", dao.Select<CRD_PI_COMPETENCEEntity>(x => x.Report_Id == reportid));//执业资格记录
            AddData<CRD_PI_ADMINAWARDEntity>(dic, "CRD_PI_ADMINAWARD", dao.Select<CRD_PI_ADMINAWARDEntity>(x => x.Report_Id == reportid));//行政奖励记录
            AddData<CRD_PI_VEHICLEEntity>(dic, "CRD_PI_VEHICLE", dao.Select<CRD_PI_VEHICLEEntity>(x => x.Report_Id == reportid));//车辆交易和抵押记录
            AddData<CRD_PI_TELPNTEntity>(dic, "CRD_PI_TELPNT", dao.Select<CRD_PI_TELPNTEntity>(x => x.Report_Id == reportid));//电信缴费记录
            AddData<CRD_AN_ANCINFOEntity>(dic, "CRD_AN_ANCINFO", dao.Select<CRD_AN_ANCINFOEntity>(x => x.Report_Id == reportid));//本人声明
            AddData<CRD_AN_DSTINFOEntity>(dic, "CRD_AN_DSTINFO", dao.Select<CRD_AN_DSTINFOEntity>(x => x.Report_Id == reportid));//异议标注
            AddData<CRD_QR_RECORDDTLINFOEntity>(dic, "CRD_QR_RECORDDTLINFO", dao.Select<CRD_QR_RECORDDTLINFOEntity>(x => x.Report_Id == reportid)); //信贷审批查询记录明细
            AddData<CRD_CD_LN_SPLEntity>(dic, "CRD_CD_LN_SPL", dao.Select<CRD_CD_LN_SPLEntity>(x => x.Report_Id == reportid));//贷款特殊交易
            AddData<CRD_CD_LND_SPLEntity>(dic, "CRD_CD_LND_SPL", dao.Select<CRD_CD_LND_SPLEntity>(x => x.Report_Id == reportid));//贷记卡特殊交易
            AddData<CRD_CD_STN_SPLEntity>(dic, "CRD_CD_STN_SPL", dao.Select<CRD_CD_STN_SPLEntity>(x => x.Report_Id == reportid));//准贷记卡特殊交易
            AddData<CRD_CD_LN_OVDEntity>(dic, "CRD_CD_LN_OVD", dao.Select<CRD_CD_LN_OVDEntity>(x => x.Report_Id == reportid));//贷款逾期/透支记录
            AddData<CRD_CD_LND_OVDEntity>(dic, "CRD_CD_LND_OVD", dao.Select<CRD_CD_LND_OVDEntity>(x => x.Report_Id == reportid)); //贷记卡逾期/透支记录
            AddData<CRD_CD_STN_OVDEntity>(dic, "CRD_CD_STN_OVD", dao.Select<CRD_CD_STN_OVDEntity>(x => x.Report_Id == reportid));//准贷记卡逾期/透支记录
            AddData<CRD_QR_REORDSMREntity>(dic, "CRD_QR_REORDSMR", dao.Select<CRD_QR_REORDSMREntity>(x => x.Report_Id == reportid));//查询记录汇总

            return JsonConvert.SerializeObject(dic, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });


        }

            
        //public string  GetCreditInfo(string cert_no)
        //{
        //    Dictionary<string, object> dic = new Dictionary<string, object>();
        //    var  report= GetReportID(cert_no);
        //    if(report==null)
        //    {
        //        throw  new Exception ("身份证号为："+cert_no+"的征信报告不存在");
        //    }
        //    decimal reportid =report.Report_Id;
        //    dic.Add("CRD_HD_REPORT", report);
        //    var identity= dao.Single<CRD_PI_IDENTITYEntity>(x => x.Report_Id == reportid);
        //    dic.Add("CRD_PI_IDENTITY", identity);
        //    AddData<CRD_PI_RESIDENCEEntity>(dic, "CRD_PI_RESIDENCE", dao.Select<CRD_PI_RESIDENCEEntity>(x => x.Report_Id == reportid));//居住信息
        //    AddData<CRD_PI_PROFESSNLEntity>(dic, "CRD_PI_PROFESSNL", dao.Select<CRD_PI_PROFESSNLEntity>(x => x.Report_Id == reportid));//职业信息
        //    AddData<CRD_IS_CREDITCUEEntity>(dic, "CRD_IS_CREDITCUE", dao.Select<CRD_IS_CREDITCUEEntity>(x => x.Report_Id == reportid));//信用提示
        //    AddData<CRD_CD_OverDueBreakeEntity>(dic, "CRD_CD_OverDueBreake", dao.Select<CRD_CD_OverDueBreakeEntity>(x => x.Report_Id == reportid));//逾期及违约信息概要
        //    AddData<CRD_IS_OVDSUMMARYEntity>(dic, "CRD_IS_OVDSUMMARY", dao.Select<CRD_IS_OVDSUMMARYEntity>(x => x.Report_Id == reportid)); //逾期(透支)信息汇总
        //    AddData<CRD_IS_SHAREDEBTEntity>(dic, "CRD_IS_SHAREDEBT", dao.Select<CRD_IS_SHAREDEBTEntity>(x => x.Report_Id == reportid));//未销户/未结清贷款/贷记卡/准贷记卡信息汇总
        //    AddData<CRD_CD_GUARANTEESummeryEntity>(dic, "CRD_CD_GUARANTEESummery", dao.Select<CRD_CD_GUARANTEESummeryEntity>(x => x.Report_Id == reportid));//对外担保信息汇总
        //    AddData<CRD_CD_ASSETDPSTEntity>(dic, "CRD_CD_ASSETDPST", dao.Select<CRD_CD_ASSETDPSTEntity>(x => x.Report_Id == reportid));//资产处置信息
        //    AddData<CRD_CD_ASRREPAYEntity>(dic, "CRD_CD_ASRREPAY", dao.Select<CRD_CD_ASRREPAYEntity>(x => x.Report_Id == reportid));//保证人代偿信息
        //    AddData<CRD_CD_LNEntity>(dic, "CRD_CD_LN", dao.Select<CRD_CD_LNEntity>(x => x.Report_Id == reportid));//贷款
        //    AddData<CRD_CD_LNDEntity>(dic, "CRD_CD_LND", dao.Select<CRD_CD_LNDEntity>(x => x.Report_Id == reportid));//贷记卡
        //    AddData<CRD_CD_STNCARDEntity>(dic, "CRD_CD_STNCARD", dao.Select<CRD_CD_STNCARDEntity>(x => x.Report_Id == reportid));//准贷记卡
        //    AddData<CRD_CD_GUARANTEEEntity>(dic, "CRD_CD_GUARANTEE", dao.Select<CRD_CD_GUARANTEEEntity>(x => x.Report_Id == reportid));//对外担保信息
        //    AddData<CRD_PI_TAXARREAREntity>(dic, "CRD_PI_TAXARREAR", dao.Select<CRD_PI_TAXARREAREntity>(x => x.Report_Id == reportid));//欠税记录
        //    AddData<CRD_PI_CIVILJDGMEntity>(dic, "CRD_PI_CIVILJDGM", dao.Select<CRD_PI_CIVILJDGMEntity>(x => x.Report_Id == reportid));//民事判决记录
        //    AddData<CRD_PI_FORCEEXCTNEntity>(dic, "CRD_PI_FORCEEXCTN", dao.Select<CRD_PI_FORCEEXCTNEntity>(x => x.Report_Id == reportid));//强制执行记录
        //    AddData<CRD_PI_ADMINPNSHMEntity>(dic, "CRD_PI_ADMINPNSHM", dao.Select<CRD_PI_ADMINPNSHMEntity>(x => x.Report_Id == reportid));//行政处罚记录
        //    AddData<CRD_PI_ACCFUNDEntity>(dic, "CRD_PI_ACCFUND", dao.Select<CRD_PI_ACCFUNDEntity>(x => x.Report_Id == reportid));//住房公积金参缴记录
        //    AddData<CRD_PI_ENDINSDPTEntity>(dic, "CRD_PI_ENDINSDPT", dao.Select<CRD_PI_ENDINSDPTEntity>(x => x.Report_Id == reportid)); //养老保险金缴存记录
        //    AddData<CRD_PI_ENDINSDLREntity>(dic, "CRD_PI_ENDINSDLR", dao.Select<CRD_PI_ENDINSDLREntity>(x => x.Report_Id == reportid));//养老保险金发放记录
        //    AddData<CRD_PI_SALVATIONEntity>(dic, "CRD_PI_SALVATION", dao.Select<CRD_PI_SALVATIONEntity>(x => x.Report_Id == reportid));//低保救助记录
        //    AddData<CRD_PI_COMPETENCEEntity>(dic, "CRD_PI_COMPETENCE", dao.Select<CRD_PI_COMPETENCEEntity>(x => x.Report_Id == reportid));//执业资格记录
        //    AddData<CRD_PI_ADMINAWARDEntity>(dic, "CRD_PI_ADMINAWARD", dao.Select<CRD_PI_ADMINAWARDEntity>(x => x.Report_Id == reportid));//行政奖励记录
        //    AddData<CRD_PI_VEHICLEEntity>(dic, "CRD_PI_VEHICLE", dao.Select<CRD_PI_VEHICLEEntity>(x => x.Report_Id == reportid));//车辆交易和抵押记录
        //    AddData<CRD_PI_TELPNTEntity>(dic, "CRD_PI_TELPNT", dao.Select<CRD_PI_TELPNTEntity>(x => x.Report_Id == reportid));//电信缴费记录
        //    AddData<CRD_AN_ANCINFOEntity>(dic, "CRD_AN_ANCINFO", dao.Select<CRD_AN_ANCINFOEntity>(x => x.Report_Id == reportid));//本人声明
        //    AddData<CRD_AN_DSTINFOEntity>(dic, "CRD_AN_DSTINFO", dao.Select<CRD_AN_DSTINFOEntity>(x => x.Report_Id == reportid));//异议标注
        //    AddData<CRD_QR_RECORDDTLINFOEntity>(dic, "CRD_QR_RECORDDTLINFO", dao.Select<CRD_QR_RECORDDTLINFOEntity>(x => x.Report_Id == reportid)); //信贷审批查询记录明细
        //    AddData<CRD_CD_LN_SPLEntity>(dic, "CRD_CD_LN_SPL", dao.Select<CRD_CD_LN_SPLEntity>(x => x.Report_Id == reportid));//贷款特殊交易
        //    AddData<CRD_CD_LND_SPLEntity>(dic, "CRD_CD_LND_SPL", dao.Select<CRD_CD_LND_SPLEntity>(x => x.Report_Id == reportid));//贷记卡特殊交易
        //    AddData<CRD_CD_STN_SPLEntity>(dic, "CRD_CD_STN_SPL", dao.Select<CRD_CD_STN_SPLEntity>(x => x.Report_Id == reportid));//准贷记卡特殊交易
        //    AddData<CRD_CD_LN_OVDEntity>(dic, "CRD_CD_LN_OVD", dao.Select<CRD_CD_LN_OVDEntity>(x => x.Report_Id == reportid));//贷款逾期/透支记录
        //    AddData<CRD_CD_LND_OVDEntity>(dic, "CRD_CD_LND_OVD", dao.Select<CRD_CD_LND_OVDEntity>(x => x.Report_Id == reportid)); //贷记卡逾期/透支记录
        //    AddData<CRD_CD_STN_OVDEntity>(dic, "CRD_CD_STN_OVD", dao.Select<CRD_CD_STN_OVDEntity>(x => x.Report_Id == reportid));//准贷记卡逾期/透支记录
        //    AddData<CRD_QR_REORDSMREntity>(dic, "CRD_QR_REORDSMR", dao.Select<CRD_QR_REORDSMREntity>(x => x.Report_Id == reportid));//查询记录汇总

        //    return  JsonConvert.SerializeObject(dic,new IsoDateTimeConverter(){DateTimeFormat="yyyy-MM-dd hh:mm:ss"});


        //}
        private void AddData<T>(Dictionary<string, object> dic, string key, List<T> list)
        {
            if (list != null && list.Count != 0)
            {
                dic.Add(key, list);
            }
        }

        public CRD_HD_REPORTEntity GetReportID(string cert_no,string name=null)
        {
             CRD_HD_REPORTEntity report = null;
            //var reportList = dao.Select<CRD_HD_REPORTEntity>(x => x.Cert_No == cert_no);
            //if (reportList != null&&reportList.Count!=0)
            //{
            //    if (reportList.Count == 1)
            //    {
            //        report = reportList[0];
            //    }
            //    else
            //    {
            //        report = reportList.OrderByDescending(x=>x.Report_Id).First();
            //    }
            //}

            string sqlstr = "select top 1 * from credit.CRD_HD_REPORT with(nolock) where 1=1 and cert_no='" + cert_no + "'";
            if (!string.IsNullOrEmpty(name))
            {
                sqlstr += " and name='" + name + "'";
            }
            sqlstr += " order by Report_Id desc";
            var reportList = dao.Select<CRD_HD_REPORTEntity>(sqlstr);
            if (reportList != null && reportList.Count > 0)
            {
                report = reportList[0];
            }
            return report;
        }
        public Tuple<decimal,string> GetReportIDAndStateByIdentityNo(string identityNo,int limiteddays)
        {
            if (!new CRD_CD_CreditUserInfoBusiness().isInLimitedDays(limiteddays, identityNo))
            {
                return new Tuple<decimal, string>(0, null);
            }
            var report = GetReportID(identityNo);
            if (report == null)
            {
                return new Tuple<decimal, string>(0, null);
            }
            return new Tuple<decimal, string>(report.Report_Id, report.Report_Sn);
        }
        public CRD_HD_REPORTEntity GetReportWithinLimiteDays(string identityNo, int limiteddays,string name)
        {
            if (!new CRD_CD_CreditUserInfoBusiness().isInLimitedDays(limiteddays, identityNo, name))
            {
                return null;
            }
            var report = GetReportID(identityNo);
            if (report == null)
            {
                return null;
            }
            return report;
        }
        #endregion

        #region 担保征信入库
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dic">dic key:文件名  value ：该文件相应的数据</param>
        public CRD_HD_REPORTEntity DanBaoSaveDataToDb(Dictionary<string, Dictionary<string, DataTable>> dic)
        {
            StringBuilder sb = new StringBuilder();//拼sql用
            NolmalBusinesshelper helper = new NolmalBusinesshelper();
            CRD_HD_REPORTEntity report = new CRD_HD_REPORTEntity();
            using (var db = dao.Open())
            {
                foreach (var itemdic in dic)
                {
                    DateTime reportcreateTime = DateTime.Now;
                    string cert_no = null;
                    string report_sn = null;
                    using (var transaction = db.OpenTransaction())
                    {
                        try
                        {
                            //主表入库
                            report = InsertDanBaoReport(db, itemdic, ref  reportcreateTime, ref cert_no, ref report_sn);
                            
                            //给其他表添加主表主键
                            addMainIdColumn(itemdic.Value, (long)report.Report_Id);
                            //贷款，贷记卡，准贷记卡入库
                            InsertCreditCard(itemdic.Value, db);
                            AddType_Dw(itemdic.Value);
                            //所有表入库
                            InsertAllEntity(db, itemdic, sb);
                            transaction.Commit();
                            
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Log4netAdapter.WriteError("html:" + itemdic.Key + "入库出现异常", ex);
                            throw ex;

                        }
                    }
                    if (itemdic.Value.ContainsKey(CommonData.CRD_QR_RECORDDTLINFO))
                        InsertCRD_QR_RECORDDTLEntity(db, itemdic.Value[CommonData.CRD_QR_RECORDDTLINFO], reportcreateTime, report.Report_Id);//查询详细信息汇总

                }
            }
            return report;
        }

        private CRD_HD_REPORTEntity InsertDanBaoReport(IDbConnection db, KeyValuePair<string, Dictionary<string, DataTable>> keyValue, ref DateTime reportCreateTime, ref string cert_no, ref string report_sn)
        {
            CRD_HD_REPORTEntity report = new CRD_HD_REPORTEntity();
            var reportdt = keyValue.Value[CommonData.creditReportTable];
            cert_no = reportdt.Rows[0]["Cert_No"].ToString();
            reportCreateTime = Convert.ToDateTime(reportdt.Rows[0]["Report_Create_Time"]);
            report_sn = reportdt.Rows[0]["Report_Sn"].ToString();
            report.Cert_No = cert_no;
            report.Report_Create_Time = reportCreateTime;
            report.Report_Sn = report_sn;
            report.Name = reportdt.Rows[0]["Name"].ToString();
            report.Cert_Type = reportdt.Rows[0]["Cert_Type"].ToString();
            report.Query_Reason = reportdt.Rows[0]["Query_Reason"].ToString();
            string query_org= reportdt.Rows[0]["Query_Org"].ToString();
            int index= query_org.IndexOf("/");
            if(index!=-1)
            {
                 report.Query_Org =query_org.Substring(0,index);
            }
            report.Query_Time = Convert.ToDateTime(reportdt.Rows[0]["Query_Time"]);
            report.SourceType = (byte)SysEnums.SourceType.Assure;

            var id =db.Insert(report, selectIdentity: true);
            report.Report_Id = id;
            keyValue.Value.Remove(CommonData.creditReportTable);
            return report;
        }

        #endregion


        #region  直接用Datatable批量插入

        public void  SaveDataToDB(Dictionary<string, Dictionary<string, DataTable>> dic)
        {
            StringBuilder sb = new StringBuilder();//拼sql用
            NolmalBusinesshelper helper = new NolmalBusinesshelper();
            using (var db = dao.Open())
            {
                foreach (var itemdic in dic)
                {
                    long reportid = 0;
                    DateTime reportcreateTime = DateTime.Now;
                    string cert_no = null;
                    string report_sn=null;
                    using (var transaction = db.OpenTransaction())
                    {
                        try
                        {
                            //主表入库
                            reportid = InsertReportEntity(db, itemdic,ref  reportcreateTime,ref cert_no,ref report_sn);
                            //给其他表添加主表主键
                            addMainIdColumn(itemdic.Value, reportid);
                            //贷款，贷记卡，准贷记卡入库
                            InsertCreditCard(itemdic.Value, db);
                            AddType_Dw(itemdic.Value);
                            //所有表入库
                            InsertAllEntity(db, itemdic, sb);
                            //更新状态
                            credit.UpdateState(db, cert_no, report_sn);
                            //入库移动文件到指定文件夹
                            helper.MoveCommitExcel(itemdic.Key);
                            transaction.Commit();
                            opeLog.AddSuccessNum(1);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            opeLog.AddFailNum(1).AddfailReson(new FailReason(itemdic.Key, "", "", "入库出现异常" + ex.Message));                       
                            Log4netAdapter.WriteError("html:" + itemdic.Key + "入库出现异常", ex);
                        }
                    }
                    if(itemdic.Value.ContainsKey(CommonData.CRD_QR_RECORDDTLINFO))
                        InsertCRD_QR_RECORDDTLEntity(db, itemdic.Value[CommonData.CRD_QR_RECORDDTLINFO], reportcreateTime, reportid);//查询详细信息汇总

                }
            }
        }
      
        private  void AddType_Dw(Dictionary<string, DataTable> dic)
        {
            if(dic.Keys.Contains (CommonData.CRD_CD_OutstandeSummary))
            {
                dic[CommonData.CRD_CD_OutstandeSummary].Columns.Add(new DataColumn("Type_Dw") { DefaultValue = CommonData.CRD_CD_OutstandeSummary });
            }
            if (dic.Keys.Contains(CommonData.CRD_CD_NoCancellLND))
            {
                dic[CommonData.CRD_CD_NoCancellLND].Columns.Add(new DataColumn("Type_Dw") { DefaultValue = CommonData.CRD_CD_NoCancellLND });
            }
            if (dic.Keys.Contains(CommonData.CRD_CD_NoCancellSTNCARD))
            {
                dic[CommonData.CRD_CD_NoCancellSTNCARD].Columns.Add(new DataColumn("Type_Dw") { DefaultValue = CommonData.CRD_CD_NoCancellSTNCARD });
            }
        }
        private void InsertCreditCard(Dictionary<string, DataTable> dic, IDbConnection db)
        {
            DealingCreditCard(dic, CommonData.CRD_CD_LN, CommonData.CRD_CD_LN_SPL, CommonData.CRD_CD_LN_OVD, "Loan_Id", db);
            DealingCreditCard(dic, CommonData.CRD_CD_LND, CommonData.CRD_CD_LND_SPL, CommonData.CRD_CD_LND_OVD, "Loancard_Id", db);
            DealingCreditCard(dic, CommonData.CRD_CD_STNCARD, CommonData.CRD_CD_STN_SPL, CommonData.CRD_CD_STN_OVD, "Standardloancard_Id", db);
        }
        private  string Getvalue(KeyValuePair<string, Dictionary<string, DataTable>> itemdic,string  columName)
        {
            string value = null;
            try
            {
                foreach (var item in itemdic.Value)
                {
                    if (item.Value.Columns.Contains(columName))
                    {
                        value = item.Value.Rows[0][columName].ToString();

                        break;
                    }
                }
                if (value == null)
                    value = GetValueByExcel(itemdic.Key, columName);
            }
            catch 
            {
                Log4netAdapter.WriteInfo("解析"+itemdic.Key+"出现问题！");
            }
          
            return value;
        }   
        private string GetValueByExcel(string path,string columName)
        {
            
            var dt= NPOIHelper.GetDataTable(path, false);
            string value = string.Empty;
            int index = -1;
            foreach(DataRow item in dt.Rows)
            {
                 if(index!=-1)
                 {
                     value= item[index].ToString();
                     break;
                 }
                 for(int i=0 ;i<dt.Columns.Count;i++)
                 {
                    if(item[i].ToString()==columName)
                    {
                        index = i;
                        break;
                    }
                 }
            }
            return value;

        }
        private long InsertReportEntity(IDbConnection db, KeyValuePair<string, Dictionary<string, DataTable>> keyValue,ref DateTime reportCreateTime,ref string cert_no,ref string report_sn)
        {
            string[] NameAndCert = keyValue.Key.Split('_');
            DataTable  reportkeyvalue=null;
            if(keyValue.Value.ContainsKey(CommonData.CRD_PI_IDENTITY))
             reportkeyvalue = keyValue.Value[CommonData.CRD_PI_IDENTITY];
            long reportId = 0;
            string cert_NO = NameAndCert.Length == 2 ? NameAndCert[1].Substring(0, NameAndCert[1].IndexOf('.')) : "";
            var credituser =  GetCert_TypeByCert_NO(cert_NO,db);
            var report= new CRD_HD_REPORTEntity()
            {
                Cert_No = cert_NO,
                Cert_Type =CommonData.certTypeDic[credituser.Cert_Type],
                Report_Sn = reportkeyvalue == null ?Getvalue(keyValue, "报告编号") : reportkeyvalue.Rows[0]["Report_sn"].ToString(),
                Name = reportkeyvalue == null ? Getvalue(keyValue, "客户姓名") : reportkeyvalue.Rows[0]["Name"].ToString(),

            };
            if (keyValue.Value.ContainsKey("外贸信托个性化输出"))
            {
                var dt = keyValue.Value["外贸信托个性化输出"];
                report.Query_Time = Convert.ToDateTime(dt.Rows[0]["查询请求时间"]);
                report.Report_Create_Time = Convert.ToDateTime(dt.Rows[0]["报告时间"]);
                report.EvaluationResult=dt.Rows[0]["外贸信托评级结果"].ToString ();
                reportCreateTime =report.Report_Create_Time.Value;
                keyValue.Value.Remove("外贸信托个性化输出");
            }
            reportId = db.Insert((report), selectIdentity: true);
            if (reportkeyvalue!=null)
            {
                reportkeyvalue.Columns.Remove("Report_sn");
                reportkeyvalue.Columns.Remove("Name");
            }
            cert_no = report.Cert_No;
            report_sn = report.Report_Sn;
            return reportId;
        }
        private void addMainIdColumn(Dictionary<string, DataTable> keyValue, long reportid)
        {
            if (reportid == 0)
            {
                throw new Exception("报告主表插入失败！");
            }
            foreach (var item in keyValue)
            {
                DataColumn dc = new DataColumn("Report_Id", typeof(decimal)) { DefaultValue = reportid };
                item.Value.Columns.Add(dc);
            }
        }

        private void InsertAllEntity(IDbConnection db, KeyValuePair<string, Dictionary<string, DataTable>> keyValue, StringBuilder sb)
        {
            foreach (var item in keyValue.Value)
            {
                if (item.Value.Rows.Count == 0)
                    continue;
                try
                {
                 
                    db.ExecuteNonQuery(
               getsqlWithDataTable(item.Value, TableMapData.GetDbName(item.Key), sb)
               );
                }
                catch(Exception ex)
                {
                    Log4netAdapter.WriteError("fileName:" +keyValue.Key+".tableName:" + item.Key + "入库出现异常.sql:"+sb.ToString(), ex);
                    throw ex;
                }
           
            }
        }
        private void InsertCRD_QR_RECORDDTLEntity(IDbConnection db,DataTable RECORDDTLINFODt, DateTime createTime, decimal report_id)
        {

            if (RECORDDTLINFODt.Rows.Count != 0)
            {
             
                try
                {
                    CRD_QR_RECORDDTLEntity entity = new CRD_QR_RECORDDTLEntity() { Report_Id = report_id };
                    string  lastthreeMonth = createTime.AddMonths(-3).ToString("yyyy-MM-dd");
                    string  lastoneMonth = createTime.AddMonths(-1).ToString("yyyy-MM-dd");
                    string creattime = createTime.ToString("yyyy-MM-dd");
                    DataRow[] drs = RECORDDTLINFODt.Select(" Query_Reason='贷款审批'  and Query_Date >'" + lastthreeMonth + "'  and Query_Date <= '" + creattime + "' ");
                    entity.COUNT_loan_IN3M = GetTimeNum(drs);
                    drs = RECORDDTLINFODt.Select(" Query_Reason='信用卡审批'  and Query_Date >'" + lastthreeMonth + "'  and Query_Date <= '" + creattime + "'  ");
                    entity.COUNT_CARD_IN3M = GetTimeNum(drs);
                    drs = RECORDDTLINFODt.Select(" Query_Reason='贷款审批'  and  Query_Date >'" + lastoneMonth + "' and  Query_Date <='" + creattime + "' ");
                    entity.COUNT_loan_IN1M = GetTimeNum(drs);
                    drs = RECORDDTLINFODt.Select(" Query_Reason='信用卡审批'  and Query_Date >'" + lastoneMonth + "' and  Query_Date <='" + creattime + "' ");
                    entity.COUNT_CARD_IN1M = GetTimeNum(drs);
                    db.Insert<CRD_QR_RECORDDTLEntity>(entity);
                }
                catch(Exception  ex)
                {
                    Log4netAdapter.WriteError("reportid" + report_id.ToString() + "查询明细信息汇总出现异常", ex);
                }
             
            }
        }
      
        private int GetTimeNum(DataRow[] drs)
        {
            if (drs == null)
                return 0;
            else
                return drs.Length;

        }
        private string getsqlWithDataTable(DataTable dt, string tableName, StringBuilder sb)
        {
          
            sb.Clear();
            if (dt.Columns.Contains("报告编号"))
                dt.Columns.Remove("报告编号");
            if (dt.Columns.Contains("客户姓名"))
                dt.Columns.Remove("客户姓名");
            if (dt.Columns.Contains("编号"))
                dt.Columns.Remove("编号");
            if (dt.Columns.Contains("业务类型"))
                dt.Columns.Remove("业务类型");

            sb.Append("insert into " + tableName + "(");
            foreach (DataColumn item in dt.Columns)
            {
                sb.Append(item.ColumnName + ",");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(")");

            int index = 0;
            foreach (DataRow item in dt.Rows)
            {
                sb.Append("select ");
                foreach (var dritem in item.ItemArray)
                {

                    CreateSql(dt, sb, index, dritem);
                       
                    index++;
                }
                index = 0;
                sb.Remove(sb.Length - 1, 1);
                sb.Append(" union all ");
            }
            sb.Remove(sb.Length - 10, 10);
            return sb.ToString();
        }

        private static void CreateSql(DataTable dt, StringBuilder sb, int index, object dritem)
        {
            if (dt.Columns[index].DataType == typeof(string) || dt.Columns[index].DataType == typeof(DateTime))
            {
                if (dritem == DBNull.Value)
                {
                    sb.Append("  null ,");
                }
                else
                {
                    sb.Append("'" + dritem.ToString().Replace("'","") + "' ,");
                }

            }
            else
            {
                if (dritem == DBNull.Value)
                {
                    sb.Append("  null  ,");
                }
                else
                {
                    sb.Append(dritem.ToString()+" ,");
                }

            }
        }

    

        private void DealingCreditCard(Dictionary<string, DataTable> dic, string tableName, string splName, string ovdName, string idName, IDbConnection db)
        {
            StringBuilder colsb = new StringBuilder();
            if (!dic.ContainsKey(tableName))
                return;
            var dt = dic[tableName];
            colsb.Append("insert into " + TableMapData.GetDbName(tableName) + "(");
            foreach (DataColumn item in dt.Columns)
            {
                if (item.ColumnName == "编号" || item.ColumnName == "报告编号" || item.ColumnName == "客户姓名")
                    continue;
                colsb.Append(item.ColumnName + ",");
            }
            colsb.Remove(colsb.Length - 1, 1);
            colsb.Append(")");

            StringBuilder valSb = new StringBuilder();
            decimal creditid = 0;
            int index = 0;
            DataTable dtspl = null;
            DataTable dtovd = null;
            if (dic.ContainsKey(splName))
            {
                dtspl = dic[splName];
                dtspl.Columns.Add(new DataColumn(idName, typeof(decimal)));
            }

            if (dic.ContainsKey(ovdName))
            {
                dtovd = dic[ovdName];
                dtovd.Columns.Add(new DataColumn(idName, typeof(decimal)));
            }
            foreach (DataRow item in dt.Rows)
            {
                valSb.Append(" values( ");
                foreach (var dritem in item.ItemArray)
                {
                    if (dt.Columns[index].ColumnName == "编号" || dt.Columns[index].ColumnName == "报告编号" || dt.Columns[index].ColumnName == "客户姓名")
                    {
                        index++;
                        continue;
                    }
                     
                    CreateSql(dt, valSb, index, dritem);
                    index++;
                }
                 
                creditid = db.Scalar<int>(colsb.ToString()+valSb.ToString().Trim(',')+")"+";select @@identity;");
      
                DataRow[] result = null;
                if (dtspl != null)
                {
                    result = dtspl.Select(" 编号='" + item["编号"].ToString() + "'");
                    foreach (var dr in result)
                    {
                        dr[idName] = creditid;
                    }
                }

                if (dtovd != null)
                {
                    result = dtovd.Select(" 编号='" + item["编号"].ToString() + "'");
                    foreach (var dr in result)
                    {
                        dr[idName] = creditid;
                    }
                }
                valSb.Clear();
                index = 0;
            }
            dic.Remove(tableName);
        }


        public CRD_CD_CreditUserInfoEntity GetCert_TypeByCert_NO(string cert_NO, IDbConnection db)
        {
            var models = db.Select<CRD_CD_CreditUserInfoEntity>("select Cert_Type,Name from credit.CRD_CD_CreditUserInfo where Cert_No=@Cert_No ", new { Cert_No = cert_NO });
            if (models == null || models.Count == 0)
                return new CRD_CD_CreditUserInfoEntity { Cert_Type = "0"};
            else
                return models.First();
        }  

        #endregion  
 
    }
}
