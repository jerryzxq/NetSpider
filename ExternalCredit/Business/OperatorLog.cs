using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.ModelLayer;
using Newtonsoft.Json;
namespace Vcredit.ExtTrade.BusinessLayer
{
    public enum FileType
    {
        /// <summary>
        /// excel文件
        /// </summary>
        ExcelFile = 1,
        /// <summary>
        /// txt文件
        /// </summary>
        TxtFile = 2
    }

    public class OperatorLog
    {
        CRD_CD_OperatorLogEntity operatorLog = new CRD_CD_OperatorLogEntity();
        public List<FailReason> failReasonList = new List<FailReason>();
        static string batchNo = DateTime.Now.ToString("yyyyMMddhhmmss");

        public static string BatchNo
        {
            private get { return OperatorLog.batchNo; }
            set { OperatorLog.batchNo = value; }
        }
        public OperatorLog(FileType fileType)
        {
            operatorLog.FileType = (byte)fileType;
        }
        public void AddSuccessNum(int num)
        {
            lock (this)
            {
                operatorLog.SuccessNum += num;
            }

        }
        public OperatorLog AddFailNum(int num)
        {
            lock (this)
            {
                operatorLog.FailNum += num;
            }
            return this;
        }
        public void SetOperateTotalNum(int totalNum)
        {
            operatorLog.OperateTotalNum += totalNum;
        }
        public OperatorLog AddfailReson(FailReason failReason)
        {
            if (failReason != null)
                failReasonList.Add(failReason);
            return this;
        }

        public CRD_CD_OperatorLogEntity GetoperatorLogEntity()
        {
            operatorLog.FailReason = ConvertfailReasonListToJson();
            operatorLog.BatchNo = BatchNo;
            return operatorLog;
        }
        string ConvertfailReasonListToJson()
        {
            if (failReasonList.Count == 0)
                return string.Empty;
            return JsonConvert.SerializeObject(failReasonList);
        }
    }

    public class FailReason : IEqualityComparer<FailReason>
    {
        private string fileName;

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
        private string reportTableField;

        public string ReportTableField
        {
            get { return reportTableField; }
            set { reportTableField = value; }
        }
        private string rowInfo;

        public string RowInfo
        {
            get { return rowInfo; }
            set { rowInfo = value; }
        }
        private string fieldInfo;

        public string FieldInfo
        {
            get { return fieldInfo; }
            set { fieldInfo = value; }
        }
        private string reason;

        public string Reason
        {
            get { return reason; }
            set { reason = value; }
        }
        public FailReason() { }
        public FailReason(string fileName, string reportTableField, string rowInfo, string fieldInfo, string reason)
        {
            this.FileName = fileName;
            this.ReportTableField = reportTableField;
            this.RowInfo = rowInfo;
            this.FieldInfo = fieldInfo;
            this.Reason = reason;
        }
        public FailReason(string fileName, string rowInfo, string fieldInfo, string reason)
        {
            this.FileName = fileName;
            this.ReportTableField = string.Empty;
            this.RowInfo = rowInfo;
            this.FieldInfo = fieldInfo;
            this.Reason = reason;
        }


        public bool Equals(FailReason x, FailReason y)
        {
            return x.FileName == y.FileName;
        }

        public int GetHashCode(FailReason obj)
        {
            return obj.ToString().GetHashCode();
        }
    }
}
