using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.OrmLite;
using Vcredit.ExtTrade.CommonLayer;
using System.Data;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExtTrade.BusinessLayer.CommonBusiness;
using Vcredit.ExtTrade.ModelLayer.Common;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
using System.Configuration;
using Vcredit.Common.Ext;
using Vcredit.ExternalCredit.CommonLayer.helper;

namespace Vcredit.ExtTrade.BusinessLayer
{
    class CreditEquelityCompare : IEqualityComparer<CRD_CD_CreditUserInfoEntity>
    {

        public bool Equals(CRD_CD_CreditUserInfoEntity x, CRD_CD_CreditUserInfoEntity y)
        {
            if (x.Cert_No == y.Cert_No && x.LocalDirectoryName != y.LocalDirectoryName)
            {
                return true;
            }
            return false;
        }

        public int GetHashCode(CRD_CD_CreditUserInfoEntity obj)
        {
            return obj.ToString().GetHashCode();
        }
    }
    /// <summary>
    /// 查询用户征信信息表
    /// </summary>
    public class CRD_CD_CreditUserInfoBusiness
    {

        BaseDao dao = new BaseDao();
        public void SaveCredit(decimal creditUserInfo_Id, byte state, string error)
        {
            if (state != (byte)RequestState.UpLoading && state != (byte)RequestState.UpLoadFail)
            {
                UpdateStateInfo(state, creditUserInfo_Id, error, DateTime.Now);
            }
            else
            {
                UpdateStateInfo(state, creditUserInfo_Id, error);
            }
        }
        public void UpdatePactNOAndState(decimal creditUserInfo_Id, byte state, string pactNo)
        {
            using (var db = dao.Open())
            {
                db.Update<CRD_CD_CreditUserInfoEntity>(new { State = state, PactNo = pactNo, ReportTime = DateTime.Now, UpdateTime = DateTime.Now },
                     x => x.CreditUserInfo_Id == creditUserInfo_Id);

            }

        }
        public CRD_PI_IDENTITYEntity QueryIdentityInfo(int reportid)
        {
            return dao.Select<CRD_PI_IDENTITYEntity>(x => x.Report_Id == reportid).FirstOrDefault();
        }
        public CRD_PI_IDENTITYEntity QueryIdentityInfo(string reportsn)
        {
            using (var db = dao.Open())
            {
                var report = db.Select<CRD_HD_REPORTEntity>(x => x.Report_Sn == reportsn).FirstOrDefault();
                if (report == null)
                    return null;
                else
                {
                    return QueryIdentityInfo((int)report.Report_Id);
                }
            }
        }
        public bool UpdateStateInfo(int state, decimal creditUserInfO_Id, string errorReason)
        {
            bool result = false;
            try
            {
                using (var db = dao.Open())
                {
                    if (db.Update<CRD_CD_CreditUserInfoEntity>(new { State = state, Error_Reason = errorReason, UpdateTime = DateTime.Now },
                         x => x.CreditUserInfo_Id == creditUserInfO_Id) > 0)
                        result = true;
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("CreditUserInfo_Id:" + creditUserInfO_Id.ToString() + "更新失败", ex);
            }

            return result;
        }
        public bool UpdateStateInfo(int state, decimal creditUserInfO_Id, string errorReason, DateTime reportTime)
        {
            bool result = false;
            try
            {
                using (var db = dao.Open())
                {
                    if (db.Update<CRD_CD_CreditUserInfoEntity>(new { State = state, Error_Reason = errorReason, ReportTime = reportTime, UpdateTime = DateTime.Now },
                         x => x.CreditUserInfo_Id == creditUserInfO_Id) > 0)
                        result = true;
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("CreditUserInfo_Id:" + creditUserInfO_Id.ToString() + "更新失败", ex);
            }

            return result;
        }
        public bool UpdateStateInfo(int state, decimal creditUserInfO_Id, string errorReason, string report_sn)
        {
            bool result = false;
            try
            {
                using (var db = dao.Open())
                {
                    if (db.Update<CRD_CD_CreditUserInfoEntity>(new { State = state, Error_Reason = errorReason, Report_sn = report_sn, UpdateTime = DateTime.Now, ReportTime = DateTime.Now },
                         x => x.CreditUserInfo_Id == creditUserInfO_Id) > 0)
                        result = true;
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("CreditUserInfo_Id:" + creditUserInfO_Id.ToString() + "更新失败", ex);
            }

            return result;
        }
        public bool UpdateFileState(CRD_CD_CreditUserInfoEntity updateCredit)
        {
            bool result = true;
            try
            {
                using (var db = dao.Open())
                {
                    var infectNum = db.ExecuteNonQuery(string.Format("Update credit.CRD_CD_CreditUserInfo set FileState={0} where CreditUserInfo_Id={1}", updateCredit.FileState, updateCredit.CreditUserInfo_Id));
                    if (infectNum <= 0)
                        result = false;
                }
            }
            catch (Exception ex)
            {
                result = false;
                Log4netAdapter.WriteError(updateCredit.Cert_No + "UpdateFileState更新出现异常", ex);
            }
            return result;
        }
        public bool UpdateOnlyApplyId(CRD_CD_CreditUserInfoEntity credit)
        {
            using (var db = dao.Open())
            {
                if (db.Update<CRD_CD_CreditUserInfoEntity>(new { ApplyID = credit.ApplyID },
                     x => x.CreditUserInfo_Id == credit.CreditUserInfo_Id) > 0)
                    return true;
            }
            return false;
        }
        public bool UpdateFileState(int fileState, int creditUserInfO_Id)
        {
            bool result = false;
            try
            {
                using (var db = dao.Open())
                {
                    if (db.Update<CRD_CD_CreditUserInfoEntity>(new { FileState = fileState },
                         x => x.CreditUserInfo_Id == creditUserInfO_Id) > 0)
                        result = true;
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("CreditUserInfo_Id:" + creditUserInfO_Id.ToString() + "更新FileState失败", ex);
            }
            return result;
        }
        public CRD_CD_CreditUserInfoEntity Select(string cer_no, string name, string busType, string date)
        {
            return dao.Select<CRD_CD_CreditUserInfoEntity>
             (x => x.Cert_No == cer_no && x.Name == name && x.BusType == busType && x.Authorization_Date == date).OrderByDescending(x => x.CreditUserInfo_Id).FirstOrDefault();
        }
        public List<CRD_CD_CreditUserInfoEntity> GetNotUploadFileInfo(int sourcetype = 10)
        {
            return dao.Select<CRD_CD_CreditUserInfoEntity>
             (x => x.SourceType == sourcetype && (x.FileState == 0 || x.FileState == 1) && x.ApplyID != null &&
             x.ApplyID != 0);

        }

        public object GetByRequest(QueryRequestEntity request)
        {
            int totalCount = 0;
            List<CRD_CD_CreditUserInfoEntity> list = new List<CRD_CD_CreditUserInfoEntity>();
            string wheresql = " 1=1";
            if (!string.IsNullOrEmpty(request.Name))
            {
                wheresql += " and Name='" + request.Name + "'";
            }
            if (!string.IsNullOrEmpty(request.SourceType))
            {
                wheresql += " and SourceType =" + request.SourceType;
            }
            if (!string.IsNullOrEmpty(request.State))
            {
                if (request.State.Contains(","))
                    wheresql += " and state in (" + request.State + ")";
                else
                    wheresql += " and state =" + request.State;
            }
            if (!string.IsNullOrEmpty(request.StartDate) && !string.IsNullOrEmpty(request.EndDate))
            {
                wheresql += "  and  ReportTime between '" + request.StartDate + "' and '" + request.EndDate + "' ";
            }
            if (!string.IsNullOrEmpty(request.Cert_No))
            {
                wheresql += " and Cert_No='" + request.Cert_No + "'";
            }
            if (!string.IsNullOrEmpty(request.BusType))
            {
                wheresql += " and BusType='" + request.BusType + "'";
            }
            string querysql = "select  * from (select row_number() over(ORDER BY CreditUserInfo_Id DESC) as rownumber,* from [credit].[CRD_CD_CreditUserInfo] where " + wheresql + " ) A where rownumber between  " + (request.PageSize * (request.PageIndex - 1) + 1).ToString() + "  and  " + (request.PageSize * (request.PageIndex - 1) + request.PageSize).ToString() + "  ";
            string countsql = " select count(*) from  [credit].[CRD_CD_CreditUserInfo] where " + wheresql;
            using (var db = dao.Open())
            {
                var result = db.Select<int>(countsql).FirstOrDefault();
                if (result != 0)
                {
                    totalCount = result;
                    list = db.Select<CRD_CD_CreditUserInfoEntity>(querysql);
                }
                else
                {
                    totalCount = 0;
                }
            }
            return new { Count = totalCount, CreditUserInfoList = list };

        }

        public List<CRD_CD_CreditUserInfoEntity> GetByCertNo(string certNo, string name, string certType)
        {
            return dao.Select<CRD_CD_CreditUserInfoEntity>(x => x.Cert_No == certNo && x.Name == name && x.Cert_Type == certType);
        }

        public bool HaveNewRequest(string certno, int limiteday, ref CRD_CD_CreditUserInfoEntity entity)
        {
            entity = GetEntityByCertNo(certno, SysEnums.SourceType.Trade);

            if (entity == null || entity.Time_Stamp.Value.AddDays(limiteday).CompareTo(DateTime.Now) < 0)
                return false;
            return true;
        }
        public CRD_CD_CreditUserInfoEntity GetEntityByCertNo(string certNo, SysEnums.SourceType sourceType)
        {
            var list = dao.Select<CRD_CD_CreditUserInfoEntity>(x => x.Cert_No == certNo && x.SourceType == Convert.ToInt16(sourceType)).OrderByDescending(x => x.CreditUserInfo_Id);
            return list.FirstOrDefault();

        }
        public int GetStateBycert_no(string cert_no, int limiteday)
        {
            var creditList = dao.Select<CRD_CD_CreditUserInfoEntity>(x => x.Cert_No == cert_no).OrderByDescending(x => x.CreditUserInfo_Id);
            int state = -1;
            if (creditList != null && creditList.Count() != 0)
            {
                if (creditList.First().Time_Stamp.Value.AddDays(limiteday).CompareTo(DateTime.Now) > 0)
                {
                    if (creditList.Count() == 1)
                    {
                        state = creditList.First().State.Value;
                    }
                    else
                    {
                        state = creditList.OrderByDescending(x => x.CreditUserInfo_Id).First().State.Value;
                    }
                }

            }
            return state;
        }
        public CRD_CD_CreditUserInfoEntity GetStateInfoBycert_no(string cert_no, int limiteday, string name)
        {
            //var creditList = dao.Select<CRD_CD_CreditUserInfoEntity>(x => x.Cert_No == cert_no).OrderByDescending(x => x.CreditUserInfo_Id);
            string sqlstr = "select top 1 * from credit.CRD_CD_CreditUserInfo with(nolock) where 1=1 and cert_no='" + cert_no + "'";
            if (!string.IsNullOrEmpty(name))
            {
                sqlstr += " and name='" + name + "'";
            }
            sqlstr += " order by CreditUserInfo_Id desc";
            var creditList = dao.Select<CRD_CD_CreditUserInfoEntity>(sqlstr);
            if (creditList != null && creditList.Count() > 0)
            {
                if (creditList.First().Time_Stamp.Value.AddDays(limiteday).CompareTo(DateTime.Now) > 0)
                {
                    if (creditList.Count() == 1)
                    {
                        return creditList.First();
                    }
                    else
                    {
                        return creditList.OrderByDescending(x => x.CreditUserInfo_Id).First();
                    }
                }

            }
            return null;
        }

        public void UpdateState(IDbConnection db, string certNo, string report_sn)
        {
            if (db.ExecuteNonQuery("Update credit.CRD_CD_CreditUserInfo set  state=5 ,Error_Reason=null,Report_sn='" + report_sn + "'  where CreditUserInfo_Id=(Select  max(CreditUserInfo_Id)  from credit.CRD_CD_CreditUserInfo where Cert_No ='" + certNo + "' And SourceType=" + ((int)SysEnums.SourceType.Trade).ToString() + ")") <= 0)
            {
                Log4netAdapter.WriteInfo(string.Format("reportsn:{0},certno:{1}再次更新失败", report_sn, certNo));
            }
        }
        /// <summary>
        /// 担保征信
        /// </summary>
        /// <param name="certNo"></param>
        /// <param name="report_sn"></param>
        public void UpdateState(string certNo, string report_sn)
        {
            using (var db = dao.Open())
            {
                db.ExecuteNonQuery("Update credit.CRD_CD_CreditUserInfo set  state=5 ,Error_Reason=null,Report_sn='" + report_sn + "'  where CreditUserInfo_Id=(Select  max(CreditUserInfo_Id)  from credit.CRD_CD_CreditUserInfo where Cert_No ='" + certNo + "' And SourceType=" + ((int)SysEnums.SourceType.Assure).ToString() + ")");

            }
        }

        /// <summary>
        /// 获取配置文件和默认的上传的数据
        /// </summary>
        /// <param name="directoryName"></param>
        /// <returns></returns>
        public List<CRD_CD_CreditUserInfoEntity> GetAllList(SysEnums.SourceType sourceType = SysEnums.SourceType.Trade)
        {
            string sql = @"
      select top 500 [CreditUserInfo_Id]
      ,[ExpiryDate_Num]
      ,[Authorization_Date]
      ,[Name]
      ,[Cert_Type]
      ,[Cert_No]
      ,[Query_Org]
      ,[User_Code]
      ,[State]
      ,[Time_Stamp]
      ,[UpLoadDirectoryName]
      ,[NameFirstLetter]
      ,[Error_Reason]
      ,[LocalDirectoryName]
      ,[BusType]
      ,[QueryReason]
      ,[SourceType]
      ,[Report_sn]
      ,[BatNo]
      ,[PactNo]
      ,[czAuth]
      ,[czId]
      ,[FileState]
      ,[UpdateTime]
      ,[ReportTime]
      ,[Ukey]
      ,[ApplyID],grantType
            into #temp FROM [credit].[CRD_CD_CreditUserInfo] WITH(NOLOCK) where  Time_Stamp between 
       DATEADD(day,-30,getdate()) and getdate()  And [SourceType] ={0} {1}
       select *from #temp 
       inner join 
           (
              select Cert_No,Name from #temp group by Cert_No,Name having count(*)=1
           )  tb1 on  tb1.Cert_No=#temp.Cert_no and tb1.Name=#temp.Name            
     drop table  #temp";
            string whereStr = string.Empty;
            List<CRD_CD_CreditUserInfoEntity> list = new List<CRD_CD_CreditUserInfoEntity>();
            if (!string.IsNullOrEmpty(ConfigData.requestState) || !string.IsNullOrEmpty(ConfigData.directoryNames))
            {
                whereStr += CreateRequestStateAndDirSql(ConfigData.requestState, ConfigData.directoryNames);
            }
            else if (!string.IsNullOrEmpty(ConfigData.cert_Nos))
            {
                whereStr += CreateCertNoSql();
            }
            else if (!string.IsNullOrEmpty(ConfigData.Ids))
            {
                whereStr += CertIDsSql();
            }
            else
            {
                whereStr += " And State in(0,1)";
            }

            return dao.Select<CRD_CD_CreditUserInfoEntity>(string.Format(sql, ((int)sourceType).ToString(), whereStr));
        }
        private string CertIDsSql()
        {
            string sql = string.Empty;

            if (ConfigData.Ids.Contains(","))
            {
                sql += " And CreditUserInfo_Id  in (" + ConfigData.Ids + ")";
            }
            else
            {
                sql += "And CreditUserInfo_Id =" + ConfigData.Ids;
            }
            return sql;
        }
        private string CreateRequestStateAndDirSql(string requestState, string directoryNames)
        {
            string sql = string.Empty;

            if (!string.IsNullOrEmpty(requestState))
            {
                if (requestState.Contains(","))
                {
                    sql += " And State in(" + requestState + ")";
                }
                else
                {
                    sql += " And State=" + requestState;
                }
            }
            if (!string.IsNullOrEmpty(directoryNames))
            {
                if (directoryNames.Contains(","))
                {
                    sql += " And Authorization_Date  in ('" + directoryNames.Replace(",", "','") + "')";
                }
                else
                {
                    sql += " And Authorization_Date ='" + directoryNames + "'";
                }
            }
            return sql;
        }
        private string CreateCertNoSql()
        {
            string sql = string.Empty;

            if (ConfigData.cert_Nos.Contains(","))
            {
                sql += " And Cert_No  in ('" + ConfigData.cert_Nos.Replace(",", "','") + "')";
            }
            else
            {
                sql += "And Cert_No ='" + ConfigData.cert_Nos + "'";
            }
            return sql;
        }
        public void UpdateState(List<CRD_CD_CreditUserInfoEntity> list, byte state)
        {
            string sql = "Update credit.CRD_CD_CreditUserInfo set BatNo='{0}',state={1} ,UpdateTime='{2}',Error_Reason='{3}'  where CreditUserInfo_Id={4}";
            if (state == (byte)RequestState.UpLoadSuccess)
            {
                sql = "Update credit.CRD_CD_CreditUserInfo set BatNo='{0}',state={1} ,UpdateTime='{2}',Error_Reason='{3}',ReportTime='" + DateTime.Now.ToString() + "'  where CreditUserInfo_Id={4}";
            }
            try
            {
                using (var db = dao.Open())
                {
                    foreach (var item in list)
                    {
                        if (db.ExecuteNonQuery(string.Format(sql, item.BatNo ?? string.Empty, state, DateTime.Now.ToString(), item.Error_Reason ?? string.Empty, item.CreditUserInfo_Id.ToString())) <= 0)
                        {
                            Log4netAdapter.WriteInfo(item.Cert_No + "更新状态" + state.ToString() + "失败");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError(string.Join(",", list.Select(x => x.Cert_No)) + "UpdateState更新出现异常", ex);
            }
        }
        public List<CRD_CD_CreditUserInfoEntity> GetList()
        {
            return dao.Select<CRD_CD_CreditUserInfoEntity>("SELECT  * FROM [credit].[CRD_CD_CreditUserInfo] WITH(NOLOCK) where  State in(0,1) And SourceType=" + ((int)SysEnums.SourceType.Trade).ToString());
        }

        public List<CRD_CD_CreditUserInfoEntity> GetAllListByConfig(string requestState, string directoryNames)
        {

            if (string.IsNullOrEmpty(requestState) && string.IsNullOrEmpty(directoryNames))
                return new List<CRD_CD_CreditUserInfoEntity>(); ;
            string sql = "where SourceType=" + ((int)SysEnums.SourceType.Trade).ToString();

            if (!string.IsNullOrEmpty(requestState))
            {
                if (requestState.Contains(","))
                {
                    sql += " And State in(" + requestState + ")";
                }
                else
                {
                    sql += " And State=" + requestState;
                }
            }
            if (!string.IsNullOrEmpty(directoryNames))
            {
                if (directoryNames.Contains(","))
                {
                    sql += " And Authorization_Date  in ('" + directoryNames.Replace(",", "','") + "')";
                }
                else
                {
                    sql += " And Authorization_Date ='" + directoryNames + "'";
                }
            }
            return dao.Select<CRD_CD_CreditUserInfoEntity>(" select * from credit.CRD_CD_CreditUserInfo WITH(NOLOCK) " + sql);
        }
        public List<CRD_CD_CreditUserInfoEntity> GetAllListByCert_no(string cert_Nos)
        {
            if (string.IsNullOrEmpty(cert_Nos))
                return new List<CRD_CD_CreditUserInfoEntity>();
            string sql = "where SourceType=" + ((int)SysEnums.SourceType.Trade).ToString() + " and";

            if (cert_Nos.Contains(","))
            {
                sql += " Cert_No  in ('" + cert_Nos.Replace(",", "','") + "')";
            }
            else
            {
                sql += " Cert_No ='" + cert_Nos + "'";
            }

            return dao.Select<CRD_CD_CreditUserInfoEntity>(" select * from credit.CRD_CD_CreditUserInfo WITH(NOLOCK) " + sql);
        }
        public bool Exist(string cert_no)
        {
            var list = dao.Select<CRD_CD_CreditUserInfoEntity>(item => item.Cert_No == cert_no && item.SourceType == (int)SysEnums.SourceType.Trade);
            if (list == null || list.Count == 0)
                return false;
            return true;
        }
        public void Save(CRD_CD_CreditUserInfoEntity entity)
        {
            if (entity.Time_Stamp == null)
            {
                entity.Time_Stamp = DateTime.Now;
            }
            dao.Save(entity);
        }

        public bool Insert(CRD_CD_CreditUserInfoEntity entity)
        {
            return dao.Insert(entity);
        }

        public void InsertList(List<CRD_CD_CreditUserInfoEntity> entityList)
        {
            if (entityList[0].Time_Stamp == null || entityList[0].State == null)
            {
                entityList.ForEach(item => { item.Time_Stamp = DateTime.Now; item.State = 0; });
            }
            if (entityList.Count == 1)
            {
                dao.Insert(entityList[0]);
            }
            else
            {
                dao.InsertAll(entityList);
            }

        }
        public bool UpdateListStateByID(List<CRD_CD_CreditUserInfoEntity> list)
        {
            bool result = true;
            try
            {
                using (var db = dao.Open())
                {
                    db.UpdateAll(list);
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError(string.Join(",", list.Select(x => x.Cert_No)) + "UpdateListStateByID更新失败", ex);
                result = false;
            }
            return result;
        }
        public bool Update(CRD_CD_CreditUserInfoEntity entity)
        {
            bool isuccess = true;
            try
            {
                isuccess = dao.Update(entity);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError(entity.Cert_No + "UpdateListStateByID更新失败", ex);
                isuccess = false;
            }
            return isuccess;

        }
        public bool UpdateListStateByCert_No(List<CRD_CD_CreditUserInfoEntity> list)
        {

            bool result = true;
            if (list.Count == 0)
                return result;
            using (var db = dao.Open())
            {
                foreach (var item in list)
                {
                    try
                    {
                        if (db.ExecuteNonQuery("Update credit.CRD_CD_CreditUserInfo set  state=" + item.State + ", Error_Reason='" + item.Error_Reason + "' where CreditUserInfo_Id=(Select  max(CreditUserInfo_Id)  from credit.CRD_CD_CreditUserInfo where Cert_No ='" + item.Cert_No + "' And SourceType=" + ((int)SysEnums.SourceType.Trade).ToString() + ")") == 0)
                        {
                            result = false;
                            Log4netAdapter.WriteInfo("Cert_NO是1" + item.Cert_No + "更新失败");
                        }
                    }
                    catch (Exception ex)
                    {

                        Log4netAdapter.WriteError("Cert_NO是" + item.Cert_No + "更新失败", ex);
                        result = false;
                    }
                }
            }
            return result;
        }
        #region 新外贸
        public bool UpdateListStateByBatchNoCertNo(List<CRD_CD_CreditUserInfoEntity> list, string batchNo)
        {

            bool result = true;
            if (list.Count == 0)
                return result;
            using (var db = dao.Open())
            {
                foreach (var item in list)
                {
                    try
                    {
                        if (db.ExecuteNonQuery("Update credit.CRD_CD_CreditUserInfo set  UpdateTime='" + item.UpdateTime + "'  ,state=" + item.State + ", Error_Reason='" + item.Error_Reason + "'  where Cert_No ='" + item.Cert_No + "' And  BatNo='" + batchNo + "' And SourceType=" + ((int)SysEnums.SourceType.Trade).ToString()) == 0)
                        {
                            result = false;
                            Log4netAdapter.WriteInfo("新外贸Cert_NO是1" + item.Cert_No + "更新失败");
                        }
                    }
                    catch (Exception ex)
                    {

                        Log4netAdapter.WriteError("新外贸Cert_NO是" + item.Cert_No + "更新异常", ex);
                        result = false;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 根据批次号和机构号或者身份证修改状态
        /// </summary>
        /// <param name="db"></param>
        /// <param name="batNo"></param>
        /// <param name="state"></param>
        /// <param name="certNo"></param>
        public void UpdateStateByBatNo(string batNo, string query_Org, byte state, string report_sn, string errorReson, string certNo = null, IDbConnection db = null)
        {
            if (state == 7)
            {
                string a = null;
            }
            bool needClose = false;
            try
            {
                if (db == null)
                {
                    needClose = true;
                    db = dao.Open();
                }
                string sql = "Update credit.CRD_CD_CreditUserInfo set UpdateTime='" + DateTime.Now + "', state=" + state + " ,Report_sn='" + report_sn + "',Error_Reason='" + errorReson + "'  where  SourceType =10 and Query_Org='" + query_Org + "' ";
                int influenceRowNum = 0;
                if (!string.IsNullOrEmpty(certNo))
                {
                    sql += " and  cert_no='" + certNo + "'";
                }
                if (!string.IsNullOrEmpty(batNo))
                {
                    sql += " and BatNo='" + batNo + "'";
                }
                influenceRowNum = db.ExecuteNonQuery(sql);
                if (influenceRowNum <= 0)
                {
                    if (state == (byte)RequestState.SuccessCome) //如果成功获取的状态
                    {
                        UpdateState(db, certNo, report_sn); //再一次尝试改状态
                        string lastsql = db.GetLastSql();
                        Log4netAdapter.WriteInfo(string.Format("bathno:{0},query_org:{1},State:{2},certNO:{3}再次更新,lastsql:{4}", batNo, query_Org, state.ToString(), certNo, lastsql));

                    }

                    Log4netAdapter.WriteInfo(string.Format("bathno:{0},query_org:{1},State:{2},certNO:{3}更新失败", batNo, query_Org, state.ToString(), certNo));
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError(string.Format("bathno:{0},query_org:{1},State:{2},certNO:{3}更新出现异常", batNo, query_Org, state.ToString(), certNo), ex);
            }
            finally
            {
                if (needClose)
                    db.Close();
            }
        }
        /// <summary>
        /// 获取已经上传请求查询数据的批次号（考虑添加时间限制）
        /// </summary>
        /// <returns></returns>
        public List<string> GetBatchNos()
        {
            List<string> batchList = new List<string>();
            var list = GetSubmitCreditInfo(SysEnums.SourceType.Trade);
            if (list.Count != 0)
            {
                batchList = list.Select(x => x.BatNo).Distinct().ToList();
            }
            return batchList;

        }
        public List<CRD_CD_CreditUserInfoEntity> GetSubmitCreditInfo(SysEnums.SourceType sourceType)
        {
            var list = dao.Select<CRD_CD_CreditUserInfoEntity>(x => x.State == (byte)RequestState.UpLoadSuccess && x.SourceType == Convert.ToInt16(sourceType));
            return list;
        }
        public List<CRD_CD_CreditUserInfoEntity> GetSubmitCreditInfoByCofing(SysEnums.SourceType sourceType, string batNo)
        {

            var list = dao.Select<CRD_CD_CreditUserInfoEntity>(x => x.State == (byte)RequestState.UpLoadSuccess
               && x.SourceType == Convert.ToInt16(sourceType) && x.BatNo == batNo);
            return list;
        }

        #endregion
        public void UpdateEmptyState(Dictionary<string, CRD_CD_CreditUserInfoEntity> list)
        {
            if (list.Count == 0)
                return;
            NolmalBusinesshelper helper = new NolmalBusinesshelper();
            using (var db = dao.Open())
            {
                foreach (var item in list)
                {
                    try
                    {
                        if (db.ExecuteNonQuery("Update credit.CRD_CD_CreditUserInfo set  state=" + item.Value.State + ", Error_Reason='" + item.Value.Error_Reason + "' where CreditUserInfo_Id=(Select  max(CreditUserInfo_Id)  from credit.CRD_CD_CreditUserInfo where Cert_No ='" + item.Value.Cert_No + "' And SourceType=" + ((int)SysEnums.SourceType.Trade).ToString() + ")") == 0)
                        {

                            Log4netAdapter.WriteInfo("空征信Cert_NO是1" + item.Value.Cert_No + "更新失败");
                        }
                        else
                        {
                            helper.MoveCommitExcel(item.Key);
                        }
                    }
                    catch (Exception ex)
                    {

                        Log4netAdapter.WriteError("空征信Cert_NO是" + item.Value.Cert_No + "更新失败", ex);

                    }
                }
            }
        }
        public bool isInLimitedDays(int limteddays, string cert_no, string name = null)
        {
            bool result = false;
            using (var db = dao.Open())
            {
                DateTime? timeTamp = null;
                if (string.IsNullOrEmpty(name))
                {
                    timeTamp = db.Scalar<DateTime?>(" select  Time_Stamp from credit.CRD_CD_CreditUserInfo WITH(NOLOCK) where Cert_No='" + cert_no + "' order by  CreditUserInfo_Id Desc");
                }
                else
                {
                    timeTamp = db.Scalar<DateTime?>(" select  Time_Stamp from credit.CRD_CD_CreditUserInfo WITH(NOLOCK) where Cert_No='" + cert_no + "' and Name='" + name + "' order by  CreditUserInfo_Id Desc");
                }
                if (timeTamp != null && timeTamp.Value.AddDays(limteddays).CompareTo(DateTime.Now) > 0)
                    return true;
            }
            return result;
        }

        /// <summary>
        /// 针对人行原因导致查询失败的担保客户，取消之前30天内禁止二次查询的限制
        /// </summary>
        public bool isInLimitedDays_Assure(int limteddays, string cert_no, string name)
        {
            var result = false;
            var credit = dao.Select<CRD_CD_CreditUserInfoEntity>(x => x.Cert_No == cert_no && x.Name == name).OrderByDescending(x => x.CreditUserInfo_Id).FirstOrDefault();

            if (credit == null)
            {
                return result;
            }
            if (credit.State != (int)RequestState.QueryFail)
            {
                if (credit.Time_Stamp != null && credit.Time_Stamp.Value.AddDays(limteddays).CompareTo(DateTime.Now) > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        public bool IsInMaxRequestNum()
        {
            using (var db = dao.Open())
            {
                int count = db.Scalar<int>(" select Count(*) from  credit.CRD_CD_CreditUserInfo WITH(NOLOCK) where Authorization_Date ='" + DateTime.Now.ToString("yyyyMMdd") + "' And SourceType=" + (int)Vcredit.ExternalCredit.CommonLayer.SysEnums.SourceType.Trade);
                if (count >= ConfigData.everyDayMaxDealingNum)
                    return false;
                else
                    return true;

            }
        }
        /// <summary>
        /// 根据证件号 判断是否能重新查询
        /// </summary>
        /// <param name="limteddays"></param>
        /// <param name="cert_no"></param>
        /// <returns></returns>
        public bool CanReTryQuery(int limteddays, string cert_no)
        {
            bool result = false;
            using (var db = dao.Open())
            {
                DateTime? timeTamp = db.Scalar<DateTime?>(" select  Time_Stamp from credit.CRD_CD_CreditUserInfo WITH(NOLOCK) where Cert_No='" + cert_no + "' order by  CreditUserInfo_Id Desc");
                if (timeTamp == null)
                    return true;

                if (timeTamp != null && timeTamp.Value.AddDays(limteddays).CompareTo(DateTime.Now) <= 0)
                    return true;

            }
            return result;
        }

        /// <summary>
        /// 获取最近的数据实体
        /// </summary>
        /// <param name="certNo"></param>
        /// <returns></returns>
        public CRD_CD_CreditUserInfoEntity GetLastEntity(string certNo,byte sourceType)
        {
            using (var db = dao.Open())
            {
                var entity = db.Select<CRD_CD_CreditUserInfoEntity>(x => x.Cert_No == certNo && x.SourceType == sourceType).OrderByDescending(x => x.Time_Stamp).FirstOrDefault();

                return entity;
            }
        }



        /// <summary>
        /// 根据配置文件获取待提交担保用户数据（系统如果配置多个job每个job根据主键取余提交）
        /// </summary>
        /// <param name="_PerReportCount"></param>
        /// <param name="jobCount"></param>
        /// <param name="jobIndex"></param>
        /// <returns></returns>
        public List<CRD_CD_CreditUserInfoEntity> GetUsersByJobConfig(int _PerReportCount, int? jobCount, int? jobIndex)
        {
            using (var db = dao.Open())
            {
                string sql = string.Format(@" SELECT TOP {0} 
                                                * 
                                            FROM credit.CRD_CD_CreditUserInfo
                                            WHERE 1=1
                                                AND SourceType = {1}
                                                AND (State = {2} OR State = {3})
                                                AND CreditUserInfo_Id % {4} = {5}
                                                ORDER BY CreditUserInfo_Id
                                            "
                                           , _PerReportCount
                                           , (int)SysEnums.SourceType.Assure
                                           , (int)RequestState.Default, (int)RequestState.UpLoadFail
                                           , jobCount, jobIndex);

                var userInfoList = db.Select<CRD_CD_CreditUserInfoEntity>(sql).ToList();

                return userInfoList;
            }
        }

        /// <summary>
        /// 查询当天上报返回连接失败的用户数据（系统如果配置多个job每个job根据主键取余提交）
        /// </summary>
        /// <param name="_PerReportCount"></param>
        /// <param name="jobCount"></param>
        /// <param name="jobIndex"></param>
        /// <returns></returns>
        public List<CRD_CD_CreditUserInfoEntity> GetConnectionFailedUsersByJobConfig(int _PerReportCount, int? jobCount, int? jobIndex)
        {
            using (var db = dao.Open())
            {
                string sql = string.Format(@" SELECT TOP {0} 
                                                * 
                                            FROM credit.CRD_CD_CreditUserInfo
                                            WHERE 1=1
                                                AND SourceType = {1}
                                                AND (State = {2})
                                                AND CreditUserInfo_Id % {3} = {4}
                                                AND DATEDIFF(DAY, ReportTime, GETDATE()) = 0 -- 当天上报
                                                ORDER BY CreditUserInfo_Id
                                            "
                                           , _PerReportCount
                                           , (int)SysEnums.SourceType.Assure
                                           , (int)RequestState.ConnectionFailed
                                           , jobCount, jobIndex);

                var userInfoList = db.Select<CRD_CD_CreditUserInfoEntity>(sql).ToList();

                return userInfoList;
            }
        }

        /// <summary>
        /// 当前用户在限定期内的待上报的担保数据数量
        /// </summary>
        /// <param name="_currentUser"></param>
        /// <param name="upLoadLimitedDays"></param>
        /// <returns></returns>
        public int GetNeedCommitUserCountByLimitDay(CRD_CD_CreditUserInfoEntity _currentUser, int upLoadLimitedDays)
        {
            using (var db = dao.Open())
            {
                string sql = string.Format(@" SELECT count(1) 
                                                FROM credit.CRD_CD_CreditUserInfo
                                            WHERE 1=1
                                                AND SourceType = {0}
                                                AND (State = {1} OR State = {2} OR State = {3} OR State = {4})
                                                AND Cert_No = '{5}'
                                                AND Name = '{6}'
                                                AND DATEDIFF(DAY, Time_Stamp, GETDATE()) <= {7}
                                            "
                    , (int)SysEnums.SourceType.Assure
                    , (int)RequestState.Default, (int)RequestState.UpLoadFail, (int)RequestState.UpLoading, (int)RequestState.ConnectionFailed
                    , _currentUser.Cert_No
                    , _currentUser.Name
                    , upLoadLimitedDays);

                var count = db.Scalar<int>(sql);

                return count;
            }
        }

        /// <summary>
        /// 实体更新
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool UpdateEntity(CRD_CD_CreditUserInfoEntity entity)
        {
            return dao.Update(entity);
        }

        /// <summary>
        /// 获取待下载解析的担保征信数据
        /// </summary>
        /// <returns></returns>
		public List<CRD_CD_CreditUserInfoEntity> GetNeedDownload(int perReportCount)
        {
            // 状态为上传成功 RequestState.UpLoadSuccess OR RequestState.AnalysisFail 的数据
            // 尝试下载这些数据的征信

            // 提交到解析时间区间
            var diffHours = DataConvertor.ToDouble(ConfigurationManager.AppSettings["AnalysisDiffHours"], -1);
            if (diffHours <= -1)
                throw new ArgumentException("AnalysisDiffHours 参数配置不正确");

            // 上报开始时间
            var reportTimeBegin = ConfigurationManager.AppSettings["ReportTimeBegin"].ToDateTime();
            if (reportTimeBegin == null)
                throw new ArgumentException("ReportTimeBegin 参数没有配置");

            var jobIndex = ConfigurationManager.AppSettings["jobIndex"].ToInt();
            var jobCount = ConfigurationManager.AppSettings["jobCount"].ToInt();

            if (jobIndex == null || jobCount == null)
                throw new ArgumentException("appsettings必须配置参数jobIndex、jobCount");

            if (jobIndex >= jobCount)
                throw new ArgumentException("当前jobIndex必须小于jobCount");

            var dt = DateTime.Now.AddHours(-diffHours);

            string sql = string.Format(@" SELECT TOP {0} * FROM credit.CRD_CD_CreditUserInfo
											WHERE 1=1
												AND SourceType = {1}
												AND (State = {2} OR State = {3})
												AND ReportTime <= '{4}'
												AND ReportTime >= '{5}'
												AND CreditUserInfo_Id % {6} = {7}
												ORDER BY CreditUserInfo_Id
										"
                                            , perReportCount
                                            , (int)SysEnums.SourceType.Assure
                                            , (int)RequestState.UpLoadSuccess, (int)RequestState.AnalysisFail
                                            , dt.ToString("yyyy-MM-dd HH:mm:ss"), reportTimeBegin.Value.ToString("yyyy-MM-dd HH:mm:ss")
                                            , jobCount, jobIndex);

            var entities = dao.Select<CRD_CD_CreditUserInfoEntity>(sql).ToList();

            return entities;
        }

        public long GetApplyCount(string strBusType, int iSourceType)
        {
            SqlExpression<CRD_CD_CreditUserInfoEntity> sqlexp = dao.SqlExpression<CRD_CD_CreditUserInfoEntity>();

            sqlexp.Where(x => x.BusType == strBusType &&
                         x.SourceType == iSourceType &&
                         x.Time_Stamp >= DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")) &&
                         x.Time_Stamp < DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")).AddDays(1));

            return dao.Count(sqlexp);
        }

		public long GetApplyCount(int iSourceType)
		{
			SqlExpression<CRD_CD_CreditUserInfoEntity> sqlexp = dao.SqlExpression<CRD_CD_CreditUserInfoEntity>();

			sqlexp.Where(x => x.SourceType == iSourceType &&
						 x.Time_Stamp >= DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")) &&
						 x.Time_Stamp < DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")).AddDays(1));

			return dao.Count(sqlexp);
		}

        /// <summary>
        /// 将连接失败的标记为查询失败
        /// </summary>
        public void ConnectionFailedMarkFail(int diffDay)
        {
            var queryDate = DateTime.Now.AddDays(diffDay).ToString("yyyy-MM-dd");

            var reportTimeBegin = DateTime.Parse(queryDate);
            var reportTimeEnd = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));

            var entities = dao.Select<CRD_CD_CreditUserInfoEntity>(x => x.SourceType == (int)SysEnums.SourceType.Assure &&
                                                                        (x.State == (int)RequestState.ConnectionFailed) &&
                                                                        (x.ReportTime >= reportTimeBegin && x.ReportTime < reportTimeEnd)
                                                                  )
                                                                  .ToList();

            var msg = string.Format("上报时间：{0}，状态为连接失败的总数为：{1}条，修改状态为“查询失败”", queryDate, entities.Count);
            Log4netAdapter.WriteInfo(msg);
            Console.WriteLine(msg);

            foreach (var item in entities)
            {
                item.State = (int)RequestState.QueryFail;
                item.Error_Reason += "--非当天返回修改状态为查询失败";
                item.UpdateTime = DateTime.Now;

                // todo
                this.Update(item);
            }
        }

    }
}
