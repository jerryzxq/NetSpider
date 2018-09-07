using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ServiceStack.OrmLite;
using Vcredit.ExtTrade.ModelLayer;
using Vcredit.ExtTrade.CommonLayer;
using System.IO;
namespace Vcredit.ExtTrade.BusinessLayer.CommonBusiness
{
    public  class Businesshelper
    {
        
       
        internal void InsertList<T>(List<T> list, long reportid, IDbConnection db) where T : BaseEntity
        {
            list.ForEach(x => x.Report_Id = reportid);
            db.InsertAll(list);
          
        }
        internal void InsertEntity<T>(T entity, long reportid, IDbConnection db) where T : BaseEntity
        {
            entity.Report_Id = reportid;
            if (db.Insert(entity) <= 0)
            {
                throw new Exception(typeof(T).Name + "InsertEntity入库失败");
            }
        }
        /// <summary>
        ///贷款信息
        /// </summary>
        /// <param name="stnList"></param>
        /// <param name="reportid"></param>
        /// <param name="db"></param>
        /// <param name="nameAndCertno">姓名和身份证：用于记日志</param>
        internal void InsertLnList(List<CRD_CD_LNEntity> lnList,long reportid ,IDbConnection db)
        {
            long lnid = 0;
            foreach (var item in lnList)
            {
                item.Report_Id = reportid;
                lnid = db.Insert(item, selectIdentity: true);

                if (lnid != 0)
                {
                    if (item.LnSPLList.Count != 0)
                    {
                        item.LnSPLList.ForEach(x => { x.Report_Id = item.Report_Id; x.Card_Id = lnid; });
                        db.InsertAll(item.LnSPLList);
                    }
                    if (item.LnoverList.Count != 0)
                    {
                        item.LnoverList.ForEach(x => { x.Report_Id = item.Report_Id; x.Card_Id = lnid; });
                        db.InsertAll(item.LnoverList);
                    }
     
                }
                else
                {
                    throw new Exception("InsertLnList入库失败");
                }
            }
        }
        /// <summary>
        /// 贷记卡信息入库
        /// </summary>
        /// <param name="stnList"></param>
        /// <param name="reportid"></param>
        /// <param name="db"></param>
        /// <param name="nameAndCertno">姓名和身份证：用于记日志</param>
        internal void InsertLndList(List<CRD_CD_LNDEntity> lndList,long reportid, IDbConnection db)
        {
            long lndid = 0;
            foreach (var item in lndList)
            {
                item.Report_Id = reportid;
                lndid = db.Insert(item, selectIdentity: true);

                if (lndid != 0)
                {
                    if (item.LndSPLList.Count != 0)
                    {
                        item.LndSPLList.ForEach(x => { x.Report_Id = item.Report_Id; x.Card_Id = lndid; });
                        db.InsertAll(item.LndSPLList);
                    }

                    if (item.LndoverList.Count != 0)
                    {
                        item.LndoverList.ForEach(x => { x.Report_Id = item.Report_Id; x.Card_Id = lndid; });
                        db.InsertAll(item.LndoverList);
                    }
                }
                else
                {
                    throw new Exception("InsertLnList入库失败");
                }
            }
        }
        /// <summary>
        /// 准贷记卡信息入库
        /// </summary>
        /// <param name="stnList"></param>
        /// <param name="reportid"></param>
        /// <param name="db"></param>
        /// <param name="nameAndCertno">姓名和身份证：用于记日志</param>
        internal void InsertstnList(List<CRD_CD_STNCARDEntity> stnList,long reportid, IDbConnection db)
        {
            long stnid = 0;
            foreach (var item in stnList)
            {
                item.Report_Id = reportid;
                stnid = db.Insert(item, selectIdentity: true);

                if (stnid != 0)
                {
                    if (item.StnSPLList.Count != 0)
                    {
                        item.StnSPLList.ForEach(x => { x.Report_Id = item.Report_Id; x.Card_Id = stnid; });
                        db.InsertAll(item.StnSPLList);
                    }

                    if (item.StnoverList.Count != 0)
                    {
                        item.StnoverList.ForEach(x => { x.Report_Id = item.Report_Id; x.Card_Id = stnid; });
                        db.InsertAll(item.StnoverList);
                    }
                }
                else
                {
                    throw new Exception("InsertLnList入库失败");
                }
            }
        }

    }
}
