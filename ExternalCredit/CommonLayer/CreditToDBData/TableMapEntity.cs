using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExtTrade.CommonLayer
{
   public  class TableMapEntity
    {
        string reportTableName;
         /// <summary>
         /// 征信报告表名
         /// </summary>
        public string ReportTableName 
        {
            get { return reportTableName; }
            set { reportTableName = value; }
        }

        string dBTableName;
        /// <summary>
        /// 数据库表名
        /// </summary>
        public string DBTableName
        {
            get { return dBTableName; }
            set { dBTableName = value; }
        }

        string reportFileName;
        /// <summary>
        /// 征信报告字段名
        /// </summary>
        public string ReportFileName
        {
            get { return reportFileName; }
            set { reportFileName = value; }
        }

        string dBFileName;
        /// <summary>
        ///数据库表字段名
        /// </summary>
        public string DBFileName
        {
            get { return dBFileName; }
            set { dBFileName = value; }
        }


    }
}
