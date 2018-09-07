using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
using ServiceStack.OrmLite;
using System.Data;
using Vcredit.Common.Utility;

namespace Vcredit.ExtTrade.BusinessLayer.CommonBusiness
{
    public class JinCityComBus
    {
        readonly NolmalBusinesshelper busHelper = new NolmalBusinesshelper();
        readonly BaseDao dao = new BaseDao();
        public void SaveJinCityCreditInfoToDB(CreditContainer jincityContainer)
        {
            using (var db = dao.Open())
            {
                using (var transaction = db.OpenTransaction())
                {
                    try
                    {
                        long reportid = db.Insert(jincityContainer.Report, selectIdentity: true);//主表
                        if (reportid == 0)
                        {
                            Log4netAdapter.WriteInfo(string.Format("姓名：{0}，身份证：{1}入库出现异常", jincityContainer.Report.Name, jincityContainer.Report.Cert_No));
                            transaction.Rollback();
                            return;
                        }
                        busHelper.InsertEntity(jincityContainer.Identity, reportid, db);//身份信息入库
                        busHelper.InsertList(jincityContainer.ResidenceList, reportid, db);//居住信息入库
                        busHelper.InsertList(jincityContainer.ProfessionList, reportid, db);//职业信息
                        busHelper.InsertEntity(jincityContainer.NoCancellLND, reportid, db);//未销户贷记卡
                        busHelper.InsertEntity(jincityContainer.NoCancellSTNCARD, reportid, db);//为销户准贷记卡
                        busHelper.InsertEntity(jincityContainer.OutStandeSummary, reportid, db);//未结清贷款
                        busHelper.InsertLnList(jincityContainer.LnList, reportid, db);//贷款信息
                        busHelper.InsertLndList(jincityContainer.LndiLst, reportid, db);//贷记卡信息
                        busHelper.InsertstnList(jincityContainer.StnCardList, reportid, db);//准贷记卡信息
                        busHelper.InsertList(jincityContainer.GuaranteeList, reportid, db);//对外担保信息
                        busHelper.InsertList(jincityContainer.AccfundList, reportid, db);//住房公积金缴存记录
                        busHelper.InsertList(jincityContainer.EndInsdptList, reportid, db);//养老保险缴存记录
                        busHelper.InsertList(jincityContainer.EndinsdlrList, reportid, db);//养老保险发放记录
                        busHelper.InsertList(jincityContainer.SalvationList, reportid, db);//低保救济记录
                        busHelper.InsertList(jincityContainer.RecorddtlinfoList, reportid, db);//查询记录
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Log4netAdapter.WriteError(string.Format("姓名：{0}，身份证：{1}入库出现异常", jincityContainer.Report.Name, jincityContainer.Report.Cert_No), ex);
                    }
                }

            }
        }


    }
}
