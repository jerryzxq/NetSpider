using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
using ServiceStack.OrmLite;
using Vcredit.Common.Utility;
using System.Data;
using Vcredit.ExtTrade.ModelLayer.Nolmal.NewForeignTradeModel;
using Vcredit.ExtTrade.ModelLayer.Common;

namespace Vcredit.ExtTrade.BusinessLayer.CommonBusiness
{
    public class ForeignComBus
    {
        readonly NolmalBusinesshelper busHelper = new NolmalBusinesshelper();
        CRD_CD_CreditUserInfoBusiness creditBus = new CRD_CD_CreditUserInfoBusiness();

    
        public bool  SaveShangHaiCreditInfoToDB(NewForeignContainer item, CRD_CD_CreditUserInfoEntity creidtEntity)
        {

            BaseDao dao = new BaseDao();

            using (var db = dao.Open())
            {
                long reportid = default(long);
                using (var transaction = db.OpenTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(item.CRD_HD_REPORT.Report_Sn))
                        {
                            if (!string.IsNullOrEmpty(item.CRD_HD_REPORT.Cert_No))
                            {
                               creditBus.SaveCredit(creidtEntity.CreditUserInfo_Id.Value, (byte)RequestState.HaveNoData, "征信空白");
                                Log4netAdapter.WriteInfo(string.Format("姓名：{0}，身份证：{1}征信空白", item.CRD_HD_REPORT.Name, item.CRD_HD_REPORT.Cert_No));
                            }
                            transaction.Commit();
                            return true;
                        }
                       reportid = db.Insert(item.CRD_HD_REPORT, selectIdentity: true);//主表
                      var sql= db.GetLastSql();
                        if (reportid == 0)
                        {
                            creditBus.SaveCredit(creidtEntity.CreditUserInfo_Id.Value, (byte)RequestState.AnalysisFail, "主表入库失败");
                            Log4netAdapter.WriteInfo(string.Format("姓名：{0}，身份证：{1}入库出现异常", item.CRD_HD_REPORT.Name, item.CRD_HD_REPORT.Cert_No));
                            transaction.Rollback();
                            return false;
                        }
                        busHelper.InsertEntity(item.CRD_PI_IDENTITY, reportid, db);//身份信息入库
                        busHelper.InsertList(item.CRD_PI_RESIDENCE, reportid, db);//居住信息入库
                        busHelper.InsertList(item.CRD_PI_PROFESSNL, reportid, db);//职业信息
                        busHelper.InsertList(item.CRD_IS_CREDITCUE, reportid, db);//信用提示
                        busHelper.InsertList(item.CRD_CD_OverDueBreake, reportid, db);//逾期及违约信息概要
                        busHelper.InsertList(item.CRD_IS_OVDSUMMARY, reportid, db);//逾期(透支)信息汇总
                        busHelper.InsertList(item.CRD_IS_SHAREDEBT, reportid, db);//未销户或者未结清贷款贷记卡，准贷记卡
                        busHelper.InsertList(item.CRD_CD_GUARANTEESummery, reportid, db);//对外担保信息汇总
                        busHelper.InsertList(item.CRD_CD_ASSETDPST, reportid, db);//资产处置信息
                        busHelper.InsertList(item.CRD_CD_ASRREPAY, reportid, db);//保证人代偿信息
                        busHelper.InsertLnList(item.CRD_CD_LN, reportid, db);//贷款信息
                        busHelper.InsertLndList(item.CRD_CD_LND, reportid, db);//贷记卡信息
                        busHelper.InsertstnList(item.CRD_CD_STNCARD, reportid, db);//准贷记卡信息
                        busHelper.InsertList(item.CRD_CD_GUARANTEE, reportid, db);//对外担保信息
                        busHelper.InsertList(item.CRD_PI_TAXARREAR, reportid, db);//欠税记录
                        busHelper.InsertList(item.CRD_PI_FORCEEXCTN, reportid, db);//强制执行记录
                        busHelper.InsertList(item.CRD_PI_ADMINPNSHM, reportid, db);//行政处罚记录
                        busHelper.InsertList(item.CRD_PI_ACCFUND, reportid, db);//住房公积金缴存记录
                        busHelper.InsertList(item.CRD_PI_ENDINSDPT, reportid, db);//养老保险缴存记录
                        busHelper.InsertList(item.CRD_PI_ENDINSDLR, reportid, db);//养老保险发放记录
                        busHelper.InsertList(item.CRD_PI_SALVATION, reportid, db);//低保救济记录
                        busHelper.InsertList(item.CRD_PI_COMPETENCE, reportid, db);//执业资格记录
                        busHelper.InsertList(item.CRD_PI_ADMINAWARD, reportid, db);//行政奖励记录
                        busHelper.InsertList(item.CRD_PI_VEHICLE, reportid, db);//车辆交易和抵押记录
                        busHelper.InsertList(item.CRD_PI_TELPNT, reportid, db);//电信缴费记录
                        busHelper.InsertList(item.CRD_AN_ANCINFO, reportid, db);//本人声明
                        busHelper.InsertList(item.CRD_AN_DSTINFO, reportid, db);//异议标注
                        busHelper.InsertList(item.CRD_QR_REORDSMR, reportid, db);//查询记录汇总
                        busHelper.InsertList(item.CRD_QR_RECORDDTLINFO, reportid, db);//信贷审批查询记录明细
                        creditBus.UpdateStateInfo((byte)RequestState.SuccessCome, creidtEntity.CreditUserInfo_Id.Value, null, item.CRD_HD_REPORT.Report_Sn);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        creditBus.SaveCredit( creidtEntity.CreditUserInfo_Id.Value,(byte)RequestState.AnalysisFail, "入库失败");
                        Log4netAdapter.WriteError(string.Format("姓名：{0}，身份证：{1}入库出现异常", item.CRD_HD_REPORT.Name, item.CRD_HD_REPORT.Cert_No), ex);
                        return false;
                    }
                }
                if (reportid != 0)
                    InsertCRD_QR_RECORDDTLEntity(db, item.CRD_QR_RECORDDTLINFO, item.CRD_HD_REPORT.Report_Create_Time, reportid);
                return true;
            }
        }
        public List<CRD_HD_REPORTEntity> SaveNewForeignCreditInfoToDB(List<NewForeignContainer> containnerList, string batchno, string brNo)
        {
            BaseDao dao = new BaseDao();
            List<CRD_HD_REPORTEntity> reportList = new List<CRD_HD_REPORTEntity>();
            using (var db = dao.Open())
            {
                foreach (var item in containnerList)
                {
                    if (item == null)
                    {
                        Log4netAdapter.WriteInfo("批次号:" + batchno + "CreditDataList出现null值");
                        reportList.Add(item.CRD_HD_REPORT);
                        continue;
                    }
                    long reportid = 0;
                    using (var transaction = db.OpenTransaction())
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(item.CRD_HD_REPORT.Report_Sn))
                            {
                                if (!string.IsNullOrEmpty(item.CRD_HD_REPORT.Cert_No))
                                {
                                    creditBus.UpdateStateByBatNo(batchno, brNo, (byte)RequestState.HaveNoData, string.Empty, "没有征信报告", item.CRD_HD_REPORT.Cert_No, db); //根据身份证号，批次号和机构号
                                    Log4netAdapter.WriteInfo(string.Format("姓名：{0}，身份证：{1}征信空白", item.CRD_HD_REPORT.Name, item.CRD_HD_REPORT.Cert_No));
                                }
                                else
                                {
                                    Log4netAdapter.WriteInfo("身份证不能为空");
                                }
                                transaction.Commit();
                                reportList.Add(item.CRD_HD_REPORT);
                                continue;
                            }
                            reportid = db.Insert(item.CRD_HD_REPORT, selectIdentity: true);//主表
                            if (reportid == 0)
                            {
                                Log4netAdapter.WriteInfo(string.Format("姓名：{0}，身份证：{1}入库出现异常", item.CRD_HD_REPORT.Name, item.CRD_HD_REPORT.Cert_No));
                                transaction.Rollback();
                                reportList.Add(item.CRD_HD_REPORT);
                                continue;
                            }
                            busHelper.InsertEntity(item.CRD_PI_IDENTITY, reportid, db);//身份信息入库
                            busHelper.InsertList(item.CRD_PI_RESIDENCE, reportid, db);//居住信息入库
                            busHelper.InsertList(item.CRD_PI_PROFESSNL, reportid, db);//职业信息
                            busHelper.InsertList(item.CRD_IS_CREDITCUE, reportid, db);//信用提示
                            busHelper.InsertList(item.CRD_CD_OverDueBreake, reportid, db);//逾期及违约信息概要
                            busHelper.InsertList(item.CRD_IS_OVDSUMMARY, reportid, db);//逾期(透支)信息汇总
                            busHelper.InsertList(item.CRD_IS_SHAREDEBT, reportid, db);//未销户或者未结清贷款贷记卡，准贷记卡
                            busHelper.InsertList(item.CRD_CD_GUARANTEESummery, reportid, db);//对外担保信息汇总
                            busHelper.InsertList(item.CRD_CD_ASSETDPST, reportid, db);//资产处置信息
                            busHelper.InsertList(item.CRD_CD_ASRREPAY, reportid, db);//保证人代偿信息
                            busHelper.InsertLnList(item.CRD_CD_LN, reportid, db);//贷款信息
                            busHelper.InsertLndList(item.CRD_CD_LND, reportid, db);//贷记卡信息
                            busHelper.InsertstnList(item.CRD_CD_STNCARD, reportid, db);//准贷记卡信息
                            busHelper.InsertList(item.CRD_CD_GUARANTEE, reportid, db);//对外担保信息
                            busHelper.InsertList(item.CRD_PI_TAXARREAR, reportid, db);//欠税记录
                            busHelper.InsertList(item.CRD_PI_FORCEEXCTN, reportid, db);//强制执行记录
                            busHelper.InsertList(item.CRD_PI_ADMINPNSHM, reportid, db);//行政处罚记录
                            busHelper.InsertList(item.CRD_PI_ACCFUND, reportid, db);//住房公积金缴存记录
                            busHelper.InsertList(item.CRD_PI_ENDINSDPT, reportid, db);//养老保险缴存记录
                            busHelper.InsertList(item.CRD_PI_ENDINSDLR, reportid, db);//养老保险发放记录
                            busHelper.InsertList(item.CRD_PI_SALVATION, reportid, db);//低保救济记录
                            busHelper.InsertList(item.CRD_PI_COMPETENCE, reportid, db);//执业资格记录
                            busHelper.InsertList(item.CRD_PI_ADMINAWARD, reportid, db);//行政奖励记录
                            busHelper.InsertList(item.CRD_PI_VEHICLE, reportid, db);//车辆交易和抵押记录
                            busHelper.InsertList(item.CRD_PI_TELPNT, reportid, db);//电信缴费记录
                            busHelper.InsertList(item.CRD_AN_ANCINFO, reportid, db);//本人声明
                            busHelper.InsertList(item.CRD_AN_DSTINFO, reportid, db);//异议标注
                            busHelper.InsertList(item.CRD_QR_REORDSMR, reportid, db);//查询记录汇总
                            busHelper.InsertList(item.CRD_QR_RECORDDTLINFO, reportid, db);//信贷审批查询记录明细
                            creditBus.UpdateStateByBatNo(batchno, brNo, (byte)RequestState.SuccessCome, item.CRD_HD_REPORT.Report_Sn, string.Empty, item.CRD_HD_REPORT.Cert_No, db); //根据身份证号，批次号和机构号
                            item.CRD_HD_REPORT.Report_Id = reportid;
                            reportList.Add(item.CRD_HD_REPORT);
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            creditBus.UpdateStateByBatNo(batchno, brNo, (byte)RequestState.AnalysisFail, item.CRD_HD_REPORT.Report_Sn, "征信入库出现异常", item.CRD_HD_REPORT.Cert_No); //根据身份证号，批次号和机构号
                            reportList.Add(item.CRD_HD_REPORT);
                            Log4netAdapter.WriteError(string.Format("姓名：{0}，身份证：{1}入库出现异常", item.CRD_HD_REPORT.Name, item.CRD_HD_REPORT.Cert_No), ex);
                        }
                    }
                    if (reportid != 0)
                        InsertCRD_QR_RECORDDTLEntity(db, item.CRD_QR_RECORDDTLINFO, item.CRD_HD_REPORT.Report_Create_Time, reportid);

                   
                }
            }
            return reportList;
        }
        public void InsertCRD_QR_RECORDDTLEntity(IDbConnection db, List<CRD_QR_RECORDDTLINFOEntity> recordList, DateTime? createTime, decimal report_id)
        {

            if (recordList != null && recordList.Count != 0)
            {

                try
                {
                    CRD_QR_RECORDDTLEntity entity = new CRD_QR_RECORDDTLEntity() { Report_Id = report_id };
                    DateTime lastthreeMonth = createTime.Value.AddMonths(-3);
                    DateTime lastoneMonth = createTime.Value.AddMonths(-1);

                    var drs = recordList.Where(x => x.Query_Reason == "贷款审批" && DateTime.Compare(x.Query_Date.Value, lastthreeMonth) > 0 && DateTime.Compare(x.Query_Date.Value, createTime.Value) <= 0);
                    entity.COUNT_loan_IN3M = GetTimeNum(drs);
                    drs = recordList.Where(x => x.Query_Reason == "信用卡审批" && DateTime.Compare(x.Query_Date.Value, lastthreeMonth) > 0 && DateTime.Compare(x.Query_Date.Value, createTime.Value) <= 0);
                    entity.COUNT_CARD_IN3M = GetTimeNum(drs);
                    drs = recordList.Where(x => x.Query_Reason == "贷款审批" && DateTime.Compare(x.Query_Date.Value, lastoneMonth) > 0 && DateTime.Compare(x.Query_Date.Value, createTime.Value) <= 0);
                    entity.COUNT_loan_IN1M = GetTimeNum(drs);
                    drs = recordList.Where(x => x.Query_Reason == "信用卡审批" && DateTime.Compare(x.Query_Date.Value, lastoneMonth) > 0 && DateTime.Compare(x.Query_Date.Value, createTime.Value) <= 0);
                    entity.COUNT_CARD_IN1M = GetTimeNum(drs);
                    db.Insert<CRD_QR_RECORDDTLEntity>(entity);
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError("reportid" + report_id.ToString() + "查询明细信息汇总出现异常", ex);
                }

            }
        }
        private int GetTimeNum(IEnumerable<CRD_QR_RECORDDTLINFOEntity> recordList)
        {
            if (recordList == null || recordList.Count() == 0)
                return 0;
            else
                return recordList.Count();

        }

    }
}
