using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.ModelLayer;
using Vcredit.ExtTrade.CommonLayer;
using System.IO;
using ServiceStack.OrmLite;
using System.Reflection;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.BusinessLayer.CommonBusiness;
using Vcredit.Common.Utility;
using Vcredit.ExtTrade.ModelLayer.Common;

namespace Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade
{
    public class ExcelFile : AbastractFileBase
    {
        OperatorLog opeLog = new OperatorLog(FileType.ExcelFile);
        List<string> allFileName = new List<string>();
        CRD_CD_CreditUserInfoBusiness creditbus = new CRD_CD_CreditUserInfoBusiness();
       Dictionary<string,CRD_CD_CreditUserInfoEntity> NoCreditDataList =new Dictionary<string,CRD_CD_CreditUserInfoEntity>();//存储没有征信报告数据的身份证号。
        public override bool SaveData()
        {
            Dictionary<string, DataTable> dic = ReadFile();
            Dictionary<string, Dictionary<string, DataTable>> dicList = new Dictionary<string, Dictionary<string, DataTable>>();
            DealingExcelData ded = new DealingExcelData(opeLog);
            foreach (var item in dic)
            {
                try
                {
                    ded.FileName = item.Key;
                    dicList.Add(item.Key,ded.SpliteTable(item.Value));
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError("Excel:" + item.Key + "出现异常", ex);
                    opeLog.AddFailNum(1).AddfailReson(new FailReason(item.Key, "", "", "解析出现异常"));
                }
            };

            bool result = true;
            if (dicList.Count != 0)
            {
                Log4netAdapter.WriteInfo("Excel开始入库操作");
                new BridgingBusiness(opeLog).SaveDataToDB(dicList);
                Log4netAdapter.WriteInfo("入库结束");
            }
            else
            {
                result = false;
            }
            var logEntity = opeLog.GetoperatorLogEntity();
            new CRD_CD_OperatorLogBusiness().InsertEntity(logEntity);//插入操作日志
            UpdateCreditStateInfo(logEntity);//跟新的征信信息状态。
            return result;
        }
        Dictionary<string, DataTable> ReadFile()
        {
            Dictionary<string, DataTable> dicList = new Dictionary<string, DataTable>();
            string[] suffixs = { ".xls", ".xlsx" };
            List<FileInfo> fileInfos = GetLocalFileInfo(suffixs);
            FilterHaveCommitFile(fileInfos);
            opeLog.SetOperateTotalNum(fileInfos.Count);
            DataTable dt=null;
            //从本地读取文件
            foreach (var item in fileInfos)
            {
                allFileName.Add(item.FullName);
                try
                {
                    if (!dicList.ContainsKey(item.FullName))
                    {
                        dt= NPOIHelper.GetDataTable(item.FullName, false);
                        if (dt.Rows.Count == 0 || dt.Rows[0][0].ToString().TrimStart().StartsWith("个人征信系统中没有"))
                        {
                            AddNoCreditData(item.FullName,item.Name);
                            Log4netAdapter.WriteInfo(item.FullName + "文件没有数据");
                            continue;
                        }
                        dicList.Add(item.FullName, dt);
                    }
                    else
                    {
                        Log4netAdapter.WriteInfo(item.FullName + "文件信息重复");
                    }
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError("读取" + item.FullName + "文件excel出现异常", ex);
                    opeLog.AddFailNum(1).AddfailReson(new FailReason(item.FullName, "", "", ex.Message));
                }

            }
            return dicList;
        }
        private void AddNoCreditData(string fullName,string filename)
        {
            CRD_CD_CreditUserInfoEntity credit = new CRD_CD_CreditUserInfoEntity(){ 
            Cert_No=CommonFun.GetMidStr(filename,"_","."),
            State=(byte)RequestState.HaveNoData,
            };
            NoCreditDataList.Add(fullName,credit);
        }
        protected void FilterHaveCommitFile(List<FileInfo> fileInfos)
        {
            if (fileInfos.Count == 0)
                return;
            FileInfo[] fileInfoarray = new FileInfo[fileInfos.Count];
            fileInfos.CopyTo(fileInfoarray);
            var fileGroups = fileInfoarray.GroupBy(x => x.DirectoryName);
            foreach (IGrouping<string, FileInfo> item in fileGroups)
            {
                if (Directory.Exists(item.Key.Replace("Return", "Returned")))
                {
                    foreach (FileInfo x in item)
                    {
                        string returnedfile = x.FullName.Replace("Return", "Returned");
                        if (File.Exists(returnedfile))
                        {
                            Log4netAdapter.WriteInfo("文件" + x.FullName + "已经解析过");
                            fileInfos.Remove(x);
                        }
                    }
                }
            }
        }
     
        #region 跟新的征信信息状态
        void UpdateCreditStateInfo(CRD_CD_OperatorLogEntity entity)
        {
            List<CRD_CD_CreditUserInfoEntity> list = new List<CRD_CD_CreditUserInfoEntity>();
            if (entity.FailNum != 0)
            {
                var failResonList = GetDistinctFilename();
                if (failResonList != null || failResonList.Count() != 0)
                {
                    foreach (var item in failResonList)
                    {
                        allFileName.Remove(item.FileName);//删除失败的文件信息
                        list.Add(new CRD_CD_CreditUserInfoEntity()
                        {
                            Error_Reason = item.Reason.Length >= 200 ? item.Reason.Substring(0, 199) : item.Reason,
                            State = (byte)RequestState.AnalysisFail,
                            Cert_No = GetCert_No(item.FileName)
                        });
                    }
                }
            }
            creditbus.UpdateListStateByCert_No(list);
            creditbus.UpdateEmptyState(NoCreditDataList);
        }
      
        string GetCert_No(string fileName)
        {
            int index = fileName.IndexOf('_');
            fileName = fileName.Substring(index + 1, fileName.IndexOf('.') - index - 1);
            return fileName;

        }
        IEnumerable<FailReason> GetDistinctFilename()
        {
            var failResonList = opeLog.failReasonList.Distinct(new FailReason()).Where(item => !string.IsNullOrEmpty(item.FileName));
            return failResonList;
        }
   
        #endregion

    }
}
