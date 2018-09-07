using NSoup.Nodes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade;
using Vcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure.AnalisisHtml
{
    sealed  class  DataTableOperator
    {
     
        internal static List<DataColumn> createDataColumns(string tableName,Element columnele)
        {
            List<DataColumn> list = new List<DataColumn>();
            var eles = columnele.Children;
            foreach (var eleitem in eles)
            {
                string columnName = eleitem.Text();
                try
                {
                        DataColumn dc = GetCol(tableName,columnName);
                        list.Add(dc);
                }
                catch(Exception ex)
                {

                }
            
      
            }
            return list;
        }
        internal static DataTable CreateDataTableByCol(string tableName, List<string> columnNameArr, List<List<string>> datas)
        {
            DataTable dt = new DataTable();
            foreach (string item in columnNameArr)
            {
                dt.Columns.Add(GetCol(tableName, item));
            }
            AddDataRows(datas, dt);
            return dt;
        }

        internal static void AddDataRows(List<List<string>> datas, DataTable dt)
        {
            foreach (var item in datas)
            {
                var dr = dt.NewRow();
                for (int i = 0; i < item.Count; i++)
                {
                    dr[i] = SetDefaultValue(dt.Columns[i].DataType, item[i]);
                }
                dt.Rows.Add(dr);
            }
        }

        internal static DataTable GetTable(string tableName,Dictionary<string,string> nameFileDic)
        {
           var list= TableMapData.GetColumns(tableName);
           foreach (var item in list)
           {
               nameFileDic.Add(item.ReportFileName.Trim(), item.DBFileName.Trim());
           }
           return   CreateDataTableByCol(tableName, list.Select(x=>x.DBFileName).ToList(), new List<List<string>>());
        }
        internal static DataColumn GetCol(string tableName, string columnName)
        {
            columnName = columnName.Replace(" ","");//去掉空格。
            IEnumerable<System.Reflection.PropertyInfo> pinfos = null;
            DataColumn dc = new DataColumn();
            string fileName = TableMapData.GetFileName(tableName, columnName).Trim();
            if (MappingModel.proInfosDic.Keys.Contains(tableName))
            {
                pinfos = MappingModel.proInfosDic[tableName].Where(item => item.Name == fileName);//判断是否有相应的类型
            }
            if (pinfos != null && pinfos.Count() != 0)
            {
                dc.ColumnName = fileName;
                dc.DataType = pinfos.FirstOrDefault().PropertyType;

            }
            else
            {
                dc.ColumnName = fileName;
            }
            dc.DefaultValue = DBNull.Value;
            return dc;
        }
        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns>值，验证信息</returns>
        internal static object  SetDefaultValue(Type type, string valStr)
        {
            if (type == typeof(string))
                return valStr;
            valStr = valStr.Replace(",", "").Trim();
            if (valStr.Equals(CommonData.nullstr, StringComparison.OrdinalIgnoreCase) || valStr == string.Empty || valStr == CommonData.doubleTrunk || valStr ==CommonData.zero)
            {
                   return  DBNull.Value;
            }
            else if (type == typeof(int) || type == typeof(decimal))
            {
                   int  val=0;
                   if (!int.TryParse(valStr, out val))
                       throw new Exception(valStr + "数字转换失败");
            }
            else if (type == typeof(DateTime))
            {
                DateTime dt = new DateTime(1800, 11, 11);
                if( !DateTime.TryParse(valStr, out dt) )
                        throw new Exception(valStr + "时间转换失败");
                 
            }
            return valStr;

        }
        internal static string GetfieldData(string loadStr, string fieldName, string second)
        {
            int index = loadStr.IndexOf(fieldName) + fieldName.Length;
            return loadStr.Substring(index, loadStr.IndexOf(second) - index).Replace(",", "").Trim();
        }
       
    
    }
}
