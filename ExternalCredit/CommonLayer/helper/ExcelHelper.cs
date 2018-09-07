using System;
using System.IO;
using System.Data;
using System.Collections;
using System.Data.OleDb;
using System.Web;
using System.Collections.Generic;


namespace Vcredit.ExtTrade.CommonLayer
{
    /// <summary>
    /// Excel操作类
    /// </summary>
    /// Microsoft Excel 11.0 Object Library
    public class ExcelHelper
    {
  
        /// <summary>
        /// 获取Excel文件数据表列表
        /// </summary>
        public static List<string> GetExcelTables(string ExcelFileName)
        {

            DataTable dt = new DataTable();
            List<string> TablesList = new List<string>();
            FileInfo fInfo=new FileInfo (ExcelFileName);
            if (fInfo.Exists)
            {
                using (OleDbConnection conn = new OleDbConnection(CreateOleDbConnectionStr(fInfo)))
                {
                    try
                    {
                        conn.Open();
                        dt = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,new object[] { null, null, null, "TABLE" });
       
                    }
                    catch (Exception exp)
                    {
                        throw exp;
                    }

                    //获取数据表个数
                    int tablecount = dt.Rows.Count;
                    for (int i = 0; i < tablecount; i++)
                    {
                        string tablename = dt.Rows[i][2].ToString().Trim().TrimEnd('$');
                        if (TablesList.IndexOf(tablename) < 0)
                        {
                            TablesList.Add(tablename);
                        }
                    }
                }
            }
            return TablesList;
        }
        static string CreateOleDbConnectionStr(FileInfo fileInfo)
        {
            string extension = fileInfo.Extension;
            string strConn = string.Empty;
            switch (extension)
            {
                case ".xls":
                    strConn = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileInfo .FullName + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;CharacterSet=65001;'";
                    break;
                case ".xlsx":
                    strConn = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileInfo.FullName + ";Extended Properties='Excel 12.0;HDR=Yes;IMEX=1;CharacterSet=65001;'";
                    break;
                default:
                    strConn = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileInfo.FullName + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;CharacterSet=65001;'";
                    break;
            }
            return strConn;
        }
        /// <summary>
        /// 将Excel文件导出至DataTable(第一行作为表头)
        /// </summary>
        /// <param name="ExcelFilePath">Excel文件路径</param>
        /// <param name="TableName">数据表名，如果数据表名错误，默认为第一个数据表名</param>
        public static DataTable InputFromExcel(string ExcelFilePath, string TableName)
        {
            FileInfo finfo = new FileInfo(ExcelFilePath);
            if (!finfo.Exists)
            {
                throw new Exception("Excel文件不存在！");
            }

            //如果数据表名不存在，则数据表名为Excel文件的第一个数据表
            List<string> TableList = new List<string>();
            TableList = GetExcelTables(ExcelFilePath);

            if (TableList.IndexOf(TableName) < 0)
            {
                TableName = TableList[0].ToString().Trim();
            }

            DataTable table = new DataTable();
            OleDbConnection dbcon = new OleDbConnection(CreateOleDbConnectionStr(finfo));
            OleDbCommand cmd = new OleDbCommand("select * from [" + TableName + "$]", dbcon);
            OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);

            try
            {
                if (dbcon.State == ConnectionState.Closed)
                {
                    dbcon.Open();
                }
                adapter.Fill(table);
            }
            catch (Exception exp)
            {
                throw exp;
            }
            finally
            {
                if (dbcon.State == ConnectionState.Open)
                {
                    dbcon.Close();
                }
            }
            return table;
        }


    }
}